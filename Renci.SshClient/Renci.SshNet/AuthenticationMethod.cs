using System;
using System.Collections.Generic;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Base class for all supported authentication methods
    /// </summary>
    public abstract class AuthenticationMethod
    {
        /// <summary>
        /// Gets authentication method name
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets connection username.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Gets the authentication error message.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Gets list of allowed authentications.
        /// </summary>
        public IEnumerable<string> AllowedAuthentications { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationMethod"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <exception cref="ArgumentException"><paramref name="username"/> is whitespace or null.</exception>
        protected AuthenticationMethod(string username)
        {
            if (username.IsNullOrWhiteSpace())
                throw new ArgumentException("username");

            this.Username = username;
        }

        /// <summary>
        /// Authenticates the specified session.
        /// </summary>
        /// <param name="session">The session to authenticate.</param>
        /// <returns>Result of authentication  process.</returns>
        public abstract AuthenticationResult Authenticate(Session session);
    }
}
