using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Properties;

namespace Renci.SshNet.Tests.Classes
{
    public partial class ForwardedPortLocalTest
    {
        [TestMethod]
        [TestCategory("integration")]
        [ExpectedException(typeof(SshConnectionException))]
        public void Test_PortForwarding_Local_Without_Connecting()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                var port1 = new ForwardedPortLocal("localhost", 8084, "www.renci.org", 80);
                client.AddForwardedPort(port1);
                port1.Exception += delegate(object sender, ExceptionEventArgs e)
                {
                    Assert.Fail(e.Exception.ToString());
                };
                port1.Start();

                System.Threading.Tasks.Parallel.For(0, 100,

                    //new ParallelOptions
                    //{
                    //    MaxDegreeOfParallelism = 20,
                    //},
                    (counter) =>
                    {
                        var start = DateTime.Now;
                        var req = HttpWebRequest.Create("http://localhost:8084");
                        using (var response = req.GetResponse())
                        {
                            var data = ReadStream(response.GetResponseStream());
                            var end = DateTime.Now;

                            Debug.WriteLine(string.Format("Request# {2}: Lenght: {0} Time: {1}", data.Length, (end - start), counter));
                        }
                    }
                );
            }
        }

        [TestMethod]
        [TestCategory("integration")]
        public void Test_PortForwarding_Local()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                var port1 = new ForwardedPortLocal("localhost", 8084, "www.renci.org", 80);
                client.AddForwardedPort(port1);
                port1.Exception += delegate(object sender, ExceptionEventArgs e)
                {
                    Assert.Fail(e.Exception.ToString());
                };
                port1.Start();

                System.Threading.Tasks.Parallel.For(0, 100,

                    //new ParallelOptions
                    //{
                    //    MaxDegreeOfParallelism = 20,
                    //},
                    (counter) =>
                    {
                        var start = DateTime.Now;
                        var req = HttpWebRequest.Create("http://localhost:8084");
                        using (var response = req.GetResponse())
                        {
                            var data = ReadStream(response.GetResponseStream());
                            var end = DateTime.Now;

                            Debug.WriteLine(string.Format("Request# {2}: Length: {0} Time: {1}", data.Length, (end - start), counter));
                        }
                    }
                );
            }
        }

        private static byte[] ReadStream(Stream stream)
        {
            byte[] buffer = new byte[1024];
            using (var ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read > 0)
                        ms.Write(buffer, 0, read);
                    else
                        return ms.ToArray();
                }
            }
        }
    }
}