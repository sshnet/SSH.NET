using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Properties;

namespace Renci.SshNet.Tests.Classes
{
    public partial class ForwardedPortRemoteTest
    {
        [TestMethod]
        [TestCategory("integration")]
        public void Test_PortForwarding_Remote()
        {
            //  ******************************************************************
            //  ************* Tests are still in not finished ********************
            //  ******************************************************************

            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                var port1 = new ForwardedPortRemote(8082, "www.renci.org", 80);
                client.AddForwardedPort(port1);
                port1.Exception += delegate(object sender, ExceptionEventArgs e)
                {
                    Assert.Fail(e.Exception.ToString());
                };
                port1.Start();
                var boundport = port1.BoundPort;

                System.Threading.Tasks.Parallel.For(0, 5,

                    //new ParallelOptions
                    //{
                    //    MaxDegreeOfParallelism = 1,
                    //},
                    (counter) =>
                    {
                        var cmd = client.CreateCommand(string.Format("wget -O- http://localhost:{0}", boundport));
                        var result = cmd.Execute();
                        var end = DateTime.Now;
                        Debug.WriteLine(string.Format("Length: {0}", result.Length));
                    }
                );
                Thread.Sleep(1000 * 100);
                port1.Stop();
            }
        }
    }
}