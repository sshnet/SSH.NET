
namespace Renci.SshClient.Services
{
    internal class UserAuthenticationHost : UserAuthentication
    {
        public override string Name
        {
            get
            {
                return "hostbased";
            }
        }
        public UserAuthenticationHost(SessionInfo sessionInfo)
            : base(sessionInfo)
        {

        }

        public override bool Start()
        {
            throw new System.NotImplementedException();
        }
    }
}
