using System;
using System.IO;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

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

                Assert.AreEqual(testBuffer.Length, stream.Length);

                _ = stream.Read(outputBuffer, 0, outputBuffer.Length);

                Assert.AreEqual(0, stream.Length);

                CollectionAssert.AreEqual(testBuffer, outputBuffer);
            }
        }

        [TestMethod]
        [TestCategory("PipeStream")]
        public void Test_PipeStream_Write_Read_Byte()
        {
            var testBuffer = new byte[1024];
            new Random().NextBytes(testBuffer);

            using (var stream = new PipeStream())
            {
                stream.Write(testBuffer, 0, testBuffer.Length);
                Assert.AreEqual(testBuffer.Length, stream.Length);
                _ = stream.ReadByte();
                Assert.AreEqual(testBuffer.Length - 1, stream.Length);
                _ = stream.ReadByte();
                Assert.AreEqual(testBuffer.Length - 2, stream.Length);
            }
        }

        [TestMethod]
        public void Read()
        {
            const int sleepTime = 100;

            using (var target = new PipeStream())
            {
                target.WriteByte(0x0a);
                target.WriteByte(0x0d);
                target.WriteByte(0x09);

                var readBuffer = new byte[2];
                var bytesRead = target.Read(readBuffer, 0, readBuffer.Length);
                Assert.AreEqual(2, bytesRead);
                Assert.AreEqual(0x0a, readBuffer[0]);
                Assert.AreEqual(0x0d, readBuffer[1]);

                var writeToStreamThread = new Thread(() =>
                    {
                        Thread.Sleep(sleepTime);
                        var writeBuffer = new byte[] { 0x05, 0x03 };
                        target.Write(writeBuffer, 0, writeBuffer.Length);
                    });
                writeToStreamThread.Start();

                readBuffer = new byte[2];
                bytesRead = target.Read(readBuffer, 0, readBuffer.Length);
                Assert.AreEqual(2, bytesRead);
                Assert.AreEqual(0x09, readBuffer[0]);
                Assert.AreEqual(0x05, readBuffer[1]);
            }
        }

        [TestMethod]
        public void SeekShouldThrowNotSupportedException()
        {
            const long offset = 0;
            const SeekOrigin origin = default;

            using (var target = new PipeStream())
            {
                try
                {
                    _ = target.Seek(offset, origin);
                    Assert.Fail();
                }
                catch (NotSupportedException ex)
                {
                    Assert.IsNull(ex.InnerException);
                }
            }
        }

        [TestMethod]
        public void SetLengthShouldThrowNotSupportedException()
        {
            using (var target = new PipeStream())
            {
                try
                {
                    target.SetLength(1);
                    Assert.Fail();
                }
                catch (NotSupportedException ex)
                {
                    Assert.IsNull(ex.InnerException);
                }
            }
        }

        [TestMethod]
        public void WriteTest()
        {
            using (var target = new PipeStream())
            {
                var writeBuffer = new byte[] { 0x0a, 0x05, 0x0d };
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
        }

        [TestMethod]
        public void CanReadTest()
        {
            using (var target = new PipeStream())
            {
                Assert.IsTrue(target.CanRead);
            }
        }

        [TestMethod]
        public void CanSeekTest()
        {
            using (var target = new PipeStream())
            {
                Assert.IsFalse(target.CanSeek);
            }
        }

        [TestMethod]
        public void CanWriteTest()
        {
            using (var target = new PipeStream())
            {
                Assert.IsTrue(target.CanWrite);
            }
        }

        [TestMethod]
        public void LengthTest()
        {
            using (var target = new PipeStream())
            {
                Assert.AreEqual(0L, target.Length);
                target.Write(new byte[] { 0x0a, 0x05, 0x0d }, 0, 2);
                Assert.AreEqual(2L, target.Length);
                target.WriteByte(0x0a);
                Assert.AreEqual(3L, target.Length);
                _ = target.Read(new byte[2], 0, 2);
                Assert.AreEqual(1L, target.Length);
                _ = target.ReadByte();
                Assert.AreEqual(0L, target.Length);
            }
        }

        /// <summary>
        /// A test for MaxBufferLength
        /// </summary>
        [TestMethod]
        public void MaxBufferLengthTest()
        {
            using (var target = new PipeStream())
            {
                Assert.AreEqual(200 * 1024 * 1024, target.MaxBufferLength);
                target.MaxBufferLength = 0L;
                Assert.AreEqual(0L, target.MaxBufferLength);
            }
        }

        [TestMethod]
        public void Position_GetterAlwaysReturnsZero()
        {
            using (var target = new PipeStream())
            {
                Assert.AreEqual(0, target.Position);
                target.WriteByte(0x0a);
                Assert.AreEqual(0, target.Position);
                _ = target.ReadByte();
                Assert.AreEqual(0, target.Position);
            }
        }

        [TestMethod]
        public void Position_SetterAlwaysThrowsNotSupportedException()
        {
            using (var target = new PipeStream())
            {
                try
                {
                    target.Position = 0;
                    Assert.Fail();
                }
                catch (NotSupportedException ex)
                {
                    Assert.IsNull(ex.InnerException);
                }
            }
        }
    }
}
