using System.Net;
using System.Net.Sockets;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Defines methods which will filter <see cref="IPAddress" />'s to prefer or require a specific <see cref="System.Net.Sockets.AddressFamily" /> by <see cref="IPAddress.AddressFamily" />
    /// </summary>
    public sealed class AddressTypeFilterClass
    {
        /// <summary>
        /// The address family to prefer or require
        /// </summary>
        public readonly AddressFamily AddressFamily;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressTypeFilterClass"/> class which requires or prefers addresses of type <paramref name="addressFamily"/>
        /// </summary>
        /// <param name="addressFamily">The address family to require or prefer.</param>
        public AddressTypeFilterClass(System.Net.Sockets.AddressFamily addressFamily)
        {
            AddressFamily = addressFamily;
        }

        /// <summary>
        /// Returns a single address which will be of type <see cref="AddressFamily"/> or the first <paramref name="addresses"/>
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        /// <returns>A single address which will be of type <see cref="AddressFamily"/> or the first <paramref name="addresses"/></returns>
        public IPAddress PreferAddressOfType(IPAddress[] addresses)
        {
            var preferAddressOfType = GetFirstAddressOfType(addresses);
            if (preferAddressOfType != null)
            {
                return preferAddressOfType;
            }
            return addresses[0];
        }

        /// <summary>
        /// Returns a single address which will be of type <see cref="AddressFamily"/>
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        /// <returns>A single address which will be of type <see cref="AddressFamily"/></returns>
        /// <exception cref="SocketException">If an address of the required type is not found</exception>
        public IPAddress RequireAddressOfType(IPAddress[] addresses)
        {
            var preferAddressOfType = GetFirstAddressOfType(addresses);
            if (preferAddressOfType != null)
            {
                return preferAddressOfType;
            }

            // The type of the address was not found so throw an exception
            throw new SocketException((int)SocketError.TypeNotFound);
        }

        /// <summary>
        /// Returns a single address which will be of type <see cref="AddressFamily"/> or null if none are found
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        /// <returns>A single address which will be of type <see cref="AddressFamily"/> or null if none are found</returns>
        private IPAddress GetFirstAddressOfType(IPAddress[] addresses)
        {
            foreach (var address in addresses)
            {
                if (address.AddressFamily == AddressFamily)
                {
                    return address;
                }
            }
            return null;
        }
    }
}