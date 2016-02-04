using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Security.Cryptography.Ciphers.Paddings;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography.Ciphers.Paddings
{
    /// <summary>
    ///This is a test class for PKCS5PaddingTest and is intended
    ///to contain all PKCS5PaddingTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PKCS5PaddingTest : TestBase
    {
        /// <summary>
        ///A test for Pad
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PadTest()
        {
            PKCS5Padding target = new PKCS5Padding(); // TODO: Initialize to an appropriate value
            int blockSize = 0; // TODO: Initialize to an appropriate value
            byte[] input = null; // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.Pad(blockSize, input);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for PKCS5Padding Constructor
        ///</summary>
        [TestMethod()]
        public void PKCS5PaddingConstructorTest()
        {
            PKCS5Padding target = new PKCS5Padding();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
