namespace Renci.SshClient
{
    public class ConnectionInfo
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public KeyFile KeyFile { get; set; }

        public ConnectionInfo()
        {
            //  Set default connection values
            this.Port = 22;
        }
    }
}
