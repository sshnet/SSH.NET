using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;
using System.IO;
using System.Text;
using System.Threading;
#if FEATURE_TPL
using System.Diagnostics;
using System.Threading.Tasks;
#endif // FEATURE_TPL

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

        [TestMethod]
        [TestCategory("integration")]
        public void Test_Run_SingleCommand()
        {
            var host = Resources.HOST;
            var username = Resources.USERNAME;
            var password = Resources.PASSWORD;

            using (var client = new SshClient(host, username, password))
            {
                #region Example SshCommand RunCommand Result
                client.Connect();

                var testValue = Guid.NewGuid().ToString();
                var command = client.RunCommand(string.Format("echo {0}", testValue));
                var result = command.Result;
                result = result.Substring(0, result.Length - 1);    //  Remove \n character returned by command

                client.Disconnect();
                #endregion

                Assert.IsTrue(result.Equals(testValue));
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_Execute_SingleCommand()
        {
            var host = Resources.HOST;
            var username = Resources.USERNAME;
            var password = Resources.PASSWORD;

            using (var client = new SshClient(host, username, password))
            {
                #region Example SshCommand CreateCommand Execute
                client.Connect();

                var testValue = Guid.NewGuid().ToString();
                var command = string.Format("echo {0}", testValue);
                var cmd = client.CreateCommand(command);
                var result = cmd.Execute();
                result = result.Substring(0, result.Length - 1);    //  Remove \n character returned by command

                client.Disconnect();
                #endregion

                Assert.IsTrue(result.Equals(testValue));
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_Execute_OutputStream()
        {
            var host = Resources.HOST;
            var username = Resources.USERNAME;
            var password = Resources.PASSWORD;

            using (var client = new SshClient(host, username, password))
            {
                #region Example SshCommand CreateCommand Execute OutputStream
                client.Connect();

                var cmd = client.CreateCommand("ls -l");   //  very long list
                var asynch = cmd.BeginExecute();

                var reader = new StreamReader(cmd.OutputStream);

                while (!asynch.IsCompleted)
                {
                    var result = reader.ReadToEnd();
                    if (string.IsNullOrEmpty(result))
                        continue;
                    Console.Write(result);
                }
                cmd.EndExecute(asynch);

                client.Disconnect();
                #endregion

                Assert.Inconclusive();
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_Execute_ExtendedOutputStream()
        {
            var host = Resources.HOST;
            var username = Resources.USERNAME;
            var password = Resources.PASSWORD;

            using (var client = new SshClient(host, username, password))
            {
                #region Example SshCommand CreateCommand Execute ExtendedOutputStream

                client.Connect();
                var cmd = client.CreateCommand("echo 12345; echo 654321 >&2");
                var result = cmd.Execute();

                Console.Write(result);

                var reader = new StreamReader(cmd.ExtendedOutputStream);
                Console.WriteLine("DEBUG:");
                Console.Write(reader.ReadToEnd());

                client.Disconnect();

                #endregion

                Assert.Inconclusive();
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        [ExpectedException(typeof(SshOperationTimeoutException))]
        public void Test_Execute_Timeout()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                #region Example SshCommand CreateCommand Execute CommandTimeout
                client.Connect();
                var cmd = client.CreateCommand("sleep 10s");
                cmd.CommandTimeout = TimeSpan.FromSeconds(5);
                cmd.Execute();
                client.Disconnect();
                #endregion
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_Execute_Infinite_Timeout()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                var cmd = client.CreateCommand("sleep 10s");
                cmd.Execute();
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_Execute_InvalidCommand()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();

                var cmd = client.CreateCommand(";");
                cmd.Execute();
                if (string.IsNullOrEmpty(cmd.Error))
                {
                    Assert.Fail("Operation should fail");
                }
                Assert.IsTrue(cmd.ExitStatus > 0);

                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_Execute_InvalidCommand_Then_Execute_ValidCommand()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                var cmd = client.CreateCommand(";");
                cmd.Execute();
                if (string.IsNullOrEmpty(cmd.Error))
                {
                    Assert.Fail("Operation should fail");
                }
                Assert.IsTrue(cmd.ExitStatus > 0);

                var result = ExecuteTestCommand(client);

                client.Disconnect();

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_Execute_Command_with_ExtendedOutput()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                var cmd = client.CreateCommand("echo 12345; echo 654321 >&2");
                cmd.Execute();

                //var extendedData = Encoding.ASCII.GetString(cmd.ExtendedOutputStream.ToArray());
                var extendedData = new StreamReader(cmd.ExtendedOutputStream, Encoding.ASCII).ReadToEnd();
                client.Disconnect();

                Assert.AreEqual("12345\n", cmd.Result);
                Assert.AreEqual("654321\n", extendedData);
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_Execute_Command_Reconnect_Execute_Command()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                var result = ExecuteTestCommand(client);
                Assert.IsTrue(result);

                client.Disconnect();
                client.Connect();
                result = ExecuteTestCommand(client);
                Assert.IsTrue(result);
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_Execute_Command_ExitStatus()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                #region Example SshCommand RunCommand ExitStatus
                client.Connect();

                var cmd = client.RunCommand("exit 128");
                
                Console.WriteLine(cmd.ExitStatus);

                client.Disconnect();
                #endregion

                Assert.IsTrue(cmd.ExitStatus == 128);
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_Execute_Command_Asynchronously()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();

                var cmd = client.CreateCommand("sleep 5s; echo 'test'");
                var asyncResult = cmd.BeginExecute(null, null);
                while (!asyncResult.IsCompleted)
                {
                    Thread.Sleep(100);
                }

                cmd.EndExecute(asyncResult);

                Assert.IsTrue(cmd.Result == "test\n");

                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_Execute_Command_Asynchronously_With_Error()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();

                var cmd = client.CreateCommand("sleep 5s; ;");
                var asyncResult = cmd.BeginExecute(null, null);
                while (!asyncResult.IsCompleted)
                {
                    Thread.Sleep(100);
                }

                cmd.EndExecute(asyncResult);

                Assert.IsFalse(string.IsNullOrEmpty(cmd.Error));

                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_Execute_Command_Asynchronously_With_Callback()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();

                var callbackCalled = false;

                var cmd = client.CreateCommand("sleep 5s; echo 'test'");
                var asyncResult = cmd.BeginExecute(new AsyncCallback((s) =>
                {
                    callbackCalled = true;
                }), null);
                while (!asyncResult.IsCompleted)
                {
                    Thread.Sleep(100);
                }

                cmd.EndExecute(asyncResult);

                Assert.IsTrue(callbackCalled);

                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_Execute_Command_Asynchronously_With_Callback_On_Different_Thread()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();

                var currentThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = 0;

                var cmd = client.CreateCommand("sleep 5s; echo 'test'");
                var asyncResult = cmd.BeginExecute(new AsyncCallback((s) =>
                {
                    callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                }), null);
                while (!asyncResult.IsCompleted)
                {
                    Thread.Sleep(100);
                }

                cmd.EndExecute(asyncResult);

                Assert.AreNotEqual(currentThreadId, callbackThreadId);

                client.Disconnect();
            }
        }

        /// <summary>
        /// Tests for Issue 563.
        /// </summary>
        [WorkItem(563), TestMethod]
        [TestCategory("integration")]
        public void Test_Execute_Command_Same_Object_Different_Commands()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                var cmd = client.CreateCommand("echo 12345");
                cmd.Execute();
                Assert.AreEqual("12345\n", cmd.Result);
                cmd.Execute("echo 23456");
                Assert.AreEqual("23456\n", cmd.Result);
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_Get_Result_Without_Execution()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                var cmd = client.CreateCommand("ls -l");

                Assert.IsTrue(string.IsNullOrEmpty(cmd.Result));
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_Get_Error_Without_Execution()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                var cmd = client.CreateCommand("ls -l");

                Assert.IsTrue(string.IsNullOrEmpty(cmd.Error));
                client.Disconnect();
            }
        }

        [WorkItem(703), TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        [TestCategory("integration")]
        public void Test_EndExecute_Before_BeginExecute()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                var cmd = client.CreateCommand("ls -l");
                cmd.EndExecute(null);
                client.Disconnect();
            }
        }

        /// <summary>
        ///A test for BeginExecute
        ///</summary>
        [TestMethod()]
        [TestCategory("integration")]
        public void BeginExecuteTest()
        {
            string expected = "123\n";
            string result;

            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                #region Example SshCommand CreateCommand BeginExecute IsCompleted EndExecute

                client.Connect();

                var cmd = client.CreateCommand("sleep 15s;echo 123"); // Perform long running task

                var asynch = cmd.BeginExecute();

                while (!asynch.IsCompleted)
                {
                    //  Waiting for command to complete...
                    Thread.Sleep(2000);
                }
                result = cmd.EndExecute(asynch);
                client.Disconnect();

                #endregion

                Assert.IsNotNull(asynch);
                Assert.AreEqual(expected, result);
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_Execute_Invalid_Command()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                #region Example SshCommand CreateCommand Error

                client.Connect();

                var cmd = client.CreateCommand(";");
                cmd.Execute();
                if (!string.IsNullOrEmpty(cmd.Error))
                {
                    Console.WriteLine(cmd.Error);
                }

                client.Disconnect();

                #endregion

                Assert.Inconclusive();
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

#if FEATURE_TPL
        [TestMethod]
        [TestCategory("integration")]
        public void Test_MultipleThread_Example_MultipleConnections()
        {
            var host = Resources.HOST;
            var username = Resources.USERNAME;
            var password = Resources.PASSWORD;

            try
            {
#region Example SshCommand RunCommand Parallel
                System.Threading.Tasks.Parallel.For(0, 10000,
                    () =>
                    {
                        var client = new SshClient(host, username, password);
                        client.Connect();
                        return client;
                    },
                    (int counter, ParallelLoopState pls, SshClient client) =>
                    {
                        var result = client.RunCommand("echo 123");
                        Debug.WriteLine(string.Format("TestMultipleThreadMultipleConnections #{0}", counter));
                        return client;
                    },
                    (SshClient client) =>
                    {
                        client.Disconnect();
                        client.Dispose();
                    }
                );
#endregion

            }
            catch (Exception exp)
            {
                Assert.Fail(exp.ToString());
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_MultipleThread_10000_MultipleConnections()
        {
            try
            {
                System.Threading.Tasks.Parallel.For(0, 10000,
                    () =>
                    {
                        var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD);
                        client.Connect();
                        return client;
                    },
                    (int counter, ParallelLoopState pls, SshClient client) =>
                    {
                        var result = ExecuteTestCommand(client);
                        Debug.WriteLine(string.Format("TestMultipleThreadMultipleConnections #{0}", counter));
                        Assert.IsTrue(result);
                        return client;
                    },
                    (SshClient client) =>
                    {
                        client.Disconnect();
                        client.Dispose();
                    }
                );
            }
            catch (Exception exp)
            {
                Assert.Fail(exp.ToString());
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_MultipleThread_10000_MultipleSessions()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                System.Threading.Tasks.Parallel.For(0, 10000,
                    (counter) =>
                    {
                        var result = ExecuteTestCommand(client);
                        Debug.WriteLine(string.Format("TestMultipleThreadMultipleConnections #{0}", counter));
                        Assert.IsTrue(result);
                    }
                );

                client.Disconnect();
            }
        }
#endif // FEATURE_TPL

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