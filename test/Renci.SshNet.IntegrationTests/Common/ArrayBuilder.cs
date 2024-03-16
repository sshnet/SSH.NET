namespace Renci.SshNet.IntegrationTests.Common
{
    internal class ArrayBuilder<T>
    {
        private readonly List<T> _buffer;

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
