using System;
using System.Globalization;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SshCommandTest_EndExecute_ChannelOpen : TestBase
    {
        private Mock<ISession> _sessionMock;
        private Mock<IChannelSession> _channelSessionMock;
        private string _commandText;
        private Encoding _encoding;
        private SshCommand _sshCommand;
        private IAsyncResult _asyncResult;
        private string _actual;
        private string _dataA;
        private string _dataB;
        private string _extendedDataA;
        private string _extendedDataB;
        private int _expectedExitStatus;

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
            _expectedExitStatus = random.Next();
            _dataA = random.Next().ToString(CultureInfo.InvariantCulture);
            _dataB = random.Next().ToString(CultureInfo.InvariantCulture);
            _extendedDataA = random.Next().ToString(CultureInfo.InvariantCulture);
            _extendedDataB = random.Next().ToString(CultureInfo.InvariantCulture);
            _asyncResult = null;

            var seq = new MockSequence();
            _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
            _channelSessionMock.InSequence(seq).Setup(p => p.Open());
            _channelSessionMock.InSequence(seq).Setup(p => p.SendExecRequest(_commandText))
                .Returns(true)
                .Raises(c => c.Closed += null, new ChannelEventArgs(5));
            _channelSessionMock.InSequence(seq).Setup(p => p.Dispose());

            _sshCommand = new SshCommand(_sessionMock.Object, _commandText, _encoding);
            _asyncResult = _sshCommand.BeginExecute();

            _channelSessionMock.Raise(c => c.DataReceived += null,
                new ChannelDataEventArgs(0, _encoding.GetBytes(_dataA)));
            _channelSessionMock.Raise(c => c.ExtendedDataReceived += null,
                new ChannelExtendedDataEventArgs(0, _encoding.GetBytes(_extendedDataA), 0));
            _channelSessionMock.Raise(c => c.DataReceived += null,
                new ChannelDataEventArgs(0, _encoding.GetBytes(_dataB)));
            _channelSessionMock.Raise(c => c.ExtendedDataReceived += null,
                new ChannelExtendedDataEventArgs(0, _encoding.GetBytes(_extendedDataB), 0));
            _channelSessionMock.Raise(c => c.RequestReceived += null,
                new ChannelRequestEventArgs(new ExitStatusRequestInfo((uint) _expectedExitStatus)));
        }

        private void Act()
        {
            _actual = _sshCommand.EndExecute(_asyncResult);
        }

        [TestMethod]
        public void ChannelSessionShouldBeDisposedOnce()
        {
            _channelSessionMock.Verify(p => p.Dispose(), Times.Once);
        }

        [TestMethod]
        public void EndExecuteShouldReturnAllDataReceivedInSpecifiedEncoding()
        {
            Assert.AreEqual(string.Concat(_dataA, _dataB), _actual);
        }

        [TestMethod]
        public void EndExecuteShouldThrowArgumentExceptionWhenInvokedAgainWithSameAsyncResult()
        {
            try
            {
                _sshCommand.EndExecute(_asyncResult);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(typeof (ArgumentException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("EndExecute can only be called once for each asynchronous operation.", ex.Message);
                Assert.IsNull(ex.ParamName);
            }
        }

        [TestMethod]
        public void ErrorShouldReturnZeroLengthString()
        {
            Assert.AreEqual(string.Empty, _sshCommand.Error);
        }

        [TestMethod]
        public void ExitStatusShouldReturnExitStatusFromExitStatusRequestInfo()
        {
            Assert.AreEqual(_expectedExitStatus, _sshCommand.ExitStatus);
        }

        [TestMethod]
        public void ExtendedOutputStreamShouldContainAllExtendedDataReceived()
        {
            var extendedDataABytes = _encoding.GetBytes(_extendedDataA);
            var extendedDataBBytes = _encoding.GetBytes(_extendedDataB);

            var extendedOutputStream = _sshCommand.ExtendedOutputStream;
            Assert.AreEqual(extendedDataABytes.Length + extendedDataBBytes.Length, extendedOutputStream.Length);

            var buffer = new byte[extendedOutputStream.Length];
            var bytesRead = extendedOutputStream.Read(buffer, 0, buffer.Length);

            Assert.AreEqual(buffer.Length, bytesRead);
            Assert.AreEqual(string.Concat(_extendedDataA, _extendedDataB), _encoding.GetString(buffer));
            Assert.AreEqual(0, extendedOutputStream.Length);
        }

        [TestMethod]
        public void OutputStreamShouldBeEmpty()
        {
            Assert.AreEqual(0, _sshCommand.OutputStream.Length);
        }

        [TestMethod]
        public void ResultShouldReturnAllDataReceived()
        {
            Assert.AreEqual(string.Concat(_dataA, _dataB), _sshCommand.Result);
        }
    }
}
