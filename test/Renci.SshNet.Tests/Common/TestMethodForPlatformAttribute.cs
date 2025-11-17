using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshNet.Tests.Common
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TestMethodForPlatformAttribute : TestMethodAttribute
    {
        public TestMethodForPlatformAttribute(string platform, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1) : base(callerFilePath, callerLineNumber)
        {
            Platform = platform;
        }

        public string Platform { get; }

        public override async Task<TestResult[]> ExecuteAsync(ITestMethod testMethod)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Create(Platform)))
            {
                return await base.ExecuteAsync(testMethod);
            }

            var message = $"Test not executed. The test is intended for the '{Platform}' platform only.";
            return new[]
                {
                    new TestResult
                        {
                            Outcome = UnitTestOutcome.Inconclusive,
                            TestFailureException = new AssertInconclusiveException(message)
                        }
                };

        }
    }
}
