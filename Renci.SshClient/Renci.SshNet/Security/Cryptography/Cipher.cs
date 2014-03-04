using System;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Base class for cipher implementation.
    /// </summary>
    public abstract class Cipher
    {
        /// <summary>
        /// Gets the minimum data size.
        /// </summary>
        /// <value>
        /// The minimum data size.
        /// </value>
        public abstract byte MinimumSize { get; }

        /// <summary>
        /// Encrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Encrypted data.</returns>
        public abstract byte[] Encrypt(byte[] input);

        /// <summary>
        /// Decrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Decrypted data.</returns>
        public abstract byte[] Decrypt(byte[] input);

        #region Packing functions

        /// <summary>
        /// Populates buffer with big endian number representation.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        protected static void UInt32ToBigEndian(uint number, byte[] buffer)
        {
            buffer[0] = (byte)(number >> 24);
            buffer[1] = (byte)(number >> 16);
            buffer[2] = (byte)(number >> 8);
            buffer[3] = (byte)(number);
        }

        /// <summary>
        /// Populates buffer with big endian number representation.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        protected static void UInt32ToBigEndian(uint number, byte[] buffer, int offset)
        {
            buffer[offset] = (byte)(number >> 24);
            buffer[offset + 1] = (byte)(number >> 16);
            buffer[offset + 2] = (byte)(number >> 8);
            buffer[offset + 3] = (byte)(number);
        }

        /// <summary>
        /// Converts big endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>Converted <see cref="Int32" />.</returns>
        protected static uint BigEndianToUInt32(byte[] buffer)
        {
            uint n = (uint)buffer[0] << 24;
            n |= (uint)buffer[1] << 16;
            n |= (uint)buffer[2] << 8;
            n |= (uint)buffer[3];
            return n;
        }

        /// <summary>
        /// Converts big endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        /// <returns>Converted <see cref="UInt32" />.</returns>
        protected static uint BigEndianToUInt32(byte[] buffer, int offset)
        {
            uint n = (uint)buffer[offset] << 24;
            n |= (uint)buffer[offset + 1] << 16;
            n |= (uint)buffer[offset + 2] << 8;
            n |= (uint)buffer[offset + 3];
            return n;
        }

        /// <summary>
        /// Converts big endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>Converted <see cref="UInt64" />.</returns>
        protected static ulong BigEndianToUInt64(byte[] buffer)
        {
            uint hi = BigEndianToUInt32(buffer);
            uint lo = BigEndianToUInt32(buffer, 4);
            return ((ulong)hi << 32) | (ulong)lo;
        }

        /// <summary>
        /// Converts big endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        /// <returns>Converted <see cref="UInt64" />.</returns>
        protected static ulong BigEndianToUInt64(byte[] buffer, int offset)
        {
            uint hi = BigEndianToUInt32(buffer, offset);
            uint lo = BigEndianToUInt32(buffer, offset + 4);
            return ((ulong)hi << 32) | (ulong)lo;
        }

        /// <summary>
        /// Populates buffer with big endian number representation.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        protected static void UInt64ToBigEndian(ulong number, byte[] buffer)
        {
            UInt32ToBigEndian((uint)(number >> 32), buffer);
            UInt32ToBigEndian((uint)(number), buffer, 4);
        }

        /// <summary>
        /// Populates buffer with big endian number representation.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        protected static void UInt64ToBigEndian(ulong number, byte[] buffer, int offset)
        {
            UInt32ToBigEndian((uint)(number >> 32), buffer, offset);
            UInt32ToBigEndian((uint)(number), buffer, offset + 4);
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        protected static void UInt32ToLittleEndian(uint number, byte[] buffer)
        {
            buffer[0] = (byte)(number);
            buffer[1] = (byte)(number >> 8);
            buffer[2] = (byte)(number >> 16);
            buffer[3] = (byte)(number >> 24);
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        protected static void UInt32ToLittleEndian(uint number, byte[] buffer, int offset)
        {
            buffer[offset] = (byte)(number);
            buffer[offset + 1] = (byte)(number >> 8);
            buffer[offset + 2] = (byte)(number >> 16);
            buffer[offset + 3] = (byte)(number >> 24);
        }

        /// <summary>
        /// Converts little endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>Converted <see cref="UInt32" />.</returns>
        protected static uint LittleEndianToUInt32(byte[] buffer)
        {
            uint n = (uint)buffer[0];
            n |= (uint)buffer[1] << 8;
            n |= (uint)buffer[2] << 16;
            n |= (uint)buffer[3] << 24;
            return n;
        }

        /// <summary>
        /// Converts little endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        /// <returns>Converted <see cref="Int32" />.</returns>
        protected static uint LittleEndianToUInt32(byte[] buffer, int offset)
        {
            uint n = (uint)buffer[offset];
            n |= (uint)buffer[offset + 1] << 8;
            n |= (uint)buffer[offset + 2] << 16;
            n |= (uint)buffer[offset + 3] << 24;
            return n;
        }

        /// <summary>
        /// Converts little endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>Converted <see cref="UInt64" />.</returns>
        protected static ulong LittleEndianToUInt64(byte[] buffer)
        {
            uint lo = LittleEndianToUInt32(buffer);
            uint hi = LittleEndianToUInt32(buffer, 4);
            return ((ulong)hi << 32) | (ulong)lo;
        }

        /// <summary>
        /// Converts little endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        /// <returns>Converted <see cref="UInt64" />.</returns>
        protected static ulong LittleEndianToUInt64(byte[] buffer, int offset)
        {
            uint lo = LittleEndianToUInt32(buffer, offset);
            uint hi = LittleEndianToUInt32(buffer, offset + 4);
            return ((ulong)hi << 32) | (ulong)lo;
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        protected static void UInt64ToLittleEndian(ulong number, byte[] buffer)
        {
            UInt32ToLittleEndian((uint)(number), buffer);
            UInt32ToLittleEndian((uint)(number >> 32), buffer, 4);
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="number">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        protected static void UInt64ToLittleEndian(ulong number, byte[] buffer, int offset)
        {
            UInt32ToLittleEndian((uint)(number), buffer, offset);
            UInt32ToLittleEndian((uint)(number >> 32), buffer, offset + 4);
        }

        #endregion
    }
}
