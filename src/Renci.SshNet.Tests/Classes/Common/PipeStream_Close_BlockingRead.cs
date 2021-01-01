using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class PipeStream_Close_BlockingRead : TripleATestBase
    {
        private PipeStream _pipeStream;
        private int _bytesRead;
        private Thread _readThread;

        protected override void Arrange()
        {
            _pipeStream = new PipeStream();

            _pipeStream.WriteByte(10);
            _pipeStream.WriteByte(13);
            _pipeStream.WriteByte(25);

            _bytesRead = 123;

            _readThread = new Thread(() => _bytesRead = _pipeStream.Read(new byte[4], 0, 4));
            _readThread.Start();

            // ensure we've started reading
            Assert.IsFalse(_readThread.Join(50));
        }

        protected override void Act()
        {
            _pipeStream.Close();

            // give async read time to complete
            _readThread.Join(100);
        }

        [TestMethod]
        public void BlockingReadShouldHaveBeenInterrupted()
        {
            Assert.AreEqual(ThreadState.Stopped, _readThread.ThreadState);
        }

        [TestMethod]
        public void ReadShouldHaveReturnedZero()
        {
            Assert.AreEqual(0, _bytesRead);
        }
    }
}
