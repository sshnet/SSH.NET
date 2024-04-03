namespace Renci.SshNet.IntegrationTests
{
    public class SshConnectionRestorer : IDisposable
    {
        private SshClient _sshClient;

        public SshConnectionRestorer(SshClient sshClient)
        {
            _sshClient = sshClient;
        }

        public void RestoreConnections()
        {
            var command = _sshClient.CreateCommand("sudo sed -i '/DenyUsers sshnet/d' /etc/ssh/sshd_config");
            var output = command.Execute();
            if (command.ExitStatus != 0)
            {
                throw new ApplicationException(
                    $"Unblocking user sshnet failed with exit code {command.ExitStatus}.\r\n{output}\r\n{command.Error}");
            }
            command = _sshClient.CreateCommand("sudo /usr/sbin/sshd.pam");
            output = command.Execute();
            if (command.ExitStatus != 0)
            {
                throw new ApplicationException(
                    $"Resuming ssh service failed with exit code {command.ExitStatus}.\r\n{output}\r\n{command.Error}");
            }
        }

        public void Dispose()
        {
            _sshClient?.Dispose();
            _sshClient = null;
        }
    }
}
