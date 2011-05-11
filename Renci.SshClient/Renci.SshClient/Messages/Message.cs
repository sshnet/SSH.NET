using System.Collections.Generic;
using System.Linq;
using Renci.SshClient.Common;
using System.Globalization;

namespace Renci.SshClient.Messages
{
    /// <summary>
    /// Base class for all SSH protocol messages
    /// </summary>
    public abstract class Message : SshData
    {
        /// <summary>
        /// Loads the specified data.
        /// </summary>
        /// <typeparam name="T">SSH message type</typeparam>
        /// <param name="data">Message data.</param>
        /// <returns>SSH message object</returns>
        internal static T Load<T>(byte[] data) where T : Message, new()
        {
            T message = new T();

            message.LoadBytes(data);

            message.ResetReader();

            message.LoadData();

            return message;
        }

        /// <summary>
        /// Gets the index that represents zero in current data type.
        /// </summary>
        /// <value>
        /// The index of the zero reader.
        /// </value>
        protected override int ZeroReaderIndex
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Gets data bytes array
        /// </summary>
        /// <returns></returns>
        public override byte[] GetBytes()
        {
            var messageAttribute = this.GetType().GetCustomAttributes(typeof(MessageAttribute), true).SingleOrDefault() as MessageAttribute;

            if (messageAttribute == null)
                throw new SshException(string.Format(CultureInfo.CurrentCulture, "Type '{0}' is not a valid message type.", this.GetType().AssemblyQualifiedName));

            var data = new List<byte>(base.GetBytes());

            data.Insert(0, messageAttribute.Number);

            return data.ToArray();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var messageAttribute = this.GetType().GetCustomAttributes(typeof(MessageAttribute), true).SingleOrDefault() as MessageAttribute;

            if (messageAttribute == null)
                return string.Format(CultureInfo.CurrentCulture, "'{0}' without Message attribute.", this.GetType().FullName);

            return messageAttribute.Name;
        }
    }
}