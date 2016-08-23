
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archer.DataSecurity.Expression
{
    public class Item
    {
        public override string ToString()
        {
            return "?";
        }
    }

    public class Binary : Item
    {
        public Item Left { get; set; }
        public Item Right { get; set; }

        public override string ToString()
        {
            return "(" + Left + " ? " + ")";
        }
    }

    public class Equals : Binary
    {
        public override string ToString()
        {
            return "(" + Left + " == " + Right + ")";
        }
    }

    public class And : Binary
    {
        public override string ToString()
        {
            return "(" + Left + " && " + Right + ")";
        }
    }

    public class Or : Binary
    {
        public override string ToString()
        {
            return "(" + Left + " || " + Right + ")";
        }
    }

    public class Set : Item
    {
        private Collection<Item> _items = new Collection<Item>();

        public Collection<Item> Items { get { return _items; } }

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
    }

    public class In : Set
    {
        public Item Operand { get; set; }

        public override string ToString()
        {
            return "(" + Operand + " IN " + base.ToString() + ")";
        }
    }

    public class NotIn : Set
    {
        public Item Operand { get; set; }

        public override string ToString()
        {
            return "(" + Operand + " IN " + base.ToString() + ")";
        }
    }

    public class Constant<T> : Item
    {
        public T Value { get; set; }

        public Constant(T val)
        {
            Value = val;
        }

        public Constant(object val)
        {
            Value = (T)Convert.ChangeType(val, typeof (T));
        } 

        public override string ToString()
        {
            if (Value is string)
                return "\"" + Value + "\"";
            else
            {
                return Value.ToString();
            }
        }
    }

    public class Variable<T> : Item
    {
        public string Name { get; set; }
        public T Value { get; set; }
        public Type Type { get { return typeof (T); } }

        public override string ToString()
        {
            return Name;
        }
    }
}
