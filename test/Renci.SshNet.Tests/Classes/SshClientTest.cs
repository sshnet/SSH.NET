using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides client connection to SSH server.
    /// </summary>
    [TestClass]
    public class SshClientTest : TestBase
    {
        [TestMethod]
        public void CreateShellStream1_NeverConnected()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
            {
                const string terminalName = "vt100";
                const uint columns = 80;
                const uint rows = 25;
                const uint width = 640;
                const uint height = 480;
                const int bufferSize = 4096;

                try
                {
                    client.CreateShellStream(terminalName, columns, rows, width, height, bufferSize);
                    Assert.Fail();
                }
                catch (SshConnectionException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Client not connected.", ex.Message);
                }
            }
        }

        [TestMethod]
        public void CreateShellStream2_NeverConnected()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
            {
                const string terminalName = "vt100";
                const uint columns = 80;
                const uint rows = 25;
                const uint width = 640;
                const uint height = 480;
                var terminalModes = new Dictionary<TerminalModes, uint>();
                const int bufferSize = 4096;

                try
                {
                    client.CreateShellStream(terminalName, columns, rows, width, height, bufferSize, terminalModes);
                    Assert.Fail();
                }
                catch (SshConnectionException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Client not connected.", ex.Message);
                }
            }
        }

        [TestMethod]
        public void CreateShell1_NeverConnected()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
            {
                var encoding = Encoding.UTF8;
                const string input = "INPUT";
                var output = new MemoryStream();
                var extendedOutput = new MemoryStream();

                try
                {
                    client.CreateShell(encoding, input, output, extendedOutput);
                    Assert.Fail();
                }
                catch (SshConnectionException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Client not connected.", ex.Message);
                }
            }
        }

        [TestMethod]
        public void CreateShell2_NeverConnected()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
            {
                var encoding = Encoding.UTF8;
                const string input = "INPUT";
                var output = new MemoryStream();
                var extendedOutput = new MemoryStream();
                const string terminalName = "vt100";
                const uint columns = 80;
                const uint rows = 25;
                const uint width = 640;
                const uint height = 480;
                var terminalModes = new Dictionary<TerminalModes, uint>();

                try
                {
                    client.CreateShell(
                        encoding,
                        input,
                        output,
                        extendedOutput,
                        terminalName,
                        columns,
                        rows,
                        width,
                        height,
                        terminalModes);
                    Assert.Fail();
                }
                catch (SshConnectionException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Client not connected.", ex.Message);
                }
            }
        }

        [TestMethod]
        public void CreateShell3_NeverConnected()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
            {
                var encoding = Encoding.UTF8;
                const string input = "INPUT";
                var output = new MemoryStream();
                var extendedOutput = new MemoryStream();
                const string terminalName = "vt100";
                const uint columns = 80;
                const uint rows = 25;
                const uint width = 640;
                const uint height = 480;
                var terminalModes = new Dictionary<TerminalModes, uint>();
                const int bufferSize = 4096;

                try
                {
                    client.CreateShell(
                        encoding,
                        input,
                        output,
                        extendedOutput,
                        terminalName,
                        columns,
                        rows,
                        width,
                        height,
                        terminalModes,
                        bufferSize);
                    Assert.Fail();
                }
                catch (SshConnectionException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Client not connected.", ex.Message);
                }
            }
        }

        [TestMethod]
        public void CreateShell4_NeverConnected()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
            {
                var input = new MemoryStream();
                var output = new MemoryStream();
                var extendedOutput = new MemoryStream();

                try
                {
                    client.CreateShell(input, output, extendedOutput);
                    Assert.Fail();
                }
                catch (SshConnectionException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Client not connected.", ex.Message);
                }
            }
        }

        [TestMethod]
        public void CreateShell5_NeverConnected()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
            {
                var input = new MemoryStream();
                var output = new MemoryStream();
                var extendedOutput = new MemoryStream();
                const string terminalName = "vt100";
                const uint columns = 80;
                const uint rows = 25;
                const uint width = 640;
                const uint height = 480;
                var terminalModes = new Dictionary<TerminalModes, uint>();

                try
                {
                    client.CreateShell(
                        input,
                        output,
                        extendedOutput,
                        terminalName,
                        columns,
                        rows,
                        width,
                        height,
                        terminalModes);
                    Assert.Fail();
                }
                catch (SshConnectionException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Client not connected.", ex.Message);
                }
            }
        }

        [TestMethod]
        public void CreateShell6_NeverConnected()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
            {
                var input = new MemoryStream();
                var output = new MemoryStream();
                var extendedOutput = new MemoryStream();
                const string terminalName = "vt100";
                const uint columns = 80;
                const uint rows = 25;
                const uint width = 640;
                const uint height = 480;
                var terminalModes = new Dictionary<TerminalModes, uint>();
                const int bufferSize = 4096;

                try
                {
                    client.CreateShell(
                        input,
                        output,
                        extendedOutput,
                        terminalName,
                        columns,
                        rows,
                        width,
                        height,
                        terminalModes,
                        bufferSize);
                    Assert.Fail();
                }
                catch (SshConnectionException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Client not connected.", ex.Message);
                }
            }
        }

        [TestMethod]
        public void CreateCommand_CommandText_NeverConnected()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
            {
                try
                {
                    client.CreateCommand("ls");
                    Assert.Fail();
                }
                catch (SshConnectionException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Client not connected.", ex.Message);
                }
            }
        }

        [TestMethod]
        public void CreateCommand_CommandTextAndEncoding_NeverConnected()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
            {
                try
                {
                    client.CreateCommand("ls", Encoding.UTF8);
                    Assert.Fail();
                }
                catch (SshConnectionException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Client not connected.", ex.Message);
                }
            }
        }

        [TestMethod]
        public void CreateCommand_CommandTextAndIncludeExecutionTimeInResult()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
            {
                try
                {
                    client.CreateCommand("ls", true);
                    Assert.Fail();
                }
                catch (SshConnectionException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Client not connected.", ex.Message);
                }
            }
        }

        [TestMethod]
        public void AddForwardedPort_NeverConnected()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
            {
                var port = new ForwardedPortLocal(50, "host", 8080);

                try
                {
                    client.AddForwardedPort(port);
                    Assert.Fail();
                }
                catch (SshConnectionException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Client not connected.", ex.Message);
                }
            }
        }

        [TestMethod]
        public void RunCommand_CommandText_NeverConnected()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
            {
                try
                {
                    client.RunCommand("ls");
                    Assert.Fail();
                }
                catch (SshConnectionException ex)
                {
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Client not connected.", ex.Message);
                }
            }
        }
    }
}
