using System;
using System.Net;
using System.Net.Sockets;
#if SILVERLIGHT
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif
using Renci.SshNet.Abstractions;

namespace Renci.SshNet.Tests.Abstractions
{
    [TestClass]
    public class DnsAbstraction_GetHostAddresses
    {
        [TestMethod]
        public void ShouldThrowArgumentNullExceptionWhenHostNameOrAddressIsNull()
        {
            const string hostNameOrAddress = null;

            try
            {
                DnsAbstraction.GetHostAddresses(hostNameOrAddress);
                Assert.Fail();
            }
            catch (ArgumentNullException)
            {
            }
        }

        [TestMethod]
        public void ShouldThrowSocketExceptionWhenHostIsNotFound()
        {
            const string hostNameOrAddress = "surelydoesnotexist.OrAmIWrong";

            try
            {
                var addresses = DnsAbstraction.GetHostAddresses(hostNameOrAddress);
                Assert.Fail(addresses.ToString());
            }
            catch (SocketException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(SocketError.HostNotFound, ex.SocketErrorCode);
            }
        }

        [TestMethod]
        public void ShouldReturnHostAddressesOfLocalHostWhenHostNameOrAddressIsEmpty()
        {
            const string hostNameOrAddress = "";

            var addresses = DnsAbstraction.GetHostAddresses(hostNameOrAddress);
            Assert.IsNotNull(addresses);

#if !SILVERLIGHT
            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            Assert.IsNotNull(hostEntry);

            Assert.AreEqual(hostEntry.AddressList.Length, addresses.Length);
            for (var i = 0; i < hostEntry.AddressList.Length; i++)
                Assert.AreEqual(hostEntry.AddressList[i], addresses[i]);
#endif
        }

        [TestMethod]
        public void ShouldReturnSingleIpv4AddressWhenHostNameOrAddressIsValidIpv4Address()
        {
            const string hostNameOrAddress = "1.2.3.4";

            var addresses = DnsAbstraction.GetHostAddresses(hostNameOrAddress);
            Assert.IsNotNull(addresses);
            Assert.AreEqual(1, addresses.Length);
            Assert.AreEqual(AddressFamily.InterNetwork, addresses[0].AddressFamily);
            Assert.AreEqual(IPAddress.Parse(hostNameOrAddress), addresses[0]);
        }

        [TestMethod]
        public void ShouldReturnSingleIpv6AddressWhenHostNameOrAddressIsValidIpv6Address()
        {
            const string hostNameOrAddress = "2001:0:9d38:90d7:384f:2133:ab3d:d152";

            var addresses = DnsAbstraction.GetHostAddresses(hostNameOrAddress);
            Assert.IsNotNull(addresses);
            Assert.AreEqual(1, addresses.Length);
            Assert.AreEqual(AddressFamily.InterNetworkV6, addresses[0].AddressFamily);
            Assert.AreEqual(IPAddress.Parse(hostNameOrAddress), addresses[0]);
        }
    }
}
