using System.Globalization;

namespace Renci.SshNet.IntegrationTests
{
    internal sealed class RemoteSshd
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
                    throw new ApplicationException($"Stopping ssh service failed with exit code {stopCommand.ExitStatus.ToString(CultureInfo.InvariantCulture)}.\r\n{stopOutput}\r\n{stopCommand.Error}");
                }

                var resetFailedCommand = client.CreateCommand("sudo /usr/sbin/sshd.pam");
                var resetFailedOutput = resetFailedCommand.Execute();
                if (resetFailedCommand.ExitStatus != 0)
                {
                    throw new ApplicationException($"Reset failures for ssh service failed with exit code {resetFailedCommand.ExitStatus.ToString(CultureInfo.InvariantCulture)}.\r\n{resetFailedOutput}\r\n{resetFailedCommand.Error}");
                }
            }

            return this;
        }
    }
}
