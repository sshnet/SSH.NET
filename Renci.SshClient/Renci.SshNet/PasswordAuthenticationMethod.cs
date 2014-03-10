using System;
using System.Linq;
using System.Text;
using System.Threading;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality to perform password authentication.
    /// </summary>
    public partial class PasswordAuthenticationMethod : AuthenticationMethod, IDisposable
    {
        private AuthenticationResult _authenticationResult = AuthenticationResult.Failure;

        private Session _session;

        private EventWaitHandle _authenticationCompleted = new AutoResetEvent(false);

        private Exception _exception;

        private readonly RequestMessage _requestMessage;

        private readonly byte[] _password;

        /// <summary>
        /// Gets authentication method name
        /// </summary>
        public override string Name
        {
            get { return this._requestMessage.MethodName; }
        }

        /// <summary>
        /// Occurs when user's password has expired and needs to be changed.
        /// </summary>
        public event EventHandler<AuthenticationPasswordChangeEventArgs> PasswordExpired;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordAuthenticationMethod"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <exception cref="ArgumentException"><paramref name="username"/> is whitespace or null.</exception>
        /// <exception cref="ArgumentException"><paramref name="password"/> is null.</exception>
        public PasswordAuthenticationMethod(string username, string password)
            : this(username, Encoding.UTF8.GetBytes(password))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordAuthenticationMethod"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <exception cref="ArgumentException"><paramref name="username"/> is whitespace or null.</exception>
        /// <exception cref="ArgumentException"><paramref name="password"/> is null.</exception>
        public PasswordAuthenticationMethod(string username, byte[] password)
            : base(username)
        {
            if (password == null)
                throw new ArgumentNullException("password");

            this._password = password;

            this._requestMessage = new RequestMessagePassword(ServiceName.Connection, this.Username, this._password);
        }

        /// <summary>
        /// Authenticates the specified session.
        /// </summary>
        /// <param name="session">The session to authenticate.</param>
        /// <returns>
        /// Result of authentication  process.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="session" /> is null.</exception>
        public override AuthenticationResult Authenticate(Session session)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            this._session = session;

            session.UserAuthenticationSuccessReceived += Session_UserAuthenticationSuccessReceived;
            session.UserAuthenticationFailureReceived += Session_UserAuthenticationFailureReceived;
            session.MessageReceived += Session_MessageReceived;

            session.RegisterMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");

            session.SendMessage(this._requestMessage);

            session.WaitOnHandle(this._authenticationCompleted);
            
            session.UserAuthenticationSuccessReceived -= Session_UserAuthenticationSuccessReceived;
            session.UserAuthenticationFailureReceived -= Session_UserAuthenticationFailureReceived;
            session.MessageReceived -= Session_MessageReceived;


            if (this._exception != null)
            {
                throw this._exception;
            }

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

        private void Session_MessageReceived(object sender, MessageEventArgs<Message> e)
        {
            if (e.Message is PasswordChangeRequiredMessage)
            {
                this._session.UnRegisterMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");

                this.ExecuteThread(() =>
                {
                    try
                    {
                        var eventArgs = new AuthenticationPasswordChangeEventArgs(this.Username);

                        //  Raise an event to allow user to supply a new password
                        if (this.PasswordExpired != null)
                        {
                            this.PasswordExpired(this, eventArgs);
                        }

                        //  Send new authentication request with new password
                        this._session.SendMessage(new RequestMessagePassword(ServiceName.Connection, this.Username, this._password, eventArgs.NewPassword));
                    }
                    catch (Exception exp)
                    {
                        this._exception = exp;
                        this._authenticationCompleted.Set();
                    }
                });
            }
        }

        partial void ExecuteThread(Action action);

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
        ~PasswordAuthenticationMethod()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion

    }
}
