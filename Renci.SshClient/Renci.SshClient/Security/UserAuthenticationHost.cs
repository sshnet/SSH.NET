namespace Renci.SshClient.Security
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

        protected override bool Run()
        {
            throw new System.NotImplementedException();
        }

        protected override void HandleMessage<T>(T message)
        {
            throw new System.NotImplementedException();
        }
    }
}
