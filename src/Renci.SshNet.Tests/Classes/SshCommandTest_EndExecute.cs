using System;
using System.Globalization;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SshCommand_EndExecute : TestBase
    {
        private Mock<ISession> _sessionMock;
        private Mock<IChannelSession> _channelSessionMock;
        private string _commandText;
        private Encoding _encoding;
        private SshCommand _sshCommand;

        protected override void OnInit()
        {
            base.OnInit();

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _commandText = new Random().Next().ToString(CultureInfo.InvariantCulture);
            _encoding = Encoding.UTF8;
            _channelSessionMock = new Mock<IChannelSession>(MockBehavior.Strict);

            _sshCommand = new SshCommand(_sessionMock.Object, _commandText, _encoding);
        }

        [TestMethod]
        public void EndExecute_ChannelClosed_ShouldDisposeChannelSession()
        {
            var seq = new MockSequence();

            _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
            _channelSessionMock.InSequence(seq).Setup(p => p.Open());
            _channelSessionMock.InSequence(seq).Setup(p => p.SendExecRequest(_commandText))
                .Returns(true)
                .Raises(c => c.Closed += null, new ChannelEventArgs(5));
            _channelSessionMock.InSequence(seq).Setup(p => p.Dispose());

            var asyncResult = _sshCommand.BeginExecute();
            _sshCommand.EndExecute(asyncResult);

            _channelSessionMock.Verify(p => p.Dispose(), Times.Once);
        }

        [TestMethod]
        public void EndExecute_ChannelOpen_ShouldSendEofAndCloseAndDisposeChannelSession()
        {
            var seq = new MockSequence();

            _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
            _channelSessionMock.InSequence(seq).Setup(p => p.Open());
            _channelSessionMock.InSequence(seq).Setup(p => p.SendExecRequest(_commandText))
                .Returns(true)
                .Raises(c => c.Closed += null, new ChannelEventArgs(5));
            _channelSessionMock.InSequence(seq).Setup(p => p.Dispose());

            var asyncResult = _sshCommand.BeginExecute();
            _sshCommand.EndExecute(asyncResult);

            _channelSessionMock.Verify(p => p.Dispose(), Times.Once);
        }
    }
}
