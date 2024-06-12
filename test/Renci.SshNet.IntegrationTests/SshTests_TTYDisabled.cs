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
        public void Ssh_CreateShellStream()
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
        public void Ssh_CreateShellStreamNoTerminal()
        {
            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var shellStream = client.CreateShellStreamNoTerminal(bufferSize: 1024))
                {
                    var foo = new string('a', 90);
                    shellStream.WriteLine($"echo {foo}");
                    var line = shellStream.ReadLine(TimeSpan.FromSeconds(1));
                    Assert.IsNotNull(line);
                    Assert.IsTrue(line.EndsWith(foo), line);
                }
            }
        }


        [TestMethod]
        public void Ssh_CreateShell()
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
        public void Ssh_CreateShellNoTerminal()
        {
            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var input = new MemoryStream())
                using (var output = new MemoryStream())
                using (var extOutput = new MemoryStream())
                {
                    var shell = client.CreateShellNoTerminal(input, output, extOutput, 1024);

                    shell.Start();

                    var inputWriter = new StreamWriter(input, Encoding.ASCII, 1024);
                    var foo = new string('a', 90);
                    inputWriter.WriteLine(foo);

                    var outputReader = new StreamReader(output, Encoding.ASCII, false, 1024);
                    var outputString = outputReader.ReadToEnd();

                    Assert.IsNotNull(outputString);
                    Assert.IsTrue(outputString.EndsWith(foo), outputString);

                    shell.Stop();
                }
            }
        }
    }
}
