using System.Collections.Generic;
using System.Linq;
using Renci.SshClient.Common;

namespace Renci.SshClient.Messages
{
    public abstract class Message : SshData
    {
        public abstract MessageTypes MessageType { get; }

        internal static T Load<T>(IEnumerable<byte> data) where T : Message, new()
        {
            var messageType = (MessageTypes)data.FirstOrDefault();

            T message = new T();

            message.LoadBytes(data);

            message.ResetReader();

            message.LoadData();

            return message;
        }

        protected override int ZeroReaderIndex
        {
            get
            {
                return 1;
            }
        }

        public override IEnumerable<byte> GetBytes()
        {
            var data = new List<byte>(base.GetBytes());

            data.Insert(0, (byte)this.MessageType);

            return data;
        }

        public override string ToString()
        {
            return this.MessageType.ToString();
        }

    }
}
