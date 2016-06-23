using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Defines methods which will filter <see cref="IPAddress"/>'s to prefer or require specific <see cref="AddressFamily"/>'s by <see cref="IPAddress.AddressFamily"/>
    /// </summary>
    public sealed class AddressTypesFilterClass
    {
        /// <summary>
        /// The address families to prefer or require
        /// </summary>
        public readonly AddressFamily[] AddressFamilies;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressTypesFilterClass"/> class which requires or prefers addresses of types <paramref name="addressFamilies"/>
        /// </summary>
        /// <param name="addressFamilies">The address families to require or prefer.</param>
        /// <exception cref="ArgumentException">If no <paramref name="addressFamilies"/> are passed in</exception>
        public AddressTypesFilterClass(params AddressFamily[] addressFamilies)
        {
            if (addressFamilies == null || addressFamilies.Length == 0)
            {
                throw new ArgumentException("At least one System.Net.Sockets.AddressFamily must be specified");
            }
            AddressFamilies = addressFamilies;
        }

        /// <summary>
        /// Returns a single address which will be one of the types specified in <see cref="AddressFamilies"/> or the first <paramref name="addresses"/>
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        /// <returns>A single address which will be one of the types specified in <see cref="AddressFamilies"/> or the first <paramref name="addresses"/></returns>
        public IPAddress PreferAddressOfTypes(IPAddress[] addresses)
        {
            var address = GetFirstAddressOfAnyType(addresses);
            if (address != null)
            {
                return address;
            }

            return addresses[0];
        }

        /// <summary>
        /// Returns a single address which will be one of the types specified in <see cref="AddressFamilies"/>
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        /// <returns>A single address which will be one of the types specified in <see cref="AddressFamilies"/></returns>
        /// <exception cref="SocketException">If an address of the required type is not found</exception>
        public IPAddress RequireAddressOfTypes(IPAddress[] addresses)
        {
            var address = GetFirstAddressOfAnyType(addresses);
            if (address != null)
            {
                return address;
            }

            // The type of the addresses was not found so throw an exception
            throw new SocketException((int)SocketError.TypeNotFound);
        }

        /// <summary>
        /// Returns a single address which will be one of the types specified in <see cref="AddressFamilies"/> or the first <paramref name="addresses"/>
        /// , the preference of <see cref="IPAddress.AddressFamily"/> will be tried in the same order as the <see cref="AddressFamilies"/>
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        /// <returns>A single address which will be one of the types specified in <see cref="AddressFamilies"/> or the first <paramref name="addresses"/>
        /// , the preference of <see cref="IPAddress.AddressFamily"/> will be tried in the same order as the <see cref="AddressFamilies"/></returns>
        public IPAddress PreferAddressOfTypesOrdered(IPAddress[] addresses)
        {
            var address = GetFirstAddressOfAnyTypeOrdered(addresses);
            if (address != null)
            {
                return address;
            }

            return addresses[0];
        }

        /// <summary>
        /// Returns a single address which will be one of the types specified in <see cref="AddressFamilies"/>
        /// , the preference of <see cref="IPAddress.AddressFamily"/> will be tried in the same order as the <see cref="AddressFamilies"/>
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        /// <returns>A single address which will be one of the types specified in <see cref="AddressFamilies"/>
        /// , the preference of <see cref="IPAddress.AddressFamily"/> will be tried in the same order as the <see cref="AddressFamilies"/></returns>
        /// <exception cref="SocketException">If an address of the required type is not found</exception>
        public IPAddress RequireAddressOfTypesOrdered(IPAddress[] addresses)
        {
            var address = GetFirstAddressOfAnyType(addresses);
            if (address != null)
            {
                return address;
            }

            // The type of the addresses was not found so throw an exception
            throw new SocketException((int)SocketError.TypeNotFound);
        }

        /// <summary>
        /// Returns a single address which will be of any type in <see cref="AddressFamilies"/> or null if none are found
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        /// <returns>A single address which will be of any type in <see cref="AddressFamilies"/> or null if none are found</returns>
        private IPAddress GetFirstAddressOfAnyType(IPAddress[] addresses)
        {
            foreach (var address in addresses)
            {
                if (AddressFamilies.Contains(address.AddressFamily))
                {
                    return address;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns a single address which will be of any type in <see cref="AddressFamilies"/> or null if none are found
        /// , the preference of <see cref="IPAddress.AddressFamily"/> will be tried in the same order as the <see cref="AddressFamilies"/>
        /// </summary>
        /// <param name="addresses">The addresses.</param>
        /// <returns>A single address which will be of any type in <see cref="AddressFamilies"/> or null if none are found
        /// , the preference of <see cref="IPAddress.AddressFamily"/> will be tried in the same order as the <see cref="AddressFamilies"/></returns>
        private IPAddress GetFirstAddressOfAnyTypeOrdered(IPAddress[] addresses)
        {
            foreach (var addressFamily in AddressFamilies)
            {
                foreach (var address in addresses)
                {
                    if (address.AddressFamily == addressFamily)
                    {
                        return address;
                    }
                }
            }
            return null;
        }
    }
}
