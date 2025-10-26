using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Represents an SFTP session.
    /// </summary>
    internal interface ISftpSession : ISubsystemSession
    {
        /// <summary>
        /// Gets the SFTP protocol version.
        /// </summary>
        /// <value>
        /// The SFTP protocol version.
        /// </value>
        uint ProtocolVersion { get; }

        /// <summary>
        /// Gets the remote working directory.
        /// </summary>
        /// <value>
        /// The remote working directory.
        /// </value>
        string WorkingDirectory { get; }

        /// <summary>
        /// Changes the current working directory to the specified path.
        /// </summary>
        /// <param name="path">The new working directory.</param>
        void ChangeDirectory(string path);

        /// <summary>
        /// Asynchronously requests to change the current working directory to the specified path.
        /// </summary>
        /// <param name="path">The new working directory.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> that tracks the asynchronous change working directory request.</returns>
        Task ChangeDirectoryAsync(string path, CancellationToken cancellationToken);

        /// <summary>
        /// Resolves a given path into an absolute path on the server.
        /// </summary>
        /// <param name="path">The path to resolve.</param>
        /// <returns>
        /// The absolute path.
        /// </returns>
        string GetCanonicalPath(string path);

        /// <summary>
        /// Asynchronously resolves a given path into an absolute path on the server.
        /// </summary>
        /// <param name="path">The path to resolve.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents an asynchronous operation to resolve <paramref name="path"/> into
        /// an absolute path. The value of its <see cref="Task{Task}.Result"/> contains the absolute
        /// path of the specified path.
        /// </returns>
        Task<string> GetCanonicalPathAsync(string path, CancellationToken cancellationToken);

        /// <summary>
        /// Asynchronously performs a <c>SSH_FXP_FSTAT</c> request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns>
        /// The file attributes.
        /// </returns>
        SftpFileAttributes RequestFStat(byte[] handle);

        /// <summary>
        /// Asynchronously performs a <c>SSH_FXP_FSTAT</c> request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task the represents the asynchronous <c>SSH_FXP_FSTAT</c> request. The value of its
        /// <see cref="Task{Task}.Result"/> contains the file attributes of the specified handle.
        /// </returns>
        Task<SftpFileAttributes> RequestFStatAsync(byte[] handle, CancellationToken cancellationToken);

        /// <summary>
        /// Performs SSH_FXP_STAT request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// File attributes.
        /// </returns>
        SftpFileAttributes RequestStat(string path);

        /// <summary>
        /// Performs SSH_FXP_LSTAT request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// File attributes.
        /// </returns>
        SftpFileAttributes RequestLStat(string path);

        /// <summary>
        ///  Asynchronously performs SSH_FXP_LSTAT request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task the represents the asynchronous <c>SSH_FXP_LSTAT</c> request. The value of its
        /// <see cref="Task{SftpFileAttributes}.Result"/> contains the file attributes of the specified path.
        /// </returns>
        Task<SftpFileAttributes> RequestLStatAsync(string path, CancellationToken cancellationToken);

        /// <summary>
        /// Performs SSH_FXP_MKDIR request.
        /// </summary>
        /// <param name="path">The path.</param>
        void RequestMkDir(string path);

        /// <summary>
        /// Asynchronously performs SSH_FXP_MKDIR request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous <c>SSH_FXP_MKDIR</c> operation.</returns>
        Task RequestMkDirAsync(string path, CancellationToken cancellationToken);

        /// <summary>
        /// Performs a <c>SSH_FXP_OPEN</c> request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="flags">The flags.</param>
        /// <returns>
        /// The file handle for the specified path.
        /// </returns>
        byte[] RequestOpen(string path, Flags flags);

        /// <summary>
        /// Asynchronously performs a <c>SSH_FXP_OPEN</c> request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task the represents the asynchronous <c>SSH_FXP_OPEN</c> request. The value of its
        /// <see cref="Task{Task}.Result"/> contains the file handle of the specified path.
        /// </returns>
        Task<byte[]> RequestOpenAsync(string path, Flags flags, CancellationToken cancellationToken);

        /// <summary>
        /// Performs a <c>SSH_FXP_OPENDIR</c> request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// A file handle for the specified path.
        /// </returns>
        byte[] RequestOpenDir(string path);

        /// <summary>
        /// Asynchronously performs a <c>SSH_FXP_OPENDIR</c> request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous <c>SSH_FXP_OPENDIR</c> request. The value of its
        /// <see cref="Task{Task}.Result"/> contains the handle of the specified path.
        /// </returns>
        Task<byte[]> RequestOpenDirAsync(string path, CancellationToken cancellationToken);

        /// <summary>
        /// Performs posix-rename@openssh.com extended request.
        /// </summary>
        /// <param name="oldPath">The old path.</param>
        /// <param name="newPath">The new path.</param>
        void RequestPosixRename(string oldPath, string newPath);

        /// <summary>
        /// Performs SSH_FXP_READ request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>
        /// The data that was read, or an empty array when the end of the file was reached.
        /// </returns>
        byte[] RequestRead(byte[] handle, ulong offset, uint length);

        /// <summary>
        /// Asynchronously performs a <c>SSH_FXP_READ</c> request.
        /// </summary>
        /// <param name="handle">The handle to the file to read from.</param>
        /// <param name="offset">The offset in the file to start reading from.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous <c>SSH_FXP_READ</c> request. The value of
        /// its <see cref="Task{Task}.Result"/> contains the data read from the file, or an empty
        /// array when the end of the file is reached.
        /// </returns>
        Task<byte[]> RequestReadAsync(byte[] handle, ulong offset, uint length, CancellationToken cancellationToken);

        /// <summary>
        /// Performs a <c>SSH_FXP_READDIR</c> request.
        /// </summary>
        /// <param name="handle">The handle of the directory to read.</param>
        /// <returns>
        /// A <see cref="Dictionary{TKey,TValue}"/> where the <c>key</c> is the name of a file in the directory
        /// and the <c>value</c> is the <see cref="SftpFileAttributes"/> of the file.
        /// </returns>
        KeyValuePair<string, SftpFileAttributes>[] RequestReadDir(byte[] handle);

        /// <summary>
        /// Performs a <c>SSH_FXP_READDIR</c> request.
        /// </summary>
        /// <param name="handle">The handle of the directory to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous <c>SSH_FXP_READDIR</c> request. The value of its
        /// <see cref="Task{Task}.Result"/> contains a <see cref="Dictionary{TKey,TValue}"/> where the
        /// <c>key</c> is the name of a file in the directory and the <c>value</c> is the <see cref="SftpFileAttributes"/>
        /// of the file.
        /// </returns>
        Task<KeyValuePair<string, SftpFileAttributes>[]> RequestReadDirAsync(byte[] handle, CancellationToken cancellationToken);

        /// <summary>
        /// Performs a <c>SSH_FXP_REMOVE</c> request.
        /// </summary>
        /// <param name="path">The path.</param>
        void RequestRemove(string path);

        /// <summary>
        /// Asynchronously performs a <c>SSH_FXP_REMOVE</c> request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous <c>SSH_FXP_REMOVE</c> request.
        /// </returns>
        Task RequestRemoveAsync(string path, CancellationToken cancellationToken);

        /// <summary>
        /// Performs a <c>SSH_FXP_RENAME</c> request.
        /// </summary>
        /// <param name="oldPath">The old path.</param>
        /// <param name="newPath">The new path.</param>
        void RequestRename(string oldPath, string newPath);

        /// <summary>
        /// Asynchronously performs a <c>SSH_FXP_RENAME</c> request.
        /// </summary>
        /// <param name="oldPath">The old path.</param>
        /// <param name="newPath">The new path.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous <c>SSH_FXP_RENAME</c> request.
        /// </returns>
        Task RequestRenameAsync(string oldPath, string newPath, CancellationToken cancellationToken);

        /// <summary>
        /// Performs SSH_FXP_RMDIR request.
        /// </summary>
        /// <param name="path">The path.</param>
        void RequestRmDir(string path);

        /// <summary>
        /// Asynchronously performs an SSH_FXP_RMDIR request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous <c>SSH_FXP_RMDIR</c> request.
        /// </returns>
        Task RequestRmDirAsync(string path, CancellationToken cancellationToken);

        /// <summary>
        /// Performs SSH_FXP_SETSTAT request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="attributes">The attributes.</param>
        void RequestSetStat(string path, SftpFileAttributes attributes);

        /// <summary>
        /// Performs a <c>statvfs@openssh.com</c> extended request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// The file system information for the specified path.
        /// </returns>
        SftpFileSystemInformation RequestStatVfs(string path);

        /// <summary>
        /// Asynchronously performs a <c>statvfs@openssh.com</c> extended request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the <c>statvfs@openssh.com</c> extended request. The value of its
        /// <see cref="Task{Task}.Result"/> contains the file system information for the specified
        /// path.
        /// </returns>
        Task<SftpFileSystemInformation> RequestStatVfsAsync(string path, CancellationToken cancellationToken);

        /// <summary>
        /// Performs SSH_FXP_SYMLINK request.
        /// </summary>
        /// <param name="linkpath">The linkpath.</param>
        /// <param name="targetpath">The targetpath.</param>
        void RequestSymLink(string linkpath, string targetpath);

        /// <summary>
        /// Performs SSH_FXP_FSETSTAT request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="attributes">The attributes.</param>
        void RequestFSetStat(byte[] handle, SftpFileAttributes attributes);

        /// <summary>
        /// Performs SSH_FXP_WRITE request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="serverOffset">The the zero-based offset (in bytes) relative to the beginning of the file that the write must start at.</param>
        /// <param name="data">The buffer holding the data to write.</param>
        /// <param name="offset">the zero-based offset in <paramref name="data" /> at which to begin taking bytes to write.</param>
        /// <param name="length">The length (in bytes) of the data to write.</param>
        /// <param name="wait">The wait event handle if needed.</param>
        /// <param name="writeCompleted">The callback to invoke when the write has completed.</param>
        void RequestWrite(byte[] handle,
                          ulong serverOffset,
                          byte[] data,
                          int offset,
                          int length,
                          AutoResetEvent wait,
                          Action<SftpStatusResponse> writeCompleted = null);

        /// <summary>
        /// Asynchronouly performs a <c>SSH_FXP_WRITE</c> request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="serverOffset">The the zero-based offset (in bytes) relative to the beginning of the file that the write must start at.</param>
        /// <param name="data">The buffer holding the data to write.</param>
        /// <param name="offset">the zero-based offset in <paramref name="data" /> at which to begin taking bytes to write.</param>
        /// <param name="length">The length (in bytes) of the data to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous <c>SSH_FXP_WRITE</c> request.
        /// </returns>
        Task RequestWriteAsync(byte[] handle, ulong serverOffset, byte[] data, int offset, int length, CancellationToken cancellationToken);

        /// <summary>
        /// Performs a <c>SSH_FXP_CLOSE</c> request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        void RequestClose(byte[] handle);

        /// <summary>
        /// Performs a <c>SSH_FXP_CLOSE</c> request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>
        /// A task that represents the asynchronous <c>SSH_FXP_CLOSE</c> request.
        /// </returns>
        Task RequestCloseAsync(byte[] handle, CancellationToken cancellationToken);

        /// <summary>
        /// Calculates the optimal size of the buffer to read data from the channel.
        /// </summary>
        /// <param name="bufferSize">The buffer size configured on the client.</param>
        /// <returns>
        /// The optimal size of the buffer to read data from the channel.
        /// </returns>
        uint CalculateOptimalReadLength(uint bufferSize);

        /// <summary>
        /// Calculates the optimal size of the buffer to write data on the channel.
        /// </summary>
        /// <param name="bufferSize">The buffer size configured on the client.</param>
        /// <param name="handle">The file handle.</param>
        /// <returns>
        /// The optimal size of the buffer to write data on the channel.
        /// </returns>
        /// <remarks>
        /// Currently, we do not take the remote window size into account.
        /// </remarks>
        uint CalculateOptimalWriteLength(uint bufferSize, byte[] handle);
    }
}
