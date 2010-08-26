using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Renci.SshClient.Common
{
    public class BlockingStack<T> : IEnumerable<T>, ICollection, IEnumerable
    {
        private Stack<T> _stack;

        public BlockingStack()
        {
            this._stack = new Stack<T>();
        }

        public BlockingStack(IEnumerable<T> collection)
        {
            this._stack = new Stack<T>(collection);
        }

        public BlockingStack(int capacity)
        {
            this._stack = new Stack<T>(capacity);
        }

        // Summary:
        //     Gets the number of elements contained in the System.Collections.Generic.Stack<T>.
        //
        // Returns:
        //     The number of elements contained in the System.Collections.Generic.Stack<T>.
        public int Count
        {
            get
            {
                return this._stack.Count;
            }
        }

        public void Clear()
        {
            lock (this)
            {
                this._stack.Clear();
                Monitor.PulseAll(this);
            }
        }

        public bool Contains(T item)
        {
            return this._stack.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (this)
            {
                this._stack.CopyTo(array, arrayIndex);
                Monitor.PulseAll(this);
            }
        }

        public T Peek()
        {
            return this._stack.Peek();
        }

        public T Pop()
        {
            lock (this)
            {
                var result = this._stack.Pop();
                Monitor.PulseAll(this);
                return result;
            }
        }

        public T WaitAndPop()
        {
            lock (this)
            {
                //  Wait for item to be added to the stack
                while (this._stack.Count == 0)
                {
                    Monitor.Wait(this);
                }
                var result = this._stack.Pop();
                Monitor.PulseAll(this);
                return result;
            }
        }

        public void Push(T item)
        {
            lock (this)
            {
                this._stack.Push(item);
                Monitor.PulseAll(this);
            }
        }

        public T[] ToArray()
        {
            lock (this)
            {
                var result = this._stack.ToArray();
                Monitor.PulseAll(this);
                return result;
            }
        }

        public void TrimExcess()
        {
            lock (this)
            {
                this._stack.TrimExcess();
                Monitor.PulseAll(this);
            }
        }

        #region ICollection Members

        public void CopyTo(System.Array array, int index)
        {
            throw new System.NotImplementedException();
        }

        public bool IsSynchronized
        {
            get { throw new System.NotImplementedException(); }
        }

        public object SyncRoot
        {
            get { throw new System.NotImplementedException(); }
        }

        #endregion


        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return this._stack.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._stack.GetEnumerator();
        }

        #endregion
    }
}
