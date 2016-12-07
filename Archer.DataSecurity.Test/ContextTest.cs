using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using Archer.DataSecurity.Filter;
using Archer.DataSecurity.Model;
using Archer.DataSecurity.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Archer.DataSecurity.Test
{
    /// <summary>
    /// student
    /// </summary>
    public class Student
    {
        [Key]
        public string ID { get; set; }
        public string Name { get; set; }
        public string Sex { get; set; }
        public DateTime? Birthday { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public virtual ICollection<Score> CourseScores { get; set; }
    }

    public class Score
    {
        [Key]
        [Column(Order = 0)]
        public string StudentId { get; set; }
        [Key]
        [Column(Order = 1)]
        public string Course { get; set; }
        [Key]
        [Column(Order = 2)]
        public string Semester { get; set; }
        public double Value { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
    }

    public class SchoolDbContext : DbContext
    {
        public SchoolDbContext()
            : base("test")
        {
        }

        public SchoolDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Score> Scores { get; set; }
    }

    [TestClass]
    public class ContextTest
    {
        protected static Student[] Students = new[]
        {
            new Student { ID = "张山", Name = "张山", Address = "广州番禹区张山家", Birthday = DateTime.Now, Email = "张山@test.com", PhoneNumber = "12303939999", Sex = "Male" },
            new Student { ID = "王二", Name = "王二", Address = "广州番禹区王二家", Birthday = DateTime.Now, Email = "王二@test.com", PhoneNumber = "12303939999", Sex = "Male" },
            new Student { ID = "李娟", Name = "李娟", Address = "广州番禹区李娟家", Birthday = DateTime.Now, Email = "李娟@test.com", PhoneNumber = "12303939999", Sex = "Female" },
            new Student { ID = "赵燕", Name = "赵燕", Address = "广州番禹区赵燕家", Birthday = DateTime.Now, Email = "赵燕@test.com", PhoneNumber = "12303939999", Sex = "Female" },
        };

        protected static Score[] Scores = new[]
        {
            new Score {StudentId = "张山", Course = "数学", Semester = "初一上学期", Value = 95},
            new Score {StudentId = "张山", Course = "语文", Semester = "初二上学期", Value = 85},
            new Score {StudentId = "王二", Course = "数学", Semester = "初二上学期", Value = 55},
            new Score {StudentId = "王二", Course = "语文", Semester = "初一上学期", Value = 58},
            new Score {StudentId = "李娟", Course = "数学", Semester = "初二上学期", Value = 69},
            new Score {StudentId = "李娟", Course = "语文", Semester = "初一上学期", Value = 45},
            new Score {StudentId = "赵燕", Course = "数学", Semester = "初二上学期", Value = 93},
            new Score {StudentId = "赵燕", Course = "语文", Semester = "初一上学期", Value = 95},
        };

        protected static DomainType[] DomainTypes = new[]
        {
            new DomainType { DomainTypeID = "Sex", DomainTypeName = "性别" },
            new DomainType { DomainTypeID = "Course", DomainTypeName = "科目"},
            new DomainType { DomainTypeID = "Score", DomainTypeName = "成绩"}
        };

        protected static DomainTypeEntityMap[] DomainTypeMaps = new[]
        {
            new DomainTypeEntityMap { DomainTypeID = "Score", EntityName = "Archer.DataSecurity.Test.Score", FieldName = "Value" },
        };

        protected static AccessRule[] AccessRules = new[]
        {
            new AccessRule { AccessRuleID = "Sex_All_All", AccessRuleName = "操作所有性别相关数据", AccessType = AccessType.FullAccess, Filter = "Sex != null" },
            new AccessRule { AccessRuleID = "Sex_Male_All", AccessRuleName = "操作男性相关数据", AccessType = AccessType.FullAccess, Filter = "Sex == 'Male'" },
            new AccessRule { AccessRuleID = "Sex_Female_All", AccessRuleName = "操作女性相关数据", AccessType = AccessType.FullAccess, Filter = "Sex == 'Female'" },
            new AccessRule { AccessRuleID = "Course_Math_Read", AccessRuleName = "查询数学相关数据", AccessType = AccessType.ReadOnly, Filter = "(Course == '数学' && Course == '语文') && Sex != null" },
            new AccessRule { AccessRuleID = "Course_YUWEN_All", AccessRuleName = "操作语文相关数据", AccessType = AccessType.FullAccess, Filter = "Course == '语文'" },
            new AccessRule { AccessRuleID = "No_Course", AccessRuleName = "操作不存在字段", AccessType = AccessType.FullAccess, Filter = "NoCourse == '语文' && NoCourse == '数学'" },
            new AccessRule { AccessRuleID = "BUG_ID1", AccessRuleName = "操作不存在字段", AccessType = AccessType.FullAccess, Filter = "ID=='C000001'" },
            new AccessRule { AccessRuleID = "BUG_ID2", AccessRuleName = "操作不存在字段", AccessType = AccessType.FullAccess, Filter = "ID!='0' && SupplierCode!='0' && ProdCataCode!='0'" },
        };

        protected void PrepareTestData()
        {
            var db = new SchoolDbContext("test");
            foreach (var student in Students)
            {
                if (db.Students.All(a => a.ID != student.ID)) db.Students.Add(student);
            }
            foreach (var score in Scores)
            {
                if (
                    db.Scores.All(
                        a => !(a.StudentId == score.StudentId && a.Course == score.Course && a.Semester == score.Semester)))
                    db.Scores.Add(score);
            }
            db.SaveChanges();
        }

        protected void ClearTestData()
        {
            var db = new SchoolDbContext("test");
            foreach (var score in Scores)
            {
                var s = db.Scores.FirstOrDefault(
                    a =>
                        a.StudentId == score.StudentId && a.Course == score.Course && a.Semester == score.Semester);
                if (s != null)
                    db.Scores.Remove(s);
            }
            foreach (var student in Students)
            {
                var s = db.Students.FirstOrDefault(a => a.ID == student.ID);
                if (s != null)
                    db.Students.Remove(s);
            }
            db.SaveChanges();
        }

        protected void PrepareDomainRules()
        {
            var dmm = new DomainManager("test");
            foreach (var domainType in DomainTypes)
            {
                dmm.CreateOrUpdateDomainType(domainType.DomainTypeID, domainType.DomainTypeName);
            }
            foreach (var map in DomainTypeMaps)
            {
                dmm.MapDomainTypeToEntity(map.DomainTypeID, map.EntityName, map.FieldName);
            }
            var mgr = new DataSecurityManager("test");
            foreach (var rule in AccessRules)
            {
                mgr.AddOrUpdateAccessRule(rule);
            }
        }

        protected void ClearDomainRules()
        {
            var mgr = new DataSecurityManager("test");
            foreach (var rule in AccessRules)
            {
                mgr.DeleteAccessRule(rule.AccessRuleID);
            }
            var dmm = new DomainManager("test");
            foreach (var map in DomainTypeMaps)
            {
                dmm.UnmapDomainTypeFromEntity(map.DomainTypeID, map.EntityName);
            }
            foreach (var domainType in DomainTypes)
            {
                dmm.DeleteDomainType(domainType.DomainTypeID);
            }
        }

        [TestMethod]
        public void TestQueryConstraintWithSex()
        {
            DataSecurityManager.InitializeDefaultManager("test");
            // Prepare test data
            PrepareTestData();
            PrepareDomainRules();
            var db = new SchoolDbContext("test");
            var mgr = new DataSecurityManager("test");
            // 在没有设置Constraint前，角色不具备任何的访问权限
            mgr.DelRoleConstraints("Admin");
            var all = db.Students.FilterForRole("Admin", AccessType.ReadOnly);
            Assert.AreEqual(all.Count(), 0);
            // 设置全部权限
            mgr.AddRoleConstraint("Admin", "Sex_All_All");
            all = db.Students.FilterForRole("Admin", AccessType.ReadOnly);
            Assert.AreEqual(all.Count(), db.Students.Count());
            // 设置男性访问
            mgr.DelRoleConstraint("Admin", "Sex_All_All");
            mgr.AddRoleConstraint("Admin", "Sex_Male_All");
            var query = db.Students.FilterForRole("Admin", AccessType.ReadOnly);
            Assert.IsTrue(query.All(a => a.Sex == "Male"));
            // 改为女性访问
            mgr.DelRoleConstraint("Admin", "Sex_Male_All");
            mgr.AddRoleConstraint("Admin", "Sex_Female_All");
            query = db.Students.FilterForRole("Admin", AccessType.FullAccess);
            Assert.IsTrue(query.All(a => a.Sex == "Female"));
            mgr.DelRoleConstraint("Admin", "Sex_Female_All");
            // Clear test data
            ClearDomainRules();
            ClearTestData();
        }

        [TestMethod]
        public void TestConstraintOnSexAndScore()
        {
            DataSecurityManager.InitializeDefaultManager("test");
            // Prepare test data
            PrepareTestData();
            PrepareDomainRules();
            var db = new SchoolDbContext("test");
            var mgr = new DataSecurityManager("test");
            // Grant to role. 只允许Admin访问男生数据，数学成绩
            //mgr.AddRoleConstraint("Admin", "Sex_Male_All");
            //mgr.AddRoleConstraint("Admin", "No_Course");  //当不存在的字段出现在表达式2次，无法解析(需求是单个排除不存在的字段条件，不要整体都排除)   //andy
                                                          //mgr.AddRoleConstraint("Admin", "Course_Math_Read");
            mgr.AddRoleConstraint("Admin", "BUG_ID1");
            mgr.AddRoleConstraint("Admin", "BUG_ID2");
            // 单查学生数据，应只有男生的数据查出
            var students = db.Students.FilterForRole("Admin", AccessType.ReadOnly);
            //Assert.IsTrue(students.All(a => a.Sex == "Male"));
            Console.WriteLine("\n学生：");
            foreach (var student in students.ToList())
            {
                Console.WriteLine("{0},{1}", student.Name, student.Sex);
            }
            // 单查成绩数据，应只有数学的成绩查出
            var scores = db.Scores.FilterForRole("Admin", AccessType.ReadOnly);
            //Assert.IsTrue(scores.All(a => a.Course == "数学" || a.Course == "语文"));
            Console.WriteLine("\n成绩：");
            foreach (var score in scores.ToList())
            {
                Console.WriteLine("{0}: {1}: {2}", score.Student.Name, score.Course, score.Value);
            }
            // 联合查询：查出所有成绩在60分以上的同学。此时因只查出“数学成绩在60分以上的男同学”
            var studentsScoreBt60 = from s in students
                                    join c in scores on s.ID equals c.StudentId
                                    where c.Value > 60
                                    select s;
            //Assert.IsTrue(studentsScoreBt60.All(a => a.Sex == "Male"));
            Console.WriteLine("\n成绩60分以上的学生：");
            foreach (var student in studentsScoreBt60.ToList())
            {
                Console.WriteLine("{0},{1}", student.Name, student.Sex);
            }
            // Clear test data
            ClearDomainRules();
            ClearTestData();
        }
    }
}
