using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Archer.DataSecurity.Store;

namespace Archer.DataSecurity.Service
{
    public class DomainTypeMap
    {
        private class Entry
        {
            public string TypeID { get; set; }
            public Type EntityType { get; set; }
            public PropertyInfo PropertyInfo { get; set; }
        }

        private static object _lock = new object();
        private static Dictionary<string, Dictionary<string, Entry>> _dict =
            new Dictionary<string, Dictionary<string, Entry>>();

        private string _connectionString;

        public DomainTypeMap()
        {
            _connectionString = null;
        }

        public DomainTypeMap(string nameOrConnectionString)
        {
            _connectionString = nameOrConnectionString;
        }

        protected PropertyInfo ResolveProperty(string domainTypeID, Type entityType)
        {
            using (var db = _connectionString == null ? new ConfigDbContext() : new ConfigDbContext(_connectionString))
            {
                var domainType = db.DomainTypes.FirstOrDefault(a => a.DomainTypeID == domainTypeID);
                if (domainType == null)
                    throw new InvalidOperationException(string.Format("Domain type {0} is not exist!", domainTypeID));
                var map = domainType.EntityMaps.FirstOrDefault(a => a.EntityName == entityType.FullName);
                var propName = map == null ? domainType.DomainTypeID : map.FieldName;
                var propInfo = entityType.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                if (propInfo == null)
                    throw new InvalidOperationException(String.Format("Property {0} is not in Entity {1}!", propName,
                        entityType.FullName));
                return propInfo;
            }
        }

        public PropertyInfo GetMappedProperty(string domainTypeID, Type entityType)
        {
            lock (_lock)
            {
                Dictionary<string, Entry> entries = null;
                if (!_dict.ContainsKey(domainTypeID))
                {
                    entries = new Dictionary<string, Entry>();
                    _dict[domainTypeID] = entries;
                }
                else
                {
                    entries = _dict[domainTypeID];
                }
                if (entries.ContainsKey(entityType.FullName))
                {
                    var entry = entries[entityType.FullName];
                    return entry.PropertyInfo;
                }
                else
                {
                    var propInfo = ResolveProperty(domainTypeID, entityType);
                    var entry = new Entry
                    {
                        TypeID = domainTypeID,
                        EntityType = entityType,
                        PropertyInfo = propInfo
                    };
                    entries[entityType.FullName] = entry;
                    return propInfo;
                }
            }
        }
    }
}
