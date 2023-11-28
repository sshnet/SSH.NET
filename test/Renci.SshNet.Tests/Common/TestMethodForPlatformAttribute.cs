using System.Runtime.InteropServices;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshNet.Tests.Common
{
    public class TestMethodForPlatformAttribute : TestMethodAttribute
    {
        public TestMethodForPlatformAttribute(string platform)
        {
            Platform = OSPlatform.Create(platform);
        }

        public OSPlatform Platform { get; set; }

        public override TestResult[] Execute(ITestMethod testMethod)
        {
            if (RuntimeInformation.IsOSPlatform(Platform))
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
