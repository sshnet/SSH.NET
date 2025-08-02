using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

#if !NET
using Renci.SshNet.Abstractions;
#endif
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
    [TestClass]
    public class SftpClientTest_AsyncExceptions
    {
        private MySession _session;
        private SftpClient _client;

        [TestInitialize]
        public void Init()
        {
            var socketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
            var serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);

            var connInfo = new PasswordConnectionInfo("host", "user", "pwd");

            _session = new MySession(connInfo);

            var concreteServiceFactory = new ServiceFactory();

            serviceFactoryMock
                .Setup(p => p.CreateSocketFactory())
                .Returns(socketFactoryMock.Object);

            serviceFactoryMock
                .Setup(p => p.CreateSession(It.IsAny<ConnectionInfo>(), socketFactoryMock.Object))
                .Returns(_session);

            serviceFactoryMock
                .Setup(p => p.CreateSftpResponseFactory())
                .Returns(concreteServiceFactory.CreateSftpResponseFactory);

            serviceFactoryMock
                .Setup(p => p.CreateSftpSession(_session, It.IsAny<int>(), It.IsAny<Encoding>(), It.IsAny<ISftpResponseFactory>()))
                .Returns(concreteServiceFactory.CreateSftpSession);

            _client = new SftpClient(connInfo, false, serviceFactoryMock.Object);
            _client.Connect();
        }

        [TestMethod]
        public async Task Async_ObservesSessionDisconnected()
        {
            Task<SftpFileStream> openTask = _client.OpenAsync("path", FileMode.Create, FileAccess.Write, CancellationToken.None);

            Assert.IsFalse(openTask.IsCompleted);

            _session.InvokeDisconnected();

            var ex = await Assert.ThrowsExactlyAsync<SshException>(() => openTask);
            Assert.AreEqual("Connection was closed by the server.", ex.Message);
        }

        [TestMethod]
        public async Task Async_ObservesChannelClosed()
        {
            Task<SftpFileStream> openTask = _client.OpenAsync("path", FileMode.Create, FileAccess.Write, CancellationToken.None);

            Assert.IsFalse(openTask.IsCompleted);

            _session.InvokeChannelCloseReceived();

            var ex = await Assert.ThrowsExactlyAsync<SshException>(() => openTask);
            Assert.AreEqual("Channel was closed.", ex.Message);
        }

        [TestMethod]
        public async Task Async_ObservesCancellationToken()
        {
            using CancellationTokenSource cts = new();

            Task<SftpFileStream> openTask = _client.OpenAsync("path", FileMode.Create, FileAccess.Write, cts.Token);

            Assert.IsFalse(openTask.IsCompleted);

            await cts.CancelAsync();

            var ex = await Assert.ThrowsExactlyAsync<TaskCanceledException>(() => openTask);
            Assert.AreEqual(cts.Token, ex.CancellationToken);
        }

        [TestMethod]
        public async Task Async_ObservesOperationTimeout()
        {
            _client.OperationTimeout = TimeSpan.FromMilliseconds(250);

            Task<SftpFileStream> openTask = _client.OpenAsync("path", FileMode.Create, FileAccess.Write, CancellationToken.None);

            var ex = await Assert.ThrowsExactlyAsync<SshOperationTimeoutException>(() => openTask);
        }

        [TestMethod]
        public async Task Async_ObservesErrorOccurred()
        {
            Task<SftpFileStream> openTask = _client.OpenAsync("path", FileMode.Create, FileAccess.Write, CancellationToken.None);

            Assert.IsFalse(openTask.IsCompleted);

            MyException ex = new("my exception");

            _session.InvokeErrorOccurred(ex);

            var ex2 = await Assert.ThrowsExactlyAsync<MyException>(() => openTask);
            Assert.AreEqual(ex.Message, ex2.Message);
        }

#pragma warning disable IDE0022 // Use block body for method
#pragma warning disable IDE0025 // Use block body for property
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

            public void InvokeDisconnected()
            {
                Disconnected?.Invoke(this, new EventArgs());
            }

            public void InvokeChannelCloseReceived()
            {
                ChannelCloseReceived?.Invoke(
                    this,
                    new MessageEventArgs<ChannelCloseMessage>(new ChannelCloseMessage(0)));
            }

            public void InvokeErrorOccurred(Exception ex)
            {
                ErrorOccured?.Invoke(this, new ExceptionEventArgs(ex));
            }

            public void SendMessage(Message message)
            {
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
                                        ResponseId = 1,
                                        Files = [new("thepath", new SftpFileAttributes(default, default, default, default, default, default, default))]
                                    }.GetBytes())));
                    }
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

        [TestCleanup]
        public void Cleanup() => _client?.Dispose();

#pragma warning disable
        private class MyException : Exception
        {
            public MyException(string message) : base(message)
            {
            }
        }
    }
}
