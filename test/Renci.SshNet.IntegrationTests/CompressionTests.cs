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
            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.ConnectionInfo.CompressionAlgorithms.Clear();
                client.ConnectionInfo.CompressionAlgorithms.Add(compressor);

                client.Connect();
                client.Disconnect();
            }
        }
    }
}
