using System;
using Renci.SshNet.Channels;
using System.IO;
using Renci.SshNet.Common;
using System.Text.RegularExpressions;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides SCP client functionality.
    /// </summary>
    public partial class ScpClient
    {
        private static readonly Regex DirectoryInfoRe = new Regex(@"D(?<mode>\d{4}) (?<length>\d+) (?<filename>.+)");
        private static readonly Regex TimestampRe = new Regex(@"T(?<mtime>\d+) 0 (?<atime>\d+) 0");

        /// <summary>
        /// Uploads the specified file to the remote host.
        /// </summary>
        /// <param name="fileInfo">The file system info.</param>
        /// <param name="path">A relative or absolute path for the remote file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is a zero-length <see cref="string"/>.</exception>
        /// <exception cref="ScpException">A directory with the specified path exists on the remote host.</exception>
        /// <exception cref="SshException">The secure copy execution request was rejected by the server.</exception>
        public void Upload(FileInfo fileInfo, string path)
        {
            if (fileInfo == null)
                throw new ArgumentNullException("fileInfo");

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
        /// <exception cref="ArgumentNullException"><paramref name="directoryInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is a zero-length string.</exception>
        /// <exception cref="ScpException"><paramref name="path"/> does not exist on the remote host, is not a directory or the user does not have the required permission.</exception>
        /// <exception cref="SshException">The secure copy execution request was rejected by the server.</exception>
        public void Upload(DirectoryInfo directoryInfo, string path)
        {
            if (directoryInfo == null)
                throw new ArgumentNullException("directoryInfo");
            if (path == null)
                throw new ArgumentNullException("path");
            if (path.Length == 0)
                throw new ArgumentException("The path cannot be a zero-length string.", "path");

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
                if (!channel.SendExecRequest(string.Format("scp -r -p -d -t {0}", _remotePathTransformation.Transform(path))))
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
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="filename"/> is <c>null</c> or empty.</exception>
        /// <exception cref="ScpException"><paramref name="filename"/> exists on the remote host, and is not a regular file.</exception>
        /// <exception cref="SshException">The secure copy execution request was rejected by the server.</exception>
        public void Download(string filename, FileInfo fileInfo)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("filename");
            if (fileInfo == null)
                throw new ArgumentNullException("fileInfo");

            using (var input = ServiceFactory.CreatePipeStream())
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += (sender, e) => input.Write(e.Data, 0, e.Data.Length);
                channel.Open();

                // Send channel command request
                if (!channel.SendExecRequest(string.Format("scp -pf {0}", _remotePathTransformation.Transform(filename))))
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
        /// <exception cref="ArgumentException"><paramref name="directoryName"/> is <c>null</c> or empty.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="directoryInfo"/> is <c>null</c>.</exception>
        /// <exception cref="ScpException">File or directory with the specified path does not exist on the remote host.</exception>
        /// <exception cref="SshException">The secure copy execution request was rejected by the server.</exception>
        public void Download(string directoryName, DirectoryInfo directoryInfo)
        {
            if (string.IsNullOrEmpty(directoryName))
                throw new ArgumentException("directoryName");
            if (directoryInfo == null)
                throw new ArgumentNullException("directoryInfo");

            using (var input = ServiceFactory.CreatePipeStream())
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += (sender, e) => input.Write(e.Data, 0, e.Data.Length);
                channel.Open();

                // Send channel command request
                if (!channel.SendExecRequest(string.Format("scp -prf {0}", _remotePathTransformation.Transform(directoryName))))
                {
                    throw SecureExecutionRequestRejectedException();
                }
                // Send reply
                SendSuccessConfirmation(channel);

                InternalDownload(channel, input, directoryInfo);
            }
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
            var zeroTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var modificationSeconds = (long) (fileOrDirectory.LastWriteTimeUtc - zeroTime).TotalSeconds;
            var accessSeconds = (long) (fileOrDirectory.LastAccessTimeUtc - zeroTime).TotalSeconds;
            SendData(channel, string.Format("T{0} 0 {1} 0\n", modificationSeconds, accessSeconds));
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
            //  Upload files
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

            //  Upload directories
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
                    SendSuccessConfirmation(channel); //  Send reply

                    directoryCounter--;

                    currentDirectoryFullName = new DirectoryInfo(currentDirectoryFullName).Parent.FullName;

                    if (directoryCounter == 0)
                        break;
                    continue;
                }

                var match = DirectoryInfoRe.Match(message);
                if (match.Success)
                {
                    SendSuccessConfirmation(channel); //  Send reply

                    //  Read directory
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
                        //  Don't create directory for first level
                        newDirectoryInfo = fileSystemInfo as DirectoryInfo;
                    }

                    directoryCounter++;

                    currentDirectoryFullName = newDirectoryInfo.FullName;
                    continue;
                }

                match = FileInfoRe.Match(message);
                if (match.Success)
                {
                    //  Read file
                    SendSuccessConfirmation(channel); //  Send reply

                    var length = long.Parse(match.Result("${length}"));
                    var fileName = match.Result("${filename}");

                    var fileInfo = fileSystemInfo as FileInfo;

                    if (fileInfo == null)
                        fileInfo = new FileInfo(Path.Combine(currentDirectoryFullName, fileName));

                    using (var output = fileInfo.OpenWrite())
                    {
                        InternalDownload(channel, input, output, fileName, length);
                    }

                    fileInfo.LastAccessTime = accessedTime;
                    fileInfo.LastWriteTime = modifiedTime;

                    if (directoryCounter == 0)
                        break;
                    continue;
                }

                match = TimestampRe.Match(message);
                if (match.Success)
                {
                    //  Read timestamp
                    SendSuccessConfirmation(channel); //  Send reply

                    var mtime = long.Parse(match.Result("${mtime}"));
                    var atime = long.Parse(match.Result("${atime}"));

                    var zeroTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    modifiedTime = zeroTime.AddSeconds(mtime);
                    accessedTime = zeroTime.AddSeconds(atime);
                    continue;
                }

                SendErrorConfirmation(channel, string.Format("\"{0}\" is not valid protocol message.", message));
            }
        }

        /// <summary>
        /// Return a value indicating whether the specified path is a valid SCP file path.
        /// </summary>
        /// <param name="path">The path to verify.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="path"/> is a valid SCP file path; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// To match OpenSSH behavior (introduced as a result of CVE-2018-20685), a file path is considered
        /// invalid in any of the following conditions:
        /// <list type="bullet">
        ///   <item>
        ///     <description><paramref name="path"/> is a zero-length string.</description>
        ///   </item>
        ///   <item>
        ///     <description><paramref name="path"/> is &quot;<c>.</c>&quot;.</description>
        ///   </item>
        ///   <item>
        ///     <description><paramref name="path"/> is &quot;<c>..</c>&quot;.</description>
        ///   </item>
        ///   <item>
        ///     <description><paramref name="path"/> contains a forward slash (/).</description>
        ///   </item>
        /// </list>
        /// </remarks>
        private static bool IsValidScpFilePath(string path)
        {
            return path != null &&
                   path.Length != 0 &&
                   path != "." &&
                   path != ".." &&
                   path.IndexOf('/') == -1;
        }
    }
}
