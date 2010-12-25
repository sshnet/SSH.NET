using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshClient.Messages.Authentication;
using Renci.SshClient.Common;
using Renci.SshClient.Messages;

namespace Renci.SshClient
{
    public class PasswordConnectionInfo : ConnectionInfo, IDisposable
    {
        private EventWaitHandle _authenticationCompleted = new AutoResetEvent(false);

        private Exception _exception;

        public override string Name
        {
            get
            {
                return "password";
            }
        }

        public string Password { get; private set; }

        public event EventHandler<AuthenticationPasswordChangeEventArgs> PasswordExpired;

        public PasswordConnectionInfo(string host, string username, string password)
            : this(host, 22, username, password)
        {

        }

        public PasswordConnectionInfo(string host, int port, string username, string password)
            : base(host, port, username)
        {
            this.Password = password;
        }

        protected override void OnAuthenticate()
        {
            this.Session.RegisterMessage<PasswordChangeRequiredMessage>();

            this.SendMessage(new RequestMessagePassword(ServiceNames.Connection, this.Username, this.Password));

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
                this.Session.UnRegisterMessage<PasswordChangeRequiredMessage>();

                var eventTask = Task.Factory.StartNew(() =>
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
                        this.SendMessage(new RequestMessagePassword(ServiceNames.Connection, this.Username, this.Password, eventArgs.NewPassword));
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

        ~PasswordConnectionInfo()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
