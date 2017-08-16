using System.Collections.Generic;

namespace Renci.SshNet.Tests.Common
{
    public class ArrayBuilder<T>
    {
        private List<T> _buffer;

        public ArrayBuilder()
        {
            _buffer = new List<T>();
        }

        public ArrayBuilder<T> Add(T[] array)
        {
            return Add(array, 0, array.Length);
        }

        public ArrayBuilder<T> Add(T[] array, int index, int length)
        {
            for (var i = 0; i < length; i++)
                _buffer.Add(array[index + i]);
            return this;
        }

        public T[] Build()
        {
            return _buffer.ToArray();
        }
    }
}
