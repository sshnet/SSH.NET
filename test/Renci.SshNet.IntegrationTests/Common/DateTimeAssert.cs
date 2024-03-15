namespace Renci.SshNet.IntegrationTests.Common
{
    internal static class DateTimeAssert
    {
        public static void AreEqual(DateTime expected, DateTime actual)
        {
            Assert.AreEqual(expected, actual, $"Expected {expected:o}, but was {actual:o}.");
            Assert.AreEqual(expected.Kind, actual.Kind);
        }
    }
}
