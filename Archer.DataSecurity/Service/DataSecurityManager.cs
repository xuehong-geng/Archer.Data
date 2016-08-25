using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Archer.DataSecurity.Filter;
using Archer.DataSecurity.Model;
using Archer.DataSecurity.Store;

namespace Archer.DataSecurity.Service
{
    public class DomainFilterTranslator : IExpressionTranslator
    {
        private DomainTypeMap _map;
        private Type _entityType;

        public DomainFilterTranslator(DomainTypeMap map, Type entityType)
        {
            _map = map;
            _entityType = entityType;
        }

        public Item Translate(Item item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (item is Variable)
            {   // Variables must be translated to reference of entity fields
                var v = item as Variable;
                var propInfo = _map.GetMappedProperty(v.Name, _entityType);
                return new Variable(v.Type, propInfo.Name);
            }
            return item;
        }
    }

    public class DataSecurityManager
    {
        private static DataSecurityManager _default = null;

        public static DataSecurityManager Default
        {
            get
            {
                if (_default == null)
                    throw new InvalidOperationException(
                        "DataSecurityManager has not been initialized yet! Please call InitializeDefaultManager before using Default manager.");
                return _default;

            }
        }

        public static void InitializeDefaultManager(string nameOrConnectionString)
        {
            _default = new DataSecurityManager(nameOrConnectionString);
        }

        private DomainTypeMap _map = null;
        private string _connectionString;

        public DataSecurityManager()
        {
            _map = new DomainTypeMap();
        }

        public DataSecurityManager(string nameOrConnectionString)
        {
            _connectionString = nameOrConnectionString;
            _map = new DomainTypeMap(_connectionString);
        }

        protected ConfigDbContext OpenDb()
        {
            if (_connectionString == null)
                return new ConfigDbContext();
            else
            {
                return new ConfigDbContext(_connectionString);
            }
        }

        public Rule GetRuleOnEntityForRole<T>(string role, AccessType accessType) where T : class 
        {
            using(var db = OpenDb())
            {
                // Get all rules of this role
                var rules = db.AccessConstraints.Where(
                    a => a.ActorType == ActorType.Role && a.ActorId == role && (int)a.Rule.AccessType >= (int)accessType)
                    .Select(a => a.Rule);
                // Choose those that suitable to entity
                var translator = new DomainFilterTranslator(_map, typeof (T));
                var acceptRules = new List<Item>();
                foreach (var rule in rules)
                {
                    try
                    {
                        var exp = Parser.ParseExpression(rule.Filter);
                        exp.Translate(translator);
                        acceptRules.Add(exp);
                    }
                    catch(Exception err)
                    {
                        Trace.TraceInformation("Rule {0} is not suitable to entity {1}. Err:{2}", rule.Id,
                            typeof (T).FullName, err.Message);
                    }
                }
                // Merge those rules into single rule using OR operator
                Item root = null;
                while (acceptRules.Any())
                {
                    var rule = acceptRules[0];
                    acceptRules.RemoveAt(0);
                    if (root == null)
                        root = rule;
                    else
                    {
                        root = new Or {Left = root, Right = rule};
                    }
                }

                return new Rule
                {
                    AccessType = accessType,
                    DataFilter = root
                };
            }
        }

        public Expression<Func<T, bool>> GetFilterExpressionForRole<T>(string role, AccessType accessType) where T : class 
        {
            var rule = GetRuleOnEntityForRole<T>(role, accessType);
            if (rule.DataFilter == null)
                return null;
            var param = System.Linq.Expressions.Expression.Parameter(typeof(T), "p");
            var exp = rule.DataFilter.ToLinq(new ToLinqContext
            {
                EntityType = typeof(T),
                EntityReference = param
            });
            return Expression.Lambda<Func<T, bool>>(exp, param);
        }

        public string CreateAccessRule(string name, Item filter, AccessType accessType)
        {
            using (var db = OpenDb())
            {
                var rule = new AccessRule
                {
                    Id = Guid.NewGuid().ToString().ToLower(),
                    Name = name,
                    AccessType = accessType,
                    Filter = filter.ToString()
                };
                db.AccessRules.Add(rule);
                db.SaveChanges();
                return rule.Id;
            }
        }

        public void DeleteAccessRule(string id)
        {
            using (var db = OpenDb())
            {
                var rule = db.AccessRules.FirstOrDefault(a => a.Id == id);
                if (rule != null)
                {
                    // Remove all constraints too
                    foreach (var cons in rule.Constraints.ToList())
                    {
                        rule.Constraints.Remove(cons);
                    }
                    db.SaveChanges();
                    // Remove rule
                    db.AccessRules.Remove(rule);
                    db.SaveChanges();
                }
            }
        }

        public void UpdateAccessRule(string id, Item filter)
        {
            using (var db = OpenDb())
            {
                var rule = db.AccessRules.FirstOrDefault(a => a.Id == id);
                if (rule == null)
                    throw new InvalidOperationException("Access rule not exist!");
                rule.Filter = filter.ToString();
                db.SaveChanges();
            }
        }

        public void AddRoleConstraint(string role, string rule)
        {
            using (var db = OpenDb())
            {
                var cons =
                    db.AccessConstraints.FirstOrDefault(
                        a => a.ActorType == ActorType.Role && a.ActorId == role && a.RuleId == rule);
                if (cons != null)
                    return;
                cons = new AccessConstraint
                {
                    ActorType = ActorType.Role,
                    ActorId = role,
                    ActorName = role,
                    RuleId = rule
                };
                db.AccessConstraints.Add(cons);
                db.SaveChanges();
            }
        }

        public void DelRoleConstraint(string role, string rule)
        {
            using (var db = OpenDb())
            {
                var cons =
                    db.AccessConstraints.FirstOrDefault(
                        a => a.ActorType == ActorType.Role && a.ActorId == role && a.RuleId == rule);
                if (cons == null)
                    return;
                db.AccessConstraints.Remove(cons);
                db.SaveChanges();
            }
        }
    }
}
