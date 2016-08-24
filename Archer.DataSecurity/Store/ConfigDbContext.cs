using System.Data.Entity;
using Archer.DataSecurity.Model;

namespace Archer.DataSecurity.Store
{
    public class ConfigDbContext : DbContext
    {
        public ConfigDbContext()
            : base("DataSecurity")
        {
        }

        public ConfigDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        public DbSet<DomainType> DomainTypes { get; set; }
        public DbSet<Domain> Domains { get; set; }
        public DbSet<DomainTypeEntityMap> DomainTypeEntityMaps { get; set; }

        public DbSet<AccessConstraint> AccessConstraints { get; set; }
        public DbSet<AccessRule> AccessRules { get; set; }
    }
}
