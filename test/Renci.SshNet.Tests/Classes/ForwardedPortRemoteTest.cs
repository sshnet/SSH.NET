using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality for remote port forwarding
    /// </summary>
    [TestClass]
    public partial class ForwardedPortRemoteTest : TestBase
    {
        [TestMethod]
        public void Start_NotAddedToClient()
        {
            const int boundPort = 80;
            var host = string.Empty;
            const uint port = 22;

            using (var target = new ForwardedPortRemote(boundPort, host, port))
            {
                try
                {
                    target.Start();
                    Assert.Fail();
                }
                catch (InvalidOperationException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Forwarded port is not added to a client.", ex.Message);
                }
            }
        }
    }
}
