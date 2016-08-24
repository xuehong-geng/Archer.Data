using System;
using Archer.DataSecurity.Filter;
using Archer.DataSecurity.Model;
using Archer.DataSecurity.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Archer.DataSecurity.Test
{
    [TestClass]
    public class RuleTest
    {
        [TestMethod]
        public void TestRuleManagement()
        {
            var mgr = new DataSecurityManager("test");
            var exp = Parser.ParseExpression("Sex == 'Male'");
            var rid = mgr.CreateAccessRule("test", exp, AccessType.FullAccess);
            var exp1 = Parser.ParseExpression("Sex == 'Female'");
            mgr.UpdateAccessRule(rid, exp1);
            mgr.AddRoleConstraint("Administrator", rid);
            mgr.DelRoleConstraint("Administrator", rid);
            mgr.DeleteAccessRule(rid);
        }
    }
}
