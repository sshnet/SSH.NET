using System;

namespace Renci.SshNet.Messages.Connection
{
    internal class TcpIpForwardGlobalRequestMessage : GlobalRequestMessage
    {
        private byte[] _addressToBind;

        public TcpIpForwardGlobalRequestMessage(string addressToBind, uint portToBind)
            : base(Ascii.GetBytes("tcpip-forward"), true)
        {
            AddressToBind = addressToBind;
            PortToBind = portToBind;
        }

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
                capacity += 4; // AddressToBind length
                capacity += _addressToBind.Length; // AddressToBind
                capacity += 4; // PortToBind
                return capacity;
            }
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();
            WriteBinaryString(_addressToBind);
            Write(PortToBind);
        }
    }
}
