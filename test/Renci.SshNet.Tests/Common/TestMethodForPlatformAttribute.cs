using System;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshNet.Tests.Common
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TestMethodForPlatformAttribute : TestMethodAttribute
    {
        public TestMethodForPlatformAttribute(string platform)
        {
            Platform = platform;
        }

        public string Platform { get; }

        public override TestResult[] Execute(ITestMethod testMethod)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Create(Platform)))
            {
                return base.Execute(testMethod);
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
