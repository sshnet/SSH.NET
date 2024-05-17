#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    /// </remarks>
    public partial class ScpClient : BaseClient
    {
        private const string Message = "filename";
        private static readonly Regex FileInfoRe = new Regex(@"C(?<mode>\d{4}) (?<length>\d+) (?<filename>.+)", RegexOptions.Compiled);
        private static readonly byte[] SuccessConfirmationCode = { 0 };
        private static readonly byte[] ErrorConfirmationCode = { 1 };
        private static readonly Regex DirectoryInfoRe = new Regex(@"D(?<mode>\d{4}) (?<length>\d+) (?<filename>.+)", RegexOptions.Compiled);
        private static readonly Regex TimestampRe = new Regex(@"T(?<mtime>\d+) 0 (?<atime>\d+) 0", RegexOptions.Compiled);

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
        /// The transformation to apply to remote paths. The default is <see cref="RemotePathTransformation.DoubleQuote"/>.
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
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _remotePathTransformation = value;
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
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <see langword="null"/>.</exception>
        public ScpClient(ConnectionInfo connectionInfo)
            : this(connectionInfo, ownsConnectionInfo: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is <see langword="null"/> or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="IPEndPoint.MinPort"/> and <see cref="IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public ScpClient(string host, int port, string username, string password)
            : this(new PasswordConnectionInfo(host, port, username, password), ownsConnectionInfo: true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is <see langword="null"/> or contains only whitespace characters.</exception>
        public ScpClient(string host, string username, string password)
            : this(host, ConnectionInfo.DefaultPort, username, password)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is <see langword="null"/> or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="IPEndPoint.MinPort"/> and <see cref="IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public ScpClient(string host, int port, string username, params IPrivateKeySource[] keyFiles)
            : this(new PrivateKeyConnectionInfo(host, port, username, keyFiles), ownsConnectionInfo: true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is <see langword="null"/> or contains only whitespace characters.</exception>
        public ScpClient(string host, string username, params IPrivateKeySource[] keyFiles)
            : this(host, ConnectionInfo.DefaultPort, username, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="ownsConnectionInfo">Specified whether this instance owns the connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// If <paramref name="ownsConnectionInfo"/> is <see langword="true"/>, then the
        /// connection info will be disposed when this instance is disposed.
        /// </remarks>
        private ScpClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo)
            : this(connectionInfo, ownsConnectionInfo, new ServiceFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="ownsConnectionInfo">Specified whether this instance owns the connection info.</param>
        /// <param name="serviceFactory">The factory to use for creating new services.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="serviceFactory"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// If <paramref name="ownsConnectionInfo"/> is <see langword="true"/>, then the
        /// connection info will be disposed when this instance is disposed.
        /// </remarks>
        internal ScpClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory)
            : base(connectionInfo, ownsConnectionInfo, serviceFactory)
        {
            OperationTimeout = Timeout.InfiniteTimeSpan;
            BufferSize = 1024 * 16;
            _remotePathTransformation = serviceFactory.CreateRemotePathDoubleQuoteTransformation();
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
                channel.DataReceived += (sender, e) => input.Write(e.Data, 0, e.Data.Length);
                channel.Open();

                // Pass only the directory part of the path to the server, and use the (hidden) -d option to signal
                // that we expect the target to be a directory.
                if (!channel.SendExecRequest(string.Format("scp -t -d {0}", _remotePathTransformation.Transform(posixPath.Directory))))
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
            if (fileInfo is null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            if (Session is null)
            {
                throw new SshConnectionException("Client not connected.");
            }

            var posixPath = PosixPath.CreateAbsoluteOrRelativeFilePath(path);

            using (var input = ServiceFactory.CreatePipeStream())
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += (sender, e) => input.Write(e.Data, 0, e.Data.Length);
                channel.Open();

                // Pass only the directory part of the path to the server, and use the (hidden) -d option to signal
                // that we expect the target to be a directory.
                if (!channel.SendExecRequest($"scp -t -d {_remotePathTransformation.Transform(posixPath.Directory)}"))
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
            if (directoryInfo is null)
            {
                throw new ArgumentNullException(nameof(directoryInfo));
            }

            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.Length == 0)
            {
                throw new ArgumentException("The path cannot be a zero-length string.", nameof(path));
            }

            if (Session is null)
            {
                throw new SshConnectionException("Client not connected.");
            }

            using (var input = ServiceFactory.CreatePipeStream())
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += (sender, e) => input.Write(e.Data, 0, e.Data.Length);
                channel.Open();

                // start copy with the following options:
                // -p preserve modification and access times
                // -r copy directories recursively
                // -d expect path to be a directory
                // -t copy to remote
                if (!channel.SendExecRequest($"scp -r -p -d -t {_remotePathTransformation.Transform(path)}"))
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
            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("filename");
            }

            if (fileInfo is null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            if (Session is null)
            {
                throw new SshConnectionException("Client not connected.");
            }

            using (var input = ServiceFactory.CreatePipeStream())
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += (sender, e) => input.Write(e.Data, 0, e.Data.Length);
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
            if (string.IsNullOrEmpty(directoryName))
            {
                throw new ArgumentException("directoryName");
            }

            if (directoryInfo is null)
            {
                throw new ArgumentNullException(nameof(directoryInfo));
            }

            if (Session is null)
            {
                throw new SshConnectionException("Client not connected.");
            }

            using (var input = ServiceFactory.CreatePipeStream())
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += (sender, e) => input.Write(e.Data, 0, e.Data.Length);
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
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException(Message);
            }

            if (destination is null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (Session is null)
            {
                throw new SshConnectionException("Client not connected.");
            }

            using (var input = ServiceFactory.CreatePipeStream())
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += (sender, e) => input.Write(e.Data, 0, e.Data.Length);
                channel.Open();

                // Send channel command request
                if (!channel.SendExecRequest(string.Concat("scp -f ", _remotePathTransformation.Transform(filename))))
                {
                    throw SecureExecutionRequestRejectedException();
                }

                SendSuccessConfirmation(channel); // Send reply

                var message = ReadString(input);
                var match = FileInfoRe.Match(message);

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
#if NET ||NETSTANDARD2_1_OR_GREATER
            var zeroTime = DateTime.UnixEpoch;
#else
            var zeroTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
#endif
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

                    var currentDirectoryParent = new DirectoryInfo(currentDirectoryFullName).Parent;

                    if (currentDirectoryParent is null)
                    {
                        break;
                    }

                    currentDirectoryFullName = currentDirectoryParent.FullName;

                    if (directoryCounter == 0)
                    {
                        break;
                    }

                    continue;
                }

                var match = DirectoryInfoRe.Match(message);
                if (match.Success)
                {
                    SendSuccessConfirmation(channel); // Send reply

                    // Read directory
                    var filename = match.Result("${filename}");

                    DirectoryInfo newDirectoryInfo;
                    if (directoryCounter > 0)
                    {
                        newDirectoryInfo = Directory.CreateDirectory(Path.Combine(currentDirectoryFullName, filename));
                        newDirectoryInfo.LastAccessTime = accessedTime;
                        newDirectoryInfo.LastWriteTime = modifiedTime;
                    }
                    else
                    {
                        // Don't create directory for first level
                        newDirectoryInfo = (DirectoryInfo) fileSystemInfo;
                    }

                    directoryCounter++;

                    currentDirectoryFullName = newDirectoryInfo.FullName;
                    continue;
                }

                match = FileInfoRe.Match(message);
                if (match.Success)
                {
                    // Read file
                    SendSuccessConfirmation(channel); //  Send reply

                    var length = long.Parse(match.Result("${length}"), CultureInfo.InvariantCulture);
                    var fileName = match.Result("${filename}");

                    if (fileSystemInfo is not FileInfo fileInfo)
                    {
                        fileInfo = new FileInfo(Path.Combine(currentDirectoryFullName, fileName));
                    }

                    using (var output = fileInfo.OpenWrite())
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

                match = TimestampRe.Match(message);
                if (match.Success)
                {
                    // Read timestamp
                    SendSuccessConfirmation(channel); //  Send reply

                    var mtime = long.Parse(match.Result("${mtime}"), CultureInfo.InvariantCulture);
                    var atime = long.Parse(match.Result("${atime}"), CultureInfo.InvariantCulture);

#if NET || NETSTANDARD2_1_OR_GREATER
                    var zeroTime = DateTime.UnixEpoch;
#else
                    var zeroTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
#endif
                    modifiedTime = zeroTime.AddSeconds(mtime);
                    accessedTime = zeroTime.AddSeconds(atime);
                    continue;
                }

                SendErrorConfirmation(channel, string.Format("\"{0}\" is not valid protocol message.", message));
            }
        }
    }
}
