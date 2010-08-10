
using System.Collections.Generic;
using System.IO;
using Renci.SshClient.Channels;
using Renci.SshClient.Common;

namespace Renci.SshClient
{
    public class Sftp
    {
        private Session _session;

        private ChannelSftp _channel;

        internal Sftp(Session session)
        {
            this._session = session;

            //this._channel = new ChannelSftp(this._session);
            this._channel = this._session.CreateChannel<ChannelSftp>();
        }


        public IEnumerable<FtpFileInfo> ListDirectory(string path)
        {
            return this._channel.ListDirectory(path);
        }

        public void UploadFile(Stream source, string fileName)
        {
            this._channel.UploadFile(source, fileName);
        }

        public void UploadFile(string source, string fileName)
        {
            this._channel.UploadFile(File.OpenRead(source), fileName);
        }

        public void DownloadFile(string fileName, Stream destination)
        {
            this._channel.DownloadFile(fileName, destination);
        }

        public void DownloadFile(string fileName, string destination)
        {
            var file = File.Create(destination);
            this._channel.DownloadFile(fileName, file);
        }

        public void RemoveFile(string fileName)
        {
            this._channel.RemoveFile(fileName);
        }

        public void RenameFile(string oldFileName, string newFileName)
        {
            this._channel.RenameFile(oldFileName, newFileName);
        }

        public void CreateDirectory(string directoryName)
        {
            this._channel.CreateDirectory(directoryName);
        }

        public void RemoveDirectory(string directoryName)
        {
            this._channel.RemoveDirectory(directoryName);
        }
    }
}
