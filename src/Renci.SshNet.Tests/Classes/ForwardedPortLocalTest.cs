using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality for local port forwarding
    /// </summary>
    [TestClass]
    public partial class ForwardedPortLocalTest : TestBase
    {
        [TestMethod]
        [WorkItem(713)]
        [Owner("Kenneth_aa")]
        [TestCategory("PortForwarding")]
        [TestCategory("integration")]
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
                while (!hasTestedTunnel)
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
        public void ConstructorShouldThrowArgumentNullExceptionWhenBoundHostIsNull()
        {
            try
            {
                new ForwardedPortLocal(null, 8080, Resources.HOST, 80);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("boundHost", ex.ParamName);
            }
        }

        [TestMethod]
        public void ConstructorShouldNotThrowExceptionWhenBoundHostIsEmpty()
        {
            var boundHost = string.Empty;

            var forwardedPort = new ForwardedPortLocal(boundHost, 8080, Resources.HOST, 80);

            Assert.AreSame(boundHost, forwardedPort.BoundHost);
        }

        [TestMethod]
        public void ConstructorShouldNotThrowExceptionWhenBoundHostIsInvalidDnsName()
        {
            const string boundHost = "in_valid_host.";

            var forwardedPort = new ForwardedPortLocal(boundHost, 8080, Resources.HOST, 80);

            Assert.AreSame(boundHost, forwardedPort.BoundHost);
        }

        [TestMethod]
        public void ConstructorShouldThrowArgumentNullExceptionWhenHostIsNull()
        {
            try
            {
                new ForwardedPortLocal(Resources.HOST, 8080, null, 80);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("host", ex.ParamName);
            }
        }

        [TestMethod]
        public void ConstructorShouldNotThrowExceptionWhenHostIsEmpty()
        {
            var host = string.Empty;

            var forwardedPort = new ForwardedPortLocal(Resources.HOST, 8080, string.Empty, 80);

            Assert.AreSame(host, forwardedPort.Host);
        }

        [TestMethod]
        public void ConstructorShouldNotThrowExceptionWhenHostIsInvalidDnsName()
        {
            const string host = "in_valid_host.";

            var forwardedPort = new ForwardedPortLocal(Resources.HOST, 8080, host, 80);

            Assert.AreSame(host, forwardedPort.Host);
        }

        /// <summary>
        ///A test for ForwardedPortRemote Constructor
        ///</summary>
        [TestMethod]
        [TestCategory("integration")]
        public void Test_ForwardedPortRemote()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                #region Example SshClient AddForwardedPort Start Stop ForwardedPortLocal
                client.Connect();
                var port = new ForwardedPortLocal(8082, "www.cnn.com", 80);
                client.AddForwardedPort(port);
                port.Exception += delegate(object sender, ExceptionEventArgs e)
                {
                    Console.WriteLine(e.Exception.ToString());
                };
                port.Start();

                Thread.Sleep(1000 * 60 * 20); //	Wait 20 minutes for port to be forwarded

                port.Stop();
                #endregion
            }
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

#if FEATURE_TPL
        [TestMethod]
        [TestCategory("integration")]
        [ExpectedException(typeof(SshConnectionException))]
        public void Test_PortForwarding_Local_Without_Connecting()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                var port1 = new ForwardedPortLocal("localhost", 8084, "www.renci.org", 80);
                client.AddForwardedPort(port1);
                port1.Exception += delegate (object sender, ExceptionEventArgs e)
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
                port1.Exception += delegate (object sender, ExceptionEventArgs e)
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

        private static byte[] ReadStream(System.IO.Stream stream)
        {
            byte[] buffer = new byte[1024];
            using (var ms = new System.IO.MemoryStream())
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
#endif // FEATURE_TPL
    }
}