using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshNet.Tests.Common
{
    public static class ArgumentExceptionAssert
    {
        public static void MessageEquals(string paramName, string expected, ArgumentException exception)
        {
            var type = exception.GetType();
            var newMessage = (ArgumentException)Activator.CreateInstance(type, paramName, expected);

            Assert.AreEqual(newMessage?.Message, exception.Message);
        }
    }
}