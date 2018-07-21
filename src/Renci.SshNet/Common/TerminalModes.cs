namespace Renci.SshNet.Common
{
    /// <summary>
    /// Specifies the initial assignments of the opcode values that are used in the 'encoded terminal modes' valu
    /// </summary>
    public enum TerminalModes : byte
    {
        /// <summary>
        /// Indicates end of options.
        /// </summary> 
        TTY_OP_END = 0,
        
        /// <summary>
        /// Interrupt character; 255 if none.  Similarly for the other characters.  Not all of these characters are supported on all systems.
        /// </summary> 
        VINTR = 1,

        /// <summary>
        /// The quit character (sends SIGQUIT signal on POSIX systems).
        /// </summary> 
        VQUIT = 2,
        
        /// <summary>
        /// Erase the character to left of the cursor. 
        /// </summary>
        VERASE = 3,

        /// <summary>
        /// Kill the current input line.
        /// </summary>
        VKILL = 4,

        /// <summary>
        /// End-of-file character (sends EOF from the terminal).
        /// </summary>
        VEOF = 5,
        
        /// <summary>
        /// End-of-line character in addition to carriage return and/or linefeed.
        /// </summary>
        VEOL = 6,
        
        /// <summary>
        /// Additional end-of-line character.
        /// </summary>
        VEOL2 = 7,
        
        /// <summary>
        /// Continues paused output (normally control-Q).
        /// </summary>
        VSTART = 8,
        
        /// <summary>
        /// Pauses output (normally control-S).
        /// </summary>
        VSTOP = 9,
        
        /// <summary>
        /// Suspends the current program.
        /// </summary>
        VSUSP = 10,
        
        /// <summary>
        /// Another suspend character.
        /// </summary>
        VDSUSP = 11,

        /// <summary>
        /// Reprints the current input line.
        /// </summary>
        VREPRINT = 12,

        /// <summary>
        /// Erases a word left of cursor.
        /// </summary>
        VWERASE = 13,

        /// <summary>
        /// Enter the next character typed literally, even if it is a special character
        /// </summary>
        VLNEXT = 14,

        /// <summary>
        /// Character to flush output.
        /// </summary>
        VFLUSH = 15,

        /// <summary>
        /// Switch to a different shell layer.
        /// </summary>
        VSWTCH = 16,

        /// <summary>
        /// Prints system status line (load, command, pid, etc).
        /// </summary>
        VSTATUS = 17,

        /// <summary>
        /// Toggles the flushing of terminal output.
        /// </summary>
        VDISCARD = 18,

        /// <summary>
        /// The ignore parity flag.  The parameter SHOULD be 0 if this flag is FALSE, and 1 if it is TRUE.
        /// </summary>
        IGNPAR = 30,

        /// <summary>
        /// Mark parity and framing errors.
        /// </summary>
        PARMRK = 31,

        /// <summary>
        /// Enable checking of parity errors.
        /// </summary>
        INPCK = 32,

        /// <summary>
        /// Strip 8th bit off characters.
        /// </summary>
        ISTRIP = 33,

        /// <summary>
        /// Map NL into CR on input.
        /// </summary>
        INLCR = 34,

        /// <summary>
        /// Ignore CR on input.
        /// </summary>
        IGNCR = 35,

        /// <summary>
        /// Map CR to NL on input.
        /// </summary>
        ICRNL = 36,

        /// <summary>
        /// Translate uppercase characters to lowercase.
        /// </summary>
        IUCLC = 37,

        /// <summary>
        /// Enable output flow control.
        /// </summary>
        IXON = 38,

        /// <summary>
        /// Any char will restart after stop.
        /// </summary>
        IXANY = 39,

        /// <summary>
        /// Enable input flow control.
        /// </summary>
        IXOFF = 40,

        /// <summary>
        /// Ring bell on input queue full.
        /// </summary>
        IMAXBEL = 41,

        /// <summary>
        /// Terminal input and output is assumed to be encoded in UTF-8.
        /// </summary>
        IUTF8 = 42,

        /// <summary>
        /// Enable signals INTR, QUIT, [D]SUSP.
        /// </summary>
        ISIG = 50,

        /// <summary>
        /// Canonicalize input lines.
        /// </summary>
        ICANON = 51,

        /// <summary>
        /// Enable input and output of uppercase characters by preceding their lowercase equivalents with "\".
        /// </summary>
        XCASE = 52,

        /// <summary>
        /// Enable echoing.
        /// </summary>
        ECHO = 53,

        /// <summary>
        /// Visually erase chars.
        /// </summary>
        ECHOE = 54,

        /// <summary>
        /// Kill character discards current line.
        /// </summary>
        ECHOK = 55,

        /// <summary>
        /// Echo NL even if ECHO is off.
        /// </summary>
        ECHONL = 56,

        /// <summary>
        /// Don't flush after interrupt.
        /// </summary>
        NOFLSH = 57,

        /// <summary>
        /// Stop background jobs from output.
        /// </summary>
        TOSTOP = 58,

        /// <summary>
        /// Enable extensions.
        /// </summary>
        IEXTEN = 59,

        /// <summary>
        /// Echo control characters as ^(Char).
        /// </summary>
        ECHOCTL = 60,

        /// <summary>
        /// Visual erase for line kill.
        /// </summary>
        ECHOKE = 61,

        /// <summary>
        /// Retype pending input.
        /// </summary>
        PENDIN = 62,

        /// <summary>
        /// Enable output processing.
        /// </summary>
        OPOST = 70,

        /// <summary>
        /// Convert lowercase to uppercase.
        /// </summary>
        OLCUC = 71,

        /// <summary>
        /// Map NL to CR-NL.
        /// </summary>
        ONLCR = 72,

        /// <summary>
        /// Translate carriage return to newline (output).
        /// </summary>
        OCRNL = 73,

        /// <summary>
        /// Translate newline to carriage return-newline (output).
        /// </summary>
        ONOCR = 74,

        /// <summary>
        /// Newline performs a carriage return (output).
        /// </summary>
        ONLRET = 75,

        /// <summary>
        /// 7 bit mode.
        /// </summary>
        CS7 = 90,

        /// <summary>
        /// 8 bit mode.
        /// </summary>
        CS8 = 91,

        /// <summary>
        /// Parity enable.
        /// </summary>
        PARENB = 92,

        /// <summary>
        /// Odd parity, else even.
        /// </summary>
        PARODD = 93,

        /// <summary>
        /// Specifies the input baud rate in bits per second.
        /// </summary>
        TTY_OP_ISPEED = 128,

        /// <summary>
        /// Specifies the output baud rate in bits per second.
        /// </summary>
        TTY_OP_OSPEED = 129,
    }
}
