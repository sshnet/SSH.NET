
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
        public UserAuthenticationHost(Session session)
            : base(session)
        {

        }

        public override bool Start()
        {
            throw new System.NotImplementedException();
        }
    }
}
