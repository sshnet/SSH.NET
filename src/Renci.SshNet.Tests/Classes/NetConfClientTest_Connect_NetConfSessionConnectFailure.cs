using System;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.NetConf;
using Renci.SshNet.Security;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class NetConfClientTest_Connect_NetConfSessionConnectFailure
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private Mock<ISession> _sessionMock;
        private Mock<INetConfSession> _netConfSessionMock;
        private ConnectionInfo _connectionInfo;
        private ApplicationException _netConfSessionConnectionException;
        private NetConfClient _netConfClient;
        private ApplicationException _actualException;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        private void Arrange()
        {
            SetupData();
            CreateMocks();
            SetupMocks();

            _netConfClient = new NetConfClient(_connectionInfo, false, _serviceFactoryMock.Object);
        }

        private void SetupData()
        {
            _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));
            _netConfSessionConnectionException = new ApplicationException();
        }

        private void CreateMocks()
        {
            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _netConfSessionMock = new Mock<INetConfSession>(MockBehavior.Strict);
        }

        private void SetupMocks()
        {
            var sequence = new MockSequence();

            _serviceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateSession(_connectionInfo))
                               .Returns(_sessionMock.Object);
            _sessionMock.InSequence(sequence)
                        .Setup(p => p.Connect());
            _serviceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateNetConfSession(_sessionMock.Object, _subSystem, -1))
                               .Returns(_netConfSessionMock.Object);
            _netConfSessionMock.InSequence(sequence)
                            .Setup(p => p.Connect())
                            .Throws(_netConfSessionConnectionException);
            _netConfSessionMock.InSequence(sequence)
                            .Setup(p => p.Dispose());
            _sessionMock.InSequence(sequence)
                        .Setup(p => p.Dispose());
        }

        private void Act()
        {
            try
            {
                _netConfClient.Connect();
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
            Assert.AreSame(_netConfSessionConnectionException, _actualException);
        }

        [TestMethod]
        public void SessionShouldBeNull()
        {
            Assert.IsNull(_netConfClient.Session);
        }

        [TestMethod]
        public void NetConfSessionShouldBeNull()
        {
            Assert.IsNull(_netConfClient.NetConfSession);
        }

        [TestMethod]
        public void ErrorOccuredOnSessionShouldNoLongerBeSignaledViaErrorOccurredOnNetConfClient()
        {
            var errorOccurredSignalCount = 0;

            _netConfClient.ErrorOccurred += (sender, args) => Interlocked.Increment(ref errorOccurredSignalCount);

            _sessionMock.Raise(p => p.ErrorOccured += null, new ExceptionEventArgs(new Exception()));

            Assert.AreEqual(0, errorOccurredSignalCount);
        }

        [TestMethod]
        public void HostKeyReceivedOnSessionShouldNoLongerBeSignaledViaHostKeyReceivedOnSftpClient()
        {
            var hostKeyReceivedSignalCount = 0;

            _netConfClient.HostKeyReceived += (sender, args) => Interlocked.Increment(ref hostKeyReceivedSignalCount);

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
