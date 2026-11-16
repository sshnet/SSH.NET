using System;
using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Verifies that a download fails, rather than spinning forever, when the channel is closed
    /// before all of the announced file content has been received.
    /// </summary>
    [TestClass]
    public class ScpClientTest_Download_ChannelClosedDuringFileContent : ScpClientTranscriptTestBase
    {
        [TestMethod]
        public void DownloadDirectoryInfo_ShouldThrow()
        {
            var transcript = new ScpServerTranscript()
                .Line("D0755 0 remote")
                .Line("C0644 100 file")
                .Raw("0123456789");

            AssertThrows(transcript, client => client.Download("/remote", new DirectoryInfo(Destination)));
        }

        [TestMethod]
        public void DownloadFileInfo_ShouldThrow()
        {
            var transcript = new ScpServerTranscript()
                .Line("C0644 100 file")
                .Raw("0123456789");

            AssertThrows(transcript, client => client.Download("/remote/file", new FileInfo(Path.Combine(Destination, "file"))));
        }

        [TestMethod]
        public void DownloadStream_ShouldThrow()
        {
            var transcript = new ScpServerTranscript()
                .Line("C0644 100 file")
                .Raw("0123456789");

            using (var destination = new MemoryStream())
            {
                AssertThrows(transcript, client => client.Download("/remote/file", destination));
            }
        }

        [TestMethod]
        public void DownloadEmptyFile_ShouldStillSucceed()
        {
            var transcript = new ScpServerTranscript()
                .File("file", string.Empty);

            using (var destination = new MemoryStream())
            {
                var result = transcript.Run(client => client.Download("/remote/file", destination), closeChannel: true);

                Assert.IsTrue(result.Completed);
                Assert.IsNull(result.Exception, result.Exception?.ToString());
                Assert.AreEqual(0, destination.Length);
            }
        }

        private static void AssertThrows(ScpServerTranscript transcript, Action<ScpClient> download)
        {
            var result = transcript.Run(download, closeChannel: true, timeout: TimeSpan.FromSeconds(5));

            Assert.IsTrue(result.Completed);
            Assert.IsInstanceOfType<SshException>(result.Exception);
        }
    }
}
