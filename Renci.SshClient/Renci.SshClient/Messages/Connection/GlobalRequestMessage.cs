
namespace Renci.SshClient.Messages.Connection
{
    internal class GlobalRequestMessage : Message
    {
        public GlobalRequestNames RequestName { get; set; }

        public bool WantReply { get; set; }

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
                    this.RequestName = GlobalRequestNames.TcpipForward;
                    break;
                case "cancel-tcpip-forward":
                    this.RequestName = GlobalRequestNames.CancelTcpipForward;
                    break;
                default:
                    break;
            }

            this.WantReply = this.ReadBoolean();
        }

        protected override void SaveData()
        {
            switch (this.RequestName)
            {
                case GlobalRequestNames.TcpipForward:
                    this.Write("tcpip-forward");
                    break;
                case GlobalRequestNames.CancelTcpipForward:
                    this.Write("cancel-tcpip-forward");
                    break;
                default:
                    break;
            }
            this.Write(this.WantReply);
        }
    }
}
