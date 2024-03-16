using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Sftp;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    /// <summary>
    ///This is a test class for SftpDownloadAsyncResultTest and is intended
    ///to contain all SftpDownloadAsyncResultTest Unit Tests
    ///</summary>
    [TestClass]
    public class SftpDownloadAsyncResultTest : TestBase
    {
        [TestMethod]
        public void SftpDownloadAsyncResultConstructorTest()
        {
            const AsyncCallback asyncCallback = null;
            var state = new object();
            var target = new SftpDownloadAsyncResult(asyncCallback, state);

            Assert.IsFalse(target.CompletedSynchronously);
            Assert.IsFalse(target.EndInvokeCalled);
            Assert.IsFalse(target.IsCompleted);
            Assert.IsFalse(target.IsDownloadCanceled);
            Assert.AreEqual(0UL, target.DownloadedBytes);
            Assert.AreSame(state, target.AsyncState);
        }

        [TestMethod]
        public void SetAsCompleted_Exception_CompletedSynchronously()
        {
            var downloadCompleted = new ManualResetEvent(false);
            object state = "STATE";
            Exception exception = new IOException();
            IAsyncResult callbackResult = null;
            var target = new SftpDownloadAsyncResult(asyncResult =>
                {
                    downloadCompleted.Set();
                    callbackResult = asyncResult;
                }, state);

            target.SetAsCompleted(exception, true);

            Assert.AreSame(target, callbackResult);
            Assert.IsFalse(target.IsDownloadCanceled);
            Assert.IsTrue(target.IsCompleted);
            Assert.IsTrue(target.CompletedSynchronously);
            Assert.IsTrue(downloadCompleted.WaitOne(TimeSpan.Zero));
        }

        [TestMethod]
        public void EndInvoke_CompletedWithException()
        {
            object state = "STATE";
            Exception exception = new IOException();
            var target = new SftpDownloadAsyncResult(null, state);
            target.SetAsCompleted(exception, true);

            try
            {
                target.EndInvoke();
                Assert.Fail();
            }
            catch (IOException ex)
            {
                Assert.AreSame(exception, ex);
            }
        }

        [TestMethod]
        public void Update()
        {
            var target = new SftpDownloadAsyncResult(null, null);

            target.Update(123);
            target.Update(431);

            Assert.AreEqual(431UL, target.DownloadedBytes);
        }
    }
}
