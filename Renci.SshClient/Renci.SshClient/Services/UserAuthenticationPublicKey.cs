
using Renci.SshClient.Messages;
using Renci.SshClient.Messages.Authentication;
namespace Renci.SshClient.Services
{
    internal class UserAuthenticationPublicKey : UserAuthentication
    {
        public override string Name
        {
            get
            {
                return "publickey";
            }
        }

        public UserAuthenticationPublicKey(Session session)
            : base(session)
        {

        }


        public override bool Start()
        {
            if (this.Session.ConnectionInfo.KeyFile != null)
            {
                //  TODO:   Complete full public key implemention which includes other messages
                this.SendMessage(new PublicKeyRequestMessage
                {
                    ServiceName = ServiceNames.Connection,
                    Username = this.Session.ConnectionInfo.Username,
                    PublicKeyAlgorithmName = this.Session.ConnectionInfo.KeyFile.AlgorithmName,
                    PublicKeyData = this.Session.ConnectionInfo.KeyFile.PublicKey,
                    Signature = this.Session.ConnectionInfo.KeyFile.GetSignature(this.Session.SessionId),
                });
                return true;
            }
            return false;
        }
    }
}
