#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides SCP client functionality.
    /// </summary>
    /// <remarks>
    /// <para>
    /// More information on the SCP protocol is available here: https://github.com/net-ssh/net-scp/blob/master/lib/net/scp.rb.
    /// </para>
    /// <para>
    /// Known issues in OpenSSH:
    /// <list type="bullet">
    ///   <item>
    ///     <description>Recursive download (-prf) does not deal well with specific UTF-8 and newline characters.</description>
    ///     <description>Recursive update does not support empty path for uploading to home directory.</description>
    ///   </item>
    /// </list>
    /// </para>
    /// <para>
    /// <note type="caution">
    /// SCP performs a transfer by running <c>scp</c> on the server with the remote path embedded in the
    /// command. How that path must be encoded depends on the kind of server:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///     On a shell-based server the command is interpreted by a shell, so the path must be quoted or
    ///     escaped according to that shell's rules. An unsuitable transformation can allow a crafted path
    ///     to be executed as a command on the server.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     On a non-shell-based server the path is used literally and must not be quoted or escaped
    ///     (see <see cref="RemotePathTransformation.None"/>); otherwise the quoting or escape
    ///     characters end up as part of the file or directory path.
    ///     </description>
    ///   </item>
    /// </list>
    /// Choose the <see cref="RemotePathTransformation"/> supplied to the constructor to suit the remote
    /// server and the trust you place in the paths you pass. Prefer <see cref="SftpClient"/>, which does
    /// not involve a remote shell, where possible.
    /// </note>
    /// </para>
    /// </remarks>
