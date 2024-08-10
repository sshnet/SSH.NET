using System.ComponentModel;
using System.Net;
using System.Net.Sockets;

using Renci.SshNet.Common;
using Renci.SshNet.IntegrationTests.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.IntegrationTests
{
    [TestClass]
    public class SshTests : TestBase
    {
        private IConnectionInfoFactory _connectionInfoFactory;
        private IConnectionInfoFactory _adminConnectionInfoFactory;
        private RemoteSshdConfig _remoteSshdConfig;

        [TestInitialize]
        public void SetUp()
        {
            _connectionInfoFactory = new LinuxVMConnectionFactory(SshServerHostName, SshServerPort);
            _adminConnectionInfoFactory = new LinuxAdminConnectionFactory(SshServerHostName, SshServerPort);

            _remoteSshdConfig = new RemoteSshd(_adminConnectionInfoFactory).OpenConfig();
            _remoteSshdConfig.AllowTcpForwarding()
                             .PermitTTY(true)
                             .PrintMotd(false)
                             .Update()
                             .Restart();
        }

        [TestCleanup]
        public void TearDown()
        {
            _remoteSshdConfig?.Reset();
        }

        /// <summary>
        /// Test for a channel that is being closed by the server.
        /// </summary>
        [TestMethod]
        public void Ssh_ShellStream_Exit()
        {
            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                var terminalModes = new Dictionary<TerminalModes, uint>
                    {
                        { TerminalModes.ECHO, 0 }
                    };

                using (var shellStream = client.CreateShellStream("xterm", 80, 24, 800, 600, 1024, terminalModes))
                {
                    shellStream.WriteLine("echo Hello!");
                    shellStream.WriteLine("exit");

                    Thread.Sleep(1000);

                    try
                    {
                        shellStream.Write("ABC");
                        Assert.Fail();
                    }
                    catch (ObjectDisposedException ex)
                    {
                        Assert.IsNull(ex.InnerException);
                        Assert.AreEqual("Renci.SshNet.ShellStream", ex.ObjectName);
                        Assert.AreEqual($"Cannot access a disposed object.{Environment.NewLine}Object name: '{ex.ObjectName}'.", ex.Message);
                    }

                    var line = shellStream.ReadLine();
                    Assert.IsNotNull(line);
                    Assert.IsTrue(line.EndsWith("Hello!"), line);

                    Assert.IsTrue(shellStream.ReadLine() is null || shellStream.ReadLine() is null); // we might first get e.g. "renci-ssh-tests-server:~$"
                }
            }
        }

        [TestMethod]
        public void Ssh_CreateShellStreamNoTerminal()
        {
            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var shellStream = client.CreateShellStreamNoTerminal(bufferSize: 1024))
                {
                    var foo = new string('a', 90);
                    shellStream.WriteLine($"echo {foo}");
                    var line = shellStream.ReadLine(TimeSpan.FromSeconds(1));
                    Assert.IsNotNull(line);
                    Assert.IsTrue(line.EndsWith(foo), line);
                }
            }
        }

        /// <summary>
        /// https://github.com/sshnet/SSH.NET/issues/63
        /// </summary>
        [TestMethod]
        [Category("Reproduction Tests")]
        public void Ssh_ShellStream_IntermittendOutput()
        {
            const string remoteFile = "/home/sshnet/test.sh";

            List<string> expectedLines = ["renci-ssh-tests-server:~$ Line 1 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
                "Line 2 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
                "Line 3 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
                "Line 4 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
                "Line 5 ",
                "Line 6",
                "renci-ssh-tests-server:~$ "]; // No idea how stable this is.

            var scriptBuilder = new StringBuilder();
            scriptBuilder.Append("#!/bin/sh\n");
            scriptBuilder.Append("echo Line 1 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\n");
            scriptBuilder.Append("sleep .5\n");
            scriptBuilder.Append("echo Line 2 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\n");
            scriptBuilder.Append("echo Line 3 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\n");
            scriptBuilder.Append("echo Line 4 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\n");
            scriptBuilder.Append("sleep 2\n");
            scriptBuilder.Append("echo \"Line 5 \"\n");
            scriptBuilder.Append("echo Line 6 \n");
            scriptBuilder.Append("exit 13\n");

            using (var sshClient = new SshClient(_connectionInfoFactory.Create()))
            {
                sshClient.Connect();

                CreateShellScript(_connectionInfoFactory, remoteFile, scriptBuilder.ToString());

                try
                {
                    var terminalModes = new Dictionary<TerminalModes, uint>
                    {
                        { TerminalModes.ECHO, 0 }
                    };

                    using (var shellStream = sshClient.CreateShellStream("xterm", 80, 24, 800, 600, 1024, terminalModes))
                    {
                        shellStream.WriteLine(remoteFile);
                        shellStream.WriteLine("exit");
                        using (var reader = new StreamReader(shellStream))
                        {
                            var lines = new List<string>();
                            string line = null;
                            while ((line = reader.ReadLine()) != null)
                            {
                                lines.Add(line);
                            }

                            CollectionAssert.AreEqual(expectedLines, lines, string.Join("\n", lines));
                        }
                    }
                }
                finally
                {
                    RemoveFileOrDirectory(sshClient, remoteFile);
                }
            }
        }

        /// <summary>
        /// Issue 1555
        /// </summary>
        [TestMethod]
        public void Ssh_CreateShell()
        {
            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var input = new MemoryStream())
                using (var output = new MemoryStream())
                using (var extOutput = new MemoryStream())
                {
                    var shell = client.CreateShell(input, output, extOutput);
                    shell.Start();

                    var inputWriter = new StreamWriter(input, Encoding.ASCII, 1024);
                    inputWriter.WriteLine("echo $PATH");

                    var outputReader = new StreamReader(output, Encoding.ASCII, false, 1024);
                    Console.WriteLine(outputReader.ReadToEnd());

                    shell.Stop();
                }
            }
        }

        [TestMethod]
        public void Ssh_CreateShellNoTerminal()
        {
            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                using (var input = new MemoryStream())
                using (var output = new MemoryStream())
                using (var extOutput = new MemoryStream())
                {
                    var shell = client.CreateShellNoTerminal(input, output, extOutput, 1024);

                    shell.Start();

                    var inputWriter = new StreamWriter(input, Encoding.ASCII, 1024);
                    var foo = new string('a', 90);
                    inputWriter.WriteLine($"echo {foo}");
                    inputWriter.Flush();
                    input.Position = 0;

                    Thread.Sleep(1000);

                    output.Position = 0;
                    var outputReader = new StreamReader(output, Encoding.ASCII, false, 1024);
                    var outputString = outputReader.ReadLine();

                    Assert.IsNotNull(outputString);
                    Assert.IsTrue(outputString.EndsWith(foo), outputString);

                    shell.Stop();
                }
            }
        }

        [TestMethod]
        public void Ssh_Command_IntermittentOutput_EndExecute()
        {
            const string remoteFile = "/home/sshnet/test.sh";

            var expectedResult = string.Join("\n",
                                             "Line 1 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
                                             "Line 2 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
                                             "Line 3 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
                                             "Line 4 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
                                             "Line 5 ",
                                             "Line 6",
                                             "");

            var scriptBuilder = new StringBuilder();
            scriptBuilder.Append("#!/bin/sh\n");
            scriptBuilder.Append("echo Line 1 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\n");
            scriptBuilder.Append("sleep .5\n");
            scriptBuilder.Append("echo Line 2 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\n");
            scriptBuilder.Append("echo Line 3 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\n");
            scriptBuilder.Append("echo Line 4 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\n");
            scriptBuilder.Append("sleep 2\n");
            scriptBuilder.Append("echo \"Line 5 \"\n");
            scriptBuilder.Append("echo Line 6 \n");
            scriptBuilder.Append("exit 13\n");

            using (var sshClient = new SshClient(_connectionInfoFactory.Create()))
            {
                sshClient.Connect();

                CreateShellScript(_connectionInfoFactory, remoteFile, scriptBuilder.ToString());

                try
                {
                    using (var cmd = sshClient.CreateCommand("chmod 777 " + remoteFile))
                    {
                        cmd.Execute();

                        Assert.AreEqual(0, cmd.ExitStatus, cmd.Error);
                    }

                    using (var command = sshClient.CreateCommand(remoteFile))
                    {
                        var asyncResult = command.BeginExecute();
                        var actualResult = command.EndExecute(asyncResult);

                        Assert.AreEqual(expectedResult, actualResult);
                        Assert.AreEqual(expectedResult, command.Result);
                        Assert.AreEqual(13, command.ExitStatus);
                    }
                }
                finally
                {
                    RemoveFileOrDirectory(sshClient, remoteFile);
                }
            }
        }

        [TestMethod]
        public async Task Ssh_Command_IntermittentOutput_OutputStream()
        {
            const string remoteFile = "/home/sshnet/test.sh";

            var expectedResult = string.Join("\n",
                                             "Line 1 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
                                             "Line 2 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
                                             "Line 3 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
                                             "Line 4 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
                                             "Line 5 ",
                                             "Line 6");

            var scriptBuilder = new StringBuilder();
            scriptBuilder.Append("#!/bin/sh\n");
            scriptBuilder.Append("echo Line 1 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\n");
            scriptBuilder.Append("sleep .5\n");
            scriptBuilder.Append("echo Line 2 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\n");
            scriptBuilder.Append("echo Line 3 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\n");
            scriptBuilder.Append("echo Line 4 xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\n");
            scriptBuilder.Append("sleep 2\n");
            scriptBuilder.Append("echo \"Line 5 \"\n");
            scriptBuilder.Append("echo Line 6 \n");
            scriptBuilder.Append("exit 13\n");

            using (var sshClient = new SshClient(_connectionInfoFactory.Create()))
            {
                sshClient.Connect();

                CreateShellScript(_connectionInfoFactory, remoteFile, scriptBuilder.ToString());

                try
                {
                    using (var cmd = sshClient.CreateCommand("chmod 777 " + remoteFile))
                    {
                        cmd.Execute();

                        Assert.AreEqual(0, cmd.ExitStatus, cmd.Error);
                    }

                    using (var command = sshClient.CreateCommand(remoteFile))
                    {
                        await command.ExecuteAsync();

                        Assert.AreEqual(13, command.ExitStatus);

                        using (var reader = new StreamReader(command.OutputStream))
                        {
                            var lines = new List<string>();
                            string line = null;
                            while ((line = reader.ReadLine()) != null)
                            {
                                lines.Add(line);
                            }

                            Assert.AreEqual(6, lines.Count, string.Join("\n", lines));
                            Assert.AreEqual(expectedResult, string.Join("\n", lines));
                        }

                        // We have already consumed OutputStream ourselves, so we expect Result to be empty.
                        Assert.AreEqual("", command.Result);
                    }
                }
                finally
                {
                    RemoveFileOrDirectory(sshClient, remoteFile);
                }
            }
        }

        [TestMethod]
        public void Ssh_DynamicPortForwarding_DisposeSshClientWithoutStoppingPort()
        {
            const string searchText = "HTTP/1.1 301 Moved Permanently";
            const string hostName = "github.com";

            var httpGetRequest = Encoding.ASCII.GetBytes($"GET / HTTP/1.1\r\nHost: {hostName}\r\n\r\n");
            Socket socksSocket;

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(200);
                client.Connect();

                var forwardedPort = new ForwardedPortDynamic(1080);
                forwardedPort.Exception += (sender, args) => Console.WriteLine(args.Exception.ToString());
                client.AddForwardedPort(forwardedPort);
                forwardedPort.Start();

                var socksClient = new Socks5Handler(new IPEndPoint(IPAddress.Loopback, 1080),
                                                    string.Empty,
                                                    string.Empty);

                socksSocket = socksClient.Connect(hostName, 80);
                socksSocket.Send(httpGetRequest);

                var httpResponse = GetHttpResponse(socksSocket, Encoding.ASCII);
                Assert.IsTrue(httpResponse.Contains(searchText), httpResponse);
            }

            Assert.IsTrue(socksSocket.Connected);

            // check if client socket was properly closed
            Assert.AreEqual(0, socksSocket.Receive(new byte[1], 0, 1, SocketFlags.None));
        }

        [TestMethod]
        public void Ssh_DynamicPortForwarding_DomainName()
        {
            const string searchText = "HTTP/1.1 301 Moved Permanently";
            const string hostName = "github.com";

            // Set-up a host alias for google.be on the remote server that is not known locally; this allows us to
            // verify whether the host name is resolved remotely.
            const string hostNameAlias = "dynamicportforwarding-test.for.sshnet";

            // Construct a HTTP request for which we expected the response to contain the search text.
            var httpGetRequest = Encoding.ASCII.GetBytes($"GET / HTTP/1.1\r\nHost: {hostName}\r\n\r\n");

            var ipAddresses = Dns.GetHostAddresses(hostName);
            var hostsFileUpdated = AddOrUpdateHostsEntry(_adminConnectionInfoFactory, ipAddresses[0], hostNameAlias);

            try
            {
                using (var client = new SshClient(_connectionInfoFactory.Create()))
                {
                    client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(200);
                    client.Connect();

                    var forwardedPort = new ForwardedPortDynamic(1080);
                    forwardedPort.Exception += (sender, args) => Console.WriteLine(args.Exception.ToString());
                    client.AddForwardedPort(forwardedPort);
                    forwardedPort.Start();

                    var socksClient = new Socks5Handler(new IPEndPoint(IPAddress.Loopback, 1080),
                                                        string.Empty,
                                                        string.Empty);
                    var socksSocket = socksClient.Connect(hostNameAlias, 80);

                    socksSocket.Send(httpGetRequest);
                    var httpResponse = GetHttpResponse(socksSocket, Encoding.ASCII);
                    Assert.IsTrue(httpResponse.Contains(searchText), httpResponse);

                    // Verify if port is still open
                    socksSocket.Send(httpGetRequest);
                    GetHttpResponse(socksSocket, Encoding.ASCII);

                    forwardedPort.Stop();

                    Assert.IsTrue(socksSocket.Connected);

                    // check if client socket was properly closed
                    Assert.AreEqual(0, socksSocket.Receive(new byte[1], 0, 1, SocketFlags.None));

                    forwardedPort.Start();

                    // create new SOCKS connection and very whether the forwarded port is functional again
                    socksSocket = socksClient.Connect(hostNameAlias, 80);

                    socksSocket.Send(httpGetRequest);
                    httpResponse = GetHttpResponse(socksSocket, Encoding.ASCII);
                    Assert.IsTrue(httpResponse.Contains(searchText), httpResponse);

                    forwardedPort.Dispose();

                    Assert.IsTrue(socksSocket.Connected);

                    // check if client socket was properly closed
                    Assert.AreEqual(0, socksSocket.Receive(new byte[1], 0, 1, SocketFlags.None));

                    forwardedPort.Dispose();
                }
            }
            finally
            {
                if (hostsFileUpdated)
                {
                    RemoveHostsEntry(_adminConnectionInfoFactory, ipAddresses[0], hostNameAlias);
                }
            }
        }

        [TestMethod]
        public void Ssh_DynamicPortForwarding_IPv4()
        {
            const string searchText = "HTTP/1.1 301 Moved Permanently";
            const string hostName = "github.com";

            var httpGetRequest = Encoding.ASCII.GetBytes($"GET /null HTTP/1.1\r\nHost: {hostName}\r\n\r\n");

            var ipv4 = Dns.GetHostAddresses(hostName).FirstOrDefault(p => p.AddressFamily == AddressFamily.InterNetwork);
            Assert.IsNotNull(ipv4, $@"No IPv4 address found for '{hostName}'.");

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(200);
                client.Connect();

                var forwardedPort = new ForwardedPortDynamic(1080);
                forwardedPort.Exception += (sender, args) => Console.WriteLine(args.Exception.ToString());
                client.AddForwardedPort(forwardedPort);
                forwardedPort.Start();

                var socksClient = new Socks5Handler(new IPEndPoint(IPAddress.Loopback, 1080),
                                                    string.Empty,
                                                    string.Empty);
                var socksSocket = socksClient.Connect(new IPEndPoint(ipv4, 80));

                socksSocket.Send(httpGetRequest);
                var httpResponse = GetHttpResponse(socksSocket, Encoding.ASCII);
                Assert.IsTrue(httpResponse.Contains(searchText), httpResponse);

                forwardedPort.Dispose();

                // check if client socket was properly closed
                Assert.AreEqual(0, socksSocket.Receive(new byte[1], 0, 1, SocketFlags.None));
            }
        }

        /// <summary>
        /// Verifies whether channels are effectively closed.
        /// </summary>
        [TestMethod]
        public void Ssh_LocalPortForwardingCloseChannels()
        {
            const string hostNameAlias = "localportforwarding-test.for.sshnet";
            const string hostName = "github.com";

            var ipAddress = Dns.GetHostAddresses(hostName)[0];

            var hostsFileUpdated = AddOrUpdateHostsEntry(_adminConnectionInfoFactory, ipAddress, hostNameAlias);

            try
            {
                var connectionInfo = _connectionInfoFactory.Create();
                connectionInfo.MaxSessions = 1;

                using (var client = new SshClient(connectionInfo))
                {
                    client.Connect();

                    var localEndPoint = new IPEndPoint(IPAddress.Loopback, 1225);

                    for (var i = 0; i < (connectionInfo.MaxSessions + 1); i++)
                    {
                        var forwardedPort = new ForwardedPortLocal(localEndPoint.Address.ToString(),
                                                                   (uint)localEndPoint.Port,
                                                                   hostNameAlias,
                                                                   80);
                        client.AddForwardedPort(forwardedPort);
                        forwardedPort.Start();

                        try
                        {
                            var httpRequest = (HttpWebRequest)WebRequest.Create("http://" + localEndPoint);
                            httpRequest.Host = hostName;
                            httpRequest.Method = "GET";
                            httpRequest.AllowAutoRedirect = false;

                            try
                            {
                                using (var httpResponse = (HttpWebResponse)httpRequest.GetResponse())
                                {
                                    Assert.AreEqual(HttpStatusCode.MovedPermanently, httpResponse.StatusCode);
                                }
                            }
                            catch (WebException ex)
                            {
                                Assert.AreEqual(WebExceptionStatus.ProtocolError, ex.Status);
                                Assert.IsNotNull(ex.Response);

                                using (var httpResponse = ex.Response as HttpWebResponse)
                                {
                                    Assert.IsNotNull(httpResponse);
                                    Assert.AreEqual(HttpStatusCode.MovedPermanently, httpResponse.StatusCode);
                                }
                            }
                        }
                        finally
                        {
                            client.RemoveForwardedPort(forwardedPort);
                        }
                    }
                }
            }
            finally
            {
                if (hostsFileUpdated)
                {
                    RemoveHostsEntry(_adminConnectionInfoFactory, ipAddress, hostNameAlias);
                }
            }
        }

        [TestMethod]
        public void Ssh_LocalPortForwarding()
        {
            const string hostNameAlias = "localportforwarding-test.for.sshnet";
            const string hostName = "github.com";

            var ipAddress = Dns.GetHostAddresses(hostName)[0];

            var hostsFileUpdated = AddOrUpdateHostsEntry(_adminConnectionInfoFactory, ipAddress, hostNameAlias);

            try
            {
                using (var client = new SshClient(_connectionInfoFactory.Create()))
                {
                    client.Connect();

                    var localEndPoint = new IPEndPoint(IPAddress.Loopback, 1225);

                    var forwardedPort = new ForwardedPortLocal(localEndPoint.Address.ToString(),
                                                               (uint)localEndPoint.Port,
                                                               hostNameAlias,
                                                               80);
                    forwardedPort.Exception +=
                        (sender, args) => Console.WriteLine(@"ForwardedPort exception: " + args.Exception);
                    client.AddForwardedPort(forwardedPort);
                    forwardedPort.Start();

                    try
                    {
                        var httpRequest = (HttpWebRequest)WebRequest.Create("http://" + localEndPoint);
                        httpRequest.Host = hostName;
                        httpRequest.Method = "GET";
                        httpRequest.Accept = "text/html";
                        httpRequest.AllowAutoRedirect = false;

                        try
                        {
                            using (var httpResponse = (HttpWebResponse)httpRequest.GetResponse())
                            {
                                Assert.AreEqual(HttpStatusCode.MovedPermanently, httpResponse.StatusCode);
                            }
                        }
                        catch (WebException ex)
                        {
                            Assert.AreEqual(WebExceptionStatus.ProtocolError, ex.Status);
                            Assert.IsNotNull(ex.Response);

                            using (var httpResponse = ex.Response as HttpWebResponse)
                            {
                                Assert.IsNotNull(httpResponse);
                                Assert.AreEqual(HttpStatusCode.MovedPermanently, httpResponse.StatusCode);
                            }
                        }
                    }
                    finally
                    {
                        client.RemoveForwardedPort(forwardedPort);
                    }
                }
            }
            finally
            {
                if (hostsFileUpdated)
                {
                    RemoveHostsEntry(_adminConnectionInfoFactory, ipAddress, hostNameAlias);
                }
            }
        }

        [TestMethod]
        public void Ssh_RemotePortForwarding()
        {
            var hostAddresses = Dns.GetHostAddresses(Dns.GetHostName());
            var ipv4HostAddress = hostAddresses.First(p => p.AddressFamily == AddressFamily.InterNetwork);

            var endpoint1 = new IPEndPoint(ipv4HostAddress, 10000);
            var endpoint2 = new IPEndPoint(ipv4HostAddress, 10001);

            var areBytesReceivedOnListener1 = false;
            var areBytesReceivedOnListener2 = false;

            var bytesReceivedOnListener1 = new List<byte>();
            var bytesReceivedOnListener2 = new List<byte>();

            using (var socketListener1 = new AsyncSocketListener(endpoint1))
            using (var socketListener2 = new AsyncSocketListener(endpoint2))
            using (var bytesReceivedEventOnListener1 = new AutoResetEvent(false))
            using (var bytesReceivedEventOnListener2 = new AutoResetEvent(false))
            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                socketListener1.BytesReceived += (received, socket) =>
                {
                    bytesReceivedOnListener1.AddRange(received);
                    bytesReceivedEventOnListener1.Set();
                };

                socketListener1.Start();

                socketListener2.BytesReceived += (received, socket) =>
                {
                    bytesReceivedOnListener2.AddRange(received);
                    bytesReceivedEventOnListener2.Set();
                };

                socketListener2.Start();

                client.Connect();

                var forwardedPort1 = new ForwardedPortRemote(IPAddress.Loopback,
                                                             10002,
                                                             endpoint1.Address,
                                                             (uint)endpoint1.Port);
                forwardedPort1.Exception += (sender, args) => Console.WriteLine(@"forwardedPort1 exception: " + args.Exception);
                client.AddForwardedPort(forwardedPort1);
                forwardedPort1.Start();

                var forwardedPort2 = new ForwardedPortRemote(IPAddress.Loopback,
                                                             10003,
                                                             endpoint2.Address,
                                                             (uint)endpoint2.Port);
                forwardedPort2.Exception += (sender, args) => Console.WriteLine(@"forwardedPort2 exception: " + args.Exception);
                client.AddForwardedPort(forwardedPort2);
                forwardedPort2.Start();

                using (var s = client.CreateShellStream("a", 80, 25, 800, 600, 200))
                {
                    s.WriteLine($"telnet {forwardedPort1.BoundHost} {forwardedPort1.BoundPort}");
                    s.Expect($"Connected to {forwardedPort1.BoundHost}\r\n");
                    s.WriteLine("ABC");
                    s.Flush();
                    s.Expect("ABC");
                    s.Close();
                }

                using (var s = client.CreateShellStream("b", 80, 25, 800, 600, 200))
                {
                    s.WriteLine($"telnet {forwardedPort2.BoundHost} {forwardedPort2.BoundPort}");
                    s.Expect($"Connected to {forwardedPort2.BoundHost}\r\n");
                    s.WriteLine("DEF");
                    s.Flush();
                    s.Expect("DEF");
                    s.Close();
                }

                areBytesReceivedOnListener1 = bytesReceivedEventOnListener1.WaitOne(1000);
                areBytesReceivedOnListener2 = bytesReceivedEventOnListener2.WaitOne(1000);

                forwardedPort1.Stop();
                forwardedPort2.Stop();
            }

            Assert.IsTrue(areBytesReceivedOnListener1);
            Assert.IsTrue(areBytesReceivedOnListener2);

            var textReceivedOnListener1 = Encoding.ASCII.GetString(bytesReceivedOnListener1.ToArray());
            Assert.AreEqual("ABC\r\n", textReceivedOnListener1);

            var textReceivedOnListener2 = Encoding.ASCII.GetString(bytesReceivedOnListener2.ToArray());
            Assert.AreEqual("DEF\r\n", textReceivedOnListener2);
        }

        /// <summary>
        /// Issue 1591
        /// </summary>
        [TestMethod]
        public void Ssh_ExecuteShellScript()
        {
            const string remoteFile = "/home/sshnet/run.sh";
            const string content = "#\bin\bash\necho Hello World!";

            using (var client = new SftpClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                if (client.Exists(remoteFile))
                {
                    client.DeleteFile(remoteFile);
                }

                using (var memoryStream = new MemoryStream())
                using (var sw = new StreamWriter(memoryStream, Encoding.ASCII))
                {
                    sw.Write(content);
                    sw.Flush();
                    memoryStream.Position = 0;
                    client.UploadFile(memoryStream, remoteFile);
                }
            }

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.Connect();

                try
                {
                    var runChmod = client.RunCommand("chmod u+x " + remoteFile);
                    runChmod.Execute();
                    Assert.AreEqual(0, runChmod.ExitStatus, runChmod.Error);

                    var runLs = client.RunCommand("ls " + remoteFile);
                    var asyncResultLs = runLs.BeginExecute();

                    var runScript = client.RunCommand(remoteFile);
                    var asyncResultScript = runScript.BeginExecute();

                    Assert.IsTrue(asyncResultScript.AsyncWaitHandle.WaitOne(10000));
                    var resultScript = runScript.EndExecute(asyncResultScript);
                    Assert.AreEqual("Hello World!\n", resultScript);

                    Assert.IsTrue(asyncResultLs.AsyncWaitHandle.WaitOne(10000));
                    var resultLs = runLs.EndExecute(asyncResultLs);
                    Assert.AreEqual(remoteFile + "\n", resultLs);
                }
                finally
                {
                    RemoveFileOrDirectory(client, remoteFile);
                }
            }
        }

        /// <summary>
        /// Verifies if a hosts file contains an entry for a given combination of IP address and hostname,
        /// and if necessary add either the host entry or an alias to an exist entry for the specified IP
        /// address.
        /// </summary>
        /// <param name="linuxAdminConnectionFactory"></param>
        /// <param name="ipAddress"></param>
        /// <param name="hostName"></param>
        /// <returns>
        /// <see langword="true"/> if an entry was added or updated in the specified hosts file; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        private static bool AddOrUpdateHostsEntry(IConnectionInfoFactory linuxAdminConnectionFactory,
                                                  IPAddress ipAddress,
                                                  string hostName)
        {
            const string hostsFile = "/etc/hosts";

            using (var client = new ScpClient(linuxAdminConnectionFactory.Create()))
            {
                client.Connect();

                var hostConfig = HostConfig.Read(client, hostsFile);

                var hostEntry = hostConfig.Entries.SingleOrDefault(h => h.IPAddress.Equals(ipAddress));
                if (hostEntry != null)
                {
                    if (hostEntry.HostName == hostName)
                    {
                        return false;
                    }

                    foreach (var alias in hostEntry.Aliases)
                    {
                        if (alias == hostName)
                        {
                            return false;
                        }
                    }

                    hostEntry.Aliases.Add(hostName);
                }
                else
                {
                    bool mappingFound = false;

                    for (var i = (hostConfig.Entries.Count - 1); i >= 0; i--)
                    {
                        hostEntry = hostConfig.Entries[i];

                        if (hostEntry.HostName == hostName)
                        {
                            if (hostEntry.IPAddress.Equals(ipAddress))
                            {
                                mappingFound = true;
                                continue;
                            }

                            // If hostname is currently mapped to another IP address, then remove the 
                            // current mapping
                            hostConfig.Entries.RemoveAt(i);
                        }
                        else
                        {
                            for (var j = (hostEntry.Aliases.Count - 1); j >= 0; j--)
                            {
                                var alias = hostEntry.Aliases[j];

                                if (alias == hostName)
                                {
                                    hostEntry.Aliases.RemoveAt(j);
                                }
                            }
                        }
                    }

                    if (!mappingFound)
                    {
                        hostEntry = new HostEntry(ipAddress, hostName);
                        hostConfig.Entries.Add(hostEntry);
                    }
                }

                hostConfig.Write(client, hostsFile);
                return true;
            }
        }

        /// <summary>
        /// Remove the mapping between a given IP address and host name from the remote hosts file either by
        /// removing a host entry entirely (if no other aliases are defined for the IP address) or removing
        /// the aliases that match the host name for the IP address.
        /// </summary>
        /// <param name="linuxAdminConnectionFactory"></param>
        /// <param name="ipAddress"></param>
        /// <param name="hostName"></param>
        /// <returns>
        /// <see langword="true"/> if the hosts file was updated; otherwise, <see langword="false"/>.
        /// </returns>
        private static bool RemoveHostsEntry(IConnectionInfoFactory linuxAdminConnectionFactory,
                                             IPAddress ipAddress,
                                             string hostName)
        {
            const string hostsFile = "/etc/hosts";

            using (var client = new ScpClient(linuxAdminConnectionFactory.Create()))
            {
                client.Connect();

                var hostConfig = HostConfig.Read(client, hostsFile);

                var hostEntry = hostConfig.Entries.SingleOrDefault(h => h.IPAddress.Equals(ipAddress));
                if (hostEntry == null)
                {
                    return false;
                }

                if (hostEntry.HostName == hostName)
                {
                    if (hostEntry.Aliases.Count == 0)
                    {
                        hostConfig.Entries.Remove(hostEntry);
                    }
                    else
                    {
                        // Use one of the aliases (that are different from the specified host name) as host name
                        // of the host entry.

                        for (var i = hostEntry.Aliases.Count - 1; i >= 0; i--)
                        {
                            var alias = hostEntry.Aliases[i];
                            if (alias == hostName)
                            {
                                hostEntry.Aliases.RemoveAt(i);
                            }
                            else if (hostEntry.HostName == hostName)
                            {
                                // If we haven't already used one of the aliases as host name of the host entry
                                // then do this now and remove the alias.

                                hostEntry.HostName = alias;
                                hostEntry.Aliases.RemoveAt(i);
                            }
                        }

                        // If for some reason the host name of the host entry matched the specified host name
                        // and it only had aliases that match the host name, then remove the host entry altogether.
                        if (hostEntry.Aliases.Count == 0 && hostEntry.HostName == hostName)
                        {
                            hostConfig.Entries.Remove(hostEntry);
                        }
                    }
                }
                else
                {
                    var aliasRemoved = false;

                    for (var i = hostEntry.Aliases.Count - 1; i >= 0; i--)
                    {
                        if (hostEntry.Aliases[i] == hostName)
                        {
                            hostEntry.Aliases.RemoveAt(i);
                            aliasRemoved = true;
                        }
                    }

                    if (!aliasRemoved)
                    {
                        return false;
                    }
                }

                hostConfig.Write(client, hostsFile);
                return true;
            }
        }

        private static string GetHttpResponse(Socket socket, Encoding encoding)
        {
            var httpResponseBuffer = new byte[2048];

            // We expect:
            // * The response to contain the searchText in the first receive.
            // * The full response to be returned in the first receive.

            var bytesReceived = socket.Receive(httpResponseBuffer,
                                               0,
                                               httpResponseBuffer.Length,
                                               SocketFlags.None);
            if (bytesReceived == 0)
            {
                return null;
            }

            if (bytesReceived == httpResponseBuffer.Length)
            {
                throw new Exception("We expect the HTTP response to be less than the buffer size. If not, we won't consume the full response.");
            }

            using (var sr = new StringReader(encoding.GetString(httpResponseBuffer, 0, bytesReceived)))
            {
                return sr.ReadToEnd();
            }
        }

        private static void CreateShellScript(IConnectionInfoFactory connectionInfoFactory, string remoteFile, string script)
        {
            using (var sftpClient = new SftpClient(connectionInfoFactory.Create()))
            {
                sftpClient.Connect();

                using (var sw = sftpClient.CreateText(remoteFile, new UTF8Encoding(false)))
                {
                    sw.Write(script);
                }

                sftpClient.ChangePermissions(remoteFile, 0x1FF);
            }
        }

        private static void RemoveFileOrDirectory(SshClient client, string remoteFile)
        {
            using (var cmd = client.CreateCommand("rm -Rf " + remoteFile))
            {
                cmd.Execute();
                Assert.AreEqual(0, cmd.ExitStatus, cmd.Error);
            }
        }
    }
}
