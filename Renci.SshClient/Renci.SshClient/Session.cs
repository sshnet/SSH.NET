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
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Authentication;
using Renci.SshClient.Messages.Connection;
using Renci.SshClient.Messages.Transport;
using Renci.SshClient.Security;

namespace Renci.SshClient
{
    internal class Session : IDisposable
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
        /// Holds reference to key exchange algorithm being used by this connection
        /// </summary>
        private KeyExchange _keyExhcange;

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

        /// <summary>
        /// WaitHandle to signal that listner ended
        /// </summary>
        private EventWaitHandle _listenerWaitHandle = new AutoResetEvent(false);

        /// <summary>
        /// Exception that need to be thrown by waiting thread
        /// </summary>
        private Exception _exceptionToThrow;

        /// <summary>
        /// Specifies weither connection is authenticated
        /// </summary>
        private bool _isAuthenticated;

        /// <summary>
        /// Specifies whether user issued Disconnect command or not
        /// </summary>
        private bool _isDisconnecting;

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
        public IEnumerable<byte> SessionId
        {
            get
            {
                return this._keyExhcange.SessionId;
            }
        }

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
                        KeyExchangeAlgorithms = Settings.KeyExchangeAlgorithms.Keys,
                        ServerHostKeyAlgorithms = Settings.HostKeyAlgorithms.Keys,
                        EncryptionAlgorithmsClientToServer = Settings.Encryptions.Keys,
                        EncryptionAlgorithmsServerToClient = Settings.Encryptions.Keys,
                        MacAlgorithmsClientToSserver = Settings.HmacAlgorithms.Keys,
                        MacAlgorithmsServerToClient = Settings.HmacAlgorithms.Keys,
                        CompressionAlgorithmsClientToServer = Settings.CompressionAlgorithms.Keys,
                        CompressionAlgorithmsServerToClient = Settings.CompressionAlgorithms.Keys,
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

        #region Message events

        /// <summary>
        /// Occurs when <see cref="DisconnectMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<DisconnectMessage>> DisconnectReceived;

        /// <summary>
        /// Occurs when <see cref="IgnoreMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<IgnoreMessage>> IgnoreReceived;

        /// <summary>
        /// Occurs when <see cref="UnimplementedMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<UnimplementedMessage>> UnimplementedReceived;

        /// <summary>
        /// Occurs when <see cref="DebugMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<DebugMessage>> DebugReceived;

        /// <summary>
        /// Occurs when <see cref="ServiceRequestMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<ServiceRequestMessage>> ServiceRequestReceived;

        /// <summary>
        /// Occurs when <see cref="ServiceAcceptMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<ServiceAcceptMessage>> ServiceAcceptReceived;

        /// <summary>
        /// Occurs when <see cref="KeyExchangeInitMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<KeyExchangeInitMessage>> KeyExchangeInitReceived;

        /// <summary>
        /// Occurs when <see cref="NewKeysMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<NewKeysMessage>> NewKeysReceived;

        /// <summary>
        /// Occurs when <see cref="RequestMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<RequestMessage>> UserAuthenticationRequestReceived;

        /// <summary>
        /// Occurs when <see cref="FailureMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<FailureMessage>> UserAuthenticationFailureReceived;

        /// <summary>
        /// Occurs when <see cref="SuccessMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<SuccessMessage>> UserAuthenticationSuccessReceived;

        /// <summary>
        /// Occurs when <see cref="BannerMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<BannerMessage>> UserAuthenticationBannerReceived;

        /// <summary>
        /// Occurs when <see cref="GlobalRequestMessage"/> message received
        /// </summary>        
        public event EventHandler<MessageEventArgs<GlobalRequestMessage>> GlobalRequestReceived;

        /// <summary>
        /// Occurs when <see cref="RequestSuccessMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<RequestSuccessMessage>> RequestSuccessReceived;

