#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
using System;
using System.Buffers.Binary;
#endif

namespace Renci.SshNet.Common
{
    /// <summary>
    /// Provides convenience methods for conversion to and from both Big Endian and Little Endian.
    /// </summary>
    internal static class Pack
    {
        /// <summary>
        /// Converts little endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>Converted <see cref="ushort" />.</returns>
        internal static ushort LittleEndianToUInt16(byte[] buffer)
        {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            return BinaryPrimitives.ReadUInt16LittleEndian(buffer);
#else
            ushort n = buffer[0];
            n |= (ushort)(buffer[1] << 8);
            return n;
#endif
        }

        /// <summary>
        /// Converts little endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>Converted <see cref="uint" />.</returns>
        internal static uint LittleEndianToUInt32(byte[] buffer)
        {
            return LittleEndianToUInt32(buffer, offset: 0);
        }

        /// <summary>
        /// Converts little endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        /// <returns>Converted <see cref="uint" />.</returns>
        internal static uint LittleEndianToUInt32(byte[] buffer, int offset)
        {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            return BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(offset));
#else
            uint n = buffer[offset++];
            n |= (uint)buffer[offset++] << 8;
            n |= (uint)buffer[offset++] << 16;
            n |= (uint)buffer[offset] << 24;
            return n;
#endif
        }

