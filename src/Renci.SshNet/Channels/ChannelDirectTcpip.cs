using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Microsoft.Extensions.Logging;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Channels
{
    /// <summary>
    /// Implements "direct-tcpip" SSH channel.
    /// </summary>
    internal sealed class ChannelDirectTcpip : ClientChannel, IChannelDirectTcpip
    {
        private readonly Lock _socketLock = new Lock();
        private readonly ILogger _logger;
        private EventWaitHandle _channelOpen = new AutoResetEvent(initialState: false);
        private EventWaitHandle _channelData = new AutoResetEvent(initialState: false);
        private IForwardedPort _forwardedPort;
        private Socket _socket;

        /// <summary>
        /// Holds a value indicating whether data received on the channel may be written to
        /// <see cref="_socket"/>.
        /// </summary>
        /// <value>
        /// <see langword="false"/> until <see cref="Bind()"/> releases the relay; see
        /// <see cref="StartRelay"/> for why it is held back.
        /// </value>
        private bool _relaying;

        /// <summary>
        /// Holds the data received on the channel before the relay was released, or
        /// <see langword="null"/> when there is none.
        /// </summary>
        private Queue<byte[]> _pendingData;

        /// <summary>
        /// Holds the shutdown that was requested before the relay was released, or
        /// <see langword="null"/> when none was.
        /// </summary>
        private SocketShutdown? _pendingShutdown;

        /// <summary>
        /// Holds a value indicating whether <see cref="_socket"/> must be closed as soon as the relay
        /// is released.
        /// </summary>
        private bool _pendingClose;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelDirectTcpip"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="localChannelNumber">The local channel number.</param>
        /// <param name="localWindowSize">Size of the window.</param>
        /// <param name="localPacketSize">Size of the packet.</param>
        public ChannelDirectTcpip(ISession session, uint localChannelNumber, uint localWindowSize, uint localPacketSize)
            : base(session, localChannelNumber, localWindowSize, localPacketSize)
        {
            _logger = session.SessionLoggerFactory.CreateLogger<ChannelDirectTcpip>();
        }

        /// <summary>
        /// Gets the type of the channel.
        /// </summary>
        /// <value>
        /// The type of the channel.
        /// </value>
        public override ChannelTypes ChannelType
        {
            get { return ChannelTypes.DirectTcpip; }
        }

        /// <summary>
        /// Opens the channel for the specified remote host and port.
        /// </summary>
        /// <param name="remoteHost">The name of the remote host to forward to.</param>
        /// <param name="port">The port of the remote host to forward to.</param>
        /// <param name="forwardedPort">The forwarded port for which the channel is opened.</param>
        /// <param name="socket">The socket to receive requests from, and send responses from the remote host to.</param>
        /// <remarks>
        /// The channel takes ownership of <paramref name="socket"/> here, but does not yet write to it:
        /// the caller may still have its own reply to send first. Data received before
        /// <see cref="Bind()"/> is buffered rather than written. See <see cref="StartRelay"/>.
        /// </remarks>
        public void Open(string remoteHost, uint port, IForwardedPort forwardedPort, Socket socket)
        {
            if (IsOpen)
            {
                throw new SshException("Channel is already open.");
            }

            if (!IsConnected)
            {
                throw new SshException("Session is not connected.");
            }

            _socket = socket;
            _forwardedPort = forwardedPort;
            _forwardedPort.Closing += ForwardedPort_Closing;

            var ep = (IPEndPoint)socket.RemoteEndPoint;

            // Open channel
            SendMessage(new ChannelOpenMessage(LocalChannelNumber,
                                               LocalWindowSize,
                                               LocalPacketSize,
                                               new DirectTcpipChannelInfo(remoteHost, port, ep.Address.ToString(), (uint)ep.Port)));

            // Wait for channel to open
            WaitOnHandle(_channelOpen);
        }

        /// <summary>
        /// Occurs as the forwarded port is being stopped.
        /// </summary>
        private void ForwardedPort_Closing(object sender, EventArgs eventArgs)
        {
            // The port is going away, so this is an abort rather than an orderly close: act on the
            // socket right away even when the relay has not been released, discarding anything that
            // is still buffered. There is nobody left to relay it for, and the whole point of this
            // handler is to interrupt a blocking receive.
            lock (_socketLock)
            {
                // signal to the client that we will not send anything anymore; this should also interrupt the
                // blocking receive in Bind if the client sends FIN/ACK in time
                ShutdownSocketCore(SocketShutdown.Send);

                // if the FIN/ACK is not sent in time by the remote client, then interrupt the blocking receive
                // by closing the socket
                CloseSocketCore();
            }
        }

        /// <summary>
        /// Binds channel to remote host.
        /// </summary>
        public void Bind()
        {
            // Release the data that arrived while the forwarded port was still writing its own reply
            // to the client, and only then start pumping in the other direction.
            StartRelay();

            // Cannot bind if channel is not open
            if (!IsOpen)
            {
                return;
            }

            var socket = _socket;
            if (socket is null)
            {
                return;
            }

            var buffer = new byte[RemotePacketSize];

            SocketAbstraction.ReadContinuous(socket, buffer, 0, buffer.Length, SendData);

            // even though the client has disconnected, we still want to properly close the
            // channel
            //
            // we'll do this in in Close() - invoked through Dispose(bool) - that way we have
            // a single place from which we send an SSH_MSG_CHANNEL_EOF message and wait for
            // the SSH_MSG_CHANNEL_CLOSE message
        }

        /// <summary>
        /// Starts writing data received on the channel to the socket, first flushing whatever arrived
        /// while the relay was held back.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A forwarded port writes its own reply to the client between <see cref="Open"/> and
        /// <see cref="Bind()"/> - for <see cref="ForwardedPortDynamic"/> that is the SOCKS reply, which
        /// RFC 1928 s6 requires to precede any byte from the target. <see cref="OnData"/> runs on the
        /// session's message-receive thread, so without this gate the two would write to the same
        /// socket from two threads with no ordering between them, and a target that speaks first - SSH,
        /// SMTP, IMAP, POP3, FTP, MySQL, PostgreSQL, Redis - would land its greeting where the reply
        /// belonged.
        /// </para>
        /// <para>
        /// A shutdown or close requested while the relay was closed is applied here too, after the
        /// buffered data, so that a target which speaks and then immediately hangs up still gets its
        /// bytes to the client in the right order.
        /// </para>
        /// </remarks>
        private void StartRelay()
        {
            lock (_socketLock)
            {
                if (_relaying)
                {
                    return;
                }

                _relaying = true;

                var pending = _pendingData;
                _pendingData = null;

                if (pending is not null)
                {
                    while (pending.Count > 0)
                    {
                        var data = pending.Dequeue();
                        SendToSocket(data, 0, data.Length);
                    }
                }

                if (_pendingShutdown is SocketShutdown pendingShutdown)
                {
                    _pendingShutdown = null;
                    ShutdownSocketCore(pendingShutdown);
                }

                if (_pendingClose)
                {
                    _pendingClose = false;
                    CloseSocketCore();
                }
            }
        }

        /// <summary>
        /// Writes data to the socket, if there still is one and it is connected.
        /// </summary>
        /// <param name="data">An array of <see cref="byte"/> containing the data to write.</param>
        /// <param name="offset">The zero-based offset in <paramref name="data"/> at which to begin taking data from.</param>
        /// <param name="count">The number of bytes of <paramref name="data"/> to write.</param>
        /// <remarks>
        /// The caller must hold <see cref="_socketLock"/>.
        /// </remarks>
        private void SendToSocket(byte[] data, int offset, int count)
        {
            var socket = _socket;

            if (socket.IsConnected())
            {
                SocketAbstraction.Send(socket, data, offset, count);
            }
        }

        /// <summary>
        /// Closes the socket, hereby interrupting the blocking receive in <see cref="Bind()"/>.
        /// </summary>
        private void CloseSocket()
        {
            lock (_socketLock)
            {
                if (!_relaying)
                {
                    // Disposing the socket now would strand both the reply the forwarded port is about
                    // to write and anything OnData has buffered. StartRelay applies this afterwards.
                    _pendingClose = true;
                    return;
                }

                CloseSocketCore();
            }
        }

        /// <summary>
        /// Disposes the socket.
        /// </summary>
        /// <remarks>
        /// The caller must hold <see cref="_socketLock"/>.
        /// </remarks>
        private void CloseSocketCore()
        {
            // closing a socket actually disposes the socket, so we can safely dereference
            // the field to avoid entering the lock again later
            _socket?.Dispose();
            _socket = null;

            // there is no longer anywhere to write buffered data to
            _pendingData = null;
        }

        /// <summary>
        /// Shuts down the socket.
        /// </summary>
        /// <param name="how">One of the <see cref="SocketShutdown"/> values that specifies the operation that will no longer be allowed.</param>
        private void ShutdownSocket(SocketShutdown how)
        {
            lock (_socketLock)
            {
                if (!_relaying)
                {
                    // Shutting the send side down now would break the reply the forwarded port is
                    // about to write, and would discard whatever OnData has buffered. StartRelay
                    // applies this after the buffer has been drained.
                    _pendingShutdown = _pendingShutdown is null || _pendingShutdown == how
                        ? how
                        : SocketShutdown.Both;
                    return;
                }

                ShutdownSocketCore(how);
            }
        }

        /// <summary>
        /// Shuts down the socket, without regard for the relay.
        /// </summary>
        /// <param name="how">One of the <see cref="SocketShutdown"/> values that specifies the operation that will no longer be allowed.</param>
        /// <remarks>
        /// The caller must hold <see cref="_socketLock"/>.
        /// </remarks>
        private void ShutdownSocketCore(SocketShutdown how)
        {
            var socket = _socket;

            if (!socket.IsConnected())
            {
                return;
            }

            try
            {
                socket.Shutdown(how);
            }
            catch (SocketException ex)
            {
                _logger.LogInformation(ex, "Failure shutting down socket");
            }
        }

        /// <summary>
        /// Closes the channel, waiting for the SSH_MSG_CHANNEL_CLOSE message to be received from the server.
        /// </summary>
        protected override void Close()
        {
            var forwardedPort = _forwardedPort;
            if (forwardedPort != null)
            {
                forwardedPort.Closing -= ForwardedPort_Closing;
                _forwardedPort = null;
            }

            // signal to the client that we will not send anything anymore; this will also interrupt the
            // blocking receive in Bind if the client sends FIN/ACK in time
            //
            // if the FIN/ACK is not sent in time, the socket will be closed after the channel is closed
            ShutdownSocket(SocketShutdown.Send);

            // close the SSH channel
            base.Close();

            // close the socket
            CloseSocket();
        }

        /// <summary>
        /// Called when channel data is received.
        /// </summary>
        /// <param name="data">The data.</param>
        protected override void OnData(ArraySegment<byte> data)
        {
            base.OnData(data);

            lock (_socketLock)
            {
                if (_socket is null)
                {
                    return;
                }

                if (!_relaying)
                {
                    // We are on the session's message-receive thread and the forwarded port has not
                    // finished writing its own reply to the client yet. Hold on to the data instead of
                    // racing that thread for the socket; StartRelay writes it out in order. The array
                    // belongs to the message being processed, so it has to be copied.
                    var buffered = new byte[data.Count];
                    Buffer.BlockCopy(data.Array, data.Offset, buffered, 0, data.Count);

                    _pendingData ??= new Queue<byte[]>();
                    _pendingData.Enqueue(buffered);
                    return;
                }

                SendToSocket(data.Array, data.Offset, data.Count);
            }
        }

        /// <summary>
        /// Called when channel is opened by the server.
        /// </summary>
        /// <param name="remoteChannelNumber">The remote channel number.</param>
        /// <param name="initialWindowSize">Initial size of the window.</param>
        /// <param name="maximumPacketSize">Maximum size of the packet.</param>
        protected override void OnOpenConfirmation(uint remoteChannelNumber, uint initialWindowSize, uint maximumPacketSize)
        {
            base.OnOpenConfirmation(remoteChannelNumber, initialWindowSize, maximumPacketSize);

            _ = _channelOpen.Set();
        }

        protected override void OnOpenFailure(uint reasonCode, string description, string language)
        {
            base.OnOpenFailure(reasonCode, description, language);

            _ = _channelOpen.Set();
        }

        /// <summary>
        /// Called when channel has no more data to receive.
        /// </summary>
        protected override void OnEof()
        {
            base.OnEof();

            // the channel will send no more data, and hence it does not make sense to receive
            // any more data from the client to send to the remote party (and we surely won't
            // send anything anymore)
            //
            // this will also interrupt the blocking receive in Bind()
            ShutdownSocket(SocketShutdown.Send);
        }

        /// <summary>
        /// Called whenever an unhandled <see cref="Exception"/> occurs in <see cref="Session"/> causing
        /// the message loop to be interrupted, or when an exception occurred processing a channel message.
        /// </summary>
        protected override void OnErrorOccurred(Exception exp)
        {
            base.OnErrorOccurred(exp);

            // signal to the client that we will not send anything anymore; this will also interrupt the
            // blocking receive in Bind if the client sends FIN/ACK in time
            //
            // if the FIN/ACK is not sent in time, the socket will be closed in Close(bool)
            ShutdownSocket(SocketShutdown.Send);
        }

        /// <summary>
        /// Called when the server wants to terminate the connection immediately.
        /// </summary>
        /// <remarks>
        /// The sender MUST NOT send or receive any data after this message, and
        /// the recipient MUST NOT accept any data after receiving this message.
        /// </remarks>
        protected override void OnDisconnected()
        {
            base.OnDisconnected();

            // the channel will accept or send no more data, and hence it does not make sense
            // to accept any more data from the client (and we surely won't send anything
            // anymore)
            //
            // so lets signal to the client that we will not send or receive anything anymore
            // this will also interrupt the blocking receive in Bind()
            ShutdownSocket(SocketShutdown.Both);
        }

        protected override void Dispose(bool disposing)
        {
            // make sure we've unsubscribed from all session events and closed the channel
            // before we starting disposing
            base.Dispose(disposing);

            if (disposing)
            {
                lock (_socketLock)
                {
                    CloseSocketCore();
                }

                var channelOpen = _channelOpen;
                if (channelOpen != null)
                {
                    _channelOpen = null;
                    channelOpen.Dispose();
                }

                var channelData = _channelData;
                if (channelData != null)
                {
                    _channelData = null;
                    channelData.Dispose();
                }
            }
        }
    }
}
