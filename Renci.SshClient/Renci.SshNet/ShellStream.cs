using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using System.Threading;
using System.Text.RegularExpressions;

namespace Renci.SshNet
{
    /// <summary>
    /// Contains operation for working with SSH Shell.
    /// </summary>
    public class ShellStream : Stream
    {
        //  TODO:   Replace reading into StringBuilder with reading into Stack or byte array for better efficiency.

        private readonly Session _session;

        private ChannelSession _channel;

        private Encoding _encoding;

        private StringBuilder _dataBuffer;

        /// <summary>
        /// Occurs when data was received.
        /// </summary>
        public event EventHandler<ShellDataEventArgs> DataReceived;

        /// <summary>
        /// Occurs when an error occurred.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ErrorOccurred;

        /// <summary>
        /// Gets a value that indicates whether data is available on the <see cref="ShellStream"/> to be read.
        /// </summary>
        /// <value>
        ///   <c>true</c> if data is available to be read; otherwise, <c>false</c>.
        /// </value>
        public bool DataAvailable
        {
            get
            {
                return this._dataBuffer.Length > 0;
            }
        }

        internal ShellStream(Session session, string terminalName, uint columns, uint rows, uint width, uint height, int maxLines, params KeyValuePair<TerminalModes, uint>[] terminalModeValues)
        {
            this._dataBuffer = new StringBuilder();
            this._encoding = new Renci.SshNet.Common.ASCIIEncoding();
            this._session = session;

            this._channel = this._session.CreateChannel<ChannelSession>();
            this._channel.DataReceived += new EventHandler<ChannelDataEventArgs>(Channel_DataReceived);
            this._channel.Closed += new EventHandler<ChannelEventArgs>(Channel_Closed);
            this._session.Disconnected += new EventHandler<EventArgs>(Session_Disconnected);
            this._session.ErrorOccured += new EventHandler<ExceptionEventArgs>(Session_ErrorOccured);

            this._channel.Open();
            this._channel.SendPseudoTerminalRequest(terminalName, columns, rows, width, height, terminalModeValues);
            this._channel.SendShellRequest();
        }

        #region Stream overide methods

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        public override bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>true if the stream supports seeking; otherwise, false.</returns>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>true if the stream supports writing; otherwise, false.</returns>
        public override bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        ///   
        /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Length
        {
            get { return this._dataBuffer.Length; }
        }

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        /// <returns>The current position within the stream.</returns>
        ///   
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///   
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Position
        {
            get { return 0; }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length. </exception>
        ///   
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="buffer"/> is null. </exception>
        ///   
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="offset"/> or <paramref name="count"/> is negative. </exception>
        ///   
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///   
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            var data = this._encoding.GetBytes(this._dataBuffer.ToString().ToCharArray(), 0, Math.Min(count, this._dataBuffer.Length));

            if (data.Length > 0)
            {
                Array.Copy(data, 0, buffer, offset, data.Length);
                this._dataBuffer.Remove(0, data.Length);
            }

            return data.Length;
        }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///   
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///   
        /// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length. </exception>
        ///   
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="buffer"/> is null. </exception>
        ///   
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="offset"/> or <paramref name="count"/> is negative. </exception>
        ///   
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        ///   
        /// <exception cref="T:System.NotSupportedException">The stream does not support writing. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            this._channel.SendData(buffer.Skip(offset).Take(count).ToArray());
        }

        #endregion

        /// <summary>
        /// Expects the specified expression and performs action when one is found.
        /// </summary>
        /// <param name="expectActions">The expected expressions and actions to perform.</param>
        public void Expect(params ExpectAction[] expectActions)
        {
            var expectedFound = false;

            lock (this._dataBuffer)
            {
                do
                {
                    if (this._dataBuffer.Length > 0)
                    {
                        foreach (var expectAction in expectActions)
                        {
                            var match = expectAction.Expect.Match(this._dataBuffer.ToString());
                            if (match.Success)
                            {
                                var result = this._dataBuffer.ToString().Substring(0, match.Index + match.Length);

                                //  Clean buffer up to expected text
                                this._dataBuffer.Remove(0, match.Index + match.Length);

                                expectAction.Action(result);
                                expectedFound = true;
                            }
                        }

                        if (!expectedFound)
                            Monitor.Wait(this._dataBuffer);
                    }
                    else
                    {
                        Monitor.Wait(this._dataBuffer);
                    }
                }
                while (!expectedFound);
            }
        }

