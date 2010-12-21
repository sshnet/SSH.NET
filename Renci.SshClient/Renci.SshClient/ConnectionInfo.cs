using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
namespace Renci.SshClient
{
    public class ConnectionInfo
    {
        public string Host { get; private set; }

        public int Port { get; private set; }

        public string Username { get; private set; }

        public string Password { get; private set; }

        public ICollection<PrivateKeyFile> KeyFiles { get; private set; }

        public TimeSpan Timeout { get; set; }

        public int RetryAttempts { get; set; }

        public int MaxSessions { get; set; }

        private ConnectionInfo()
        {
            //  Set default connection values
            this.Timeout = TimeSpan.FromSeconds(30);
            this.RetryAttempts = 10;
            this.MaxSessions = 10;
        }

        public ConnectionInfo(string host, int port, string username, string password)
            : this()
        {
            this.Host = host;
            this.Port = port;
            this.Username = username;
            this.Password = password;
        }

        public ConnectionInfo(string host, int port, string username, params PrivateKeyFile[] keyFiles)
            : this()
        {
            this.Host = host;
            this.Port = port;
            this.Username = username;
            this.KeyFiles = new Collection<PrivateKeyFile>(keyFiles);
        }
    }
}
