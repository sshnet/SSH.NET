using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
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
    internal abstract class Session : IDisposable
    {
        protected const int MAXIMUM_PACKET_SIZE = 35000;

        /// <summary>
        /// Creates new session using connection informaiton.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <returns>New version specific session.</returns>
        public static Session CreateSession(ConnectionInfo connectionInfo)
        {
            var ep = new IPEndPoint(Dns.GetHostAddresses(connectionInfo.Host)[0], connectionInfo.Port);
            var socket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            socket.ExclusiveAddressUse = true;

            //  Connect socket with 5 seconds timeout
            var connectResult = socket.BeginConnect(ep, null, null);

            connectResult.AsyncWaitHandle.WaitOne(connectionInfo.Timeout);

            socket.EndConnect(connectResult);

            //  Get server version from the server,
            //  ignore text lines which are sent before if any
            var serverVersion = string.Empty;

            using (StreamReader sr = new StreamReader(new NetworkStream(socket)))
            {
                while (true)
                {
                    serverVersion = sr.ReadLine();
                    if (string.IsNullOrEmpty(serverVersion))
                    {
                        continue;
                    }
                    else if (serverVersion.StartsWith("SSH"))
                    {
                        break;
                    }
                }
            }

            //  TODO:   Create session based on server version
            var session = new SessionSSHv2(connectionInfo, socket, serverVersion);

            return session;
        }

        /// <summary>
        /// Holds connection socket.
        /// </summary>
        private Socket _socket;

        /// <summary>
        /// Holds connection socket stream to communicate with the server
        /// </summary>
        private NetworkStream _socketStream;

        /// <summary>
        /// Holds reference to key exchange algorithm being used by this connection
        /// </summary>
        private KeyExchange _keyExhcange;

        /// <summary>
        /// Holds reference to task that listnes for incoming messages
        /// </summary>
        private Task _messageListener;

        /// <summary>
        /// WaitHandle to signal that key exchange was finished, weither it was succesfull or not.
        /// </summary>
        private EventWaitHandle _keyExhangedFinishedWaitHandle = new AutoResetEvent(false);

        /// <summary>
        /// WaitHandle to signale that last service request was accepted
        /// </summary>
        private EventWaitHandle _serviceAccepted = new AutoResetEvent(false);

        /// <summary>
        /// WaitHandle to signal that exception was thrown by another thread.
        /// </summary>
        private EventWaitHandle _exceptionWaitHandle = new AutoResetEvent(false);

        /// <summary>
        /// Keeps track of all open channels
        /// </summary>
        private IDictionary<uint, uint> _openChannels = new Dictionary<uint, uint>();

        /// <summary>
        /// Exception that need to be thrown by waiting thread
        /// </summary>
        private Exception _exceptionToThrow;

        /// <summary>
        /// Specifies weither connection is authenticated
        /// </summary>
        private bool _isAuthenticated;

        /// <summary>
        /// Occurs when new message received.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Gets or sets the HMAC algorithm to use when receiving message from the server.
        /// </summary>
        /// <value>The server mac.</value>
        protected HMAC ServerMac { get; private set; }

        /// <summary>
        /// Gets or sets the HMAC algorithm to use when sending message to the server.
        /// </summary>
        /// <value>The client mac.</value>
        protected HMAC ClientMac { get; private set; }

        /// <summary>
        /// Gets or sets the client cipher which used to encrypt messages sent to server.
        /// </summary>
        /// <value>The client cipher.</value>
        protected Cipher ClientCipher { get; private set; }

        /// <summary>
        /// Gets or sets the server cipher which used to decrypt messages sent by server.
        /// </summary>
        /// <value>The server cipher.</value>
        protected Cipher ServerCipher { get; private set; }

        /// <summary>
        /// Gets or sets the compression algorithm to use when receiving message from the server.
        /// </summary>
        /// <value>The server decompression.</value>
        protected Compression ServerDecompression { get; private set; }

        /// <summary>
        /// Gets or sets the compression algorithm to use when sending message to the server.
        /// </summary>
        /// <value>The client compression.</value>
        protected Compression ClientCompression { get; private set; }

        /// <summary>
        /// Gets a value indicating whether socket connected.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if socket connected; otherwise, <c>false</c>.
        /// </value>
        protected bool IsConnected { get; private set; }

        //  TODO:   Consider refactor to make setter private
        public IEnumerable<byte> SessionId { get; set; }

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
        /// Initializes a new instance of the <see cref="Session"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="socket">The socket.</param>
        /// <param name="serverVersion">The server version.</param>
        protected Session(ConnectionInfo connectionInfo, Socket socket, string serverVersion)
        {
            this.ConnectionInfo = connectionInfo;
            this._socket = socket;
            this._socket.NoDelay = true;
            this._socket.Blocking = true;

            this.ServerVersion = serverVersion;
            this.ClientVersion = string.Format("SSH-2.0-Renci.SshClient.{0}", this.GetType().Assembly.GetName().Version);

            this._socketStream = new NetworkStream(socket);
        }

        private static IDictionary<Type, Func<Session, Channel>> _channels = new Dictionary<Type, Func<Session, Channel>>()
            {
                {typeof(ChannelExec), (session) => { return new ChannelExec(session);}},
                {typeof(ChannelSftp), (session) => { return new ChannelSftp(session);}}
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
            return _channels[typeof(T)](this) as T;
        }

        /// <summary>
        /// Connects to the server.
        /// </summary>
        public void Connect()
        {
            lock (this._socket)
            {
                //  If connected dont connect again
                if (this.IsConnected)
                    return;

                this.IsConnected = true;

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
            }
        }

        /// <summary>
        /// Disconnects from the server
        /// </summary>
        public void Disconnect()
        {
            //  TODO:   Change message to something more appropriate
            this.Disconnect(DisconnectReasonCodes.ByApplication, "Connection terminated by the client.");

            this.DisconnectCleanup();
        }

        /// <summary>
        /// Sends packet message to the server.
        /// </summary>
        /// <param name="message">The message.</param>
        internal abstract void SendMessage(Message message);

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

            //  TODO:   Signal waitandle to stop waiting, in case someone waiting for it.
            //  TODO:   throw exception in case of _disconnectWaitHandle or _exceptionWaitHandle was signalled

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
                //  TODO:   Issue timeout disconnect message if approapriate
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Receives the message from the server.
        /// </summary>
        /// <returns></returns>
        protected abstract Message ReceiveMessage();

        /// <summary>
        /// Handles the message.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message.</param>
        protected virtual void HandleMessage<T>(T message) where T : Message
        {
            //  Do nothing as message could be proccessed by other module
        }

        #region Handle transport messages

        protected virtual void HandleMessage(DisconnectMessage message)
        {
            this.IsConnected = false;   //  Connection is terminated after this message

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

            //  Update encryption and decryption algorithm
            this.ServerMac = this._keyExhcange.ServerMac;
            this.ClientMac = this._keyExhcange.ClientMac;
            this.ClientCipher = this._keyExhcange.ClientCipher;
            this.ServerCipher = this._keyExhcange.ServerCipher;
            this.ServerDecompression = this._keyExhcange.ServerDecompression;
            this.ClientCompression = this._keyExhcange.ClientCompression;

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

            try
            {
                var totalBytesRead = 0;

                while (totalBytesRead < length)
                {
                    int bytesRead = this._socketStream.Read(buffer, 0, length);
                    if (bytesRead > 0)
                    {
                        totalBytesRead += bytesRead;
                    }
                    else
                    {
                        totalBytesRead = bytesRead;
                        break;
                    }
                }

                if (totalBytesRead < length)
                {
                    this.IsConnected = false;
                    if (totalBytesRead > 0)
                    {
                        throw new InvalidDataException(string.Format("Data read {0}, expected {1}", totalBytesRead, length));
                    }
                    else
                    {
                        //  Socket was disconnected by the server
                        throw new IOException("Unable to read data to the transport connection: An established connection was aborted by the software in your host machine");
                    }
                }
                else
                    return buffer;
            }
            catch (Exception)
            {
                //  If exception was thrown mark connection as closed and rethrow an exception
                this.IsConnected = false;
                throw;
            }

        }

        /// <summary>
        /// Writes the specified data to the server.
        /// </summary>
        /// <param name="data">The data.</param>
        protected void Write(byte[] data)
        {
            try
            {
                this._socketStream.Write(data, 0, data.Length);
                this._socketStream.Flush();
            }
            catch (Exception)
            {
                this.IsConnected = false;
                throw;
            }
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
        /// Perfom neccesary cleanup when client disconects from the server
        /// </summary>
        private void DisconnectCleanup()
        {
            //  Stop running listener thread

            //  Close all open channels if any
            foreach (var channelId in this._openChannels.Values)
            {
                this.SendMessage(new ChannelCloseMessage
                {
                    ChannelNumber = channelId,
                });
            }

            //  Close socket connection if still open
            if (this.IsConnected)
            {
                this._socket.Disconnect(false);
                this.IsConnected = false;
            }
        }

        /// <summary>
        /// Listnets for incoming message from the server and handles them. This method run as a task on seperate thread.
        /// </summary>
        private void MessageListener()
        {
            try
            {
                while (this.IsConnected)
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

                //  In case of error issue disconntect command
                this.Disconnect(DisconnectReasonCodes.ByApplication, exp.ToString());

                this._exceptionToThrow = exp;

                this._exceptionWaitHandle.Set();
            }
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

        /// <summary>
        /// Handles <see cref="ChannelOpenConfirmationMessage"/> message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void HandleMessage(ChannelOpenConfirmationMessage message)
        {
            //  Keep track of open channels
            this._openChannels.Add(message.ChannelNumber, message.ServerChannelNumber);
        }

        /// <summary>
        /// Handles <see cref="ChannelCloseMessage"/> message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void HandleMessage(ChannelCloseMessage message)
        {
            //  Keep track of open channels
            this._openChannels.Remove(message.ChannelNumber);
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
