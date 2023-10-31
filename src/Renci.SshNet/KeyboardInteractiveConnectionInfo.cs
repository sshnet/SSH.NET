using System;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides connection information when keyboard interactive authentication method is used.
    /// </summary>
    /// <example>
    ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\KeyboardInteractiveConnectionInfoTest.cs" region="Example KeyboardInteractiveConnectionInfo AuthenticationPrompt" language="C#" title="Connect using interactive method" />
    /// </example>
    public class KeyboardInteractiveConnectionInfo : ConnectionInfo, IDisposable
    {
        private bool _isDisposed;

        /// <summary>
        /// Occurs when server prompts for more authentication information.
        /// </summary>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\KeyboardInteractiveConnectionInfoTest.cs" region="Example KeyboardInteractiveConnectionInfo AuthenticationPrompt" language="C#" title="Connect using interactive method" />
        /// </example>
        public event EventHandler<AuthenticationPromptEventArgs> AuthenticationPrompt;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardInteractiveConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="username">The username.</param>
        public KeyboardInteractiveConnectionInfo(string host, string username)
            : this(host, DefaultPort, username, ProxyTypes.None, string.Empty, 0, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardInteractiveConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">The username.</param>
        public KeyboardInteractiveConnectionInfo(string host, int port, string username)
            : this(host, port, username, ProxyTypes.None, string.Empty, 0, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardInteractiveConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        public KeyboardInteractiveConnectionInfo(string host, int port, string username, ProxyTypes proxyType, string proxyHost, int proxyPort)
            : this(host, port, username, proxyType, proxyHost, proxyPort, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardInteractiveConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        public KeyboardInteractiveConnectionInfo(string host, int port, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername)
            : this(host, port, username, proxyType, proxyHost, proxyPort, proxyUsername, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardInteractiveConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        public KeyboardInteractiveConnectionInfo(string host, string username, ProxyTypes proxyType, string proxyHost, int proxyPort)
            : this(host, DefaultPort, username, proxyType, proxyHost, proxyPort, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardInteractiveConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        public KeyboardInteractiveConnectionInfo(string host, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername)
            : this(host, DefaultPort, username, proxyType, proxyHost, proxyPort, proxyUsername, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardInteractiveConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="proxyPassword">The proxy password.</param>
        public KeyboardInteractiveConnectionInfo(string host, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername, string proxyPassword)
            : this(host, DefaultPort, username, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardInteractiveConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="proxyPassword">The proxy password.</param>
        public KeyboardInteractiveConnectionInfo(string host, int port, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername, string proxyPassword)
            : base(host, port, username, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword, new KeyboardInteractiveAuthenticationMethod(username))
        {
            foreach (var authenticationMethod in AuthenticationMethods)
            {
                if (authenticationMethod is KeyboardInteractiveAuthenticationMethod kbdInteractive)
                {
                    kbdInteractive.AuthenticationPrompt += AuthenticationMethod_AuthenticationPrompt;
                }
            }

        }

        private void AuthenticationMethod_AuthenticationPrompt(object sender, AuthenticationPromptEventArgs e)
        {
            AuthenticationPrompt?.Invoke(sender, e);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (AuthenticationMethods != null)
                {
                    foreach (var authenticationMethods in AuthenticationMethods)
                    {
                        if (authenticationMethods is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="KeyboardInteractiveConnectionInfo"/> class.
        /// </summary>
        ~KeyboardInteractiveConnectionInfo()
        {
            Dispose(disposing: false);
        }
    }
}
