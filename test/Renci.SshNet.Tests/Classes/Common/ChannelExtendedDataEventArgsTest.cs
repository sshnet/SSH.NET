using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class ChannelExtendedDataEventArgsTest
    {
        [TestMethod]
        public void Complete()
        {
            const uint channelNumber = 12u;
            var data = new byte[] { 0x0d, 0xff };
            var dataTypeCode = 7u;

            var e = new ChannelExtendedDataEventArgs(channelNumber, data, dataTypeCode);

            Assert.AreEqual(channelNumber, e.ChannelNumber);
            Assert.AreSame(data, e.Data);
            Assert.AreEqual(dataTypeCode, e.DataTypeCode);
        }

        [TestMethod]
        public void DataIsNull()
        {
            const uint channelNumber = 0u;
            const byte[] data = null;
            var dataTypeCode = 0u;

            try
            {
                _ = new ChannelExtendedDataEventArgs(channelNumber, data, dataTypeCode);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(typeof(ArgumentNullException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("info", ex.ParamName);
            }
        }

        [TestMethod]
        public void Minimal()
        {
            const uint channelNumber = 0u;
            var data = Array.Empty<byte>();
            var dataTypeCode = 0u;

            var e = new ChannelExtendedDataEventArgs(channelNumber, data, dataTypeCode);

            Assert.AreEqual(channelNumber, e.ChannelNumber);
            Assert.AreSame(data, e.Data);
            Assert.AreEqual(dataTypeCode, e.DataTypeCode);
        }
    }
}
