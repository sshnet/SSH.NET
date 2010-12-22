using Renci.SshClient.Common;

namespace Renci.SshClient.Messages.Connection
{
    public abstract class RequestInfo : SshData
    {
        public abstract string RequestName { get; }

        public virtual bool WantReply { get; protected set; }

        protected override void LoadData()
        {
            this.WantReply = this.ReadBoolean();
        }

        protected override void SaveData()
        {
            this.Write(this.WantReply);
        }
    }
}
