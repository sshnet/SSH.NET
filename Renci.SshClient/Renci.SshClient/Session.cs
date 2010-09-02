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
        /// WaitHandle to signal that key exchange was finished, weither it was succesfull or not.
        /// </summary>
        private EventWaitHandle _keyExhangedFinishedWaitHandle;

        /// <summary>
        /// WaitHandle to signale that last service request was accepted
        /// </summary>
        private EventWaitHandle _serviceAccepted;

        /// <summary>
        /// WaitHandle to signal that exception was thrown by another thread.
        /// </summary>
        private EventWaitHandle _exceptionWaitHandle;

        /// <summary>
        /// WaitHandle to signal that listner ended
        /// </summary>
        private EventWaitHandle _listenerWaitHandle;

        /// <summary>
        /// Keeps track of all open channels
        /// </summary>
        private Dictionary<uint, Channel> _openChannels;

        /// <summary>
        /// Exception that need to be thrown by waiting thread
        /// </summary>
        private Exception _exceptionToThrow;

        /// <summary>
        /// Specifies weither connection is authenticated
        /// </summary>
        private bool _isAuthenticated;

        /// <summary>
        /// Specifies weither Disconnect method was called
        /// </summary>
        private bool _isDisconnecting;

        /// <summary>
        /// holds number to be used for session channels
        /// </summary>
        private BlockingStack<uint> _channelNumbers;

        /// <summary>
        /// Gets or sets the HMAC algorithm to use when receiving message from the server.
        /// </summary>
        /// <value>The server mac.</value>
        private HMAC _serverMac;

        /// <summary>
        /// Gets or sets the HMAC algorithm to use when sending message to the server.
        /// </summary>
        /// <value>The client mac.</value>
        private HMAC _clientMac;

        /// <summary>
        /// Gets or sets the client cipher which used to encrypt messages sent to server.
        /// </summary>
        /// <value>The client cipher.</value>
        private Cipher _clientCipher;

        /// <summary>
        /// Gets or sets the server cipher which used to decrypt messages sent by server.
        /// </summary>
        /// <value>The server cipher.</value>
        private Cipher _serverCipher;

        /// <summary>
        /// Gets or sets the compression algorithm to use when receiving message from the server.
        /// </summary>
        /// <value>The server decompression.</value>
        private Compression _serverDecompression;

        /// <summary>
        /// Gets or sets the compression algorithm to use when sending message to the server.
        /// </summary>
        /// <value>The client compression.</value>
        private Compression _clientCompression;

        /// <summary>
        /// Gets a value indicating whether socket connected.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if socket connected; otherwise, <c>false</c>.
        /// </value>
        protected bool IsConnected
        {
            get
            {
                return this._socket != null && this._socket.Connected && this._isAuthenticated;
            }
        }

        /// <summary>
        /// Occurs when new message received.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Gets or sets the session id.
        /// </summary>
        /// <value>The session id.</value>
        public IEnumerable<byte> SessionId { get; private set; }

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

        internal Session(ConnectionInfo connectionInfo)
        {
            this.ConnectionInfo = connectionInfo;
            this.ClientVersion = string.Format("SSH-2.0-Renci.SshClient.{0}", this.GetType().Assembly.GetName().Version);
        }

        private static IDictionary<Type, Func<Session, uint, Channel>> _channels = new Dictionary<Type, Func<Session, uint, Channel>>()
            {
                {typeof(ChannelExec), (session, channelId) => { return new ChannelExec(session, channelId);}},
                {typeof(ChannelSftp), (session, channelId) => { return new ChannelSftp(session, channelId);}}
            };

        /// <summary>
        /// Creates the new channel.
        /// </summary>
        /// <typeparam name="T">Type of channel to create.</typeparam>
        /// <returns>New channel of specified type</returns>
        public T CreateChannel<T>() where T : Channel
        {
            if (!this.IsConnected)
            {
                throw new InvalidOperationException("Not connected");
            }

            T channel;
            //  Gets newxt available channel number or waits for one to become available
            var clientChannelId = this._channelNumbers.WaitAndPop();

            lock (this._openChannels)
            {
                channel = _channels[typeof(T)](this, clientChannelId) as T;

                channel.Closed += Channel_Closed;
                channel.OpenFailed += Channel_Closed;

                this._openChannels.Add(clientChannelId, channel);
            }

            return channel;
        }

        public void Connect()
        {
            this.Connect(this.ConnectionInfo);
        }

        /// <summary>
        /// Connects to the server.
        /// </summary>
        public void Connect(ConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
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

                    this.Initialize();

                    //  Prepopulate chanel numbers that will be used
                    for (int i = connectionInfo.MaxSessions - 1; i > 0; i--)
                    {
                        this._channelNumbers.Push((uint)i - 1);
                    }


                    var ep = new IPEndPoint(Dns.GetHostAddresses(connectionInfo.Host)[0], connectionInfo.Port);
                    this._socket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                    //  Connect socket with 5 seconds timeout
                    var connectResult = this._socket.BeginConnect(ep, null, null);

                    connectResult.AsyncWaitHandle.WaitOne(connectionInfo.Timeout);

                    this._socket.EndConnect(connectResult);

                    this._socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, 1);

                    Match versionMatch = null;
                    //  Get server version from the server,
                    //  ignore text lines which are sent before if any
                    using (var ns = new NetworkStream(this._socket))
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

                    //  Get server SSH version
                    var version = versionMatch.Result("${protoversion}");

                    if (!version.Equals("2.0"))
                    {
                        throw new NotSupportedException(string.Format("Server version '{0}' is not supported.", version));
                    }

                    this.Write(Encoding.ASCII.GetBytes(string.Format("{0}\n", this.ClientVersion)));

                    //  Register Transport response messages
                    this.RegisterMessageType<DisconnectMessage>(MessageTypes.Disconnect);
                    this.RegisterMessageType<IgnoreMessage>(MessageTypes.Ignore);
                    this.RegisterMessageType<UnimplementedMessage>(MessageTypes.Unimplemented);
                    this.RegisterMessageType<DebugMessage>(MessageTypes.Debug);
                    this.RegisterMessageType<ServiceAcceptMessage>(MessageTypes.ServiceAcceptRequest);
                    this.RegisterMessageType<KeyExchangeInitMessage>(MessageTypes.KeyExchangeInit);
                    this.RegisterMessageType<NewKeysMessage>(MessageTypes.NewKeys);

                    //  Start incoming request listener
                    this._messageListener = Task.Factory.StartNew(() => { this.MessageListener(); });

                    //  Wait for key exchange to be completed
                    this.WaitHandle(this._keyExhangedFinishedWaitHandle);

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

            this.Disconnect(DisconnectReasonCodes.ByApplication, "Connection terminated by the client.");

            this.DisconnectCleanup();
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
            if (this._exceptionToThrow != null)
            {
                var exception = this._exceptionToThrow;
                this._exceptionToThrow = null;
                throw exception;
            }
            else if (index > waitHandles.Length)
            {
                this.Disconnect(DisconnectReasonCodes.ByApplication, "Operation timeout");
                throw new TimeoutException("Operation timeout");
            }
        }

        /// <summary>
        /// Sends packet message to the server.
        /// </summary>
        /// <param name="message">The message.</param>
        internal virtual void SendMessage(Message message)
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

            //  TOOO:   If compression specified then compress only payload (messageData)

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
        protected virtual Message ReceiveMessage()
        {
            if (!this._socket.Connected)
                return null;

            //  No lock needed since all messages read by only one thread

            List<byte> decryptedData;

            var blockSize = this._serverCipher == null ? (byte)8 : (byte)this._serverCipher.BlockSize;

            //  Read packet lenght first
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
                throw new IOException(string.Format("Bad packet length {0}", packetLength));

            //  Read rest of the packet data
            int bytesToRead = (int)(packetLength - (blockSize - 4));

            if (this._serverCipher == null)
            {
                decryptedData.AddRange(this.Read(bytesToRead));
            }
            else
            {
                decryptedData.AddRange(this._serverCipher.Decrypt(this.Read(bytesToRead)));
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
                    throw new IOException("MAC error");
                }
            }

            //  TODO:   Issue new keys after x number of packets
            this._inboundPacketSequence++;

            var paddingLength = decryptedData[4];

            //  TODO:   Decrypt message payload

            var payload = decryptedData.Skip(5).Take((int)(packetLength - paddingLength - 1));

            return this.LoadMessage(payload);
        }

        /// <summary>
        /// Handles the message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message.</param>
        protected virtual void HandleMessage<T>(T message) where T : Message
        {
            //  Do nothing as message could be proccessed by other module
            if (message is ChannelMessage)
            {
                this.HandleMessage(message as ChannelMessage);
            }
        }

        #region Handle transport messages

        protected virtual void HandleMessage(DisconnectMessage message)
        {
            this.DisconnectCleanup();
        }

        protected virtual void HandleMessage(IgnoreMessage message)
        {
            throw new NotImplementedException();
        }

        protected virtual void HandleMessage(UnimplementedMessage message)
        {
            throw new NotImplementedException();
        }

        protected virtual void HandleMessage(DebugMessage message)
        {
            throw new NotImplementedException();
        }

        protected virtual void HandleMessage(ServiceAcceptMessage message)
        {
            this._serviceAccepted.Set();
        }

        protected virtual void HandleMessage(ServiceRequestMessage message)
        {
            throw new NotImplementedException();
        }

        protected virtual void HandleMessage(KeyExchangeInitMessage message)
        {
            this._keyExhangedFinishedWaitHandle.Reset();

            if (message.FirstKexPacketFollows)
            {
                //  TODO:   Expect guess packet
                throw new NotImplementedException("Guess packets are not supported.");
            }

            //  Create key exchange algorithm
            this._keyExhcange = KeyExchange.Create(message, this);

            this._keyExhcange.Failed += delegate(object sender, KeyExchangeFailedEventArgs e)
            {
                this.Disconnect(DisconnectReasonCodes.KeyExchangeFailed, e.Message);
                throw new InvalidOperationException(e.Message);
            };

            this._keyExhcange.Start(message);
        }

        protected virtual void HandleMessage(NewKeysMessage message)
        {
            this._keyExhcange.Finish();

            this.SessionId = this._keyExhcange.SessionId;
            //  Update encryption and decryption algorithm
            this._serverMac = this._keyExhcange.ServerMac;
            this._clientMac = this._keyExhcange.ClientMac;
            this._clientCipher = this._keyExhcange.ClientCipher;
            this._serverCipher = this._keyExhcange.ServerCipher;
            this._serverDecompression = this._keyExhcange.ServerDecompression;
            this._clientCompression = this._keyExhcange.ClientCompression;

            this._keyExhangedFinishedWaitHandle.Set();
        }

        #endregion

        #region Handle connection messages

        protected virtual void HandleMessage(GlobalRequestMessage message)
        {
            //  TODO:   Add implemention for this message
        }

        protected virtual void HandleMessage(RequestSuccessMessage message)
        {
            //  TODO:   Add implemention for this message
        }

        protected virtual void HandleMessage(RequestFailureMessage message)
        {
            //  TODO:   Add implemention for this message
        }

        #endregion

        #region Handle channel messages

        protected virtual void HandleMessage(ChannelMessage message)
        {
            var channel = this._openChannels[message.ChannelNumber];
            channel.HandleChannelMessage(message);
        }

        private void Channel_Closed(object sender, ChannelEventArgs e)
        {
            lock (this._openChannels)
            {
                this._openChannels.Remove(e.ChannelNumber);
                this._channelNumbers.Push(e.ChannelNumber);
            }
        }

        #endregion

        /// <summary>
        /// Disconnects client from the server with specified reason code.
        /// </summary>
        /// <param name="reasonCode">The reason code.</param>
        /// <param name="message">The message.</param>
        protected void Disconnect(DisconnectReasonCodes reasonCode, string message)
        {
            if (this.IsConnected)
            {
                this.SendMessage(new DisconnectMessage
                {
                    ReasonCode = reasonCode,
                    Description = message,
                });

                this.DisconnectCleanup();
            }
        }

        /// <summary>
        /// Reads the specified length of bytes from the server
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        protected byte[] Read(int length)
        {
            var buffer = new byte[length];
            var offset = 0;
            int received = 0;  // how many bytes is already received
            do
            {
                try
                {
                    received += this._socket.Receive(buffer, offset + received, length - received, SocketFlags.None);
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
                        throw exp;  // any serious error occurr
                }
            } while (received < length);


            return buffer;
        }

        /// <summary>
        /// Writes the specified data to the server.
        /// </summary>
        /// <param name="data">The data.</param>
        protected void Write(byte[] data)
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
                        throw ex;  // any serious error occurr
                }
            } while (sent < length);

        }

        /// <summary>
        /// Initiates new key request by the client
        /// </summary>
        protected void RequestNewKeys()
        {
            //  TODO:   Create method to issue new keys when required
            //this._keyExhcange.Start();
        }

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
        /// Initializes this session.
        /// </summary>
        /// <remarks>Initialization required when same session object being reconnected</remarks>
        private void Initialize()
        {
            //  TODO:   Consider reseting objects to initial stage instead of creating a new one
            //  Initialize session
            this._outboundPacketSequence = 0;
            this._inboundPacketSequence = 0;
            this._openChannels = new Dictionary<uint, Channel>();
            this._keyExhangedFinishedWaitHandle = new AutoResetEvent(false);
            this._serviceAccepted = new AutoResetEvent(false);
            this._exceptionWaitHandle = new AutoResetEvent(false);
            this._listenerWaitHandle = new AutoResetEvent(false);
            this._channelNumbers = new BlockingStack<uint>();
            this._exceptionToThrow = null;
            this.SessionId = null;
            this.ServerVersion = null;
            this._keyExhcange = null;
            this._serverMac = null;
            this._clientMac = null;
            this._clientCipher = null;
            this._serverCipher = null;
            this._serverDecompression = null;
            this._clientCompression = null;
            this._isAuthenticated = false;
            this._isDisconnecting = false;
        }

        private bool ValidateHash(List<byte> decryptedData, byte[] serverHash, uint packetSequence)
        {
            var clientHashData = new List<byte>();
            clientHashData.AddRange(BitConverter.GetBytes(packetSequence).Reverse());
            clientHashData.AddRange(decryptedData);

            var clientHash = this._serverMac.ComputeHash(clientHashData.ToArray());
            return serverHash.SequenceEqual(clientHash);
        }

        /// <summary>
        /// Perfom neccesary cleanup when client disconects from the server
        /// </summary>
        private void DisconnectCleanup()
        {
            //  Close all open channels if any
            if (this.IsConnected)
            {
                foreach (var channel in this._openChannels.Values)
                {
                    channel.Close();
                }
            }

            //  Close socket connection if still open
            if (this._socket != null)
            {
                this._socket.Close();
            }

            if (this._messageListener != null)
            {
                //  Wait for listner task to finish
                this._messageListener.Wait();
                this._messageListener = null;
            }
        }

        /// <summary>
        /// Listnets for incoming message from the server and handles them. This method run as a task on seperate thread.
        /// </summary>
        private void MessageListener()
        {
            try
            {
                while (this._socket.Connected)
                {
                    dynamic message = this.ReceiveMessage();

                    if (message == null)
                    {
                        throw new NullReferenceException("The 'message' variable cannot be null");
                    }

                    //  Handle session messages first
                    this.HandleMessage(message);

                    //  Raise an event that message received
                    this.RaiseMessageReceived(this, new MessageReceivedEventArgs(message));
                }
            }
            catch (Exception exp)
            {
                //  TODO:   This exception can be swolloed if it occures while running in the background, look for possible solutions

                //  Ignore this error since socket was disconected
                if (exp is SocketException && ((SocketException)exp).SocketErrorCode == SocketError.ConnectionAborted && this._isDisconnecting)
                {
                    //  Do nothing since connection was disconnected by the client
                }
                else
                {
                    //  In case of error issue disconntect command
                    this.Disconnect(DisconnectReasonCodes.ByApplication, exp.ToString());

                    this._exceptionToThrow = exp;

                    this._exceptionWaitHandle.Set();
                }

                //  Ensure socket is disconnected
                this._socket.Close();
            }

            this._listenerWaitHandle.Set();
        }

        /// <summary>
        /// Raises the MessageReceived event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="Renci.SshClient.Common.MessageReceivedEventArgs"/> instance containing the event data.</param>
        private void RaiseMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            if (this.MessageReceived != null)
            {
                this.MessageReceived(sender, args);
            }
        }

        #region IDisposable Members

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this._socket != null)
                    {
                        this.Disconnect();
                        this._socket.Dispose();
                    }

                    if (this._keyExhangedFinishedWaitHandle != null)
                    {
                        this._keyExhangedFinishedWaitHandle.Dispose();
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
                }

                // Note disposing has been done.
                disposed = true;
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
