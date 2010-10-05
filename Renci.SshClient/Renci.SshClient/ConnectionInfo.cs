using System;
namespace Renci.SshClient
{
    public class ConnectionInfo
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public PrivateKeyFile KeyFile { get; set; }

        public TimeSpan Timeout { get; set; }

        public int RetryAttempts { get; set; }

        public int MaxSessions { get; set; }

        public ConnectionInfo()
        {
            //  Set default connection values
            this.Port = 22;
            this.Timeout = TimeSpan.FromMinutes(30);
            this.RetryAttempts = 10;
            this.MaxSessions = 10;
        }
    }
}
