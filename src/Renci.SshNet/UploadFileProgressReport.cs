namespace Renci.SshNet
{
    /// <summary>
    /// Provides the progress for a file upload.
    /// </summary>
    public struct UploadFileProgressReport
    {
        /// <summary>
        /// Gets the total number of bytes uploaded.
        /// </summary>
        public ulong TotalBytesUploaded { get; internal set; }
    }
}
