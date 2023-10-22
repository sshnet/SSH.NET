using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class ChannelDataEventArgsTest : TestBase
    {
        [TestMethod]
        public void Complete()
        {
            var data = new byte[] { 0x45, 0x12 };

            var e = new ChannelDataEventArgs(12u, data);

            Assert.AreEqual(12U, e.ChannelNumber);
            Assert.AreSame(data, e.Data);
        }

        [TestMethod]
        public void DataIsNull()
        {
            const byte[] data = null;

            try
            {
                _ = new ChannelDataEventArgs(12U, data);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(typeof(ArgumentNullException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("data", ex.ParamName);
            }
        }

        [TestMethod]
        public void Minimal()
        {
            var data = Array.Empty<byte>();

            var e = new ChannelDataEventArgs(0u, data);

            Assert.AreEqual(0U, e.ChannelNumber);
            Assert.AreSame(data, e.Data);
        }
    }
}
