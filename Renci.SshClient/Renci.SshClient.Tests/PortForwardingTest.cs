using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshClient.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class PortForwardingTest
    {
        public PortForwardingTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestLocalPortForwarding()
        {
            var client = CreateShellUsingPassword();
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

        [TestMethod]
        public void TestRemotePortForwarding()
        {
            //  ******************************************************************
            //  ************* Tests are still in not finished ********************
            //  ******************************************************************

            var client = CreateShellUsingPassword();
            client.Connect();
            var port1 = client.AddForwardedPort<ForwardedPortRemote>(8082, "www.renci.org", 80);
            port1.Exception += delegate(object sender, ExceptionEventArgs e)
            {
                Assert.Fail(e.Exception.ToString());
            };
            port1.Start();

            System.Threading.Tasks.Parallel.For(0, 100,
                //new ParallelOptions
                //{
                //    MaxDegreeOfParallelism = 1,
                //},
                (counter) =>
                {
                    var start = DateTime.Now;
                    try
                    {

                        var cmd = client.CreateCommand("wget -O- http://localhost:8082");
                        var result = cmd.Execute();
                        var end = DateTime.Now;
                        Debug.Write(string.Format("Length: {0}", result.Length));
                    }
                    catch (Exception exp)
                    {

                        throw;
                    }
                }
            );
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


        private static SshClient CreateShellUsingPassword()
        {
            return new SshClient(ConnectionData.Host, ConnectionData.Port, ConnectionData.Username, ConnectionData.Password);
        }
    }
}
