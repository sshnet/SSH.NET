using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Authentication
{   
    /// <summary>
    ///This is a test class for BannerMessageTest and is intended
    ///to contain all BannerMessageTest Unit Tests
    ///</summary>
    [TestClass]
    public class BannerMessageTest : TestBase
    {
        /// <summary>
        ///A test for BannerMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void BannerMessageConstructorTest()
        {
            BannerMessage target = new BannerMessage();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
