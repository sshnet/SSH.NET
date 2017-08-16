using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Specialized <see cref="MemoryStream"/> for reading and writing data SSH data.
    /// </summary>
    public class SshDataStream : MemoryStream
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SshDataStream"/> class with an expandable capacity initialized
        /// as specified.
        /// </summary>
        /// <param name="capacity">The initial size of the internal array in bytes.</param>
        public SshDataStream(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new non-resizable instance of the <see cref="SshDataStream"/> class based on the specified byte array.
        /// </summary>
        /// <param name="buffer">The array of unsigned bytes from which to create the current stream.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
        public SshDataStream(byte[] buffer)
            : base(buffer)
        {
        }

        /// <summary>
        /// Initializes a new non-resizable instance of the <see cref="SshDataStream"/> class based on the specified byte array.
        /// </summary>
        /// <param name="buffer">The array of unsigned bytes from which to create the current stream.</param>
        /// <param name="offset">The zero-based offset in <paramref name="buffer"/> at which to begin reading SSH data.</param>
        /// <param name="count">The number of bytes to load.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
        public SshDataStream(byte[] buffer, int offset, int count)
            : base(buffer, offset, count)
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
        /// Writes an <see cref="uint"/> to the SSH data stream.
        /// </summary>
        /// <param name="value"><see cref="uint"/> data to write.</param>
        public void Write(uint value)
        {
            var bytes = Pack.UInt32ToBigEndian(value);
            Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Writes an <see cref="ulong"/> to the SSH data stream.
        /// </summary>
        /// <param name="value"><see cref="ulong"/> data to write.</param>
        public void Write(ulong value)
        {
            var bytes = Pack.UInt64ToBigEndian(value);
            Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Writes a <see cref="BigInteger"/> into the SSH data stream.
        /// </summary>
        /// <param name="data">The <see cref="BigInteger" /> to write.</param>
        public void Write(BigInteger data)
        {
            var bytes = data.ToByteArray().Reverse();
            WriteBinary(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Writes bytes array data into the SSH data stream.
        /// </summary>
        /// <param name="data">Byte array data to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <c>null</c>.</exception>
        public void Write(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            Write(data, 0, data.Length);
        }

        /// <summary>
        /// Reads a byte array from the SSH data stream.
        /// </summary>
        /// <returns>
        /// The byte array read from the SSH data stream.
        /// </returns>
        public byte[] ReadBinary()
        {
            var length = ReadUInt32();

            if (length > int.MaxValue)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Data longer than {0} is not supported.", int.MaxValue));
            }

            return ReadBytes((int)length);
        }

        /// <summary>
        /// Writes a buffer preceded by its length into the SSH data stream.
        /// </summary>
        /// <param name="buffer">The data to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
        public void WriteBinary(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            WriteBinary(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes a buffer preceded by its length into the SSH data stream.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method write <paramref name="count"/> bytes from buffer to the current SSH data stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin writing bytes to the SSH data stream.</param>
        /// <param name="count">The number of bytes to be written to the current SSH data stream.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
        public void WriteBinary(byte[] buffer, int offset, int count)
        {
            Write((uint) count);
            Write(buffer, offset, count);
        }

        /// <summary>
        /// Writes string data to the SSH data stream using the specified encoding.
        /// </summary>
        /// <param name="s">The string data to write.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <c>null</c>.</exception>
        public void Write(string s, Encoding encoding)
        {
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            var bytes = encoding.GetBytes(s);
            WriteBinary(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Reads a <see cref="BigInteger"/> from the SSH datastream.
        /// </summary>
        /// <returns>
        /// The <see cref="BigInteger"/> read from the SSH data stream.
        /// </returns>
        public BigInteger ReadBigInt()
        {
            var length = ReadUInt32();
            var data = ReadBytes((int) length);
            return new BigInteger(data.Reverse());
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
            return Pack.BigEndianToUInt32(data);
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
            return Pack.BigEndianToUInt64(data);
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

            var bytes = ReadBytes((int) length);
            return encoding.GetString(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Reads next specified number of bytes data type from internal buffer.
        /// </summary>
        /// <param name="length">Number of bytes to read.</param>
        /// <returns>
        /// An array of bytes that was read from the internal buffer.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is greater than the internal buffer size.</exception>
        private byte[] ReadBytes(int length)
        {
            var data = new byte[length];
            var bytesRead = Read(data, 0, length);

            if (bytesRead < length)
                throw new ArgumentOutOfRangeException("length",
                    string.Format(CultureInfo.InvariantCulture,
                        "The requested length ({0}) is greater than the actual number of bytes read ({1}).", length, bytesRead));

            return data;
        }

        /// <summary>
        /// Writes the stream contents to a byte array, regardless of the <see cref="MemoryStream.Position"/>.
        /// </summary>
        /// <returns>
        /// This method returns the contents of the <see cref="SshDataStream"/> as a byte array.
        /// </returns>
        /// <remarks>
        /// If the current instance was constructed on a provided byte array, a copy of the section of the array
        /// to which this instance has access is returned.
        /// </remarks>
        public override byte[] ToArray()
        {
            if (Capacity == Length)
            {
#if FEATURE_MEMORYSTREAM_GETBUFFER
                return GetBuffer();
#elif FEATURE_MEMORYSTREAM_TRYGETBUFFER
                ArraySegment<byte> buffer;
                if (TryGetBuffer(out buffer))
                    return buffer.Array;
#endif
            }
            return base.ToArray();
        }
    }
}
