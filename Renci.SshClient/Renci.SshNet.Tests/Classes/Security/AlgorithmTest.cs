using Renci.SshNet.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests
{
    /// <summary>
    ///This is a test class for AlgorithmTest and is intended
    ///to contain all AlgorithmTest Unit Tests
    ///</summary>
    [TestClass()]
    public class AlgorithmTest : TestBase
    {
        internal virtual Algorithm CreateAlgorithm()
        {
            // TODO: Instantiate an appropriate concrete class.
            Algorithm target = null;
            return target;
        }

        /// <summary>
        ///A test for Name
        ///</summary>
        [TestMethod()]
        public void NameTest()
        {
            Algorithm target = CreateAlgorithm(); // TODO: Initialize to an appropriate value
            string actual;
            actual = target.Name;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
