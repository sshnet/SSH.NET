namespace Renci.SshNet.Compression
{
    /// <summary>
    /// Specifies compression modes
    /// </summary>
    public enum CompressionMode
    {
        /// <summary>
        /// Specifies that content should be compressed.
        /// </summary>
        Compress = 0,

        /// <summary>
        /// Specifies that content should be decompressed.
        /// </summary>
        Decompress = 1,
    }
}
