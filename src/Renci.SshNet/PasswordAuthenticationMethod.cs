using System;
using System.Text;
using System.Threading;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality to perform password authentication.
    /// </summary>
    public class PasswordAuthenticationMethod : AuthenticationMethod, IDisposable
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
            get { return _requestMessage.MethodName; }
        }

        /// <summary>
        /// Gets the password as a sequence of bytes.
        /// </summary>
        /// <value>
        /// The password as a sequence of bytes.
        /// </value>
        internal byte[] Password
        {
            get { return _password; }
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
        /// <exception cref="ArgumentException"><paramref name="username"/> is whitespace or <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="password"/> is <c>null</c>.</exception>
        public PasswordAuthenticationMethod(string username, string password)
            : this(username, Encoding.UTF8.GetBytes(password))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordAuthenticationMethod"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <exception cref="ArgumentException"><paramref name="username"/> is whitespace or <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="password"/> is <c>null</c>.</exception>
        public PasswordAuthenticationMethod(string username, byte[] password)
            : base(username)
        {
            if (password == null)
                throw new ArgumentNullException("password");

            _password = password;
            _requestMessage = new RequestMessagePassword(ServiceName.Connection, Username, _password);
        }

        /// <summary>
        /// Authenticates the specified session.
        /// </summary>
        /// <param name="session">The session to authenticate.</param>
        /// <returns>
        /// Result of authentication  process.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="session" /> is <c>null</c>.</exception>
        public override AuthenticationResult Authenticate(Session session)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            _session = session;

            session.UserAuthenticationSuccessReceived += Session_UserAuthenticationSuccessReceived;
            session.UserAuthenticationFailureReceived += Session_UserAuthenticationFailureReceived;
            session.UserAuthenticationPasswordChangeRequiredReceived += Session_UserAuthenticationPasswordChangeRequiredReceived;

            try
            {
                session.RegisterMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
                session.SendMessage(_requestMessage);
                session.WaitOnHandle(_authenticationCompleted);
            }
            finally 
            {
                session.UnRegisterMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");
                session.UserAuthenticationSuccessReceived -= Session_UserAuthenticationSuccessReceived;
                session.UserAuthenticationFailureReceived -= Session_UserAuthenticationFailureReceived;
                session.UserAuthenticationPasswordChangeRequiredReceived -= Session_UserAuthenticationPasswordChangeRequiredReceived;
            }

            if (_exception != null)
                throw _exception;

            return _authenticationResult;
        }

        private void Session_UserAuthenticationSuccessReceived(object sender, MessageEventArgs<SuccessMessage> e)
        {
            _authenticationResult = AuthenticationResult.Success;

            _authenticationCompleted.Set();
        }

        private void Session_UserAuthenticationFailureReceived(object sender, MessageEventArgs<FailureMessage> e)
        {
            if (e.Message.PartialSuccess)
                _authenticationResult = AuthenticationResult.PartialSuccess;
            else
                _authenticationResult = AuthenticationResult.Failure;

            //  Copy allowed authentication methods
            AllowedAuthentications = e.Message.AllowedAuthentications;

            _authenticationCompleted.Set();
        }

        private void Session_UserAuthenticationPasswordChangeRequiredReceived(object sender, MessageEventArgs<PasswordChangeRequiredMessage> e)
        {
            _session.UnRegisterMessage("SSH_MSG_USERAUTH_PASSWD_CHANGEREQ");

            ThreadAbstraction.ExecuteThread(() =>
            {
                try
                {
                    var eventArgs = new AuthenticationPasswordChangeEventArgs(Username);

                    //  Raise an event to allow user to supply a new password
                    if (PasswordExpired != null)
                    {
                        PasswordExpired(this, eventArgs);
                    }

                    //  Send new authentication request with new password
                    _session.SendMessage(new RequestMessagePassword(ServiceName.Connection, Username, _password, eventArgs.NewPassword));
                }
                catch (Exception exp)
                {
                    _exception = exp;
                    _authenticationCompleted.Set();
                }
            });
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
            if (_isDisposed)
                return;
           
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
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="PasswordAuthenticationMethod"/> is reclaimed by garbage collection.
        /// </summary>
        ~PasswordAuthenticationMethod()
        {
            Dispose(false);
        }

        #endregion
    }
}
