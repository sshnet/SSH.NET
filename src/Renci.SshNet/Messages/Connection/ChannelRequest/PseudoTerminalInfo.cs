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
        public const string Name = "pty-req";

        /// <summary>
        /// Gets the name of the request.
        /// </summary>
        /// <value>
        /// The name of the request.
        /// </value>
        public override string RequestName
        {
            get { return Name; }
        }

        /// <summary>
        /// Gets or sets the value of the TERM environment variable (e.g., vt100).
        /// </summary>
        /// <value>
        /// The value of the TERM environment variable.
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
        /// <param name="environmentVariable">The <c>TERM</c> environment variable which a identifier for the text window’s capabilities.</param>
        /// <param name="columns">The terminal width in columns.</param>
        /// <param name="rows">The terminal width in rows.</param>
        /// <param name="width">The terminal height in pixels.</param>
        /// <param name="height">The terminal height in pixels.</param>
        /// <param name="terminalModeValues">The terminal mode values.</param>
        /// <remarks>
        /// <para>
        /// The <c>TERM</c> environment variable contains an identifier for the text window's capabilities.
        /// You can get a detailed list of these cababilities by using the ‘infocmp’ command.
        /// </para>
        /// <para>
        /// The column/row dimensions override the pixel dimensions(when nonzero). Pixel dimensions refer
        /// to the drawable area of the window.
        /// </para>
        /// </remarks>
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
            Write(PixelWidth);
            Write(PixelHeight);

            if (TerminalModeValues != null && TerminalModeValues.Count > 0)
            {
                // write total length of encoded terminal modes, which is 1 bytes for the opcode / terminal mode
                // and 4 bytes for the uint argument for each entry; the encoded terminal modes are terminated by
                // opcode TTY_OP_END (which is 1 byte)
                Write((uint) TerminalModeValues.Count*(1 + 4) + 1);

                foreach (var item in TerminalModeValues)
                {
                    Write((byte) item.Key);
                    Write(item.Value);
                }

                Write((byte) TerminalModes.TTY_OP_END);
            }
            else
            {
                // when there are no terminal mode, the length of the string is zero
                Write((uint) 0);
            }
        }
    }
}
