using System;
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
#pragma warning disable MA0204 // Remove unnecessary partial modifier; not true for all targets
    internal sealed partial class ProtocolVersionExchange : IProtocolVersionExchange
    {
        private const byte Null = 0x00;
        private const string ServerVersionPattern = "^SSH-(?<protoversion>[^-]+)-(?<softwareversion>.*?)([ ](?<comments>.+))?$";

        private const int MaximumBannerLines = 1024;
        private const int MaximumBannerLineLength = 8192;

#if NET
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
            for (var n = 0; n < MaximumBannerLines; n++)
            {
                var line = SocketReadLine(socket, timeout, bytesReceived);

                var identificationMatch = ServerVersionRegex.Match(line);
                if (identificationMatch.Success)
                {
                    return new SshIdentification(GetGroupValue(identificationMatch, "protoversion"),
                                                 GetGroupValue(identificationMatch, "softwareversion"),
                                                 GetGroupValue(identificationMatch, "comments"));
                }
            }

            throw CreateTooManyLinesReceivedException();
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
#if NET
            await SocketAbstraction.SendAsync(socket, Encoding.UTF8.GetBytes(clientVersion + "\x0D\x0A"), cancellationToken).ConfigureAwait(false);
#else
            SocketAbstraction.Send(socket, Encoding.UTF8.GetBytes(clientVersion + "\x0D\x0A"));
#endif // NET

            var bytesReceived = new List<byte>();

            // Get server version from the server,
            // ignore text lines which are sent before if any
            for (var n = 0; n < MaximumBannerLines; n++)
            {
                var line = await SocketReadLineAsync(socket, bytesReceived, cancellationToken).ConfigureAwait(false);

                var identificationMatch = ServerVersionRegex.Match(line);
                if (identificationMatch.Success)
                {
                    return new SshIdentification(GetGroupValue(identificationMatch, "protoversion"),
                                                 GetGroupValue(identificationMatch, "softwareversion"),
                                                 GetGroupValue(identificationMatch, "comments"));
                }
            }

            throw CreateTooManyLinesReceivedException();
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
        private static string SocketReadLine(Socket socket, TimeSpan timeout, List<byte> buffer)
        {
            var data = new byte[1];

            buffer.Clear();

            while (buffer.Count < MaximumBannerLineLength)
            {
                var bytesRead = SocketAbstraction.Read(socket, data, 0, data.Length, timeout);
                if (bytesRead == 0)
                {
                    throw CreateConnectionLostException();
                }

                var byteRead = data[0];
                buffer.Add(byteRead);

                // The null character MUST NOT be sent
                if (byteRead is Null)
                {
                    throw CreateServerResponseContainsNullCharacterException();
                }

                if (byteRead == Session.LineFeed)
                {
                    if (buffer.Count >= 2 && buffer[buffer.Count - 2] == Session.CarriageReturn)
                    {
                        // Return current line without CRLF
                        return Encoding.UTF8.GetString(buffer.ToArray(), 0, buffer.Count - 2);
                    }

                    // Even though RFC4253 clearly indicates that the identification string should be terminated
                    // by a CR LF we also support banners and identification strings that are terminated by a LF

                    // Return current line without LF
                    return Encoding.UTF8.GetString(buffer.ToArray(), 0, buffer.Count - 1);
                }
            }

            throw CreateBannerLineTooLongException();
        }

        private static async Task<string> SocketReadLineAsync(Socket socket, List<byte> buffer, CancellationToken cancellationToken)
        {
            var data = new byte[1];

            buffer.Clear();

            while (buffer.Count < MaximumBannerLineLength)
            {
                var bytesRead = await SocketAbstraction.ReadAsync(socket, data, cancellationToken).ConfigureAwait(false);
                if (bytesRead == 0)
                {
                    throw CreateConnectionLostException();
                }

                var byteRead = data[0];
                buffer.Add(byteRead);

                // The null character MUST NOT be sent
                if (byteRead is Null)
                {
                    throw CreateServerResponseContainsNullCharacterException();
                }

                if (byteRead == Session.LineFeed)
                {
                    if (buffer.Count >= 2 && buffer[buffer.Count - 2] == Session.CarriageReturn)
                    {
                        // Return current line without CRLF
                        return Encoding.UTF8.GetString(buffer.ToArray(), 0, buffer.Count - 2);
                    }

                    // Even though RFC4253 clearly indicates that the identification string should be terminated
                    // by a CR LF we also support banners and identification strings that are terminated by a LF

                    // Return current line without LF
                    return Encoding.UTF8.GetString(buffer.ToArray(), 0, buffer.Count - 1);
                }
            }

            throw CreateBannerLineTooLongException();
        }

        private static SshConnectionException CreateConnectionLostException()
        {
            return new SshConnectionException(
                "The connection to the remote server was closed before a valid SSH identification string was received.",
                DisconnectReason.ConnectionLost);
        }

        private static SshConnectionException CreateServerResponseContainsNullCharacterException()
        {
            return new SshConnectionException(
                "The server response contained an invalid null character before a valid SSH identification string was received.",
                DisconnectReason.ProtocolError);
        }

        private static SshConnectionException CreateTooManyLinesReceivedException()
        {
            var message = string.Format(
                CultureInfo.InvariantCulture,
                "The server response exceeded the maximum allowed number of banner lines ({0}) before a valid SSH identification string was received.",
                MaximumBannerLines);

            return new SshConnectionException(message, DisconnectReason.ProtocolError);
        }

        private static SshConnectionException CreateBannerLineTooLongException()
        {
            var message = string.Format(
                CultureInfo.InvariantCulture,
                "The server returned a banner line that exceeds the maximum allowed length of {0} bytes.",
                MaximumBannerLineLength);

            return new SshConnectionException(message, DisconnectReason.ProtocolError);
        }
    }
}
