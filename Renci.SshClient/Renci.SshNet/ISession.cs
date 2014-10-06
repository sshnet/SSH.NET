using System;
using Renci.SshNet.Messages.Authentication;

namespace Renci.SshNet
{
    public interface ISession
    {
        void RegisterMessage(string messageName);
        void UnRegisterMessage(string messageName);
        event EventHandler<MessageEventArgs<BannerMessage>> UserAuthenticationBannerReceived;
    }
}
