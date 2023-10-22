namespace Renci.SshNet.IntegrationTests
{
    /// <summary>
    /// The SSH client integration tests
    /// </summary>
    [TestClass]
    public sealed class SshClientTests : IntegrationTestBase, IDisposable
    {
        private readonly SshClient _sshClient;

        public SshClientTests()
        {
            _sshClient = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password);
            _sshClient.Connect();
        }

        [TestMethod]
        public void Echo_Command_with_all_characters()
        {
            var builder = new StringBuilder();
            var response = _sshClient.RunCommand("echo $'test !@#$%^&*()_+{}:,./<>[];\\|'");

            Assert.AreEqual("test !@#$%^&*()_+{}:,./<>[];\\|\n", response.Result);
        }

        public void Dispose()
        {
            _sshClient.Disconnect();
            _sshClient.Dispose();
        }
    }
}
