using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Verifies that the upload methods reject names that cannot be sent in an SCP C or D record: a name
    /// containing a line feed, or an empty file name (a remote path that ends with <c>/</c>).
    /// </summary>
    /// <remarks>
    /// A line feed would end the record early, so that the server reads the rest of the name as the
    /// start of the file content, and content crafted to match is then read as further records. The
    /// integration tests (<c>ScpTests</c>) show the effect on a real server: another file is created.
    /// </remarks>
    [TestClass]
    public class ScpClientTest_Upload_InvalidName : ScpClientTranscriptTestBase
    {
        [TestMethod]
        public void UploadStream_FileNameWithLineFeed_ShouldThrowArgumentExceptionBeforeOpeningChannel()
        {
            using (var source = new MemoryStream("content"u8.ToArray()))
            {
                var result = new ScpServerTranscript().Run(
                    client => client.Upload(source, "/home/sshnet/report.txt\nrest-of-name"),
                    timeout: TimeSpan.FromSeconds(5));

                AssertRejectedBeforeOpeningChannel(result);
            }
        }

        [TestMethod]
        public void UploadFileInfo_FileNameWithLineFeed_ShouldThrowArgumentExceptionBeforeOpeningChannel()
        {
            var filePath = Path.Combine(Root, "file.txt");
            File.WriteAllText(filePath, "content");

            var result = new ScpServerTranscript().Run(
                client => client.Upload(new FileInfo(filePath), "/home/sshnet/report.txt\nrest-of-name"),
                timeout: TimeSpan.FromSeconds(5));

            AssertRejectedBeforeOpeningChannel(result);
        }

        [TestMethod]
        [DataRow("/home/sshnet/")]
        [DataRow("dir/")]
        [DataRow("/")]
        public void UploadStream_PathWithoutFileName_ShouldThrowArgumentExceptionBeforeOpeningChannel(string remotePath)
        {
            using (var stream = new MemoryStream("content"u8.ToArray()))
            {
                var result = new ScpServerTranscript().Run(
                    client => client.Upload(stream, remotePath),
                    timeout: TimeSpan.FromSeconds(5));

                AssertRejectedBeforeOpeningChannel(result);
            }
        }

        [TestMethod]
        [DataRow("/home/sshnet/")]
        [DataRow("dir/")]
        public void UploadFileInfo_PathWithoutFileName_ShouldThrowArgumentExceptionBeforeOpeningChannel(string remotePath)
        {
            var localPath = Path.Combine(Root, "file.txt");
            File.WriteAllText(localPath, "content");

            var result = new ScpServerTranscript().Run(
                client => client.Upload(new FileInfo(localPath), remotePath),
                timeout: TimeSpan.FromSeconds(5));

            AssertRejectedBeforeOpeningChannel(result);
        }

        [TestMethod]
        [OSCondition(ConditionMode.Exclude, OperatingSystems.Windows,
            IgnoreMessage = "A line feed is not valid in a file name on Windows.")]
        public void UploadDirectoryInfo_LocalFileNameWithLineFeed_ShouldThrowScpException()
        {
            var localDir = Directory.CreateDirectory(Path.Combine(Root, "source"));
            File.WriteAllText(Path.Combine(localDir.FullName, "report.txt\nrest-of-name"), "content");

            var transcript = new ScpServerTranscript().Raw([0]); // Status byte for the exec request.

            var result = transcript.Run(
                client => client.Upload(localDir, "/home/sshnet/target"),
                timeout: TimeSpan.FromSeconds(5));

            AssertRejectedBeforeSendingAnyRecord(result);
        }

        [TestMethod]
        [OSCondition(ConditionMode.Exclude, OperatingSystems.Windows,
            IgnoreMessage = "A line feed is not valid in a directory name on Windows.")]
        public void UploadDirectoryInfo_LocalDirectoryNameWithLineFeed_ShouldThrowScpException()
        {
            var localDir = Directory.CreateDirectory(Path.Combine(Root, "source"));
            _ = Directory.CreateDirectory(Path.Combine(localDir.FullName, "dir\nrest-of-name"));

            var transcript = new ScpServerTranscript().Raw([0]); // Status byte for the exec request.

            var result = transcript.Run(
                client => client.Upload(localDir, "/home/sshnet/target"),
                timeout: TimeSpan.FromSeconds(5));

            AssertRejectedBeforeSendingAnyRecord(result);
        }

        private static void AssertRejectedBeforeOpeningChannel(ScpTranscriptResult result)
        {
            Assert.IsTrue(result.Completed);
            var ex = Assert.IsInstanceOfType<ArgumentException>(result.Exception);
            Assert.AreEqual("path", ex.ParamName);
            Assert.IsEmpty(result.ExecRequests);
            Assert.IsEmpty(result.SentData);
        }

        private static void AssertRejectedBeforeSendingAnyRecord(ScpTranscriptResult result)
        {
            Assert.IsTrue(result.Completed);
            Assert.IsInstanceOfType<ScpException>(result.Exception);
            Assert.Contains("line feed", result.Exception.Message, StringComparison.Ordinal);
            Assert.IsEmpty(result.SentData);
        }
    }
}
