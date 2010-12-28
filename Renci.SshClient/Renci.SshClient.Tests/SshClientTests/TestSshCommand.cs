using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using Renci.SshClient.Tests.Properties;
using Renci.SshClient.Common;

namespace Renci.SshClient.Tests.SshClientTests
{
    [TestClass]
    public class TestSshCommand
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

        [TestMethod]
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
                var extendedData = Encoding.ASCII.GetString(cmd.ExtendedOutputStream.ToArray());
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

        private static bool ExecuteTestCommand(Renci.SshClient.SshClient s)
        {
            var testValue = Guid.NewGuid().ToString();
            var command = string.Format("echo {0}", testValue);
            //var command = string.Format("echo {0};sleep 2s", testValue);
            var cmd = s.CreateCommand(command);
            var result = cmd.Execute();
            result = result.Substring(0, result.Length - 1);    //  Remove \n chararacter returned by command
            return result.Equals(testValue);
        }
    }
}
