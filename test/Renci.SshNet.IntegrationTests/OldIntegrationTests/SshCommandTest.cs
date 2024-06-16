using System.Diagnostics;

using Renci.SshNet.Common;

namespace Renci.SshNet.IntegrationTests.OldIntegrationTests
{
    /// <summary>
    /// Represents SSH command that can be executed.
    /// </summary>
    [TestClass]
    public class SshCommandTest : IntegrationTestBase
    {
        [TestMethod]
        public void Test_Run_SingleCommand()
        {
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                #region Example SshCommand RunCommand Result
                client.Connect();

                var testValue = Guid.NewGuid().ToString();
                using var command = client.RunCommand(string.Format("echo {0}", testValue));
                var result = command.Result;
                result = result.Substring(0, result.Length - 1);    //  Remove \n character returned by command

                client.Disconnect();
                #endregion

                Assert.AreEqual(testValue, result);
            }
        }

        [TestMethod]
        public void Test_Execute_SingleCommand()
        {
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                #region Example SshCommand CreateCommand Execute
                client.Connect();

                var testValue = Guid.NewGuid().ToString();
                var command = string.Format("echo -n {0}", testValue);
                using var cmd = client.CreateCommand(command);
                var result = cmd.Execute();

                client.Disconnect();
                #endregion

                Assert.AreEqual(testValue, result);
            }
        }

        [TestMethod]
        [Timeout(5000)]
        public void Test_CancelAsync_Unfinished_Command()
        {
            using var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password);
            client.Connect();
            var testValue = Guid.NewGuid().ToString();
            using var cmd = client.CreateCommand($"sleep 15s; echo {testValue}");

            var asyncResult = cmd.BeginExecute();

            cmd.CancelAsync();

            Assert.ThrowsException<OperationCanceledException>(() => cmd.EndExecute(asyncResult));
            Assert.IsTrue(asyncResult.IsCompleted);
            Assert.IsTrue(asyncResult.AsyncWaitHandle.WaitOne(0));
            Assert.AreEqual(string.Empty, cmd.Result);
            Assert.AreEqual("TERM", cmd.ExitSignal);
            Assert.IsNull(cmd.ExitStatus);
        }

        [TestMethod]
        [Timeout(5000)]
        public async Task Test_CancelAsync_Kill_Unfinished_Command()
        {
            using var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password);
            client.Connect();
            var testValue = Guid.NewGuid().ToString();
            using var cmd = client.CreateCommand($"sleep 15s; echo {testValue}");

            var asyncResult = cmd.BeginExecute();

            Task<string> executeTask = Task.Factory.FromAsync(asyncResult, cmd.EndExecute);

            cmd.CancelAsync(forceKill: true);

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => executeTask);
            Assert.IsTrue(asyncResult.IsCompleted);
            Assert.IsTrue(asyncResult.AsyncWaitHandle.WaitOne(0));
            Assert.AreEqual(string.Empty, cmd.Result);
            Assert.AreEqual("KILL", cmd.ExitSignal);
            Assert.IsNull(cmd.ExitStatus);
        }

        [TestMethod]
        public void Test_CancelAsync_Finished_Command()
        {
            using var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password);
            client.Connect();
            var testValue = Guid.NewGuid().ToString();
            using var cmd = client.CreateCommand($"echo -n {testValue}");

            var asyncResult = cmd.BeginExecute();

            Assert.IsTrue(asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5)));

            cmd.CancelAsync(); // Should not throw
            Assert.AreEqual(testValue, cmd.EndExecute(asyncResult)); // Should not throw
            cmd.CancelAsync(); // Should not throw

            Assert.IsTrue(asyncResult.IsCompleted);
            Assert.AreEqual(testValue, cmd.Result);
            Assert.AreEqual(0, cmd.ExitStatus);
            Assert.IsNull(cmd.ExitSignal);
        }

        [TestMethod]
        public void Test_Execute_ExtendedOutputStream()
        {
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                #region Example SshCommand CreateCommand Execute ExtendedOutputStream

                client.Connect();
                using var cmd = client.CreateCommand("echo 12345; echo 654321 >&2");
                using var reader = new StreamReader(cmd.ExtendedOutputStream);

                Assert.AreEqual("12345\n", cmd.Execute());
                Assert.AreEqual("654321\n", reader.ReadToEnd());

                client.Disconnect();

                #endregion
            }
        }

        [TestMethod]
        public void Test_Execute_Timeout()
        {
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                #region Example SshCommand CreateCommand Execute CommandTimeout
                client.Connect();
                using var cmd = client.CreateCommand("sleep 10s");
                cmd.CommandTimeout = TimeSpan.FromSeconds(2);
                Assert.ThrowsException<SshOperationTimeoutException>(cmd.Execute);
                client.Disconnect();
                #endregion
            }
        }

        [TestMethod]
        public void Test_Execute_InvalidCommand()
        {
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                client.Connect();

                using var cmd = client.CreateCommand(";");
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
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                client.Connect();
                using var cmd = client.CreateCommand(";");
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
        public void Test_Execute_Command_Reconnect_Execute_Command()
        {
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
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
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                client.Connect();

                using var cmd = client.RunCommand("exit 128");

                Assert.AreEqual(128, cmd.ExitStatus);
                Assert.IsNull(cmd.ExitSignal);
            }
        }

        [TestMethod]
        public void Test_Execute_Command_Asynchronously()
        {
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                client.Connect();

                using var callbackCalled = new ManualResetEventSlim();

                using var cmd = client.CreateCommand("sleep 2s; echo 'test'");
                var asyncResult = cmd.BeginExecute(new AsyncCallback((s) =>
                {
                    callbackCalled.Set();
                }), state: null);

                while (!asyncResult.IsCompleted)
                {
                    Thread.Sleep(100);
                }

                Assert.IsTrue(asyncResult.AsyncWaitHandle.WaitOne(0));

                cmd.EndExecute(asyncResult);

                Assert.AreEqual("test\n", cmd.Result);
                Assert.IsTrue(callbackCalled.Wait(TimeSpan.FromSeconds(1)));

                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_Execute_Command_Asynchronously_With_Error()
        {
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                client.Connect();

                using var cmd = client.CreateCommand("sleep 2s; ;");
                var asyncResult = cmd.BeginExecute(null, null);

                Assert.IsTrue(asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5)));

                cmd.EndExecute(asyncResult);

                Assert.IsFalse(string.IsNullOrEmpty(cmd.Error));

                client.Disconnect();
            }
        }

        [TestMethod]
        public void Test_Execute_Command_Asynchronously_With_Callback_On_Different_Thread()
        {
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                client.Connect();

                var currentThreadId = Thread.CurrentThread.ManagedThreadId;
                int callbackThreadId = 0;
                using var callbackCalled = new ManualResetEventSlim();

                using var cmd = client.CreateCommand("sleep 2s; echo 'test'");
                var asyncResult = cmd.BeginExecute(new AsyncCallback((s) =>
                {
                    callbackThreadId = Thread.CurrentThread.ManagedThreadId;
                    callbackCalled.Set();
                }), null);

                cmd.EndExecute(asyncResult);

                Assert.IsTrue(callbackCalled.Wait(TimeSpan.FromSeconds(1)));

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
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                client.Connect();
                using var cmd = client.CreateCommand("echo 12345");
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
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                client.Connect();
                using var cmd = client.CreateCommand("ls -l");

                Assert.AreEqual(string.Empty, cmd.Result);
                Assert.AreEqual(string.Empty, cmd.Error);
                client.Disconnect();
            }
        }

        [WorkItem(703), TestMethod]
        public void Test_EndExecute_Before_BeginExecute()
        {
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                client.Connect();
                using var cmd = client.CreateCommand("ls -l");
                Assert.ThrowsException<ArgumentNullException>(() => cmd.EndExecute(null));
                client.Disconnect();
            }
        }

        /// <summary>
        ///A test for BeginExecute
        ///</summary>
        [TestMethod()]
        public void BeginExecuteTest()
        {
            string expected = "123\n";
            string result;

            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                #region Example SshCommand CreateCommand BeginExecute IsCompleted EndExecute

                client.Connect();

                using var cmd = client.CreateCommand("sleep 2s;echo 123"); // Perform long running task

                var asynch = cmd.BeginExecute();

                Assert.IsTrue(asynch.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5)));

                result = cmd.EndExecute(asynch);
                client.Disconnect();

                #endregion

                Assert.IsNotNull(asynch);
                Assert.AreEqual(expected, result);
            }
        }

        [TestMethod]

        public void Test_MultipleThread_100_MultipleConnections()
        {
            try
            {
                var options = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = 8
                };

                Parallel.For(0, 100, options,
                    () =>
                    {
                        var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password);
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
        public void Test_MultipleThread_100_MultipleSessions()
        {
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                client.Connect();
                Parallel.For(0, 100,
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
        public void Test_ExecuteAsync_Dispose_CommandFinishes()
        {
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                client.Connect();

                var cmd = client.CreateCommand("sleep 5s");
                var asyncResult = cmd.BeginExecute(null, null);

                cmd.Dispose();

                Assert.IsTrue(asyncResult.AsyncWaitHandle.WaitOne(0));

                Assert.ThrowsException<ObjectDisposedException>(() => cmd.EndExecute(asyncResult));
            }
        }

        private static bool ExecuteTestCommand(SshClient s)
        {
            var testValue = Guid.NewGuid().ToString();
            var command = string.Format("echo -n {0}", testValue);
            using var cmd = s.CreateCommand(command);
            var result = cmd.Execute();
            return result.Equals(testValue);
        }
    }
}
