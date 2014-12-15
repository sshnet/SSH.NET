using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Renci.SshNet.Common
{
    public class SshDataStream : MemoryStream
    {
        public SshDataStream(int capacity)
            : base(capacity)
        {
        }

        public SshDataStream(byte[] buffer)
            : base(buffer)
        {
        }

        /// <summary>
        /// Gets a value indicating whether all data from the SSH data stream has been read.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is end of data; otherwise, <c>false</c>.
        /// </value>
        public bool IsEndOfData
        {
            get
            {
                return Position >= Length;
            }
        }

        /// <summary>
        /// Writes <see cref="uint"/> data to the SSH data stream.
        /// </summary>
        /// <param name="value"><see cref="uint"/> data to write.</param>
        public void Write(uint value)
        {
            var bytes = value.GetBytes();
            Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Writes <see cref="ulong"/> data to the SSH data stream.
        /// </summary>
        /// <param name="value"><see cref="ulong"/> data to write.</param>
        public void Write(ulong value)
        {
            var bytes = value.GetBytes();
            Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Writes string data to the SSH data stream using the specified encoding.
        /// </summary>
        /// <param name="value">The string data to write.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is null.</exception>
        public void Write(string value, Encoding encoding)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            var bytes = encoding.GetBytes(value);
            var bytesLength = bytes.Length;
            Write((uint) bytesLength);
            Write(bytes, 0, bytesLength);
        }

        /// <summary>
        /// Reads the next <see cref="uint"/> data type from the SSH data stream.
        /// </summary>
        /// <returns>
        /// The <see cref="uint"/> read from the SSH data stream.
        /// </returns>
        public uint ReadUInt32()
        {
            var data = ReadBytes(4);
            return (uint)(data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3]);
        }

        /// <summary>
        /// Reads the next <see cref="ulong"/> data type from the SSH data stream.
        /// </summary>
        /// <returns>
        /// The <see cref="ulong"/> read from the SSH data stream.
        /// </returns>
        public ulong ReadUInt64()
        {
            var data = ReadBytes(8);
            return ((ulong) data[0] << 56 | (ulong) data[1] << 48 | (ulong) data[2] << 40 | (ulong) data[3] << 32 |
                    (ulong) data[4] << 24 | (ulong) data[5] << 16 | (ulong) data[6] << 8 | data[7]);
        }

        /// <summary>
        /// Reads the next <see cref="string"/> data type from the SSH data stream.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/> read from the SSH data stream.
        /// </returns>
        public string ReadString(Encoding encoding)
        {
            var length = ReadUInt32();

            if (length > int.MaxValue)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Strings longer than {0} is not supported.", int.MaxValue));
            }
            return encoding.GetString(ReadBytes((int) length), 0, (int) length);
        }

        /// <summary>
        /// Reads next specified number of bytes data type from internal buffer.
        /// </summary>
        /// <param name="length">Number of bytes to read.</param>
        /// <returns>An array of bytes that was read from the internal buffer.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is greater than the internal buffer size.</exception>
        private byte[] ReadBytes(int length)
        {
            var data = new byte[length];
            var bytesRead = base.Read(data, 0, length);

            if (bytesRead < length)
                throw new ArgumentOutOfRangeException("length");

            return data;
        }

        public override byte[] ToArray()
        {
            if (Capacity == Length)
            {
                return GetBuffer();
            }
            return base.ToArray();
        }
    }
}
