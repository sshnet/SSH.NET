using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_BANNER message.
    /// </summary>
    public class BannerMessageTest : TestBase
    {
        /// <summary>
        /// Gets banner message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets banner language.
        /// </summary>
        public string Language { get; private set; }
    }
}