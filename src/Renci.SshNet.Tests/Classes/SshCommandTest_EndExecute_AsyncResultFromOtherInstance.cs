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
    public class SshCommandTest_EndExecute_AsyncResultFromOtherInstance : TestBase
    {
        private Mock<ISession> _sessionMock;
        private Mock<IChannelSession> _channelSessionAMock;
        private Mock<IChannelSession> _channelSessionBMock;
        private string _commandText;
        private Encoding _encoding;
        private SshCommand _sshCommandA;
        private SshCommand _sshCommandB;
        private ArgumentException _actualException;
        private IAsyncResult _asyncResultB;

        protected override void OnInit()
        {
            base.OnInit();

            Arrange();
            Act();
        }

        private void Arrange()
        {
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _channelSessionAMock = new Mock<IChannelSession>(MockBehavior.Strict);
            _channelSessionBMock = new Mock<IChannelSession>(MockBehavior.Strict);
            _commandText = new Random().Next().ToString(CultureInfo.InvariantCulture);
            _encoding = Encoding.UTF8;
            _asyncResultB = null;

            var seq = new MockSequence();
            _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(_channelSessionAMock.Object);
            _channelSessionAMock.InSequence(seq).Setup(p => p.Open());
            _channelSessionAMock.InSequence(seq).Setup(p => p.SendExecRequest(_commandText)).Returns(true);
            _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(_channelSessionBMock.Object);
            _channelSessionBMock.InSequence(seq).Setup(p => p.Open());
            _channelSessionBMock.InSequence(seq).Setup(p => p.SendExecRequest(_commandText)).Returns(true);

            _sshCommandA = new SshCommand(_sessionMock.Object, _commandText, _encoding);
            _sshCommandA.BeginExecute();

            _sshCommandB = new SshCommand(_sessionMock.Object, _commandText, _encoding);
            _asyncResultB = _sshCommandB.BeginExecute();
        }

        private void Act()
        {
            try
            {
                _sshCommandA.EndExecute(_asyncResultB);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void EndExecuteShouldHaveThrownArgumentException()
        {
            Assert.IsNotNull(_actualException);
            Assert.IsNull(_actualException.InnerException);
            Assert.AreEqual(string.Format("The {0} object was not returned from the corresponding asynchronous method on this class.", typeof(IAsyncResult).Name), _actualException.Message);
            Assert.IsNull(_actualException.ParamName);
        }
    }
}
