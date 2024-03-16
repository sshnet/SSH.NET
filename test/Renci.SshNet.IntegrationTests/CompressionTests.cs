using Renci.SshNet.Compression;

namespace Renci.SshNet.IntegrationTests
{
    [TestClass]
    public class CompressionTests : IntegrationTestBase
    {
        private IConnectionInfoFactory _connectionInfoFactory;

        [TestInitialize]
        public void SetUp()
        {
            _connectionInfoFactory = new LinuxVMConnectionFactory(SshServerHostName, SshServerPort);
        }

        [TestMethod]
        public void None()
        {
            DoTest(new KeyValuePair<string, Func<Compressor>>("none", null));
        }

#if NET6_0_OR_GREATER
        [TestMethod]
        public void ZlibOpenSsh()
        {
            DoTest(new KeyValuePair<string, Func<Compressor>>("zlib@openssh.com", () => new ZlibOpenSsh()));
        }
#endif

        private void DoTest(KeyValuePair<string, Func<Compressor>> compressor)
        {
            using (var scpClient = new ScpClient(_connectionInfoFactory.Create()))
            {
                scpClient.ConnectionInfo.CompressionAlgorithms.Clear();
                scpClient.ConnectionInfo.CompressionAlgorithms.Add(compressor);

                scpClient.Connect();

                Assert.AreEqual(compressor.Key, scpClient.ConnectionInfo.CurrentClientCompressionAlgorithm);
                Assert.AreEqual(compressor.Key, scpClient.ConnectionInfo.CurrentServerCompressionAlgorithm);

                var file = $"/tmp/{Guid.NewGuid()}.txt";
                var fileContent = "RepeatingRepeatingRepeatingRepeatingRepeatingRepeatingRepeatingRepeatingRepeatingRepeatingRepeatingRepeatingRepeatingRepeatingRepeatingRepeatingRepeatingRepeatingRepeatingRepeating";

                using var uploadStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
                scpClient.Upload(uploadStream, file);

                using var downloadStream = new MemoryStream();
                scpClient.Download(file, downloadStream);

                var result = Encoding.UTF8.GetString(downloadStream.ToArray());

                Assert.AreEqual(fileContent, result);
            }
        }
    }
}