        /// <summary>
        /// Expects the expression specified by text.
        /// </summary>
        /// <param name="text">The text to expect.</param>
        /// <returns></returns>
        public string Expect(string text)
        {
            return this.Expect(new Regex(Regex.Escape(text)));
        }

        /// <summary>
        /// Expects the expression specified by regular expression.
        /// </summary>
        /// <param name="regex">The regular expresssion to expect.</param>
        /// <returns>Text available in the shell that contains all the text that ends with expected expression.</returns>
        public string Expect(Regex regex)
        {
            var result = string.Empty;
            lock (this._dataBuffer)
            {
                var match = regex.Match(this._dataBuffer.ToString());
                while (!match.Success)
                {
                    Monitor.Wait(this._dataBuffer);
                    match = regex.Match(this._dataBuffer.ToString());
                }

                result = this._dataBuffer.ToString().Substring(0, match.Index + match.Length);

                //  Clean buffer up to expected text
                this._dataBuffer.Remove(0, match.Index + match.Length);
            }

            return result;
        }

        /// <summary>
        /// Reads the line from the shell. If line is not available it will block the execution and will wait for new line.
        /// </summary>
        /// <returns></returns>
        public string ReadLine()
        {
            string result = string.Empty;
            lock (this._dataBuffer)
            {
                var index = this._dataBuffer.ToString().IndexOf("\r\n");
                while (index < 0)
                {
                    Monitor.Wait(this._dataBuffer);
                    index = this._dataBuffer.ToString().IndexOf("\r\n");
                }

                result = this._dataBuffer.ToString().Substring(0, index);

                this._dataBuffer.Remove(0, index);
            }
            return result;
        }

        /// <summary>
        /// Reads text available in the shell.
        /// </summary>
        /// <returns></returns>
        public string Read()
        {
            string result = this._dataBuffer.ToString();
            lock (this._dataBuffer)
            {
                this._dataBuffer.Length = 0;
            }
            return result;
        }

        /// <summary>
        /// Writes the specified text to the shell.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Write(string text)
        {
            var data = this._encoding.GetBytes(text);
            this._channel.SendData(data);
        }

        /// <summary>
        /// Writes the line to the shell.
        /// </summary>
        /// <param name="line">The line.</param>
        public void WriteLine(string line)
        {
            var commandText = string.Format("{0}{1}", line, "\r\n");
            this.Write(commandText);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (this._channel != null)
            {
                this._channel.Dispose();
                this._channel = null;
            }
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            this.OnRaiseError(e);
        }

        private void Session_Disconnected(object sender, EventArgs e)
        {
            //  If channel is open then close it to cause Channel_Closed method to be called
            if (this._channel != null && this._channel.IsOpen)
            {
                this._channel.SendEof();

                this._channel.Close();
            }
        }

        private void Channel_Closed(object sender, ChannelEventArgs e)
        {
            //  TODO:   Do we need to call dispose here ??
            this.Dispose();
        }

        private void Channel_DataReceived(object sender, ChannelDataEventArgs e)
        {
            lock (this._dataBuffer)
            {
                this._dataBuffer.Append(this._encoding.GetString(e.Data, 0, e.Data.Length));

                Monitor.Pulse(this._dataBuffer);
            }

            this.OnDataReceived(e.Data);
        }

        private void OnRaiseError(ExceptionEventArgs e)
        {
            if (this.ErrorOccurred != null)
                this.ErrorOccurred(this, e);
        }

        private void OnDataReceived(byte[] data)
        {
            if (this.DataReceived != null)
                this.DataReceived(this, new ShellDataEventArgs(data));
        }
    }
}
