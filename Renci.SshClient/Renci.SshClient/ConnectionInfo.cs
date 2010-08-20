namespace Renci.SshClient
{
    public class ConnectionInfo
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public PrivateKeyFile KeyFile { get; set; }

        public int Timeout { get; set; }

        public ConnectionInfo()
        {
            //  Set default connection values
            this.Port = 22;
            this.Timeout = 1000 * 10;   //  Set default timeout to 10 sec
        }
    }
}
