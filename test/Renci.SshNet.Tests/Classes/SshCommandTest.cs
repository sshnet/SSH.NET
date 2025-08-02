using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Represents SSH command that can be executed.
    /// </summary>
    [TestClass]
    public partial class SshCommandTest : TestBase
    {
        [TestMethod]
        public void Test_Execute_SingleCommand_Without_Connecting()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                Assert.ThrowsExactly<SshConnectionException>(() => client.CreateCommand("echo Hello"));
            }
        }
    }
}