        /// <summary>
        /// Occurs when <see cref="RequestFailureMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<RequestFailureMessage>> RequestFailureReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelOpenMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<ChannelOpenMessage>> ChannelOpenReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelOpenConfirmationMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<ChannelOpenConfirmationMessage>> ChannelOpenConfirmationReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelOpenFailureMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<ChannelOpenFailureMessage>> ChannelOpenFailureReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelWindowAdjustMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<ChannelWindowAdjustMessage>> ChannelWindowAdjustReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelDataMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<ChannelDataMessage>> ChannelDataReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelExtendedDataMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<ChannelExtendedDataMessage>> ChannelExtendedDataReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelEofMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<ChannelEofMessage>> ChannelEofReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelCloseMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<ChannelCloseMessage>> ChannelCloseReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelRequestMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<ChannelRequestMessage>> ChannelRequestReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelSuccessMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<ChannelSuccessMessage>> ChannelSuccessReceived;

        /// <summary>
        /// Occurs when <see cref="ChannelFailureMessage"/> message received
        /// </summary>
        public event EventHandler<MessageEventArgs<ChannelFailureMessage>> ChannelFailureReceived;

        /// <summary>
        /// Occurs when message received and is not handled by any of the event handlers
        /// </summary>
        public event EventHandler<MessageEventArgs<Message>> MessageReceived;

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
                    //  If connected dont connect again
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
                                versionMatch = _serverVersionRe.Match(this.ServerVersion);
                                if (string.IsNullOrEmpty(this.ServerVersion))
                                {
                                    throw new InvalidOperationException("Server string is null or empty.");
                                }
                                else if (versionMatch.Success)
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
                        throw new SshException(string.Format("Server version '{0}' is not supported.", version), DisconnectReasons.ProtocolVersionNotSupported);
                    }

                    if (version.Equals("1.99"))
                    {
                        this.Write(Encoding.ASCII.GetBytes(string.Format("{0}", this.ClientVersion)));
                    }
                    else
                    {
                        this.Write(Encoding.ASCII.GetBytes(string.Format("{0}\n", this.ClientVersion)));
                    }

                    //  Register Transport response messages
                    this.RegisterMessageType<DisconnectMessage>(MessageTypes.Disconnect);
                    this.RegisterMessageType<IgnoreMessage>(MessageTypes.Ignore);
                    this.RegisterMessageType<UnimplementedMessage>(MessageTypes.Unimplemented);
                    this.RegisterMessageType<DebugMessage>(MessageTypes.Debug);
                    this.RegisterMessageType<ServiceAcceptMessage>(MessageTypes.ServiceAcceptRequest);
                    this.RegisterMessageType<KeyExchangeInitMessage>(MessageTypes.KeyExchangeInit);
                    this.RegisterMessageType<NewKeysMessage>(MessageTypes.NewKeys);


                    this._keyExhcange = new KeyExchange(this);

                    //  Start incoming request listener
                    this._messageListener = Task.Factory.StartNew(() => { this.MessageListener(); });

                    //  Wait for key exchange to be completed
                    this.WaitHandle(this._keyExhcange.WaitHandle);

                    //  If sessionId is not set then its not connected
                    if (this.SessionId == null)
                    {
                        this.Disconnect();
                        return;
                    }

                    //  Request user authorization service
                    this.SendMessage(new ServiceRequestMessage
                    {
                        ServiceName = ServiceNames.UserAuthentication,
                    });

                    //  Wait for service to be accepted
                    this.WaitHandle(this._serviceAccepted);

                    //  This implemention will ignore supported by server methods and will try to authenticated user using method supported by the client.
                    string errorMessage = null; //  Hold last authentication error if any
                    foreach (var methodName in Settings.SupportedAuthenticationMethods.Keys)
                    {
                        var userAuthentication = Settings.SupportedAuthenticationMethods[methodName](this);

                        if (userAuthentication.Execute())
                        {
                            if (userAuthentication.IsAuthenticated)
                            {
                                this._isAuthenticated = true;
                                break;
                            }
                            else
                            {
                                errorMessage = userAuthentication.ErrorMessage;
                            }
                        }
                    }

                    if (!this._isAuthenticated)
                    {
                        throw new AuthenticationException(errorMessage ?? "User cannot be authenticated.");
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

            this.SendDisconnect(DisconnectReasons.ByApplication, "Connection terminated by the client.");
        }

        public T CreateChannel<T>() where T : Channel, new()
        {
            return CreateChannel<T>(0, 0x100000, 0x8000);
        }

        public T CreateChannel<T>(uint serverChannelNumber, uint windowSize, uint packetSize) where T : Channel, new()
        {
            T channel = new T();
            lock (this)
            {
                channel.Initialize(this, serverChannelNumber, windowSize, packetSize);
            }
            return channel;
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
                    this._listenerWaitHandle,   //  When listener exits
                    waitHandle,
                };

            var index = 0;
