using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class ProtocolVersionExchangeTest_BannerLimits
    {
        private const int MaximumBannerLines = 1024;
        private const int MaximumBannerLineLength = 8192;

        private IPEndPoint _serverEndPoint;
        private AsyncSocketListener _server;
        private byte[] _serverResponse;

        [TestInitialize]
        public void Setup()
        {
            _serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);

            _server = new AsyncSocketListener(_serverEndPoint);
            _server.Start();
            _server.Connected += socket => socket.Send(_serverResponse);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _server?.Dispose();
            _server = null;
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task TooManyLines_Throws(bool isAsync)
        {
            _serverResponse = Encoding.UTF8.GetBytes(string.Join("\r\n", Enumerable.Repeat("Banner line", MaximumBannerLines + 1)));

            using var client = new SshClient(_serverEndPoint.Address.ToString(), _serverEndPoint.Port, "user", "password");

            var exception = await Assert.ThrowsExactlyAsync<SshConnectionException>(() => Connect(client, isAsync));

            Assert.StartsWith("The server response exceeded the maximum allowed number of banner lines", exception.Message, StringComparison.Ordinal);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task LineTooLong_Throws(bool isAsync)
        {
            _serverResponse = Encoding.UTF8.GetBytes(new string('A', MaximumBannerLineLength + 1));

            using var client = new SshClient(_serverEndPoint.Address.ToString(), _serverEndPoint.Port, "user", "password");

            var exception = await Assert.ThrowsExactlyAsync<SshConnectionException>(() => Connect(client, isAsync));

            Assert.StartsWith("The server returned a banner line that exceeds the maximum allowed length", exception.Message, StringComparison.Ordinal);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task LineOfMaximumLength_IsAccepted(bool isAsync)
        {
            var bannerLine = new string('A', MaximumBannerLineLength - 2) + "\r\n";

            _serverResponse = Encoding.UTF8.GetBytes(bannerLine + "SSH-BadVersion-\r\n");

            using var client = new SshClient(_serverEndPoint.Address.ToString(), _serverEndPoint.Port, "user", "password");

            // Since the "server" is not a full implementation, we let it return a bad protoversion
            // which surfaces a different error in the client and means that we did at least detect
            // the identification.
            var exception = await Assert.ThrowsExactlyAsync<SshConnectionException>(() => Connect(client, isAsync));

            Assert.AreEqual(DisconnectReason.ProtocolVersionNotSupported, exception.DisconnectReason);
            Assert.AreEqual("Server version 'BadVersion' is not supported.", exception.Message);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task MaximumNumberOfLines_IsAccepted(bool isAsync)
        {
            var banner = string.Concat(Enumerable.Repeat("Banner line\r\n", MaximumBannerLines - 1));

            _serverResponse = Encoding.UTF8.GetBytes(banner + "SSH-BadVersion-\r\n");

            using var client = new SshClient(_serverEndPoint.Address.ToString(), _serverEndPoint.Port, "user", "password");

            // Since the "server" is not a full implementation, we let it return a bad protoversion
            // which surfaces a different error in the client and means that we did at least detect
            // the identification.
            var exception = await Assert.ThrowsExactlyAsync<SshConnectionException>(() => Connect(client, isAsync));

            Assert.AreEqual(DisconnectReason.ProtocolVersionNotSupported, exception.DisconnectReason);
            Assert.AreEqual("Server version 'BadVersion' is not supported.", exception.Message);
        }

        private async Task Connect(SshClient client, bool isAsync)
        {
            if (isAsync)
            {
                await client.ConnectAsync(CancellationToken.None);
            }
            else
            {
                client.Connect();
            }
        }
    }
}
