using System.Collections.Generic;
using System.Linq;
using Renci.SshClient.Common;

namespace Renci.SshClient.Messages
{
    public abstract class Message : SshData
    {
        internal static T Load<T>(IEnumerable<byte> data) where T : Message, new()
        {
            var messageType = data.FirstOrDefault();

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
            var messageAttribute = this.GetType().GetCustomAttributes(typeof(MessageAttribute), true).SingleOrDefault() as MessageAttribute;

            if (messageAttribute == null)
                throw new SshException(string.Format("Type '{0}' is not a valid message type.", this.GetType().AssemblyQualifiedName));

            var data = new List<byte>(base.GetBytes());

            data.Insert(0, messageAttribute.Number);

            return data;
        }

        public override string ToString()
        {
            var messageAttribute = this.GetType().GetCustomAttributes(typeof(MessageAttribute), true).SingleOrDefault() as MessageAttribute;

            if (messageAttribute == null)
                throw new SshException(string.Format("Type '{0}' is not a valid message type.", this.GetType().AssemblyQualifiedName));

            return messageAttribute.Name;
        }
    }
}
