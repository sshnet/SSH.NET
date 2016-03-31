namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides data for <see cref="Renci.SshNet.ConnectionInfo.AuthenticationBanner"/> event.
    /// </summary>
    public class AuthenticationBannerEventArgs : AuthenticationEventArgs
    {
        /// <summary>
        /// Gets banner message.
        /// </summary>
        public string BannerMessage { get; private set; }

        /// <summary>
        /// Gets banner language.
        /// </summary>
        public string Language { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationBannerEventArgs"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="message">Banner message.</param>
        /// <param name="language">Banner language.</param>
        public AuthenticationBannerEventArgs(string username, string message, string language)
            : base(username)
        {
            BannerMessage = message;
            Language = language;
        }
    }
}
