using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;
using System.Text;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Represents SSH command that can be executed.
    /// </summary>
    [TestClass]
    public partial class SshCommandTest : TestBase
    {
        [TestMethod]
        [ExpectedException(typeof(SshConnectionException))]
        public void Test_Execute_SingleCommand_Without_Connecting()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                var result = ExecuteTestCommand(client);

                Assert.IsTrue(result);
            }
        }

        /// <summary>
        ///A test for BeginExecute
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void BeginExecuteTest1()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string commandText = string.Empty; // TODO: Initialize to an appropriate value
            var encoding = Encoding.UTF8;
            SshCommand target = new SshCommand(session, commandText, encoding); // TODO: Initialize to an appropriate value
            string commandText1 = string.Empty; // TODO: Initialize to an appropriate value
            AsyncCallback callback = null; // TODO: Initialize to an appropriate value
            object state = null; // TODO: Initialize to an appropriate value
            IAsyncResult expected = null; // TODO: Initialize to an appropriate value
            IAsyncResult actual;
            actual = target.BeginExecute(commandText1, callback, state);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CancelAsync
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void CancelAsyncTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string commandText = string.Empty; // TODO: Initialize to an appropriate value
            var encoding = Encoding.UTF8;
            SshCommand target = new SshCommand(session, commandText, encoding); // TODO: Initialize to an appropriate value
            target.CancelAsync();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }


        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void ExecuteTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string commandText = string.Empty; // TODO: Initialize to an appropriate value
            var encoding = Encoding.UTF8;
            SshCommand target = new SshCommand(session, commandText, encoding); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.Execute();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void ExecuteTest1()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string commandText = string.Empty; // TODO: Initialize to an appropriate value
            var encoding = Encoding.UTF8;
            SshCommand target = new SshCommand(session, commandText, encoding); // TODO: Initialize to an appropriate value
            string commandText1 = string.Empty; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.Execute(commandText1);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CommandTimeout
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void CommandTimeoutTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string commandText = string.Empty; // TODO: Initialize to an appropriate value
            var encoding = Encoding.UTF8;
            SshCommand target = new SshCommand(session, commandText, encoding); // TODO: Initialize to an appropriate value
            TimeSpan expected = new TimeSpan(); // TODO: Initialize to an appropriate value
            TimeSpan actual;
            target.CommandTimeout = expected;
            actual = target.CommandTimeout;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Error
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void ErrorTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string commandText = string.Empty; // TODO: Initialize to an appropriate value
            var encoding = Encoding.UTF8;
            SshCommand target = new SshCommand(session, commandText, encoding); // TODO: Initialize to an appropriate value
            string actual;
            actual = target.Error;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Result
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void ResultTest()
        {
            Session session = null; // TODO: Initialize to an appropriate value
            string commandText = string.Empty; // TODO: Initialize to an appropriate value
            var encoding = Encoding.UTF8;
            SshCommand target = new SshCommand(session, commandText, encoding); // TODO: Initialize to an appropriate value
            string actual;
            actual = target.Result;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        private static bool ExecuteTestCommand(SshClient s)
        {
            var testValue = Guid.NewGuid().ToString();
            var command = string.Format("echo {0}", testValue);
            var cmd = s.CreateCommand(command);
            var result = cmd.Execute();
            result = result.Substring(0, result.Length - 1);    //  Remove \n character returned by command
            return result.Equals(testValue);
        }
    }
}
