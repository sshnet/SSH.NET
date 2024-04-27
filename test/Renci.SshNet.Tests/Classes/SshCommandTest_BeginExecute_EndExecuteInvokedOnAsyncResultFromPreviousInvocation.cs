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
    public class SshCommandTest_BeginExecute_EndExecuteInvokedOnAsyncResultFromPreviousInvocation : TestBase
    {
        private Mock<ISession> _sessionMock;
        private Mock<IChannelSession> _channelSessionAMock;
        private Mock<IChannelSession> _channelSessionBMock;
        private string _commandText;
        private Encoding _encoding;
        private SshCommand _sshCommand;
        private IAsyncResult _asyncResultA;
        private IAsyncResult _asyncResultB;

        protected override void OnInit()
        {
            base.OnInit();

            Arrange();
            Act();
        }

        private void Arrange()
        {
            var random = new Random();

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _channelSessionAMock = new Mock<IChannelSession>(MockBehavior.Strict);
            _channelSessionBMock = new Mock<IChannelSession>(MockBehavior.Strict);
            _commandText = random.Next().ToString(CultureInfo.InvariantCulture);
            _encoding = Encoding.UTF8;
            _asyncResultA = null;
            _asyncResultB = null;

            var seq = new MockSequence();
            _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(_channelSessionAMock.Object);
            _channelSessionAMock.InSequence(seq).Setup(p => p.Open());
            _channelSessionAMock.InSequence(seq).Setup(p => p.SendExecRequest(_commandText))
                .Returns(true)
                .Raises(c => c.Closed += null, new ChannelEventArgs(5));
            _channelSessionAMock.InSequence(seq).Setup(p => p.Dispose());

            _sshCommand = new SshCommand(_sessionMock.Object, _commandText, _encoding);
            _asyncResultA = _sshCommand.BeginExecute();
            _sshCommand.EndExecute(_asyncResultA);

            _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(_channelSessionBMock.Object);
            _channelSessionBMock.InSequence(seq).Setup(p => p.Open());
            _channelSessionBMock.InSequence(seq).Setup(p => p.SendExecRequest(_commandText)).Returns(true);
        }

        private void Act()
        {
            _asyncResultB = _sshCommand.BeginExecute();
        }

        [TestMethod]
        public void BeginExecuteShouldReturnNewAsyncResult()
        {
            Assert.IsNotNull(_asyncResultB);
            Assert.AreNotSame(_asyncResultA, _asyncResultB);
        }
    }
}
