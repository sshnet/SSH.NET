using System;
using System.Diagnostics;
#if NET6_0_OR_GREATER
using System.Net.Http;
#else
using System.Net;
#endif // NET6_0_OR_GREATER
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;

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
            using (var client = new SshClient(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();

                var port1 = new ForwardedPortLocal("localhost", 8084, "www.google.com", 80);
                client.AddForwardedPort(port1);
                port1.Exception += delegate(object sender, ExceptionEventArgs e)
                {
                    Assert.Fail(e.Exception.ToString());
                };

                port1.Start();

                var hasTestedTunnel = false;

                _ = ThreadPool.QueueUserWorkItem(delegate(object state)
                    {
                        try
                        {
                            var url = "http://www.google.com/";
                            Debug.WriteLine("Starting web request to \"" + url + "\"");

#if NET6_0_OR_GREATER
                            var httpClient = new HttpClient();
                            var response = httpClient.GetAsync(url)
                                                     .ConfigureAwait(false)
                                                     .GetAwaiter()
                                                     .GetResult();
#else
                            var request = (HttpWebRequest) WebRequest.Create(url);
                            var response = (HttpWebResponse) request.GetResponse();
#endif // NET6_0_OR_GREATER

                            Assert.IsNotNull(response);

                            Debug.WriteLine("Http Response status code: " + response.StatusCode.ToString());

                            response.Dispose();

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
                    Thread.Sleep(1000);
                }

                try
                {
                    // Try stop the port forwarding, wait 3 seconds and fail if it is still started.
                    _ = ThreadPool.QueueUserWorkItem(delegate(object state)
                        {
                            Debug.WriteLine("Trying to stop port forward.");
                            port1.Stop();
                            Debug.WriteLine("Port forwarding stopped.");
                        });

                    Thread.Sleep(3000);
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
                _ = new ForwardedPortLocal(null, 8080, Resources.HOST, 80);
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
                _ = new ForwardedPortLocal(Resources.HOST, 8080, null, 80);
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

                _ = Parallel.For(0,
                                 100,
                                 counter =>
                                    {
                                        var start = DateTime.Now;

#if NET6_0_OR_GREATER
                                        var httpClient = new HttpClient();
                                        using (var response = httpClient.GetAsync("http://localhost:8084").GetAwaiter().GetResult())
                                        {
                                            var data = ReadStream(response.Content.ReadAsStream());
#else
                                        var request = (HttpWebRequest) WebRequest.Create("http://localhost:8084");
                                        using (var response = (HttpWebResponse) request.GetResponse())
                                        {
                                            var data = ReadStream(response.GetResponseStream());
#endif // NET6_0_OR_GREATER
                                            var end = DateTime.Now;

                                            Debug.WriteLine(string.Format("Request# {2}: Lenght: {0} Time: {1}", data.Length, end - start, counter));
                                        }
                                    });
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

                _ = Parallel.For(0,
                                 100,
                                 counter =>
                                    {
                                        var start = DateTime.Now;

#if NET6_0_OR_GREATER
                                        var httpClient = new HttpClient();
                                        using (var response = httpClient.GetAsync("http://localhost:8084").GetAwaiter().GetResult())
                                        {
                                            var data = ReadStream(response.Content.ReadAsStream());
#else
                                        var request = (HttpWebRequest) WebRequest.Create("http://localhost:8084");
                                        using (var response = (HttpWebResponse) request.GetResponse())
                                        {
                                            var data = ReadStream(response.GetResponseStream());
#endif // NET6_0_OR_GREATER
                                            var end = DateTime.Now;

                                            Debug.WriteLine(string.Format("Request# {2}: Length: {0} Time: {1}", data.Length, end - start, counter));
                                        }
                                    });
            }
        }

        private static byte[] ReadStream(System.IO.Stream stream)
        {
            var buffer = new byte[1024];
            using (var ms = new System.IO.MemoryStream())
            {
                while (true)
                {
                    var read = stream.Read(buffer, 0, buffer.Length);
                    if (read > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    else
                    {
                        return ms.ToArray();
                    }
                }
            }
        }
    }
}
