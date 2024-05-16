﻿// Based in huge part on:
//
// https://github.com/mono/mono/blob/master/mcs/class/System.Numerics/Test/System.Numerics/BigIntegerTest.cs
//
// Authors:
// Rodrigo Kumpera <rkumpera@novell.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
//

//#define FEATURE_NUMERICS_BIGINTEGER

using System;
using System.Globalization;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;

#if FEATURE_NUMERICS_BIGINTEGER
using BigInteger = System.Numerics.BigInteger;
#else
using BigInteger = Renci.SshNet.Common.BigInteger;
#endif


namespace Renci.SshNet.Tests.Classes.Common
{
    [TestClass]
    public class BigIntegerTest
    {
        private static readonly byte[] Huge_a =
        {
            0x1D, 0x33, 0xFB, 0xFE, 0xB1, 0x2, 0x85, 0x44, 0xCA, 0xDC, 0xFB, 0x70, 0xD, 0x39,
            0xB1, 0x47, 0xB6, 0xE6, 0xA2, 0xD1, 0x19, 0x1E, 0x9F, 0xE4, 0x3C, 0x1E, 0x16, 0x56, 0x13, 0x9C, 0x4D, 0xD3,
            0x5C, 0x74, 0xC9, 0xBD, 0xFA, 0x56, 0x40, 0x58, 0xAC, 0x20, 0x6B, 0x55, 0xA2, 0xD5, 0x41, 0x38, 0xA4, 0x6D,
            0xF6, 0x8C
        };

        private static readonly byte[] Huge_b =
        {
            0x96, 0x5, 0xDA, 0xFE, 0x93, 0x17, 0xC1, 0x93, 0xEC, 0x2F, 0x30, 0x2D, 0x8F,
            0x28, 0x13, 0x99, 0x70, 0xF4, 0x4C, 0x60, 0xA6, 0x49, 0x24, 0xF9, 0xB3, 0x4A, 0x41, 0x67, 0xDC, 0xDD, 0xB1,
            0xA5, 0xA6, 0xC0, 0x3D, 0x57, 0x9A, 0xCB, 0x29, 0xE2, 0x94, 0xAC, 0x6C, 0x7D, 0xEF, 0x3E, 0xC6, 0x7A, 0xC1,
            0xA8, 0xC8, 0xB0, 0x20, 0x95, 0xE6, 0x4C, 0xE1, 0xE0, 0x4B, 0x49, 0xD5, 0x5A, 0xB7
        };

        private static readonly byte[] Huge_add =
        {
            0xB3, 0x38, 0xD5, 0xFD, 0x45, 0x1A, 0x46, 0xD8, 0xB6, 0xC, 0x2C, 0x9E, 0x9C,
            0x61, 0xC4, 0xE0, 0x26, 0xDB, 0xEF, 0x31, 0xC0, 0x67, 0xC3, 0xDD, 0xF0, 0x68, 0x57, 0xBD, 0xEF, 0x79, 0xFF,
            0x78, 0x3, 0x35, 0x7, 0x15, 0x95, 0x22, 0x6A, 0x3A, 0x41, 0xCD, 0xD7, 0xD2, 0x91, 0x14, 0x8, 0xB3, 0x65,
            0x16, 0xBF, 0x3D, 0x20, 0x95, 0xE6, 0x4C, 0xE1, 0xE0, 0x4B, 0x49, 0xD5, 0x5A, 0xB7
        };

        private static readonly byte[] A_m_b =
        {
            0x87, 0x2D, 0x21, 0x0, 0x1E, 0xEB, 0xC3, 0xB0, 0xDD, 0xAC, 0xCB, 0x43, 0x7E, 0x10,
            0x9E, 0xAE, 0x45, 0xF2, 0x55, 0x71, 0x73, 0xD4, 0x7A, 0xEB, 0x88, 0xD3, 0xD4, 0xEE, 0x36, 0xBE, 0x9B, 0x2D,
            0xB6, 0xB3, 0x8B, 0x66, 0x60, 0x8B, 0x16, 0x76, 0x17, 0x74, 0xFE, 0xD7, 0xB2, 0x96, 0x7B, 0xBD, 0xE2, 0xC4,
            0x2D, 0xDC, 0xDE, 0x6A, 0x19, 0xB3, 0x1E, 0x1F, 0xB4, 0xB6, 0x2A, 0xA5, 0x48
        };

        private static readonly byte[] B_m_a =
        {
            0x79, 0xD2, 0xDE, 0xFF, 0xE1, 0x14, 0x3C, 0x4F, 0x22, 0x53, 0x34, 0xBC, 0x81,
            0xEF, 0x61, 0x51, 0xBA, 0xD, 0xAA, 0x8E, 0x8C, 0x2B, 0x85, 0x14, 0x77, 0x2C, 0x2B, 0x11, 0xC9, 0x41, 0x64,
            0xD2, 0x49, 0x4C, 0x74, 0x99, 0x9F, 0x74, 0xE9, 0x89, 0xE8, 0x8B, 0x1, 0x28, 0x4D, 0x69, 0x84, 0x42, 0x1D,
            0x3B, 0xD2, 0x23, 0x21, 0x95, 0xE6, 0x4C, 0xE1, 0xE0, 0x4B, 0x49, 0xD5, 0x5A, 0xB7
        };

        private static readonly byte[] Huge_mul =
        {
            0xFE, 0x83, 0xE1, 0x9B, 0x8D, 0x61, 0x40, 0xD1, 0x60, 0x19, 0xBD, 0x38, 0xF0,
            0xFF, 0x90, 0xAE, 0xDD, 0xAE, 0x73, 0x2C, 0x20, 0x23, 0xCF, 0x6, 0x7A, 0xB4, 0x1C, 0xE7, 0xD9, 0x64, 0x96,
            0x2C, 0x87, 0x7E, 0x1D, 0xB3, 0x8F, 0xD4, 0x33, 0xBA, 0xF4, 0x22, 0xB4, 0xDB, 0xC0, 0x5B, 0xA5, 0x64, 0xA0,
            0xBC, 0xCA, 0x3E, 0x94, 0x95, 0xDA, 0x49, 0xE2, 0xA8, 0x33, 0xA2, 0x6A, 0x33, 0xB1, 0xF2, 0xEA, 0x99, 0x32,
            0xD0, 0xB2, 0xAE, 0x55, 0x75, 0xBD, 0x19, 0xFC, 0x9A, 0xEC, 0x54, 0x87, 0x2A, 0x6, 0xCC, 0x78, 0xDA, 0x88,
            0xBB, 0xAB, 0xA5, 0x47, 0xEF, 0xC7, 0x2B, 0xC7, 0x5B, 0x32, 0x31, 0xCD, 0xD9, 0x53, 0x96, 0x1A, 0x9D, 0x9A,
            0x57, 0x40, 0x51, 0xB6, 0x5D, 0xC, 0x17, 0xD1, 0x86, 0xE9, 0xA4, 0x20
        };

        private static readonly byte[] Huge_div = { 0x0 };

        private static readonly byte[] Huge_rem =
        {
            0x1D, 0x33, 0xFB, 0xFE, 0xB1, 0x2, 0x85, 0x44, 0xCA, 0xDC, 0xFB, 0x70, 0xD,
            0x39, 0xB1, 0x47, 0xB6, 0xE6, 0xA2, 0xD1, 0x19, 0x1E, 0x9F, 0xE4, 0x3C, 0x1E, 0x16, 0x56, 0x13, 0x9C, 0x4D,
            0xD3, 0x5C, 0x74, 0xC9, 0xBD, 0xFA, 0x56, 0x40, 0x58, 0xAC, 0x20, 0x6B, 0x55, 0xA2, 0xD5, 0x41, 0x38, 0xA4,
            0x6D, 0xF6, 0x8C
        };
        private static readonly byte[][] Add_a = { new byte[] { 1 }, new byte[] { 0xFF }, Huge_a };
        private static readonly byte[][] Add_b = { new byte[] { 1 }, new byte[] { 1 }, Huge_b };
        private static readonly byte[][] Add_c = { new byte[] { 2 }, new byte[] { 0 }, Huge_add };

        private readonly NumberFormatInfo _nfi = NumberFormatInfo.InvariantInfo;
        private NumberFormatInfo _nfiUser;

        [TestInitialize]
        public void SetUpFixture()
        {
            _nfiUser = new NumberFormatInfo
            {
                CurrencyDecimalDigits = 3,
                CurrencyDecimalSeparator = ":",
                CurrencyGroupSeparator = "/",
                CurrencyGroupSizes = new[] { 2, 1, 0 },
                CurrencyNegativePattern = 10,  // n $-
                CurrencyPositivePattern = 3,  // n $
                CurrencySymbol = "XYZ",
                PercentDecimalDigits = 1,
                PercentDecimalSeparator = ";",
                PercentGroupSeparator = "~",
                PercentGroupSizes = new[] { 1 },
                PercentNegativePattern = 2,
                PercentPositivePattern = 2,
                PercentSymbol = "%%%",
                NumberDecimalSeparator = "."
            };
        }

        [TestMethod]
        public void Mul()
        {
            long[] values = { -1000000000L, -1000, -1, 0, 1, 1000, 100000000L };

            for (var i = 0; i < values.Length; ++i)
            {
                for (var j = 0; j < values.Length; ++j)
                {
                    var a = new BigInteger(values[i]);
                    var b = new BigInteger(values[j]);
                    var c = a * b;
                    Assert.AreEqual(values[i] * values[j], (long)c, "#_" + i + "_" + j);
                }
            }
        }

        [TestMethod]
        public void TestHugeMul()
        {
            var a = new BigInteger(Huge_a);
            var b = new BigInteger(Huge_b);

            Assert.IsTrue(Huge_mul.IsEqualTo((a * b).ToByteArray()));
        }

        [TestMethod]
        public void DivRem()
        {
            long[] values = { -10000000330L, -5000, -1, 0, 1, 1000, 333, 10234544400L };

            for (var i = 0; i < values.Length; ++i)
            {
                for (var j = 0; j < values.Length; ++j)
                {
                    if (values[j] == 0)
                    {
                        continue;
                    }

                    var a = new BigInteger(values[i]);
                    var b = new BigInteger(values[j]);
                    var c = BigInteger.DivRem(a, b, out var d);

                    Assert.AreEqual(values[i] / values[j], (long)c, "#a_" + i + "_" + j);
                    Assert.AreEqual(values[i] % values[j], (long)d, "#b_" + i + "_" + j);
                }
            }
        }

