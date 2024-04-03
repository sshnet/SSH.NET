using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;
using System;
using System.Text.RegularExpressions;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ExpectActionTest : TestBase
    {
        [TestMethod()]
        public void Constructor_StringAndAction()
        {
            var expect = new Random().Next().ToString(CultureInfo.InvariantCulture);
            Action<string> action = Console.WriteLine;

            var target = new ExpectAction(expect, action);

            Assert.IsNotNull(target.Expect);
            Assert.AreEqual(expect, target.Expect.ToString());
            Assert.AreSame(action, target.Action);
        }

        [TestMethod()]
        public void Constructor_RegexAndAction()
        {
            var expect = new Regex("^.*");
            Action<string> action = Console.WriteLine;

            var target = new ExpectAction(expect, action);

            Assert.AreSame(expect, target.Expect);
            Assert.AreSame(action, target.Action);
        }
    }
}