#if NOTIMEOUT
            index = EventWaitHandle.WaitAny(waitHandles);
#else
            index = EventWaitHandle.WaitAny(waitHandles, this.ConnectionInfo.Timeout);
#endif
            if (index == 0 && this._exceptionToThrow != null)
            {
                var exception = this._exceptionToThrow;
                this._exceptionToThrow = null;
                throw exception;
            }
            else if (index == 1)
            {
                throw new SshException("Connection was terminated.");
            }
            else if (index > waitHandles.Length)
            {
                this.SendDisconnect(DisconnectReasons.ByApplication, "Operation timeout");
                throw new TimeoutException("Operation timeout");
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
            var paddingMultiplier = this._keyExhcange.ClientCipher == null ? (byte)8 : (byte)this._keyExhcange.ClientCipher.BlockSize;    //    Should be recalculate base on cipher min lenght if sipher specified

            var messageData = message.GetBytes();

            if (messageData.Count() > Session.MAXIMUM_PAYLOAD_SIZE)
            {
                throw new InvalidOperationException(string.Format("Payload cannot be more then {0} bytes.", Session.MAXIMUM_PAYLOAD_SIZE));
            }

            if (this._keyExhcange.ClientCompression != null)
            {
                messageData = this._keyExhcange.ClientCompression.Compress(messageData);
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
                if (this._keyExhcange.ClientCipher != null)
                {
                    encryptedData = new List<byte>(this._keyExhcange.ClientCipher.Encrypt(packetData));
                }

                //  Add message authentication code (MAC)
                if (this._keyExhcange.ClientMac != null)
                {
                    var hash = this._keyExhcange.ClientMac.ComputeHash(hashData.ToArray());

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

            var blockSize = this._keyExhcange.ServerCipher == null ? (byte)8 : (byte)this._keyExhcange.ServerCipher.BlockSize;

            //  Read packet lenght first
            var data = new List<byte>(this.Read(blockSize));

            if (this._keyExhcange.ServerCipher == null)
            {
                decryptedData = data.ToList();
            }
            else
            {
                decryptedData = new List<byte>(this._keyExhcange.ServerCipher.Decrypt(data));
            }

            var packetLength = (uint)(decryptedData[0] << 24 | decryptedData[1] << 16 | decryptedData[2] << 8 | decryptedData[3]);

            //  Test packet minimum and maximum boundaries
            if (packetLength < Math.Max((byte)16, blockSize) - 4 || packetLength > Session.MAXIMUM_PACKET_SIZE - 4)
                throw new SshException(string.Format("Bad packet length {0}", packetLength), DisconnectReasons.ProtocolError);

            //  Read rest of the packet data
            int bytesToRead = (int)(packetLength - (blockSize - 4));

            if (bytesToRead > 0)
            {
                if (this._keyExhcange.ServerCipher == null)
                {
                    decryptedData.AddRange(this.Read(bytesToRead));
                }
                else
                {
                    decryptedData.AddRange(this._keyExhcange.ServerCipher.Decrypt(this.Read(bytesToRead)));
                }
            }

            var paddingLength = decryptedData[4];

            var messagePayload = decryptedData.Skip(5).Take((int)(packetLength - paddingLength - 1));

            if (this._keyExhcange.ServerDecompression != null)
            {
                messagePayload = new List<byte>(this._keyExhcange.ServerDecompression.Uncompress(messagePayload));
            }

            //  Validate message against MAC            
            if (this._keyExhcange.ServerMac != null)
            {
                var serverHash = this.Read(this._keyExhcange.ServerMac.HashSize / 8);

                var clientHashData = new List<byte>();
                clientHashData.AddRange(BitConverter.GetBytes(this._inboundPacketSequence).Reverse());
                clientHashData.AddRange(decryptedData);

                //  Calculate packet hash
                var clientHash = this._keyExhcange.ServerMac.ComputeHash(clientHashData.ToArray());

                if (!serverHash.SequenceEqual(clientHash))
                {
                    throw new SshException("MAC error", DisconnectReasons.MacError);
                }
            }

            this._inboundPacketSequence++;

            return this.LoadMessage(messagePayload);
        }

        private void SendDisconnect(DisconnectReasons reasonCode, string message)
        {
            var disconnectMessage = new DisconnectMessage
            {
                ReasonCode = reasonCode,
                Description = message,
            };

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

            this._socket.Disconnect(true);

            this._socket.Close();

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
            this._keyExhcange.HandleMessage(message);

            if (this.KeyExchangeInitReceived != null)
            {
                this.KeyExchangeInitReceived(this, new MessageEventArgs<KeyExchangeInitMessage>(message));
            }
        }

        protected virtual void OnNewKeysReceived(NewKeysMessage message)
        {
            if (this.NewKeysReceived != null)
            {
                this.NewKeysReceived(this, new MessageEventArgs<NewKeysMessage>(message));
            }
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
                        throw new SshException("An established connection was aborted by the software in your host machine.", DisconnectReasons.ConnectionLost);
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
            while (true)
            {
                try
                {
                    var message = this.ReceiveMessage();

                    //Debug.WriteLine(message);

                    if (message == null)
                    {
                        throw new NullReferenceException("The 'message' variable cannot be null");
                    }
                    else if (this._keyExhcange.InProgress)
                    {
                        this._keyExhcange.HandleMessage((dynamic)message);
                        continue;   //  Get next message, all non kexinit messages should be ignored
                    }
                    else
                    {
                        //  TODO:   Handle each message on induvidual thread
                        //Task.Factory.StartNew(() =>
                        //{
                        //    try
                        //    {
                        //  Handle message
                        this.HandleMessage((dynamic)message);
                        //    }
                        //    catch (Exception exp)
                        //    {
                        //        throw;
                        //    }
                        //});
                    }
                }
                catch (SshException exp)
                {
                    if (exp.DisconnectReason == DisconnectReasons.ConnectionLost && this._isDisconnecting)
                    {
                        //  Ignore this error since we called disconnect command and this error is expected
                        break;
                    }
                    else if (exp.DisconnectReason != DisconnectReasons.None)
                    {
                        this.SendDisconnect(exp.DisconnectReason, exp.ToString());
                    }

                    this._exceptionToThrow = exp;

                    this._exceptionWaitHandle.Set();
                }
                catch (Exception exp)
                {
                    //  TODO:   This exception can be swolloed if it occures while running in the background, look for possible solutions

                    this.SendDisconnect(DisconnectReasons.ByApplication, exp.ToString());

                    this._exceptionToThrow = exp;

                    this._exceptionWaitHandle.Set();
                }
            }

            this._listenerWaitHandle.Set();
        }

        #region IDisposable Members

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
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

                    if (this._listenerWaitHandle != null)
                    {
                        this._listenerWaitHandle.Dispose();
                    }

                    if (this._keyExhcange != null)
                    {
                        this._keyExhcange.Dispose();
                    }

                    if (this._sessionSemaphore != null)
                    {
                        this._sessionSemaphore.Dispose();
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
