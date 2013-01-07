using Renci.SshNet.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests
{
    /// <summary>
    ///This is a test class for HostAlgorithmTest and is intended
    ///to contain all HostAlgorithmTest Unit Tests
    ///</summary>
    [TestClass()]
    public class HostAlgorithmTest : TestBase
    {
        internal virtual HostAlgorithm CreateHostAlgorithm()
        {
            // TODO: Instantiate an appropriate concrete class.
            HostAlgorithm target = null;
            return target;
        }

        /// <summary>
        ///A test for Sign
        ///</summary>
        [TestMethod()]
        public void SignTest()
        {
            HostAlgorithm target = CreateHostAlgorithm(); // TODO: Initialize to an appropriate value
            byte[] data = null; // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.Sign(data);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for VerifySignature
        ///</summary>
        [TestMethod()]
        public void VerifySignatureTest()
        {
            HostAlgorithm target = CreateHostAlgorithm(); // TODO: Initialize to an appropriate value
            byte[] data = null; // TODO: Initialize to an appropriate value
            byte[] signature = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.VerifySignature(data, signature);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Data
        ///</summary>
        [TestMethod()]
        public void DataTest()
        {
            HostAlgorithm target = CreateHostAlgorithm(); // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.Data;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
