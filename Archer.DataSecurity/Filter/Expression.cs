﻿using System;
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
        public abstract void Translate(IExpressionTranslator translator);
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

        public override void Translate(IExpressionTranslator translator)
        {
            if (translator != null)
            {
                Left = translator.Translate(Left);
                Right = translator.Translate(Right);
            }
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

    public abstract class Set : Item
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
            return Expression.NewArrayInit(Type, _items.Select(a => a.ToLinq(ctx)));
        }

        public override void Translate(IExpressionTranslator translator)
        {
            if (translator == null)
                return;
            var newCol = new Collection<ValueOrReference>();
            foreach (var item in _items)
            {
                newCol.Add(translator.Translate(item) as ValueOrReference);
            }
            _items = newCol;
        }
    }

    public class In : Set
    {
        public ValueOrReference Operand { get; set; }

        public override string ToString()
        {
            return "(" + Operand + " IN " + base.ToString() + ")";
        }

        public override Expression ToLinq(ToLinqContext ctx)
        {
            ThrowIfInvalid();
            var array = base.ToLinq(ctx);
            var arrType = Type;
            var memberType = arrType.GetMethod("Contains");
            return Expression.Call(array, memberType, Operand.ToLinq(ctx));
        }

        public override void Translate(IExpressionTranslator translator)
        {
            if (translator == null)
                return;
            base.Translate(translator);
            Operand = translator.Translate(Operand) as ValueOrReference;
        }
    }

    public class NotIn : Set
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
            var arrType = Type;
            var memberType = arrType.GetMethod("Contains");
            var call = Expression.Call(array, memberType, Operand.ToLinq(ctx));
            return Expression.Not(call);
        }

        public override void Translate(IExpressionTranslator translator)
        {
            if (translator == null)
                return;
            base.Translate(translator);
            Operand = translator.Translate(Operand) as ValueOrReference;
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

        public override void Translate(IExpressionTranslator translator)
        {
            throw new InvalidOperationException("Value and Reference must not be translated by itself!");
        }
    }

    public class Constant : ValueOrReference
    {
        public object Value { get; set; }

        public Constant(object val)
        {
            Value = Convert.ChangeType(val, Type);
        }

        public override string ToString()
        {
            if (Value is string)
                return "'" + Value + "'";
            return Value.ToString();
        }

        public override Expression ToLinq(ToLinqContext ctx)
        {
            return Expression.Constant(Value);
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
