using System;
using System.Collections.Generic;
using System.IO;

namespace Renci.SshClient
{
    public class SshClient : IDisposable
    {
        private Session _session;

        private List<ForwardedPort> _forwardedPorts = new List<ForwardedPort>();

        public ConnectionInfo ConnectionInfo { get; private set; }

        public bool IsConnected
        {
            get
            {
                if (this._session == null)
                    return false;
                else
                    return this._session.IsConnected;
            }
        }

        //private Sftp _sftp;
        ///// <summary>
        ///// Gets the shell.
        ///// </summary>
        ///// <value>The shell.</value>
        //public Sftp Sftp
        //{
        //    get
        //    {
        //        if (this._sftp == null)
        //        {
        //            this._sftp = new Sftp(this._session);
        //        }
        //        return this._sftp;
        //    }
        //}

        public IEnumerable<ForwardedPort> ForwardedPorts
        {
            get
            {
                return this._forwardedPorts.AsReadOnly();
            }
        }

        public SshClient(ConnectionInfo connectionInfo)
        {
            this.ConnectionInfo = connectionInfo;
            this._session = new Session(connectionInfo);
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
            this._session = new Session(this.ConnectionInfo);
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
            //this._sftp = null;
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
            var cmd = this.CreateCommand(commandText);
            cmd.Execute();
            return cmd;
        }

        public Shell CreateShell(Stream input, TextWriter output, TextWriter extendedOutput, string terminalName, uint columns, uint rows, uint width, uint height, string terminalMode)
        {
            return new Shell(this._session, input, output, extendedOutput, terminalName, columns, rows, width, height, terminalMode);
        }

        //public Sftp CreateSftp()
        //{
        //    return new Sftp(this._session);
        //}

        public void RemoveForwardedPort(ForwardedPort port)
        {
            this._forwardedPorts.Remove(port);
        }

        #region IDisposable Members

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this._session != null)
                    {
                        this._session.Dispose();
                    }
                }

                // Note disposing has been done.
                disposed = true;
            }
        }

        ~SshClient()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion
    }
}
