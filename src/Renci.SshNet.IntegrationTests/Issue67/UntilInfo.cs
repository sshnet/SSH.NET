namespace Renci.SshNet.IntegrationTests.Issue67
{
    public class UntilInfo
    {
        public UntilInfo(string untilString, string exceptionMessage = null)
        {
            UntilString = untilString;
            ExceptionMessage = exceptionMessage;
            UntilCharArray = untilString.ToCharArray();
            CompareLen = 0;
        }

        public string UntilString
        {
            get;
            private set;
        }

        public string ExceptionMessage
        {
            get;
            private set;
        }

        public char[] UntilCharArray
        {
            get;
            private set;
        }

        public int CompareLen
        {
            get;
            set;
        }
    }
}
