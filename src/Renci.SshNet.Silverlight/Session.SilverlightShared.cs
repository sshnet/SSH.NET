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

        /// <summary>
        /// Closes the socket.
        /// </summary>
        /// <remarks>
        /// This method will wait up to <c>10</c> seconds to send any remaining data.
        /// </remarks>
        partial void SocketDisconnect()
        {
            _socket.Close(10);
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
