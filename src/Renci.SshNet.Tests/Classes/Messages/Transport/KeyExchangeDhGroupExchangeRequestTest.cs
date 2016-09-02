using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Transport
{
    /// <summary>
    /// Represents SSH_MSG_KEX_DH_GEX_REQUEST message.
    /// </summary>
    [TestClass]
    public class KeyExchangeDhGroupExchangeRequestTest
    {
        private uint _minimum;
        private uint _preferred;
        private uint _maximum;

        public void Init()
        {
            var random = new Random();
            _minimum = (uint) random.Next(1, int.MaxValue);
            _preferred = (uint) random.Next(1, int.MaxValue);
            _maximum = (uint) random.Next(1, int.MaxValue);
        }


        [TestMethod]
        [TestCategory("KeyExchangeInitMessage")]
        [Owner("olegkap")]
        [Description("Validates KeyExchangeInitMessage message serialization.")]
        public void Test_KeyExchangeDhGroupExchangeRequest_GetBytes()
        {
            var request = new KeyExchangeDhGroupExchangeRequest(_minimum, _preferred, _maximum);

            var bytes = request.GetBytes();

            var expectedBytesLength = 0;
            expectedBytesLength += 1; // Type
            expectedBytesLength += 4; // Minimum
            expectedBytesLength += 4; // Preferred
            expectedBytesLength += 4; // Maximum

            Assert.AreEqual(expectedBytesLength, bytes.Length);

            var sshDataStream = new SshDataStream(bytes);

            Assert.AreEqual(KeyExchangeDhGroupExchangeRequest.MessageNumber, sshDataStream.ReadByte());
            Assert.AreEqual(_minimum, sshDataStream.ReadUInt32());
            Assert.AreEqual(_preferred, sshDataStream.ReadUInt32());
            Assert.AreEqual(_maximum, sshDataStream.ReadUInt32());

            Assert.IsTrue(sshDataStream.IsEndOfData);
        }

        [TestMethod]
        public void Load()
        {
            var request = new KeyExchangeDhGroupExchangeRequest(_minimum, _preferred, _maximum);
            var bytes = request.GetBytes();
            var target = new KeyExchangeDhGroupExchangeRequest(0, 0, 0);

            target.Load(bytes, 1, bytes.Length - 1);

            Assert.AreEqual(_minimum, target.Minimum);
            Assert.AreEqual(_preferred, target.Preferred);
            Assert.AreEqual(_maximum, target.Maximum);
        }
    }
}