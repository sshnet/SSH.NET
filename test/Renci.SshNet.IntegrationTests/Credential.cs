namespace Renci.SshNet.IntegrationTests
{
    internal sealed class Credential
    {
        public Credential(string userName, string password)
        {
            UserName = userName;
            Password = password;
        }

        public string UserName { get; }
        public string Password { get; }
    }
}
