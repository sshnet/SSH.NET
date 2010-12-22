using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshClient.Channels;
using Renci.SshClient.Common;
using Renci.SshClient.Compression;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Authentication;
using Renci.SshClient.Messages.Connection;
using Renci.SshClient.Messages.Transport;
using Renci.SshClient.Security;

namespace Renci.SshClient
{
    public class Session : IDisposable
    {
        protected const int MAXIMUM_PACKET_SIZE = 35000;

        protected const int MAXIMUM_PAYLOAD_SIZE = 32768;

        private static RNGCryptoServiceProvider _randomizer = new System.Security.Cryptography.RNGCryptoServiceProvider();

        private static Regex _serverVersionRe = new Regex("^SSH-(?<protoversion>[^-]+)-(?<softwareversion>.+)( SP.+)?$", RegexOptions.Compiled);

        /// <summary>
        /// Controls how many authentication attempts can take place at the same time.
        /// </summary>
        /// <remarks>
        /// Some server may restrict number to prevent authentication attacks
        /// </remarks>
        private static SemaphoreSlim _authenticationConnection = new SemaphoreSlim(3);

        /// <summary>
        /// Holds connection socket.
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// Holds reference to task that listnes for incoming messages
        /// </summary>
        private Task _messageListener;

        /// <summary>
        /// Specifies outbount packet number
        /// </summary>
        private volatile UInt32 _outboundPacketSequence = 0;

        /// <summary>
        /// Specifies incomin packet number
        /// </summary>
        private UInt32 _inboundPacketSequence = 0;

        /// <summary>
        /// WaitHandle to signale that last service request was accepted
        /// </summary>
        private EventWaitHandle _serviceAccepted = new AutoResetEvent(false);

        /// <summary>
        /// WaitHandle to signal that exception was thrown by another thread.
        /// </summary>
        private EventWaitHandle _exceptionWaitHandle = new AutoResetEvent(false);

        private EventWaitHandle _keyExchangeCompletedWaitHandle = new ManualResetEvent(false);

        /// <summary>
        /// Exception that need to be thrown by waiting thread
        /// </summary>
        private Exception _exception;

        /// <summary>
        /// Specifies weither connection is authenticated
        /// </summary>
        private bool _isAuthenticated;

        /// <summary>
        /// Specifies whether user issued Disconnect command or not
        /// </summary>
        private bool _isDisconnecting;

        private KeyExchange _keyExchange;

        private HMac _serverMac;

        private HMac _clientMac;

        private Cipher _clientCipher;

        private Cipher _serverCipher;

        private Compressor _serverDecompression;

        private Compressor _clientCompression;

        /// <summary>
        /// Hold session specific semaphores
        /// </summary>
        private List<SemaphoreSlim> _semaphores = new List<SemaphoreSlim>();

        private SemaphoreSlim _sessionSemaphore;
        /// <summary>
        /// Gets the session semaphore that controls session channels.
        /// </summary>
        /// <value>The session semaphore.</value>
        public SemaphoreSlim SessionSemaphore
        {
            get
            {
                if (this._sessionSemaphore == null)
                {
                    lock (this)
                    {
                        if (this._sessionSemaphore == null)
                        {
                            this._sessionSemaphore = new SemaphoreSlim(this.ConnectionInfo.MaxSessions);
                        }
                    }
                }

                return this._sessionSemaphore;
            }
        }

        private uint _nextChannelNumber;
        /// <summary>
        /// Gets the next channel number.
        /// </summary>
        /// <value>The next channel number.</value>
        public uint NextChannelNumber
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
        /// Gets a value indicating whether socket connected.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if socket connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected
        {
            get
            {
                return this._socket != null && this._socket.Connected && this._isAuthenticated && this._messageListener.Status == TaskStatus.Running;
            }
        }

