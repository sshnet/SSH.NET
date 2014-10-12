using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality to connect and interact with SSH server.
    /// </summary>
    [TestClass]
    public partial class SessionTest : TestBase
    {
        [TestMethod]
        public void ConnectShouldSkipLinesBeforeProtocolIdentificationString()
        {
            var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
            var connectionInfo = CreateConnectionInfo(serverEndPoint, TimeSpan.FromSeconds(5));

            using (var serverStub = new AsyncSocketListener(serverEndPoint))
            {
                serverStub.Connected += (socket) =>
                {
                    socket.Send(Encoding.ASCII.GetBytes("\r\n"));
                    socket.Send(Encoding.ASCII.GetBytes("WELCOME banner\r\n"));
                    socket.Send(Encoding.ASCII.GetBytes("SSH-666-SshStub\r\n"));
                    socket.Shutdown(SocketShutdown.Send);
                };
                serverStub.Start();

                using (var session = new Session(connectionInfo))
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
                serverStub.Connected += (socket) =>
                {
                    socket.Send(Encoding.ASCII.GetBytes("\r\n"));
                    socket.Send(Encoding.ASCII.GetBytes("WELCOME banner\r\n"));
                    socket.Send(Encoding.ASCII.GetBytes("SSH-666-SshStub"));
                    socket.Shutdown(SocketShutdown.Send);
                };
                serverStub.Start();

                using (var session = new Session(connectionInfo))
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
                serverStub.Connected += (socket) =>
                {
                    socket.Send(Encoding.ASCII.GetBytes("\r\n"));
                    socket.Send(Encoding.ASCII.GetBytes("WELCOME banner\r\n"));
                    clientSocket = socket;
                };
                serverStub.Start();

                using (var session = new Session(CreateConnectionInfo(serverEndPoint, TimeSpan.FromMilliseconds(500))))
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
                serverStub.Connected += (socket) =>
                {
                    socket.Send(Encoding.ASCII.GetBytes("\r\n"));
                    socket.Send(Encoding.ASCII.GetBytes("WELCOME banner\r\n"));
                    socket.Shutdown(SocketShutdown.Send);
                };
                serverStub.Start();

                using (var session = new Session(CreateConnectionInfo(serverEndPoint, TimeSpan.FromSeconds(5))))
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
                serverStub.Connected += (socket) =>
                {
                    socket.Send(Encoding.ASCII.GetBytes("\r\n"));
                    socket.Send(Encoding.ASCII.GetBytes("WELCOME banner"));
                    socket.Shutdown(SocketShutdown.Send);
                };
                serverStub.Start();

                using (var session = new Session(CreateConnectionInfo(serverEndPoint, TimeSpan.FromSeconds(5))))
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
                serverStub.Connected += (socket) =>
                {
                    socket.Send(Encoding.ASCII.GetBytes("\r\n"));
                    socket.Send(Encoding.ASCII.GetBytes("WELCOME banner\r\n"));
                    socket.Send(Encoding.ASCII.GetBytes("\r\n"));
                    socket.Shutdown(SocketShutdown.Send);
                };
                serverStub.Start();

                using (var session = new Session(CreateConnectionInfo(serverEndPoint, TimeSpan.FromSeconds(5))))
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


        /// <summary>
        ///A test for SessionSemaphore
        ///</summary>
        [TestMethod()]
        [Ignore]
        public void SessionSemaphoreTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            Session target = new Session(connectionInfo); // TODO: Initialize to an appropriate value
            SemaphoreLight actual;
            actual = target.SessionSemaphore;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsConnected
        ///</summary>
        [TestMethod()]
        [Ignore]
        public void IsConnectedTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            Session target = new Session(connectionInfo); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsConnected;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ClientInitMessage
        ///</summary>
        [TestMethod()]
        [Ignore]
        public void ClientInitMessageTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            Session target = new Session(connectionInfo); // TODO: Initialize to an appropriate value
            Message actual;
            actual = target.ClientInitMessage;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for UnRegisterMessage
        ///</summary>
        [TestMethod()]
        [Ignore]
        public void UnRegisterMessageTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            Session target = new Session(connectionInfo); // TODO: Initialize to an appropriate value
            string messageName = string.Empty; // TODO: Initialize to an appropriate value
            target.UnRegisterMessage(messageName);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for RegisterMessage
        ///</summary>
        [TestMethod()]
        [Ignore]
        public void RegisterMessageTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            Session target = new Session(connectionInfo); // TODO: Initialize to an appropriate value
            string messageName = string.Empty; // TODO: Initialize to an appropriate value
            target.RegisterMessage(messageName);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod()]
        [Ignore]
        public void DisposeTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            Session target = new Session(connectionInfo); // TODO: Initialize to an appropriate value
            target.Dispose();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Disconnect
        ///</summary>
        [TestMethod()]
        [Ignore]
        public void DisconnectTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            Session target = new Session(connectionInfo); // TODO: Initialize to an appropriate value
            target.Disconnect();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Connect
        ///</summary>
        [TestMethod()]
        [Ignore]
        public void ConnectTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            Session target = new Session(connectionInfo); // TODO: Initialize to an appropriate value
            target.Connect();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
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