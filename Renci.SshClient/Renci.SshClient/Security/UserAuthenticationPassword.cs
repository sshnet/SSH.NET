using System;
using System.Threading;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Authentication;
using System.Threading.Tasks;
using Renci.SshClient.Common;

namespace Renci.SshClient.Security
{
    internal class UserAuthenticationPassword : UserAuthentication, IDisposable
    {
        private EventWaitHandle _authenticationCompleted = new AutoResetEvent(false);

        private Exception _exception;

        private PasswordConnectionInfo _connectionInfo;

        public override string Name
        {
            get
            {
                return "password";
            }
        }

        protected override void OnAuthenticate()
        {
            this._connectionInfo = this.Session.ConnectionInfo as PasswordConnectionInfo;

            if (this._connectionInfo == null)
                return;

            this.Session.RegisterMessageType<PasswordChangeRequiredMessage>(MessageTypes.UserAuthenticationPasswordChangeRequired);

            this.SendMessage(new RequestMessagePassword
                {
                    ServiceName = ServiceNames.Connection,
                    Username = this.Username,
                    Password = this._connectionInfo.Password ?? string.Empty,
                });

            this.WaitHandle(this._authenticationCompleted);

            if (this._exception != null)
            {
                throw this._exception;
            }
        }

        protected override void Session_UserAuthenticationSuccessMessageReceived(object sender, MessageEventArgs<SuccessMessage> e)
        {
            base.Session_UserAuthenticationSuccessMessageReceived(sender, e);
            this._authenticationCompleted.Set();
        }

        protected override void Session_UserAuthenticationFailureReceived(object sender, MessageEventArgs<FailureMessage> e)
        {
            base.Session_UserAuthenticationFailureReceived(sender, e);
            this._authenticationCompleted.Set();
        }

        protected override void Session_MessageReceived(object sender, MessageEventArgs<Message> e)
        {
            base.Session_MessageReceived(sender, e);

            if (e.Message is PasswordChangeRequiredMessage)
            {
                this.Session.UnRegisterMessageType(MessageTypes.UserAuthenticationPasswordChangeRequired);

                var eventTask = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var eventArgs = new AuthenticationPasswordChangeEventArgs(this.Username);

                        //  Raise an event to allow user to supply a new password
                        this.RaiseAuthenticating(eventArgs);

                        //  Send new authentication request with new password
                        this.SendMessage(new RequestMessagePassword
                        {
                            ServiceName = ServiceNames.Connection,
                            Username = this.Username,
                            Password = this._connectionInfo.Password ?? string.Empty,
                            NewPassword = eventArgs.NewPassword ?? string.Empty,
                        });
                    }
                    catch (Exception exp)
                    {
                        this._exception = exp;
                        this._authenticationCompleted.Set();
                    }
                });
            }
        }

        #region IDisposable Members

        private bool isDisposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this._authenticationCompleted != null)
                    {
                        this._authenticationCompleted.Dispose();
                    }
                }

                // Note disposing has been done.
                isDisposed = true;
            }
        }

        ~UserAuthenticationPassword()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
