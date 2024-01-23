namespace Renci.SshNet.IntegrationTests
{
    /// <summary>
    /// The SSH client integration tests
    /// </summary>
    [TestClass]
    public class SshClientTests : IntegrationTestBase, IDisposable
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
            var response = _sshClient.RunCommand("echo $'test !@#$%^&*()_+{}:,./<>[];\\|'");

            Assert.AreEqual("test !@#$%^&*()_+{}:,./<>[];\\|\n", response.Result);
        }

        [TestMethod]
        public void Send_InputStream_to_Command()
        {
            var inputByteArray = Encoding.UTF8.GetBytes("Hello world!");

            // Make the server echo back the input file with "cat"
            var command = _sshClient.CreateCommand("cat");

            var asyncResult = command.BeginExecute();
            command.InputStream.Write(inputByteArray);
            command.EndExecute(asyncResult);

            Assert.AreEqual("Hello world!", command.Result);
            Assert.AreEqual(string.Empty, command.Error);
        }

        public void Dispose()
        {
            _sshClient.Disconnect();
            _sshClient.Dispose();
        }
    }
}
