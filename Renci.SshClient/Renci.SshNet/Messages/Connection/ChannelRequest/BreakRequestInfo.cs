using System;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "break" type channel request information
    /// </summary>
    internal class BreakRequestInfo : RequestInfo
    {
        /// <summary>
        /// Channel request name
        /// </summary>
        public const string NAME = "break";

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
        /// Gets break length in milliseconds.
        /// </summary>
        public UInt32 BreakLength { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecRequestInfo"/> class.
        /// </summary>
        public BreakRequestInfo()
        {
            this.WantReply = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecRequestInfo"/> class.
        /// </summary>
        /// <param name="breakLength">Length of the break.</param>
        public BreakRequestInfo(UInt32 breakLength)
            : this()
        {
            this.BreakLength = breakLength;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            this.BreakLength = this.ReadUInt32();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.BreakLength);
        }
    }
}
