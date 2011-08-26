using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Renci.SshNet.Security;
using Renci.SshNet.Compression;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Common;
using System.Threading;
using System.Net;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Security.Cryptography.Ciphers;
using System.Security.Cryptography;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
namespace Renci.SshNet
{
    /// <summary>
    /// Represents remote connection information base class.
    /// </summary>
    public abstract class ConnectionInfo
    {
        /// <summary>
        /// Gets connection name
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets a value indicating whether connection is authenticated.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if connection is authenticated; otherwise, <c>false</c>.
        /// </value>
        public bool IsAuthenticated { get; private set; }

        /// <summary>
        /// Gets the authentication error message.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Gets reference to the session object.
        /// </summary>
        protected Session Session { get; private set; }

        /// <summary>
        /// Gets supported key exchange algorithms for this connection.
        /// </summary>
        public IDictionary<string, Type> KeyExchangeAlgorithms { get; private set; }

        /// <summary>
        /// Gets supported encryptions for this connection.
        /// </summary>
        public IDictionary<string, CipherInfo> Encryptions { get; private set; }

        /// <summary>
        /// Gets supported hash algorithms for this connection.
        /// </summary>
        public IDictionary<string, Func<byte[], HashAlgorithm>> HmacAlgorithms { get; private set; }

        /// <summary>
        /// Gets supported host key algorithms for this connection.
        /// </summary>
        public IDictionary<string, Func<byte[], HostAlgorithm>> HostKeyAlgorithms { get; private set; }

        /// <summary>
        /// Gets supported authentication methods for this connection.
        /// </summary>
        public IDictionary<string, Type> AuthenticationMethods { get; private set; }

        /// <summary>
        /// Gets supported compression algorithms for this connection.
        /// </summary>
        public IDictionary<string, Type> CompressionAlgorithms { get; private set; }

        /// <summary>
        /// Gets supported channel requests for this connection.
        /// </summary>
        public IDictionary<string, RequestInfo> ChannelRequests { get; private set; }

        /// <summary>
        /// Gets connection host.
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Gets connection port.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets connection username.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Gets or sets connection timeout.
        /// </summary>
        /// <value>
        /// Connection timeout.
        /// </value>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Gets or sets number of retry attempts when session channel creation failed.
        /// </summary>
        /// <value>
        /// Number of retry attempts.
        /// </value>
        public int RetryAttempts { get; set; }

        /// <summary>
        /// Gets or sets maximum number of session channels to be open simultaneously.
        /// </summary>
        /// <value>
        /// The max sessions.
        /// </value>
        public int MaxSessions { get; set; }

        /// <summary>
        /// Occurs when authentication banner is sent by the server.
        /// </summary>
        public event EventHandler<AuthenticationBannerEventArgs> AuthenticationBanner;

