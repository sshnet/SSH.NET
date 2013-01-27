using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Tests.Common;
using System.Linq;

namespace Renci.SshNet.Tests.Classes.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEX_DH_GEX_REQUEST message.
    /// </summary>
    [TestClass]
    public class KeyExchangeDhGroupExchangeRequestTest : TestBase
    {
        [TestMethod]
        [TestCategory("KeyExchangeInitMessage")]
        [Owner("olegkap")]
        [Description("Validates KeyExchangeInitMessage message serialization.")]
        public void Test_KeyExchangeDhGroupExchangeRequest_GetBytes()
        {
            var m = new KeyExchangeDhGroupExchangeRequest(1024, 1024, 1204);
            var input = new byte[] { 0x22, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, };
            var output = m.GetBytes();

            //  Skip first 17 bytes since 16 bytes are randomly generated
            Assert.IsTrue(input.SequenceEqual(output.Skip(17)));
        }
    }
}