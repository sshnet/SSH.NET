using System;
using System.Net;
#if FEATURE_DEVICEINFORMATION_APM
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Phone.Net.NetworkInformation;
#endif // FEATURE_DEVICEINFORMATION_APM

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
            IPAddress address;
            if (IPAddress.TryParse(hostNameOrAddress, out address))
                return new [] { address};

#if FEATURE_DEVICEINFORMATION_APM
            var resolveCompleted = new ManualResetEvent(false);
            NameResolutionResult nameResolutionResult = null;
            DeviceNetworkInformation.ResolveHostNameAsync(new DnsEndPoint(hostNameOrAddress, 0), result =>
            {
                nameResolutionResult = result;
                resolveCompleted.Set();
            }, null);

            // wait until address is resolved
            resolveCompleted.WaitOne();

            if (nameResolutionResult.NetworkErrorCode == NetworkError.Success)
            {
                var addresses = new List<IPAddress>(nameResolutionResult.IPEndPoints.Select(p => p.Address).Distinct());
                return addresses.ToArray();
            }
            throw new SocketException((int)nameResolutionResult.NetworkErrorCode);
#else
            throw new NotSupportedException("Resolving hostname to IP address is not supported.");
#endif // FEATURE_DEVICEINFORMATION_APM
#endif
        }
    }
}
