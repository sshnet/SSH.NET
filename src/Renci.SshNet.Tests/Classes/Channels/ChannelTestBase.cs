using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Renci.SshNet.Tests.Classes.Channels
{
    public abstract class ChannelTestBase
    {
        internal Mock<ISession> SessionMock { get; private set; }
        internal Mock<ISshConnectionInfo> ConnectionInfoMock { get; private set; }

        [TestInitialize]
        public void Initialize()
        {
            Arrange();
            Act();
        }

        protected abstract void SetupData();

        protected void CreateMocks()
        {
            SessionMock = new Mock<ISession>(MockBehavior.Strict);
            ConnectionInfoMock = new Mock<ISshConnectionInfo>(MockBehavior.Strict);
        }

        protected abstract void SetupMocks();

        protected virtual void Arrange()
        {
            SetupData();
            CreateMocks();
            SetupMocks();
        }

        protected abstract void Act();
    }
}
