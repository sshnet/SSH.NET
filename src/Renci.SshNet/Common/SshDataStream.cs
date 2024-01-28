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
        /// Initializes a new instance of the <see cref="SshDataStream"/> class for the specified byte array.
        /// </summary>
        /// <param name="buffer">The array of unsigned bytes from which to create the current stream.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        public SshDataStream(byte[] buffer)
            : base(buffer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshDataStream"/> class for the specified byte array.
        /// </summary>
        /// <param name="buffer">The array of unsigned bytes from which to create the current stream.</param>
        /// <param name="offset">The zero-based offset in <paramref name="buffer"/> at which to begin reading SSH data.</param>
        /// <param name="count">The number of bytes to load.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        public SshDataStream(byte[] buffer, int offset, int count)
            : base(buffer, offset, count)
        {
        }

        /// <summary>
        /// Gets a value indicating whether all data from the SSH data stream has been read.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance is end of data; otherwise, <see langword="false"/>.
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
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            Span<byte> bytes = stackalloc byte[4];
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(bytes, value);
            Write(bytes);
#else
            var bytes = Pack.UInt32ToBigEndian(value);
            Write(bytes, 0, bytes.Length);
#endif
        }

        /// <summary>
        /// Writes an <see cref="ulong"/> to the SSH data stream.
        /// </summary>
        /// <param name="value"><see cref="ulong"/> data to write.</param>
        public void Write(ulong value)
        {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            Span<byte> bytes = stackalloc byte[8];
            System.Buffers.Binary.BinaryPrimitives.WriteUInt64BigEndian(bytes, value);
            Write(bytes);
#else
            var bytes = Pack.UInt64ToBigEndian(value);
            Write(bytes, 0, bytes.Length);
#endif
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
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is <see langword="null"/>.</exception>
        public void Write(byte[] data)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Write(data, 0, data.Length);
        }

        /// <summary>
        /// Writes string data to the SSH data stream using the specified encoding.
        /// </summary>
        /// <param name="s">The string data to write.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="s"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <see langword="null"/>.</exception>
        public void Write(string s, Encoding encoding)
        {
            if (encoding is null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            ReadOnlySpan<char> value = s;
            var count = encoding.GetByteCount(value);
            var bytes = count <= 256 ? stackalloc byte[count] : new byte[count];
            encoding.GetBytes(value, bytes);
            Write((uint) count);
            Write(bytes);
#else
            var bytes = encoding.GetBytes(s);
            WriteBinary(bytes, 0, bytes.Length);
#endif
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
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        public void WriteBinary(byte[] buffer)
        {
            if (buffer is null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            WriteBinary(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes a buffer preceded by its length into the SSH data stream.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method write <paramref name="count"/> bytes from buffer to the current SSH data stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin writing bytes to the SSH data stream.</param>
        /// <param name="count">The number of bytes to be written to the current SSH data stream.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
        public void WriteBinary(byte[] buffer, int offset, int count)
        {
            Write((uint) count);
            Write(buffer, offset, count);
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
        /// Reads the next <see cref="ushort"/> data type from the SSH data stream.
        /// </summary>
        /// <returns>
        /// The <see cref="ushort"/> read from the SSH data stream.
        /// </returns>
        public ushort ReadUInt16()
        {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            Span<byte> bytes = stackalloc byte[2];
            ReadBytes(bytes);
            return System.Buffers.Binary.BinaryPrimitives.ReadUInt16BigEndian(bytes);
#else
            var data = ReadBytes(2);
            return Pack.BigEndianToUInt16(data);
#endif
        }

        /// <summary>
        /// Reads the next <see cref="uint"/> data type from the SSH data stream.
        /// </summary>
        /// <returns>
        /// The <see cref="uint"/> read from the SSH data stream.
        /// </returns>
        public uint ReadUInt32()
        {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            Span<byte> span = stackalloc byte[4];
            ReadBytes(span);
            return System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(span);
#else
            var data = ReadBytes(4);
            return Pack.BigEndianToUInt32(data);
#endif // NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
        }

        /// <summary>
        /// Reads the next <see cref="ulong"/> data type from the SSH data stream.
        /// </summary>
        /// <returns>
        /// The <see cref="ulong"/> read from the SSH data stream.
        /// </returns>
        public ulong ReadUInt64()
        {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            Span<byte> span = stackalloc byte[8];
            ReadBytes(span);
            return System.Buffers.Binary.BinaryPrimitives.ReadUInt64BigEndian(span);
#else
            var data = ReadBytes(8);
            return Pack.BigEndianToUInt64(data);
#endif // NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
        }

        /// <summary>
        /// Reads the next <see cref="string"/> data type from the SSH data stream.
        /// </summary>
        /// <param name="encoding">The character encoding to use. Defaults to <see cref="Encoding.UTF8"/>.</param>
        /// <returns>
        /// The <see cref="string"/> read from the SSH data stream.
        /// </returns>
        public string ReadString(Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;

            var length = ReadUInt32();

            if (length > int.MaxValue)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Strings longer than {0} is not supported.", int.MaxValue));
            }

            var bytes = ReadBytes((int) length);
            return encoding.GetString(bytes, 0, bytes.Length);
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
                return GetBuffer();
            }

            return base.ToArray();
        }

        /// <summary>
        /// Reads next specified number of bytes data type from internal buffer.
        /// </summary>
        /// <param name="length">Number of bytes to read.</param>
        /// <returns>
        /// An array of bytes that was read from the internal buffer.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is greater than the internal buffer size.</exception>
        internal byte[] ReadBytes(int length)
        {
            var data = new byte[length];
            var bytesRead = Read(data, 0, length);
            if (bytesRead < length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), string.Format(CultureInfo.InvariantCulture, "The requested length ({0}) is greater than the actual number of bytes read ({1}).", length, bytesRead));
            }

            return data;
        }

#if NETSTANDARD2_1 || NET6_0_OR_GREATER
        /// <summary>
        /// Reads data into the specified <paramref name="buffer" />.
        /// </summary>
        /// <param name="buffer">The buffer to read into.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="buffer"/> is larger than the total of bytes available.</exception>
        private void ReadBytes(Span<byte> buffer)
        {
            var bytesRead = Read(buffer);
            if (bytesRead < buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(buffer), string.Format(CultureInfo.InvariantCulture, "The requested length ({0}) is greater than the actual number of bytes read ({1}).", buffer.Length, bytesRead));
            }
        }
#endif // NETSTANDARD2_1 || NET6_0_OR_GREATER
    }
}
