#if !NET9_0_OR_GREATER
#nullable enable
#pragma warning disable SA1649 // File name should match first type name

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Renci.SshNet.Common;

namespace Renci.SshNet
{
    internal sealed class OrderedDictionary<TKey, TValue> : IOrderedDictionary<TKey, TValue>
        where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _dictionary;
        private readonly List<KeyValuePair<TKey, TValue>> _list;

        private KeyCollection? _keys;
        private ValueCollection? _values;

        public OrderedDictionary(EqualityComparer<TKey>? comparer = null)
        {
            _dictionary = new Dictionary<TKey, TValue>(comparer);
            _list = new List<KeyValuePair<TKey, TValue>>();
        }

        public TValue this[TKey key]
        {
            get
            {
                return _dictionary[key];
            }
            set
            {
                if (_dictionary.TryAdd(key, value))
                {
                    _list.Add(new KeyValuePair<TKey, TValue>(key, value));
                }
                else
                {
                    _dictionary[key] = value;
                    _list[IndexOf(key)] = new KeyValuePair<TKey, TValue>(key, value);
                }

                AssertConsistency();
            }
        }

        [Conditional("DEBUG")]
        private void AssertConsistency()
        {
            Debug.Assert(_list.Count == _dictionary.Count);

            foreach (var kvp in _list)
            {
                Debug.Assert(_dictionary.TryGetValue(kvp.Key, out var value));
                Debug.Assert(EqualityComparer<TValue>.Default.Equals(kvp.Value, value));
            }

            foreach (var kvp in _dictionary)
            {
                var index = EnumeratingIndexOf(kvp.Key);
                Debug.Assert(index >= 0);
                Debug.Assert(EqualityComparer<TValue>.Default.Equals(kvp.Value, _list[index].Value));
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return _keys ??= new KeyCollection(this);
            }
        }

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get
            {
                return Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return _values ??= new ValueCollection(this);
            }
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get
            {
                return Values;
            }
        }

        public int Count
        {
            get
            {
                Debug.Assert(_list.Count == _dictionary.Count);
                return _list.Count;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            _list.Add(new KeyValuePair<TKey, TValue>(key, value));

            AssertConsistency();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _dictionary.Clear();
            _list.Clear();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            return _dictionary.ContainsValue(value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public KeyValuePair<TKey, TValue> GetAt(int index)
        {
            return _list[index];
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(TKey key)
        {
            // Fast lookup.
            if (!_dictionary.ContainsKey(key))
            {
                Debug.Assert(EnumeratingIndexOf(key) == -1);
                return -1;
            }

            var index = EnumeratingIndexOf(key);

            Debug.Assert(index >= 0);

            return index;
        }

        private int EnumeratingIndexOf(TKey key)
        {
            Debug.Assert(key is not null);

            var i = -1;

            foreach (var kvp in _list)
            {
                i++;

                if (_dictionary.Comparer.Equals(key, kvp.Key))
                {
                    return i;
                }
            }

            return -1;
        }

        public void Insert(int index, TKey key, TValue value)
        {
            // This validation is also done by _list.Insert but we must
            // do it before _dictionary.Add to avoid corrupting the state.
            if ((uint)index > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            _dictionary.Add(key, value);
            _list.Insert(index, new KeyValuePair<TKey, TValue>(key, value));

            AssertConsistency();
        }

        public bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            if (_dictionary.Remove(key, out value))
            {
                _list.RemoveAt(EnumeratingIndexOf(key));
                AssertConsistency();
                return true;
            }

            AssertConsistency();
            value = default!;
            return false;
        }

        public bool Remove(TKey key)
        {
            if (_dictionary.Remove(key))
            {
                _list.RemoveAt(EnumeratingIndexOf(key));
                AssertConsistency();
                return true;
            }

            AssertConsistency();
            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            if (((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Remove(item))
            {
                _list.RemoveAt(EnumeratingIndexOf(item.Key));
                AssertConsistency();
                return true;
            }

            AssertConsistency();
            return false;
        }

        public void RemoveAt(int index)
        {
            var key = _list[index].Key;

            _list.RemoveAt(index);

            var success = _dictionary.Remove(key);
            Debug.Assert(success);

            AssertConsistency();
        }

        public void SetAt(int index, TKey key, TValue value)
        {
            if ((uint)index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (TryGetValue(key, out _, out var existingIndex))
            {
                if (index != existingIndex)
                {
                    throw new ArgumentException("An item with the same key has already been added", nameof(key));
                }
            }
            else
            {
                var oldKeyRemoved = _dictionary.Remove(_list[index].Key);

                Debug.Assert(oldKeyRemoved);
            }

            _dictionary[key] = value;
            _list[index] = new KeyValuePair<TKey, TValue>(key, value);

            AssertConsistency();
        }

        public void SetAt(int index, TValue value)
        {
            var key = _list[index].Key;

            _list[index] = new KeyValuePair<TKey, TValue>(key, value);
            _dictionary[key] = value;

            AssertConsistency();
        }

        public void SetPosition(int index, int newIndex)
        {
            if ((uint)newIndex >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(newIndex));
            }

            var kvp = _list[index];

            _list.RemoveAt(index);
            _list.Insert(newIndex, kvp);

            AssertConsistency();
        }

        public void SetPosition(TKey key, int newIndex)
        {
            // This performs the same lookup that IndexOf would
            // but throws a nicely formatted KeyNotFoundException
            // if the key does not exist in the collection.
            _ = _dictionary[key];

            Debug.Assert(key is not null);

            var oldIndex = EnumeratingIndexOf(key);

            Debug.Assert(oldIndex >= 0);

            SetPosition(oldIndex, newIndex);
        }

        public bool TryAdd(TKey key, TValue value)
        {
            if (_dictionary.TryAdd(key, value))
            {
                _list.Add(new KeyValuePair<TKey, TValue>(key, value));
                AssertConsistency();
                return true;
            }

            AssertConsistency();
            return false;
        }

        public bool TryAdd(TKey key, TValue value, out int index)
        {
            if (_dictionary.TryAdd(key, value))
            {
                _list.Add(new KeyValuePair<TKey, TValue>(key, value));
                index = _list.Count - 1;
                AssertConsistency();
                return true;
            }

            index = EnumeratingIndexOf(key);
            AssertConsistency();
            return false;
        }

#if NET
        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
#else
        public bool TryGetValue(TKey key, out TValue value)
#endif
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value, out int index)
        {
            if (_dictionary.TryGetValue(key, out value))
            {
                index = EnumeratingIndexOf(key);
                return true;
            }

            index = -1;
            return false;
        }

        private sealed class KeyCollection : KeyOrValueCollection<TKey>
        {
            public KeyCollection(OrderedDictionary<TKey, TValue> orderedDictionary)
                : base(orderedDictionary)
            {
            }

            public override bool Contains(TKey item)
            {
                return OrderedDictionary._dictionary.ContainsKey(item);
            }

            public override void CopyTo(TKey[] array, int arrayIndex)
            {
                base.CopyTo(array, arrayIndex); // Validation

                foreach (var kvp in OrderedDictionary._list)
                {
                    array[arrayIndex++] = kvp.Key;
                }
            }

            public override IEnumerator<TKey> GetEnumerator()
            {
                return OrderedDictionary._list.Select(kvp => kvp.Key).GetEnumerator();
            }
        }

        private sealed class ValueCollection : KeyOrValueCollection<TValue>
        {
            public ValueCollection(OrderedDictionary<TKey, TValue> orderedDictionary)
                : base(orderedDictionary)
            {
            }

            public override bool Contains(TValue item)
            {
                return OrderedDictionary._dictionary.ContainsValue(item);
            }

            public override void CopyTo(TValue[] array, int arrayIndex)
            {
                base.CopyTo(array, arrayIndex); // Validation

                foreach (var kvp in OrderedDictionary._list)
                {
                    array[arrayIndex++] = kvp.Value;
                }
            }

            public override IEnumerator<TValue> GetEnumerator()
            {
                return OrderedDictionary._list.Select(kvp => kvp.Value).GetEnumerator();
            }
        }

        private abstract class KeyOrValueCollection<T> : ICollection<T>
        {
            protected OrderedDictionary<TKey, TValue> OrderedDictionary { get; }

            protected KeyOrValueCollection(OrderedDictionary<TKey, TValue> orderedDictionary)
            {
                OrderedDictionary = orderedDictionary;
            }

            public int Count
            {
                get
                {
                    return OrderedDictionary.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public void Add(T item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public abstract bool Contains(T item);

            public virtual void CopyTo(T[] array, int arrayIndex)
            {
                ThrowHelper.ThrowIfNull(array);
                ThrowHelper.ThrowIfNegative(arrayIndex);

                if (array.Length - arrayIndex < Count)
                {
                    throw new ArgumentException(
                        "Destination array was not long enough. Check the destination index, length, and the array's lower bounds.",
                        nameof(array));
                }
            }

            public abstract IEnumerator<T> GetEnumerator();

            public bool Remove(T item)
            {
                throw new NotSupportedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
#endif
