using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography
{
    /// <summary>
    /// Implements DSA digital signature algorithm.
    /// </summary>
    [TestClass]
    public class DsaDigitalSignatureTest : TestBase
    {
        /// <summary>
        ///A test for DsaDigitalSignature Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DsaDigitalSignatureConstructorTest()
        {
            DsaKey key = null; // TODO: Initialize to an appropriate value
            DsaDigitalSignature target = new DsaDigitalSignature(key);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DisposeTest()
        {
            DsaKey key = null; // TODO: Initialize to an appropriate value
            DsaDigitalSignature target = new DsaDigitalSignature(key); // TODO: Initialize to an appropriate value
            target.Dispose();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Sign
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SignTest()
        {
            DsaKey key = null; // TODO: Initialize to an appropriate value
            DsaDigitalSignature target = new DsaDigitalSignature(key); // TODO: Initialize to an appropriate value
            byte[] input = null; // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.Sign(input);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Verify
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void VerifyTest()
        {
            DsaKey key = null; // TODO: Initialize to an appropriate value
            DsaDigitalSignature target = new DsaDigitalSignature(key); // TODO: Initialize to an appropriate value
            byte[] input = null; // TODO: Initialize to an appropriate value
            byte[] signature = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Verify(input, signature);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}