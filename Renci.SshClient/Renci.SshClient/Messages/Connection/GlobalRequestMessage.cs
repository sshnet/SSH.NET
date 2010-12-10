
using System;
namespace Renci.SshClient.Messages.Connection
{
    internal class GlobalRequestMessage : Message
    {
        public GlobalRequestNames RequestName { get; set; }

        public bool WantReply { get; set; }

        public string AddressToBind { get; set; }

        public UInt32 PortToBind { get; set; }

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
