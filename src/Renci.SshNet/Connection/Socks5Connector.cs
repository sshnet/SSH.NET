using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using System;
using System.Net.Sockets;

namespace Renci.SshNet.Connection
{
    /// <summary>
    /// Establishes a tunnel via a SOCKS5 proxy server.
    /// </summary>
    /// <remarks>
    /// https://en.wikipedia.org/wiki/SOCKS#SOCKS5
    /// </remarks>
    internal class Socks5Connector : ConnectorBase
    {
        public Socks5Connector(ISocketFactory socketFactory) : base(socketFactory)
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
            var greeting = new byte[]
                {
                    // SOCKS version number
                    0x05,
                    // Number of supported authentication methods
                    0x02,
                    // No authentication
                    0x00,
                    // Username/Password authentication
                    0x02
                };
            SocketAbstraction.Send(socket, greeting);

            var socksVersion = SocketReadByte(socket);
            if (socksVersion != 0x05)
            {
                throw new ProxyException(string.Format("SOCKS Version '{0}' is not supported.", socksVersion));
            }

            var authenticationMethod = SocketReadByte(socket);
            switch (authenticationMethod)
            {
                case 0x00:
                    break;
                case 0x02:
                    // Create username/password authentication request
                    var authenticationRequest = CreateSocks5UserNameAndPasswordAuthenticationRequest(connectionInfo.ProxyUsername, connectionInfo.ProxyPassword);
                    // Send authentication request
                    SocketAbstraction.Send(socket, authenticationRequest);
                    // Read authentication result
                    var authenticationResult = SocketAbstraction.Read(socket, 2, connectionInfo.Timeout);

                    if (authenticationResult[0] != 0x01)
                        throw new ProxyException("SOCKS5: Server authentication version is not valid.");
                    if (authenticationResult[1] != 0x00)
                        throw new ProxyException("SOCKS5: Username/Password authentication failed.");
                    break;
                case 0xFF:
                    throw new ProxyException("SOCKS5: No acceptable authentication methods were offered.");
            }

            var connectionRequest = CreateSocks5ConnectionRequest(connectionInfo.Host, (ushort) connectionInfo.Port);
            SocketAbstraction.Send(socket, connectionRequest);

            //  Read Server SOCKS5 version
            if (SocketReadByte(socket) != 5)
            {
                throw new ProxyException("SOCKS5: Version 5 is expected.");
            }

            //  Read response code
            var status = SocketReadByte(socket);

            switch (status)
            {
                case 0x00:
                    break;
                case 0x01:
                    throw new ProxyException("SOCKS5: General failure.");
                case 0x02:
                    throw new ProxyException("SOCKS5: Connection not allowed by ruleset.");
                case 0x03:
                    throw new ProxyException("SOCKS5: Network unreachable.");
                case 0x04:
                    throw new ProxyException("SOCKS5: Host unreachable.");
                case 0x05:
                    throw new ProxyException("SOCKS5: Connection refused by destination host.");
                case 0x06:
                    throw new ProxyException("SOCKS5: TTL expired.");
                case 0x07:
                    throw new ProxyException("SOCKS5: Command not supported or protocol error.");
                case 0x08:
                    throw new ProxyException("SOCKS5: Address type not supported.");
                default:
                    throw new ProxyException("SOCKS5: Not valid response.");
            }

            //  Read reserved byte
            if (SocketReadByte(socket) != 0)
            {
                throw new ProxyException("SOCKS5: 0 byte is expected.");
            }

            var addressType = SocketReadByte(socket);
            switch (addressType)
            {
                case 0x01:
                    var ipv4 = new byte[4];
                    SocketRead(socket, ipv4, 0, 4);
                    break;
                case 0x04:
                    var ipv6 = new byte[16];
                    SocketRead(socket, ipv6, 0, 16);
                    break;
                default:
                    throw new ProxyException(string.Format("Address type '{0}' is not supported.", addressType));
            }

            var port = new byte[2];

            //  Read 2 bytes to be ignored
            SocketRead(socket, port, 0, 2);
        }

        /// <summary>
        /// https://tools.ietf.org/html/rfc1929
        /// </summary>
        private static byte[] CreateSocks5UserNameAndPasswordAuthenticationRequest(string username, string password)
        {
            if (username.Length > byte.MaxValue)
                throw new ProxyException("Proxy username is too long.");
            if (password.Length > byte.MaxValue)
                throw new ProxyException("Proxy password is too long.");

            var authenticationRequest = new byte
                [
                    // Version of the negotiation
                    1 +
                    // Length of the username
                    1 +
                    // Username
                    username.Length +
                    // Length of the password
                    1 +
                    // Password
                    password.Length
                ];

            var index = 0;

            // Version of the negiotiation
            authenticationRequest[index++] = 0x01;

            // Length of the username
            authenticationRequest[index++] = (byte) username.Length;

            // Username
            SshData.Ascii.GetBytes(username, 0, username.Length, authenticationRequest, index);
            index += username.Length;

            // Length of the password
            authenticationRequest[index++] = (byte) password.Length;

            // Password
            SshData.Ascii.GetBytes(password, 0, password.Length, authenticationRequest, index);

            return authenticationRequest;
        }

        private static byte[] CreateSocks5ConnectionRequest(string hostname, ushort port)
        {
            byte addressType;
            var addressBytes = GetSocks5DestinationAddress(hostname, out addressType);

            var connectionRequest = new byte
                [
                    // SOCKS version number
                    1 +
                    // Command code
                    1 +
                    // Reserved
                    1 +
                    // Address type
                    1 +
                    // Address
                    addressBytes.Length +
                    // Port number
                    2
                ];

            var index = 0;

            // SOCKS version number
            connectionRequest[index++] = 0x05;

            // Command code
            connectionRequest[index++] = 0x01; // establish a TCP/IP stream connection

            // Reserved
            connectionRequest[index++] = 0x00;

            // Address type
            connectionRequest[index++] = addressType;

            // Address
            Buffer.BlockCopy(addressBytes, 0, connectionRequest, index, addressBytes.Length);
            index += addressBytes.Length;

            // Port number
            Pack.UInt16ToBigEndian(port, connectionRequest, index);

            return connectionRequest;
        }

        private static byte[] GetSocks5DestinationAddress(string hostname, out byte addressType)
        {
            var ip = DnsAbstraction.GetHostAddresses(hostname)[0];

            byte[] address;

            switch (ip.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    addressType = 0x01; // IPv4
                    address = ip.GetAddressBytes();
                    break;
                case AddressFamily.InterNetworkV6:
                    addressType = 0x04; // IPv6
                    address = ip.GetAddressBytes();
                    break;
                default:
                    throw new ProxyException(string.Format("SOCKS5: IP address '{0}' is not supported.", ip));
            }

            return address;
        }
    }
}
