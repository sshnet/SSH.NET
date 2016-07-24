using Renci.SshNet.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Compression
{   
    /// <summary>
    ///This is a test class for ZlibStreamTest and is intended
    ///to contain all ZlibStreamTest Unit Tests
    ///</summary>
    [TestClass]
    public class ZlibStreamTest : TestBase
    {
        /// <summary>
        ///A test for ZlibStream Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void ZlibStreamConstructorTest()
        {
            Stream stream = null; // TODO: Initialize to an appropriate value
            CompressionMode mode = new CompressionMode(); // TODO: Initialize to an appropriate value
            ZlibStream target = new ZlibStream(stream, mode);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Write
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void WriteTest()
        {
            Stream stream = null; // TODO: Initialize to an appropriate value
            CompressionMode mode = new CompressionMode(); // TODO: Initialize to an appropriate value
            ZlibStream target = new ZlibStream(stream, mode); // TODO: Initialize to an appropriate value
            byte[] buffer = null; // TODO: Initialize to an appropriate value
            int offset = 0; // TODO: Initialize to an appropriate value
            int count = 0; // TODO: Initialize to an appropriate value
            target.Write(buffer, offset, count);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}
