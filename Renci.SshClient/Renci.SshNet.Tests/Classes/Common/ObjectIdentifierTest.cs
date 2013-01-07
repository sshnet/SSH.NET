using Renci.SshNet.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{   
    /// <summary>
    ///This is a test class for ObjectIdentifierTest and is intended
    ///to contain all ObjectIdentifierTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ObjectIdentifierTest : TestBase
    {
        /// <summary>
        ///A test for ObjectIdentifier Constructor
        ///</summary>
        [TestMethod()]
        public void ObjectIdentifierConstructorTest()
        {
            ulong[] identifiers = null; // TODO: Initialize to an appropriate value
            ObjectIdentifier target = new ObjectIdentifier(identifiers);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
