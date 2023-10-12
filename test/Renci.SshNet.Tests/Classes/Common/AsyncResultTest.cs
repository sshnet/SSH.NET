using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    ///This is a test class for AsyncResultTest and is intended
    ///to contain all AsyncResultTest Unit Tests
    ///</summary>
    [TestClass]
    [Ignore] // placeholder for actual test
    public class AsyncResultTest : TestBase
    {
        /// <summary>
        ///A test for EndInvoke
        ///</summary>
        public void EndInvokeTest1Helper<TResult>()
        {
            var target = CreateAsyncResult<TResult>(); // TODO: Initialize to an appropriate value
            var expected = default(TResult); // TODO: Initialize to an appropriate value
            var actual = target.EndInvoke();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        internal virtual AsyncResult<TResult> CreateAsyncResult<TResult>()
        {
            // TODO: Instantiate an appropriate concrete class.
            AsyncResult<TResult> target = null;
            return target;
        }

        [TestMethod]
        public void EndInvokeTest1()
        {
            EndInvokeTest1Helper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for SetAsCompleted
        ///</summary>
        public void SetAsCompletedTest1Helper<TResult>()
        {
            var target = CreateAsyncResult<TResult>(); // TODO: Initialize to an appropriate value
            TResult result = default; // TODO: Initialize to an appropriate value
            var completedSynchronously = false; // TODO: Initialize to an appropriate value
            target.SetAsCompleted(result, completedSynchronously);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        [TestMethod]
        public void SetAsCompletedTest1()
        {
            SetAsCompletedTest1Helper<GenericParameterHelper>();
        }

        internal virtual AsyncResult CreateAsyncResult()
        {
            // TODO: Instantiate an appropriate concrete class.
            AsyncResult target = null;
            return target;
        }

        /// <summary>
        ///A test for EndInvoke
        ///</summary>
        [TestMethod]
        public void EndInvokeTest()
        {
            var target = CreateAsyncResult(); // TODO: Initialize to an appropriate value
            target.EndInvoke();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for SetAsCompleted
        ///</summary>
        [TestMethod]
        public void SetAsCompletedTest()
        {
            var target = CreateAsyncResult(); // TODO: Initialize to an appropriate value
            Exception exception = null; // TODO: Initialize to an appropriate value
            var completedSynchronously = false; // TODO: Initialize to an appropriate value
            target.SetAsCompleted(exception, completedSynchronously);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for AsyncState
        ///</summary>
        [TestMethod]
        public void AsyncStateTest()
        {
            var target = CreateAsyncResult(); // TODO: Initialize to an appropriate value
            var actual = target.AsyncState;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for AsyncWaitHandle
        ///</summary>
        [TestMethod]
        public void AsyncWaitHandleTest()
        {
            var target = CreateAsyncResult(); // TODO: Initialize to an appropriate value
            var actual = target.AsyncWaitHandle;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CompletedSynchronously
        ///</summary>
        [TestMethod]
        public void CompletedSynchronouslyTest()
        {
            var target = CreateAsyncResult(); // TODO: Initialize to an appropriate value
            var actual = target.CompletedSynchronously;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsCompleted
        ///</summary>
        [TestMethod]
        public void IsCompletedTest()
        {
            var target = CreateAsyncResult(); // TODO: Initialize to an appropriate value
            var actual = target.IsCompleted;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
