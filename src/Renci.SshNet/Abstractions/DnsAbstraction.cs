using System;
using System.Net;
using System.Net.Sockets;

#if FEATURE_DNS_SYNC
#elif FEATURE_DNS_APM
using Renci.SshNet.Common;
#elif FEATURE_DNS_TAP
#elif FEATURE_DEVICEINFORMATION_APM
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Phone.Net.NetworkInformation;
#elif FEATURE_DATAGRAMSOCKET
using System.Collections.Generic;
using Windows.Networking;
using Windows.Networking.Sockets;
#endif

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
        /// <exception cref="ArgumentNullException"><paramref name="hostNameOrAddress"/> is <c>null</c>.</exception>
        /// <exception cref="SocketException">An error is encountered when resolving <paramref name="hostNameOrAddress"/>.</exception>
        public static IPAddress[] GetHostAddresses(string hostNameOrAddress)
        {
            // TODO Eliminate sync variant, and implement timeout

#if FEATURE_DNS_SYNC
            return Dns.GetHostAddresses(hostNameOrAddress);
#elif FEATURE_DNS_APM
            var asyncResult = Dns.BeginGetHostAddresses(hostNameOrAddress, null, null);
            if (!asyncResult.AsyncWaitHandle.WaitOne(Session.InfiniteTimeSpan))
                throw new SshOperationTimeoutException("Timeout resolving host name.");
            return Dns.EndGetHostAddresses(asyncResult);
#elif FEATURE_DNS_TAP
            return Dns.GetHostAddressesAsync(hostNameOrAddress).GetAwaiter().GetResult();
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
#elif FEATURE_DATAGRAMSOCKET

            // TODO we may need to only return those IP addresses that are supported on the current system
            // TODO http://wojciechkulik.pl/csharp/winrt-how-to-detect-supported-ip-versions

            var endpointPairs = DatagramSocket.GetEndpointPairsAsync(new HostName(hostNameOrAddress), "").GetAwaiter().GetResult();
            var addresses = new List<IPAddress>();
            foreach (var endpointPair in endpointPairs)
            {
                if (endpointPair.RemoteHostName.Type == HostNameType.Ipv4 || endpointPair.RemoteHostName.Type == HostNameType.Ipv6)
                    addresses.Add(IPAddress.Parse(endpointPair.RemoteHostName.CanonicalName));
            }
            if (addresses.Count == 0)
                throw new SocketException((int) System.Net.Sockets.SocketError.HostNotFound);
            return addresses.ToArray();
#else
            throw new NotSupportedException("Resolving hostname to IP address is not implemented.");
#endif // FEATURE_DEVICEINFORMATION_APM
#endif
        }
    }
}
