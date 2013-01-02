using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality to perform password authentication.
    /// </summary>
    [TestClass]
    public partial class PasswordAuthenticationMethodTest : TestBase
    {
        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("PasswordAuthenticationMethod: Pass null as username, \"valid\" as password.")]
        [ExpectedException(typeof(ArgumentException))]
        public void Password_Test_Pass_Null_Username()
        {
            new PasswordAuthenticationMethod(null, "valid");
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("PasswordAuthenticationMethod: Pass \"valid\" as username, null as password.")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Password_Test_Pass_Null_Password()
        {
            new PasswordAuthenticationMethod("valid", (string)null);
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("PasswordAuthenticationMethod: Pass \"valid\" as username, \"valid\" as password.")]
        public void Password_Test_Pass_Valid_Username_And_Password()
        {
            new PasswordAuthenticationMethod("valid", "valid");
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("PasswordAuthenticationMethod: Pass String.Empty as username, \"valid\" as password.")]
        [ExpectedException(typeof(ArgumentException))]
        public void Password_Test_Pass_Whitespace()
        {
            new PasswordAuthenticationMethod(string.Empty, "valid");
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("PasswordAuthenticationMethod: Pass \"valid\" as username, String.Empty as password.")]
        public void Password_Test_Pass_Valid()
        {
            new PasswordAuthenticationMethod("valid", string.Empty);
        }

        [TestMethod]
        [WorkItem(1140)]
        [TestCategory("BaseClient")]
        [Description("Test whether IsConnected is false after disconnect.")]
        [Owner("Kenneth_aa")]
        public void Test_BaseClient_IsConnected_True_After_Disconnect()
        {
            // 2012-04-29 - Kenneth_aa
            // The problem with this test, is that after SSH Net calls .Disconnect(), the library doesn't wait
            // for the server to confirm disconnect before IsConnected is checked. And now I'm not mentioning
            // anything about Socket's either.

            var connectionInfo = new PasswordAuthenticationMethod(Resources.USERNAME, Resources.PASSWORD);

            using (SftpClient client = new SftpClient(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD))
            {
                client.Connect();
                Assert.AreEqual<bool>(true, client.IsConnected, "IsConnected is not true after Connect() was called.");

                client.Disconnect();

                Assert.AreEqual<bool>(false, client.IsConnected, "IsConnected is true after Disconnect() was called.");
            }
        }
    }
}