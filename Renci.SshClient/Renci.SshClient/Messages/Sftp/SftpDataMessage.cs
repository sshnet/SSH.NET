using System;
using System.Collections.Generic;
using System.Linq;
using Renci.SshClient.Messages.Connection;

namespace Renci.SshClient.Messages.Sftp
{
    internal class SftpDataMessage : ChannelDataMessage
    {
        //public override MessageTypes MessageType
        //{
        //    get { return MessageTypes.ChannelData; }
        //}

        private SftpMessage _message;
        public SftpMessage Message
        {
            get
            {
                return this._message;
            }
            set
            {
                this._message = value;
                var messageData = this._message.GetBytes();
                List<byte> data = new List<byte>();
                data.AddRange(BitConverter.GetBytes((uint)messageData.Count()).Reverse());
                data.AddRange(messageData);
                this.Data = data.GetSshString();
            }
        }

        //protected override void SaveData()
        //{
        //    base.SaveData();
        //    var data = this.Message.GetBytes();
        //    //this.Write((uint)data.Count() + 4);
        //    //this.Write(data.GetSshString());

        //    List<byte> data1 = new List<byte>();
        //    data1.AddRange(BitConverter.GetBytes((uint)data.Count()).Reverse());
        //    data1.AddRange(data);
        //    this.Write(data1.GetSshString());
        //}
    }
}
