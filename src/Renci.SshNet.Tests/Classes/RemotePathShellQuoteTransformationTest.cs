using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class RemotePathShellQuoteTransformationTest
    {
        private IRemotePathTransformation _transformation;

        [TestInitialize]
        public void SetUp()
        {
            _transformation = new RemotePathShellQuoteTransformation();
        }

        /// <summary>
        /// Test cases from triple-slash comments
        /// </summary>
        [TestMethod]
        public void Mixed()
        {
            Assert.AreEqual("'/var/log/auth.log'", _transformation.Transform("/var/log/auth.log"));
            Assert.AreEqual("'/var/mp3/Guns N'\"'\"' Roses'", _transformation.Transform("/var/mp3/Guns N' Roses"));
            Assert.AreEqual("'/var/garbage'\\!'/temp'", _transformation.Transform("/var/garbage!/temp"));
            Assert.AreEqual("'/var/would be '\"'\"'kewl'\"'\"\\!', not?'", _transformation.Transform("/var/would be 'kewl'!, not?"));
            Assert.AreEqual("''", _transformation.Transform(string.Empty));
            Assert.AreEqual("'Hello \"World\"'", _transformation.Transform("Hello \"World\""));
        }

        [TestMethod]
        public void Null()
        {
            const string path = null;

            try
            {
                _transformation.Transform(path);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("path", ex.ParamName);
            }
        }

        [TestMethod]
        public void Ampersand_Embedded()
        {
            const string path = "You&Me";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'You&Me'", actual);
        }

        [TestMethod]
        public void Ampersand_Leading()
        {
            const string path = "&Or";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'&Or'", actual);
        }

        [TestMethod]
        public void Ampersand_LeadingAndTrailing()
        {
            const string path = "&Or&";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'&Or&'", actual);
        }

        [TestMethod]
        public void Ampersand_Trailing()
        {
            const string path = "And&";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'And&'", actual);
        }

        [TestMethod]
        public void Asterisk_Embedded()
        {
            const string path = "Love*Hate";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Love*Hate'", actual);
        }

        [TestMethod]
        public void Asterisk_Leading()
        {
            const string path = "*Times";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'*Times'", actual);
        }

        [TestMethod]
        public void Asterisk_LeadingAndTrailing()
        {
            const string path = "*WAR*";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'*WAR*'", actual);
        }

        [TestMethod]
        public void Asterisk_Trailing()
        {
            const string path = "Censor*";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Censor*'", actual);
        }

        [TestMethod]
        public void Backslash_Embedded()
        {
            const string path = "Hello\\World";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Hello\\World'", actual);
        }

        [TestMethod]
        public void Backslash_Leading()
        {
            const string path = "\\Hello";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'\\Hello'", actual);
        }

        [TestMethod]
        public void Backslash_LeadingAndTrailing()
        {
            const string path = "\\Hello\\";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'\\Hello\\'", actual);
        }

        [TestMethod]
        public void Backslash_Trailing()
        {
            const string path = "HelloWorld\\";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'HelloWorld\\'", actual);
        }

        [TestMethod]
        public void Backtick_Embedded()
        {
            const string path = "back`tick";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'back`tick'", actual);
        }

        [TestMethod]
        public void Backtick_Leading()
        {
            const string path = "`front";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'`front'", actual);
        }

        [TestMethod]
        public void Backtick_LeadingAndTrailing()
        {
            const string path = "`FrontAndBack`";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'`FrontAndBack`'", actual);
        }

        [TestMethod]
        public void Backtick_Trailing()
        {
            const string path = "back`";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'back`'", actual);
        }

        [TestMethod]
        public void Circumflex_Embedded()
        {
            const string path = "You^Me";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'You^Me'", actual);
        }

        [TestMethod]
        public void Circumflex_Leading()
        {
            const string path = "^Or";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'^Or'", actual);
        }

        [TestMethod]
        public void Circumflex_LeadingAndTrailing()
        {
            const string path = "^Or^";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'^Or^'", actual);
        }

        [TestMethod]
        public void Circumflex_Trailing()
        {
            const string path = "And^";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'And^'", actual);
        }

        [TestMethod]
        public void CurlyBrackets_Close_Embedded()
        {
            const string path = "Halo}Devine";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Halo}Devine'", actual);
        }

        [TestMethod]
        public void CurlyBrackets_Close_Leading()
        {
            const string path = "}Open";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'}Open'", actual);
        }

        [TestMethod]
        public void CurlyBrackets_Close_LeadingAndTrailing()
        {
            const string path = "}Closed}";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'}Closed}'", actual);
        }

        [TestMethod]
        public void CurlyBrackets_Close_Trailing()
        {
            const string path = "Finish}";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Finish}'", actual);
        }

        [TestMethod]
        public void CurlyBrackets_Open_Embedded()
        {
            const string path = "Halo{Devine";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Halo{Devine'", actual);
        }

        [TestMethod]
        public void CurlyBrackets_Open_Leading()
        {
            const string path = "{Open";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'{Open'", actual);
        }

        [TestMethod]
        public void CurlyBrackets_Open_LeadingAndTrailing()
        {
            const string path = "{Closed{";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'{Closed{'", actual);
        }

        [TestMethod]
        public void CurlyBrackets_Open_Trailing()
        {
            const string path = "Finish{";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Finish{'", actual);
        }

        [TestMethod]
        public void Dollar_Embedded()
        {
            const string path = "IGiveYouOne$ForYourThoughts";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'IGiveYouOne$ForYourThoughts'", actual);
        }

        [TestMethod]
        public void Dollar_Leading()
        {
            const string path = "$Blues";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'$Blues'", actual);
        }

        [TestMethod]
        public void Dollar_LeadingAndTrailing()
        {
            const string path = "$SUM$";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'$SUM$'", actual);
        }

        [TestMethod]
        public void Dollar_Trailing()
        {
            const string path = "NotCravingForMore$";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'NotCravingForMore$'", actual);
        }

        [TestMethod]
        public void DoubleQuote_Embedded()
        {
            const string path = "DoNot\"MeOnThis";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'DoNot\"MeOnThis'", actual);
        }

        [TestMethod]
        public void DoubleQuote_Leading()
        {
            const string path = "\"OrNotToQuote";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'\"OrNotToQuote'", actual);
        }

        [TestMethod]
        public void DoubleQuote_LeadingAndTrailing()
        {
            const string path = "\"OrNotTo\"";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'\"OrNotTo\"'", actual);
        }

        [TestMethod]
        public void DoubleQuote_Trailing()
        {
            const string path = "Famous\"";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Famous\"'", actual);
        }

        [TestMethod]
        public void Equals_Embedded()
        {
            const string path = "You=Me";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'You=Me'", actual);
        }

        [TestMethod]
        public void Equals_Leading()
        {
            const string path = "=Or";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'=Or'", actual);
        }

        [TestMethod]
        public void Equals_LeadingAndTrailing()
        {
            const string path = "=Or=";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'=Or='", actual);
        }

        [TestMethod]
        public void Equals_Trailing()
        {
            const string path = "And=";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'And='", actual);
        }

        [TestMethod]
        public void ExclamationMark_Embedded_Single()
        {
            const string path = "/var/garbage!/temp";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'/var/garbage'\\!'/temp'", actual);
        }

        [TestMethod]
        public void ExclamationMark_Embedded_Sequence()
        {
            const string path = "/var/garbage!!/temp";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'/var/garbage'\\!\\!'/temp'", actual);
        }

        [TestMethod]
        public void ExclamationMark_Leading()
        {
            const string path = "!Error";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("\\!'Error'", actual);
        }

        [TestMethod]
        public void ExclamationMark_LeadingAndTrailing()
        {
            const string path = "!ignore!";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("\\!'ignore'\\!", actual);
        }

        [TestMethod]
        public void ExclamationMark_Trailing()
        {
            const string path = "Done!";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Done'\\!", actual);
        }

        [TestMethod]
        public void GreaterThan_Embedded()
        {
            const string path = "You>Me";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'You>Me'", actual);
        }

        [TestMethod]
        public void GreaterThan_Leading()
        {
            const string path = ">Or";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'>Or'", actual);
        }

        [TestMethod]
        public void GreaterThan_LeadingAndTrailing()
        {
            const string path = ">Or>";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'>Or>'", actual);
        }

        [TestMethod]
        public void GreaterThan_Trailing()
        {
            const string path = "And>";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'And>'", actual);
        }

        [TestMethod]
        public void Hash_Embedded()
        {
            const string path = "Smoke#EveryDay";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Smoke#EveryDay'", actual);
        }

        [TestMethod]
        public void Hash_Leading()
        {
            const string path = "#4Ever";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'#4Ever'", actual);
        }

        [TestMethod]
        public void Hash_LeadingAndTrailing()
        {
            const string path = "#4Ever#";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'#4Ever#'", actual);
        }

        [TestMethod]
        public void Hash_Trailing()
        {
            const string path = "Legalize#";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Legalize#'", actual);
        }

        [TestMethod]
        public void LessThan_Embedded()
        {
            const string path = "You<Me";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'You<Me'", actual);
        }

        [TestMethod]
        public void LessThan_Leading()
        {
            const string path = "<Or";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'<Or'", actual);
        }

        [TestMethod]
        public void LessThan_LeadingAndTrailing()
        {
            const string path = "<Or<";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'<Or<'", actual);
        }

        [TestMethod]
        public void LessThan_Trailing()
        {
            const string path = "And<";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'And<'", actual);
        }

        [TestMethod]
        public void NewLine_Embedded()
        {
            const string path = "line\nfeed";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'line\nfeed'", actual);
        }

        [TestMethod]
        public void NewLine_Leading()
        {
            const string path = "\nFooter";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'\nFooter'", actual);
        }

        [TestMethod]
        public void NewLine_LeadingAndTrailing()
        {
            const string path = "\nBanner\n";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'\nBanner\n'", actual);
        }

        [TestMethod]
        public void NewLine_Trailing()
        {
            const string path = "Header\n";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Header\n'", actual);
        }

        [TestMethod]
        public void Parentheses_Close_Embedded()
        {
            const string path = "Halo)Devine";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Halo)Devine'", actual);
        }

        [TestMethod]
        public void Parentheses_Close_Leading()
        {
            const string path = ")Open";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("')Open'", actual);
        }

        [TestMethod]
        public void Parentheses_Close_LeadingAndTrailing()
        {
            const string path = ")Closed)";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("')Closed)'", actual);
        }

        [TestMethod]
        public void Parentheses_Close_Trailing()
        {
            const string path = "Finish)";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Finish)'", actual);
        }

        [TestMethod]
        public void Parentheses_Open_Embedded()
        {
            const string path = "Halo(Devine";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Halo(Devine'", actual);
        }

        [TestMethod]
        public void Parentheses_Open_Leading()
        {
            const string path = "(Open";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'(Open'", actual);
        }

        [TestMethod]
        public void Parentheses_Open_LeadingAndTrailing()
        {
            const string path = "(Closed(";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'(Closed('", actual);
        }

        [TestMethod]
        public void Parentheses_Open_Trailing()
        {
            const string path = "Finish(";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Finish('", actual);
        }

        [TestMethod]
        public void Percentage_Embedded()
        {
            const string path = "Ten%OfOneDollarIsTenCent";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Ten%OfOneDollarIsTenCent'", actual);
        }

        [TestMethod]
        public void Percentage_Leading()
        {
            const string path = "%MoreOrLess";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'%MoreOrLess'", actual);
        }

        [TestMethod]
        public void Percentage_LeadingAndTrailing()
        {
            const string path = "%USERNAME%";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'%USERNAME%'", actual);
        }

        [TestMethod]
        public void Percentage_Trailing()
        {
            const string path = "TakeA%";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'TakeA%'", actual);
        }

        [TestMethod]
        public void Pipe_Embedded()
        {
            const string path = "You|Me";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'You|Me'", actual);
        }

        [TestMethod]
        public void Pipe_Leading()
        {
            const string path = "|Or";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'|Or'", actual);
        }

        [TestMethod]
        public void Pipe_LeadingAndTrailing()
        {
            const string path = "|Or|";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'|Or|'", actual);
        }

        [TestMethod]
        public void Pipe_Trailing()
        {
            const string path = "And|";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'And|'", actual);
        }

        [TestMethod]
        public void QuestionMark_Embedded()
        {
            const string path = "WhatTimeIsIt?SheSaid";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'WhatTimeIsIt?SheSaid'", actual);
        }

        [TestMethod]
        public void QuestionMark_Leading()
        {
            const string path = "?Quizz";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'?Quizz'", actual);
        }

        [TestMethod]
        public void QuestionMark_LeadingAndTrailing()
        {
            const string path = "?Crazy?";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'?Crazy?'", actual);
        }

        [TestMethod]
        public void QuestionMark_Trailing()
        {
            const string path = "WhatTimeIsLove?";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'WhatTimeIsLove?'", actual);
        }

        [TestMethod]
        public void Semicolon_Embedded()
        {
            const string path = "Rain;Storm";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Rain;Storm'", actual);
        }

        [TestMethod]
        public void Semicolon_Leading()
        {
            const string path = ";List";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("';List'", actual);
        }

        [TestMethod]
        public void Semicolon_LeadingAndTrailing()
        {
            const string path = ";Trapped;";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("';Trapped;'", actual);
        }

        [TestMethod]
        public void Semicolon_Trailing()
        {
            const string path = "Time;";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Time;'", actual);
        }

        [TestMethod]
        public void SingleQuote_Embedded_Single()
        {
            const string path = "Rain'Storm";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Rain'\"'\"'Storm'", actual);
        }

        [TestMethod]
        public void SingleQuote_Embedded_Sequence()
        {
            const string path = "Rain''Storm";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Rain'\"''\"'Storm'", actual);
        }

        [TestMethod]
        public void SingleQuote_Leading()
        {
            const string path = "'List";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("\"'\"'List'", actual);
        }

        [TestMethod]
        public void SingleQuote_LeadingAndTrailing()
        {
            const string path = "'Trapped'";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("\"'\"'Trapped'\"'\"", actual);
        }

        [TestMethod]
        public void SingleQuote_Trailing()
        {
            const string path = "Time'";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Time'\"'\"", actual);
        }

        [TestMethod]
        public void SingleQuoteAndExclamationMark_Embedded()
        {
            const string path = "Rain'!Storm";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Rain'\"'\"\\!'Storm'", actual);
        }

        [TestMethod]
        public void SingleQuoteAndExclamationMark_Leading()
        {
            const string path = "'!Rain";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("\"'\"\\!'Rain'", actual);
        }

        [TestMethod]
        public void SingleQuoteAndExclamationMark_LeadingAndTrailing()
        {
            const string path = "'!Rain'!";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("\"'\"\\!'Rain'\"'\"\\!", actual);
        }

        [TestMethod]
        public void SingleQuoteAndExclamationMark_Trailing()
        {
            const string path = "Rain'!";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Rain'\"'\"\\!", actual);
        }

        [TestMethod]
        public void Space_Embedded()
        {
            const string path = "You Me";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'You Me'", actual);
        }

        [TestMethod]
        public void Space_Leading()
        {
            const string path = " Or";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("' Or'", actual);
        }

        [TestMethod]
        public void Space_LeadingAndTrailing()
        {
            const string path = " Or ";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("' Or '", actual);
        }

        [TestMethod]
        public void Space_Trailing()
        {
            const string path = "And ";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'And '", actual);
        }

        [TestMethod]
        public void SquareBrackets_Close_Embedded()
        {
            const string path = "Halo]Devine";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Halo]Devine'", actual);
        }

        [TestMethod]
        public void SquareBrackets_Close_Leading()
        {
            const string path = "]Open";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("']Open'", actual);
        }

        [TestMethod]
        public void SquareBrackets_Close_LeadingAndTrailing()
        {
            const string path = "]Closed]";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("']Closed]'", actual);
        }

        [TestMethod]
        public void SquareBrackets_Close_Trailing()
        {
            const string path = "Finish]";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Finish]'", actual);
        }

        [TestMethod]
        public void SquareBrackets_Open_Embedded()
        {
            const string path = "Halo[Devine";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Halo[Devine'", actual);
        }

        [TestMethod]
        public void SquareBrackets_Open_Leading()
        {
            const string path = "[Open";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'[Open'", actual);
        }

        [TestMethod]
        public void SquareBrackets_Open_LeadingAndTrailing()
        {
            const string path = "[Closed[";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'[Closed['", actual);
        }

        [TestMethod]
        public void SquareBrackets_Open_Trailing()
        {
            const string path = "Finish[";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Finish['", actual);
        }

        [TestMethod]
        public void Tab_Embedded()
        {
            const string path = "You\tMe";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'You\tMe'", actual);
        }

        [TestMethod]
        public void Tab_Leading()
        {
            const string path = "\tOr";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'\tOr'", actual);
        }

        [TestMethod]
        public void Tab_LeadingAndTrailing()
        {
            const string path = "\tOr\t";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'\tOr\t'", actual);
        }

        [TestMethod]
        public void Tab_Trailing()
        {
            const string path = "And\t";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'And\t'", actual);
        }

        [TestMethod]
        public void Tilde_Embedded()
        {
            const string path = "Seven~Nine";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Seven~Nine'", actual);
        }

        [TestMethod]
        public void Tilde_Leading()
        {
            const string path = "~Ten";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'~Ten'", actual);
        }

        [TestMethod]
        public void Tilde_LeadingAndTrailing()
        {
            const string path = "~One~";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'~One~'", actual);
        }

        [TestMethod]
        public void Tilde_Trailing()
        {
            const string path = "Two~";

            var actual = _transformation.Transform(path);

            Assert.AreEqual("'Two~'", actual);
        }
    }
}
