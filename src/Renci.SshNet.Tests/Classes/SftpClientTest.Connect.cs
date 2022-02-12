using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SftpClientTest_Connect
    {
        [TestMethod]
        public void Connect_HostNameInvalid_ShouldThrowSocketExceptionWithErrorCodeHostNotFound()
        {
            var connectionInfo = new ConnectionInfo("invalid.", 40, "user",
                new KeyboardInteractiveAuthenticationMethod("user"));
            var sftpClient = new SftpClient(connectionInfo);

            try
            {
                sftpClient.Connect();
                Assert.Fail();
            }
            catch (SocketException ex)
            {
                Assert.AreEqual(ex.ErrorCode, (int) SocketError.HostNotFound);
            }
        }

        [TestMethod]
        public void Connect_ProxyHostNameInvalid_ShouldThrowSocketExceptionWithErrorCodeHostNotFound()
        {
            var connectionInfo = new ConnectionInfo("localhost", 40, "user", ProxyTypes.Http, "invalid.", 80,
                "proxyUser", "proxyPwd", new KeyboardInteractiveAuthenticationMethod("user"));
            var sftpClient = new SftpClient(connectionInfo);

            try
            {
                sftpClient.Connect();
                Assert.Fail();
            }
            catch (SocketException ex)
            {
                Assert.AreEqual(ex.ErrorCode, (int)SocketError.HostNotFound);
            }
        }
    }
}
