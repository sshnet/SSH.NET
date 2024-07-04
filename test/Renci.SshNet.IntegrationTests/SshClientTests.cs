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
        public void Test_BigCommand()
        {
            using var command = _sshClient.CreateCommand("head -c 10000000 /dev/urandom | base64"); // 10MB of data please

            var asyncResult = command.BeginExecute();

            long totalBytesRead = 0;
            int bytesRead;
            byte[] buffer = new byte[4096];

            while ((bytesRead = command.OutputStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                totalBytesRead += bytesRead;
            }

            var result = command.EndExecute(asyncResult);

            Assert.AreEqual(13_508_775, totalBytesRead);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void Send_InputStream_to_Command()
        {
            var inputByteArray = Encoding.UTF8.GetBytes("Hello world!");

            // Make the server echo back the input file with "cat"
            using (var command = _sshClient.CreateCommand("cat"))
            {
                var asyncResult = command.BeginExecute();

                using (var inputStream = command.CreateInputStream())
                {
                    inputStream.Write(inputByteArray, 0, inputByteArray.Length);
                }

                command.EndExecute(asyncResult);

                Assert.AreEqual("Hello world!", command.Result);
                Assert.AreEqual(string.Empty, command.Error);
            }
        }

        [TestMethod]
        public void Send_InputStream_to_Command_OneByteAtATime()
        {
            var inputByteArray = Encoding.UTF8.GetBytes("Hello world!");

            // Make the server echo back the input file with "cat"
            using (var command = _sshClient.CreateCommand("cat"))
            {
                var asyncResult = command.BeginExecute();

                using (var inputStream = command.CreateInputStream())
                {
                    for (var i = 0; i < inputByteArray.Length; i++)
                    {
                        inputStream.WriteByte(inputByteArray[i]);
                    }
                }

                command.EndExecute(asyncResult);

                Assert.AreEqual("Hello world!", command.Result);
                Assert.AreEqual(string.Empty, command.Error);
            }
        }

        [TestMethod]
        public void CreateInputStream_BeforeBeginExecute_ThrowsInvalidOperationException()
        {
            var command = _sshClient.CreateCommand("ls");

            Assert.ThrowsException<InvalidOperationException>(command.CreateInputStream);
        }

        [TestMethod]
        public void CreateInputStream_AfterEndExecute_ThrowsInvalidOperationException()
        {
            var command = _sshClient.CreateCommand("ls");
            var asyncResult = command.BeginExecute();
            command.EndExecute(asyncResult);

            Assert.ThrowsException<InvalidOperationException>(command.CreateInputStream);
        }

        public void Dispose()
        {
            _sshClient.Disconnect();
            _sshClient.Dispose();
        }
    }
}
