using System.Collections.Generic;
using System.IO;
using Renci.SshClient.Channels;
using Renci.SshClient.Common;

namespace Renci.SshClient
{
    public class Sftp
    {
        private ChannelSessionSftp _channel;

        private ChannelSessionSftp Channel
        {
            get
            {
                if (this._channel == null)
                {
                    this._channel = this._session.CreateChannel<ChannelSessionSftp>();
                }
                return this._channel;
            }
        }

        private readonly Session _session;

        internal Sftp(Session session)
        {
            this._session = session;
        }

        public IEnumerable<FtpFileInfo> ListDirectory(string path)
        {
            return this.Channel.ListDirectory(path);
        }

        public void UploadFile(Stream source, string fileName)
        {
            this.Channel.UploadFile(source, fileName);
        }

        public void UploadFile(string source, string fileName)
        {
            using (var sourceFile = File.OpenRead(source))
            {
                this.Channel.UploadFile(sourceFile, fileName);
            }
        }

        public void DownloadFile(string fileName, Stream destination)
        {
            this.Channel.DownloadFile(fileName, destination);
        }

        public void DownloadFile(string fileName, string destination)
        {
            using (var destinationFile = File.Create(destination))
            {
                this.Channel.DownloadFile(fileName, destinationFile);
            }
        }

        public void RemoveFile(string fileName)
        {
            this.Channel.RemoveFile(fileName);
        }

        public void RenameFile(string oldFileName, string newFileName)
        {
            this.Channel.RenameFile(oldFileName, newFileName);
        }

        public void CreateDirectory(string directoryName)
        {
            this.Channel.CreateDirectory(directoryName);
        }

        public void RemoveDirectory(string directoryName)
        {
            this.Channel.RemoveDirectory(directoryName);
        }
    }
}
