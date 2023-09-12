namespace Renci.SshNet.IntegrationTests
{
    public class LinuxVMConnectionFactory : IConnectionInfoFactory
    {
        

        private const string ProxyHost = "127.0.0.1";
        private const int ProxyPort = 1234;
        private const string ProxyUserName = "test";
        private const string ProxyPassword = "123";
        private readonly string _host;
        private readonly int _port;
        private readonly AuthenticationMethodFactory _authenticationMethodFactory;


        public LinuxVMConnectionFactory(string sshServerHostName, ushort sshServerPort)
        {
            _host = sshServerHostName;
            _port = sshServerPort;

            _authenticationMethodFactory = new AuthenticationMethodFactory();
        }

        public LinuxVMConnectionFactory(string sshServerHostName, ushort sshServerPort, AuthenticationMethodFactory authenticationMethodFactory)
        {
            _host = sshServerHostName;
            _port = sshServerPort;

            _authenticationMethodFactory = authenticationMethodFactory;
        }

        public ConnectionInfo Create()
        {
            return Create(_authenticationMethodFactory.CreateRegularUserPrivateKeyAuthenticationMethod());
        }

        public ConnectionInfo Create(params AuthenticationMethod[] authenticationMethods)
        {
            return new ConnectionInfo(_host, _port, Users.Regular.UserName, authenticationMethods);
        }

        public ConnectionInfo CreateWithProxy()
        {
            return CreateWithProxy(_authenticationMethodFactory.CreateRegularUserPrivateKeyAuthenticationMethod());
        }

        public ConnectionInfo CreateWithProxy(params AuthenticationMethod[] authenticationMethods)
        {
            return new ConnectionInfo(
                _host,
                _port,
                Users.Regular.UserName,
                ProxyTypes.Socks4,
                ProxyHost,
                ProxyPort,
                ProxyUserName,
                ProxyPassword,
                authenticationMethods);
        }
    }
}

