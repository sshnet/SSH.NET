using Renci.SshNet.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    ///This is a test class for BigIntegerTest and is intended
    ///to contain all BigIntegerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BigIntegerTest : TestBase
    {
        /// <summary>
        ///A test for BigInteger Constructor
        ///</summary>
        [TestMethod()]
        public void BigIntegerConstructorTest()
        {
            short sign = 0; // TODO: Initialize to an appropriate value
            uint[] data = null; // TODO: Initialize to an appropriate value
            BigInteger target = new BigInteger(sign, data);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for BigInteger Constructor
        ///</summary>
        [TestMethod()]
        public void BigIntegerConstructorTest1()
        {
            int value = 0; // TODO: Initialize to an appropriate value
            BigInteger target = new BigInteger(value);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for BigInteger Constructor
        ///</summary>
        [TestMethod()]
        public void BigIntegerConstructorTest2()
        {
            uint value = 0; // TODO: Initialize to an appropriate value
            BigInteger target = new BigInteger(value);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for BigInteger Constructor
        ///</summary>
        [TestMethod()]
        public void BigIntegerConstructorTest3()
        {
            long value = 0; // TODO: Initialize to an appropriate value
            BigInteger target = new BigInteger(value);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for BigInteger Constructor
        ///</summary>
        [TestMethod()]
        public void BigIntegerConstructorTest4()
        {
            ulong value = 0; // TODO: Initialize to an appropriate value
            BigInteger target = new BigInteger(value);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for BigInteger Constructor
        ///</summary>
        [TestMethod()]
        public void BigIntegerConstructorTest5()
        {
            double value = 0F; // TODO: Initialize to an appropriate value
            BigInteger target = new BigInteger(value);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for BigInteger Constructor
        ///</summary>
        [TestMethod()]
        public void BigIntegerConstructorTest6()
        {
            float value = 0F; // TODO: Initialize to an appropriate value
            BigInteger target = new BigInteger(value);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for BigInteger Constructor
        ///</summary>
        [TestMethod()]
        public void BigIntegerConstructorTest7()
        {
            Decimal value = new Decimal(); // TODO: Initialize to an appropriate value
            BigInteger target = new BigInteger(value);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for BigInteger Constructor
        ///</summary>
        [TestMethod()]
        public void BigIntegerConstructorTest8()
        {
            byte[] value = null; // TODO: Initialize to an appropriate value
            BigInteger target = new BigInteger(value);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Abs
        ///</summary>
        [TestMethod()]
        public void AbsTest()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = BigInteger.Abs(value);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Add
        ///</summary>
        [TestMethod()]
        public void AddTest()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = BigInteger.Add(left, right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Compare
        ///</summary>
        [TestMethod()]
        public void CompareTest()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = BigInteger.Compare(left, right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CompareTo
        ///</summary>
        [TestMethod()]
        public void CompareToTest()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            long other = 0; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.CompareTo(other);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CompareTo
        ///</summary>
        [TestMethod()]
        public void CompareToTest1()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            ulong other = 0; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.CompareTo(other);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CompareTo
        ///</summary>
        [TestMethod()]
        public void CompareToTest2()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger other = new BigInteger(); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.CompareTo(other);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for CompareTo
        ///</summary>
        [TestMethod()]
        public void CompareToTest3()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            object obj = null; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.CompareTo(obj);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for DivRem
        ///</summary>
        [TestMethod()]
        public void DivRemTest()
        {
            BigInteger dividend = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger divisor = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger remainder = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger remainderExpected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = BigInteger.DivRem(dividend, divisor, out remainder);
            Assert.AreEqual(remainderExpected, remainder);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Divide
        ///</summary>
        [TestMethod()]
        public void DivideTest()
        {
            BigInteger dividend = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger divisor = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = BigInteger.Divide(dividend, divisor);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [TestMethod()]
        public void EqualsTest()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger other = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Equals(other);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [TestMethod()]
        public void EqualsTest1()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            ulong other = 0; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Equals(other);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [TestMethod()]
        public void EqualsTest2()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            object obj = null; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Equals
        ///</summary>
        [TestMethod()]
        public void EqualsTest3()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            long other = 0; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.Equals(other);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GetHashCode
        ///</summary>
        [TestMethod()]
        public void GetHashCodeTest()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.GetHashCode();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for GreatestCommonDivisor
        ///</summary>
        [TestMethod()]
        public void GreatestCommonDivisorTest()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = BigInteger.GreatestCommonDivisor(left, right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Log
        ///</summary>
        [TestMethod()]
        public void LogTest()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            double baseValue = 0F; // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            actual = BigInteger.Log(value, baseValue);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Log
        ///</summary>
        [TestMethod()]
        public void LogTest1()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            actual = BigInteger.Log(value);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Log10
        ///</summary>
        [TestMethod()]
        public void Log10Test()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            actual = BigInteger.Log10(value);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Max
        ///</summary>
        [TestMethod()]
        public void MaxTest()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = BigInteger.Max(left, right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Min
        ///</summary>
        [TestMethod()]
        public void MinTest()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = BigInteger.Min(left, right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ModInverse
        ///</summary>
        [TestMethod()]
        public void ModInverseTest()
        {
            BigInteger bi = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger modulus = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = BigInteger.ModInverse(bi, modulus);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ModPow
        ///</summary>
        [TestMethod()]
        public void ModPowTest()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger exponent = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger modulus = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = BigInteger.ModPow(value, exponent, modulus);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Multiply
        ///</summary>
        [TestMethod()]
        public void MultiplyTest()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = BigInteger.Multiply(left, right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Negate
        ///</summary>
        [TestMethod()]
        public void NegateTest()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = BigInteger.Negate(value);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseTest()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            IFormatProvider provider = null; // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = BigInteger.Parse(value, provider);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseTest1()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            NumberStyles style = new NumberStyles(); // TODO: Initialize to an appropriate value
            IFormatProvider provider = null; // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = BigInteger.Parse(value, style, provider);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Parse
        ///</summary>
        [TestMethod()]
        public void ParseTest2()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            NumberStyles style = new NumberStyles(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = BigInteger.Parse(value, style);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for PositiveMod
        ///</summary>
        [TestMethod()]
        public void PositiveModTest()
        {
            BigInteger dividend = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger divisor = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = BigInteger.PositiveMod(dividend, divisor);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Pow
        ///</summary>
        [TestMethod()]
        public void PowTest()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            int exponent = 0; // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = BigInteger.Pow(value, exponent);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Random
        ///</summary>
        [TestMethod()]
        public void RandomTest()
        {
            int bitLength = 0; // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = BigInteger.Random(bitLength);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Remainder
        ///</summary>
        [TestMethod()]
        public void RemainderTest()
        {
            BigInteger dividend = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger divisor = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = BigInteger.Remainder(dividend, divisor);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Subtract
        ///</summary>
        [TestMethod()]
        public void SubtractTest()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = BigInteger.Subtract(left, right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ToByteArray
        ///</summary>
        [TestMethod()]
        public void ToByteArrayTest()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.ToByteArray();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void ToStringTest()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            string format = string.Empty; // TODO: Initialize to an appropriate value
            IFormatProvider provider = null; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.ToString(format, provider);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void ToStringTest1()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            string format = string.Empty; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.ToString(format);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void ToStringTest2()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            IFormatProvider provider = null; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.ToString(provider);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for ToString
        ///</summary>
        [TestMethod()]
        public void ToStringTest3()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for TryParse
        ///</summary>
        [TestMethod()]
        public void TryParseTest()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            NumberStyles style = new NumberStyles(); // TODO: Initialize to an appropriate value
            CultureInfo cultureInfo = null; // TODO: Initialize to an appropriate value
            BigInteger result = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger resultExpected = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = BigInteger.TryParse(value, style, cultureInfo, out result);
            Assert.AreEqual(resultExpected, result);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for TryParse
        ///</summary>
        [TestMethod()]
        public void TryParseTest1()
        {
            string value = string.Empty; // TODO: Initialize to an appropriate value
            BigInteger result = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger resultExpected = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = BigInteger.TryParse(value, out result);
            Assert.AreEqual(resultExpected, result);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Addition
        ///</summary>
        [TestMethod()]
        public void op_AdditionTest()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = (left + right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_BitwiseAnd
        ///</summary>
        [TestMethod()]
        public void op_BitwiseAndTest()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = (left & right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_BitwiseOr
        ///</summary>
        [TestMethod()]
        public void op_BitwiseOrTest()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = (left | right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Decrement
        ///</summary>
        [TestMethod()]
        public void op_DecrementTest()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = --(value);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Division
        ///</summary>
        [TestMethod()]
        public void op_DivisionTest()
        {
            BigInteger dividend = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger divisor = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = (dividend / divisor);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Equality
        ///</summary>
        [TestMethod()]
        public void op_EqualityTest()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left == right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Equality
        ///</summary>
        [TestMethod()]
        public void op_EqualityTest1()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            long right = 0; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left == right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Equality
        ///</summary>
        [TestMethod()]
        public void op_EqualityTest2()
        {
            long left = 0; // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left == right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Equality
        ///</summary>
        [TestMethod()]
        public void op_EqualityTest3()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            ulong right = 0; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left == right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Equality
        ///</summary>
        [TestMethod()]
        public void op_EqualityTest4()
        {
            ulong left = 0; // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left == right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_ExclusiveOr
        ///</summary>
        [TestMethod()]
        public void op_ExclusiveOrTest()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = (left ^ right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [TestMethod()]
        public void op_ExplicitTest()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            short expected = 0; // TODO: Initialize to an appropriate value
            short actual;
            actual = ((short)(value));
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [TestMethod()]
        public void op_ExplicitTest1()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            ushort expected = 0; // TODO: Initialize to an appropriate value
            ushort actual;
            actual = ((ushort)(value));
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [TestMethod()]
        public void op_ExplicitTest2()
        {
            Decimal value = new Decimal(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = ((BigInteger)(value));
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [TestMethod()]
        public void op_ExplicitTest3()
        {
            float value = 0F; // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = ((BigInteger)(value));
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [TestMethod()]
        public void op_ExplicitTest4()
        {
            double value = 0F; // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = ((BigInteger)(value));
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [TestMethod()]
        public void op_ExplicitTest5()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            Decimal expected = new Decimal(); // TODO: Initialize to an appropriate value
            Decimal actual;
            actual = ((Decimal)(value));
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [TestMethod()]
        public void op_ExplicitTest6()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            float expected = 0F; // TODO: Initialize to an appropriate value
            float actual;
            actual = ((float)(value));
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [TestMethod()]
        public void op_ExplicitTest7()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            double expected = 0F; // TODO: Initialize to an appropriate value
            double actual;
            actual = ((double)(value));
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [TestMethod()]
        public void op_ExplicitTest8()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            ulong expected = 0; // TODO: Initialize to an appropriate value
            ulong actual;
            actual = ((ulong)(value));
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [TestMethod()]
        public void op_ExplicitTest9()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            long expected = 0; // TODO: Initialize to an appropriate value
            long actual;
            actual = ((long)(value));
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [TestMethod()]
        public void op_ExplicitTest10()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            sbyte expected = 0; // TODO: Initialize to an appropriate value
            sbyte actual;
            actual = ((sbyte)(value));
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [TestMethod()]
        public void op_ExplicitTest11()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            byte expected = 0; // TODO: Initialize to an appropriate value
            byte actual;
            actual = ((byte)(value));
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [TestMethod()]
        public void op_ExplicitTest12()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = ((int)(value));
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Explicit
        ///</summary>
        [TestMethod()]
        public void op_ExplicitTest13()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            uint expected = 0; // TODO: Initialize to an appropriate value
            uint actual;
            actual = ((uint)(value));
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_GreaterThan
        ///</summary>
        [TestMethod()]
        public void op_GreaterThanTest()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            long right = 0; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left > right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_GreaterThan
        ///</summary>
        [TestMethod()]
        public void op_GreaterThanTest1()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            ulong right = 0; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left > right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_GreaterThan
        ///</summary>
        [TestMethod()]
        public void op_GreaterThanTest2()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left > right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_GreaterThan
        ///</summary>
        [TestMethod()]
        public void op_GreaterThanTest3()
        {
            long left = 0; // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left > right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_GreaterThan
        ///</summary>
        [TestMethod()]
        public void op_GreaterThanTest4()
        {
            ulong left = 0; // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left > right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_GreaterThanOrEqual
        ///</summary>
        [TestMethod()]
        public void op_GreaterThanOrEqualTest()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left >= right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_GreaterThanOrEqual
        ///</summary>
        [TestMethod()]
        public void op_GreaterThanOrEqualTest1()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            long right = 0; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left >= right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_GreaterThanOrEqual
        ///</summary>
        [TestMethod()]
        public void op_GreaterThanOrEqualTest2()
        {
            long left = 0; // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left >= right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_GreaterThanOrEqual
        ///</summary>
        [TestMethod()]
        public void op_GreaterThanOrEqualTest3()
        {
            ulong left = 0; // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left >= right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_GreaterThanOrEqual
        ///</summary>
        [TestMethod()]
        public void op_GreaterThanOrEqualTest4()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            ulong right = 0; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left >= right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [TestMethod()]
        public void op_ImplicitTest()
        {
            short value = 0; // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = value;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [TestMethod()]
        public void op_ImplicitTest1()
        {
            uint value = 0; // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = value;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [TestMethod()]
        public void op_ImplicitTest2()
        {
            int value = 0; // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = value;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [TestMethod()]
        public void op_ImplicitTest3()
        {
            ushort value = 0; // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = value;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [TestMethod()]
        public void op_ImplicitTest4()
        {
            byte value = 0; // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = value;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [TestMethod()]
        public void op_ImplicitTest5()
        {
            sbyte value = 0; // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = value;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [TestMethod()]
        public void op_ImplicitTest6()
        {
            long value = 0; // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = value;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Implicit
        ///</summary>
        [TestMethod()]
        public void op_ImplicitTest7()
        {
            ulong value = 0; // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = value;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Increment
        ///</summary>
        [TestMethod()]
        public void op_IncrementTest()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = ++(value);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Inequality
        ///</summary>
        [TestMethod()]
        public void op_InequalityTest()
        {
            long left = 0; // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left != right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Inequality
        ///</summary>
        [TestMethod()]
        public void op_InequalityTest1()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            ulong right = 0; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left != right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Inequality
        ///</summary>
        [TestMethod()]
        public void op_InequalityTest2()
        {
            ulong left = 0; // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left != right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Inequality
        ///</summary>
        [TestMethod()]
        public void op_InequalityTest3()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            long right = 0; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left != right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Inequality
        ///</summary>
        [TestMethod()]
        public void op_InequalityTest4()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left != right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_LeftShift
        ///</summary>
        [TestMethod()]
        public void op_LeftShiftTest()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            int shift = 0; // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = (value << shift);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_LessThan
        ///</summary>
        [TestMethod()]
        public void op_LessThanTest()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            ulong right = 0; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left < right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_LessThan
        ///</summary>
        [TestMethod()]
        public void op_LessThanTest1()
        {
            long left = 0; // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left < right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_LessThan
        ///</summary>
        [TestMethod()]
        public void op_LessThanTest2()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            long right = 0; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left < right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_LessThan
        ///</summary>
        [TestMethod()]
        public void op_LessThanTest3()
        {
            ulong left = 0; // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left < right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_LessThan
        ///</summary>
        [TestMethod()]
        public void op_LessThanTest4()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left < right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_LessThanOrEqual
        ///</summary>
        [TestMethod()]
        public void op_LessThanOrEqualTest()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left <= right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_LessThanOrEqual
        ///</summary>
        [TestMethod()]
        public void op_LessThanOrEqualTest1()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            long right = 0; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left <= right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_LessThanOrEqual
        ///</summary>
        [TestMethod()]
        public void op_LessThanOrEqualTest2()
        {
            long left = 0; // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left <= right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_LessThanOrEqual
        ///</summary>
        [TestMethod()]
        public void op_LessThanOrEqualTest3()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            ulong right = 0; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left <= right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_LessThanOrEqual
        ///</summary>
        [TestMethod()]
        public void op_LessThanOrEqualTest4()
        {
            ulong left = 0; // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = (left <= right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Modulus
        ///</summary>
        [TestMethod()]
        public void op_ModulusTest()
        {
            BigInteger dividend = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger divisor = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = (dividend % divisor);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Multiply
        ///</summary>
        [TestMethod()]
        public void op_MultiplyTest()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = (left * right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_OnesComplement
        ///</summary>
        [TestMethod()]
        public void op_OnesComplementTest()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = ~(value);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_RightShift
        ///</summary>
        [TestMethod()]
        public void op_RightShiftTest()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            int shift = 0; // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = (value >> shift);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_Subtraction
        ///</summary>
        [TestMethod()]
        public void op_SubtractionTest()
        {
            BigInteger left = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger right = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = (left - right);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_UnaryNegation
        ///</summary>
        [TestMethod()]
        public void op_UnaryNegationTest()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = -(value);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for op_UnaryPlus
        ///</summary>
        [TestMethod()]
        public void op_UnaryPlusTest()
        {
            BigInteger value = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger expected = new BigInteger(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = +(value);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for BitLength
        ///</summary>
        [TestMethod()]
        public void BitLengthTest()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            int actual;
            actual = target.BitLength;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsEven
        ///</summary>
        [TestMethod()]
        public void IsEvenTest()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsEven;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsOne
        ///</summary>
        [TestMethod()]
        public void IsOneTest()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsOne;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsPowerOfTwo
        ///</summary>
        [TestMethod()]
        public void IsPowerOfTwoTest()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsPowerOfTwo;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for IsZero
        ///</summary>
        [TestMethod()]
        public void IsZeroTest()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.IsZero;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for MinusOne
        ///</summary>
        [TestMethod()]
        public void MinusOneTest()
        {
            BigInteger actual;
            actual = BigInteger.MinusOne;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for One
        ///</summary>
        [TestMethod()]
        public void OneTest()
        {
            BigInteger actual;
            actual = BigInteger.One;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Sign
        ///</summary>
        [TestMethod()]
        public void SignTest()
        {
            BigInteger target = new BigInteger(); // TODO: Initialize to an appropriate value
            int actual;
            actual = target.Sign;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Zero
        ///</summary>
        [TestMethod()]
        public void ZeroTest()
        {
            BigInteger actual;
            actual = BigInteger.Zero;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
