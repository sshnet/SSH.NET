#if NET6_0_OR_GREATER
using Renci.SshNet.Messages.Authentication;

namespace Renci.SshNet.Compression
{
    /// <summary>
    /// Represents "zlib@openssh.com" compression implementation.
    /// </summary>
    internal sealed class ZlibOpenSsh : Zlib
    {
        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "zlib@openssh.com"; }
        }

        /// <summary>
        /// Initializes the algorithm.
        /// </summary>
        /// <param name="session">The session.</param>
        public override void Init(Session session)
        {
            base.Init(session);

            IsActive = false;
            session.UserAuthenticationSuccessReceived += Session_UserAuthenticationSuccessReceived;
        }

        private void Session_UserAuthenticationSuccessReceived(object sender, MessageEventArgs<SuccessMessage> e)
        {
            IsActive = true;
            Session.UserAuthenticationSuccessReceived -= Session_UserAuthenticationSuccessReceived;
        }
    }
}
#endif
