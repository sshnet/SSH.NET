using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class PipeStream_Flush_BytesRemainingAfterRead : TripleATestBase
    {
        private PipeStream _pipeStream;
        private byte[] _readBuffer;
        private int _bytesRead;
        private Thread _readThread;

        protected override void Arrange()
        {
            _pipeStream = new PipeStream();
            _pipeStream.WriteByte(10);
            _pipeStream.WriteByte(13);
            _pipeStream.WriteByte(15);
            _pipeStream.WriteByte(18);
            _pipeStream.WriteByte(23);
            _pipeStream.WriteByte(28);

            _bytesRead = 0;
            _readBuffer = new byte[4];

            _readThread = new Thread(() => _bytesRead = _pipeStream.Read(_readBuffer, 0, _readBuffer.Length));
            _readThread.Start();

            // ensure we've started reading
            _readThread.Join(50);
        }

        protected override void Act()
        {
            _pipeStream.Flush();

            // give async read time to complete
            _readThread.Join(100);
        }

        [TestMethod]
        public void AsyncReadShouldHaveFinished()
        {
            Assert.AreEqual(ThreadState.Stopped, _readThread.ThreadState);
        }

        [TestMethod]
        public void ReadShouldReturnNumberOfBytesAvailableThatAreWrittenToBuffer()
        {
            Assert.AreEqual(4, _bytesRead);
        }

        [TestMethod]
        public void BytesAvailableInStreamShouldHaveBeenWrittenToBuffer()
        {
            Assert.AreEqual(10, _readBuffer[0]);
            Assert.AreEqual(13, _readBuffer[1]);
            Assert.AreEqual(15, _readBuffer[2]);
            Assert.AreEqual(18, _readBuffer[3]);
        }

        [TestMethod]
        public void RemainingBytesCanBeRead()
        {
            var buffer = new byte[3];

            var bytesRead = _pipeStream.Read(buffer, 0, 2);

            Assert.AreEqual(2, bytesRead);
            Assert.AreEqual(23, buffer[0]);
            Assert.AreEqual(28, buffer[1]);
            Assert.AreEqual(0, buffer[2]);
        }

        [TestMethod]
        public void ReadingMoreBytesThanAvailableDoesNotBlock()
        {
            var buffer = new byte[4];

            var bytesRead = _pipeStream.Read(buffer, 0, buffer.Length);

            Assert.AreEqual(2, bytesRead);
            Assert.AreEqual(23, buffer[0]);
            Assert.AreEqual(28, buffer[1]);
            Assert.AreEqual(0, buffer[2]);
            Assert.AreEqual(0, buffer[3]);
        }

        [TestMethod]
        public void WriteCausesSubsequentReadToBlockUntilRequestedNumberOfBytesAreAvailable()
        {
            _pipeStream.WriteByte(32);

            var buffer = new byte[4];
            int bytesRead = int.MaxValue;

            Thread readThread = new Thread(() =>
            {
                bytesRead = _pipeStream.Read(buffer, 0, buffer.Length);
            });
            readThread.Start();

            Assert.IsFalse(readThread.Join(500));
            readThread.Abort();

            Assert.AreEqual(int.MaxValue, bytesRead);
            Assert.AreEqual(0, buffer[0]);
            Assert.AreEqual(0, buffer[1]);
            Assert.AreEqual(0, buffer[2]);
            Assert.AreEqual(0, buffer[3]);
        }
    }
}
