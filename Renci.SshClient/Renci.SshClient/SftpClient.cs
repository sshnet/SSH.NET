using System;
using System.Collections.Generic;
using System.IO;
using Renci.SshClient.Sftp;

namespace Renci.SshClient
{
    public class SftpClient
    {
        private SftpSession _sftpSession;

        private Session _session;

        public ConnectionInfo ConnectionInfo { get; private set; }

        public bool IsConnected
        {
            get
            {
                if (this._session == null)
                {
                    return false;
                }
                else
                {
                    return this._session.IsConnected;
                }
            }
        }

        public int OperationTimeout { get; set; }

        public int BufferSize { get; set; }

        #region Constructors

        public SftpClient(ConnectionInfo connectionInfo)
        {
            this.OperationTimeout = -1;

            this.ConnectionInfo = connectionInfo;
        }

        public SftpClient(string host, int port, string username, string password)
            : this(new ConnectionInfo
            {
                Host = host,
                Port = port,
                Username = username,
                Password = password,
            })
        {
        }

        public SftpClient(string host, string username, string password)
            : this(new ConnectionInfo
            {
                Host = host,
                Username = username,
                Password = password,
            })
        {
        }

        public SftpClient(string host, int port, string username, PrivateKeyFile keyFile)
            : this(new ConnectionInfo
            {
                Host = host,
                Port = port,
                Username = username,
                KeyFile = keyFile,
            })
        {
        }

        public SftpClient(string host, string username, PrivateKeyFile keyFile)
            : this(new ConnectionInfo
            {
                Host = host,
                Username = username,
                KeyFile = keyFile,
            })
        {
        }

        #endregion

        public void Connect()
        {
            if (this._session == null)
            {
                this._session = new Session(this.ConnectionInfo);
            }
            this._session.Connect();

            this._sftpSession = new SftpSession(this._session, this.OperationTimeout);

            this._sftpSession.Connect();
        }

        public void Disconnect()
        {
            this._sftpSession.Disconnect();

            this._session.Disconnect();

            this._session = null;
        }

        #region List Directory

        public IAsyncResult BeginListDirectory(string path, AsyncCallback asyncCallback, object state)
        {
            var cmd = new ListDirectoryCommand(this._sftpSession, path);
            return cmd.BeginExecute(asyncCallback, state);
        }

        public IEnumerable<SftpFile> EndListDirectory(IAsyncResult asyncResult)
        {
            var sftpAsyncResult = asyncResult as SftpAsyncResult;

            if (sftpAsyncResult == null)
            {
                throw new InvalidOperationException("Not valid IAsyncResult object.");
            }

            sftpAsyncResult.Command.EndExecute(sftpAsyncResult);

            var cmd = sftpAsyncResult.Command as ListDirectoryCommand;

            if (cmd == null)
            {
                throw new InvalidOperationException("Not valid IAsyncResult object.");
            }
            return cmd.Files;
        }

        public IEnumerable<SftpFile> ListDirectory(string path)
        {
            return this.EndListDirectory(this.BeginListDirectory(path, null, null));
        }

        #endregion

        public void RenameFile(string oldFilename, string newFileName)
        {
        }

        public void RemoveFile(string filename)
        {
        }

        public void RemoveDirectory(string path)
        {
        }

        public void CreateDirectory(string path)
        {
        }

        #region Upload File

        public IAsyncResult BeginUploadFile(string filename, Stream input, AsyncCallback asyncCallback, object state)
        {
            var cmd = new UploadFileCommand(this._sftpSession, filename, input);

            return cmd.BeginExecute(asyncCallback, state);
        }

        public void EndUploadFile(IAsyncResult asyncResult)
        {
            var sftpAsyncResult = asyncResult as SftpAsyncResult;

            if (sftpAsyncResult == null)
            {
                throw new InvalidOperationException("Not valid IAsyncResult object.");
            }

            sftpAsyncResult.Command.EndExecute(sftpAsyncResult);

            var cmd = sftpAsyncResult.Command as UploadFileCommand;

            if (cmd == null)
            {
                throw new InvalidOperationException("Not valid IAsyncResult object.");
            }
        }

        public void UploadFile(string filename, Stream input)
        {
            this.EndUploadFile(this.BeginUploadFile(filename, input, null, null));
        }

        #endregion

        #region Download File

        public IAsyncResult BeginDownload(string filename, Stream output, AsyncCallback asyncCallback, object state)
        {
            var cmd = new DownloadFileCommand(this._sftpSession, filename, output);

            return cmd.BeginExecute(asyncCallback, state);
        }

        public void EndDownload(IAsyncResult asyncResult)
        {
            var sftpAsyncResult = asyncResult as SftpAsyncResult;

            if (sftpAsyncResult == null)
            {
                throw new InvalidOperationException("Not valid IAsyncResult object.");
            }

            sftpAsyncResult.Command.EndExecute(sftpAsyncResult);

            var cmd = sftpAsyncResult.Command as DownloadFileCommand;

            if (cmd == null)
            {
                throw new InvalidOperationException("Not valid IAsyncResult object.");
            }
        }

        public void Download(string filename, Stream output)
        {
            this.EndDownload(this.BeginDownload(filename, output, null, null));
        }

        #endregion

        public void ChangeDirectory(string path)
        {
        }
    }
}