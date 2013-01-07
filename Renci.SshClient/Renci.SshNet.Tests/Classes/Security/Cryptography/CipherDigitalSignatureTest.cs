using Renci.SshNet.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests
{
    /// <summary>
    ///This is a test class for CipherDigitalSignatureTest and is intended
    ///to contain all CipherDigitalSignatureTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CipherDigitalSignatureTest : TestBase
    {
        internal virtual CipherDigitalSignature CreateCipherDigitalSignature()
        {
            // TODO: Instantiate an appropriate concrete class.
            CipherDigitalSignature target = null;
            return target;
        }

        /// <summary>
        ///A test for Sign
        ///</summary>
        [TestMethod()]
        public void SignTest()
        {
            CipherDigitalSignature target = CreateCipherDigitalSignature(); // TODO: Initialize to an appropriate value
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
        [TestMethod()]
        public void VerifyTest()
        {
            CipherDigitalSignature target = CreateCipherDigitalSignature(); // TODO: Initialize to an appropriate value
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
