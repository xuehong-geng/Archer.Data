using System;
using Archer.DataSecurity.Filter;
using Archer.DataSecurity.Service;
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
            var tokens = Parser.Parse(exp);
            foreach (var token in tokens)
            {
                Console.Write(token.Text + " ");
            }
            Console.WriteLine("");
            var tree = Parser.CreateExpressionTree(tokens);
            Console.WriteLine(tree.ToString());
        }

        [TestMethod]
        public void TestEntityFilter()
        {
            var ctx = new SchoolDbContext();
            var filter = ctx.Students.Filter("Sex == 'Male' && Email == 'gxh@foxmail.com'");
            Console.WriteLine(filter.ToString());
        }
    }
}
