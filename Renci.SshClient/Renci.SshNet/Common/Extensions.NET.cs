using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Renci.SshNet
{
    /// <summary>
    /// Collection of different extension method specific for .NET 4.0
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Determines whether [is null or white space] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is null or white space] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsNullOrWhiteSpace(this string value)
        {
            if (string.IsNullOrEmpty(value)) return true;

            return value.All(char.IsWhiteSpace);
        }
        
        internal static bool CanRead(this Socket socket)
        {
            return socket.Connected && socket.Poll(-1, SelectMode.SelectRead) && socket.Available > 0;
        }

        internal static bool CanWrite(this Socket socket)
        {
            return socket.Connected && socket.Poll(-1, SelectMode.SelectWrite);
        }

        internal static IPAddress GetIPAddress(this string host)
        {
            IPAddress ipAddress;
            if (!IPAddress.TryParse(host, out ipAddress))
                ipAddress = Dns.GetHostAddresses(host).First();

            return ipAddress;
        }
    }
}
