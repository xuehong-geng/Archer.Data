using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
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
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sex { get; set; }
        public DateTime? Birthday { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
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
    }

    [TestClass]
    public class ContextTest
    {
        [TestMethod]
        public void TestFilteredEntitySet()
        {

            DataSecurityManager.InitializeDefaultManager("test");
            // Prepare test data
            var db = new SchoolDbContext("test");
            db.Students.Add(new Student
            {
                Id = Guid.NewGuid().ToString(),
                Name = "TEST",
                Address = "TEST",
                Birthday = DateTime.Now,
                Email = "TEST@dee.com",
                PhoneNumber = "12303939999",
                Sex = "Male"
            });
            db.Students.Add(new Student
            {
                Id = Guid.NewGuid().ToString(),
                Name = "TEST",
                Address = "TEST",
                Birthday = DateTime.Now,
                Email = "TEST@dee.com",
                PhoneNumber = "12303939999",
                Sex = "Female"
            });
            db.SaveChanges();
            // Create rule expressions
            var expMale = Parser.ParseExpression("Sex == 'Male'");
            var expFemale = Parser.ParseExpression("Sex == 'Female'");
            // Add rule
            var mgr = new DataSecurityManager("test");
            var ruleMale = mgr.CreateAccessRule("Male", expMale, AccessType.FullAccess);
            var ruleFemale = mgr.CreateAccessRule("Female", expFemale, AccessType.FullAccess);
            // Create domain type
            var dmgr = new DomainManager("test");
            dmgr.CreateOrUpdateDomainType("Sex","性别");
            // Grant to role
            mgr.AddRoleConstraint("Admin", ruleMale);
            // Query
            var query = db.Students.FilterForRole("Admin", AccessType.ReadOnly);
            Assert.IsTrue(query.All(a => a.Sex == "Male"));
            // Change rule
            mgr.DelRoleConstraint("Admin", ruleMale);
            mgr.AddRoleConstraint("Admin", ruleFemale);
            // Requery
            query = db.Students.FilterForRole("Admin", AccessType.FullAccess);
            Assert.IsTrue(query.All(a => a.Sex == "Female"));
            mgr.DelRoleConstraint("Admin", ruleFemale);
            // Clear test data
            mgr.DeleteAccessRule(ruleMale);
            mgr.DeleteAccessRule(ruleFemale);
            foreach (var st in db.Students.Where(a => a.Name == "TEST").ToList())
            {
                db.Students.Remove(st);
            }
            db.SaveChanges();
        }
    }
}
