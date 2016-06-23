using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography.Ciphers
{
    /// <summary>
    ///This is a test class for CipherPaddingTest and is intended
    ///to contain all CipherPaddingTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CipherPaddingTest : TestBase
    {
        internal virtual CipherPadding CreateCipherPadding()
        {
            // TODO: Instantiate an appropriate concrete class.
            CipherPadding target = null;
            return target;
        }

        /// <summary>
        ///A test for Pad
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PadTest()
        {
            CipherPadding target = CreateCipherPadding(); // TODO: Initialize to an appropriate value
            int blockSize = 0; // TODO: Initialize to an appropriate value
            byte[] input = null; // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.Pad(blockSize, input);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
