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

        protected override bool Run()
        {
            throw new System.NotImplementedException();
        }
    }
}
