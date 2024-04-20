using System;
using System.Linq;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SftpClientTest_Connect_SftpSessionConnectFailure : SftpClientTestBase
    {
        private ConnectionInfo _connectionInfo;
        private ApplicationException _sftpSessionConnectionException;
        private SftpClient _sftpClient;
        private ApplicationException _actualException;

        protected override void SetupData()
        {
            _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));
            _sftpSessionConnectionException = new ApplicationException();
        }

        protected override void SetupMocks()
        {
            var sequence = new MockSequence();

            _ = ServiceFactoryMock.InSequence(sequence)
                                   .Setup(p => p.CreateSocketFactory())
                                   .Returns(SocketFactoryMock.Object);
            _ = ServiceFactoryMock.InSequence(sequence)
                                   .Setup(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object))
                                   .Returns(SessionMock.Object);
            _ = SessionMock.InSequence(sequence)
                            .Setup(p => p.Connect());
            _ = ServiceFactoryMock.InSequence(sequence)
                                   .Setup(p => p.CreateSftpResponseFactory())
                                   .Returns(SftpResponseFactoryMock.Object);
            _ = ServiceFactoryMock.InSequence(sequence)
                                   .Setup(p => p.CreateSftpSession(SessionMock.Object, -1, _connectionInfo.Encoding, SftpResponseFactoryMock.Object))
                                   .Returns(SftpSessionMock.Object);
            _ = SftpSessionMock.InSequence(sequence)
                                .Setup(p => p.Connect())
                                .Throws(_sftpSessionConnectionException);
            _ = SftpSessionMock.InSequence(sequence)
                                .Setup(p => p.Dispose());
            _ = SessionMock.InSequence(sequence)
                            .Setup(p => p.Dispose());
        }

        protected override void Arrange()
        {
            base.Arrange();

            _sftpClient = new SftpClient(_connectionInfo, false, ServiceFactoryMock.Object);
        }

        protected override void Act()
        {
            try
            {
                _sftpClient.Connect();
                Assert.Fail();
            }
            catch (ApplicationException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void ConnectShouldHaveThrownApplicationException()
        {
            Assert.IsNotNull(_actualException);
            Assert.AreSame(_sftpSessionConnectionException, _actualException);
        }

        [TestMethod]
        public void SessionShouldBeNull()
        {
            Assert.IsNull(_sftpClient.Session);
        }

        [TestMethod]
        public void SftpSessionShouldBeNull()
        {
            Assert.IsNull(_sftpClient.SftpSession);
        }

        [TestMethod]
        public void ErrorOccuredOnSessionShouldNoLongerBeSignaledViaErrorOccurredOnSftpClient()
        {
            var errorOccurredSignalCount = 0;

            _sftpClient.ErrorOccurred += (sender, args) => Interlocked.Increment(ref errorOccurredSignalCount);

            SessionMock.Raise(p => p.ErrorOccured += null, new ExceptionEventArgs(new Exception()));

            Assert.AreEqual(0, errorOccurredSignalCount);
        }

        [TestMethod]
        public void HostKeyReceivedOnSessionShouldNoLongerBeSignaledViaHostKeyReceivedOnSftpClient()
        {
            var hostKeyReceivedSignalCount = 0;

            _sftpClient.HostKeyReceived += (sender, args) => Interlocked.Increment(ref hostKeyReceivedSignalCount);

            SessionMock.Raise(p => p.HostKeyReceived += null, new HostKeyEventArgs(GetKeyHostAlgorithm()));

            Assert.AreEqual(0, hostKeyReceivedSignalCount);
        }

        private static KeyHostAlgorithm GetKeyHostAlgorithm()
        {
            using (var s = TestBase.GetData("Key.RSA.txt"))
            {
                var privateKey = new PrivateKeyFile(s);
                return (KeyHostAlgorithm) privateKey.HostKeyAlgorithms.First();
            }
        }
    }
}
