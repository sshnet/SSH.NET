using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Properties;
using Renci.SshNet.Common;
using System.Threading;

namespace Renci.SshNet.Tests.SshClientTests
{
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestClass]
	public partial class TestPortForwarding
	{
		[TestMethod]
		[WorkItem(713)]
		[Owner("kenneth_aa")]
        [TestCategory("PortForwarding")]
		[Description("Test if calling Stop on ForwardedPortLocal instance causes wait.")]
		public void Test_PortForwarding_Local_Stop_Hangs_On_Wait()
		{
			using (var client = new SshClient(Resources.HOST, Int32.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD))
			{
				client.Connect();

                var port1 = new ForwardedPortLocal("localhost", 8084, "www.google.com", 80);
                client.AddForwardedPort(port1);
                port1.Exception += delegate(object sender, ExceptionEventArgs e)
				{
					Assert.Fail(e.Exception.ToString());
				};

				port1.Start();

				bool hasTestedTunnel = false;
				System.Threading.ThreadPool.QueueUserWorkItem(delegate(object state)
				{
					try
					{
						var url = "http://www.google.com/";
						Debug.WriteLine("Starting web request to \"" + url + "\"");
						HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
						HttpWebResponse response = (HttpWebResponse)request.GetResponse();

						Assert.IsNotNull(response);

						Debug.WriteLine("Http Response status code: " + response.StatusCode.ToString());

						response.Close();

						hasTestedTunnel = true;
					}
					catch (Exception ex)
					{
						Assert.Fail(ex.ToString());
					}

				});

				// Wait for the web request to complete.
				while(!hasTestedTunnel)
				{
					System.Threading.Thread.Sleep(1000);
				}

				try
				{
					// Try stop the port forwarding, wait 3 seconds and fail if it is still started.
					System.Threading.ThreadPool.QueueUserWorkItem(delegate(object state)
					{
						Debug.WriteLine("Trying to stop port forward.");
						port1.Stop();
						Debug.WriteLine("Port forwarding stopped.");

					});

					System.Threading.Thread.Sleep(3000);
					if (port1.IsStarted)
					{ 
						Assert.Fail("Port forwarding not stopped.");
					}
				}
				catch (Exception ex)
				{
					Assert.Fail(ex.ToString());
				}
				client.Disconnect();
				Debug.WriteLine("Success.");
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
                var port1 = new ForwardedPortLocal(null, 8080, null, 80);
                client.AddForwardedPort(port1);
                client.Disconnect();
			}
		}

		[TestMethod]
		[Description("Test passing null to AddForwardedPort hosts (remote).")]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Test_AddForwardedPort_Remote_Hosts_Are_Null()
		{
			using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
			{
				client.Connect();
                var port1 = new ForwardedPortRemote(null, 8080, null, 80);
                client.AddForwardedPort(port1);
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
                var port1 = new ForwardedPortRemote(string.Empty, 8080, string.Empty, 80);
                client.AddForwardedPort(port1);
                client.Disconnect();
			}
		}

		[TestMethod]
		[Description("Test passing string.Empty to AddForwardedPort host (local).")]
		[ExpectedException(typeof(ArgumentException))]
		public void Test_AddForwardedPort_Local_Hosts_Are_Empty()
		{
			using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
			{
				client.Connect();
                var port1 = new ForwardedPortLocal(string.Empty, 8080, string.Empty, 80);
                client.AddForwardedPort(port1);
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
                var port1 = new ForwardedPortRemote("localhost", IPEndPoint.MaxPort + 1, "www.renci.org", IPEndPoint.MaxPort + 1);
                client.AddForwardedPort(port1);
                client.Disconnect();
			}
		}

		[TestMethod]
		[Description("Test passing null to constructor of PortForwardEventArgs.")]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Test_PortForwardEventArgs_Host_Null()
		{
			var args = new PortForwardEventArgs(null, 80);
		}

		[TestMethod]
		[Description("Test passing string.Empty to constructor of PortForwardEventArgs.")]
		[ExpectedException(typeof(ArgumentException))]
		public void Test_PortForwardEventArgs_Host_Empty()
		{
			var args = new PortForwardEventArgs(string.Empty, 80);
		}

		[TestMethod]
		[Description("Test passing an invalid port to constructor of PortForwardEventArgs.")]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Test_PortForwardEventArgs_Port_Invalid()
		{
			var args = new PortForwardEventArgs("string", IPEndPoint.MaxPort + 1);
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
