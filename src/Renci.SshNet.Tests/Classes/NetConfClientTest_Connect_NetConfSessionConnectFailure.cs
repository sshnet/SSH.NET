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
    public class NetConfClientTest_Connect_NetConfSessionConnectFailure : NetConfClientTestBase
    {
        private ConnectionInfo _connectionInfo;
        private ApplicationException _netConfSessionConnectionException;
        private NetConfClient _netConfClient;
        private ApplicationException _actualException;

        protected override void SetupData()
        {
            _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));
            _netConfSessionConnectionException = new ApplicationException();
            _netConfClient = new NetConfClient(_connectionInfo, false, _serviceFactoryMock.Object);
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
                               .Setup(p => p.CreateNetConfSession(_sessionMock.Object, -1))
                               .Returns(_netConfSessionMock.Object);
            _netConfSessionMock.InSequence(sequence)
                            .Setup(p => p.Connect())
                            .Throws(_netConfSessionConnectionException);
            _netConfSessionMock.InSequence(sequence)
                            .Setup(p => p.Dispose());
            _sessionMock.InSequence(sequence)
                        .Setup(p => p.Dispose());
        }

        protected override void Act()
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
