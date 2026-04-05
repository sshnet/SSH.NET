namespace Renci.SshNet
{
    /// <summary>
    /// Provides the progress for a file download.
    /// </summary>
    public struct DownloadFileProgressReport
    {
        /// <summary>
        /// Gets the total number of bytes downloaded.
        /// </summary>
        public ulong TotalBytesDownloaded { get; internal set; }
    }
}
