using Renci.SshNet.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Compression
{   
    /// <summary>
    ///This is a test class for CompressorTest and is intended
    ///to contain all CompressorTest Unit Tests
    ///</summary>
    [TestClass]
    [Ignore] // placeholders only
    public class CompressorTest : TestBase
    {
        internal virtual Compressor CreateCompressor()
        {
            // TODO: Instantiate an appropriate concrete class.
            Compressor target = null;
            return target;
        }

        /// <summary>
        ///A test for Compress
        ///</summary>
        [TestMethod()]
        public void CompressTest()
        {
            Compressor target = CreateCompressor(); // TODO: Initialize to an appropriate value
            byte[] data = null; // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.Compress(data);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Decompress
        ///</summary>
        [TestMethod()]
        public void DecompressTest()
        {
            Compressor target = CreateCompressor(); // TODO: Initialize to an appropriate value
            byte[] data = null; // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.Decompress(data);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod()]
        public void DisposeTest()
        {
            Compressor target = CreateCompressor(); // TODO: Initialize to an appropriate value
            target.Dispose();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Init
        ///</summary>
        [TestMethod()]
        public void InitTest()
        {
            Compressor target = CreateCompressor(); // TODO: Initialize to an appropriate value
            Session session = null; // TODO: Initialize to an appropriate value
            target.Init(session);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}
