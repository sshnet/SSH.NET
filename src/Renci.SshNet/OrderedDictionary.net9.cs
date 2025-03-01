#if NET9_0_OR_GREATER
#nullable enable
#pragma warning disable SA1649 // File name should match first type name

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Renci.SshNet
{
    internal sealed class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>
        where TKey : notnull
    {
        private readonly System.Collections.Generic.OrderedDictionary<TKey, TValue> _impl;

        public OrderedDictionary(EqualityComparer<TKey>? comparer = null)
        {
            _impl = new System.Collections.Generic.OrderedDictionary<TKey, TValue>(comparer);
        }

        public TValue this[TKey key]
        {
            get
            {
                return _impl[key];
            }
            set
            {
                _impl[key] = value;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return _impl.Keys;
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get
            {
                return ((IReadOnlyDictionary<TKey, TValue>)_impl).Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return _impl.Values;
            }
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get
            {
                return ((IReadOnlyDictionary<TKey, TValue>)_impl).Values;
            }
        }

        public int Count
        {
            get
            {
                return _impl.Count;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return ((ICollection<KeyValuePair<TKey, TValue>>)_impl).IsReadOnly;
            }
        }

        public void Add(TKey key, TValue value)
        {
            _impl.Add(key, value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_impl).Add(item);
        }

        public void Clear()
        {
            _impl.Clear();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_impl).Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return _impl.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            return _impl.ContainsValue(value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_impl).CopyTo(array, arrayIndex);
        }

        public KeyValuePair<TKey, TValue> GetAt(int index)
        {
            return _impl.GetAt(index);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _impl.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(TKey key)
        {
            return _impl.IndexOf(key);
        }

        public void Insert(int index, TKey key, TValue value)
        {
            _impl.Insert(index, key, value);
        }

        public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return _impl.Remove(key, out value);
        }

        public bool Remove(TKey key)
        {
            return _impl.Remove(key);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_impl).Remove(item);
        }

        public void RemoveAt(int index)
        {
            _impl.RemoveAt(index);
        }

        public void SetAt(int index, TKey key, TValue value)
        {
            _impl.SetAt(index, key, value);
        }

        public void SetAt(int index, TValue value)
        {
            _impl.SetAt(index, value);
        }

        public void SetPosition(int index, int newIndex)
        {
            if ((uint)newIndex >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(newIndex));
            }

            var kvp = _impl.GetAt(index);

            _impl.RemoveAt(index);

            _impl.Insert(newIndex, kvp.Key, kvp.Value);
        }

        public void SetPosition(TKey key, int newIndex)
        {
            if ((uint)newIndex >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(newIndex));
            }

            if (!_impl.Remove(key, out var value))
            {
                // Please throw a nicely formatted, localised exception.
                _ = _impl[key];

                Debug.Fail("Previous line should throw KeyNotFoundException.");
            }

            _impl.Insert(newIndex, key, value);
        }

        public bool TryAdd(TKey key, TValue value)
        {
            return _impl.TryAdd(key, value);
        }

        public bool TryAdd(TKey key, TValue value, out int index)
        {
#if NET10_0_OR_GREATER
            return _impl.TryAdd(key, value, out index);
#else
            var success = _impl.TryAdd(key, value);

            index = _impl.IndexOf(key);

            return success;
#endif
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return _impl.TryGetValue(key, out value);
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value, out int index)
        {
#if NET10_0_OR_GREATER
            return _impl.TryGetValue(key, out value, out index);
#else
            if (_impl.TryGetValue(key, out value))
            {
                index = _impl.IndexOf(key);
                return true;
            }

            index = -1;
            return false;
#endif
        }
    }
}
#endif
