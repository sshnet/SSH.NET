using System;

using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Base class for all supported authentication methods.
    /// </summary>
    public abstract class AuthenticationMethod : IAuthenticationMethod, IDisposable
    {
        /// <summary>
        /// Gets the name of the authentication method.
        /// </summary>
        /// <value>
        /// The name of the authentication method.
        /// </value>
#pragma warning disable CA2119 // Seal methods that satisfy private interfaces
        public abstract string Name { get; }
#pragma warning restore CA2119 // Seal methods that satisfy private interfaces

        /// <summary>
        /// Gets connection username.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Gets or sets the list of allowed authentications.
        /// </summary>
        public string[] AllowedAuthentications { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationMethod"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <exception cref="ArgumentException"><paramref name="username"/> is whitespace or <see langword="null"/>.</exception>
        protected AuthenticationMethod(string username)
        {
            ThrowHelper.ThrowIfNullOrWhiteSpace(username);

            Username = username;
        }

        /// <summary>
        /// Authenticates the specified session.
        /// </summary>
        /// <param name="session">The session to authenticate.</param>
        /// <returns>
        /// The result of the authentication process.
        /// </returns>
        public abstract AuthenticationResult Authenticate(Session session);

        /// <summary>
        /// Authenticates the specified session.
        /// </summary>
        /// <param name="session">The session to authenticate.</param>
        /// <returns>
        /// The result of the authentication process.
        /// </returns>
        AuthenticationResult IAuthenticationMethod.Authenticate(ISession session)
        {
            return Authenticate((Session)session);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
