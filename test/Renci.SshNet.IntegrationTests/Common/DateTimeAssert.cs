namespace Renci.SshNet.IntegrationTests.Common
{
    public static class DateTimeAssert
    {
        public static void AreEqual(DateTime expected, DateTime actual)
        {
            Assert.AreEqual(expected, actual, $"Expected {expected:o}, but was {actual:o}.");
            Assert.AreEqual(expected.Kind, actual.Kind);
        }

        public static void AreEqual(DateTime expected, DateTime actual, TimeSpan maxDelta)
        {
            TimeSpan actualDelta = (expected - actual).Duration();
            Assert.IsTrue(actualDelta <= maxDelta, $"Expected max delta of {maxDelta} between {expected:o} and {actual:o}, but was {actualDelta}");
            Assert.AreEqual(expected.Kind, actual.Kind);
        }
    }
}
