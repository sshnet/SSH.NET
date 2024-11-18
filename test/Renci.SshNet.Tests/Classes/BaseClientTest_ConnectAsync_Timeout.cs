using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

#if !NET8_0_OR_GREATER
using Renci.SshNet.Abstractions;
#endif
using Renci.SshNet.Common;
using Renci.SshNet.Connection;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class BaseClientTest_ConnectAsync_Timeout
    {
        private BaseClient _client;

        [TestInitialize]
        public void Init()
        {
            var sessionMock = new Mock<ISession>();
            var serviceFactoryMock = new Mock<IServiceFactory>();
            var socketFactoryMock = new Mock<ISocketFactory>();

            sessionMock.Setup(p => p.ConnectAsync(It.IsAny<CancellationToken>()))
                .Returns<CancellationToken>(c => Task.Delay(Timeout.Infinite, c));

            serviceFactoryMock.Setup(p => p.CreateSocketFactory())
                                               .Returns(socketFactoryMock.Object);

            var connectionInfo = new ConnectionInfo("host", "user", new PasswordAuthenticationMethod("user", "pwd"))
            {
                Timeout = TimeSpan.FromSeconds(1)
            };

            serviceFactoryMock.Setup(p => p.CreateSession(connectionInfo, socketFactoryMock.Object))
                                   .Returns(sessionMock.Object);

            _client = new MyClient(connectionInfo, false, serviceFactoryMock.Object);
        }

        [TestMethod]
        public async Task ConnectAsyncWithTimeoutThrowsSshTimeoutException()
        {
            await Assert.ThrowsExceptionAsync<SshOperationTimeoutException>(() => _client.ConnectAsync(CancellationToken.None));
        }

        [TestMethod]
        public async Task ConnectAsyncWithCancelledTokenThrowsOperationCancelledException()
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            await cancellationTokenSource.CancelAsync();
            await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => _client.ConnectAsync(cancellationTokenSource.Token));
        }

        [TestCleanup]
        public void Cleanup()
        {
            _client?.Dispose();
        }

        private class MyClient : BaseClient
        {
            public MyClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory) : base(connectionInfo, ownsConnectionInfo, serviceFactory)
            {
            }
        }
    }
}