#pragma warning disable MA0204 // Remove unnecessary partial modifier; not true for all targets
    public partial class ScpClient : BaseClient
    {
        private const string ConstructorObsoleteMessage =
           @"SCP with insufficiently-escaped paths can allow remote command injection. Use a constructor " +
            "taking an IRemotePathTransformation which suits the escaping rules of the remote server and " +
            "the trust environment in which this code runs, and consider using SFTP where possible.";

        private const string FileInfoPattern = @"C(?<mode>\d{4}) (?<length>\d+) (?<filename>.+)";
        private const string DirectoryInfoPattern = @"D(?<mode>\d{4}) (?<length>\d+) (?<filename>.+)";
        private const string TimestampPattern = @"T(?<mtime>\d+) 0 (?<atime>\d+) 0";

#if NET
        private static readonly Regex FileInfoRegex = GetFileInfoRegex();
        private static readonly Regex DirectoryInfoRegex = GetDirectoryInfoRegex();
        private static readonly Regex TimestampRegex = GetTimestampRegex();

        [GeneratedRegex(FileInfoPattern)]
        private static partial Regex GetFileInfoRegex();

        [GeneratedRegex(DirectoryInfoPattern)]
        private static partial Regex GetDirectoryInfoRegex();

        [GeneratedRegex(TimestampPattern)]
        private static partial Regex GetTimestampRegex();
#else
        private static readonly Regex FileInfoRegex = new Regex(FileInfoPattern, RegexOptions.Compiled);
        private static readonly Regex DirectoryInfoRegex = new Regex(DirectoryInfoPattern, RegexOptions.Compiled);
        private static readonly Regex TimestampRegex = new Regex(TimestampPattern, RegexOptions.Compiled);
#endif

        private static readonly byte[] SuccessConfirmationCode = { 0 };
        private static readonly byte[] ErrorConfirmationCode = { 1 };

        private static readonly char[] InvalidLocalNameChars = Path.GetInvalidFileNameChars();

        private IRemotePathTransformation _remotePathTransformation;
        private TimeSpan _operationTimeout;

        /// <summary>
        /// Gets or sets the operation timeout.
        /// </summary>
        /// <value>
        /// The timeout to wait until an operation completes. The default value is negative
        /// one (-1) milliseconds, which indicates an infinite time-out period.
        /// </value>
        public TimeSpan OperationTimeout
        {
            get
            {
                return _operationTimeout;
            }
            set
            {
                value.EnsureValidTimeout(nameof(OperationTimeout));

                _operationTimeout = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the buffer.
        /// </summary>
        /// <value>
        /// The size of the buffer. The default buffer size is 16384 bytes.
        /// </value>
        public uint BufferSize { get; set; }

        /// <summary>
        /// Gets or sets the transformation to apply to remote paths.
        /// </summary>
        /// <value>
        /// The transformation to apply to remote paths. This is initialized from the transformation
        /// passed to the constructor; the obsolete constructors that do not take one use
        /// <see cref="RemotePathTransformation.DoubleQuote"/>.
        /// </value>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// <para>
        /// This transformation is applied to the remote file or directory path that is passed to the
        /// <c>scp</c> command.
        /// </para>
        /// <para>
        /// See <see cref="SshNet.RemotePathTransformation"/> for the transformations that are supplied
        /// out-of-the-box with SSH.NET.
        /// </para>
        /// </remarks>
        public IRemotePathTransformation RemotePathTransformation
        {
            get
            {
                return _remotePathTransformation;
            }
            set
            {
                ArgumentNullException.ThrowIfNull(value);

                _remotePathTransformation = value;
            }
        }

        private static IRemotePathTransformation DefaultTransform
        {
            get
            {
                return SshNet.RemotePathTransformation.DoubleQuote;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the "-d" flag should be passed to the scp process on the server
        /// when uploading files. Defaults to <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// The "-d" flag is an undocumented flag that ensures that the target is actually a directory. However,
        /// some scp implementations (like Cisco) do not support this flag and will fail.
        /// You can set this to <see langword="false"/> to work around this.
        /// </remarks>
        public bool UseDirectoryFlag { get; set; } = true;

        private string EnsureIsDirectoryArg
        {
            get
            {
                return UseDirectoryFlag ? "-d" : string.Empty;
            }
        }

        /// <summary>
        /// Occurs when downloading file.
        /// </summary>
        public event EventHandler<ScpDownloadEventArgs>? Downloading;

        /// <summary>
        /// Occurs when uploading file.
        /// </summary>
        public event EventHandler<ScpUploadEventArgs>? Uploading;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="remotePathTransformation">The transformation to apply to remote paths.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <see langword="null"/>.</exception>
        public ScpClient(ConnectionInfo connectionInfo, IRemotePathTransformation remotePathTransformation)
            : this(connectionInfo, ownsConnectionInfo: false, remotePathTransformation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <param name="remotePathTransformation">The transformation to apply to remote paths.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is <see langword="null"/> or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="IPEndPoint.MinPort"/> and <see cref="IPEndPoint.MaxPort"/>.</exception>
        public ScpClient(string host, int port, string username, string password, IRemotePathTransformation remotePathTransformation)
            : this(new PasswordConnectionInfo(host, port, username, password), ownsConnectionInfo: true, remotePathTransformation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <param name="remotePathTransformation">The transformation to apply to remote paths.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is <see langword="null"/> or contains only whitespace characters.</exception>
        public ScpClient(string host, string username, string password, IRemotePathTransformation remotePathTransformation)
            : this(host, ConnectionInfo.DefaultPort, username, password, remotePathTransformation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="remotePathTransformation">The transformation to apply to remote paths.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is <see langword="null"/> or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="IPEndPoint.MinPort"/> and <see cref="IPEndPoint.MaxPort"/>.</exception>
        public ScpClient(string host, int port, string username, IRemotePathTransformation remotePathTransformation, params IPrivateKeySource[] keyFiles)
            : this(new PrivateKeyConnectionInfo(host, port, username, keyFiles), ownsConnectionInfo: true, remotePathTransformation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="remotePathTransformation">The transformation to apply to remote paths.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is <see langword="null"/> or contains only whitespace characters.</exception>
        public ScpClient(string host, string username, IRemotePathTransformation remotePathTransformation, params IPrivateKeySource[] keyFiles)
            : this(host, ConnectionInfo.DefaultPort, username, remotePathTransformation, keyFiles)
        {
        }

        /// <inheritdoc cref="ScpClient(ConnectionInfo, IRemotePathTransformation)"/>
        [Obsolete(ConstructorObsoleteMessage)]
        public ScpClient(ConnectionInfo connectionInfo)
            : this(connectionInfo, ownsConnectionInfo: false, DefaultTransform)
        {
        }

        /// <inheritdoc cref="ScpClient(string, int, string, string, IRemotePathTransformation)"/>
        [Obsolete(ConstructorObsoleteMessage)]
        public ScpClient(string host, int port, string username, string password)
            : this(new PasswordConnectionInfo(host, port, username, password), ownsConnectionInfo: true, DefaultTransform)
        {
        }

        /// <inheritdoc cref="ScpClient(string, string, string, IRemotePathTransformation)"/>
        [Obsolete(ConstructorObsoleteMessage)]
        public ScpClient(string host, string username, string password)
            : this(host, ConnectionInfo.DefaultPort, username, password, DefaultTransform)
        {
        }

        /// <inheritdoc cref="ScpClient(string, int, string, IRemotePathTransformation, IPrivateKeySource[])"/>
        [Obsolete(ConstructorObsoleteMessage)]
        public ScpClient(string host, int port, string username, params IPrivateKeySource[] keyFiles)
            : this(new PrivateKeyConnectionInfo(host, port, username, keyFiles), ownsConnectionInfo: true, DefaultTransform)
        {
        }

        /// <inheritdoc cref="ScpClient(string, string, IRemotePathTransformation, IPrivateKeySource[])"/>
        [Obsolete(ConstructorObsoleteMessage)]
        public ScpClient(string host, string username, params IPrivateKeySource[] keyFiles)
            : this(host, ConnectionInfo.DefaultPort, username, DefaultTransform, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="ownsConnectionInfo">Specified whether this instance owns the connection info.</param>
        /// <param name="remotePathTransformation">The transformation to apply to remote paths.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// If <paramref name="ownsConnectionInfo"/> is <see langword="true"/>, then the
        /// connection info will be disposed when this instance is disposed.
        /// </remarks>
        private ScpClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IRemotePathTransformation remotePathTransformation)
            : this(connectionInfo, ownsConnectionInfo, new ServiceFactory(), remotePathTransformation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="ownsConnectionInfo">Specified whether this instance owns the connection info.</param>
        /// <param name="serviceFactory">The factory to use for creating new services.</param>
        /// <param name="remotePathTransformation">The transformation to apply to remote paths.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="serviceFactory"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// If <paramref name="ownsConnectionInfo"/> is <see langword="true"/>, then the
        /// connection info will be disposed when this instance is disposed.
        /// </remarks>
        internal ScpClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory, IRemotePathTransformation remotePathTransformation)
            : base(connectionInfo, ownsConnectionInfo, serviceFactory)
        {
            OperationTimeout = Timeout.InfiniteTimeSpan;
            BufferSize = 1024 * 16;
            _remotePathTransformation = remotePathTransformation;
        }

        /// <summary>
        /// Uploads the specified stream to the remote host.
        /// </summary>
        /// <param name="source">The <see cref="Stream"/> to upload.</param>
        /// <param name="path">A relative or absolute path for the remote file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is a zero-length <see cref="string"/>.</exception>
        /// <exception cref="ScpException">A directory with the specified path exists on the remote host.</exception>
        /// <exception cref="SshException">The secure copy execution request was rejected by the server.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public void Upload(Stream source, string path)
        {
            if (Session is null)
            {
                throw new SshConnectionException("Client not connected.");
            }

            var posixPath = PosixPath.CreateAbsoluteOrRelativeFilePath(path);

            using (var input = ServiceFactory.CreatePipeStream())
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += (sender, e) => input.Write(e.Data.Array!, e.Data.Offset, e.Data.Count);
                channel.Closed += (sender, e) => input.Dispose();
                channel.Open();

                // Pass only the directory part of the path to the server, and optionally use the (hidden) -d option to signal
                // that we expect the target to be a directory.
                if (!channel.SendExecRequest($"scp -t {EnsureIsDirectoryArg} {_remotePathTransformation.Transform(posixPath.Directory)}"))
                {
                    throw SecureExecutionRequestRejectedException();
                }

                CheckReturnCode(input);

                UploadFileModeAndName(channel, input, source.Length, posixPath.File);
                UploadFileContent(channel, input, source, posixPath.File);
            }
        }

        /// <summary>
        /// Uploads the specified file to the remote host.
        /// </summary>
        /// <param name="fileInfo">The file system info.</param>
        /// <param name="path">A relative or absolute path for the remote file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo" /> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is a zero-length <see cref="string"/>.</exception>
        /// <exception cref="ScpException">A directory with the specified path exists on the remote host.</exception>
        /// <exception cref="SshException">The secure copy execution request was rejected by the server.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public void Upload(FileInfo fileInfo, string path)
        {
            ArgumentNullException.ThrowIfNull(fileInfo);

            if (Session is null)
            {
                throw new SshConnectionException("Client not connected.");
            }

            var posixPath = PosixPath.CreateAbsoluteOrRelativeFilePath(path);

            using (var input = ServiceFactory.CreatePipeStream())
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += (sender, e) => input.Write(e.Data.Array!, e.Data.Offset, e.Data.Count);
                channel.Closed += (sender, e) => input.Dispose();
                channel.Open();

                // Pass only the directory part of the path to the server, and optionally use the (hidden) -d option to signal
                // that we expect the target to be a directory.
                if (!channel.SendExecRequest($"scp -t {EnsureIsDirectoryArg} {_remotePathTransformation.Transform(posixPath.Directory)}"))
                {
                    throw SecureExecutionRequestRejectedException();
                }

                CheckReturnCode(input);

                using (var source = fileInfo.OpenRead())
                {
                    UploadTimes(channel, input, fileInfo);
                    UploadFileModeAndName(channel, input, source.Length, posixPath.File);
                    UploadFileContent(channel, input, source, fileInfo.Name);
                }
            }
        }

        /// <summary>
        /// Uploads the specified directory to the remote host.
        /// </summary>
        /// <param name="directoryInfo">The directory info.</param>
        /// <param name="path">A relative or absolute path for the remote directory.</param>
        /// <exception cref="ArgumentNullException"><paramref name="directoryInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is a zero-length string.</exception>
        /// <exception cref="ScpException"><paramref name="path"/> does not exist on the remote host, is not a directory or the user does not have the required permission.</exception>
        /// <exception cref="SshException">The secure copy execution request was rejected by the server.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public void Upload(DirectoryInfo directoryInfo, string path)
        {
            ArgumentNullException.ThrowIfNull(directoryInfo);
            ArgumentException.ThrowIfNullOrEmpty(path);

            if (Session is null)
            {
                throw new SshConnectionException("Client not connected.");
            }

            using (var input = ServiceFactory.CreatePipeStream())
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += (sender, e) => input.Write(e.Data.Array!, e.Data.Offset, e.Data.Count);
                channel.Closed += (sender, e) => input.Dispose();
                channel.Open();

                // start copy with the following options:
                // -p preserve modification and access times
                // -r copy directories recursively
                // -d expect path to be a directory
                // -t copy to remote
                if (!channel.SendExecRequest($"scp -r -p {EnsureIsDirectoryArg} -t {_remotePathTransformation.Transform(path)}"))
                {
                    throw SecureExecutionRequestRejectedException();
                }

                CheckReturnCode(input);

                UploadDirectoryContent(channel, input, directoryInfo);
            }
        }

        /// <summary>
        /// Downloads the specified file from the remote host to local file.
        /// </summary>
        /// <param name="filename">Remote host file name.</param>
        /// <param name="fileInfo">Local file information.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="filename"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="ScpException"><paramref name="filename"/> exists on the remote host, and is not a regular file.</exception>
        /// <exception cref="SshException">The secure copy execution request was rejected by the server.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public void Download(string filename, FileInfo fileInfo)
        {
            ArgumentException.ThrowIfNullOrEmpty(filename);
            ArgumentNullException.ThrowIfNull(fileInfo);

            if (Session is null)
            {
                throw new SshConnectionException("Client not connected.");
            }

            using (var input = ServiceFactory.CreatePipeStream())
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += (sender, e) => input.Write(e.Data.Array!, e.Data.Offset, e.Data.Count);
                channel.Closed += (sender, e) => input.Dispose();
                channel.Open();

                // Send channel command request
                if (!channel.SendExecRequest($"scp -pf {_remotePathTransformation.Transform(filename)}"))
                {
                    throw SecureExecutionRequestRejectedException();
                }

                // Send reply
                SendSuccessConfirmation(channel);

                InternalDownload(channel, input, fileInfo);
            }
        }

        /// <summary>
        /// Downloads the specified directory from the remote host to local directory.
        /// </summary>
        /// <param name="directoryName">Remote host directory name.</param>
        /// <param name="directoryInfo">Local directory information.</param>
        /// <exception cref="ArgumentException"><paramref name="directoryName"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="directoryInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ScpException">File or directory with the specified path does not exist on the remote host.</exception>
        /// <exception cref="SshException">The secure copy execution request was rejected by the server.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public void Download(string directoryName, DirectoryInfo directoryInfo)
        {
            ArgumentException.ThrowIfNullOrEmpty(directoryName);
            ArgumentNullException.ThrowIfNull(directoryInfo);

            if (Session is null)
            {
                throw new SshConnectionException("Client not connected.");
            }

            using (var input = ServiceFactory.CreatePipeStream())
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += (sender, e) => input.Write(e.Data.Array!, e.Data.Offset, e.Data.Count);
                channel.Closed += (sender, e) => input.Dispose();
                channel.Open();

                // Send channel command request
                if (!channel.SendExecRequest($"scp -prf {_remotePathTransformation.Transform(directoryName)}"))
                {
                    throw SecureExecutionRequestRejectedException();
                }

                // Send reply
                SendSuccessConfirmation(channel);

                InternalDownload(channel, input, directoryInfo);
            }
        }

        /// <summary>
        /// Downloads the specified file from the remote host to the stream.
        /// </summary>
        /// <param name="filename">A relative or absolute path for the remote file.</param>
        /// <param name="destination">The <see cref="Stream"/> to download the remote file to.</param>
        /// <exception cref="ArgumentException"><paramref name="filename"/> is <see langword="null"/> or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="destination"/> is <see langword="null"/>.</exception>
        /// <exception cref="ScpException"><paramref name="filename"/> exists on the remote host, and is not a regular file.</exception>
        /// <exception cref="SshException">The secure copy execution request was rejected by the server.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        public void Download(string filename, Stream destination)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(filename);
            ArgumentNullException.ThrowIfNull(destination);

            if (Session is null)
            {
                throw new SshConnectionException("Client not connected.");
            }

            using (var input = ServiceFactory.CreatePipeStream())
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += (sender, e) => input.Write(e.Data.Array!, e.Data.Offset, e.Data.Count);
                channel.Closed += (sender, e) => input.Dispose();
                channel.Open();

                // Send channel command request
                if (!channel.SendExecRequest(string.Concat("scp -f ", _remotePathTransformation.Transform(filename))))
                {
                    throw SecureExecutionRequestRejectedException();
                }

                SendSuccessConfirmation(channel); // Send reply

                var message = ReadString(input);
                var match = FileInfoRegex.Match(message);

                if (match.Success)
                {
                    // Read file
                    SendSuccessConfirmation(channel); //  Send reply

                    var length = long.Parse(match.Result("${length}"), CultureInfo.InvariantCulture);
                    var fileName = match.Result("${filename}");

                    InternalDownload(channel, input, destination, fileName, length);
                }
                else
                {
                    SendErrorConfirmation(channel, string.Format("\"{0}\" is not valid protocol message.", message));
                }
            }
        }

        private static void SendData(IChannel channel, byte[] buffer, int length)
        {
            channel.SendData(buffer, 0, length);
        }

        private static void SendData(IChannel channel, byte[] buffer)
        {
            channel.SendData(buffer);
        }

        private static int ReadByte(Stream stream)
        {
            var b = stream.ReadByte();

            if (b == -1)
            {
                throw new SshException("Stream has been closed.");
            }

            return b;
        }

        private static SshException SecureExecutionRequestRejectedException()
        {
            throw new SshException("Secure copy execution request was rejected by the server. Please consult the server logs.");
        }

        /// <summary>
        /// Sets mode, size and name of file being upload.
        /// </summary>
        /// <param name="channel">The channel to perform the upload in.</param>
        /// <param name="input">A <see cref="Stream"/> from which any feedback from the server can be read.</param>
        /// <param name="fileSize">The size of the content to upload.</param>
        /// <param name="serverFileName">The name of the file, without path, to which the content is to be uploaded.</param>
        /// <remarks>
        /// <para>
        /// When the SCP transfer is already initiated for a file, a zero-length <see cref="string"/> should
        /// be specified for <paramref name="serverFileName"/>. This prevents the server from uploading the
        /// content to a file with path <c>&lt;file path&gt;/<paramref name="serverFileName"/></c> if there's
        /// already a directory with this path, and allows us to receive an error response.
        /// </para>
        /// </remarks>
        private void UploadFileModeAndName(IChannelSession channel, Stream input, long fileSize, string serverFileName)
        {
            SendData(channel, string.Format("C0644 {0} {1}\n", fileSize, serverFileName));
            CheckReturnCode(input);
        }

        /// <summary>
        /// Uploads the content of a file.
        /// </summary>
        /// <param name="channel">The channel to perform the upload in.</param>
        /// <param name="input">A <see cref="Stream"/> from which any feedback from the server can be read.</param>
        /// <param name="source">The content to upload.</param>
        /// <param name="remoteFileName">The name of the remote file, without path, to which the content is uploaded.</param>
        /// <remarks>
        /// <paramref name="remoteFileName"/> is only used for raising the <see cref="Uploading"/> event.
        /// </remarks>
        private void UploadFileContent(IChannelSession channel, Stream input, Stream source, string remoteFileName)
        {
            var totalLength = source.Length;
            var buffer = new byte[BufferSize];

            var read = source.Read(buffer, 0, buffer.Length);

            long totalRead = 0;

            while (read > 0)
            {
                SendData(channel, buffer, read);

                totalRead += read;

                RaiseUploadingEvent(remoteFileName, totalLength, totalRead);

                read = source.Read(buffer, 0, buffer.Length);
            }

            if (totalLength == 0 && totalRead == 0)
            {
                RaiseUploadingEvent(remoteFileName, totalLength, totalRead);
            }

            SendSuccessConfirmation(channel);
            CheckReturnCode(input);
        }

        private void RaiseDownloadingEvent(string filename, long size, long downloaded)
        {
            Downloading?.Invoke(this, new ScpDownloadEventArgs(filename, size, downloaded));
        }

        private void RaiseUploadingEvent(string filename, long size, long uploaded)
        {
            Uploading?.Invoke(this, new ScpUploadEventArgs(filename, size, uploaded));
        }

        private static void SendSuccessConfirmation(IChannel channel)
        {
            SendData(channel, SuccessConfirmationCode);
        }

        private void SendErrorConfirmation(IChannel channel, string message)
        {
            SendData(channel, ErrorConfirmationCode);
            SendData(channel, string.Concat(message, "\n"));
        }

        /// <summary>
        /// Checks the return code.
        /// </summary>
        /// <param name="input">The output stream.</param>
        private void CheckReturnCode(Stream input)
        {
            var b = ReadByte(input);

            if (b > 0)
            {
                var errorText = ReadString(input);

                throw new ScpException(errorText);
            }
        }

        private void SendData(IChannel channel, string command)
        {
            channel.SendData(ConnectionInfo.Encoding.GetBytes(command));
        }

        /// <summary>
        /// Read a LF-terminated string from the <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read from.</param>
        /// <returns>
        /// The string without trailing LF.
        /// </returns>
        private string ReadString(Stream stream)
        {
            var hasError = false;

            var buffer = new List<byte>();

            var b = ReadByte(stream);
            if (b is 1 or 2)
            {
                hasError = true;
                b = ReadByte(stream);
            }

            while (b != SshNet.Session.LineFeed)
            {
                buffer.Add((byte)b);
                b = ReadByte(stream);
            }

            var readBytes = buffer.ToArray();

            if (hasError)
            {
                throw new ScpException(ConnectionInfo.Encoding.GetString(readBytes, 0, readBytes.Length));
            }

            return ConnectionInfo.Encoding.GetString(readBytes, 0, readBytes.Length);
        }

        /// <summary>
        /// Uploads the <see cref="FileSystemInfo.LastWriteTimeUtc"/> and <see cref="FileSystemInfo.LastAccessTimeUtc"/>
        /// of the next file or directory to upload.
        /// </summary>
        /// <param name="channel">The channel to perform the upload in.</param>
        /// <param name="input">A <see cref="Stream"/> from which any feedback from the server can be read.</param>
        /// <param name="fileOrDirectory">The file or directory to upload.</param>
        private void UploadTimes(IChannelSession channel, Stream input, FileSystemInfo fileOrDirectory)
        {
            var zeroTime = DateTime.UnixEpoch;
            var modificationSeconds = (long)(fileOrDirectory.LastWriteTimeUtc - zeroTime).TotalSeconds;
            var accessSeconds = (long)(fileOrDirectory.LastAccessTimeUtc - zeroTime).TotalSeconds;
            SendData(channel, string.Format(CultureInfo.InvariantCulture, "T{0} 0 {1} 0\n", modificationSeconds, accessSeconds));
            CheckReturnCode(input);
        }

        /// <summary>
        /// Upload the files and subdirectories in the specified directory.
        /// </summary>
        /// <param name="channel">The channel to perform the upload in.</param>
        /// <param name="input">A <see cref="Stream"/> from which any feedback from the server can be read.</param>
        /// <param name="directoryInfo">The directory to upload.</param>
        private void UploadDirectoryContent(IChannelSession channel, Stream input, DirectoryInfo directoryInfo)
        {
            // Upload files
            var files = directoryInfo.GetFiles();
            foreach (var file in files)
            {
                using (var source = file.OpenRead())
                {
                    UploadTimes(channel, input, file);
                    UploadFileModeAndName(channel, input, source.Length, file.Name);
                    UploadFileContent(channel, input, source, file.Name);
                }
            }

            // Upload directories
            var directories = directoryInfo.GetDirectories();
            foreach (var directory in directories)
            {
                UploadTimes(channel, input, directory);
                UploadDirectoryModeAndName(channel, input, directory.Name);
                UploadDirectoryContent(channel, input, directory);
            }

            // Mark upload of current directory complete
            SendData(channel, "E\n");
            CheckReturnCode(input);
        }

        /// <summary>
        /// Sets mode and name of the directory being upload.
        /// </summary>
        private void UploadDirectoryModeAndName(IChannelSession channel, Stream input, string directoryName)
        {
            SendData(channel, string.Format("D0755 0 {0}\n", directoryName));
            CheckReturnCode(input);
        }

        /// <summary>
        /// Ensures that a file or directory name received from the remote host in the SCP
        /// protocol stream is a plain local name that cannot redirect a write outside of the
        /// caller-supplied destination directory.
        /// </summary>
        /// <param name="name">The file or directory name as sent by the server.</param>
        /// <exception cref="ScpException">
        /// <paramref name="name"/> is empty, refers to the current or parent directory, or
        /// contains a character that is not valid in a local file name (such as a directory
        /// separator).
        /// </exception>
        internal static void EnsureValidLocalName(string name)
        {
            // A legitimate SCP server only ever sends a single, plain path component in a
            // C (file) or D (directory) record. Reject anything that is empty, refers to the
            // current/parent directory, or carries a directory separator, drive qualifier or
            // other character that is not valid in a local file name. Path.GetInvalidFileNameChars()
            // is platform-aware: on Windows it includes '\', '/' and ':'; on Unix it includes '/'.
            if (string.IsNullOrEmpty(name) ||
                name.Equals(".", StringComparison.Ordinal) ||
                name.Equals("..", StringComparison.Ordinal) ||
                name.IndexOfAny(InvalidLocalNameChars) >= 0)
            {
                throw new ScpException($"The server sent a file or directory name (\"{name}\") that is not a valid local name.");
            }
        }

        private void InternalDownload(IChannel channel, Stream input, Stream output, string filename, long length)
        {
            var buffer = new byte[Math.Min(length, BufferSize)];
            var needToRead = length;

            do
            {
                var read = input.Read(buffer, 0, (int)Math.Min(needToRead, BufferSize));

                output.Write(buffer, 0, read);

                RaiseDownloadingEvent(filename, length, length - needToRead);

                needToRead -= read;
            }
            while (needToRead > 0);

            output.Flush();

            // Raise one more time when file downloaded
            RaiseDownloadingEvent(filename, length, length - needToRead);

            // Send confirmation byte after last data byte was read
            SendSuccessConfirmation(channel);

            CheckReturnCode(input);
        }

        private void InternalDownload(IChannelSession channel, Stream input, FileSystemInfo fileSystemInfo)
        {
            var modifiedTime = DateTime.Now;
            var accessedTime = DateTime.Now;

            var startDirectoryFullName = fileSystemInfo.FullName;
            var currentDirectoryFullName = startDirectoryFullName;
            var directoryCounter = 0;

            while (true)
            {
                var message = ReadString(input);

                if (message == "E")
                {
                    SendSuccessConfirmation(channel); // Send reply

                    directoryCounter--;

                    if (directoryCounter == 0)
                    {
                        break;
                    }

                    var currentDirectoryParent = new DirectoryInfo(currentDirectoryFullName).Parent;

                    Debug.Assert(currentDirectoryParent is not null, $"Should be {directoryCounter.ToString(CultureInfo.InvariantCulture)} levels deeper than {startDirectoryFullName}.");

                    currentDirectoryFullName = currentDirectoryParent.FullName;

                    continue;
                }

                var match = DirectoryInfoRegex.Match(message);
                if (match.Success)
                {
                    SendSuccessConfirmation(channel); // Send reply

                    // Read directory
                    var filename = match.Result("${filename}");

                    DirectoryInfo newDirectoryInfo;
                    if (directoryCounter > 0)
                    {
                        // The server-supplied name is combined into a local path; ensure it
                        // cannot escape the destination directory.
                        EnsureValidLocalName(filename);

                        newDirectoryInfo = Directory.CreateDirectory(Path.Combine(currentDirectoryFullName, filename));
                        newDirectoryInfo.LastAccessTime = accessedTime;
                        newDirectoryInfo.LastWriteTime = modifiedTime;
                    }
                    else
                    {
                        // Don't create directory for first level
                        newDirectoryInfo = (DirectoryInfo)fileSystemInfo;
                    }

                    directoryCounter++;

                    currentDirectoryFullName = newDirectoryInfo.FullName;
                    continue;
                }

                match = FileInfoRegex.Match(message);
                if (match.Success)
                {
                    // Read file
                    SendSuccessConfirmation(channel); //  Send reply

                    var length = long.Parse(match.Result("${length}"), CultureInfo.InvariantCulture);
                    var fileName = match.Result("${filename}");

                    if (fileSystemInfo is not FileInfo fileInfo)
                    {
                        // The server-supplied name is combined into a local path; ensure it
                        // cannot escape the destination directory.
                        EnsureValidLocalName(fileName);

                        fileInfo = new FileInfo(Path.Combine(currentDirectoryFullName, fileName));
                    }

                    using (var output = fileInfo.Open(FileMode.Create, FileAccess.Write))
                    {
                        InternalDownload(channel, input, output, fileName, length);
                    }

                    fileInfo.LastAccessTime = accessedTime;
                    fileInfo.LastWriteTime = modifiedTime;

                    if (directoryCounter == 0)
                    {
                        break;
                    }

                    continue;
                }

                match = TimestampRegex.Match(message);
                if (match.Success)
                {
                    // Read timestamp
                    SendSuccessConfirmation(channel); //  Send reply

                    var mtime = long.Parse(match.Result("${mtime}"), CultureInfo.InvariantCulture);
                    var atime = long.Parse(match.Result("${atime}"), CultureInfo.InvariantCulture);

                    var zeroTime = DateTime.UnixEpoch;
                    modifiedTime = zeroTime.AddSeconds(mtime);
                    accessedTime = zeroTime.AddSeconds(atime);
                    continue;
                }

                SendErrorConfirmation(channel, string.Format("\"{0}\" is not valid protocol message.", message));
            }
        }
    }
}
