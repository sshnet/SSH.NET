using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using System;
using System.Net.Sockets;

namespace Renci.SshNet.Connection
{
    internal class Socks4Connector : ConnectorBase
    {
        public override Socket Connect(IConnectionInfo connectionInfo)
        {
            var socket = SocketConnect(connectionInfo.ProxyHost, connectionInfo.ProxyPort, connectionInfo.Timeout);

            var connectionRequest = CreateSocks4ConnectionRequest(connectionInfo.Host, (ushort)connectionInfo.Port, connectionInfo.ProxyUsername);
            SocketAbstraction.Send(socket, connectionRequest);

            //  Read null byte
            if (SocketReadByte(socket) != 0)
            {
                throw new ProxyException("SOCKS4: Null is expected.");
            }

            //  Read response code
            var code = SocketReadByte(socket);

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

            var dummyBuffer = new byte[6]; // field 3 (2 bytes) and field 4 (4) should be ignored
            SocketRead(socket, dummyBuffer, 0, 6);

            return socket;
        }

        private static byte[] CreateSocks4ConnectionRequest(string hostname, ushort port, string username)
        {
            var addressBytes = GetSocks4DestinationAddress(hostname);

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
                    username.Length +
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
    }
}
