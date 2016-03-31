﻿using System.Threading.Tasks;
using System.Linq;
using System;
using Renci.SshNet.Messages;
using System.Threading;

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

        /// <summary>
        /// Executes the specified action in a separate thread.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        partial void ExecuteThread(Action action)
        {
            ThreadPool.QueueUserWorkItem(o => action());
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
