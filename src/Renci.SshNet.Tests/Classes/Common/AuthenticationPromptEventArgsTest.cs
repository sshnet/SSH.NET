﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    
    
    /// <summary>
    ///This is a test class for AuthenticationPromptEventArgsTest and is intended
    ///to contain all AuthenticationPromptEventArgsTest Unit Tests
    ///</summary>
    [TestClass]
    public class AuthenticationPromptEventArgsTest : TestBase
    {
        /// <summary>
        ///A test for AuthenticationPromptEventArgs Constructor
        ///</summary>
        [TestMethod]
        public void AuthenticationPromptEventArgsConstructorTest()
        {
            var username = string.Empty; // TODO: Initialize to an appropriate value
            var instruction = string.Empty; // TODO: Initialize to an appropriate value
            var language = string.Empty; // TODO: Initialize to an appropriate value
            IEnumerable<AuthenticationPrompt> prompts = null; // TODO: Initialize to an appropriate value
            var target = new AuthenticationPromptEventArgs(username, instruction, language, prompts);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
