using Renci.SshNet.Common;

namespace Renci.SshNet.IntegrationTests.OldIntegrationTests
{
    /// <summary>
    /// Provides client connection to SSH server.
    /// </summary>
    [TestClass]
    public class SshClientTest : IntegrationTestBase
    {
        [TestMethod]
        [TestCategory("Authentication")]
        public void Test_Connect_Handle_HostKeyReceived()
        {
            var hostKeyValidated = false;

            #region Example SshClient Connect HostKeyReceived
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                client.HostKeyReceived += delegate(object sender, HostKeyEventArgs e)
                {
                    hostKeyValidated = true;
                    Console.WriteLine(string.Join(", ", e.FingerPrint));
                    if (e.FingerPrint.SequenceEqual(new byte[] { 179, 185, 208, 27, 115, 196, 96, 180, 206, 237, 6, 248, 88, 73, 163, 218 }))
                    {
                        e.CanTrust = true;
                    }
                    else
                    {
                        e.CanTrust = false;
                    }
                };
                client.Connect();
                //  Do something here
                client.Disconnect();
            }
            #endregion

            Assert.IsTrue(hostKeyValidated);
        }
    }
}
