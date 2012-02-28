using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// Gets the authentication error message.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationMethod"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="port">The port.</param>
        /// <param name="username">The username.</param>
        protected AuthenticationMethod(string host, int port, string username)
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
        /// <param name="session">The session to authenticate.</param>
        /// <returns></returns>
        public abstract AuthenticationResult Authenticate(Session session);
    }
}
