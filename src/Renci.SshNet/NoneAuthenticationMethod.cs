using System;
using System.Threading;

using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for "none" authentication method.
    /// </summary>
    public class NoneAuthenticationMethod : AuthenticationMethod
    {
        private AuthenticationResult _authenticationResult = AuthenticationResult.Failure;
        private EventWaitHandle _authenticationCompleted = new AutoResetEvent(initialState: false);
        private bool _isDisposed;

        /// <summary>
        /// Gets the name of the authentication method.
        /// </summary>
        public override string Name
        {
            get { return "none"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoneAuthenticationMethod"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <exception cref="ArgumentException"><paramref name="username"/> is whitespace or <see langword="null"/>.</exception>
        public NoneAuthenticationMethod(string username)
            : base(username)
        {
        }

        /// <summary>
        /// Authenticates the specified session.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>
        /// Result of authentication  process.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="session" /> is <see langword="null"/>.</exception>
        public override AuthenticationResult Authenticate(Session session)
        {
            ThrowHelper.ThrowIfNull(session);

            session.UserAuthenticationSuccessReceived += Session_UserAuthenticationSuccessReceived;
            session.UserAuthenticationFailureReceived += Session_UserAuthenticationFailureReceived;

            try
            {
                session.SendMessage(new RequestMessageNone(ServiceName.Connection, Username));
                session.WaitOnHandle(_authenticationCompleted);
            }
            finally
            {
                session.UserAuthenticationSuccessReceived -= Session_UserAuthenticationSuccessReceived;
                session.UserAuthenticationFailureReceived -= Session_UserAuthenticationFailureReceived;
            }

            return _authenticationResult;
        }

        private void Session_UserAuthenticationSuccessReceived(object sender, MessageEventArgs<SuccessMessage> e)
        {
            _authenticationResult = AuthenticationResult.Success;

            _ = _authenticationCompleted.Set();
        }

        private void Session_UserAuthenticationFailureReceived(object sender, MessageEventArgs<FailureMessage> e)
        {
            if (e.Message.PartialSuccess)
            {
                _authenticationResult = AuthenticationResult.PartialSuccess;
            }
            else
            {
                _authenticationResult = AuthenticationResult.Failure;
            }

            // Copy allowed authentication methods
            AllowedAuthentications = e.Message.AllowedAuthentications;

            _ = _authenticationCompleted.Set();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                var authenticationCompleted = _authenticationCompleted;
                if (authenticationCompleted != null)
                {
                    authenticationCompleted.Dispose();
                    _authenticationCompleted = null;
                }

                _isDisposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
