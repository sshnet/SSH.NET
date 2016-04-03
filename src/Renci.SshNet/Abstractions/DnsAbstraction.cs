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
#if FEATURE_DNS_SYNC
            return Dns.GetHostAddresses(hostNameOrAddress);
#elif FEATURE_DNS_TAP
            return Dns.GetHostAddressesAsync(hostNameOrAddress).Result;
#else
            #error Retrieving IP addresses for a given host is not implemented.
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
#if FEATURE_DNS_SYNC
            return Dns.GetHostEntry(hostNameOrAddress);
#elif FEATURE_DNS_TAP
            return Dns.GetHostEntryAsync(hostNameOrAddress).Result;
#else
            #error Resolving host name or IP address to an IPHostEntry is not implemented.
#endif
        }
    }
}
