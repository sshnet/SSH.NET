namespace Renci.SshNet
{
    internal interface IClientAuthentication
    {
        void Authenticate(IConnectionInfoInternal connectionInfo, ISession session);
    }
}
