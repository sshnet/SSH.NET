using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

#if NETFRAMEWORK
using Renci.SshNet.Abstractions;
#endif
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class PipeStreamTest : TestBase
    {
        [TestMethod]
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

            var writeBuffer = new byte[] { 0x05, 0x03 };
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

            await pipeStream.DisposeAsync();

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

            await pipeStream.DisposeAsync();

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

            await pipeStream.WriteAsync(new byte[] { 1, 2, 3, 4 }, 0, 4);

            Assert.AreEqual(0, await readTask);
        }

#if NET
        [TestMethod]
        public async Task Read_EmptySpan_OnlyReturnsZeroWhenDataAvailable()
        {
            // And zero byte reads should block but then return 0 once data
            // is available (the span version).

            var pipeStream = new PipeStream();

            ValueTask<int> readTask = pipeStream.ReadAsync(Memory<byte>.Empty);

            await Task.Delay(50);

            Assert.IsFalse(readTask.IsCompleted);

            await pipeStream.WriteAsync(new byte[] { 1, 2, 3, 4 });

            Assert.AreEqual(0, await readTask);
        }
#endif

        [TestMethod]
        public async Task ReadAsync_DataAvailable_CompletesSynchronously()
        {
            var pipeStream = new PipeStream();

            pipeStream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);

            var readBuffer = new byte[3];

            Task<int> readTask = pipeStream.ReadAsync(readBuffer, 0, 3);

            Assert.IsTrue(readTask.IsCompleted);
            Assert.AreEqual(3, await readTask);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, readBuffer);

            readTask = pipeStream.ReadAsync(readBuffer, 0, 3);

            Assert.IsTrue(readTask.IsCompleted);
            Assert.AreEqual(1, await readTask);
            Assert.AreEqual(4, readBuffer[0]);

            // No data left, so the next read should block.
            readTask = pipeStream.ReadAsync(readBuffer, 0, 3);

            await Task.Delay(50);

            Assert.IsFalse(readTask.IsCompleted);

            await pipeStream.WriteAsync(new byte[] { 5 }, 0, 1);

            Assert.AreEqual(1, await readTask);
            Assert.AreEqual(5, readBuffer[0]);
        }

        [TestMethod]
        public async Task ReadAsync_AfterDispose_CompletesSynchronously()
        {
            var pipeStream = new PipeStream();

            pipeStream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);

            pipeStream.Dispose();

            Task<int> readTask = pipeStream.ReadAsync(new byte[5], 0, 5);

            Assert.IsTrue(readTask.IsCompleted);
            Assert.AreEqual(4, await readTask);

            readTask = pipeStream.ReadAsync(new byte[5], 0, 5);

            Assert.IsTrue(readTask.IsCompleted);
            Assert.AreEqual(0, await readTask);
        }

        [TestMethod]
        public async Task ReadAsync_DataAvailable_CancelledToken_TaskIsCancelled()
        {
            var pipeStream = new PipeStream();

            pipeStream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);

            var token = new CancellationToken(canceled: true);

            Task<int> readTask = pipeStream.ReadAsync(new byte[4], 0, 4, token);

            Assert.IsTrue(readTask.IsCanceled);

            var ex = await Assert.ThrowsAsync<OperationCanceledException>(() => readTask);

            Assert.AreEqual(token, ex.CancellationToken);
            Assert.AreEqual(4, pipeStream.Length);
        }

#if NET
        [TestMethod]
        public async Task ReadAsync_Memory_DataAvailable_CompletesSynchronously()
        {
            var pipeStream = new PipeStream();

            pipeStream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);

            var readBuffer = new byte[3];

            ValueTask<int> readTask = pipeStream.ReadAsync(readBuffer.AsMemory());

            Assert.IsTrue(readTask.IsCompletedSuccessfully);
            Assert.AreEqual(3, await readTask);
            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, readBuffer);

            ValueTask<int> readTask2 = pipeStream.ReadAsync(readBuffer.AsMemory());

            Assert.IsTrue(readTask2.IsCompletedSuccessfully);
            Assert.AreEqual(1, await readTask2);
            Assert.AreEqual(4, readBuffer[0]);

            // No data left, so the next read should block.
            ValueTask<int> readTask3 = pipeStream.ReadAsync(readBuffer.AsMemory());

            await Task.Delay(50);

            Assert.IsFalse(readTask3.IsCompleted);

            await pipeStream.WriteAsync(new byte[] { 5 });

            Assert.AreEqual(1, await readTask3);
            Assert.AreEqual(5, readBuffer[0]);
        }
#endif

        [TestMethod]
        public void Read_AfterDispose_StillWorks()
        {
            var pipeStream = new PipeStream();

            pipeStream.Write(new byte[] { 1, 2, 3, 4 }, 0, 4);

            pipeStream.Dispose();
#pragma warning disable S3966 // Objects should not be disposed more than once
            pipeStream.Dispose(); // Check that multiple Dispose is OK.
#pragma warning restore S3966 // Objects should not be disposed more than once

            Assert.IsTrue(pipeStream.CanRead);

            Assert.AreEqual(4, pipeStream.Read(new byte[5], 0, 5));
            Assert.AreEqual(0, pipeStream.Read(new byte[5], 0, 5));
        }

        [TestMethod]
        public void SeekShouldThrowNotSupportedException()
        {
            var target = new PipeStream();
            Assert.Throws<NotSupportedException>(() => target.Seek(offset: 0, SeekOrigin.Begin));
        }

        [TestMethod]
        public void SetLengthShouldThrowNotSupportedException()
        {
            var target = new PipeStream();
            Assert.Throws<NotSupportedException>(() => target.SetLength(1));
        }

        [TestMethod]
        public void WriteTest()
        {
            var target = new PipeStream();

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

#if NET
        [TestMethod]
        public void WriteTest_Span()
        {
            var target = new PipeStream();

            var writeBuffer = new byte[] { 0x0a, 0x05, 0x0d };
            target.Write(writeBuffer.AsSpan(0, 2));

            writeBuffer = new byte[] { 0x02, 0x04, 0x03, 0x06, 0x09 };
            target.Write(writeBuffer.AsSpan(1, 2));

            var readBuffer = new byte[6];
            var bytesRead = target.Read(readBuffer.AsSpan(0, 4));

            Assert.AreEqual(4, bytesRead);
            Assert.AreEqual(0x0a, readBuffer[0]);
            Assert.AreEqual(0x05, readBuffer[1]);
            Assert.AreEqual(0x04, readBuffer[2]);
            Assert.AreEqual(0x03, readBuffer[3]);
            Assert.AreEqual(0x00, readBuffer[4]);
            Assert.AreEqual(0x00, readBuffer[5]);
        }
#endif

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
            target.Dispose();
            Assert.IsFalse(target.CanWrite);
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
            Assert.Throws<NotSupportedException>(() => target.Position = 0);
        }
    }
}
