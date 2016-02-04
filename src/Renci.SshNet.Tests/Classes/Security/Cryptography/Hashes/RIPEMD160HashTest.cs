using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography
{
    /// <summary>
    ///This is a test class for RIPEMD160HashTest and is intended
    ///to contain all RIPEMD160HashTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RIPEMD160HashTest : TestBase
    {
        /// <summary>
        ///A test for RIPEMD160Hash Constructor
        ///</summary>
        [TestMethod()]
        public void RIPEMD160HashConstructorTest()
        {
            RIPEMD160Hash target = new RIPEMD160Hash();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Initialize
        ///</summary>
        [TestMethod()]
        public void InitializeTest()
        {
            RIPEMD160Hash target = new RIPEMD160Hash(); // TODO: Initialize to an appropriate value
            target.Initialize();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for CanReuseTransform
        ///</summary>
        [TestMethod()]
        public void CanReuseTransformTest()
        {
            RIPEMD160Hash target = new RIPEMD160Hash();
            Assert.IsTrue(target.CanReuseTransform);
        }

        /// <summary>
        ///A test for CanTransformMultipleBlocks
        ///</summary>
        [TestMethod()]
        public void CanTransformMultipleBlocksTest()
        {
            RIPEMD160Hash target = new RIPEMD160Hash();
            Assert.IsTrue(target.CanTransformMultipleBlocks);
        }

        /// <summary>
        ///A test for HashSize
        ///</summary>
        [TestMethod()]
        public void HashSizeTest()
        {
            RIPEMD160Hash target = new RIPEMD160Hash();
            Assert.AreEqual(target.HashSize, 160);
        }

        /// <summary>
        ///A test for InputBlockSize
        ///</summary>
        [TestMethod()]
        public void InputBlockSizeTest()
        {
            RIPEMD160Hash target = new RIPEMD160Hash();
            Assert.AreEqual(target.InputBlockSize, 64);
        }

        /// <summary>
        ///A test for OutputBlockSize
        ///</summary>
        [TestMethod()]
        public void OutputBlockSizeTest()
        {
            RIPEMD160Hash target = new RIPEMD160Hash();
            Assert.AreEqual(target.OutputBlockSize, 64);
        }

        [TestMethod()]
        public void ComputeHash1()
        {
            RIPEMD160Hash target = new RIPEMD160Hash();
            var input = ASCIIEncoding.ASCII.GetBytes("a");
            var expeceted = new byte[] { 0x0b, 0xdc, 0x9d, 0x2d, 0x25, 0x6b, 0x3e, 0xe9, 0xda, 0xae, 0x34, 0x7b, 0xe6, 0xf4, 0xdc, 0x83, 0x5a, 0x46, 0x7f, 0xfe };
            var output = target.ComputeHash(input);
            Assert.IsTrue(output.SequenceEqual(expeceted));
        }


        [TestMethod()]
        public void ComputeHash2()
        {
            RIPEMD160Hash target = new RIPEMD160Hash();
            var input = ASCIIEncoding.ASCII.GetBytes("abc");
            var expeceted = new byte[] { 0x8e, 0xb2, 0x08, 0xf7, 0xe0, 0x5d, 0x98, 0x7a, 0x9b, 0x04, 0x4a, 0x8e, 0x98, 0xc6, 0xb0, 0x87, 0xf1, 0x5a, 0x0b, 0xfc };
            var output = target.ComputeHash(input);
            Assert.IsTrue(output.SequenceEqual(expeceted));
        }


        [TestMethod()]
        public void ComputeHash3()
        {
            RIPEMD160Hash target = new RIPEMD160Hash();
            var input = ASCIIEncoding.ASCII.GetBytes("message digest");
            var expeceted = new byte[] { 0x5d, 0x06, 0x89, 0xef, 0x49, 0xd2, 0xfa, 0xe5, 0x72, 0xb8, 0x81, 0xb1, 0x23, 0xa8, 0x5f, 0xfa, 0x21, 0x59, 0x5f, 0x36 };
            var output = target.ComputeHash(input);
            Assert.IsTrue(output.SequenceEqual(expeceted));
        }

    }
}
