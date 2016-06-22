using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using System.Collections.Generic;
using System.IO;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Represents instance of the SSH shell object
    /// </summary>
    [TestClass]
    [Ignore] // class contains just for unit tests
    public partial class ShellTestTest : TestBase
    {
        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod()]
        public void DisposeTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            Stream input = null; // TODO: Initialize to an appropriate value
            Stream output = null; // TODO: Initialize to an appropriate value
            Stream extendedOutput = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModes = null; // TODO: Initialize to an appropriate value
            int bufferSize = 0; // TODO: Initialize to an appropriate value
            Shell target = new Shell(session, input, output, extendedOutput, terminalName, columns, rows, width, height, terminalModes, bufferSize); // TODO: Initialize to an appropriate value
            target.Dispose();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Start
        ///</summary>
        [TestMethod()]
        public void StartTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            Stream input = null; // TODO: Initialize to an appropriate value
            Stream output = null; // TODO: Initialize to an appropriate value
            Stream extendedOutput = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModes = null; // TODO: Initialize to an appropriate value
            int bufferSize = 0; // TODO: Initialize to an appropriate value
            Shell target = new Shell(session, input, output, extendedOutput, terminalName, columns, rows, width, height, terminalModes, bufferSize); // TODO: Initialize to an appropriate value
            target.Start();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Stop
        ///</summary>
        [TestMethod()]
        public void StopTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            Stream input = null; // TODO: Initialize to an appropriate value
            Stream output = null; // TODO: Initialize to an appropriate value
            Stream extendedOutput = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModes = null; // TODO: Initialize to an appropriate value
            int bufferSize = 0; // TODO: Initialize to an appropriate value
            Shell target = new Shell(session, input, output, extendedOutput, terminalName, columns, rows, width, height, terminalModes, bufferSize); // TODO: Initialize to an appropriate value
            target.Stop();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}