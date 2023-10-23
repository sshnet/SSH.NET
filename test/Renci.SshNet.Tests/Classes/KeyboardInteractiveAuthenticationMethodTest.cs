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
        public void Keyboard_Test_UsernameIsNull()
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
                ArgumentExceptionAssert.MessageEquals("Cannot be null or only whitespace.", ex);
                Assert.AreEqual("username", ex.ParamName);
            }
            finally
            {
                kiam?.Dispose();
            }
        }

        [TestMethod]
        public void Keyboard_Test_UsernameIsEmpty()
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
                ArgumentExceptionAssert.MessageEquals("Cannot be null or only whitespace.", ex);
                Assert.AreEqual("username", ex.ParamName);
            }
            finally
            {
                kiam?.Dispose();
            }
        }

        [TestMethod]
        public void Keyboard_Test_UsernameIsWhitespace()
        {
            KeyboardInteractiveAuthenticationMethod kiam = null;

            try
            {
                kiam = new KeyboardInteractiveAuthenticationMethod(username: "   ");
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(typeof(ArgumentException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                ArgumentExceptionAssert.MessageEquals("Cannot be null or only whitespace.", ex);
                Assert.AreEqual("username", ex.ParamName);
            }
            finally
            {
                kiam?.Dispose();
            }
        }
    }
}
