namespace Renci.SshNet.IntegrationTests
{
    public sealed class LinuxAdminConnectionFactory : IConnectionInfoFactory
    {
        private readonly string _host;
        private readonly int _port;

        public LinuxAdminConnectionFactory(string sshServerHostName, ushort sshServerPort)
        {
            _host = sshServerHostName;
            _port = sshServerPort;
        }

        public ConnectionInfo Create()
        {
            var user = Users.Admin;
            return new ConnectionInfo(_host, _port, user.UserName, new PasswordAuthenticationMethod(user.UserName, user.Password));
        }

        public ConnectionInfo Create(params AuthenticationMethod[] authenticationMethods)
        {
#pragma warning disable MA0025 // Implement the functionality instead of throwing NotImplementedException
            throw new NotImplementedException();
#pragma warning restore MA0025 // Implement the functionality instead of throwing NotImplementedException
        }

        public ConnectionInfo CreateWithProxy()
        {
#pragma warning disable MA0025 // Implement the functionality instead of throwing NotImplementedException
            throw new NotImplementedException();
#pragma warning restore MA0025 // Implement the functionality instead of throwing NotImplementedException
        }

        public ConnectionInfo CreateWithProxy(params AuthenticationMethod[] authenticationMethods)
        {
#pragma warning disable MA0025 // Implement the functionality instead of throwing NotImplementedException
            throw new NotImplementedException();
#pragma warning restore MA0025 // Implement the functionality instead of throwing NotImplementedException
        }
    }
}
