#nullable enable
using System;
using System.Text;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Base class for command output related events.
    /// </summary>
    public class CommandOutputEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandOutputEventArgs"/> class.
        /// </summary>
        /// <param name="rawData">The raw data received.</param>
        /// <param name="encoding">The encoding used for the transmission.</param>
        public CommandOutputEventArgs(ArraySegment<byte> rawData, Encoding encoding)
        {
            RawData = rawData;
            Encoding = encoding;
        }

        /// <summary>
        /// Gets the received data as <see langword="string"/>.
        /// </summary>
        public string Text
        {
            get
            {
                return Encoding.GetString(RawData.Array!, RawData.Offset, RawData.Count);
            }
        }

        /// <summary>
        /// Gets the raw data received from the server. This is the data that was used to create the <see cref="Text"/> property.
        /// </summary>
        public ArraySegment<byte> RawData { get; }

        /// <summary>
        /// Gets the output encoding used.
        /// </summary>
        public Encoding Encoding { get; }
    }
}
