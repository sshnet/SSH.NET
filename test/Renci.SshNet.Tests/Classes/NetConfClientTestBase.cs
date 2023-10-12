using Moq;
using Renci.SshNet.NetConf;

namespace Renci.SshNet.Tests.Classes
{
    public abstract class NetConfClientTestBase : BaseClientTestBase
    {
        internal Mock<INetConfSession> NetConfSessionMock { get; private set; }

        protected override void CreateMocks()
        {
            base.CreateMocks();

            NetConfSessionMock = new Mock<INetConfSession>(MockBehavior.Strict);
        }
    }
}
