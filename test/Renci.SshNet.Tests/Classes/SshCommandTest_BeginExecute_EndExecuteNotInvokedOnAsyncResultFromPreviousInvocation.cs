using System;
using System.Globalization;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SshCommandTest_BeginExecute_EndExecuteNotInvokedOnAsyncResultFromPreviousInvocation : TestBase
    {
        private Mock<ISession> _sessionMock;
        private Mock<IChannelSession> _channelSessionMock;
        private string _commandText;
        private Encoding _encoding;
        private SshCommand _sshCommand;
        private InvalidOperationException _actualException;

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
            _channelSessionMock = new Mock<IChannelSession>(MockBehavior.Strict);
            _commandText = random.Next().ToString(CultureInfo.InvariantCulture);
            _encoding = Encoding.UTF8;

            var seq = new MockSequence();
            _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
            _channelSessionMock.InSequence(seq).Setup(p => p.Open());
            _channelSessionMock.InSequence(seq).Setup(p => p.SendExecRequest(_commandText)).Returns(true);

            _sshCommand = new SshCommand(_sessionMock.Object, _commandText, _encoding);
            _sshCommand.BeginExecute();
        }

        private void Act()
        {
            try
            {
                _sshCommand.BeginExecute();
                Assert.Fail();
            }
            catch (InvalidOperationException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void BeginExecuteShouldThrowInvalidOperationException()
        {
            Assert.IsNotNull(_actualException);
            Assert.IsNull(_actualException.InnerException);
            Assert.AreEqual("Asynchronous operation is already in progress.", _actualException.Message);
        }
    }
}
