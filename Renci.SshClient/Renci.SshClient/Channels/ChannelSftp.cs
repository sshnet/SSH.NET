using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Renci.SshClient.Common;
using Renci.SshClient.Messages.Connection;
using Renci.SshClient.Messages.Sftp;

namespace Renci.SshClient.Channels
{
    internal class ChannelSftp : Channel
    {
        private EventWaitHandle _channelRequestSuccessWaitHandle = new AutoResetEvent(false);

        private EventWaitHandle _testWaitHandle = new AutoResetEvent(false);

        private EventWaitHandle _responseMessageReceivedWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        private uint _requestId;

        private SftpMessage _responseMessage;

        private string _remoteCurrentDir;

        private string _localCurentDir;

        private StringBuilder _packetData;

        public override ChannelTypes ChannelType
        {
            get { return ChannelTypes.Session; }
        }

        public ChannelSftp(SessionInfo sessionInfo, uint windowSize, uint packetSize)
            : base(sessionInfo, windowSize, packetSize)
        {
        }

        public ChannelSftp(SessionInfo sessionInfo)
            : base(sessionInfo, 0x100000, 0x4000)
        {
        }

        public override void Open()
        {
            base.Open();

            //  Send channel command request
            this.SendMessage(new ChannelRequestMessage
            {
                ChannelNumber = this.ServerChannelNumber,
                RequestName = RequestNames.Subsystem,
                WantReply = true,
                SubsystemName = "sftp",
            });

            this.SessionInfo.WaitHandle(this._channelRequestSuccessWaitHandle);

            this.SendMessage(new InitMessage
            {
                Version = 6,
            });

            var versionMessage = this.ReceiveMessage<VersionMessage>();

            if (versionMessage == null)
            {
                throw new InvalidOperationException("Version message expected.");
            }

            if (versionMessage.Version != 3)
            {
                throw new NotSupportedException(string.Format("Server SFTP version {0} is not supported.", versionMessage.Version));
            }

            //  Get default current directories
            var files = this.GetRealPath(".");

            this._remoteCurrentDir = files.First().Name;
            this._localCurentDir = Directory.GetCurrentDirectory();
        }

        public void UploadFile(Stream source, string destination)
        {
            this.Open();

            string handle = string.Empty;

            try
            {
                handle = this.OpenRemoteFile(destination, Flags.Write | Flags.CreateNewOrOpen | Flags.Truncate);

                var buffer = new byte[1024];
                ulong offset = 0;
                while (source.Read(buffer, 0, buffer.Length) > 0)
                {
                    this.RemoteWrite(handle, offset, buffer.GetSshString());
                    offset += (ulong)buffer.Length;
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(handle))
                    this.CloseRemoteHandle(handle);
            }

            this.Close();
        }

        internal void DownloadFile(string fileName, Stream destination)
        {
            this.Open();

            string handle = string.Empty;

            try
            {
                handle = this.OpenRemoteFile(fileName, Flags.Read);

                ulong offset = 0;
                uint bufferSize = 1024;
                string data;

                while ((data = this.RemoteRead(handle, offset, bufferSize)) != null)
                {
                    var fileData = data.GetSshBytes().ToArray();
                    destination.Write(fileData, 0, (int)bufferSize);
                    destination.Flush();
                    offset += (ulong)fileData.Length;
                }

            }
            finally
            {
                if (!string.IsNullOrEmpty(handle))
                    this.CloseRemoteHandle(handle);
            }

            this.Close();
        }

        public void CreateDirectory(string directoryName)
        {
            this.Open();

            this.CreateRemoteDirectory(directoryName);

            this.Close();
        }

        public void RemoveDirectory(string directoryName)
        {
            this.Open();

            this.RemoveRemoteDirectory(directoryName);

            this.Close();
        }

        public void RemoveFile(string fileName)
        {
            this.Open();

            this.RemoveRemoteFile(fileName);

            this.Close();
        }

        public void RenameFile(string oldFileName, string newFileName)
        {
            this.Open();

            this.RenameRemoteFile(oldFileName, newFileName);

            this.Close();
        }

