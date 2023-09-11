namespace Renci.SshNet.IntegrationTests
{
    internal class ProcessDisruptor
    {
        private readonly IConnectionInfoFactory _connectionInfoFactory;

        public ProcessDisruptor(IConnectionInfoFactory connectionInfoFactory)
        {
            _connectionInfoFactory = connectionInfoFactory;
        }

        public ProcessDisruptorOperation BreakConnections()
        {
            var client = new SshClient(_connectionInfoFactory.Create());
            
            client.Connect();

            PauseSshd(client);
            
            return new ProcessDisruptorOperation(client);
        }

        private static void PauseSshd(SshClient client)
        {
            var command = client.CreateCommand("sudo echo 'DenyUsers sshnet' >> /etc/ssh/sshd_config");
            var output = command.Execute();
            if (command.ExitStatus != 0)
            {
                throw new ApplicationException(
                    $"Resuming ssh service failed with exit code {command.ExitStatus}.\r\n{output}\r\n{command.Error}");
            }
            command = client.CreateCommand("sudo pkill -9 -U sshnet -f sshd.pam");
            output = command.Execute();
            if (command.ExitStatus != 0)
            {
                throw new ApplicationException(
                    $"Resuming ssh service failed with exit code {command.ExitStatus}.\r\n{output}\r\n{command.Error}");
            }
        }
    }
}
