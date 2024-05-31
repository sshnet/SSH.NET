using Renci.SshNet.Common;
using Renci.SshNet.IntegrationTests.Common;

namespace Renci.SshNet.IntegrationTests
{
    [TestClass]
    public class SshTests_TTYDisabled : TestBase
    {
        private IConnectionInfoFactory _connectionInfoFactory;
        private IConnectionInfoFactory _adminConnectionInfoFactory;
        private RemoteSshdConfig _remoteSshdConfig;

        [TestInitialize]
        public void SetUp()
        {
            _connectionInfoFactory = new LinuxVMConnectionFactory(SshServerHostName, SshServerPort);
            _adminConnectionInfoFactory = new LinuxAdminConnectionFactory(SshServerHostName, SshServerPort);

            _remoteSshdConfig = new RemoteSshd(_adminConnectionInfoFactory).OpenConfig();
            _remoteSshdConfig.AllowTcpForwarding()
                             .PermitTTY(false)
                             .PrintMotd(false)
                             .Update()
                             .Restart();
        }

        [TestCleanup]
        public void TearDown()
        {
            _remoteSshdConfig?.Reset();
        }

        [TestMethod]
        public void Ssh_CreateShellStream_WithPseudoTerminal()
        {
            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                try
                {
                    client.CreateShellStream("xterm", 80, 24, 800, 600, 1024, null);
                    Assert.Fail("Should not be able to create ShellStream with pseudo-terminal settings when PermitTTY is no at server side.");
                }
                catch (SshException ex)
                {
                    Assert.AreEqual("The pseudo-terminal request was not accepted by the server. Consult the server log for more information.", ex.Message);
                }
            }
        }

        [TestMethod]
        public void Ssh_CreateShellStream_WithoutPseudoTerminal()
        {
            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var shellStream = client.CreateShellStream(bufferSize: 1024))
                {
                    shellStream.WriteLine("echo Hello!");
                    var line = shellStream.ReadLine(TimeSpan.FromSeconds(1));
                    Assert.IsNotNull(line);
                    Assert.IsTrue(line.EndsWith("Hello!"), line);
                }
            }
        }


        [TestMethod]
        public void Ssh_CreateShell_WithPseudoTerminal()
        {
            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var input = new MemoryStream())
                using (var output = new MemoryStream())
                using (var extOutput = new MemoryStream())
                {
                    var shell = client.CreateShell(input, output, extOutput, "xterm", 80, 24, 800, 600, null, 1024);

                    try
                    {
                        shell.Start();
                        Assert.Fail("Should not be able to create ShellStream with terminal settings when PermitTTY is no at server side.");
                    }
                    catch (SshException ex)
                    {
                        Assert.AreEqual("The pseudo-terminal request was not accepted by the server. Consult the server log for more information.", ex.Message);
                    }
                }
            }
        }

        [TestMethod]
        public void Ssh_CreateShell_WithoutPseudoTerminal()
        {
            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var input = new MemoryStream())
                using (var output = new MemoryStream())
                using (var extOutput = new MemoryStream())
                {
                    var shell = client.CreateShell(input, output, extOutput);

                    shell.Start();

                    var inputWriter = new StreamWriter(input, Encoding.ASCII, 1024);
                    inputWriter.WriteLine("echo $PATH");

                    var outputReader = new StreamReader(output, Encoding.ASCII, false, 1024);
                    Console.WriteLine(outputReader.ReadToEnd());

                    shell.Stop();
                }
            }
        }
    }
}
