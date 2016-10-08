using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archer.DataSecurity.Model
{
    public class DomainType
    {
        [Key]
        public string Id { get; set; }
        /// <summary>
        /// 标准的表的查询字段
        /// </summary>
        public string Name { get; set; }    
        /// <summary>
        /// domain type的具体定义 关系表，如果字段被其他用了，可以通过这里设置其他字段名来关联domiantype
        /// </summary>
        public virtual ICollection<DomainTypeEntityMap> EntityMaps { get; set; } 
        public virtual ICollection<Domain> Domains { get; set; } 
    }

    /// <summary>
    /// domainType的外键关联字段名
    /// </summary>
    public class DomainTypeEntityMap
    {
        /// <summary>
        /// domaintype表的主键
        /// </summary>
        [Key]
        [Column(Order = 0)]        
        public string DomainTypeId { get; set; }
        /// <summary>
        /// 实体字段名
        /// </summary>
        [Key]
        [Column(Order = 1)]
        public string EntityName { get; set; }
        /// <summary>
        /// 查询用字段名
        /// </summary>
        public string FieldName { get; set; }

        [ForeignKey("DomainTypeId")]
        public virtual DomainType DomainType { get; set; }
    }

    public class Domain
    {
        [Key]
        [Column(Order = 0)]
        public string Id { get; set; }
        [Key]
        [Column(Order = 1)]
        public string DomainTypeId { get; set; }
        public string Name { get; set; }
        public string ParentId { get; set; }

        [ForeignKey("DomainTypeId")]
        public virtual DomainType DomainType { get; set; }
    }
}
