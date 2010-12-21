using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshClient.Tests.Properties;

namespace Renci.SshClient.Tests.SshClientTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class TestPortForwarding
    {
        [TestMethod]
        public void TestLocalPortForwarding()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                var port1 = client.AddForwardedPort<ForwardedPortLocal>(8084, "www.renci.org", 80);
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
        public void TestRemotePortForwarding()
        {
            //  ******************************************************************
            //  ************* Tests are still in not finished ********************
            //  ******************************************************************

            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                var port1 = client.AddForwardedPort<ForwardedPortRemote>(8082, "www.renci.org", 80);
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
                        var start = DateTime.Now;
                        try
                        {

                            var cmd = client.CreateCommand(string.Format("wget -O- http://localhost:{0}", boundport));
                            var result = cmd.Execute();
                            var end = DateTime.Now;
                            Debug.WriteLine(string.Format("Length: {0}", result.Length));
                        }
                        catch (Exception exp)
                        {

                            throw;
                        }
                    }
                );
            }
        }

        private static byte[] ReadStream(Stream stream)
        {
            byte[] buffer = new byte[1024];
            using (MemoryStream ms = new MemoryStream())
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
