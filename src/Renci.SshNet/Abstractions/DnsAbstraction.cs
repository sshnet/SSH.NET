using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshNet.Abstractions
{
    internal static class DnsAbstraction
    {
        /// <summary>
        /// Returns the Internet Protocol (IP) addresses for the specified host.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        /// <returns>
        /// An array of type <see cref="IPAddress"/> that holds the IP addresses for the host that
        /// is specified by the <paramref name="hostNameOrAddress"/> parameter.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="hostNameOrAddress"/> is <see langword="null"/>.</exception>
        /// <exception cref="SocketException">An error is encountered when resolving <paramref name="hostNameOrAddress"/>.</exception>
        public static IPAddress[] GetHostAddresses(string hostNameOrAddress)
        {
            /* TODO Eliminate sync variant, and implement timeout */

            return Dns.GetHostAddresses(hostNameOrAddress);
        }

        /// <summary>
        /// Returns the Internet Protocol (IP) addresses for the specified host.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to signal the asynchronous operation should be canceled.</param>
        /// <returns>
        /// A task with result of an array of type <see cref="IPAddress"/> that holds the IP addresses for the host that
        /// is specified by the <paramref name="hostNameOrAddress"/> parameter.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="hostNameOrAddress"/> is <see langword="null"/>.</exception>
        /// <exception cref="SocketException">An error is encountered when resolving <paramref name="hostNameOrAddress"/>.</exception>
        public static Task<IPAddress[]> GetHostAddressesAsync(string hostNameOrAddress, CancellationToken cancellationToken)
        {
#if NET6_0_OR_GREATER
            return Dns.GetHostAddressesAsync(hostNameOrAddress, cancellationToken);
#else
            return Dns.GetHostAddressesAsync(hostNameOrAddress);
#endif
        }
    }
}
