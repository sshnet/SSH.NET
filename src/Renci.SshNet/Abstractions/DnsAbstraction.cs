using System.Net;

namespace Renci.SshNet.Abstractions
{
    internal static class DnsAbstraction
    {
        /// <summary>
        /// Returns the Internet Protocol (IP) addresses for the specified host.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address to resolve</param>
        /// <returns>
        /// An array of type <see cref="IPAddress"/> that holds the IP addresses for the host that
        /// is specified by the <paramref name="hostNameOrAddress"/> parameter.
        /// </returns>
        public static IPAddress[] GetHostAddresses(string hostNameOrAddress)
        {
#if FEATURE_DNS_ASYNC
            return Dns.GetHostAddressesAsync(hostNameOrAddress).Result;
#else
            return Dns.GetHostAddresses(hostNameOrAddress);
#endif
        }

        /// <summary>
        /// Resolves a host name or IP address to an <see cref="IPHostEntry"/> instance.
        /// </summary>
        /// <param name="hostNameOrAddress">The host name or IP address to resolve.</param>
        /// <returns>
        /// An <see cref="IPHostEntry"/> instance that contains address information about the host
        /// specified in <paramref name="hostNameOrAddress"/>.
        /// </returns>
        public static IPHostEntry GetHostEntry(string hostNameOrAddress)
        {
#if FEATURE_DNS_ASYNC
            return Dns.GetHostEntryAsync(hostNameOrAddress).Result;
#else
            return Dns.GetHostEntry(hostNameOrAddress);
#endif
        }
    }
}
