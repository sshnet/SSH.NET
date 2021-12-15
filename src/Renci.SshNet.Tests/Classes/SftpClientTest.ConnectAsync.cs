#if FEATURE_TAP
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SftpClientTest_ConnectAsync
    {
        [TestMethod]
        public async Task Connect_HostNameInvalid_ShouldThrowSocketExceptionWithErrorCodeHostNotFound()
        {
            var connectionInfo = new ConnectionInfo(Guid.NewGuid().ToString("N"), 40, "user",
                new KeyboardInteractiveAuthenticationMethod("user"));
            var sftpClient = new SftpClient(connectionInfo);

            try
            {
                await sftpClient.ConnectAsync(default);
                Assert.Fail();
            }
            catch (SocketException ex)
            {
                Assert.AreEqual(SocketError.HostNotFound, (SocketError) ex.ErrorCode);
            }
        }

        [TestMethod]
        public async Task Connect_ProxyHostNameInvalid_ShouldThrowSocketExceptionWithErrorCodeHostNotFound()
        {
            var connectionInfo = new ConnectionInfo("localhost", 40, "user", ProxyTypes.Http, Guid.NewGuid().ToString("N"), 80,
                "proxyUser", "proxyPwd", new KeyboardInteractiveAuthenticationMethod("user"));
            var sftpClient = new SftpClient(connectionInfo);

            try
            {
                await sftpClient.ConnectAsync(default);
                Assert.Fail();
            }
            catch (SocketException ex)
            {
                Assert.AreEqual(SocketError.HostNotFound, (SocketError) ex.ErrorCode);
            }
        }
    }
}
#endif