using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Common
{
    [TestClass]
    public class PipeStream_Close_BlockingWrite
    {
        private PipeStream _pipeStream;
        private Exception _writeException;
        private IAsyncResult _asyncWriteResult;

        [TestInitialize]
        public void Init()
        {
            _pipeStream = new PipeStream {MaxBufferLength = 3};

            Action writeAction = () =>
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
                };
            _asyncWriteResult = writeAction.BeginInvoke(null, null);
            // ensure we've started writing
            _asyncWriteResult.AsyncWaitHandle.WaitOne(50);

            Act();
        }

        protected void Act()
        {
            _pipeStream.Close();
        }

        [TestMethod]
        public void BlockingWriteShouldHaveBeenInterrupted()
        {
            Assert.IsTrue(_asyncWriteResult.AsyncWaitHandle.WaitOne(200));
        }

        [TestMethod]
        public void WriteShouldHaveThrownObjectDisposedException()
        {
            _asyncWriteResult.AsyncWaitHandle.WaitOne(200);

            Assert.IsNotNull(_writeException);
            Assert.AreEqual(typeof (ObjectDisposedException), _writeException.GetType());
        }
    }
}
