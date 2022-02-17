using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class PipeStream_Flush_NoBytesRemainingAfterRead : TripleATestBase
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

            _bytesRead = 0;
            _readBuffer = new byte[4];

            _readThread = new Thread(() => _bytesRead = _pipeStream.Read(_readBuffer, 0, _readBuffer.Length));
            _readThread.Start();

            // ensure we've started reading
            Assert.IsFalse(_readThread.Join(50));
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
            Assert.AreEqual(2, _bytesRead);
        }

        [TestMethod]
        public void BytesAvailableInStreamShouldHaveBeenWrittenToBuffer()
        {
            Assert.AreEqual(10, _readBuffer[0]);
            Assert.AreEqual(13, _readBuffer[1]);
            Assert.AreEqual(0, _readBuffer[2]);
            Assert.AreEqual(0, _readBuffer[3]);
        }
    }
}
