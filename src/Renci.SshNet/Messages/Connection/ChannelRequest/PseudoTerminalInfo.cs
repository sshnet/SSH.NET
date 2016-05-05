using Renci.SshNet.Common;
using System.Collections.Generic;

namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "pty-req" type channel request information
    /// </summary>
    internal class PseudoTerminalRequestInfo : RequestInfo
    {
        /// <summary>
        /// Channel request name
        /// </summary>
        public const string NAME = "pty-req";

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
        /// Gets or sets the environment variable (e.g., vt100).
        /// </summary>
        /// <value>
        /// The environment variable.
        /// </value>
        public string EnvironmentVariable { get; set; }

        /// <summary>
        /// Gets or sets the terminal width in columns (e.g., 80).
        /// </summary>
        /// <value>
        /// The terminal width in columns.
        /// </value>
        public uint Columns { get; set; }

        /// <summary>
        /// Gets or sets the terminal width in rows (e.g., 24).
        /// </summary>
        /// <value>
        /// The terminal width in rows.
        /// </value>
        public uint Rows { get; set; }

        /// <summary>
        /// Gets or sets the terminal width in pixels (e.g., 640).
        /// </summary>
        /// <value>
        /// The terminal width in pixels.
        /// </value>
        public uint PixelWidth { get; set; }

        /// <summary>
        /// Gets or sets the terminal height in pixels (e.g., 480).
        /// </summary>
        /// <value>
        /// The terminal height in pixels.
        /// </value>
        public uint PixelHeight { get; set; }

        /// <summary>
        /// Gets or sets the terminal mode.
        /// </summary>
        /// <value>
        /// The terminal mode.
        /// </value>
        public IDictionary<TerminalModes, uint> TerminalModeValues { get; set; }

        /// <summary>
        /// Gets the size of the message in bytes.
        /// </summary>
        /// <value>
        /// <c>-1</c> to indicate that the size of the message cannot be determined,
        /// or is too costly to calculate.
        /// </value>
        protected override int BufferCapacity
        {
            get { return -1; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PseudoTerminalRequestInfo"/> class.
        /// </summary>
        public PseudoTerminalRequestInfo()
        {
            WantReply = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PseudoTerminalRequestInfo"/> class.
        /// </summary>
        /// <param name="environmentVariable">The environment variable.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="terminalModeValues">The terminal mode values.</param>
        public PseudoTerminalRequestInfo(string environmentVariable, uint columns, uint rows, uint width, uint height, IDictionary<TerminalModes, uint> terminalModeValues)
            : this()
        {
            EnvironmentVariable = environmentVariable;
            Columns = columns;
            Rows = rows;
            PixelWidth = width;
            PixelHeight = height;
            TerminalModeValues = terminalModeValues;
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            Write(EnvironmentVariable);
            Write(Columns);
            Write(Rows);
            Write(Rows);
            Write(PixelHeight);

            if (TerminalModeValues != null)
            {
                Write((uint)TerminalModeValues.Count * (1 + 4) + 1);

                foreach (var item in TerminalModeValues)
                {
                    Write((byte)item.Key);
                    Write(item.Value);
                }
                Write((byte)0);
            }
            else
            {
                Write((uint)0);
            }
        }
    }
}
