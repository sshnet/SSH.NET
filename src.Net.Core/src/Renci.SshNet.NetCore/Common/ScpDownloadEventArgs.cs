using System;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides data for the Downloading event.
    /// </summary>
    public class ScpDownloadEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the downloaded filename.
        /// </summary>
        public string Filename { get; private set; }

        /// <summary>
        /// Gets the downloaded file size.
        /// </summary>
        public long Size { get; private set; }

        /// <summary>
        /// Gets number of downloaded bytes so far.
        /// </summary>
        public long Downloaded { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpDownloadEventArgs"/> class.
        /// </summary>
        /// <param name="filename">The downloaded filename.</param>
        /// <param name="size">The downloaded file size.</param>
        /// <param name="downloaded">The number of downloaded bytes so far.</param>
        public ScpDownloadEventArgs(string filename, long size, long downloaded)
        {
            Filename = filename;
            Size = size;
            Downloaded = downloaded;
        }
    }
}
