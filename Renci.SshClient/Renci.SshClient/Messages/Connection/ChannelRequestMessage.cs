
namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelRequestMessage : ChannelMessage
    {
        public override MessageTypes MessageType
        {
            get { return MessageTypes.ChannelRequest; }
        }

        public ChannelRequestNames RequestName { get; set; }

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
                    this.RequestName = ChannelRequestNames.Exec;
                    this.WantReply = this.ReadBoolean();
                    this.Command = this.ReadString();
                    break;
                case "subsystem":
                    this.RequestName = ChannelRequestNames.Subsystem;
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
                    this.RequestName = ChannelRequestNames.ExitStatus;
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
                case ChannelRequestNames.PseudoTerminal:
                    break;
                case ChannelRequestNames.X11Forwarding:
                    break;
                case ChannelRequestNames.EnvironmentVariable:
                    break;
                case ChannelRequestNames.Shell:
                    break;
                case ChannelRequestNames.Exec:
                    this.Write("exec");
                    this.Write(this.WantReply);
                    this.Write(this.Command);
                    break;
                case ChannelRequestNames.Subsystem:
                    this.Write("subsystem");
                    this.Write(this.WantReply);
                    this.Write(this.SubsystemName);
                    break;
                case ChannelRequestNames.WindowChange:
                    break;
                case ChannelRequestNames.XonXoff:
                    break;
                case ChannelRequestNames.Signal:
                    break;
                case ChannelRequestNames.ExitStatus:
                    this.Write("exit-status");
                    this.Write(this.WantReply);
                    this.Write(this.ExitStatus);
                    break;
                case ChannelRequestNames.ExitSignal:
                    break;
                default:
                    break;
            }
        }
    }
}
