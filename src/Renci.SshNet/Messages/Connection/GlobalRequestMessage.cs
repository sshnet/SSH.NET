using System;
using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_GLOBAL_REQUEST message.
    /// </summary>
    [Message("SSH_MSG_GLOBAL_REQUEST", 80)]
    public class GlobalRequestMessage : Message
    {
        private byte[] _requestName;
        private byte[] _addressToBind;

        /// <summary>
        /// Gets the name of the request.
        /// </summary>
        /// <value>
        /// The name of the request.
        /// </value>
        public GlobalRequestName RequestName
        {
            get { return _requestName.ToGlobalRequestName(); }
        }

        /// <summary>
        /// Gets a value indicating whether message reply should be sent..
        /// </summary>
        /// <value>
        ///   <c>true</c> if message reply should be sent; otherwise, <c>false</c>.
        /// </value>
        public bool WantReply { get; private set; }

        /// <summary>
        /// Gets the address to bind to.
        /// </summary>
        public string AddressToBind
        {
            get { return Utf8.GetString(_addressToBind, 0, _addressToBind.Length); }
            private set { _addressToBind = Utf8.GetBytes(value); }
        }

        /// <summary>
        /// Gets port number to bind to.
        /// </summary>
        public uint PortToBind { get; private set; }

        /// <summary>
        /// Gets the size of the message in bytes.
        /// </summary>
        /// <value>
        /// The size of the messages in bytes.
        /// </value>
        protected override int BufferCapacity
        {
            get
            {
                var capacity = base.BufferCapacity;
                capacity += 4; // RequestName length
                capacity += _requestName.Length; // RequestName
                capacity += 1; // WantReply
                capacity += 4; // AddressToBind length
                capacity += _addressToBind.Length; // AddressToBind
                capacity += 4; // PortToBind
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalRequestMessage"/> class.
        /// </summary>
        public GlobalRequestMessage()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalRequestMessage"/> class.
        /// </summary>
        /// <param name="requestName">Name of the request.</param>
        /// <param name="wantReply">if set to <c>true</c> [want reply].</param>
        /// <param name="addressToBind">The address to bind.</param>
        /// <param name="portToBind">The port to bind.</param>
        public GlobalRequestMessage(GlobalRequestName requestName, bool wantReply, string addressToBind, uint portToBind)
        {
            _requestName = requestName.ToArray();
            WantReply = wantReply;
            AddressToBind = addressToBind;
            PortToBind = portToBind;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            _requestName = ReadBinary();
            WantReply = ReadBoolean();
            _addressToBind = ReadBinary();
            PortToBind = ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            WriteBinaryString(_requestName);
            Write(WantReply);
            WriteBinaryString(_addressToBind);
            Write(PortToBind);
        }
    }
}
