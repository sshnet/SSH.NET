using System.Threading.Tasks;
using System.Linq;
using Renci.SshNet.Messages;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality to connect and interact with SSH server.
    /// </summary>
    public partial class Session
    {
        partial void HandleMessageCore(Message message)
        {
            HandleMessage((dynamic)message);
        }

        partial void InternalRegisterMessage(string messageName)
        {
            lock (_messagesMetadata)
            {
                Parallel.ForEach(
                    from m in _messagesMetadata where m.Name == messageName select m,
                    item => { item.Enabled = true; item.Activated = true; });
            }
        }

        partial void InternalUnRegisterMessage(string messageName)
        {
            lock (_messagesMetadata)
            {
                Parallel.ForEach(
                    from m in _messagesMetadata where m.Name == messageName select m,
                    item => { item.Enabled = false; item.Activated = false; });
            }
        }
    }
}
