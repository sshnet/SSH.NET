#if NETCOREAPP3_0 || NETSTANDARD2_1
using System;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using Renci.SshNet.Security.Cryptography.Ciphers;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// AEAD cipher implementation.
    /// </summary>
    public sealed class AEADCipher : BlockCipher
    {
        private readonly CipherPadding _padding;

        /// <summary>
        /// Instance for AesGcm Class
        /// </summary>
        private AesGcm cipher;

        /// <summary>
        /// The size of the block in bytes.
        /// AES128-GCM: 16 bytes
        /// AES192-GCM: 24 bytes
        /// AES256-GCM: 32 bytes
        /// </summary>
        private readonly byte _blockSize;

        /// <summary>
        /// The key stream for AES-GCM Encrypt and Decrypt.
        /// </summary>
        private readonly byte[] _key;

        /// <summary>
        /// The IV for AES-GCM Encrypt and Decrypt.
        /// </summary>
        private byte[] _iv;

        /// <summary>
        /// The IV length for AES-GCM.
        /// Length sz is basd on [RFC5647] Section 7.1 .Auguest 2009
        /// </summary>
        private readonly int NONCE_BYTES = 12;

        /// <summary>
        /// Packet Length Field 4 bytes for AES GCM which is Additional Authenticated Data (AAD) in plain text.
        /// Based on [RFC5647] Section 7.2 .Auguest 2009
        /// </summary>
        private readonly int PACKET_LENGTH_FIELD_SZ = 4;

        /// <summary>
        /// The size of the tag based on [RFC5647] Section 6.3 .Auguest 2009
        /// Both AEAD_AES_128_GCM and AEAD_AES_256_GCM produce a 16-octet Authentication Tag
        /// AEAD_AES_192_GCM can also use 16 bytes of the tag size based on NIST Special Publication 800-38D .page 8
        /// </summary>
        private readonly int TAG_SIZE = 16;

        /// <summary>
        /// Find the right offset for AEAD cipher decryption.
        /// </summary>
        /// <param name="blockSz">The block cipher size of AEAD Cipher.</param>
        /// <param name="inboundPacketSequenceLength">The inbound packet sequence length.</param>
        /// <returns>The decrypt offset used for the decrypt function of AEAD cipher is
        /// inboundPacketSequenceLength + PACKET_LENGTH_FIELD_SZ</returns>
        public override int decryptOffset(int inboundPacketSequenceLength, int blockSz)
        {
            return inboundPacketSequenceLength + PACKET_LENGTH_FIELD_SZ;
        }

        /// <summary>
        /// AEAD Mode server mac length is the tag size based on [RFC5647] Section 6.3 .Auguest 2009
        /// Both AEAD_AES_128_GCM and AEAD_AES_256_GCM produce a 16-octet Authentication Tag
        /// AEAD_AES_192_GCM can also use 16 bytes of the tag size based on NIST Special Publication 800-38D .page 8
        /// </summary>
        /// <returns>The tag length for AEAD AES-GCM.</returns>
        public override int serverMacLength(HashAlgorithm _serverMac)
        {
            return TAG_SIZE;
        }

        /// <summary>
        /// AEAD Mode Flag
        /// </summary>
        /// <value>
        /// AEAD Mode is set to true if it's AEAD AES-GCM Operation.
        /// </value>
        public override bool isAEAD
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AEADCipher"/> class.
        /// </summary>
        /// <param name="key">The key for encryption decryption during AES-GCM.</param>
        /// <param name="iv">The iv as known as nonce during AES-GCM.</param>
        /// <param name="blockSize">The size of the block cipher.</param>
        /// <param name="mode">The mode is set to null for AEAD AES-GCM.</param>
        /// <param name="padding">The padding class for AEAD AES-GCM.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Keysize is not valid for this algorithm.</exception>
        public AEADCipher(byte[] key, byte[] iv, byte blockSize, CipherMode mode, CipherPadding padding)
            : base(key, blockSize, mode, padding)
        {
            _blockSize = blockSize;
            _padding = padding;

            // Get the IV length of bytes for AES-GCM (12 bytes) from initial IV.
            // Length sz is basd on [RFC5647] Section 7.1 .Auguest 2009
            _iv = new byte[NONCE_BYTES];
            Array.Copy(iv, 0, _iv, 0, NONCE_BYTES);

            _key = new byte[key.Length];
            _key = key;

            // Key Size Check for AES-GCM
            // AES128-GCM: 16 bytes [RFC5116] Section 5.2 .January 2008
            // AES192-GCM: 24 bytes [RFC5084] Section 1.3 .November 2007
            // AES256-GCM: 32 bytes [RFC5116] Section 5.3 .January 2008
            if (!(_key.Length == 16 || _key.Length == 24 || _key.Length == 32))
                throw new ArgumentException(string.Format("KeySize '{0}' is not valid for this algorithm.", _key.Length));

            cipher = new AesGcm(_key);

            // If cipher is null throw exception
            if (cipher == null)
                throw new ArgumentException(string.Format("Invalid AES-GCM Class Cipher."));
        }
#if DEBUG_AESGCM

        /// <summary>
        /// GetHexStringFrom.
        /// </summary>
        /// <returns>String.</returns>
        public static string GetHexStringFrom(byte[] byteArray)
        {
            return  BitConverter.ToString(byteArray); //To convert the whole array
        }

#endif

        /// <summary>
        /// AEAD AES-GCM Encrypt Function
        /// </summary>
        /// <param name="inputBuffer">The input data which contains packet length field (Additional Authenticated Data)
        /// and the plain text.
        /// </param>
        /// <returns>
        /// The number of bytes encrypted.
        /// </returns>
        // Structure of the inputBuffer:
        // |------4 bytes------||-------------------Plain Text--------------------|
        // [packet length field][padding length field sz][payload][random paddings]
        private byte[] GcmEncrypt(byte[] inputBuffer)
        {
            if (inputBuffer.Length - PACKET_LENGTH_FIELD_SZ < _blockSize)
                throw new ArgumentException("Invalid input buffer");

            var tag = new byte[TAG_SIZE];
            var cipherText = new byte[inputBuffer.Length - PACKET_LENGTH_FIELD_SZ];

            var associatedData = new ReadOnlySpan<byte>(inputBuffer, 0, PACKET_LENGTH_FIELD_SZ);
            var plainText = new ReadOnlySpan<byte>(inputBuffer, PACKET_LENGTH_FIELD_SZ, cipherText.Length);

#if DEBUG_AESGCM

            string var_iv = GetHexStringFrom(_iv);
            string var_key = GetHexStringFrom(_key);
            string var_pt = GetHexStringFrom(plainText.ToArray());
            string var_aad = GetHexStringFrom(associatedData.ToArray());

            Console.WriteLine("[DEBUG] In GcmEncrypt()");
            Console.WriteLine("[DEBUG] Encrypt Length: {0}", inputBuffer.Length - PACKET_LENGTH_FIELD_SZ);
            Console.WriteLine("[DEBUG] IV: {0}", var_iv);
            Console.WriteLine("[DEBUG] Key: {0}", var_key);
            Console.WriteLine("[DEBUG] PT before Encrypt: {0}", var_pt);
            Console.WriteLine("[DEBUG] AAD before Encrypt: {0}", var_aad);

#endif //DEBUG_AESGCM
            // AES-GCM Encrypt Function
            cipher.Encrypt(_iv, plainText, cipherText, tag, associatedData);

            // Reallocate the original buffer by the length of tag size
            Array.Resize(ref inputBuffer, inputBuffer.Length + TAG_SIZE);

            // Add cipher text into reallocated input buffer
            Array.Copy(cipherText, 0, inputBuffer, associatedData.Length, cipherText.Length);

            // Add cipher text into reallocated input buffer
            Array.Copy(tag, 0, inputBuffer, associatedData.Length + cipherText.Length, TAG_SIZE);

            int j = _iv.Length;

            // Increment the counter after each operation
            while (--j >= 0 && ++_iv[j] == 0)
                ;

            return inputBuffer;
        }

        /// <summary>
        /// AEAD AES-GCM Decrypt Function
        /// </summary>
        /// <param name="inputBuffer">The input data that contains inbound packet sequence length, packet length, cipher text(CT)
        /// and authenticated tag(AT).</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <returns>Plain text.</returns>
        // Structure of the inputBuffer:
        // |4 bytes(offset)-||------4 bytes------||------------------Cipher Text--------------------||-------TAG-------|
        // [inbound sequence][packet length field][padding length field sz][payload][random paddings][Authenticated TAG]
        private byte[] GcmDecrypt(byte[] inputBuffer, int inputOffset)
        {
            if (inputBuffer.Length - inputOffset < _blockSize)
                throw new ArgumentException("Invalid input buffer");

            var plainText = new byte[inputBuffer.Length - inputOffset - PACKET_LENGTH_FIELD_SZ - TAG_SIZE];

            // Parse out the Additional Authenticated Data from input buffer
            var associatedData = new ReadOnlySpan<byte>(inputBuffer, inputOffset, PACKET_LENGTH_FIELD_SZ);

            // Parse out the cipher text from input buffer
            var cipherText = new ReadOnlySpan<byte>(inputBuffer, inputOffset + PACKET_LENGTH_FIELD_SZ, plainText.Length);

            // Parse out the Authenticated Tag from input buffer
            var tag = new ReadOnlySpan<byte>(inputBuffer, inputOffset + PACKET_LENGTH_FIELD_SZ + plainText.Length, TAG_SIZE);
#if DEBUG_AESGCM

            string var_iv = GetHexStringFrom(_iv);
            string var_key = GetHexStringFrom(_key);
            string var_ct = GetHexStringFrom(cipherText.ToArray());
            string var_tag = GetHexStringFrom(tag.ToArray());
            string var_aad = GetHexStringFrom(associatedData.ToArray());

            Console.WriteLine("[DEBUG] In GcmDecrypt()");
            Console.WriteLine("[DEBUG] IV: {0}", var_iv);
            Console.WriteLine("[DEBUG] Key: {0}", var_key);
            Console.WriteLine("[DEBUG] AAD before decrypt: {0}", var_aad);
            Console.WriteLine("[DEBUG] CT before decrypt: {0}", var_ct);
            Console.WriteLine("[DEBUG] TAG before decrypt: {0}", var_tag );

#endif //DEBUG_AESGCM

            cipher.Decrypt(_iv, cipherText, tag, plainText, associatedData);

            // Reallocate the original buffer to only contain plain text
            Array.Resize(ref inputBuffer, plainText.Length);

            // Add plain text into reallocated input buffer
            Array.Copy(plainText, 0, inputBuffer, 0, plainText.Length);

            int j = _iv.Length;

            while (--j >= 0 && ++_iv[j] == 0)
                ;

            return inputBuffer;
        }

        /// <summary>
        /// Encrypts the specified data.
        /// </summary>
        /// <param name="data">The input data which contains outbound sequence length, packet length field and plain text.</param>
        /// <param name="offset">The offset (the outbound sequence length) into the input byte array from
        /// which to begin using data.</param>
        /// <param name="length">The total length of the input data which contains packet length field and plain text<paramref name="data"/>.</param>
        /// <returns>Encrypted data</returns>
        // Structure of the data:
        // |-4 bytes(offset)-||------4 bytes------||-------------------Plain Text--------------------|
        // [outbound sequence][packet field length][padding length field sz][payload][random paddings]
        public override byte[] Encrypt(byte[] data, int offset, int length)
        {
            // Length minus packet field length should be PT which needs to be multiplication of 16 in AEAD AES-GCM
            // Based on [RFC5647] Section 7.2 .Auguest 2009
            if ((length - PACKET_LENGTH_FIELD_SZ) % _blockSize > 0)
            {
                if (_padding == null)
                {
                    throw new ArgumentException("Padding Should Not Be Null for AEAD AES GCM Encrypt");
                }

                // Get the number of paddings need to be added
                var paddingLength = _blockSize - ((length - PACKET_LENGTH_FIELD_SZ) % _blockSize);

                data = _padding.Pad(data, offset, length, paddingLength);

                // dataLength is the sum of outbound sequence length, updated packet length field and updated plain text
                var dataLength = data.Length;

                // Convert into an array for GCM Encrypt
                data = new ReadOnlySpan<byte>(data, offset, dataLength - offset).ToArray();

                // Structure of updated data buffer before invoke GcmEncrypt():
                // |----------4 bytes----------||----------------------------------------Plain Text-------------------------------------------|
                // [updated packet length field][updated padding length field sz][payload][random paddings + paddingDataLength of random bytes]
                // output = PakcetLength Field Length(Additional Authenticated Data) + Cipher Text + Authenticated Tag
                // Basd on [RFC5647] Section 7.2 .Auguest 2009
                var output = GcmEncrypt(data);

                if (output.Length != dataLength - offset + TAG_SIZE)
                {
                    throw new ArgumentException("Ecryption Failed for AES-GCM");
                }
#if DEBUG_AESGCM

                string output_buffer = GetHexStringFrom(output);
                Console.WriteLine("[DEBUG] In AEADBlockCipher Encrypt()");
                Console.WriteLine("[DEBUG] Output Payload Length After Encryption: {0} ", output.Length);
                Console.WriteLine("[DEBUG] Output Payload After Encryption");
                Console.WriteLine("[DEBUG] {0}", output_buffer);

#endif
                return output;
            }

            throw new ArgumentException("Shouldn't Reach Here for AEAD AES GCM as Padding is Always Needed. ");
        }

        /// <summary>
        /// For AEAD AES GCM since there is no first block decryption, this function will return what have been received
        /// </summary>
        /// <param name="data">The first block of data that contains 4 bytes of packet length field and 12 bytes of cipher text(CT).</param>
        /// <returns>The first block of data that contains 4 bytes of packet length field and 12 bytes of cipher text(CT).</returns>
        public override byte[] Decrypt(byte[] data)
        {
            return data;
        }

        /// <summary>
        /// Decrypts the specified data.
        /// </summary>
        /// <param name="data">The packet data that contains inbound packet sequence length, packet length field(AAD), cipher text(CT)
        /// and authenticated tag(AT).</param>
        /// <param name="offset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="length">The length of cipher text that is a multiplication of 16 <paramref name="data"/>.</param>
        /// <returns>Decrypted plain text</returns>
        // Structure of the inputBuffer:
        // |4 bytes(offset)-||------4 bytes------||------------------Cipher Text--------------------||-------TAG-------|
        // [inbound sequence][packet length field][padding length field sz][payload][random paddings][Authenticated TAG]
        public override byte[] Decrypt(byte[] data, int offset, int length)
        {
            // Since when invoke decrypt() in session.cs the offset is PACKET_LENGTH_FIELD_SZ + inboundPacketSequenceLength
            // For AEAD AES-GCM, the offset should be inboundPacketSequenceLength so we need to substract PACKET_LENGTH_FIELD_SZ
            offset -= PACKET_LENGTH_FIELD_SZ;

            if (length % _blockSize > 0)
            {
                // Shouldn't reach here for AEAD AES-GCM since the length is always a multiplication of 16
                // Still keep the padding utility

                if (_padding == null)
                {
                    throw new ArgumentException("Padding is NULL");
                }

                var paddingLength = _blockSize - (length % _blockSize);

                // length equals to cipher text length, thus need to add packet field length back before invoke the padding function
                data = _padding.Pad(data, offset, length + PACKET_LENGTH_FIELD_SZ, paddingLength);

                length = data.Length;
            }

            var output = GcmDecrypt(data, offset);

            if (output.Length != length)
            {
                throw new ArgumentException("Ecryption Failed for AEAD AES-GCM");
            }
#if DEBUG_AESGCM

            string output_buffer = GetHexStringFrom(output);
            Console.WriteLine("[DEBUG] AEAD BlockCipher Decrypt()");
            Console.WriteLine("[DEBUG] Output Payload Length After Decryption: {0} ", output.Length);
            Console.WriteLine("[DEBUG] Output Payload After Decryption");
            Console.WriteLine("[DEBUG] {0}", output_buffer);

#endif //DEBUG_AESGCM

            return output;
        }

        /// <summary>
        /// Encrypts the specified region of the input byte array and copies the encrypted data to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input data to encrypt.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">The output to which to write encrypted data.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>
        /// The number of bytes encrypted.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="inputBuffer"/> or <paramref name="outputBuffer"/> is <c>null</c>.</exception>
        /// <exception cref="IndexOutOfRangeException"><paramref name="inputBuffer"/> or <paramref name="outputBuffer"/> is too short.</exception>
        public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            throw new ArgumentException("Invalid function call");
        }

        /// <summary>
        /// Decrypts the specified region of the input byte array and copies the decrypted data to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input data to decrypt.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">The output to which to write decrypted data.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>
        /// The number of bytes decrypted.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="inputBuffer"/> or <paramref name="outputBuffer"/> is <c>null</c>.</exception>
        /// <exception cref="IndexOutOfRangeException"><paramref name="inputBuffer"/> or <paramref name="outputBuffer"/> is too short.</exception>
        public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            throw new ArgumentException("Invalid function call");
        }
    }
}
#endif
