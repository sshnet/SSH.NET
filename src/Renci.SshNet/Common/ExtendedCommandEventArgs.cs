using System;
#nullable enable
using System.Text;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Class for extended text output related events.
    /// </summary>
    public class ExtendedCommandEventArgs : CommandOutputEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedCommandEventArgs"/> class.
        /// </summary>
        /// <param name="rawData">The raw data received.</param>
        /// <param name="encoding">The encoding used for the transmission.</param>
        /// <param name="dataTypeCode">The data type code.</param>
        public ExtendedCommandEventArgs(ArraySegment<byte> rawData, Encoding encoding, uint dataTypeCode)
            : base(rawData, encoding)
        {
            DataTypeCode = dataTypeCode;
        }

        /// <summary>
        /// Gets the data type code.
        /// </summary>
        public uint DataTypeCode { get; }

        /// <summary>
        /// Gets a value indicating whether the current data represents an stderr output.
        /// </summary>
        public bool IsError
        {
            get
            {
                return DataTypeCode == 1;
            }
        }
    }
}
