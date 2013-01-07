using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography
{
    /// <summary>
    /// SHA1 algorithm implementation
    /// </summary>
    [TestClass]
    public class SHA1HashTest : TestBase
    {
        /// <summary>
        ///A test for SHA1Hash Constructor
        ///</summary>
        [TestMethod()]
        public void SHA1HashConstructorTest()
        {
            SHA1Hash target = new SHA1Hash();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Initialize
        ///</summary>
        [TestMethod()]
        public void InitializeTest()
        {
            SHA1Hash target = new SHA1Hash(); // TODO: Initialize to an appropriate value
            target.Initialize();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for CanReuseTransform
        ///</summary>
        [TestMethod()]
        public void CanReuseTransformTest()
        {
            SHA1Hash target = new SHA1Hash(); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.CanReuseTransform;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CanTransformMultipleBlocks
        ///</summary>
        [TestMethod()]
        public void CanTransformMultipleBlocksTest()
        {
            SHA1Hash target = new SHA1Hash(); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.CanTransformMultipleBlocks;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for HashSize
        ///</summary>
        [TestMethod()]
        public void HashSizeTest()
        {
            SHA1Hash target = new SHA1Hash(); // TODO: Initialize to an appropriate value
            int actual;
            actual = target.HashSize;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for InputBlockSize
        ///</summary>
        [TestMethod()]
        public void InputBlockSizeTest()
        {
            SHA1Hash target = new SHA1Hash(); // TODO: Initialize to an appropriate value
            int actual;
            actual = target.InputBlockSize;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for OutputBlockSize
        ///</summary>
        [TestMethod()]
        public void OutputBlockSizeTest()
        {
            SHA1Hash target = new SHA1Hash(); // TODO: Initialize to an appropriate value
            int actual;
            actual = target.OutputBlockSize;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}