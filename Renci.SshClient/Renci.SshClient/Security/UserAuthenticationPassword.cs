using System.Threading;
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Authentication;

namespace Renci.SshClient.Security
{
    internal class UserAuthenticationPassword : UserAuthentication
    {
        private EventWaitHandle _authenticationCompleted = new AutoResetEvent(false);

        public override string Name
        {
            get
            {
                return "password";
            }
        }

        public UserAuthenticationPassword(Session session)
            : base(session)
        {

        }

        protected override bool Run()
        {
            //  TODO:   Handle all user authentication messages
            //Message.RegisterMessageType<PasswordChangeRequiredMessage>(MessageTypes.UserAuthenticationPasswordChangeRequired);

            if (string.IsNullOrEmpty(this.Session.ConnectionInfo.Password))
                return false;

            this.Session.SendMessage(new PasswordRequestMessage
                {
                    ServiceName = ServiceNames.Connection,
                    Username = this.Session.ConnectionInfo.Username,
                    Password = this.Session.ConnectionInfo.Password,
                });

            this.Session.WaitHandle(this._authenticationCompleted);

            return true;
        }

        protected override void HandleMessage<T>(T message)
        {
            //  TODO:   Handle password specific messages
        }

        protected override void HandleMessage(SuccessMessage message)
        {
            base.HandleMessage(message);
            this._authenticationCompleted.Set();
        }

        protected override void HandleMessage(FailureMessage message)
        {
            base.HandleMessage(message);
            this._authenticationCompleted.Set();
        }
    }
}
