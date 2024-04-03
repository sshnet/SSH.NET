namespace Renci.SshNet.IntegrationTests
{
    public static class Users
    {
        public static readonly Credential Regular = new Credential("sshnet", "ssh4ever");
        public static readonly Credential Admin = new Credential("sshnetadm", "ssh4ever");
    }
}
