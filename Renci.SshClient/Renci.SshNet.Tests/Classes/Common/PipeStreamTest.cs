using System.Threading;
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
        [TestMethod]
        public void PipeStreamConstructorTest()
        {
            PipeStream target = new PipeStream();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Flush
        ///</summary>
        [TestMethod]
        public void FlushTest()
        {
            PipeStream target = new PipeStream(); // TODO: Initialize to an appropriate value
            target.Flush();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        [TestMethod]
        public void Read()
        {
            const int sleepTime = 100;

            var target = new PipeStream();
            target.WriteByte(0x0a);
            target.WriteByte(0x0d);
            target.WriteByte(0x09);

            var readBuffer = new byte[2];
            var bytesRead = target.Read(readBuffer, 0, readBuffer.Length);
            Assert.AreEqual(2, bytesRead);
            Assert.AreEqual(0x0a, readBuffer[0]);
            Assert.AreEqual(0x0d, readBuffer[1]);

            var writeToStreamThread = new Thread(
                () =>
                    {
                        Thread.Sleep(sleepTime);
                        var writeBuffer = new byte[] {0x05, 0x03};
                        target.Write(writeBuffer, 0, writeBuffer.Length);
                    });
            writeToStreamThread.Start();

            readBuffer = new byte[2];
            bytesRead = target.Read(readBuffer, 0, readBuffer.Length);
            Assert.AreEqual(2, bytesRead);
            Assert.AreEqual(0x09, readBuffer[0]);
            Assert.AreEqual(0x05, readBuffer[1]);
        }

        [TestMethod]
        public void SeekShouldThrowNotSupportedException()
        {
            const long offset = 0;
            const SeekOrigin origin = new SeekOrigin();
            var target = new PipeStream();

            try
            {
                target.Seek(offset, origin);
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
            }

        }

        [TestMethod]
        public void SetLengthShouldThrowNotSupportedException()
        {
            var target = new PipeStream();

            try
            {
                target.SetLength(1);
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
            }
        }

        [TestMethod]
        public void WriteTest()
        {
            var target = new PipeStream();

            var writeBuffer = new byte[] {0x0a, 0x05, 0x0d};
            target.Write(writeBuffer, 0, 2);

            writeBuffer = new byte[] { 0x02, 0x04, 0x03, 0x06, 0x09 };
            target.Write(writeBuffer, 1, 2);

            var readBuffer = new byte[6];
            var bytesRead = target.Read(readBuffer, 0, 4);

            Assert.AreEqual(4, bytesRead);
            Assert.AreEqual(0x0a, readBuffer[0]);
            Assert.AreEqual(0x05, readBuffer[1]);
            Assert.AreEqual(0x04, readBuffer[2]);
            Assert.AreEqual(0x03, readBuffer[3]);
            Assert.AreEqual(0x00, readBuffer[4]);
            Assert.AreEqual(0x00, readBuffer[5]);
        }

        /// <summary>
        ///A test for BlockLastReadBuffer
        ///</summary>
        [TestMethod]
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

        [TestMethod]
        public void CanReadTest()
        {
            var target = new PipeStream();
            Assert.IsTrue(target.CanRead);
        }

        [TestMethod]
        public void CanSeekTest()
        {
            var target = new PipeStream(); // TODO: Initialize to an appropriate value
            Assert.IsFalse(target.CanSeek);
        }

        [TestMethod]
        public void CanWriteTest()
        {
            var target = new PipeStream();
            Assert.IsTrue(target.CanWrite);
        }

        [TestMethod]
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
        [TestMethod]
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

        [TestMethod]
        public void Position_GetterAlwaysReturnsZero()
        {
            var target = new PipeStream();

            Assert.AreEqual(0, target.Position);
            target.WriteByte(0x0a);
            Assert.AreEqual(0, target.Position);
            target.ReadByte();
            Assert.AreEqual(0, target.Position);
        }

        [TestMethod]
        public void Position_SetterAlwaysThrowsNotSupportedException()
        {
            var target = new PipeStream();

            try
            {
                target.Position = 0;
                Assert.Fail();
            }
            catch (NotSupportedException)
            {
            }
        }
    }
}