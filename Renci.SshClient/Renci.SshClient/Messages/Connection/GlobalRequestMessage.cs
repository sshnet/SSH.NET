using System;

namespace Renci.SshClient.Messages.Connection
{
    public class GlobalRequestMessage : Message
    {
        public GlobalRequestNames RequestName { get; private set; }

        public bool WantReply { get; private set; }

        public string AddressToBind { get; private set; }

        public UInt32 PortToBind { get; private set; }

        public GlobalRequestMessage()
        {

        }

        public GlobalRequestMessage(GlobalRequestNames requestName, bool wantReply)
        {
            this.RequestName = requestName;
            this.WantReply = wantReply;
        }

        public GlobalRequestMessage(GlobalRequestNames requestName, bool wantReply, string addressToBind, uint portToBind)
            : this(requestName, wantReply)
        {
            this.AddressToBind = addressToBind;
            this.PortToBind = portToBind;
        }

        public override MessageTypes MessageType
        {
            get { return MessageTypes.GlobalRequest; }
        }

        protected override void LoadData()
        {
            var requestName = this.ReadString();
            switch (requestName)
            {
                case "tcpip-forward":
                    this.RequestName = GlobalRequestNames.TcpIpForward;
                    break;
                case "cancel-tcpip-forward":
                    this.RequestName = GlobalRequestNames.CancelTcpIpForward;
                    break;
                default:
                    break;
            }

            this.WantReply = this.ReadBoolean();
            this.AddressToBind = this.ReadString();
            this.PortToBind = this.ReadUInt32();
        }

        protected override void SaveData()
        {
            switch (this.RequestName)
            {
                case GlobalRequestNames.TcpIpForward:
                    this.Write("tcpip-forward");
                    break;
                case GlobalRequestNames.CancelTcpIpForward:
                    this.Write("cancel-tcpip-forward");
                    break;
                case GlobalRequestNames.KeepAlive:
                    this.Write("keep-alive-message-ignore-me");
                    break;
                default:
                    break;
            }

            this.Write(this.WantReply);

            switch (this.RequestName)
            {
                case GlobalRequestNames.TcpIpForward:
                case GlobalRequestNames.CancelTcpIpForward:
                    this.Write(this.AddressToBind);
                    this.Write(this.PortToBind);
                    break;
                case GlobalRequestNames.KeepAlive:
                    break;
                default:
                    break;
            }
        }
    }
}
