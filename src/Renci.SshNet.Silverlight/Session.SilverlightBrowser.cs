using Renci.SshNet.Messages;

namespace Renci.SshNet
{
    public partial class Session
    {
        partial void HandleMessageCore(Message message)
        {
            this.HandleMessage((dynamic) message);
        }
    }
}