        /// <summary>
        /// Converts little endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>Converted <see cref="ulong" />.</returns>
        internal static ulong LittleEndianToUInt64(byte[] buffer)
        {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            return BinaryPrimitives.ReadUInt64LittleEndian(buffer);
#else
            ulong n = buffer[0];
            n |= (ulong)buffer[1] << 8;
            n |= (ulong)buffer[2] << 16;
            n |= (ulong)buffer[3] << 24;
            n |= (ulong)buffer[4] << 32;
            n |= (ulong)buffer[5] << 40;
            n |= (ulong)buffer[6] << 48;
            n |= (ulong)buffer[7] << 56;
            return n;
#endif
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        internal static byte[] UInt16ToLittleEndian(ushort value)
        {
            var buffer = new byte[2];
            UInt16ToLittleEndian(value, buffer);
            return buffer;
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        private static void UInt16ToLittleEndian(ushort value, byte[] buffer)
        {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
#else
            buffer[0] = (byte)(value & 0x00FF);
            buffer[1] = (byte)((value & 0xFF00) >> 8);
#endif
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        internal static byte[] UInt32ToLittleEndian(uint value)
        {
            var buffer = new byte[4];
            UInt32ToLittleEndian(value, buffer, offset: 0);
            return buffer;
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        internal static void UInt32ToLittleEndian(uint value, byte[] buffer, int offset)
        {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(offset), value);
#else
            buffer[offset++] = (byte)(value & 0x000000FF);
            buffer[offset++] = (byte)((value & 0x0000FF00) >> 8);
            buffer[offset++] = (byte)((value & 0x00FF0000) >> 16);
            buffer[offset] = (byte)((value & 0xFF000000) >> 24);
#endif
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        internal static byte[] UInt64ToLittleEndian(ulong value)
        {
            var buffer = new byte[8];
            UInt64ToLittleEndian(value, buffer);
            return buffer;
        }

        /// <summary>
        /// Populates buffer with little endian number representation.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <param name="buffer">The buffer.</param>
        private static void UInt64ToLittleEndian(ulong value, byte[] buffer)
        {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
#else
            buffer[0] = (byte)(value & 0x00000000000000FF);
            buffer[1] = (byte)((value & 0x000000000000FF00) >> 8);
            buffer[2] = (byte)((value & 0x0000000000FF0000) >> 16);
            buffer[3] = (byte)((value & 0x00000000FF000000) >> 24);
            buffer[4] = (byte)((value & 0x000000FF00000000) >> 32);
            buffer[5] = (byte)((value & 0x0000FF0000000000) >> 40);
            buffer[6] = (byte)((value & 0x00FF000000000000) >> 48);
            buffer[7] = (byte)((value & 0xFF00000000000000) >> 56);
#endif
        }

        internal static byte[] UInt16ToBigEndian(ushort value)
        {
            var buffer = new byte[2];
            UInt16ToBigEndian(value, buffer, offset: 0);
            return buffer;
        }

        internal static void UInt16ToBigEndian(ushort value, byte[] buffer, int offset)
        {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            BinaryPrimitives.WriteUInt16BigEndian(buffer.AsSpan(offset), value);
#else
            buffer[offset] = (byte)(value >> 8);
            buffer[offset + 1] = (byte)(value & 0x00FF);
#endif
        }

        internal static void UInt32ToBigEndian(uint value, byte[] buffer)
        {
            UInt32ToBigEndian(value, buffer, offset: 0);
        }

        internal static void UInt32ToBigEndian(uint value, byte[] buffer, int offset)
        {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            BinaryPrimitives.WriteUInt32BigEndian(buffer.AsSpan(offset), value);
#else
            buffer[offset++] = (byte)((value & 0xFF000000) >> 24);
            buffer[offset++] = (byte)((value & 0x00FF0000) >> 16);
            buffer[offset++] = (byte)((value & 0x0000FF00) >> 8);
            buffer[offset] = (byte)(value & 0x000000FF);
#endif
        }

        internal static byte[] UInt32ToBigEndian(uint value)
        {
            var buffer = new byte[4];
            UInt32ToBigEndian(value, buffer);
            return buffer;
        }

        internal static void UInt64ToBigEndian(ulong value, byte[] buffer, int offset)
        {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            BinaryPrimitives.WriteUInt64BigEndian(buffer.AsSpan(offset), value);
#else
            buffer[offset++] = (byte)((value & 0xFF00000000000000) >> 56);
            buffer[offset++] = (byte)((value & 0x00FF000000000000) >> 48);
            buffer[offset++] = (byte)((value & 0x0000FF0000000000) >> 40);
            buffer[offset++] = (byte)((value & 0x000000FF00000000) >> 32);
            buffer[offset++] = (byte)((value & 0x00000000FF000000) >> 24);
            buffer[offset++] = (byte)((value & 0x0000000000FF0000) >> 16);
            buffer[offset++] = (byte)((value & 0x000000000000FF00) >> 8);
            buffer[offset] = (byte)(value & 0x00000000000000FF);
#endif
        }

        /// <summary>
        /// Converts big endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>Converted <see cref="ushort" />.</returns>
        internal static ushort BigEndianToUInt16(byte[] buffer)
        {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            return BinaryPrimitives.ReadUInt16BigEndian(buffer);
#else
            return (ushort)(buffer[0] << 8 | buffer[1]);
#endif
        }

        /// <summary>
        /// Converts big endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The buffer offset.</param>
        /// <returns>Converted <see cref="uint" />.</returns>
        internal static uint BigEndianToUInt32(byte[] buffer, int offset)
        {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            return BinaryPrimitives.ReadUInt32BigEndian(buffer.AsSpan(offset));
#else
            return (uint)buffer[offset + 0] << 24 |
                   (uint)buffer[offset + 1] << 16 |
                   (uint)buffer[offset + 2] << 8 |
                   buffer[offset + 3];
#endif
        }

        /// <summary>
        /// Converts big endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>Converted <see cref="uint" />.</returns>
        internal static uint BigEndianToUInt32(byte[] buffer)
        {
            return BigEndianToUInt32(buffer, offset: 0);
        }

        /// <summary>
        /// Converts big endian bytes into number.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>Converted <see cref="ulong" />.</returns>
        internal static ulong BigEndianToUInt64(byte[] buffer)
        {
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
            return BinaryPrimitives.ReadUInt64BigEndian(buffer);
#else
            return (ulong)buffer[0] << 56 |
                   (ulong)buffer[1] << 48 |
                   (ulong)buffer[2] << 40 |
                   (ulong)buffer[3] << 32 |
                   (ulong)buffer[4] << 24 |
                   (ulong)buffer[5] << 16 |
                   (ulong)buffer[6] << 8 |
                   buffer[7];
#endif
        }
    }
}
