using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;

namespace Renci.SshNet.Connection
{
    /// <summary>
    /// Establishes a tunnel via an HTTP proxy server.
    /// </summary>
    /// <remarks>
    /// <list type="table">
    ///   <listheader>
    ///     <term>Specification</term>
    ///     <description>URL</description>
    ///   </listheader>
    ///   <item>
    ///     <term>HTTP CONNECT method</term>
    ///     <description>https://tools.ietf.org/html/rfc7231#section-4.3.6</description>
    ///   </item>
    ///   <item>
    ///     <term>HTTP Authentication: Basic and Digest Access Authentication</term>
    ///     <description>https://tools.ietf.org/html/rfc2617</description>
    ///   </item>
    /// </list>
    /// </remarks>
    internal sealed class HttpConnector : ProxyConnector
    {
        public HttpConnector(ISocketFactory socketFactory)
            : base(socketFactory)
        {
        }

        protected override void HandleProxyConnect(IConnectionInfo connectionInfo, Socket socket)
        {
            var httpResponseRe = new Regex(@"HTTP/(?<version>\d[.]\d) (?<statusCode>\d{3}) (?<reasonPhrase>.+)$");
            var httpHeaderRe = new Regex(@"(?<fieldName>[^\[\]()<>@,;:\""/?={} \t]+):(?<fieldValue>.+)?");

            _ = socket.Send(Encoding.ASCII.GetBytes(string.Format(CultureInfo.InvariantCulture,
                                                                                "CONNECT {0}:{1} HTTP/1.0\r\n",
                                                                                connectionInfo.Host,
                                                                                connectionInfo.Port)));

            // Send proxy authorization if specified
            if (!string.IsNullOrEmpty(connectionInfo.ProxyUsername))
            {
                var authorization = string.Format(CultureInfo.InvariantCulture,
                                                  "Proxy-Authorization: Basic {0}\r\n",
                                                  Convert.ToBase64String(Encoding.ASCII.GetBytes($"{connectionInfo.ProxyUsername}:{connectionInfo.ProxyPassword}")));
                _ = socket.Send(Encoding.ASCII.GetBytes(authorization));
            }

            _ = socket.Send(Encoding.ASCII.GetBytes("\r\n"));

            HttpStatusCode? statusCode = null;
            var contentLength = 0;

            while (true)
            {
                var response = SocketReadLine(socket, connectionInfo.Timeout);
                if (response is null)
                {
                    // server shut down socket
                    break;
                }

                if (statusCode is null)
                {
                    var statusMatch = httpResponseRe.Match(response);
                    if (statusMatch.Success)
                    {
                        var httpStatusCode = statusMatch.Result("${statusCode}");
                        statusCode = (HttpStatusCode) int.Parse(httpStatusCode, CultureInfo.InvariantCulture);
                        if (statusCode != HttpStatusCode.OK)
                        {
                            throw new ProxyException($"HTTP: Status code {httpStatusCode}, \"{statusMatch.Result("${reasonPhrase}")}\"");
                        }
                    }

                    continue;
                }

                // continue on parsing message headers coming from the server
                var headerMatch = httpHeaderRe.Match(response);
                if (headerMatch.Success)
                {
                    var fieldName = headerMatch.Result("${fieldName}");
                    if (fieldName.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                    {
                        contentLength = int.Parse(headerMatch.Result("${fieldValue}"), CultureInfo.InvariantCulture);
                    }

                    continue;
                }

                // check if we've reached the CRLF which separates request line and headers from the message body
                if (response.Length == 0)
                {
                    // read response body if specified
                    if (contentLength > 0)
                    {
                        var contentBody = new byte[contentLength];
                        _ = SocketRead(socket, contentBody, 0, contentLength, connectionInfo.Timeout);
                    }

                    break;
                }
            }

            if (statusCode is null)
            {
                throw new ProxyException("HTTP response does not contain status line.");
            }
        }

        /// <summary>
        /// Performs a blocking read on the socket until a line is read.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to read from.</param>
        /// <param name="readTimeout">A <see cref="TimeSpan"/> that represents the time to wait until a line is read.</param>
        /// <exception cref="SshOperationTimeoutException">The read has timed-out.</exception>
        /// <exception cref="SocketException">An error occurred when trying to access the socket.</exception>
        /// <returns>
        /// The line read from the socket, or <see langword="null"/> when the remote server has shutdown and all data has been received.
        /// </returns>
        private static string SocketReadLine(Socket socket, TimeSpan readTimeout)
        {
            var encoding = SshData.Ascii;
            var buffer = new List<byte>();
            var data = new byte[1];

            // read data one byte at a time to find end of line and leave any unhandled information in the buffer
            // to be processed by subsequent invocations
            do
            {
                var bytesRead = SocketAbstraction.Read(socket, data, 0, data.Length, readTimeout);
                if (bytesRead == 0)
                {
                    // the remote server shut down the socket
                    break;
                }

                var b = data[0];
                buffer.Add(b);

                if (b == Session.LineFeed && buffer.Count > 1 && buffer[buffer.Count - 2] == Session.CarriageReturn)
                {
                    // Return line without CRLF
                    return encoding.GetString(buffer.ToArray(), 0, buffer.Count - 2);
                }
            }
            while (true);

            if (buffer.Count == 0)
            {
                return null;
            }

            return encoding.GetString(buffer.ToArray(), 0, buffer.Count);
        }
    }
}
