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
        public string Name { get; set; }    

        public virtual ICollection<DomainTypeEntityMap> EntityMaps { get; set; } 
        public virtual ICollection<Domain> Domains { get; set; } 
    }

    public class DomainTypeEntityMap
    {
        [Key]
        [Column(Order = 0)]
        public string DomainTypeId { get; set; }
        [Key]
        [Column(Order = 1)]
        public string EntityName { get; set; }
        public string FieldName { get; set; }

        [ForeignKey("DomainTypeId")]
        public virtual DomainType DomainType { get; set; }
    }

    public class Domain
    {
        [Key]
        public string Id { get; set; }
        public string DomainTypeId { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string ParentId { get; set; }
        public string FullName { get; set; }

        [ForeignKey("DomainTypeId")]
        public virtual DomainType DomainType { get; set; }
        [ForeignKey("ParentId")]
        public virtual Domain Parent { get; set; }
        public virtual ICollection<Domain> Children { get; set; } 
    }
}
