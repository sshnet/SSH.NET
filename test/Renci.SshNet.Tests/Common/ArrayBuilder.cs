using System;
using System.Collections.Generic;

namespace Renci.SshNet.Tests.Common
{
    public class ArrayBuilder<T>
    {
        private readonly List<T> _buffer;

        public ArrayBuilder()
        {
            _buffer = new List<T>();
        }

        public ArrayBuilder<T> Add(T[] array)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            return Add(array, 0, array.Length);
        }

        public ArrayBuilder<T> Add(T[] array, int index, int length)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            for (var i = 0; i < length; i++)
            {
                _buffer.Add(array[index + i]);
            }

            return this;
        }

        public T[] Build()
        {
            return _buffer.ToArray();
        }
    }
}
