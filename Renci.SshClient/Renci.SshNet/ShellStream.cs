using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using System.Threading;

namespace Renci.SshNet
{
    public class ShellStream : Stream
    {
        private readonly Session _session;

        private ChannelSession _channel;

        private Queue<byte> _incoming;

        private Queue<byte> _outgoing;

        private int _bufferSize;

        private EventWaitHandle _expectedTextWaitHandle = new AutoResetEvent(false);

        private string _expectedText;

        private int _expectedIndex;

        private StringBuilder _expectedBuffer;

        /// <summary>
        /// Occurs when an error occurred.
        /// </summary>
        public event EventHandler<ExceptionEventArgs> ErrorOccurred;

        /// <summary>
        /// Gets a value that indicates whether data is available on the <see cref="ShellStream"/> to be read.
        /// </summary>
        /// <value>
        ///   <c>true</c> if can be read; otherwise, <c>false</c>.
        /// </value>
        public bool DataAvailable
        {
            get
            {
                lock (this._incoming)
                {
                    //Monitor.Wait(this._incoming);
                    var result = this._incoming.Count > 0;
                    //Monitor.Pulse(this._incoming);
                    return result;
                }
            }
        }

        public EventWaitHandle DataAvailableWaitHandle { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellStream"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <param name="terminalName">Name of the terminal.</param>
        /// <param name="columns">The columns.</param>
        /// <param name="rows">The rows.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <param name="terminalModeValues">The terminal mode values.</param>
        internal ShellStream(Session session, string terminalName, uint columns, uint rows, uint width, uint height, int bufferSize, params KeyValuePair<TerminalModes, uint>[] terminalModeValues)
        {
            this._session = session;

            this._incoming = new Queue<byte>(bufferSize);
            this._outgoing = new Queue<byte>(bufferSize);

            this.DataAvailableWaitHandle = new AutoResetEvent(false);

            this._bufferSize = bufferSize;

            this._channel = this._session.CreateChannel<ChannelSession>();
            this._channel.DataReceived += Channel_DataReceived;
            this._channel.Closed += Channel_Closed;
            this._session.Disconnected += Session_Disconnected;
            this._session.ErrorOccured += Session_ErrorOccured;

            this._channel.Open();
            this._channel.SendPseudoTerminalRequest(terminalName, columns, rows, width, height, terminalModeValues);
            this._channel.SendShellRequest();
        }

        public void Expect(string expected, TimeSpan timeout)
        {
            this._expectedText = expected;
            this._expectedIndex = 0;

            this._expectedTextWaitHandle.WaitOne(timeout);

            this._expectedText = null;
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
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        ///   
        /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception>
        ///   
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Length
        {
            get { return this._incoming.Count; }
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
            var read = 0;

            if (this._incoming.Count > 0)
            {
                lock (this._incoming)
                {
                    while (this._incoming.Count < 1)
                        Monitor.Wait(this._incoming);

                    for (read = 0; read < count && this._incoming.Count > 0; read++)
                    {
                        buffer[offset + read] = this._incoming.Dequeue();
                    }

                    Monitor.Pulse(this._incoming);
                }
            }
            return read;
        }

        /// <summary>
        /// Sets the position within the current stream.
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
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Sets the length of the current stream.
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
            for (int i = 0; i < count; i++)
            {
                this._outgoing.Enqueue(buffer[offset + i]);
                if (this._outgoing.Count >= this._bufferSize)
                {
                    this.Flush();
                }
            }
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Flush()
        {
            if (this._outgoing.Count > 0)
            {
                this._channel.SendData(this._outgoing.ToArray());
                this._outgoing.Clear();
            }
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
                if (this._channel.IsOpen)
                {
                    this._channel.SendEof();

                    this._channel.Close();
                }

                //  TODO:   Check why this._channel could be null here, had an exception thrown here
                this._channel.DataReceived -= Channel_DataReceived;
                this._channel.Closed -= Channel_Closed;

                this._channel = null;
            }

            this._session.Disconnected -= Session_Disconnected;
            this._session.ErrorOccured -= Session_ErrorOccured;

            if (this.DataAvailableWaitHandle != null)
            {
                this.DataAvailableWaitHandle.Dispose();
                this.DataAvailableWaitHandle = null;
            }
        }

        #endregion

        private void Channel_DataReceived(object sender, Common.ChannelDataEventArgs e)
        {
            lock (this._incoming)
            {
                // wait until the buffer isn't full
                while (this._incoming.Count >= this._bufferSize)
                    Monitor.Wait(this._incoming);

                for (int i = 0; i < e.Data.Length; i++)
                {
                    if (this._expectedText != null)
                    {
                        if (this._expectedText[_expectedIndex] == e.Data[i])
                        {
                            //  Expected character found
                            this._expectedIndex++;

                            //  Check if expected text is completely found
                            if (this._expectedIndex == this._expectedText.Length)
                            {
                                this._expectedTextWaitHandle.Set();
                            }
                        }
                        else
                        {
                            //  Still waiting for first expected character
                            this._expectedIndex = 0;
                        }
                    }
                    this._incoming.Enqueue(e.Data[i]);
                }

                Monitor.Pulse(this._incoming); // signal that write has occurred
            }

            this.DataAvailableWaitHandle.Set();

        }

        private void Channel_Closed(object sender, Common.ChannelEventArgs e)
        {
            this.Dispose();
        }

        private void Session_Disconnected(object sender, System.EventArgs e)
        {
            //  If channel is open then close it to cause Channel_Closed method to be called
            if (this._channel != null && this._channel.IsOpen)
            {
                this._channel.SendEof();

                this._channel.Close();
            }
        }

        private void Session_ErrorOccured(object sender, ExceptionEventArgs e)
        {
            this.RaiseError(e);
        }

        private void RaiseError(ExceptionEventArgs e)
        {
            if (this.ErrorOccurred != null)
            {
                this.ErrorOccurred(this, e);
            }
        }
    }
}
