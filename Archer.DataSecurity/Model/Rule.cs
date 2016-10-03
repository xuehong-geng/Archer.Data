﻿using System;
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

    /// <summary>
    /// 数据的访问规则配置
    /// </summary>
    public class AccessRule
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Resource { get; set; }
        public AccessType AccessType { get; set; }
        /// <summary>
        /// 对数据的过滤条件，相当于where的查询条件表达式。条件的字段名是domaintype的Name
        /// </summary>
        public string Filter { get; set; }
        /// <summary> 
        /// 数据的访问规则类型，是针对人，角色，组织之类的
        /// 限制规则，告诉系统，针对那个类型生效
        /// </summary>
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
