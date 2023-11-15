namespace Renci.SshNet.IntegrationTests.Common
{
    public static class DateTimeExtensions
    {
        public static DateTime TruncateToWholeSeconds(this DateTime dateTime)
        {
            return dateTime.AddMilliseconds(-dateTime.Millisecond)
                           .AddMicroseconds(-dateTime.Microsecond)
                           .AddTicks(-(dateTime.Nanosecond / 100));
        }
    }
}
