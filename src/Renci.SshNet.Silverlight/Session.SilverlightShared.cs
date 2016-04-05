using System.Linq;

namespace Renci.SshNet
{
    public partial class Session
    {
        /// <summary>
        /// Gets a value indicating whether the socket is connected.
        /// </summary>
        /// <param name="isConnected"><c>true</c> if the socket is connected; otherwise, <c>false</c></param>
        partial void IsSocketConnected(ref bool isConnected)
        {
            isConnected = (_socket != null && _socket.Connected);
        }

        partial void InternalRegisterMessage(string messageName)
        {
            lock (_messagesMetadata)
            {
                foreach (var item in from m in _messagesMetadata where m.Name == messageName select m)
                {
                    item.Enabled = true;
                    item.Activated = true;
                }
            }
        }

        partial void InternalUnRegisterMessage(string messageName)
        {
            lock (_messagesMetadata)
            {
                foreach (var item in from m in _messagesMetadata where m.Name == messageName select m)
                {
                    item.Enabled = false;
                    item.Activated = false;
                }
            }
        }
    }
}
