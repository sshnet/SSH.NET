using System;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SshCommandTest_Dispose : TestBase
    {
        private Mock<ISession> _sessionMock;
        private Mock<IChannelSession> _channelSessionMock;
        private string _commandText;
        private Encoding _encoding;
        private SshCommand _sshCommand;
        private Stream _outputStream;
        private Stream _extendedOutputStream;

        protected override void OnInit()
        {
            base.OnInit();

            Arrange();
            Act();
        }

        private void Arrange()
        {
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _commandText = new Random().Next().ToString(CultureInfo.InvariantCulture);
            _encoding = Encoding.UTF8;
            _channelSessionMock = new Mock<IChannelSession>(MockBehavior.Strict);

            var seq = new MockSequence();

            _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
            _channelSessionMock.InSequence(seq).Setup(p => p.Open());
            _channelSessionMock.InSequence(seq).Setup(p => p.SendExecRequest(_commandText)).Returns(true);
            _channelSessionMock.InSequence(seq).Setup(p => p.Dispose());

            _sshCommand = new SshCommand(_sessionMock.Object, _commandText, _encoding);
            _sshCommand.BeginExecute();

            _outputStream = _sshCommand.OutputStream;
            _extendedOutputStream = _sshCommand.ExtendedOutputStream;
        }

        private void Act()
        {
            _sshCommand.Dispose();
        }

        [TestMethod]
        public void ChannelSessionShouldBeDisposedOnce()
        {
            _channelSessionMock.Verify(p => p.Dispose(), Times.Once);
        }

        [TestMethod]
        public void OutputStreamShouldReturnNull()
        {
            Assert.IsNull(_sshCommand.OutputStream);
        }

        [TestMethod]
        public void OutputStreamShouldHaveBeenDisposed()
        {
            try
            {
                _outputStream.ReadByte();
                Assert.Fail();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [TestMethod]
        public void ExtendedOutputStreamShouldReturnNull()
        {
            Assert.IsNull(_sshCommand.ExtendedOutputStream);
        }

        [TestMethod]
        public void ExtendedOutputStreamShouldHaveBeenDisposed()
        {
            try
            {
                _extendedOutputStream.ReadByte();
                Assert.Fail();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [TestMethod]
        public void RaisingDisconnectedOnSessionShouldDoNothing()
        {
            _sessionMock.Raise(s => s.Disconnected += null, new EventArgs());
        }

        [TestMethod]
        public void RaisingErrorOccuredOnSessionShouldDoNothing()
        {
            _sessionMock.Raise(s => s.ErrorOccured += null, new ExceptionEventArgs(new Exception()));
        }

        [TestMethod]
        public void InvokingDisposeAgainShouldNotRaiseAnException()
        {
            _sshCommand.Dispose();
        }
    }
}
