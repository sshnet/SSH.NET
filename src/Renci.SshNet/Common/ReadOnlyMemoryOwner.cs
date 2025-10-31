#nullable enable
using System;
using System.Buffers;
using System.Diagnostics;
using System.Net;

namespace Renci.SshNet.Common
{
    /// <summary>
    /// A type representing ownership of a rented, read-only buffer.
    /// </summary>
    internal sealed class ReadOnlyMemoryOwner : IMemoryOwner<byte>
    {
        private ArrayBuffer _buffer;

        public ReadOnlyMemoryOwner(ArrayBuffer buffer)
        {
            _buffer = buffer;

            AssertValid();
        }

        [Conditional("DEBUG")]
        private void AssertValid()
        {
            Debug.Assert(
                _buffer.ActiveLength > 0 || _buffer.AvailableLength == 0,
                "If the buffer is empty, then it should have been returned to the pool.");
        }

        public int Length
        {
            get
            {
                AssertValid();
                return _buffer.ActiveLength;
            }
        }

        public bool IsEmpty
        {
            get
            {
                AssertValid();
                return _buffer.ActiveLength == 0;
            }
        }

        public ReadOnlySpan<byte> Span
        {
            get
            {
                AssertValid();
                return _buffer.ActiveReadOnlySpan;
            }
        }

        Memory<byte> IMemoryOwner<byte>.Memory
        {
            get
            {
                AssertValid();
                return _buffer.ActiveMemory;
            }
        }

        public void Slice(int start)
        {
            AssertValid();

            _buffer.Discard(start);

            if (_buffer.ActiveLength == 0)
            {
                // Return the rented buffer as soon as it's no longer in use.
                _buffer.ClearAndReturnBuffer();
            }
        }

        public void Dispose()
        {
            AssertValid();

            _buffer.ClearAndReturnBuffer();
        }
    }
}
