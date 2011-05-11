using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Renci.SshClient.Messages.Authentication;
using Renci.SshClient.Messages;

namespace Renci.SshClient
{
    /// <summary>
    /// Provides connection information when password authentication method is used
    /// </summary>
    public class NoneConnectionInfo : ConnectionInfo, IDisposable
    {
        private EventWaitHandle _authenticationCompleted = new AutoResetEvent(false);

        /// <summary>
        /// Gets list of allowed authentications.
        /// </summary>
        public IEnumerable<string> AllowedAuthentications { get; private set; }

        /// <summary>
        /// Gets connection name
        /// </summary>
        public override string Name
        {
            get
            {
                return "none";
            }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Connection username.</param>
        public NoneConnectionInfo(string host, string username)
            : this(host, 22, username)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Connection username.</param>
        public NoneConnectionInfo(string host, int port, string username)
            : base(host, port, username)
        {
        }

        /// <summary>
        /// Called when connection needs to be authenticated.
        /// </summary>
        protected override void OnAuthenticate()
        {
            this.SendMessage(new RequestMessageNone(ServiceNames.Connection, this.Username));

            this.WaitHandle(this._authenticationCompleted);
        }

        /// <summary>
        /// Handles the UserAuthenticationSuccessMessageReceived event of the session.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        protected override void Session_UserAuthenticationSuccessMessageReceived(object sender, MessageEventArgs<SuccessMessage> e)
        {
            base.Session_UserAuthenticationSuccessMessageReceived(sender, e);
            this._authenticationCompleted.Set();
        }

        /// <summary>
        /// Handles the UserAuthenticationFailureReceived event of the session.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        protected override void Session_UserAuthenticationFailureReceived(object sender, MessageEventArgs<FailureMessage> e)
        {
            base.Session_UserAuthenticationFailureReceived(sender, e);
            
            //  Copy allowed authentication methods
            this.AllowedAuthentications = e.Message.AllowedAuthentications.ToList();

            this._authenticationCompleted.Set();
        }

        #region IDisposable Members

        private bool isDisposed = false;

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
                        this._authenticationCompleted = null;
                    }
                }

                // Note disposing has been done.
                isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="PasswordConnectionInfo"/> is reclaimed by garbage collection.
        /// </summary>
        ~NoneConnectionInfo()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }

}
