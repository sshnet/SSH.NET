using System;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents SSH_MSG_GLOBAL_REQUEST message.
    /// </summary>
    [Message("SSH_MSG_GLOBAL_REQUEST", 80)]
    public class GlobalRequestMessage : Message
    {
        /// <summary>
        /// Gets the name of the request.
        /// </summary>
        /// <value>
        /// The name of the request.
        /// </value>
        public GlobalRequestName RequestName { get; private set; }

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
        public string AddressToBind { get; private set; }
        //  TODO:   Extract AddressToBind property to be in different class and GlobalREquestMessage to be a base class fo it.

        /// <summary>
        /// Gets port number to bind to.
        /// </summary>
        public UInt32 PortToBind { get; private set; }

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
        public GlobalRequestMessage(GlobalRequestName requestName, bool wantReply)
        {
            this.RequestName = requestName;
            this.WantReply = wantReply;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalRequestMessage"/> class.
        /// </summary>
        /// <param name="requestName">Name of the request.</param>
        /// <param name="wantReply">if set to <c>true</c> [want reply].</param>
        /// <param name="addressToBind">The address to bind.</param>
        /// <param name="portToBind">The port to bind.</param>
        public GlobalRequestMessage(GlobalRequestName requestName, bool wantReply, string addressToBind, uint portToBind)
            : this(requestName, wantReply)
        {
            this.AddressToBind = addressToBind;
            this.PortToBind = portToBind;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            var requestName = this.ReadAsciiString();

            this.WantReply = this.ReadBoolean();

            switch (requestName)
            {
                case "tcpip-forward":
                    this.RequestName = GlobalRequestName.TcpIpForward;
                    this.AddressToBind = this.ReadString();
                    this.PortToBind = this.ReadUInt32();
                    break;
                case "cancel-tcpip-forward":
                    this.RequestName = GlobalRequestName.CancelTcpIpForward;
                    this.AddressToBind = this.ReadString();
                    this.PortToBind = this.ReadUInt32();
                    break;
            }
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            switch (this.RequestName)
            {
                case GlobalRequestName.TcpIpForward:
                    this.WriteAscii("tcpip-forward");
                    break;
                case GlobalRequestName.CancelTcpIpForward:
                    this.WriteAscii("cancel-tcpip-forward");
                    break;
            }

            this.Write(this.WantReply);

            switch (this.RequestName)
            {
                case GlobalRequestName.TcpIpForward:
                case GlobalRequestName.CancelTcpIpForward:
                    this.Write(this.AddressToBind);
                    this.Write(this.PortToBind);
                    break;
            }
        }
    }
}
