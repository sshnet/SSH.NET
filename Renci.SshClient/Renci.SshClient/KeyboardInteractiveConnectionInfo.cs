using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Renci.SshClient.Messages.Authentication;
using Renci.SshClient.Messages;
using Renci.SshClient.Common;
using System.Threading.Tasks;

namespace Renci.SshClient
{
    /// <summary>
    /// Provides connection information when keyboard interactive authentication method is used
    /// </summary>
    public class KeyboardInteractiveConnectionInfo : ConnectionInfo, IDisposable
    {
        private EventWaitHandle _authenticationCompleted = new AutoResetEvent(false);

        private Exception _exception;

        /// <summary>
        /// Gets connection name
        /// </summary>
        public override string Name
        {
            get
            {
                return "keyboard-interactive";
            }
        }

        /// <summary>
        /// Occurs when server prompts for more authentication information.
        /// </summary>
        public event EventHandler<AuthenticationPromptEventArgs> AuthenticationPrompt;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardInteractiveConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="username">The username.</param>
        public KeyboardInteractiveConnectionInfo(string host, string username)
            : this(host, 22, username)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardInteractiveConnectionInfo"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Connection username.</param>
        public KeyboardInteractiveConnectionInfo(string host, int port, string username)
            : base(host, port, username)
        {
        }

        /// <summary>
        /// Called when connection needs to be authenticated.
        /// </summary>
        protected override void OnAuthenticate()
        {
            this.Session.RegisterMessage("SSH_MSG_USERAUTH_INFO_REQUEST");

            this.Session.SendMessage(new RequestMessageKeyboardInteractive(ServiceNames.Connection, this.Username));

            this.WaitHandle(this._authenticationCompleted);

            this.Session.UnRegisterMessage("SSH_MSG_USERAUTH_INFO_REQUEST");

            if (this._exception != null)
            {
                throw this._exception;
            }
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
            this._authenticationCompleted.Set();
        }

        /// <summary>
        /// Handles the MessageReceived event of the session.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event data.</param>
        protected override void Session_MessageReceived(object sender, MessageEventArgs<Message> e)
        {
            var informationRequestMessage = e.Message as InformationRequestMessage;
            if (informationRequestMessage != null)
            {
                var eventArgs = new AuthenticationPromptEventArgs(this.Username, informationRequestMessage.Instruction, informationRequestMessage.Language, informationRequestMessage.Prompts);

                var eventTask = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (this.AuthenticationPrompt != null)
                        {
                            this.AuthenticationPrompt(this, eventArgs);
                        }

                        var informationResponse = new InformationResponseMessage();

                        foreach (var response in from r in eventArgs.Prompts orderby r.Id ascending select r.Response)
                        {
                            informationResponse.Responses.Add(response);
                        }

                        //  Send information response message
                        this.SendMessage(informationResponse);
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
        /// <see cref="KeyboardInteractiveConnectionInfo"/> is reclaimed by garbage collection.
        /// </summary>
        ~KeyboardInteractiveConnectionInfo()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
