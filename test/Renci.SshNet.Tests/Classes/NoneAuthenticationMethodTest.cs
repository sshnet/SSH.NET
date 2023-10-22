using System;
using System.Globalization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality for "none" authentication method
    /// </summary>
    [TestClass]
    public class NoneAuthenticationMethodTest : TestBase
    {
        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("NoneAuthenticationMethod: Pass null as username.")]
        public void None_Test_Pass_Null()
        {
            NoneAuthenticationMethod nam = null;

            try
            {
                nam = new NoneAuthenticationMethod(null);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(typeof(ArgumentException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("xx", ex.ParamName);
            }
            finally
            {
                nam?.Dispose();
            }
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("NoneAuthenticationMethod: Pass String.Empty as username.")]
        public void None_Test_Pass_Whitespace()
        {
            NoneAuthenticationMethod nam = null;

            try
            {
                nam = new NoneAuthenticationMethod(string.Empty);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(typeof(ArgumentException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("xx", ex.ParamName);
            }
            finally
            {
                nam?.Dispose();
            }
        }

        [TestMethod]
        public void Name()
        {
            var username = new Random().Next().ToString(CultureInfo.InvariantCulture);

            using (var target = new NoneAuthenticationMethod(username))
            {
                Assert.AreEqual("none", target.Name);
            }
        }

        [TestMethod]
        public void Username()
        {
            var username = new Random().Next().ToString(CultureInfo.InvariantCulture);

            using (var target = new NoneAuthenticationMethod(username))
            {
                Assert.AreSame(username, target.Username);
            }
        }
    }
}
