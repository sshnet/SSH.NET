using System;
using System.Threading;
using Renci.SshNet.Sftp.Responses;

namespace Renci.SshNet.Sftp
{
    internal interface ISftpSession : ISubsystemSession
    {
        /// <summary>
        /// Performs SSH_FXP_FSTAT request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns>
        /// File attributes
        /// </returns>
        SftpFileAttributes RequestFStat(byte[] handle);

        /// <summary>
        /// Performs SSH_FXP_OPEN request
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="nullOnError">if set to <c>true</c> returns <c>null</c> instead of throwing an exception.</param>
        /// <returns>File handle.</returns>
        byte[] RequestOpen(string path, Flags flags, bool nullOnError = false);

        /// <summary>
        /// Performs SSH_FXP_READ request.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns>data array; null if EOF</returns>
        byte[] RequestRead(byte[] handle, ulong offset, uint length);

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
