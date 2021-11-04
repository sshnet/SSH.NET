namespace Renci.SshNet.Sftp
{
    /// <summary>
    /// Specifies how the uploaded file should be created.
    /// </summary>
    public enum UploadMode
    {
        /// <summary>
        /// A new file should be created. If the file already exists, an exception is thrown.
        /// </summary>
        CreateNew = 1,

        /// <summary>
        /// A new file should be created. If the file already exists, it will be overwritten.
        /// </summary>
        Overwrite = 2,

        /// <summary>
        /// Opens the file if it exists and seeks to the end of the file, or creates a new file.
        /// </summary>
        Append = 6
    }
}
