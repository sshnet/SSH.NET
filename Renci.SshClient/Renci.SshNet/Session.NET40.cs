using System.Threading.Tasks;
using System.Linq;
using System;
using System.Net.Sockets;
using System.Net;
using Renci.SshNet.Messages;
using Renci.SshNet.Common;
using System.Threading;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality to connect and interact with SSH server.
    /// </summary>
    public partial class Session
    {
        partial void HandleMessageCore(Message message)
        {
            this.HandleMessage((dynamic)message);
        }

        partial void ExecuteThread(Action action)
        {
            ThreadPool.QueueUserWorkItem((o) => { action(); });
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
