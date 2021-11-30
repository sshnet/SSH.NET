#if NETCOREAPP3_0 || NETSTANDARD2_1
using System;
using System.Text;
using Renci.SshNet.Common;
using Renci.SshNet.Abstractions;

namespace Renci.SshNet.Security.Cryptography.Ciphers.Paddings
{
    /// <summary>
    /// Implements AEAD cipher padding
    /// </summary>
    public class AEADPadding : CipherPadding
    {
        /// <summary>
        /// Packet Length Field 4 bytes for AES GCM which is AAD as well.
        /// Based on [RFC5647] Section 7.2 .Auguest 2009
        /// </summary>
        private readonly int PAD_LENGTH_FIELD_SZ = 1;

        /// <summary>
        /// Packet Length Field 4 bytes for AES GCM which is AAD as well.
        /// Based on [RFC5647] Section 7.2 .Auguest 2009
        /// </summary>
        private readonly int PACKET_LENGTH_FIELD_SZ = 4;

        /// <summary>
        /// Pads the specified input to match the block size.
        /// </summary>
        /// <param name="blockSize">The size of the block cipher for AEAD AES-GCM.</param>
        /// <param name="input">The input buffer which contains inbound sequence length, packet length field and Cipher Text.</param>
        /// <param name="offset">The offset into the input data array from which to begin using data..</param>
        /// <param name="length">The number of bytes in <paramref name="input"/> to take into account.</param>
        /// <returns>
        /// The padded data array.
        /// </returns>
        public override byte[] Pad(int blockSize, byte[] input, int offset, int length)
        {
            throw new ArgumentException("Not Implemented");
        }

        /// <summary>
        /// Pads the specified input with a given number of bytes.
        /// </summary>
        /// <param name="input">The packet Data which contains outbound/inbound sequence length, packet length field and plain text PT .</param>
        /// <param name="offset">The offset into the input data array from which to begin using data.</param>
        /// <param name="length">The number of bytes in <paramref name="input"/> to take into account
        /// which is input buffer data - outbound/inbound sequence length.</param>
        /// <param name="paddingDataLength">The number of padding bytes need to add to <paramref name="input"/>.</param>
        /// <returns>
        /// The padded data array.
        /// </returns>
        // Structure of the data:
        // |-----4 bytes(offset)-----||-----4 bytes-------|-------------------Plain Text--------------------|
        // [outbound/inbound sequence][packet length field][padding length field sz][payload][random paddings]
        // After re construct the new output buffer will be:
        // |----------4 bytes----------||----------------------------------------Plain Text-------------------------------------------|
        // [updated packet length field][updated padding length field sz][payload][random paddings + paddingDataLength of random bytes]
        public override byte[] Pad(byte[] input, int offset, int length, int paddingDataLength)
        {
            // Generate the compensated padding bytes
            var paddingBytes = new byte[paddingDataLength];
            CryptoAbstraction.GenerateRandom(paddingBytes);

            // Find Old padding byte length then convert to int type
            var padding_field = Buffer.GetByte(input, offset + PACKET_LENGTH_FIELD_SZ);
            int padding_field_length = Convert.ToInt32(padding_field);

            // New padding length byte
            var _paddingSz = padding_field_length + paddingDataLength;

            padding_field = Convert.ToByte(_paddingSz);

            // Extract old packet length field
            var packetlength_field = new ReadOnlySpan<byte>(input, offset, PACKET_LENGTH_FIELD_SZ).ToArray();
            var packetlength_field_int32 = Pack.BigEndianToUInt32(packetlength_field, 0);

            // Calculate the msg payload length
            var messageLength = packetlength_field_int32 - PAD_LENGTH_FIELD_SZ - padding_field_length;

            // New packet length field
            packetlength_field_int32 = packetlength_field_int32 + (uint)paddingDataLength;
            Pack.UInt32ToBigEndian(packetlength_field_int32, packetlength_field);

            // Only update packet length field, padding length byte and new padding bytes from the original data buffer
            // Based on [RFC5647] Section 7.2
            Array.Resize(ref input, offset + length + paddingDataLength);

            //Update the packet length field from original array
            Array.Copy(packetlength_field, 0, input, offset, PACKET_LENGTH_FIELD_SZ);

            //Update the padding length field from original array
            Buffer.SetByte(input, offset + PACKET_LENGTH_FIELD_SZ, padding_field);

            //Update the padding bytes from original array
            Array.Copy(paddingBytes, 0, input, offset + PACKET_LENGTH_FIELD_SZ + PAD_LENGTH_FIELD_SZ + (int)messageLength + (int)padding_field_length, paddingDataLength);

            return input;
        }
    }
}
#endif