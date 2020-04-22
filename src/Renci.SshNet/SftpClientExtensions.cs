#if !NET35
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Renci.SshNet.Sftp;
using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// The SftpClient extensions to implement the Task-based Asynchronous Pattern (TAP) over the originally implemented Asynchronous Programming Model (APM).
    /// </summary>
    public static class SftpClientExtensions
    {
        /// <summary>
        /// Asynchronously downloads a remote file specified by the path into the stream.
        /// </summary>
        /// <param name="sftpClient"></param>
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
        public static Task DownloadFileAsync(this SftpClient sftpClient, string path, Stream output, Action<ulong> downloadCallback = null)
        {
            if (sftpClient == null)
                throw new ArgumentNullException("sftpClient");

            var tcs = new TaskCompletionSource<bool>();
            sftpClient.BeginDownloadFile(path, output, iar =>
            {
                try
                {
                    sftpClient.EndDownloadFile(iar);
                    tcs.TrySetResult(true);
                }
                catch (OperationCanceledException)
                {
                    tcs.TrySetCanceled();
                }
                catch (Exception exc)
                {
                    tcs.TrySetException(exc);
                }
            }, null, downloadCallback);
            return tcs.Task;
        }

        /// <summary>
        /// Asynchronously retrieves list of files in remote directory.
        /// </summary>
        /// <param name="sftpClient"></param>
        /// <param name="path">The path.</param>
        /// <param name="listCallback">The list callback.</param>
        /// <returns>
        /// A list of files.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The method was called after the client was disposed.</exception>
        public static Task<IEnumerable<SftpFile>> ListDirectoryAsync(this SftpClient sftpClient, string path, Action<int> listCallback = null)
        {
            if (sftpClient == null)
                throw new ArgumentNullException("sftpClient");

            var tcs = new TaskCompletionSource<IEnumerable<SftpFile>>();
            sftpClient.BeginListDirectory(path, iar =>
            {
                try
                {
                    tcs.TrySetResult(sftpClient.EndListDirectory(iar));
                }
                catch (OperationCanceledException)
                {
                    tcs.TrySetCanceled();
                }
                catch (Exception exc)
                {
                    tcs.TrySetException(exc);
                }
            }, null, listCallback);
            return tcs.Task;
        }


        /// <summary>
        /// Asynchronously synchronizes the directories.
        /// </summary>
        /// <param name="sftpClient"></param>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="destinationPath">The destination path.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <returns>
        /// A list of uploaded files.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="sourcePath"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="destinationPath"/> is <c>null</c> or contains only whitespace.</exception>
        /// <exception cref="SftpPathNotFoundException"><paramref name="destinationPath"/> was not found on the remote host.</exception>
        public static Task<IEnumerable<FileInfo>> SynchronizeDirectoriesAsync(this SftpClient sftpClient
            , string sourcePath, string destinationPath, string searchPattern)
        {
            if (sftpClient == null)
                throw new ArgumentNullException("sftpClient");

            return Task<IEnumerable<FileInfo>>.Factory.FromAsync(sftpClient.BeginSynchronizeDirectories,
                sftpClient.EndSynchronizeDirectories, sourcePath, destinationPath, searchPattern, null);
        }

        /// <summary>
        /// Asynchronously uploads stream into remote file. If the file exists it will be overwritten.
        /// </summary>
        /// <param name="sftpClient"></param>
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
        public static Task UploadFileAsync(this SftpClient sftpClient, Stream input, string path, Action<ulong> uploadCallback = null)
        {
            return UploadFileAsync(sftpClient, input, path, true, uploadCallback);
        }

        /// <summary>
        /// Asynchronously uploads stream into remote file.
        /// </summary>
        /// <param name="sftpClient"></param>
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
        public static Task UploadFileAsync(this SftpClient sftpClient, Stream input, string path, bool canOverride, Action<ulong> uploadCallback = null)
        {
            if (sftpClient == null)
                throw new ArgumentNullException("sftpClient");

            var tcs = new TaskCompletionSource<bool>();
            sftpClient.BeginUploadFile(input, path, canOverride, iar =>
            {
                try
                {
                    sftpClient.EndUploadFile(iar);
                    tcs.TrySetResult(true);
                }
                catch (OperationCanceledException)
                {
                    tcs.TrySetCanceled();
                }
                catch (Exception exc)
                {
                    tcs.TrySetException(exc);
                }
            }, null, uploadCallback);
            return tcs.Task;
        }

    }
}
#endif