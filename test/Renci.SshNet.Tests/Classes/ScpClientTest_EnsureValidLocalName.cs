using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Tests for <see cref="ScpClient.EnsureValidLocalName(string)"/>, which guards the recursive
    /// download against server-supplied SCP file/directory names that would write outside the
    /// caller-supplied destination directory.
    /// </summary>
    [TestClass]
    public class ScpClientTest_EnsureValidLocalName
    {
        [TestMethod]
        public void PlainFileName_DoesNotThrow()
        {
            ScpClient.EnsureValidLocalName("owned.txt");
            ScpClient.EnsureValidLocalName("2024-report.tar.gz");
            ScpClient.EnsureValidLocalName("file with spaces.dat");
            ScpClient.EnsureValidLocalName("...");
        }

        [TestMethod]
        public void UnicodeFileName_DoesNotThrow()
        {
            ScpClient.EnsureValidLocalName("файл.txt");
        }

        [TestMethod]
        public void Empty_ThrowsScpException()
        {
            _ = Assert.ThrowsExactly<ScpException>(() => ScpClient.EnsureValidLocalName(string.Empty));
        }

        [TestMethod]
        public void CurrentDirectory_ThrowsScpException()
        {
            _ = Assert.ThrowsExactly<ScpException>(() => ScpClient.EnsureValidLocalName("."));
        }

        [TestMethod]
        public void ParentDirectory_ThrowsScpException()
        {
            _ = Assert.ThrowsExactly<ScpException>(() => ScpClient.EnsureValidLocalName(".."));
        }

        [TestMethod]
        public void ForwardSlashPath_ThrowsScpException()
        {
            // '/' is an invalid file name character on every platform.
            _ = Assert.ThrowsExactly<ScpException>(() => ScpClient.EnsureValidLocalName("sub/child"));
            _ = Assert.ThrowsExactly<ScpException>(() => ScpClient.EnsureValidLocalName("../escaped/owned.txt"));
        }

        [TestMethod]
        public void RootedUnixPath_ThrowsScpException()
        {
            // Contains '/', so it is rejected regardless of platform.
            _ = Assert.ThrowsExactly<ScpException>(() => ScpClient.EnsureValidLocalName("/tmp/sshnet-owned.txt"));
        }

        [TestMethod]
        public void NullCharacter_ThrowsScpException()
        {
            // NUL is an invalid file name character on every platform.
            _ = Assert.ThrowsExactly<ScpException>(() => ScpClient.EnsureValidLocalName("safe\0evil"));
        }

        [TestMethod]
        [OSCondition(OperatingSystems.Windows, IgnoreMessage = "'\\' is only a path separator (and invalid file name char) on Windows.")]
        public void BackslashPath_OnWindows_ThrowsScpException()
        {
            _ = Assert.ThrowsExactly<ScpException>(() => ScpClient.EnsureValidLocalName("..\\escaped\\owned.txt"));
            _ = Assert.ThrowsExactly<ScpException>(() => ScpClient.EnsureValidLocalName("sub\\child"));
        }

        [TestMethod]
        [OSCondition(OperatingSystems.Windows, IgnoreMessage = "':' is only an invalid file name char on Windows.")]
        public void DriveQualifiedPath_OnWindows_ThrowsScpException()
        {
            _ = Assert.ThrowsExactly<ScpException>(() => ScpClient.EnsureValidLocalName("C:\\Windows\\System32\\evil.dll"));
        }

        [TestMethod]
        [OSCondition(OperatingSystems.Windows, IgnoreMessage = "':' is only an invalid file name char on Windows.")]
        public void AlternateDataStreamName_OnWindows_ThrowsScpException()
        {
            // NTFS alternate data stream syntax: writes a hidden stream of "safe.txt".
            _ = Assert.ThrowsExactly<ScpException>(() => ScpClient.EnsureValidLocalName("safe.txt:evil"));
        }

        [TestMethod]
        [OSCondition(ConditionMode.Exclude, OperatingSystems.Windows, IgnoreMessage = "'\\' is a valid file name byte only on Unix-like platforms.")]
        public void BackslashName_OnUnix_DoesNotThrow()
        {
            // On Unix, '\' is an ordinary file name byte and cannot traverse directories,
            // so a name containing it must remain accepted (no behaviour change).
            ScpClient.EnsureValidLocalName("name\\with\\backslashes.txt");
        }
    }
}
