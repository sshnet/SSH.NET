namespace Renci.SshNet.Common
{
    using System;
    using System.Threading;

    /// <summary>
    /// Fast concurrent generic linked list queue.
    /// </summary>
    public class LinkedListQueue<T> : IDisposable
    {
        sealed class Entry<E>
        {
            public E Item;
            public Entry<E> Next;
        }

        private readonly object _lock = new object();

        private Entry<T> _first;
        private Entry<T> _last;

        private bool _isAddingCompleted;

        /// <summary>
        /// Gets whether this <see cref="T:Renci.SshNet.Common.LinkedListQueue`1"/> has been marked as complete for adding and is empty.
        /// </summary>
        /// <value>Whether this queue has been marked as complete for adding and is empty.</value>
        public bool IsCompleted
        {
            get { return _isAddingCompleted && _first == null && _last == null; }
        }

        /// <summary>
        /// Gets whether this <see cref="T:Renci.SshNet.Common.LinkedListQueue`1"/> has been marked as complete for adding.
        /// </summary>
        /// <value>Whether this queue has been marked as complete for adding.</value>
        public bool IsAddingCompleted
        {
            get { return _isAddingCompleted; }
            set
            {
                lock (_lock)
                {
                    _isAddingCompleted = value;
                }
            }
        }

        /// <summary>
        /// Adds the item to <see cref="T:Renci.SshNet.Common.LinkedListQueue`1"/>.
        /// </summary>
        /// <param name="item">The item to be added to the queue. The value can be a null reference.</param>
        public void Add(T item)
        {
            lock (_lock)
            {
                if (_isAddingCompleted)
                    return;

                var entry = new Entry<T>();
                entry.Item = item;

                if (_last != null)
                {
                    _last.Next = entry;
                }

                _last = entry;

                if (_first == null)
                {
                    _first = entry;
                }

                Monitor.PulseAll(_lock);
            }
        }

        /// <summary>
        /// Marks the <see cref="T:Renci.SshNet.Common.LinkedListQueue`1"/> instances as not accepting any more additions.
        /// </summary>
        public void CompleteAdding()
        {
            lock (_lock)
            {
                IsAddingCompleted = true;
                Monitor.PulseAll(_lock);
            }
        }

        /// <summary>
        /// Tries to remove an item from the <see cref="T:Renci.SshNet.Common.LinkedListQueue`1"/>.
        /// </summary>
        /// <returns><c>true</c>, if an item could be removed; otherwise <c>false</c>.</returns>
        /// <param name="item">The item to be removed from the queue.</param>
        public bool TryTake(out T item)
        {
            lock (_lock)
            {
                while (_first == null && !_isAddingCompleted)
                    Monitor.Wait(_lock);

                if (_first == null && _isAddingCompleted)
                {
                    item = default(T);
                    return false;
                }

                item = _first.Item;
                _first = _first.Next;
                return true;
            }
        }

        /// <summary>
        /// Releases all resource used by the <see cref="T:Renci.SshNet.Common.LinkedListQueue`1"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:Renci.SshNet.Common.LinkedListQueue`1"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="T:Renci.SshNet.Common.LinkedListQueue`1"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:Renci.SshNet.Common.LinkedListQueue`1"/> so the garbage collector can reclaim the memory that
        /// the <see cref="T:Renci.SshNet.Common.LinkedListQueue`1"/> was occupying.</remarks>
        public void Dispose()
        {
            lock (_lock)
            {
                _first = null;
                _last = null;
                _isAddingCompleted = true;
            }
        }
    }
}
