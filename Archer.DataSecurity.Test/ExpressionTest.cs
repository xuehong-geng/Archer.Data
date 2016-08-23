using System;
using Archer.DataSecurity.Expression;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Archer.DataSecurity.Test
{
    [TestClass]
    public class ExpressionTest
    {
        [TestMethod]
        public void TestExpressionParser()
        {
            string exp = "((a == 0) && ((b == 2) || (c == 'dde')))";
            //string exp = "a == 0 && (b == 2 || c == 'dde')";
            Console.WriteLine(exp);
            var p = new Parser();
            var tokens = p.Parse(exp);
            foreach (var token in tokens)
            {
                Console.Write(token.Text + " ");
            }
            Console.WriteLine("");
            var tree = p.CreateExpressionTree(tokens);
            Console.WriteLine(tree.ToString());
        }
    }
}
