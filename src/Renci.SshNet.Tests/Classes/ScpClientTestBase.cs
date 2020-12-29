using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    public abstract class ScpClientTestBase : BaseClientTestBase
    {
        internal Mock<IRemotePathTransformation> _remotePathTransformationMock;
        internal Mock<IChannelSession> _channelSessionMock;
        internal Mock<PipeStream> _pipeStreamMock;

        protected override void CreateMocks()
        {
            base.CreateMocks();

            _remotePathTransformationMock = new Mock<IRemotePathTransformation>(MockBehavior.Strict);
            _channelSessionMock = new Mock<IChannelSession>(MockBehavior.Strict);
            _pipeStreamMock = new Mock<PipeStream>(MockBehavior.Strict);
        }
    }
}
