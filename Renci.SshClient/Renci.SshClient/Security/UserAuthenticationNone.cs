using System;
using System.Threading;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Authentication;
using System.Collections.Generic;

namespace Renci.SshClient.Security
{
    internal class UserAuthenticationNone : UserAuthentication, IDisposable
    {
        private EventWaitHandle _authenticationCompleted = new AutoResetEvent(false);

        public override string Name
        {
            get { return "none"; }
        }

        public IEnumerable<string> Methods { get; private set; }

        protected override void OnAuthenticate()
        {
            this.SendMessage(new RequestMessage(ServiceNames.Connection, this.Username));

            this.WaitHandle(this._authenticationCompleted);
        }

        protected override void Session_UserAuthenticationSuccessMessageReceived(object sender, MessageEventArgs<SuccessMessage> e)
        {
            base.Session_UserAuthenticationSuccessMessageReceived(sender, e);
            this._authenticationCompleted.Set();
        }

        protected override void Session_UserAuthenticationFailureReceived(object sender, MessageEventArgs<FailureMessage> e)
        {
            base.Session_UserAuthenticationFailureReceived(sender, e);
            this.Methods = e.Message.AllowedAuthentications;
            this._authenticationCompleted.Set();
        }

        #region IDisposable Members

        private bool _isDisposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

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
                    }
                }

                // Note disposing has been done.
                _isDisposed = true;
            }
        }

        ~UserAuthenticationNone()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
