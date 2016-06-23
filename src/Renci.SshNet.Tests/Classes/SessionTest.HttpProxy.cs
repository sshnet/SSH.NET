using System;
using System.Net;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    public partial class SessionTest
    {
        [TestMethod]
        public void ConnectShouldThrowProxyExceptionWhenHttpProxyResponseDoesNotContainStatusLine()
        {
            var proxyEndPoint = new IPEndPoint(IPAddress.Loopback, 8123);
            var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);

            using (var proxyStub = new HttpProxyStub(proxyEndPoint))
            {
                proxyStub.Responses.Add(Encoding.ASCII.GetBytes("Whatever\r\n"));
                proxyStub.Start();

                using (var session = new Session(CreateConnectionInfoWithProxy(proxyEndPoint, serverEndPoint, "anon"), _serviceFactoryMock.Object))
                {
                    try
                    {
                        session.Connect();
                        Assert.Fail();
                    }
                    catch (ProxyException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual("HTTP response does not contain status line.", ex.Message);
                    }
                }
            }
        }

        [TestMethod]
        public void ConnectShouldThrowProxyExceptionWhenHttpProxyReturnsHttpStatusOtherThan200()
        {
            var proxyEndPoint = new IPEndPoint(IPAddress.Loopback, 8123);
            var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);

            using (var proxyStub = new HttpProxyStub(proxyEndPoint))
            {
                proxyStub.Responses.Add(Encoding.ASCII.GetBytes("HTTP/1.0 501 Custom\r\n"));
                proxyStub.Start();

                using (var session = new Session(CreateConnectionInfoWithProxy(proxyEndPoint, serverEndPoint, "anon"), _serviceFactoryMock.Object))
                {
                    try
                    {
                        session.Connect();
                        Assert.Fail();
                    }
                    catch (ProxyException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual("HTTP: Status code 501, \"Custom\"", ex.Message);
                    }
                }
            }
        }

        [TestMethod]
        public void ConnectShouldSkipHeadersWhenHttpProxyReturnsHttpStatus200()
        {
            var proxyEndPoint = new IPEndPoint(IPAddress.Loopback, 8123);
            var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);

            using (var proxyStub = new HttpProxyStub(proxyEndPoint))
            {
                proxyStub.Responses.Add(Encoding.ASCII.GetBytes("HTTP/1.0 200 OK\r\n"));
                proxyStub.Responses.Add(Encoding.ASCII.GetBytes("Content-Type: application/octet-stream\r\n"));
                proxyStub.Responses.Add(Encoding.ASCII.GetBytes("\r\n"));
                proxyStub.Responses.Add(Encoding.ASCII.GetBytes("SSH-666-SshStub"));
                proxyStub.Start();

                using (var session = new Session(CreateConnectionInfoWithProxy(proxyEndPoint, serverEndPoint, "anon"), _serviceFactoryMock.Object))
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
                    }
                }
            }
        }

        [TestMethod]
        public void ConnectShouldSkipContentWhenHttpProxyReturnsHttpStatus200()
        {
            var proxyEndPoint = new IPEndPoint(IPAddress.Loopback, 8123);
            var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);

            using (var proxyStub = new HttpProxyStub(proxyEndPoint))
            {
                proxyStub.Responses.Add(Encoding.ASCII.GetBytes("HTTP/1.0 200 OK\r\n"));
                proxyStub.Responses.Add(Encoding.ASCII.GetBytes("Content-Length: 13\r\n"));
                proxyStub.Responses.Add(Encoding.ASCII.GetBytes("Content-Type: application/octet-stream\r\n"));
                proxyStub.Responses.Add(Encoding.ASCII.GetBytes("\r\n"));
                proxyStub.Responses.Add(Encoding.ASCII.GetBytes("DUMMY_CONTENT"));
                proxyStub.Responses.Add(Encoding.ASCII.GetBytes("SSH-666-SshStub"));
                proxyStub.Start();

                using (var session = new Session(CreateConnectionInfoWithProxy(proxyEndPoint, serverEndPoint, "anon"), _serviceFactoryMock.Object))
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
                    }
                }
            }
        }

        [TestMethod]
        public void ConnectShouldWriteConnectMethodToHttpProxy()
        {
            var proxyEndPoint = new IPEndPoint(IPAddress.Loopback, 8123);
            var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);

            using (var proxyStub = new HttpProxyStub(proxyEndPoint))
            {
                proxyStub.Responses.Add(Encoding.ASCII.GetBytes("HTTP/1.0 501 Custom\r\n"));
                proxyStub.Start();

                using (var session = new Session(CreateConnectionInfoWithProxy(proxyEndPoint, serverEndPoint, "anon"), _serviceFactoryMock.Object))
                {
                    try
                    {
                        session.Connect();
                        Assert.Fail();
                    }
                    catch (ProxyException)
                    {
                    }
                }

                Assert.AreEqual(string.Format("CONNECT {0} HTTP/1.0", serverEndPoint), proxyStub.HttpRequest.RequestLine);
            }
        }

        [TestMethod]
        public void ConnectShouldWriteProxyAuthorizationToHttpProxyWhenProxyUserNameIsNotNullAndNotEmpty()
        {
            var proxyEndPoint = new IPEndPoint(IPAddress.Loopback, 8123);
            var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);

            using (var proxyStub = new HttpProxyStub(proxyEndPoint))
            {
                proxyStub.Responses.Add(Encoding.ASCII.GetBytes("HTTP/1.0 501 Custom\r\n"));
                proxyStub.Start();

                var connectionInfo = CreateConnectionInfoWithProxy(proxyEndPoint, serverEndPoint, "anon");
                using (var session = new Session(connectionInfo, _serviceFactoryMock.Object))
                {
                    try
                    {
                        session.Connect();
                        Assert.Fail();
                    }
                    catch (ProxyException)
                    {
                    }
                }

                var expectedProxyAuthorizationHeader = CreateProxyAuthorizationHeader(connectionInfo);
                Assert.IsNotNull(proxyStub.HttpRequest.Headers.SingleOrDefault(p => p == expectedProxyAuthorizationHeader));
            }
        }

        [TestMethod]
        public void ConnectShouldNotWriteProxyAuthorizationToHttpProxyWhenProxyUserNameIsEmpty()
        {
            var proxyEndPoint = new IPEndPoint(IPAddress.Loopback, 8123);
            var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);

            using (var proxyStub = new HttpProxyStub(proxyEndPoint))
            {
                proxyStub.Responses.Add(Encoding.ASCII.GetBytes("HTTP/1.0 501 Custom\r\n"));
                proxyStub.Start();

                var connectionInfo = CreateConnectionInfoWithProxy(proxyEndPoint, serverEndPoint, string.Empty);
                using (var session = new Session(connectionInfo, _serviceFactoryMock.Object))
                {
                    try
                    {
                        session.Connect();
                        Assert.Fail();
                    }
                    catch (ProxyException)
                    {
                    }
                }

                Assert.IsFalse(proxyStub.HttpRequest.Headers.Any(p => p.StartsWith("Proxy-Authorization:")));
            }
        }

        [TestMethod]
        public void ConnectShouldNotWriteProxyAuthorizationToHttpProxyWhenProxyUserNameIsNull()
        {
            var proxyEndPoint = new IPEndPoint(IPAddress.Loopback, 8123);
            var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);

            using (var proxyStub = new HttpProxyStub(proxyEndPoint))
            {
                proxyStub.Responses.Add(Encoding.ASCII.GetBytes("HTTP/1.0 501 Custom\r\n"));
                proxyStub.Start();

                var connectionInfo = CreateConnectionInfoWithProxy(proxyEndPoint, serverEndPoint, null);
                using (var session = new Session(connectionInfo, _serviceFactoryMock.Object))
                {
                    try
                    {
                        session.Connect();
                        Assert.Fail();
                    }
                    catch (ProxyException)
                    {
                    }
                }

                Assert.IsFalse(proxyStub.HttpRequest.Headers.Any(p => p.StartsWith("Proxy-Authorization:")));
            }
        }

        private static ConnectionInfo CreateConnectionInfoWithProxy(IPEndPoint proxyEndPoint, IPEndPoint serverEndPoint, string proxyUserName)
        {
            return new ConnectionInfo(
                serverEndPoint.Address.ToString(),
                serverEndPoint.Port,
                "eric",
                ProxyTypes.Http,
                proxyEndPoint.Address.ToString(),
                proxyEndPoint.Port,
                proxyUserName,
                "proxypwd",
                new NoneAuthenticationMethod("eric"));
        }

        private static string CreateProxyAuthorizationHeader(ConnectionInfo connectionInfo)
        {
            return string.Format("Proxy-Authorization: Basic {0}",
                Convert.ToBase64String(
                    Encoding.ASCII.GetBytes(string.Format("{0}:{1}", connectionInfo.ProxyUsername,
                        connectionInfo.ProxyPassword))));
        }
    }
}
