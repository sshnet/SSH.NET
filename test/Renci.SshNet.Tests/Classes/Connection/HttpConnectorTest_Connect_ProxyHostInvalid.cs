﻿using System.Net.Sockets;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class HttpConnectorTest_Connect_ProxyHostInvalid : HttpConnectorTestBase
    {
        private ConnectionInfo _connectionInfo;
        private SocketException _actualException;
        private Socket _clientSocket;

        protected override void SetupData()
        {
            base.SetupData();

            _connectionInfo = new ConnectionInfo("localhost",
                                                 40,
                                                 "user",
                                                 ProxyTypes.Http,
                                                 "invalid.",
                                                 80,
                                                 "proxyUser",
                                                 "proxyPwd",
                                                 new KeyboardInteractiveAuthenticationMethod("user"));
            _actualException = null;
            _clientSocket = SocketFactory.Create(SocketType.Stream, ProtocolType.Tcp);
        }

        protected override void SetupMocks()
        {
            _ = SocketFactoryMock.Setup(p => p.Create(SocketType.Stream, ProtocolType.Tcp))
                                 .Returns(_clientSocket);
        }

        protected override void Act()
        {
            try
            {
                _ = Connector.Connect(_connectionInfo);
                Assert.Fail();
            }
            catch (SocketException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void ConnectShouldHaveThrownSocketException()
        {
            Assert.IsNotNull(_actualException);
            Assert.IsNull(_actualException.InnerException);
            Assert.IsTrue(_actualException.SocketErrorCode is SocketError.HostNotFound or SocketError.TryAgain);
        }
    }
}
