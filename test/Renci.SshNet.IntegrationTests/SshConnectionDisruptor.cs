namespace Renci.SshNet.IntegrationTests
{
    internal class SshConnectionDisruptor
    {
        private readonly IConnectionInfoFactory _connectionInfoFactory;

        public SshConnectionDisruptor(IConnectionInfoFactory connectionInfoFactory)
        {
            _connectionInfoFactory = connectionInfoFactory;
        }

        public SshConnectionRestorer BreakConnections()
        {
            var client = new SshClient(_connectionInfoFactory.Create());

            client.Connect();

            PauseSshd(client);

            return new SshConnectionRestorer(client);
        }

        private static void PauseSshd(SshClient client)
        {
            var command = client.CreateCommand("sudo echo 'DenyUsers sshnet' >> /etc/ssh/sshd_config");
            var output = command.Execute();
            if (command.ExitStatus != 0)
            {
                throw new ApplicationException(
                    $"Blocking user sshnet failed with exit code {command.ExitStatus}.\r\n{output}\r\n{command.GetError()}");
            }
            command = client.CreateCommand("sudo pkill -9 -U sshnet -f sshd.pam");
            output = command.Execute();
            if (command.ExitStatus != 0)
            {
                throw new ApplicationException(
                    $"Killing sshd.pam service failed with exit code {command.ExitStatus}.\r\n{output}\r\n{command.GetError()}");
            }
        }
    }
}
