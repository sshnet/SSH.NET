using System.Diagnostics;

namespace Renci.SshNet.IntegrationTests.Issue67
{
    public class UnblockStreamUtility
    {
        internal static string[] ReadUntil(UnblockStreamReader reader, List<UntilInfo> untilInfoList, int noResponseTimeoutSeconds)
        {
            List<string> resultList = new List<string>();
            char[] buffer = new char[65536];
            int curBufferLen = 0;
            int noResponseTimeoutMilliseconds = noResponseTimeoutSeconds * 1000;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (true)
            {
                char readChar = new char();
                int readCharLen = reader.ReadChar(ref readChar);
                if (readCharLen == 0)
                {
                    Thread.Sleep(10);
                    if (stopwatch.ElapsedMilliseconds >= noResponseTimeoutMilliseconds)
                    {
                        stopwatch.Stop();
                        throw new Exception("No Response Timeout!");
                    }
                    continue;
                }
                else { stopwatch.Restart(); }
                buffer[curBufferLen] = readChar;

                foreach (UntilInfo untilInfo in untilInfoList)
                {
                    if (readChar == untilInfo.UntilCharArray[untilInfo.CompareLen])
                    {
                        untilInfo.CompareLen++;
                        if (untilInfo.CompareLen == untilInfo.UntilCharArray.Length)
                        {
                            untilInfo.CompareLen = 0;
                            string lineStr = new string(buffer, 0, curBufferLen + 1);
                            if (lineStr.EndsWith("\r\r\n"))
                            {
                                lineStr = lineStr.Substring(0, lineStr.Length - 3);
                            }
                            else if (lineStr.EndsWith("\r\n"))
                            {
                                lineStr = lineStr.Substring(0, lineStr.Length - 2);
                            }
                            else if (lineStr.EndsWith("\n"))
                            {
                                lineStr = lineStr.Substring(0, lineStr.Length - 1);
                            }

                            resultList.Add(lineStr);
                            curBufferLen = 0;

                            if (untilInfo.ExceptionMessage != null)
                            {
                                throw new Exception(untilInfo.ExceptionMessage);
                            }
                            else
                            {
                                return resultList.ToArray();
                            }
                        }
                    }
                    else
                    {
                        untilInfo.CompareLen = 0;
                    }
                }

                if (readChar == '\n')
                {
                    string lineStr = new string(buffer, 0, curBufferLen + 1);

                    if (lineStr.EndsWith("\r\r\n"))
                    {
                        lineStr = lineStr.Substring(0, lineStr.Length - 3);
                    }
                    else if (lineStr.EndsWith("\r\n"))
                    {
                        lineStr = lineStr.Substring(0, lineStr.Length - 2);
                    }
                    else if (lineStr.EndsWith("\n"))
                    {
                        lineStr = lineStr.Substring(0, lineStr.Length - 1);
                    }

                    resultList.Add(lineStr);

                    curBufferLen = 0;
                    foreach (UntilInfo untilInfo in untilInfoList)
                    {
                        untilInfo.CompareLen = 0;
                    }
                    continue;
                }
                curBufferLen++;
            }
        }
    }
}
