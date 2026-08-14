using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class DefaultConnectionHandlerTest
    {
        [TestMethod]
        public void InstanceShouldReturnTheSameObjectEachTime()
        {
            Assert.AreSame(DefaultConnectionHandler.Instance, DefaultConnectionHandler.Instance);
        }

        [TestMethod]
        public void ConnectShouldEstablishAConnection()
        {
            var connectionInfo = new ConnectionInfo(
                IPAddress.Loopback.ToString(),
                1029,
                "user",
                new NoneAuthenticationMethod("user"));

            using var server = new AsyncSocketListener(new IPEndPoint(IPAddress.Loopback, connectionInfo.Port));
            server.Start();

            using var socket = DefaultConnectionHandler.Instance.Connect(connectionInfo);

            Assert.IsTrue(socket.Connected);
        }

        [TestMethod]
        public async Task ConnectAsyncShouldEstablishAConnection()
        {
            var connectionInfo = new ConnectionInfo(
                IPAddress.Loopback.ToString(),
                1030,
                "user",
                new NoneAuthenticationMethod("user"));

            using var server = new AsyncSocketListener(new IPEndPoint(IPAddress.Loopback, connectionInfo.Port));
            server.Start();

            using var socket = await DefaultConnectionHandler.Instance.ConnectAsync(connectionInfo, CancellationToken.None);

            Assert.IsTrue(socket.Connected);
        }
    }
}
