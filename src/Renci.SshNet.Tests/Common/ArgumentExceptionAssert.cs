using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshNet.Tests.Common
{
    public static class ArgumentExceptionAssert
    {
        public static void MessageEquals(string paramName, string expected, ArgumentException exception)
        {
            var newMessage = new ArgumentException(expected, paramName);

            Assert.AreEqual(newMessage.Message, exception.Message);
        }
    }
}