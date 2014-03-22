using System;
using System.Linq;
using System.Threading;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality for "none" authentication method
    /// </summary>
    public class NoneAuthenticationMethod : AuthenticationMethod, IDisposable
    {
        private AuthenticationResult _authenticationResult = AuthenticationResult.Failure;

        private EventWaitHandle _authenticationCompleted = new AutoResetEvent(false);

        /// <summary>
        /// Gets connection name
        /// </summary>
        public override string Name
        {
            get { return "none"; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoneAuthenticationMethod"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <exception cref="ArgumentException"><paramref name="username"/> is whitespace or null.</exception>
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
        /// <exception cref="System.ArgumentNullException"><paramref name="session" /> is null.</exception>
        public override AuthenticationResult Authenticate(Session session)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            session.UserAuthenticationSuccessReceived += Session_UserAuthenticationSuccessReceived;
            session.UserAuthenticationFailureReceived += Session_UserAuthenticationFailureReceived;

            session.SendMessage(new RequestMessageNone(ServiceName.Connection, this.Username));

            session.WaitOnHandle(this._authenticationCompleted);

            session.UserAuthenticationSuccessReceived -= Session_UserAuthenticationSuccessReceived;
            session.UserAuthenticationFailureReceived -= Session_UserAuthenticationFailureReceived;

            return this._authenticationResult;
        }

        private void Session_UserAuthenticationSuccessReceived(object sender, MessageEventArgs<SuccessMessage> e)
        {
            this._authenticationResult = AuthenticationResult.Success;

            this._authenticationCompleted.Set();
        }

        private void Session_UserAuthenticationFailureReceived(object sender, MessageEventArgs<FailureMessage> e)
        {
            if (e.Message.PartialSuccess)
                this._authenticationResult = AuthenticationResult.PartialSuccess;
            else
                this._authenticationResult = AuthenticationResult.Failure;

            //  Copy allowed authentication methods
            this.AllowedAuthentications = e.Message.AllowedAuthentications.ToList();

            this._authenticationCompleted.Set();
        }
        
        #region IDisposable Members

        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this._isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this._authenticationCompleted != null)
                    {
                        this._authenticationCompleted.Dispose();
                        this._authenticationCompleted = null;
                    }
                }

                // Note disposing has been done.
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="PasswordConnectionInfo"/> is reclaimed by garbage collection.
        /// </summary>
        ~NoneAuthenticationMethod()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion

    }
}
