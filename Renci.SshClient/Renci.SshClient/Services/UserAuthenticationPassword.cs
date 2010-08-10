using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Authentication;

namespace Renci.SshClient.Services
{
    internal class UserAuthenticationPassword : UserAuthentication
    {
        public override string Name
        {
            get
            {
                return "password";
            }
        }

        public UserAuthenticationPassword(SessionInfo sessionInfo)
            : base(sessionInfo)
        {

        }

        public override bool Start()
        {
            //  TODO:   Handle all user authentication messages
            //Message.RegisterMessageType<PasswordChangeRequiredMessage>(MessageTypes.UserAuthenticationPasswordChangeRequired);

            if (!string.IsNullOrEmpty(this.SessionInfo.ConnectionInfo.Password))
            {
                this.SendMessage(new PasswordRequestMessage
                {
                    ServiceName = ServiceNames.Connection,
                    Username = this.SessionInfo.ConnectionInfo.Username,
                    Password = this.SessionInfo.ConnectionInfo.Password,
                });
                return true;
            }
            return false;
        }
    }
}
