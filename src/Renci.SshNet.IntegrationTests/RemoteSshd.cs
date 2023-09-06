﻿using Renci.SshNet.TestTools.OpenSSH;

namespace Renci.SshNet.IntegrationTests
{
    internal class RemoteSshd
    {
        private readonly IConnectionInfoFactory _connectionInfoFactory;

        public RemoteSshd(IConnectionInfoFactory connectionInfoFactory)
        {
            _connectionInfoFactory = connectionInfoFactory;
        }

        public RemoteSshdConfig OpenConfig()
        {
            return new RemoteSshdConfig(this, _connectionInfoFactory);
        }

        public RemoteSshd Restart()
        {
            // Restart SSH daemon
            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                // Kill all processes that start with 'sshd' and that run as root
                var stopCommand = client.CreateCommand("sudo pkill -9 -U 0 sshd.pam");
                var stopOutput = stopCommand.Execute();
                if (stopCommand.ExitStatus != 0)
                {
                    throw new ApplicationException($"Stopping ssh service failed with exit code {stopCommand.ExitStatus}.\r\n{stopOutput}");
                }

                var resetFailedCommand = client.CreateCommand("sudo /usr/sbin/sshd.pam");
                var resetFailedOutput = resetFailedCommand.Execute();
                if (resetFailedCommand.ExitStatus != 0)
                {
                    throw new ApplicationException($"Reset failures for ssh service failed with exit code {resetFailedCommand.ExitStatus}.\r\n{resetFailedOutput}");
                }
            }

            return this;
        }
    }

    internal class RemoteSshdConfig
    {
        private const string SshdConfigFilePath = "/etc/ssh/sshd_config";
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false, true);

        private readonly RemoteSshd _remoteSshd;
        private readonly IConnectionInfoFactory _connectionInfoFactory;
        private readonly SshdConfig _config;

        public RemoteSshdConfig(RemoteSshd remoteSshd, IConnectionInfoFactory connectionInfoFactory)
        {
            _remoteSshd = remoteSshd;
            _connectionInfoFactory = connectionInfoFactory;

            using (var client = new ScpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var memoryStream = new MemoryStream())
                {
                    client.Download(SshdConfigFilePath, memoryStream);

                    memoryStream.Position = 0;
                    _config = SshdConfig.LoadFrom(memoryStream, Encoding.UTF8);
                }
            }
        }

        /// <summary>
        /// Specifies whether challenge-response authentication is allowed.
        /// </summary>
        /// <param name="value"><see langword="true"/> to allow challenge-response authentication.</param>
        /// <returns>
        /// The current <see cref="RemoteSshdConfig"/> instance.
        /// </returns>
        public RemoteSshdConfig WithChallengeResponseAuthentication(bool? value)
        {
            _config.ChallengeResponseAuthentication = value;
            return this;
        }

        /// <summary>
        /// Specifies whether to allow keyboard-interactive authentication.
        /// </summary>
        /// <param name="value"><see langword="true"/> to allow keyboard-interactive authentication.</param>
        /// <returns>
        /// The current <see cref="RemoteSshdConfig"/> instance.
        /// </returns>
        public RemoteSshdConfig WithKeyboardInteractiveAuthentication(bool value)
        {
            _config.KeyboardInteractiveAuthentication = value;
            return this;
        }

        /// <summary>
        /// Specifies whether <c>sshd</c> should print /etc/motd when a user logs in interactively.
        /// </summary>
        /// <param name="value"><see langword="true"/> if <c>sshd</c> should print /etc/motd when a user logs in interactively.</param>
        /// <returns>
        /// The current <see cref="RemoteSshdConfig"/> instance.
        /// </returns>
        public RemoteSshdConfig PrintMotd(bool? value = true)
        {
            _config.PrintMotd = value;
            return this;
        }

        /// <summary>
        /// Specifies whether TCP forwarding is permitted.
        /// </summary>
        /// <param name="value"><see langword="true"/> to allow TCP forwarding.</param>
        /// <returns>
        /// The current <see cref="RemoteSshdConfig"/> instance.
        /// </returns>
        public RemoteSshdConfig AllowTcpForwarding(bool? value = true)
        {
            _config.AllowTcpForwarding = value;
            return this;
        }

        public RemoteSshdConfig WithAuthenticationMethods(string user, string authenticationMethods)
        {
            var sshNetMatch = _config.Matches.FirstOrDefault(m => m.Users.Contains(user));
            if (sshNetMatch == null)
            {
                sshNetMatch = new Match(new[] { user }, new string[0]);
                _config.Matches.Add(sshNetMatch);
            }

            sshNetMatch.AuthenticationMethods = authenticationMethods;

            return this;
        }

        public RemoteSshdConfig ClearCiphers()
        {
            _config.Ciphers.Clear();
            return this;
        }

        public RemoteSshdConfig AddCipher(Cipher cipher)
        {
            _config.Ciphers.Add(cipher);
            return this;
        }

        public RemoteSshdConfig ClearKeyExchangeAlgorithms()
        {
            _config.KeyExchangeAlgorithms.Clear();
            return this;
        }

        public RemoteSshdConfig AddKeyExchangeAlgorithm(KeyExchangeAlgorithm keyExchangeAlgorithm)
        {
            _config.KeyExchangeAlgorithms.Add(keyExchangeAlgorithm);
            return this;
        }

        public RemoteSshdConfig ClearPublicKeyAcceptedAlgorithms()
        {
            _config.PublicKeyAcceptedAlgorithms.Clear();
            return this;
        }

        public RemoteSshdConfig AddPublicKeyAcceptedAlgorithms(PublicKeyAlgorithm publicKeyAlgorithm)
        {
            _config.PublicKeyAcceptedAlgorithms.Add(publicKeyAlgorithm);
            return this;
        }

        public RemoteSshdConfig ClearHostKeyAlgorithms()
        {
            _config.HostKeyAlgorithms.Clear();
            return this;
        }

        public RemoteSshdConfig AddHostKeyAlgorithm(HostKeyAlgorithm hostKeyAlgorithm)
        {
            _config.HostKeyAlgorithms.Add(hostKeyAlgorithm);
            return this;
        }

        public RemoteSshdConfig ClearSubsystems()
        {
            _config.Subsystems.Clear();
            return this;
        }

        public RemoteSshdConfig AddSubsystem(Subsystem subsystem)
        {
            _config.Subsystems.Add(subsystem);
            return this;
        }

        public RemoteSshdConfig WithLogLevel(LogLevel logLevel)
        {
            _config.LogLevel = logLevel;
            return this;
        }

        public RemoteSshdConfig WithUsePAM(bool usePAM)
        {
            _config.UsePAM = usePAM;
            return this;
        }

        public RemoteSshdConfig ClearHostKeyFiles()
        {
            _config.HostKeyFiles.Clear();
            return this;
        }

        public RemoteSshdConfig AddHostKeyFile(string hostKeyFile)
        {
            _config.HostKeyFiles.Add(hostKeyFile);
            return this;
        }

        public RemoteSshd Update()
        {
            using (var client = new ScpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var memoryStream = new MemoryStream())
                using (var sw = new StreamWriter(memoryStream, Utf8NoBom))
                {
                    sw.NewLine = "\n";
                    _config.SaveTo(sw);
                    sw.Flush();

                    memoryStream.Position = 0;

                    client.Upload(memoryStream, SshdConfigFilePath);
                }
            }

            return _remoteSshd;
        }
    }
}
