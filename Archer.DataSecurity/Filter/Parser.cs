using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Archer.DataSecurity.Filter
{
    internal class Constants
    {
        internal const string BLANK = " ";
        internal const string RETURN = "\r";
        internal const string NEWLINE = "\n";
        internal const string TAB = "\t";
        //internal const string OPR_EVALUATE	= "=";
        internal const string OPR_PLUS = "+";
        internal const string OPR_MINUS = "-";
        internal const string OPR_MULTIPLY = "*";
        internal const string OPR_DIVIDE = "/";
        internal const string OPR_LB = "(";
        internal const string OPR_RB = ")";
        internal const string OPR_EQUAL = "==";
        internal const string OPR_BIGGER = ">";
        internal const string OPR_SMALLER = "<";
        internal const string OPR_BIGGEREQUAL = ">=";
        internal const string OPR_SMALLEREQUAL = "<=";
        internal const string OPR_NOTEQUAL = "!=";
        internal const string OPR_AND = "&&";
        internal const string OPR_OR = "||";
        internal const string OPR_NOT = "!";
        internal const string OPR_IN = "in";
        internal const string OPR_NOTIN = "not in";
    }

    // Operator's priorities
    class OprPriority
    {
        string opr;
        int priority;

        public static OprPriority[] g_OprPriorities =
        {
	        //{OPR_EVALUATE		,0},
	        new OprPriority{ opr = Constants.OPR_PLUS           , priority = 1},
            new OprPriority{ opr = Constants.OPR_MINUS          , priority = 1},
            new OprPriority{ opr = Constants.OPR_MULTIPLY       , priority = 2},
            new OprPriority{ opr = Constants.OPR_DIVIDE         , priority = 2},
            new OprPriority{ opr = Constants.OPR_LB             , priority = 9},
            new OprPriority{ opr = Constants.OPR_RB             , priority = 9},
            new OprPriority{ opr = Constants.OPR_EQUAL          , priority = 5},
            new OprPriority{ opr = Constants.OPR_BIGGER         , priority = 5},
            new OprPriority{ opr = Constants.OPR_SMALLER        , priority = 5},
            new OprPriority{ opr = Constants.OPR_BIGGEREQUAL    , priority = 5},
            new OprPriority{ opr = Constants.OPR_SMALLEREQUAL   , priority = 5},
            new OprPriority{ opr = Constants.OPR_NOTEQUAL       , priority = 5},
            new OprPriority{ opr = Constants.OPR_AND            , priority = 4},
            new OprPriority{ opr = Constants.OPR_OR             , priority = 3},
            new OprPriority{ opr = Constants.OPR_NOT            , priority = 3},
            new OprPriority{ opr = Constants.OPR_IN             , priority = 5},
            new OprPriority{ opr = Constants.OPR_NOTIN          , priority = 5},
        };

        public static int GetPriority(string opr)
        {
            for (int i = 0; i < g_OprPriorities.Length; i++)
            {
                if (opr == g_OprPriorities[i].opr)
                    return g_OprPriorities[i].priority;
            }
            return -1;
        }
    }

    public class Token
    {
        public string Text { get; set; }

        public Token()
        {
        }

        public Token(string text)
        {
            Text = text;
        }

        public Token(Token other)
        {
            Text = other.Text;
        }

        public static bool IsBlank(char cr)
        {
            string c = new string(cr,1);
            if (c == Constants.BLANK ||
                c == Constants.RETURN ||
                c == Constants.NEWLINE ||
                c == Constants.TAB)
                return true;
            else
                return false;
        }
    }

    public class Operand : Token
    {
        public Operand()
        {
        }

        public Operand(string text)
            : base(text)
        {
        }

        public Operand(Token other)
            : base(other.Text)
        {
        }

        public bool IsConst(out Type type)
        {
            string strText = Text.Trim();
            if ((strText.Substring(0, 1) == "'" &&
                 strText.Substring(strText.Length - 1, 1) == "'") ||
                (strText.Substring(0, 1) == "\"" &&
                 strText.Substring(strText.Length - 1, 1) == "\""))
            {
                // It's a string const
                type = typeof (string);
                return true;
            }
            else if (strText.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                     strText.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                // It's a boolean
                type = typeof (bool);
                return true;
            }
            else if (strText.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                // It's a null
                type = typeof (object);
                return true;
            }
            else
            {
                int iDigitCnt = 0, iDotCnt = 0, iCharCnt = 0;
                for (int i = 0; i < strText.Length; i++)
                {
                    switch (strText[i])
                    {
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            iDigitCnt++;
                            break;
                        case '.':
                            iDotCnt++;
                            break;
                        default:
                            iCharCnt++;
                            break;
                    }
                }
                if (iCharCnt < 1 && iDotCnt <= 1)
                {
                    // It's a number
                    if (iDotCnt > 0)
                        type = typeof (double);
                    else if (iDigitCnt >= 10)
                        type = typeof (long);
                    else
                        type = typeof (int);
                    return true;
                }
                else
                {
                    // It's not a number, must be a special variable or sub expression
                    type = typeof (object);
                    return false;
                }
            }
        }

        public object GetConst()
        {
            Type vt;
            if (!IsConst(out vt))
            {
                throw new InvalidOperationException("Invalid constant!");
            }

            string str = Text.Trim();
            if (vt == typeof (string))
            {
                str = str.Substring(1, str.Length - 2);
                return str;
            }
            else if (vt == typeof (bool))
            {
                return str.Equals("true", StringComparison.OrdinalIgnoreCase) ? true : false;
            }
            else if (str.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            else
            {
                return Convert.ChangeType(str, vt);
            }
        }

        public bool IsSet()
        {
            string strText = Text.Trim();
            if (strText.Substring(0, 1) == "[" &&
                strText.Substring(strText.Length - 1, 1) == "]")
            {
                return true;
            }
            return false;
        }

        public bool IsNull()
        {
            return "null".Equals(Text, StringComparison.OrdinalIgnoreCase);
        }
    }

    public class Operator : Token
    {
        public Operator()
        {
        }

        public Operator(string text)
            : base(text)
        {
        }

        public Operator(Token other)
            : base(other.Text)
        {
        }

        public static string StartWithOperator(string str)
        {
            if (str.StartsWith(Constants.OPR_EQUAL)) return Constants.OPR_EQUAL;
            //else if(str.Find(OPR_EVALUATE)) return Constants.OPR_EVALUATE;
            else if (str.StartsWith(Constants.OPR_BIGGEREQUAL)) return Constants.OPR_BIGGEREQUAL;
            else if (str.StartsWith(Constants.OPR_SMALLEREQUAL)) return Constants.OPR_SMALLEREQUAL;
            else if (str.StartsWith(Constants.OPR_NOTEQUAL)) return Constants.OPR_NOTEQUAL;
            else if (str.StartsWith(Constants.OPR_AND)) return Constants.OPR_AND;
            else if (str.StartsWith(Constants.OPR_OR)) return Constants.OPR_OR;
            else if (str.StartsWith(Constants.OPR_PLUS)) return Constants.OPR_PLUS;
            else if (str.StartsWith(Constants.OPR_MINUS)) return Constants.OPR_MINUS;
            else if (str.StartsWith(Constants.OPR_MULTIPLY)) return Constants.OPR_MULTIPLY;
            else if (str.StartsWith(Constants.OPR_DIVIDE)) return Constants.OPR_DIVIDE;
            else if (str.StartsWith(Constants.OPR_LB)) return Constants.OPR_LB;
            else if (str.StartsWith(Constants.OPR_RB)) return Constants.OPR_RB;
            else if (str.StartsWith(Constants.OPR_BIGGER)) return Constants.OPR_BIGGER;
            else if (str.StartsWith(Constants.OPR_SMALLER)) return Constants.OPR_SMALLER;
            else if (str.StartsWith(Constants.OPR_NOT)) return Constants.OPR_NOT;
            else if (str.StartsWith(Constants.OPR_IN, StringComparison.OrdinalIgnoreCase)) return Constants.OPR_IN;
            else if (str.StartsWith(Constants.OPR_NOTIN, StringComparison.OrdinalIgnoreCase)) return Constants.OPR_NOTIN;
            else
                return null;
        }

        public bool IsHigherThan(Operator other)
        {
            return OprPriority.GetPriority(Text) > OprPriority.GetPriority(other.Text);
        }

        public bool IsSingleOperand()
        {
            if (Text == Constants.OPR_NOT)
                return true;
            else
                return false;
        }
    }

    public static class Parser
    {
        private static Token GetNext(string exp, int iTotal, ref int iPos)
        {
            if (iPos >= iTotal)
                return null;

            int iCur = iPos;
            if (Token.IsBlank(exp[iCur]))
            {   // Start with blank char, overcome all black chars
                while (iCur < iTotal && Token.IsBlank(exp[iCur]))
                    iCur++;
                if (iCur >= iTotal)
                    return null;
            }
            int iLast = iCur;
            string strItem = "";
            bool bOpr = false;
            if (exp[iLast].ToString() == Constants.OPR_LB ||
                exp[iLast].ToString() == Constants.OPR_RB)
            {   // Start with bracket
                iLast++;
                iPos = iLast;
                return new Operator(exp.Substring(iCur, 1));
            }
            else if (null != (strItem = Operator.StartWithOperator(exp.Substring(iCur))))
            {   // Start with operator, we pick a operator this time
                iLast = iCur + strItem.Length;
                bOpr = true;
            }
            else
            {   // Not start with operator, we pick a operand this time
                while (iLast < iTotal && (!Token.IsBlank(exp[iLast]) && null == Operator.StartWithOperator(exp.Substring(iLast))))
                    iLast++;
                strItem = exp.Substring(iCur, iLast - iCur);
            }
            iPos = iLast;

            strItem = strItem.Trim();
            if (string.IsNullOrEmpty(strItem))
                return null;

            if (bOpr)
                return new Operator(strItem);
            else
                return new Operand(strItem);
        }

        private static List<Token> Infix2Suffix(List<Token> Infix)
        {
            // a + b - (c + d - 9)/23 * ((32 + c) * (33 - b))
            // ab+cd+9-23/32c+*33b-*-
            List<Token> Suffix = new List<Token>();
            List<Operator> oprStack = new List<Operator>();
            while (Infix.Count > 0)
            {
                Token pItem = Infix[0]; Infix.RemoveAt(0);
                if (pItem is Operand)
                {   // A operand, append to suffix
                    Suffix.Add(pItem);
                }
                else if (pItem is Operator)
                {   // A operator
                    Operator pOpr = (Operator)pItem;
                    if (pOpr.Text == Constants.OPR_LB)
                    {   // Left bracket, push to operator stack
                        oprStack.Add(pOpr);
                    }
                    else if (pOpr.Text == Constants.OPR_RB)
                    {   // Right bracket, pop out all operator to suffix until a LB
                        Operator pLast = null;
                        if (oprStack.Count < 1)
                            pLast = null;
                        else
                        {
                            pLast = oprStack.Last();
                            oprStack.RemoveAt(oprStack.Count-1);
                        }
                        if (pLast == null)
                            throw new InvalidExpressionException("Right bracket has no left bracket or content!");
                        while (pLast.Text != Constants.OPR_LB)
                        {
                            Suffix.Add(pLast);
                            if (oprStack.Count < 1)
                            {   // End before match a LB, syntax error
                                throw new InvalidExpressionException("Left bracket was missing!");
                            }
                            pLast = oprStack.Last();
                            oprStack.RemoveAt(oprStack.Count - 1);
                        }
                    }
                    else
                    {   // Other operators
                        Operator pLast = oprStack.Count < 1 ? null : oprStack.Last();
                        if (pLast == null)
                            oprStack.Add(pOpr);
                        else
                        {
                            while (pLast.Text != Constants.OPR_LB && !pOpr.IsHigherThan(pLast))
                            {   // New operator is not higher than old one, pop out old one
                                oprStack.RemoveAt(oprStack.Count-1);
                                Suffix.Add(pLast);
                                if (oprStack.Count < 1)
                                    break;
                                pLast = oprStack.Last();
                            }
                            oprStack.Add(pOpr);
                        }
                    }
                }
            }
            // Handle lasted operators
            while (oprStack.Count > 0)
            {
                Suffix.Add(oprStack.Last());
                oprStack.RemoveAt(oprStack.Count-1);
            }

            return Suffix;
        }

        private static Binary CreateOperator(Operator opr)
        {
            if (opr.Text == Constants.OPR_EQUAL)
                return new Equals();
            else if(opr.Text == Constants.OPR_NOTEQUAL)
                return new NotEquals();
            else if(opr.Text == Constants.OPR_AND)
                return new And();
            else if(opr.Text == Constants.OPR_OR)
                return new Or();
            else if (opr.Text == Constants.OPR_IN)
                return new In();
            else if (opr.Text == Constants.OPR_NOTIN)
                return new NotIn();
            else
            {
                throw new NotSupportedException("Not supported operator!");
            }
        }

        private static Item CreateOperand(Operand opr)
        {
            Type type;
            if (opr.IsConst(out type))
            {
                return new Constant(opr.GetConst());
            }
            else if (opr.IsSet())
            {
                // Create a set
                string txt = opr.Text.Trim();
                string list = txt.Substring(1, txt.Length - 2); // Remove [ and ]
                var items = list.Split(',');
                var set = new Set(typeof(Array));
                foreach (var item in items)
                {
                    var operand = new Operand(item.Trim());
                    var i = CreateOperand(operand);
                    set.Items.Add(i as ValueOrReference);
                }
                return set;
            }
            else
            {
                return new Variable(type, opr.Text);
            }
        }

        public static Item CreateExpressionTree(List<Token> suffix)
        {
            Item tree = null;
            Stack<Binary> stack = new Stack<Binary>();
            for (int i = suffix.Count - 1; i >= 0; i--)
            {
                var token = suffix[i];
                if (token is Operator)
                {
                    var b = CreateOperator(token as Operator);
                    if (tree == null)
                        tree = b;
                    stack.Push(b);
                }
                else if (token is Operand)
                {
                    var item = CreateOperand(token as Operand);
                    if (stack.Count < 1)
                        throw new InvalidOperationException("Stack is empty! Invalid expression!");
                    var b = stack.Peek();
                    if (b.Right == null)
                        b.Right = item;
                    else if (b.Left == null)
                        b.Left = item;
                    var p = b;
                    while (p.Left != null && p.Right != null)
                    {
                        // This exp is completed, must be insert into last one
                        if (stack.Count > 0)
                        {
                            p = stack.Pop();
                            if (stack.Count < 1)
                                break;
                            var parent = stack.Peek();
                            if (parent.Right == null)
                                parent.Right = p;
                            else if (parent.Left == null)
                                parent.Left = p;
                            else
                            {
                                throw new InvalidOperationException("Parent expression has no more zoom!");
                            }
                            if (parent.Left != null && parent.Right != null)
                            {
                                p = stack.Peek();
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                        }
                    }
                }
            }
            return tree;
        }

        public static List<Token> Parse(string exp)
        {
            List<Token> stack = new List<Token>();
            int iPos = 0;
            if (string.IsNullOrEmpty(exp))
                throw new ArgumentNullException(nameof(exp));
            int iTotal = exp.Length;
            // Pick operand and operator and construct infix expression
            while (iPos < iTotal)
            {
                // Find next item
                Token pItem = GetNext(exp, iTotal, ref iPos);
                if (pItem == null)
                    break;
                if (pItem is Operand)
                {   // Next one is a operand
                    if (stack.Count > 0 && stack[stack.Count-1] is Operand)
                    {   // Last one is a operand too, syntax failed
                        throw new InvalidExpressionException(
                            string.Format("Operator missing between two operands! Last operands : {0}.", pItem.Text));
                    }
                }
                else if (pItem is Operator)
                {   // Next one is a operator
                    //if(Stack.GetCount() > 0 && Stack.GetTail()->IsKindOf(RUNTIME_CLASS(COperator)))
                    //{	// Last one is a operator too, syntax failed
                    //	delete pItem;
                    //	return FALSE;
                    //}
                }
                stack.Add(pItem);
            }
            // Convert infix express to suffix express
            return Infix2Suffix(stack);
        }

        public static Item ParseExpression(string exp)
        {
            var tokens = Parse(exp);
            return CreateExpressionTree(tokens);
        }
    }
}
