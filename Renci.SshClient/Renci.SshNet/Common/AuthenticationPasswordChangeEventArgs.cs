using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Renci.SshClient.Common
{
    /// <summary>
    /// Provides data for <see cref="Renci.SshClient.PasswordConnectionInfo.PasswordExpired"/> event.
    /// </summary>
    public class AuthenticationPasswordChangeEventArgs : AuthenticationEventArgs
    {
        /// <summary>
        /// Gets or sets the new password.
        /// </summary>
        /// <value>
        /// The new password.
        /// </value>
        public string NewPassword { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationPasswordChangeEventArgs"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        public AuthenticationPasswordChangeEventArgs(string username)
            : base(username)
        {
        }
    }
}
