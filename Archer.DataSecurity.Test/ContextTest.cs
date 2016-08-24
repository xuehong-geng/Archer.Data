using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Archer.DataSecurity.Test
{
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
        }
    }
}
