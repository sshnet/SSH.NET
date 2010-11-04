using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshClient.Common;

namespace Renci.SshClient.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ShellTest
    {
        public ShellTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestConnectUsingPassword()
        {
            var s = CreateShellUsingPassword();
            s.Connect();
            s.Disconnect();
        }

        [TestMethod]
        public void TestExecuteSingleCommand()
        {
            var s = CreateShellUsingPassword();
            s.Connect();
            var result = ExecuteTestCommand(s);
            s.Disconnect();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestReconnecting()
        {
            var s = CreateShellUsingPassword();
            s.Connect();
            var result = ExecuteTestCommand(s);
            s.Disconnect();

            Assert.IsTrue(result);

            s.Connect();
            result = ExecuteTestCommand(s);
            s.Disconnect();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestMultipleThreadMultipleSessions_10000()
        {
            //  TODO:   Restore test to 10000 items
            var s = CreateShellUsingPassword();
            s.Connect();
            var numOfLoops = 100000;
            var exeCounter = 0;
            System.Threading.Tasks.Parallel.For(0, numOfLoops,
                (counter) =>
                {
                    var result = ExecuteTestCommand(s);
                    Debug.WriteLine(string.Format("TestMultipleThreadMultipleConnections #{0}", counter));
                    exeCounter++;
                    Assert.IsTrue(result);
                }
            );

            s.Disconnect();

            Assert.AreEqual(exeCounter, numOfLoops);
        }

        [TestMethod]
        public void TestMultipleThreadMultipleConnections_10000()
        {
            try
            {
                //  TODO:   Restore test to 10000 items
                System.Threading.Tasks.Parallel.For(0, 100,
                    () =>
                    {
                        var s = CreateShellUsingPassword();
                        s.Connect();
                        return s;
                    },
                    (int counter, ParallelLoopState pls, SshClient s) =>
                    {
                        var result = ExecuteTestCommand(s);
                        Debug.WriteLine(string.Format("TestMultipleThreadMultipleConnections #{0}", counter));
                        Assert.IsTrue(result);
                        return s;
                    },
                    (SshClient s) =>
                    {
                        s.Disconnect();
                    }
                );
            }
            catch (Exception exp)
            {
                Assert.Fail(exp.ToString());
            }
        }

        [TestMethod]
        public void TestExtendedOutput()
        {
            var s = CreateShellUsingPassword();
            MemoryStream ms = new MemoryStream();
            s.Connect();
            var result = s.Shell.Execute("echo 12345; echo 654321 >&2", ms);
            var extendedData = Encoding.ASCII.GetString(ms.ToArray());
            result = result.Substring(0, result.Length - 1);    //  Remove \n chararacter returned by command
            extendedData = extendedData.Substring(0, extendedData.Length - 1);    //  Remove \n chararacter returned by command
            s.Disconnect();

            Assert.AreEqual("12345", result);
            Assert.AreEqual("654321", extendedData);
        }

        [TestMethod]
        public void TestInvalidCommandExecution()
        {
            var s = CreateShellUsingPassword();
            s.Connect();
            try
            {
                s.Shell.Execute(";");
                Assert.Fail("Operation should fail");
            }
            catch (SshException exp)
            {
                Assert.AreEqual(exp.ExitStatus, (uint)2);
            }
            s.Disconnect();
        }

        [TestMethod]
        public void TestInvalidCommandThenValidCommandExecution()
        {
            var s = CreateShellUsingPassword();
            s.Connect();
            try
            {
                s.Shell.Execute(";");
                Assert.Fail("Operation should fail");
            }
            catch (SshException exp)
            {
                Assert.AreEqual(exp.ExitStatus, (uint)2);
            }
            var result = ExecuteTestCommand(s);
            s.Disconnect();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestRsaKeyConnection()
        {
            var s = CreateShellUsingRSAKey();
            s.Connect();
            var result = ExecuteTestCommand(s);
            s.Disconnect();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestDssKeyConnection()
        {
            var s = CreateShellUsingRSAKey();
            s.Connect();
            var result = ExecuteTestCommand(s);
            s.Disconnect();
            Assert.IsTrue(result);
        }

        private static SshClient CreateShellUsingPassword()
        {
            return new SshClient(ConnectionData.Host, ConnectionData.Port, ConnectionData.Username, ConnectionData.Password);
        }

        private static SshClient CreateShellUsingRSAKey()
        {
            return new SshClient(ConnectionData.Host, ConnectionData.Port, ConnectionData.Username, new PrivateKeyFile(ConnectionData.RsaKeyFilePath));
        }

        private static SshClient CreateShellUsingDSSKey()
        {
            return new SshClient(ConnectionData.Host, ConnectionData.Port, ConnectionData.Username, new PrivateKeyFile(ConnectionData.DssKeyFilePath));
        }

        private static bool ExecuteTestCommand(SshClient s)
        {
            var testValue = Guid.NewGuid().ToString();
            var command = string.Format("echo {0}", testValue);
            //var command = string.Format("echo {0};sleep 2s", testValue);
            var result = s.Shell.Execute(command);
            result = result.Substring(0, result.Length - 1);    //  Remove \n chararacter returned by command
            return result.Equals(testValue);
        }


    }
}
