using Renci.SshNet.Messages.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Messages.Authentication
{   
    /// <summary>
    ///This is a test class for BannerMessageTest and is intended
    ///to contain all BannerMessageTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BannerMessageTest : TestBase
    {
        /// <summary>
        ///A test for BannerMessage Constructor
        ///</summary>
        [TestMethod()]
        public void BannerMessageConstructorTest()
        {
            BannerMessage target = new BannerMessage();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
