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
            var param = System.Linq.Expressions.Expression.Parameter(typeof (T), "p");
            var exp = rawExp.ToLinq(new ToLinqContext
            {
                EntityType = typeof(T),
                EntityReference = param
            });
            var lambda = Expression.Lambda<Func<T, bool>>(exp, param);
            return entitySet.Where(lambda);
        }
    }
}
