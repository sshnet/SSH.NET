using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Connection;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Sftp;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Tests.Classes
{
    public partial class SftpClientTest
    {
        [TestMethod]
        public void UploadFile_ObservesErrorResponses()
        {
            // A regression test for UploadFile hanging instead of observing
            // error responses from the server.
            // https://github.com/sshnet/SSH.NET/issues/957

            var serviceFactoryMock = new Mock<IServiceFactory>();

            var connInfo = new PasswordConnectionInfo("host", "user", "pwd");

            var session = new MySession(connInfo);

            var concreteServiceFactory = new ServiceFactory();

            serviceFactoryMock
                .Setup(p => p.CreateSession(It.IsAny<ConnectionInfo>(), It.IsAny<ISocketFactory>()))
                .Returns(session);

            serviceFactoryMock
                .Setup(p => p.CreateSftpResponseFactory())
                .Returns(concreteServiceFactory.CreateSftpResponseFactory);

            serviceFactoryMock
                .Setup(p => p.CreateSftpSession(session, It.IsAny<int>(), It.IsAny<Encoding>(), It.IsAny<ISftpResponseFactory>()))
                .Returns(concreteServiceFactory.CreateSftpSession);

            using var client = new SftpClient(connInfo, false, serviceFactoryMock.Object);
            client.Connect();

            Assert.Throws<SftpPermissionDeniedException>(() => client.UploadFile(
                new OneByteStream(new MemoryStream("Hello World"u8.ToArray())),
                "path.txt"));
        }

#pragma warning disable IDE0022 // Use block body for method
#pragma warning disable IDE0025 // Use block body for property
#pragma warning disable IDE0027 // Use block body for accessor
#pragma warning disable CS0067 // event is unused

        private class MySession(ConnectionInfo connectionInfo) : ISession
        {
            public IConnectionInfo ConnectionInfo => connectionInfo;

            public event EventHandler<MessageEventArgs<ChannelCloseMessage>> ChannelCloseReceived;
            public event EventHandler<MessageEventArgs<ChannelDataMessage>> ChannelDataReceived;
            public event EventHandler<MessageEventArgs<ChannelEofMessage>> ChannelEofReceived;
            public event EventHandler<MessageEventArgs<ChannelExtendedDataMessage>> ChannelExtendedDataReceived;
            public event EventHandler<MessageEventArgs<ChannelFailureMessage>> ChannelFailureReceived;
            public event EventHandler<MessageEventArgs<ChannelOpenConfirmationMessage>> ChannelOpenConfirmationReceived;
            public event EventHandler<MessageEventArgs<ChannelOpenFailureMessage>> ChannelOpenFailureReceived;
            public event EventHandler<MessageEventArgs<ChannelOpenMessage>> ChannelOpenReceived;
            public event EventHandler<MessageEventArgs<ChannelRequestMessage>> ChannelRequestReceived;
            public event EventHandler<MessageEventArgs<ChannelSuccessMessage>> ChannelSuccessReceived;
            public event EventHandler<MessageEventArgs<ChannelWindowAdjustMessage>> ChannelWindowAdjustReceived;
            public event EventHandler<EventArgs> Disconnected;
            public event EventHandler<ExceptionEventArgs> ErrorOccured;
            public event EventHandler<SshIdentificationEventArgs> ServerIdentificationReceived;
            public event EventHandler<HostKeyEventArgs> HostKeyReceived;
            public event EventHandler<MessageEventArgs<RequestSuccessMessage>> RequestSuccessReceived;
            public event EventHandler<MessageEventArgs<RequestFailureMessage>> RequestFailureReceived;
            public event EventHandler<MessageEventArgs<BannerMessage>> UserAuthenticationBannerReceived;

            private uint _numRequests;
            private int _numWriteRequests;

            public void SendMessage(Message message)
            {
                // Initialisation sequence for SFTP session

                if (message is ChannelOpenMessage)
                {
                    ChannelOpenConfirmationReceived?.Invoke(
                        this,
                        new MessageEventArgs<ChannelOpenConfirmationMessage>(
                            new ChannelOpenConfirmationMessage(0, int.MaxValue, int.MaxValue, 0)));
                }
                else if (message is ChannelRequestMessage)
                {
                    ChannelSuccessReceived?.Invoke(
                        this,
                        new MessageEventArgs<ChannelSuccessMessage>(new ChannelSuccessMessage(0)));
                }
                else if (message is ChannelDataMessage dataMsg)
                {
                    if (dataMsg.Data[sizeof(uint)] == (byte)SftpMessageTypes.Init)
                    {
                        ChannelDataReceived?.Invoke(
                            this,
                            new MessageEventArgs<ChannelDataMessage>(
                                new ChannelDataMessage(0, new SftpVersionResponse() { Version = 3 }.GetBytes())));
                    }
                    else if (dataMsg.Data[sizeof(uint)] == (byte)SftpMessageTypes.RealPath)
                    {
                        ChannelDataReceived?.Invoke(
                            this,
                            new MessageEventArgs<ChannelDataMessage>(
                                new ChannelDataMessage(0,
                                    new SftpNameResponse(3, Encoding.UTF8)
                                    {
                                        ResponseId = ++_numRequests,
                                        Files = [new("thepath", new SftpFileAttributes(default, default, default, default, default, default, default))]
                                    }.GetBytes())));
                    }
                    else if (dataMsg.Data[sizeof(uint)] == (byte)SftpMessageTypes.Open)
                    {
                        ChannelDataReceived?.Invoke(
                            this,
                            new MessageEventArgs<ChannelDataMessage>(
                                new ChannelDataMessage(0,
                                    new SftpHandleResponse(3)
                                    {
                                        ResponseId = ++_numRequests,
                                        Handle = "file"u8.ToArray()
                                    }.GetBytes())));
                    }

                    // --------- The actual interesting part of all of this ---------
                    //
                    else if (dataMsg.Data[sizeof(uint)] == (byte)SftpMessageTypes.Write)
                    {
                        // Fail the 5th write request
                        var statusCode = ++_numWriteRequests == 5 ? StatusCode.PermissionDenied : StatusCode.Ok;
                        var responseId = ++_numRequests;

                        // Dispatch the responses on a different thread to simulate reality.
                        _ = Task.Run(() =>
                        {
                            ChannelDataReceived?.Invoke(
                                this,
                                new MessageEventArgs<ChannelDataMessage>(
                                    new ChannelDataMessage(0,
                                        new SftpStatusResponse(3)
                                        {
                                            ResponseId = responseId,
                                            StatusCode = statusCode
                                        }.GetBytes())));
                        });
                    }
                    //
                    // --------------------------------------------------------------
                }
            }

            public bool IsConnected => false;

            public SemaphoreSlim SessionSemaphore { get; } = new(1);

            public IChannelSession CreateChannelSession() => new ChannelSession(this, 0, int.MaxValue, int.MaxValue);

            public WaitHandle MessageListenerCompleted => throw new NotImplementedException();

            public ILoggerFactory SessionLoggerFactory => NullLoggerFactory.Instance;

            public void Connect()
            {
            }

            public Task ConnectAsync(CancellationToken cancellationToken) => throw new NotImplementedException();

            public IChannelDirectTcpip CreateChannelDirectTcpip() => throw new NotImplementedException();

            public IChannelForwardedTcpip CreateChannelForwardedTcpip(uint remoteChannelNumber, uint remoteWindowSize, uint remoteChannelDataPacketSize)
                => throw new NotImplementedException();

            public void Dispose()
            {
            }

            public void OnDisconnecting()
            {
            }

            public void Disconnect() => throw new NotImplementedException();

            public void RegisterMessage(string messageName) => throw new NotImplementedException();

            public bool TrySendMessage(Message message) => throw new NotImplementedException();

            public WaitResult TryWait(WaitHandle waitHandle, TimeSpan timeout, out Exception exception) => throw new NotImplementedException();

            public WaitResult TryWait(WaitHandle waitHandle, TimeSpan timeout) => throw new NotImplementedException();

            public void UnRegisterMessage(string messageName) => throw new NotImplementedException();

            public void WaitOnHandle(WaitHandle waitHandle)
            {
            }

            public void WaitOnHandle(WaitHandle waitHandle, TimeSpan timeout) => throw new NotImplementedException();
        }

        private class OneByteStream : Stream
        {
            private readonly Stream _stream;

            public OneByteStream(Stream stream)
            {
                _stream = stream;
            }

            public override bool CanRead => _stream.CanRead;

            public override bool CanSeek => throw new NotImplementedException();

            public override bool CanWrite => throw new NotImplementedException();

            public override long Length => _stream.Length;

            public override long Position
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public override void Flush() => throw new NotImplementedException();

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _stream.Read(buffer, offset, Math.Min(1, count));
            }

            public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

            public override void SetLength(long value) => throw new NotImplementedException();

            public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
        }
    }
}
