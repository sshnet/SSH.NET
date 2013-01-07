using Renci.SshNet.Security.Cryptography.Ciphers.Paddings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests
{
    /// <summary>
    ///This is a test class for PKCS7PaddingTest and is intended
    ///to contain all PKCS7PaddingTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PKCS7PaddingTest : TestBase
    {
        /// <summary>
        ///A test for Pad
        ///</summary>
        [TestMethod()]
        public void PadTest()
        {
            PKCS7Padding target = new PKCS7Padding(); // TODO: Initialize to an appropriate value
            int blockSize = 0; // TODO: Initialize to an appropriate value
            byte[] input = null; // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.Pad(blockSize, input);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for PKCS7Padding Constructor
        ///</summary>
        [TestMethod()]
        public void PKCS7PaddingConstructorTest()
        {
            PKCS7Padding target = new PKCS7Padding();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
