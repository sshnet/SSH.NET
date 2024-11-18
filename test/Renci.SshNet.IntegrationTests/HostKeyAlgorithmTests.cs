using Renci.SshNet.Common;
using Renci.SshNet.IntegrationTests.Common;
using Renci.SshNet.Security;
using Renci.SshNet.TestTools.OpenSSH;

namespace Renci.SshNet.IntegrationTests
{
    [TestClass]
    public class HostKeyAlgorithmTests : IntegrationTestBase
    {
        private IConnectionInfoFactory _connectionInfoFactory;
        private RemoteSshdConfig _remoteSshdConfig;

        [TestInitialize]
        public void SetUp()
        {
            _connectionInfoFactory = new LinuxVMConnectionFactory(SshServerHostName, SshServerPort);
            _remoteSshdConfig = new RemoteSshd(new LinuxAdminConnectionFactory(SshServerHostName, SshServerPort)).OpenConfig();
        }

        [TestCleanup]
        public void TearDown()
        {
            _remoteSshdConfig?.Reset();
        }

        [TestMethod]
        public void SshDss()
        {
            DoTest(HostKeyAlgorithm.SshDss, HostKeyFile.Dsa);
        }

        [TestMethod]
        public void SshRsa()
        {
            DoTest(HostKeyAlgorithm.SshRsa, HostKeyFile.Rsa);
        }

        [TestMethod]
        public void SshRsaSha256()
        {
            DoTest(HostKeyAlgorithm.RsaSha2256, HostKeyFile.Rsa);
        }

        [TestMethod]
        public void SshRsaSha512()
        {
            DoTest(HostKeyAlgorithm.RsaSha2512, HostKeyFile.Rsa);
        }

        [TestMethod]
        public void SshEd25519()
        {
            DoTest(HostKeyAlgorithm.SshEd25519, HostKeyFile.Ed25519);
        }

        [TestMethod]
        public void Ecdsa256()
        {
            DoTest(HostKeyAlgorithm.EcdsaSha2Nistp256, HostKeyFile.Ecdsa256);
        }

        [TestMethod]
        public void Ecdsa384()
        {
            DoTest(HostKeyAlgorithm.EcdsaSha2Nistp384, HostKeyFile.Ecdsa384);
        }

        [TestMethod]
        public void Ecdsa521()
        {
            DoTest(HostKeyAlgorithm.EcdsaSha2Nistp521, HostKeyFile.Ecdsa521);
        }

        [TestMethod]
        public void SshRsaCertificate()
        {
            DoTest(HostKeyAlgorithm.SshRsaCertV01OpenSSH, HostCertificateFile.RsaCertRsa);
        }

        [TestMethod]
        public void SshRsaSha256Certificate()
        {
            DoTest(HostKeyAlgorithm.RsaSha2256CertV01OpenSSH, HostCertificateFile.RsaCertRsa);
        }

        [TestMethod]
        public void Ecdsa256Certificate()
        {
            DoTest(HostKeyAlgorithm.EcdsaSha2Nistp256CertV01OpenSSH, HostCertificateFile.Ecdsa256CertRsa);
        }

        [TestMethod]
        public void Ecdsa384Certificate()
        {
            DoTest(HostKeyAlgorithm.EcdsaSha2Nistp384CertV01OpenSSH, HostCertificateFile.Ecdsa384CertEcdsa);
        }

        [TestMethod]
        public void Ecdsa521Certificate()
        {
            DoTest(HostKeyAlgorithm.EcdsaSha2Nistp521CertV01OpenSSH, HostCertificateFile.Ecdsa521CertEd25519);
        }

        [TestMethod]
        public void Ed25519Certificate()
        {
            DoTest(HostKeyAlgorithm.SshEd25519CertV01OpenSSH, HostCertificateFile.Ed25519CertEcdsa);
        }

        private void DoTest(HostKeyAlgorithm hostKeyAlgorithm, HostKeyFile hostKeyFile, HostCertificateFile hostCertificateFile = null)
        {
            _remoteSshdConfig.ClearHostKeyAlgorithms()
                             .AddHostKeyAlgorithm(hostKeyAlgorithm)
                             .ClearHostKeyFiles()
                             .AddHostKeyFile(hostKeyFile.FilePath)
                             .WithHostKeyCertificate(hostCertificateFile?.FilePath)
                             .Update()
                             .Restart();

            HostKeyEventArgs hostKeyEventsArgs = null;

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.HostKeyReceived += (sender, e) => hostKeyEventsArgs = e;
                client.Connect();
                client.Disconnect();
            }

            Assert.IsNotNull(hostKeyEventsArgs);
            Assert.AreEqual(hostKeyAlgorithm.Name, hostKeyEventsArgs.HostKeyName);
            Assert.AreEqual(hostKeyFile.KeyLength, hostKeyEventsArgs.KeyLength);
            CollectionAssert.AreEqual(hostKeyFile.FingerPrint, hostKeyEventsArgs.FingerPrint);

            if (hostCertificateFile is not null)
            {
                Assert.IsNotNull(hostKeyEventsArgs.Certificate);
                Assert.AreEqual(Certificate.CertificateType.Host, hostKeyEventsArgs.Certificate.Type);
                Assert.AreEqual(hostCertificateFile.CAFingerPrint, hostKeyEventsArgs.Certificate.CertificateAuthorityKeyFingerPrint);
            }
            else
            {
                Assert.IsNull(hostKeyEventsArgs.Certificate);
            }
        }

        private void DoTest(HostKeyAlgorithm hostKeyAlgorithm, HostCertificateFile hostCertificateFile)
        {
            DoTest(hostKeyAlgorithm, hostCertificateFile.HostKeyFile, hostCertificateFile);
        }
    }
}
