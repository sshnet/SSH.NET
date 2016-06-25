using Renci.SshNet.Messages.Connection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    ///This is a test class for ChannelOpenInfoTest and is intended
    ///to contain all ChannelOpenInfoTest Unit Tests
    ///</summary>
    [TestClass]
    [Ignore] // placeholders only
    public class ChannelOpenInfoTest : TestBase
    {
        internal virtual ChannelOpenInfo CreateChannelOpenInfo()
        {
            // TODO: Instantiate an appropriate concrete class.
            ChannelOpenInfo target = null;
            return target;
        }

        /// <summary>
        ///A test for ChannelType
        ///</summary>
        [TestMethod()]
        public void ChannelTypeTest()
        {
            ChannelOpenInfo target = CreateChannelOpenInfo(); // TODO: Initialize to an appropriate value
            string actual;
            actual = target.ChannelType;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
