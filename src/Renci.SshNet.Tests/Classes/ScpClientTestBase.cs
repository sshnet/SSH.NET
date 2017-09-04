using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    public abstract class ScpClientTestBase
    {
        internal Mock<IServiceFactory> _serviceFactoryMock;
        internal Mock<IRemotePathTransformation> _remotePathTransformationMock;
        internal Mock<ISession> _sessionMock;
        internal Mock<IChannelSession> _channelSessionMock;
        internal Mock<PipeStream> _pipeStreamMock;

        protected abstract void SetupData();

        protected void CreateMocks()
        {
            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            _remotePathTransformationMock = new Mock<IRemotePathTransformation>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _channelSessionMock = new Mock<IChannelSession>(MockBehavior.Strict);
            _pipeStreamMock = new Mock<PipeStream>(MockBehavior.Strict);
        }

        protected abstract void SetupMocks();

        protected virtual void Arrange()
        {
            SetupData();
            CreateMocks();
            SetupMocks();
        }

        [TestInitialize]
        public void Initialize()
        {
            Arrange();
            Act();
        }

        protected abstract void Act();
    }
}