        [TestMethod]
        public void TestHugeDivRem()
        {
            var a = new BigInteger(Huge_a);
            var b = new BigInteger(Huge_b);
            var c = BigInteger.DivRem(a, b, out var d);

            AssertEqual(Huge_div, c.ToByteArray());
            AssertEqual(Huge_rem, d.ToByteArray());
        }

        [TestMethod]
        public void Pow()
        {
            try
            {
                _ = BigInteger.Pow(1, -1);
                Assert.Fail("#1");
            }
            catch (ArgumentOutOfRangeException) { }

            Assert.AreEqual(1, (int)BigInteger.Pow(99999, 0), "#2");
            Assert.AreEqual(99999, (int)BigInteger.Pow(99999, 1), "#5");
            Assert.AreEqual(59049, (int)BigInteger.Pow(3, 10), "#4");
            Assert.AreEqual(177147, (int)BigInteger.Pow(3, 11), "#5");
            Assert.AreEqual(-177147, (int)BigInteger.Pow(-3, 11), "#6");
        }

        [TestMethod]
        public void ModPow()
        {
            try
            {
                _ = BigInteger.ModPow(1, -1, 5);
                Assert.Fail("#1");
            }
            catch (ArgumentOutOfRangeException) { }

            try
            {
                _ = BigInteger.ModPow(1, 5, 0);
                Assert.Fail("#2");
            }
            catch (DivideByZeroException) { }

            Assert.AreEqual(4L, (long)BigInteger.ModPow(3, 2, 5), "#2");
            Assert.AreEqual(20L, (long)BigInteger.ModPow(555, 10, 71), "#3");
            Assert.AreEqual(20L, (long)BigInteger.ModPow(-555, 10, 71), "#3");
            Assert.AreEqual(-24L, (long)BigInteger.ModPow(-555, 11, 71), "#3");
        }

        [TestMethod]
        public void GreatestCommonDivisor()
        {
            Assert.AreEqual(999999, (int)BigInteger.GreatestCommonDivisor(999999, 0), "#1");
            Assert.AreEqual(999999, (int)BigInteger.GreatestCommonDivisor(0, 999999), "#2");
            Assert.AreEqual(1, (int)BigInteger.GreatestCommonDivisor(999999, 1), "#3");
            Assert.AreEqual(1, (int)BigInteger.GreatestCommonDivisor(1, 999999), "#4");
            Assert.AreEqual(1, (int)BigInteger.GreatestCommonDivisor(1, 0), "#5");
            Assert.AreEqual(1, (int)BigInteger.GreatestCommonDivisor(0, 1), "#6");

            Assert.AreEqual(1, (int)BigInteger.GreatestCommonDivisor(999999, -1), "#7");
            Assert.AreEqual(1, (int)BigInteger.GreatestCommonDivisor(-1, 999999), "#8");
            Assert.AreEqual(1, (int)BigInteger.GreatestCommonDivisor(-1, 0), "#9");
            Assert.AreEqual(1, (int)BigInteger.GreatestCommonDivisor(0, -1), "#10");

            Assert.AreEqual(2, (int)BigInteger.GreatestCommonDivisor(12345678, 8765432), "#11");
            Assert.AreEqual(2, (int)BigInteger.GreatestCommonDivisor(-12345678, 8765432), "#12");
            Assert.AreEqual(2, (int)BigInteger.GreatestCommonDivisor(12345678, -8765432), "#13");
            Assert.AreEqual(2, (int)BigInteger.GreatestCommonDivisor(-12345678, -8765432), "#14");

            Assert.AreEqual(40, (int)BigInteger.GreatestCommonDivisor(5581 * 40, 6671 * 40), "#15");

            Assert.AreEqual(5, (int)BigInteger.GreatestCommonDivisor(-5, 0), "#16");
            Assert.AreEqual(5, (int)BigInteger.GreatestCommonDivisor(0, -5), "#17");
        }

        [TestMethod]
        public void Log()
        {
            const double delta = 0.000000000000001d;

            Assert.AreEqual(double.NegativeInfinity, BigInteger.Log(0));
            Assert.AreEqual(0d, BigInteger.Log(1));
            Assert.AreEqual(double.NaN, BigInteger.Log(-1));
            Assert.AreEqual(2.3025850929940459d, BigInteger.Log(10), delta);
            Assert.AreEqual(6.9077552789821368d, BigInteger.Log(1000), delta);
            Assert.AreEqual(double.NaN, BigInteger.Log(-234));
        }

        [TestMethod]
        public void LogN()
        {
            const double delta = 0.000000000000001d;

            Assert.AreEqual(double.NaN, BigInteger.Log(10, 1), "#1");
            Assert.AreEqual(double.NaN, BigInteger.Log(10, 0), "#2");
            Assert.AreEqual(double.NaN, BigInteger.Log(10, -1), "#3");

            Assert.AreEqual(double.NaN, BigInteger.Log(10, double.NaN), "#4");
            Assert.AreEqual(double.NaN, BigInteger.Log(10, double.NegativeInfinity), "#5");
            Assert.AreEqual(double.NaN, BigInteger.Log(10, double.PositiveInfinity), "#6");

            Assert.AreEqual(0d, BigInteger.Log(1, 0), "#7");
            Assert.AreEqual(double.NaN, BigInteger.Log(1, double.NegativeInfinity), "#8");
            Assert.AreEqual(0, BigInteger.Log(1, double.PositiveInfinity), "#9");
            Assert.AreEqual(double.NaN, BigInteger.Log(1, double.NaN), "#10");

            Assert.AreEqual(-2.5129415947320606d, BigInteger.Log(10, 0.4), delta, "#11");
        }

        [TestMethod]
        public void DivRemByZero()
        {
            try
            {
                _ = BigInteger.DivRem(100, 0, out var d);
                Assert.Fail("#1");
            }
            catch (DivideByZeroException)
            {
            }
        }

        [TestMethod]
        public void TestAdd()
        {
            for (var i = 0; i < Add_a.Length; ++i)
            {
                var a = new BigInteger(Add_a[i]);
                var b = new BigInteger(Add_b[i]);
                var c = new BigInteger(Add_c[i]);

                Assert.AreEqual(c, a + b, "#" + i + "a");
                Assert.AreEqual(c, b + a, "#" + i + "b");
                Assert.AreEqual(c, BigInteger.Add(a, b), "#" + i + "c");
                AssertEqual(Add_c[i], (a + b).ToByteArray());
            }
        }

        [TestMethod]
        public void TestAdd2()
        {
            long[] values = { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };

            for (var i = 0; i < values.Length; ++i)
            {
                for (var j = 0; j < values.Length; ++j)
                {
                    var a = new BigInteger(values[i]);
                    var b = new BigInteger(values[j]);
                    var c = a + b;
                    Assert.AreEqual(values[i] + values[j], (long)c, "#_" + i + "_" + j);
                }
            }
        }

        [TestMethod]
        public void TestHugeSub()
        {
            var a = new BigInteger(Huge_a);
            var b = new BigInteger(Huge_b);

            AssertEqual(A_m_b, (a - b).ToByteArray());
            AssertEqual(B_m_a, (b - a).ToByteArray());
        }

        [TestMethod]
        public void TestSub()
        {
            long[] values = { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };

            for (var i = 0; i < values.Length; ++i)
            {
                for (var j = 0; j < values.Length; ++j)
                {
                    var a = new BigInteger(values[i]);
                    var b = new BigInteger(values[j]);
                    var c = a - b;
                    var d = BigInteger.Subtract(a, b);

                    Assert.AreEqual(values[i] - values[j], (long)c, "#_" + i + "_" + j);
                    Assert.AreEqual(values[i] - values[j], (long)d, "#_" + i + "_" + j);
                }
            }
        }

        [TestMethod]
        public void TestMin()
        {
            long[] values = { -100000000000L, -1000L, -1L, 0L, 1L, 1000L, 100000000000L };

            for (var i = 0; i < values.Length; ++i)
            {
                for (var j = 0; j < values.Length; ++j)
                {
                    var a = new BigInteger(values[i]);
                    var b = new BigInteger(values[j]);
                    var c = BigInteger.Min(a, b);

                    Assert.AreEqual(Math.Min(values[i], values[j]), (long)c, "#_" + i + "_" + j);
                }
            }
        }

        [TestMethod]
        public void TestMax()
        {
            long[] values = { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };

            for (var i = 0; i < values.Length; ++i)
            {
                for (var j = 0; j < values.Length; ++j)
                {
                    var a = new BigInteger(values[i]);
                    var b = new BigInteger(values[j]);
                    var c = BigInteger.Max(a, b);

                    Assert.AreEqual(Math.Max(values[i], values[j]), (long)c, "#_" + i + "_" + j);
                }
            }
        }

        [TestMethod]
        public void TestAbs()
        {
            long[] values = { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };

            for (var i = 0; i < values.Length; ++i)
            {
                var a = new BigInteger(values[i]);
                var c = BigInteger.Abs(a);

                Assert.AreEqual(Math.Abs(values[i]), (long)c, "#_" + i);
            }
        }

        [TestMethod]
        public void TestNegate()
        {
            long[] values = { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };

            for (var i = 0; i < values.Length; ++i)
            {
                var a = new BigInteger(values[i]);
                var c = -a;
                var d = BigInteger.Negate(a);

                Assert.AreEqual(-values[i], (long)c, "#_" + i);
                Assert.AreEqual(-values[i], (long)d, "#_" + i);
            }
        }

        [TestMethod]
        public void TestInc()
        {
            long[] values = { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };

            for (var i = 0; i < values.Length; ++i)
            {
                var a = new BigInteger(values[i]);
                var b = ++a;

                Assert.AreEqual(++values[i], (long)b, "#_" + i);
            }
        }

        [TestMethod]
        public void TestDec()
        {
            long[] values = { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };

            for (var i = 0; i < values.Length; ++i)
            {
                var a = new BigInteger(values[i]);
                var b = --a;

                Assert.AreEqual(--values[i], (long)b, "#_" + i);
            }
        }

        [TestMethod]
        public void TestBitwiseOps()
        {
            long[] values = { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L, 0xFFFF00000000L };

            for (var i = 0; i < values.Length; ++i)
            {
                for (var j = 0; j < values.Length; ++j)
                {
                    var a = new BigInteger(values[i]);
                    var b = new BigInteger(values[j]);

                    Assert.AreEqual(values[i] | values[j], (long)(a | b), "#b_" + i + "_" + j);
                    Assert.AreEqual(values[i] & values[j], (long)(a & b), "#a_" + i + "_" + j);
                    Assert.AreEqual(values[i] ^ values[j], (long)(a ^ b), "#c_" + i + "_" + j);
                    Assert.AreEqual(~values[i], (long)~a, "#d_" + i + "_" + j);
                }
            }
        }

