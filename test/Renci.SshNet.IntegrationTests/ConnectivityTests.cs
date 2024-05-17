using Renci.SshNet.Common;
using Renci.SshNet.IntegrationTests.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.IntegrationTests
{
    [TestClass]
    public class ConnectivityTests : IntegrationTestBase
    {
        private AuthenticationMethodFactory _authenticationMethodFactory;
        private IConnectionInfoFactory _connectionInfoFactory;
        private IConnectionInfoFactory _adminConnectionInfoFactory;
        private RemoteSshdConfig _remoteSshdConfig;
        private SshConnectionDisruptor _sshConnectionDisruptor;

        [TestInitialize]
        public void SetUp()
        {
            _authenticationMethodFactory = new AuthenticationMethodFactory();
            _connectionInfoFactory = new LinuxVMConnectionFactory(SshServerHostName, SshServerPort, _authenticationMethodFactory);
            _adminConnectionInfoFactory = new LinuxAdminConnectionFactory(SshServerHostName, SshServerPort);
            _remoteSshdConfig = new RemoteSshd(_adminConnectionInfoFactory).OpenConfig();
            _sshConnectionDisruptor = new SshConnectionDisruptor(_adminConnectionInfoFactory);
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
                    using (var stream = client.CreateShellStream("vt220", 20, 20, 20, 20, 20))
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
            SshConnectionRestorer disruptor = null;
            try
            {
                Exception errorOccurred = null;
                int count = 0;
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    client.ErrorOccurred += (sender, args) =>
                                                {
                                                    Console.WriteLine("Exception " + count++);
                                                    Console.WriteLine(args.Exception);
                                                    errorOccurred = args.Exception;
                                                };
                    client.Connect();

                    disruptor = _sshConnectionDisruptor.BreakConnections();
                    hostNetworkConnectionDisabled = true;
                    WaitForConnectionInterruption(client);
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
                    disruptor?.RestoreConnections();
                    disruptor?.Dispose();
                }
            }
        }

        [TestMethod]
        public void Common_DetectLossOfNetworkConnectivityThroughKeepAlive()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                Exception errorOccurred = null;
                int count = 0;
                client.ErrorOccurred += (sender, args) =>
                                            {
                                                Console.WriteLine("Exception "+ count++);
                                                Console.WriteLine(args.Exception);
                                                errorOccurred = args.Exception;
                                            };
                client.KeepAliveInterval = new TimeSpan(0, 0, 0, 0, 50);
                client.Connect();

                var disruptor = _sshConnectionDisruptor.BreakConnections();

                try
                {
                    WaitForConnectionInterruption(client);

                    Assert.IsFalse(client.IsConnected);

                    Assert.IsNotNull(errorOccurred);
                    Assert.AreEqual(typeof(SshConnectionException), errorOccurred.GetType());

                    var connectionException = (SshConnectionException) errorOccurred;
                    Assert.AreEqual(DisconnectReason.ConnectionLost, connectionException.DisconnectReason);
                    Assert.IsNull(connectionException.InnerException);
                    Assert.AreEqual("An established connection was aborted by the server.", connectionException.Message);
                }
                finally
                {
                    disruptor?.RestoreConnections();
                    disruptor?.Dispose();
                }
            }
        }

        private static void WaitForConnectionInterruption(SftpClient client)
        {
            for (var i = 0; i < 500; i++)
            {
                if (!client.IsConnected)
                {
                    break;
                }

                Thread.Sleep(100);
            }

            // After interruption, you have to wait for the events to propagate.
            Thread.Sleep(100);
        }

        [TestMethod]
        public void Common_DetectConnectionResetThroughSftpInvocation()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.KeepAliveInterval = TimeSpan.FromSeconds(1);
                client.OperationTimeout = TimeSpan.FromSeconds(60);
                ManualResetEvent errorOccurredSignaled = new ManualResetEvent(false);
                Exception errorOccurred = null;
                client.ErrorOccurred += (sender, args) =>
                {
                    errorOccurred = args.Exception;
                    errorOccurredSignaled.Set();
                };
                client.Connect();

                var disruptor = _sshConnectionDisruptor.BreakConnections();

                try
                {
                    WaitForConnectionInterruption(client);
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
                    disruptor.RestoreConnections();
                    disruptor.Dispose();
                }
            }
        }

        [TestMethod]
        public void Common_LossOfNetworkConnectivityDisconnectAndConnect()
        {
            bool vmNetworkConnectionDisabled = false;
            SshConnectionRestorer disruptor = null;
            try
            {
                using (var client = new SftpClient(_connectionInfoFactory.Create()))
                {
                    Exception errorOccurred = null;
                    client.ErrorOccurred += (sender, args) => errorOccurred = args.Exception;

                    client.Connect();

                    disruptor = _sshConnectionDisruptor.BreakConnections();
                    vmNetworkConnectionDisabled = true;

                    WaitForConnectionInterruption(client);
                    // disconnect while network connectivity is lost
                    client.Disconnect();

                    Assert.IsFalse(client.IsConnected);

                    disruptor.RestoreConnections();
                    vmNetworkConnectionDisabled = false;

                    // connect when network connectivity is restored
                    client.Connect();
                    client.ChangeDirectory(client.WorkingDirectory);
                    client.Dispose();

                    Assert.IsNotNull(errorOccurred);
                    Assert.AreEqual(typeof(SshConnectionException), errorOccurred.GetType());

                    var connectionException = (SshConnectionException) errorOccurred;
                    Assert.AreEqual(DisconnectReason.ConnectionLost, connectionException.DisconnectReason);
                    Assert.IsNull(connectionException.InnerException);
                    Assert.AreEqual("An established connection was aborted by the server.", connectionException.Message);
                }
            }
            finally
            {
                if (vmNetworkConnectionDisabled)
                {
                    disruptor.RestoreConnections();
                }
                disruptor?.Dispose();
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

                var disruptor = _sshConnectionDisruptor.BreakConnections();
                try
                {
                    WaitForConnectionInterruption(client);
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
                    disruptor.RestoreConnections();
                    disruptor.Dispose();
                }
            }
        }

        [TestMethod]
        public void SftpClient_HandleSftpSessionClose()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();
                Assert.IsTrue(client.IsConnected);

                client.SftpSession.Disconnect();
                Assert.IsFalse(client.IsConnected);

                client.Connect();
                Assert.IsTrue(client.IsConnected);

                client.Disconnect();
                Assert.IsFalse(client.IsConnected);
            }
        }

        [TestMethod]
        public async Task SftpClient_HandleSftpSessionCloseAsync()
        {
            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                await client.ConnectAsync(CancellationToken.None);
                Assert.IsTrue(client.IsConnected);

                client.SftpSession.Disconnect();
                Assert.IsFalse(client.IsConnected);

                await client.ConnectAsync(CancellationToken.None);
                Assert.IsTrue(client.IsConnected);

                client.Disconnect();
                Assert.IsFalse(client.IsConnected);
            }
        }

        [TestMethod]
        public void Common_DetectSessionKilledOnServer()
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

        [TestMethod]
        public void Common_HostKeyValidationSHA256_Success()
        {
            var hostValidationSuccessful = false;

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.HostKeyReceived += (sender, e) =>
                {
                    if (e.FingerPrintSHA256 == "9fa6vbz64gimzsGZ/xZi3aaYE1o7E96iU2NjcfQNGwI")
                    {
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

        [TestMethod]
        public void Common_HostKeyValidationMD5_Success()
        {
            var hostValidationSuccessful = false;

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.HostKeyReceived += (sender, e) =>
                {
                    if (e.FingerPrintMD5 == "3d:90:d8:0d:d5:e0:b6:13:42:7c:78:1e:19:a3:99:2b")
                    {
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

    }
}
