using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;
using System.IO;
using System.Text;
using System.Threading;

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
        public void Test_Execute_SingleCommand()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                var result = ExecuteTestCommand(client);
                client.Disconnect();

                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SshOperationTimeoutException))]
        public void Test_Execute_Timeout()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                var cmd = client.CreateCommand("sleep 10s");
                cmd.CommandTimeout = TimeSpan.FromSeconds(5);
                cmd.Execute();
                client.Disconnect();
            }
        }

        [TestMethod]
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
        public void Test_Execute_Command_ExitStatus()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();

                var cmd = client.RunCommand("exit 128");
                Assert.IsTrue(cmd.ExitStatus == 128);

                client.Disconnect();
            }
        }

        [TestMethod]
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