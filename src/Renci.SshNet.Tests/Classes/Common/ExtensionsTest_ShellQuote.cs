using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class ExtensionsTest_ShellQuote
    {
        [TestMethod]
        public void Null()
        {
            const string value = null;

            try
            {
                value.ShellQuote();
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("value", ex.ParamName);
            }
        }

        [TestMethod]
        public void Empty()
        {
            var value = string.Empty;

            var actual = value.ShellQuote();

            Assert.AreEqual("''", actual);
        }

        [TestMethod]
        public void RegularCharacters()
        {
            var value = "onetwo";

            var actual = value.ShellQuote();

            Assert.AreEqual("'onetwo'", actual);
        }

        /// <summary>
        /// Tests all special character listed <a href="http://pubs.opengroup.org/onlinepubs/7908799/xcu/chap2.html">here</a>
        /// except for newline and single-quote, which are tested separately.
        /// </summary>
        [TestMethod]
        public void SpecialCharacters()
        {
            var value = "|&;<>()$`\\\" \t\n*?[#~=%";

            var actual = value.ShellQuote();

            Assert.AreEqual("'|&;<>()$`\\\" \t\n*?[#~=%'", actual);
        }

        [TestMethod]
        public void SingleExclamationPoint()
        {
            var value = "!one!two!";

            var actual = value.ShellQuote();

            Assert.AreEqual("\\!'one'\\!'two'\\!", actual);
        }

        [TestMethod]
        public void SequenceOfExclamationPoints()
        {
            var value = "one!!!two";

            var actual = value.ShellQuote();

            Assert.AreEqual("'one'\\!\\!\\!'two'", actual);
        }

        [TestMethod]
        public void SingleQuotes()
        {
            var value = "'a'b'c'd'";

            var actual = value.ShellQuote();

            Assert.AreEqual("\"'\"'a'\"'\"'b'\"'\"'c'\"'\"'d'\"'\"", actual);
        }

        [TestMethod]
        public void SequenceOfSingleQuotes()
        {
            var value = "one''two";

            var actual = value.ShellQuote();

            Assert.AreEqual("'one'\"''\"'two'", actual);
        }

        [TestMethod]
        public void LineFeeds()
        {
            var value = "one\ntwo\nthree\nfour";

            var actual = value.ShellQuote();

            Assert.AreEqual("'one\ntwo\nthree\nfour'", actual);
        }

        [TestMethod]
        public void SequenceOfLineFeeds()
        {
            var value = "one\n\ntwo";

            var actual = value.ShellQuote();

            Assert.AreEqual("'one\n\ntwo'", actual);
        }

        public void SequenceOfSingleQuoteAndExclamationMark()
        {
            var value = "/var/would be 'kewl'!/not?";

            var actual = value.ShellQuote();

            Assert.AreEqual("'/var/would be '\"'\"'kewl'\"'\"\\!'/not?'", actual);
        }
    }
}
