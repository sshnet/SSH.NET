using System;
using System.IO;
using System.Threading.Tasks;

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
                stream.Write(testBuffer, 0, 512);

                Assert.AreEqual(512, stream.Length);

                Assert.AreEqual(128, stream.Read(outputBuffer, 64, 128));
                
                Assert.AreEqual(384, stream.Length);

                CollectionAssert.AreEqual(new byte[64].Concat(testBuffer.Take(128)).Concat(new byte[832]), outputBuffer);
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
                Assert.AreEqual(1024, stream.Length);
                Assert.AreEqual(testBuffer[0], stream.ReadByte());
                Assert.AreEqual(1023, stream.Length);
                Assert.AreEqual(testBuffer[1], stream.ReadByte());
                Assert.AreEqual(1022, stream.Length);
            }
        }

        [TestMethod]
        public void Read()
        {
            var target = new PipeStream();
            target.WriteByte(0x0a);
            target.WriteByte(0x0d);
            target.WriteByte(0x09);

            var readBuffer = new byte[2];
            var bytesRead = target.Read(readBuffer, 0, readBuffer.Length);
            Assert.AreEqual(2, bytesRead);
            Assert.AreEqual(0x0a, readBuffer[0]);
            Assert.AreEqual(0x0d, readBuffer[1]);

            var writeBuffer = new byte[] {0x05, 0x03};
            target.Write(writeBuffer, 0, writeBuffer.Length);

            readBuffer = new byte[4];
            bytesRead = target.Read(readBuffer, 0, readBuffer.Length);
            Assert.AreEqual(3, bytesRead);
            Assert.AreEqual(0x09, readBuffer[0]);
            Assert.AreEqual(0x05, readBuffer[1]);
            Assert.AreEqual(0x03, readBuffer[2]);
            Assert.AreEqual(0x00, readBuffer[3]);
        }

        [TestMethod]
        public async Task Read_NonEmptyArray_OnlyReturnsZeroAfterDispose()
        {
            // When there is no data available, a read should block,
            // but then unblock (and return 0) after disposal.

            var pipeStream = new PipeStream();

            Task<int> readTask = pipeStream.ReadAsync(new byte[16], 0, 16);

            await Task.Delay(50);

            Assert.IsFalse(readTask.IsCompleted);

            pipeStream.Dispose();

            Assert.AreEqual(0, await readTask);
        }

        [TestMethod]
        public async Task Read_EmptyArray_OnlyReturnsZeroAfterDispose()
        {
            // Similarly, zero byte reads should still block until after disposal.

            var pipeStream = new PipeStream();

            Task<int> readTask = pipeStream.ReadAsync(Array.Empty<byte>(), 0, 0);

            await Task.Delay(50);

            Assert.IsFalse(readTask.IsCompleted);

            pipeStream.Dispose();

            Assert.AreEqual(0, await readTask);
        }

        [TestMethod]
        public async Task Read_EmptyArray_OnlyReturnsZeroWhenDataAvailable()
        {
            // And zero byte reads should block but then return 0 once data
            // is available.

            var pipeStream = new PipeStream();

            Task<int> readTask = pipeStream.ReadAsync(Array.Empty<byte>(), 0, 0);

            await Task.Delay(50);

            Assert.IsFalse(readTask.IsCompleted);

            pipeStream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);

            Assert.AreEqual(0, await readTask);
        }

        [TestMethod]
        public void Read_AfterDispose_StillWorks()
        {
            var pipeStream = new PipeStream();

            pipeStream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);

            pipeStream.Dispose();
#pragma warning disable S3966 // Objects should not be disposed more than once
            pipeStream.Dispose(); // Check that multiple Dispose is OK.
#pragma warning restore S3966 // Objects should not be disposed more than once

            Assert.AreEqual(4, pipeStream.Read(new byte[5], 0, 5));
            Assert.AreEqual(0, pipeStream.Read(new byte[5], 0, 5));
        }

        [TestMethod]
        public void SeekShouldThrowNotSupportedException()
        {
            const long offset = 0;
            const SeekOrigin origin = new SeekOrigin();
            var target = new PipeStream();

            try
            {
                _ = target.Seek(offset, origin);
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

        [TestMethod]
        public void CanReadTest()
        {
            var target = new PipeStream();
            Assert.IsTrue(target.CanRead);
        }

        [TestMethod]
        public void CanSeekTest()
        {
            var target = new PipeStream();
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
            var target = new PipeStream();
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

        [TestMethod]
        public void Position_GetterAlwaysReturnsZero()
        {
            var target = new PipeStream();

            Assert.AreEqual(0, target.Position);
            target.WriteByte(0x0a);
            Assert.AreEqual(0, target.Position);
            _ = target.ReadByte();
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
