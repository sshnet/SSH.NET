using System.Net.Sockets;

namespace Renci.SshNet.Abstractions
{
    internal static class SocketAbstraction
    {
        public static bool CanRead(Socket socket)
        {
            if (socket.Connected)
            {
#if FEATURE_SOCKET_POLL
                return socket.Poll(-1, SelectMode.SelectRead) && socket.Available > 0;
#endif // FEATURE_SOCKET_POLL
            }

            return false;

        }

        public static bool CanWrite(Socket socket)
        {
            if (socket.Connected)
            {
#if FEATURE_SOCKET_POLL
                return socket.Poll(-1, SelectMode.SelectWrite);
#endif // FEATURE_SOCKET_POLL
            }

            return false;
        }
    }
}
