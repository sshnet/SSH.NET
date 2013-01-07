using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Renci.SshNet.Examples
{
    class SshCommandExamples
    {
        public void Example()
        {
            #region Example1
            using (var client = new SshClient("host1", "username", "password"))
            {
                client.Connect();
                var cmd = client.CreateCommand("sleep 30s;date"); // Perform long running task
                var asynch = cmd.BeginExecute(null, null);
                while (!asynch.IsCompleted)
                {
                    Console.WriteLine("Waiting for command to complete...");
                    Thread.Sleep(2000);
                }
                cmd.EndExecute(asynch);
                client.Disconnect();
            }
            #endregion

            #region Example2
            using (var client = new SshClient("host2", "username", "password"))
            {
                client.Connect();
                var cmd = client.CreateCommand("sleep 30s;date"); // Perform long running task
                var asynch = cmd.BeginExecute(null, null);
                while (!asynch.IsCompleted)
                {
                    Console.WriteLine("Waiting for command to complete...");
                    Thread.Sleep(2000);
                }
                cmd.EndExecute(asynch);
                client.Disconnect();
            }
            #endregion

            #region Example3

            using (var client = new SshClient("host3", "username", "password"))
            {
                client.Connect();
                var cmd = client.CreateCommand("sleep 30s;date"); // Perform long running task
                var asynch = cmd.BeginExecute(null, null);
                while (!asynch.IsCompleted)
                {
                    Console.WriteLine("Waiting for command to complete...");
                    Thread.Sleep(2000);
                }
                cmd.EndExecute(asynch);
                client.Disconnect();
            }
            #endregion
        }
    }
}
