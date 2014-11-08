namespace Renci.SshNet
{
    internal interface IServiceFactory
    {
        ISession CreateSession(ConnectionInfo connectionInfo);
    }
}
