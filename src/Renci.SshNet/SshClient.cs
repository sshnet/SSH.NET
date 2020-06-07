using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides client connection to SSH server.
    /// </summary>
    public class SshClient : BaseClient
    {
        /// <summary>
        /// Holds the list of forwarded ports
        /// </summary>
        private readonly List<ForwardedPort> _forwardedPorts;

        /// <summary>
        /// Holds a value indicating whether the current instance is disposed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the current instance is disposed; otherwise, <c>false</c>.
        /// </value>
        private bool _isDisposed;

        private Stream _inputStream;

        /// <summary>
        /// Gets the list of forwarded ports.
        /// </summary>
        public IEnumerable<ForwardedPort> ForwardedPorts
        {
            get
            {
                return _forwardedPorts.AsReadOnly();
            }
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SshClient" /> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\PasswordConnectionInfoTest.cs" region="Example PasswordConnectionInfo" language="C#" title="Connect using PasswordConnectionInfo object" />
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\PasswordConnectionInfoTest.cs" region="Example PasswordConnectionInfo PasswordExpired" language="C#" title="Connect using PasswordConnectionInfo object with passwod change option" />
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\PrivateKeyConnectionInfoTest.cs" region="Example PrivateKeyConnectionInfo PrivateKeyFile" language="C#" title="Connect using PrivateKeyConnectionInfo" />
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient Connect Timeout" language="C#" title="Specify connection timeout when connecting" />
        /// </example>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <c>null</c>.</exception>
        public SshClient(ConnectionInfo connectionInfo)
            : this(connectionInfo, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is <c>null</c> or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="IPEndPoint.MinPort"/> and <see cref="IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "C2A000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public SshClient(string host, int port, string username, string password)
            : this(new PasswordConnectionInfo(host, port, username, password), true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient(host, username) Connect" language="C#" title="Connect using username and password" />
        /// </example>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is <c>null</c> or contains only whitespace characters.</exception>
        public SshClient(string host, string username, string password)
            : this(host, ConnectionInfo.DefaultPort, username, password)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient(host, username) Connect PrivateKeyFile" language="C#" title="Connect using username and private key" />
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient(host, username) Connect PrivateKeyFile PassPhrase" language="C#" title="Connect using username and private key and pass phrase" />
        /// </example>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is <c>null</c> or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="IPEndPoint.MinPort"/> and <see cref="IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public SshClient(string host, int port, string username, params PrivateKeyFile[] keyFiles)
            : this(new PrivateKeyConnectionInfo(host, port, username, keyFiles), true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient(host, username) Connect PrivateKeyFile" language="C#" title="Connect using private key" />
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshClientTest.cs" region="Example SshClient(host, username) Connect PrivateKeyFile PassPhrase" language="C#" title="Connect using private key and pass phrase" />
        /// </example>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is <c>null</c> or contains only whitespace characters.</exception>
        public SshClient(string host, string username, params PrivateKeyFile[] keyFiles)
            : this(host, ConnectionInfo.DefaultPort, username, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="ownsConnectionInfo">Specified whether this instance owns the connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <c>null</c>.</exception>
        /// <remarks>
        /// If <paramref name="ownsConnectionInfo"/> is <c>true</c>, then the
        /// connection info will be disposed when this instance is disposed.
        /// </remarks>
        private SshClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo)
            : this(connectionInfo, ownsConnectionInfo, new ServiceFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="ownsConnectionInfo">Specified whether this instance owns the connection info.</param>
        /// <param name="serviceFactory">The factory to use for creating new services.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="serviceFactory"/> is <c>null</c>.</exception>
        /// <remarks>
        /// If <paramref name="ownsConnectionInfo"/> is <c>true</c>, then the
        /// connection info will be disposed when this instance is disposed.
        /// </remarks>
        internal SshClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory)
            : base(connectionInfo, ownsConnectionInfo, serviceFactory)
        {
            _forwardedPorts = new List<ForwardedPort>();
        }

        #endregion

        /// <summary>
        /// Called when client is disconnecting from the server.
        /// </summary>
        protected override void OnDisconnecting()
        {
            base.OnDisconnecting();

            foreach (var port in _forwardedPorts)
            {
                port.Stop();
            }
        }

        /// <summary>
        /// Adds the forwarded port.
        /// </summary>
        /// <param name="port">The port.</param>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\ForwardedPortRemoteTest.cs" region="Example SshClient AddForwardedPort Start Stop ForwardedPortRemote" language="C#" title="Remote port forwarding" />
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\ForwardedPortLocalTest.cs" region="Example SshClient AddForwardedPort Start Stop ForwardedPortLocal" language="C#" title="Local port forwarding" />
        /// </example>
        /// <exception cref="InvalidOperationException">Forwarded port is already added to a different client.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="port"/> is <c>null</c>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public void AddForwardedPort(ForwardedPort port)
        {
            if (port == null)
                throw new ArgumentNullException("port");
            EnsureSessionIsOpen();

            AttachForwardedPort(port);
            _forwardedPorts.Add(port);
        }

        /// <summary>
        /// Stops and removes the forwarded port from the list.
        /// </summary>
        /// <param name="port">Forwarded port.</param>
        /// <exception cref="ArgumentNullException"><paramref name="port"/> is <c>null</c>.</exception>
        public void RemoveForwardedPort(ForwardedPort port)
        {
            if (port == null)
                throw new ArgumentNullException("port");

            //  Stop port forwarding before removing it
            port.Stop();

            DetachForwardedPort(port);
            _forwardedPorts.Remove(port);
        }

        private void AttachForwardedPort(ForwardedPort port)
        {
            if (port.Session != null && port.Session != Session)
                throw new InvalidOperationException("Forwarded port is already added to a different client.");

            port.Session = Session;
        }

        private static void DetachForwardedPort(ForwardedPort port)
        {
            port.Session = null;
        }

        /// <summary>
        /// Creates the command to be executed.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns><see cref="SshCommand"/> object.</returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public SshCommand CreateCommand(string commandText)
        {
            return CreateCommand(commandText, ConnectionInfo.Encoding);
        }

        /// <summary>
        /// Creates the command to be executed with specified encoding.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="encoding">The encoding to use for results.</param>
        /// <returns><see cref="SshCommand"/> object which uses specified encoding.</returns>
        /// <remarks>This method will change current default encoding.</remarks>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="commandText"/> or <paramref name="encoding"/> is <c>null</c>.</exception>
        public SshCommand CreateCommand(string commandText, Encoding encoding)
        {
            EnsureSessionIsOpen();

            ConnectionInfo.Encoding = encoding;
            return new SshCommand(Session, commandText, encoding);
        }

        /// <summary>
        /// Creates and executes the command.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns>Returns an instance of <see cref="SshCommand"/> with execution results.</returns>
        /// <remarks>This method internally uses asynchronous calls.</remarks>
        /// <example>
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshCommandTest.cs" region="Example SshCommand RunCommand Result" language="C#" title="Running simple command" />
        ///     <code source="..\..\src\Renci.SshNet.Tests\Classes\SshCommandTest.cs" region="Example SshCommand RunCommand Parallel" language="C#" title="Run many commands in parallel" />
        /// </example>
        /// <exception cref="ArgumentException">CommandText property is empty.</exception>
        /// <exception cref="T:Renci.SshNet.Common.SshException">Invalid Operation - An existing channel was used to execute this command.</exception>
        /// <exception cref="InvalidOperationException">Asynchronous operation is already in progress.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="commandText"/> is <c>null</c>.</exception>
        public SshCommand RunCommand(string commandText)
        {
            var cmd = CreateCommand(commandText);
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
        /// <param name="terminalModes">The terminal mode.</param>
        /// <param name="bufferSize">Size of the internal read buffer.</param>
        /// <returns>
        /// Returns a representation of a <see cref="Shell" /> object.
        /// </returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public Shell CreateShell(Stream input, Stream output, Stream extendedOutput, string terminalName, uint columns, uint rows, uint width, uint height, IDictionary<TerminalModes, uint> terminalModes, int bufferSize)
        {
            EnsureSessionIsOpen();

            return new Shell(Session, input, output, extendedOutput, terminalName, columns, rows, width, height, terminalModes, bufferSize);
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
        /// <param name="terminalModes">The terminal mode.</param>
        /// <returns>
        /// Returns a representation of a <see cref="Shell" /> object.
        /// </returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public Shell CreateShell(Stream input, Stream output, Stream extendedOutput, string terminalName, uint columns, uint rows, uint width, uint height, IDictionary<TerminalModes, uint> terminalModes)
        {
            return CreateShell(input, output, extendedOutput, terminalName, columns, rows, width, height, terminalModes, 1024);
        }

        /// <summary>
        /// Creates the shell.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="extendedOutput">The extended output.</param>
        /// <returns>
        /// Returns a representation of a <see cref="Shell" /> object.
        /// </returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public Shell CreateShell(Stream input, Stream output, Stream extendedOutput)
        {
            return CreateShell(input, output, extendedOutput, string.Empty, 0, 0, 0, 0, null, 1024);
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
        /// <param name="terminalModes">The terminal mode.</param>
        /// <param name="bufferSize">Size of the internal read buffer.</param>
        /// <returns>
        /// Returns a representation of a <see cref="Shell" /> object.
        /// </returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public Shell CreateShell(Encoding encoding, string input, Stream output, Stream extendedOutput, string terminalName, uint columns, uint rows, uint width, uint height, IDictionary<TerminalModes, uint> terminalModes, int bufferSize)
        {
            // TODO let shell dispose of input stream when we own the stream!

            _inputStream = new MemoryStream();
            var writer = new StreamWriter(_inputStream, encoding);
            writer.Write(input);
            writer.Flush();
            _inputStream.Seek(0, SeekOrigin.Begin);

            return CreateShell(_inputStream, output, extendedOutput, terminalName, columns, rows, width, height, terminalModes, bufferSize);
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
        /// <param name="terminalModes">The terminal modes.</param>
        /// <returns>
        /// Returns a representation of a <see cref="Shell" /> object.
        /// </returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public Shell CreateShell(Encoding encoding, string input, Stream output, Stream extendedOutput, string terminalName, uint columns, uint rows, uint width, uint height, IDictionary<TerminalModes, uint> terminalModes)
        {
            return CreateShell(encoding, input, output, extendedOutput, terminalName, columns, rows, width, height, terminalModes, 1024);
        }

        /// <summary>
        /// Creates the shell.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="extendedOutput">The extended output.</param>
        /// <returns>
        /// Returns a representation of a <see cref="Shell" /> object.
        /// </returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public Shell CreateShell(Encoding encoding, string input, Stream output, Stream extendedOutput)
        {
            return CreateShell(encoding, input, output, extendedOutput, string.Empty, 0, 0, 0, 0, null, 1024);
        }

        /// <summary>
        /// Creates the shell stream.
        /// </summary>
        /// <param name="terminalName">The <c>TERM</c> environment variable.</param>
        /// <param name="columns">The terminal width in columns.</param>
        /// <param name="rows">The terminal width in rows.</param>
        /// <param name="width">The terminal height in pixels.</param>
        /// <param name="height">The terminal height in pixels.</param>
        /// <param name="bufferSize">The size of the buffer.</param>
        /// <returns>
        /// The created <see cref="ShellStream"/> instance.
        /// </returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <remarks>
        /// <para>
        /// The <c>TERM</c> environment variable contains an identifier for the text window's capabilities.
        /// You can get a detailed list of these cababilities by using the ‘infocmp’ command.
        /// </para>
        /// <para>
        /// The column/row dimensions override the pixel dimensions(when nonzero). Pixel dimensions refer
        /// to the drawable area of the window.
        /// </para>
        /// </remarks>
        public ShellStream CreateShellStream(string terminalName, uint columns, uint rows, uint width, uint height, int bufferSize)
        {
            return CreateShellStream(terminalName, columns, rows, width, height, bufferSize, null);
        }

        /// <summary>
        /// Creates the shell stream.
        /// </summary>
        /// <param name="terminalName">The <c>TERM</c> environment variable.</param>
        /// <param name="columns">The terminal width in columns.</param>
        /// <param name="rows">The terminal width in rows.</param>
        /// <param name="width">The terminal height in pixels.</param>
        /// <param name="height">The terminal height in pixels.</param>
        /// <param name="bufferSize">The size of the buffer.</param>
        /// <param name="terminalModeValues">The terminal mode values.</param>
        /// <returns>
        /// The created <see cref="ShellStream"/> instance.
        /// </returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <remarks>
        /// <para>
        /// The <c>TERM</c> environment variable contains an identifier for the text window's capabilities.
        /// You can get a detailed list of these cababilities by using the ‘infocmp’ command.
        /// </para>
        /// <para>
        /// The column/row dimensions override the pixel dimensions(when non-zero). Pixel dimensions refer
        /// to the drawable area of the window.
        /// </para>
        /// </remarks>
        public ShellStream CreateShellStream(string terminalName, uint columns, uint rows, uint width, uint height, int bufferSize, IDictionary<TerminalModes, uint> terminalModeValues)
        {
            EnsureSessionIsOpen();

            return ServiceFactory.CreateShellStream(Session, terminalName, columns, rows, width, height, terminalModeValues, bufferSize);
        }

        /// <summary>
        /// Stops forwarded ports.
        /// </summary>
        protected override void OnDisconnected()
        {
            base.OnDisconnected();

            for (var i = _forwardedPorts.Count - 1; i >= 0; i--)
            {
                var port = _forwardedPorts[i];
                DetachForwardedPort(port);
                _forwardedPorts.RemoveAt(i);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_isDisposed)
                return;

            if (disposing)
            {
                if (_inputStream != null)
                {
                    _inputStream.Dispose();
                    _inputStream = null;
                }

                _isDisposed = true;
            }
        }

        private void EnsureSessionIsOpen()
        {
            if (Session == null)
                throw new SshConnectionException("Client not connected.");
        }
    }
}
