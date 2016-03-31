using System.Linq;
using System;
using Renci.SshNet.Messages;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Diagnostics;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides functionality to connect and interact with SSH server.
    /// </summary>
    public partial class Session
    {
        private static readonly Dictionary<Type, MethodInfo> _handlers;

        static Session()
        {
            _handlers = new Dictionary<Type, MethodInfo>();

            foreach (var method in typeof(Session).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(x => x.Name == "HandleMessage"))
            {
                if (method.IsGenericMethod) continue;

                var args = method.GetParameters();
                if (args.Length != 1) continue;

                var argType = args[0].ParameterType;
                if (!argType.IsSubclassOf(typeof(Message))) continue;

                _handlers.Add(argType, method);
            }
        }

        /// <summary>
        /// Handles SSH messages.
        /// </summary>
        /// <param name="message">The message.</param>
        partial void HandleMessageCore(Message message)
        {
            Debug.Assert(message != null);

            MethodInfo method;

            if (_handlers.TryGetValue(message.GetType(), out method))
            {
                try
                {
                    method.Invoke(this, new object[] { message });
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException ?? ex;
                }
            }
            else
            {
                HandleMessage(message);
            }
        }

        partial void InternalRegisterMessage(string messageName)
        {
            lock (this._messagesMetadata)
            {
                foreach (var m in from m in this._messagesMetadata where m.Name == messageName select m)
                {
                    m.Enabled = true; 
                    m.Activated = true;
                }
            }
        }

        partial void InternalUnRegisterMessage(string messageName)
        {
            lock (this._messagesMetadata)
            {
                foreach (var m in from m in this._messagesMetadata where m.Name == messageName select m)
                {
                    m.Enabled = false;
                    m.Activated = false;
                }
            }
        }
    }
}
