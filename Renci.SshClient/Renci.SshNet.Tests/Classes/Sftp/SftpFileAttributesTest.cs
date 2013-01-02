namespace Renci.SshNet.Tests.Classes.Sftp
{
    /// <summary>
    /// Contains SFTP file attributes.
    /// </summary>
    public class SftpFileAttributesTest
    {
        /// <summary>
        /// Gets or sets the time the current file or directory was last accessed.
        /// </summary>
        /// <value>
        /// The time that the current file or directory was last accessed.
        /// </value>
        //public DateTime LastAccessTime { get; set; }

        ///// <summary>
        ///// Gets or sets the time when the current file or directory was last written to.
        ///// </summary>
        ///// <value>
        ///// The time the current file was last written.
        ///// </value>
        //public DateTime LastWriteTime { get; set; }

        ///// <summary>
        ///// Gets or sets the size, in bytes, of the current file.
        ///// </summary>
        ///// <value>
        ///// The size of the current file in bytes.
        ///// </value>
        //public long Size { get; set; }

        ///// <summary>
        ///// Gets or sets file user id.
        ///// </summary>
        ///// <value>
        ///// File user id.
        ///// </value>
        //public int UserId { get; set; }

        ///// <summary>
        ///// Gets or sets file group id.
        ///// </summary>
        ///// <value>
        ///// File group id.
        ///// </value>
        //public int GroupId { get; set; }

        ///// <summary>
        ///// Gets a value indicating whether file represents a socket.
        ///// </summary>
        ///// <value>
        /////   <c>true</c> if file represents a socket; otherwise, <c>false</c>.
        ///// </value>
        //public bool IsSocket { get; private set; }

        ///// <summary>
        ///// Gets a value indicating whether file represents a symbolic link.
        ///// </summary>
        ///// <value>
        ///// 	<c>true</c> if file represents a symbolic link; otherwise, <c>false</c>.
        ///// </value>
        //public bool IsSymbolicLink { get; private set; }

        ///// <summary>
        ///// Gets a value indicating whether file represents a regular file.
        ///// </summary>
        ///// <value>
        ///// 	<c>true</c> if file represents a regular file; otherwise, <c>false</c>.
        ///// </value>
        //public bool IsRegularFile { get; private set; }

        ///// <summary>
        ///// Gets a value indicating whether file represents a block device.
        ///// </summary>
        ///// <value>
        ///// 	<c>true</c> if file represents a block device; otherwise, <c>false</c>.
        ///// </value>
        //public bool IsBlockDevice { get; private set; }

        ///// <summary>
        ///// Gets a value indicating whether file represents a directory.
        ///// </summary>
        ///// <value>
        ///// 	<c>true</c> if file represents a directory; otherwise, <c>false</c>.
        ///// </value>
        //public bool IsDirectory { get; private set; }

        ///// <summary>
        ///// Gets a value indicating whether file represents a character device.
        ///// </summary>
        ///// <value>
        ///// 	<c>true</c> if file represents a character device; otherwise, <c>false</c>.
        ///// </value>
        //public bool IsCharacterDevice { get; private set; }

        ///// <summary>
        ///// Gets a value indicating whether file represents a named pipe.
        ///// </summary>
        ///// <value>
        ///// 	<c>true</c> if file represents a named pipe; otherwise, <c>false</c>.
        ///// </value>
        //public bool IsNamedPipe { get; private set; }

        ///// <summary>
        ///// Gets a value indicating whether the owner can read from this file.
        ///// </summary>
        ///// <value>
        /////   <c>true</c> if owner can read from this file; otherwise, <c>false</c>.
        ///// </value>
        //public bool OwnerCanRead { get; set; }

        ///// <summary>
        ///// Gets a value indicating whether the owner can write into this file.
        ///// </summary>
        ///// <value>
        /////   <c>true</c> if owner can write into this file; otherwise, <c>false</c>.
        ///// </value>
        //public bool OwnerCanWrite { get; set; }

        ///// <summary>
        ///// Gets a value indicating whether the owner can execute this file.
        ///// </summary>
        ///// <value>
        /////   <c>true</c> if owner can execute this file; otherwise, <c>false</c>.
        ///// </value>
        //public bool OwnerCanExecute { get; set; }

        ///// <summary>
        ///// Gets a value indicating whether the group members can read from this file.
        ///// </summary>
        ///// <value>
        /////   <c>true</c> if group members can read from this file; otherwise, <c>false</c>.
        ///// </value>
        //public bool GroupCanRead { get; set; }

        ///// <summary>
        ///// Gets a value indicating whether the group members can write into this file.
        ///// </summary>
        ///// <value>
        /////   <c>true</c> if group members can write into this file; otherwise, <c>false</c>.
        ///// </value>
        //public bool GroupCanWrite { get; set; }

        ///// <summary>
        ///// Gets a value indicating whether the group members can execute this file.
        ///// </summary>
        ///// <value>
        /////   <c>true</c> if group members can execute this file; otherwise, <c>false</c>.
        ///// </value>
        //public bool GroupCanExecute { get; set; }

        ///// <summary>
        ///// Gets a value indicating whether the others can read from this file.
        ///// </summary>
        ///// <value>
        /////   <c>true</c> if others can read from this file; otherwise, <c>false</c>.
        ///// </value>
        //public bool OthersCanRead { get; set; }

        ///// <summary>
        ///// Gets a value indicating whether the others can write into this file.
        ///// </summary>
        ///// <value>
        /////   <c>true</c> if others can write into this file; otherwise, <c>false</c>.
        ///// </value>
        //public bool OthersCanWrite { get; set; }

        ///// <summary>
        ///// Gets a value indicating whether the others can execute this file.
        ///// </summary>
        ///// <value>
        /////   <c>true</c> if others can execute this file; otherwise, <c>false</c>.
        ///// </value>
        //public bool OthersCanExecute { get; set; }

        ///// <summary>
        ///// Gets or sets the extensions.
        ///// </summary>
        ///// <value>
        ///// The extensions.
        ///// </value>
        //public IDictionary<string, string> Extensions { get; private set; }

        ///// <summary>
        ///// Sets the permissions.
        ///// </summary>
        ///// <param name="mode">The mode.</param>
        //public void SetPermissions(short mode)
        //{
        //    if (mode < 0 || mode > 999)
        //    {
        //        throw new ArgumentOutOfRangeException("mode");
        //    }

        //    var modeBytes = mode.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0').ToArray();

        //    var permission = (modeBytes[0] & 0x0F) * 8 * 8 + (modeBytes[1] & 0x0F) * 8 + (modeBytes[2] & 0x0F);

        //    this.OwnerCanRead = (permission & S_IRUSR) == S_IRUSR;
        //    this.OwnerCanWrite = (permission & S_IWUSR) == S_IWUSR;
        //    this.OwnerCanExecute = (permission & S_IXUSR) == S_IXUSR;

        //    this.GroupCanRead = (permission & S_IRGRP) == S_IRGRP;
        //    this.GroupCanWrite = (permission & S_IWGRP) == S_IWGRP;
        //    this.GroupCanExecute = (permission & S_IXGRP) == S_IXGRP;

        //    this.OthersCanRead = (permission & S_IROTH) == S_IROTH;
        //    this.OthersCanWrite = (permission & S_IWOTH) == S_IWOTH;
        //    this.OthersCanExecute = (permission & S_IXOTH) == S_IXOTH;
        //}
    }
}