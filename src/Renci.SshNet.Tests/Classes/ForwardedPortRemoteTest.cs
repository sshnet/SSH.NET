using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;
using System.Net;
using System.Threading;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality for remote port forwarding
    /// </summary>
    [TestClass]
    public partial class ForwardedPortRemoteTest : TestBase
    {
        [TestMethod]
        [Description("Test passing null to AddForwardedPort hosts (remote).")]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestCategory("integration")]
        public void Test_AddForwardedPort_Remote_Hosts_Are_Null()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                var port1 = new ForwardedPortRemote((string)null, 8080, (string)null, 80);
                client.AddForwardedPort(port1);
                client.Disconnect();
            }
        }

        [TestMethod]
        [Description("Test passing invalid port numbers to AddForwardedPort.")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [TestCategory("integration")]
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

        /// <summary>
        ///A test for ForwardedPortRemote Constructor
        ///</summary>
        [TestMethod]
        [TestCategory("integration")]
        public void Test_ForwardedPortRemote()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                #region Example SshClient AddForwardedPort Start Stop ForwardedPortRemote
                client.Connect();
                var port = new ForwardedPortRemote(8082, "www.cnn.com", 80);
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


        /// <summary>
        ///A test for Stop
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void StopTest()
        {
            uint boundPort = 0; // TODO: Initialize to an appropriate value
            string host = string.Empty; // TODO: Initialize to an appropriate value
            uint port = 0; // TODO: Initialize to an appropriate value
            ForwardedPortRemote target = new ForwardedPortRemote(boundPort, host, port); // TODO: Initialize to an appropriate value
            target.Stop();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        [TestMethod]
        public void Start_NotAddedToClient()
        {
            const int boundPort = 80;
            string host = string.Empty;
            const uint port = 22;
            var target = new ForwardedPortRemote(boundPort, host, port);

            try
            {
                target.Start();
                Assert.Fail();
            }
            catch (InvalidOperationException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("Forwarded port is not added to a client.", ex.Message);
            }
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void DisposeTest()
        {
            uint boundPort = 0; // TODO: Initialize to an appropriate value
            string host = string.Empty; // TODO: Initialize to an appropriate value
            uint port = 0; // TODO: Initialize to an appropriate value
            ForwardedPortRemote target = new ForwardedPortRemote(boundPort, host, port); // TODO: Initialize to an appropriate value
            target.Dispose();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for ForwardedPortRemote Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void ForwardedPortRemoteConstructorTest()
        {
            string boundHost = string.Empty; // TODO: Initialize to an appropriate value
            uint boundPort = 0; // TODO: Initialize to an appropriate value
            string host = string.Empty; // TODO: Initialize to an appropriate value
            uint port = 0; // TODO: Initialize to an appropriate value
            ForwardedPortRemote target = new ForwardedPortRemote(boundHost, boundPort, host, port);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for ForwardedPortRemote Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void ForwardedPortRemoteConstructorTest1()
        {
            uint boundPort = 0; // TODO: Initialize to an appropriate value
            string host = string.Empty; // TODO: Initialize to an appropriate value
            uint port = 0; // TODO: Initialize to an appropriate value
            ForwardedPortRemote target = new ForwardedPortRemote(boundPort, host, port);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

#if FEATURE_TPL
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
                port1.Exception += delegate (object sender, ExceptionEventArgs e)
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
                        System.Diagnostics.Debug.WriteLine(string.Format("Length: {0}", result.Length));
                    }
                );
                Thread.Sleep(1000 * 100);
                port1.Stop();
            }
        }
#endif // FEATURE_TPL
    }
}