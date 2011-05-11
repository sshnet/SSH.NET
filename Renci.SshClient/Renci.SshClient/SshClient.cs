using System;
using System.Collections.Generic;
using System.IO;
using System.Collections.ObjectModel;
using System.Text;

namespace Renci.SshClient
{
    /// <summary>
    /// Provides client connection to SSH server.
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="SshClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        public SshClient(ConnectionInfo connectionInfo)
            : base(connectionInfo)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        public SshClient(string host, int port, string username, string password)
            : this(new PasswordConnectionInfo(host, port, username, password))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        public SshClient(string host, string username, string password)
            : this(host, 22, username, password)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        public SshClient(string host, int port, string username, params PrivateKeyFile[] keyFiles)
            : this(new PrivateKeyConnectionInfo(host, port, username, keyFiles))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
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
        /// <param name="boundHost">The bound host.</param>
        /// <param name="boundPort">The bound port.</param>
        /// <param name="connectedHost">The connected host.</param>
        /// <param name="connectedPort">The connected port.</param>
        /// <returns>
        /// Forwarded port
        /// </returns>
        public T AddForwardedPort<T>(string boundHost, uint boundPort, string connectedHost, uint connectedPort) where T : ForwardedPort, new()
        {
            //  Ensure that connection is established.
            this.EnsureConnection();

            T port = new T();

            port.Session = this.Session;
            port.BoundHost = boundHost;
            port.BoundPort = boundPort;
            port.Host = connectedHost;
            port.Port = connectedPort;

            this._forwardedPorts.Add(port);

            return port;
        }

        /// <summary>
        /// Adds forwarded port to the list bound to "localhost".
        /// </summary>
        /// <typeparam name="T">Type of forwarded port to add</typeparam>
        /// <param name="boundPort">The bound port.</param>
        /// <param name="connectedHost">The connected host.</param>
        /// <param name="connectedPort">The connected port.</param>
        /// <returns></returns>
        public T AddForwardedPort<T>(uint boundPort, string connectedHost, uint connectedPort) where T : ForwardedPort, new()
        {
            return this.AddForwardedPort<T>("localhost", boundPort, connectedHost, connectedPort);
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
        /// <returns><see cref="SshCommand"/> object.</returns>
        public SshCommand CreateCommand(string commandText)
        {
            return this.CreateCommand(commandText, Encoding.ASCII);
        }

        /// <summary>
        /// Creates the command to be executed with specified encoding.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="encoding">The encoding to use for results.</param>
        /// <returns><see cref="SshCommand"/> object which uses specified encoding.</returns>
        public SshCommand CreateCommand(string commandText, Encoding encoding)
        {
            //  Ensure that connection is established.
            this.EnsureConnection();

            return new SshCommand(this.Session, commandText, encoding);
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
        /// Creates the shell.
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
        /// <param name="bufferSize">Size of the internal read buffer.</param>
        /// <returns></returns>
        public Shell CreateShell(Stream input, Stream output, Stream extendedOutput, string terminalName, uint columns, uint rows, uint width, uint height, string terminalMode, int bufferSize)
        {
            //  Ensure that connection is established.
            this.EnsureConnection();

            return new Shell(this.Session, input, output, extendedOutput, terminalName, columns, rows, width, height, terminalMode, bufferSize);
        }

        /// <summary>
        /// Creates the shell.
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
        public Shell CreateShell(Stream input, Stream output, Stream extendedOutput, string terminalName, uint columns, uint rows, uint width, uint height, string terminalMode)
        {
            return this.CreateShell(input, output, extendedOutput, terminalName, columns, rows, width, height, terminalMode, 1024);
        }

        /// <summary>
        /// Creates the shell.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="extendedOutput">The extended output.</param>
        /// <returns></returns>
        public Shell CreateShell(Stream input, Stream output, Stream extendedOutput)
        {
            return this.CreateShell(input, output, extendedOutput, string.Empty, 0, 0, 0, 0, string.Empty, 1024);
        }

        /// <summary>
        /// Creates the shell.
        /// </summary>
        /// <param name="encoding">The encoding to use to send the input.</param>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="extendedOutput">The extended output.</param>
        /// <param name="terminalName">Name of the terminal.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="terminalMode">The terminal mode.</param>
        /// <param name="bufferSize">Size of the internal read buffer.</param>
        /// <returns></returns>
        public Shell CreateShell(Encoding encoding, string input, Stream output, Stream extendedOutput, string terminalName, uint columns, uint rows, uint width , uint height , string terminalMode, int bufferSize)
        {
            //  Ensure that connection is established.
            this.EnsureConnection();

            var inputStream = new MemoryStream();
            new StreamWriter(inputStream, encoding).Write(input);
            inputStream.Seek(0, SeekOrigin.Begin);

            return this.CreateShell(inputStream, output, extendedOutput, terminalName, columns, rows, width, height, terminalMode, bufferSize);
        }

        /// <summary>
        /// Creates the shell.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
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
        public Shell CreateShell(Encoding encoding, string input, Stream output, Stream extendedOutput, string terminalName, uint columns, uint rows, uint width, uint height, string terminalMode)
        {
            return this.CreateShell(encoding, input, output, extendedOutput, terminalName, columns, rows, width, height, terminalMode, 1024);
        }

        /// <summary>
        /// Creates the shell.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="extendedOutput">The extended output.</param>
        /// <returns></returns>
        public Shell CreateShell(Encoding encoding, string input, Stream output, Stream extendedOutput)
        {
            return this.CreateShell(encoding, input, output, extendedOutput, string.Empty, 0, 0, 0, 0, string.Empty, 1024);
        }

    }
}
