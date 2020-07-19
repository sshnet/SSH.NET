using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Renci.SshNet.Sftp;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Implementation of the SSH File Transfer Protocol (SFTP) over SSH.
    /// </summary>
    public interface ISftpClient
    {
        /// <summary>
        /// Gets or sets the maximum size of the buffer in bytes.
        /// </summary>
        /// <value>
        /// The size of the buffer. The default buffer size is 32768 bytes (32 KB).
        /// </value>
        /// <remarks>
        /// <para>
        /// For write operations, this limits the size of the payload for
        /// individual SSH_FXP_WRITE messages. The actual size is always
        /// capped at the maximum packet size supported by the peer
        /// (minus the size of protocol fields).
        /// </para>
        /// <para>
        /// For read operations, this controls the size of the payload which
        /// is requested from the peer in a SSH_FXP_READ message. The peer
        /// will send the requested number of bytes in a SSH_FXP_DATA message,
        /// possibly split over multiple SSH_MSG_CHANNEL_DATA messages.
        /// </para>
        /// <para>
        /// To optimize the size of the SSH packets sent by the peer,
        /// the actual requested size will take into account the size of the
        /// SSH_FXP_DATA protocol fields.
        /// </para>
        /// <para>
        /// The size of the each indivual SSH_FXP_DATA message is limited to the
        /// local maximum packet size of the channel, which is set to <c>64 KB</c>
        /// for SSH.NET. However, the peer can limit this even further.
        /// </para>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        uint BufferSize { get; set; }

        /// <summary>
        /// Gets or sets the operation timeout.
        /// </summary>
        /// <value>
        /// The timeout to wait until an operation completes. The default value is negative
        /// one (-1) milliseconds, which indicates an infinite timeout period.
        /// </value>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> represents a value that is less than -1 or greater than <see cref="Int32.MaxValue"/> milliseconds.</exception>
        TimeSpan OperationTimeout { get; set; }

        /// <summary>
        /// Gets sftp protocol version.
        /// </summary>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        int ProtocolVersion { get; }

        /// <summary>
        /// Gets remote working directory.
        /// </summary>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        string WorkingDirectory { get; }

        /// <summary>
        /// Appends lines to a file, creating the file if it does not already exist.
        /// </summary>
        /// <param name="path">The file to append the lines to. The file is created if it does not already exist.</param>
        /// <param name="contents">The lines to append to the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is<b>null</b> <para>-or-</para> <paramref name="contents"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// The characters are written to the file using UTF-8 encoding without a Byte-Order Mark (BOM)
        /// </remarks>
        void AppendAllLines(string path, IEnumerable<string> contents);

        /// <summary>
        /// Appends lines to a file by using a specified encoding, creating the file if it does not already exist.
        /// </summary>
        /// <param name="path">The file to append the lines to. The file is created if it does not already exist.</param>
        /// <param name="contents">The lines to append to the file.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>. <para>-or-</para> <paramref name="contents"/> is <b>null</b>. <para>-or-</para> <paramref name="encoding"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding);

        /// <summary>
        /// Appends the specified string to the file, creating the file if it does not already exist.
        /// </summary>
        /// <param name="path">The file to append the specified string to.</param>
        /// <param name="contents">The string to append to the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>. <para>-or-</para> <paramref name="contents"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// The characters are written to the file using UTF-8 encoding without a Byte-Order Mark (BOM).
        /// </remarks>
        void AppendAllText(string path, string contents);

        /// <summary>
        /// Appends the specified string to the file, creating the file if it does not already exist.
        /// </summary>
        /// <param name="path">The file to append the specified string to.</param>
        /// <param name="contents">The string to append to the file.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>. <para>-or-</para> <paramref name="contents"/> is <b>null</b>. <para>-or-</para> <paramref name="encoding"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        void AppendAllText(string path, string contents, Encoding encoding);

        /// <summary>
        /// Creates a <see cref="StreamWriter"/> that appends UTF-8 encoded text to the specified file,
        /// creating the file if it does not already exist.
        /// </summary>
        /// <param name="path">The path to the file to append to.</param>
        /// <returns>
        /// A <see cref="StreamWriter"/> that appends text to a file using UTF-8 encoding without a
        /// Byte-Order Mark (BOM).
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        StreamWriter AppendText(string path);

        /// <summary>
        /// Creates a <see cref="StreamWriter"/> that appends text to a file using the specified
        /// encoding, creating the file if it does not already exist.
        /// </summary>
        /// <param name="path">The path to the file to append to.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <returns>
        /// A <see cref="StreamWriter"/> that appends text to a file using the specified encoding.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>. <para>-or-</para> <paramref name="encoding"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        StreamWriter AppendText(string path, Encoding encoding);

        /// <summary>
        /// Begins an asynchronous file downloading into the stream.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="output">The output.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="output" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to perform the operation was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="output" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        IAsyncResult BeginDownloadFile(string path, Stream output);

        /// <summary>
        /// Begins an asynchronous file downloading into the stream.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="output">The output.</param>
        /// <param name="asyncCallback">The method to be called when the asynchronous write operation is completed.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="output" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to perform the operation was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="output" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        IAsyncResult BeginDownloadFile(string path, Stream output, AsyncCallback asyncCallback);

        /// <summary>
        /// Begins an asynchronous file downloading into the stream.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="output">The output.</param>
        /// <param name="asyncCallback">The method to be called when the asynchronous write operation is completed.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <param name="downloadCallback">The download callback.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="output" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="output" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        IAsyncResult BeginDownloadFile(string path, Stream output, AsyncCallback asyncCallback, object state, Action<ulong> downloadCallback = null);

        /// <summary>
        /// Begins an asynchronous operation of retrieving list of files in remote directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="asyncCallback">The method to be called when the asynchronous write operation is completed.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <param name="listCallback">The list callback.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        IAsyncResult BeginListDirectory(string path, AsyncCallback asyncCallback, object state, Action<int> listCallback = null);

        /// <summary>
        /// Begins the synchronize directories.
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destinationPath">The destination path.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <param name="asyncCallback">The async callback.</param>
        /// <param name="state">The state.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that represents the asynchronous directory synchronization.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="sourcePath"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="destinationPath"/> is <c>null</c> or contains only whitespace.</exception>
        IAsyncResult BeginSynchronizeDirectories(string sourcePath, string destinationPath, string searchPattern, AsyncCallback asyncCallback, object state);

        /// <summary>
        /// Begins an asynchronous uploading the stream into remote file.
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to list the contents of the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions thrown by the stream.
        /// </para>
        /// <para>
        /// If the remote file already exists, it is overwritten and truncated.
        /// </para>
        /// </remarks>
        IAsyncResult BeginUploadFile(Stream input, string path);

        /// <summary>
        /// Begins an asynchronous uploading the stream into remote file.
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <param name="asyncCallback">The method to be called when the asynchronous write operation is completed.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to list the contents of the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions thrown by the stream.
        /// </para>
        /// <para>
        /// If the remote file already exists, it is overwritten and truncated.
        /// </para>
        /// </remarks>
        IAsyncResult BeginUploadFile(Stream input, string path, AsyncCallback asyncCallback);

        /// <summary>
        /// Begins an asynchronous uploading the stream into remote file.
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <param name="asyncCallback">The method to be called when the asynchronous write operation is completed.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <param name="uploadCallback">The upload callback.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to list the contents of the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions thrown by the stream.
        /// </para>
        /// <para>
        /// If the remote file already exists, it is overwritten and truncated.
        /// </para>
        /// </remarks>
        IAsyncResult BeginUploadFile(Stream input, string path, AsyncCallback asyncCallback, object state, Action<ulong> uploadCallback = null);

        /// <summary>
        /// Begins an asynchronous uploading the stream into remote file.
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <param name="canOverride">Specified whether an existing file can be overwritten.</param>
        /// <param name="asyncCallback">The method to be called when the asynchronous write operation is completed.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <param name="uploadCallback">The upload callback.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions thrown by the stream.
        /// </para>
        /// <para>
        /// When <paramref name="path"/> refers to an existing file, set <paramref name="canOverride"/> to <c>true</c> to overwrite and truncate that file.
        /// If <paramref name="canOverride"/> is <c>false</c>, the upload will fail and <see cref="EndUploadFile(IAsyncResult)"/> will throw an
        /// <see cref="SshException"/>.
        /// </para>
        /// </remarks>
        IAsyncResult BeginUploadFile(Stream input, string path, bool canOverride, AsyncCallback asyncCallback, object state, Action<ulong> uploadCallback = null);

        /// <summary>
        /// Changes remote directory to path.
        /// </summary>
        /// <param name="path">New directory path.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to change directory denied by remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="path"/> was not found on the remote host.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        void ChangeDirectory(string path);

        /// <summary>
        /// Changes permissions of file(s) to specified mode.
        /// </summary>
        /// <param name="path">File(s) path, may match multiple files.</param>
        /// <param name="mode">The mode.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to change permission on the path(s) was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="path"/> was not found on the remote host.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        void ChangePermissions(string path, short mode);

        /// <summary>
        /// Creates or overwrites a file in the specified path.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <returns>
        /// A <see cref="SftpFileStream"/> that provides read/write access to the file specified in path.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// If the target file already exists, it is first truncated to zero bytes.
        /// </remarks>
        SftpFileStream Create(string path);

        /// <summary>
        /// Creates or overwrites the specified file.
        /// </summary>
        /// <param name="path">The path and name of the file to create.</param>
        /// <param name="bufferSize">The maximum number of bytes buffered for reads and writes to the file.</param>
        /// <returns>
        /// A <see cref="SftpFileStream"/> that provides read/write access to the file specified in path.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// If the target file already exists, it is first truncated to zero bytes.
        /// </remarks>
        SftpFileStream Create(string path, int bufferSize);

        /// <summary>
        /// Creates remote directory specified by path.
        /// </summary>
        /// <param name="path">Directory path to create.</param>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to create the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        void CreateDirectory(string path);

        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary>
        /// <param name="path">The file to be opened for writing.</param>
        /// <returns>
        /// A <see cref="StreamWriter"/> that writes text to a file using UTF-8 encoding without
        /// a Byte-Order Mark (BOM).
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// If the target file already exists, it is overwritten. It is not first truncated to zero bytes.
        /// </para>
        /// <para>
        /// If the target file does not exist, it is created.
        /// </para>
        /// </remarks>
        StreamWriter CreateText(string path);

        /// <summary>
        /// Creates or opens a file for writing text using the specified encoding.
        /// </summary>
        /// <param name="path">The file to be opened for writing.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <returns>
        /// A <see cref="StreamWriter"/> that writes to a file using the specified encoding.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// If the target file already exists, it is overwritten. It is not first truncated to zero bytes.
        /// </para>
        /// <para>
        /// If the target file does not exist, it is created.
        /// </para>
        /// </remarks>
        StreamWriter CreateText(string path, Encoding encoding);

        /// <summary>
        /// Deletes the specified file or directory.
        /// </summary>
        /// <param name="path">The name of the file or directory to be deleted. Wildcard characters are not supported.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="path"/> was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        void Delete(string path);

        /// <summary>
        /// Deletes remote directory specified by path.
        /// </summary>
        /// <param name="path">Directory to be deleted path.</param>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="path"/> was not found on the remote host.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to delete the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        void DeleteDirectory(string path);

        /// <summary>
        /// Deletes remote file specified by path.
        /// </summary>
        /// <param name="path">File to be deleted path.</param>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="path"/> was not found on the remote host.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to delete the file was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        void DeleteFile(string path);

        /// <summary>
        /// Downloads remote file specified by the path into the stream.
        /// </summary>
        /// <param name="path">File to download.</param>
        /// <param name="output">Stream to write the file into.</param>
        /// <param name="downloadCallback">The download callback.</param>
        /// <exception cref="ArgumentNullException"><paramref name="output" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to perform the operation was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="path"/> was not found on the remote host.</exception>/// 
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="output" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        void DownloadFile(string path, Stream output, Action<ulong> downloadCallback = null);

        /// <summary>
        /// Ends an asynchronous file downloading into the stream.
        /// </summary>
        /// <param name="asyncResult">The pending asynchronous SFTP request.</param>
        /// <exception cref="ArgumentException">The <see cref="IAsyncResult"/> object did not come from the corresponding async method on this type.<para>-or-</para><see cref="EndDownloadFile(IAsyncResult)"/> was called multiple times with the same <see cref="IAsyncResult"/>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to perform the operation was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SftpPathNotFoundException">The path was not found on the remote host.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        void EndDownloadFile(IAsyncResult asyncResult);

        /// <summary>
        /// Ends an asynchronous operation of retrieving list of files in remote directory.
        /// </summary>
        /// <param name="asyncResult">The pending asynchronous SFTP request.</param>
        /// <returns>
        /// A list of files.
        /// </returns>
        /// <exception cref="ArgumentException">The <see cref="IAsyncResult"/> object did not come from the corresponding async method on this type.<para>-or-</para><see cref="EndListDirectory(IAsyncResult)"/> was called multiple times with the same <see cref="IAsyncResult"/>.</exception>
        IEnumerable<SftpFile> EndListDirectory(IAsyncResult asyncResult);

        /// <summary>
        /// Ends the synchronize directories.
        /// </summary>
        /// <param name="asyncResult">The async result.</param>
        /// <returns>
        /// A list of uploaded files.
        /// </returns>
        /// <exception cref="ArgumentException">The <see cref="IAsyncResult"/> object did not come from the corresponding async method on this type.<para>-or-</para><see cref="EndSynchronizeDirectories(IAsyncResult)"/> was called multiple times with the same <see cref="IAsyncResult"/>.</exception>
        /// <exception cref="SftpPathNotFoundException">The destination path was not found on the remote host.</exception>
        IEnumerable<FileInfo> EndSynchronizeDirectories(IAsyncResult asyncResult);

        /// <summary>
        /// Ends an asynchronous uploading the stream into remote file.
        /// </summary>
        /// <param name="asyncResult">The pending asynchronous SFTP request.</param>
        /// <exception cref="ArgumentException">The <see cref="IAsyncResult"/> object did not come from the corresponding async method on this type.<para>-or-</para><see cref="EndUploadFile(IAsyncResult)"/> was called multiple times with the same <see cref="IAsyncResult"/>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The directory of the file was not found on the remote host.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to upload the file was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        void EndUploadFile(IAsyncResult asyncResult);

        /// <summary>
        /// Checks whether file or directory exists;
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// <c>true</c> if directory or file exists; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to perform the operation was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        bool Exists(string path);

        /// <summary>
        /// Gets reference to remote file or directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// A reference to <see cref="SftpFile"/> file object.
        /// </returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="path"/> was not found on the remote host.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        SftpFile Get(string path);

        /// <summary>
        /// Gets the <see cref="SftpFileAttributes"/> of the file on the path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>
        /// The <see cref="SftpFileAttributes"/> of the file on the path.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="path"/> was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        SftpFileAttributes GetAttributes(string path);

        /// <summary>
        /// Returns the date and time the specified file or directory was last accessed.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain access date and time information.</param>
        /// <returns>
        /// A <see cref="DateTime"/> structure set to the date and time that the specified file or directory was last accessed.
        /// This value is expressed in local time.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        DateTime GetLastAccessTime(string path);

        /// <summary>
        /// Returns the date and time, in coordinated universal time (UTC), that the specified file or directory was last accessed.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain access date and time information.</param>
        /// <returns>
        /// A <see cref="DateTime"/> structure set to the date and time that the specified file or directory was last accessed.
        /// This value is expressed in UTC time.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        DateTime GetLastAccessTimeUtc(string path);

        /// <summary>
        /// Returns the date and time the specified file or directory was last written to.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain write date and time information.</param>
        /// <returns>
        /// A <see cref="DateTime"/> structure set to the date and time that the specified file or directory was last written to.
        /// This value is expressed in local time.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        DateTime GetLastWriteTime(string path);

        /// <summary>
        /// Returns the date and time, in coordinated universal time (UTC), that the specified file or directory was last written to.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain write date and time information.</param>
        /// <returns>
        /// A <see cref="DateTime"/> structure set to the date and time that the specified file or directory was last written to.
        /// This value is expressed in UTC time.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        DateTime GetLastWriteTimeUtc(string path);

        /// <summary>
        /// Gets status using statvfs@openssh.com request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// A <see cref="SftpFileSytemInformation"/> instance that contains file status information.
        /// </returns>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <b>null</b>.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        SftpFileSytemInformation GetStatus(string path);

        /// <summary>
        /// Retrieves list of files in remote directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="listCallback">The list callback.</param>
        /// <returns>
        /// A list of files.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path" /> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to list the contents of the directory was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        IEnumerable<SftpFile> ListDirectory(string path, Action<int> listCallback = null);

        /// <summary>
        /// Opens a <see cref="SftpFileStream"/> on the specified path with read/write access.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
        /// <returns>
        /// An unshared <see cref="SftpFileStream"/> that provides access to the specified file, with the specified mode and read/write access.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        SftpFileStream Open(string path, FileMode mode);

        /// <summary>
        /// Opens a <see cref="SftpFileStream"/> on the specified path, with the specified mode and access.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode">A <see cref="FileMode"/> value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
        /// <param name="access">A <see cref="FileAccess"/> value that specifies the operations that can be performed on the file.</param>
        /// <returns>
        /// An unshared <see cref="SftpFileStream"/> that provides access to the specified file, with the specified mode and access.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        SftpFileStream Open(string path, FileMode mode, FileAccess access);

        /// <summary>
        /// Opens an existing file for reading.
        /// </summary>
        /// <param name="path">The file to be opened for reading.</param>
        /// <returns>
        /// A read-only <see cref="SftpFileStream"/> on the specified path.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        SftpFileStream OpenRead(string path);

        /// <summary>
        /// Opens an existing UTF-8 encoded text file for reading.
        /// </summary>
        /// <param name="path">The file to be opened for reading.</param>
        /// <returns>
        /// A <see cref="StreamReader"/> on the specified path.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        StreamReader OpenText(string path);

        /// <summary>
        /// Opens a file for writing.
        /// </summary>
        /// <param name="path">The file to be opened for writing.</param>
        /// <returns>
        /// An unshared <see cref="SftpFileStream"/> object on the specified path with <see cref="FileAccess.Write"/> access.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// If the file does not exist, it is created.
        /// </remarks>
        SftpFileStream OpenWrite(string path);

        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>
        /// A byte array containing the contents of the file.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        byte[] ReadAllBytes(string path);

        /// <summary>
        /// Opens a text file, reads all lines of the file using UTF-8 encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>
        /// A string array containing all lines of the file.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        string[] ReadAllLines(string path);

        /// <summary>
        /// Opens a file, reads all lines of the file with the specified encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <returns>
        /// A string array containing all lines of the file.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        string[] ReadAllLines(string path, Encoding encoding);

        /// <summary>
        /// Opens a text file, reads all lines of the file with the UTF-8 encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>
        /// A string containing all lines of the file.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        string ReadAllText(string path);

        /// <summary>
        /// Opens a file, reads all lines of the file with the specified encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="encoding">The encoding applied to the contents of the file.</param>
        /// <returns>
        /// A string containing all lines of the file.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        string ReadAllText(string path, Encoding encoding);

        /// <summary>
        /// Reads the lines of a file with the UTF-8 encoding.
        /// </summary>
        /// <param name="path">The file to read.</param>
        /// <returns>
        /// The lines of the file.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        IEnumerable<string> ReadLines(string path);

        /// <summary>
        /// Read the lines of a file that has a specified encoding.
        /// </summary>
        /// <param name="path">The file to read.</param>
        /// <param name="encoding">The encoding that is applied to the contents of the file.</param>
        /// <returns>
        /// The lines of the file.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        IEnumerable<string> ReadLines(string path, Encoding encoding);

        /// <summary>
        /// Renames remote file from old path to new path.
        /// </summary>
        /// <param name="oldPath">Path to the old file location.</param>
        /// <param name="newPath">Path to the new file location.</param>
        /// <exception cref="ArgumentNullException"><paramref name="oldPath"/> is <b>null</b>. <para>-or-</para> or <paramref name="newPath"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to rename the file was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        void RenameFile(string oldPath, string newPath);

        /// <summary>
        /// Renames remote file from old path to new path.
        /// </summary>
        /// <param name="oldPath">Path to the old file location.</param>
        /// <param name="newPath">Path to the new file location.</param>
        /// <param name="isPosix">if set to <c>true</c> then perform a posix rename.</param>
        /// <exception cref="ArgumentNullException"><paramref name="oldPath" /> is <b>null</b>. <para>-or-</para> or <paramref name="newPath" /> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to rename the file was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        void RenameFile(string oldPath, string newPath, bool isPosix);

        /// <summary>
        /// Sets the specified <see cref="SftpFileAttributes"/> of the file on the specified path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <param name="fileAttributes">The desired <see cref="SftpFileAttributes"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        void SetAttributes(string path, SftpFileAttributes fileAttributes);

        /// <summary>
        /// Creates a symbolic link from old path to new path.
        /// </summary>
        /// <param name="path">The old path.</param>
        /// <param name="linkPath">The new path.</param>
        /// <exception cref="ArgumentException"><paramref name="path"/> is <b>null</b>. <para>-or-</para> <paramref name="linkPath"/> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to create the symbolic link was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message"/> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        void SymbolicLink(string path, string linkPath);

        /// <summary>
        /// Synchronizes the directories.
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destinationPath">The destination path.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <returns>
        /// A list of uploaded files.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="sourcePath"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="destinationPath"/> is <c>null</c> or contains only whitespace.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="destinationPath"/> was not found on the remote host.</exception>
        IEnumerable<FileInfo> SynchronizeDirectories(string sourcePath, string destinationPath, string searchPattern);

        /// <summary>
        /// Uploads stream into remote file.
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <param name="uploadCallback">The upload callback.</param>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to upload the file was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        void UploadFile(Stream input, string path, Action<ulong> uploadCallback = null);

        /// <summary>
        /// Uploads stream into remote file.
        /// </summary>
        /// <param name="input">Data input stream.</param>
        /// <param name="path">Remote file path.</param>
        /// <param name="canOverride">if set to <c>true</c> then existing file will be overwritten.</param>
        /// <param name="uploadCallback">The upload callback.</param>
        /// <exception cref="ArgumentNullException"><paramref name="input" /> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="path" /> is <b>null</b> or contains only whitespace characters.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPermissionDeniedException">Permission to upload the file was denied by the remote host. <para>-or-</para> A SSH command was denied by the server.</exception>
        /// <exception cref="SshException">A SSH error where <see cref="Exception.Message" /> is the message from the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// Method calls made by this method to <paramref name="input" />, may under certain conditions result in exceptions thrown by the stream.
        /// </remarks>
        void UploadFile(Stream input, string path, bool canOverride, Action<ulong> uploadCallback = null);

        /// <summary>
        /// Writes the specified byte array to the specified file, and closes the file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="bytes">The bytes to write to the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// If the target file already exists, it is overwritten. It is not first truncated to zero bytes.
        /// </para>
        /// <para>
        /// If the target file does not exist, it is created.
        /// </para>
        /// </remarks>
        void WriteAllBytes(string path, byte[] bytes);

        /// <summary>
        /// Writes a collection of strings to the file using the UTF-8 encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The lines to write to the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// The characters are written to the file using UTF-8 encoding without a Byte-Order Mark (BOM).
        /// </para>
        /// <para>
        /// If the target file already exists, it is overwritten. It is not first truncated to zero bytes.
        /// </para>
        /// <para>
        /// If the target file does not exist, it is created.
        /// </para>
        /// </remarks>
        void WriteAllLines(string path, IEnumerable<string> contents);

        /// <summary>
        /// Writes a collection of strings to the file using the specified encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The lines to write to the file.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// If the target file already exists, it is overwritten. It is not first truncated to zero bytes.
        /// </para>
        /// <para>
        /// If the target file does not exist, it is created.
        /// </para>
        /// </remarks>
        void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding);

        /// <summary>
        /// Write the specified string array to the file using the UTF-8 encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string array to write to the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// The characters are written to the file using UTF-8 encoding without a Byte-Order Mark (BOM).
        /// </para>
        /// <para>
        /// If the target file already exists, it is overwritten. It is not first truncated to zero bytes.
        /// </para>
        /// <para>
        /// If the target file does not exist, it is created.
        /// </para>
        /// </remarks>
        void WriteAllLines(string path, string[] contents);

        /// <summary>
        /// Writes the specified string array to the file by using the specified encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string array to write to the file.</param>
        /// <param name="encoding">An <see cref="Encoding"/> object that represents the character encoding applied to the string array.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// If the target file already exists, it is overwritten. It is not first truncated to zero bytes.
        /// </para>
        /// <para>
        /// If the target file does not exist, it is created.
        /// </para>
        /// </remarks>
        void WriteAllLines(string path, string[] contents, Encoding encoding);

        /// <summary>
        /// Writes the specified string to the file using the UTF-8 encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// The characters are written to the file using UTF-8 encoding without a Byte-Order Mark (BOM).
        /// </para>
        /// <para>
        /// If the target file already exists, it is overwritten. It is not first truncated to zero bytes.
        /// </para>
        /// <para>
        /// If the target file does not exist, it is created.
        /// </para>
        /// </remarks>
        void WriteAllText(string path, string contents);

        /// <summary>
        /// Writes the specified string to the file using the specified encoding, and closes the file.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        /// <param name="encoding">The encoding to apply to the string.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <b>null</b>.</exception>
        /// <exception cref="SshConnectionException">Client is not connected.</exception>
        /// <exception cref="SftpPathNotFoundException">The specified path is invalid, or its directory was not found on the remote host.</exception>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        /// <remarks>
        /// <para>
        /// If the target file already exists, it is overwritten. It is not first truncated to zero bytes.
        /// </para>
        /// <para>
        /// If the target file does not exist, it is created.
        /// </para>
        /// </remarks>
        void WriteAllText(string path, string contents, Encoding encoding);
    }
}