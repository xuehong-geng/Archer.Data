using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Archer.DataSecurity.Filter;
using Archer.DataSecurity.Model;

namespace Archer.DataSecurity.Service
{
    public static class EntityFrameworkExtensions
    {
        public static IQueryable<T> Filter<T>(this IQueryable<T> entitySet, string expression) where T : class
        {
            var rawExp = Parser.ParseExpression(expression);
            return Filter<T>(entitySet, rawExp);
        }

        public static IQueryable<T> Filter<T>(this IQueryable<T> entitySet, Item expression) where T : class
        {
            var param = System.Linq.Expressions.Expression.Parameter(typeof(T), "p");
            var exp = expression.ToLinq(new ToLinqContext
            {
                EntityType = typeof(T),
                EntityReference = param
            });
            var lambda = Expression.Lambda<Func<T, bool>>(exp, param);
            return entitySet.Where(lambda);
        }

        public static IQueryable<T> FilterForRole<T>(this IQueryable<T> entitySet, string role, AccessType accessType) where T : class
        {
            var lambda = DataSecurityManager.Default.GetFilterExpressionForRole<T>(role, accessType);
            return lambda == null ? entitySet : entitySet.Where(lambda);
        }
        public static IQueryable<T> FilterForRoles<T>(this IQueryable<T> entitySet, string[] roles, AccessType accessType) where T : class
        {
            var lambda = DataSecurityManager.Default.GetFilterExpressionForRole<T>(roles, accessType);
            return lambda == null ? entitySet : entitySet.Where(lambda);
        }

    }
}