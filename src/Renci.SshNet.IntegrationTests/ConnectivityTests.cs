using System.Diagnostics;
using System.Management;
using System.Text.RegularExpressions;

using Renci.SshNet.Common;
using Renci.SshNet.IntegrationTests.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.IntegrationTests
{
    [TestClass]
    public class ConnectivityTests : TestBase
    {
        private const string NetworkConnectionId = "Ethernet 2";

        private AuthenticationMethodFactory _authenticationMethodFactory;
        private IConnectionInfoFactory _connectionInfoFactory;
        private IConnectionInfoFactory _adminConnectionInfoFactory;
        private RemoteSshdConfig _remoteSshdConfig;

        [TestInitialize]
        public void SetUp()
        {
            _authenticationMethodFactory = new AuthenticationMethodFactory();
            _connectionInfoFactory = new LinuxVMConnectionFactory(SshServerHostName, SshServerPort, _authenticationMethodFactory);
            _adminConnectionInfoFactory = new LinuxAdminConnectionFactory(SshServerHostName, SshServerPort);
            _remoteSshdConfig = new RemoteSshd(_adminConnectionInfoFactory).OpenConfig();
        }

        [TestCleanup]
        public void TearDown()
        {
            _remoteSshdConfig?.Reset();
        }

        [TestMethod]
        public void Common_CreateMoreChannelsThanMaxSessions()
        {
            var connectionInfo = _connectionInfoFactory.Create();
            connectionInfo.MaxSessions = 2;

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();

                // create one more channel than the maximum number of sessions
                // as that would block indefinitely when creating the last channel
                // if the channel would not be properly closed
                for (var i = 0; i < connectionInfo.MaxSessions + 1; i++)
                {
                    using (var stream = client.CreateShellStream("vt220", 20, 20, 20, 20, 0))
                    {
                        stream.WriteLine("echo test");
                        stream.ReadLine();
                    }
                }
            }
        }

        [TestMethod]
        public void Common_DisposeAfterLossOfNetworkConnectivity()
        {
            var hostNetworkConnectionDisabled = false;

            try
            {
                Exception errorOccurred = null;

                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.ErrorOccurred += (sender, args) => errorOccurred = args.Exception;
                    client.Connect();

                    DisableHostNetworkConnection(NetworkConnectionId);
                    hostNetworkConnectionDisabled = true;
                }

                Assert.IsNotNull(errorOccurred);
                Assert.AreEqual(typeof(SshConnectionException), errorOccurred.GetType());

                var connectionException = (SshConnectionException) errorOccurred;
                Assert.AreEqual(DisconnectReason.ConnectionLost, connectionException.DisconnectReason);
                Assert.IsNull(connectionException.InnerException);
                Assert.AreEqual("An established connection was aborted by the server.", connectionException.Message);
            }
            finally
            {
                if (hostNetworkConnectionDisabled)
                {
                    EnableHostNetworkConnection(NetworkConnectionId);
                    ResetVirtualMachineNetworkConnection();
                }
            }
        }

        [TestMethod]
        public void Common_DetectLossOfNetworkConnectivityThroughKeepAlive()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                Exception errorOccurred = null;
                client.ErrorOccurred += (sender, args) => errorOccurred = args.Exception;
                client.KeepAliveInterval = new TimeSpan(0, 0, 0, 0, 50);
                client.Connect();

                DisableHostNetworkConnection(NetworkConnectionId);

                try
                {
                    for (var i = 0; i < 500; i++)
                    {
                        if (!client.IsConnected)
                        {
                            break;
                        }

                        Thread.Sleep(100);
                    }

                    Assert.IsFalse(client.IsConnected);

                    Assert.IsNotNull(errorOccurred);
                    Assert.AreEqual(typeof(SshConnectionException), errorOccurred.GetType());

                    var connectionException = (SshConnectionException)errorOccurred;
                    Assert.AreEqual(DisconnectReason.ConnectionLost, connectionException.DisconnectReason);
                    Assert.IsNull(connectionException.InnerException);
                    Assert.AreEqual("An established connection was aborted by the server.", connectionException.Message);
                }
                finally
                {
                    EnableHostNetworkConnection(NetworkConnectionId);
                    ResetVirtualMachineNetworkConnection();
                }
            }
        }

        [TestMethod]
        public void Common_DetectConnectionResetThroughSftpInvocation()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                ManualResetEvent errorOccurredSignaled = new ManualResetEvent(false);
                Exception errorOccurred = null;
                client.ErrorOccurred += (sender, args) =>
                    {
                        errorOccurred = args.Exception;
                        errorOccurredSignaled.Set();
                    };
                client.Connect();

                DisableHostNetworkConnection(NetworkConnectionId);

                try
                {
                    client.ListDirectory("/");
                    Assert.Fail();
                }
                catch (SshConnectionException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Client not connected.", ex.Message);

                    Assert.IsNotNull(errorOccurred);
                    Assert.AreEqual(typeof(SshConnectionException), errorOccurred.GetType());

                    var connectionException = (SshConnectionException) errorOccurred;
                    Assert.AreEqual(DisconnectReason.ConnectionLost, connectionException.DisconnectReason);
                    Assert.IsNull(connectionException.InnerException);
                    Assert.AreEqual("An established connection was aborted by the server.", connectionException.Message);
                }
                finally
                {
                    EnableHostNetworkConnection(NetworkConnectionId);
                    ResetVirtualMachineNetworkConnection();
                }
            }
        }

        [TestMethod]
        public void Common_LossOfNetworkConnectivityDisconnectAndConnect()
        {
            bool vmNetworkConnectionDisabled = false;

            try
            {
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    Exception errorOccurred = null;
                    client.ErrorOccurred += (sender, args) => errorOccurred = args.Exception;

                    client.Connect();

                    DisableVirtualMachineNetworkConnection();
                    vmNetworkConnectionDisabled = true;
                    ResetVirtualMachineNetworkConnection();

                    // disconnect while network connectivity is lost
                    client.Disconnect();

                    Assert.IsFalse(client.IsConnected);

                    EnableVirtualMachineNetworkConnection();
                    vmNetworkConnectionDisabled = false;
                    ResetVirtualMachineNetworkConnection();

                    // connect when network connectivity is restored
                    client.Connect();
                    client.ChangeDirectory(client.WorkingDirectory);
                    client.Dispose();

                    Assert.IsNull(errorOccurred);
                }
            }
            finally
            {
                if (vmNetworkConnectionDisabled)
                {
                    EnableVirtualMachineNetworkConnection();
                    ResetVirtualMachineNetworkConnection();
                }
            }
        }

        [TestMethod]
        public void Common_DetectLossOfNetworkConnectivityThroughSftpInvocation()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                ManualResetEvent errorOccurredSignaled = new ManualResetEvent(false);
                Exception errorOccurred = null;
                client.ErrorOccurred += (sender, args) =>
                    {
                        errorOccurred = args.Exception;
                        errorOccurredSignaled.Set();
                    };
                client.Connect();

                DisableVirtualMachineNetworkConnection();
                ResetVirtualMachineNetworkConnection();

                try
                {
                    client.ListDirectory("/");
                    Assert.Fail();
                }
                catch (SshConnectionException ex)
                {
                    Assert.AreEqual(DisconnectReason.ConnectionLost, ex.DisconnectReason);
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("An established connection was aborted by the server.", ex.Message);
                }
                finally
                {
                    EnableVirtualMachineNetworkConnection();
                    ResetVirtualMachineNetworkConnection();
                }
            }
        }

        [TestMethod]
        public void  Common_DetectSessionKilledOnServer()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                ManualResetEvent errorOccurredSignaled = new ManualResetEvent(false);
                Exception errorOccurred = null;
                client.ErrorOccurred += (sender, args) =>
                    {
                        errorOccurred = args.Exception;
                        errorOccurredSignaled.Set();
                    };
                client.Connect();

                // Kill the server session
                using (var adminClient = new SshClient(_adminConnectionInfoFactory.Create()))
                {
                    adminClient.Connect();

                    var command = $"sudo ps --no-headers -u {client.ConnectionInfo.Username} -f | grep \"{client.ConnectionInfo.Username}@notty\" | awk '{{print $2}}' | xargs sudo kill -9";
                    var sshCommand = adminClient.CreateCommand(command);
                    var result = sshCommand.Execute();
                    Assert.AreEqual(0, sshCommand.ExitStatus, sshCommand.Error);
                }

                Assert.IsTrue(errorOccurredSignaled.WaitOne(200));
                Assert.IsNotNull(errorOccurred);
                Assert.AreEqual(typeof(SshConnectionException), errorOccurred.GetType());
                Assert.IsNull(errorOccurred.InnerException);
                Assert.AreEqual("An established connection was aborted by the server.", errorOccurred.Message);
                Assert.IsFalse(client.IsConnected);
            }
        }

        [TestMethod]
        public void Common_HostKeyValidation_Failure()
        {
            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.HostKeyReceived += (sender, e) => { e.CanTrust = false; };

                try
                {
                    client.Connect();
                    Assert.Fail();
                }
                catch (SshConnectionException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Key exchange negotiation failed.", ex.Message);
                }
            }
        }

        [TestMethod]
        public void Common_HostKeyValidation_Success()
        {
            byte[] host_rsa_key_openssh_fingerprint =
                {
                    0x3d, 0x90, 0xd8, 0x0d, 0xd5, 0xe0, 0xb6, 0x13,
                    0x42, 0x7c, 0x78, 0x1e, 0x19, 0xa3, 0x99, 0x2b
                };

            var hostValidationSuccessful = false;

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.HostKeyReceived += (sender, e) =>
                    {
                        if (host_rsa_key_openssh_fingerprint.Length == e.FingerPrint.Length)
                        {
                            for (var i = 0; i < host_rsa_key_openssh_fingerprint.Length; i++)
                            {
                                if (host_rsa_key_openssh_fingerprint[i] != e.FingerPrint[i])
                                {
                                    e.CanTrust = false;
                                    break;
                                }
                            }

                            hostValidationSuccessful = e.CanTrust;
                        }
                        else
                        {
                            e.CanTrust = false;
                        }
                    };
                client.Connect();
            }

            Assert.IsTrue(hostValidationSuccessful);
        }

        /// <summary>
        /// Verifies whether we handle a disconnect initiated by the SSH server (through a SSH_MSG_DISCONNECT message).
        /// </summary>
        /// <remarks>
        /// We force this by only configuring <c>keyboard-interactive</c> as authentication method, while <c>ChallengeResponseAuthentication</c>
        /// is not enabled.  This causes OpenSSH to terminate the connection because there are no authentication methods left.
        /// </remarks>
        [TestMethod]
        public void Common_ServerRejectsConnection()
        {
            _remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, "keyboard-interactive")
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(_authenticationMethodFactory.CreateRegularUserKeyboardInteractiveAuthenticationMethod());
            using (var client = new SftpClient(connectionInfo))
            {
                try
                {
                    client.Connect();
                    Assert.Fail();
                }
                catch (SshConnectionException ex)
                {
                    Assert.AreEqual(DisconnectReason.ProtocolError, ex.DisconnectReason);
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("The connection was closed by the server: no authentication methods enabled (ProtocolError).", ex.Message);
                }
            }
        }


        [TestMethod]
        [WorkItem(1140)]
        [Description("Test whether IsConnected is false after disconnect.")]
        [Owner("Kenneth_aa")]
        public void Test_BaseClient_IsConnected_True_After_Disconnect()
        {
            // 2012-04-29 - Kenneth_aa
            // The problem with this test, is that after SSH Net calls .Disconnect(), the library doesn't wait
            // for the server to confirm disconnect before IsConnected is checked. And now I'm not mentioning
            // anything about Socket's either.

            var connectionInfo = new PasswordAuthenticationMethod(User.UserName, User.Password);

            using (SftpClient client = new SftpClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                client.Connect();
                Assert.AreEqual(true, client.IsConnected, "IsConnected is not true after Connect() was called.");

                client.Disconnect();

                Assert.AreEqual(false, client.IsConnected, "IsConnected is true after Disconnect() was called.");
            }
        }

        private static void DisableHostNetworkConnection(string networkConnection)
        {
            SelectQuery wmiQuery = new SelectQuery("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionId != NULL");
            ManagementObjectSearcher searchProcedure = new ManagementObjectSearcher(wmiQuery);
            foreach (ManagementObject item in searchProcedure.Get())
            {
                var netConnectionId = (string)item["NetConnectionId"];

                if (netConnectionId == networkConnection)
                {
                    var returnValue = item.InvokeMethod("Disable", null);
                    if (returnValue is uint retValue)
                    {
                        if (retValue == 0)
                        {
                            return;
                        }

                        throw new ApplicationException($"Failed to disable '{networkConnection}' network connection. Return value is {retValue}.{Environment.NewLine}Make sure you're running the tests with elevated priviliges.");
                    }
                    else if (returnValue == null)
                    {
                        throw new ApplicationException($"Failed to disable '{networkConnection}' network connection. Return value is null.");
                    }
                    else
                    {
                        throw new ApplicationException($"Failed to disable '{networkConnection}' network connection. Unexpected return value {returnValue} ({returnValue.GetType()}).");
                    }
                }
            }

            throw new ApplicationException($"Failed to disable '{networkConnection}' network connection. Network connection not found.");
        }

        private static void EnableHostNetworkConnection(string networkConnection)
        {
            SelectQuery wmiQuery = new SelectQuery("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionId != NULL");
            ManagementObjectSearcher searchProcedure = new ManagementObjectSearcher(wmiQuery);
            foreach (ManagementObject item in searchProcedure.Get())
            {
                var netConnectionId = (string)item["NetConnectionId"];

                if (netConnectionId == networkConnection)
                {
                    var returnValue = item.InvokeMethod("Enable", null);
                    if (returnValue is uint retValue)
                    {
                        if (retValue == 0u)
                        {
                            Console.WriteLine($"Enable host network connection for '{networkConnection}'.");
                            Thread.Sleep(5000);
                            return;
                        }

                        throw new ApplicationException($"Failed to enable '{networkConnection}' network connection. Return value is {retValue}..{Environment.NewLine}Make sure you're running the tests with elevated priviliges.");
                    }
                    else if (returnValue == null)
                    {
                        throw new ApplicationException($"Failed to enable '{networkConnection}' network connection. Return value is null.");
                    }
                    else
                    {
                        throw new ApplicationException($"Failed to enable '{networkConnection}' network connection. Unexpected return value {returnValue} ({returnValue.GetType()}).");
                    }
                }
            }

            throw new ApplicationException($"Failed to enable '{networkConnection}' network connection. Network connection not found.");
        }

        private static string VirtualBoxFolder
        {
            get
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    if (!Environment.Is64BitProcess)
                    {
                        // dotnet test runs tests in a 32-bit process (no watter what I f***in' try), so let's hard-code the
                        // path to VirtualBox
                        return Path.Combine("c:\\Program Files", "Oracle", "VirtualBox");
                    }
                }

                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Oracle", "VirtualBox");
            }
        }

        private static List<string> GetRunningVMs()
        {
            var runningVmRegex = new Regex("\"(?<name>.+?)\"\\s?(?<uuid>{.+?})");

            var startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(VirtualBoxFolder, "VBoxManage.exe"),
                    Arguments = "list runningvms",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };

            var process = Process.Start(startInfo);
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new ApplicationException($"Failed to get list of running VMs. Exit code is {process.ExitCode}.");
            }

            var runningVms = new List<string>();

            string line;

            while ((line = process.StandardOutput.ReadLine()) != null)
            {
                var match = runningVmRegex.Match(line);
                if (match != null)
                {
                    runningVms.Add(match.Groups["name"].Value);
                }
            }

            return runningVms;
        }

        private static void SetLinkState(string vmName, bool on)
        {
            var linkStateValue = (on ? "on" : "off");

            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(VirtualBoxFolder, "VBoxManage.exe"),
                Arguments = $"controlvm \"{vmName}\" setlinkstate1 {linkStateValue}",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = Process.Start(startInfo);
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new ApplicationException($"Failed to set linkstate for VM '{vmName}' to '{linkStateValue}'. Exit code is {process.ExitCode}.");
            }
            else
            {
                Console.WriteLine($"Changed linkstate for VM '{vmName}' to '{linkStateValue}.");
            }
        }

        private static void SetPromiscuousMode(string vmName, string value)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(VirtualBoxFolder, "VBoxManage.exe"),
                Arguments = $"controlvm \"{vmName}\" nicpromisc1 {value}",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var process = Process.Start(startInfo);
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new ApplicationException($"Failed to set promiscuous for VM '{vmName}' to '{value}'. Exit code is {process.ExitCode}.");
            }
            else
            {
                Console.WriteLine($"Changed promiscuous for VM '{vmName}' to '{value}'.");
            }
        }

        private static void DisableVirtualMachineNetworkConnection()
        {
            var runningVMs = GetRunningVMs();
            Assert.AreEqual(1, runningVMs.Count);

            SetLinkState(runningVMs[0], false);
            Thread.Sleep(1000);
        }

        private static void EnableVirtualMachineNetworkConnection()
        {
            var runningVMs = GetRunningVMs();
            Assert.AreEqual(1, runningVMs.Count);

            SetLinkState(runningVMs[0], true);
            Thread.Sleep(1000);
        }

        private static void ResetVirtualMachineNetworkConnection()
        {
            var runningVMs = GetRunningVMs();
            Assert.AreEqual(1, runningVMs.Count);

            SetPromiscuousMode(runningVMs[0], "allow-all");
            Thread.Sleep(1000);
            SetPromiscuousMode(runningVMs[0], "deny");
            Thread.Sleep(1000);
        }
    }
}
