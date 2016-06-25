using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography
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
        [TestMethod]
        [Ignore] // placeholder for actual test
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
        [TestMethod]
        [Ignore] // placeholder for actual test
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
