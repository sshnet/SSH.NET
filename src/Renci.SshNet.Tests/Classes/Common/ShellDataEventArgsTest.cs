using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    ///This is a test class for ShellDataEventArgsTest and is intended
    ///to contain all ShellDataEventArgsTest Unit Tests
    ///</summary>
    [TestClass]
    public class ShellDataEventArgsTest
    {
        /// <summary>
        ///A test for ShellDataEventArgs Constructor
        ///</summary>
        [TestMethod]
        public void ShellDataEventArgsConstructorTest()
        {
            byte[] data = null; // TODO: Initialize to an appropriate value
            ShellDataEventArgs target = new ShellDataEventArgs(data);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ShellDataEventArgs Constructor
        ///</summary>
        [TestMethod]
        public void ShellDataEventArgsConstructorTest1()
        {
            string line = string.Empty; // TODO: Initialize to an appropriate value
            ShellDataEventArgs target = new ShellDataEventArgs(line);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
