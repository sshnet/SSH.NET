using System;
using Renci.SshNet.Common;

namespace Renci.SshNet.Messages.Authentication
{
    /// <summary>
    /// Represents SSH_MSG_USERAUTH_REQUEST message. Server as a base message for other user authentication requests.
    /// </summary>
    [Message("SSH_MSG_USERAUTH_REQUEST", AuthenticationMessageCode)]
    public abstract class RequestMessage : Message
    {
        /// <summary>
        /// Returns the authentication message code for <c>SSH_MSG_USERAUTH_REQUEST</c>.
        /// </summary>
        internal const int AuthenticationMessageCode = 50;

        private readonly byte[] _serviceName;
        private readonly byte[] _userName;
        private readonly byte[] _methodNameBytes;
        private readonly string _methodName;

        /// <summary>
        /// Gets authentication username as UTF-8 encoded byte array.
        /// </summary>
        public byte[] Username
        {
            get { return _userName; }
        }

        /// <summary>
        /// Gets the name of the service as ASCII encoded byte array.
        /// </summary>
        /// <value>
        /// The name of the service.
        /// </value>
        public byte[] ServiceName
        {
            get { return _serviceName; }
        }

        /// <summary>
        /// Gets the name of the authentication method.
        /// </summary>
        /// <value>
        /// The name of the method.
        /// </value>
        public virtual string MethodName
        {
            get { return _methodName; }
        }

        /// <summary>
        /// Gets the size of the message in bytes.
        /// </summary>
        /// <value>
        /// The size of the messages in bytes.
        /// </value>
        protected override int BufferCapacity
        {
            get
            {
                var capacity = base.BufferCapacity;
                capacity += 4; // Username length
                capacity += Username.Length; // Username
                capacity += 4; // ServiceName length
                capacity += ServiceName.Length; // ServiceName
                capacity += 4; // MethodName length
                capacity += _methodNameBytes.Length; // MethodName
                return capacity;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestMessage"/> class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="username">Authentication username.</param>
        /// <param name="methodName">The name of the authentication method.</param>
        protected RequestMessage(ServiceName serviceName, string username, string methodName)
        {
            _serviceName = serviceName.ToArray();
            _userName = Utf8.GetBytes(username);
            _methodNameBytes = Ascii.GetBytes(methodName);
            _methodName = methodName;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            throw new InvalidOperationException("Load data is not supported.");
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            WriteBinaryString(_userName);
            WriteBinaryString(_serviceName);
            WriteBinaryString(_methodNameBytes);
        }

        internal override void Process(Session session)
        {
            throw new NotImplementedException();
        }
    }
}

