using System;
using System.Collections.Generic;
using System.Threading;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp
{
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
        /// Resolves a given path into an absolute path on the server.
        /// </summary>
        /// <param name="path">The path to resolve.</param>
        /// <returns>
        /// The absolute path.
        /// </returns>
        string GetCanonicalPath(string path);

        /// <summary>
        /// Performs SSH_FXP_FSTAT request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns>
        /// File attributes
        /// </returns>
        SftpFileAttributes RequestFStat(byte[] handle);

        /// <summary>
        /// Performs SSH_FXP_LSTAT request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// File attributes
        /// </returns>
        SftpFileAttributes RequestLStat(string path);

        /// <summary>
        /// Performs SSH_FXP_MKDIR request.
        /// </summary>
        /// <param name="path">The path.</param>
        void RequestMkDir(string path);

        /// <summary>
        /// Performs SSH_FXP_OPEN request
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="nullOnError">if set to <c>true</c> returns <c>null</c> instead of throwing an exception.</param>
        /// <returns>File handle.</returns>
        byte[] RequestOpen(string path, Flags flags, bool nullOnError = false);

        /// <summary>
        /// Performs SSH_FXP_OPENDIR request
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="nullOnError">if set to <c>true</c> returns null instead of throwing an exception.</param>
        /// <returns>File handle.</returns>
        byte[] RequestOpenDir(string path, bool nullOnError = false);

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
        /// <returns>data array; null if EOF</returns>
        byte[] RequestRead(byte[] handle, ulong offset, uint length);

        /// <summary>
        /// Performs SSH_FXP_READDIR request
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns></returns>
        KeyValuePair<string, SftpFileAttributes>[] RequestReadDir(byte[] handle);

        /// <summary>
        /// Performs SSH_FXP_REMOVE request.
        /// </summary>
        /// <param name="path">The path.</param>
        void RequestRemove(string path);

        /// <summary>
        /// Performs SSH_FXP_RENAME request.
        /// </summary>
        /// <param name="oldPath">The old path.</param>
        /// <param name="newPath">The new path.</param>
        void RequestRename(string oldPath, string newPath);

        /// <summary>
        /// Performs SSH_FXP_RMDIR request.
        /// </summary>
        /// <param name="path">The path.</param>
        void RequestRmDir(string path);

        /// <summary>
        /// Performs SSH_FXP_SETSTAT request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="attributes">The attributes.</param>
        void RequestSetStat(string path, SftpFileAttributes attributes);

        /// <summary>
        /// Performs statvfs@openssh.com extended request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="nullOnError">if set to <c>true</c> [null on error].</param>
        /// <returns></returns>
        SftpFileSytemInformation RequestStatVfs(string path, bool nullOnError = false);

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
        /// <param name="offset">The offset.</param>
        /// <param name="data">The data to send.</param>
        /// <param name="wait">The wait event handle if needed.</param>
        /// <param name="writeCompleted">The callback to invoke when the write has completed.</param>
        void RequestWrite(byte[] handle, ulong offset, byte[] data, AutoResetEvent wait, Action<SftpStatusResponse> writeCompleted = null);

        /// <summary>
        /// Performs SSH_FXP_CLOSE request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        void RequestClose(byte[] handle);

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
