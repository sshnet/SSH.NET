using System;
using System.Linq;
using System.Threading;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Authentication;
using Renci.SshClient.Common;
using System.Threading.Tasks;

namespace Renci.SshClient.Security
{
    internal class UserAuthenticationKeyboardInteractive : UserAuthentication, IDisposable
    {
        private EventWaitHandle _authenticationCompleted = new AutoResetEvent(false);

        private Exception _exception;

        public override string Name
        {
            get
            {
                return "keyboard-interactive";
            }
        }

        protected override void OnAuthenticate()
        {
            this.Session.RegisterMessageType<InformationRequestMessage>(MessageTypes.UserAuthenticationInformationRequest);

            this.Session.SendMessage(new RequestMessageKeyboardInteractive
                {
                    ServiceName = ServiceNames.Connection,
                    Username = this.Session.ConnectionInfo.Username,
                });

            this.WaitHandle(this._authenticationCompleted);

            this.Session.UnRegisterMessageType(MessageTypes.UserAuthenticationInformationRequest);

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
            var informationRequestMessage = e.Message as InformationRequestMessage;
            if (informationRequestMessage != null)
            {
                var eventArgs = new AuthenticationPromptEventArgs(this.Username, informationRequestMessage.Instruction, informationRequestMessage.Language, informationRequestMessage.Prompts);

                var eventTask = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        this.RaiseAuthenticating(eventArgs);

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

        ~UserAuthenticationKeyboardInteractive()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
