using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Compression;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Security;
using System.Globalization;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality to connect and interact with SSH server.
    /// </summary>
    public partial class Session : IDisposable
    {
        /// <summary>
        /// Specifies maximum packet size defined by the protocol.
        /// </summary>
        private const int MaximumSshPacketSize = LocalChannelDataPacketSize + 3000;

        /// <summary>
        /// Holds the initial local window size for the channels.
        /// </summary>
        /// <value>
        /// 2 MB.
        /// </value>
        private const int InitialLocalWindowSize = LocalChannelDataPacketSize * 32;

        /// <summary>
        /// Holds the maximum size of channel data packets that we receive.
        /// </summary>
        /// <value>
        /// 64 KB.
        /// </value>
        private const int LocalChannelDataPacketSize = 1024*64;

        private static readonly RNGCryptoServiceProvider Randomizer = new RNGCryptoServiceProvider();

#if SILVERLIGHT
        private static readonly Regex ServerVersionRe = new Regex("^SSH-(?<protoversion>[^-]+)-(?<softwareversion>.+)( SP.+)?$");
#else
        private static readonly Regex ServerVersionRe = new Regex("^SSH-(?<protoversion>[^-]+)-(?<softwareversion>.+)( SP.+)?$", RegexOptions.Compiled);
#endif

        /// <summary>
        /// Controls how many authentication attempts can take place at the same time.
        /// </summary>
        /// <remarks>
        /// Some server may restrict number to prevent authentication attacks
        /// </remarks>
        private static readonly SemaphoreLight AuthenticationConnection = new SemaphoreLight(3);

        /// <summary>
        /// Holds metada about session messages
        /// </summary>
        private IEnumerable<MessageMetadata> _messagesMetadata;

        /// <summary>
        /// Holds connection socket.
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// Holds locker object for the socket
        /// </summary>
        private readonly object _socketLock = new object();

        /// <summary>
        /// Holds reference to task that listens for incoming messages
        /// </summary>
        private EventWaitHandle _messageListenerCompleted;

        /// <summary>
        /// Specifies outbound packet number
        /// </summary>
        private volatile UInt32 _outboundPacketSequence;

        /// <summary>
        /// Specifies incoming packet number
        /// </summary>
        private UInt32 _inboundPacketSequence;

        /// <summary>
        /// WaitHandle to signal that last service request was accepted
        /// </summary>
        private EventWaitHandle _serviceAccepted = new AutoResetEvent(false);

        /// <summary>
        /// WaitHandle to signal that exception was thrown by another thread.
        /// </summary>
        private EventWaitHandle _exceptionWaitHandle = new ManualResetEvent(false);

        /// <summary>
        /// WaitHandle to signal that key exchange was completed.
        /// </summary>
        private EventWaitHandle _keyExchangeCompletedWaitHandle = new ManualResetEvent(false);

        /// <summary>
        /// WaitHandle to signal that key exchange is in progress.
        /// </summary>
        private bool _keyExchangeInProgress;

        /// <summary>
        /// Exception that need to be thrown by waiting thread
        /// </summary>
        private Exception _exception;

        /// <summary>
        /// Specifies whether connection is authenticated
        /// </summary>
        private bool _isAuthenticated;

        /// <summary>
        /// Specifies whether user issued Disconnect command or not
        /// </summary>
        private bool _isDisconnecting;

        private KeyExchange _keyExchange;

        private HashAlgorithm _serverMac;

        private HashAlgorithm _clientMac;

        private Cipher _clientCipher;

        private Cipher _serverCipher;

        private Compressor _serverDecompression;

        private Compressor _clientCompression;

        private SemaphoreLight _sessionSemaphore;

        /// <summary>
        /// Gets the session semaphore that controls session channels.
        /// </summary>
        /// <value>The session semaphore.</value>
        public SemaphoreLight SessionSemaphore
        {
            get
            {
                if (this._sessionSemaphore == null)
                {
                    lock (this)
                    {
                        if (this._sessionSemaphore == null)
                        {
                            this._sessionSemaphore = new SemaphoreLight(this.ConnectionInfo.MaxSessions);
                        }
                    }
                }

                return this._sessionSemaphore;
            }
        }

        private bool _isDisconnectMessageSent;

        private uint _nextChannelNumber;
        /// <summary>
        /// Gets the next channel number.
        /// </summary>
        /// <value>The next channel number.</value>
        internal uint NextChannelNumber
        {
            get
            {
                uint result;

                lock (this)
                {
                    result = this._nextChannelNumber++;
                }

                return result;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the session is connected.
        /// </summary>
        /// <value>
        /// <c>true</c> if the session is connected; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This methods returns true in all but the following cases:
        /// <list type="bullet">
        ///     <item>
        ///         <description>The SSH_MSG_DISCONNECT message - which is used to disconnect from the server - has been sent.</description>
        ///     </item>
        ///     <item>
        ///         <description>The client has not been authenticated successfully.</description>
        ///     </item>
        ///     <item>
        ///         <description>The listener thread - which is used to receive messages from the server - has stopped.</description>
        ///     </item>
        ///     <item>
        ///         <description>The socket used to communicate with the server is no longer connected.</description>
        ///     </item>
        /// </list>
        /// </remarks>
        public bool IsConnected
        {
            get
            {
                if (_isDisconnectMessageSent || !this._isAuthenticated)
                    return false;
                if (_messageListenerCompleted == null || _messageListenerCompleted.WaitOne(0))
                    return false;

                var isSocketConnected = false;
                IsSocketConnected(ref isSocketConnected);
                return isSocketConnected;
            }
        }

        /// <summary>
        /// Gets the session id.
        /// </summary>
        /// <value>
        /// The session id, or <c>null</c> if the client has not been authenticated.
        /// </value>
        public byte[] SessionId { get; private set; }

        private Message _clientInitMessage;
        /// <summary>
        /// Gets the client init message.
        /// </summary>
        /// <value>The client init message.</value>
        public Message ClientInitMessage
        {
            get
            {
                if (this._clientInitMessage == null)
                {
                    this._clientInitMessage = new KeyExchangeInitMessage
                    {
                        KeyExchangeAlgorithms = this.ConnectionInfo.KeyExchangeAlgorithms.Keys.ToArray(),
                        ServerHostKeyAlgorithms = this.ConnectionInfo.HostKeyAlgorithms.Keys.ToArray(),
                        EncryptionAlgorithmsClientToServer = this.ConnectionInfo.Encryptions.Keys.ToArray(),
                        EncryptionAlgorithmsServerToClient = this.ConnectionInfo.Encryptions.Keys.ToArray(),
                        MacAlgorithmsClientToServer = this.ConnectionInfo.HmacAlgorithms.Keys.ToArray(),
                        MacAlgorithmsServerToClient = this.ConnectionInfo.HmacAlgorithms.Keys.ToArray(),
                        CompressionAlgorithmsClientToServer = this.ConnectionInfo.CompressionAlgorithms.Keys.ToArray(),
                        CompressionAlgorithmsServerToClient = this.ConnectionInfo.CompressionAlgorithms.Keys.ToArray(),
                        LanguagesClientToServer = new[] {string.Empty},
                        LanguagesServerToClient = new[] {string.Empty},
                        FirstKexPacketFollows = false,
                        Reserved = 0
                    };
                }
                return this._clientInitMessage;
            }
        }

        /// <summary>
        /// Gets or sets the server version string.
        /// </summary>
        /// <value>The server version.</value>
        public string ServerVersion { get; private set; }

        /// <summary>
        /// Gets or sets the client version string.
        /// </summary>
        /// <value>The client version.</value>
        public string ClientVersion { get; private set; }

        /// <summary>
        /// Gets or sets the connection info.
        /// </summary>
        /// <value>The connection info.</value>
        public ConnectionInfo ConnectionInfo { get; private set; }

        /// <summary>
        /// Occurs when an error occurred.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ErrorOccured;

        /// <summary>
        /// Occurs when session has been disconnected form the server.
        /// </summary>
        public event EventHandler<EventArgs> Disconnected;

        /// <summary>
        /// Occurs when host key received.
        /// </summary>
        public event EventHandler<HostKeyEventArgs> HostKeyReceived;

        #region Message events

        /// <summary>
        /// Occurs when <see cref="DisconnectMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<DisconnectMessage>> DisconnectReceived;

        /// <summary>
        /// Occurs when <see cref="IgnoreMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<IgnoreMessage>> IgnoreReceived;

        /// <summary>
        /// Occurs when <see cref="UnimplementedMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<UnimplementedMessage>> UnimplementedReceived;

        /// <summary>
        /// Occurs when <see cref="DebugMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<DebugMessage>> DebugReceived;

        /// <summary>
        /// Occurs when <see cref="ServiceRequestMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<ServiceRequestMessage>> ServiceRequestReceived;

        /// <summary>
        /// Occurs when <see cref="ServiceAcceptMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<ServiceAcceptMessage>> ServiceAcceptReceived;

        /// <summary>
        /// Occurs when <see cref="KeyExchangeInitMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<KeyExchangeInitMessage>> KeyExchangeInitReceived;

        /// <summary>
        /// Occurs when <see cref="NewKeysMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<NewKeysMessage>> NewKeysReceived;

        /// <summary>
        /// Occurs when <see cref="RequestMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<RequestMessage>> UserAuthenticationRequestReceived;

        /// <summary>
        /// Occurs when <see cref="FailureMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<FailureMessage>> UserAuthenticationFailureReceived;

        /// <summary>
        /// Occurs when <see cref="SuccessMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<SuccessMessage>> UserAuthenticationSuccessReceived;

        /// <summary>
        /// Occurs when <see cref="BannerMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<BannerMessage>> UserAuthenticationBannerReceived;

        /// <summary>
        /// Occurs when <see cref="GlobalRequestMessage"/> message received
        /// </summary>        
        internal event EventHandler<MessageEventArgs<GlobalRequestMessage>> GlobalRequestReceived;

        /// <summary>
        /// Occurs when <see cref="RequestSuccessMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<RequestSuccessMessage>> RequestSuccessReceived;

        /// <summary>
        /// Occurs when <see cref="RequestFailureMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<RequestFailureMessage>> RequestFailureReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelOpenMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<ChannelOpenMessage>> ChannelOpenReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelOpenConfirmationMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<ChannelOpenConfirmationMessage>> ChannelOpenConfirmationReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelOpenFailureMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<ChannelOpenFailureMessage>> ChannelOpenFailureReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelWindowAdjustMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<ChannelWindowAdjustMessage>> ChannelWindowAdjustReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelDataMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<ChannelDataMessage>> ChannelDataReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelExtendedDataMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<ChannelExtendedDataMessage>> ChannelExtendedDataReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelEofMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<ChannelEofMessage>> ChannelEofReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelCloseMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<ChannelCloseMessage>> ChannelCloseReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelRequestMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<ChannelRequestMessage>> ChannelRequestReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelSuccessMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<ChannelSuccessMessage>> ChannelSuccessReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelFailureMessage"/> message received
        /// </summary>
        internal event EventHandler<MessageEventArgs<ChannelFailureMessage>> ChannelFailureReceived;

        /// <summary>
        /// Occurs when message received and is not handled by any of the event handlers
        /// </summary>
        internal event EventHandler<MessageEventArgs<Message>> MessageReceived;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Session"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <c>null</c>.</exception>
        internal Session(ConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
                throw new ArgumentNullException("connectionInfo");

            this.ConnectionInfo = connectionInfo;
            //this.ClientVersion = string.Format(CultureInfo.CurrentCulture, "SSH-2.0-Renci.SshNet.SshClient.{0}", this.GetType().Assembly.GetName().Version);
            this.ClientVersion = string.Format(CultureInfo.CurrentCulture, "SSH-2.0-Renci.SshNet.SshClient.0.0.1");
        }

        /// <summary>
        /// Connects to the server.
        /// </summary>
        public void Connect()
        {
            if (this.IsConnected)
                return;

            try
            {
                AuthenticationConnection.Wait();

                if (this.IsConnected)
                    return;

                lock (this)
                {
                    //  If connected don't connect again
                    if (this.IsConnected)
                        return;

                    // reset connection specific information
                    Reset();

                    //  Build list of available messages while connecting
                    this._messagesMetadata = GetMessagesMetadata();

                    switch (this.ConnectionInfo.ProxyType)
                    {
                        case ProxyTypes.None:
                            this.SocketConnect(this.ConnectionInfo.Host, this.ConnectionInfo.Port);
                            break;
                        case ProxyTypes.Socks4:
                            this.SocketConnect(this.ConnectionInfo.ProxyHost, this.ConnectionInfo.ProxyPort);
                            this.ConnectSocks4();
                            break;
                        case ProxyTypes.Socks5:
                            this.SocketConnect(this.ConnectionInfo.ProxyHost, this.ConnectionInfo.ProxyPort);
                            this.ConnectSocks5();
                            break;
                        case ProxyTypes.Http:
                            this.SocketConnect(this.ConnectionInfo.ProxyHost, this.ConnectionInfo.ProxyPort);
                            this.ConnectHttp();
                            break;
                    }

                    Match versionMatch;

                    //  Get server version from the server,
                    //  ignore text lines which are sent before if any
                    while (true)
                    {
                        string serverVersion = string.Empty;
                        this.SocketReadLine(ref serverVersion);

                        this.ServerVersion = serverVersion;
                        if (string.IsNullOrEmpty(this.ServerVersion))
                        {
                            throw new InvalidOperationException("Server string is null or empty.");
                        }

                        versionMatch = ServerVersionRe.Match(this.ServerVersion);

                        if (versionMatch.Success)
                        {
                            break;
                        }
                    }

                    //  Set connection versions
                    this.ConnectionInfo.ServerVersion = this.ServerVersion;
                    this.ConnectionInfo.ClientVersion = this.ClientVersion;

                    //  Get server SSH version
                    var version = versionMatch.Result("${protoversion}");

                    var softwareName = versionMatch.Result("${softwareversion}");

                    this.Log(string.Format("Server version '{0}' on '{1}'.", version, softwareName));

                    if (!(version.Equals("2.0") || version.Equals("1.99")))
                    {
                        throw new SshConnectionException(string.Format(CultureInfo.CurrentCulture, "Server version '{0}' is not supported.", version), DisconnectReason.ProtocolVersionNotSupported);
                    }

                    this.SocketWrite(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0}\x0D\x0A", this.ClientVersion)));

                    //  Register Transport response messages
                    this.RegisterMessage("SSH_MSG_DISCONNECT");
                    this.RegisterMessage("SSH_MSG_IGNORE");
                    this.RegisterMessage("SSH_MSG_UNIMPLEMENTED");
                    this.RegisterMessage("SSH_MSG_DEBUG");
                    this.RegisterMessage("SSH_MSG_SERVICE_ACCEPT");
                    this.RegisterMessage("SSH_MSG_KEXINIT");
                    this.RegisterMessage("SSH_MSG_NEWKEYS");

                    //  Some server implementations might sent this message first, prior establishing encryption algorithm
                    this.RegisterMessage("SSH_MSG_USERAUTH_BANNER");

                    //  Start incoming request listener
                    this._messageListenerCompleted = new ManualResetEvent(false);

                    this.ExecuteThread(() =>
                    {
                        try
                        {
                            this.MessageListener();
                        }
                        finally
                        {
                            this._messageListenerCompleted.Set();
                        }
                    });

                    //  Wait for key exchange to be completed
                    this.WaitOnHandle(this._keyExchangeCompletedWaitHandle);

                    //  If sessionId is not set then its not connected
                    if (this.SessionId == null)
                    {
                        this.Disconnect();
                        return;
                    }

                    //  Request user authorization service
                    this.SendMessage(new ServiceRequestMessage(ServiceName.UserAuthentication));

                    //  Wait for service to be accepted
                    this.WaitOnHandle(this._serviceAccepted);

                    if (string.IsNullOrEmpty(this.ConnectionInfo.Username))
                    {
                        throw new SshException("Username is not specified.");
                    }

                    this.ConnectionInfo.Authenticate(this);
                    this._isAuthenticated = true;

                    //  Register Connection messages
                    this.RegisterMessage("SSH_MSG_GLOBAL_REQUEST");
                    this.RegisterMessage("SSH_MSG_REQUEST_SUCCESS");
                    this.RegisterMessage("SSH_MSG_REQUEST_FAILURE");
                    this.RegisterMessage("SSH_MSG_CHANNEL_OPEN_CONFIRMATION");
                    this.RegisterMessage("SSH_MSG_CHANNEL_OPEN_FAILURE");
                    this.RegisterMessage("SSH_MSG_CHANNEL_WINDOW_ADJUST");
                    this.RegisterMessage("SSH_MSG_CHANNEL_EXTENDED_DATA");
                    this.RegisterMessage("SSH_MSG_CHANNEL_REQUEST");
                    this.RegisterMessage("SSH_MSG_CHANNEL_SUCCESS");
                    this.RegisterMessage("SSH_MSG_CHANNEL_FAILURE");
                    this.RegisterMessage("SSH_MSG_CHANNEL_DATA");
                    this.RegisterMessage("SSH_MSG_CHANNEL_EOF");
                    this.RegisterMessage("SSH_MSG_CHANNEL_CLOSE");

                    Monitor.Pulse(this);
                }
            }
            finally
            {
                AuthenticationConnection.Release();
            }
        }

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        /// <remarks>
        /// This sends a <b>SSH_MSG_DISCONNECT</b> message to the server, waits for the
        /// server to close the socket on its end and subsequently closes the client socket.
        /// </remarks>
        public void Disconnect()
        {
            Disconnect(DisconnectReason.ByApplication, "Connection terminated by the client.");
        }

        private void Disconnect(DisconnectReason reason, string message)
        {
            this._isDisconnecting = true;

            //  send disconnect message to the server if the connection is still open
            // and the disconnect message has not yet been sent
            //
            // note that this should also cause the listener thread to be stopped as
            // the server should respond by closing the socket
            SendDisconnect(reason, message);

            // disconnect socket, and dispose it
            SocketDisconnectAndDispose();

            if (_messageListenerCompleted != null)
            {
                // at this point, we are sure that the listener thread will stop
                // as we've disconnected the socket
                _messageListenerCompleted.WaitOne();
                _messageListenerCompleted.Dispose();
                _messageListenerCompleted = null;
            }
        }

        internal T CreateClientChannel<T>() where T : ClientChannel, new()
        {
            var channel = new T();
            lock (this)
            {
                channel.Initialize(this, InitialLocalWindowSize, LocalChannelDataPacketSize);
            }
            return channel;
        }

        internal T CreateServerChannel<T>(uint remoteChannelNumber, uint remoteWindowSize, uint remoteChannelDataPacketSize) where T : ServerChannel, new()
        {
            var channel = new T();
            lock (this)
            {
                channel.Initialize(this, InitialLocalWindowSize, LocalChannelDataPacketSize, remoteChannelNumber, remoteWindowSize,
                    remoteChannelDataPacketSize);
            }
            return channel;
        }

        /// <summary>
        /// Sends "keep alive" message to keep connection alive.
        /// </summary>
        internal void SendKeepAlive()
        {
            this.SendMessage(new IgnoreMessage());
        }

        /// <summary>
        /// Waits for the specified handle or the exception handle for the receive thread
        /// to signal within the connection timeout.
        /// </summary>
        /// <param name="waitHandle">The wait handle.</param>
        /// <exception cref="SshConnectionException">A received package was invalid or failed the message integrity check.</exception>
        /// <exception cref="SshOperationTimeoutException">None of the handles are signaled in time and the session is not disconnecting.</exception>
        /// <exception cref="SocketException">A socket error was signaled while receiving messages from the server.</exception>
        /// <remarks>
        /// When neither handles are signaled in time and the session is not closing, then the
        /// session is disconnected.
        /// 
        /// </remarks>
        internal void WaitOnHandle(WaitHandle waitHandle)
        {
            var waitHandles = new[]
                {
                    this._exceptionWaitHandle,
                    this._messageListenerCompleted,
                    waitHandle
                };

            switch (WaitHandle.WaitAny(waitHandles, ConnectionInfo.Timeout))
            {
                case 0:
                    throw this._exception;
                case 1:
                    // when the session is NOT disconnecting, the listener should actually
                    // never complete without setting the exception wait handle and should
                    // end up in case 0... 
                    //
                    // when the session is disconnecting, the completion of the listener
                    // should not be considered an error (quite the oppposite actually)
                    if (!_isDisconnecting)
                    {
                        throw new SshConnectionException("Client not connected.");
                    }
                    break;
                case WaitHandle.WaitTimeout:
                    // when the session is NOT disconnecting, then we want to disconnect
                    // the session altogether in case of a timeout, and throw a
                    // SshOperationTimeoutException
                    //
                    // when the session is disconnecting, a timeout is likely when no
                    // network connectivity is available; depending on the configured
                    // timeout either the WaitAny times out first or a SocketException
                    // detailing a timeout thrown hereby completing the listener thread
                    // (which makes us end up in case 1). Either way, we do not want to
                    // disconnect while we're already disconnecting and we do not want
                    // to report an exception to the client when we're disconnecting
                    // anyway
                    if (!_isDisconnecting)
                    {
                        this.Disconnect(DisconnectReason.ByApplication, "Operation timeout");
                        throw new SshOperationTimeoutException("Session operation has timed out");
                    }
                    break;
            }
        }

        /// <summary>
        /// Sends packet message to the server.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendMessage(Message message)
        {
            if (this._socket == null || !this._socket.CanWrite())
                throw new SshConnectionException("Client not connected.");

            if (this._keyExchangeInProgress && !(message is IKeyExchangedAllowed))
            {
                //  Wait for key exchange to be completed
                this.WaitOnHandle(this._keyExchangeCompletedWaitHandle);
            }

            this.Log(string.Format("SendMessage to server '{0}': '{1}'.", message.GetType().Name, message));

            //  Messages can be sent by different thread so we need to synchronize it
            var paddingMultiplier = this._clientCipher == null ? (byte)8 : Math.Max((byte)8, this._serverCipher.MinimumSize);    //    Should be recalculate base on cipher min length if cipher specified

            var messageData = message.GetBytes();

            if (this._clientCompression != null)
            {
                messageData = this._clientCompression.Compress(messageData);
            }

            var packetLength = messageData.Length + 4 + 1; //  add length bytes and padding byte
            var paddingLength = (byte)((-packetLength) & (paddingMultiplier - 1));
            if (paddingLength < paddingMultiplier)
            {
                paddingLength += paddingMultiplier;
            }

            //  Build Packet data
            var packetData = new byte[4 + 1 + messageData.Length + paddingLength];

            //  Add packet length
            ((uint)packetData.Length - 4).GetBytes().CopyTo(packetData, 0);

            //  Add packet padding length
            packetData[4] = paddingLength;

            //  Add packet payload
            messageData.CopyTo(packetData, 4 + 1);

            //  Add random padding
            var paddingBytes = new byte[paddingLength];
            Randomizer.GetBytes(paddingBytes);
            paddingBytes.CopyTo(packetData, 4 + 1 + messageData.Length);

            //  Lock handling of _outboundPacketSequence since it must be sent sequently to server
            lock (this._socketLock)
            {
                if (this._socket == null || !this._socket.Connected)
                    throw new SshConnectionException("Client not connected.");

                //  Calculate packet hash
                var hashData = new byte[4 + packetData.Length];
                this._outboundPacketSequence.GetBytes().CopyTo(hashData, 0);
                packetData.CopyTo(hashData, 4);

                //  Encrypt packet data
                if (this._clientCipher != null)
                {
                    packetData = this._clientCipher.Encrypt(packetData);
                }

                if (packetData.Length > MaximumSshPacketSize)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Packet is too big. Maximum packet size is {0} bytes.", MaximumSshPacketSize));
                }

                if (this._clientMac == null)
                {
                    this.SocketWrite(packetData);
                }
                else
                {
                    var hash = this._clientMac.ComputeHash(hashData.ToArray());

                    var data = new byte[packetData.Length + this._clientMac.HashSize / 8];
                    packetData.CopyTo(data, 0);
                    hash.CopyTo(data, packetData.Length);

                    this.SocketWrite(data);
                }

                this._outboundPacketSequence++;

                Monitor.Pulse(this._socketLock);
            }
        }

        private static IEnumerable<MessageMetadata> GetMessagesMetadata()
        {
            return new []
                {
                    new MessageMetadata { Name = "SSH_MSG_NEWKEYS", Number = 21, Type = typeof(NewKeysMessage) },
                    new MessageMetadata { Name = "SSH_MSG_REQUEST_FAILURE", Number = 82, Type = typeof(RequestFailureMessage) },
                    new MessageMetadata { Name = "SSH_MSG_KEXINIT", Number = 20, Type = typeof(KeyExchangeInitMessage) },
                    new MessageMetadata { Name = "SSH_MSG_CHANNEL_OPEN_FAILURE", Number = 92, Type = typeof(ChannelOpenFailureMessage) },
                    new MessageMetadata { Name = "SSH_MSG_CHANNEL_FAILURE", Number = 100, Type = typeof(ChannelFailureMessage) },
                    new MessageMetadata { Name = "SSH_MSG_CHANNEL_EXTENDED_DATA", Number = 95, Type = typeof(ChannelExtendedDataMessage) },
                    new MessageMetadata { Name = "SSH_MSG_CHANNEL_DATA", Number = 94, Type = typeof(ChannelDataMessage) },
                    new MessageMetadata { Name = "SSH_MSG_USERAUTH_REQUEST", Number = 50, Type = typeof(RequestMessage) },
                    new MessageMetadata { Name = "SSH_MSG_CHANNEL_REQUEST", Number = 98, Type = typeof(ChannelRequestMessage) },
                    new MessageMetadata { Name = "SSH_MSG_USERAUTH_BANNER", Number = 53, Type = typeof(BannerMessage) },
                    new MessageMetadata { Name = "SSH_MSG_USERAUTH_INFO_RESPONSE", Number = 61, Type = typeof(InformationResponseMessage) },
                    new MessageMetadata { Name = "SSH_MSG_USERAUTH_FAILURE", Number = 51, Type = typeof(FailureMessage) },
                    new MessageMetadata { Name = "SSH_MSG_DEBUG", Number = 4, Type = typeof(DebugMessage), },
                    new MessageMetadata { Name = "SSH_MSG_KEXDH_INIT", Number = 30, Type = typeof(KeyExchangeDhInitMessage) },
                    new MessageMetadata { Name = "SSH_MSG_GLOBAL_REQUEST", Number = 80, Type = typeof(GlobalRequestMessage) },
                    new MessageMetadata { Name = "SSH_MSG_CHANNEL_OPEN", Number = 90, Type = typeof(ChannelOpenMessage) },
                    new MessageMetadata { Name = "SSH_MSG_CHANNEL_OPEN_CONFIRMATION", Number = 91, Type = typeof(ChannelOpenConfirmationMessage) },
                    new MessageMetadata { Name = "SSH_MSG_USERAUTH_INFO_REQUEST", Number = 60, Type = typeof(InformationRequestMessage) },
                    new MessageMetadata { Name = "SSH_MSG_UNIMPLEMENTED", Number = 3, Type = typeof(UnimplementedMessage) },
                    new MessageMetadata { Name = "SSH_MSG_REQUEST_SUCCESS", Number = 81, Type = typeof(RequestSuccessMessage) },
                    new MessageMetadata { Name = "SSH_MSG_CHANNEL_SUCCESS", Number = 99, Type = typeof(ChannelSuccessMessage) },
                    new MessageMetadata { Name = "SSH_MSG_USERAUTH_PASSWD_CHANGEREQ", Number = 60, Type = typeof(PasswordChangeRequiredMessage) },
                    new MessageMetadata { Name = "SSH_MSG_DISCONNECT", Number = 1, Type = typeof(DisconnectMessage) },
                    new MessageMetadata { Name = "SSH_MSG_SERVICE_REQUEST", Number = 5, Type = typeof(ServiceRequestMessage) },
                    new MessageMetadata { Name = "SSH_MSG_KEX_DH_GEX_REQUEST", Number = 34, Type = typeof(KeyExchangeDhGroupExchangeRequest) },
                    new MessageMetadata { Name = "SSH_MSG_KEX_DH_GEX_GROUP", Number = 31, Type = typeof(KeyExchangeDhGroupExchangeGroup) },
                    new MessageMetadata { Name = "SSH_MSG_USERAUTH_SUCCESS", Number = 52, Type = typeof(SuccessMessage) },
                    new MessageMetadata { Name = "SSH_MSG_USERAUTH_PK_OK", Number = 60, Type = typeof(PublicKeyMessage) },
                    new MessageMetadata { Name = "SSH_MSG_IGNORE", Number = 2, Type = typeof(IgnoreMessage) },
                    new MessageMetadata { Name = "SSH_MSG_CHANNEL_WINDOW_ADJUST", Number = 93, Type = typeof(ChannelWindowAdjustMessage) },
                    new MessageMetadata { Name = "SSH_MSG_CHANNEL_EOF", Number = 96, Type = typeof(ChannelEofMessage) },
                    new MessageMetadata { Name = "SSH_MSG_CHANNEL_CLOSE", Number = 97, Type = typeof(ChannelCloseMessage) },
                    new MessageMetadata { Name = "SSH_MSG_SERVICE_ACCEPT", Number = 6, Type = typeof(ServiceAcceptMessage) },
                    new MessageMetadata { Name = "SSH_MSG_KEXDH_REPLY", Number = 31, Type = typeof(KeyExchangeDhReplyMessage) },
                    new MessageMetadata { Name = "SSH_MSG_KEX_DH_GEX_INIT", Number = 32, Type = typeof(KeyExchangeDhGroupExchangeInit) },
                    new MessageMetadata { Name = "SSH_MSG_KEX_DH_GEX_REPLY", Number = 33, Type = typeof(KeyExchangeDhGroupExchangeReply) }
                };
        }

        /// <summary>
        /// Receives the message from the server.
        /// </summary>
        /// <returns>Incoming SSH message.</returns>
        /// <exception cref="SshConnectionException"></exception>
        private Message ReceiveMessage()
        {
            //  No lock needed since all messages read by only one thread
            var blockSize = this._serverCipher == null ? (byte)8 : Math.Max((byte)8, this._serverCipher.MinimumSize);

            //  Read packet length first
            var firstBlock = this.Read(blockSize);

            if (this._serverCipher != null)
            {
                firstBlock = this._serverCipher.Decrypt(firstBlock);
            }

            var packetLength = (uint)(firstBlock[0] << 24 | firstBlock[1] << 16 | firstBlock[2] << 8 | firstBlock[3]);

            //  Test packet minimum and maximum boundaries
            if (packetLength < Math.Max((byte)16, blockSize) - 4 || packetLength > MaximumSshPacketSize - 4)
                throw new SshConnectionException(string.Format(CultureInfo.CurrentCulture, "Bad packet length {0}", packetLength), DisconnectReason.ProtocolError);

            //  Read rest of the packet data
            var bytesToRead = (int)(packetLength - (blockSize - 4));

            var data = new byte[bytesToRead + blockSize];

            firstBlock.CopyTo(data, 0);

            byte[] serverHash = null;
            if (this._serverMac != null)
            {
                serverHash = new byte[this._serverMac.HashSize / 8];
                bytesToRead += serverHash.Length;
            }

            if (bytesToRead > 0)
            {
                var nextBlocks = this.Read(bytesToRead);

                if (serverHash != null)
                {
                    Buffer.BlockCopy(nextBlocks, nextBlocks.Length - serverHash.Length, serverHash, 0, serverHash.Length);
                    nextBlocks = nextBlocks.Take(nextBlocks.Length - serverHash.Length).ToArray();
                }

                if (nextBlocks.Length > 0)
                {
                    if (this._serverCipher != null)
                    {
                        nextBlocks = this._serverCipher.Decrypt(nextBlocks);
                    }
                    nextBlocks.CopyTo(data, blockSize);
                }
            }

            var paddingLength = data[4];

            var messagePayload = new byte[packetLength - paddingLength - 1];
            Buffer.BlockCopy(data, 5, messagePayload, 0, messagePayload.Length);

            if (this._serverDecompression != null)
            {
                messagePayload = this._serverDecompression.Decompress(messagePayload);
            }

            //  Validate message against MAC
            if (this._serverMac != null)
            {
                var clientHashData = new byte[4 + data.Length];
                var lengthBytes = this._inboundPacketSequence.GetBytes();

                lengthBytes.CopyTo(clientHashData, 0);
                data.CopyTo(clientHashData, 4);

                //  Calculate packet hash
                var clientHash = this._serverMac.ComputeHash(clientHashData);

                if (!serverHash.SequenceEqual(clientHash))
                {
                    throw new SshConnectionException("MAC error", DisconnectReason.MacError);
                }
            }

            this._inboundPacketSequence++;

            return this.LoadMessage(messagePayload);
        }

        private void SendDisconnect(DisconnectReason reasonCode, string message)
        {
            // only send a disconnect message if it wasn't already sent, and we're
            // still connected
            if (this._isDisconnectMessageSent || !IsConnected)
                return;

            var disconnectMessage = new DisconnectMessage(reasonCode, message);

            this.SendMessage(disconnectMessage);

            this._isDisconnectMessageSent = true;
        }

        partial void HandleMessageCore(Message message);

        /// <summary>
        /// Handles the message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message.</param>
        private void HandleMessage<T>(T message) where T : Message
        {
            this.OnMessageReceived(message);
        }

        #region Handle transport messages

        private void HandleMessage(DisconnectMessage message)
        {
            this.OnDisconnectReceived(message);

            //  disconnect from the socket, and dispose it
            SocketDisconnectAndDispose();
        }

        private void HandleMessage(IgnoreMessage message)
        {
            this.OnIgnoreReceived(message);
        }

        private void HandleMessage(UnimplementedMessage message)
        {
            this.OnUnimplementedReceived(message);
        }

        private void HandleMessage(DebugMessage message)
        {
            this.OnDebugReceived(message);
        }

        private void HandleMessage(ServiceRequestMessage message)
        {
            this.OnServiceRequestReceived(message);
        }

        private void HandleMessage(ServiceAcceptMessage message)
        {
            //  TODO:   Refactor to avoid this method here
            this.OnServiceAcceptReceived(message);

            this._serviceAccepted.Set();
        }

        private void HandleMessage(KeyExchangeInitMessage message)
        {
            this.OnKeyExchangeInitReceived(message);
        }

        private void HandleMessage(NewKeysMessage message)
        {
            this.OnNewKeysReceived(message);
        }

        #endregion

        #region Handle User Authentication messages

        private void HandleMessage(RequestMessage message)
        {
            this.OnUserAuthenticationRequestReceived(message);
        }

        private void HandleMessage(FailureMessage message)
        {
            this.OnUserAuthenticationFailureReceived(message);
        }

        private void HandleMessage(SuccessMessage message)
        {
            this.OnUserAuthenticationSuccessReceived(message);
        }

        private void HandleMessage(BannerMessage message)
        {
            this.OnUserAuthenticationBannerReceived(message);
        }

        #endregion

        #region Handle connection messages

        private void HandleMessage(GlobalRequestMessage message)
        {
            this.OnGlobalRequestReceived(message);
        }

        private void HandleMessage(RequestSuccessMessage message)
        {
            this.OnRequestSuccessReceived(message);
        }

        private void HandleMessage(RequestFailureMessage message)
        {
            this.OnRequestFailureReceived(message);
        }

        private void HandleMessage(ChannelOpenMessage message)
        {
            this.OnChannelOpenReceived(message);
        }

        private void HandleMessage(ChannelOpenConfirmationMessage message)
        {
            this.OnChannelOpenConfirmationReceived(message);
        }

        private void HandleMessage(ChannelOpenFailureMessage message)
        {
            this.OnChannelOpenFailureReceived(message);
        }

        private void HandleMessage(ChannelWindowAdjustMessage message)
        {
            this.OnChannelWindowAdjustReceived(message);
        }

        private void HandleMessage(ChannelDataMessage message)
        {
            this.OnChannelDataReceived(message);
        }

        private void HandleMessage(ChannelExtendedDataMessage message)
        {
            this.OnChannelExtendedDataReceived(message);
        }

        private void HandleMessage(ChannelEofMessage message)
        {
            this.OnChannelEofReceived(message);
        }

        private void HandleMessage(ChannelCloseMessage message)
        {
            this.OnChannelCloseReceived(message);
        }

        private void HandleMessage(ChannelRequestMessage message)
        {
            this.OnChannelRequestReceived(message);
        }

        private void HandleMessage(ChannelSuccessMessage message)
        {
            this.OnChannelSuccessReceived(message);
        }

        private void HandleMessage(ChannelFailureMessage message)
        {
            this.OnChannelFailureReceived(message);
        }

        #endregion

        #region Handle received message events

        /// <summary>
        /// Called when <see cref="DisconnectMessage"/> received.
        /// </summary>
        /// <param name="message"><see cref="DisconnectMessage"/> message.</param>
        protected virtual void OnDisconnectReceived(DisconnectMessage message)
        {
            this.Log(string.Format("Disconnect received: {0} {1}", message.ReasonCode, message.Description));

            var disconnectReceived = DisconnectReceived;
            if (disconnectReceived != null)
                disconnectReceived(this, new MessageEventArgs<DisconnectMessage>(message));

            var disconnected = Disconnected;
            if (disconnected != null)
                disconnected(this, new EventArgs());
        }

        /// <summary>
        /// Called when <see cref="IgnoreMessage"/> received.
        /// </summary>
        /// <param name="message"><see cref="IgnoreMessage"/> message.</param>
        protected virtual void OnIgnoreReceived(IgnoreMessage message)
        {
            var handlers = IgnoreReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<IgnoreMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="UnimplementedMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="UnimplementedMessage"/> message.</param>
        protected virtual void OnUnimplementedReceived(UnimplementedMessage message)
        {
            var handlers = UnimplementedReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<UnimplementedMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="DebugMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="DebugMessage"/> message.</param>
        protected virtual void OnDebugReceived(DebugMessage message)
        {
            var handlers = DebugReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<DebugMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="ServiceRequestMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="ServiceRequestMessage"/> message.</param>
        protected virtual void OnServiceRequestReceived(ServiceRequestMessage message)
        {
            var handlers = ServiceRequestReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<ServiceRequestMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="ServiceAcceptMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="ServiceAcceptMessage"/> message.</param>
        protected virtual void OnServiceAcceptReceived(ServiceAcceptMessage message)
        {
            var handlers = ServiceAcceptReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<ServiceAcceptMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="KeyExchangeInitMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="KeyExchangeInitMessage"/> message.</param>
        protected virtual void OnKeyExchangeInitReceived(KeyExchangeInitMessage message)
        {
            this._keyExchangeInProgress = true;

            this._keyExchangeCompletedWaitHandle.Reset();

            //  Disable all registered messages except key exchange related
            foreach (var messageMetadata in this._messagesMetadata)
            {
                if (messageMetadata.Activated && messageMetadata.Number > 2 && (messageMetadata.Number < 20 || messageMetadata.Number > 30))
                    messageMetadata.Enabled = false;
            }

            var keyExchangeAlgorithmName = (from c in this.ConnectionInfo.KeyExchangeAlgorithms.Keys
                                            from s in message.KeyExchangeAlgorithms
                                            where s == c
                                            select c).FirstOrDefault();

            if (keyExchangeAlgorithmName == null)
            {
                throw new SshConnectionException("Failed to negotiate key exchange algorithm.", DisconnectReason.KeyExchangeFailed);
            }

            //  Create instance of key exchange algorithm that will be used
            this._keyExchange = this.ConnectionInfo.KeyExchangeAlgorithms[keyExchangeAlgorithmName].CreateInstance<KeyExchange>();

            this.ConnectionInfo.CurrentKeyExchangeAlgorithm = keyExchangeAlgorithmName;

            this._keyExchange.HostKeyReceived += KeyExchange_HostKeyReceived;

            //  Start the algorithm implementation
            this._keyExchange.Start(this, message);

            var keyExchangeInitReceived = KeyExchangeInitReceived;
            if (keyExchangeInitReceived != null)
                keyExchangeInitReceived(this, new MessageEventArgs<KeyExchangeInitMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="NewKeysMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="NewKeysMessage"/> message.</param>
        protected virtual void OnNewKeysReceived(NewKeysMessage message)
        {
            //  Update sessionId
            if (this.SessionId == null)
            {
                this.SessionId = this._keyExchange.ExchangeHash;
            }

            //  Dispose of old ciphers and hash algorithms
            if (this._serverMac != null)
            {
                this._serverMac.Clear();
                this._serverMac = null;
            }

            if (this._clientMac != null)
            {
                this._clientMac.Clear();
                this._clientMac = null;
            }

            //  Update negotiated algorithms
            this._serverCipher = this._keyExchange.CreateServerCipher();
            this._clientCipher = this._keyExchange.CreateClientCipher();
            this._serverMac = this._keyExchange.CreateServerHash();
            this._clientMac = this._keyExchange.CreateClientHash();
            this._clientCompression = this._keyExchange.CreateCompressor();
            this._serverDecompression = this._keyExchange.CreateDecompressor();

            //  Dispose of old KeyExchange object as it is no longer needed.
            if (this._keyExchange != null)
            {
                this._keyExchange.HostKeyReceived -= KeyExchange_HostKeyReceived;
                this._keyExchange.Dispose();
                this._keyExchange = null;
            }

            //  Enable all active registered messages
            foreach (var messageMetadata in this._messagesMetadata)
            {
                if (messageMetadata.Activated)
                    messageMetadata.Enabled = true;
            }

            var newKeysReceived = NewKeysReceived;
            if (newKeysReceived != null)
                newKeysReceived(this, new MessageEventArgs<NewKeysMessage>(message));

            //  Signal that key exchange completed
            this._keyExchangeCompletedWaitHandle.Set();

            this._keyExchangeInProgress = false;
        }

        /// <summary>
        /// Called when client is disconnecting from the server.
        /// </summary>
        internal void OnDisconnecting()
        {
            _isDisconnecting = true;
        }

        /// <summary>
        /// Called when <see cref="RequestMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="RequestMessage"/> message.</param>
        protected virtual void OnUserAuthenticationRequestReceived(RequestMessage message)
        {
            var handlers = UserAuthenticationRequestReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<RequestMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="FailureMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="FailureMessage"/> message.</param>
        protected virtual void OnUserAuthenticationFailureReceived(FailureMessage message)
        {
            var handlers = UserAuthenticationFailureReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<FailureMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="SuccessMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="SuccessMessage"/> message.</param>
        protected virtual void OnUserAuthenticationSuccessReceived(SuccessMessage message)
        {
            var handlers = UserAuthenticationSuccessReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<SuccessMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="BannerMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="BannerMessage"/> message.</param>
        protected virtual void OnUserAuthenticationBannerReceived(BannerMessage message)
        {
            var handlers = UserAuthenticationBannerReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<BannerMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="GlobalRequestMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="GlobalRequestMessage"/> message.</param>
        protected virtual void OnGlobalRequestReceived(GlobalRequestMessage message)
        {
            var handlers = GlobalRequestReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<GlobalRequestMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="RequestSuccessMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="RequestSuccessMessage"/> message.</param>
        protected virtual void OnRequestSuccessReceived(RequestSuccessMessage message)
        {
            var handlers = RequestSuccessReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<RequestSuccessMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="RequestFailureMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="RequestFailureMessage"/> message.</param>
        protected virtual void OnRequestFailureReceived(RequestFailureMessage message)
        {
            var handlers = RequestFailureReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<RequestFailureMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="ChannelOpenMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="ChannelOpenMessage"/> message.</param>
        protected virtual void OnChannelOpenReceived(ChannelOpenMessage message)
        {
            var handlers = ChannelOpenReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<ChannelOpenMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="ChannelOpenConfirmationMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="ChannelOpenConfirmationMessage"/> message.</param>
        protected virtual void OnChannelOpenConfirmationReceived(ChannelOpenConfirmationMessage message)
        {
            var handlers = ChannelOpenConfirmationReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<ChannelOpenConfirmationMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="ChannelOpenFailureMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="ChannelOpenFailureMessage"/> message.</param>
        protected virtual void OnChannelOpenFailureReceived(ChannelOpenFailureMessage message)
        {
            var handlers = ChannelOpenFailureReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<ChannelOpenFailureMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="ChannelWindowAdjustMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="ChannelWindowAdjustMessage"/> message.</param>
        protected virtual void OnChannelWindowAdjustReceived(ChannelWindowAdjustMessage message)
        {
            var handlers = ChannelWindowAdjustReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<ChannelWindowAdjustMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="ChannelDataMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="ChannelDataMessage"/> message.</param>
        protected virtual void OnChannelDataReceived(ChannelDataMessage message)
        {
            var handlers = ChannelDataReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<ChannelDataMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="ChannelExtendedDataMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="ChannelExtendedDataMessage"/> message.</param>
        protected virtual void OnChannelExtendedDataReceived(ChannelExtendedDataMessage message)
        {
            var handlers = ChannelExtendedDataReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<ChannelExtendedDataMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="ChannelCloseMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="ChannelCloseMessage"/> message.</param>
        protected virtual void OnChannelEofReceived(ChannelEofMessage message)
        {
            var handlers = ChannelEofReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<ChannelEofMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="ChannelCloseMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="ChannelCloseMessage"/> message.</param>
        protected virtual void OnChannelCloseReceived(ChannelCloseMessage message)
        {
            var handlers = ChannelCloseReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<ChannelCloseMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="ChannelRequestMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="ChannelRequestMessage"/> message.</param>
        protected virtual void OnChannelRequestReceived(ChannelRequestMessage message)
        {
            var handlers = ChannelRequestReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<ChannelRequestMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="ChannelSuccessMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="ChannelSuccessMessage"/> message.</param>
        protected virtual void OnChannelSuccessReceived(ChannelSuccessMessage message)
        {
            var handlers = ChannelSuccessReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<ChannelSuccessMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="ChannelFailureMessage"/> message received.
        /// </summary>
        /// <param name="message"><see cref="ChannelFailureMessage"/> message.</param>
        protected virtual void OnChannelFailureReceived(ChannelFailureMessage message)
        {
            var handlers = ChannelFailureReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<ChannelFailureMessage>(message));
        }

        /// <summary>
        /// Called when <see cref="Message"/> message received.
        /// </summary>
        /// <param name="message"><see cref="Message"/> message.</param>
        protected virtual void OnMessageReceived(Message message)
        {
            var handlers = MessageReceived;
            if (handlers != null)
                handlers(this, new MessageEventArgs<Message>(message));
        }

        #endregion

        private void KeyExchange_HostKeyReceived(object sender, HostKeyEventArgs e)
        {
            var handlers = HostKeyReceived;
            if (handlers != null)
                handlers(this, e);
        }

        /// <summary>
        /// Reads the specified length of bytes from the server.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns>
        /// The bytes read from the server.
        /// </returns>
        private byte[] Read(int length)
        {
            var buffer = new byte[length];

            this.SocketRead(length, ref buffer);

            return buffer;
        }

        #region Message loading functions

        /// <summary>
        /// Registers SSH Message with the session.
        /// </summary>
        /// <param name="messageName">Name of the message.</param>
        public void RegisterMessage(string messageName)
        {
            this.InternalRegisterMessage(messageName);
        }

        /// <summary>
        /// Removes SSH message from the session
        /// </summary>
        /// <param name="messageName">Name of the message.</param>
        public void UnRegisterMessage(string messageName)
        {
            this.InternalUnRegisterMessage(messageName);
        }

        /// <summary>
        /// Loads the message.
        /// </summary>
        /// <param name="data">Message data.</param>
        /// <returns>New message</returns>
        private Message LoadMessage(byte[] data)
        {
            var messageType = data[0];
            var messageMetadata = (from m in this._messagesMetadata where m.Number == messageType && m.Enabled && m.Activated select m).SingleOrDefault();
            if (messageMetadata == null)
                throw new SshException(string.Format(CultureInfo.CurrentCulture, "Message type {0} is not valid.", messageType));

            var message = messageMetadata.Type.CreateInstance<Message>();

            message.Load(data);

            this.Log(string.Format("ReceiveMessage from server: '{0}': '{1}'.", message.GetType().Name, message));

            return message;
        }

        partial void InternalRegisterMessage(string messageName);

        partial void InternalUnRegisterMessage(string messageName);

        #endregion

        partial void ExecuteThread(Action action);

        /// <summary>
        /// Gets a value indicating whether the socket is connected.
        /// </summary>
        /// <value>
        /// <c>true</c> if the socket is connected; otherwise, <c>false</c>.
        /// </value>
        partial void IsSocketConnected(ref bool isConnected);

        partial void SocketConnect(string host, int port);

        partial void SocketDisconnect();

        partial void SocketRead(int length, ref byte[] buffer);

        partial void SocketReadLine(ref string response);

        partial void Log(string text);

        /// <summary>
        /// Writes the specified data to the server.
        /// </summary>
        /// <param name="data">The data.</param>
        partial void SocketWrite(byte[] data);

        /// <summary>
        /// Disconnects and disposes the socket.
        /// </summary>
        private void SocketDisconnectAndDispose()
        {
            if (_socket != null)
            {
                lock (_socketLock)
                {
                    if (_socket != null)
                    {
                        SocketDisconnect();
                        _socket.Dispose();
                        _socket = null;
                    }
                }
            }
        }

        /// <summary>
        /// Listens for incoming message from the server and handles them. This method run as a task on separate thread.
        /// </summary>
        private void MessageListener()
        {
            try
            {
                while (this._socket != null && this._socket.Connected)
                {
                    var message = this.ReceiveMessage();
                    this.HandleMessageCore(message);
                }
            }
            catch (Exception exp)
            {
                this.RaiseError(exp);
            }
        }

        private byte SocketReadByte()
        {
            var buffer = new byte[1];

            this.SocketRead(1, ref buffer);

            return buffer[0];
        }

        private void SocketWriteByte(byte data)
        {
            this.SocketWrite(new[] {data});
        }

        private void ConnectSocks4()
        {
            //  Send socks version number
            this.SocketWriteByte(0x04);

            //  Send command code
            this.SocketWriteByte(0x01);

            //  Send port
            this.SocketWriteByte((byte)(this.ConnectionInfo.Port / 0xFF));
            this.SocketWriteByte((byte)(this.ConnectionInfo.Port % 0xFF));

            //  Send IP
            IPAddress ipAddress = this.ConnectionInfo.Host.GetIPAddress();
            this.SocketWrite(ipAddress.GetAddressBytes());

            //  Send username
            var username = new Common.ASCIIEncoding().GetBytes(this.ConnectionInfo.ProxyUsername);
            this.SocketWrite(username);
            this.SocketWriteByte(0x00);

            //  Read 0
            if (this.SocketReadByte() != 0)
            {
                throw new ProxyException("SOCKS4: Null is expected.");
            }

            //  Read response code
            var code = this.SocketReadByte();

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

            var dummyBuffer = new byte[4];

            //  Read 2 bytes to be ignored
            this.SocketRead(2, ref dummyBuffer);

            //  Read 4 bytes to be ignored
            this.SocketRead(4, ref dummyBuffer);
        }

        private void ConnectSocks5()
        {
            //  Send socks version number
            this.SocketWriteByte(0x05);

            //  Send number of supported authentication methods
            this.SocketWriteByte(0x02);

            //  Send supported authentication methods
            this.SocketWriteByte(0x00); //  No authentication
            this.SocketWriteByte(0x02); //  Username/Password

            var socksVersion = this.SocketReadByte();
            if (socksVersion != 0x05)
                throw new ProxyException(string.Format("SOCKS Version '{0}' is not supported.", socksVersion));

            var authenticationMethod = this.SocketReadByte();
            switch (authenticationMethod)
            {
                case 0x00:
                    break;
                case 0x02:

                    //  Send version
                    this.SocketWriteByte(0x01);

                    var encoding = new Common.ASCIIEncoding();

                    var username = encoding.GetBytes(this.ConnectionInfo.ProxyUsername);

                    if (username.Length > byte.MaxValue)
                        throw new ProxyException("Proxy username is too long.");

                    //  Send username length
                    this.SocketWriteByte((byte)username.Length);

                    //  Send username
                    this.SocketWrite(username);

                    var password = encoding.GetBytes(this.ConnectionInfo.ProxyPassword);

                    if (password.Length > byte.MaxValue)
                        throw new ProxyException("Proxy password is too long.");

                    //  Send username length
                    this.SocketWriteByte((byte)password.Length);

                    //  Send username
                    this.SocketWrite(password);

                    var serverVersion = this.SocketReadByte();

                    if (serverVersion != 1)
                        throw new ProxyException("SOCKS5: Server authentication version is not valid.");

                    var statusCode = this.SocketReadByte();
                    if (statusCode != 0)
                        throw new ProxyException("SOCKS5: Username/Password authentication failed.");

                    break;
                case 0xFF:
                    throw new ProxyException("SOCKS5: No acceptable authentication methods were offered.");
            }

            //  Send socks version number
            this.SocketWriteByte(0x05);

            //  Send command code
            this.SocketWriteByte(0x01); //  establish a TCP/IP stream connection

            //  Send reserved, must be 0x00
            this.SocketWriteByte(0x00);

            IPAddress ip = this.ConnectionInfo.Host.GetIPAddress();

            //  Send address type and address
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                this.SocketWriteByte(0x01);
                var address = ip.GetAddressBytes();
                this.SocketWrite(address);
            }
            else if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                this.SocketWriteByte(0x04);
                var address = ip.GetAddressBytes();
                this.SocketWrite(address);
            }
            else
            {
                throw new ProxyException(string.Format("SOCKS5: IP address '{0}' is not supported.", ip));
            }

            //  Send port
            this.SocketWriteByte((byte)(this.ConnectionInfo.Port / 0xFF));
            this.SocketWriteByte((byte)(this.ConnectionInfo.Port % 0xFF));

            //  Read Server SOCKS5 version
            if (this.SocketReadByte() != 5)
            {
                throw new ProxyException("SOCKS5: Version 5 is expected.");
            }

            //  Read response code
            var status = this.SocketReadByte();

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
            if (this.SocketReadByte() != 0)
            {
                throw new ProxyException("SOCKS5: 0 byte is expected.");
            }

            var addressType = this.SocketReadByte();
            var responseIp = new byte[16];

            switch (addressType)
            {
                case 0x01:
                    this.SocketRead(4, ref responseIp);
                    break;
                case 0x04:
                    this.SocketRead(16, ref responseIp);
                    break;
                default:
                    throw new ProxyException(string.Format("Address type '{0}' is not supported.", addressType));
            }

            var port = new byte[2];

            //  Read 2 bytes to be ignored
            this.SocketRead(2, ref port);
        }

        private void ConnectHttp()
        {
            var httpResponseRe = new Regex(@"HTTP/(?<version>\d[.]\d) (?<statusCode>\d{3}) (?<reasonPhrase>.+)$");
            var httpHeaderRe = new Regex(@"(?<fieldName>[^\[\]()<>@,;:\""/?={} \t]+):(?<fieldValue>.+)?");

            var encoding = new Common.ASCIIEncoding();

            this.SocketWrite(encoding.GetBytes(string.Format("CONNECT {0}:{1} HTTP/1.0\r\n", this.ConnectionInfo.Host, this.ConnectionInfo.Port)));

            //  Sent proxy authorization is specified
            if (!string.IsNullOrEmpty(this.ConnectionInfo.ProxyUsername))
            {
                var authorization = string.Format("Proxy-Authorization: Basic {0}\r\n",
                                                  Convert.ToBase64String(encoding.GetBytes(string.Format("{0}:{1}", this.ConnectionInfo.ProxyUsername, this.ConnectionInfo.ProxyPassword)))
                                                  );
                this.SocketWrite(encoding.GetBytes(authorization));
            }

            this.SocketWrite(encoding.GetBytes("\r\n"));

            HttpStatusCode? statusCode = null;
            var response = string.Empty;
            var contentLength = 0;

            while (true)
            {
                this.SocketReadLine(ref response);

                if (statusCode == null)
                {
                    var statusMatch = httpResponseRe.Match(response);
                    if (statusMatch.Success)
                    {
                        var httpStatusCode = statusMatch.Result("${statusCode}");
                        statusCode = (HttpStatusCode) int.Parse(httpStatusCode);
                        if (statusCode != HttpStatusCode.OK)
                        {
                            var reasonPhrase = statusMatch.Result("${reasonPhrase}");
                            throw new ProxyException(string.Format("HTTP: Status code {0}, \"{1}\"", httpStatusCode,
                                reasonPhrase));
                        }
                        continue;
                    }
                }

                // continue on parsing message headers coming from the server
                var headerMatch = httpHeaderRe.Match(response);
                if (headerMatch.Success)
                {
                    var fieldName = headerMatch.Result("${fieldName}");
                    if (fieldName.Equals("Content-Length", StringComparison.InvariantCultureIgnoreCase))
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
                        SocketRead(contentLength, ref contentBody);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="ErrorOccured"/> event.
        /// </summary>
        /// <param name="exp">The exp.</param>
        private void RaiseError(Exception exp)
        {
            var connectionException = exp as SshConnectionException;

            if (_isDisconnecting)
            {
                //  a connection exception which is raised while isDisconnecting is normal and
                //  should be ignored
                if (connectionException != null)
                    return;

                // any timeout while disconnecting can be caused by loss of connectivity
                // altogether and should be ignored
                var socketException = exp as SocketException;
                if (socketException != null && socketException.SocketErrorCode == SocketError.TimedOut)
                    return;
            }

            this._exception = exp;

            this._exceptionWaitHandle.Set();

            var errorOccured = ErrorOccured;
            if (errorOccured != null)
                errorOccured(this, new ExceptionEventArgs(exp));

            if (connectionException != null && connectionException.DisconnectReason != DisconnectReason.ConnectionLost)
            {
                this.Disconnect(connectionException.DisconnectReason, exp.ToString());
            }
        }

        /// <summary>
        /// Resets connection-specific information to ensure state of a previous connection
        /// does not affect new connections.
        /// </summary>
        private void Reset()
        {
            if (_exceptionWaitHandle != null)
                _exceptionWaitHandle.Reset();
            if (_keyExchangeCompletedWaitHandle != null)
                _keyExchangeCompletedWaitHandle.Reset();
            if (_messageListenerCompleted != null)
                _messageListenerCompleted.Reset();

            SessionId = null;
            _isDisconnectMessageSent = false;
            _isDisconnecting = false;
            _isAuthenticated = false;
            _exception = null;
            _keyExchangeInProgress = false;
        }

        #region IDisposable Members

        private bool _disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged ResourceMessages.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged ResourceMessages.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged ResourceMessages.
                if (disposing)
                {
                    Disconnect();

                    if (this._serviceAccepted != null)
                    {
                        this._serviceAccepted.Dispose();
                        this._serviceAccepted = null;
                    }

                    if (this._exceptionWaitHandle != null)
                    {
                        this._exceptionWaitHandle.Dispose();
                        this._exceptionWaitHandle = null;
                    }

                    if (this._keyExchangeCompletedWaitHandle != null)
                    {
                        this._keyExchangeCompletedWaitHandle.Dispose();
                        this._keyExchangeCompletedWaitHandle = null;
                    }

                    if (this._serverMac != null)
                    {
                        this._serverMac.Clear();
                        this._serverMac = null;
                    }

                    if (this._clientMac != null)
                    {
                        this._clientMac.Clear();
                        this._clientMac = null;
                    }

                    if (this._keyExchange != null)
                    {
                        this._keyExchange.HostKeyReceived -= KeyExchange_HostKeyReceived;
                        this._keyExchange.Dispose();
                        this._keyExchange = null;
                    }
                }

                // Note disposing has been done.
                this._disposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Session"/> is reclaimed by garbage collection.
        /// </summary>
        ~Session()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion

        private class MessageMetadata
        {
            public string Name { get; set; }

            public byte Number { get; set; }

            public bool Enabled { get; set; }

            public bool Activated { get; set; }

            public Type Type { get; set; }
        }
    }
}