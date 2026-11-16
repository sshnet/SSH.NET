using System;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshNet.Tests.Common
{
    /// <summary>
    /// Base class for tests that replay a scripted server transcript (see <see cref="ScpServerTranscript"/>)
    /// into a download, with an isolated destination directory.
    /// </summary>
    /// <remarks>
    /// The destination is <c>&lt;root&gt;/a/b/dest</c> below a unique temporary root, so that anything
    /// written above it, into <c>a/b</c> or <c>a</c>, is detected by <see cref="AssertNothingOutsideDestination"/>.
    /// </remarks>
    public abstract class ScpClientTranscriptTestBase
    {
        /// <summary>
        /// Gets the temporary root directory, which is deleted after each test.
        /// </summary>
        protected string Root { get; private set; }

        /// <summary>
        /// Gets the destination directory, <c>&lt;root&gt;/a/b/dest</c>.
        /// </summary>
        protected string Destination { get; private set; }

        /// <summary>
        /// Gets the parent of the destination directory, <c>&lt;root&gt;/a/b</c>.
        /// </summary>
        protected string DestinationParent
        {
            get
            {
                return Path.GetDirectoryName(Destination);
            }
        }

        [TestInitialize]
        public void CreateDestination()
        {
            Root = Path.Combine(Path.GetTempPath(), "sshnet-scp-" + Guid.NewGuid().ToString("N"));
            Destination = Directory.CreateDirectory(Path.Combine(Root, "a", "b", "dest")).FullName;
        }

        [TestCleanup]
        public void DeleteRoot()
        {
            if (Root != null && Directory.Exists(Root))
            {
                Directory.Delete(LiteralPath(Root), recursive: true);
            }
        }

        /// <summary>
        /// Returns the same path with the <c>\\?\</c> prefix on Windows, which turns off Win32 path
        /// normalization, so that entries with names such as <c>file.</c> or <c>CON</c> (which some
        /// tests create on purpose) can be read and deleted.
        /// </summary>
        protected static string LiteralPath(string path)
        {
            return Path.DirectorySeparatorChar == '\\' ? @"\\?\" + path : path;
        }

        /// <summary>
        /// Lists every file system entry below <paramref name="directory"/>, as paths relative to it using
        /// '/' as separator, in no particular order.
        /// </summary>
        protected static string[] ListEntries(string directory)
        {
            return Directory.GetFileSystemEntries(directory, "*", SearchOption.AllDirectories)
                            .Select(p => p.Substring(directory.Length + 1).Replace(Path.DirectorySeparatorChar, '/'))
                            .ToArray();
        }

        /// <summary>
        /// Runs a recursive download of <paramref name="transcript"/> into the destination directory, or
        /// into <paramref name="destinationPath"/> if specified (for example, the destination spelled
        /// differently).
        /// </summary>
        protected ScpTranscriptResult DownloadDirectory(ScpServerTranscript transcript, string destinationPath = null)
        {
            return transcript.Run(
                client => client.Download("/remote", new DirectoryInfo(destinationPath ?? Destination)),
                closeChannel: true);
        }

        /// <summary>
        /// Asserts that nothing exists below the root other than the directories leading to the
        /// destination, the contents of the destination, and <paramref name="expectedOutside"/> (a path
        /// relative to the root, using '/' as separator), if specified.
        /// </summary>
        /// <param name="expectedOutside">An entry outside of the destination that the test created itself.</param>
        /// <param name="context">A description of the scenario, included in the failure message.</param>
        protected void AssertNothingOutsideDestination(string expectedOutside = null, string context = null)
        {
            var outside = ListEntries(Root).Where(p => p is not "a" and not "a/b" and not "a/b/dest" &&
                                                      !p.StartsWith("a/b/dest/", StringComparison.Ordinal) &&
                                                      !string.Equals(p, expectedOutside, StringComparison.Ordinal))
                                           .ToArray();

            Assert.IsEmpty(outside, context + " Written outside of the destination: " + string.Join(", ", outside));
        }

        /// <summary>
        /// Deletes everything in the destination directory.
        /// </summary>
        protected void ClearDestination()
        {
            foreach (var entry in Directory.GetFileSystemEntries(LiteralPath(Destination)))
            {
                if (Directory.Exists(entry))
                {
                    Directory.Delete(entry, recursive: true);
                }
                else
                {
                    File.Delete(entry);
                }
            }
        }
    }
}
