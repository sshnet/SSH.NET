using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography
{
    /// <summary>
    /// SHA256 algorithm implementation.
    /// </summary>
    [TestClass]
    public class SHA256HashTest : TestBase
    {
        /// <summary>
        ///A test for SHA256Hash Constructor
        ///</summary>
        [TestMethod()]
        public void SHA256HashConstructorTest()
        {
            SHA256Hash target = new SHA256Hash();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Initialize
        ///</summary>
        [TestMethod()]
        public void InitializeTest()
        {
            SHA256Hash target = new SHA256Hash(); // TODO: Initialize to an appropriate value
            target.Initialize();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for CanReuseTransform
        ///</summary>
        [TestMethod()]
        public void CanReuseTransformTest()
        {
            SHA256Hash target = new SHA256Hash(); // TODO: Initialize to an appropriate value
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
            SHA256Hash target = new SHA256Hash(); // TODO: Initialize to an appropriate value
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
            SHA256Hash target = new SHA256Hash(); // TODO: Initialize to an appropriate value
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
            SHA256Hash target = new SHA256Hash(); // TODO: Initialize to an appropriate value
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
            SHA256Hash target = new SHA256Hash(); // TODO: Initialize to an appropriate value
            int actual;
            actual = target.OutputBlockSize;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}