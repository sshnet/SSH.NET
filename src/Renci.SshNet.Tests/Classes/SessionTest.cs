using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality to connect and interact with SSH server.
    /// </summary>
    [TestClass]
    public partial class SessionTest : TestBase
    {
        private Mock<IServiceFactory> _serviceFactoryMock;

        protected override void OnInit()
        {
            base.OnInit();

            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
        }

        [TestMethod]
        public void ConstructorShouldThrowArgumentNullExceptionWhenConnectionInfoIsNull()
        {
            ConnectionInfo connectionInfo = null;
            var serviceFactory = new Mock<IServiceFactory>(MockBehavior.Strict).Object;

            try
            {
                new Session(connectionInfo, serviceFactory);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("connectionInfo", ex.ParamName);
            }
        }

        [TestMethod]
        public void ConstructorShouldThrowArgumentNullExceptionWhenServiceFactoryIsNull()
        {
            var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
            var connectionInfo = CreateConnectionInfo(serverEndPoint, TimeSpan.FromSeconds(5));
            IServiceFactory serviceFactory = null;

            try
            {
                new Session(connectionInfo, serviceFactory);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("serviceFactory", ex.ParamName);
            }
        }

        [TestMethod]
        public void ConnectShouldSkipLinesBeforeProtocolIdentificationString()
        {
            var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
            var connectionInfo = CreateConnectionInfo(serverEndPoint, TimeSpan.FromSeconds(5));

            using (var serverStub = new AsyncSocketListener(serverEndPoint))
            {
                serverStub.Connected += socket =>
                    {
                        socket.Send(Encoding.ASCII.GetBytes("\r\n"));
                        socket.Send(Encoding.ASCII.GetBytes("WELCOME banner\r\n"));
                        socket.Send(Encoding.ASCII.GetBytes("SSH-666-SshStub\r\n"));
                        socket.Shutdown(SocketShutdown.Send);
                    };
                serverStub.Start();

                using (var session = new Session(connectionInfo, _serviceFactoryMock.Object))
                {
                    try
                    {
                        session.Connect();
                        Assert.Fail();
                    }
                    catch (SshConnectionException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual("Server version '666' is not supported.", ex.Message);

                        Assert.AreEqual("SSH-666-SshStub", connectionInfo.ServerVersion);
                    }
                }
            }
        }

        [TestMethod]
        public void ConnectShouldSupportProtocolIdentificationStringThatDoesNotEndWithCrlf()
        {
            var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
            var connectionInfo = CreateConnectionInfo(serverEndPoint, TimeSpan.FromSeconds(5));

            using (var serverStub = new AsyncSocketListener(serverEndPoint))
            {
                serverStub.Connected += socket =>
                    {
                        socket.Send(Encoding.ASCII.GetBytes("\r\n"));
                        socket.Send(Encoding.ASCII.GetBytes("WELCOME banner\r\n"));
                        socket.Send(Encoding.ASCII.GetBytes("SSH-666-SshStub"));
                        socket.Shutdown(SocketShutdown.Send);
                    };
                serverStub.Start();

                using (var session = new Session(connectionInfo, _serviceFactoryMock.Object))
                {
                    try
                    {
                        session.Connect();
                        Assert.Fail();
                    }
                    catch (SshConnectionException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual("Server version '666' is not supported.", ex.Message);

                        Assert.AreEqual("SSH-666-SshStub", connectionInfo.ServerVersion);
                    }
                }
            }
        }

        [TestMethod]
        public void ConnectShouldThrowSshOperationExceptionWhenServerDoesNotRespondWithinConnectionTimeout()
        {
            var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
            var timeout = TimeSpan.FromMilliseconds(500);
            Socket clientSocket = null;

            using (var serverStub = new AsyncSocketListener(serverEndPoint))
            {
                serverStub.Connected += socket =>
                    {
                        socket.Send(Encoding.ASCII.GetBytes("\r\n"));
                        socket.Send(Encoding.ASCII.GetBytes("WELCOME banner\r\n"));
                        clientSocket = socket;
                    };
                serverStub.Start();

                using (var session = new Session(CreateConnectionInfo(serverEndPoint, TimeSpan.FromMilliseconds(500)), _serviceFactoryMock.Object))
                {
                    try
                    {
                        session.Connect();
                        Assert.Fail();
                    }
                    catch (SshOperationTimeoutException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual(string.Format(CultureInfo.InvariantCulture, "Socket read operation has timed out after {0:F0} milliseconds.", timeout.TotalMilliseconds), ex.Message);

                        Assert.IsNotNull(clientSocket);
                        Assert.IsTrue(clientSocket.Connected);

                        // shut down socket
                        clientSocket.Shutdown(SocketShutdown.Send);
                    }
                }
            }
        }

        [TestMethod]
        public void ConnectShouldSshConnectionExceptionWhenServerResponseDoesNotContainProtocolIdentificationString()
        {
            var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);

            // response ends with CRLF
            using (var serverStub = new AsyncSocketListener(serverEndPoint))
            {
                serverStub.Connected += socket =>
                    {
                        socket.Send(Encoding.ASCII.GetBytes("\r\n"));
                        socket.Send(Encoding.ASCII.GetBytes("WELCOME banner\r\n"));
                        socket.Shutdown(SocketShutdown.Send);
                    };
                serverStub.Start();

                using (var session = new Session(CreateConnectionInfo(serverEndPoint, TimeSpan.FromSeconds(5)), _serviceFactoryMock.Object))
                {
                    try
                    {
                        session.Connect();
                        Assert.Fail();
                    }
                    catch (SshConnectionException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual("Server response does not contain SSH protocol identification.", ex.Message);
                    }
                }
            }

            // response does not end with CRLF
            using (var serverStub = new AsyncSocketListener(serverEndPoint))
            {
                serverStub.Connected += socket =>
                    {
                        socket.Send(Encoding.ASCII.GetBytes("\r\n"));
                        socket.Send(Encoding.ASCII.GetBytes("WELCOME banner"));
                        socket.Shutdown(SocketShutdown.Send);
                    };
                serverStub.Start();

                using (var session = new Session(CreateConnectionInfo(serverEndPoint, TimeSpan.FromSeconds(5)), _serviceFactoryMock.Object))
                {
                    try
                    {
                        session.Connect();
                        Assert.Fail();
                    }
                    catch (SshConnectionException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual("Server response does not contain SSH protocol identification.", ex.Message);
                    }
                }
            }

            // last line is empty
            using (var serverStub = new AsyncSocketListener(serverEndPoint))
            {
                serverStub.Connected += socket =>
                    {
                        socket.Send(Encoding.ASCII.GetBytes("\r\n"));
                        socket.Send(Encoding.ASCII.GetBytes("WELCOME banner\r\n"));
                        socket.Send(Encoding.ASCII.GetBytes("\r\n"));
                        socket.Shutdown(SocketShutdown.Send);
                    };
                serverStub.Start();

                using (var session = new Session(CreateConnectionInfo(serverEndPoint, TimeSpan.FromSeconds(5)), _serviceFactoryMock.Object))
                {
                    try
                    {
                        session.Connect();
                        Assert.Fail();
                    }
                    catch (SshConnectionException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual("Server response does not contain SSH protocol identification.", ex.Message);
                    }
                }
            }
        }

        [TestMethod]
        public void Connect_HostNameInvalid_ShouldThrowSocketExceptionWithErrorCodeHostNotFound()
        {
            var connectionInfo = new ConnectionInfo("invalid.", 40, "user",
                new KeyboardInteractiveAuthenticationMethod("user"));
            var session = new Session(connectionInfo, _serviceFactoryMock.Object);

            try
            {
                session.Connect();
                Assert.Fail();
            }
            catch (SocketException ex)
            {
                Assert.AreEqual(ex.ErrorCode, (int)SocketError.HostNotFound);
            }
        }

        [TestMethod]
        public void Connect_ProxyHostNameInvalid_ShouldThrowSocketExceptionWithErrorCodeHostNotFound()
        {
            var connectionInfo = new ConnectionInfo("localhost", 40, "user", ProxyTypes.Http, "invalid.", 80,
                "proxyUser", "proxyPwd", new KeyboardInteractiveAuthenticationMethod("user"));
            var session = new Session(connectionInfo, _serviceFactoryMock.Object);

            try
            {
                session.Connect();
                Assert.Fail();
            }
            catch (SocketException ex)
            {
                Assert.AreEqual(ex.ErrorCode, (int)SocketError.HostNotFound);
            }
        }

        [TestMethod]
        public void DisconnectShouldNotThrowExceptionWhenSocketIsNotConnected()
        {
            var connectionInfo = new ConnectionInfo("localhost", 6767, Resources.USERNAME,
                new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));
            var session = new Session(connectionInfo, _serviceFactoryMock.Object);

            try
            {
                session.Connect();
                Assert.Fail();
            }
            catch (SocketException)
            {
                session.Disconnect();
            }
        }

        [TestMethod]
        public void DisconnectShouldNotThrowExceptionWhenConnectHasNotBeenInvoked()
        {
            var connectionInfo = new ConnectionInfo("localhost", 6767, Resources.USERNAME,
                new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));
            var session = new Session(connectionInfo, _serviceFactoryMock.Object);

            session.Disconnect();
        }

        [TestMethod]
        public void DisposeShouldNotThrowExceptionWhenSocketIsNotConnected()
        {
            var connectionInfo = new ConnectionInfo("localhost", 6767, Resources.USERNAME,
                new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));
            var session = new Session(connectionInfo, _serviceFactoryMock.Object);

            try
            {
                session.Connect();
                Assert.Fail();
            }
            catch (SocketException)
            {
                session.Dispose();
            }
        }

        [TestMethod]
        public void DisposeShouldNotThrowExceptionWhenConenectHasNotBeenInvoked()
        {
            var connectionInfo = new ConnectionInfo("localhost", 6767, Resources.USERNAME,
                new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));
            var session = new Session(connectionInfo, _serviceFactoryMock.Object);

            session.Disconnect();
        }

        private static ConnectionInfo CreateConnectionInfo(IPEndPoint serverEndPoint, TimeSpan timeout)
        {
            var connectionInfo = new ConnectionInfo(
                serverEndPoint.Address.ToString(),
                serverEndPoint.Port,
                "eric",
                new NoneAuthenticationMethod("eric"));
            connectionInfo.Timeout = timeout;
            return connectionInfo;
        }
    }
}