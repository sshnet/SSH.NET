using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using System;
using System.Net.Sockets;
using System.Text;

namespace Renci.SshNet.Connection
{
    /// <summary>
    /// Establishes a tunnel via a SOCKS4 proxy server.
    /// </summary>
    /// <remarks>
    /// https://www.openssh.com/txt/socks4.protocol
    /// </remarks>
    internal class Socks4Connector : ConnectorBase
    {
        public Socks4Connector(ISocketFactory socketFactory) : base(socketFactory)
        {
        }

        public override Socket Connect(IConnectionInfo connectionInfo)
        {
            var socket = SocketConnect(connectionInfo.ProxyHost, connectionInfo.ProxyPort, connectionInfo.Timeout);

            try
            {
                HandleProxyConnect(connectionInfo, socket);
                return socket;
            }
            catch (Exception)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Dispose();

                throw;
            }
        }

        /// <summary>
        /// Establishes a connection to the server via a SOCKS5 proxy.
        /// </summary>
        /// <param name="connectionInfo">The connection information.</param>
        /// <param name="socket">The <see cref="Socket"/>.</param>
        private void HandleProxyConnect(IConnectionInfo connectionInfo, Socket socket)
        {
            var connectionRequest = CreateSocks4ConnectionRequest(connectionInfo.Host, (ushort)connectionInfo.Port, connectionInfo.ProxyUsername);
            SocketAbstraction.Send(socket, connectionRequest);

            //  Read reply version
            if (SocketReadByte(socket, connectionInfo.Timeout) != 0x00)
            {
                throw new ProxyException("SOCKS4: Null is expected.");
            }

            //  Read response code
            var code = SocketReadByte(socket, connectionInfo.Timeout);

            switch (code)
            {
                case 0x5a:
                    break;
                case 0x5b:
                    throw new ProxyException("SOCKS4: Connection rejected.");
                case 0x5c:
                    throw new ProxyException("SOCKS4: Client is not running identd or not reachable from the server.");
                case 0x5d:
                    throw new ProxyException("SOCKS4: Client's identd could not confirm the user ID string in the request.");
                default:
                    throw new ProxyException("SOCKS4: Not valid response.");
            }

            var destBuffer = new byte[6]; // destination port and IP address should be ignored
            SocketRead(socket, destBuffer, 0, destBuffer.Length, connectionInfo.Timeout);
        }

        private static byte[] CreateSocks4ConnectionRequest(string hostname, ushort port, string username)
        {
            var addressBytes = GetSocks4DestinationAddress(hostname);
            var proxyUserBytes = GetProxyUserBytes(username);

            var connectionRequest = new byte
                [
                    // SOCKS version number
                    1 +
                    // Command code
                    1 +
                    // Port number
                    2 +
                    // IP address
                    addressBytes.Length +
                    // Username
                    proxyUserBytes.Length +
                    // Null terminator
                    1
                ];

            var index = 0;

            // SOCKS version number
            connectionRequest[index++] = 0x04;

            // Command code
            connectionRequest[index++] = 0x01; // establish a TCP/IP stream connection

            // Port number
            Pack.UInt16ToBigEndian(port, connectionRequest, index);
            index += 2;

            // Address
            Buffer.BlockCopy(addressBytes, 0, connectionRequest, index, addressBytes.Length);
            index += addressBytes.Length;

            // User name
            Buffer.BlockCopy(proxyUserBytes, 0, connectionRequest, index, proxyUserBytes.Length);
            index += proxyUserBytes.Length;

            // Null terminator
            connectionRequest[index] = 0x00;

            return connectionRequest;
        }

        private static byte[] GetSocks4DestinationAddress(string hostname)
        {
            var addresses = DnsAbstraction.GetHostAddresses(hostname);

            for (var i = 0; i < addresses.Length; i++)
            {
                var address = addresses[i];
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return address.GetAddressBytes();
                }
            }

            throw new ProxyException(string.Format("SOCKS4 only supports IPv4. No such address found for '{0}'.", hostname));
        }

        private static byte[] GetProxyUserBytes(string proxyUser)
        {
            if (proxyUser == null)
            {
                return Array<byte>.Empty;
            }

#if FEATURE_ENCODING_ASCII
            return Encoding.ASCII.GetBytes(proxyUser);
#else
            return new ASCIIEncoding().GetBytes(proxyUser);
#endif
        }
    }
}