        public IEnumerable<FtpFileInfo> ListDirectory(string path)
        {
            //  Open channel
            this.Open();

            string handle = string.Empty;
            IEnumerable<FtpFileInfo> files = null;

            try
            {
                //  Open directory
                handle = this.OpenRemoteDirectory(path);

                //  Read directory data
                files = this.ReadRemoteDirectory(handle);
            }
            finally
            {
                //  Close directory
                if (!string.IsNullOrEmpty(handle))
                    this.CloseRemoteHandle(handle);
            }

            //  Read directory
            this.Close();

            return files;

        }

        protected override void OnChannelSuccess()
        {
            base.OnChannelSuccess();

            this._channelRequestSuccessWaitHandle.Set();
        }

        protected override void OnChannelData(string data)
        {
            base.OnChannelData(data);

            if (this._packetData == null)
            {
                var packetLength = BitConverter.ToUInt32(data.GetSshBytes().Take(4).Reverse().ToArray(), 0);
                this._packetData = new StringBuilder((int)packetLength, (int)packetLength);
                this._packetData.Append(data.GetSshBytes().Skip(4).GetSshString());
            }
            else
            {
                this._packetData.Append(data);
            }


            if (this._packetData.Length < this._packetData.MaxCapacity)
            {
                //  Wait for more packet data
                return;
            }


            dynamic sftpMessage = SftpMessage.Load(this._packetData.ToString().GetSshBytes());

            this._packetData = null;

            //  TODO:   Handle SSH_FXP_STATUS here
            //  TODO:   Validate message request id is correct

            this._responseMessage = sftpMessage;

            this._responseMessageReceivedWaitHandle.Set();
        }

        private T ReceiveMessage<T>() where T : SftpMessage
        {
            var message = this.ReceiveMessage() as T;

            if (message == null)
            {
                throw new InvalidOperationException(string.Format("Message of type '{0}' expected in this context.", typeof(T).Name));
            }
            return message;

        }

        private SftpMessage ReceiveMessage()
        {
            this.SessionInfo.WaitHandle(this._responseMessageReceivedWaitHandle);

            var statusMessage = this._responseMessage as StatusMessage;

            if (statusMessage != null)
            {
                //  Handle error status messages
                switch (statusMessage.StatusCode)
                {
                    case StatusCodes.Ok:
                        break;
                    case StatusCodes.Eof:
                        break;
                    case StatusCodes.NoSuchFile:
                        throw new FileNotFoundException("File or directory not found on the remote server.");
                    case StatusCodes.PermissionDenied:
                        throw new NotImplementedException();
                    case StatusCodes.Failure:
                        throw new InvalidOperationException("Operation failed.");
                    case StatusCodes.BadMessage:
                        throw new NotImplementedException();
                    case StatusCodes.NoConnection:
                        throw new NotImplementedException();
                    case StatusCodes.ConnectionLost:
                        throw new NotImplementedException();
                    case StatusCodes.OperationUnsupported:
                        throw new NotSupportedException("Operation is not supported.");
                    default:
                        break;
                }
            }

            return this._responseMessage;
        }

        private void SendMessage(SftpMessage sftpMessage)
        {
            sftpMessage.RequestId = this._requestId++;
            var message = new SftpDataMessage
            {
                ChannelNumber = this.ServerChannelNumber,
                Data = sftpMessage,
            };

            this.SendMessage(message);

            this._responseMessageReceivedWaitHandle.Reset();
        }

        private string OpenRemoteFile(string fileName, Flags flags)
        {
            this.SendMessage(new OpenMessage
            {
                Filename = fileName,
                Flags = flags,
            });

            var handleMessage = this.ReceiveMessage<HandleMessage>();

            return handleMessage.Handle;
        }

        private string RemoteRead(string handle, ulong offset, uint length)
        {
            this.SendMessage(new ReadMessage
            {
                Handle = handle,
                Offset = offset,
                Length = length,
            });

            var message = this.ReceiveMessage();

            var statusMessage = message as StatusMessage;
            var dataMessage = message as DataMessage;

            if (statusMessage != null)
            {
                if (statusMessage.StatusCode == StatusCodes.Eof)
                {
                    return null;
                }

                throw new InvalidOperationException("Invalid status code.");
            }
            else if (dataMessage != null)
            {
                return dataMessage.Data;
            }
            else
            {
                throw new InvalidOperationException(string.Format("Message type '{0}' is not valid in this context.", message.SftpMessageType));
            }
        }

