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
    public partial class ShellStream : Stream
    {
        private readonly Session _session;

        private const int _bufferSize = 1024;

        private ChannelSession _channel;

        private readonly Encoding _encoding;

        private readonly Queue<byte> _incoming;

        private readonly Queue<byte> _outgoing;

        private AutoResetEvent _dataReceived = new AutoResetEvent(false);

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
                lock (this._incoming)
                {
                    return this._incoming.Count > 0;
                }
            }
        }

        internal ShellStream(Session session, string terminalName, uint columns, uint rows, uint width, uint height, int maxLines, IDictionary<TerminalModes, uint> terminalModeValues)
        {
            this._encoding = session.ConnectionInfo.Encoding;
            this._session = session;
            this._incoming = new Queue<byte>();
            this._outgoing = new Queue<byte>();

            this._channel = this._session.CreateClientChannel<ChannelSession>();
            this._channel.DataReceived += Channel_DataReceived;
            this._channel.Closed += Channel_Closed;
            this._session.Disconnected += Session_Disconnected;
            this._session.ErrorOccured += Session_ErrorOccured;

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
            if (this._channel == null)
            {
                throw new ObjectDisposedException("ShellStream");
            }
            this._channel.SendData(this._outgoing.ToArray());
            this._outgoing.Clear();
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
            get
            {
                lock (this._incoming)
                {
                    return this._incoming.Count;
                }
            }
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
            var i = 0;

            lock (this._incoming)
            {
                for (; i < count && this._incoming.Count > 0; i++)
                {
                    buffer[offset + i] = this._incoming.Dequeue();
                }
            }

            return i;
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
        public override long Seek(long offset, SeekOrigin origin)
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
            foreach (var b in buffer.Skip(offset).Take(count).ToArray())
            {
                if (this._outgoing.Count < _bufferSize)
                {
                    this._outgoing.Enqueue(b);
                    continue;
                }

                this.Flush();
            }
        }

        #endregion

        /// <summary>
        /// Expects the specified expression and performs action when one is found.
        /// </summary>
        /// <param name="expectActions">The expected expressions and actions to perform.</param>
        public void Expect(params ExpectAction[] expectActions)
        {
            this.Expect(TimeSpan.Zero, expectActions);
        }

        /// <summary>
        /// Expects the specified expression and performs action when one is found.
        /// </summary>
        /// <param name="timeout">Time to wait for input.</param>
        /// <param name="expectActions">The expected expressions and actions to perform, if the specified time elapsed and expected condition have not met, that method will exit without executing any action.</param>
        public void Expect(TimeSpan timeout, params ExpectAction[] expectActions)
        {
            var expectedFound = false;
            var text = string.Empty;

            do
            {
                lock (this._incoming)
                {
                    if (this._incoming.Count > 0)
                    {
                        text = this._encoding.GetString(this._incoming.ToArray(), 0, this._incoming.Count);
                    }

                    if (text.Length > 0)
                    {
                        foreach (var expectAction in expectActions)
                        {
                            var match = expectAction.Expect.Match(text);

                            if (match.Success)
                            {
                                var result = text.Substring(0, match.Index + match.Length);

                                for (int i = 0; i < match.Index + match.Length && this._incoming.Count > 0; i++)
                                {
                                    //  Remove processed items from the queue
                                    this._incoming.Dequeue();
                                }

                                expectAction.Action(result);
                                expectedFound = true;
                            }
                        }
                    }
                }

                if (!expectedFound)
                {
                    if (timeout.Ticks > 0)
                    {
                        if (!this._dataReceived.WaitOne(timeout))
                        {
                            return;
                        }
                    }
                    else
                    {
                        this._dataReceived.WaitOne();
                    }
                }
            }
            while (!expectedFound);
        }

        /// <summary>
        /// Begins the expect.
        /// </summary>
        /// <param name="expectActions">The expect actions.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        public IAsyncResult BeginExpect(params ExpectAction[] expectActions)
        {
            return this.BeginExpect(TimeSpan.Zero, null, null, expectActions);
        }

        /// <summary>
        /// Begins the expect.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="expectActions">The expect actions.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        public IAsyncResult BeginExpect(AsyncCallback callback, params ExpectAction[] expectActions)
        {
            return this.BeginExpect(TimeSpan.Zero, callback, null, expectActions);
        }

        /// <summary>
        /// Begins the expect.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        /// <param name="expectActions">The expect actions.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        public IAsyncResult BeginExpect(AsyncCallback callback, object state, params ExpectAction[] expectActions)
        {
            return this.BeginExpect(TimeSpan.Zero, callback, state, expectActions);
        }

        /// <summary>
        /// Begins the expect.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="callback">The callback.</param>
        /// <param name="state">The state.</param>
        /// <param name="expectActions">The expect actions.</param>
        /// <returns>
        /// An <see cref="IAsyncResult" /> that references the asynchronous operation.
        /// </returns>
        public IAsyncResult BeginExpect(TimeSpan timeout, AsyncCallback callback, object state, params ExpectAction[] expectActions)
        {
            var text = string.Empty;

            //  Create new AsyncResult object
            var asyncResult = new ExpectAsyncResult(callback, state);

            //  Execute callback on different thread                
            this.ExecuteThread(() =>
            {
                string expectActionResult = null;
                try
                {

                    do
                    {
                        lock (this._incoming)
                        {

                            if (this._incoming.Count > 0)
                            {
                                text = this._encoding.GetString(this._incoming.ToArray(), 0, this._incoming.Count);
                            }

                            if (text.Length > 0)
                            {
                                foreach (var expectAction in expectActions)
                                {
                                    var match = expectAction.Expect.Match(text);

                                    if (match.Success)
                                    {
                                        var result = text.Substring(0, match.Index + match.Length);

                                        for (int i = 0; i < match.Index + match.Length && this._incoming.Count > 0; i++)
                                        {
                                            //  Remove processed items from the queue
                                            this._incoming.Dequeue();
                                        }

                                        expectAction.Action(result);

                                        if (callback != null)
                                        {
                                            callback(asyncResult);
                                        }
                                        expectActionResult = result;
                                    }
                                }
                            }
                        }

                        if (expectActionResult != null)
                            break;

                        if (timeout.Ticks > 0)
                        {
                            if (!this._dataReceived.WaitOne(timeout))
                            {
                                if (callback != null)
                                {
                                    callback(asyncResult);
                                }
                                break;
                            }
                        }
                        else
                        {
                            this._dataReceived.WaitOne();
                        }
                    } while (true);

                    asyncResult.SetAsCompleted(expectActionResult, true);
                }
                catch (Exception exp)
                {
                    asyncResult.SetAsCompleted(exp, true);
                }
            });

            return asyncResult;
        }

        /// <summary>
        /// Ends the execute.
        /// </summary>
        /// <param name="asyncResult">The async result.</param>
        /// <exception cref="System.ArgumentException">Either the IAsyncResult object did not come from the corresponding async method on this type, or EndExecute was called multiple times with the same IAsyncResult.</exception>
        public string EndExpect(IAsyncResult asyncResult)
        {
            var ar = asyncResult as ExpectAsyncResult;

            if (ar == null || ar.EndInvokeCalled)
                throw new ArgumentException("Either the IAsyncResult object did not come from the corresponding async method on this type, or EndExecute was called multiple times with the same IAsyncResult.");

            // Wait for operation to complete, then return result or throw exception
            return ar.EndInvoke();
        }

        /// <summary>
        /// Expects the expression specified by text.
        /// </summary>
        /// <param name="text">The text to expect.</param>
        /// <returns>
        /// Text available in the shell that ends with expected text.
        /// </returns>
        public string Expect(string text)
        {
            return this.Expect(new Regex(Regex.Escape(text)), TimeSpan.FromMilliseconds(-1));
        }

        /// <summary>
        /// Expects the expression specified by text.
        /// </summary>
        /// <param name="text">The text to expect.</param>
        /// <param name="timeout">Time to wait for input.</param>
        /// <returns>
        /// Text available in the shell that ends with expected text, if the specified time elapsed returns null.
        /// </returns>
        public string Expect(string text, TimeSpan timeout)
        {
            return this.Expect(new Regex(Regex.Escape(text)), timeout);
        }

        /// <summary>
        /// Expects the expression specified by regular expression.
        /// </summary>
        /// <param name="regex">The regular expression to expect.</param>
        /// <returns>Text available in the shell that contains all the text that ends with expected expression.</returns>
        public string Expect(Regex regex)
        {
            return this.Expect(regex, TimeSpan.Zero);
        }

        /// <summary>
        /// Expects the expression specified by regular expression.
        /// </summary>
        /// <param name="regex">The regular expression to expect.</param>
        /// <param name="timeout">Time to wait for input.</param>
        /// <returns>
        /// Text available in the shell that contains all the text that ends with expected expression, if the specified time elapsed returns null.
        /// </returns>
        public string Expect(Regex regex, TimeSpan timeout)
        {
            //  TODO:   Refactor this method, will deda lock
            var text = string.Empty;

            while (true)
            {
                lock (this._incoming)
                {
                    if (this._incoming.Count > 0)
                    {
                        text = this._encoding.GetString(this._incoming.ToArray(), 0, this._incoming.Count);
                    }

                    var match = regex.Match(text);

                    if (match.Success)
                    {
                        //  Remove processed items from the queue
                        for (int i = 0; i < match.Index + match.Length && this._incoming.Count > 0; i++)
                        {
                            this._incoming.Dequeue();
                        }
                        break;
                    }
                }

                if (timeout.Ticks > 0)
                {
                    if (!this._dataReceived.WaitOne(timeout))
                    {
                        return null;
                    }
                }
                else
                {
                    this._dataReceived.WaitOne();
                }

            }

            return text;
        }

        /// <summary>
        /// Reads the line from the shell. If line is not available it will block the execution and will wait for new line.
        /// </summary>
        /// <returns>The line read from the shell.</returns>
        public string ReadLine()
        {
            return this.ReadLine(TimeSpan.Zero);
        }

        /// <summary>
        /// Reads the line from the shell. If line is not available it will block the execution and will wait for new line.
        /// </summary>
        /// <param name="timeout">Time to wait for input.</param>
        /// <returns>
        /// The line read from the shell, if the specified time elapsed returns null.
        /// </returns>
        public string ReadLine(TimeSpan timeout)
        {
            var text = string.Empty;

            while (true)
            {
                lock (this._incoming)
                {
                    if (this._incoming.Count > 0)
                    {
                        text = this._encoding.GetString(this._incoming.ToArray(), 0, this._incoming.Count);
                    }

                    var index = text.IndexOf("\r\n");

                    if (index >= 0)
                    {
                        text = text.Substring(0, index);

                        //  Remove processed items from the queue
                        for (int i = 0; i < index + 2 && this._incoming.Count > 0; i++)
                        {
                            this._incoming.Dequeue();
                        }
                        break;
                    }
                }

                if (timeout.Ticks > 0)
                {
                    if (!this._dataReceived.WaitOne(timeout))
                    {
                        return null;
                    }
                }
                else
                {
                    this._dataReceived.WaitOne();
                }

            }

            return text;
        }

        /// <summary>
        /// Reads text available in the shell.
        /// </summary>
        /// <returns>The text available in the shell.</returns>
        public string Read()
        {
            string text;

            lock (this._incoming)
            {
                text = this._encoding.GetString(this._incoming.ToArray(), 0, this._incoming.Count);
                this._incoming.Clear();
            }

            return text;
        }

        /// <summary>
        /// Writes the specified text to the shell.
        /// </summary>
        /// <param name="text">The text to be written to the shell.</param>
        public void Write(string text)
        {
            if (this._channel == null)
            {
                throw new ObjectDisposedException("ShellStream");
            }

            var data = this._encoding.GetBytes(text);
            this._channel.SendData(data);
        }

        /// <summary>
        /// Writes the line to the shell.
        /// </summary>
        /// <param name="line">The line to be written to the shell.</param>
        public void WriteLine(string line)
        {
            var commandText = string.Format("{0}{1}", line, "\r");
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

            if (this._dataReceived != null)
            {
                this._dataReceived.Dispose();
                this._dataReceived = null;
            }

            if (this._session != null)
            {
                this._session.Disconnected -= Session_Disconnected;
                this._session.ErrorOccured -= Session_ErrorOccured;
            }
        }

        /// <summary>
        /// Waits for the handle to be signaled or for an error to occurs.
        /// </summary>
        /// <param name="waitHandle">The wait handle.</param>
        protected void WaitOnHandle(WaitHandle waitHandle)
        {
            this._session.WaitOnHandle(waitHandle);
        }

        partial void ExecuteThread(Action action);

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
            lock (this._incoming)
            {
                foreach (var b in e.Data)
                    this._incoming.Enqueue(b);
            }

            if (_dataReceived != null)
                _dataReceived.Set();

            this.OnDataReceived(e.Data);
        }

        private void OnRaiseError(ExceptionEventArgs e)
        {
            var handler = this.ErrorOccurred;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnDataReceived(byte[] data)
        {
            var handler = this.DataReceived;
            if (handler != null)
            {
                handler(this, new ShellDataEventArgs(data));
            }
        }
    }
}
