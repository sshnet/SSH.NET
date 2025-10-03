using System;
using System.Buffers.Binary;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes.Sftp
{
    [TestClass]
    public class SftpFileAttributesTest
    {
        [TestMethod]
        [DataRow(0xC000u, true, false, false, false, false, false, false)] // Socket
        [DataRow(0xA000u, false, true, false, false, false, false, false)] // Symbolic link
        [DataRow(0x8000u, false, false, true, false, false, false, false)] // Regular file
        [DataRow(0x6000u, false, false, false, true, false, false, false)] // Block device
        [DataRow(0x4000u, false, false, false, false, true, false, false)] // Directory
        [DataRow(0x2000u, false, false, false, false, false, true, false)] // Character device
        [DataRow(0x1000u, false, false, false, false, false, false, true)] // Named pipe
        public void FileTypePropertiesAreMutuallyExclusive(
            uint permissions,
            bool isSocket,
            bool isSymbolicLink,
            bool isRegularFile,
            bool isBlockDevice,
            bool isDirectory,
            bool isCharacterDevice,
            bool isNamedPipe)
        {
            var attributeBytes = new byte[8];
            attributeBytes[3] = 0x4; // SSH_FILEXFER_ATTR_PERMISSIONS
            BinaryPrimitives.WriteUInt32BigEndian(attributeBytes.AsSpan(4), permissions);

            var attributes = SftpFileAttributes.FromBytes(attributeBytes);

            Assert.AreEqual(isSocket, attributes.IsSocket);
            Assert.AreEqual(isSymbolicLink, attributes.IsSymbolicLink);
            Assert.AreEqual(isRegularFile, attributes.IsRegularFile);
            Assert.AreEqual(isBlockDevice, attributes.IsBlockDevice);
            Assert.AreEqual(isDirectory, attributes.IsDirectory);
            Assert.AreEqual(isCharacterDevice, attributes.IsCharacterDevice);
            Assert.AreEqual(isNamedPipe, attributes.IsNamedPipe);
        }

        [TestMethod]
        public void FromBytesGetBytes()
        {
            // 81a4 in hex = 100644 in octal
            var attributes = SftpFileAttributes.FromBytes([0, 0, 0, 0x4, 0, 0, 0x81, 0xa4]);

            Assert.IsTrue(attributes.IsRegularFile);

            Assert.IsFalse(attributes.IsUIDBitSet);
            Assert.IsFalse(attributes.IsGroupIDBitSet);
            Assert.IsFalse(attributes.IsStickyBitSet);
            Assert.IsTrue(attributes.OwnerCanRead);
            Assert.IsTrue(attributes.OwnerCanWrite);
            Assert.IsFalse(attributes.OwnerCanExecute);
            Assert.IsTrue(attributes.GroupCanRead);
            Assert.IsFalse(attributes.GroupCanWrite);
            Assert.IsFalse(attributes.GroupCanExecute);
            Assert.IsTrue(attributes.OthersCanRead);
            Assert.IsFalse(attributes.OthersCanWrite);
            Assert.IsFalse(attributes.OthersCanExecute);

            Assert.AreEqual(-1, attributes.Size); // Erm, OK?
            Assert.AreEqual(-1, attributes.UserId);
            Assert.AreEqual(-1, attributes.GroupId);

            Assert.AreEqual(default, attributes.LastAccessTimeUtc);
            Assert.AreEqual(DateTimeKind.Utc, attributes.LastAccessTimeUtc.Kind);

            Assert.AreEqual(default, attributes.LastWriteTimeUtc);
            Assert.AreEqual(DateTimeKind.Utc, attributes.LastWriteTimeUtc.Kind);

            Assert.AreEqual("-rw-r--r--", attributes.ToString());

            // No changes
            CollectionAssert.AreEqual(
                new byte[] { 0, 0, 0, 0 },
                attributes.GetBytes());


            // Permissions change
            attributes.IsUIDBitSet = true;
            attributes.OwnerCanExecute = true;

            CollectionAssert.AreEqual(
                new byte[] { 0, 0, 0, 0x4, 0, 0, 0x89, 0xe4 },
                attributes.GetBytes());

            Assert.AreEqual("-rwsr--r--", attributes.ToString());

            // Size change
            attributes.Size = 123;

            CollectionAssert.AreEqual(
                new byte[] {
                    0, 0, 0, 0x1 | 0x4,
                    0, 0, 0, 0, 0, 0, 0, 123,
                    0, 0, 0x89, 0xe4 },
                attributes.GetBytes());

            Assert.IsTrue(attributes.ToString().StartsWith("-rwsr--r-- Size: ", StringComparison.Ordinal));

            // Uid/gid change
            attributes.UserId = 99;
            attributes.GroupId = 66;

            CollectionAssert.AreEqual(
                new byte[] {
                    0, 0, 0, 0x1 | 0x2 | 0x4,
                    0, 0, 0, 0, 0, 0, 0, 123,
                    0, 0, 0, 99, 0, 0, 0, 66,
                    0, 0, 0x89, 0xe4 },
                attributes.GetBytes());


            // Access/mod time change
            attributes.LastAccessTimeUtc = new DateTime(2025, 08, 10, 17, 51, 37, DateTimeKind.Unspecified);
            attributes.LastWriteTime = new DateTimeOffset(2016, 12, 02, 13, 18, 20, TimeSpan.FromHours(3)).LocalDateTime;

            var expectedTimeBytes = new byte[8];
            BinaryPrimitives.WriteUInt32BigEndian(expectedTimeBytes, 1754848297);
            BinaryPrimitives.WriteUInt32BigEndian(expectedTimeBytes.AsSpan(4), 1480673900);

            CollectionAssert.AreEqual(
                new byte[] {
                    0, 0, 0, 0x1 | 0x2 | 0x4 | 0x8,
                    0, 0, 0, 0, 0, 0, 0, 123,
                    0, 0, 0, 99, 0, 0, 0, 66,
                    0, 0, 0x89, 0xe4
                }.Concat(expectedTimeBytes),
                attributes.GetBytes());

            Assert.AreEqual(new DateTime(2016, 12, 02, 10, 18, 20, DateTimeKind.Utc), attributes.LastWriteTimeUtc);
            Assert.AreEqual(DateTimeKind.Utc, attributes.LastWriteTimeUtc.Kind);

            var attributesString = attributes.ToString();
            Assert.IsTrue(attributesString.StartsWith("-rwsr--r-- Size: ", StringComparison.Ordinal));
            Assert.Contains(" LastWriteTime: ", attributesString, StringComparison.CurrentCulture);
        }

        [TestMethod]
        [DataRow((short)8888)]
        [DataRow((short)10000)]
        [DataRow((short)8000)]
        [DataRow((short)0080)]
        [DataRow((short)0008)]
        [DataRow((short)1797)]
        [DataRow((short)-1)]
        [DataRow(short.MaxValue)]
        public void SetPermissions_InvalidMode_ThrowsArgumentOutOfRangeException(short mode)
        {
            var attributes = SftpFileAttributes.FromBytes([0, 0, 0, 0]);

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => attributes.SetPermissions(mode));
            Assert.AreEqual("mode", ex.ParamName);
        }

        [TestMethod]
        [DataRow((short)0777, false, false, false, true, true, true, true, true, true, true, true, true)]
        [DataRow((short)0755, false, false, false, true, true, true, true, false, true, true, false, true)]
        [DataRow((short)0644, false, false, false, true, true, false, true, false, false, true, false, false)]
        [DataRow((short)0444, false, false, false, true, false, false, true, false, false, true, false, false)]
        [DataRow((short)0000, false, false, false, false, false, false, false, false, false, false, false, false)]
        [DataRow((short)4700, true, false, false, true, true, true, false, false, false, false, false, false)]
        [DataRow((short)3001, false, true, true, false, false, false, false, false, false, false, false, true)]
        [DataRow((short)7777, true, true, true, true, true, true, true, true, true, true, true, true)]
        public void SetPermissions_ValidMode(
            short mode,
            bool setUid, bool setGid, bool sticky,
            bool ownerRead, bool ownerWrite, bool ownerExec,
            bool groupRead, bool groupWrite, bool groupExec,
            bool othersRead, bool othersWrite, bool othersExec)
        {
            var attributes = SftpFileAttributes.FromBytes([0, 0, 0, 0]);

            attributes.SetPermissions(mode);

            Assert.AreEqual(setUid, attributes.IsUIDBitSet);
            Assert.AreEqual(setGid, attributes.IsGroupIDBitSet);
            Assert.AreEqual(sticky, attributes.IsStickyBitSet);
            Assert.AreEqual(ownerRead, attributes.OwnerCanRead);
            Assert.AreEqual(ownerWrite, attributes.OwnerCanWrite);
            Assert.AreEqual(ownerExec, attributes.OwnerCanExecute);
            Assert.AreEqual(groupRead, attributes.GroupCanRead);
            Assert.AreEqual(groupWrite, attributes.GroupCanWrite);
            Assert.AreEqual(groupExec, attributes.GroupCanExecute);
            Assert.AreEqual(othersRead, attributes.OthersCanRead);
            Assert.AreEqual(othersWrite, attributes.OthersCanWrite);
            Assert.AreEqual(othersExec, attributes.OthersCanExecute);
        }

        [TestMethod]
        [DataRow(0xC000u, (short)1770, "srwxrwx--T")] // Socket
        [DataRow(0xA000u, (short)2707, "lrwx--Srwx")] // Symbolic link
        [DataRow(0x8000u, (short)4755, "-rwsr-xr-x")] // Regular file
        [DataRow(0x8000u, (short)4644, "-rwSr--r--")] // Regular file
        [DataRow(0x6000u, (short)2711, "brwx--s--x")] // Block device
        [DataRow(0x4000u, (short)1777, "drwxrwxrwt")] // Directory
        [DataRow(0x4000u, (short)1776, "drwxrwxrwT")] // Directory
        [DataRow(0x2000u, (short)0660, "crw-rw----")] // Character device
        [DataRow(0x1000u, (short)0022, "p----w--w-")] // Named pipe
        public void ToStringWithPermissions(
            uint fileType,
            short permissions,
            string expected)
        {
            var attributeBytes = new byte[8];
            attributeBytes[3] = 0x4; // SSH_FILEXFER_ATTR_PERMISSIONS
            BinaryPrimitives.WriteUInt32BigEndian(attributeBytes.AsSpan(4), fileType);

            var attributes = SftpFileAttributes.FromBytes(attributeBytes);

            attributes.SetPermissions(permissions);

            Assert.AreEqual(expected, attributes.ToString());
        }
    }
}
