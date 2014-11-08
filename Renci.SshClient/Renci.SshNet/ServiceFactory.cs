namespace Renci.SshNet
{
    internal class ServiceFactory : IServiceFactory
    {
        public ISession CreateSession(ConnectionInfo connectionInfo)
        {
            return new Session(connectionInfo);
        }
    }
}
