using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;

using Renci.SshNet.Abstractions;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality to perform keyboard interactive authentication.
    /// </summary>
    public class KeyboardInteractiveAuthenticationMethod : AuthenticationMethod, IDisposable
    {
        private readonly RequestMessage _requestMessage;
        private AuthenticationResult _authenticationResult = AuthenticationResult.Failure;
        private Session _session;
        private EventWaitHandle _authenticationCompleted = new AutoResetEvent(initialState: false);
        private Exception _exception;
        private bool _isDisposed;

        /// <summary>
        /// Gets the name of the authentication method.
        /// </summary>
        /// <value>
        /// The name of the authentication method.
        /// </value>
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
            {
                ExceptionDispatchInfo.Capture(_exception).Throw();
            }

            return _authenticationResult;
        }

        private void Session_UserAuthenticationSuccessReceived(object sender, MessageEventArgs<SuccessMessage> e)
        {
            _authenticationResult = AuthenticationResult.Success;
            _ = _authenticationCompleted.Set();
        }

        private void Session_UserAuthenticationFailureReceived(object sender, MessageEventArgs<FailureMessage> e)
        {
            if (e.Message.PartialSuccess)
            {
                _authenticationResult = AuthenticationResult.PartialSuccess;
            }
            else
            {
                _authenticationResult = AuthenticationResult.Failure;
            }

            // Copy allowed authentication methods
            AllowedAuthentications = e.Message.AllowedAuthentications;

            _ = _authenticationCompleted.Set();
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
                        AuthenticationPrompt?.Invoke(this, eventArgs);

                        var informationResponse = new InformationResponseMessage();

                        foreach (var response in from r in eventArgs.Prompts orderby r.Id ascending select r.Response)
                        {
                            informationResponse.Responses.Add(response);
                        }

                        // Send information response message
                        _session.SendMessage(informationResponse);
                    }
                    catch (Exception exp)
                    {
                        _exception = exp;
                        _ = _authenticationCompleted.Set();
                    }
                });
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }

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
    }
}
