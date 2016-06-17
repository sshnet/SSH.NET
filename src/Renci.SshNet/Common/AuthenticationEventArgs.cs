using System;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Base class for authentication events.
    /// </summary>
    public abstract class AuthenticationEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the username.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationEventArgs"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        protected AuthenticationEventArgs(string username)
        {
            Username = username;
        }
    }
}
