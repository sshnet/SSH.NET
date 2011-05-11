
namespace Renci.SshClient
{
    public abstract class SshBase
    {
        public ConnectionInfo ConnectionInfo { get; private set; }

        internal Session Session { get; private set; }

        public SshBase(ConnectionInfo connectionInfo)
        {
            this.ConnectionInfo = connectionInfo;
            this.Session = new Session(this.ConnectionInfo);
        }

        public SshBase(string host, int port, string username, string password)
            : this(new ConnectionInfo
            {
                Host = host,
                Port = port,
                Username = username,
                Password = password,
            })
        {
        }

        public SshBase(string host, string username, string password)
            : this(new ConnectionInfo
            {
                Host = host,
                Username = username,
                Password = password,
            })
        {
        }

        public SshBase(string host, int port, string username, PrivateKeyFile keyFile)
            : this(new ConnectionInfo
            {
                Host = host,
                Port = port,
                Username = username,
                KeyFile = keyFile,
            })
        {
        }

        public SshBase(string host, string username, PrivateKeyFile keyFile)
            : this(new ConnectionInfo
            {
                Host = host,
                Username = username,
                KeyFile = keyFile,
            })
        {
        }

        public void Connect()
        {
            this.Session.Connect();
        }

        public virtual void Disconnect()
        {
            this.Session.Disconnect();
        }

    }
}
