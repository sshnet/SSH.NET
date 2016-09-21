using System.Net.Sockets;
using Renci.SshNet.Abstractions;
#if SILVERLIGHT
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Renci.SshNet.Tests.Abstractions
{
    [TestClass]
    public class SocketAbstraction_CanWrite
    {
        [TestMethod]
        public void ShouldReturnFalseWhenSocketIsNull()
        {
            const Socket socket = null;

            var actual = SocketAbstraction.CanWrite(socket);

            Assert.IsFalse(actual);
        }
    }
}
