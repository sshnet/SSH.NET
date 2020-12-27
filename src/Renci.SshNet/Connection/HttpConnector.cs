using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Renci.SshNet.Connection
{
    internal class HttpConnector : ConnectorBase
    {
        public override Socket Connect(IConnectionInfo connectionInfo)
        {
            var socket = SocketConnect(connectionInfo.ProxyHost, connectionInfo.ProxyPort, connectionInfo.Timeout);

            var httpResponseRe = new Regex(@"HTTP/(?<version>\d[.]\d) (?<statusCode>\d{3}) (?<reasonPhrase>.+)$");
            var httpHeaderRe = new Regex(@"(?<fieldName>[^\[\]()<>@,;:\""/?={} \t]+):(?<fieldValue>.+)?");

            SocketAbstraction.Send(socket, SshData.Ascii.GetBytes(string.Format("CONNECT {0}:{1} HTTP/1.0\r\n", connectionInfo.Host, connectionInfo.Port)));

            //  Sent proxy authorization is specified
            if (!string.IsNullOrEmpty(connectionInfo.ProxyUsername))
            {
                var authorization = string.Format("Proxy-Authorization: Basic {0}\r\n",
                                                  Convert.ToBase64String(SshData.Ascii.GetBytes(string.Format("{0}:{1}", connectionInfo.ProxyUsername, connectionInfo.ProxyPassword))));
                SocketAbstraction.Send(socket, SshData.Ascii.GetBytes(authorization));
            }

            SocketAbstraction.Send(socket, SshData.Ascii.GetBytes("\r\n"));

            HttpStatusCode? statusCode = null;
            var contentLength = 0;

            while (true)
            {
                var response = SocketReadLine(socket, connectionInfo.Timeout);
                if (response == null)
                {
                    // server shut down socket
                    break;
                }

                if (statusCode == null)
                {
                    var statusMatch = httpResponseRe.Match(response);
                    if (statusMatch.Success)
                    {
                        var httpStatusCode = statusMatch.Result("${statusCode}");
                        statusCode = (HttpStatusCode)int.Parse(httpStatusCode);
                        if (statusCode != HttpStatusCode.OK)
                        {
                            var reasonPhrase = statusMatch.Result("${reasonPhrase}");
                            throw new ProxyException(string.Format("HTTP: Status code {0}, \"{1}\"", httpStatusCode,
                                reasonPhrase));
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
                        contentLength = int.Parse(headerMatch.Result("${fieldValue}"));
                    }
                    continue;
                }

                // check if we've reached the CRLF which separates request line and headers from the message body
                if (response.Length == 0)
                {
                    //  read response body if specified
                    if (contentLength > 0)
                    {
                        var contentBody = new byte[contentLength];
                        SocketRead(socket, contentBody, 0, contentLength);
                    }
                    break;
                }
            }

            if (statusCode == null)
            {
                throw new ProxyException("HTTP response does not contain status line.");
            }

            return socket;
        }

        /// <summary>
        /// Performs a blocking read on the socket until a line is read.
        /// </summary>
        /// <param name="socket">The <see cref="Socket"/> to read from.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> that represents the time to wait until a line is read.</param>
        /// <exception cref="SshOperationTimeoutException">The read has timed-out.</exception>
        /// <exception cref="SocketException">An error occurred when trying to access the socket.</exception>
        /// <returns>
        /// The line read from the socket, or <c>null</c> when the remote server has shutdown and all data has been received.
        /// </returns>
        private static string SocketReadLine(Socket socket, TimeSpan timeout)
        {
            var encoding = SshData.Ascii;
            var buffer = new List<byte>();
            var data = new byte[1];

            // read data one byte at a time to find end of line and leave any unhandled information in the buffer
            // to be processed by subsequent invocations
            do
            {
                var bytesRead = SocketAbstraction.Read(socket, data, 0, data.Length, timeout);
                if (bytesRead == 0)
                    // the remote server shut down the socket
                    break;

                var b = data[0];

                if (b == Session.LineFeed && buffer.Count > 1 && buffer[buffer.Count - 1] == Session.CarriageReturn)
                {
                    // Return line without CR
                    return encoding.GetString(buffer.ToArray(), 0, buffer.Count - 1);
                }

                buffer.Add(b);
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
