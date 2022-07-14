namespace Renci.SshNet.Common
{
    using System;

    /// <summary>
    /// A generic pipe to pass through data.
    /// </summary>
    internal class Pipe : IDisposable
    {
        private readonly LinkedListQueue<byte[]> _queue;

        /// <summary>
        /// Gets the input stream.
        /// </summary>
        /// <value>The input stream.</value>
        public PipeInputStream InputStream { get; private set; }

        /// <summary>
        /// Gets the output stream.
        /// </summary>
        /// <value>The output stream.</value>
        public PipeOutputStream OutputStream { get; private set; }

        public Pipe()
        {
            _queue = new LinkedListQueue<byte[]>();
            InputStream = new PipeInputStream(_queue);
            OutputStream = new PipeOutputStream(_queue);
        }

        /// <summary>
        /// Releases all resource used by the <see cref="T:Renci.SshNet.Common.Pipe"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="T:Renci.SshNet.Common.Pipe"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="T:Renci.SshNet.Common.Pipe"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:Renci.SshNet.Common.Pipe"/> so the garbage collector can reclaim the memory that the
        /// <see cref="T:Renci.SshNet.Common.Pipe"/> was occupying.</remarks>
        public void Dispose()
        {
            OutputStream.Dispose();
            InputStream.Dispose();
            _queue.Dispose();
        }
    }
}
