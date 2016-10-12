using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archer.DataSecurity.Filter;
using Archer.DataSecurity.Model;
using Archer.DataSecurity.Store;

namespace Archer.DataSecurity.Service
{
    /// <summary>
    /// domain数据维护
    /// </summary>
    public class DomainManager
    {
        private ConfigDbContext _db;

        public DomainManager()
        {
            _db = new ConfigDbContext();
        }

        public DomainManager(string nameOrConnectionString)
        {
            _db = new ConfigDbContext(nameOrConnectionString);
        }

        public ConfigDbContext DbContext { get { return _db; } }

        public DomainType FindDomainTypeByID(string id)
        {
            return _db.DomainTypes.FirstOrDefault(a => a.DomainTypeID == id);
        }

        public DomainType CreateOrUpdateDomainType(string id, string name)
        {
            var dt = _db.DomainTypes.FirstOrDefault(a => a.DomainTypeID == id);
            if (dt == null)
            {
                dt = new DomainType
                {
                    DomainTypeID = id,
                    DomainTypeName = name
                };
                _db.DomainTypes.Add(dt);
            }
            else
            {
                dt.DomainTypeName = name;
            }
            _db.SaveChanges();
            return dt;
        }

        public void DeleteDomainType(string id)
        {
            var dt = _db.DomainTypes.FirstOrDefault(a => a.DomainTypeID == id);
            if (dt != null)
            {
                foreach (var map in dt.EntityMaps.ToList())
                {
                    dt.EntityMaps.Remove(map);
                }
                foreach (var dm in dt.Domains.ToList())
                {
                    dt.Domains.Remove(dm);
                }
                _db.SaveChanges();
                _db.DomainTypes.Remove(dt);
                _db.SaveChanges();
            }
        }

        public void MapDomainTypeToEntity(string domainTypeID, string entityName, string fieldName)
        {
            var map =
                _db.DomainTypeEntityMaps.FirstOrDefault(
                    a => a.DomainTypeID == domainTypeID && a.EntityName == entityName);
            if (map == null)
            {
                map = new DomainTypeEntityMap
                {
                    DomainTypeID = domainTypeID,
                    EntityName = entityName,
                    FieldName = fieldName
                };
                _db.DomainTypeEntityMaps.Add(map);
            }
            else
            {
                map.FieldName = fieldName;
            }
            _db.SaveChanges();
        }

        public void UnmapDomainTypeFromEntity(string domainTypeID, string entityName)
        {
            var map =
                _db.DomainTypeEntityMaps.FirstOrDefault(
                    a => a.DomainTypeID == domainTypeID && a.EntityName == entityName);
            if (map == null)
                return;
            _db.DomainTypeEntityMaps.Remove(map);
            _db.SaveChanges();
        }

        public void AddOrUpdateDomain(DomainType type, string id, string name)
        {
            var dm = _db.Domains.FirstOrDefault(a => a.DomainTypeID == type.DomainTypeID && a.DomainID == id);
            if (dm == null)
            {
                dm = new Domain
                {
                    DomainTypeID = type.DomainTypeID,
                    DomainID = id,
                    DomainName = name,
                    ParentID = null
                };
                _db.Domains.Add(dm);
            }
            else
            {
                dm.DomainName = name;
            }
            _db.SaveChanges();
        }

        public void DeleteDomain(DomainType type, string id)
        {
            var dm = _db.Domains.FirstOrDefault(a => a.DomainTypeID == type.DomainTypeID && a.DomainID == id);
            if (dm == null)
                return;
            _db.Domains.Remove(dm);
            _db.SaveChanges();
        }
    }

    public static class DomainExpressionExtensions
    {
        public static Item Equals(this DomainType type, Domain domain)
        {
            if (domain == null)
                throw new ArgumentNullException(nameof(domain));
            if (domain.DomainTypeID != type.DomainTypeID)
                throw new InvalidOperationException("Cannot compare domain with different type.");
            return new Equals { Left = new Variable(typeof(string), type.DomainTypeID), Right = new Constant(domain.DomainTypeID) };
        }

        public static Item NotEquals(this DomainType type, Domain domain)
        {
            if (domain == null)
                throw new ArgumentNullException(nameof(domain));
            if (domain.DomainTypeID != type.DomainTypeID)
                throw new InvalidOperationException("Cannot compare domain with different type.");
            return new NotEquals { Left = new Variable(typeof(string), type.DomainTypeID), Right = new Constant(domain.DomainTypeID) };
        }

        public static Item In(this DomainType type, IEnumerable<Domain> domains)
        {
            if (domains == null)
                throw new ArgumentNullException(nameof(domains));
            var set = new Set(typeof(string));
            foreach (var domain in domains)
            {
                if (domain.DomainTypeID != type.DomainTypeID)
                    throw new InvalidOperationException("Cannot compare domain with different type.");
                set.Items.Add(new Constant(domain.DomainTypeID));
            }
            return new In { Left = new Variable(typeof(string), type.DomainTypeID), Right = set };
        }

        public static Item NotIn(this DomainType type, IEnumerable<Domain> domains)
        {
            if (domains == null)
                throw new ArgumentNullException(nameof(domains));
            var set = new Set(typeof(string));
            foreach (var domain in domains)
            {
                if (domain.DomainTypeID != type.DomainTypeID)
                    throw new InvalidOperationException("Cannot compare domain with different type.");
                set.Items.Add(new Constant(domain.DomainTypeID));
            }
            return new NotIn { Left = new Variable(typeof(string), type.DomainTypeID), Right = set };
        }
    }
}
