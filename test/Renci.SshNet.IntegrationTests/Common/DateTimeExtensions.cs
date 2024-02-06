namespace Renci.SshNet.IntegrationTests.Common
{
    public static class DateTimeExtensions
    {
        public static DateTime TruncateToWholeSeconds(this DateTime dateTime)
        {
            return new DateTime(dateTime.Ticks - (dateTime.Ticks % TimeSpan.TicksPerSecond), dateTime.Kind);
        }
    }
}