        /// <summary>
        /// Prevents a default instance of the <see cref="ConnectionInfo"/> class from being created.
        /// </summary>
        private ConnectionInfo()
        {
            //  Set default connection values
            this.Timeout = TimeSpan.FromSeconds(30);
            this.RetryAttempts = 10;
            this.MaxSessions = 10;

            this.KeyExchangeAlgorithms = new Dictionary<string, Type>()
            {
                {"diffie-hellman-group-exchange-sha256", typeof(KeyExchangeDiffieHellmanGroupExchangeSha256)},
                {"diffie-hellman-group-exchange-sha1", typeof(KeyExchangeDiffieHellmanGroupExchangeSha1)},
                {"diffie-hellman-group14-sha1", typeof(KeyExchangeDiffieHellmanGroup14Sha1)},
                {"diffie-hellman-group1-sha1", typeof(KeyExchangeDiffieHellmanGroup1Sha1)},
            };

            this.Encryptions = new Dictionary<string, CipherInfo>()
            {
                {"3des-cbc", new CipherInfo(192, (key, iv)=>{ return new TripleDesCipher(key, new CbcCipherMode(iv), null); }) },
                {"aes128-cbc", new CipherInfo(128, (key, iv)=>{ return new AesCipher(key, new CbcCipherMode(iv), null); }) },
                {"aes192-cbc", new CipherInfo(192, (key, iv)=>{ return new AesCipher(key, new CbcCipherMode(iv), null); }) },
                {"aes256-cbc", new CipherInfo(256, (key, iv)=>{ return new AesCipher(key, new CbcCipherMode(iv), null); }) },
                {"blowfish-cbc", new CipherInfo(128, (key, iv)=>{ return new BlowfishCipher(key, new CbcCipherMode(iv), null); }) },
                ////{"twofish-cbc", typeof(...)},
                ////{"twofish192-cbc", typeof(...)},
                ////{"twofish128-cbc", typeof(...)},
                ////{"twofish256-cbc", typeof(...)},
                ////{"serpent256-cbc", typeof(CipherSerpent256CBC)},
                ////{"serpent192-cbc", typeof(...)},
                ////{"serpent128-cbc", typeof(...)},
                ////{"arcfour128", typeof(...)},
                ////{"arcfour256", typeof(...)},
                ////{"arcfour", typeof(...)},
                ////{"idea-cbc", typeof(...)},
                {"cast128-cbc", new CipherInfo(128, (key, iv)=>{ return new CastCipher(key, new CbcCipherMode(iv), null); }) },
                ////{"rijndael-cbc@lysator.liu.se", typeof(...)},                
                {"aes128-ctr", new CipherInfo(128, (key, iv)=>{ return new AesCipher(key, new CtrCipherMode(iv), null); }) },
                {"aes192-ctr", new CipherInfo(192, (key, iv)=>{ return new AesCipher(key, new CtrCipherMode(iv), null); }) },
                {"aes256-ctr", new CipherInfo(256, (key, iv)=>{ return new AesCipher(key, new CtrCipherMode(iv), null); }) },
            };

            this.HmacAlgorithms = new Dictionary<string, Func<byte[], HashAlgorithm>>()
            {
                {"hmac-md5", (key) => { return new HMac<MD5Hash>(key.Take(16).ToArray());}},
                {"hmac-sha1", (key) => { return new HMac<SHA1Hash>(key.Take(20).ToArray());}},
                //{"umac-64@openssh.com", typeof(HMacSha1)},
                //{"hmac-ripemd160", typeof(HMacSha1)},
                //{"hmac-ripemd160@openssh.com", typeof(HMacSha1)},
                //{"hmac-md5-96", typeof(...)},
                //{"hmac-sha1-96", typeof(...)},
                //{"none", typeof(...)},
            };

            this.HostKeyAlgorithms = new Dictionary<string, Func<byte[], HostAlgorithm>>()
            {
                {"ssh-rsa", (data) => { return new KeyHostAlgorithm("ssh-rsa", new RsaKey(), data); }},
                {"ssh-dss", (data) => { return new KeyHostAlgorithm("ssh-dss", new DsaKey(), data); }},
                //{"x509v3-sign-rsa", () => { ... },
                //{"x509v3-sign-dss", () => { ... },
                //{"spki-sign-rsa", () => { ... },
                //{"spki-sign-dss", () => { ... },
                //{"pgp-sign-rsa", () => { ... },
                //{"pgp-sign-dss", () => { ... },
            };

            this.AuthenticationMethods = new Dictionary<string, Type>()
            {
                {"none", typeof(ConnectionInfo)},
                {"publickey", typeof(PrivateKeyConnectionInfo)},
                {"password", typeof(PasswordConnectionInfo)},
                {"keyboard-interactive", typeof(KeyboardInteractiveConnectionInfo)},
                //{"hostbased", typeof(...)},                
                //{"gssapi-keyex", typeof(...)},                
                //{"gssapi-with-mic", typeof(...)},
            };

            this.CompressionAlgorithms = new Dictionary<string, Type>()
            {
                {"none", null}, 
                {"zlib", typeof(Zlib)}, 
                {"zlib@openssh.com", typeof(ZlibOpenSsh)}, 
            };

            this.ChannelRequests = new Dictionary<string, RequestInfo>()
            {
                {EnvironmentVariableRequestInfo.NAME, new EnvironmentVariableRequestInfo()}, 
                {ExecRequestInfo.NAME, new ExecRequestInfo()}, 
                {ExitSignalRequestInfo.NAME, new ExitSignalRequestInfo()}, 
                {ExitStatusRequestInfo.NAME, new ExitStatusRequestInfo()}, 
                {PseudoTerminalRequestInfo.NAME, new PseudoTerminalRequestInfo()}, 
                {ShellRequestInfo.NAME, new ShellRequestInfo()}, 
                {SignalRequestInfo.NAME, new SignalRequestInfo()}, 
                {SubsystemRequestInfo.NAME, new SubsystemRequestInfo()}, 
                {WindowChangeRequestInfo.NAME, new WindowChangeRequestInfo()}, 
                {X11ForwardingRequestInfo.NAME, new X11ForwardingRequestInfo()}, 
                {XonXoffRequestInfo.NAME, new XonXoffRequestInfo()}, 
                {EndOfWriteRequestInfo.NAME, new EndOfWriteRequestInfo()}, 
                {KeepAliveRequestInfo.NAME, new KeepAliveRequestInfo()}, 
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Connection username.</param>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is null or contains whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="IPEndPoint.MinPort"/> and <see cref="IPEndPoint.MaxPort"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="username"/> is null or empty.</exception>
        protected ConnectionInfo(string host, int port, string username)
            : this()
        {
            if (!host.IsValidHost())
                throw new ArgumentException("host");

            if (!port.IsValidPort())
                throw new ArgumentOutOfRangeException("port");

            if (username.IsNullOrWhiteSpace())
                throw new ArgumentException("username");

            this.Host = host;
            this.Port = port;
            this.Username = username;
        }

        /// <summary>
        /// Authenticates the specified session.
        /// </summary>
        /// <param name="session">The session to be authenticated.</param>
        /// <returns>true if authenticated; otherwise false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="session"/> is null.</exception>
        public bool Authenticate(Session session)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            this.Session = session;

            this.Session.RegisterMessage("SSH_MSG_USERAUTH_FAILURE");
            this.Session.RegisterMessage("SSH_MSG_USERAUTH_SUCCESS");
            this.Session.RegisterMessage("SSH_MSG_USERAUTH_BANNER");

            this.Session.UserAuthenticationFailureReceived += Session_UserAuthenticationFailureReceived;
            this.Session.UserAuthenticationSuccessReceived += Session_UserAuthenticationSuccessMessageReceived;
            this.Session.UserAuthenticationBannerReceived += Session_UserAuthenticationBannerMessageReceived;
            this.Session.MessageReceived += Session_MessageReceived;

            this.OnAuthenticate();

            this.Session.UserAuthenticationFailureReceived -= Session_UserAuthenticationFailureReceived;
            this.Session.UserAuthenticationSuccessReceived -= Session_UserAuthenticationSuccessMessageReceived;
            this.Session.UserAuthenticationBannerReceived -= Session_UserAuthenticationBannerMessageReceived;
            this.Session.MessageReceived -= Session_MessageReceived;

            this.Session.UnRegisterMessage("SSH_MSG_USERAUTH_FAILURE");
            this.Session.UnRegisterMessage("SSH_MSG_USERAUTH_SUCCESS");
            this.Session.UnRegisterMessage("SSH_MSG_USERAUTH_BANNER");

            return this.IsAuthenticated;
        }

        /// <summary>
        /// Called when connection needs to be authenticated.
        /// </summary>
        protected abstract void OnAuthenticate();

        /// <summary>
        /// Sends SSH message to the server.
        /// </summary>
        /// <param name="message">The message.</param>
        protected void SendMessage(Message message)
        {
            this.Session.SendMessage(message);
        }

        /// <summary>
        /// Waits the handle to signal.
        /// </summary>
        /// <param name="eventWaitHandle">The event wait handle.</param>
        protected void WaitHandle(WaitHandle eventWaitHandle)
        {
            this.Session.WaitHandle(eventWaitHandle);
        }

        /// <summary>
        /// Handles the UserAuthenticationFailureReceived event of the session.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        protected virtual void Session_UserAuthenticationFailureReceived(object sender, MessageEventArgs<FailureMessage> e)
        {
            this.ErrorMessage = e.Message.Message;
            this.IsAuthenticated = false;
        }

        /// <summary>
        /// Handles the UserAuthenticationSuccessMessageReceived event of the session.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        protected virtual void Session_UserAuthenticationSuccessMessageReceived(object sender, MessageEventArgs<SuccessMessage> e)
        {
            this.IsAuthenticated = true;
        }

        /// <summary>
        /// Handles the UserAuthenticationBannerMessageReceived event of the session.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        protected virtual void Session_UserAuthenticationBannerMessageReceived(object sender, MessageEventArgs<BannerMessage> e)
        {
            if (this.AuthenticationBanner != null)
            {
                this.AuthenticationBanner(this, new AuthenticationBannerEventArgs(this.Username, e.Message.Message, e.Message.Language));
            }
        }

        /// <summary>
        /// Handles the MessageReceived event of the session.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        protected virtual void Session_MessageReceived(object sender, MessageEventArgs<Message> e)
        {
        }
    }
}
