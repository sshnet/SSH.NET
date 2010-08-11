using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using Renci.SshClient.Messages;

namespace Renci.SshClient
{
    internal class SessionSSHv2 : Session
    {
        private static object _readLock = new object();
        private static object _writeLock = new object();

        private static RNGCryptoServiceProvider _randomizer = new System.Security.Cryptography.RNGCryptoServiceProvider();

        private UInt32 _outboundPacketSequence = 0;
        private UInt32 _inboundPacketSequence = 0;

        internal SessionSSHv2(ConnectionInfo connectionInfo, Socket socket, string serverVersion)
            : base(connectionInfo, socket, serverVersion)
        {
        }

        internal override void SendMessage(Message message)
        {
            if (!this.IsSocketConnected)
                return;

            //  TODO:  Refactor so we lock only _outboundPacketSequence and _inboundPacketSequence relevant operations

            //  Messages can be sent by different thread so we need to synchronize it
            lock (_writeLock)
            {
                var paddingMultiplier = this.ClientCipher == null ? (byte)8 : (byte)this.ClientCipher.BlockSize;    //    Should be recalculate base on cipher min lenght if sipher specified

                //  TODO:   Maximum uncomporessed payload 32768
                //  TOOO:   If compression specified then compress only payload

                var messageData = message.GetBytes();

                var packetLength = messageData.Count() + 4 + 1; //  add length bytes and padding byte
                byte paddingLength = (byte)((-packetLength) & (paddingMultiplier - 1));
                if (paddingLength < paddingMultiplier)
                {
                    paddingLength += paddingMultiplier;
                }

                //  Build Packet data
                var packetData = new List<byte>();

                //  Add packet padding length
                packetData.Add(paddingLength);

                //  Add packet payload
                packetData.AddRange(messageData);

                //  Add random padding
                var paddingBytes = new byte[paddingLength];
                _randomizer.GetBytes(paddingBytes);
                packetData.AddRange(paddingBytes);

                //  Insert packet length
                packetData.InsertRange(0, BitConverter.GetBytes((uint)(packetData.Count())).Reverse());

                //  Calculate packet hash
                var hashData = new List<byte>();
                hashData.AddRange(BitConverter.GetBytes((this._outboundPacketSequence)).Reverse());
                hashData.AddRange(packetData);

                //  Encrypt packet data
                var encryptedData = packetData.ToList();
                if (this.ClientCipher != null)
                {
                    //encryptedData = new List<byte>(this.Encrypt(packetData));
                    encryptedData = new List<byte>(this.ClientCipher.Encrypt(packetData));
                }

                //  Add message authentication code (MAC)
                if (this.ClientMac != null)
                {
                    var hash = this.ClientMac.ComputeHash(hashData.ToArray());

                    encryptedData.AddRange(hash);
                }

                if (encryptedData.Count > Session.MAXIMUM_PACKET_SIZE)
                {
                    throw new InvalidOperationException("Packet is too big. Maximum packet size is 35000 bytes.");
                }

                this.Write(encryptedData.ToArray());

                this._outboundPacketSequence++;
            }
        }

        protected override Message ReceiveMessage()
        {
            if (!this.IsSocketConnected)
                return null;

            //  No lock needed since all messages read by only one thread

            List<byte> decryptedData;

            //var blockSize = this.Decryption == null ? (byte)8 : (byte)this.Decryption.InputBlockSize;
            var blockSize = this.ServerCipher == null ? (byte)8 : (byte)this.ServerCipher.BlockSize;

            //  Read packet lenght first
            var data = new List<byte>(this.Read(blockSize));

            if (this.ServerCipher == null)
            {
                decryptedData = data.ToList();
            }
            else
            {
                //decryptedData = new List<byte>(this.Decrypt(data));
                decryptedData = new List<byte>(this.ServerCipher.Decrypt(data));
            }

            var packetLength = BitConverter.ToUInt32(decryptedData.Take(4).Reverse().ToArray(), 0);

            //  Test packet minimum and maximum boundaries
            if (packetLength < Math.Max((byte)16, blockSize) - 4 || packetLength > Session.MAXIMUM_PACKET_SIZE - 4)
                throw new InvalidOperationException(string.Format("Bad packet length {0}", packetLength));

            //  Read rest of the packet data
            int bytesToRead = (int)(packetLength - (blockSize - 4));

            while (bytesToRead > 0)
            {
                data = new List<byte>(this.Read(blockSize));

                if (this.ServerCipher == null)
                {
                    decryptedData.AddRange(data);
                }
                else
                {
                    //decryptedData.AddRange(this.Decrypt(data));
                    decryptedData.AddRange(this.ServerCipher.Decrypt(data));
                }
                bytesToRead -= blockSize;
            }

            //  Validate message against MAC
            if (this.ServerMac != null)
            {
                var serverHash = this.Read(this.ServerMac.HashSize / 8);

                var clientHashData = new List<byte>();
                clientHashData.AddRange(BitConverter.GetBytes(this._inboundPacketSequence).Reverse());
                clientHashData.AddRange(decryptedData);

                //  Calculate packet hash
                var clientHash = this.ServerMac.ComputeHash(clientHashData.ToArray());

                if (!serverHash.IsEqualTo(clientHash))
                {
                    throw new InvalidOperationException("MAC error");
                }
            }

            //  TODO:   Issue new keys after x number of packets
            this._inboundPacketSequence++;

            var paddingLength = decryptedData[4];

            return Message.Load(decryptedData.Skip(5).Take((int)(packetLength - paddingLength - 1)));
        }

        private bool ValidateHash(List<byte> decryptedData, byte[] serverHash, uint packetSequence)
        {
            var clientHashData = new List<byte>();
            clientHashData.AddRange(BitConverter.GetBytes(packetSequence).Reverse());
            clientHashData.AddRange(decryptedData);

            var clientHash = this.ServerMac.ComputeHash(clientHashData.ToArray());
            if (!serverHash.IsEqualTo(clientHash))
            {
                return false;
            }
            return true;
        }

        //private IEnumerable<byte> Encrypt(List<byte> data)
        //{
        //    var temp = new byte[data.Count];
        //    this.Encryption.TransformBlock(data.ToArray(), 0, data.Count, temp, 0);
        //    return temp;
        //}

        //private IEnumerable<byte> Decrypt(List<byte> data)
        //{
        //    var temp = new byte[data.Count];
        //    this.Decryption.TransformBlock(data.ToArray(), 0, data.Count, temp, 0);
        //    return temp;
        //}
    }
}
