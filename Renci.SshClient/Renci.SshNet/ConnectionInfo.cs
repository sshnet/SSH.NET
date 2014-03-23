using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Renci.SshNet.Security;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using Renci.SshNet.Security.Cryptography.Ciphers;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents remote connection information class.
    /// </summary>
    /// <remarks>
    /// This class is NOT thread-safe. Do not use the same <see cref="ConnectionInfo"/> with multiple
    /// client instances.
    /// </remarks>
    public class ConnectionInfo
    {
        internal static int DEFAULT_PORT = 22;

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
        public IDictionary<string, HashInfo> HmacAlgorithms { get; private set; }

        /// <summary>
        /// Gets supported host key algorithms for this connection.
        /// </summary>
        public IDictionary<string, Func<byte[], KeyHostAlgorithm>> HostKeyAlgorithms { get; private set; }

        /// <summary>
        /// Gets supported authentication methods for this connection.
        /// </summary>
        public IEnumerable<AuthenticationMethod> AuthenticationMethods { get; private set; }

        /// <summary>
        /// Gets supported compression algorithms for this connection.
        /// </summary>
        public IDictionary<string, Type> CompressionAlgorithms { get; private set; }

        /// <summary>
        /// Gets supported channel requests for this connection.
        /// </summary>
        public IDictionary<string, RequestInfo> ChannelRequests { get; private set; }

        /// <summary>
        /// Gets a value indicating whether connection is authenticated.
        /// </summary>
        /// <value>
        /// <c>true</c> if connection is authenticated; otherwise, <c>false</c>.
        /// </value>
        public bool IsAuthenticated { get; private set; }

        /// <summary>
        /// Gets connection host.
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Gets connection port.
        /// </summary>
        /// <value>
        /// The connection port. The default value is 22.
        /// </value>
        public int Port { get; private set; }

        /// <summary>
        /// Gets connection username.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Gets proxy type.
        /// </summary>
        /// <value>
        /// The type of the proxy.
        /// </value>
        public ProxyTypes ProxyType { get; private set; }

        /// <summary>
        /// Gets proxy connection host.
        /// </summary>
        public string ProxyHost { get; private set; }

        /// <summary>
        /// Gets proxy connection port.
        /// </summary>
        public int ProxyPort { get; private set; }

        /// <summary>
        /// Gets proxy connection username.
        /// </summary>
        public string ProxyUsername { get; private set; }

        /// <summary>
        /// Gets proxy connection password.
        /// </summary>
        public string ProxyPassword { get; private set; }

        /// <summary>
        /// Gets or sets connection timeout.
        /// </summary>
        /// <value>
        /// The connection timeout. The default value is 30 seconds.
        /// </value>
        /// <example>
        ///   <code source="..\..\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient Connect Timeout" language="C#" title="Specify connection timeout" />
        /// </example>
        public TimeSpan Timeout { get; set; }

        /// <summary>
        /// Gets or sets the character encoding.
        /// </summary>
        /// <value>
        /// The character encoding. The default is <see cref="System.Text.Encoding.UTF8"/>.
        /// </value>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Gets or sets number of retry attempts when session channel creation failed.
        /// </summary>
        /// <value>
        /// The number of retry attempts when session channel creation failed. The default
        /// value is 10.
        /// </value>
        public int RetryAttempts { get; set; }

        /// <summary>
        /// Gets or sets maximum number of session channels to be open simultaneously.
        /// </summary>
        /// <value>
        /// The maximum number of session channels to be open simultaneously. The default
        /// value is 10.
        /// </value>
        public int MaxSessions { get; set; }

        /// <summary>
        /// Occurs when authentication banner is sent by the server.
        /// </summary>
        /// <example>
        ///     <code source="..\..\Renci.SshNet.Tests\Classes\PasswordConnectionInfoTest.cs" region="Example PasswordConnectionInfo AuthenticationBanner" language="C#" title="Display authentication banner" />
        /// </example>
        public event EventHandler<AuthenticationBannerEventArgs> AuthenticationBanner;

        /// <summary>
        /// Gets the current key exchange algorithm.
        /// </summary>
        public string CurrentKeyExchangeAlgorithm { get; internal set; }

        /// <summary>
        /// Gets the current server encryption.
        /// </summary>
        public string CurrentServerEncryption { get; internal set; }

        /// <summary>
        /// Gets the current client encryption.
        /// </summary>
        public string CurrentClientEncryption { get; internal set; }

        /// <summary>
        /// Gets the current server hash algorithm.
        /// </summary>
        public string CurrentServerHmacAlgorithm { get; internal set; }

        /// <summary>
        /// Gets the current client hash algorithm.
        /// </summary>
        public string CurrentClientHmacAlgorithm { get; internal set; }

        /// <summary>
        /// Gets the current host key algorithm.
        /// </summary>
        public string CurrentHostKeyAlgorithm { get; internal set; }

        /// <summary>
        /// Gets the current server compression algorithm.
        /// </summary>
        public string CurrentServerCompressionAlgorithm { get; internal set; }

        /// <summary>
        /// Gets the server version.
        /// </summary>
        public string ServerVersion { get; internal set; }

        /// <summary>
        /// Get the client version.
        /// </summary>
        public string ClientVersion { get; internal set; }

        /// <summary>
        /// Gets the current client compression algorithm.
        /// </summary>
        public string CurrentClientCompressionAlgorithm { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="username">The username.</param>
        /// <param name="authenticationMethods">The authentication methods.</param>
        public ConnectionInfo(string host, string username, params AuthenticationMethod[] authenticationMethods)
            : this(host, ConnectionInfo.DEFAULT_PORT, username, ProxyTypes.None, null, 0, null, null, authenticationMethods)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">The username.</param>
        /// <param name="authenticationMethods">The authentication methods.</param>
        public ConnectionInfo(string host, int port, string username, params AuthenticationMethod[] authenticationMethods)
            : this(host, port, username, ProxyTypes.None, null, 0, null, null, authenticationMethods)
        {
        }

        //  TODO: DOCS Add exception documentation for this class.

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionInfo" /> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="proxyPassword">The proxy password.</param>
        /// <param name="authenticationMethods">The authentication methods.</param>
        /// <exception cref="System.ArgumentException">host</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">proxyPort</exception>
        /// <exception cref="ArgumentException"><paramref name="host" /> is invalid, or <paramref name="username" /> is null or contains whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port" /> is not within <see cref="F:System.Net.IPEndPoint.MinPort" /> and <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
        /// <exception cref="ArgumentException"><paramref name="host" /> is invalid, or <paramref name="username" /> is null or contains whitespace characters.</exception>
        public ConnectionInfo(string host, int port, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername, string proxyPassword, params AuthenticationMethod[] authenticationMethods)
        {
            if (!host.IsValidHost())
                throw new ArgumentException("host");

            if (proxyType != ProxyTypes.None)
            {
                if (string.IsNullOrEmpty(proxyHost) && !proxyHost.IsValidHost())
                    throw new ArgumentException("proxyHost");

                if (!proxyPort.IsValidPort())
                    throw new ArgumentOutOfRangeException("proxyPort");
            }

            if (!port.IsValidPort())
                throw new ArgumentOutOfRangeException("port");

            if (username.IsNullOrWhiteSpace())
                throw new ArgumentException("username");

            if (authenticationMethods == null || authenticationMethods.Length < 1)
                throw new ArgumentException("authenticationMethods");

            //  Set default connection values
            this.Timeout = TimeSpan.FromSeconds(30);
            this.RetryAttempts = 10;
            this.MaxSessions = 10;
            this.Encoding = Encoding.UTF8;

            this.KeyExchangeAlgorithms = new Dictionary<string, Type>()
            {
                {"diffie-hellman-group-exchange-sha256", typeof(KeyExchangeDiffieHellmanGroupExchangeSha256)},
                {"diffie-hellman-group-exchange-sha1", typeof(KeyExchangeDiffieHellmanGroupExchangeSha1)},
                {"diffie-hellman-group14-sha1", typeof(KeyExchangeDiffieHellmanGroup14Sha1)},
                {"diffie-hellman-group1-sha1", typeof(KeyExchangeDiffieHellmanGroup1Sha1)},
                //{"ecdh-sha2-nistp256", typeof(KeyExchangeEllipticCurveDiffieHellman)},
                //{"ecdh-sha2-nistp256", typeof(...)},
                //{"ecdh-sha2-nistp384", typeof(...)},
                //{"ecdh-sha2-nistp521", typeof(...)},
                //"gss-group1-sha1-toWM5Slw5Ew8Mqkay+al2g==" - WinSSHD
                //"gss-gex-sha1-toWM5Slw5Ew8Mqkay+al2g==" - WinSSHD

            };

            this.Encryptions = new Dictionary<string, CipherInfo>()
            {
                {"aes256-ctr", new CipherInfo(256, (key, iv)=>{ return new AesCipher(key, new CtrCipherMode(iv), null); }) },
                {"3des-cbc", new CipherInfo(192, (key, iv)=>{ return new TripleDesCipher(key, new CbcCipherMode(iv), null); }) },
                {"aes128-cbc", new CipherInfo(128, (key, iv)=>{ return new AesCipher(key, new CbcCipherMode(iv), null); }) },
                {"aes192-cbc", new CipherInfo(192, (key, iv)=>{ return new AesCipher(key, new CbcCipherMode(iv), null); }) },
                {"aes256-cbc", new CipherInfo(256, (key, iv)=>{ return new AesCipher(key, new CbcCipherMode(iv), null); }) },
                {"blowfish-cbc", new CipherInfo(128, (key, iv)=>{ return new BlowfishCipher(key, new CbcCipherMode(iv), null); }) },
                {"twofish-cbc", new CipherInfo(256, (key, iv)=>{ return new TwofishCipher(key, new CbcCipherMode(iv), null); }) },
                {"twofish192-cbc", new CipherInfo(192, (key, iv)=>{ return new TwofishCipher(key, new CbcCipherMode(iv), null); }) },
                {"twofish128-cbc", new CipherInfo(128, (key, iv)=>{ return new TwofishCipher(key, new CbcCipherMode(iv), null); }) },
                {"twofish256-cbc", new CipherInfo(256, (key, iv)=>{ return new TwofishCipher(key, new CbcCipherMode(iv), null); }) },
                ////{"serpent256-cbc", typeof(CipherSerpent256CBC)},
                ////{"serpent192-cbc", typeof(...)},
                ////{"serpent128-cbc", typeof(...)},
                {"arcfour", new CipherInfo(128, (key, iv)=>{ return new Arc4Cipher(key, false); }) },
                {"arcfour128", new CipherInfo(128, (key, iv)=>{ return new Arc4Cipher(key, true); }) },
                {"arcfour256", new CipherInfo(256, (key, iv)=>{ return new Arc4Cipher(key, true); }) },
                ////{"idea-cbc", typeof(...)},
                {"cast128-cbc", new CipherInfo(128, (key, iv)=>{ return new CastCipher(key, new CbcCipherMode(iv), null); }) },
                ////{"rijndael-cbc@lysator.liu.se", typeof(...)},                
                {"aes128-ctr", new CipherInfo(128, (key, iv)=>{ return new AesCipher(key, new CtrCipherMode(iv), null); }) },
                {"aes192-ctr", new CipherInfo(192, (key, iv)=>{ return new AesCipher(key, new CtrCipherMode(iv), null); }) },
            };

            this.HmacAlgorithms = new Dictionary<string, HashInfo>()
            {
                {"hmac-md5", new HashInfo(16 * 8, (key)=>{ return new HMac<MD5Hash>(key); }) },
                {"hmac-sha1", new HashInfo(20 * 8, (key)=>{ return new HMac<SHA1Hash>(key); }) },
                {"hmac-sha2-256", new HashInfo(32 * 8, (key)=>{ return new HMac<SHA256Hash>(key); }) },
                {"hmac-sha2-256-96", new HashInfo(32 * 8, (key)=>{ return new HMac<SHA256Hash>(key, 96); }) },
                //{"hmac-sha2-512", new HashInfo(64 * 8, (key)=>{ return new HMac<SHA512Hash>(key); }) },
                //{"hmac-sha2-512-96", new HashInfo(64 * 8, (key)=>{ return new HMac<SHA512Hash>(key, 96); }) },
                //{"umac-64@openssh.com", typeof(HMacSha1)},
                {"hmac-ripemd160", new HashInfo(160, (key)=>{ return new HMac<RIPEMD160Hash>(key); }) },
                {"hmac-ripemd160@openssh.com", new HashInfo(160, (key)=>{ return new HMac<RIPEMD160Hash>(key); }) },
                {"hmac-md5-96", new HashInfo(16 * 8, (key)=>{ return new HMac<MD5Hash>(key, 96); }) },
                {"hmac-sha1-96", new HashInfo(20 * 8, (key)=>{ return new HMac<SHA1Hash>(key, 96); }) },
                //{"none", typeof(...)},
            };

            this.HostKeyAlgorithms = new Dictionary<string, Func<byte[], KeyHostAlgorithm>>()
            {
                {"ssh-rsa", (data) => { return new KeyHostAlgorithm("ssh-rsa", new RsaKey(), data); }},
                {"ssh-dss", (data) => { return new KeyHostAlgorithm("ssh-dss", new DsaKey(), data); }},
                //{"ecdsa-sha2-nistp256 "}
                //{"x509v3-sign-rsa", () => { ... },
                //{"x509v3-sign-dss", () => { ... },
                //{"spki-sign-rsa", () => { ... },
                //{"spki-sign-dss", () => { ... },
                //{"pgp-sign-rsa", () => { ... },
                //{"pgp-sign-dss", () => { ... },
            };

            this.CompressionAlgorithms = new Dictionary<string, Type>()
            {
                //{"zlib@openssh.com", typeof(ZlibOpenSsh)}, 
                //{"zlib", typeof(Zlib)}, 
                {"none", null}, 
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

            this.Host = host;
            this.Port = port;
            this.Username = username;

            this.ProxyType = proxyType;
            this.ProxyHost = proxyHost;
            this.ProxyPort = proxyPort;
            this.ProxyUsername = proxyUsername;
            this.ProxyPassword = proxyPassword;

            this.AuthenticationMethods = authenticationMethods;
        }

        /// <summary>
        /// Authenticates the specified session.
        /// </summary>
        /// <param name="session">The session to be authenticated.</param>
        /// <exception cref="ArgumentNullException"><paramref name="session"/> is null.</exception>
        /// <exception cref="SshAuthenticationException">No suitable authentication method found to complete authentication, or permission denied.</exception>
        public void Authenticate(Session session)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            session.RegisterMessage("SSH_MSG_USERAUTH_FAILURE");
            session.RegisterMessage("SSH_MSG_USERAUTH_SUCCESS");
            session.RegisterMessage("SSH_MSG_USERAUTH_BANNER");
            session.UserAuthenticationBannerReceived += Session_UserAuthenticationBannerReceived;

            try
            {
                // the exception to report an authentication failure with
                SshAuthenticationException authenticationException = null;

                // try to authenticate against none
                var noneAuthenticationMethod = new NoneAuthenticationMethod(this.Username);

                var authenticated = noneAuthenticationMethod.Authenticate(session);
                if (authenticated != AuthenticationResult.Success)
                {
                    var failedAuthenticationMethods = new List<AuthenticationMethod>();
                    if (TryAuthenticate(session, noneAuthenticationMethod.AllowedAuthentications.ToList(), failedAuthenticationMethods, ref authenticationException))
                    {
                        authenticated = AuthenticationResult.Success;
                    }
                }

                this.IsAuthenticated = authenticated == AuthenticationResult.Success;
                if (!IsAuthenticated)
                    throw authenticationException;
            }
            finally
            {
                session.UserAuthenticationBannerReceived -= Session_UserAuthenticationBannerReceived;
                session.UnRegisterMessage("SSH_MSG_USERAUTH_FAILURE");
                session.UnRegisterMessage("SSH_MSG_USERAUTH_SUCCESS");
                session.UnRegisterMessage("SSH_MSG_USERAUTH_BANNER");
            }
        }

        private bool TryAuthenticate(Session session, IList<string> allowedAuthenticationMethods, IList<AuthenticationMethod> failedAuthenticationMethods, ref SshAuthenticationException authenticationException)
        {
            if (!allowedAuthenticationMethods.Any())
            {
                authenticationException = new SshAuthenticationException("No authentication methods defined on SSH server.");
                return false;
            }

            // we want to try authentication methods in the order in which they were
            //  passed in the ctor, not the order in which the SSH server returns
            // the allowed authentication methods
            var matchingAuthenticationMethods = AuthenticationMethods.Where(a => allowedAuthenticationMethods.Contains(a.Name)).ToList();
            if (!matchingAuthenticationMethods.Any())
            {
                authenticationException = new SshAuthenticationException(string.Format("No suitable authentication method found to complete authentication ({0}).", string.Join(",", allowedAuthenticationMethods.ToArray())));
                return false;
            }

            foreach (var authenticationMethod in matchingAuthenticationMethods)
            {
                if (failedAuthenticationMethods.Contains(authenticationMethod))
                    continue;

                var authenticationResult = authenticationMethod.Authenticate(session);
                switch (authenticationResult)
                {
                    case AuthenticationResult.PartialSuccess:
                        if (TryAuthenticate(session, authenticationMethod.AllowedAuthentications.ToList(), failedAuthenticationMethods, ref authenticationException))
                            authenticationResult = AuthenticationResult.Success;
                        break;
                    case AuthenticationResult.Failure:
                        failedAuthenticationMethods.Add(authenticationMethod);
                        authenticationException = new SshAuthenticationException(string.Format("Permission denied ({0}).", authenticationMethod.Name));
                        break;
                    case AuthenticationResult.Success:
                        authenticationException = null;
                        break;
                }

                if (authenticationResult == AuthenticationResult.Success)
                    return true;
            }

            return false;
        }

        private void Session_UserAuthenticationBannerReceived(object sender, MessageEventArgs<BannerMessage> e)
        {
            if (this.AuthenticationBanner != null)
            {
                this.AuthenticationBanner(this, new AuthenticationBannerEventArgs(this.Username, e.Message.Message, e.Message.Language));
            }
        }
    }
}
