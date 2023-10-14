using System;
using System.Net;
using System.Text;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides connection information when password authentication method is used
    /// </summary>
    public class PasswordConnectionInfo : ConnectionInfo, IDisposable
    {
        private bool _isDisposed;

        /// <summary>
        /// Occurs when user's password has expired and needs to be changed.
        /// </summary>
        public event EventHandler<AuthenticationPasswordChangeEventArgs> PasswordExpired;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo" /> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="password">Connection password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is <c>null</c> or contains only whitespace characters.</exception>
        public PasswordConnectionInfo(string host, string username, string password)
            : this(host, DefaultPort, username, Encoding.UTF8.GetBytes(password))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="password">Connection password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is <c>null</c> or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="IPEndPoint.MinPort"/> and <see cref="IPEndPoint.MaxPort"/>.</exception>
        public PasswordConnectionInfo(string host, int port, string username, string password)
            : this(host, port, username, Encoding.UTF8.GetBytes(password), ProxyTypes.None, string.Empty, 0, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="password">Connection password.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        public PasswordConnectionInfo(string host, int port, string username, string password, ProxyTypes proxyType, string proxyHost, int proxyPort)
            : this(host, port, username, Encoding.UTF8.GetBytes(password), proxyType, proxyHost, proxyPort, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="password">Connection password.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        public PasswordConnectionInfo(string host, int port, string username, string password, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername)
            : this(host, port, username, Encoding.UTF8.GetBytes(password), proxyType, proxyHost, proxyPort, proxyUsername, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="password">Connection password.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        public PasswordConnectionInfo(string host, string username, string password, ProxyTypes proxyType, string proxyHost, int proxyPort)
            : this(host, DefaultPort, username, Encoding.UTF8.GetBytes(password), proxyType, proxyHost, proxyPort, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="password">Connection password.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        public PasswordConnectionInfo(string host, string username, string password, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername)
            : this(host, DefaultPort, username, Encoding.UTF8.GetBytes(password), proxyType, proxyHost, proxyPort, proxyUsername, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="password">Connection password.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="proxyPassword">The proxy password.</param>
        public PasswordConnectionInfo(string host, string username, string password, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername, string proxyPassword)
            : this(host, DefaultPort, username, Encoding.UTF8.GetBytes(password), proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="password">Connection password.</param>
        public PasswordConnectionInfo(string host, string username, byte[] password)
            : this(host, DefaultPort, username, password)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo" /> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="password">Connection password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host" /> is invalid, or <paramref name="username" /> is <c>null</c> or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port" /> is not within <see cref="IPEndPoint.MinPort" /> and <see cref="IPEndPoint.MaxPort" />.</exception>
        public PasswordConnectionInfo(string host, int port, string username, byte[] password)
            : this(host, port, username, password, ProxyTypes.None, string.Empty, 0, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="password">Connection password.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        public PasswordConnectionInfo(string host, int port, string username, byte[] password, ProxyTypes proxyType, string proxyHost, int proxyPort)
            : this(host, port, username, password, proxyType, proxyHost, proxyPort, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="password">Connection password.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        public PasswordConnectionInfo(string host, int port, string username, byte[] password, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername)
            : this(host, port, username, password, proxyType, proxyHost, proxyPort, proxyUsername, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="password">Connection password.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        public PasswordConnectionInfo(string host, string username, byte[] password, ProxyTypes proxyType, string proxyHost, int proxyPort)
            : this(host, DefaultPort, username, password, proxyType, proxyHost, proxyPort, string.Empty, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="password">Connection password.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        public PasswordConnectionInfo(string host, string username, byte[] password, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername)
            : this(host, DefaultPort, username, password, proxyType, proxyHost, proxyPort, proxyUsername, string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="password">Connection password.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="proxyPassword">The proxy password.</param>
        public PasswordConnectionInfo(string host, string username, byte[] password, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername, string proxyPassword)
            : this(host, DefaultPort, username, password, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">Connection username.</param>
        /// <param name="password">Connection password.</param>
        /// <param name="proxyType">Type of the proxy.</param>
        /// <param name="proxyHost">The proxy host.</param>
        /// <param name="proxyPort">The proxy port.</param>
        /// <param name="proxyUsername">The proxy username.</param>
        /// <param name="proxyPassword">The proxy password.</param>
        public PasswordConnectionInfo(string host, int port, string username, byte[] password, ProxyTypes proxyType, string proxyHost, int proxyPort, string proxyUsername, string proxyPassword)
            : base(host, port, username, proxyType, proxyHost, proxyPort, proxyUsername, proxyPassword, new PasswordAuthenticationMethod(username, password))
        {
            foreach (var authenticationMethod in AuthenticationMethods)
            {
                if (authenticationMethod is PasswordAuthenticationMethod pwdAuthentication)
                {
                    pwdAuthentication.PasswordExpired += AuthenticationMethod_PasswordExpired;
                }
            }
        }

        private void AuthenticationMethod_PasswordExpired(object sender, AuthenticationPasswordChangeEventArgs e)
        {
            PasswordExpired?.Invoke(sender, e);
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
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
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
                    foreach (var authenticationMethod in AuthenticationMethods)
                    {
                        if (authenticationMethod is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        ~PasswordConnectionInfo()
        {
            Dispose(disposing: false);
        }
    }
}
