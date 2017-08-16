using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Renci.SshNet.Tests.Common
{
    public static class DictionaryAssert
    {
        public static void AreEqual<TKey, TValue>(IDictionary<TKey, TValue> expected, IDictionary<TKey, TValue> actual)
        {
            if (ReferenceEquals(expected, actual))
                return;

            if (expected == null)
                throw new AssertFailedException("Expected dictionary to be null, but was not null.");

            if (actual == null)
                throw new AssertFailedException("Expected dictionary not to be null, but was null.");

            if (expected.Count != actual.Count)
                throw new AssertFailedException(string.Format("Expected dictionary to contain {0} entries, but was {1}.",
                                                              expected.Count, actual.Count));

            foreach (var expectedEntry in expected)
            {
                TValue actualValue;
                if (!actual.TryGetValue(expectedEntry.Key, out actualValue))
                {
                    throw new AssertFailedException(string.Format("Dictionary contains no entry with key '{0}'.", expectedEntry.Key));
                }

                if (!Equals(expectedEntry.Value, actualValue))
                {
                    throw new AssertFailedException(string.Format("Value for key '{0}' does not match.", expectedEntry.Key));
                }
            }
        }
    }
}
