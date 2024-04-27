using Renci.SshNet.Common;
using Renci.SshNet.IntegrationTests.Common;

namespace Renci.SshNet.IntegrationTests
{
    [TestClass]
    public class AuthenticationTests : IntegrationTestBase
    {
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

            using (var client = new SshClient(_adminConnectionInfoFactory.Create()))
            {
                client.Connect();

                // Reset the password back to the "regular" password.
                using (var cmd = client.RunCommand($"echo \"{Users.Regular.Password}\n{Users.Regular.Password}\" | sudo passwd " + Users.Regular.UserName))
                {
                    Assert.AreEqual(0, cmd.ExitStatus, cmd.Error);
                }

                // Remove password expiration
                using (var cmd = client.RunCommand($"sudo chage --expiredate -1 " + Users.Regular.UserName))
                {
                    Assert.AreEqual(0, cmd.ExitStatus, cmd.Error);
                }
            }
        }

        [TestMethod]
        public void Multifactor_PublicKey()
        {
            _remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, "publickey")
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(_authenticationMethodFactory.CreateRegularUserPrivateKeyAuthenticationMethod());
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
            }
        }

        [TestMethod]
        [TestCategory("Authentication")]
        public void Multifactor_PublicKey_Connect_Then_Reconnect()
        {
            _remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, "publickey")
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(_authenticationMethodFactory.CreateRegularUserPrivateKeyAuthenticationMethod());
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        public void Multifactor_PublicKeyWithPassPhrase()
        {
            _remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, "publickey")
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(_authenticationMethodFactory.CreateRegularUserPrivateKeyWithPassPhraseAuthenticationMethod());
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SshPassPhraseNullOrEmptyException))]
        public void Multifactor_PublicKeyWithEmptyPassPhrase()
        {
            _remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, "publickey")
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(_authenticationMethodFactory.CreateRegularUserPrivateKeyWithEmptyPassPhraseAuthenticationMethod());
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
            }
        }

        [TestMethod]
        public void Multifactor_PublicKey_MultiplePrivateKey()
        {
            _remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, "publickey")
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(_authenticationMethodFactory.CreateRegularUserMultiplePrivateKeyAuthenticationMethod());
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
            }
        }

        [TestMethod]
        public void Multifactor_PublicKey_MultipleAuthenticationMethod()
        {
            _remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, "publickey")
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(_authenticationMethodFactory.CreateRegularUserPrivateKeyAuthenticationMethod(),
                                                               _authenticationMethodFactory.CreateRegularUserPrivateKeyAuthenticationMethod());
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
            }
        }

        [TestMethod]
        public void Multifactor_KeyboardInteractiveAndPublicKey()
        {
            _remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, "keyboard-interactive,publickey")
                             .WithChallengeResponseAuthentication(true)
                             .WithKeyboardInteractiveAuthentication(true)
                             .WithUsePAM(true)
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(_authenticationMethodFactory.CreateRegularUserPasswordAuthenticationMethodWithBadPassword(),
                                                               _authenticationMethodFactory.CreateRegularUserKeyboardInteractiveAuthenticationMethod(),
                                                               _authenticationMethodFactory.CreateRegularUserPrivateKeyAuthenticationMethod());
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
            }
        }

        [TestMethod]
        public void Multifactor_Password_ExceedsPartialSuccessLimit()
        {
            // configure server to require more successfull authentications from a given method than our partial
            // success limit (5) allows
            _remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, "password,password,password,password,password,password")
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(_authenticationMethodFactory.CreateRegularUserPasswordAuthenticationMethod());
            using (var client = new SftpClient(connectionInfo))
            {
                try
                {
                    client.Connect();
                    Assert.Fail();
                }
                catch (SshAuthenticationException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Reached authentication attempt limit for method (password).", ex.Message);
                }
            }
        }

        [TestMethod]
        public void Multifactor_Password_MatchPartialSuccessLimit()
        {
            // configure server to require a number of successfull authentications from a given method that exactly
            // matches our partial success limit (5)

            _remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, "password,password,password,password,password")
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(_authenticationMethodFactory.CreateRegularUserPasswordAuthenticationMethod());
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
            }
        }

        [TestMethod]
        public void Multifactor_Password_Or_PublicKeyAndKeyboardInteractive()
        {
            _remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, "password publickey,keyboard-interactive")
                             .WithChallengeResponseAuthentication(true)
                             .WithKeyboardInteractiveAuthentication(true)
                             .WithUsePAM(true)
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(_authenticationMethodFactory.CreateRegularUserPrivateKeyAuthenticationMethod(),
                                                               _authenticationMethodFactory.CreateRegularUserPasswordAuthenticationMethod());
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
            }
        }

        [TestMethod]
        public void Multifactor_Password_Or_PublicKeyAndPassword_BadPassword()
        {
            _remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, "password publickey,password")
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(_authenticationMethodFactory.CreateRegularUserPasswordAuthenticationMethodWithBadPassword(),
                                                               _authenticationMethodFactory.CreateRegularUserPrivateKeyAuthenticationMethod());
            using (var client = new SftpClient(connectionInfo))
            {
                try
                {
                    client.Connect();
                    Assert.Fail();
                }
                catch (SshAuthenticationException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Permission denied (password).", ex.Message);
                }
            }
        }

        [TestMethod]
        public void Multifactor_PasswordAndPublicKey_Or_PasswordAndPassword()
        {
            _remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, "password,publickey password,password")
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(_authenticationMethodFactory.CreateRegularUserPasswordAuthenticationMethod(),
                                                               _authenticationMethodFactory.CreateRegularUserPrivateKeyAuthenticationMethodWithBadKey());
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
            }

            connectionInfo = _connectionInfoFactory.Create(_authenticationMethodFactory.CreateRegularUserPasswordAuthenticationMethodWithBadPassword(),
                                                               _authenticationMethodFactory.CreateRegularUserPrivateKeyAuthenticationMethod());
            using (var client = new SftpClient(connectionInfo))
            {
                try
                {
                    client.Connect();
                    Assert.Fail();
                }
                catch (SshAuthenticationException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Permission denied (password).", ex.Message);
                }
            }

        }

        [TestMethod]
        public void Multifactor_PasswordAndPassword_Or_PublicKey()
        {
            _remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, "password,password publickey")
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(_authenticationMethodFactory.CreateRegularUserPasswordAuthenticationMethod(),
                                                               _authenticationMethodFactory.CreateRegularUserPrivateKeyAuthenticationMethodWithBadKey());
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
            }

            connectionInfo = _connectionInfoFactory.Create(_authenticationMethodFactory.CreateRegularUserPasswordAuthenticationMethod());
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
            }

        }

        [TestMethod]
        public void Multifactor_Password_Or_Password()
        {
            _remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, "password password")
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(_authenticationMethodFactory.CreateRegularUserPasswordAuthenticationMethod());
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
            }

            connectionInfo = _connectionInfoFactory.Create(_authenticationMethodFactory.CreateRegularUserPasswordAuthenticationMethod(),
                                                           _authenticationMethodFactory.CreateRegularUserPrivateKeyAuthenticationMethodWithBadKey());
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
            }
        }

        [TestMethod]
        public void KeyboardInteractive_PasswordExpired()
        {
            var temporaryPassword = new Random().Next().ToString();

            using (var client = new SshClient(_adminConnectionInfoFactory.Create()))
            {
                client.Connect();

                // Temporarity modify password so that when we expire this password, we change reset the password back to
                // the "regular" password.
                using (var cmd = client.RunCommand($"echo \"{temporaryPassword}\n{temporaryPassword}\" | sudo passwd " + Users.Regular.UserName))
                {
                    Assert.AreEqual(0, cmd.ExitStatus, cmd.Error);
                }

                // Force the password to expire immediately
                using (var cmd = client.RunCommand($"sudo chage -d 0 " + Users.Regular.UserName))
                {
                    Assert.AreEqual(0, cmd.ExitStatus, cmd.Error);
                }
            }

            _remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, "keyboard-interactive")
                             .WithChallengeResponseAuthentication(true)
                             .WithKeyboardInteractiveAuthentication(true)
                             .WithUsePAM(true)
                             .Update()
                             .Restart();

            var keyboardInteractive = new KeyboardInteractiveAuthenticationMethod(Users.Regular.UserName);
            int authenticationPromptCount = 0;
            keyboardInteractive.AuthenticationPrompt += (sender, args) =>
            {
                Console.WriteLine(args.Instruction);
                foreach (var authenticationPrompt in args.Prompts)
                {
                    Console.WriteLine(authenticationPrompt.Request);
                    switch (authenticationPromptCount)
                    {
                        case 0:
                            // Regular password prompt
                            authenticationPrompt.Response = temporaryPassword;
                            break;
                        case 1:
                            // Password expired, provide current password
                            authenticationPrompt.Response = temporaryPassword;
                            break;
                        case 2:
                            // Password expired, provide new password
                            authenticationPrompt.Response = Users.Regular.Password;
                            break;
                        case 3:
                            // Password expired, retype new password
                            authenticationPrompt.Response = Users.Regular.Password;
                            break;
                    }

                    authenticationPromptCount++;
                }
            };

            var connectionInfo = _connectionInfoFactory.Create(keyboardInteractive);
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                Assert.AreEqual(4, authenticationPromptCount);
            }
        }

        [TestMethod]
        public void KeyboardInteractiveConnectionInfo()
        {
            _remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, "keyboard-interactive")
                             .WithChallengeResponseAuthentication(true)
                             .WithKeyboardInteractiveAuthentication(true)
                             .WithUsePAM(true)
                             .Update()
                             .Restart();

            var host = SshServerHostName;
            var port = SshServerPort;
            var username = User.UserName;
            var password = User.Password;

            #region Example KeyboardInteractiveConnectionInfo AuthenticationPrompt

            var connectionInfo = new KeyboardInteractiveConnectionInfo(host, port, username);
            connectionInfo.AuthenticationPrompt += delegate (object sender, AuthenticationPromptEventArgs e)
                                                       {
                                                           Console.WriteLine(e.Instruction);

                                                           foreach (var prompt in e.Prompts)
                                                           {
                                                               Console.WriteLine(prompt.Request);
                                                               prompt.Response = password;
                                                           }
                                                       };

            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();

                //  Do something here
                client.Disconnect();
            }

            #endregion

            Assert.AreEqual(connectionInfo.Host, SshServerHostName);
            Assert.AreEqual(connectionInfo.Username, User.UserName);
        }

        [TestMethod]
        public void KeyboardInteractive_NoResponseSet_ThrowsSshAuthenticationException()
        {
            // ...instead of a cryptic ArgumentNullException
            // https://github.com/sshnet/SSH.NET/issues/382

            _remoteSshdConfig.WithAuthenticationMethods(Users.Regular.UserName, "keyboard-interactive")
                             .WithChallengeResponseAuthentication(true)
                             .WithKeyboardInteractiveAuthentication(true)
                             .WithUsePAM(true)
                             .Update()
                             .Restart();

            var connectionInfo = _connectionInfoFactory.Create(new KeyboardInteractiveAuthenticationMethod(Users.Regular.UserName));

            using (var client = new SftpClient(connectionInfo))
            {
                try
                {
                    client.Connect();
                    Assert.Fail();
                }
                catch (SshAuthenticationException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.IsTrue(ex.Message.StartsWith("AuthenticationPrompt.Response is null for prompt \"Password: \""), $"Message was \"{ex.Message}\"");
                }
            }
        }
    }
}
