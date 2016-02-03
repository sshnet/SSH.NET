using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Renci.SshClient.Common;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Authentication;
using Renci.SshClient.Messages.Transport;
using Renci.SshClient.Security;

namespace Renci.SshClient.Services
{
    internal class UserAuthenticationService : Service
    {
        private IList<string> _executedMethods = new List<string>();

        private EventWaitHandle _serviceAccepted = new AutoResetEvent(false);

        private EventWaitHandle _authenticationCompleted = new AutoResetEvent(false);

        public override ServiceNames ServiceName
        {
            get { return ServiceNames.UserAuthentication; }
        }

        public EventWaitHandle AuthenticationCompletedHandle { get; private set; }

        public string ErrorMessage { get; private set; }

        public bool IsAuthenticated { get; private set; }

        public UserAuthenticationService(Session session)
            : base(session)
        {
            this.AuthenticationCompletedHandle = new AutoResetEvent(false);
        }

        public void AuthenticateUser()
        {
            //  Register Authentication response messages
            Message.RegisterMessageType<FailureMessage>(MessageTypes.UserAuthenticationFailure);
            Message.RegisterMessageType<SuccessMessage>(MessageTypes.UserAuthenticationSuccess);
            Message.RegisterMessageType<BannerMessage>(MessageTypes.UserAuthenticationBanner);

            //  Attach event handlers to handle messages
            this.Session.MessageReceived += SessionInfo_MessageReceived;

            //  Request user authorization service
            this.SendMessage(new ServiceRequestMessage
            {
                ServiceName = ServiceNames.UserAuthentication,
            });

            //  Wait for service to be accepted
            this.Session.WaitHandle(this._serviceAccepted);

            //  Start by quering supported authentication methods
            this.SendMessage(new RequestMessage
            {
                Username = this.Session.ConnectionInfo.Username,
                ServiceName = ServiceNames.Connection,
            });

            //  Wait for authentication to be completed
            this.Session.WaitHandle(this._authenticationCompleted);

            this.Session.MessageReceived -= SessionInfo_MessageReceived;

            Message.UnRegisterMessageType(MessageTypes.UserAuthenticationFailure);
            Message.UnRegisterMessageType(MessageTypes.UserAuthenticationSuccess);
            Message.UnRegisterMessageType(MessageTypes.UserAuthenticationBanner);
        }

        private void SessionInfo_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            this.HandleMessage((dynamic)e.Message);
        }

        private void HandleMessage<T>(T message)
        {
            //  Ignore messages that cannot be handled by this module
        }

        private void HandleMessage(ServiceAcceptMessage message)
        {
            if (message.ServiceName == ServiceNames.UserAuthentication)
            {
                this._serviceAccepted.Set();
            }
        }

        private void HandleMessage(SuccessMessage message)
        {
            this.AuthenticationSucceded();
        }

        private void HandleMessage(FailureMessage message)
        {
            if (message.PartialSuccess)
            {
                this.AuthenticationFailed(message.Message);
                return;
            }

            //  Get method that was not executed yet
            var methodsToTry = message.AllowedAuthentications.Except(this._executedMethods);

            if (methodsToTry.Count() == 0)
            {
                this.AuthenticationFailed(string.Format("User '{0}' cannot be authorized.", this.Session.ConnectionInfo.Username));
                return;
            }

            //  Execute authentication method
            foreach (var methodName in methodsToTry)
            {
                UserAuthentication userAuthentication = null;

                if (methodName == "publickey")
                {
                    userAuthentication = new UserAuthenticationPublicKey(this.Session);
                }
                else if (methodName == "password")
                {
                    userAuthentication = new UserAuthenticationPassword(this.Session);
                }
                this._executedMethods.Add(methodName);
                if (userAuthentication != null)
                {
                    if (userAuthentication.Start())
                        break;
                }
            }
        }

        private void HandleMessage(BannerMessage message)
        {
        }

        private void AuthenticationFailed(string message)
        {
            this.IsAuthenticated = false;
            this.ErrorMessage = message;
            this._authenticationCompleted.Set();
        }

        private void AuthenticationSucceded()
        {
            this.IsAuthenticated = true;
            this._authenticationCompleted.Set();
        }



    }
}
