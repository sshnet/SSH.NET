
using System.Collections.Generic;
namespace Renci.SshClient
{
    public class SshClient
    {
        private Session _session;

        private ConnectionInfo _connectionInfo;

        private List<ForwardedPort> _forwardedPorts = new List<ForwardedPort>();

        private Sftp _sftp;
        /// <summary>
        /// Gets the shell.
        /// </summary>
        /// <value>The shell.</value>
        public Sftp Sftp
        {
            get
            {
                if (this._sftp == null)
                {
                    this._sftp = new Sftp(this._session);
                }
                return this._sftp;
            }
        }

        public IEnumerable<ForwardedPort> ForwardedPorts
        {
            get
            {
                return this._forwardedPorts.AsReadOnly();
            }
        }

        public SshClient(ConnectionInfo connectionInfo)
        {
            this._connectionInfo = connectionInfo;
            this._session = new Session(this._connectionInfo);
        }

        public SshClient(string host, int port, string username, string password)
            : this(new ConnectionInfo
            {
                Host = host,
                Port = port,
                Username = username,
                Password = password,
            })
        {
        }

        public SshClient(string host, string username, string password)
            : this(new ConnectionInfo
            {
                Host = host,
                Username = username,
                Password = password,
            })
        {
        }

        public SshClient(string host, int port, string username, PrivateKeyFile keyFile)
            : this(new ConnectionInfo
            {
                Host = host,
                Port = port,
                Username = username,
                KeyFile = keyFile,
            })
        {
        }

        public SshClient(string host, string username, PrivateKeyFile keyFile)
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
            this._session = new Session(this._connectionInfo);
            this._session.Connect();
        }

        public void Disconnect()
        {
            foreach (var port in this._forwardedPorts)
            {
                port.Stop();
            }
            this._session.Disconnect();

            //  Clean up objects created using previouse session instance
            this._sftp = null;
        }

        public T AddForwardedPort<T>(uint boundPort, string connectedHost, uint connectedPort) where T : ForwardedPort, new()
        {
            T port = new T();

            port.Session = this._session;
            port.BoundPort = boundPort;
            port.ConnectedHost = connectedHost;
            port.ConnectedPort = connectedPort;

            this._forwardedPorts.Add(port);

            return port;
        }

        public SshCommand CreateCommand(string commandText)
        {
            return new SshCommand(this._session, commandText);
        }

        public SshCommand RunCommand(string commandText)
        {
            var cmd = new SshCommand(this._session, commandText);
            cmd.Execute();
            return cmd;
        }

        public void RemoveForwardedPort(ForwardedPort port)
        {
            this._forwardedPorts.Remove(port);
        }
    }
}
