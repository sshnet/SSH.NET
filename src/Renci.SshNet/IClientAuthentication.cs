using System;

using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents a mechanism to authenticate a given client.
    /// </summary>
    internal interface IClientAuthentication
    {
        /// <summary>
        /// Attempts to perform authentication for a given <see cref="ISession"/> using the
        /// <see cref="IConnectionInfoInternal.AuthenticationMethods"/> of the specified
        /// <see cref="IConnectionInfoInternal"/>.
        /// </summary>
        /// <param name="connectionInfo">A <see cref="IConnectionInfoInternal"/> to use for authenticating.</param>
        /// <param name="session">The <see cref="ISession"/> for which to perform authentication.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> or <paramref name="session"/> is <see langword="null"/>.</exception>
        /// <exception cref="SshAuthenticationException">Failed to Authenticate the client.</exception>
        void Authenticate(IConnectionInfoInternal connectionInfo, ISession session);
    }
}
