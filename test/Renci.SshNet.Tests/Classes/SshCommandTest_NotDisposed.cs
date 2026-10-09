using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Regression tests for https://github.com/sshnet/SSH.NET/issues/1204: an <see cref="SshCommand"/>
    /// that completed but was not disposed must not be kept alive by the (long-lived) session, and
    /// must not keep its channel subscribed to the session.
    /// </summary>
    [TestClass]
    public class SshCommandTest_NotDisposed : TestBase
    {
        private Mock<ISession> _sessionMock;
        private Mock<IChannelSession> _channelSessionMock;
        private string _commandText;

        protected override void OnInit()
        {
            base.OnInit();

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
            _channelSessionMock = new Mock<IChannelSession>(MockBehavior.Strict);
            _commandText = new Random().Next().ToString(CultureInfo.InvariantCulture);
        }

        [TestMethod]
        public void ConstructorShouldNotCreateChannelNorSubscribeToSession()
        {
            // Strict mock: CreateChannelSession has not been set up, so calling it would throw.
            _ = new SshCommand(_sessionMock.Object, _commandText, Encoding.UTF8);

            _sessionMock.Verify(p => p.CreateChannelSession(), Times.Never);
        }

        [TestMethod]
        public void CompletedCommandShouldReleaseChannelWithoutDispose()
        {
            var seq = new MockSequence();
            _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
            _channelSessionMock.InSequence(seq).Setup(p => p.Open());
            _channelSessionMock.InSequence(seq).Setup(p => p.SendExecRequest(_commandText)).Returns(true);
            _channelSessionMock.InSequence(seq).Setup(p => p.Dispose());

            var sshCommand = new SshCommand(_sessionMock.Object, _commandText, Encoding.UTF8);
            var asyncResult = sshCommand.BeginExecute();

            _channelSessionMock.Verify(p => p.Dispose(), Times.Never);

            _channelSessionMock.Raise(c => c.Closed += null, new ChannelEventArgs(5));

            _ = sshCommand.EndExecute(asyncResult);

            // The channel was released as soon as it was closed at both ends.
            _channelSessionMock.Verify(p => p.Dispose(), Times.Once);

            // Disposing afterwards must not dispose the channel again.
            sshCommand.Dispose();
            _channelSessionMock.Verify(p => p.Dispose(), Times.Once);
        }

        [TestMethod]
        public void ChannelClosedBeforeSendExecRequestReturnsShouldNotDisposeChannelUnderneathIt()
        {
            // Reproduces the race fixed by #1596: SSH_MSG_CHANNEL_CLOSE is processed on the
            // message loop thread while the executing thread is still in SendExecRequest.
            var disposedDuringSendExecRequest = false;

            var seq = new MockSequence();
            _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
            _channelSessionMock.InSequence(seq).Setup(p => p.Open());
            _channelSessionMock.InSequence(seq).Setup(p => p.SendExecRequest(_commandText))
                .Returns(true)
                .Callback(() =>
                {
                    _channelSessionMock.Raise(c => c.Closed += null, new ChannelEventArgs(5));

                    disposedDuringSendExecRequest = _channelSessionMock.Invocations.Any(i => i.Method.Name == nameof(IDisposable.Dispose));
                });
            _channelSessionMock.InSequence(seq).Setup(p => p.Dispose());

            var sshCommand = new SshCommand(_sessionMock.Object, _commandText, Encoding.UTF8);
            var asyncResult = sshCommand.BeginExecute();
            _ = sshCommand.EndExecute(asyncResult);

            Assert.IsFalse(disposedDuringSendExecRequest);
            _channelSessionMock.Verify(p => p.Dispose(), Times.Once);
        }

        [TestMethod]
        public void CompletedCommandShouldNotBeKeptAliveBySession()
        {
            var seq = new MockSequence();
            _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
            _channelSessionMock.InSequence(seq).Setup(p => p.Open());
            _channelSessionMock.InSequence(seq).Setup(p => p.SendExecRequest(_commandText)).Returns(true);
            _channelSessionMock.InSequence(seq).Setup(p => p.Dispose());

            var weakReference = ExecuteAndForget();

            // Moq records invocations (including the event handler delegates passed to add_/remove_
            // accessors), which would otherwise keep the command alive. The event handlers that are
            // still subscribed are stored separately and are not cleared by this - which is exactly
            // what this test is about.
            _sessionMock.Invocations.Clear();
            _channelSessionMock.Invocations.Clear();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.IsFalse(weakReference.IsAlive, "The completed SshCommand is still referenced (by the session).");
            GC.KeepAlive(_sessionMock.Object);
        }

        [TestMethod]
        public void FailureToStartShouldReleaseChannelAndAllowReexecution()
        {
            var seq = new MockSequence();
            _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
            _channelSessionMock.InSequence(seq).Setup(p => p.Open()).Throws(new SshException("open failed"));
            _channelSessionMock.InSequence(seq).Setup(p => p.Dispose());

            var sshCommand = new SshCommand(_sessionMock.Object, _commandText, Encoding.UTF8);

            _ = Assert.ThrowsExactly<SshException>(sshCommand.BeginExecute);

            _channelSessionMock.Verify(p => p.Dispose(), Times.Once);

            // Previously the instance was left "in progress" forever.
            var channelSessionBMock = new Mock<IChannelSession>(MockBehavior.Strict);
            _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(channelSessionBMock.Object);
            channelSessionBMock.InSequence(seq).Setup(p => p.Open());
            channelSessionBMock.InSequence(seq).Setup(p => p.SendExecRequest(_commandText)).Returns(true);

            _ = sshCommand.BeginExecute();
        }

        [TestMethod]
        public void CancelledCommandWhoseChannelIsNotClosedShouldReleaseChannel()
        {
            using var channelDisposed = new ManualResetEventSlim();

            var seq = new MockSequence();
            _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
            _channelSessionMock.InSequence(seq).Setup(p => p.Open());
            _channelSessionMock.InSequence(seq).Setup(p => p.SendExecRequest(_commandText)).Returns(true);
            _channelSessionMock.InSequence(seq).Setup(p => p.SendSignalRequest("TERM")).Returns(true);
            _channelSessionMock.InSequence(seq).Setup(p => p.Dispose()).Callback(channelDisposed.Set);

            var sshCommand = new SshCommand(_sessionMock.Object, _commandText, Encoding.UTF8);
            var executeTask = sshCommand.ExecuteAsync();

            // The server does not reply to the signal and does not close the channel.
            sshCommand.CancelAsync(millisecondsTimeout: 10);

            Assert.IsTrue(executeTask.IsCanceled);
            Assert.IsTrue(channelDisposed.Wait(TimeSpan.FromSeconds(5)), "The channel was not released.");
        }

        [TestMethod]
        public void SessionErrorShouldReleaseChannel()
        {
            using var channelDisposed = new ManualResetEventSlim();
            var exception = new SshException("session error");

            var seq = new MockSequence();
            _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
            _channelSessionMock.InSequence(seq).Setup(p => p.Open());
            _channelSessionMock.InSequence(seq).Setup(p => p.SendExecRequest(_commandText)).Returns(true);
            _channelSessionMock.InSequence(seq).Setup(p => p.Dispose()).Callback(channelDisposed.Set);

            var sshCommand = new SshCommand(_sessionMock.Object, _commandText, Encoding.UTF8);
            var executeTask = sshCommand.ExecuteAsync();

            _sessionMock.Raise(s => s.ErrorOccured += null, new ExceptionEventArgs(exception));

            Assert.AreSame(exception, executeTask.Exception?.InnerException);
            Assert.IsTrue(channelDisposed.Wait(TimeSpan.FromSeconds(5)), "The channel was not released.");
        }

        [TestMethod]
        public void CancelledCommandWhoseChannelIsNotClosedShouldNotBeKeptAliveBySession()
        {
            using var channelDisposed = new ManualResetEventSlim();

            var seq = new MockSequence();
            _sessionMock.InSequence(seq).Setup(p => p.CreateChannelSession()).Returns(_channelSessionMock.Object);
            _channelSessionMock.InSequence(seq).Setup(p => p.Open());
            _channelSessionMock.InSequence(seq).Setup(p => p.SendExecRequest(_commandText)).Returns(true);
            _channelSessionMock.InSequence(seq).Setup(p => p.SendSignalRequest("TERM")).Returns(true);
            _channelSessionMock.InSequence(seq).Setup(p => p.Dispose()).Callback(channelDisposed.Set);

            var weakReference = ExecuteCancelAndForget();

            Assert.IsTrue(channelDisposed.Wait(TimeSpan.FromSeconds(5)), "The channel was not released.");

            // See CompletedCommandShouldNotBeKeptAliveBySession.
            _sessionMock.Invocations.Clear();
            _channelSessionMock.Invocations.Clear();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.IsFalse(weakReference.IsAlive, "The cancelled SshCommand is still referenced (by the session or its channel).");
            GC.KeepAlive(_sessionMock.Object);
            GC.KeepAlive(_channelSessionMock.Object);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private WeakReference ExecuteCancelAndForget()
        {
            var sshCommand = new SshCommand(_sessionMock.Object, _commandText, Encoding.UTF8);
            _ = sshCommand.ExecuteAsync();
            sshCommand.CancelAsync(millisecondsTimeout: 10);

            // Deliberately not disposed.
            return new WeakReference(sshCommand);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private WeakReference ExecuteAndForget()
        {
            var sshCommand = new SshCommand(_sessionMock.Object, _commandText, Encoding.UTF8);
            var asyncResult = sshCommand.BeginExecute();
            _channelSessionMock.Raise(c => c.Closed += null, new ChannelEventArgs(5));
            _ = sshCommand.EndExecute(asyncResult);

            // Deliberately not disposed.
            return new WeakReference(sshCommand);
        }
    }
}
