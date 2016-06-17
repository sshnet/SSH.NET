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
        /// <param name="path">The path.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo" /> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="path"/> is null or empty.</exception>
        public void Upload(FileInfo fileInfo, string path)
        {
            if (fileInfo == null)
                throw new ArgumentNullException("fileInfo");
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path");

            using (var input = ServiceFactory.CreatePipeStream())
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += delegate(object sender, ChannelDataEventArgs e)
                {
                    input.Write(e.Data, 0, e.Data.Length);
                    input.Flush();
                };

                channel.Open();

                if (!channel.SendExecRequest(string.Format("scp -t \"{0}\"", path)))
                    throw new SshException("Secure copy execution request was rejected by the server. Please consult the server logs.");

                CheckReturnCode(input);

                InternalUpload(channel, input, fileInfo, fileInfo.Name);

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
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += delegate(object sender, ChannelDataEventArgs e)
                {
                    input.Write(e.Data, 0, e.Data.Length);
                    input.Flush();
                };

                channel.Open();

                //  Send channel command request
                channel.SendExecRequest(string.Format("scp -rt \"{0}\"", path));
                CheckReturnCode(input);

                InternalSetTimestamp(channel, input, directoryInfo.LastWriteTimeUtc, directoryInfo.LastAccessTimeUtc);
                SendData(channel, string.Format("D0755 0 {0}\n", Path.GetFileName(path)));
                CheckReturnCode(input);

                InternalUpload(channel, input, directoryInfo);

                SendData(channel, "E\n");
                CheckReturnCode(input);

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
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += delegate(object sender, ChannelDataEventArgs e)
                {
                    input.Write(e.Data, 0, e.Data.Length);
                    input.Flush();
                };

                channel.Open();

                //  Send channel command request
                channel.SendExecRequest(string.Format("scp -pf \"{0}\"", filename));
                SendConfirmation(channel); //  Send reply

                InternalDownload(channel, input, fileInfo);

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
            using (var channel = Session.CreateChannelSession())
            {
                channel.DataReceived += delegate(object sender, ChannelDataEventArgs e)
                {
                    input.Write(e.Data, 0, e.Data.Length);
                    input.Flush();
                };

                channel.Open();

                //  Send channel command request
                channel.SendExecRequest(string.Format("scp -prf \"{0}\"", directoryName));
                SendConfirmation(channel); //  Send reply

                InternalDownload(channel, input, directoryInfo);

                channel.Close();
            }
        }

        private void InternalUpload(IChannelSession channel, Stream input, FileInfo fileInfo, string filename)
        {
            InternalSetTimestamp(channel, input, fileInfo.LastWriteTimeUtc, fileInfo.LastAccessTimeUtc);
            using (var source = fileInfo.OpenRead())
            {
                InternalUpload(channel, input, source, filename);
            }
        }

        private void InternalUpload(IChannelSession channel, Stream input, DirectoryInfo directoryInfo)
        {
            //  Upload files
            var files = directoryInfo.GetFiles();
            foreach (var file in files)
            {
                InternalUpload(channel, input, file, file.Name);
            }

            //  Upload directories
            var directories = directoryInfo.GetDirectories();
            foreach (var directory in directories)
            {
                InternalSetTimestamp(channel, input, directoryInfo.LastWriteTimeUtc, directoryInfo.LastAccessTimeUtc);
                SendData(channel, string.Format("D0755 0 {0}\n", directory.Name));
                CheckReturnCode(input);

                InternalUpload(channel, input, directory);

                SendData(channel, "E\n");
                CheckReturnCode(input);
            }
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
                    SendConfirmation(channel); //  Send reply

                    directoryCounter--;

                    currentDirectoryFullName = new DirectoryInfo(currentDirectoryFullName).Parent.FullName;

                    if (directoryCounter == 0)
                        break;
                    continue;
                }

                var match = DirectoryInfoRe.Match(message);
                if (match.Success)
                {
                    SendConfirmation(channel); //  Send reply

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

                match = FileInfoRe.Match(message);
                if (match.Success)
                {
                    //  Read file
                    SendConfirmation(channel); //  Send reply

                    var mode = match.Result("${mode}");
                    var length = long.Parse(match.Result("${length}"));
                    var fileName = match.Result("${filename}");

                    var fileInfo = fileSystemInfo as FileInfo;

                    if (fileInfo == null)
                        fileInfo = new FileInfo(string.Format("{0}{1}{2}", currentDirectoryFullName, Path.DirectorySeparatorChar, fileName));

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
                    SendConfirmation(channel); //  Send reply

                    var mtime = long.Parse(match.Result("${mtime}"));
                    var atime = long.Parse(match.Result("${atime}"));

                    var zeroTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    modifiedTime = zeroTime.AddSeconds(mtime);
                    accessedTime = zeroTime.AddSeconds(atime);
                    continue;
                }

                SendConfirmation(channel, 1, string.Format("\"{0}\" is not valid protocol message.", message));
            }
        }
    }
}
