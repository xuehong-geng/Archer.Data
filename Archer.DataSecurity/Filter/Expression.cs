using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Archer.DataSecurity.Filter
{
    public class ToLinqContext
    {
        public Type EntityType { get; set; }
        public ParameterExpression EntityReference { get; set; }
    }

    public abstract class Item
    {
        public override string ToString()
        {
            return "?";
        }

        public abstract Expression ToLinq(ToLinqContext ctx);
    }

    public abstract class Binary : Item
    {
        public Item Left { get; set; }
        public Item Right { get; set; }

        protected void ThrowIfInvalid()
        {
            if (Left == null)
                throw new InvalidOperationException("Left expression of binary operatoin is null!");
            if (Right == null)
                throw new InvalidOperationException("Right expression of binary operation is null!");
        }
    }

    public class Equals : Binary
    {
        public override string ToString()
        {
            return "(" + Left + " == " + Right + ")";
        }

        public override Expression ToLinq(ToLinqContext ctx)
        {
            ThrowIfInvalid();
            return Expression.Equal(Left.ToLinq(ctx), Right.ToLinq(ctx));
        }
    }

    public class And : Binary
    {
        public override string ToString()
        {
            return "(" + Left + " && " + Right + ")";
        }

        public override Expression ToLinq(ToLinqContext ctx)
        {
            ThrowIfInvalid();
            return Expression.And(Left.ToLinq(ctx), Right.ToLinq(ctx));
        }
    }

    public class Or : Binary
    {
        public override string ToString()
        {
            return "(" + Left + " || " + Right + ")";
        }

        public override Expression ToLinq(ToLinqContext ctx)
        {
            ThrowIfInvalid();
            return Expression.Or(Left.ToLinq(ctx), Right.ToLinq(ctx));
        }
    }

    public abstract class Set<T> : Item
    {
        private Collection<ValueOrReference<T>> _items = new Collection<ValueOrReference<T>>();

        public Collection<ValueOrReference<T>> Items { get { return _items; } }

        public override string ToString()
        {
            var str = new StringBuilder();
            str.Append("(");
            bool first = true;
            foreach (var item in Items)
            {
                if (first)
                    first = false;
                else
                {
                    str.Append(", ");
                }
                str.Append(item);
            }
            return str.ToString();
        }

        protected void ThrowIfInvalid()
        {
            foreach (var item in _items)
            {
                if (item == null)
                    throw new InvalidOperationException("Item expression in Set is null!");
            }
        }

        public override Expression ToLinq(ToLinqContext ctx)
        {
            return Expression.NewArrayInit(typeof (T), _items.Select(a => a.ToLinq(ctx)));
        }
    }

    public class In<T> : Set<T>
    {
        public ValueOrReference<T> Operand { get; set; }

        public override string ToString()
        {
            return "(" + Operand + " IN " + base.ToString() + ")";
        }

        public override Expression ToLinq(ToLinqContext ctx)
        {
            ThrowIfInvalid();
            var array = base.ToLinq(ctx);
            var arrType = typeof (T[]);
            var memberType = arrType.GetMethod("Contains");
            return Expression.Call(array, memberType, Operand.ToLinq(ctx));
        }
    }

    public class NotIn<T> : Set<T>
    {
        public Item Operand { get; set; }

        public override string ToString()
        {
            return "(" + Operand + " IN " + base.ToString() + ")";
        }

        public override Expression ToLinq(ToLinqContext ctx)
        {
            ThrowIfInvalid();
            var array = base.ToLinq(ctx);
            var arrType = typeof(T[]);
            var memberType = arrType.GetMethod("Contains");
            var call = Expression.Call(array, memberType, Operand.ToLinq(ctx));
            return Expression.Not(call);
        }
    }

    public abstract class ValueOrReference<TV> : Item
    {
        public Type Type { get { return typeof (TV); } }
    }

    public class Constant<TV> : ValueOrReference<TV>
    {
        public TV Value { get; set; }

        public Constant(TV val)
        {
            Value = val;
        }

        public Constant(object val)
        {
            Value = (TV)Convert.ChangeType(val, typeof (TV));
        } 

        public override string ToString()
        {
            if (Value is string)
                return "\"" + Value + "\"";
            return Value.ToString();
        }

        public override Expression ToLinq(ToLinqContext ctx)
        {
            return Expression.Constant(Value);
        }
    }

    public class Variable<TV> : ValueOrReference<TV>
    {
        public string Name { get; set; }
        public TV Value { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public override Expression ToLinq(ToLinqContext ctx)
        {
            // The variable must be the name of property of entity
            var propInfo = ctx.EntityType.GetProperty(Name, BindingFlags.Public | BindingFlags.Instance);
            if (propInfo == null)
                throw new InvalidOperationException(string.Format("Property {0} does not exist in type {1}!", Name,
                    ctx.EntityType.FullName));
            return Expression.MakeMemberAccess(ctx.EntityReference, propInfo);
        }
    }
}
