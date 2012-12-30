using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Channels;
using System.IO;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides SCP client functionality.
    /// </summary>
    public partial class ScpClient
    {
        /// <summary>
        /// Uploads the specified file to the remote host.
        /// </summary>
        /// <param name="fileInfo">Local file to upload.</param>
        /// <param name="filename">Remote host file name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo"/> or <paramref name="filename"/> is null.</exception>
        public void Upload(FileInfo fileInfo, string filename)
        {
            if (fileInfo == null)
                throw new ArgumentNullException("fileInfo");

            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("filename");

            //  Ensure that connection is established.
            this.EnsureConnection();

            using (var input = new PipeStream())
            using (var channel = this.Session.CreateChannel<ChannelSession>())
            {
                channel.DataReceived += delegate(object sender, Common.ChannelDataEventArgs e)
                {
                    input.Write(e.Data, 0, e.Data.Length);
                    input.Flush();
                };

                channel.Open();

                //  Send channel command request
                channel.SendExecRequest(string.Format("scp -t \"{0}\"", filename));

                this.CheckReturnCode(input);

                this.InternalUpload(channel, input, fileInfo, filename);

                channel.Close();
            }
        }

        /// <summary>
        /// Uploads the specified directory to the remote host.
        /// </summary>
        /// <param name="directoryInfo">Local directory to upload.</param>
        /// <param name="filename">Remote host directory name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="directoryInfo"/> or <paramref name="filename"/> is null.</exception>
        public void Upload(DirectoryInfo directoryInfo, string filename)
        {
            if (directoryInfo == null)
                throw new ArgumentNullException("directoryInfo");

            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("filename");

            //  Ensure that connection is established.
            this.EnsureConnection();

            using (var input = new PipeStream())
            using (var channel = this.Session.CreateChannel<ChannelSession>())
            {
                channel.DataReceived += delegate(object sender, Common.ChannelDataEventArgs e)
                {
                    input.Write(e.Data, 0, e.Data.Length);
                    input.Flush();
                };

                channel.Open();

                //  Send channel command request
                channel.SendExecRequest(string.Format("scp -rt \"{0}\"", filename));
                this.CheckReturnCode(input);

                this.InternalUpload(channel, input, directoryInfo, filename);

                channel.Close();
            }
        }

        /// <summary>
        /// Downloads the specified file from the remote host to local file.
        /// </summary>
        /// <param name="filename">Remote host file name.</param>
        /// <param name="fileInfo">Local file information.</param>
        /// <exception cref="ArgumentNullException"><paramref name="fileInfo"/> or <paramref name="filename"/> is null.</exception>
        public void Download(string filename, FileInfo fileInfo)
        {
            if (fileInfo == null)
                throw new ArgumentNullException("fileInfo");

            if (string.IsNullOrEmpty(filename))
                throw new ArgumentException("filename");

            //  Ensure that connection is established.
            this.EnsureConnection();

            using (var input = new PipeStream())
            using (var channel = this.Session.CreateChannel<ChannelSession>())
            {
                channel.DataReceived += delegate(object sender, Common.ChannelDataEventArgs e)
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
        /// <exception cref="ArgumentNullException"><paramref name="directoryInfo"/> or <paramref name="directoryName"/> is null.</exception>
        public void Download(string directoryName, DirectoryInfo directoryInfo)
        {
            if (directoryInfo == null)
                throw new ArgumentNullException("directoryInfo");

            if (string.IsNullOrEmpty(directoryName))
                throw new ArgumentException("directoryName");

            //  Ensure that connection is established.
            this.EnsureConnection();

            using (var input = new PipeStream())
            using (var channel = this.Session.CreateChannel<ChannelSession>())
            {
                channel.DataReceived += delegate(object sender, Common.ChannelDataEventArgs e)
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
                this.InternalFileUpload(channel, input, source, filename);
            }
        }

        private void InternalUpload(ChannelSession channel, PipeStream input, DirectoryInfo directoryInfo, string directoryName)
        {
            this.InternalSetTimestamp(channel, input, directoryInfo.LastWriteTimeUtc, directoryInfo.LastAccessTimeUtc);

            this.SendData(channel, string.Format("D0755 0 {0}\n", directoryName));
            this.CheckReturnCode(input);

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
                this.InternalUpload(channel, input, directory, directory.Name);
            }

            this.SendData(channel, "E\n");
            this.CheckReturnCode(input);
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

                    //if (currentDirectoryFullName == startDirectoryFullName)
                    if (directoryCounter == 0)
                        break;
                    else
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
                        this.InternalFileDownload(channel, input, output, fileName, length);
                    }

                    fileInfo.LastAccessTime = accessedTime;
                    fileInfo.LastWriteTime = modifiedTime;

                    if (fileSystemInfo is FileInfo)
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
