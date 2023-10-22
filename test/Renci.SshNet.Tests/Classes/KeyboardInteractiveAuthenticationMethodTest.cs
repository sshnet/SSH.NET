using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality to perform keyboard interactive authentication.
    /// </summary>
    [TestClass]
    public partial class KeyboardInteractiveAuthenticationMethodTest : TestBase
    {
        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("KeyboardInteractiveAuthenticationMethod: Pass null as username.")]
        public void Keyboard_Test_Pass_Null()
        {
            KeyboardInteractiveAuthenticationMethod kiam = null;

            try
            {
                kiam = new KeyboardInteractiveAuthenticationMethod(username: null);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(typeof(ArgumentException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("ddd", ex.ParamName);
            }
            finally
            {
                kiam?.Dispose();
            }
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("KeyboardInteractiveAuthenticationMethod: Pass String.Empty as username.")]
        public void Keyboard_Test_Pass_Whitespace()
        {
            KeyboardInteractiveAuthenticationMethod kiam = null;

            try
            {
                kiam = new KeyboardInteractiveAuthenticationMethod(username: string.Empty);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(typeof(ArgumentException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("ddd", ex.ParamName);
            }
            finally
            {
                kiam?.Dispose();
            }
        }
    }
}
