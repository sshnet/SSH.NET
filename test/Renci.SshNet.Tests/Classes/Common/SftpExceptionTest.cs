using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class SftpExceptionTest
    {
        [TestMethod]
        public void StatusCodes()
        {
            Assert.AreEqual(StatusCode.BadMessage, new SftpException(StatusCode.BadMessage).StatusCode);
            Assert.AreEqual(StatusCode.OperationUnsupported, new SftpException(StatusCode.OperationUnsupported, null).StatusCode);
            Assert.AreEqual(StatusCode.Failure, new SftpException(StatusCode.Failure, null, null).StatusCode);

            Assert.AreEqual(StatusCode.PermissionDenied, new SftpPermissionDeniedException().StatusCode);
            Assert.AreEqual(StatusCode.PermissionDenied, new SftpPermissionDeniedException(null).StatusCode);
            Assert.AreEqual(StatusCode.PermissionDenied, new SftpPermissionDeniedException(null, null).StatusCode);

            Assert.AreEqual(StatusCode.NoSuchFile, new SftpPathNotFoundException().StatusCode);
            Assert.AreEqual(StatusCode.NoSuchFile, new SftpPathNotFoundException(null).StatusCode);
            Assert.AreEqual(StatusCode.NoSuchFile, new SftpPathNotFoundException(null, path: null).StatusCode);
            Assert.AreEqual(StatusCode.NoSuchFile, new SftpPathNotFoundException(null, innerException: null).StatusCode);
            Assert.AreEqual(StatusCode.NoSuchFile, new SftpPathNotFoundException(null, null, null).StatusCode);
        }

        [TestMethod]
        public void Message()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(new SftpException(StatusCode.Failure).Message));
            Assert.IsFalse(string.IsNullOrWhiteSpace(new SftpException(StatusCode.Failure, "").Message));
            Assert.AreEqual("Custom message", new SftpException(StatusCode.Failure, "Custom message").Message);

            Assert.IsFalse(string.IsNullOrWhiteSpace(new SftpPermissionDeniedException().Message));
            Assert.IsFalse(string.IsNullOrWhiteSpace(new SftpPermissionDeniedException("").Message));
            Assert.IsFalse(string.IsNullOrWhiteSpace(new SftpPermissionDeniedException("", null).Message));
            Assert.AreEqual("Custom message1", new SftpPermissionDeniedException("Custom message1").Message);
            Assert.AreEqual("Custom message2", new SftpPermissionDeniedException("Custom message2", null).Message);

            Assert.IsFalse(string.IsNullOrWhiteSpace(new SftpPathNotFoundException().Message));
            Assert.IsFalse(string.IsNullOrWhiteSpace(new SftpPathNotFoundException("").Message));
            Assert.IsFalse(string.IsNullOrWhiteSpace(new SftpPathNotFoundException("", path: null).Message));
            Assert.AreEqual("Custom message1", new SftpPathNotFoundException("Custom message1").Message);
            Assert.AreEqual("Custom message2", new SftpPathNotFoundException("Custom message2", path: null).Message);
            Assert.AreEqual("Custom message2", new SftpPathNotFoundException("Custom message2", "path1").Message);
            Assert.AreEqual("Custom message3", new SftpPathNotFoundException("Custom message3", innerException: null).Message);
            Assert.AreEqual("Custom message4", new SftpPathNotFoundException("Custom message4", null, null).Message);
        }

        [TestMethod]
        public void PathNotFoundException_Path()
        {
            Assert.IsNull(new SftpPathNotFoundException().Path);
            Assert.IsNull(new SftpPathNotFoundException("message").Path);
            Assert.AreEqual("path1", new SftpPathNotFoundException("message", "path1").Path);
            Assert.AreEqual("path2", new SftpPathNotFoundException(null, "path2", null).Path);

            Assert.Contains("Path: 'path3'.", new SftpPathNotFoundException(message: null, path: "path3").Message, StringComparison.Ordinal);
            Assert.Contains("Path: 'path4'.", new SftpPathNotFoundException(message: "", path: "path4").Message, StringComparison.Ordinal);
        }
    }
}
