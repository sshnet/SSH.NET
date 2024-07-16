﻿#nullable enable
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Serves as base class for client implementations, provides common client functionality.
    /// </summary>
    public interface IBaseClient : IDisposable
    {
        /// <summary>
        /// Gets the connection info.
        /// </summary>
        /// <value>
        /// The connection info.
        /// </value>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        ConnectionInfo ConnectionInfo { get; }

        /// <summary>
        /// Gets a value indicating whether this client is connected to the server.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this client is connected; otherwise, <see langword="false"/>.
        /// </value>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        bool IsConnected { get; }

        /// <summary>
        /// Gets or sets the keep-alive interval.
        /// </summary>
        /// <value>
        /// The keep-alive interval. Specify negative one (-1) milliseconds to disable the
        /// keep-alive. This is the default value.
        /// </value>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        TimeSpan KeepAliveInterval { get; set; }

        /// <summary>
        /// Occurs when an error occurred.
        /// </summary>
        event EventHandler<ExceptionEventArgs> ErrorOccurred;

        /// <summary>
        /// Occurs when host key received.
        /// </summary>
        event EventHandler<HostKeyEventArgs> HostKeyReceived;

        /// <summary>
        /// Connects client to the server.
        /// </summary>
        /// <exception cref="InvalidOperationException">The client is already connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <exception cref="SocketException">Socket connection to the SSH server or proxy server could not be established, or an error occurred while resolving the hostname.</exception>
        /// <exception cref="SshConnectionException">SSH session could not be established.</exception>
        /// <exception cref="SshAuthenticationException">Authentication of SSH session failed.</exception>
        /// <exception cref="ProxyException">Failed to establish proxy connection.</exception>
        void Connect();

        /// <summary>
        /// Asynchronously connects client to the server.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous connect operation.
        /// </returns>
        /// <exception cref="InvalidOperationException">The client is already connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <exception cref="SocketException">Socket connection to the SSH server or proxy server could not be established, or an error occurred while resolving the hostname.</exception>
        /// <exception cref="SshConnectionException">SSH session could not be established.</exception>
        /// <exception cref="SshAuthenticationException">Authentication of SSH session failed.</exception>
        /// <exception cref="ProxyException">Failed to establish proxy connection.</exception>
        Task ConnectAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Disconnects client from the server.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        void Disconnect();

        /// <summary>
        /// Sends a keep-alive message to the server.
        /// </summary>
        /// <remarks>
        /// Use <see cref="KeepAliveInterval"/> to configure the client to send a keep-alive at regular
        /// intervals.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        void SendKeepAlive();
    }
}
