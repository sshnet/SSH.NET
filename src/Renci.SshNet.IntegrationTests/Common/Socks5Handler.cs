using System.Net;
using System.Net.Sockets;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.IntegrationTests.Common
{
    class Socks5Handler
    {
        private readonly IPEndPoint _proxyEndPoint;
        private readonly string _userName;
        private readonly string _password;

        public Socks5Handler(IPEndPoint proxyEndPoint, string userName, string password)
        {
            _proxyEndPoint = proxyEndPoint;
            _userName = userName;
            _password = password;
        }

        public Socket Connect(IPEndPoint endPoint)
        {
            if (endPoint == null)
            {
                throw new ArgumentNullException("endPoint");
            }

            var addressBytes = GetAddressBytes(endPoint);
            return Connect(addressBytes, endPoint.Port);
        }

        public Socket Connect(string host, int port)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            if (host.Length > byte.MaxValue)
            {
                throw new ArgumentException($@"Cannot be more than {byte.MaxValue} characters.", nameof(host));
            }

            var addressBytes = new byte[host.Length + 2];
            addressBytes[0] = 0x03;
            addressBytes[1] = (byte) host.Length;
            Encoding.ASCII.GetBytes(host, 0, host.Length, addressBytes, 2);
            return Connect(addressBytes, port);
        }

        private Socket Connect(byte[] addressBytes, int port)
        {
            var socket = SocketAbstraction.Connect(_proxyEndPoint, TimeSpan.FromSeconds(5));

            //  Send socks version number
            SocketWriteByte(socket, 0x05);

            //  Send number of supported authentication methods
            SocketWriteByte(socket, 0x02);

            //  Send supported authentication methods
            SocketWriteByte(socket, 0x00); //  No authentication
            SocketWriteByte(socket, 0x02); //  Username/Password

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

                    //  Send version
                    SocketWriteByte(socket, 0x01);

                    var username = Encoding.ASCII.GetBytes(_userName);
                    if (username.Length > byte.MaxValue)
                    {
                        throw new ProxyException("Proxy username is too long.");
                    }

                    //  Send username length
                    SocketWriteByte(socket, (byte) username.Length);

                    //  Send username
                    SocketAbstraction.Send(socket, username);

                    var password = Encoding.ASCII.GetBytes(_password);

                    if (password.Length > byte.MaxValue)
                    {
                        throw new ProxyException("Proxy password is too long.");
                    }

                    //  Send username length
                    SocketWriteByte(socket, (byte) password.Length);

                    //  Send username
                    SocketAbstraction.Send(socket, password);

                    var serverVersion = SocketReadByte(socket);

                    if (serverVersion != 1)
                    {
                        throw new ProxyException("SOCKS5: Server authentication version is not valid.");
                    }

                    var statusCode = SocketReadByte(socket);
                    if (statusCode != 0)
                    {
                        throw new ProxyException("SOCKS5: Username/Password authentication failed.");
                    }

                    break;
                case 0xFF:
                    throw new ProxyException("SOCKS5: No acceptable authentication methods were offered.");
                default:
                    throw new ProxyException("SOCKS5: No acceptable authentication methods were offered.");
            }

            //  Send socks version number
            SocketWriteByte(socket, 0x05);

            //  Send command code
            SocketWriteByte(socket, 0x01); //  establish a TCP/IP stream connection

            //  Send reserved, must be 0x00
            SocketWriteByte(socket, 0x00);

            //  Send address type and address
            SocketAbstraction.Send(socket, addressBytes);

            //  Send port
            SocketWriteByte(socket, (byte)(port / 0xFF));
            SocketWriteByte(socket, (byte)(port % 0xFF));

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
                    throw new ProxyException("SOCKS4: Not valid response.");
            }

            //  Read 0
            if (SocketReadByte(socket) != 0)
            {
                throw new ProxyException("SOCKS5: 0 byte is expected.");
            }

            var addressType = SocketReadByte(socket);
            var responseIp = new byte[16];

            switch (addressType)
            {
                case 0x01:
                    SocketRead(socket, responseIp, 0, 4);
                    break;
                case 0x04:
                    SocketRead(socket, responseIp, 0, 16);
                    break;
                default:
                    throw new ProxyException(string.Format("Address type '{0}' is not supported.", addressType));
            }

            var portBytes = new byte[2];

            //  Read 2 bytes to be ignored
            SocketRead(socket, portBytes, 0, 2);

            return socket;
        }

        private static byte[] GetAddressBytes(IPEndPoint endPoint)
        {
            if (endPoint.AddressFamily == AddressFamily.InterNetwork)
            {
                var addressBytes = new byte[4 + 1];
                addressBytes[0] = 0x01;
                var address = endPoint.Address.GetAddressBytes();
                Buffer.BlockCopy(address, 0, addressBytes, 1, address.Length);
                return addressBytes;
            }

            if (endPoint.AddressFamily == AddressFamily.InterNetworkV6)
            {
                var addressBytes = new byte[16 + 1];
                addressBytes[0] = 0x04;
                var address = endPoint.Address.GetAddressBytes();
                Buffer.BlockCopy(address, 0, addressBytes, 1, address.Length);
                return addressBytes;
            }

            throw new ProxyException(string.Format("SOCKS5: IP address '{0}' is not supported.", endPoint.Address));
        }

        private static void SocketWriteByte(Socket socket, byte data)
        {
            SocketAbstraction.Send(socket, new[] { data });
        }

        private static byte SocketReadByte(Socket socket)
        {
            var buffer = new byte[1];
            SocketRead(socket, buffer, 0, 1);
            return buffer[0];
        }

        private static int SocketRead(Socket socket, byte[] buffer, int offset, int length)
        {
            var bytesRead = SocketAbstraction.Read(socket, buffer, offset, length, TimeSpan.FromMilliseconds(-1));
            if (bytesRead == 0)
            {
                // when we're in the disconnecting state (either triggered by client or server), then the
                // SshConnectionException will interrupt the message listener loop (if not already interrupted)
                // and the exception itself will be ignored (in RaiseError)
                throw new SshConnectionException("An established connection was aborted by the server.",
                    DisconnectReason.ConnectionLost);
            }
            return bytesRead;
        }
    }
}
