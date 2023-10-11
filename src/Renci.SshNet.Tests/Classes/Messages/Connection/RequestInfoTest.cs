using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Messages.Connection
{
    /// <summary>
    ///This is a test class for RequestInfoTest and is intended
    ///to contain all RequestInfoTest Unit Tests
    ///</summary>
    [TestClass]
    [Ignore] // placeholders only
    public class RequestInfoTest : TestBase
    {
        internal virtual RequestInfo CreateRequestInfo()
        {
            // TODO: Instantiate an appropriate concrete class.
            RequestInfo target = null;
            return target;
        }

        /// <summary>
        ///A test for RequestName
        ///</summary>
        [TestMethod()]
        public void RequestNameTest()
        {
            var target = CreateRequestInfo(); // TODO: Initialize to an appropriate value
            var actual = target.RequestName;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
