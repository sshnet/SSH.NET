using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    
    
    /// <summary>
    ///This is a test class for AuthenticationBannerEventArgsTest and is intended
    ///to contain all AuthenticationBannerEventArgsTest Unit Tests
    ///</summary>
    [TestClass]
    public class AuthenticationBannerEventArgsTest : TestBase
    {
        /// <summary>
        ///A test for AuthenticationBannerEventArgs Constructor
        ///</summary>
        [TestMethod]
        public void AuthenticationBannerEventArgsConstructorTest()
        {
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string message = string.Empty; // TODO: Initialize to an appropriate value
            string language = string.Empty; // TODO: Initialize to an appropriate value
            AuthenticationBannerEventArgs target = new AuthenticationBannerEventArgs(username, message, language);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
