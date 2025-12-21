#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    internal static class AssertExtensions
    {
        extension(Assert)
        {
            public static void SequenceStartsWith<T>(
                T[] expectedPrefix,
                IEnumerable<T>? actual,
                string? message = "",
                [CallerArgumentExpression(nameof(expectedPrefix))] string expectedExpression = "",
                [CallerArgumentExpression(nameof(actual))] string actualExpression = "")
            {
                Assert.IsNotNull(actual, valueExpression: actualExpression);
                Assert.AreSequenceEqual(
                    expectedPrefix,
                    actual.Take(expectedPrefix.Length),
                    message,
                    expectedExpression,
                    actualExpression);
            }
        }
    }
}
