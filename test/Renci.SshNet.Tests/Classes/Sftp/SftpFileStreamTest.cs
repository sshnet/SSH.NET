using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileStreamTest
    {
        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task BadFileMode_ThrowsArgumentOutOfRangeException(bool isAsync)
        {
            ArgumentOutOfRangeException ex;

            if (isAsync)
            {
                ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                    SftpFileStream.OpenAsync(new Mock<ISftpSession>().Object, "file.txt", mode: 0, FileAccess.Read, bufferSize: 1024, CancellationToken.None));
            }
            else
            {
                ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    SftpFileStream.Open(new Mock<ISftpSession>().Object, "file.txt", mode: 0, FileAccess.Read, bufferSize: 1024));
            }

            Assert.AreEqual("mode", ex.ParamName);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task BadFileAccess_ThrowsArgumentOutOfRangeException(bool isAsync)
        {
            ArgumentOutOfRangeException ex;

            if (isAsync)
            {
                ex = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                    SftpFileStream.OpenAsync(new Mock<ISftpSession>().Object, "file.txt", FileMode.Open, access: 0, bufferSize: 1024, CancellationToken.None));
            }
            else
            {
                ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    SftpFileStream.Open(new Mock<ISftpSession>().Object, "file.txt", FileMode.Open, access: 0, bufferSize: 1024));
            }

            Assert.AreEqual("access", ex.ParamName);
        }

        [TestMethod]
        [DataRow(FileMode.Append, FileAccess.Read, false)]
        [DataRow(FileMode.Append, FileAccess.Read, true)]
        [DataRow(FileMode.Append, FileAccess.ReadWrite, false)]
        [DataRow(FileMode.Append, FileAccess.ReadWrite, true)]
        [DataRow(FileMode.Create, FileAccess.Read, false)]
        [DataRow(FileMode.Create, FileAccess.Read, true)]
        [DataRow(FileMode.CreateNew, FileAccess.Read, false)]
        [DataRow(FileMode.CreateNew, FileAccess.Read, true)]
        [DataRow(FileMode.Truncate, FileAccess.Read, false)]
        [DataRow(FileMode.Truncate, FileAccess.Read, true)]
        public async Task InvalidModeAccessCombination_ThrowsArgumentException(FileMode mode, FileAccess access, bool isAsync)
        {
            ArgumentException ex;

            if (isAsync)
            {
                ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                    SftpFileStream.OpenAsync(new Mock<ISftpSession>().Object, "file.txt", mode, access, bufferSize: 1024, CancellationToken.None));
            }
            else
            {
                ex = Assert.Throws<ArgumentException>(() =>
                    SftpFileStream.Open(new Mock<ISftpSession>().Object, "file.txt", mode, access, bufferSize: 1024));
            }

            Assert.AreEqual("access", ex.ParamName);
        }

        [TestMethod]
        public void ReadWithWriteAccess_ThrowsNotSupportedException()
        {
            var sessionMock = new Mock<ISftpSession>();

            sessionMock.Setup(s => s.CalculateOptimalWriteLength(It.IsAny<uint>(), It.IsAny<byte[]>())).Returns<uint, byte[]>((x, _) => x);
            sessionMock.Setup(s => s.IsOpen).Returns(true);

            SetupRemoteSize(sessionMock, 128);

            var s = SftpFileStream.Open(sessionMock.Object, "file.txt", FileMode.Create, FileAccess.Write, bufferSize: 1024);

            Assert.IsFalse(s.CanRead);

            Assert.Throws<NotSupportedException>(() => _ = s.Read(new byte[4], 0, 4));
            Assert.Throws<NotSupportedException>(() => _ = s.ReadByte());
            Assert.Throws<NotSupportedException>(() => _ = s.ReadAsync(new byte[4], 0, 4).GetAwaiter().GetResult());
            Assert.Throws<NotSupportedException>(() => _ = s.EndRead(s.BeginRead(new byte[4], 0, 4, null, null)));
#if NET
            Assert.Throws<NotSupportedException>(() => _ = s.Read(new byte[4]));
            Assert.Throws<NotSupportedException>(() => _ = s.ReadAsync(new byte[4]).AsTask().GetAwaiter().GetResult());
#endif
            Assert.Throws<NotSupportedException>(() => s.CopyTo(Stream.Null));
            Assert.Throws<NotSupportedException>(() => s.CopyToAsync(Stream.Null).GetAwaiter().GetResult());
        }

        [TestMethod]
        public void WriteWithReadAccess_ThrowsNotSupportedException()
        {
            var sessionMock = new Mock<ISftpSession>();

            sessionMock.Setup(s => s.CalculateOptimalWriteLength(It.IsAny<uint>(), It.IsAny<byte[]>())).Returns<uint, byte[]>((x, _) => x);
            sessionMock.Setup(s => s.IsOpen).Returns(true);

            var s = SftpFileStream.Open(sessionMock.Object, "file.txt", FileMode.Open, FileAccess.Read, bufferSize: 1024);

            Assert.IsFalse(s.CanWrite);

            Assert.Throws<NotSupportedException>(() => s.Write(new byte[4], 0, 4));
            Assert.Throws<NotSupportedException>(() => s.WriteByte(0xf));
            Assert.Throws<NotSupportedException>(() => s.WriteAsync(new byte[4], 0, 4).GetAwaiter().GetResult());
            Assert.Throws<NotSupportedException>(() => s.EndWrite(s.BeginWrite(new byte[4], 0, 4, null, null)));
#if NET
            Assert.Throws<NotSupportedException>(() => s.Write(new byte[4]));
            Assert.Throws<NotSupportedException>(() => s.WriteAsync(new byte[4]).AsTask().GetAwaiter().GetResult());
#endif
            Assert.Throws<NotSupportedException>(() => s.SetLength(1024));
        }

        [TestMethod]
        [DataRow(-1, SeekOrigin.Begin)]
        [DataRow(-1, SeekOrigin.Current)]
        [DataRow(-1000, SeekOrigin.End)]
        public void SeekBeforeBeginning_ThrowsIOException(long offset, SeekOrigin origin)
        {
            var sessionMock = new Mock<ISftpSession>();

            sessionMock.Setup(s => s.CalculateOptimalReadLength(It.IsAny<uint>())).Returns<uint>(x => x);
            sessionMock.Setup(s => s.CalculateOptimalWriteLength(It.IsAny<uint>(), It.IsAny<byte[]>())).Returns<uint, byte[]>((x, _) => x);
            sessionMock.Setup(s => s.IsOpen).Returns(true);

            SetupRemoteSize(sessionMock, 128);

            var s = SftpFileStream.Open(sessionMock.Object, "file.txt", FileMode.Open, FileAccess.Read, bufferSize: 1024);

            Assert.Throws<IOException>(() => s.Seek(offset, origin));
        }

        private static void SetupRemoteSize(Mock<ISftpSession> sessionMock, long size)
        {
            sessionMock.Setup(s => s.RequestFStat(It.IsAny<byte[]>())).Returns(new SftpFileAttributes(
                default, default, size: size, default, default, default, default
                ));
        }

        // Operations which should cause writes to be flushed because they depend on
        // the remote file being up to date.
        // Most of these are already implicitly covered by integration tests and may
        // not be so valuable here.
        [TestMethod]
        public void Flush_SendsBufferedWrites()
        {
            TestSendsBufferedWrites(s => s.Flush());
        }

        [TestMethod]
        public void Read_SendsBufferedWrites()
        {
            TestSendsBufferedWrites(s => _ = s.Read(new byte[16], 0, 16));
        }

        [TestMethod]
        public void Seek_SendsBufferedWrites()
        {
            TestSendsBufferedWrites(s => _ = s.Seek(-1, SeekOrigin.Current));
        }

        [TestMethod]
        public void SetPosition_SendsBufferedWrites()
        {
            TestSendsBufferedWrites(s => s.Position++);
        }

        [TestMethod]
        public void SetLength_SendsBufferedWrites()
        {
            TestSendsBufferedWrites(s => s.SetLength(256));
        }

        [TestMethod]
        public void GetLength_SendsBufferedWrites()
        {
            TestSendsBufferedWrites(s => _ = s.Length);
        }

        [TestMethod]
        public void Dispose_SendsBufferedWrites()
        {
            TestSendsBufferedWrites(s => s.Dispose());
        }

        private void TestSendsBufferedWrites(Action<SftpFileStream> flushAction)
        {
            var sessionMock = new Mock<ISftpSession>();

            sessionMock.Setup(s => s.CalculateOptimalReadLength(It.IsAny<uint>())).Returns<uint>(x => x);
            sessionMock.Setup(s => s.CalculateOptimalWriteLength(It.IsAny<uint>(), It.IsAny<byte[]>())).Returns<uint, byte[]>((x, _) => x);
            sessionMock.Setup(s => s.IsOpen).Returns(true);
            SetupRemoteSize(sessionMock, 0);

            var s = SftpFileStream.Open(sessionMock.Object, "file.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, bufferSize: 1024);

            // Buffer some data
            byte[] newData = "Some new bytes"u8.ToArray();
            s.Write(newData, 0, newData.Length);

            byte[] newData2 = "Some more bytes"u8.ToArray();
            s.Write(newData2, 0, newData2.Length);

            // The written data does not exceed bufferSize so we do not expect
            // it to have been sent.
            sessionMock.Verify(s => s.RequestWrite(
                It.IsAny<byte[]>(),
                It.IsAny<ulong>(),
                It.IsAny<byte[]>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<AutoResetEvent>(),
                It.IsAny<Action<SftpStatusResponse>>()),
                Times.Never);

            // Whatever is called here should trigger the bytes to be sent
            flushAction(s);

            VerifyRequestWrite(sessionMock, newData.Concat(newData2), serverOffset: 0);
        }

        [TestMethod]
        public void Dispose()
        {
            var sessionMock = new Mock<ISftpSession>();

            sessionMock.Setup(s => s.CalculateOptimalWriteLength(It.IsAny<uint>(), It.IsAny<byte[]>())).Returns<uint, byte[]>((x, _) => x);
            sessionMock.Setup(s => s.IsOpen).Returns(true);

            var s = SftpFileStream.Open(sessionMock.Object, "file.txt", FileMode.Create, FileAccess.ReadWrite, bufferSize: 1024);

            Assert.IsTrue(s.CanRead);
            Assert.IsTrue(s.CanWrite);

            s.Dispose();
            sessionMock.Verify(p => p.RequestClose(It.IsAny<byte[]>()), Times.Once);

            Assert.IsFalse(s.CanRead);
            Assert.IsFalse(s.CanSeek);
            Assert.IsFalse(s.CanWrite);

            Assert.Throws<ObjectDisposedException>(() => s.Read(new byte[16], 0, 16));
            Assert.Throws<ObjectDisposedException>(() => s.ReadByte());
            Assert.Throws<ObjectDisposedException>(() => s.Write(new byte[16], 0, 16));
            Assert.Throws<ObjectDisposedException>(() => s.WriteByte(0xf));
            Assert.Throws<ObjectDisposedException>(() => s.CopyTo(Stream.Null));
            Assert.Throws<ObjectDisposedException>(s.Flush);
            Assert.Throws<ObjectDisposedException>(() => s.Seek(0, SeekOrigin.Begin));
            Assert.Throws<ObjectDisposedException>(() => s.SetLength(128));
            Assert.Throws<ObjectDisposedException>(() => _ = s.Length);

            // Test no-op second dispose
            s.Dispose();
            sessionMock.Verify(p => p.RequestClose(It.IsAny<byte[]>()), Times.Once);
        }

        [TestMethod]
        public void FstatFailure_DisablesSeek()
        {
            TestFstatFailure(fstat => fstat.Throws<SftpPermissionDeniedException>());
        }

        [TestMethod]
        public void FstatSizeNotReturned_DisablesSeek()
        {
            TestFstatFailure(fstat => fstat.Returns(SftpFileAttributes.FromBytes([0, 0, 0, 0])));
        }

        private void TestFstatFailure(Action<Moq.Language.Flow.ISetup<ISftpSession, SftpFileAttributes>> fstatSetup)
        {
            var sessionMock = new Mock<ISftpSession>();

            sessionMock.Setup(s => s.CalculateOptimalReadLength(It.IsAny<uint>())).Returns<uint>(x => x);
            sessionMock.Setup(s => s.CalculateOptimalWriteLength(It.IsAny<uint>(), It.IsAny<byte[]>())).Returns<uint, byte[]>((x, _) => x);
            sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
            sessionMock.Setup(s => s.IsOpen).Returns(true);

            fstatSetup(sessionMock.Setup(s => s.RequestFStat(It.IsAny<byte[]>())));

            var s = SftpFileStream.Open(sessionMock.Object, "file.txt", FileMode.Open, FileAccess.ReadWrite, bufferSize: 1024);

            Assert.IsFalse(s.CanSeek);
            Assert.IsTrue(s.CanRead);
            Assert.IsTrue(s.CanWrite);

            Assert.Throws<NotSupportedException>(() => s.Position);
            Assert.Throws<NotSupportedException>(() => s.Length);
            Assert.Throws<NotSupportedException>(() => s.Seek(0, SeekOrigin.Begin));
            Assert.Throws<NotSupportedException>(() => s.SetLength(1024));

            // Reads and writes still succeed.
            _ = s.Read(new byte[16], 0, 16);
            s.Write(new byte[16], 0, 16);
            s.Flush();
        }

        private static void VerifyRequestWrite(Mock<ISftpSession> sessionMock, ReadOnlyMemory<byte> newData, int serverOffset)
        {
            sessionMock.Verify(s => s.RequestWrite(
                /* handle: */         It.IsAny<byte[]>(),
                /* serverOffset: */   (ulong)serverOffset,
                /* data: */           It.Is<byte[]>(x => IndexOf(x, newData) >= 0),
                /* offset: */         It.IsAny<int>(),
                /* length: */         newData.Length,
                /* wait: */           It.IsAny<AutoResetEvent>(),
                /* writeCompleted: */ It.IsAny<Action<SftpStatusResponse>>()),
                Times.Once);
        }

        private static int IndexOf(byte[] searchSpace, ReadOnlyMemory<byte> searchValue)
        {
            // Needed in a (non-local) function because expression lambdas can't contain spans
            return searchSpace.AsSpan().IndexOf(searchValue.Span);
        }
    }
}
