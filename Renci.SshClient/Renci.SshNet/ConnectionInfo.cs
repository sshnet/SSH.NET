using System;
using System.Collections.Generic;
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
    public class ConnectionInfo : IConnectionInfoInternal
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
        /// Gets the supported channel requests for this connection.
        /// </summary>
        /// <value>
        /// The supported channel requests for this connection.
        /// </value>
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
        /// <exception cref="ArgumentNullException"><paramref name="host"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is a zero-length string.</exception>
        /// <exception cref="ArgumentException"><paramref name="username" /> is null, a zero-length string or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="authenticationMethods"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No <paramref name="authenticationMethods"/> specified.</exception>
        public ConnectionInfo(string host, string username, params AuthenticationMethod[] authenticationMethods)
            : this(host, DEFAULT_PORT, username, ProxyTypes.None, null, 0, null, null, authenticationMethods)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">The username.</param>
        /// <param name="authenticationMethods">The authentication methods.</param>
        /// <exception cref="ArgumentNullException"><paramref name="host"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="username" /> is null, a zero-length string or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port" /> is not within <see cref="F:System.Net.IPEndPoint.MinPort" /> and <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="authenticationMethods"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No <paramref name="authenticationMethods"/> specified.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="host"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="username" /> is null, a zero-length string or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port" /> is not within <see cref="F:System.Net.IPEndPoint.MinPort" /> and <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="proxyType"/> is not <see cref="ProxyTypes.None"/> and <paramref name="proxyHost" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="proxyType"/> is not <see cref="ProxyTypes.None"/> and <paramref name="proxyPort" /> is not within <see cref="F:System.Net.IPEndPoint.MinPort" /> and <see cref="F:System.Net.IPEndPoint.MaxPort" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="authenticationMethods"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No <paramref name="authenticationMethods"/> specified.</exception>
        public ConnectionInfo(string host, int port, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername, string proxyPassword, params AuthenticationMethod[] authenticationMethods)
        {
            if (host == null)
                throw new ArgumentNullException("host");
            port.ValidatePort("port");

            if (username == null)
                throw new ArgumentNullException("username");
            if (username.All(char.IsWhiteSpace))
                throw new ArgumentException("Cannot be empty or contain only whitespace.", "username");

            if (proxyType != ProxyTypes.None)
            {
                if (proxyHost == null)
                    throw new ArgumentNullException("proxyHost");
                proxyPort.ValidatePort("proxyPort");
            }

            if (authenticationMethods == null)
                throw new ArgumentNullException("authenticationMethods");
            if (!authenticationMethods.Any())
                throw new ArgumentException("At least one authentication method should be specified.", "authenticationMethods");

            //  Set default connection values
            this.Timeout = TimeSpan.FromSeconds(30);
            this.RetryAttempts = 10;
            this.MaxSessions = 10;
            this.Encoding = Encoding.UTF8;

            this.KeyExchangeAlgorithms = new Dictionary<string, Type>
                {
                    {"diffie-hellman-group-exchange-sha256", typeof (KeyExchangeDiffieHellmanGroupExchangeSha256)},
                    {"diffie-hellman-group-exchange-sha1", typeof (KeyExchangeDiffieHellmanGroupExchangeSha1)},
                    {"diffie-hellman-group14-sha1", typeof (KeyExchangeDiffieHellmanGroup14Sha1)},
                    {"diffie-hellman-group1-sha1", typeof (KeyExchangeDiffieHellmanGroup1Sha1)},
                    //{"ecdh-sha2-nistp256", typeof(KeyExchangeEllipticCurveDiffieHellman)},
                    //{"ecdh-sha2-nistp256", typeof(...)},
                    //{"ecdh-sha2-nistp384", typeof(...)},
                    //{"ecdh-sha2-nistp521", typeof(...)},
                    //"gss-group1-sha1-toWM5Slw5Ew8Mqkay+al2g==" - WinSSHD
                    //"gss-gex-sha1-toWM5Slw5Ew8Mqkay+al2g==" - WinSSHD
                };

            this.Encryptions = new Dictionary<string, CipherInfo>
                {
                    {"aes256-ctr", new CipherInfo(256, (key, iv) => new AesCipher(key, new CtrCipherMode(iv), null))},
                    {"3des-cbc", new CipherInfo(192, (key, iv) => new TripleDesCipher(key, new CbcCipherMode(iv), null))},
                    {"aes128-cbc", new CipherInfo(128, (key, iv) => new AesCipher(key, new CbcCipherMode(iv), null))},
                    {"aes192-cbc", new CipherInfo(192, (key, iv) => new AesCipher(key, new CbcCipherMode(iv), null))},
                    {"aes256-cbc", new CipherInfo(256, (key, iv) => new AesCipher(key, new CbcCipherMode(iv), null))},
                    {"blowfish-cbc", new CipherInfo(128, (key, iv) => new BlowfishCipher(key, new CbcCipherMode(iv), null))},
                    {"twofish-cbc", new CipherInfo(256, (key, iv) => new TwofishCipher(key, new CbcCipherMode(iv), null))},
                    {"twofish192-cbc", new CipherInfo(192, (key, iv) => new TwofishCipher(key, new CbcCipherMode(iv), null))},
                    {"twofish128-cbc", new CipherInfo(128, (key, iv) => new TwofishCipher(key, new CbcCipherMode(iv), null))},
                    {"twofish256-cbc", new CipherInfo(256, (key, iv) => new TwofishCipher(key, new CbcCipherMode(iv), null))},
                    ////{"serpent256-cbc", typeof(CipherSerpent256CBC)},
                    ////{"serpent192-cbc", typeof(...)},
                    ////{"serpent128-cbc", typeof(...)},
                    {"arcfour", new CipherInfo(128, (key, iv) => new Arc4Cipher(key, false))},
                    {"arcfour128", new CipherInfo(128, (key, iv) => new Arc4Cipher(key, true))},
                    {"arcfour256", new CipherInfo(256, (key, iv) => new Arc4Cipher(key, true))},
                    ////{"idea-cbc", typeof(...)},
                    {"cast128-cbc", new CipherInfo(128, (key, iv) => new CastCipher(key, new CbcCipherMode(iv), null))},
                    ////{"rijndael-cbc@lysator.liu.se", typeof(...)},                
                    {"aes128-ctr", new CipherInfo(128, (key, iv) => new AesCipher(key, new CtrCipherMode(iv), null))},
                    {"aes192-ctr", new CipherInfo(192, (key, iv) => new AesCipher(key, new CtrCipherMode(iv), null))},
                };

            this.HmacAlgorithms = new Dictionary<string, HashInfo>
                {
                    {"hmac-md5", new HashInfo(16*8, key => new HMac<MD5Hash>(key))},
                    {"hmac-sha1", new HashInfo(20*8, key => new HMac<SHA1Hash>(key))},
                    {"hmac-sha2-256", new HashInfo(32*8, key => new HMac<SHA256Hash>(key))},
                    {"hmac-sha2-256-96", new HashInfo(32*8, key => new HMac<SHA256Hash>(key, 96))},
                    //{"hmac-sha2-512", new HashInfo(64 * 8, key => new HMac<SHA512Hash>(key))},
                    //{"hmac-sha2-512-96", new HashInfo(64 * 8,  key => new HMac<SHA512Hash>(key, 96))},
                    //{"umac-64@openssh.com", typeof(HMacSha1)},
                    {"hmac-ripemd160", new HashInfo(160, key => new HMac<RIPEMD160Hash>(key))},
                    {"hmac-ripemd160@openssh.com", new HashInfo(160, key => new HMac<RIPEMD160Hash>(key))},
                    {"hmac-md5-96", new HashInfo(16*8, key => new HMac<MD5Hash>(key, 96))},
                    {"hmac-sha1-96", new HashInfo(20*8, key => new HMac<SHA1Hash>(key, 96))},
                    //{"none", typeof(...)},
                };

            this.HostKeyAlgorithms = new Dictionary<string, Func<byte[], KeyHostAlgorithm>>
                {
                    {"ssh-rsa", data => new KeyHostAlgorithm("ssh-rsa", new RsaKey(), data)},
                    {"ssh-dss", data => new KeyHostAlgorithm("ssh-dss", new DsaKey(), data)},
                    //{"ecdsa-sha2-nistp256 "}
                    //{"x509v3-sign-rsa", () => { ... },
                    //{"x509v3-sign-dss", () => { ... },
                    //{"spki-sign-rsa", () => { ... },
                    //{"spki-sign-dss", () => { ... },
                    //{"pgp-sign-rsa", () => { ... },
                    //{"pgp-sign-dss", () => { ... },
                };

            this.CompressionAlgorithms = new Dictionary<string, Type>
                {
                    //{"zlib@openssh.com", typeof(ZlibOpenSsh)}, 
                    //{"zlib", typeof(Zlib)}, 
                    {"none", null},
                };

            this.ChannelRequests = new Dictionary<string, RequestInfo>
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
        internal void Authenticate(ISession session)
        {
            IsAuthenticated = false;
            var clientAuthentication = new ClientAuthentication();
            clientAuthentication.Authenticate(this, session);
            IsAuthenticated = true;
        }

        /// <summary>
        /// Signals that an authentication banner message was received from the server.
        /// </summary>
        /// <param name="sender">The session in which the banner message was received.</param>
        /// <param name="e">The banner message.{</param>
        void IConnectionInfoInternal.UserAuthenticationBannerReceived(object sender, MessageEventArgs<BannerMessage> e)
        {
            var authenticationBanner = AuthenticationBanner;
            if (authenticationBanner != null)
            {
                authenticationBanner(this,
                    new AuthenticationBannerEventArgs(Username, e.Message.Message, e.Message.Language));
            }
        }

        IAuthenticationMethod IConnectionInfoInternal.CreateNoneAuthenticationMethod()
        {
            return new NoneAuthenticationMethod(Username);
        }

        IEnumerable<IAuthenticationMethod> IConnectionInfoInternal.AuthenticationMethods
        {
            get { return AuthenticationMethods.Cast<IAuthenticationMethod>(); }
        }
    }
}
