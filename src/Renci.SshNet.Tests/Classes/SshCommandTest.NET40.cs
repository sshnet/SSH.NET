using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Properties;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Renci.SshNet.Tests.Classes
{
    public partial class SshCommandTest
    {

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

        //[TestMethod]
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

        //[TestMethod]
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
    }
}