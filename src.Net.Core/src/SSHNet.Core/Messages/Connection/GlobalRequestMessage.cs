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
#if true //old TUNING
        private byte[] _requestName;
        private byte[] _addressToBind;
#endif

        /// <summary>
        /// Gets the name of the request.
        /// </summary>
        /// <value>
        /// The name of the request.
        /// </value>
#if true //old TUNING
        public GlobalRequestName RequestName
        {
            get { return _requestName.ToGlobalRequestName(); }
        }
#else
        public GlobalRequestName RequestName { get; private set; }
#endif

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
#if true //old TUNING
        public string AddressToBind
        {
            get { return Utf8.GetString(_addressToBind, 0, _addressToBind.Length); }
            private set { _addressToBind = Utf8.GetBytes(value); }
        }
#else
        public string AddressToBind { get; private set; }
        //  TODO:   Extract AddressToBind property to be in different class and GlobalREquestMessage to be a base class fo it.
#endif

        /// <summary>
        /// Gets port number to bind to.
        /// </summary>
        public UInt32 PortToBind { get; private set; }

#if true //old TUNING
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
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalRequestMessage"/> class.
        /// </summary>
        public GlobalRequestMessage()
        {

        }

#if false //old  !TUNING
        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalRequestMessage"/> class.
        /// </summary>
        /// <param name="requestName">Name of the request.</param>
        /// <param name="wantReply">if set to <c>true</c> [want reply].</param>
        public GlobalRequestMessage(GlobalRequestName requestName, bool wantReply)
        {
            RequestName = requestName;
            WantReply = wantReply;
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalRequestMessage"/> class.
        /// </summary>
        /// <param name="requestName">Name of the request.</param>
        /// <param name="wantReply">if set to <c>true</c> [want reply].</param>
        /// <param name="addressToBind">The address to bind.</param>
        /// <param name="portToBind">The port to bind.</param>
        public GlobalRequestMessage(GlobalRequestName requestName, bool wantReply, string addressToBind, uint portToBind)
#if false //old  !TUNING
            : this(requestName, wantReply)
#endif
        {
#if true //old TUNING
            _requestName = requestName.ToArray();
            WantReply = wantReply;
#endif
            AddressToBind = addressToBind;
            PortToBind = portToBind;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
#if true //old TUNING
            _requestName = ReadBinary();
#else
            var requestName = ReadAsciiString();
#endif

            WantReply = ReadBoolean();

#if true //old TUNING
            _addressToBind = ReadBinary();
            PortToBind = ReadUInt32();
#else
            switch (requestName)
            {
                case "tcpip-forward":
                    RequestName = GlobalRequestName.TcpIpForward;
                    AddressToBind = ReadString();
                    PortToBind = ReadUInt32();
                    break;
                case "cancel-tcpip-forward":
                    RequestName = GlobalRequestName.CancelTcpIpForward;
                    AddressToBind = ReadString();
                    PortToBind = ReadUInt32();
                    break;
            }
#endif
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
#if true //old TUNING
            WriteBinaryString(_requestName);
#else
            switch (RequestName)
            {
                case GlobalRequestName.TcpIpForward:
                    WriteAscii("tcpip-forward");
                    break;
                case GlobalRequestName.CancelTcpIpForward:
                    WriteAscii("cancel-tcpip-forward");
                    break;
            }
#endif

            Write(WantReply);

#if true //old TUNING
            WriteBinaryString(_addressToBind);
            Write(PortToBind);
#else
            switch (RequestName)
            {
                case GlobalRequestName.TcpIpForward:
                case GlobalRequestName.CancelTcpIpForward:
                    Write(AddressToBind);
                    Write(PortToBind);
                    break;
            }
#endif
        }
    }
}
