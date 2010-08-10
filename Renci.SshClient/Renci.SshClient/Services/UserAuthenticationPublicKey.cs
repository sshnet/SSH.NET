
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

        public UserAuthenticationPublicKey(SessionInfo sessionInfo)
            : base(sessionInfo)
        {

        }


        public override bool Start()
        {
            if (this.SessionInfo.ConnectionInfo.KeyFile != null)
            {
                //  TODO:   Complete full public key implemention which includes other messages
                this.SendMessage(new PublicKeyRequestMessage
                {
                    ServiceName = ServiceNames.Connection,
                    Username = this.SessionInfo.ConnectionInfo.Username,
                    PublicKeyAlgorithmName = this.SessionInfo.ConnectionInfo.KeyFile.AlgorithmName,
                    PublicKeyData = this.SessionInfo.ConnectionInfo.KeyFile.PublicKey,
                    Signature = this.SessionInfo.ConnectionInfo.KeyFile.GetSignature(this.SessionInfo.SessionId),
                });
                return true;
            }
            return false;
        }
    }
}
