using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
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

        public static Session CreateSession(ConnectionInfo connectionInfo)
        {
            var ep = new IPEndPoint(Dns.GetHostAddresses(connectionInfo.Host)[0], connectionInfo.Port);
            var socket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            socket.ExclusiveAddressUse = true;

            //  Connect socket with 5 seconds timeout
            var connectResult = socket.BeginConnect(ep, null, null);

            connectResult.AsyncWaitHandle.WaitOne(1000 * 5);

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

        private Socket _socket;

        private NetworkStream _sockeStream;

        private int _waitTimeout = 1000 * 10;   //  Default 10 sec wait timeout

        private KeyExchange _keyExhcange;

        private BackgroundWorker _messageListener;

        private EventWaitHandle _keyExhangedFinishedWaitHandle = new AutoResetEvent(false);

        private EventWaitHandle _serviceAccepted = new AutoResetEvent(false);

        private EventWaitHandle _exceptionWaitHandle = new AutoResetEvent(false);

        private IDictionary<uint, uint> _openChannels = new Dictionary<uint, uint>();

        private IDictionary<string, UserAuthentication> _executedAuthenticationMethods = new Dictionary<string, UserAuthentication>();

        /// <summary>
        /// Exception that need to be thrown by waiting thread
        /// </summary>
        private Exception _exceptionToThrow;

        /// <summary>
        /// Specifies weither connection is authenticated
        /// </summary>
        private bool _isAuthenticated;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        protected HMAC ServerMac { get; private set; }

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

        protected Compression ServerDecompression { get; private set; }

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

        public string ServerVersion { get; private set; }

        public string ClientVersion { get; private set; }

        public ConnectionInfo ConnectionInfo { get; private set; }

        protected Session(ConnectionInfo connectionInfo, Socket socket, string serverVersion)
        {
            this.ConnectionInfo = connectionInfo;
            this._socket = socket;
            this._socket.NoDelay = true;
            this._socket.Blocking = true;

            this.ServerVersion = serverVersion;
            this.ClientVersion = string.Format("SSH-2.0-Renci.SshClient.{0}", this.GetType().Assembly.GetName().Version);

            this._sockeStream = new NetworkStream(socket);
            this.IsConnected = true;
        }

        private static IDictionary<Type, Func<Session, Channel>> _channels = new Dictionary<Type, Func<Session, Channel>>()
            {
                {typeof(ChannelExec), (session) => { return new ChannelExec(session);}},
                {typeof(ChannelSftp), (session) => { return new ChannelSftp(session);}}
            };

        public T CreateChannel<T>() where T : Channel
        {
            if (!this.IsConnected)
            {
                throw new InvalidOperationException("Not connected");
            }
            return _channels[typeof(T)](this) as T;
        }

        public void Connect()
        {
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
            this._messageListener = new BackgroundWorker();
            this._messageListener.DoWork += MessageListener;
            this._messageListener.RunWorkerAsync();

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

        public void Disconnect()
        {
            //  TODO:   Change message to something more appropriate
            this.Disconnect(DisconnectReasonCodes.ByApplication, "Connection terminated by the client.");

            this.DisconnectCleanup();
        }

        internal abstract void SendMessage(Message message);

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
            index = EventWaitHandle.WaitAny(waitHandles, this._waitTimeout);
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

        protected abstract Message ReceiveMessage();

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

        protected byte[] Read(int length)
        {
            var buffer = new byte[length];

            try
            {
                //int bytesRead = this._sockeStream.Read(buffer, 0, length);

                var totalBytesRead = 0;

                while (totalBytesRead < length)
                {
                    int bytesRead = this._sockeStream.Read(buffer, 0, length);
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

        protected void Write(byte[] data)
        {
            try
            {
                this._sockeStream.Write(data, 0, data.Length);
                this._sockeStream.Flush();
            }
            catch (Exception)
            {
                this.IsConnected = false;
                throw;
            }
        }

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

        private void MessageListener(object sender, DoWorkEventArgs e)
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

        private void RaiseMessageReceived(object sender, MessageReceivedEventArgs args)
        {
            if (this.MessageReceived != null)
            {
                this.MessageReceived(sender, args);
            }
        }

        private void HandleMessage(ChannelOpenConfirmationMessage message)
        {
            //  Keep track of open channels
            this._openChannels.Add(message.ChannelNumber, message.ServerChannelNumber);
            Debug.WriteLine("Open channel " + message.ServerChannelNumber);
        }

        private void HandleMessage(ChannelCloseMessage message)
        {
            //  Keep track of open channels
            Debug.WriteLine("Close channel " + this._openChannels[message.ChannelNumber]);
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
