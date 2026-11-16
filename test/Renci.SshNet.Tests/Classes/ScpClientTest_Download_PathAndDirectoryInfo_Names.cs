using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Verifies how a recursive download handles the file and directory names sent by the server:
    /// plain names are created as-is, and names that are not a plain local name, or that the local
    /// file system would resolve to a different name or location, are rejected.
    /// </summary>
    /// <remarks>
    /// Each name is checked both in a C (file) record and in a nested D (directory) record; the name
    /// in the first D record is ignored, as the destination itself is used for it.
    /// </remarks>
    [TestClass]
    public class ScpClientTest_Download_PathAndDirectoryInfo_Names : ScpClientTranscriptTestBase
    {
        [TestMethod]
        [DataRow("file.txt")]
        [DataRow(".hidden")]
        [DataRow("файл.txt")]
        [DataRow("~")]
        public void PlainName_ShouldBeDownloaded(string name)
        {
            var result = DownloadDirectory(new ScpServerTranscript()
                .Line("D0755 0 remote")
                .Line("D0755 0 " + name)
                .File(name, "content")
                .Line("E")
                .Line("E"));

            Assert.IsNull(result.Exception, result.Exception?.ToString());
            CollectionAssert.AreEquivalent(new[] { name, name + "/" + name }, ListEntries(Destination));
        }

        [TestMethod]
        [OSCondition(ConditionMode.Exclude, OperatingSystems.Windows, IgnoreMessage = "These are ordinary names on Unix-like platforms.")]
        [DataRow("...")]
        [DataRow(".. ")]
        [DataRow("file.")]
        [DataRow("file ")]
        [DataRow("CON")]
        [DataRow("PROGRA~1")]
        [DataRow("name\\with\\backslashes")]
        [DataRow("file.txt:stream")]
        [DataRow("C:file.txt")]
        [DataRow("\\\\?\\C:\\file.txt")]
        [DataRow("\\\\.\\C:\\file.txt")]
        [DataRow("\\??\\C:\\file.txt")]
        public void OrdinaryUnixName_ShouldBeDownloaded(string name)
        {
            var result = DownloadDirectory(new ScpServerTranscript()
                .Line("D0755 0 remote")
                .Line("D0755 0 " + name)
                .File(name, "content")
                .Line("E")
                .Line("E"));

            Assert.IsNull(result.Exception, result.Exception?.ToString());
            CollectionAssert.AreEquivalent(new[] { name, name + "/" + name }, ListEntries(Destination));
        }

        [TestMethod]
        [OSCondition(OperatingSystems.Windows, IgnoreMessage = "The \\\\?\\ prefix only exists on Windows.")]
        public void PlainName_LiteralPathDestination_OnWindows_ShouldBeDownloaded()
        {
            var result = Download("sub", isDirectory: true, LiteralPath(Destination));

            Assert.IsNull(result.Exception, result.Exception?.ToString());
            CollectionAssert.AreEquivalent(new[] { "sub", "sub/inner.txt" }, ListEntries(Destination));
        }

        [TestMethod]
        [DataRow(".")]
        [DataRow("..")]
        [DataRow("sub/child")]
        [DataRow("../escaped")]
        [DataRow("/etc/passwd")]
        [DataRow("nul\0byte")]
        public void InvalidName_ShouldThrowScpException(string name)
        {
            foreach (var isDirectory in new[] { false, true })
            {
                var result = Download(name, isDirectory);

                AssertNothingOutsideDestination(context: name);
                Assert.IsInstanceOfType<ScpException>(result.Exception, result.Exception?.ToString());
                Assert.Contains("not a valid local name", result.Exception.Message, StringComparison.Ordinal);
                Assert.IsEmpty(Directory.GetFileSystemEntries(Destination));

                // The record with the invalid name is reported to the server instead of being acknowledged:
                // the only success statuses are for the exec request and the first D record.
                Assert.AreEqual(2, result.SentData.Count(data => data.Length == 1 && data[0] == 0));
                Assert.IsTrue(result.SentData.Any(data => data.Length == 1 && data[0] == 1), "No error status was sent to the server.");
            }
        }

        [TestMethod]
        [OSCondition(OperatingSystems.Windows, IgnoreMessage = "These names are only invalid, or changed by path normalization, on Windows.")]
        [DataRow("..\\escaped")]
        [DataRow("C:\\Windows\\file.txt")]
        [DataRow("C:file.txt")]
        [DataRow("file.txt:stream")]
        [DataRow("\\\\?\\C:\\file.txt")]
        [DataRow("\\\\.\\C:\\file.txt")]
        [DataRow("\\??\\C:\\file.txt")]
        [DataRow("\\\\server\\share\\file.txt")]
        [DataRow("...")] // Path normalization removes trailing dots and spaces.
        [DataRow(".. ")]
        [DataRow(" ..")]
        [DataRow(". ")]
        [DataRow("file.")]
        [DataRow("file ")]
        public void InvalidName_OnWindows_ShouldThrowScpException(string name)
        {
            foreach (var isDirectory in new[] { false, true })
            {
                var result = Download(name, isDirectory);

                AssertNothingOutsideDestination(context: name);
                Assert.IsInstanceOfType<ScpException>(result.Exception, result.Exception?.ToString());
                Assert.Contains("not a valid local name", result.Exception.Message, StringComparison.Ordinal);
                Assert.IsEmpty(Directory.GetFileSystemEntries(Destination));
                Assert.AreEqual(2, result.SentData.Count(data => data.Length == 1 && data[0] == 0));
                Assert.IsTrue(result.SentData.Any(data => data.Length == 1 && data[0] == 1), "No error status was sent to the server.");
            }
        }

        [TestMethod]
        [OSCondition(OperatingSystems.Windows, IgnoreMessage = "The \\\\?\\ prefix only exists on Windows.")]
        [DataRow(".")]
        [DataRow("..")]
        [DataRow("...")]
        [DataRow("file.")]
        [DataRow("file ")]
        public void NameChangedByNormalization_LiteralPathDestination_OnWindows_ShouldStayWithinDestination(string name)
        {
            // Path.GetFullPath does not normalize a path with the \\?\ prefix, so these names reach the
            // file system as they are: "." and ".." are refused with an IOException, and the others are
            // created with exactly that name. Either way, nothing may be written outside the destination.
            foreach (var isDirectory in new[] { false, true })
            {
                var result = Download(name, isDirectory, LiteralPath(Destination));

                AssertNothingOutsideDestination(context: name);
                Assert.IsTrue(result.Exception is null or ScpException or IOException, result.Exception?.ToString());

                ClearDestination();
            }
        }

        [TestMethod]
        [OSCondition(OperatingSystems.Windows, IgnoreMessage = "Device names are only reserved on Windows.")]
        [DataRow("CON")]
        [DataRow("con.txt")]
        [DataRow("CON .txt")]
        [DataRow("NUL")]
        [DataRow("COM1")]
        [DataRow("LPT1")]
        public void ReservedDeviceName_OnWindows_ShouldBeRejectedOrWrittenToDestination(string name)
        {
            // Where Windows treats the name as a device in any directory, Path.GetFullPath resolves it to
            // a device path (such as \\.\CON), which is not a child of the destination and is rejected.
            // Where it does not (as in recent versions of Windows), it is an ordinary name.
            foreach (var isDirectory in new[] { false, true })
            {
                var result = Download(name, isDirectory);

                AssertNothingOutsideDestination(context: name);

                if (result.Exception is null)
                {
                    // Read through a literal path, so that the name cannot be redirected to a device.
                    var file = isDirectory ? Path.Combine(Destination, name, "inner.txt") : Path.Combine(Destination, name);
                    Assert.AreEqual("content", File.ReadAllText(LiteralPath(file)));
                }
                else
                {
                    Assert.IsInstanceOfType<ScpException>(result.Exception, result.Exception.ToString());
                    Assert.Contains("not a valid local name", result.Exception.Message, StringComparison.Ordinal);
                }

                ClearDestination();
            }
        }

        [TestMethod]
        [OSCondition(OperatingSystems.Windows, IgnoreMessage = "8.3 short names only exist on Windows.")]
        public void ShortNameOfExistingDirectory_OnWindows_ShouldStayWithinDestination()
        {
            var existing = Directory.CreateDirectory(Path.Combine(Destination, "LongDirectoryName"));

            var buffer = new char[1024];
            var length = GetShortPathName(existing.FullName, buffer, (uint)buffer.Length);
            var shortName = length is 0 or > 1024 ? existing.Name : Path.GetFileName(new string(buffer, 0, (int)length));

            if (string.Equals(shortName, existing.Name, StringComparison.OrdinalIgnoreCase))
            {
                Assert.Inconclusive("8.3 short names are not generated on this volume.");
            }

            var result = Download(shortName, isDirectory: true);

            // .NET Framework's Path.GetFullPath expands a short name, so the resolved name differs from
            // the name that was sent and is rejected. Elsewhere, the short name is used as-is, and the
            // file system resolves it to the existing directory. Either way, the download must stay
            // within the destination.
            if (result.Exception is null)
            {
                Assert.AreEqual("content", File.ReadAllText(Path.Combine(existing.FullName, "inner.txt")));
            }
            else
            {
                Assert.IsInstanceOfType<ScpException>(result.Exception);
                Assert.Contains("not a valid local name", result.Exception.Message, StringComparison.Ordinal);
            }

            AssertNothingOutsideDestination();
        }

        [TestMethod]
        public void DestinationWithTrailingSeparator_ShouldBeDownloaded()
        {
            var result = Download("sub", isDirectory: true, Destination + Path.DirectorySeparatorChar);

            Assert.IsNull(result.Exception, result.Exception?.ToString());
            CollectionAssert.AreEquivalent(new[] { "sub", "sub/inner.txt" }, ListEntries(Destination));
        }

#pragma warning disable SYSLIB1054 // Use LibraryImport; not available on .NET Framework, and only used by a Windows-only test.
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "GetShortPathNameW")]
        private static extern uint GetShortPathName(string longPath, [Out] char[] shortPath, uint bufferLength);
#pragma warning restore SYSLIB1054

        /// <summary>
        /// Downloads a remote directory containing either a file named <paramref name="name"/>, or a
        /// directory named <paramref name="name"/> that contains a file named <c>inner.txt</c>.
        /// </summary>
        private ScpTranscriptResult Download(string name, bool isDirectory, string destinationPath = null)
        {
            var transcript = new ScpServerTranscript().Line("D0755 0 remote");

            if (isDirectory)
            {
                _ = transcript.Line("D0755 0 " + name).File("inner.txt", "content").Line("E");
            }
            else
            {
                _ = transcript.File(name, "content");
            }

            return DownloadDirectory(transcript.Line("E"), destinationPath);
        }
    }
}
