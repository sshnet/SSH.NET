using System.Threading.Tasks;
using System.Linq;
using System;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality to connect and interact with SSH server.
    /// </summary>
    public partial class Session
    {
        partial void ExecuteThread(Action action)
        {
            Task.Factory.StartNew(action, TaskCreationOptions.LongRunning);
        }

        partial void InternalRegisterMessage(string messageName)
        {
            lock (this._messagesMetadata)
            {
                Parallel.ForEach(
                    from m in this._messagesMetadata where m.Name == messageName select m,
                    (item) => { item.Enabled = true; item.Activated = true; });
            }
        }

        partial void InternalUnRegisterMessage(string messageName)
        {
            lock (this._messagesMetadata)
            {
                Parallel.ForEach(
                    from m in this._messagesMetadata where m.Name == messageName select m,
                    (item) => { item.Enabled = false; item.Activated = false; });
            }
        }

    }
}
