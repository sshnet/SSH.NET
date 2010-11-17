﻿
namespace Renci.SshClient.Messages.Connection
{
    internal class ChannelRequestEnvironmentVariableMessage : ChannelRequestMessage
    {
        public const string REQUEST_NAME = "env";

        public string VariableName { get; set; }

        public string VariableValue { get; set; }

        protected override void LoadData()
        {
            base.LoadData();

            this.VariableName = this.ReadString();
            this.VariableValue = this.ReadString();
        }

        protected override void SaveData()
        {
            this.RequestName = REQUEST_NAME;

            base.SaveData();

            this.Write(this.VariableName);
            this.Write(this.VariableValue);
        }
    }
}
