using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class PipeStream_Close_BlockingWrite : TripleATestBase
    {
        private PipeStream _pipeStream;
        private Exception _writeException;
        private Thread _writeThread;

        protected override void Arrange()
        {
            _pipeStream = new PipeStream {MaxBufferLength = 3};

            ManualResetEvent isArranged = new ManualResetEvent(false);
            _writeThread = new Thread(() =>
                {
                    _pipeStream.WriteByte(10);
                    _pipeStream.WriteByte(13);
                    _pipeStream.WriteByte(25);

                    // attempting to write more bytes than the max. buffer length should block
                    // until bytes are read or the stream is closed
                    try
                    {
                        isArranged.Set();
                        _pipeStream.WriteByte(35);
                    }
                    catch (Exception ex)
                    {
                        _writeException = ex;
                    }
                });
            _writeThread.Start();

            // ensure we've started writing
            isArranged.WaitOne(10000);
        }

        protected override void Act()
        {
            _pipeStream.Close();

            // give write time to complete
            Assert.IsTrue(_writeThread.Join(10000));
        }

        [TestMethod]
        public void BlockingWriteShouldHaveBeenInterrupted()
        {
            Assert.AreEqual(ThreadState.Stopped, _writeThread.ThreadState);
        }

        [TestMethod]
        public void WriteShouldHaveThrownObjectDisposedException()
        {
            Assert.IsNotNull(_writeException);
            Assert.AreEqual(typeof (ObjectDisposedException), _writeException.GetType());
        }
    }
}
