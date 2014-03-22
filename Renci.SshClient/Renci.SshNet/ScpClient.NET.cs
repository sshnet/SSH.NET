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
        private static readonly Regex _directoryInfoRe = new Regex(@"D(?<mode>\d{4}) (?<length>\d+) (?<filename>.+)");
        private static readonly Regex _timestampRe = new Regex(@"T(?<mtime>\d+) 0 (?<atime>\d+) 0");

        /// <summary>
        /// Uploads the specified file to the remote host.
        /// </summary>
        /// <param name="fileInfo">The file system info.</param>
        /// <param name="path">The path.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo" /> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is null or empty.</exception>
        public void Upload(FileInfo fileInfo, string path)
        {
            if (fileInfo == null)
                throw new ArgumentNullException("fileInfo");
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path");

            using (var input = new PipeStream())
            using (var channel = this.Session.CreateClientChannel<ChannelSession>())
            {
                channel.DataReceived += delegate(object sender, ChannelDataEventArgs e)
                {
                    input.Write(e.Data, 0, e.Data.Length);
                    input.Flush();
                };

                channel.Open();

                //  Send channel command request
                channel.SendExecRequest(string.Format("scp -t \"{0}\"", path));
                this.CheckReturnCode(input);

                this.InternalUpload(channel, input, fileInfo, fileInfo.Name);

                channel.Close();
            }
        }

        /// <summary>
        /// Uploads the specified directory to the remote host.
        /// </summary>
        /// <param name="directoryInfo">The directory info.</param>
        /// <param name="path">The path.</param>
        /// <exception cref="ArgumentNullException">fileSystemInfo</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is null or empty.</exception>
        public void Upload(DirectoryInfo directoryInfo, string path)
        {
            if (directoryInfo == null)
                throw new ArgumentNullException("directoryInfo");
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path");

            using (var input = new PipeStream())
            using (var channel = this.Session.CreateClientChannel<ChannelSession>())
            {
                channel.DataReceived += delegate(object sender, ChannelDataEventArgs e)
                {
                    input.Write(e.Data, 0, e.Data.Length);
                    input.Flush();
                };

                channel.Open();

                //  Send channel command request
                channel.SendExecRequest(string.Format("scp -rt \"{0}\"", path));
                this.CheckReturnCode(input);

                this.InternalSetTimestamp(channel, input, directoryInfo.LastWriteTimeUtc, directoryInfo.LastAccessTimeUtc);
                this.SendData(channel, string.Format("D0755 0 {0}\n", Path.GetFileName(path)));
                this.CheckReturnCode(input);

                this.InternalUpload(channel, input, directoryInfo);

                this.SendData(channel, "E\n");
                this.CheckReturnCode(input);

                channel.Close();
            }
        }

        /// <summary>
        /// Downloads the specified file from the remote host to local file.
        /// </summary>
        /// <param name="filename">Remote host file name.</param>
        /// <param name="fileInfo">Local file information.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="filename"/> is null or empty.</exception>
        public void Download(string filename, FileInfo fileInfo)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("filename");
            if (fileInfo == null)
                throw new ArgumentNullException("fileInfo");

            using (var input = new PipeStream())
            using (var channel = this.Session.CreateClientChannel<ChannelSession>())
            {
                channel.DataReceived += delegate(object sender, ChannelDataEventArgs e)
                {
                    input.Write(e.Data, 0, e.Data.Length);
                    input.Flush();
                };

                channel.Open();

                //  Send channel command request
                channel.SendExecRequest(string.Format("scp -pf \"{0}\"", filename));
                this.SendConfirmation(channel); //  Send reply

                this.InternalDownload(channel, input, fileInfo);

                channel.Close();
            }
        }

        /// <summary>
        /// Downloads the specified directory from the remote host to local directory.
        /// </summary>
        /// <param name="directoryName">Remote host directory name.</param>
        /// <param name="directoryInfo">Local directory information.</param>
        /// <exception cref="ArgumentException"><paramref name="directoryName"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="directoryInfo"/> is null.</exception>
        public void Download(string directoryName, DirectoryInfo directoryInfo)
        {
            if (string.IsNullOrEmpty(directoryName))
                throw new ArgumentException("directoryName");
            if (directoryInfo == null)
                throw new ArgumentNullException("directoryInfo");

            using (var input = new PipeStream())
            using (var channel = this.Session.CreateClientChannel<ChannelSession>())
            {
                channel.DataReceived += delegate(object sender, ChannelDataEventArgs e)
                {
                    input.Write(e.Data, 0, e.Data.Length);
                    input.Flush();
                };

                channel.Open();

                //  Send channel command request
                channel.SendExecRequest(string.Format("scp -prf \"{0}\"", directoryName));
                this.SendConfirmation(channel); //  Send reply

                this.InternalDownload(channel, input, directoryInfo);

                channel.Close();
            }
        }

        private void InternalUpload(ChannelSession channel, Stream input, FileInfo fileInfo, string filename)
        {
            this.InternalSetTimestamp(channel, input, fileInfo.LastWriteTimeUtc, fileInfo.LastAccessTimeUtc);
            using (var source = fileInfo.OpenRead())
            {
                this.InternalUpload(channel, input, source, filename);
            }
        }

        private void InternalUpload(ChannelSession channel, Stream input, DirectoryInfo directoryInfo)
        {
            //  Upload files
            var files = directoryInfo.GetFiles();
            foreach (var file in files)
            {
                this.InternalUpload(channel, input, file, file.Name);
            }

            //  Upload directories
            var directories = directoryInfo.GetDirectories();
            foreach (var directory in directories)
            {
                this.InternalSetTimestamp(channel, input, directoryInfo.LastWriteTimeUtc, directoryInfo.LastAccessTimeUtc);
                this.SendData(channel, string.Format("D0755 0 {0}\n", directory.Name));
                this.CheckReturnCode(input);

                this.InternalUpload(channel, input, directory);

                this.SendData(channel, "E\n");
                this.CheckReturnCode(input);
            }
        }

        private void InternalDownload(ChannelSession channel, Stream input, FileSystemInfo fileSystemInfo)
        {
            DateTime modifiedTime = DateTime.Now;
            DateTime accessedTime = DateTime.Now;

            var startDirectoryFullName = fileSystemInfo.FullName;
            var currentDirectoryFullName = startDirectoryFullName;
            var directoryCounter = 0;

            while (true)
            {
                var message = ReadString(input);

                if (message == "E")
                {
                    this.SendConfirmation(channel); //  Send reply

                    directoryCounter--;

                    currentDirectoryFullName = new DirectoryInfo(currentDirectoryFullName).Parent.FullName;

                    if (directoryCounter == 0)
                        break;
                    continue;
                }

                var match = _directoryInfoRe.Match(message);
                if (match.Success)
                {
                    this.SendConfirmation(channel); //  Send reply

                    //  Read directory
                    var mode = long.Parse(match.Result("${mode}"));
                    var filename = match.Result("${filename}");

                    DirectoryInfo newDirectoryInfo;
                    if (directoryCounter > 0)
                    {
                        newDirectoryInfo = Directory.CreateDirectory(string.Format("{0}{1}{2}", currentDirectoryFullName, Path.DirectorySeparatorChar, filename));
                        newDirectoryInfo.LastAccessTime = accessedTime;
                        newDirectoryInfo.LastWriteTime = modifiedTime;
                    }
                    else
                    {
                        //  Dont create directory for first level
                        newDirectoryInfo = fileSystemInfo as DirectoryInfo;
                    }

                    directoryCounter++;

                    currentDirectoryFullName = newDirectoryInfo.FullName;
                    continue;
                }

                match = _fileInfoRe.Match(message);
                if (match.Success)
                {
                    //  Read file
                    this.SendConfirmation(channel); //  Send reply

                    var mode = match.Result("${mode}");
                    var length = long.Parse(match.Result("${length}"));
                    var fileName = match.Result("${filename}");

                    var fileInfo = fileSystemInfo as FileInfo;

                    if (fileInfo == null)
                        fileInfo = new FileInfo(string.Format("{0}{1}{2}", currentDirectoryFullName, Path.DirectorySeparatorChar, fileName));

                    using (var output = fileInfo.OpenWrite())
                    {
                        this.InternalDownload(channel, input, output, fileName, length);
                    }

                    fileInfo.LastAccessTime = accessedTime;
                    fileInfo.LastWriteTime = modifiedTime;

                    if (directoryCounter == 0)
                        break;
                    continue;
                }

                match = _timestampRe.Match(message);
                if (match.Success)
                {
                    //  Read timestamp
                    this.SendConfirmation(channel); //  Send reply

                    var mtime = long.Parse(match.Result("${mtime}"));
                    var atime = long.Parse(match.Result("${atime}"));

                    var zeroTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    modifiedTime = zeroTime.AddSeconds(mtime);
                    accessedTime = zeroTime.AddSeconds(atime);
                    continue;
                }

                this.SendConfirmation(channel, 1, string.Format("\"{0}\" is not valid protocol message.", message));
            }
        }

        partial void SendData(ChannelSession channel, string command)
        {
            channel.SendData(System.Text.Encoding.Default.GetBytes(command));
        }
    }
}
