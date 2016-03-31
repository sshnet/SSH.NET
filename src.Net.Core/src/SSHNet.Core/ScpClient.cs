﻿using System;
using System.Linq;
using System.Text;
using Renci.SshNet.Channels;
using System.IO;
using Renci.SshNet.Common;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides SCP client functionality.
    /// </summary>
    public partial class ScpClient : BaseClient
    {
        private static readonly Regex FileInfoRe = new Regex(@"C(?<mode>\d{4}) (?<length>\d+) (?<filename>.+)");
        private static char[] _byteToChar;

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
        /// Occurs when downloading file.
        /// </summary>
        public event EventHandler<ScpDownloadEventArgs> Downloading;

        /// <summary>
        /// Occurs when uploading file.
        /// </summary>
        public event EventHandler<ScpUploadEventArgs> Uploading;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is null.</exception>
        public ScpClient(ConnectionInfo connectionInfo)
            : this(connectionInfo, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is null or contains whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="F:System.Net.IPEndPoint.MinPort"/> and <see cref="System.Net.IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public ScpClient(string host, int port, string username, string password)
            : this(new PasswordConnectionInfo(host, port, username, password), true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="password">Authentication password.</param>
        /// <exception cref="ArgumentNullException"><paramref name="password"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, or <paramref name="username"/> is null or contains whitespace characters.</exception>
        public ScpClient(string host, string username, string password)
            : this(host, ConnectionInfo.DefaultPort, username, password)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="port">Connection port.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is null or contains whitespace characters.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="port"/> is not within <see cref="F:System.Net.IPEndPoint.MinPort"/> and <see cref="System.Net.IPEndPoint.MaxPort"/>.</exception>
        [SuppressMessage("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope", Justification = "Disposed in Dispose(bool) method.")]
        public ScpClient(string host, int port, string username, params PrivateKeyFile[] keyFiles)
            : this(new PrivateKeyConnectionInfo(host, port, username, keyFiles), true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpClient"/> class.
        /// </summary>
        /// <param name="host">Connection host.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="keyFiles">Authentication private key file(s) .</param>
        /// <exception cref="ArgumentNullException"><paramref name="keyFiles"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="host"/> is invalid, -or- <paramref name="username"/> is null or contains whitespace characters.</exception>
        public ScpClient(string host, string username, params PrivateKeyFile[] keyFiles)
            : this(host, ConnectionInfo.DefaultPort, username, keyFiles)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpClient"/> class.
        /// </summary>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="ownsConnectionInfo">Specified whether this instance owns the connection info.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is null.</exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="connectionInfo"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="serviceFactory"/> is null.</exception>
        /// <remarks>
        /// If <paramref name="ownsConnectionInfo"/> is <c>true</c>, then the
        /// connection info will be disposed when this instance is disposed.
        /// </remarks>
        internal ScpClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory)
            : base(connectionInfo, ownsConnectionInfo, serviceFactory)
        {
            OperationTimeout = new TimeSpan(0, 0, 0, 0, -1);
            BufferSize = 1024 * 16;

            if (_byteToChar == null)
            {
                _byteToChar = new char[128];
                var ch = '\0';
                for (var i = 0; i < 128; i++)
                {
                    _byteToChar[i] = ch++;
                }
            }
        }

        #endregion

        /// <summary>
        /// Uploads the specified stream to the remote host.
        /// </summary>
        /// <param name="source">Stream to upload.</param>
        /// <param name="path">Remote host file name.</param>
        public void Upload(Stream source, string path)
        {
            using (var input = ServiceFactory.CreatePipeStream())
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += delegate(object sender, ChannelDataEventArgs e)
                {
                    input.Write(e.Data, 0, e.Data.Length);
                    input.Flush();
                };

                channel.Open();

                var pathEnd = path.LastIndexOfAny(new[] { '\\', '/' });
                if (pathEnd != -1)
                {
                    // split the path from the file
                    var pathOnly = path.Substring(0, pathEnd);
                    var fileOnly = path.Substring(pathEnd + 1);
                    //  Send channel command request
                    channel.SendExecRequest(string.Format("scp -t \"{0}\"", pathOnly));
                    CheckReturnCode(input);

                    path = fileOnly;
                }

                InternalUpload(channel, input, source, path);

                channel.Close();
            }
        }

        /// <summary>
        /// Downloads the specified file from the remote host to the stream.
        /// </summary>
        /// <param name="filename">Remote host file name.</param>
        /// <param name="destination">The stream where to download remote file.</param>
        /// <exception cref="ArgumentException"><paramref name="filename"/> is null or contains whitespace characters.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="destination"/> is null.</exception>
        /// <remarks>Method calls made by this method to <paramref name="destination"/>, may under certain conditions result in exceptions thrown by the stream.</remarks>
        public void Download(string filename, Stream destination)
        {
            if (filename.IsNullOrWhiteSpace())
                throw new ArgumentException("filename");

            if (destination == null)
                throw new ArgumentNullException("destination");

            using (var input = new PipeStream())
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += delegate(object sender, ChannelDataEventArgs e)
                {
                    input.Write(e.Data, 0, e.Data.Length);
                    input.Flush();
                };

                channel.Open();

                //  Send channel command request
                channel.SendExecRequest(string.Format("scp -f \"{0}\"", filename));
                SendConfirmation(channel); //  Send reply

                var message = ReadString(input);
                var match = FileInfoRe.Match(message);

                if (match.Success)
                {
                    //  Read file
                    SendConfirmation(channel); //  Send reply

                    var mode = match.Result("${mode}");
                    var length = long.Parse(match.Result("${length}"));
                    var fileName = match.Result("${filename}");

                    InternalDownload(channel, input, destination, fileName, length);
                }
                else
                {
                    SendConfirmation(channel, 1, string.Format("\"{0}\" is not valid protocol message.", message));
                }

                channel.Close();
            }
        }

        private void InternalSetTimestamp(IChannelSession channel, Stream input, DateTime lastWriteTime, DateTime lastAccessime)
        {
            var zeroTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var modificationSeconds = (long)(lastWriteTime - zeroTime).TotalSeconds;
            var accessSeconds = (long)(lastAccessime - zeroTime).TotalSeconds;
            SendData(channel, string.Format("T{0} 0 {1} 0\n", modificationSeconds, accessSeconds));
            CheckReturnCode(input);
        }

        private void InternalUpload(IChannelSession channel, Stream input, Stream source, string filename)
        {
            var length = source.Length;

            SendData(channel, string.Format("C0644 {0} {1}\n", length, Path.GetFileName(filename)));
            CheckReturnCode(input);

            var buffer = new byte[BufferSize];

            var read = source.Read(buffer, 0, buffer.Length);

            long totalRead = 0;

            while (read > 0)
            {
                SendData(channel, buffer, read);

                totalRead += read;

                RaiseUploadingEvent(filename, length, totalRead);

                read = source.Read(buffer, 0, buffer.Length);
            }

            SendConfirmation(channel);
            CheckReturnCode(input);
        }

        private void InternalDownload(IChannelSession channel, Stream input, Stream output, string filename, long length)
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

            //  Raise one more time when file downloaded
            RaiseDownloadingEvent(filename, length, length - needToRead);

            //  Send confirmation byte after last data byte was read
            SendConfirmation(channel);

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

        private void SendConfirmation(IChannelSession channel)
        {
            SendData(channel, new byte[] { 0 });
        }

        private void SendConfirmation(IChannelSession channel, byte errorCode, string message)
        {
            SendData(channel, new[] { errorCode });
            SendData(channel, string.Format("{0}\n", message));
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

        partial void SendData(IChannelSession channel, string command);

        private void SendData(IChannelSession channel, byte[] buffer, int length)
        {
#if true //old TUNING
            channel.SendData(buffer, 0, length);
#else
            if (length == buffer.Length)
            {
                channel.SendData(buffer);
            }
            else
            {
                channel.SendData(buffer.Take(length).ToArray());
            }
#endif
        }

        private void SendData(IChannelSession channel, byte[] buffer)
        {
            channel.SendData(buffer);
        }

        private static int ReadByte(Stream stream)
        {
            var b = stream.ReadByte();

            while (b < 0)
            {
                Thread.Sleep(100);
                b = stream.ReadByte();
            }

            return b;
        }

        private static string ReadString(Stream stream)
        {
            var hasError = false;

            var sb = new StringBuilder();

            var b = ReadByte(stream);

            if (b == 1 || b == 2)
            {
                hasError = true;
                b = ReadByte(stream);
            }

            var ch = _byteToChar[b];

            while (ch != '\n')
            {
                sb.Append(ch);

                b = ReadByte(stream);

                ch = _byteToChar[b];
            }

            if (hasError)
                throw new ScpException(sb.ToString());

            return sb.ToString();
        }
    }
}
