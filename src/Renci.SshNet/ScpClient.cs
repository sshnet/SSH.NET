using System;
using Renci.SshNet.Channels;
using System.IO;
using Renci.SshNet.Common;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Collections.Generic;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides SCP client functionality.
    /// </summary>
    /// <remarks>
    /// <para>
    /// More information on the SCP protocol is available here:
    /// https://github.com/net-ssh/net-scp/blob/master/lib/net/scp.rb
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
        private static readonly Regex FileInfoRe = new Regex(@"C(?<mode>\d{4}) (?<length>\d+) (?<filename>.+)");
        private static readonly byte[] SuccessConfirmationCode = {0};
        private static readonly byte[] ErrorConfirmationCode = { 1 };

        private IRemotePathTransformation _remotePathTransformation;

        /// <summary>
        /// Gets or sets the operation timeout.
        /// </summary>
        /// <value>
        /// The timeout to wait until an operation completes. The default value is negative
        /// one (-1) milliseconds, which indicates an infinite time-out period.
        /// </value>
        public TimeSpan OperationTimeout { get; set; }

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
        /// The transformation to apply to remote paths. The default is <see cref="SshNet.RemotePathTransformation.DoubleQuote"/>.
        /// </value>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c>.</exception>
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
            get { return _remotePathTransformation; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _remotePathTransformation = value;
            }
        }

        /// <summary>
        /// Occurs when downloading file.
        /// </summary>
        public event EventHandler<ScpDownloadEventArgs> Downloading;

        /// <summary>
        /// Occurs when uploading file.
        /// </summary>
        public event EventHandler<ScpUploadEventArgs> Uploading;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <c>null</c>.</exception>
        public ScpClient(ConnectionInfo connectionInfo)
            : this(connectionInfo, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is <c>null</c> or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="IPEndPoint.MinPort"/> and <see cref="IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public ScpClient(string host, int port, string username, string password)
            : this(new PasswordConnectionInfo(host, port, username, password), true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is <c>null</c> or contains only whitespace characters.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is <c>null</c> or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="IPEndPoint.MinPort"/> and <see cref="IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public ScpClient(string host, int port, string username, params PrivateKeyFile[] keyFiles)
            : this(new PrivateKeyConnectionInfo(host, port, username, keyFiles), true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is <c>null</c> or contains only whitespace characters.</exception>
        public ScpClient(string host, string username, params PrivateKeyFile[] keyFiles)
            : this(host, ConnectionInfo.DefaultPort, username, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="ownsConnectionInfo">Specified whether this instance owns the connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <c>null</c>.</exception>
        /// <remarks>
        /// If <paramref name="ownsConnectionInfo"/> is <c>true</c>, then the
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
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="serviceFactory"/> is <c>null</c>.</exception>
        /// <remarks>
        /// If <paramref name="ownsConnectionInfo"/> is <c>true</c>, then the
        /// connection info will be disposed when this instance is disposed.
        /// </remarks>
        internal ScpClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory)
            : base(connectionInfo, ownsConnectionInfo, serviceFactory)
        {
            OperationTimeout = SshNet.Session.InfiniteTimeSpan;
            BufferSize = 1024 * 16;
            _remotePathTransformation = serviceFactory.CreateRemotePathDoubleQuoteTransformation();
        }

        #endregion

        /// <summary>
        /// Uploads the specified stream to the remote host.
        /// </summary>
        /// <param name="source">The <see cref="Stream"/> to upload.</param>
        /// <param name="path">A relative or absolute path for the remote file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is a zero-length <see cref="string"/>.</exception>
        /// <exception cref="ScpException">A directory with the specified path exists on the remote host.</exception>
        /// <exception cref="SshException">The secure copy execution request was rejected by the server.</exception>
        public void Upload(Stream source, string path)
        {
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
        /// Downloads the specified file from the remote host to the stream.
        /// </summary>
        /// <param name="filename">A relative or absolute path for the remote file.</param>
        /// <param name="destination">The <see cref="Stream"/> to download the remote file to.</param>
        /// <exception cref="ArgumentException"><paramref name="filename"/> is <c>null</c> or contains only whitespace characters.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="destination"/> is <c>null</c>.</exception>
        /// <exception cref="ScpException"><paramref name="filename"/> exists on the remote host, and is not a regular file.</exception>
        /// <exception cref="SshException">The secure copy execution request was rejected by the server.</exception>
        public void Download(string filename, Stream destination)
        {
            if (filename.IsNullOrWhiteSpace())
                throw new ArgumentException("filename");

            if (destination == null)
                throw new ArgumentNullException("destination");

            using (var input = ServiceFactory.CreatePipeStream())
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += (sender, e) => input.Write(e.Data, 0, e.Data.Length);
                channel.Open();

                //  Send channel command request
                if (!channel.SendExecRequest(string.Format("scp -f {0}", _remotePathTransformation.Transform(filename))))
                {
                    throw SecureExecutionRequestRejectedException();
                }
                SendSuccessConfirmation(channel); //  Send reply

                var message = ReadString(input);
                var match = FileInfoRe.Match(message);

                if (match.Success)
                {
                    //  Read file
                    SendSuccessConfirmation(channel); //  Send reply

                    var length = long.Parse(match.Result("${length}"));
                    var fileName = match.Result("${filename}");

                    InternalDownload(channel, input, destination, fileName, length);
                }
                else
                {
                    SendErrorConfirmation(channel, string.Format("\"{0}\" is not valid protocol message.", message));
                }
            }
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

        private void InternalDownload(IChannel channel, Stream input, Stream output, string filename, long length)
        {
            var buffer = new byte[Math.Min(length, BufferSize)];
            var needToRead = length;

            do
            {
                var read = input.Read(buffer, 0, (int) Math.Min(needToRead, BufferSize));

                output.Write(buffer, 0, read);

                RaiseDownloadingEvent(filename, length, length - needToRead);

                needToRead -= read;
            }
            while (needToRead > 0);

            output.Flush();

            //  Raise one more time when file downloaded
            RaiseDownloadingEvent(filename, length, length - needToRead);

            //  Send confirmation byte after last data byte was read
            SendSuccessConfirmation(channel);

            CheckReturnCode(input);
        }

        private void RaiseDownloadingEvent(string filename, long size, long downloaded)
        {
            if (Downloading != null)
            {
                Downloading(this, new ScpDownloadEventArgs(filename, size, downloaded));
            }
        }

        private void RaiseUploadingEvent(string filename, long size, long uploaded)
        {
            if (Uploading != null)
            {
                Uploading(this, new ScpUploadEventArgs(filename, size, uploaded));
            }
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
                throw new SshException("Stream has been closed.");
            return b;
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
            if (b == 1 || b == 2)
            {
                hasError = true;
                b = ReadByte(stream);
            }

            while (b != SshNet.Session.LineFeed)
            {
                buffer.Add((byte) b);
                b = ReadByte(stream);
            }

            var readBytes = buffer.ToArray();

            if (hasError)
                throw new ScpException(ConnectionInfo.Encoding.GetString(readBytes, 0, readBytes.Length));
            return ConnectionInfo.Encoding.GetString(readBytes, 0, readBytes.Length);
        }

        private static SshException SecureExecutionRequestRejectedException()
        {
            throw new SshException("Secure copy execution request was rejected by the server. Please consult the server logs.");
        }
    }
}
