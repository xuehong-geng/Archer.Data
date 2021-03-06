﻿using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Interface of Expression enumerator
    /// </summary>
    public interface IEnumerator
    {
        /// <summary>
        /// Enumerate one expression node
        /// </summary>
        /// <param name="item">Expression node</param>
        /// <returns>Returns true if should continue; false if should stop.</returns>
        bool Enumerate(Item item);
    }

    public abstract class Item
    {
        public static MethodInfo GetContainsMethod()
        {
            var type = typeof(Enumerable);
            MethodInfo containsMethod = null;
            foreach (var m in type.GetMethods())
            {
                if (m.Name != "Contains")
                    continue;
                if (2 == m.GetParameters().Count())
                {
                    containsMethod = m;
                    break;
                }
            }
            var method = containsMethod.MakeGenericMethod(new Type[] { typeof(string) });
            return method;
        }

        public override string ToString()
        {
            return "?";
        }

        public abstract Expression ToLinq(ToLinqContext ctx);
        public abstract Item Translate(IExpressionTranslator translator);
        public virtual bool Enumerate(IEnumerator enumerator)
        {
            return enumerator.Enumerate(this);
        }

        public Item Equals(Item val)
        {
            return new Equals {Left = this, Right = val};
        }

        public Item NotEquals(Item val)
        {
            return new NotEquals {Left = this, Right = val};
        }

        public Item And(Item right)
        {
            return new And {Left = this, Right = right};
        }

        public Item Or(Item right)
        {
            return new Or {Left = this, Right = right};
        }

        public Item In(Set set)
        {
            return new In {Left = this, Right = set};
        }

        public Item NotIn(Set set)
        {
            return new NotIn {Left = this, Right = set};
        }
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

        public override Item Translate(IExpressionTranslator translator)
        {
            // Translate children first
            Left = Left.Translate(translator);
            Right = Right.Translate(translator);
            // Handle null case
            if (Left is ValueOrReference && Right is ValueOrReference)
            {
                var lVal = Left as ValueOrReference;
                var rVal = Right as ValueOrReference;
                if (lVal.Type != rVal.Type)
                {   // Type are different
                    if (rVal.Type.IsAssignableFrom(lVal.Type))
                        rVal.Type = lVal.Type;
                    else if (lVal.Type.IsAssignableFrom(rVal.Type))
                        lVal.Type = rVal.Type;
                }
            }
            return this;
        }

        public override bool Enumerate(IEnumerator enumerator)
        {
            if (!enumerator.Enumerate(this))
                return false;
            if (!Left.Enumerate(enumerator))
                return false;
            return Right.Enumerate(enumerator);
        }
    }

    public abstract class Compare : Binary
    {
        public override Item Translate(IExpressionTranslator translator)
        {
            base.Translate(translator);
            if (Left == null || Right == null)
                return null;
            return this;
        }
    }

    public abstract class Logical : Binary
    {
        public override Item Translate(IExpressionTranslator translator)
        {
            var ret = base.Translate(translator);
            if (Left == null && Right == null)
                return null;
            else if (Left == null && Right != null)
                return Right;
            else if (Left != null && Right == null)
                return Left;
            else
                return this;
        }
    }

    public class Equals : Compare
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

    public class NotEquals : Compare
    {
        public override string ToString()
        {
            return "(" + Left + " != " + Right + ")";
        }

        public override Expression ToLinq(ToLinqContext ctx)
        {
            ThrowIfInvalid();
            return Expression.NotEqual(Left.ToLinq(ctx), Right.ToLinq(ctx));
        }
    }

    public class And : Logical
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

    public class Or : Logical
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

    public class Set : Item
    {
        private Collection<ValueOrReference> _items = new Collection<ValueOrReference>();

        public Collection<ValueOrReference> Items { get { return _items; } }

        public Type Type { get; set; }

        public Set()
        {
            Type = typeof(object);
        }

        public Set(Type type)
        {
            Type = type;
        }

        public override string ToString()
        {
            var str = new StringBuilder();
            str.Append("[");
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
            str.Append("]");
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
            return Expression.NewArrayInit(Type, _items.Select(a => a.ToLinq(ctx)));
        }

        public override Item Translate(IExpressionTranslator translator)
        {
            if (translator == null)
                return this;
            var newCol = new Collection<ValueOrReference>();
            foreach (var item in _items)
            {
                newCol.Add(item.Translate(translator) as ValueOrReference);
            }
            _items = newCol;
            return this;
        }

        public override bool Enumerate(IEnumerator enumerator)
        {
            if (!enumerator.Enumerate(this))
                return false;
            foreach (var item in _items)
            {
                if (!item.Enumerate(enumerator))
                    return false;
            }
            return true;
        }
    }

    public class In : Compare
    {
        public override string ToString()
        {
            return "(" + Left + " IN " + Right + ")";
        }

        public override Expression ToLinq(ToLinqContext ctx)
        {
            ThrowIfInvalid();
            if (!(Right is Set))
                throw new InvalidOperationException("Right item of 'In' must be a set!");
            var set = Right as Set;
            var array = set.ToLinq(ctx);
            var memberType = GetContainsMethod();
            return Expression.Call(null, memberType, array, Left.ToLinq(ctx));
        }
    }

    public class NotIn : Compare
    {
        public override string ToString()
        {
            return "(" + Left + " NOT IN " + Right + ")";
        }

        public override Expression ToLinq(ToLinqContext ctx)
        {
            ThrowIfInvalid();
            if (!(Right is Set))
                throw new InvalidOperationException("Right item of 'In' must be a set!");
            var set = Right as Set;
            var array = set.ToLinq(ctx);
            var memberType = GetContainsMethod();
            var call = Expression.Call(null, memberType, array, Left.ToLinq(ctx));
            return Expression.Not(call);
        }
    }

    public abstract class ValueOrReference : Item
    {
        public Type Type { get; set; }

        public ValueOrReference()
        {
            Type = typeof (object);
        }

        public ValueOrReference(Type type)
        {
            Type = type;
        }

        public override Item Translate(IExpressionTranslator translator)
        {
            if (translator == null)
                return this;
            return translator.Translate(this);
        }
    }

    public class Constant : ValueOrReference
    {
        public object Value { get; set; }

        public Constant(object val)
        {
            Value = val;
            Type = val == null ? typeof(object) : val.GetType();
        }

        public override string ToString()
        {
            if (Value is string)
                return "'" + Value + "'";
            return Value.ToString();
        }

        public override Expression ToLinq(ToLinqContext ctx)
        {
            return Expression.Constant(Value, Type);
        }
    }

    public class Variable : ValueOrReference
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public Variable(Type type, string name)
            : base(type)
        {
            Name = name;
        }

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