        /// <summary>
        /// Gets or sets the session id.
        /// </summary>
        /// <value>The session id.</value>
        public IEnumerable<byte> SessionId { get; private set; }

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
                    this._clientInitMessage = new KeyExchangeInitMessage()
                    {
                        KeyExchangeAlgorithms = this.ConnectionInfo.KeyExchangeAlgorithms.Keys,
                        ServerHostKeyAlgorithms = this.ConnectionInfo.HostKeyAlgorithms.Keys,
                        EncryptionAlgorithmsClientToServer = this.ConnectionInfo.Encryptions.Keys,
                        EncryptionAlgorithmsServerToClient = this.ConnectionInfo.Encryptions.Keys,
                        MacAlgorithmsClientToSserver = this.ConnectionInfo.HmacAlgorithms.Keys,
                        MacAlgorithmsServerToClient = this.ConnectionInfo.HmacAlgorithms.Keys,
                        CompressionAlgorithmsClientToServer = this.ConnectionInfo.CompressionAlgorithms.Keys,
                        CompressionAlgorithmsServerToClient = this.ConnectionInfo.CompressionAlgorithms.Keys,
                        LanguagesClientToServer = new string[] { string.Empty },
                        LanguagesServerToClient = new string[] { string.Empty },
                        FirstKexPacketFollows = false,
                        Reserved = 0,
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

        public event EventHandler<ErrorEventArgs> ErrorOccured;

        public event EventHandler<EventArgs> Disconnecting;

        public event EventHandler<EventArgs> Disconnected;

        /// <summary>
        /// Occurs when user is being authenticated and additional information available or required
        /// </summary>
        public event EventHandler<AuthenticationEventArgs> Authenticating;

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
        internal Session(ConnectionInfo connectionInfo)
        {
            this.ConnectionInfo = connectionInfo;
            this.ClientVersion = string.Format("SSH-2.0-Renci.SshClient.{0}", this.GetType().Assembly.GetName().Version);
        }

        /// <summary>
        /// Connects to the server.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        public void Connect()
        {
            if (this.ConnectionInfo == null)
            {
                throw new ArgumentNullException("connectionInfo");
            }

            if (this.IsConnected)
                return;

            try
            {
                _authenticationConnection.Wait();

                if (this.IsConnected)
                    return;

                lock (this)
                {
                    //  If connected don't connect again
                    if (this.IsConnected)
                        return;

                    var ep = new IPEndPoint(Dns.GetHostAddresses(this.ConnectionInfo.Host)[0], this.ConnectionInfo.Port);
                    this._socket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    //  Connect socket with 5 seconds timeout
                    var connectResult = this._socket.BeginConnect(ep, null, null);

                    connectResult.AsyncWaitHandle.WaitOne(this.ConnectionInfo.Timeout);

                    this._socket.EndConnect(connectResult);

                    this._socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, 1);

                    Match versionMatch = null;
                    //  Get server version from the server,
                    //  ignore text lines which are sent before if any
                    NetworkStream ns = null;
                    try
                    {

                        ns = new NetworkStream(this._socket);
                        using (var sr = new StreamReader(ns))
                        {
                            while (true)
                            {
                                this.ServerVersion = sr.ReadLine();
                                if (string.IsNullOrEmpty(this.ServerVersion))
                                {
                                    throw new InvalidOperationException("Server string is null or empty.");
                                }

                                versionMatch = _serverVersionRe.Match(this.ServerVersion);

                                if (versionMatch.Success)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (ns != null)
                        {
                            ns.Dispose();
                        }
                    }

                    //  Get server SSH version
                    var version = versionMatch.Result("${protoversion}");

                    if (!(version.Equals("2.0") || version.Equals("1.99")))
                    {
                        throw new SshConnectionException(string.Format("Server version '{0}' is not supported.", version), DisconnectReasons.ProtocolVersionNotSupported);
                    }

                    this.Write(Encoding.ASCII.GetBytes(string.Format("{0}\x0D\x0A", this.ClientVersion)));

                    //  Register Transport response messages
                    this.RegisterMessageType<DisconnectMessage>(MessageTypes.Disconnect);
                    this.RegisterMessageType<IgnoreMessage>(MessageTypes.Ignore);
                    this.RegisterMessageType<UnimplementedMessage>(MessageTypes.Unimplemented);
                    this.RegisterMessageType<DebugMessage>(MessageTypes.Debug);
                    this.RegisterMessageType<ServiceAcceptMessage>(MessageTypes.ServiceAcceptRequest);
                    this.RegisterMessageType<KeyExchangeInitMessage>(MessageTypes.KeyExchangeInit);
                    this.RegisterMessageType<NewKeysMessage>(MessageTypes.NewKeys);


                    //  Start incoming request listener
                    this._messageListener = Task.Factory.StartNew(() => { this.MessageListener(); }, TaskCreationOptions.LongRunning);

                    //  Wait for key exchange to be completed
                    this.WaitHandle(this._keyExchangeCompletedWaitHandle);

                    //  If sessionId is not set then its not connected
                    if (this.SessionId == null)
                    {
                        this.Disconnect();
                        return;
                    }

                    //  Request user authorization service
                    this.SendMessage(new ServiceRequestMessage(ServiceNames.UserAuthentication));

                    //  Wait for service to be accepted
                    this.WaitHandle(this._serviceAccepted);

                    if (string.IsNullOrEmpty(this.ConnectionInfo.Username))
                    {
                        throw new SshException("Username is not specified.");
                    }

                    //  Query server supported authentication methods
                    var username = this.ConnectionInfo.Username;

                    IEnumerable<string> serverMethods = null;

                    using (var noneAuthentication = new UserAuthenticationNone())
                    {
                        if (noneAuthentication.Authenticate(username, this))
                        {
                            throw new SshAuthenticationException("'none' authentication should not be allowed.");
                        }

                        serverMethods = noneAuthentication.Methods;
                    }

                    var methodNames = from serverMethod in serverMethods
                                      from clientMethod in this.ConnectionInfo.SupportedAuthenticationMethods.Keys
                                      where
                                        serverMethod == clientMethod
                                      select serverMethod;

                    foreach (var methodName in methodNames)
                    {
                        var authentication = this.ConnectionInfo.SupportedAuthenticationMethods[methodName].CreateInstance<UserAuthentication>();

                        authentication.Authenticating += delegate(object sender, AuthenticationEventArgs e)
                        {
                            if (this.Authenticating != null)
                            {
                                this.Authenticating(sender, e);
                            }
                        };

                        authentication.Authenticate(username, this);

                        if (authentication.IsAuthenticated)
                        {
                            this._isAuthenticated = true;
                            break;
                        }
                    }

                    if (!this._isAuthenticated)
                    {
                        throw new SshAuthenticationException("User cannot be authenticated.");
                    }

                    Monitor.Pulse(this);
                }
            }
            finally
            {
                _authenticationConnection.Release();
            }
        }

        /// <summary>
        /// Disconnects from the server
        /// </summary>
        public void Disconnect()
        {
            this._isDisconnecting = true;

            if (this.Disconnecting != null)
            {
                this.Disconnecting(this, new EventArgs());
            }

            this.SendDisconnect(DisconnectReasons.ByApplication, "Connection terminated by the client.");
        }

        internal T CreateChannel<T>() where T : Channel, new()
        {
            return CreateChannel<T>(0, 0x100000, 0x8000);
        }

        internal T CreateChannel<T>(uint serverChannelNumber, uint windowSize, uint packetSize) where T : Channel, new()
        {
            T channel = new T();
            lock (this)
            {
                channel.Initialize(this, serverChannelNumber, windowSize, packetSize);
            }
            return channel;
        }

        /// <summary>
        /// Sends "keep alive" message to keep connection alive.
        /// </summary>
        internal void SendKeepAlive()
        {
            this.SendMessage(new GlobalRequestMessage(GlobalRequestNames.KeepAlive, false));
        }

        /// <summary>
        /// Waits for handle to signal while checking other handles as well including timeout check to prevent waiting for ever
        /// </summary>
        /// <param name="waitHandle">The wait handle.</param>
        internal void WaitHandle(WaitHandle waitHandle)
        {
            var waitHandles = new WaitHandle[]
                {
                    this._exceptionWaitHandle,
                    waitHandle,
                };

            var index = EventWaitHandle.WaitAny(waitHandles);

            if (index < 1)
            {
                throw this._exception;
            }
            else if (index > 1)
            {
                this.SendDisconnect(DisconnectReasons.ByApplication, "Operation timeout");

                throw new SshOperationTimeoutException("Session operation has timed out");
            }
        }

        /// <summary>
        /// Sends packet message to the server.
        /// </summary>
        /// <param name="message">The message.</param>
        internal void SendMessage(Message message)
        {
            if (!this._socket.Connected)
                return;
            //  Messages can be sent by different thread so we need to synchronize it            
            var paddingMultiplier = this._clientCipher == null ? (byte)8 : (byte)this._clientCipher.BlockSize;    //    Should be recalculate base on cipher min lenght if sipher specified

            var messageData = message.GetBytes();

            if (messageData.Count() > Session.MAXIMUM_PAYLOAD_SIZE)
            {
                throw new InvalidOperationException(string.Format("Payload cannot be more then {0} bytes.", Session.MAXIMUM_PAYLOAD_SIZE));
            }

            if (this._clientCompression != null)
            {
                messageData = this._clientCompression.Compress(messageData);
            }

            var packetLength = messageData.Count() + 4 + 1; //  add length bytes and padding byte
            byte paddingLength = (byte)((-packetLength) & (paddingMultiplier - 1));
            if (paddingLength < paddingMultiplier)
            {
                paddingLength += paddingMultiplier;
            }

            //  Build Packet data
            var packetData = new List<byte>();

            //  Add packet padding length
            packetData.Add(paddingLength);

            //  Add packet payload
            packetData.AddRange(messageData);

            //  Add random padding
            var paddingBytes = new byte[paddingLength];
            _randomizer.GetBytes(paddingBytes);
            packetData.AddRange(paddingBytes);

            //  Insert packet length
            packetData.InsertRange(0, BitConverter.GetBytes((uint)(packetData.Count())).Reverse());

            //  Lock handling of _outboundPacketSequence since it must be sent sequently to server
            lock (this._socket)
            {
                //  Calculate packet hash
                var hashData = new List<byte>();
                hashData.AddRange(BitConverter.GetBytes((this._outboundPacketSequence)).Reverse());
                hashData.AddRange(packetData);

                //  Encrypt packet data
                var encryptedData = packetData.ToList();
                if (this._clientCipher != null)
                {
                    encryptedData = new List<byte>(this._clientCipher.Encrypt(packetData));
                }

                //  Add message authentication code (MAC)
                if (this._clientMac != null)
                {
                    var hash = this._clientMac.ComputeHash(hashData.ToArray());

                    encryptedData.AddRange(hash);
                }

                if (encryptedData.Count > Session.MAXIMUM_PACKET_SIZE)
                {
                    throw new InvalidOperationException(string.Format("Packet is too big. Maximum packet size is {0} bytes.", Session.MAXIMUM_PACKET_SIZE));
                }

                this.Write(encryptedData.ToArray());

                this._outboundPacketSequence++;

                Monitor.Pulse(this._socket);
            }
        }

        /// <summary>
        /// Receives the message from the server.
        /// </summary>
        /// <returns></returns>
        private Message ReceiveMessage()
        {
            if (!this._socket.Connected)
                return null;

            //  No lock needed since all messages read by only one thread

            List<byte> decryptedData;

            var blockSize = this._serverCipher == null ? (byte)8 : (byte)this._serverCipher.BlockSize;

            //  Read packet length first
            var data = new List<byte>(this.Read(blockSize));

            if (this._serverCipher == null)
            {
                decryptedData = data.ToList();
            }
            else
            {
                decryptedData = new List<byte>(this._serverCipher.Decrypt(data));
            }

            var packetLength = (uint)(decryptedData[0] << 24 | decryptedData[1] << 16 | decryptedData[2] << 8 | decryptedData[3]);

            //  Test packet minimum and maximum boundaries
            if (packetLength < Math.Max((byte)16, blockSize) - 4 || packetLength > Session.MAXIMUM_PACKET_SIZE - 4)
                throw new SshConnectionException(string.Format("Bad packet length {0}", packetLength), DisconnectReasons.ProtocolError);

            //  Read rest of the packet data
            int bytesToRead = (int)(packetLength - (blockSize - 4));

            if (bytesToRead > 0)
            {
                if (this._serverCipher == null)
                {
                    decryptedData.AddRange(this.Read(bytesToRead));
                }
                else
                {
                    decryptedData.AddRange(this._serverCipher.Decrypt(this.Read(bytesToRead)));
                }
            }

            var paddingLength = decryptedData[4];

            var messagePayload = decryptedData.Skip(5).Take((int)(packetLength - paddingLength - 1));

            if (this._serverDecompression != null)
            {
                messagePayload = new List<byte>(this._serverDecompression.Uncompress(messagePayload));
            }

            //  Validate message against MAC            
            if (this._serverMac != null)
            {
                var serverHash = this.Read(this._serverMac.HashSize / 8);

                var clientHashData = new List<byte>();
                clientHashData.AddRange(BitConverter.GetBytes(this._inboundPacketSequence).Reverse());
                clientHashData.AddRange(decryptedData);

                //  Calculate packet hash
                var clientHash = this._serverMac.ComputeHash(clientHashData.ToArray());

                if (!serverHash.SequenceEqual(clientHash))
                {
                    throw new SshConnectionException("MAC error", DisconnectReasons.MacError);
                }
            }

            this._inboundPacketSequence++;

            return this.LoadMessage(messagePayload);
        }

        private void SendDisconnect(DisconnectReasons reasonCode, string message)
        {
            var disconnectMessage = new DisconnectMessage(reasonCode, message);

            this.SendMessage(disconnectMessage);

            //  Handle disconnect message as if it was sent by the server
            this.HandleMessage(disconnectMessage);
        }

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

            this._socket.Shutdown(SocketShutdown.Both);

            this._socket.Disconnect(true);

            if (this._messageListener != null)
            {
                //  Wait for listner task to finish
                this._messageListener.Wait();
                this._messageListener = null;
            }
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
            this._serviceAccepted.Set();
            this.OnServiceAcceptReceived(message);
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

        protected virtual void OnDisconnectReceived(DisconnectMessage message)
        {
            if (this.DisconnectReceived != null)
            {
                this.DisconnectReceived(this, new MessageEventArgs<DisconnectMessage>(message));
            }

            if (this.Disconnected != null)
            {
                this.Disconnected(this, new EventArgs());
            }
        }

        protected virtual void OnIgnoreReceived(IgnoreMessage message)
        {
            if (this.IgnoreReceived != null)
            {
                this.IgnoreReceived(this, new MessageEventArgs<IgnoreMessage>(message));
            }
        }

        protected virtual void OnUnimplementedReceived(UnimplementedMessage message)
        {
            if (this.UnimplementedReceived != null)
            {
                this.UnimplementedReceived(this, new MessageEventArgs<UnimplementedMessage>(message));
            }
        }

        protected virtual void OnDebugReceived(DebugMessage message)
        {
            if (this.DebugReceived != null)
            {
                this.DebugReceived(this, new MessageEventArgs<DebugMessage>(message));
            }
        }

        protected virtual void OnServiceRequestReceived(ServiceRequestMessage message)
        {
            if (this.ServiceRequestReceived != null)
            {
                this.ServiceRequestReceived(this, new MessageEventArgs<ServiceRequestMessage>(message));
            }
        }

        protected virtual void OnServiceAcceptReceived(ServiceAcceptMessage message)
        {
            if (this.ServiceAcceptReceived != null)
            {
                this.ServiceAcceptReceived(this, new MessageEventArgs<ServiceAcceptMessage>(message));
            }
        }

        protected virtual void OnKeyExchangeInitReceived(KeyExchangeInitMessage message)
        {
            this._keyExchangeCompletedWaitHandle.Reset();

            //  Connection type messages are not allowed during key exchange phase
            this.UnRegisterMessageType(MessageTypes.GlobalRequest);
            this.UnRegisterMessageType(MessageTypes.RequestSuccess);
            this.UnRegisterMessageType(MessageTypes.RequestFailure);
            this.UnRegisterMessageType(MessageTypes.ChannelOpenConfirmation);
            this.UnRegisterMessageType(MessageTypes.ChannelOpenFailure);
            this.UnRegisterMessageType(MessageTypes.ChannelWindowAdjust);
            this.UnRegisterMessageType(MessageTypes.ChannelExtendedData);
            this.UnRegisterMessageType(MessageTypes.ChannelRequest);
            this.UnRegisterMessageType(MessageTypes.ChannelSuccess);
            this.UnRegisterMessageType(MessageTypes.ChannelData);
            this.UnRegisterMessageType(MessageTypes.ChannelEof);
            this.UnRegisterMessageType(MessageTypes.ChannelClose);

            var keyExchangeAlgorithmName = (from c in this.ConnectionInfo.KeyExchangeAlgorithms.Keys
                                            from s in message.KeyExchangeAlgorithms
                                            where s == c
                                            select c).FirstOrDefault();

            if (keyExchangeAlgorithmName == null)
            {
                throw new SshConnectionException("Failed to negotiate key exchange algorithm.", DisconnectReasons.KeyExchangeFailed);
            }

            //  Dispose of of previous key exchange if exists
            if (this._keyExchange != null)
            {
                this._keyExchange.Dispose();
            }

            //  Create instance of key exchange algorithm that will be used
            this._keyExchange = this.ConnectionInfo.KeyExchangeAlgorithms[keyExchangeAlgorithmName].CreateInstance<KeyExchange>();

            //  Start the algorithm implementation
            this._keyExchange.Start(this, message);

            if (this.KeyExchangeInitReceived != null)
            {
                this.KeyExchangeInitReceived(this, new MessageEventArgs<KeyExchangeInitMessage>(message));
            }
        }

        protected virtual void OnNewKeysReceived(NewKeysMessage message)
        {
            //  Update sessionId
            if (this.SessionId == null)
            {
                this.SessionId = this._keyExchange.ExchangeHash;
            }

            //  Finish key exchange
            this._keyExchange.Finish();

            //  Update negotiated algorithms
            this._serverCipher = this._keyExchange.ServerCipher;
            this._clientCipher = this._keyExchange.ClientCipher;
            this._serverMac = this._keyExchange.ServerHMac;
            this._clientMac = this._keyExchange.ClientHMac;
            this._clientCompression = this._keyExchange.Compressor;
            this._serverDecompression = this._keyExchange.Decompressor;

            //  Register Connection messages
            this.RegisterMessageType<GlobalRequestMessage>(MessageTypes.GlobalRequest);
            this.RegisterMessageType<RequestSuccessMessage>(MessageTypes.RequestSuccess);
            this.RegisterMessageType<RequestFailureMessage>(MessageTypes.RequestFailure);
            this.RegisterMessageType<ChannelOpenConfirmationMessage>(MessageTypes.ChannelOpenConfirmation);
            this.RegisterMessageType<ChannelOpenFailureMessage>(MessageTypes.ChannelOpenFailure);
            this.RegisterMessageType<ChannelWindowAdjustMessage>(MessageTypes.ChannelWindowAdjust);
            this.RegisterMessageType<ChannelExtendedDataMessage>(MessageTypes.ChannelExtendedData);
            this.RegisterMessageType<ChannelRequestMessage>(MessageTypes.ChannelRequest);
            this.RegisterMessageType<ChannelSuccessMessage>(MessageTypes.ChannelSuccess);
            this.RegisterMessageType<ChannelFailureMessage>(MessageTypes.ChannelFailure);
            this.RegisterMessageType<ChannelDataMessage>(MessageTypes.ChannelData);
            this.RegisterMessageType<ChannelEofMessage>(MessageTypes.ChannelEof);
            this.RegisterMessageType<ChannelCloseMessage>(MessageTypes.ChannelClose);

            if (this.NewKeysReceived != null)
            {
                this.NewKeysReceived(this, new MessageEventArgs<NewKeysMessage>(message));
            }

            //  Signal that key exchange completed
            this._keyExchangeCompletedWaitHandle.Set();
        }

        protected virtual void OnUserAuthenticationRequestReceived(RequestMessage message)
        {
            if (this.UserAuthenticationRequestReceived != null)
            {
                this.UserAuthenticationRequestReceived(this, new MessageEventArgs<RequestMessage>(message));
            }
        }

        protected virtual void OnUserAuthenticationFailureReceived(FailureMessage message)
        {
            if (this.UserAuthenticationFailureReceived != null)
            {
                this.UserAuthenticationFailureReceived(this, new MessageEventArgs<FailureMessage>(message));
            }
        }

        protected virtual void OnUserAuthenticationSuccessReceived(SuccessMessage message)
        {
            if (this.UserAuthenticationSuccessReceived != null)
            {
                this.UserAuthenticationSuccessReceived(this, new MessageEventArgs<SuccessMessage>(message));
            }
        }

        protected virtual void OnUserAuthenticationBannerReceived(BannerMessage message)
        {
            if (this.UserAuthenticationBannerReceived != null)
            {
                this.UserAuthenticationBannerReceived(this, new MessageEventArgs<BannerMessage>(message));
            }
        }

        protected virtual void OnGlobalRequestReceived(GlobalRequestMessage message)
        {
            if (this.GlobalRequestReceived != null)
            {
                this.GlobalRequestReceived(this, new MessageEventArgs<GlobalRequestMessage>(message));
            }
        }

        protected virtual void OnRequestSuccessReceived(RequestSuccessMessage message)
        {
            if (this.RequestSuccessReceived != null)
            {
                this.RequestSuccessReceived(this, new MessageEventArgs<RequestSuccessMessage>(message));
            }
        }

        protected virtual void OnRequestFailureReceived(RequestFailureMessage message)
        {
            if (this.RequestFailureReceived != null)
            {
                this.RequestFailureReceived(this, new MessageEventArgs<RequestFailureMessage>(message));
            }
        }

        protected virtual void OnChannelOpenReceived(ChannelOpenMessage message)
        {
            if (this.ChannelOpenReceived != null)
            {
                this.ChannelOpenReceived(this, new MessageEventArgs<ChannelOpenMessage>(message));
            }
        }

        protected virtual void OnChannelOpenConfirmationReceived(ChannelOpenConfirmationMessage message)
        {
            if (this.ChannelOpenConfirmationReceived != null)
            {
                this.ChannelOpenConfirmationReceived(this, new MessageEventArgs<ChannelOpenConfirmationMessage>(message));
            }
        }

        protected virtual void OnChannelOpenFailureReceived(ChannelOpenFailureMessage message)
        {
            if (this.ChannelOpenFailureReceived != null)
            {
                this.ChannelOpenFailureReceived(this, new MessageEventArgs<ChannelOpenFailureMessage>(message));
            }
        }

        protected virtual void OnChannelWindowAdjustReceived(ChannelWindowAdjustMessage message)
        {
            if (this.ChannelWindowAdjustReceived != null)
            {
                this.ChannelWindowAdjustReceived(this, new MessageEventArgs<ChannelWindowAdjustMessage>(message));
            }
        }

        protected virtual void OnChannelDataReceived(ChannelDataMessage message)
        {
            if (this.ChannelDataReceived != null)
            {
                this.ChannelDataReceived(this, new MessageEventArgs<ChannelDataMessage>(message));
            }
        }

        protected virtual void OnChannelExtendedDataReceived(ChannelExtendedDataMessage message)
        {
            if (this.ChannelExtendedDataReceived != null)
            {
                this.ChannelExtendedDataReceived(this, new MessageEventArgs<ChannelExtendedDataMessage>(message));
            }
        }

        protected virtual void OnChannelEofReceived(ChannelEofMessage message)
        {
            if (this.ChannelEofReceived != null)
            {
                this.ChannelEofReceived(this, new MessageEventArgs<ChannelEofMessage>(message));
            }
        }

        protected virtual void OnChannelCloseReceived(ChannelCloseMessage message)
        {
            if (this.ChannelCloseReceived != null)
            {
                this.ChannelCloseReceived(this, new MessageEventArgs<ChannelCloseMessage>(message));
            }
        }

        protected virtual void OnChannelRequestReceived(ChannelRequestMessage message)
        {
            if (this.ChannelRequestReceived != null)
            {
                this.ChannelRequestReceived(this, new MessageEventArgs<ChannelRequestMessage>(message));
            }
        }

        protected virtual void OnChannelSuccessReceived(ChannelSuccessMessage message)
        {
            if (this.ChannelSuccessReceived != null)
            {
                this.ChannelSuccessReceived(this, new MessageEventArgs<ChannelSuccessMessage>(message));
            }
        }

        protected virtual void OnChannelFailureReceived(ChannelFailureMessage message)
        {
            if (this.ChannelFailureReceived != null)
            {
                this.ChannelFailureReceived(this, new MessageEventArgs<ChannelFailureMessage>(message));
            }
        }

        protected virtual void OnMessageReceived(Message message)
        {
            if (this.MessageReceived != null)
            {
                this.MessageReceived(this, new MessageEventArgs<Message>(message));
            }
        }

        #endregion

        #region Read & Write operations

        /// <summary>
        /// Reads the specified length of bytes from the server
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        private byte[] Read(int length)
        {
            var buffer = new byte[length];
            var offset = 0;
            int receivedTotal = 0;  // how many bytes is already received
            do
            {
                try
                {
                    var receivedBytes = this._socket.Receive(buffer, offset + receivedTotal, length - receivedTotal, SocketFlags.None);
                    if (receivedBytes > 0)
                    {
                        receivedTotal += receivedBytes;
                        continue;
                    }
                    else
                    {
                        throw new SshConnectionException("An established connection was aborted by the software in your host machine.", DisconnectReasons.ConnectionLost);
                    }
                }
                catch (SocketException exp)
                {
                    if (exp.SocketErrorCode == SocketError.WouldBlock ||
                        exp.SocketErrorCode == SocketError.IOPending ||
                        exp.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably empty, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                        throw;  // any serious error occurr
                }
            } while (receivedTotal < length);


            return buffer;
        }

        /// <summary>
        /// Writes the specified data to the server.
        /// </summary>
        /// <param name="data">The data.</param>
        private void Write(byte[] data)
        {
            int sent = 0;  // how many bytes is already sent
            int length = data.Length;

            do
            {
                try
                {
                    sent += this._socket.Send(data, sent, length - sent, SocketFlags.None);
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.WouldBlock ||
                        ex.SocketErrorCode == SocketError.IOPending ||
                        ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    {
                        // socket buffer is probably full, wait and try again
                        Thread.Sleep(30);
                    }
                    else
                        throw;  // any serious error occurr
                }
            } while (sent < length);

        }

        #endregion

        #region Message loading functions

        private delegate T LoadFunc<out T>(IEnumerable<byte> data);

        private IDictionary<MessageTypes, LoadFunc<Message>> _registeredMessageTypes = new Dictionary<MessageTypes, LoadFunc<Message>>();

        /// <summary>
        /// Registers the message type. This will allow message type to be recognized by and handled by the system.
        /// </summary>
        /// <remarks>Some message types are not allowed during cirtain times or same code can be used for different type of message</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageType">Type of the message.</param>
        public void RegisterMessageType<T>(MessageTypes messageType) where T : Message, new()
        {
            lock (this._registeredMessageTypes)
            {
                if (this._registeredMessageTypes.ContainsKey(messageType))
                {
                    this.UnRegisterMessageType(messageType);
                }

                this._registeredMessageTypes.Add(messageType, new LoadFunc<Message>(Message.Load<T>));
            }
        }

        public void UnRegisterMessageType(MessageTypes messageType)
        {
            this._registeredMessageTypes.Remove(messageType);
        }

        public Message LoadMessage(IEnumerable<byte> data)
        {
            var messageType = (MessageTypes)data.FirstOrDefault();

            lock (this._registeredMessageTypes)
            {
                if (this._registeredMessageTypes.ContainsKey(messageType))
                {
                    return this._registeredMessageTypes[messageType](data);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Message type '{0}' is not registered.", messageType));
                }
            }
        }

        #endregion

        /// <summary>
        /// Listnets for incoming message from the server and handles them. This method run as a task on seperate thread.
        /// </summary>
        private void MessageListener()
        {
            try
            {
                while (this._socket.Connected)
                {
                    var message = this.ReceiveMessage();

                    //Debug.WriteLine(string.Format("{0} : {1}", DateTime.Now, message));

                    if (message == null)
                    {
                        throw new NullReferenceException("The 'message' variable cannot be null");
                    }
                    else
                    {
                        this.HandleMessage((dynamic)message);
                    }
                }
            }
            catch (Exception exp)
            {
                this.RaiseError(exp);
                //  TODO:   This exception can be swolloed if it occures while running in the background, look for possible solutions
                //          It can be swolled if call async, when asynch method is finished ensure all async method check for exception to be raised
            }
        }

        internal void RaiseError(Exception exp)
        {
            var connectionException = exp as SshConnectionException;

            //  If connection exception was raised while isDisconnecting is true then this is expected
            //  case and should be ignore
            if (connectionException != null && this._isDisconnecting)
                return;

            this._exception = exp;

            this._exceptionWaitHandle.Set();

            if (this.ErrorOccured != null)
            {
                this.ErrorOccured(this, new ErrorEventArgs(exp));
            }
            var disconnectReason = DisconnectReasons.ByApplication;

            if (connectionException != null)
                disconnectReason = connectionException.DisconnectReason;

            this.SendDisconnect(disconnectReason, exp.ToString());
        }

        #region IDisposable Members

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    if (this.IsConnected)
                    {
                        this.Disconnect();
                    }

                    // Dispose managed resources.
                    if (this._socket != null)
                    {
                        this._socket.Close();
                    }

                    if (this._serviceAccepted != null)
                    {
                        this._serviceAccepted.Dispose();
                    }

                    if (this._exceptionWaitHandle != null)
                    {
                        this._exceptionWaitHandle.Dispose();
                    }

                    if (this._sessionSemaphore != null)
                    {
                        this._sessionSemaphore.Dispose();
                    }

                    if (this._keyExchangeCompletedWaitHandle != null)
                    {
                        this._keyExchangeCompletedWaitHandle.Dispose();
                    }

                    if (this._keyExchange != null)
                    {
                        this._keyExchange.Dispose();
                    }
                }

                // Note disposing has been done.
                this._disposed = true;
            }
        }

        ~Session()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
