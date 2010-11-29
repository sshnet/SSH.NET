using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Renci.SshClient.Channels;
using Renci.SshClient.Common;
using Renci.SshClient.Messages.Sftp;

namespace Renci.SshClient
{
    public class Sftp
    {
        private readonly ChannelSession _channel;

        private readonly Session _session;

        private StringBuilder _packetData;

        private uint _requestId;

        private Exception _exception;

        private EventWaitHandle _sftpVersionConfirmed = new AutoResetEvent(false);

        private EventWaitHandle _sessionErrorOccuredWaitHandle = new AutoResetEvent(false);

        private IDictionary<uint, Action<SftpRequestMessage>> _requestActions;

        private string _remoteCurrentDir;

        public int OperationTimeout { get; set; }

        internal Sftp(Session session)
        {
            this.OperationTimeout = -1;
            this._requestActions = new Dictionary<uint, Action<SftpRequestMessage>>();
            this._session = session;
            this._session.ErrorOccured += Session_ErrorOccured;
            this._session.Disconnected += Session_Disconnected;
            this._channel = session.CreateChannel<ChannelSession>();
            this._channel.DataReceived += Channel_DataReceived;
            this._channel.Open();
            this._channel.SendSubsystemRequest("sftp");

            this.SendMessage(new InitMessage
            {
                Version = 3,
            });

            this.WaitHandle(this._sftpVersionConfirmed);

            this.SendMessage(new RealPathMessage
            {
                Path = ".",
            }, (m) =>
            {
                var nameMessage = m as NameMessage;
                if (nameMessage != null)
                {
                    this._remoteCurrentDir = nameMessage.Files.First().Name;
                }
                else
                {
                    throw new InvalidOperationException("");
                }
            });
        }

        public IAsyncResult BeginListDirectory(string path, AsyncCallback callback, object state)
        {
            var asyncResult = new SftpAsyncResult(this._channel, callback, state);

            this.SendMessage(new OpenDirMessage(path), (m) =>
                {
                    var message = m as HandleMessage;
                    if (message == null)
                        throw new InvalidOperationException("Handle message is not expected.");

                    this.SendMessage(new ReadDirMessage(message.Handle), (m1) =>
                    {
                        var message1 = m1 as NameMessage;
                        if (message1 == null)
                            throw new InvalidOperationException("Handle message is not expected.");
                        asyncResult.Names = message1.Files;
                        asyncResult.IsCompleted = true;
                    });
                });

            return asyncResult;


        }

        public IEnumerable<FtpFileInfo> EndListDirectory(IAsyncResult result)
        {
            var r = result as SftpAsyncResult;
            if (r == null)
            {
                throw new ArgumentException("Invalid IAsyncResult parameter.");
            }

            r.AsyncWaitHandle.WaitOne();

            return r.Names;
        }


        private void Channel_DataReceived(object sender, Common.ChannelDataEventArgs e)
        {
            if (this._packetData == null)
            {
                var packetLength = (uint)(e.Data[0] << 24 | e.Data[1] << 16 | e.Data[2] << 8 | e.Data[3]);

                this._packetData = new StringBuilder((int)packetLength, (int)packetLength);
                this._packetData.Append(e.Data.GetSshBytes().Skip(4).GetSshString());
            }
            else
            {
                this._packetData.Append(e.Data);
            }

            if (this._packetData.Length < this._packetData.MaxCapacity)
            {
                //  Wait for more packet data
                return;
            }

            dynamic sftpMessage = SftpMessage.Load(this._packetData.ToString().GetSshBytes());

            this._packetData = null;

            this.HandleMessage(sftpMessage);
        }

        private void HandleMessage(InitMessage message)
        {
            throw new InvalidOperationException("Init message should not be received by client.");
        }

        private void HandleMessage(VersionMessage message)
        {
            if (message.Version == 3)
            {
                this._sftpVersionConfirmed.Set();
            }
            else
            {
                throw new NotSupportedException(string.Format("Server SFTP version {0} is not supported.", message.Version));
            }
        }

        private void HandleMessage(SftpRequestMessage message)
        {
            if (this._requestActions.ContainsKey(message.RequestId))
            {
                var action = this._requestActions[message.RequestId];
                action(message);
                this._requestActions.Remove(message.RequestId);
            }
            else
            {
                throw new InvalidOperationException(string.Format("Request #{0} is invalid.", message.RequestId));
            }
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
        }

        private void Session_ErrorOccured(object sender, ErrorEventArgs e)
        {
            this._exception = e.GetException();

            this._sessionErrorOccuredWaitHandle.Set();
        }

        private void SendMessage(SftpMessage sftpMessage)
        {
            var message = new SftpDataMessage
            {
                LocalChannelNumber = this._channel.RemoteChannelNumber,
                Message = sftpMessage,
            };

            this._session.SendMessage(message);
        }

        private void SendMessage(SftpRequestMessage sftpMessage, Action<SftpRequestMessage> action)
        {
            sftpMessage.RequestId = this._requestId++;

            var message = new SftpDataMessage
            {
                LocalChannelNumber = this._channel.RemoteChannelNumber,
                Message = sftpMessage,
            };

            this._session.SendMessage(message);

            this._requestActions.Add(sftpMessage.RequestId, action);
        }

        private void WaitHandle(WaitHandle waitHandle)
        {
            var waitHandles = new WaitHandle[]
                {
                    this._sessionErrorOccuredWaitHandle,
                    waitHandle,
                };

            var index = EventWaitHandle.WaitAny(waitHandles, this.OperationTimeout);

            if (index < 1)
            {
                throw this._exception;
            }
            else if (index > 1)
            {
                //  throw time out error
                throw new SshOperationTimeoutException(string.Format("Sftp operation has timed out."));
            }
        }


        //public void UploadFile(Stream source, string fileName)
        //{
        //    this.Channel.UploadFile(source, fileName);
        //}

        //public void UploadFile(string source, string fileName)
        //{
        //    using (var sourceFile = File.OpenRead(source))
        //    {
        //        this.Channel.UploadFile(sourceFile, fileName);
        //    }
        //}

        //public void DownloadFile(string fileName, Stream destination)
        //{
        //    this.Channel.DownloadFile(fileName, destination);
        //}

        //public void DownloadFile(string fileName, string destination)
        //{
        //    using (var destinationFile = File.Create(destination))
        //    {
        //        this.Channel.DownloadFile(fileName, destinationFile);
        //    }
        //}

        //public void RemoveFile(string fileName)
        //{
        //    this.Channel.RemoveFile(fileName);
        //}

        //public void RenameFile(string oldFileName, string newFileName)
        //{
        //    this.Channel.RenameFile(oldFileName, newFileName);
        //}

        //public void CreateDirectory(string directoryName)
        //{
        //    this.Channel.CreateDirectory(directoryName);
        //}

        //public void RemoveDirectory(string directoryName)
        //{
        //    this.Channel.RemoveDirectory(directoryName);
        //}
    }
}
