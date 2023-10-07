namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides data for <see cref="PasswordConnectionInfo.PasswordExpired"/> event.
    /// </summary>
    public class AuthenticationPasswordChangeEventArgs : AuthenticationEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationPasswordChangeEventArgs"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        public AuthenticationPasswordChangeEventArgs(string username)
            : base(username)
        {
        }

        /// <summary>
        /// Gets or sets the new password.
        /// </summary>
        /// <value>
        /// The new password.
        /// </value>
        public byte[] NewPassword { get; set; }
    }
}
