
namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelRequestMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelRequest; }
        }

        public RequestNames RequestName { get; set; }

        public bool WantReply { get; set; }

        public string Command { get; set; }

        public string SubsystemName { get; set; }

        public uint ExitStatus { get; set; }

        protected override void LoadData()
        {
            base.LoadData();

            var requestName = this.ReadString();
            switch (requestName)
            {
                case "pty-req":
                    break;
                case "x11-req":
                    break;
                case "env":
                    break;
                case "shell":
                    break;
                case "exec":
                    this.RequestName = RequestNames.Exec;
                    this.WantReply = this.ReadBoolean();
                    this.Command = this.ReadString();
                    break;
                case "subsystem":
                    this.RequestName = RequestNames.Subsystem;
                    this.WantReply = this.ReadBoolean();
                    this.SubsystemName = this.ReadString();
                    break;
                case "window-change":
                    break;
                case "xon-xoff":
                    break;
                case "signal":
                    break;
                case "exit-status":
                    this.RequestName = RequestNames.ExitStatus;
                    this.WantReply = this.ReadBoolean();
                    this.ExitStatus = this.ReadUInt32();
                    break;
                case "exit-signal":
                    break;
                default:
                    break;
            }
        }

        protected override void SaveData()
        {
            base.SaveData();

            switch (this.RequestName)
            {
                case RequestNames.PseudoTerminal:
                    break;
                case RequestNames.X11Forwarding:
                    break;
                case RequestNames.EnvironmentVariable:
                    break;
                case RequestNames.Shell:
                    break;
                case RequestNames.Exec:
                    this.Write("exec");
                    this.Write(this.WantReply);
                    this.Write(this.Command);
                    break;
                case RequestNames.Subsystem:
                    this.Write("subsystem");
                    this.Write(this.WantReply);
                    this.Write(this.SubsystemName);
                    break;
                case RequestNames.WindowChange:
                    break;
                case RequestNames.XonXoff:
                    break;
                case RequestNames.Signal:
                    break;
                case RequestNames.ExitStatus:
                    this.Write("exit-status");
                    this.Write(this.WantReply);
                    this.Write(this.ExitStatus);
                    break;
                case RequestNames.ExitSignal:
                    break;
                default:
                    break;
            }
        }
    }
}
