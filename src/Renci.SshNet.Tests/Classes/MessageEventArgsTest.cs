using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides data for message events.
    /// </summary>
    /// <typeparam name="T">Message type</typeparam>
    [TestClass]
    public class MessageEventArgsTest : TestBase
    {
        /// <summary>
        ///A test for MessageEventArgs`1 Constructor
        ///</summary>
        public void MessageEventArgsConstructorTestHelper<T>()
        {
            T message = default(T); // TODO: Initialize to an appropriate value
            MessageEventArgs<T> target = new MessageEventArgs<T>(message);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        [TestMethod]
        [Ignore] // placeholder for actual test
        public void MessageEventArgsConstructorTest()
        {
            MessageEventArgsConstructorTestHelper<GenericParameterHelper>();
        }
    }
}