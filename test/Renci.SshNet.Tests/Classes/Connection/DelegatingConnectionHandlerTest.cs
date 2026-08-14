using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Connection;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class DelegatingConnectionHandlerTest
    {
        private FakeConnectionHandler _innerHandler;
        private TestableDelegatingConnectionHandler _handler;
        private ConnectionInfo _connectionInfo;

        [TestInitialize]
        public void Setup()
        {
            _innerHandler = new FakeConnectionHandler();
            _handler = new TestableDelegatingConnectionHandler(_innerHandler);
            _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("user"));
        }

        [TestCleanup]
        public void TearDown()
        {
            _innerHandler.SocketToReturn.Dispose();
        }

        [TestMethod]
        public void ConstructorShouldThrowArgumentNullExceptionWhenInnerHandlerIsNull()
        {
            _ = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new TestableDelegatingConnectionHandler(innerHandler: null));
        }

        [TestMethod]
        public void ConnectShouldDelegateToInnerHandler()
        {
            var actual = _handler.Connect(_connectionInfo);

            Assert.AreSame(_innerHandler.SocketToReturn, actual);
            Assert.AreEqual(1, _innerHandler.ConnectCallCount);
        }

        [TestMethod]
        public async Task ConnectAsyncShouldDelegateToInnerHandler()
        {
            var actual = await _handler.ConnectAsync(_connectionInfo, CancellationToken.None);

            Assert.AreSame(_innerHandler.SocketToReturn, actual);
            Assert.AreEqual(1, _innerHandler.ConnectAsyncCallCount);
        }

        [TestMethod]
        public void DisposeShouldDisposeInnerHandler()
        {
            _handler.Dispose();

            Assert.AreEqual(1, _innerHandler.DisposeCallCount);
        }

        private sealed class TestableDelegatingConnectionHandler : DelegatingConnectionHandler
        {
            public TestableDelegatingConnectionHandler(ConnectionHandler innerHandler)
                : base(innerHandler)
            {
            }
        }

        private sealed class FakeConnectionHandler : ConnectionHandler
        {
            public Socket SocketToReturn { get; } = new Socket(SocketType.Stream, ProtocolType.Tcp);

            public int ConnectCallCount { get; private set; }

            public int ConnectAsyncCallCount { get; private set; }

            public int DisposeCallCount { get; private set; }

            public override Socket Connect(ConnectionInfo connectionInfo)
            {
                ConnectCallCount++;
                return SocketToReturn;
            }

            public override Task<Socket> ConnectAsync(ConnectionInfo connectionInfo, CancellationToken cancellationToken)
            {
                ConnectAsyncCallCount++;
                return Task.FromResult(SocketToReturn);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    DisposeCallCount++;
                }

                base.Dispose(disposing);
            }
        }
    }
}
