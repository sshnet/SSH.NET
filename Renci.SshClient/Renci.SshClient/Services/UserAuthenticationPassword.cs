using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Authentication;

namespace Renci.SshClient.Security
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

        public UserAuthenticationPassword(Session session)
            : base(session)
        {

        }

        public override bool Start()
        {
            //  TODO:   Handle all user authentication messages
            //Message.RegisterMessageType<PasswordChangeRequiredMessage>(MessageTypes.UserAuthenticationPasswordChangeRequired);

            if (!string.IsNullOrEmpty(this.Session.ConnectionInfo.Password))
            {
                this.SendMessage(new PasswordRequestMessage
                {
                    ServiceName = ServiceNames.Connection,
                    Username = this.Session.ConnectionInfo.Username,
                    Password = this.Session.ConnectionInfo.Password,
                });
                return true;
            }
            return false;
        }
    }
}
