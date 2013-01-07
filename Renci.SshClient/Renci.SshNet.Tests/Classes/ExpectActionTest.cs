using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;
using System;
using System.Text.RegularExpressions;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Specifies behavior for expected expression
    /// </summary>
    [TestClass]
    public class ExpectActionTest : TestBase
    {
        /// <summary>
        ///A test for ExpectAction Constructor
        ///</summary>
        [TestMethod()]
        public void ExpectActionConstructorTest()
        {
            string expect = string.Empty; // TODO: Initialize to an appropriate value
            Action<string> action = null; // TODO: Initialize to an appropriate value
            ExpectAction target = new ExpectAction(expect, action);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ExpectAction Constructor
        ///</summary>
        [TestMethod()]
        public void ExpectActionConstructorTest1()
        {
            Regex expect = null; // TODO: Initialize to an appropriate value
            Action<string> action = null; // TODO: Initialize to an appropriate value
            ExpectAction target = new ExpectAction(expect, action);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}