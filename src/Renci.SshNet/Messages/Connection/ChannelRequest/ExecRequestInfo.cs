using System;
using System.Text;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "exec" type channel request information
    /// </summary>
    internal class ExecRequestInfo : RequestInfo
    {
#if TUNING
        private byte[] _command;
#endif

        /// <summary>
        /// Channel request name
        /// </summary>
        public const string NAME = "exec";

        /// <summary>
        /// Gets the name of the request.
        /// </summary>
        /// <value>
        /// The name of the request.
        /// </value>
        public override string RequestName
        {
            get { return NAME; }
        }

        /// <summary>
        /// Gets command to execute.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
#if TUNING
        public string Command
        {
            get { return Encoding.GetString(_command, 0, _command.Length); }
        }
#else
        public string Command { get; private set; }
#endif

        /// <summary>
        /// Gets the encoding.
        /// </summary>
        /// <value>
        /// The encoding.
        /// </value>
        public Encoding Encoding { get; private set; }

#if TUNING
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
                capacity += 4; // Command length
                capacity += _command.Length; // Command
                return capacity;
            }
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecRequestInfo"/> class.
        /// </summary>
        public ExecRequestInfo()
        {
            WantReply = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecRequestInfo"/> class.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="command"/> or <paramref name="encoding"/> is null.</exception>
        public ExecRequestInfo(string command, Encoding encoding)
            : this()
        {
            if (command == null)
                throw new ArgumentNullException("command");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

#if TUNING
            _command = encoding.GetBytes(command);
#else
            Command = command;
#endif
            Encoding = encoding;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

#if TUNING
            _command = ReadBinary();
            Encoding = Utf8;
#else
            Command = ReadString();
#endif
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

#if TUNING
            WriteBinaryString(_command);
#else
            Write(Command, Encoding);
#endif
        }
    }
}
