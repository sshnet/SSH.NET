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
        private Thread _writehread;

        protected override void Arrange()
        {
            _pipeStream = new PipeStream {MaxBufferLength = 3};

            _writehread = new Thread(() =>
                {
                    _pipeStream.WriteByte(10);
                    _pipeStream.WriteByte(13);
                    _pipeStream.WriteByte(25);

                    // attempting to write more bytes than the max. buffer length should block
                    // until bytes are read or the stream is closed
                    try
                    {
                        _pipeStream.WriteByte(35);
                    }
                    catch (Exception ex)
                    {
                        _writeException = ex;
                        throw;
                    }
                });
            _writehread.Start();

            // ensure we've started writing
            Assert.IsFalse(_writehread.Join(50));
        }

        protected override void Act()
        {
            _pipeStream.Close();

            // give write time to complete
            _writehread.Join(100);
        }

        [TestMethod]
        public void BlockingWriteShouldHaveBeenInterrupted()
        {
            Assert.AreEqual(ThreadState.Stopped, _writehread.ThreadState);
        }

        [TestMethod]
        public void WriteShouldHaveThrownObjectDisposedException()
        {
            Assert.IsNotNull(_writeException);
            Assert.AreEqual(typeof (ObjectDisposedException), _writeException.GetType());
        }
    }
}
