﻿using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality to perform private key authentication.
    /// </summary>
    [TestClass]
    public class PrivateKeyAuthenticationMethodTest : TestBase
    {
        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [TestCategory("PrivateKeyAuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("PrivateKeyAuthenticationMethod: Pass null as username, null as password.")]
        [ExpectedException(typeof(ArgumentException))]
        public void PrivateKey_Test_Pass_Null()
        {
            new PrivateKeyAuthenticationMethod(null, null);
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [TestCategory("PrivateKeyAuthenticationMethod")]
        [Owner("olegkap")]
        [Description("PrivateKeyAuthenticationMethod: Pass valid username, null as password.")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PrivateKey_Test_Pass_PrivateKey_Null()
        {
            new PrivateKeyAuthenticationMethod("username", null);
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [TestCategory("PrivateKeyAuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("PrivateKeyAuthenticationMethod: Pass String.Empty as username, null as password.")]
        [ExpectedException(typeof(ArgumentException))]
        public void PrivateKey_Test_Pass_Whitespace()
        {
            new PrivateKeyAuthenticationMethod(string.Empty, null);
        }
    }
}
