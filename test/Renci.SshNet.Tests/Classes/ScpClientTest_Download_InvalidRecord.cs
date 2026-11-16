using System;
using System.IO;
using System.Linq;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Verifies how downloads handle control records: names that contain record-like text, records
    /// that are malformed or unknown, and records that are valid but not expected at that point. The
    /// single-file downloads accept only the records that a server sends for a single file: for
    /// <see cref="ScpClient.Download(string, Stream)"/> a C record, and for
    /// <see cref="ScpClient.Download(string, FileInfo)"/> an optional T record followed by a C record.
    /// </summary>
    [TestClass]
    public class ScpClientTest_Download_InvalidRecord : ScpClientTranscriptTestBase
    {
        private string Target
        {
            get
            {
                return Path.Combine(Destination, "target");
            }
        }

        [TestMethod]
        public void FileNameContainingDirectoryRecord_ShouldBeDownloadedAsFile()
        {
            // An honest server sends a file whose (legitimate) name contains the text of a D
            // record. Its content must be written to that file, not interpreted as records.
            const string Name = "zD0755 0 x";
            const string Content = "E\nC0644 8 injected.txt\ninjected";

            var transcript = new ScpServerTranscript()
                .Line("D0755 0 remote")
                .File(Name, Content)
                .Line("E");

            var result = transcript.Run(
                client => client.Download("/remote", new DirectoryInfo(Destination)),
                closeChannel: true);

            Assert.IsTrue(result.Completed);
            Assert.IsNull(result.Exception, result.Exception?.ToString());
            CollectionAssert.AreEquivalent(new[] { Name }, ListEntries(Destination));
            Assert.AreEqual(Content, File.ReadAllText(Path.Combine(Destination, Name)));
        }

        [TestMethod]
        [DataRow("Xjunk")]
        [DataRow("garbageC0644 1 f")]
        [DataRow("C0644 99999999999999999999 f")]
        [DataRow("C0644 +1 f")]
        [DataRow("C0899 1 f")]
        [DataRow("T99999999999999999999 0 1 0")]
        [DataRow("T999999999999 0 1 0")]
        [DataRow("E trailing")]
        [DataRow("C0644  1 f")]
        [DataRow("C064 1 f")]
        [DataRow("C0644 1 ")]
        [DataRow("D0755 0")]
        [DataRow("C0644 \u0663 f")]
        [DataRow("C\u0660644 1 f")]
        [DataRow("T1 0 2")]
        [DataRow("T\u0661 0 2 0")]
        public void DownloadDirectoryInfo_InvalidRecord_ShouldThrowScpException(string line)
        {
            var transcript = new ScpServerTranscript()
                .Line("D0755 0 remote")
                .Line(line)
                .File("after.txt", "abc")
                .Line("E");

            var result = transcript.Run(
                client => client.Download("/remote", new DirectoryInfo(Destination)),
                closeChannel: true);

            AssertRejected(result, expectedSuccessStatuses: 2);
            AssertNothingOutsideDestination();
            Assert.IsEmpty(Directory.GetFileSystemEntries(Destination));
        }

        [TestMethod]
        public void DownloadDirectoryInfo_EndDirectoryBeforeAnyDirectory_ShouldThrowScpException()
        {
            // Without the check, the E record would move the download to the parent of the destination,
            // and the following file would overwrite the existing file there.
            var existing = Path.Combine(DestinationParent, "existing.txt");
            File.WriteAllText(existing, "original");

            var transcript = new ScpServerTranscript()
                .Line("E")
                .File("existing.txt", "data");

            var result = DownloadDirectory(transcript);

            AssertRejected(result, expectedSuccessStatuses: 1);
            Assert.AreEqual("original", File.ReadAllText(existing));
            AssertNothingOutsideDestination(expectedOutside: "a/b/existing.txt");
            Assert.IsEmpty(Directory.GetFileSystemEntries(Destination));
        }

        [TestMethod]
        [DataRow("Xjunk")]
        [DataRow("C0644 99999999999999999999 f")]
        [DataRow("T999999999999 0 1 0")]
        [DataRow("D0755 0 dir")]
        [DataRow("E")]
        public void DownloadFileInfo_RecordOtherThanTimesOrFile_ShouldThrowScpException(string line)
        {
            var transcript = new ScpServerTranscript()
                .Line(line)
                .File("f", "abc");

            var result = transcript.Run(
                client => client.Download("/remote/f", new FileInfo(Target)),
                closeChannel: true);

            AssertRejected(result, expectedSuccessStatuses: 1);
            Assert.IsEmpty(Directory.GetFileSystemEntries(Destination));
        }

        [TestMethod]
        [DataRow("T1 0 2 0")]
        [DataRow("D0755 0 dir")]
        [DataRow("E")]
        [DataRow("Xjunk")]
        public void DownloadFileInfo_TimesFollowedByRecordOtherThanFile_ShouldThrowScpException(string line)
        {
            // Only one T record is allowed, and it must be followed by the C record.
            var transcript = new ScpServerTranscript()
                .Line("T1700000000 0 1700000000 0")
                .Line(line)
                .File("f", "abc");

            var result = transcript.Run(
                client => client.Download("/remote/f", new FileInfo(Target)),
                closeChannel: true);

            AssertRejected(result, expectedSuccessStatuses: 2);
            Assert.IsEmpty(Directory.GetFileSystemEntries(Destination));
        }

        [TestMethod]
        public void DownloadFileInfo_TimesThenFile_ShouldWriteToTargetIgnoringServerName()
        {
            var transcript = new ScpServerTranscript()
                .Line("T1700000000 0 1700000000 0")
                .File("server-name", "content");

            var result = transcript.Run(
                client => client.Download("/remote/f", new FileInfo(Target)),
                closeChannel: true);

            Assert.IsTrue(result.Completed);
            Assert.IsNull(result.Exception, result.Exception?.ToString());
            CollectionAssert.AreEquivalent(new[] { "target" }, ListEntries(Destination));
            Assert.AreEqual("content", File.ReadAllText(Target));
            Assert.AreEqual(DateTime.UnixEpoch.AddSeconds(1700000000), File.GetLastWriteTimeUtc(Target));
        }

        [TestMethod]
        [DataRow("Xjunk")]
        [DataRow("garbageC0644 1 f")]
        [DataRow("C0644 99999999999999999999 f")]
        [DataRow("C0644 9223372036854775808 f")]
        [DataRow("D0755 0 dir")]
        [DataRow("E")]
        [DataRow("T1 0 2 0")]
        public void DownloadStream_RecordOtherThanFile_ShouldThrowScpException(string line)
        {
            var transcript = new ScpServerTranscript()
                .Line(line)
                .File("f", "abc");

            using (var destination = new MemoryStream())
            {
                var result = transcript.Run(
                    client => client.Download("/remote/f", destination),
                    closeChannel: true);

                AssertRejected(result, expectedSuccessStatuses: 1);
                Assert.AreEqual(0, destination.Length);
            }
        }

        private static void AssertRejected(ScpTranscriptResult result, int expectedSuccessStatuses)
        {
            Assert.IsTrue(result.Completed);
            Assert.IsInstanceOfType<ScpException>(result.Exception);
            Assert.Contains("invalid or unexpected SCP record", result.Exception.Message, StringComparison.Ordinal);

            // The rejected record is not acknowledged: the only success status bytes are the one sent
            // after the exec request and one for each record accepted before it.
            Assert.AreEqual(
                expectedSuccessStatuses,
                result.SentData.Count(data => data.Length == 1 && data[0] == 0),
                "Unexpected number of success status bytes; was the rejected record acknowledged?");

            // The error is also reported to the server: a 0x01 status byte followed by a message.
            Assert.IsTrue(
                result.SentData.Any(data => data.Length == 1 && data[0] == 1),
                "No error status was sent to the server.");
            Assert.IsTrue(
                result.SentData.Any(data => string.Equals(Encoding.UTF8.GetString(data), "Invalid or unexpected SCP record.\n", StringComparison.Ordinal)),
                "No error message was sent to the server.");
        }
    }
}