        private void RemoteWrite(string handle, ulong offset, string data)
        {
            this.SendMessage(new WriteMessage
            {
                Handle = handle,
                Offset = offset,
                Data = data,
            });

            var message = this.ReceiveMessage<StatusMessage>();

            this.EnsureStatusCode(message, StatusCodes.Ok);
        }

        private void RemoveRemoteFile(string fileName)
        {
            this.SendMessage(new RemoveMessage
            {
                Filename = fileName,
            });

            var message = this.ReceiveMessage<StatusMessage>();

            this.EnsureStatusCode(message, StatusCodes.Ok);
        }

        private void RenameRemoteFile(string oldFileName, string newFileName)
        {
            this.SendMessage(new RenameMessage
            {
                OldPath = oldFileName,
                NewPath = newFileName,
            });

            var message = this.ReceiveMessage<StatusMessage>();

            this.EnsureStatusCode(message, StatusCodes.Ok);
        }

        private void CreateRemoteDirectory(string directoryName)
        {
            this.SendMessage(new MkDirMessage
            {
                Path = directoryName,
            });

            var message = this.ReceiveMessage<StatusMessage>();

            this.EnsureStatusCode(message, StatusCodes.Ok);
        }

        private void RemoveRemoteDirectory(string directoryName)
        {
            this.SendMessage(new RmDirMessage
            {
                Path = directoryName,
            });

            var message = this.ReceiveMessage<StatusMessage>();

            this.EnsureStatusCode(message, StatusCodes.Ok);
        }

        private string OpenRemoteDirectory(string path)
        {
            this.SendMessage(new OpenDirMessage
            {
                Path = path,
            });

            var handleMessage = this.ReceiveMessage<HandleMessage>();

            return handleMessage.Handle;
        }

        private IEnumerable<FtpFileInfo> ReadRemoteDirectory(string handle)
        {
            this.SendMessage(new ReadDirMessage
            {
                Handle = handle,
            });

            var message = this.ReceiveMessage<NameMessage>();

            return message.Files;
        }

        private void CloseRemoteHandle(string handle)
        {
            this.SendMessage(new CloseMessage
            {
                Handle = handle,
            });

            var status = this.ReceiveMessage<StatusMessage>();
            //  TODO:   If close is fails wait a litle a try to close it again, in case server fluashed data into the file during close
        }

        private Attributes GetRemoteFileAttributes(string filename)
        {
            this.SendMessage(new StatMessage
            {
                Path = filename,
            });

            var message = this.ReceiveMessage<AttrsMessage>();

            return message.Attributes;
        }

        private Attributes GetRemoteLinkFileAttributes(string filename)
        {
            this.SendMessage(new LStatMessage
            {
                Path = filename,
            });

            var message = this.ReceiveMessage<AttrsMessage>();

            return message.Attributes;
        }

        private Attributes GetRemoteOpenFileAttributes(string handle)
        {
            this.SendMessage(new FStatMessage
            {
                Handle = handle,
            });

            var message = this.ReceiveMessage<AttrsMessage>();

            return message.Attributes;
        }

        private void SetRemoteFileAttributes(string filename, Attributes attributes)
        {
            this.SendMessage(new SetStatMessage
            {
                Path = filename,
                Attributes = attributes
            });

            var message = this.ReceiveMessage<StatusMessage>();

            this.EnsureStatusCode(message, StatusCodes.Ok);
        }

        private void SetRemoteOpenFileAttributes(string handle, Attributes attributes)
        {
            this.SendMessage(new FSetStatMessage
            {
                Handle = handle,
                Attributes = attributes
            });

            var message = this.ReceiveMessage<StatusMessage>();

            this.EnsureStatusCode(message, StatusCodes.Ok);
        }

        private IEnumerable<FtpFileInfo> GetRealPath(string path)
        {
            this.SendMessage(new RealPathMessage
            {
                Path = path,
            });

            var message = this.ReceiveMessage<NameMessage>();

            return message.Files;

        }

        private void EnsureStatusCode(StatusMessage message, StatusCodes code)
        {
            if (message.StatusCode == code)
            {
                return;
            }
            else
            {
                throw new InvalidOperationException("Invalid status code.");
            }
        }

    }
}
