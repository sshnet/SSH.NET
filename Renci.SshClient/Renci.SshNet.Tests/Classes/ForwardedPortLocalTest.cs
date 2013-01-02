using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;
using System.Diagnostics;
using System.Net;

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
    }
}