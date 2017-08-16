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
        /// <param name="nullOnError">if set to <c>true</c> returns <c>null</c> instead of throwing an exception.</param>
        /// <returns>
        /// File attributes
        /// </returns>
        SftpFileAttributes RequestFStat(byte[] handle, bool nullOnError);

        /// <summary>
        /// Performs SSH_FXP_STAT request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="nullOnError">if set to <c>true</c> returns null instead of throwing an exception.</param>
        /// <returns>
        /// File attributes
        /// </returns>
        SftpFileAttributes RequestStat(string path, bool nullOnError = false);

        /// <summary>
        /// Performs SSH_FXP_STAT request
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> delegate that is executed when <see cref="BeginOpen(string, Flags, AsyncCallback, object)"/> completes.</param>
        /// <param name="state">An object that contains any additional user-defined data.</param>
        /// <returns>
        /// A <see cref="SftpOpenAsyncResult"/> that represents the asynchronous call.
        /// </returns>
        SFtpStatAsyncResult BeginStat(string path, AsyncCallback callback, object state);

        /// <summary>
        /// Handles the end of an asynchronous read.
        /// </summary>
        /// <param name="asyncResult">An <see cref="SFtpStatAsyncResult"/> that represents an asynchronous call.</param>
        /// <returns>
        /// The file attributes.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <c>null</c>.</exception>
        SftpFileAttributes EndStat(SFtpStatAsyncResult asyncResult);

        /// <summary>
        /// Performs SSH_FXP_LSTAT request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// File attributes
        /// </returns>
        SftpFileAttributes RequestLStat(string path);

        /// <summary>
        /// Performs SSH_FXP_LSTAT request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> delegate that is executed when <see cref="BeginLStat(string, AsyncCallback, object)"/> completes.</param>
        /// <param name="state">An object that contains any additional user-defined data.</param>
        /// <returns>
        /// A <see cref="SFtpStatAsyncResult"/> that represents the asynchronous call.
        /// </returns>
        SFtpStatAsyncResult BeginLStat(string path, AsyncCallback callback, object state);

        /// <summary>
        /// Handles the end of an asynchronous SSH_FXP_LSTAT request.
        /// </summary>
        /// <param name="asyncResult">An <see cref="SFtpStatAsyncResult"/> that represents an asynchronous call.</param>
        /// <returns>
        /// The file attributes.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <c>null</c>.</exception>
        SftpFileAttributes EndLStat(SFtpStatAsyncResult asyncResult);

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
        /// Performs SSH_FXP_OPEN request
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> delegate that is executed when <see cref="BeginOpen(string, Flags, AsyncCallback, object)"/> completes.</param>
        /// <param name="state">An object that contains any additional user-defined data.</param>
        /// <returns>
        /// A <see cref="SftpOpenAsyncResult"/> that represents the asynchronous call.
        /// </returns>
        SftpOpenAsyncResult BeginOpen(string path, Flags flags, AsyncCallback callback, object state);

        /// <summary>
        /// Handles the end of an asynchronous read.
        /// </summary>
        /// <param name="asyncResult">An <see cref="SftpOpenAsyncResult"/> that represents an asynchronous call.</param>
        /// <returns>
        /// A <see cref="byte"/> array representing a file handle.
        /// </returns>
        /// <remarks>
        /// If all available data has been read, the <see cref="EndOpen(SftpOpenAsyncResult)"/> method completes
        /// immediately and returns zero bytes.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <c>null</c>.</exception>
        byte[] EndOpen(SftpOpenAsyncResult asyncResult);

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
        /// Begins an asynchronous read using a SSH_FXP_READ request.
        /// </summary>
        /// <param name="handle">The handle to the file to read from.</param>
        /// <param name="offset">The offset in the file to start reading from.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> delegate that is executed when <see cref="BeginRead(byte[], ulong, uint, AsyncCallback, object)"/> completes.</param>
        /// <param name="state">An object that contains any additional user-defined data.</param>
        /// <returns>
        /// A <see cref="SftpReadAsyncResult"/> that represents the asynchronous call.
        /// </returns>
        SftpReadAsyncResult BeginRead(byte[] handle, ulong offset, uint length, AsyncCallback callback, object state);

        /// <summary>
        /// Handles the end of an asynchronous read.
        /// </summary>
        /// <param name="asyncResult">An <see cref="SftpReadAsyncResult"/> that represents an asynchronous call.</param>
        /// <returns>
        /// A <see cref="byte"/> array representing the data read.
        /// </returns>
        /// <remarks>
        /// If all available data has been read, the <see cref="EndRead(SftpReadAsyncResult)"/> method completes
        /// immediately and returns zero bytes.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <c>null</c>.</exception>
        byte[] EndRead(SftpReadAsyncResult asyncResult);

        /// <summary>
        /// Performs SSH_FXP_READDIR request
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns></returns>
        KeyValuePair<string, SftpFileAttributes>[] RequestReadDir(byte[] handle);

        /// <summary>
        /// Performs SSH_FXP_REALPATH request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> delegate that is executed when <see cref="BeginRealPath(string, AsyncCallback, object)"/> completes.</param>
        /// <param name="state">An object that contains any additional user-defined data.</param>
        /// <returns>
        /// A <see cref="SftpRealPathAsyncResult"/> that represents the asynchronous call.
        /// </returns>
        SftpRealPathAsyncResult BeginRealPath(string path, AsyncCallback callback, object state);

        /// <summary>
        /// Handles the end of an asynchronous SSH_FXP_REALPATH request.
        /// </summary>
        /// <param name="asyncResult">An <see cref="SftpRealPathAsyncResult"/> that represents an asynchronous call.</param>
        /// <returns>
        /// The absolute path.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <c>null</c>.</exception>
        string EndRealPath(SftpRealPathAsyncResult asyncResult);

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
        /// Performs SSH_FXP_CLOSE request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        void RequestClose(byte[] handle);

        /// <summary>
        /// Performs SSH_FXP_CLOSE request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> delegate that is executed when <see cref="BeginClose(byte[], AsyncCallback, object)"/> completes.</param>
        /// <param name="state">An object that contains any additional user-defined data.</param>
        /// <returns>
        /// A <see cref="SftpCloseAsyncResult"/> that represents the asynchronous call.
        /// </returns>
        SftpCloseAsyncResult BeginClose(byte[] handle, AsyncCallback callback, object state);

        /// <summary>
        /// Handles the end of an asynchronous close.
        /// </summary>
        /// <param name="asyncResult">An <see cref="SftpCloseAsyncResult"/> that represents an asynchronous call.</param>
        /// <exception cref="ArgumentNullException"><paramref name="asyncResult"/> is <c>null</c>.</exception>
        void EndClose(SftpCloseAsyncResult asyncResult);

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

        ISftpFileReader CreateFileReader(byte[] handle, ISftpSession sftpSession, uint chunkSize, int maxPendingReads, long? fileSize);
    }
}
