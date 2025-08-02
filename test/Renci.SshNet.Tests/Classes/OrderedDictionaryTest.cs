using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class OrderedDictionaryTest
    {
        private static void AssertEqual<TKey, TValue>(List<KeyValuePair<TKey, TValue>> expected, OrderedDictionary<TKey, TValue> o)
        {
            Assert.AreEqual(expected.Count, o.Count);

            CollectionAssert.AreEqual(expected, ToList(o)); // Test the enumerator

            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i], o.GetAt(i));

                Assert.AreEqual(expected[i].Value, o[expected[i].Key]);

                Assert.IsTrue(o.TryGetValue(expected[i].Key, out TValue value));
                Assert.AreEqual(expected[i].Value, value);

                Assert.IsTrue(o.TryGetValue(expected[i].Key, out value, out int index));
                Assert.AreEqual(expected[i].Value, value);
                Assert.AreEqual(i, index);

                Assert.IsTrue(((ICollection<KeyValuePair<TKey, TValue>>)o).Contains(expected[i]));
                Assert.IsTrue(o.ContainsKey(expected[i].Key));
                Assert.IsTrue(o.ContainsValue(expected[i].Value));
                Assert.IsTrue(o.Keys.Contains(expected[i].Key));
                Assert.IsTrue(o.Values.Contains(expected[i].Value));

                Assert.AreEqual(i, o.IndexOf(expected[i].Key));

                Assert.IsFalse(o.TryAdd(expected[i].Key, default));
                Assert.IsFalse(o.TryAdd(expected[i].Key, default, out index));
                Assert.AreEqual(i, index);
            }

            Assert.AreEqual(expected.Count, o.Keys.Count);
            CollectionAssert.AreEqual(expected.Select(kvp => kvp.Key).ToList(), ToList(o.Keys));
            CollectionAssert.AreEqual(ToList(o.Keys), ToList(((IReadOnlyDictionary<TKey, TValue>)o).Keys));

            Assert.AreEqual(expected.Count, o.Values.Count);
            CollectionAssert.AreEqual(expected.Select(kvp => kvp.Value).ToList(), ToList(o.Values));
            CollectionAssert.AreEqual(ToList(o.Values), ToList(((IReadOnlyDictionary<TKey, TValue>)o).Values));

            // Test CopyTo
            var kvpArray = new KeyValuePair<TKey, TValue>[1 + expected.Count + 1];
            ((ICollection<KeyValuePair<TKey, TValue>>)o).CopyTo(kvpArray, 1);
            CollectionAssert.AreEqual(
                (List<KeyValuePair<TKey, TValue>>)[default, .. expected, default],
                kvpArray);

            var keysArray = new TKey[1 + expected.Count + 1];
            o.Keys.CopyTo(keysArray, 1);
            CollectionAssert.AreEqual(
                (List<TKey>)[default, .. expected.Select(kvp => kvp.Key), default],
                keysArray);

            var valuesArray = new TValue[1 + expected.Count + 1];
            o.Values.CopyTo(valuesArray, 1);
            CollectionAssert.AreEqual(
                (List<TValue>)[default, .. expected.Select(kvp => kvp.Value), default],
                valuesArray);

            // Creates a List<T> via enumeration, avoiding the ICollection<T>.CopyTo
            // optimisation in the List<T> constructor.
            static List<T> ToList<T>(IEnumerable<T> values)
            {
                List<T> list = new();
                foreach (T t in values)
                {
                    list.Add(t);
                }
                return list;
            }
        }

        [TestMethod]
        public void NullKey_ThrowsArgumentNull()
        {
            OrderedDictionary<string, int> o = new() { { "a", 4 } };

            Assert.ThrowsExactly<ArgumentNullException>(() => o[null]);
            Assert.ThrowsExactly<ArgumentNullException>(() => o.Add(null, 1));
            Assert.ThrowsExactly<ArgumentNullException>(() => ((ICollection<KeyValuePair<string, int>>)o).Add(new KeyValuePair<string, int>(null, 1)));
            Assert.ThrowsExactly<ArgumentNullException>(() => ((ICollection<KeyValuePair<string, int>>)o).Contains(new KeyValuePair<string, int>(null, 1)));
            Assert.ThrowsExactly<ArgumentNullException>(() => o.ContainsKey(null));
            Assert.ThrowsExactly<ArgumentNullException>(() => o.IndexOf(null));
            Assert.ThrowsExactly<ArgumentNullException>(() => o.Insert(0, null, 1));
            Assert.ThrowsExactly<ArgumentNullException>(() => o.Remove(null, out _));
            Assert.ThrowsExactly<ArgumentNullException>(() => o.Remove(null));
            Assert.ThrowsExactly<ArgumentNullException>(() => ((ICollection<KeyValuePair<string, int>>)o).Remove(new KeyValuePair<string, int>(null, 1)));
            Assert.ThrowsExactly<ArgumentNullException>(() => o.SetAt(0, null, 1));
            Assert.ThrowsExactly<ArgumentNullException>(() => o.SetPosition(null, 0));
            Assert.ThrowsExactly<ArgumentNullException>(() => o.TryAdd(null, 1));
            Assert.ThrowsExactly<ArgumentNullException>(() => o.TryAdd(null, 1, out _));
            Assert.ThrowsExactly<ArgumentNullException>(() => o.TryGetValue(null, out _));
            Assert.ThrowsExactly<ArgumentNullException>(() => o.TryGetValue(null, out _, out _));
        }

        [TestMethod]
        public void Indexer_Match_GetterReturnsValue()
        {
            OrderedDictionary<string, int> o = new() { { "a", 4 }, { "b", 8 } };

            Assert.AreEqual(8, o["b"]);
        }

        [TestMethod]
        public void Indexer_Match_SetterChangesValue()
        {
            OrderedDictionary<string, int> o = new() { { "a", 4 }, { "b", 8 } };

            o["a"] = 5;

            AssertEqual([new("a", 5), new("b", 8)], o);
        }

        [TestMethod]
        public void Indexer_NoMatch_GetterThrowsKeyNotFound()
        {
            OrderedDictionary<string, int> o = new() { { "a", 4 } };

            Assert.ThrowsExactly<KeyNotFoundException>(() => o["b"]);
        }

        [TestMethod]
        public void Indexer_NoMatch_SetterAddsItem()
        {
            OrderedDictionary<string, int> o = new() { { "a", 4 } };

            o["b"] = 8;

            AssertEqual([new("a", 4), new("b", 8)], o);
        }

        [TestMethod]
        public void Add_Match()
        {
            OrderedDictionary<string, int> o = new() { { "a", 4 } };

            Assert.ThrowsExactly<ArgumentException>(() => o.Add("a", 8));
            Assert.ThrowsExactly<ArgumentException>(() => ((ICollection<KeyValuePair<string, int>>)o).Add(new KeyValuePair<string, int>("a", 8)));
        }

        [TestMethod]
        public void Clear()
        {
            OrderedDictionary<string, int> o = new() { { "a", 4 }, { "b", 8 } };

            AssertEqual([new("a", 4), new("b", 8)], o);
            o.Clear();
            AssertEqual([], o);
        }

        [TestMethod]
        public void CopyTo()
        {
            OrderedDictionary<string, int> o = new() { { "a", 4 }, { "b", 8 } };

            Assert.ThrowsExactly<ArgumentNullException>(() => ((ICollection<KeyValuePair<string, int>>)o).CopyTo(null, 0));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ((ICollection<KeyValuePair<string, int>>)o).CopyTo(new KeyValuePair<string, int>[3], -1));
            Assert.ThrowsExactly<ArgumentException>(() => ((ICollection<KeyValuePair<string, int>>)o).CopyTo(new KeyValuePair<string, int>[3], 3));
            Assert.ThrowsExactly<ArgumentException>(() => ((ICollection<KeyValuePair<string, int>>)o).CopyTo(new KeyValuePair<string, int>[3], 2));

            Assert.ThrowsExactly<ArgumentNullException>(() => o.Keys.CopyTo(null, 0));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.Keys.CopyTo(new string[3], -1));
            Assert.ThrowsExactly<ArgumentException>(() => o.Keys.CopyTo(new string[3], 3));
            Assert.ThrowsExactly<ArgumentException>(() => o.Keys.CopyTo(new string[3], 2));

            Assert.ThrowsExactly<ArgumentNullException>(() => o.Values.CopyTo(null, 0));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.Values.CopyTo(new int[3], -1));
            Assert.ThrowsExactly<ArgumentException>(() => o.Values.CopyTo(new int[3], 3));
            Assert.ThrowsExactly<ArgumentException>(() => o.Values.CopyTo(new int[3], 2));
        }

        [TestMethod]
        public void ContainsKvp_ChecksKeyAndValue()
        {
            OrderedDictionary<string, int> o = new() { { "a", 4 } };

            Assert.IsFalse(((ICollection<KeyValuePair<string, int>>)o).Contains(new KeyValuePair<string, int>("a", 8)));
            Assert.IsTrue(((ICollection<KeyValuePair<string, int>>)o).Contains(new KeyValuePair<string, int>("a", 4)));
        }

        [TestMethod]
        public void NullValues_Permitted()
        {
            OrderedDictionary<string, string> o = new() { { "a", "1" } };

            Assert.IsFalse(o.ContainsValue(null));

            o.Add("b", null);

            AssertEqual([new("a", "1"), new("b", null)], o);
        }

        [TestMethod]
        public void GetAt_OutOfRange()
        {
            OrderedDictionary<string, string> o = new() { { "a", "1" } };

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.GetAt(-2));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.GetAt(-1));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.GetAt(1));
        }

        [TestMethod]
        public void RemoveKvp_ChecksKeyAndValue()
        {
            OrderedDictionary<string, int> o = new() { { "a", 4 } };

            Assert.IsFalse(((ICollection<KeyValuePair<string, int>>)o).Remove(new KeyValuePair<string, int>("a", 8)));
            AssertEqual([new("a", 4)], o);

            Assert.IsTrue(((ICollection<KeyValuePair<string, int>>)o).Remove(new KeyValuePair<string, int>("a", 4)));
            AssertEqual([], o);
        }

        [TestMethod]
        public void SetAt()
        {
            OrderedDictionary<string, double> o = new();

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.SetAt(-2, 1.1));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.SetAt(-1, 1.1));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.SetAt(0, 1.1));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.SetAt(1, 1.1));

            o.Add("a", 4);

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.SetAt(-2, 1.1));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.SetAt(-1, 1.1));

            o.SetAt(0, 1.1);

            AssertEqual([new("a", 1.1)], o);

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.SetAt(1, 5.5));
        }

        [TestMethod]
        public void SetAt3Params_OutOfRange()
        {
            OrderedDictionary<string, int> o = new() { { "a", 4 }, { "b", 8 }, { "c", 12 } };

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.SetAt(-1, "d", 16));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.SetAt(3, "d", 16));
        }

        [TestMethod]
        public void SetAt3Params_ExistingKeyCorrectIndex_PermitsChangingValue()
        {
            OrderedDictionary<string, int> o = new() { { "a", 4 }, { "b", 8 }, { "c", 12 } };

            o.SetAt(2, "c", 16);

            AssertEqual([new("a", 4), new("b", 8), new("c", 16)], o);
        }

        [TestMethod]
        public void SetAt3Params_ExistingKeyDifferentIndex_Throws()
        {
            OrderedDictionary<string, int> o = new() { { "a", 4 }, { "b", 8 }, { "c", 12 } };

            Assert.ThrowsExactly<ArgumentException>(() => o.SetAt(1, "c", 16));
        }

        [TestMethod]
        public void SetAt3Params_PermitsChangingToNewKey()
        {
            OrderedDictionary<string, int> o = new() { { "a", 4 }, { "b", 8 }, { "c", 12 } };

            o.SetAt(1, "d", 16);

            AssertEqual([new("a", 4), new("d", 16), new("c", 12)], o);
        }

        [TestMethod]
        public void Get_NonExistent()
        {
            OrderedDictionary<string, float> o = new() { { "a", 4 } };

            Assert.ThrowsExactly<KeyNotFoundException>(() => o["doesn't exist"]);
            Assert.IsFalse(((ICollection<KeyValuePair<string, float>>)o).Contains(new KeyValuePair<string, float>("doesn't exist", 1)));
            Assert.IsFalse(o.ContainsKey("doesn't exist"));
            Assert.IsFalse(o.ContainsValue(999));
            Assert.AreEqual(-1, o.IndexOf("doesn't exist"));

            Assert.IsFalse(o.Remove("doesn't exist", out float value));
            Assert.AreEqual(default, value);

            Assert.IsFalse(o.Remove("doesn't exist"));

            Assert.IsFalse(((ICollection<KeyValuePair<string, float>>)o).Remove(new KeyValuePair<string, float>("doesn't exist", 1)));

            Assert.IsFalse(o.TryGetValue("doesn't exist", out value));
            Assert.AreEqual(default, value);

            Assert.IsFalse(o.TryGetValue("doesn't exist", out value, out int index));
            Assert.AreEqual(default, value);
            Assert.AreEqual(-1, index);

            AssertEqual([new("a", 4)], o);
        }

        [TestMethod]
        public void Insert()
        {
            OrderedDictionary<string, float> o = new() { { "a", 4 }, { "b", 8 } };

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.Insert(-1, "c", 12));

            o.Insert(0, "c", 12); // Start
            AssertEqual([new("c", 12), new("a", 4), new("b", 8)], o);

            o.Insert(2, "d", 12); // Middle
            AssertEqual([new("c", 12), new("a", 4), new("d", 12), new("b", 8)], o);

            o.Insert(o.Count, "e", 16); // End
            AssertEqual([new("c", 12), new("a", 4), new("d", 12), new("b", 8), new("e", 16)], o);

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.Insert(o.Count + 1, "f", 16));

            // Existing key
            Assert.ThrowsExactly<ArgumentException>(() => o.Insert(0, "a", 12));
        }

        [TestMethod]
        public void Remove_Success()
        {
            OrderedDictionary<string, float> o = new() { { "a", 4 }, { "b", 8 }, { "c", 12 } };

            Assert.IsTrue(o.Remove("b"));
            AssertEqual([new("a", 4), new("c", 12)], o);

            Assert.IsTrue(o.Remove("a", out float value));
            Assert.AreEqual(4, value);
            AssertEqual([new("c", 12)], o);

            // ICollection.Remove must match Key and Value
            Assert.IsFalse(((ICollection<KeyValuePair<string, float>>)o).Remove(new KeyValuePair<string, float>("c", -1)));
            AssertEqual([new("c", 12)], o);

            Assert.IsTrue(((ICollection<KeyValuePair<string, float>>)o).Remove(new KeyValuePair<string, float>("c", 12)));
            AssertEqual([], o);
        }

        [TestMethod]
        public void RemoveAt()
        {
            OrderedDictionary<string, float> o = new() { { "a", 4 }, { "b", 8 }, { "c", 12 }, { "d", 16 } };

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.RemoveAt(-2));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.RemoveAt(-1));

            o.RemoveAt(0); // Start
            AssertEqual([new("b", 8), new("c", 12), new("d", 16)], o);

            o.RemoveAt(1); // Middle
            AssertEqual([new("b", 8), new("d", 16)], o);

            o.RemoveAt(1); // End
            AssertEqual([new("b", 8)], o);

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.RemoveAt(1));
        }

        [TestMethod]
        public void SetPosition_ByIndex()
        {
            OrderedDictionary<string, float> o = new() { { "a", 4 }, { "b", 8 }, { "c", 12 }, { "d", 16 } };

            ArgumentOutOfRangeException ex;

            ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.SetPosition(-1, 0));
            Assert.AreEqual("index", ex.ParamName);

            ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.SetPosition(0, -1));
            Assert.AreEqual("newIndex", ex.ParamName);

            ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.SetPosition(0, 4));
            Assert.AreEqual("newIndex", ex.ParamName);

            ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.SetPosition(4, 0));
            Assert.AreEqual("index", ex.ParamName);

            o.SetPosition(1, 0);
            AssertEqual([new("b", 8), new("a", 4), new("c", 12), new("d", 16)], o);

            o.SetPosition(0, 1);
            AssertEqual([new("a", 4), new("b", 8), new("c", 12), new("d", 16)], o);

            o.SetPosition(1, 2);
            AssertEqual([new("a", 4), new("c", 12), new("b", 8), new("d", 16)], o);

            o.SetPosition(2, 1);
            AssertEqual([new("a", 4), new("b", 8), new("c", 12), new("d", 16)], o);

            o.SetPosition(0, 3);
            AssertEqual([new("b", 8), new("c", 12), new("d", 16), new("a", 4)], o);

            o.SetPosition(3, 1);
            AssertEqual([new("b", 8), new("a", 4), new("c", 12), new("d", 16)], o);

            o.SetPosition(1, 1); // No-op
            AssertEqual([new("b", 8), new("a", 4), new("c", 12), new("d", 16)], o);
        }

        [TestMethod]
        public void SetPosition_ByKey()
        {
            OrderedDictionary<string, float> o = new() { { "a", 4 }, { "b", 8 }, { "c", 12 }, { "d", 16 } };

            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.SetPosition("a", -1));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => o.SetPosition("a", 4));
            Assert.ThrowsExactly<KeyNotFoundException>(() => o.SetPosition("e", 0));

            o.SetPosition("b", 0);
            AssertEqual([new("b", 8), new("a", 4), new("c", 12), new("d", 16)], o);

            o.SetPosition("b", 1);
            AssertEqual([new("a", 4), new("b", 8), new("c", 12), new("d", 16)], o);

            o.SetPosition("a", 3);
            AssertEqual([new("b", 8), new("c", 12), new("d", 16), new("a", 4)], o);

            o.SetPosition("d", 2); // No-op
            AssertEqual([new("b", 8), new("c", 12), new("d", 16), new("a", 4)], o);
        }

        [TestMethod]
        public void TryAdd_Success()
        {
            OrderedDictionary<string, float> o = new() { { "a", 4 }, { "b", 8 } };

            Assert.IsTrue(o.TryAdd("c", 12));

            AssertEqual([new("a", 4), new("b", 8), new("c", 12)], o);

            Assert.IsTrue(o.TryAdd("d", 16, out int index));
            Assert.AreEqual(3, index);

            AssertEqual([new("a", 4), new("b", 8), new("c", 12), new("d", 16)], o);
        }

        [TestMethod]
        public void KeysAndValuesAreReadOnly()
        {
            OrderedDictionary<string, int> o = new() { { "a", 4 }, { "b", 8 } };

            Assert.IsTrue(o.Keys.IsReadOnly);
            Assert.ThrowsExactly<NotSupportedException>(() => o.Keys.Add("c"));
            Assert.ThrowsExactly<NotSupportedException>(o.Keys.Clear);
            Assert.ThrowsExactly<NotSupportedException>(() => o.Keys.Remove("a"));

            Assert.IsTrue(o.Values.IsReadOnly);
            Assert.ThrowsExactly<NotSupportedException>(() => o.Values.Add(12));
            Assert.ThrowsExactly<NotSupportedException>(o.Values.Clear);
            Assert.ThrowsExactly<NotSupportedException>(() => o.Values.Remove(4));
        }
    }
}
