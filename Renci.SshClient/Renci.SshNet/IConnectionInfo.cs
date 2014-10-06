using System.Collections.Generic;
using Renci.SshNet.Messages.Authentication;

namespace Renci.SshNet
{
    internal interface IConnectionInfo
    {
        /// <summary>
        /// Gets the supported authentication methods for this connection.
        /// </summary>
        /// <value>
        /// The supported authentication methods for this connection.
        /// </value>
        IEnumerable<IAuthenticationMethod> AuthenticationMethods { get; }

        void UserAuthenticationBannerReceived(object sender, MessageEventArgs<BannerMessage> e);

        /// <summary>
        /// Creates a <see cref="NoneAuthenticationMethod"/> for the credentials represented
        /// by the current <see cref="IConnectionInfo"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="NoneAuthenticationMethod"/> for the credentials represented by the
        /// current <see cref="IConnectionInfo"/>.
        /// </returns>
        IAuthenticationMethod CreateNoneAuthenticationMethod();
    }
}
