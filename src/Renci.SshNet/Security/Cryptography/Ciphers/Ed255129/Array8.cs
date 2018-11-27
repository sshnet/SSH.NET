using System;

namespace Chaos.NaCl.Internal
{
    // Array8<UInt32> Poly1305 key
    // Array8<UInt64> SHA-512 state/output
    internal struct Array8<T>
    {
        public T x0;
        public T x1;
        public T x2;
        public T x3;
        public T x4;
        public T x5;
        public T x6;
        public T x7;
    }
}