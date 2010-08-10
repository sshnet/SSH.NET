using System;
using System.Collections.Generic;
using System.Linq;
using Renci.SshClient.Common;

namespace Renci.SshClient.Messages
{
    public delegate void SendMessageDelegate(Message message);

    public abstract class Message : SshData
    {
        private static object _lock = new object();

        private delegate T LoadFunc<out T>(IEnumerable<byte> data);

        public abstract MessageTypes MessageType { get; }

        private static IDictionary<MessageTypes, LoadFunc<Message>> _registeredMessageTypes = new Dictionary<MessageTypes, LoadFunc<Message>>();

        /// <summary>
        /// Registers the message type. This will allow message type to be recognized by and handled by the system.
        /// </summary>
        /// <remarks>Some message types are not allowed during cirtain times or same code can be used for different type of message</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="messageType">Type of the message.</param>
        public static void RegisterMessageType<T>(MessageTypes messageType) where T : Message, new()
        {
            lock (_lock)
            {
                if (Message._registeredMessageTypes.ContainsKey(messageType))
                {
                    Message.UnRegisterMessageType(messageType);
                }

                Message._registeredMessageTypes.Add(messageType, new LoadFunc<Message>(Load<T>));
            }
        }

        public static void UnRegisterMessageType(MessageTypes messageType)
        {
            Message._registeredMessageTypes.Remove(messageType);
        }

        public static Message Load(IEnumerable<byte> data)
        {
            var messageType = (MessageTypes)data.FirstOrDefault();

            return Load(data, messageType);
        }

        private static Message Load(IEnumerable<byte> data, MessageTypes messageType)
        {
            lock (_lock)
            {
                if (Message._registeredMessageTypes.ContainsKey(messageType))
                {
                    return Message._registeredMessageTypes[messageType](data);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Message type '{0}' is not registered.", messageType));
                }
            }
        }

        private static T Load<T>(IEnumerable<byte> data) where T : Message, new()
        {
            var messageType = (MessageTypes)data.FirstOrDefault();

            T message = new T();

            message.LoadBytes(data);

            message.ResetReader();

            message.LoadData();

            return message;
        }

        public override IEnumerable<byte> GetBytes()
        {
            var data = new List<byte>(base.GetBytes());

            data.Insert(0, (byte)this.MessageType);

            return data;
        }

    }
}
