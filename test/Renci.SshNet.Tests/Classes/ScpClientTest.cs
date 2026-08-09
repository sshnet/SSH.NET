using System;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Tests.Common;

#pragma warning disable CS0618 // These SCP tests use the obsolete default-transformation constructors.

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides SCP client functionality.
    /// </summary>
    [TestClass]
    public class ScpClientTest : TestBase
    {
        private Random _random;

        [TestInitialize]
        public void SetUp()
        {
            _random = new Random();
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Ctor_ConnectionInfo_Null(bool remoteTransformCtor)
        {
            const ConnectionInfo connectionInfo = null;

            try
            {
                _ = remoteTransformCtor
                    ? new ScpClient(connectionInfo, RemotePathTransformation.ShellQuote)
                    : new ScpClient(connectionInfo);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("connectionInfo", ex.ParamName);
            }
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Ctor_ConnectionInfo_NotNull(bool remoteTransformCtor)
        {
            var connectionInfo = new ConnectionInfo("HOST", "USER", new PasswordAuthenticationMethod("USER", "PWD"));

            ScpClient client;
            if (remoteTransformCtor)
            {
                client = new ScpClient(connectionInfo, RemotePathTransformation.ShellQuote);
                Assert.AreSame(RemotePathTransformation.ShellQuote, client.RemotePathTransformation);
            }
            else
            {
                client = new ScpClient(connectionInfo);
                Assert.AreSame(RemotePathTransformation.DoubleQuote, client.RemotePathTransformation);
            }

            Assert.AreEqual(16 * 1024U, client.BufferSize);
            Assert.AreSame(connectionInfo, client.ConnectionInfo);
            Assert.IsFalse(client.IsConnected);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.KeepAliveInterval);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.OperationTimeout);
            Assert.IsNull(client.Session);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Ctor_HostAndPortAndUsernameAndPassword(bool remoteTransformCtor)
        {
            var host = _random.Next().ToString();
            var port = _random.Next(1, 100);
            var userName = _random.Next().ToString();
            var password = _random.Next().ToString();

            ScpClient client;
            if (remoteTransformCtor)
            {
                client = new ScpClient(host, port, userName, password, RemotePathTransformation.ShellQuote);
                Assert.AreSame(RemotePathTransformation.ShellQuote, client.RemotePathTransformation);
            }
            else
            {
                client = new ScpClient(host, port, userName, password);
                Assert.AreSame(RemotePathTransformation.DoubleQuote, client.RemotePathTransformation);
            }

            Assert.AreEqual(16 * 1024U, client.BufferSize);
            Assert.IsNotNull(client.ConnectionInfo);
            Assert.IsFalse(client.IsConnected);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.KeepAliveInterval);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.OperationTimeout);
            Assert.IsNull(client.Session);

            var passwordConnectionInfo = client.ConnectionInfo as PasswordConnectionInfo;
            Assert.IsNotNull(passwordConnectionInfo);
            Assert.AreEqual(host, passwordConnectionInfo.Host);
            Assert.AreEqual(port, passwordConnectionInfo.Port);
            Assert.AreSame(userName, passwordConnectionInfo.Username);
            Assert.IsNotNull(passwordConnectionInfo.AuthenticationMethods);
            Assert.HasCount(1, passwordConnectionInfo.AuthenticationMethods);

            var passwordAuthentication = passwordConnectionInfo.AuthenticationMethods[0] as PasswordAuthenticationMethod;
            Assert.IsNotNull(passwordAuthentication);
            Assert.AreEqual(userName, passwordAuthentication.Username);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(password), passwordAuthentication.Password);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Ctor_HostAndUsernameAndPassword(bool remoteTransformCtor)
        {
            var host = _random.Next().ToString();
            var userName = _random.Next().ToString();
            var password = _random.Next().ToString();

            ScpClient client;
            if (remoteTransformCtor)
            {
                client = new ScpClient(host, userName, password, RemotePathTransformation.ShellQuote);
                Assert.AreSame(RemotePathTransformation.ShellQuote, client.RemotePathTransformation);
            }
            else
            {
                client = new ScpClient(host, userName, password);
                Assert.AreSame(RemotePathTransformation.DoubleQuote, client.RemotePathTransformation);
            }

            Assert.AreEqual(16 * 1024U, client.BufferSize);
            Assert.IsNotNull(client.ConnectionInfo);
            Assert.IsFalse(client.IsConnected);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.KeepAliveInterval);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.OperationTimeout);
            Assert.IsNull(client.Session);

            var passwordConnectionInfo = client.ConnectionInfo as PasswordConnectionInfo;
            Assert.IsNotNull(passwordConnectionInfo);
            Assert.AreEqual(host, passwordConnectionInfo.Host);
            Assert.AreEqual(22, passwordConnectionInfo.Port);
            Assert.AreSame(userName, passwordConnectionInfo.Username);
            Assert.IsNotNull(passwordConnectionInfo.AuthenticationMethods);
            Assert.HasCount(1, passwordConnectionInfo.AuthenticationMethods);

            var passwordAuthentication = passwordConnectionInfo.AuthenticationMethods[0] as PasswordAuthenticationMethod;
            Assert.IsNotNull(passwordAuthentication);
            Assert.AreEqual(userName, passwordAuthentication.Username);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes(password), passwordAuthentication.Password);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Ctor_HostAndPortAndUsernameAndPrivateKeys(bool remoteTransformCtor)
        {
            var host = _random.Next().ToString();
            var port = _random.Next(1, 100);
            var userName = _random.Next().ToString();
            var privateKeys = new[] { GetRsaKey(), GetEcdsaKey() };

            ScpClient client;
            if (remoteTransformCtor)
            {
                client = new ScpClient(host, port, userName, RemotePathTransformation.ShellQuote, privateKeys);
                Assert.AreSame(RemotePathTransformation.ShellQuote, client.RemotePathTransformation);
            }
            else
            {
                client = new ScpClient(host, port, userName, privateKeys);
                Assert.AreSame(RemotePathTransformation.DoubleQuote, client.RemotePathTransformation);
            }

            Assert.AreEqual(16 * 1024U, client.BufferSize);
            Assert.IsNotNull(client.ConnectionInfo);
            Assert.IsFalse(client.IsConnected);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.KeepAliveInterval);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.OperationTimeout);
            Assert.IsNull(client.Session);

            var privateKeyConnectionInfo = client.ConnectionInfo as PrivateKeyConnectionInfo;
            Assert.IsNotNull(privateKeyConnectionInfo);
            Assert.AreEqual(host, privateKeyConnectionInfo.Host);
            Assert.AreEqual(port, privateKeyConnectionInfo.Port);
            Assert.AreSame(userName, privateKeyConnectionInfo.Username);
            Assert.IsNotNull(privateKeyConnectionInfo.AuthenticationMethods);
            Assert.HasCount(1, privateKeyConnectionInfo.AuthenticationMethods);

            var privateKeyAuthentication = privateKeyConnectionInfo.AuthenticationMethods[0] as PrivateKeyAuthenticationMethod;
            Assert.IsNotNull(privateKeyAuthentication);
            Assert.AreEqual(userName, privateKeyAuthentication.Username);
            Assert.IsNotNull(privateKeyAuthentication.KeyFiles);
            Assert.HasCount(privateKeys.Length, privateKeyAuthentication.KeyFiles);
            Assert.Contains(privateKeys[0], privateKeyAuthentication.KeyFiles);
            Assert.Contains(privateKeys[1], privateKeyAuthentication.KeyFiles);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void Ctor_HostAndUsernameAndPrivateKeys(bool remoteTransformCtor)
        {
            var host = _random.Next().ToString();
            var userName = _random.Next().ToString();
            var privateKeys = new[] { GetRsaKey(), GetEcdsaKey() };

            ScpClient client;
            if (remoteTransformCtor)
            {
                client = new ScpClient(host, userName, RemotePathTransformation.ShellQuote, privateKeys);
                Assert.AreSame(RemotePathTransformation.ShellQuote, client.RemotePathTransformation);
            }
            else
            {
                client = new ScpClient(host, userName, privateKeys);
                Assert.AreSame(RemotePathTransformation.DoubleQuote, client.RemotePathTransformation);
            }

            Assert.AreEqual(16 * 1024U, client.BufferSize);
            Assert.IsNotNull(client.ConnectionInfo);
            Assert.IsFalse(client.IsConnected);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.KeepAliveInterval);
            Assert.AreEqual(new TimeSpan(0, 0, 0, 0, -1), client.OperationTimeout);
            Assert.IsNull(client.Session);

            var privateKeyConnectionInfo = client.ConnectionInfo as PrivateKeyConnectionInfo;
            Assert.IsNotNull(privateKeyConnectionInfo);
            Assert.AreEqual(host, privateKeyConnectionInfo.Host);
            Assert.AreEqual(22, privateKeyConnectionInfo.Port);
            Assert.AreSame(userName, privateKeyConnectionInfo.Username);
            Assert.IsNotNull(privateKeyConnectionInfo.AuthenticationMethods);
            Assert.HasCount(1, privateKeyConnectionInfo.AuthenticationMethods);

            var privateKeyAuthentication = privateKeyConnectionInfo.AuthenticationMethods[0] as PrivateKeyAuthenticationMethod;
            Assert.IsNotNull(privateKeyAuthentication);
            Assert.AreEqual(userName, privateKeyAuthentication.Username);
            Assert.IsNotNull(privateKeyAuthentication.KeyFiles);
            Assert.HasCount(privateKeys.Length, privateKeyAuthentication.KeyFiles);
            Assert.Contains(privateKeys[0], privateKeyAuthentication.KeyFiles);
            Assert.Contains(privateKeys[1], privateKeyAuthentication.KeyFiles);
        }

        [TestMethod]
        public void RemotePathTransformation_Value_NotNull()
        {
            var client = new ScpClient("HOST", 22, "USER", "PWD");

            Assert.AreSame(RemotePathTransformation.DoubleQuote, client.RemotePathTransformation);
            client.RemotePathTransformation = RemotePathTransformation.ShellQuote;
            Assert.AreSame(RemotePathTransformation.ShellQuote, client.RemotePathTransformation);
        }

        [TestMethod]
        public void RemotePathTransformation_Value_Null()
        {
            var client = new ScpClient("HOST", 22, "USER", "PWD")
            {
                RemotePathTransformation = RemotePathTransformation.ShellQuote
            };

            try
            {
                client.RemotePathTransformation = null;
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("value", ex.ParamName);
            }

            Assert.AreSame(RemotePathTransformation.ShellQuote, client.RemotePathTransformation);
        }

        private PrivateKeyFile GetRsaKey()
        {
            using (var stream = GetData("Key.RSA.txt"))
            {
                return new PrivateKeyFile(stream);
            }
        }

        private PrivateKeyFile GetEcdsaKey()
        {
            using (var stream = GetData("Key.ECDSA.txt"))
            {
                return new PrivateKeyFile(stream);
            }
        }
    }
}
