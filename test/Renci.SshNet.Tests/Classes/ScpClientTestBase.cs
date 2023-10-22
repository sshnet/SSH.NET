using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    public abstract class ScpClientTestBase : BaseClientTestBase
    {
        internal Mock<IRemotePathTransformation> RemotePathTransformationMock { get; private set; }
        internal Mock<IChannelSession> ChannelSessionMock { get; private set; }
        internal Mock<PipeStream> PipeStreamMock { get; private set; }

        protected override void CreateMocks()
        {
            base.CreateMocks();

            RemotePathTransformationMock = new Mock<IRemotePathTransformation>(MockBehavior.Strict);
            ChannelSessionMock = new Mock<IChannelSession>(MockBehavior.Strict);
            PipeStreamMock = new Mock<PipeStream>(MockBehavior.Strict);
        }
    }
}
