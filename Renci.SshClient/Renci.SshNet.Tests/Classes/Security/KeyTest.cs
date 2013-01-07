using Renci.SshNet.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests
{
    /// <summary>
    ///This is a test class for KeyTest and is intended
    ///to contain all KeyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class KeyTest : TestBase
    {
        internal virtual Key CreateKey()
        {
            // TODO: Instantiate an appropriate concrete class.
            Key target = null;
            return target;
        }

        /// <summary>
        ///A test for Sign
        ///</summary>
        [TestMethod()]
        public void SignTest()
        {
            Key target = CreateKey(); // TODO: Initialize to an appropriate value
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
            Key target = CreateKey(); // TODO: Initialize to an appropriate value
            byte[] data = null; // TODO: Initialize to an appropriate value
            byte[] signature = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.VerifySignature(data, signature);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for KeyLength
        ///</summary>
        [TestMethod()]
        public void KeyLengthTest()
        {
            Key target = CreateKey(); // TODO: Initialize to an appropriate value
            int actual;
            actual = target.KeyLength;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Public
        ///</summary>
        [TestMethod()]
        public void PublicTest()
        {
            Key target = CreateKey(); // TODO: Initialize to an appropriate value
            BigInteger[] expected = null; // TODO: Initialize to an appropriate value
            BigInteger[] actual;
            target.Public = expected;
            actual = target.Public;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
