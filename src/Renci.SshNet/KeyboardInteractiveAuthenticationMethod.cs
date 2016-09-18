using System;
using System.Linq;
using System.Threading;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality to perform keyboard interactive authentication.
    /// </summary>
    public class KeyboardInteractiveAuthenticationMethod : AuthenticationMethod, IDisposable
    {
        private AuthenticationResult _authenticationResult = AuthenticationResult.Failure;

        private Session _session;
        private EventWaitHandle _authenticationCompleted = new AutoResetEvent(false);
        private Exception _exception;
        private readonly RequestMessage _requestMessage;

        /// <summary>
        /// Gets authentication method name
        /// </summary>
        public override string Name
        {
            get { return _requestMessage.MethodName; }
        }

        /// <summary>
        /// Occurs when server prompts for more authentication information.
        /// </summary>
        public event EventHandler<AuthenticationPromptEventArgs> AuthenticationPrompt;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyboardInteractiveAuthenticationMethod"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <exception cref="ArgumentException"><paramref name="username"/> is whitespace or <c>null</c>.</exception>
        public KeyboardInteractiveAuthenticationMethod(string username)
            : base(username)
        {
            _requestMessage = new RequestMessageKeyboardInteractive(ServiceName.Connection, username);
        }

        /// <summary>
        /// Authenticates the specified session.
        /// </summary>
        /// <param name="session">The session to authenticate.</param>
        /// <returns>Result of authentication  process.</returns>
        public override AuthenticationResult Authenticate(Session session)
        {
            _session = session;

            session.UserAuthenticationSuccessReceived += Session_UserAuthenticationSuccessReceived;
            session.UserAuthenticationFailureReceived += Session_UserAuthenticationFailureReceived;
            session.UserAuthenticationInformationRequestReceived += Session_UserAuthenticationInformationRequestReceived;

            session.RegisterMessage("SSH_MSG_USERAUTH_INFO_REQUEST");

            try
            {
                session.SendMessage(_requestMessage);
                session.WaitOnHandle(_authenticationCompleted);
            }
            finally
            {
                session.UnRegisterMessage("SSH_MSG_USERAUTH_INFO_REQUEST");
                session.UserAuthenticationSuccessReceived -= Session_UserAuthenticationSuccessReceived;
                session.UserAuthenticationFailureReceived -= Session_UserAuthenticationFailureReceived;
                session.UserAuthenticationInformationRequestReceived -= Session_UserAuthenticationInformationRequestReceived;
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

            // Copy allowed authentication methods
            AllowedAuthentications = e.Message.AllowedAuthentications;

            _authenticationCompleted.Set();
        }

        private void Session_UserAuthenticationInformationRequestReceived(object sender, MessageEventArgs<InformationRequestMessage> e)
        {
            var informationRequestMessage = e.Message;

            var eventArgs = new AuthenticationPromptEventArgs(Username,
                                                              informationRequestMessage.Instruction,
                                                              informationRequestMessage.Language,
                                                              informationRequestMessage.Prompts);

            ThreadAbstraction.ExecuteThread(() =>
                {
                    try
                    {
                        if (AuthenticationPrompt != null)
                        {
                            AuthenticationPrompt(this, eventArgs);
                        }

                        var informationResponse = new InformationResponseMessage();

                        foreach (var response in from r in eventArgs.Prompts orderby r.Id ascending select r.Response)
                        {
                            informationResponse.Responses.Add(response);
                        }

                        //  Send information response message
                        _session.SendMessage(informationResponse);
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
                    _authenticationCompleted = null;
                    authenticationCompleted.Dispose();
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="KeyboardInteractiveAuthenticationMethod"/> is reclaimed by garbage collection.
        /// </summary>
        ~KeyboardInteractiveAuthenticationMethod()
        {
            Dispose(false);
        }

        #endregion
    }
}
