using System;
using System.Threading;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Authentication;

namespace Renci.SshClient.Security
{
    internal class UserAuthenticationNone : UserAuthentication, IDisposable
    {
        private EventWaitHandle _authenticationCompleted = new AutoResetEvent(false);

        public override string Name
        {
            get { return "none"; }
        }

        public UserAuthenticationNone(Session session)
            : base(session)
        {

        }

        protected override bool Run()
        {
            this.Session.SendMessage(new RequestMessage
            {
                ServiceName = ServiceNames.Connection,
                Username = this.Session.ConnectionInfo.Username,
            });

            this.Session.WaitHandle(this._authenticationCompleted);

            return true;
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


        //protected override void HandleMessage<T>(T message)
        //{
        //}

        //protected override void HandleMessage(SuccessMessage message)
        //{
        //    base.HandleMessage(message);
        //    this._authenticationCompleted.Set();
        //}

        //protected override void HandleMessage(FailureMessage message)
        //{
        //    base.HandleMessage(message);
        //    this._authenticationCompleted.Set();
        //}

        #region IDisposable Members

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
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
                disposed = true;
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
