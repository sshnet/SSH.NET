using System.Diagnostics;
using System.Globalization;

using Renci.SshNet.Common;

namespace Renci.SshNet.IntegrationTests.OldIntegrationTests
{
    /// <summary>
    /// Provides functionality for local port forwarding
    /// </summary>
    [TestClass]
    public sealed class ForwardedPortLocalTest : IntegrationTestBase
    {
        [TestMethod]
        [WorkItem(713)]
        [Owner("Kenneth_aa")]
        [TestCategory("PortForwarding")]
        [Description("Test if calling Stop on ForwardedPortLocal instance causes wait.")]
        public void Test_PortForwarding_Local_Stop_Hangs_On_Wait()
        {
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            {
                client.Connect();

                var port = new ForwardedPortLocal("localhost", 8084, "www.google.com", 80);
                client.AddForwardedPort(port);
                port.Exception += (sender, e) =>
                    {
                        Assert.Fail(e.Exception.ToString());
                    };

                port.Start();

                var hasTestedTunnel = false;

                _ = ThreadPool.QueueUserWorkItem(state =>
                    {
                        var url = new Uri("http://www.google.com/");
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
                    });

                // Wait for the web request to complete.
                while (!hasTestedTunnel)
                {
                    Thread.Sleep(1000);
                }

                try
                {
                    // Try stop the port forwarding, wait 3 seconds and fail if it is still started.
                    _ = ThreadPool.QueueUserWorkItem(state =>
                        {
                            Debug.WriteLine("Trying to stop port forward.");
                            port.Stop();
                            Debug.WriteLine("Port forwarding stopped.");
                        });

                    Thread.Sleep(3000);
                    if (port.IsStarted)
                    {
                        Assert.Fail("Port forwarding not stopped.");
                    }
                }
                catch (Exception ex)
                {
                    Assert.Fail(ex.ToString());
                }

                client.RemoveForwardedPort(port);
                client.Disconnect();
                Debug.WriteLine("Success.");

                port.Dispose();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SshConnectionException))]
        public void Test_PortForwarding_Local_Without_Connecting()
        {
            using (var client = new SshClient(SshServerHostName, SshServerPort, User.UserName, User.Password))
            using (var port = new ForwardedPortLocal("localhost", 8084, "www.renci.org", 80))
            {
                client.AddForwardedPort(port);
                port.Exception += (sender, e) =>
                    {
                        Assert.Fail(e.Exception.ToString());
                    };
                port.Start();

                var test = Parallel.For(0,
                                        100,
                                        counter =>
                                            {
                                                var start = DateTime.Now;

#if NET6_0_OR_GREATER
                                                var httpClient = new HttpClient();
                                                using (var response = httpClient.GetAsync(new Uri("http://localhost:8084")).GetAwaiter().GetResult())
                                                {
                                                    var data = ReadStream(response.Content.ReadAsStream());
#else
                                                var request = (HttpWebRequest) WebRequest.Create("http://localhost:8084");
                                                using (var response = (HttpWebResponse) request.GetResponse())
                                                {
                                                    var data = ReadStream(response.GetResponseStream());
#endif // NET6_0_OR_GREATER
                                                    var end = DateTime.Now;

                                                    Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                                                                                  "Request# {2}: Lenght: {0} Time: {1}",
                                                                                  data.Length,
                                                                                  end - start,
                                                                                  counter));
                                                }
                                            });
            }
        }

        private static byte[] ReadStream(Stream stream)
        {
            var buffer = new byte[1024];
            using (var ms = new MemoryStream())
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
