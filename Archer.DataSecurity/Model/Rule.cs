using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Archer.DataSecurity.Filter;

namespace Archer.DataSecurity.Model
{
    public enum AccessType : int
    {
        ListOnly = 0,       // No Access to content, can only know it is exist
        ReadOnly = 1,       // Can read content
        ReadWrite = 2,      // Can read and write content or update properties
        FullAccess = 3      // Can do everything including delete it
    }

    public enum ActorType : int
    {
        User = 0,
        Role = 1,
        Group = 2,
        Organization = 3    
    }

    public class AccessRule
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Resource { get; set; }
        public AccessType AccessType { get; set; }
        public string Filter { get; set; }

        public virtual ICollection<AccessConstraint> Constraints { get; set; } 
    }

    public class AccessConstraint
    {
        [Key]
        [Column(Order = 0)]
        public ActorType ActorType { get; set; }
        [Key]
        [Column(Order = 1)]
        public string ActorId { get; set; }
        [Key]
        [Column(Order = 2)]
        public string RuleId { get; set; }
        public string ActorName { get; set; }

        [ForeignKey("RuleId")]
        public virtual AccessRule Rule { get; set; }
    }

    public class Rule
    {
        public AccessType AccessType { get; set; }
        public Item DataFilter { get; set; }
    }
}
