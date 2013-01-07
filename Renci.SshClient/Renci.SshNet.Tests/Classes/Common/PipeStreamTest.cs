using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using System;
using System.IO;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class PipeStreamTest : TestBase
    {
        [TestMethod]
        [TestCategory("PipeStream")]
        public void Test_PipeStream_Write_Read_Buffer()
        {
            var testBuffer = new byte[1024];
            new Random().NextBytes(testBuffer);

            var outputBuffer = new byte[1024];

            using (var stream = new PipeStream())
            {
                stream.Write(testBuffer, 0, testBuffer.Length);

                Assert.AreEqual(stream.Length, testBuffer.Length);

                stream.Read(outputBuffer, 0, outputBuffer.Length);

                Assert.AreEqual(stream.Length, 0);

                Assert.IsTrue(testBuffer.IsEqualTo(outputBuffer));
            }
        }

        [TestMethod]
        [TestCategory("PipeStream")]
        public void Test_PipeStream_Write_Read_Byte()
        {
            var testBuffer = new byte[1024];
            new Random().NextBytes(testBuffer);

            var outputBuffer = new byte[1024];

            using (var stream = new PipeStream())
            {
                stream.Write(testBuffer, 0, testBuffer.Length);

                Assert.AreEqual(stream.Length, testBuffer.Length);

                stream.ReadByte();

                Assert.AreEqual(stream.Length, testBuffer.Length - 1);

                stream.ReadByte();

                Assert.AreEqual(stream.Length, testBuffer.Length - 2);
            }
        }
        /// <summary>
        ///A test for PipeStream Constructor
        ///</summary>
        [TestMethod()]
        public void PipeStreamConstructorTest()
        {
            PipeStream target = new PipeStream();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Flush
        ///</summary>
        [TestMethod()]
        public void FlushTest()
        {
            PipeStream target = new PipeStream(); // TODO: Initialize to an appropriate value
            target.Flush();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Read
        ///</summary>
        [TestMethod()]
        public void ReadTest()
        {
            PipeStream target = new PipeStream(); // TODO: Initialize to an appropriate value
            byte[] buffer = null; // TODO: Initialize to an appropriate value
            int offset = 0; // TODO: Initialize to an appropriate value
            int count = 0; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.Read(buffer, offset, count);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Seek
        ///</summary>
        [TestMethod()]
        public void SeekTest()
        {
            PipeStream target = new PipeStream(); // TODO: Initialize to an appropriate value
            long offset = 0; // TODO: Initialize to an appropriate value
            SeekOrigin origin = new SeekOrigin(); // TODO: Initialize to an appropriate value
            long expected = 0; // TODO: Initialize to an appropriate value
            long actual;
            actual = target.Seek(offset, origin);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for SetLength
        ///</summary>
        [TestMethod()]
        public void SetLengthTest()
        {
            PipeStream target = new PipeStream(); // TODO: Initialize to an appropriate value
            long value = 0; // TODO: Initialize to an appropriate value
            target.SetLength(value);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Write
        ///</summary>
        [TestMethod()]
        public void WriteTest()
        {
            PipeStream target = new PipeStream(); // TODO: Initialize to an appropriate value
            byte[] buffer = null; // TODO: Initialize to an appropriate value
            int offset = 0; // TODO: Initialize to an appropriate value
            int count = 0; // TODO: Initialize to an appropriate value
            target.Write(buffer, offset, count);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for BlockLastReadBuffer
        ///</summary>
        [TestMethod()]
        public void BlockLastReadBufferTest()
        {
            PipeStream target = new PipeStream(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            target.BlockLastReadBuffer = expected;
            actual = target.BlockLastReadBuffer;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CanRead
        ///</summary>
        [TestMethod()]
        public void CanReadTest()
        {
            PipeStream target = new PipeStream(); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.CanRead;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CanSeek
        ///</summary>
        [TestMethod()]
        public void CanSeekTest()
        {
            PipeStream target = new PipeStream(); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.CanSeek;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CanWrite
        ///</summary>
        [TestMethod()]
        public void CanWriteTest()
        {
            PipeStream target = new PipeStream(); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.CanWrite;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Length
        ///</summary>
        [TestMethod()]
        public void LengthTest()
        {
            PipeStream target = new PipeStream(); // TODO: Initialize to an appropriate value
            long actual;
            actual = target.Length;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for MaxBufferLength
        ///</summary>
        [TestMethod()]
        public void MaxBufferLengthTest()
        {
            PipeStream target = new PipeStream(); // TODO: Initialize to an appropriate value
            long expected = 0; // TODO: Initialize to an appropriate value
            long actual;
            target.MaxBufferLength = expected;
            actual = target.MaxBufferLength;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Position
        ///</summary>
        [TestMethod()]
        public void PositionTest()
        {
            PipeStream target = new PipeStream(); // TODO: Initialize to an appropriate value
            long expected = 0; // TODO: Initialize to an appropriate value
            long actual;
            target.Position = expected;
            actual = target.Position;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

    }
}