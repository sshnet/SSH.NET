using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides connection information when private key authentication method is used
    /// </summary>
    /// <example>
    ///   <code source="..\..\src\Renci.SshNet.Tests\Classes\PrivateKeyConnectionInfoTest.cs" region="Example PrivateKeyConnectionInfo PrivateKeyFile" language="C#" title="Connect using private key" />
    ///   </example>
    public class PrivateKeyConnectionInfo : ConnectionInfo, IDisposable
    {
        /// <summary>
        /// Gets the key files used for authentication.
        /// </summary>
        public ICollection<PrivateKeyFile> KeyFiles { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="keyFiles">Connection key files.</param>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\PrivateKeyConnectionInfoTest.cs" region="Example PrivateKeyConnectionInfo PrivateKeyFile" language="C#" title="Connect using private key" />
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\PrivateKeyConnectionInfoTest.cs" region="Example PrivateKeyConnectionInfo PrivateKeyFile Multiple" language="C#" title="Connect using multiple private key" />
        /// </example>
        public PrivateKeyConnectionInfo(string host, string username, params PrivateKeyFile[] keyFiles)
            : this(host, DefaultPort, username, ProxyTypes.None, string.Empty, 0, string.Empty, string.Empty, keyFiles)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrivateKeyConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="keyFiles">Connection key files.</param>
        public PrivateKeyConnectionInfo(string host, int port, string username, params PrivateKeyFile[] keyFiles)
            : this(host, port, username, ProxyTypes.None, string.Empty, 0, string.Empty, string.Empty, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="keyFiles">The key files.</param>
        public PrivateKeyConnectionInfo(string host, int port, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, params PrivateKeyFile[] keyFiles)
            : this(host, port, username, proxyType, proxyHost, proxyPort, string.Empty, string.Empty, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="keyFiles">The key files.</param>
        public PrivateKeyConnectionInfo(string host, int port, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername, params PrivateKeyFile[] keyFiles)
            : this(host, port, username, proxyType, proxyHost, proxyPort, proxyUsername, string.Empty, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="keyFiles">The key files.</param>
        public PrivateKeyConnectionInfo(string host, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, params PrivateKeyFile[] keyFiles)
            : this(host, DefaultPort, username, proxyType, proxyHost, proxyPort, string.Empty, string.Empty, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="keyFiles">The key files.</param>
        public PrivateKeyConnectionInfo(string host, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername, params PrivateKeyFile[] keyFiles)
            : this(host, DefaultPort, username, proxyType, proxyHost, proxyPort, proxyUsername, string.Empty, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="proxyPassword">The proxy password.</param>
        /// <param name="keyFiles">The key files.</param>
        public PrivateKeyConnectionInfo(string host, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername, string proxyPassword, params PrivateKeyFile[] keyFiles)
            : this(host, DefaultPort, username, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="proxyPassword">The proxy password.</param>
        /// <param name="keyFiles">The key files.</param>
        public PrivateKeyConnectionInfo(string host, int port, string username, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername, string proxyPassword, params PrivateKeyFile[] keyFiles)
            : base(host, port, username, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword, new PrivateKeyAuthenticationMethod(username, keyFiles))
        {
            KeyFiles = new Collection<PrivateKeyFile>(keyFiles);
        }

        #region IDisposable Members

        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                // Dispose managed resources.
                if (AuthenticationMethods != null)
                {
                    foreach (var authenticationMethod in AuthenticationMethods)
                    {
                        var disposable = authenticationMethod as IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="PasswordConnectionInfo"/> is reclaimed by garbage collection.
        /// </summary>
        ~PrivateKeyConnectionInfo()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
