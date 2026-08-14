using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Connection;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_Connecting_WithCustomConnectionHandler : SessionTest_ConnectingBase
    {
        internal Mock<ConnectionHandler> ConnectionHandlerMock { get; private set; }

        protected override void SetupData()
        {
            base.SetupData();

            ConnectionHandlerMock = new Mock<ConnectionHandler>(MockBehavior.Strict);
            ConnectionInfo.ConnectionHandler = ConnectionHandlerMock.Object;
        }

        protected override void SetupConnectorMocks()
        {
            _ = ConnectionHandlerMock.Setup(p => p.Connect(ConnectionInfo))
                                      .Returns(ClientSocket);
        }

        [TestMethod]
        public void ConnectShouldUseTheConfiguredConnectionHandlerInsteadOfTheDefault()
        {
            Session.Connect();

            ConnectionHandlerMock.Verify(p => p.Connect(ConnectionInfo), Times.Once);
            ServiceFactoryMock.Verify(p => p.CreateConnector(It.IsAny<IConnectionInfo>(), It.IsAny<ISocketFactory>()), Times.Never);
        }
    }
}
