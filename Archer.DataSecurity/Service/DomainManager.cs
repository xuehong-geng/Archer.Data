using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archer.DataSecurity.Model;
using Archer.DataSecurity.Store;

namespace Archer.DataSecurity.Service
{
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

        public DomainType FindDomainTypeById(string id)
        {
            return _db.DomainTypes.FirstOrDefault(a => a.Id == id);
        }

        public DomainType CreateOrUpdateDomainType(string id, string name)
        {
            var dt = _db.DomainTypes.FirstOrDefault(a => a.Id == id);
            if (dt == null)
            {
                dt = new DomainType
                {
                    Id = id,
                    Name = name
                };
                _db.DomainTypes.Add(dt);
            }
            else
            {
                dt.Name = name;
            }
            _db.SaveChanges();
            return dt;
        }

        public void DeleteDomainType(string id)
        {
            var dt = _db.DomainTypes.FirstOrDefault(a => a.Id == id);
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

        public void MapDomainTypeToEntity(string domainTypeId, string entityName, string fieldName)
        {
            var map =
                _db.DomainTypeEntityMaps.FirstOrDefault(
                    a => a.DomainTypeId == domainTypeId && a.EntityName == entityName);
            if (map == null)
            {
                map = new DomainTypeEntityMap
                {
                    DomainTypeId = domainTypeId,
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

        public void UnmapDomainTypeFromEntity(string domainTypeId, string entityName)
        {
            var map =
                _db.DomainTypeEntityMaps.FirstOrDefault(
                    a => a.DomainTypeId == domainTypeId && a.EntityName == entityName);
            if (map == null)
                return;
            _db.DomainTypeEntityMaps.Remove(map);
            _db.SaveChanges();
        }

        public void AddOrUpdateDomain(DomainType type, string id, string name)
        {
            var dm = _db.Domains.FirstOrDefault(a => a.DomainTypeId == type.Id && a.Id == id);
            if (dm == null)
            {
                dm = new Domain
                {
                    DomainTypeId = type.Id,
                    Id = id,
                    Name = name,
                    ParentId = null
                };
                _db.Domains.Add(dm);
            }
            else
            {
                dm.Name = name;
            }
            _db.SaveChanges();
        }

        public void DeleteDomain(DomainType type, string id)
        {
            var dm = _db.Domains.FirstOrDefault(a => a.DomainTypeId == type.Id && a.Id == id);
            if (dm == null)
                return;
            _db.Domains.Remove(dm);
            _db.SaveChanges();
        }
    }
}
