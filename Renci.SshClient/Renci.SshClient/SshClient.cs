using System;
using System.Collections.Generic;
using System.IO;
using System.Collections.ObjectModel;

namespace Renci.SshClient
{
    public class SshClient : BaseClient
    {
        /// <summary>
        /// Holds the list of forwarded ports
        /// </summary>
        private List<ForwardedPort> _forwardedPorts = new List<ForwardedPort>();

        /// <summary>
        /// Gets the list of forwarded ports.
        /// </summary>
        public IEnumerable<ForwardedPort> ForwardedPorts
        {
            get
            {
                return this._forwardedPorts.AsReadOnly();
            }
        }

        #region Constructors

        public SshClient(ConnectionInfo connectionInfo)
            : base(connectionInfo)
        {
        }

        public SshClient(string host, int port, string username, string password)
            : this(new PasswordConnectionInfo(host, port, username, password))
        {
        }

        public SshClient(string host, string username, string password)
            : this(host, 22, username, password)
        {
        }

        public SshClient(string host, int port, string username, params PrivateKeyFile[] keyFiles)
            : this(new PrivateKeyConnectionInfo(host, port, username, keyFiles))
        {
        }

        public SshClient(string host, string username, params PrivateKeyFile[] keyFiles)
            : this(host, 22, username, keyFiles)
        {
        }

        #endregion

        /// <summary>
        /// Called when client is disconnecting from the server.
        /// </summary>
        protected override void OnDisconnecting()
        {
            base.OnDisconnecting();

            foreach (var port in this._forwardedPorts)
            {
                port.Stop();
            }
        }

        /// <summary>
        /// Adds forwarded port to the list.
        /// </summary>
        /// <typeparam name="T">Type of forwarded port to add</typeparam>
        /// <param name="boundPort">The bound port.</param>
        /// <param name="connectedHost">The connected host.</param>
        /// <param name="connectedPort">The connected port.</param>
        /// <returns>Forwarded port</returns>
        public T AddForwardedPort<T>(uint boundPort, string connectedHost, uint connectedPort) where T : ForwardedPort, new()
        {
            T port = new T();

            port.Session = this.Session;
            port.BoundPort = boundPort;
            port.ConnectedHost = connectedHost;
            port.ConnectedPort = connectedPort;

            this._forwardedPorts.Add(port);

            return port;
        }

        /// <summary>
        /// Stops and removes the forwarded port from the list.
        /// </summary>
        /// <param name="port">Forwarded port.</param>
        public void RemoveForwardedPort(ForwardedPort port)
        {
            //  Stop port forwarding before removing it
            port.Stop();

            this._forwardedPorts.Remove(port);
        }

        /// <summary>
        /// Creates the command to be executed.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns></returns>
        public SshCommand CreateCommand(string commandText)
        {
            return new SshCommand(this.Session, commandText);
        }

        /// <summary>
        /// Creates and executes the command.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns></returns>
        public SshCommand RunCommand(string commandText)
        {
            var cmd = this.CreateCommand(commandText);
            cmd.Execute();
            return cmd;
        }

        /// <summary>
        /// Creates the shell. (not complete)
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="extendedOutput">The extended output.</param>
        /// <param name="terminalName">Name of the terminal.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="terminalMode">The terminal mode.</param>
        /// <returns></returns>
        public Shell CreateShell(Stream input, TextWriter output, TextWriter extendedOutput, string terminalName, uint columns, uint rows, uint width, uint height, string terminalMode)
        {
            return new Shell(this.Session, input, output, extendedOutput, terminalName, columns, rows, width, height, terminalMode);
        }
    }
}
