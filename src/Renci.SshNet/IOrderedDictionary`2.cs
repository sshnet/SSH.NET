using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents a collection of key/value pairs that are accessible by the key or index.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    public interface IOrderedDictionary<TKey, TValue> :
        IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
        where TKey : notnull
    {
        // Some members are redefined with 'new' to resolve ambiguities.

        /// <summary>Gets or sets the value associated with the specified key.</summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a <see cref="KeyNotFoundException"/>, and a set operation creates a new element with the specified key.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="KeyNotFoundException">The property is retrieved and <paramref name="key"/> does not exist in the collection.</exception>
        /// <remarks>Setting the value of an existing key does not impact its order in the collection.</remarks>
        new TValue this[TKey key] { get; set; }

        /// <summary>Gets a collection containing the keys in the <see cref="IOrderedDictionary{TKey, TValue}"/>.</summary>
        new ICollection<TKey> Keys { get; }

        /// <summary>Gets a collection containing the values in the <see cref="IOrderedDictionary{TKey, TValue}"/>.</summary>
        new ICollection<TValue> Values { get; }

        /// <summary>Gets the number of key/value pairs contained in the <see cref="IOrderedDictionary{TKey, TValue}"/>.</summary>
        new int Count { get; }

        /// <summary>Determines whether the <see cref="IOrderedDictionary{TKey, TValue}"/> contains the specified key.</summary>
        /// <param name="key">The key to locate in the <see cref="IOrderedDictionary{TKey, TValue}"/>.</param>
        /// <returns><see langword="true"/> if the <see cref="IOrderedDictionary{TKey, TValue}"/> contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        new bool ContainsKey(TKey key);

        /// <summary>Determines whether the <see cref="IOrderedDictionary{TKey, TValue}"/> contains a specific value.</summary>
        /// <param name="value">The value to locate in the <see cref="IOrderedDictionary{TKey, TValue}"/>. The value can be null for reference types.</param>
        /// <returns><see langword="true"/> if the <see cref="IOrderedDictionary{TKey, TValue}"/> contains an element with the specified value; otherwise, <see langword="false"/>.</returns>
        bool ContainsValue(TValue value);

        /// <summary>Gets the key/value pair at the specified index.</summary>
        /// <param name="index">The zero-based index of the pair to get.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.</exception>
        KeyValuePair<TKey, TValue> GetAt(int index);

        /// <summary>Determines the index of a specific key in the <see cref="IOrderedDictionary{TKey, TValue}"/>.</summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>The index of <paramref name="key"/> if found; otherwise, -1.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        int IndexOf(TKey key);

        /// <summary>Inserts an item into the collection at the specified index.</summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="key">The key to insert.</param>
        /// <param name="value">The value to insert.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">An element with the same key already exists in the <see cref="IOrderedDictionary{TKey, TValue}"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than <see cref="Count"/>.</exception>
        void Insert(int index, TKey key, TValue value);

        /// <summary>Removes the value with the specified key from the <see cref="IOrderedDictionary{TKey, TValue}"/> and copies the element to the value parameter.</summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <param name="value">The removed element.</param>
        /// <returns><see langword="true"/> if the element is successfully found and removed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        bool Remove(TKey key, [MaybeNullWhen(false)] out TValue value);

        /// <summary>Removes the key/value pair at the specified index.</summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.</exception>
        void RemoveAt(int index);

        /// <summary>Sets the key/value pair at the specified index.</summary>
        /// <param name="index">The zero-based index at which to set the key/value pair.</param>
        /// <param name="key">The key to store at the specified index.</param>
        /// <param name="value">The value to store at the specified index.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">An element with the same key already exists at an index different to <paramref name="index"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.</exception>
        void SetAt(int index, TKey key, TValue value);

        /// <summary>Sets the value for the key at the specified index.</summary>
        /// <param name="index">The zero-based index at which to set the key/value pair.</param>
        /// <param name="value">The value to store at the specified index.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.</exception>
        void SetAt(int index, TValue value);

        /// <summary>
        /// Moves an existing key/value pair to the specified index in the collection.
        /// </summary>
        /// <param name="index">The current zero-based index of the key/value pair to move.</param>
        /// <param name="newIndex">The zero-based index at which to set the key/value pair.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> or <paramref name="newIndex"/> are less than 0 or greater than or equal to <see cref="Count"/>.
        /// </exception>
        void SetPosition(int index, int newIndex);

        /// <summary>
        /// Moves an existing key/value pair to the specified index in the collection.
        /// </summary>
        /// <param name="key">The key to move.</param>
        /// <param name="newIndex">The zero-based index at which to set the key/value pair.</param>
        /// <exception cref="KeyNotFoundException">The specified key does not exist in the collection.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newIndex"/> is less than 0 or greater than or equal to <see cref="Count"/>.</exception>
        void SetPosition(TKey key, int newIndex);

        /// <summary>Adds the specified key and value to the dictionary if the key doesn't already exist.</summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be <see langword="null"/> for reference types.</param>
        /// <returns><see langword="true"/> if the key didn't exist and the key and value were added to the dictionary; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        bool TryAdd(TKey key, TValue value);

        /// <summary>Adds the specified key and value to the dictionary if the key doesn't already exist.</summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be <see langword="null"/> for reference types.</param>
        /// <param name="index">The index of the added or existing <paramref name="key"/>. This is always a valid index into the dictionary.</param>
        /// <returns><see langword="true"/> if the key didn't exist and the key and value were added to the dictionary; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        bool TryAdd(TKey key, TValue value, out int index);

        /// <summary>Gets the value associated with the specified key.</summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified key, if the key is found;
        /// otherwise, the default value for the type of the value parameter.
        /// </param>
        /// <returns><see langword="true"/> if the <see cref="IOrderedDictionary{TKey, TValue}"/> contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        new bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value);

        /// <summary>Gets the value associated with the specified key.</summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified key, if the key is found;
        /// otherwise, the default value for the type of the value parameter.
        /// </param>
        /// <param name="index">The index of <paramref name="key"/> if found; otherwise, -1.</param>
        /// <returns><see langword="true"/> if the <see cref="IOrderedDictionary{TKey, TValue}"/> contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value, out int index);
    }
}
