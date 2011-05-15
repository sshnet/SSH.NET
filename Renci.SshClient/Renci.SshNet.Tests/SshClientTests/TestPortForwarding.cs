using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Properties;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.SshClientTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class TestPortForwarding
    {
        [TestMethod]
        [ExpectedException(typeof(SshConnectionException))]
        public void Test_PortForwarding_Local_Without_Connecting()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                var port1 = client.AddForwardedPort<ForwardedPortLocal>("localhost", 8084, "www.renci.org", 80);
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
        public void Test_PortForwarding_Local()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                var port1 = client.AddForwardedPort<ForwardedPortLocal>("localhost", 8084, "www.renci.org", 80);
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

        [TestMethod]
        public void Test_PortForwarding_Remote()
        {
            //  ******************************************************************
            //  ************* Tests are still in not finished ********************
            //  ******************************************************************

            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                var port1 = client.AddForwardedPort<ForwardedPortRemote>("", 8082, "www.renci.org", 80);
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
            }
        }

		[TestMethod]
		[Description("Test passing null to AddForwardedPort hosts (local).")]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Test_AddForwardedPort_Local_Hosts_Are_Null()
		{

			using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
			{
				client.Connect();
				var port1 = client.AddForwardedPort<ForwardedPortLocal>(null, 8080 , null, 80);
				client.Disconnect();
			}
		}

		[TestMethod]
		[Description("Test passing null to AddForwardedPort hosts (remote).")]
		public void Test_AddForwardedPort_Remote_Hosts_Are_Null()
		{
			using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
			{
				client.Connect();
				var port1 = client.AddForwardedPort<ForwardedPortRemote>(null, 8080, null, 80);
				client.Disconnect();
			}
		}

		[TestMethod]
		[Description("Test passing string.Empty to AddForwardedPort host (remote).")]
		[ExpectedException(typeof(ArgumentException))]
		public void Test_AddForwardedPort_Remote_Hosts_Are_Empty()
		{
			using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
			{
				client.Connect();
				var port1 = client.AddForwardedPort<ForwardedPortRemote>(string.Empty, 8080, string.Empty, 80);
				client.Disconnect();
			}
		}

		[TestMethod]
		[Description("Test passing string.Empty to AddForwardedPort host (local).")]
		public void Test_AddForwardedPort_Local_Hosts_Are_Empty()
		{
			using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
			{
				client.Connect();
				var port1 = client.AddForwardedPort<ForwardedPortLocal>(string.Empty, 8080, string.Empty, 80);
				client.Disconnect();
			}
		}

		[TestMethod]
		[Description("Test passing invalid port numbers to AddForwardedPort.")]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Test_AddForwardedPort_Invalid_PortNumber()
		{
			using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
			{
				client.Connect();
				var port1 = client.AddForwardedPort<ForwardedPortRemote>("localhost", IPEndPoint.MaxPort+1, "www.renci.org", IPEndPoint.MaxPort+1);
				client.Disconnect();
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
