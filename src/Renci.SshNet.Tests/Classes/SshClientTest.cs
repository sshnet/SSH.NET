using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides client connection to SSH server.
    /// </summary>
    [TestClass]
    public class SshClientTest : TestBase
    {
        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("integration")]
        public void Test_Connect_Using_Correct_Password()
        {
            var host = Resources.HOST;
            var username = Resources.USERNAME;
            var password = Resources.PASSWORD;

            #region Example SshClient(host, username) Connect
            using (var client = new SshClient(host, username, password))
            {
                client.Connect();
                //  Do something here
                client.Disconnect();
            }
            #endregion
        }

        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("integration")]
        public void Test_Connect_Handle_HostKeyReceived()
        {
            var host = Resources.HOST;
            var username = Resources.USERNAME;
            var password = Resources.PASSWORD;
            var hostKeyValidated = false;

            #region Example SshClient Connect HostKeyReceived
            using (var client = new SshClient(host, username, password))
            {
                client.HostKeyReceived += delegate(object sender, HostKeyEventArgs e)
                {
                    hostKeyValidated = true;

                    if (e.FingerPrint.SequenceEqual(new byte[] { 0x00, 0x01, 0x02, 0x03 }))
                    {
                        e.CanTrust = true;
                    }
                    else
                    {
                        e.CanTrust = false;
                    }
                };
                client.Connect();
                //  Do something here
                client.Disconnect();
            }
            #endregion

            Assert.IsTrue(hostKeyValidated);
        }

        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("integration")]
        public void Test_Connect_Timeout()
        {
            var host = Resources.HOST;
            var username = Resources.USERNAME;
            var password = Resources.PASSWORD;

            #region Example SshClient Connect Timeout
            var connectionInfo = new PasswordConnectionInfo(host, username, password);

            connectionInfo.Timeout = TimeSpan.FromSeconds(30);

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                //  Do something here
                client.Disconnect();
            }
            #endregion
            Assert.Inconclusive();
        }

        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("integration")]
        public void Test_Connect_Handle_ErrorOccurred()
        {
            var host = Resources.HOST;
            var username = Resources.USERNAME;
            var password = Resources.PASSWORD;
            var exceptionOccured = false;

            #region Example SshClient Connect ErrorOccurred
            using (var client = new SshClient(host, username, password))
            {
                client.ErrorOccurred += delegate(object sender, ExceptionEventArgs e)
                {
                    Console.WriteLine("Error occured: " + e.Exception);
                    exceptionOccured = true;
                };

                client.Connect();
                //  Do something here
                client.Disconnect();
            }
            #endregion
            Assert.IsTrue(exceptionOccured);
        }

        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("integration")]
        [ExpectedException(typeof(SshAuthenticationException))]
        public void Test_Connect_Using_Invalid_Password()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, "invalid password"))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("integration")]
        public void Test_Connect_Using_Rsa_Key_Without_PassPhrase()
        {
            var host = Resources.HOST;
            var username = Resources.USERNAME;
            MemoryStream keyFileStream = new MemoryStream(Encoding.ASCII.GetBytes(Resources.RSA_KEY_WITHOUT_PASS));

            #region Example SshClient(host, username) Connect PrivateKeyFile
            using (var client = new SshClient(host, username, new PrivateKeyFile(keyFileStream)))
            {
                client.Connect();
                client.Disconnect();
            }
            #endregion
        }

        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("integration")]
        public void Test_Connect_Using_RsaKey_With_PassPhrase()
        {
            var host = Resources.HOST;
            var username = Resources.USERNAME;
            var passphrase = Resources.PASSWORD;
            MemoryStream keyFileStream = new MemoryStream(Encoding.ASCII.GetBytes(Resources.RSA_KEY_WITH_PASS));

            #region Example SshClient(host, username) Connect PrivateKeyFile PassPhrase
            using (var client = new SshClient(host, username, new PrivateKeyFile(keyFileStream, passphrase)))
            {
                client.Connect();
                client.Disconnect();
            }
            #endregion
        }

        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("integration")]
        [ExpectedException(typeof(SshPassPhraseNullOrEmptyException))]
        public void Test_Connect_Using_Key_With_Empty_PassPhrase()
        {
            MemoryStream keyFileStream = new MemoryStream(Encoding.ASCII.GetBytes(Resources.RSA_KEY_WITH_PASS));
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, new PrivateKeyFile(keyFileStream, null)))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("integration")]
        public void Test_Connect_Using_DsaKey_Without_PassPhrase()
        {
            MemoryStream keyFileStream = new MemoryStream(Encoding.ASCII.GetBytes(Resources.DSA_KEY_WITHOUT_PASS));
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, new PrivateKeyFile(keyFileStream)))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("integration")]
        public void Test_Connect_Using_DsaKey_With_PassPhrase()
        {
            MemoryStream keyFileStream = new MemoryStream(Encoding.ASCII.GetBytes(Resources.DSA_KEY_WITH_PASS));
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, new PrivateKeyFile(keyFileStream, Resources.PASSWORD)))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("integration")]
        [ExpectedException(typeof(SshAuthenticationException))]
        public void Test_Connect_Using_Invalid_PrivateKey()
        {
            MemoryStream keyFileStream = new MemoryStream(Encoding.ASCII.GetBytes(Resources.INVALID_KEY));
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, new PrivateKeyFile(keyFileStream)))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("integration")]
        public void Test_Connect_Using_Multiple_PrivateKeys()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME,
                new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(Resources.INVALID_KEY))),
                new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(Resources.DSA_KEY_WITH_PASS)), Resources.PASSWORD),
                new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(Resources.RSA_KEY_WITH_PASS)), Resources.PASSWORD),
                new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(Resources.RSA_KEY_WITHOUT_PASS))),
                new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(Resources.DSA_KEY_WITHOUT_PASS)))
                ))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [TestCategory("Authentication")]
        [TestCategory("integration")]
        public void Test_Connect_Then_Reconnect()
        {
            using (var client = new SshClient(Resources.HOST, Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                client.Disconnect();
                client.Connect();
                client.Disconnect();
            }
        }

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

        /// <summary>
        ///A test for CreateShellStream
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void CreateShellStreamTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SshClient target = new SshClient(connectionInfo); // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int bufferSize = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModeValues = null; // TODO: Initialize to an appropriate value
            ShellStream expected = null; // TODO: Initialize to an appropriate value
            ShellStream actual;
            actual = target.CreateShellStream(terminalName, columns, rows, width, height, bufferSize, terminalModeValues);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CreateShellStream
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void CreateShellStreamTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SshClient target = new SshClient(connectionInfo); // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            int bufferSize = 0; // TODO: Initialize to an appropriate value
            ShellStream expected = null; // TODO: Initialize to an appropriate value
            ShellStream actual;
            actual = target.CreateShellStream(terminalName, columns, rows, width, height, bufferSize);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
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

        /// <summary>
        ///A test for CreateShell
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void CreateShellTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SshClient target = new SshClient(connectionInfo); // TODO: Initialize to an appropriate value
            Encoding encoding = null; // TODO: Initialize to an appropriate value
            string input = string.Empty; // TODO: Initialize to an appropriate value
            Stream output = null; // TODO: Initialize to an appropriate value
            Stream extendedOutput = null; // TODO: Initialize to an appropriate value
            Shell expected = null; // TODO: Initialize to an appropriate value
            Shell actual;
            actual = target.CreateShell(encoding, input, output, extendedOutput);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CreateShell
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void CreateShellTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SshClient target = new SshClient(connectionInfo); // TODO: Initialize to an appropriate value
            Encoding encoding = null; // TODO: Initialize to an appropriate value
            string input = string.Empty; // TODO: Initialize to an appropriate value
            Stream output = null; // TODO: Initialize to an appropriate value
            Stream extendedOutput = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModes = null; // TODO: Initialize to an appropriate value
            Shell expected = null; // TODO: Initialize to an appropriate value
            Shell actual;
            actual = target.CreateShell(encoding, input, output, extendedOutput, terminalName, columns, rows, width, height, terminalModes);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CreateShell
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void CreateShellTest2()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SshClient target = new SshClient(connectionInfo); // TODO: Initialize to an appropriate value
            Encoding encoding = null; // TODO: Initialize to an appropriate value
            string input = string.Empty; // TODO: Initialize to an appropriate value
            Stream output = null; // TODO: Initialize to an appropriate value
            Stream extendedOutput = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModes = null; // TODO: Initialize to an appropriate value
            int bufferSize = 0; // TODO: Initialize to an appropriate value
            Shell expected = null; // TODO: Initialize to an appropriate value
            Shell actual;
            actual = target.CreateShell(encoding, input, output, extendedOutput, terminalName, columns, rows, width, height, terminalModes, bufferSize);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CreateShell
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void CreateShellTest3()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SshClient target = new SshClient(connectionInfo); // TODO: Initialize to an appropriate value
            Stream input = null; // TODO: Initialize to an appropriate value
            Stream output = null; // TODO: Initialize to an appropriate value
            Stream extendedOutput = null; // TODO: Initialize to an appropriate value
            Shell expected = null; // TODO: Initialize to an appropriate value
            Shell actual;
            actual = target.CreateShell(input, output, extendedOutput);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CreateShell
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void CreateShellTest4()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SshClient target = new SshClient(connectionInfo); // TODO: Initialize to an appropriate value
            Stream input = null; // TODO: Initialize to an appropriate value
            Stream output = null; // TODO: Initialize to an appropriate value
            Stream extendedOutput = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModes = null; // TODO: Initialize to an appropriate value
            Shell expected = null; // TODO: Initialize to an appropriate value
            Shell actual;
            actual = target.CreateShell(input, output, extendedOutput, terminalName, columns, rows, width, height, terminalModes);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CreateShell
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void CreateShellTest5()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SshClient target = new SshClient(connectionInfo); // TODO: Initialize to an appropriate value
            Stream input = null; // TODO: Initialize to an appropriate value
            Stream output = null; // TODO: Initialize to an appropriate value
            Stream extendedOutput = null; // TODO: Initialize to an appropriate value
            string terminalName = string.Empty; // TODO: Initialize to an appropriate value
            uint columns = 0; // TODO: Initialize to an appropriate value
            uint rows = 0; // TODO: Initialize to an appropriate value
            uint width = 0; // TODO: Initialize to an appropriate value
            uint height = 0; // TODO: Initialize to an appropriate value
            IDictionary<TerminalModes, uint> terminalModes = null; // TODO: Initialize to an appropriate value
            int bufferSize = 0; // TODO: Initialize to an appropriate value
            Shell expected = null; // TODO: Initialize to an appropriate value
            Shell actual;
            actual = target.CreateShell(input, output, extendedOutput, terminalName, columns, rows, width, height, terminalModes, bufferSize);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
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

        /// <summary>
        ///A test for CreateCommand
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void CreateCommandTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SshClient target = new SshClient(connectionInfo); // TODO: Initialize to an appropriate value
            string commandText = string.Empty; // TODO: Initialize to an appropriate value
            Encoding encoding = null; // TODO: Initialize to an appropriate value
            SshCommand expected = null; // TODO: Initialize to an appropriate value
            SshCommand actual;
            actual = target.CreateCommand(commandText, encoding);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CreateCommand
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void CreateCommandTest1()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SshClient target = new SshClient(connectionInfo); // TODO: Initialize to an appropriate value
            string commandText = string.Empty; // TODO: Initialize to an appropriate value
            SshCommand expected = null; // TODO: Initialize to an appropriate value
            SshCommand actual;
            actual = target.CreateCommand(commandText);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
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


        /// <summary>
        ///A test for AddForwardedPort
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void AddForwardedPortTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SshClient target = new SshClient(connectionInfo); // TODO: Initialize to an appropriate value
            ForwardedPort port = null; // TODO: Initialize to an appropriate value
            target.AddForwardedPort(port);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SshClient Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SshClientConstructorTest()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            SshClient target = new SshClient(host, username, keyFiles);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SshClient Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SshClientConstructorTest1()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            SshClient target = new SshClient(host, port, username, keyFiles);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SshClient Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SshClientConstructorTest2()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            SshClient target = new SshClient(host, username, password);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SshClient Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SshClientConstructorTest3()
        {
            string host = string.Empty; // TODO: Initialize to an appropriate value
            int port = 0; // TODO: Initialize to an appropriate value
            string username = string.Empty; // TODO: Initialize to an appropriate value
            string password = string.Empty; // TODO: Initialize to an appropriate value
            SshClient target = new SshClient(host, port, username, password);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for SshClient Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void SshClientConstructorTest4()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SshClient target = new SshClient(connectionInfo);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for RemoveForwardedPort
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void RemoveForwardedPortTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SshClient target = new SshClient(connectionInfo); // TODO: Initialize to an appropriate value
            ForwardedPort port = null; // TODO: Initialize to an appropriate value
            target.RemoveForwardedPort(port);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
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

        /// <summary>
        ///A test for RunCommand
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void RunCommandTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SshClient target = new SshClient(connectionInfo); // TODO: Initialize to an appropriate value
            string commandText = string.Empty; // TODO: Initialize to an appropriate value
            SshCommand expected = null; // TODO: Initialize to an appropriate value
            SshCommand actual;
            actual = target.RunCommand(commandText);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ForwardedPorts
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void ForwardedPortsTest()
        {
            ConnectionInfo connectionInfo = null; // TODO: Initialize to an appropriate value
            SshClient target = new SshClient(connectionInfo); // TODO: Initialize to an appropriate value
            IEnumerable<ForwardedPort> actual;
            actual = target.ForwardedPorts;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

    }
}