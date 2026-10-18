#nullable enable
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Abstractions;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>Checks timeout compatibility when socket reads use polling.</summary>
    [TestClass]
    public class SocketReadTimeoutTest
    {
        /// <summary>Preserves the existing socket convention that a zero receive timeout waits indefinitely.</summary>
        [TestMethod]
        public async Task ZeroTimeoutWaitsForDelayedData()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            try
            {
                using var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(listener.LocalEndpoint);
                using var peer = listener.AcceptSocket();
                var buffer = new byte[1];
                var read = Task.Run(() => SocketAbstraction.Read(client, buffer, 0, buffer.Length, TimeSpan.Zero));
                try
                {
                    await Task.Delay(100).ConfigureAwait(false);
                    _ = peer.Send(new byte[] { 42 });
                    var completed = await Task.WhenAny(read, Task.Delay(3000)).ConfigureAwait(false);
                    Assert.AreSame(read, completed, "The read did not complete after data became available.");
                    Assert.AreEqual(1, await read.ConfigureAwait(false));
                    Assert.AreEqual((byte)42, buffer[0]);
                }
                finally
                {
                    peer.Shutdown(SocketShutdown.Both);
                    _ = await Task.WhenAny(read, Task.Delay(3000)).ConfigureAwait(false);
                    _ = read.Exception;
                }
            }
            finally
            {
                listener.Stop();
            }
        }

        /// <summary>Retains the timeout bounds checked before accessing the socket in the previous implementation.</summary>
        [TestMethod]
        [DataRow(-2L)]
        [DataRow(2147483648L)]
        public void InvalidTimeoutIsRejectedBeforeReading(long milliseconds)
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Dispose();
            var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
                SocketAbstraction.Read(socket, new byte[1], 0, 1, TimeSpan.FromMilliseconds(milliseconds)));
            Assert.AreEqual("readTimeout", exception.ParamName);
        }
    }
}
