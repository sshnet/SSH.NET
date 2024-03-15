namespace Renci.SshNet.IntegrationTests
{
    internal class LinuxAdminConnectionFactory : IConnectionInfoFactory
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
            throw new NotImplementedException();
        }

        public ConnectionInfo CreateWithProxy()
        {
            throw new NotImplementedException();
        }

        public ConnectionInfo CreateWithProxy(params AuthenticationMethod[] authenticationMethods)
        {
            throw new NotImplementedException();
        }
    }
}

