﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Connection
{
    /// <summary>
    /// Handles the SSH protocol version exchange.
    /// </summary>
    /// <remarks>
    /// https://tools.ietf.org/html/rfc4253#section-4.2.
    /// </remarks>
    internal sealed partial class ProtocolVersionExchange : IProtocolVersionExchange
    {
        private const byte Null = 0x00;
        private const string ServerVersionPattern = "^SSH-(?<protoversion>[^-]+)-(?<softwareversion>.+?)([ ](?<comments>.+))?$";

#if NET7_0_OR_GREATER
        private static readonly Regex ServerVersionRegex = GetServerVersionRegex();

        [GeneratedRegex(ServerVersionPattern, RegexOptions.ExplicitCapture)]
        private static partial Regex GetServerVersionRegex();
#else
        private static readonly Regex ServerVersionRegex = new Regex(ServerVersionPattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture);
#endif

        /// <summary>
        /// Performs the SSH protocol version exchange.
        /// </summary>
        /// <param name="clientVersion">The identification string of the SSH client.</param>
        /// <param name="socket">A <see cref="Socket"/> connected to the server.</param>
        /// <param name="timeout">The maximum time to wait for the server to respond.</param>
        /// <returns>
        /// The SSH identification of the server.
        /// </returns>
        public SshIdentification Start(string clientVersion, Socket socket, TimeSpan timeout)
        {
            // Immediately send the identification string since the spec states both sides MUST send an identification string
            // when the connection has been established
            SocketAbstraction.Send(socket, Encoding.UTF8.GetBytes(clientVersion + "\x0D\x0A"));

            var bytesReceived = new List<byte>();

            // Get server version from the server,
            // ignore text lines which are sent before if any
            while (true)
            {
                var line = SocketReadLine(socket, timeout, bytesReceived);
                if (line is null)
                {
                    if (bytesReceived.Count == 0)
                    {
                        throw CreateConnectionLostException();
                    }

                    throw CreateServerResponseDoesNotContainIdentification(bytesReceived);
                }

                var identificationMatch = ServerVersionRegex.Match(line);
                if (identificationMatch.Success)
                {
                    return new SshIdentification(GetGroupValue(identificationMatch, "protoversion"),
                                                 GetGroupValue(identificationMatch, "softwareversion"),
                                                 GetGroupValue(identificationMatch, "comments"));
                }
            }
        }

        /// <summary>
        /// Asynchronously performs the SSH protocol version exchange.
        /// </summary>
        /// <param name="clientVersion">The identification string of the SSH client.</param>
        /// <param name="socket">A <see cref="Socket"/> connected to the server.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the SSH protocol version exchange. The value of its
        /// <see cref="Task{Task}.Result"/> contains the SSH identification of the server.
        /// </returns>
        public async Task<SshIdentification> StartAsync(string clientVersion, Socket socket, CancellationToken cancellationToken)
        {
            // Immediately send the identification string since the spec states both sides MUST send an identification string
            // when the connection has been established
#if NET6_0_OR_GREATER
            await SocketAbstraction.SendAsync(socket, Encoding.UTF8.GetBytes(clientVersion + "\x0D\x0A"), cancellationToken).ConfigureAwait(false);
#else
            SocketAbstraction.Send(socket, Encoding.UTF8.GetBytes(clientVersion + "\x0D\x0A"));
#endif // NET6_0_OR_GREATER

            var bytesReceived = new List<byte>();

            // Get server version from the server,
            // ignore text lines which are sent before if any
            while (true)
            {
                var line = await SocketReadLineAsync(socket, bytesReceived, cancellationToken).ConfigureAwait(false);
                if (line is null)
                {
                    if (bytesReceived.Count == 0)
                    {
                        throw CreateConnectionLostException();
                    }

                    throw CreateServerResponseDoesNotContainIdentification(bytesReceived);
                }

                var identificationMatch = ServerVersionRegex.Match(line);
                if (identificationMatch.Success)
                {
                    return new SshIdentification(GetGroupValue(identificationMatch, "protoversion"),
                                                 GetGroupValue(identificationMatch, "softwareversion"),
                                                 GetGroupValue(identificationMatch, "comments"));
                }
            }
        }

        private static string GetGroupValue(Match match, string groupName)
        {
            var commentsGroup = match.Groups[groupName];
            if (commentsGroup.Success)
            {
                return commentsGroup.Value;
            }

            return null;
        }

        /// <summary>
        /// Performs a blocking read on the socket until a line is read.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to read from.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the time to wait until a line is read.</param>
        /// <param name="buffer">A <see cref="List{Byte}"/> to which read bytes will be added.</param>
        /// <exception cref="SshOperationTimeoutException">The read has timed-out.</exception>
        /// <exception cref="SocketException">An error occurred when trying to access the socket.</exception>
        /// <returns>
        /// The line read from the socket, or <see langword="null"/> when the remote server has shutdown and all data has been received.
        /// </returns>
        private static string SocketReadLine(Socket socket, TimeSpan timeout, List<byte> buffer)
        {
            var data = new byte[1];

            var startPosition = buffer.Count;

            // Read data one byte at a time to find end of line and leave any unhandled information in the buffer
            // to be processed by subsequent invocations.
            while (true)
            {
                var bytesRead = SocketAbstraction.Read(socket, data, 0, data.Length, timeout);
                if (bytesRead == 0)
                {
                    // The remote server shut down the socket.
                    break;
                }

                var byteRead = data[0];
                buffer.Add(byteRead);

                // The null character MUST NOT be sent
                if (byteRead is Null)
                {
                    throw CreateServerResponseContainsNullCharacterException(buffer);
                }

                if (byteRead == Session.LineFeed)
                {
                    if (buffer.Count > startPosition + 1 && buffer[buffer.Count - 2] == Session.CarriageReturn)
                    {
                        // Return current line without CRLF
                        return Encoding.UTF8.GetString(buffer.ToArray(), startPosition, buffer.Count - (startPosition + 2));
                    }

                    // Even though RFC4253 clearly indicates that the identification string should be terminated
                    // by a CR LF we also support banners and identification strings that are terminated by a LF

                    // Return current line without LF
                    return Encoding.UTF8.GetString(buffer.ToArray(), startPosition, buffer.Count - (startPosition + 1));
                }
            }

            return null;
        }

        private static async Task<string> SocketReadLineAsync(Socket socket, List<byte> buffer, CancellationToken cancellationToken)
        {
            var data = new byte[1];

            var startPosition = buffer.Count;

            // Read data one byte at a time to find end of line and leave any unhandled information in the buffer
            // to be processed by subsequent invocations.
            while (true)
            {
                var bytesRead = await SocketAbstraction.ReadAsync(socket, data, cancellationToken).ConfigureAwait(false);
                if (bytesRead == 0)
                {
                    throw new SshConnectionException("The connection was closed by the remote host.");
                }

                var byteRead = data[0];
                buffer.Add(byteRead);

                // The null character MUST NOT be sent
                if (byteRead is Null)
                {
                    throw CreateServerResponseContainsNullCharacterException(buffer);
                }

                if (byteRead == Session.LineFeed)
                {
                    if (buffer.Count > startPosition + 1 && buffer[buffer.Count - 2] == Session.CarriageReturn)
                    {
                        // Return current line without CRLF
                        return Encoding.UTF8.GetString(buffer.ToArray(), startPosition, buffer.Count - (startPosition + 2));
                    }

                    // Even though RFC4253 clearly indicates that the identification string should be terminated
                    // by a CR LF we also support banners and identification strings that are terminated by a LF

                    // Return current line without LF
                    return Encoding.UTF8.GetString(buffer.ToArray(), startPosition, buffer.Count - (startPosition + 1));
                }
            }
        }

        private static SshConnectionException CreateConnectionLostException()
        {
#pragma warning disable SA1118 // Parameter should not span multiple lines
            var message = string.Format(CultureInfo.InvariantCulture,
                                        "The server response does not contain an SSH identification string.{0}" +
                                        "The connection to the remote server was closed before any data was received.{0}{0}" +
                                        "More information on the Protocol Version Exchange is available here:{0}" +
                                        "https://tools.ietf.org/html/rfc4253#section-4.2",
                                        Environment.NewLine);
#pragma warning restore SA1118 // Parameter should not span multiple lines

            return new SshConnectionException(message, DisconnectReason.ConnectionLost);
        }

        private static SshConnectionException CreateServerResponseContainsNullCharacterException(List<byte> buffer)
        {
#pragma warning disable SA1118 // Parameter should not span multiple lines
            var message = string.Format(CultureInfo.InvariantCulture,
                                        "The server response contains a null character at position 0x{0:X8}:{1}{1}{2}{1}{1}" +
                                        "A server must not send a null character before the Protocol Version Exchange is complete.{1}{1}" +
                                        "More information is available here:{1}" +
                                        "https://tools.ietf.org/html/rfc4253#section-4.2",
                                        buffer.Count,
                                        Environment.NewLine,
                                        PacketDump.Create(buffer.ToArray(), 2));
#pragma warning restore SA1118 // Parameter should not span multiple lines

            throw new SshConnectionException(message);
        }

        private static SshConnectionException CreateServerResponseDoesNotContainIdentification(List<byte> bytesReceived)
        {
#pragma warning disable SA1118 // Parameter should not span multiple lines
            var message = string.Format(CultureInfo.InvariantCulture,
                                        "The server response does not contain an SSH identification string:{0}{0}{1}{0}{0}" +
                                        "More information on the Protocol Version Exchange is available here:{0}" +
                                        "https://tools.ietf.org/html/rfc4253#section-4.2",
                                        Environment.NewLine,
                                        PacketDump.Create(bytesReceived, 2));
#pragma warning restore SA1118 // Parameter should not span multiple lines

            throw new SshConnectionException(message, DisconnectReason.ProtocolError);
        }
    }
}
