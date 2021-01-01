using Moq;
using Renci.SshNet.NetConf;

namespace Renci.SshNet.Tests.Classes
{
    public abstract class NetConfClientTestBase : BaseClientTestBase
    {
        internal Mock<INetConfSession> _netConfSessionMock { get; private set; }

        protected override void CreateMocks()
        {
            base.CreateMocks();

            _netConfSessionMock = new Mock<INetConfSession>(MockBehavior.Strict);
        }
    }
}
