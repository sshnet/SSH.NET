using System;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Connection;
using Renci.SshNet.Security;
using Renci.SshNet.Sftp;

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

            _serviceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateSocketFactory())
                               .Returns(_socketFactoryMock.Object);
            _serviceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateSession(_connectionInfo, _socketFactoryMock.Object))
                               .Returns(_sessionMock.Object);
            _sessionMock.InSequence(sequence)
                        .Setup(p => p.Connect());
            _serviceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateSftpResponseFactory())
                               .Returns(_sftpResponseFactoryMock.Object);
            _serviceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateSftpSession(_sessionMock.Object, -1, _connectionInfo.Encoding, _sftpResponseFactoryMock.Object))
                               .Returns(_sftpSessionMock.Object);
            _sftpSessionMock.InSequence(sequence)
                            .Setup(p => p.Connect())
                            .Throws(_sftpSessionConnectionException);
            _sftpSessionMock.InSequence(sequence)
                            .Setup(p => p.Dispose());
            _sessionMock.InSequence(sequence)
                        .Setup(p => p.Dispose());
        }

        protected override void Arrange()
        {
            base.Arrange();

            _sftpClient = new SftpClient(_connectionInfo, false, _serviceFactoryMock.Object);
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

            _sessionMock.Raise(p => p.ErrorOccured += null, new ExceptionEventArgs(new Exception()));

            Assert.AreEqual(0, errorOccurredSignalCount);
        }

        [TestMethod]
        public void HostKeyReceivedOnSessionShouldNoLongerBeSignaledViaHostKeyReceivedOnSftpClient()
        {
            var hostKeyReceivedSignalCount = 0;

            _sftpClient.HostKeyReceived += (sender, args) => Interlocked.Increment(ref hostKeyReceivedSignalCount);

            _sessionMock.Raise(p => p.HostKeyReceived += null, new HostKeyEventArgs(GetKeyHostAlgorithm()));

            Assert.AreEqual(0, hostKeyReceivedSignalCount);
        }

        private static KeyHostAlgorithm GetKeyHostAlgorithm()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();

            using (var s = executingAssembly.GetManifestResourceStream(string.Format("Renci.SshNet.Tests.Data.{0}", "Key.RSA.txt")))
            {
                var privateKey = new PrivateKeyFile(s);
                return (KeyHostAlgorithm)privateKey.HostKey;
            }
        }
    }
}
