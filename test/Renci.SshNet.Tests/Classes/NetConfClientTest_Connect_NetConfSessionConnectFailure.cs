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
    internal class NetConfClientTest_Connect_NetConfSessionConnectFailure : NetConfClientTestBase
    {
        private ConnectionInfo _connectionInfo;
        private ApplicationException _netConfSessionConnectionException;
        private NetConfClient _netConfClient;
        private ApplicationException _actualException;

        protected override void SetupData()
        {
            _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));
            _netConfSessionConnectionException = new ApplicationException();
            _netConfClient = new NetConfClient(_connectionInfo, false, ServiceFactoryMock.Object);
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
                                   .Setup(p => p.CreateNetConfSession(SessionMock.Object, -1))
                                   .Returns(NetConfSessionMock.Object);
            _ = NetConfSessionMock.InSequence(sequence)
                                   .Setup(p => p.Connect())
                                   .Throws(_netConfSessionConnectionException);
            _ = NetConfSessionMock.InSequence(sequence)
                                   .Setup(p => p.Dispose());
            _ = SessionMock.InSequence(sequence)
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

            SessionMock.Raise(p => p.ErrorOccured += null, new ExceptionEventArgs(new Exception()));

            Assert.AreEqual(0, errorOccurredSignalCount);
        }

        [TestMethod]
        public void HostKeyReceivedOnSessionShouldNoLongerBeSignaledViaHostKeyReceivedOnSftpClient()
        {
            var hostKeyReceivedSignalCount = 0;

            _netConfClient.HostKeyReceived += (sender, args) => Interlocked.Increment(ref hostKeyReceivedSignalCount);

            SessionMock.Raise(p => p.HostKeyReceived += null, new HostKeyEventArgs(GetKeyHostAlgorithm()));

            Assert.AreEqual(0, hostKeyReceivedSignalCount);
        }

        private static KeyHostAlgorithm GetKeyHostAlgorithm()
        {
            using (var s = TestBase.GetData("Key.RSA.txt"))
            {
                var privateKey = new PrivateKeyFile(s);
                return (KeyHostAlgorithm)privateKey.HostKeyAlgorithms.First();
            }
        }
    }
}
