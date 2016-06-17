using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class ExtensionsTest_ToGlobalRequestName
    {
        [TestMethod]
        public void TcpipForward()
        {
            var data = Encoding.ASCII.GetBytes("tcpip-forward");

            var actual = data.ToGlobalRequestName();

            Assert.AreEqual(GlobalRequestName.TcpIpForward, actual);
        }

        [TestMethod]
        public void CancelTcpipForward()
        {
            var data = Encoding.ASCII.GetBytes("cancel-tcpip-forward");

            var actual = data.ToGlobalRequestName();

            Assert.AreEqual(GlobalRequestName.CancelTcpIpForward, actual);
        }

    }
}