        [TestMethod]
        public void TestLeftShift()
        {
            AssertEqual(new byte[] { 0x00, 0x28 }, (new BigInteger(0x0A) << 10).ToByteArray());
            AssertEqual(new byte[] { 0x00, 0xD8 }, (new BigInteger(-10) << 10).ToByteArray());
            AssertEqual(new byte[] { 0x00, 0x00, 0xFF }, (new BigInteger(-1) << 16).ToByteArray());
            AssertEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A }, (new BigInteger(0x0A) << 80).ToByteArray());
            AssertEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF6 }, (new BigInteger(-10) << 80).ToByteArray());
            AssertEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF }, (new BigInteger(-1) << 80).ToByteArray());
            AssertEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x70, 0xD9 }, (new BigInteger(-1234) << 75).ToByteArray());
            AssertEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA0, 0x91, 0x00 }, (new BigInteger(0x1234) << 75).ToByteArray());
            AssertEqual(new byte[] { 0xFF, 0x00 }, (new BigInteger(0xFF00) << -8).ToByteArray());
        }

        [TestMethod]
        public void TestRightShift()
        {
            AssertEqual(new byte[] { 0x16, 0xB0, 0x4C, 0x02 }, (new BigInteger(1234567899L) >> 5).ToByteArray());
            AssertEqual(new byte[] { 0x2C, 0x93, 0x00 }, (new BigInteger(1234567899L) >> 15).ToByteArray());
            AssertEqual(new byte[] { 0xFF, 0xFF, 0x7F }, (new BigInteger(long.MaxValue - 100) >> 40).ToByteArray());
            AssertEqual(new byte[] { 0xE9, 0x4F, 0xB3, 0xFD }, (new BigInteger(-1234567899L) >> 5).ToByteArray());
            AssertEqual(new byte[] { 0xD3, 0x6C, 0xFF }, (new BigInteger(-1234567899L) >> 15).ToByteArray());
            AssertEqual(new byte[] { 0x00, 0x00, 0x80 }, (new BigInteger(long.MinValue + 100) >> 40).ToByteArray());
            AssertEqual(new byte[] { 0xFF }, (new BigInteger(-1234567899L) >> 90).ToByteArray());
            AssertEqual(new byte[] { 0x00 }, (new BigInteger(999999) >> 90).ToByteArray());
            AssertEqual(new byte[] { 0x00, 0x00, 0xFF, 0x00 }, (new BigInteger(0xFF00) >> -8).ToByteArray());
        }

        [TestMethod]
        public void CompareOps()
        {
            long[] values = { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L };

            for (var i = 0; i < values.Length; ++i)
            {
                for (var j = 0; j < values.Length; ++j)
                {
                    var a = new BigInteger(values[i]);
                    var b = new BigInteger(values[j]);

                    Assert.AreEqual(values[i].CompareTo(values[j]), a.CompareTo(b), "#a_" + i + "_" + j);
                    Assert.AreEqual(values[i].CompareTo(values[j]), BigInteger.Compare(a, b), "#b_" + i + "_" + j);

                    Assert.AreEqual(values[i] < values[j], a < b, "#c_" + i + "_" + j);
                    Assert.AreEqual(values[i] <= values[j], a <= b, "#d_" + i + "_" + j);
                    Assert.AreEqual(values[i] == values[j], a == b, "#e_" + i + "_" + j);
                    Assert.AreEqual(values[i] != values[j], a != b, "#f_" + i + "_" + j);
                    Assert.AreEqual(values[i] >= values[j], a >= b, "#g_" + i + "_" + j);
                    Assert.AreEqual(values[i] > values[j], a > b, "#h_" + i + "_" + j);
                }
            }
        }

        [TestMethod]
        public void CompareOps2()
        {
            var a = new BigInteger(100000000000L);
            var b = new BigInteger(28282828282UL);

            Assert.IsTrue(a >= b, "#1");
            Assert.IsTrue(a >= b, "#2");
            Assert.IsFalse(a < b, "#3");
            Assert.IsFalse(a <= b, "#4");
            Assert.AreEqual(1, a.CompareTo(b), "#5");
        }

        [TestMethod]
        public void CompareULong()
        {
            long[] values = { -100000000000L, -1000, -1, 0, 1, 1000, 100000000000L, 0xAA00000000L };
            ulong[] uvalues = { 0, 1, 1000, 100000000000L, 999999, 28282828282, 0xAA00000000, ulong.MaxValue };

            for (var i = 0; i < values.Length; ++i)
            {
                for (var j = 0; j < uvalues.Length; ++j)
                {
                    var a = new BigInteger(values[i]);
                    var b = uvalues[j];
                    var c = new BigInteger(b);

                    Assert.AreEqual(a.CompareTo(c), a.CompareTo(b), "#a_" + i + "_" + j);

                    Assert.AreEqual(a > c, a > b, "#b_" + i + "_" + j);
                    Assert.AreEqual(a < c, a < b, "#c_" + i + "_" + j);
                    Assert.AreEqual(a <= c, a <= b, "#d_" + i + "_" + j);
                    Assert.AreEqual(a == c, a == b, "#e_" + i + "_" + j);
                    Assert.AreEqual(a != c, a != b, "#f_" + i + "_" + j);
                    Assert.AreEqual(a >= c, a >= b, "#g_" + i + "_" + j);

                    Assert.AreEqual(c > a, b > a, "#ib_" + i + "_" + j);
                    Assert.AreEqual(c < a, b < a, "#ic_" + i + "_" + j);
                    Assert.AreEqual(c <= a, b <= a, "#id_" + i + "_" + j);
                    Assert.AreEqual(c == a, b == a, "#ie_" + i + "_" + j);
                    Assert.AreEqual(c != a, b != a, "#if_" + i + "_" + j);
                    Assert.AreEqual(c >= a, b >= a, "#ig_" + i + "_" + j);
                }
            }
        }

        [TestMethod]
        public void CompareLong()
        {
            long[] values = { -100000000000L, -1000, -1, 0, 1, 1000, 9999999, 100000000000L, 0xAA00000000, long.MaxValue, long.MinValue };

            for (var i = 0; i < values.Length; ++i)
            {
                for (var j = 0; j < values.Length; ++j)
                {
                    var a = new BigInteger(values[i]);
                    var b = values[j];
                    var c = new BigInteger(b);

                    Assert.AreEqual(a.CompareTo(c), a.CompareTo(b), "#a_" + i + "_" + j);

                    Assert.AreEqual(a > c, a > b, "#b_" + i + "_" + j);
                    Assert.AreEqual(a < c, a < b, "#c_" + i + "_" + j);
                    Assert.AreEqual(a <= c, a <= b, "#d_" + i + "_" + j);
                    Assert.AreEqual(a == c, a == b, "#e_" + i + "_" + j);
                    Assert.AreEqual(a != c, a != b, "#f_" + i + "_" + j);
                    Assert.AreEqual(a >= c, a >= b, "#g_" + i + "_" + j);

                    Assert.AreEqual(c > a, b > a, "#ib_" + i + "_" + j);
                    Assert.AreEqual(c < a, b < a, "#ic_" + i + "_" + j);
                    Assert.AreEqual(c <= a, b <= a, "#id_" + i + "_" + j);
                    Assert.AreEqual(c == a, b == a, "#ie_" + i + "_" + j);
                    Assert.AreEqual(c != a, b != a, "#if_" + i + "_" + j);
                    Assert.AreEqual(c >= a, b >= a, "#ig_" + i + "_" + j);
                }
            }
        }

        [TestMethod]
        public void TestEquals()
        {
            var a = new BigInteger(10);
            var b = new BigInteger(10);
            var c = new BigInteger(-10);

            Assert.AreEqual(a, b, "#1");
            Assert.AreNotEqual(a, c, "#2");
            Assert.AreEqual(a, 10, "#3");
        }

        [TestMethod]
        public void Ctor_ByteArray_ShouldThrowArgumentNullExceptionWhenValueIsNull()
        {
            const byte[] value = null;

            try
            {
                var actual = new BigInteger(value);
                Assert.Fail("#1:" + actual);
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("value", ex.ParamName);
            }
        }

        [TestMethod]
        public void ByteArrayCtor()
        {
            Assert.AreEqual(0, (int)new BigInteger(new byte[0]));
            Assert.AreEqual(0, (int)new BigInteger(new byte[1]));
            Assert.AreEqual(0, (int)new BigInteger(new byte[2]));
        }

        [TestMethod]
        public void IntCtorRoundTrip()
        {
            int[] values =
            {
                int.MinValue, -0x2F33BB, -0x1F33, -0x33, 0, 0x33,
                0x80, 0x8190, 0xFF0011, 0x1234, 0x11BB99, 0x44BB22CC,
                int.MaxValue
            };

            foreach (var val in values)
            {
                var a = new BigInteger(val);
                var b = new BigInteger(a.ToByteArray());

                Assert.AreEqual(val, (int)a, "#a_" + val);
                Assert.AreEqual(val, (int)b, "#b_" + val);
            }
        }

        [TestMethod]
        public void LongCtorRoundTrip()
        {
            long[] values =
            {
                0L, long.MinValue, long.MaxValue, -1, 1L + int.MaxValue, -1L + int.MinValue, 0x1234, 0xFFFFFFFFL,
                0x1FFFFFFFFL, -0xFFFFFFFFL, -0x1FFFFFFFFL, 0x100000000L, -0x100000000L, 0x100000001L, -0x100000001L,
                4294967295L, -4294967295L, 4294967296L, -4294967296L
            };

            foreach (var val in values)
            {
                try
                {
                    var a = new BigInteger(val);
                    var b = new BigInteger(a.ToByteArray());

                    Assert.AreEqual(val, (long)a, "#a_" + val);
                    Assert.AreEqual(val, (long)b, "#b_" + val);
                    Assert.AreEqual(a, b, "#a  == #b (" + val + ")");
                }
                catch (Exception e)
                {
                    Assert.Fail("Failed to roundtrip {0}: {1}", val, e);
                }
            }
        }

        [TestMethod]
        public void ByteArrayCtorRoundTrip()
        {
            var arr = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            AssertEqual(arr, new BigInteger(arr).ToByteArray());

            arr = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 0xFF, 0x0 };
            AssertEqual(arr, new BigInteger(arr).ToByteArray());

            arr = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0xF0 };
            AssertEqual(arr, new BigInteger(arr).ToByteArray());

            arr = new byte[] { 1 };
            AssertEqual(arr, new BigInteger(arr).ToByteArray());

            arr = new byte[] { 1, 2 };
            AssertEqual(arr, new BigInteger(arr).ToByteArray());

            arr = new byte[] { 1, 2, 3 };
            AssertEqual(arr, new BigInteger(arr).ToByteArray());

            arr = new byte[] { 1, 2, 3, 4 };
            AssertEqual(arr, new BigInteger(arr).ToByteArray());

            arr = new byte[] { 1, 2, 3, 4, 5 };
            AssertEqual(arr, new BigInteger(arr).ToByteArray());

            arr = new byte[] { 1, 2, 3, 4, 5, 6 };
            AssertEqual(arr, new BigInteger(arr).ToByteArray());

            arr = new byte[] { 1, 2, 3, 4, 5, 6, 7 };
            AssertEqual(arr, new BigInteger(arr).ToByteArray());

            arr = new byte[] { 1, 2, 3, 4, 5 };
            AssertEqual(arr, new BigInteger(arr).ToByteArray());

            arr = new byte[] { 0 };
            AssertEqual(arr, new BigInteger(arr).ToByteArray());

            arr = new byte[] { 0xFF, 00 };
            AssertEqual(arr, new BigInteger(arr).ToByteArray());

            arr = new byte[] { 1, 0, 0, 0, 0, 0 };
            AssertEqual(new byte[] { 1 }, new BigInteger(arr).ToByteArray());
        }

        [TestMethod]
        public void TestIntCtorProperties()
        {
            var a = new BigInteger(10);
            Assert.IsTrue(a.IsEven, "#1");
            Assert.IsFalse(a.IsOne, "#2");
            Assert.IsFalse(a.IsPowerOfTwo, "#3");
            Assert.IsFalse(a.IsZero, "#4");
            Assert.AreEqual(1, a.Sign, "#5");

            Assert.IsFalse(new BigInteger(11).IsEven, "#6");
            Assert.IsTrue(new BigInteger(1).IsOne, "#7");
            Assert.IsTrue(new BigInteger(32).IsPowerOfTwo, "#8");
            Assert.IsTrue(new BigInteger(0).IsZero, "#9");
            Assert.IsTrue(new BigInteger().IsZero, "#9b");
            Assert.AreEqual(0, new BigInteger(0).Sign, "#10");
            Assert.AreEqual(-1, new BigInteger(-99999).Sign, "#11");

            Assert.IsFalse(new BigInteger(0).IsPowerOfTwo, "#12");
            Assert.IsFalse(new BigInteger().IsPowerOfTwo, "#12b");
            Assert.IsFalse(new BigInteger(-16).IsPowerOfTwo, "#13");
            Assert.IsTrue(new BigInteger(1).IsPowerOfTwo, "#14");
        }

        [TestMethod]
        public void TestIntCtorToString()
        {
            Assert.AreEqual("5555", new BigInteger(5555).ToString(), "#1");
            Assert.AreEqual("-99999", new BigInteger(-99999).ToString(), "#2");
        }

        [TestMethod]
        public void TestToStringFmt()
        {
            Assert.AreEqual("123456789123456", new BigInteger(123456789123456).ToString("D2"), "#1");
            Assert.AreEqual("0000000005", new BigInteger(5).ToString("d10"), "#2");
            Assert.AreEqual("0A8", new BigInteger(168).ToString("X"), "#3");
            Assert.AreEqual("0", new BigInteger(0).ToString("X"), "#4");
            Assert.AreEqual("0", new BigInteger().ToString("X"), "#4b");
            Assert.AreEqual("1", new BigInteger(1).ToString("X"), "#5");
            Assert.AreEqual("0A", new BigInteger(10).ToString("X"), "#6");
            Assert.AreEqual("F6", new BigInteger(-10).ToString("X"), "#7");

            Assert.AreEqual("10000000000000000000000000000000000000000000000000000000", BigInteger.Pow(10, 55).ToString("G"), "#8");

            Assert.AreEqual("10000000000000000000000000000000000000000000000000000000", BigInteger.Pow(10, 55).ToString("R"), "#9");


            Assert.AreEqual("000000000A", new BigInteger(10).ToString("X10"), "#10");
            Assert.AreEqual("0000000010", new BigInteger(10).ToString("G10"), "#11");
        }

        [TestMethod]
        public void TestToStringFmtProvider()
        {
            var info = new NumberFormatInfo
            {
                NegativeSign = ">",
                PositiveSign = "%"
            };

            Assert.AreEqual("10", new BigInteger(10).ToString(info), "#1");
            Assert.AreEqual(">10", new BigInteger(-10).ToString(info), "#2");
            Assert.AreEqual("0A", new BigInteger(10).ToString("X", info), "#3");
            Assert.AreEqual("F6", new BigInteger(-10).ToString("X", info), "#4");
            Assert.AreEqual("10", new BigInteger(10).ToString("G", info), "#5");
            Assert.AreEqual(">10", new BigInteger(-10).ToString("G", info), "#6");
            Assert.AreEqual("10", new BigInteger(10).ToString("D", info), "#7");
            Assert.AreEqual(">10", new BigInteger(-10).ToString("D", info), "#8");
            Assert.AreEqual("10", new BigInteger(10).ToString("R", info), "#9");
            Assert.AreEqual(">10", new BigInteger(-10).ToString("R", info), "#10");

            info = new NumberFormatInfo
            {
                NegativeSign = "#$%"
            };

            Assert.AreEqual("#$%10", new BigInteger(-10).ToString(info), "#2");
            Assert.AreEqual("#$%10", new BigInteger(-10).ToString(null, info), "#2");

            info = new NumberFormatInfo();

            Assert.AreEqual("-10", new BigInteger(-10).ToString(info), "#2");

        }

        [TestMethod]
        public void TestToIntOperator()
        {
            try
            {
                _ = (int)new BigInteger(Huge_a);
                Assert.Fail("#1");
            }
            catch (OverflowException) { }

            try
            {
                _ = (int)new BigInteger(1L + int.MaxValue);
                Assert.Fail("#2");
            }
            catch (OverflowException) { }

            try
            {
                _ = (int)new BigInteger(-1L + int.MinValue);
                Assert.Fail("#3");
            }
            catch (OverflowException) { }

            Assert.AreEqual(int.MaxValue, (int)new BigInteger(int.MaxValue), "#4");
            Assert.AreEqual(int.MinValue, (int)new BigInteger(int.MinValue), "#5");
        }


        [TestMethod]
        public void TestToLongOperator()
        {
            try
            {
                _ = (long)new BigInteger(Huge_a);
                Assert.Fail("#1");
            }
            catch (OverflowException) { }

            //long.MaxValue + 1
            try
            {
                _ = (long)new BigInteger(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00 });
                Assert.Fail("#2");
            }
            catch (OverflowException) { }

            //TODO long.MinValue - 1
            try
            {
                _ = (long)new BigInteger(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F, 0xFF });
                Assert.Fail("#3");
            }
            catch (OverflowException) { }

            Assert.AreEqual(long.MaxValue, (long)new BigInteger(long.MaxValue), "#4");
            Assert.AreEqual(long.MinValue, (long)new BigInteger(long.MinValue), "#5");
        }

        [TestMethod]
        public void TestIntCtorToByteArray()
        {
            AssertEqual(new byte[] { 0xFF }, new BigInteger(-1).ToByteArray());
            AssertEqual(new byte[] { 0xD4, 0xFE }, new BigInteger(-300).ToByteArray());
            AssertEqual(new byte[] { 0x80, 0x00 }, new BigInteger(128).ToByteArray());
            AssertEqual(new byte[] { 0x00, 0x60 }, new BigInteger(0x6000).ToByteArray());
            AssertEqual(new byte[] { 0x00, 0x80, 0x00 }, new BigInteger(0x8000).ToByteArray());
            AssertEqual(new byte[] { 0xDD, 0xBC, 0x00, 0x7A }, new BigInteger(0x7A00BCDD).ToByteArray());
            AssertEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0x7F }, new BigInteger(int.MaxValue).ToByteArray());
            AssertEqual(new byte[] { 0x00, 0x00, 0x00, 0x80 }, new BigInteger(int.MinValue).ToByteArray());
            AssertEqual(new byte[] { 0x01, 0x00, 0x00, 0x80 }, new BigInteger(int.MinValue + 1).ToByteArray());
            AssertEqual(new byte[] { 0x7F }, new BigInteger(0x7F).ToByteArray());
            AssertEqual(new byte[] { 0x45, 0xCC, 0xD0 }, new BigInteger(-0x2F33BB).ToByteArray());
            AssertEqual(new byte[] { 0 }, new BigInteger(0).ToByteArray());
            AssertEqual(new byte[] { 0 }, new BigInteger().ToByteArray());
        }

        [TestMethod]
        public void TestLongCtorToByteArray()
        {
            AssertEqual(new byte[] { 0x01 }, new BigInteger(0x01L).ToByteArray());
            AssertEqual(new byte[] { 0x02, 0x01 }, new BigInteger(0x0102L).ToByteArray());
            AssertEqual(new byte[] { 0x03, 0x02, 0x01 }, new BigInteger(0x010203L).ToByteArray());
            AssertEqual(new byte[] { 0x04, 0x03, 0x2, 0x01 }, new BigInteger(0x01020304L).ToByteArray());
            AssertEqual(new byte[] { 0x05, 0x04, 0x03, 0x2, 0x01 }, new BigInteger(0x0102030405L).ToByteArray());
            AssertEqual(new byte[] { 0x06, 0x05, 0x04, 0x03, 0x2, 0x01 }, new BigInteger(0x010203040506L).ToByteArray());
            AssertEqual(new byte[] { 0x07, 0x06, 0x05, 0x04, 0x03, 0x2, 0x01 }, new BigInteger(0x01020304050607L).ToByteArray());
            AssertEqual(new byte[] { 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x2, 0x01 }, new BigInteger(0x0102030405060708L).ToByteArray());

            AssertEqual(new byte[] { 0xFF }, new BigInteger(-0x01L).ToByteArray());
            AssertEqual(new byte[] { 0xFE, 0xFE }, new BigInteger(-0x0102L).ToByteArray());
            AssertEqual(new byte[] { 0xFD, 0xFD, 0xFE }, new BigInteger(-0x010203L).ToByteArray());
            AssertEqual(new byte[] { 0xFC, 0xFC, 0xFD, 0xFE }, new BigInteger(-0x01020304L).ToByteArray());
            AssertEqual(new byte[] { 0xFB, 0xFB, 0xFC, 0xFD, 0xFE }, new BigInteger(-0x0102030405L).ToByteArray());
            AssertEqual(new byte[] { 0xFA, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE }, new BigInteger(-0x010203040506L).ToByteArray());
            AssertEqual(new byte[] { 0xF9, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE }, new BigInteger(-0x01020304050607L).ToByteArray());
            AssertEqual(new byte[] { 0xF8, 0xF8, 0xF9, 0xFA, 0xFB, 0xFC, 0xFD, 0xFE }, new BigInteger(-0x0102030405060708L).ToByteArray());

            AssertEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x7F }, new BigInteger(long.MaxValue).ToByteArray());
            AssertEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80 }, new BigInteger(long.MinValue).ToByteArray());

            AssertEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0x7F, 0xFF }, new BigInteger(-2147483649L).ToByteArray());
        }

        [TestMethod]
        public void CompareTo()
        {
            var a = new BigInteger(99);
            Assert.AreEqual(-1, a.CompareTo(100), "#1");
            Assert.AreEqual(1, a.CompareTo(null), "#2");
        }

        [TestMethod]
        public void ShortOperators()
        {
            Assert.AreEqual(22, (int)new BigInteger(22), "#1");
            Assert.AreEqual(-22, (int)new BigInteger(-22), "#2");

            try
            {
                _ = (short)new BigInteger(10000000);
                Assert.Fail("#3");
            }
            catch (OverflowException) { }

            try
            {
                _ = (short)new BigInteger(-10000000);
                Assert.Fail("#4");
            }
            catch (OverflowException) { }
        }

        [TestMethod]
        public void Ctor_Double_NaN()
        {
            try
            {
                _ = new BigInteger(double.NaN);
                Assert.Fail();
            }
            catch (OverflowException)
            {
            }
        }

        [TestMethod]
        public void Ctor_Double_NegativeInfinity()
        {
            try
            {
                _ = new BigInteger(double.NegativeInfinity);
                Assert.Fail();
            }
            catch (OverflowException)
            {
            }
        }

        [TestMethod]
        public void Ctor_Double_PositiveInfinity()
        {
            try
            {
                _ = new BigInteger(double.PositiveInfinity);
                Assert.Fail();
            }
            catch (OverflowException)
            {
            }
        }

        [TestMethod]
        public void Ctor_Double()
        {
            Assert.AreEqual(10000, (int)new BigInteger(10000.2));
            Assert.AreEqual(10000, (int)new BigInteger(10000.9));
            Assert.AreEqual(10000, (int)new BigInteger(10000.2));
            Assert.AreEqual(0, (int)new BigInteger(0.9));
            Assert.AreEqual(12345678999L, (long)new BigInteger(12345678999.33));
        }

        [TestMethod]
        public void DoubleConversion()
        {
            Assert.AreEqual(999d, (double)new BigInteger(999), "#1");
            Assert.AreEqual(double.PositiveInfinity, (double)BigInteger.Pow(2, 1024), "#2");
            Assert.AreEqual(double.NegativeInfinity, (double)BigInteger.Pow(-2, 1025), "#3");

            Assert.AreEqual(0d, (double)BigInteger.Zero, "#4");
            Assert.AreEqual(1d, (double)BigInteger.One, "#5");
            Assert.AreEqual(-1d, (double)BigInteger.MinusOne, "#6");

            var result1 = BitConverter.Int64BitsToDouble(-4337852273739220173);
            Assert.AreEqual(result1, (double)new BigInteger(new byte[] { 53, 152, 137, 177, 240, 81, 75, 198 }), "#7");
            var result2 = BitConverter.Int64BitsToDouble(4893382453283402035);
            Assert.AreEqual(result2, (double)new BigInteger(new byte[] { 53, 152, 137, 177, 240, 81, 75, 198, 0 }), "#8");

            var result3 = BitConverter.Int64BitsToDouble(5010775143622804480);
            var result4 = BitConverter.Int64BitsToDouble(5010775143622804481);
            var result5 = BitConverter.Int64BitsToDouble(5010775143622804482);
            Assert.AreEqual(result3, (double)new BigInteger(new byte[] { 0, 0, 0, 0, 16, 128, 208, 159, 60, 46, 59, 3 }), "#9");
            Assert.AreEqual(result3, (double)new BigInteger(new byte[] { 0, 0, 0, 0, 17, 128, 208, 159, 60, 46, 59, 3 }), "#10");
            Assert.AreEqual(result3, (double)new BigInteger(new byte[] { 0, 0, 0, 0, 24, 128, 208, 159, 60, 46, 59, 3 }), "#11");
            Assert.AreEqual(result4, (double)new BigInteger(new byte[] { 0, 0, 0, 0, 32, 128, 208, 159, 60, 46, 59, 3 }), "#12");
            Assert.AreEqual(result4, (double)new BigInteger(new byte[] { 0, 0, 0, 0, 48, 128, 208, 159, 60, 46, 59, 3 }), "#13");
            Assert.AreEqual(result5, (double)new BigInteger(new byte[] { 0, 0, 0, 0, 64, 128, 208, 159, 60, 46, 59, 3 }), "#14");

            Assert.AreEqual(BitConverter.Int64BitsToDouble(-2748107935317889142), (double)new BigInteger(Huge_a), "#15");
            Assert.AreEqual(BitConverter.Int64BitsToDouble(-2354774254443231289), (double)new BigInteger(Huge_b), "#16");
            Assert.AreEqual(BitConverter.Int64BitsToDouble(8737073938546854790), (double)new BigInteger(Huge_mul), "#17");

            Assert.AreEqual(BitConverter.Int64BitsToDouble(6912920136897069886), (double)(2278888483353476799 * BigInteger.Pow(2, 451)), "#18");
            Assert.AreEqual(double.PositiveInfinity, (double)(843942696292817306 * BigInteger.Pow(2, 965)), "#19");
        }

        [TestMethod]
        public void DecimalCtor()
        {
            Assert.AreEqual(999, (int)new BigInteger(999.99m), "#1");
            Assert.AreEqual(-10000, (int)new BigInteger(-10000m), "#2");
            Assert.AreEqual(0, (int)new BigInteger(0m), "#3");
        }

        [TestMethod]
        public void DecimalConversion()
        {
            Assert.AreEqual(999m, (decimal)new BigInteger(999), "#1");

            try
            {
                var x = (decimal)BigInteger.Pow(2, 1024);
                Assert.Fail("#2: " + x);
            }
            catch (OverflowException)
            {
            }

            try
            {
                var x = (decimal)BigInteger.Pow(-2, 1025);
                Assert.Fail("#3: " + x);
            }
            catch (OverflowException)
            {
            }

            Assert.AreEqual(0m, (decimal)new BigInteger(0), "#4");
            Assert.AreEqual(1m, (decimal)new BigInteger(1), "#5");
            Assert.AreEqual(-1m, (decimal)new BigInteger(-1), "#6");
            Assert.AreEqual(9999999999999999999999999999m, (decimal)new BigInteger(9999999999999999999999999999m), "#7");
            Assert.AreEqual(0m, (decimal)new BigInteger(), "#8");
        }

        [TestMethod]
        public void Parse()
        {
            try
            {
                _ = BigInteger.Parse(null);
                Assert.Fail("#1");
            }
            catch (ArgumentNullException) { }

            try
            {
                _ = BigInteger.Parse("");
                Assert.Fail("#2");
            }
            catch (FormatException) { }


            try
            {
                _ = BigInteger.Parse("  ");
                Assert.Fail("#3");
            }
            catch (FormatException) { }

            try
            {
                _ = BigInteger.Parse("hh");
                Assert.Fail("#4");
            }
            catch (FormatException) { }

            try
            {
                _ = BigInteger.Parse("-");
                Assert.Fail("#5");
            }
            catch (FormatException) { }

            try
            {
                _ = BigInteger.Parse("-+");
                Assert.Fail("#6");
            }
            catch (FormatException) { }

            Assert.AreEqual(10, (int)BigInteger.Parse("+10"), "#7");
            Assert.AreEqual(10, (int)BigInteger.Parse("10 "), "#8");
            Assert.AreEqual(-10, (int)BigInteger.Parse("-10 "), "#9");
            Assert.AreEqual(10, (int)BigInteger.Parse("    10 "), "#10");
            Assert.AreEqual(-10, (int)BigInteger.Parse("  -10 "), "#11");

            Assert.AreEqual(-1, (int)BigInteger.Parse("F", NumberStyles.AllowHexSpecifier), "#12");
            Assert.AreEqual(-8, (int)BigInteger.Parse("8", NumberStyles.AllowHexSpecifier), "#13");
            Assert.AreEqual(8, (int)BigInteger.Parse("08", NumberStyles.AllowHexSpecifier), "#14");
            Assert.AreEqual(15, (int)BigInteger.Parse("0F", NumberStyles.AllowHexSpecifier), "#15");
            Assert.AreEqual(-1, (int)BigInteger.Parse("FF", NumberStyles.AllowHexSpecifier), "#16");
            Assert.AreEqual(255, (int)BigInteger.Parse("0FF", NumberStyles.AllowHexSpecifier), "#17");

            Assert.AreEqual(-17, (int)BigInteger.Parse("   (17)   ", NumberStyles.AllowParentheses | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite), "#18");
            Assert.AreEqual(-23, (int)BigInteger.Parse("  -23  ", NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite), "#19");

            Assert.AreEqual(300000, (int)BigInteger.Parse("3E5", NumberStyles.AllowExponent), "#20");
            var dsep = NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
            Assert.AreEqual(250, (int)BigInteger.Parse("2" + dsep + "5E2", NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint), "#21");//2.5E2 = 250
            Assert.AreEqual(25, (int)BigInteger.Parse("2500E-2", NumberStyles.AllowExponent), "#22");

            Assert.AreEqual("136236974127783066520110477975349088954559032721408", BigInteger.Parse("136236974127783066520110477975349088954559032721408", NumberStyles.None).ToString(), "#23");
            Assert.AreEqual("136236974127783066520110477975349088954559032721408", BigInteger.Parse("136236974127783066520110477975349088954559032721408").ToString(), "#24");

            try
            {
                _ = BigInteger.Parse("2E3.0", NumberStyles.AllowExponent); // decimal notation for the exponent
                Assert.Fail("#25");
            }
            catch (FormatException)
            {
            }

            try
            {
                _ = int.Parse("2" + dsep + "09E1", NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent);
                Assert.Fail("#26");
            }
            catch (OverflowException)
            {
            }
        }

        [TestMethod]
        public void TryParse_Value_ShouldReturnFalseWhenValueIsNull()
        {
            var actual = BigInteger.TryParse(null, out var x);

            Assert.IsFalse(actual);
            Assert.AreEqual(BigInteger.Zero, x);
        }

        [TestMethod]
        public void TryParse_Value()
        {
            Assert.IsFalse(BigInteger.TryParse("", out var x));
            Assert.AreEqual(BigInteger.Zero, x);

            Assert.IsFalse(BigInteger.TryParse(" ", out x));
            Assert.AreEqual(BigInteger.Zero, x);

            Assert.IsFalse(BigInteger.TryParse(" -", out x));
            Assert.AreEqual(BigInteger.Zero, x);

            Assert.IsFalse(BigInteger.TryParse(" +", out x));
            Assert.AreEqual(BigInteger.Zero, x);

            Assert.IsFalse(BigInteger.TryParse(" FF", out x));
            Assert.AreEqual(BigInteger.Zero, x);

            Assert.IsTrue(BigInteger.TryParse(" 99", out x));
            Assert.AreEqual(99, (int)x);

            Assert.IsTrue(BigInteger.TryParse("+133", out x));
            Assert.AreEqual(133, (int)x);

            Assert.IsTrue(BigInteger.TryParse("-010", out x));
            Assert.AreEqual(-10, (int)x);
        }

        [TestMethod]
        public void TryParse_ValueAndStyleAndProvider()
        {
            Assert.IsFalse(BigInteger.TryParse("null", NumberStyles.None, null, out var x));
            Assert.AreEqual(BigInteger.Zero, x);

            Assert.IsFalse(BigInteger.TryParse("-10", NumberStyles.None, null, out x));
            Assert.AreEqual(BigInteger.Zero, x);

            Assert.IsFalse(BigInteger.TryParse("(10)", NumberStyles.None, null, out x));
            Assert.AreEqual(BigInteger.Zero, x);

            Assert.IsFalse(BigInteger.TryParse(" 10", NumberStyles.None, null, out x));
            Assert.AreEqual(BigInteger.Zero, x);

            Assert.IsFalse(BigInteger.TryParse("10 ", NumberStyles.None, null, out x));
            Assert.AreEqual(BigInteger.Zero, x);

            Assert.IsTrue(BigInteger.TryParse("-10", NumberStyles.AllowLeadingSign, null, out x));
            Assert.AreEqual(-10, (int)x);

            Assert.IsTrue(BigInteger.TryParse("(10)", NumberStyles.AllowParentheses, null, out x));
            Assert.AreEqual(-10, (int)x);

            Assert.IsTrue(BigInteger.TryParse(" 10", NumberStyles.AllowLeadingWhite, null, out x));
            Assert.AreEqual(10, (int)x);

            Assert.IsTrue(BigInteger.TryParse("10 ", NumberStyles.AllowTrailingWhite, null, out x));
            Assert.AreEqual(10, (int)x);

            Assert.IsFalse(BigInteger.TryParse("$10", NumberStyles.None, null, out x));
            Assert.AreEqual(BigInteger.Zero, x);

            Assert.IsFalse(BigInteger.TryParse("$10", NumberStyles.None, _nfi, out x));
            Assert.AreEqual(BigInteger.Zero, x);

            Assert.IsFalse(BigInteger.TryParse("%10", NumberStyles.None, _nfi, out x));
            Assert.AreEqual(BigInteger.Zero, x);

            Assert.IsFalse(BigInteger.TryParse("10 ", NumberStyles.None, null, out x));
            Assert.AreEqual(BigInteger.Zero, x);

            Assert.IsTrue(BigInteger.TryParse("10", NumberStyles.None, null, out x));
            Assert.AreEqual(10, (int)x);

            Assert.IsTrue(BigInteger.TryParse(_nfi.CurrencySymbol + "10", NumberStyles.AllowCurrencySymbol, _nfi, out x));
            Assert.AreEqual(10, (int)x);

            Assert.IsFalse(BigInteger.TryParse("%10", NumberStyles.AllowCurrencySymbol, _nfi, out x));
            Assert.AreEqual(BigInteger.Zero, x);
        }

        [TestMethod]
        public void TryParse_ValueAndStyleAndProvider_ShouldReturnFalseWhenValueIsNull()
        {
            var actual = BigInteger.TryParse(null, NumberStyles.Any, CultureInfo.InvariantCulture, out var x);

            Assert.IsFalse(actual);
            Assert.AreEqual(BigInteger.Zero, x);
        }

        [TestMethod]
        public void TestUserCurrency()
        {
            const int val1 = -1234567;
            const int val2 = 1234567;

            var s = val1.ToString("c", _nfiUser);
            Assert.AreEqual("1234/5/67:000 XYZ-", s, "Currency value type 1 is not what we want to try to parse");

            var v = BigInteger.Parse("1234/5/67:000   XYZ-", NumberStyles.Currency, _nfiUser);
            Assert.AreEqual(val1, (int)v);

            s = val2.ToString("c", _nfiUser);
            Assert.AreEqual("1234/5/67:000 XYZ", s, "Currency value type 2 is not what we want to try to parse");

            v = BigInteger.Parse(s, NumberStyles.Currency, _nfiUser);
            Assert.AreEqual(val2, (int)v);
        }

        [TestMethod]
        public void TryParseWeirdCulture()
        {
            var old = Thread.CurrentThread.CurrentCulture;
            var cur = (CultureInfo)old.Clone();
            cur.NumberFormat = new NumberFormatInfo
            {
                NegativeSign = ">",
                PositiveSign = "%"
            };

            Thread.CurrentThread.CurrentCulture = cur;

            try
            {
                Assert.IsTrue(BigInteger.TryParse("%11", out var x));
                Assert.AreEqual(11, (int)x);

                Assert.IsTrue(BigInteger.TryParse(">11", out x));
                Assert.AreEqual(-11, (int)x);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = old;
            }
        }

        [TestMethod]
        public void CompareToLongToWithBigNumber()
        {
            var a = BigInteger.Parse("123456789123456789");
            Assert.AreEqual(1, a.CompareTo(2000));
            Assert.AreEqual(1, a.CompareTo(-2000));

            var b = BigInteger.Parse("-123456789123456789");
            Assert.AreEqual(-1, b.CompareTo(2000));
            Assert.AreEqual(-1, b.CompareTo(-2000));
        }

        [TestMethod]
        public void LeftShiftByInt()
        {
            var v = BigInteger.Parse("230794411440927908251127453634");

            Assert.AreEqual("230794411440927908251127453634", (v << 0).ToString(), "#0");
            Assert.AreEqual("461588822881855816502254907268", (v << 1).ToString(), "#1");
            Assert.AreEqual("923177645763711633004509814536", (v << 2).ToString(), "#2");
            Assert.AreEqual("1846355291527423266009019629072", (v << 3).ToString(), "#3");
            Assert.AreEqual("3692710583054846532018039258144", (v << 4).ToString(), "#4");
            Assert.AreEqual("7385421166109693064036078516288", (v << 5).ToString(), "#5");
            Assert.AreEqual("14770842332219386128072157032576", (v << 6).ToString(), "#6");
            Assert.AreEqual("29541684664438772256144314065152", (v << 7).ToString(), "#7");
            Assert.AreEqual("59083369328877544512288628130304", (v << 8).ToString(), "#8");
            Assert.AreEqual("118166738657755089024577256260608", (v << 9).ToString(), "#9");
            Assert.AreEqual("236333477315510178049154512521216", (v << 10).ToString(), "#10");
            Assert.AreEqual("472666954631020356098309025042432", (v << 11).ToString(), "#11");
            Assert.AreEqual("945333909262040712196618050084864", (v << 12).ToString(), "#12");
            Assert.AreEqual("1890667818524081424393236100169728", (v << 13).ToString(), "#13");
            Assert.AreEqual("3781335637048162848786472200339456", (v << 14).ToString(), "#14");
            Assert.AreEqual("7562671274096325697572944400678912", (v << 15).ToString(), "#15");
            Assert.AreEqual("15125342548192651395145888801357824", (v << 16).ToString(), "#16");
            Assert.AreEqual("30250685096385302790291777602715648", (v << 17).ToString(), "#17");
            Assert.AreEqual("60501370192770605580583555205431296", (v << 18).ToString(), "#18");
            Assert.AreEqual("121002740385541211161167110410862592", (v << 19).ToString(), "#19");
            Assert.AreEqual("242005480771082422322334220821725184", (v << 20).ToString(), "#20");
            Assert.AreEqual("484010961542164844644668441643450368", (v << 21).ToString(), "#21");
            Assert.AreEqual("968021923084329689289336883286900736", (v << 22).ToString(), "#22");
            Assert.AreEqual("1936043846168659378578673766573801472", (v << 23).ToString(), "#23");
            Assert.AreEqual("3872087692337318757157347533147602944", (v << 24).ToString(), "#24");
            Assert.AreEqual("7744175384674637514314695066295205888", (v << 25).ToString(), "#25");
            Assert.AreEqual("15488350769349275028629390132590411776", (v << 26).ToString(), "#26");
            Assert.AreEqual("30976701538698550057258780265180823552", (v << 27).ToString(), "#27");
            Assert.AreEqual("61953403077397100114517560530361647104", (v << 28).ToString(), "#28");
            Assert.AreEqual("123906806154794200229035121060723294208", (v << 29).ToString(), "#29");
            Assert.AreEqual("247813612309588400458070242121446588416", (v << 30).ToString(), "#30");
            Assert.AreEqual("495627224619176800916140484242893176832", (v << 31).ToString(), "#31");
            Assert.AreEqual("991254449238353601832280968485786353664", (v << 32).ToString(), "#32");
            Assert.AreEqual("1982508898476707203664561936971572707328", (v << 33).ToString(), "#33");
            Assert.AreEqual("3965017796953414407329123873943145414656", (v << 34).ToString(), "#34");
            Assert.AreEqual("7930035593906828814658247747886290829312", (v << 35).ToString(), "#35");
            Assert.AreEqual("15860071187813657629316495495772581658624", (v << 36).ToString(), "#36");
            Assert.AreEqual("31720142375627315258632990991545163317248", (v << 37).ToString(), "#37");
            Assert.AreEqual("63440284751254630517265981983090326634496", (v << 38).ToString(), "#38");
            Assert.AreEqual("126880569502509261034531963966180653268992", (v << 39).ToString(), "#39");
            Assert.AreEqual("253761139005018522069063927932361306537984", (v << 40).ToString(), "#40");
            Assert.AreEqual("507522278010037044138127855864722613075968", (v << 41).ToString(), "#41");
            Assert.AreEqual("1015044556020074088276255711729445226151936", (v << 42).ToString(), "#42");
            Assert.AreEqual("2030089112040148176552511423458890452303872", (v << 43).ToString(), "#43");
            Assert.AreEqual("4060178224080296353105022846917780904607744", (v << 44).ToString(), "#44");
            Assert.AreEqual("8120356448160592706210045693835561809215488", (v << 45).ToString(), "#45");
            Assert.AreEqual("16240712896321185412420091387671123618430976", (v << 46).ToString(), "#46");
            Assert.AreEqual("32481425792642370824840182775342247236861952", (v << 47).ToString(), "#47");
            Assert.AreEqual("64962851585284741649680365550684494473723904", (v << 48).ToString(), "#48");
            Assert.AreEqual("129925703170569483299360731101368988947447808", (v << 49).ToString(), "#49");
            Assert.AreEqual("259851406341138966598721462202737977894895616", (v << 50).ToString(), "#50");
            Assert.AreEqual("519702812682277933197442924405475955789791232", (v << 51).ToString(), "#51");
            Assert.AreEqual("1039405625364555866394885848810951911579582464", (v << 52).ToString(), "#52");
            Assert.AreEqual("2078811250729111732789771697621903823159164928", (v << 53).ToString(), "#53");
            Assert.AreEqual("4157622501458223465579543395243807646318329856", (v << 54).ToString(), "#54");
            Assert.AreEqual("8315245002916446931159086790487615292636659712", (v << 55).ToString(), "#55");
            Assert.AreEqual("16630490005832893862318173580975230585273319424", (v << 56).ToString(), "#56");
            Assert.AreEqual("33260980011665787724636347161950461170546638848", (v << 57).ToString(), "#57");
            Assert.AreEqual("66521960023331575449272694323900922341093277696", (v << 58).ToString(), "#58");
            Assert.AreEqual("133043920046663150898545388647801844682186555392", (v << 59).ToString(), "#59");
            Assert.AreEqual("266087840093326301797090777295603689364373110784", (v << 60).ToString(), "#60");
            Assert.AreEqual("532175680186652603594181554591207378728746221568", (v << 61).ToString(), "#61");
            Assert.AreEqual("1064351360373305207188363109182414757457492443136", (v << 62).ToString(), "#62");
            Assert.AreEqual("2128702720746610414376726218364829514914984886272", (v << 63).ToString(), "#63");
            Assert.AreEqual("4257405441493220828753452436729659029829969772544", (v << 64).ToString(), "#64");
            Assert.AreEqual("8514810882986441657506904873459318059659939545088", (v << 65).ToString(), "#65");
            Assert.AreEqual("17029621765972883315013809746918636119319879090176", (v << 66).ToString(), "#66");
            Assert.AreEqual("34059243531945766630027619493837272238639758180352", (v << 67).ToString(), "#67");
            Assert.AreEqual("68118487063891533260055238987674544477279516360704", (v << 68).ToString(), "#68");
            Assert.AreEqual("136236974127783066520110477975349088954559032721408", (v << 69).ToString(), "#69");
        }


        [TestMethod]
        public void RightShiftByInt()
        {
            var v = BigInteger.Parse("230794411440927908251127453634");
            v *= BigInteger.Pow(2, 70);

            Assert.AreEqual("272473948255566133040220955950698177909118065442816", (v >> 0).ToString(), "#0");
            Assert.AreEqual("136236974127783066520110477975349088954559032721408", (v >> 1).ToString(), "#1");
            Assert.AreEqual("68118487063891533260055238987674544477279516360704", (v >> 2).ToString(), "#2");
            Assert.AreEqual("34059243531945766630027619493837272238639758180352", (v >> 3).ToString(), "#3");
            Assert.AreEqual("17029621765972883315013809746918636119319879090176", (v >> 4).ToString(), "#4");
            Assert.AreEqual("8514810882986441657506904873459318059659939545088", (v >> 5).ToString(), "#5");
            Assert.AreEqual("4257405441493220828753452436729659029829969772544", (v >> 6).ToString(), "#6");
            Assert.AreEqual("2128702720746610414376726218364829514914984886272", (v >> 7).ToString(), "#7");
            Assert.AreEqual("1064351360373305207188363109182414757457492443136", (v >> 8).ToString(), "#8");
            Assert.AreEqual("532175680186652603594181554591207378728746221568", (v >> 9).ToString(), "#9");
            Assert.AreEqual("266087840093326301797090777295603689364373110784", (v >> 10).ToString(), "#10");
            Assert.AreEqual("133043920046663150898545388647801844682186555392", (v >> 11).ToString(), "#11");
            Assert.AreEqual("66521960023331575449272694323900922341093277696", (v >> 12).ToString(), "#12");
            Assert.AreEqual("33260980011665787724636347161950461170546638848", (v >> 13).ToString(), "#13");
            Assert.AreEqual("16630490005832893862318173580975230585273319424", (v >> 14).ToString(), "#14");
            Assert.AreEqual("8315245002916446931159086790487615292636659712", (v >> 15).ToString(), "#15");
            Assert.AreEqual("4157622501458223465579543395243807646318329856", (v >> 16).ToString(), "#16");
            Assert.AreEqual("2078811250729111732789771697621903823159164928", (v >> 17).ToString(), "#17");
            Assert.AreEqual("1039405625364555866394885848810951911579582464", (v >> 18).ToString(), "#18");
            Assert.AreEqual("519702812682277933197442924405475955789791232", (v >> 19).ToString(), "#19");
            Assert.AreEqual("259851406341138966598721462202737977894895616", (v >> 20).ToString(), "#20");
            Assert.AreEqual("129925703170569483299360731101368988947447808", (v >> 21).ToString(), "#21");
            Assert.AreEqual("64962851585284741649680365550684494473723904", (v >> 22).ToString(), "#22");
            Assert.AreEqual("32481425792642370824840182775342247236861952", (v >> 23).ToString(), "#23");
            Assert.AreEqual("16240712896321185412420091387671123618430976", (v >> 24).ToString(), "#24");
            Assert.AreEqual("8120356448160592706210045693835561809215488", (v >> 25).ToString(), "#25");
            Assert.AreEqual("4060178224080296353105022846917780904607744", (v >> 26).ToString(), "#26");
            Assert.AreEqual("2030089112040148176552511423458890452303872", (v >> 27).ToString(), "#27");
            Assert.AreEqual("1015044556020074088276255711729445226151936", (v >> 28).ToString(), "#28");
            Assert.AreEqual("507522278010037044138127855864722613075968", (v >> 29).ToString(), "#29");
            Assert.AreEqual("253761139005018522069063927932361306537984", (v >> 30).ToString(), "#30");
            Assert.AreEqual("126880569502509261034531963966180653268992", (v >> 31).ToString(), "#31");
            Assert.AreEqual("63440284751254630517265981983090326634496", (v >> 32).ToString(), "#32");
            Assert.AreEqual("31720142375627315258632990991545163317248", (v >> 33).ToString(), "#33");
            Assert.AreEqual("15860071187813657629316495495772581658624", (v >> 34).ToString(), "#34");
            Assert.AreEqual("7930035593906828814658247747886290829312", (v >> 35).ToString(), "#35");
            Assert.AreEqual("3965017796953414407329123873943145414656", (v >> 36).ToString(), "#36");
            Assert.AreEqual("1982508898476707203664561936971572707328", (v >> 37).ToString(), "#37");
            Assert.AreEqual("991254449238353601832280968485786353664", (v >> 38).ToString(), "#38");
            Assert.AreEqual("495627224619176800916140484242893176832", (v >> 39).ToString(), "#39");
            Assert.AreEqual("247813612309588400458070242121446588416", (v >> 40).ToString(), "#40");
            Assert.AreEqual("123906806154794200229035121060723294208", (v >> 41).ToString(), "#41");
            Assert.AreEqual("61953403077397100114517560530361647104", (v >> 42).ToString(), "#42");
            Assert.AreEqual("30976701538698550057258780265180823552", (v >> 43).ToString(), "#43");
            Assert.AreEqual("15488350769349275028629390132590411776", (v >> 44).ToString(), "#44");
            Assert.AreEqual("7744175384674637514314695066295205888", (v >> 45).ToString(), "#45");
            Assert.AreEqual("3872087692337318757157347533147602944", (v >> 46).ToString(), "#46");
            Assert.AreEqual("1936043846168659378578673766573801472", (v >> 47).ToString(), "#47");
            Assert.AreEqual("968021923084329689289336883286900736", (v >> 48).ToString(), "#48");
            Assert.AreEqual("484010961542164844644668441643450368", (v >> 49).ToString(), "#49");
            Assert.AreEqual("242005480771082422322334220821725184", (v >> 50).ToString(), "#50");
            Assert.AreEqual("121002740385541211161167110410862592", (v >> 51).ToString(), "#51");
            Assert.AreEqual("60501370192770605580583555205431296", (v >> 52).ToString(), "#52");
            Assert.AreEqual("30250685096385302790291777602715648", (v >> 53).ToString(), "#53");
            Assert.AreEqual("15125342548192651395145888801357824", (v >> 54).ToString(), "#54");
            Assert.AreEqual("7562671274096325697572944400678912", (v >> 55).ToString(), "#55");
            Assert.AreEqual("3781335637048162848786472200339456", (v >> 56).ToString(), "#56");
            Assert.AreEqual("1890667818524081424393236100169728", (v >> 57).ToString(), "#57");
            Assert.AreEqual("945333909262040712196618050084864", (v >> 58).ToString(), "#58");
            Assert.AreEqual("472666954631020356098309025042432", (v >> 59).ToString(), "#59");
            Assert.AreEqual("236333477315510178049154512521216", (v >> 60).ToString(), "#60");
            Assert.AreEqual("118166738657755089024577256260608", (v >> 61).ToString(), "#61");
            Assert.AreEqual("59083369328877544512288628130304", (v >> 62).ToString(), "#62");
            Assert.AreEqual("29541684664438772256144314065152", (v >> 63).ToString(), "#63");
            Assert.AreEqual("14770842332219386128072157032576", (v >> 64).ToString(), "#64");
            Assert.AreEqual("7385421166109693064036078516288", (v >> 65).ToString(), "#65");
            Assert.AreEqual("3692710583054846532018039258144", (v >> 66).ToString(), "#66");
            Assert.AreEqual("1846355291527423266009019629072", (v >> 67).ToString(), "#67");
            Assert.AreEqual("923177645763711633004509814536", (v >> 68).ToString(), "#68");
            Assert.AreEqual("461588822881855816502254907268", (v >> 69).ToString(), "#69");
        }

        [TestMethod]
        public void Bug10887()
        {
            BigInteger b = 0;
            for (var i = 1; i <= 16; i++)
            {
                b = (b * 256) + i;
            }

            var p = BigInteger.Pow(2, 32);

            Assert.AreEqual("1339673755198158349044581307228491536", b.ToString());
            Assert.AreEqual("1339673755198158349044581307228491536", ((b << 32) / p).ToString());
            Assert.AreEqual("1339673755198158349044581307228491536", ((b * p) >> 32).ToString());
        }

        [TestMethod]
        public void DefaultCtorWorks()
        {
            var a = new BigInteger();
            Assert.AreEqual(BigInteger.One, ++a, "#1");

            a = new BigInteger();
            Assert.AreEqual(BigInteger.MinusOne, --a, "#2");

            a = new BigInteger();
            Assert.AreEqual(BigInteger.MinusOne, ~a, "#3");

            a = new BigInteger();
            Assert.AreEqual("0", a.ToString(), "#4");

            a = new BigInteger();
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.AreEqual(true, a == a, "#5");

            a = new BigInteger();
            Assert.AreEqual(false, a < a, "#6");
#pragma warning restore CS1718 // Comparison made to same variable

            a = new BigInteger();
            Assert.AreEqual(true, a < 10L, "#7");

            a = new BigInteger();
            Assert.AreEqual(true, a.IsEven, "#8");

            a = new BigInteger();
            Assert.AreEqual(0, (int)a, "#9");

            a = new BigInteger();
            Assert.AreEqual((uint)0, (uint)a, "#10");

            a = new BigInteger();
            Assert.AreEqual((ulong)0, (ulong)a, "#11");

            a = new BigInteger();
            Assert.AreEqual(true, a.Equals(a), "#12");

            a = new BigInteger();
            Assert.AreEqual(a, BigInteger.Min(a, a), "#13");

            a = new BigInteger();
            Assert.AreEqual(a, BigInteger.GreatestCommonDivisor(a, a), "#14");

            a = new BigInteger();
            Assert.AreEqual(BigInteger.Zero.GetHashCode(), a.GetHashCode(), "#15");

            a = new BigInteger();
            Assert.AreEqual(BigInteger.Zero, a, "#16");
        }

        [TestMethod]
        public void Bug16526()
        {
            var x = BigInteger.Pow(2, 63);
            x *= -1;
            x -= 1;
            Assert.AreEqual("-9223372036854775809", x.ToString(), "#1");
            try
            {
                x = (long)x;
                Assert.Fail("#2 Must OVF: " + x);
            }
            catch (OverflowException)
            {
            }
        }

        [TestMethod]
        public void MinusOne()
        {
            var minusOne = BigInteger.MinusOne;

            Assert.IsFalse(minusOne.IsEven);
            Assert.IsFalse(minusOne.IsOne);
            Assert.IsFalse(minusOne.IsPowerOfTwo);
            Assert.IsFalse(minusOne.IsZero);
            Assert.AreEqual(-1, minusOne.Sign);
        }

        [TestMethod]
        public void One()
        {
            var one = BigInteger.One;

            Assert.IsFalse(one.IsEven);
            Assert.IsTrue(one.IsOne);
            Assert.IsTrue(one.IsPowerOfTwo);
            Assert.IsFalse(one.IsZero);
            Assert.AreEqual(1, one.Sign);
        }

        [TestMethod]
        public void Zero()
        {
            var zero = BigInteger.Zero;

            Assert.IsTrue(zero.IsEven);
            Assert.IsFalse(zero.IsOne);
            Assert.IsFalse(zero.IsPowerOfTwo);
            Assert.IsTrue(zero.IsZero);
            Assert.AreEqual(0, zero.Sign);
        }

        [TestMethod]
        public void Random()
        {
            var max = "26432534714839143538998938508341375449389492936207135611931371046236385860280414659368073862189301615603000443463893527273703804361856647266218472759410964268979057798543462774631912259980510080575520846081682603934587649566608158932346151315049355432937004801361578344502537300865702429436253728164365180058583916866804254965536833106467354901266304654706123552932560896874808786957654734387252964281680963136344135750381838556467139236094522411774117748615141352874979928570068255439327082539676660277104989857941859821396157749462154431239343148671646397611770487668571604363151098131876313773395912355145689712506";
            Assert.IsTrue(BigInteger.TryParse(max, NumberStyles.Number, NumberFormatInfo.CurrentInfo, out var maxBigInt));

            var random = BigInteger.One;
            while (random <= BigInteger.One || random >= maxBigInt)
            {
                random = BigInteger.Random(2048);
            }
        }

        [TestMethod]
        public void TestClientExhcangeGenerationItem130()
        {
            var test = "1090748135619415929450294929359784500348155124953172211774101106966150168922785639028532473848836817769712164169076432969224698752674677662739994265785437233596157045970922338040698100507861033047312331823982435279475700199860971612732540528796554502867919746776983759391475987142521315878719577519148811830879919426939958487087540965716419167467499326156226529675209172277001377591248147563782880558861083327174154014975134893125116015776318890295960698011614157721282527539468816519319333337503114777192360412281721018955834377615480468479252748867320362385355596601795122806756217713579819870634321561907813255153703950795271232652404894983869492174481652303803498881366210508647263668376514131031102336837488999775744046733651827239395353540348414872854639719294694323450186884189822544540647226987292160693184734654941906936646576130260972193280317171696418971553954161446191759093719524951116705577362073481319296041201283516154269044389257727700289684119460283480452306204130024913879981135908026983868205969318167819680850998649694416907952712904962404937775789698917207356355227455066183815847669135530549755439819480321732925869069136146085326382334628745456398071603058051634209386708703306545903199608523824513729625136659128221100967735450519952404248198262813831097374261650380017277916975324134846574681307337017380830353680623216336949471306191686438249305686413380231046096450953594089375540285037292470929395114028305547452584962074309438151825437902976012891749355198678420603722034900311364893046495761404333938686140037848030916292543273684533640032637639100774502371542479302473698388692892420946478947733800387782741417786484770190108867879778991633218628640533982619322466154883011452291890252336487236086654396093853898628805813177559162076363154436494477507871294119841637867701722166609831201845484078070518041336869808398454625586921201308185638888082699408686536045192649569198110353659943111802300636106509865023943661829436426563007917282050894429388841748885398290707743052973605359277515749619730823773215894755121761467887865327707115573804264519206349215850195195364813387526811742474131549802130246506341207020335797706780705406945275438806265978516209706795702579244075380490231741030862614968783306207869687868108423639971983209077624758080499988275591392787267627182442892809646874228263172435642368588260139161962836121481966092745325488641054238839295138992979335446110090325230955276870524611359124918392740353154294858383359";
            _ = BigInteger.TryParse(test, NumberStyles.Number, NumberFormatInfo.CurrentInfo, out var prime);

            BigInteger group = 2;
            var bitLength = prime.BitLength;

            BigInteger clientExchangeValue;
            do
            {
                var randomValue = BigInteger.Random(bitLength);

                //clientExchangeValue = BigInteger.ModPow(group, randomValue, prime);
                clientExchangeValue = (group ^ randomValue) % prime;
            }
            while (clientExchangeValue < 1 || clientExchangeValue > (prime - 1));
        }

        [TestMethod]
        public void TestClientExhcangeGenerationGroup1()
        {
            var test = "00FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD129024E088A67CC74020BBEA63B139B22514A08798E3404DDEF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7EDEE386BFB5A899FA5AE9F24117C4B1FE649286651ECE65381FFFFFFFFFFFFFFFF";
            _ = BigInteger.TryParse(test, NumberStyles.AllowHexSpecifier, NumberFormatInfo.CurrentInfo, out var prime);

            BigInteger group = 2;
            var bitLength = prime.BitLength;

            BigInteger clientExchangeValue;
            do
            {
                var randomValue = BigInteger.Random(bitLength);

                //clientExchangeValue = BigInteger.ModPow(group, randomValue, prime);
                clientExchangeValue = (group ^ randomValue) % prime;
            }
            while (clientExchangeValue < 1 || clientExchangeValue > (prime - 1));
        }

        [TestMethod]
        public void TestClientExhcangeGenerationGroup14()
        {
            var test = "00FFFFFFFFFFFFFFFFC90FDAA22168C234C4C6628B80DC1CD129024E088A67CC74020BBEA63B139B22514A08798E3404DDEF9519B3CD3A431B302B0A6DF25F14374FE1356D6D51C245E485B576625E7EC6F44C42E9A637ED6B0BFF5CB6F406B7EDEE386BFB5A899FA5AE9F24117C4B1FE649286651ECE45B3DC2007CB8A163BF0598DA48361C55D39A69163FA8FD24CF5F83655D23DCA3AD961C62F356208552BB9ED529077096966D670C354E4ABC9804F1746C08CA18217C32905E462E36CE3BE39E772C180E86039B2783A2EC07A28FB5C55DF06F4C52C9DE2BCBF6955817183995497CEA956AE515D2261898FA051015728E5A8AACAA68FFFFFFFFFFFFFFFF";
            _ = BigInteger.TryParse(test, NumberStyles.AllowHexSpecifier, NumberFormatInfo.CurrentInfo, out var prime);

            BigInteger group = 2;
            var bitLength = prime.BitLength;

            BigInteger clientExchangeValue;
            do
            {
                var randomValue = BigInteger.Random(bitLength);

                //clientExchangeValue = BigInteger.ModPow(group, randomValue, prime);
                clientExchangeValue = (group ^ randomValue) % prime;
            }
            while (clientExchangeValue < 1 || clientExchangeValue > (prime - 1));
        }

        private static void AssertEqual(byte[] a, byte[] b)
        {
            Assert.IsTrue(a.IsEqualTo(b));
        }
    }
}
