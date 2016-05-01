// vim: noet
// System.Numerics.BigInt
//
// Rodrigo Kumpera (rkumpera@novell.com)

//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// A big chuck of code comes the DLR (as hosted in http://ironpython.codeplex.com),
// which has the following License:
//
/* ****************************************************************************
*
* Copyright (c) Microsoft Corporation.
*
* This source code is subject to terms and conditions of the Microsoft Public License. A
* copy of the license can be found in the License.html file at the root of this distribution. If
* you cannot locate the Microsoft Public License, please send an email to
* dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound
* by the terms of the Microsoft Public License.
*
* You must not remove this notice, or any other, from this software.
*
*
* ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using Renci.SshNet.Security.Cryptography;

/*
Optimization
Have proper popcount function for IsPowerOfTwo
Use unsafe ops to avoid bounds check
CoreAdd could avoid some resizes by checking for equal sized array that top overflow
For bitwise operators, hoist the conditionals out of their main loop
Optimize BitScanBackward
Use a carry variable to make shift opts do half the number of array ops.
Schoolbook multiply is O(n^2), use Karatsuba /Toom-3 for large numbers
*/
namespace Renci.SshNet.Common
{
    /// <summary>
    /// Represents an arbitrarily large signed integer.
    /// </summary>
    public struct BigInteger : IComparable, IFormattable, IComparable<BigInteger>, IEquatable<BigInteger>
    {
        private const ulong Base = 0x100000000;
        private const int Decimalsignmask = unchecked((int)0x80000000);
        private const int Bias = 1075;

        private static readonly uint[] _zero = new uint[1];
        private static readonly uint[] _one = { 1 };

        //LSB on [0]
        private readonly uint[] _data;
        private readonly short _sign;

        /// <summary>
        /// Gets number of bits used by the number.
        /// </summary>
        /// <value>
        /// The number of the bit used.
        /// </value>
        public int BitLength
        {
            get
            {
                if (_sign == 0)
                    return 0;

                var msbIndex = _data.Length - 1;

                while (_data[msbIndex] == 0)
                    msbIndex--;

                var msbBitCount = BitScanBackward(_data[msbIndex]) + 1;

                return msbIndex * 4 * 8 + msbBitCount + ((_sign > 0) ? 0 : 1);
            }
        }

        #region Constractors

        /// <summary>
        /// Initializes a new instance of the <see cref="BigInteger"/> struct.
        /// </summary>
        /// <param name="sign">The sign.</param>
        /// <param name="data">The data.</param>
        public BigInteger(short sign, uint[] data)
        {
            _sign = sign;
            _data = data;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigInteger"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public BigInteger(int value)
        {
            if (value == 0)
            {
                _sign = 0;
                _data = _zero;
            }
            else if (value > 0)
            {
                _sign = 1;
                _data = new[] { (uint)value };
            }
            else
            {
                _sign = -1;
                _data = new[] { (uint)-value };
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigInteger"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public BigInteger(uint value)
        {
            if (value == 0)
            {
                _sign = 0;
                _data = _zero;
            }
            else
            {
                _sign = 1;
                _data = new[] { value };
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigInteger"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public BigInteger(long value)
        {
            if (value == 0)
            {
                _sign = 0;
                _data = _zero;
            }
            else if (value > 0)
            {
                _sign = 1;
                uint low = (uint)value;
                uint high = (uint)(value >> 32);

                _data = new uint[high != 0 ? 2 : 1];
                _data[0] = low;
                if (high != 0)
                    _data[1] = high;
            }
            else
            {
                _sign = -1;
                value = -value;
                uint low = (uint)value;
                uint high = (uint)((ulong)value >> 32);

                _data = new uint[high != 0 ? 2 : 1];
                _data[0] = low;
                if (high != 0)
                    _data[1] = high;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigInteger"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public BigInteger(ulong value)
        {
            if (value == 0)
            {
                _sign = 0;
                _data = _zero;
            }
            else
            {
                _sign = 1;
                uint low = (uint)value;
                uint high = (uint)(value >> 32);

                _data = new uint[high != 0 ? 2 : 1];
                _data[0] = low;
                if (high != 0)
                    _data[1] = high;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigInteger"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public BigInteger(double value)
        {
            if (double.IsNaN(value) || Double.IsInfinity(value))
                throw new OverflowException();

            byte[] bytes = BitConverter.GetBytes(value);
            ulong mantissa = Mantissa(bytes);
            if (mantissa == 0)
            {
                // 1.0 * 2**exp, we have a power of 2
                int exponent = Exponent(bytes);
                if (exponent == 0)
                {
                    _sign = 0;
                    _data = _zero;
                    return;
                }

                BigInteger res = Negative(bytes) ? MinusOne : One;
                res = res << (exponent - 0x3ff);
                _sign = res._sign;
                _data = res._data;
            }
            else
            {
                // 1.mantissa * 2**exp
                int exponent = Exponent(bytes);
                mantissa |= 0x10000000000000ul;
                BigInteger res = mantissa;
                res = exponent > Bias ? res << (exponent - Bias) : res >> (Bias - exponent);

                _sign = (short)(Negative(bytes) ? -1 : 1);
                _data = res._data;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigInteger"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public BigInteger(float value)
            : this((double)value)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigInteger"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public BigInteger(decimal value)
        {
            // First truncate to get scale to 0 and extract bits
            int[] bits = Decimal.GetBits(Decimal.Truncate(value));

            int size = 3;
            while (size > 0 && bits[size - 1] == 0) size--;

            if (size == 0)
            {
                _sign = 0;
                _data = _zero;
                return;
            }

            _sign = (short)((bits[3] & Decimalsignmask) != 0 ? -1 : 1);

            _data = new uint[size];
            _data[0] = (uint)bits[0];
            if (size > 1)
                _data[1] = (uint)bits[1];
            if (size > 2)
                _data[2] = (uint)bits[2];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigInteger"/> struct.
        /// </summary>
        /// <param name="value">The value.</param>
        public BigInteger(byte[] value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            int len = value.Length;

            if (len == 0 || (len == 1 && value[0] == 0))
            {
                _sign = 0;
                _data = _zero;
                return;
            }

            if ((value[len - 1] & 0x80) != 0)
                _sign = -1;
            else
                _sign = 1;

            if (_sign == 1)
            {
                while (value[len - 1] == 0)
                    --len;

                int size;
                var fullWords = size = len / 4;
                if ((len & 0x3) != 0)
                    ++size;

                _data = new uint[size];
                var j = 0;
                for (var i = 0; i < fullWords; ++i)
                {
                    _data[i] = (uint)value[j++] |
                    (uint)(value[j++] << 8) |
                    (uint)(value[j++] << 16) |
                    (uint)(value[j++] << 24);
                }
                size = len & 0x3;
                if (size > 0)
                {
                    var idx = _data.Length - 1;
                    for (var i = 0; i < size; ++i)
                        _data[idx] |= (uint)(value[j++] << (i * 8));
                }
            }
            else
            {
                int size;
                var fullWords = size = len / 4;
                if ((len & 0x3) != 0)
                    ++size;

                _data = new uint[size];

                uint word, borrow = 1;
                ulong sub;
                var j = 0;

                for (var i = 0; i < fullWords; ++i)
                {
                    word = (uint)value[j++] |
                    (uint)(value[j++] << 8) |
                    (uint)(value[j++] << 16) |
                    (uint)(value[j++] << 24);

                    sub = (ulong)word - borrow;
                    word = (uint)sub;
                    borrow = (uint)(sub >> 32) & 0x1u;
                    _data[i] = ~word;
                }
                size = len & 0x3;

                if (size > 0)
                {
                    word = 0;
                    uint storeMask = 0;
                    for (var i = 0; i < size; ++i)
                    {
                        word |= (uint)(value[j++] << (i * 8));
                        storeMask = (storeMask << 8) | 0xFF;
                    }

                    sub = word - borrow;
                    word = (uint)sub;
                    borrow = (uint)(sub >> 32) & 0x1u;

                    _data[_data.Length - 1] = ~word & storeMask;
                }
                if (borrow != 0) //FIXME I believe this can't happen, can someone write a test for it?
                    throw new Exception("non zero final carry");
            }
        }

        #endregion

        #region Operators

        /// <summary>
        /// Defines an explicit conversion of a System.Numerics.BigInteger object to a 32-bit signed integer value.
        /// </summary>
        /// <param name="value">The value to convert to a 32-bit signed integer.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static explicit operator int(BigInteger value)
        {
            int r;
            if (!value.AsInt32(out r))
                throw new OverflowException();
            return r;
        }

        /// <summary>
        /// Defines an explicit conversion of a System.Numerics.BigInteger object to an unsigned 32-bit integer value.
        /// </summary>
        /// <param name="value">The value to convert to an unsigned 32-bit integer.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static explicit operator uint(BigInteger value)
        {
            if (value._data.Length > 1 || value._sign == -1)
                throw new OverflowException();
            return value._data[0];
        }

        /// <summary>
        /// Defines an explicit conversion of a System.Numerics.BigInteger object to a 16-bit signed integer value.
        /// </summary>
        /// <param name="value">The value to convert to a 16-bit signed integer.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static explicit operator short(BigInteger value)
        {
            int val = (int)value;
            if (val < short.MinValue || val > short.MaxValue)
                throw new OverflowException();
            return (short)val;
        }

        /// <summary>
        /// Defines an explicit conversion of a System.Numerics.BigInteger object to an unsigned 16-bit integer value.
        /// </summary>
        /// <param name="value">The value to convert to an unsigned 16-bit integer.</param>
        /// <returns>
        /// An object that contains the value of the value parameter
        /// </returns>
        public static explicit operator ushort(BigInteger value)
        {
            var val = (uint) value;
            if (val > ushort.MaxValue)
                throw new OverflowException();
            return (ushort) val;
        }

        /// <summary>
        /// Defines an explicit conversion of a System.Numerics.BigInteger object to an unsigned byte value.
        /// </summary>
        /// <param name="value">The value to convert to a System.Byte.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static explicit operator byte(BigInteger value)
        {
            var val = (uint) value;
            if (val > byte.MaxValue)
                throw new OverflowException();
            return (byte) val;
        }

        /// <summary>
        /// Defines an explicit conversion of a System.Numerics.BigInteger object to a signed 8-bit value.
        /// </summary>
        /// <param name="value">The value to convert to a signed 8-bit value.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static explicit operator sbyte(BigInteger value)
        {
            var val = (int) value;
            if (val < sbyte.MinValue || val > sbyte.MaxValue)
                throw new OverflowException();
            return (sbyte) val;
        }

        /// <summary>
        /// Defines an explicit conversion of a System.Numerics.BigInteger object to a 64-bit signed integer value.
        /// </summary>
        /// <param name="value">The value to convert to a 64-bit signed integer.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static explicit operator long(BigInteger value)
        {
            if (value._sign == 0)
                return 0;

            if (value._data.Length > 2)
                throw new OverflowException();

            var low = value._data[0];

            if (value._data.Length == 1)
            {
                if (value._sign == 1)
                    return (long)low;
                long res = (long)low;
                return -res;
            }

            var high = value._data[1];

            if (value._sign == 1)
            {
                if (high >= 0x80000000u)
                    throw new OverflowException();
                return (((long)high) << 32) | low;
            }

            if (high > 0x80000000u)
                throw new OverflowException();

            return -((((long)high) << 32) | (long)low);
        }

        /// <summary>
        /// Defines an explicit conversion of a System.Numerics.BigInteger object to an unsigned 64-bit integer value.
        /// </summary>
        /// <param name="value">The value to convert to an unsigned 64-bit integer.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static explicit operator ulong(BigInteger value)
        {
            if (value._data.Length > 2 || value._sign == -1)
                throw new OverflowException();

            var low = value._data[0];
            if (value._data.Length == 1)
                return low;

            var high = value._data[1];
            return (((ulong)high) << 32) | low;
        }

        /// <summary>
        /// Defines an explicit conversion of a System.Numerics.BigInteger object to a <see cref="System.Double"/> value.
        /// </summary>
        /// <param name="value">The value to convert to a <see cref="System.Double"/>.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static explicit operator double(BigInteger value)
        {
            //FIXME
            try
            {
                return double.Parse(value.ToString(),
                     CultureInfo.InvariantCulture.NumberFormat);
            }
            catch (OverflowException)
            {
                return value._sign == -1 ? double.NegativeInfinity : double.PositiveInfinity;
            }
        }

        /// <summary>
        /// Defines an explicit conversion of a System.Numerics.BigInteger object to a single-precision floating-point value.
        /// </summary>
        /// <param name="value">The value to convert to a single-precision floating-point value.</param>
        /// <returns>
        /// An object that contains the closest possible representation of the value parameter.
        /// </returns>
        public static explicit operator float(BigInteger value)
        {
            //FIXME
            try
            {
                return float.Parse(value.ToString(),
                CultureInfo.InvariantCulture.NumberFormat);
            }
            catch (OverflowException)
            {
                return value._sign == -1 ? float.NegativeInfinity : float.PositiveInfinity;
            }
        }

        /// <summary>
        /// Defines an explicit conversion of a System.Numerics.BigInteger object to a <see cref="System.Decimal"/> value.
        /// </summary>
        /// <param name="value">The value to convert to a <see cref="System.Decimal"/>.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static explicit operator decimal(BigInteger value)
        {
            if (value._sign == 0)
                return decimal.Zero;

            var data = value._data;
            if (data.Length > 3)
                throw new OverflowException();

            int lo = 0, mi = 0, hi = 0;
            if (data.Length > 2)
                hi = (int) data[2];
            if (data.Length > 1)
                mi = (int) data[1];
            if (data.Length > 0)
                lo = (int) data[0];

            return new decimal(lo, mi, hi, value._sign < 0, 0);
        }

        /// <summary>
        /// Defines an implicit conversion of a signed 32-bit integer to a System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="value">The value to convert to a System.Numerics.BigInteger.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static implicit operator BigInteger(int value)
        {
            return new BigInteger(value);
        }

        /// <summary>
        /// Defines an implicit conversion of a 32-bit unsigned integer to a System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="value">The value to convert to a System.Numerics.BigInteger.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static implicit operator BigInteger(uint value)
        {
            return new BigInteger(value);
        }

        /// <summary>
        /// Defines an implicit conversion of a signed 16-bit integer to a System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="value">The value to convert to a System.Numerics.BigInteger.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static implicit operator BigInteger(short value)
        {
            return new BigInteger(value);
        }

        /// <summary>
        /// Defines an implicit conversion of a 16-bit unsigned integer to a System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="value">The value to convert to a System.Numerics.BigInteger.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static implicit operator BigInteger(ushort value)
        {
            return new BigInteger(value);
        }

        /// <summary>
        /// Defines an implicit conversion of an unsigned byte to a System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="value">The value to convert to a System.Numerics.BigInteger.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static implicit operator BigInteger(byte value)
        {
            return new BigInteger(value);
        }

        /// <summary>
        /// Defines an implicit conversion of an 8-bit signed integer to a System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="value">The value to convert to a System.Numerics.BigInteger.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static implicit operator BigInteger(sbyte value)
        {
            return new BigInteger(value);
        }

        /// <summary>
        /// Defines an implicit conversion of a signed 64-bit integer to a System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="value">The value to convert to a System.Numerics.BigInteger.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static implicit operator BigInteger(long value)
        {
            return new BigInteger(value);
        }

        /// <summary>
        /// Defines an implicit conversion of a 64-bit unsigned integer to a System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="value">The value to convert to a System.Numerics.BigInteger.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static implicit operator BigInteger(ulong value)
        {
            return new BigInteger(value);
        }

        /// <summary>
        /// Defines an explicit conversion of a <see cref="System.Double"/> value to a System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="value">The value to convert to a System.Numerics.BigInteger.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static explicit operator BigInteger(double value)
        {
            return new BigInteger(value);
        }

        /// <summary>
        /// Defines an explicit conversion of a <see cref="System.Single"/> object to a System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="value">The value to convert to a System.Numerics.BigInteger.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static explicit operator BigInteger(float value)
        {
            return new BigInteger(value);
        }

        /// <summary>
        /// Defines an explicit conversion of a <see cref="System.Decimal"/> object to a System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="value">The value to convert to a System.Numerics.BigInteger.</param>
        /// <returns>
        /// An object that contains the value of the value parameter.
        /// </returns>
        public static explicit operator BigInteger(decimal value)
        {
            return new BigInteger(value);
        }

        /// <summary>
        /// Adds the values of two specified <see cref="BigInteger"/> objects.
        /// </summary>
        /// <param name="left">The first value to add.</param>
        /// <param name="right">The second value to add.</param>
        /// <returns>
        /// The sum of left and right.
        /// </returns>
        public static BigInteger operator +(BigInteger left, BigInteger right)
        {
            if (left._sign == 0)
                return right;
            if (right._sign == 0)
                return left;

            if (left._sign == right._sign)
                return new BigInteger(left._sign, CoreAdd(left._data, right._data));

            int r = CoreCompare(left._data, right._data);

            if (r == 0)
                return new BigInteger(0, _zero);

            if (r > 0) //left > right
                return new BigInteger(left._sign, CoreSub(left._data, right._data));

            return new BigInteger(right._sign, CoreSub(right._data, left._data));
        }

        /// <summary>
        /// Subtracts a <see cref="BigInteger"/> value from another <see cref="BigInteger"/> value.
        /// </summary>
        /// <param name="left">The value to subtract from (the minuend).</param>
        /// <param name="right">The value to subtract (the subtrahend).</param>
        /// <returns>
        /// The result of subtracting right from left.
        /// </returns>
        public static BigInteger operator -(BigInteger left, BigInteger right)
        {
            if (right._sign == 0)
                return left;
            if (left._sign == 0)
                return new BigInteger((short)-right._sign, right._data);

            if (left._sign == right._sign)
            {
                int r = CoreCompare(left._data, right._data);

                if (r == 0)
                    return new BigInteger(0, _zero);

                if (r > 0) //left > right
                    return new BigInteger(left._sign, CoreSub(left._data, right._data));

                return new BigInteger((short)-right._sign, CoreSub(right._data, left._data));
            }

            return new BigInteger(left._sign, CoreAdd(left._data, right._data));
        }

        /// <summary>
        /// Multiplies two specified <see cref="BigInteger"/> values.
        /// </summary>
        /// <param name="left">The first value to multiply.</param>
        /// <param name="right">The second value to multiply.</param>
        /// <returns>
        /// The product of left and right.
        /// </returns>
        public static BigInteger operator *(BigInteger left, BigInteger right)
        {
            if (left._sign == 0 || right._sign == 0)
                return new BigInteger(0, _zero);

            if (left._data[0] == 1 && left._data.Length == 1)
            {
                if (left._sign == 1)
                    return right;
                return new BigInteger((short)-right._sign, right._data);
            }

            if (right._data[0] == 1 && right._data.Length == 1)
            {
                if (right._sign == 1)
                    return left;
                return new BigInteger((short)-left._sign, left._data);
            }

            var a = left._data;
            var b = right._data;

            var res = new uint[a.Length + b.Length];

            for (var i = 0; i < a.Length; ++i)
            {
                var ai = a[i];
                var k = i;

                ulong carry = 0;
                for (var j = 0; j < b.Length; ++j)
                {
                    carry = carry + ((ulong)ai) * b[j] + res[k];
                    res[k++] = (uint)carry;
                    carry >>= 32;
                }

                while (carry != 0)
                {
                    carry += res[k];
                    res[k++] = (uint)carry;
                    carry >>= 32;
                }
            }

            int m;
            for (m = res.Length - 1; m >= 0 && res[m] == 0; --m) ;
            if (m < res.Length - 1)
                res = Resize(res, m + 1);

            return new BigInteger((short)(left._sign * right._sign), res);
        }

        /// <summary>
        /// Divides a specified <see cref="BigInteger"/> value by another specified <see cref="BigInteger"/> value by using integer division.
        /// </summary>
        /// <param name="dividend">The value to be divided.</param>
        /// <param name="divisor">The value to divide by.</param>
        /// <returns>
        /// The integral result of the division.
        /// </returns>
        public static BigInteger operator /(BigInteger dividend, BigInteger divisor)
        {
            if (divisor._sign == 0)
                throw new DivideByZeroException();

            if (dividend._sign == 0)
                return dividend;

            uint[] quotient;
            uint[] remainderValue;

            DivModUnsigned(dividend._data, divisor._data, out quotient, out remainderValue);

            int i;
            for (i = quotient.Length - 1; i >= 0 && quotient[i] == 0; --i) ;
            if (i == -1)
                return new BigInteger(0, _zero);
            if (i < quotient.Length - 1)
                quotient = Resize(quotient, i + 1);

            return new BigInteger((short) (dividend._sign*divisor._sign), quotient);
        }

        /// <summary>
        /// Returns the remainder that results from division with two specified <see cref="BigInteger"/> values.
        /// </summary>
        /// <param name="dividend">The value to be divided.</param>
        /// <param name="divisor">The value to divide by.</param>
        /// <returns>
        /// The remainder that results from the division.
        /// </returns>
        public static BigInteger operator %(BigInteger dividend, BigInteger divisor)
        {
            if (divisor._sign == 0)
                throw new DivideByZeroException();

            if (dividend._sign == 0)
                return dividend;

            uint[] quotient;
            uint[] remainderValue;

            DivModUnsigned(dividend._data, divisor._data, out quotient, out remainderValue);

            int i;
            for (i = remainderValue.Length - 1; i >= 0 && remainderValue[i] == 0; --i) ;
            if (i == -1)
                return new BigInteger(0, _zero);

            if (i < remainderValue.Length - 1)
                remainderValue = Resize(remainderValue, i + 1);
            return new BigInteger(dividend._sign, remainderValue);
        }

        /// <summary>
        /// Negates a specified BigInteger value.
        /// </summary>
        /// <param name="value">The value to negate.</param>
        /// <returns>
        /// The result of the value parameter multiplied by negative one (-1).
        /// </returns>
        public static BigInteger operator -(BigInteger value)
        {
            if (value._sign == 0)
                return value;
            return new BigInteger((short)-value._sign, value._data);
        }

        /// <summary>
        /// Returns the value of the <see cref="BigInteger"/> operand. (The sign of the operand is unchanged.)
        /// </summary>
        /// <param name="value">An integer value.</param>
        /// <returns>
        /// The value of the value operand.
        /// </returns>
        public static BigInteger operator +(BigInteger value)
        {
            return value;
        }

        /// <summary>
        /// Increments a <see cref="BigInteger"/> value by 1.
        /// </summary>
        /// <param name="value">The value to increment.</param>
        /// <returns>
        /// The value of the value parameter incremented by 1.
        /// </returns>
        public static BigInteger operator ++(BigInteger value)
        {
            var sign = value._sign;
            var data = value._data;
            if (data.Length == 1)
            {
                if (sign == -1 && data[0] == 1)
                    return new BigInteger(0, _zero);
                if (sign == 0)
                    return new BigInteger(1, _one);
            }

            if (sign == -1)
                data = CoreSub(data, 1);
            else
                data = CoreAdd(data, 1);

            return new BigInteger(sign, data);
        }

        /// <summary>
        /// Decrements a <see cref="BigInteger"/> value by 1.
        /// </summary>
        /// <param name="value">The value to decrement.</param>
        /// <returns>
        /// The value of the value parameter decremented by 1.
        /// </returns>
        public static BigInteger operator --(BigInteger value)
        {
            var sign = value._sign;
            var data = value._data;
            if (data.Length == 1)
            {
                if (sign == 1 && data[0] == 1)
                    return new BigInteger(0, _zero);
                if (sign == 0)
                    return new BigInteger(-1, _one);
            }

            if (sign == -1)
                data = CoreAdd(data, 1);
            else
                data = CoreSub(data, 1);

            return new BigInteger(sign, data);
        }

        /// <summary>
        /// Performs a bitwise And operation on two <see cref="BigInteger"/> values.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">The second value.</param>
        /// <returns>
        /// The result of the bitwise And operation.
        /// </returns>
        public static BigInteger operator &(BigInteger left, BigInteger right)
        {
            if (left._sign == 0)
                return left;

            if (right._sign == 0)
                return right;

            var a = left._data;
            var b = right._data;
            int ls = left._sign;
            int rs = right._sign;

            bool neg_res = (ls == rs) && (ls == -1);

            var result = new uint[Math.Max(a.Length, b.Length)];

            ulong ac = 1, bc = 1, borrow = 1;

            int i;
            for (i = 0; i < result.Length; ++i)
            {
                uint va = 0;
                if (i < a.Length)
                    va = a[i];
                if (ls == -1)
                {
                    ac = ~va + ac;
                    va = (uint)ac;
                    ac = (uint)(ac >> 32);
                }

                uint vb = 0;
                if (i < b.Length)
                    vb = b[i];
                if (rs == -1)
                {
                    bc = ~vb + bc;
                    vb = (uint)bc;
                    bc = (uint)(bc >> 32);
                }

                uint word = va & vb;

                if (neg_res)
                {
                    borrow = word - borrow;
                    word = ~(uint)borrow;
                    borrow = (uint)(borrow >> 32) & 0x1u;
                }

                result[i] = word;
            }

            for (i = result.Length - 1; i >= 0 && result[i] == 0; --i) ;
            if (i == -1)
                return new BigInteger(0, _zero);

            if (i < result.Length - 1)
                result = Resize(result, i + 1);

            return new BigInteger(neg_res ? (short)-1 : (short)1, result);
        }

        /// <summary>
        /// Performs a bitwise Or operation on two <see cref="BigInteger"/> values.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">The second value.</param>
        /// <returns>
        /// The result of the bitwise Or operation.
        /// </returns>
        public static BigInteger operator |(BigInteger left, BigInteger right)
        {
            if (left._sign == 0)
                return right;

            if (right._sign == 0)
                return left;

            var a = left._data;
            var b = right._data;
            int ls = left._sign;
            int rs = right._sign;

            bool neg_res = (ls == -1) || (rs == -1);

            var result = new uint[Math.Max(a.Length, b.Length)];

            ulong ac = 1, bc = 1, borrow = 1;

            int i;
            for (i = 0; i < result.Length; ++i)
            {
                uint va = 0;
                if (i < a.Length)
                    va = a[i];
                if (ls == -1)
                {
                    ac = ~va + ac;
                    va = (uint)ac;
                    ac = (uint)(ac >> 32);
                }

                uint vb = 0;
                if (i < b.Length)
                    vb = b[i];
                if (rs == -1)
                {
                    bc = ~vb + bc;
                    vb = (uint)bc;
                    bc = (uint)(bc >> 32);
                }

                uint word = va | vb;

                if (neg_res)
                {
                    borrow = word - borrow;
                    word = ~(uint)borrow;
                    borrow = (uint)(borrow >> 32) & 0x1u;
                }

                result[i] = word;
            }

            for (i = result.Length - 1; i >= 0 && result[i] == 0; --i) ;
            if (i == -1)
                return new BigInteger(0, _zero);

            if (i < result.Length - 1)
                result = Resize(result, i + 1);

            return new BigInteger(neg_res ? (short)-1 : (short)1, result);
        }

        /// <summary>
        /// Performs a bitwise exclusive Or (XOr) operation on two <see cref="BigInteger"/> values.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">The second value.</param>
        /// <returns>
        /// The result of the bitwise Or operation.
        /// </returns>
        public static BigInteger operator ^(BigInteger left, BigInteger right)
        {
            if (left._sign == 0)
                return right;

            if (right._sign == 0)
                return left;

            var a = left._data;
            var b = right._data;
            int ls = left._sign;
            int rs = right._sign;

            bool neg_res = (ls == -1) ^ (rs == -1);

            var result = new uint[Math.Max(a.Length, b.Length)];

            ulong ac = 1, bc = 1, borrow = 1;

            int i;
            for (i = 0; i < result.Length; ++i)
            {
                uint va = 0;
                if (i < a.Length)
                    va = a[i];
                if (ls == -1)
                {
                    ac = ~va + ac;
                    va = (uint)ac;
                    ac = (uint)(ac >> 32);
                }

                uint vb = 0;
                if (i < b.Length)
                    vb = b[i];
                if (rs == -1)
                {
                    bc = ~vb + bc;
                    vb = (uint)bc;
                    bc = (uint)(bc >> 32);
                }

                uint word = va ^ vb;

                if (neg_res)
                {
                    borrow = word - borrow;
                    word = ~(uint)borrow;
                    borrow = (uint)(borrow >> 32) & 0x1u;
                }

                result[i] = word;
            }

            for (i = result.Length - 1; i >= 0 && result[i] == 0; --i) ;
            if (i == -1)
                return new BigInteger(0, _zero);

            if (i < result.Length - 1)
                result = Resize(result, i + 1);

            return new BigInteger(neg_res ? (short)-1 : (short)1, result);
        }

        /// <summary>
        /// Returns the bitwise one's complement of a <see cref="BigInteger"/> value.
        /// </summary>
        /// <param name="value">An integer value.</param>
        /// <returns>
        /// The bitwise one's complement of value.
        /// </returns>
        public static BigInteger operator ~(BigInteger value)
        {
            if (value._sign == 0)
                return new BigInteger(-1, _one);

            var data = value._data;
            int sign = value._sign;

            bool neg_res = sign == 1;

            var result = new uint[data.Length];

            ulong carry = 1, borrow = 1;

            int i;
            for (i = 0; i < result.Length; ++i)
            {
                var word = data[i];
                if (sign == -1)
                {
                    carry = ~word + carry;
                    word = (uint)carry;
                    carry = (uint)(carry >> 32);
                }

                word = ~word;

                if (neg_res)
                {
                    borrow = word - borrow;
                    word = ~(uint)borrow;
                    borrow = (uint)(borrow >> 32) & 0x1u;
                }

                result[i] = word;
            }

            for (i = result.Length - 1; i >= 0 && result[i] == 0; --i) ;
            if (i == -1)
                return new BigInteger(0, _zero);

            if (i < result.Length - 1)
                result = Resize(result, i + 1);

            return new BigInteger(neg_res ? (short)-1 : (short)1, result);
        }

        /// <summary>
        /// Shifts a <see cref="BigInteger"/> value a specified number of bits to the left.
        /// </summary>
        /// <param name="value">The value whose bits are to be shifted.</param>
        /// <param name="shift">The number of bits to shift value to the left.</param>
        /// <returns>
        /// A value that has been shifted to the left by the specified number of bits.
        /// </returns>
        public static BigInteger operator <<(BigInteger value, int shift)
        {
            if (shift == 0 || value._sign == 0)
                return value;
            if (shift < 0)
                return value >> -shift;

            var data = value._data;
            int sign = value._sign;

            var topMostIdx = BitScanBackward(data[data.Length - 1]);
            var bits = shift - (31 - topMostIdx);
            var extraWords = (bits >> 5) + ((bits & 0x1F) != 0 ? 1 : 0);

            var res = new uint[data.Length + extraWords];

            var idxShift = shift >> 5;
            var bitShift = shift & 0x1F;
            var carryShift = 32 - bitShift;

            for (var i = 0; i < data.Length; ++i)
            {
                var word = data[i];
                res[i + idxShift] |= word << bitShift;
                if (i + idxShift + 1 < res.Length)
                    res[i + idxShift + 1] = word >> carryShift;
            }

            return new BigInteger((short)sign, res);
        }

        /// <summary>
        /// Shifts a System.Numerics.BigInteger value a specified number of bits to the right.
        /// </summary>
        /// <param name="value">The value whose bits are to be shifted.</param>
        /// <param name="shift">The number of bits to shift value to the right.</param>
        /// <returns>
        /// A value that has been shifted to the right by the specified number of bits.
        /// </returns>
        public static BigInteger operator >>(BigInteger value, int shift)
        {
            if (shift == 0 || value._sign == 0)
                return value;
            if (shift < 0)
                return value << -shift;

            var data = value._data;
            int sign = value._sign;

            var topMostIdx = BitScanBackward(data[data.Length - 1]);
            var idx_shift = shift >> 5;
            var bit_shift = shift & 0x1F;

            var extra_words = idx_shift;
            if (bit_shift > topMostIdx)
                ++extra_words;
            var size = data.Length - extra_words;

            if (size <= 0)
            {
                if (sign == 1)
                    return new BigInteger(0, _zero);
                return new BigInteger(-1, _one);
            }

            var res = new uint[size];
            var carryShift = 32 - bit_shift;

            for (var i = data.Length - 1; i >= idx_shift; --i)
            {
                var word = data[i];

                if (i - idx_shift < res.Length)
                    res[i - idx_shift] |= word >> bit_shift;
                if (i - idx_shift - 1 >= 0)
                    res[i - idx_shift - 1] = word << carryShift;
            }

            //Round down instead of toward zero
            if (sign == -1)
            {
                for (var i = 0; i < idx_shift; i++)
                {
                    if (data[i] != 0u)
                    {
                        var tmp = new BigInteger((short)sign, res);
                        --tmp;
                        return tmp;
                    }
                }
                if (bit_shift > 0 && (data[idx_shift] << carryShift) != 0u)
                {
                    var tmp = new BigInteger((short)sign, res);
                    --tmp;
                    return tmp;
                }
            }
            return new BigInteger((short)sign, res);
        }

        /// <summary>
        /// Returns a value that indicates whether a <see cref="BigInteger"/> value is less than another <see cref="BigInteger"/> value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is less than right; otherwise, false.
        /// </returns>
        public static bool operator <(BigInteger left, BigInteger right)
        {
            return Compare(left, right) < 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a <see cref="BigInteger"/> value is less than a 64-bit signed integer.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is less than right; otherwise, false.
        /// </returns>
        public static bool operator <(BigInteger left, long right)
        {
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a 64-bit signed integer is less than a <see cref="BigInteger"/> value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is less than right; otherwise, false.
        /// </returns>
        public static bool operator <(long left, BigInteger right)
        {
            return right.CompareTo(left) > 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a <see cref="BigInteger"/> value is less than a 64-bit unsigned integer.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is less than right; otherwise, false.
        /// </returns>
        public static bool operator <(BigInteger left, ulong right)
        {
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a 64-bit unsigned integer is less than a <see cref="BigInteger"/> value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is less than right; otherwise, false.
        /// </returns>
        public static bool operator <(ulong left, BigInteger right)
        {
            return right.CompareTo(left) > 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a System.Numerics.BigInteger value is less than or equal to another System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is less than or equal to right; otherwise, false.
        /// </returns>
        public static bool operator <=(BigInteger left, BigInteger right)
        {
            return Compare(left, right) <= 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a System.Numerics.BigInteger value is less than or equal to a 64-bit signed integer.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is less than or equal to right; otherwise, false.
        /// </returns>
        public static bool operator <=(BigInteger left, long right)
        {
            return left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a 64-bit signed integer is less than or equal to a System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is less than or equal to right; otherwise, false.
        /// </returns>
        public static bool operator <=(long left, BigInteger right)
        {
            return right.CompareTo(left) >= 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a System.Numerics.BigInteger value is less than or equal to a 64-bit unsigned integer.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is less than or equal to right; otherwise, false.
        /// </returns>
        public static bool operator <=(BigInteger left, ulong right)
        {
            return left.CompareTo(right) <= 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a 64-bit unsigned integer is less than or equal to a System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is less than or equal to right; otherwise, false.
        /// </returns>
        public static bool operator <=(ulong left, BigInteger right)
        {
            return right.CompareTo(left) >= 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a System.Numerics.BigInteger value is greater than another System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is greater than right; otherwise, false.
        /// </returns>
        public static bool operator >(BigInteger left, BigInteger right)
        {
            return Compare(left, right) > 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a System.Numerics.BigInteger is greater than a 64-bit signed integer value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is greater than right; otherwise, false.
        /// </returns>
        public static bool operator >(BigInteger left, long right)
        {
            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a 64-bit signed integer is greater than a System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is greater than right; otherwise, false.
        /// </returns>
        public static bool operator >(long left, BigInteger right)
        {
            return right.CompareTo(left) < 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a System.Numerics.BigInteger value is greater than a 64-bit unsigned integer.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is greater than right; otherwise, false.
        /// </returns>
        public static bool operator >(BigInteger left, ulong right)
        {
            return left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a System.Numerics.BigInteger value is greater than a 64-bit unsigned integer.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is greater than right; otherwise, false.
        /// </returns>
        public static bool operator >(ulong left, BigInteger right)
        {
            return right.CompareTo(left) < 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a System.Numerics.BigInteger value is greater than or equal to another System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is greater than or equal  right; otherwise, false.
        /// </returns>
        public static bool operator >=(BigInteger left, BigInteger right)
        {
            return Compare(left, right) >= 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a System.Numerics.BigInteger value is greater than or equal to a 64-bit signed integer value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is greater than or equal right; otherwise, false.
        /// </returns>
        public static bool operator >=(BigInteger left, long right)
        {
            return left.CompareTo(right) >= 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a 64-bit signed integer is greater than or equal to a System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is greater than or equal right; otherwise, false.
        /// </returns>
        public static bool operator >=(long left, BigInteger right)
        {
            return right.CompareTo(left) <= 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a System.Numerics.BigInteger value is greater than or equal to a 64-bit unsigned integer value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is greater than or equal right; otherwise, false.
        /// </returns>
        public static bool operator >=(BigInteger left, ulong right)
        {
            return left.CompareTo(right) >= 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a 64-bit unsigned integer is greater than or equal to a System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left is greater than or equal right; otherwise, false.
        /// </returns>
        public static bool operator >=(ulong left, BigInteger right)
        {
            return right.CompareTo(left) <= 0;
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two System.Numerics.BigInteger objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if the left and right parameters have the same value; otherwise, false.
        /// </returns>
        public static bool operator ==(BigInteger left, BigInteger right)
        {
            return Compare(left, right) == 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a System.Numerics.BigInteger value and a signed long integer value are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if the left and right parameters have the same value; otherwise, false.
        /// </returns>
        public static bool operator ==(BigInteger left, long right)
        {
            return left.CompareTo(right) == 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a signed long integer value and a System.Numerics.BigInteger value are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if the left and right parameters have the same value; otherwise, false.
        /// </returns>
        public static bool operator ==(long left, BigInteger right)
        {
            return right.CompareTo(left) == 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a System.Numerics.BigInteger value and an unsigned long integer value are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if the left and right parameters have the same value; otherwise, false.
        /// </returns>
        public static bool operator ==(BigInteger left, ulong right)
        {
            return left.CompareTo(right) == 0;
        }

        /// <summary>
        /// Returns a value that indicates whether an unsigned long integer value and a System.Numerics.BigInteger value are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if the left and right parameters have the same value; otherwise, false.
        /// </returns>
        public static bool operator ==(ulong left, BigInteger right)
        {
            return right.CompareTo(left) == 0;
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="BigInteger"/> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left and right are not equal; otherwise, false.
        /// </returns>
        public static bool operator !=(BigInteger left, BigInteger right)
        {
            return Compare(left, right) != 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a <see cref="BigInteger"/> value and a 64-bit signed integer are not equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left and right are not equal; otherwise, false.
        /// </returns>
        public static bool operator !=(BigInteger left, long right)
        {
            return left.CompareTo(right) != 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a 64-bit signed integer and a <see cref="BigInteger"/> value are not equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left and right are not equal; otherwise, false.
        /// </returns>
        public static bool operator !=(long left, BigInteger right)
        {
            return right.CompareTo(left) != 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a <see cref="BigInteger"/> value and a 64-bit unsigned integer are not equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left and right are not equal; otherwise, false.
        /// </returns>
        public static bool operator !=(BigInteger left, ulong right)
        {
            return left.CompareTo(right) != 0;
        }

        /// <summary>
        /// Returns a value that indicates whether a 64-bit unsigned integer and a <see cref="BigInteger"/> value are not equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>
        /// true if left and right are not equal; otherwise, false.
        /// </returns>
        public static bool operator !=(ulong left, BigInteger right)
        {
            return right.CompareTo(left) != 0;
        }

        #endregion

        /// <summary>
        /// Indicates whether the value of the current System.Numerics.BigInteger object is an even number.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the value of the System.Numerics.BigInteger object is an even number; otherwise, <c>false</c>.
        /// </value>
        public bool IsEven
        {
            get { return (_data[0] & 0x1) == 0; }
        }

        /// <summary>
        /// Indicates whether the value of the current System.Numerics.BigInteger object is System.Numerics.BigInteger.One.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the value of the System.Numerics.BigInteger object is System.Numerics.BigInteger.One; otherwise, <c>false</c>.
        /// </value>
        public bool IsOne
        {
            get { return _sign == 1 && _data.Length == 1 && _data[0] == 1; }
        }

        /// <summary>
        /// Indicates whether the value of the current System.Numerics.BigInteger object is a power of two.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the value of the System.Numerics.BigInteger object is a power of two; otherwise, <c>false</c>.
        /// </value>
        public bool IsPowerOfTwo
        {
            get
            {
                bool foundBit = false;
                if (_sign != 1)
                    return false;
                //This function is pop count == 1 for positive numbers
                for (var i = 0; i < _data.Length; ++i)
                {
                    var p = PopulationCount(_data[i]);
                    if (p > 0)
                    {
                        if (p > 1 || foundBit)
                            return false;
                        foundBit = true;
                    }
                }
                return foundBit;
            }
        }

        /// <summary>
        /// Indicates whether the value of the current System.Numerics.BigInteger object is System.Numerics.BigInteger.Zero.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the value of the System.Numerics.BigInteger object is System.Numerics.BigInteger.Zero; otherwise, <c>false</c>.
        /// </value>
        public bool IsZero
        {
            get { return _sign == 0; }
        }

        /// <summary>
        /// Gets a value that represents the number negative one (-1).
        /// </summary>
        public static BigInteger MinusOne
        {
            get { return new BigInteger(-1, _one); }
        }

        /// <summary>
        /// Gets a value that represents the number one (1).
        /// </summary>
        public static BigInteger One
        {
            get { return new BigInteger(1, _one); }
        }

        /// <summary>
        /// Gets a number that indicates the sign (negative, positive, or zero) of the current System.Numerics.BigInteger object.
        /// </summary>
        public int Sign
        {
            get { return _sign; }
        }

        /// <summary>
        /// Gets a value that represents the number 0 (zero).
        /// </summary>
        public static BigInteger Zero
        {
            get { return new BigInteger(0, _zero); }
        }

        /// <summary>
        /// Gets the absolute value of a System.Numerics.BigInteger object.
        /// </summary>
        /// <param name="value">A number.</param>
        /// <returns>The absolute value of value.</returns>
        public static BigInteger Abs(BigInteger value)
        {
            return new BigInteger(Math.Abs(value._sign), value._data);
        }

        /// <summary>
        /// Adds two System.Numerics.BigInteger values and returns the result.
        /// </summary>
        /// <param name="left">The first value to add.</param>
        /// <param name="right">The second value to add.</param>
        /// <returns>The sum of left and right.</returns>
        public static BigInteger Add(BigInteger left, BigInteger right)
        {
            return left + right;
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="obj"/>. Zero This instance is equal to <paramref name="obj"/>. Greater than zero This instance is greater than <paramref name="obj"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="obj"/> is not the same type as this instance. </exception>
        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            if (!(obj is BigInteger))
                return -1;

            return Compare(this, (BigInteger)obj);
        }

        /// <summary>
        /// Compares this instance to a second System.Numerics.BigInteger and returns 
        /// an integer that indicates whether the value of this instance is less than, 
        /// equal to, or greater than the value of the specified object.
        /// </summary>
        /// <param name="other">The object to compare.</param>
        /// <returns>
        /// A signed integer value that indicates the relationship of this instance to 
        /// other, as shown in the following table.Return valueDescriptionLess than zeroThe 
        /// current instance is less than other.ZeroThe current instance equals other.Greater 
        /// than zeroThe current instance is greater than other.
        /// </returns>
        public int CompareTo(BigInteger other)
        {
            return Compare(this, other);
        }

        /// <summary>
        /// Compares this instance to an unsigned 64-bit integer and returns an integer 
        /// that indicates whether the value of this instance is less than, equal to, 
        /// or greater than the value of the unsigned 64-bit integer.
        /// </summary>
        /// <param name="other">The unsigned 64-bit integer to compare.</param>
        /// <returns>A signed integer that indicates the relative value of this instance and other, 
        /// as shown in the following table.Return valueDescriptionLess than zeroThe 
        /// current instance is less than other.ZeroThe current instance equals other.Greater
        /// than zeroThe current instance is greater than other.</returns>
        public int CompareTo(ulong other)
        {
            if (_sign < 0)
                return -1;
            if (_sign == 0)
                return other == 0 ? 0 : -1;

            if (_data.Length > 2)
                return 1;

            var high = (uint)(other >> 32);
            var low = (uint)other;

            return LongCompare(low, high);
        }

        /// <summary>
        /// Generates random BigInteger number
        /// </summary>
        /// <param name="bitLength">Length of random number in bits.</param>
        /// <returns>Big random number.</returns>
        public static BigInteger Random(int bitLength)
        {
            var bytesArray = new byte[bitLength / 8 + (((bitLength % 8) > 0) ? 1 : 0)];
            HashAlgorithmFactory.GenerateRandom(bytesArray);
            bytesArray[bytesArray.Length - 1] = (byte)(bytesArray[bytesArray.Length - 1] & 0x7F);   //  Ensure not a negative value
#if TUNING
            return new BigInteger(bytesArray);
#else
            return new BigInteger(bytesArray.ToArray());
#endif
        }

        /// <summary>
        /// Divides one System.Numerics.BigInteger value by another and returns the result.
        /// </summary>
        /// <param name="dividend">The value to be divided.</param>
        /// <param name="divisor">The value to divide by.</param>
        /// <returns>The quotient of the division.</returns>
        public static BigInteger Divide(BigInteger dividend, BigInteger divisor)
        {
            return dividend / divisor;
        }

        /// <summary>
        /// Divides one System.Numerics.BigInteger value by another, returns the result, and returns the remainder in an output parameter.
        /// </summary>
        /// <param name="dividend">The value to be divided.</param>
        /// <param name="divisor">The value to divide by.</param>
        /// <param name="remainder">When this method returns, contains a System.Numerics.BigInteger value that 
        /// represents the remainder from the division. This parameter is passed uninitialized.</param>
        /// <returns>The quotient of the division.</returns>
        public static BigInteger DivRem(BigInteger dividend, BigInteger divisor, out BigInteger remainder)
        {
            if (divisor._sign == 0)
                throw new DivideByZeroException();

            if (dividend._sign == 0)
            {
                remainder = dividend;
                return dividend;
            }

            uint[] quotient;
            uint[] remainderValue;

            DivModUnsigned(dividend._data, divisor._data, out quotient, out remainderValue);

            int i;
            for (i = remainderValue.Length - 1; i >= 0 && remainderValue[i] == 0; --i) ;
            if (i == -1)
            {
                remainder = new BigInteger(0, _zero);
            }
            else
            {
                if (i < remainderValue.Length - 1)
                    remainderValue = Resize(remainderValue, i + 1);
                remainder = new BigInteger(dividend._sign, remainderValue);
            }

            for (i = quotient.Length - 1; i >= 0 && quotient[i] == 0; --i) ;
            if (i == -1)
                return new BigInteger(0, _zero);
            if (i < quotient.Length - 1)
                quotient = Resize(quotient, i + 1);

            return new BigInteger((short)(dividend._sign * divisor._sign), quotient);
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance and a specified System.Numerics.BigInteger object have the same value.
        /// </summary>
        /// <param name="other">The object to compare.</param>
        /// <returns>
        /// true if this System.Numerics.BigInteger object and other have the same value; otherwise, false.
        /// </returns>
        public bool Equals(BigInteger other)
        {
            if (_sign != other._sign)
                return false;
            if (_data.Length != other._data.Length)
                return false;
            for (var i = 0; i < _data.Length; ++i)
            {
                if (_data[i] != other._data[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance and a signed 64-bit integer have the same value.
        /// </summary>
        /// <param name="other">The signed 64-bit integer value to compare.</param>
        /// <returns>true if the signed 64-bit integer and the current instance have the same value; otherwise, false.</returns>
        public bool Equals(long other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance and a specified object have the same value.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>
        ///   <c>true</c> if the obj parameter is a System.Numerics.BigInteger object or a type
        ///   capable of implicit conversion to a System.Numerics.BigInteger value, and
        ///   its value is equal to the value of the current System.Numerics.BigInteger
        ///   object; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is BigInteger))
                return false;
            return Equals((BigInteger)obj);
        }

        /// <summary>
        /// Returns a value that indicates whether the current instance and an unsigned 64-bit integer have the same value.
        /// </summary>
        /// <param name="other">The unsigned 64-bit integer to compare.</param>
        /// <returns>true if the current instance and the unsigned 64-bit integer have the same value; otherwise, false.</returns>
        public bool Equals(ulong other)
        {
            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Returns the hash code for the current System.Numerics.BigInteger object.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer hash code.
        /// </returns>
        public override int GetHashCode()
        {
            var hash = (uint)(_sign * 0x01010101u);

            for (var i = 0; i < _data.Length; ++i)
                hash ^= _data[i];
            return (int)hash;
        }

        /// <summary>
        /// Finds the greatest common divisor of two System.Numerics.BigInteger values.
        /// </summary>
        /// <param name="left">The first value.</param>
        /// <param name="right">The second value.</param>
        /// <returns>The greatest common divisor of left and right.</returns>
        public static BigInteger GreatestCommonDivisor(BigInteger left, BigInteger right)
        {
            if (left._data.Length == 1 && left._data[0] == 1)
                return new BigInteger(1, _one);
            if (right._data.Length == 1 && right._data[0] == 1)
                return new BigInteger(1, _one);
            if (left.IsZero)
                return right;
            if (right.IsZero)
                return left;

            var x = new BigInteger(1, left._data);
            var y = new BigInteger(1, right._data);

            var g = y;

            while (x._data.Length > 1)
            {
                g = x;
                x = y % x;
                y = g;

            }
            if (x.IsZero) return g;

            // TODO: should we have something here if we can convert to long?

            //
            // Now we can just do it with single precision. I am using the binary gcd method,
            // as it should be faster.
            //

            var yy = x._data[0];
            var xx = (uint)(y % yy);

            var t = 0;

            while (((xx | yy) & 1) == 0)
            {
                xx >>= 1; yy >>= 1; t++;
            }
            while (xx != 0)
            {
                while ((xx & 1) == 0) xx >>= 1;
                while ((yy & 1) == 0) yy >>= 1;
                if (xx >= yy)
                    xx = (xx - yy) >> 1;
                else
                    yy = (yy - xx) >> 1;
            }

            return yy << t;
        }

        /// <summary>
        /// Returns the logarithm of a specified number in a specified base.
        /// </summary>
        /// <param name="value">A number whose logarithm is to be found.</param>
        /// <param name="baseValue">The base of the logarithm.</param>
        /// <returns>The base baseValue logarithm of value, as shown in the table in the Remarks section.</returns>
        public static double Log(BigInteger value, double baseValue)
        {
            //  LAMESPEC Log doesn't specify to how many ulp is has to be precise 
            //  We are equilavent to MS with about 2 ULP

            if (value._sign == -1 || baseValue == 1.0d || baseValue == -1.0d ||
            baseValue == double.NegativeInfinity || double.IsNaN(baseValue))
                return double.NaN;

            if (baseValue == 0.0d || baseValue == double.PositiveInfinity)
                return value.IsOne ? 0 : double.NaN;

            if (value._sign == 0)
                return double.NegativeInfinity;

            var length = value._data.Length - 1;
            var bitCount = -1;
            for (var curBit = 31; curBit >= 0; curBit--)
            {
                if ((value._data[length] & (1 << curBit)) != 0)
                {
                    bitCount = curBit + length * 32;
                    break;
                }
            }

            long bitlen = bitCount;
            double c = 0, d = 1;

            var testBit = One;
            var tempBitlen = bitlen;
            while (tempBitlen > int.MaxValue)
            {
                testBit = testBit << int.MaxValue;
                tempBitlen -= int.MaxValue;
            }
            testBit = testBit << (int)tempBitlen;

            for (var curbit = bitlen; curbit >= 0; --curbit)
            {
                if ((value & testBit)._sign != 0)
                    c += d;
                d *= 0.5;
                testBit = testBit >> 1;
            }
            return (Math.Log(c) + Math.Log(2) * bitlen) / Math.Log(baseValue);
        }

        /// <summary>
        /// Returns the natural (base e) logarithm of a specified number.
        /// </summary>
        /// <param name="value">The number whose logarithm is to be found.</param>
        /// <returns>The natural (base e) logarithm of value, as shown in the table in the Remarks section.</returns>
        public static double Log(BigInteger value)
        {
            return Log(value, Math.E);
        }

        /// <summary>
        /// Returns the base 10 logarithm of a specified number.
        /// </summary>
        /// <param name="value">A number whose logarithm is to be found.</param>
        /// <returns>The base 10 logarithm of value, as shown in the table in the Remarks section.</returns>
        public static double Log10(BigInteger value)
        {
            return Log(value, 10);
        }

        /// <summary>
        /// Returns the larger of two System.Numerics.BigInteger values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>The left or right parameter, whichever is larger.</returns>
        public static BigInteger Max(BigInteger left, BigInteger right)
        {
            int ls = left._sign;
            int rs = right._sign;

            if (ls > rs)
                return left;
            if (rs > ls)
                return right;

            var r = CoreCompare(left._data, right._data);
            if (ls == -1)
                r = -r;

            if (r >= 0)
                return left;
            return right;
        }

        /// <summary>
        /// Returns the smaller of two System.Numerics.BigInteger values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>The left or right parameter, whichever is smaller.</returns>
        public static BigInteger Min(BigInteger left, BigInteger right)
        {
            int ls = left._sign;
            int rs = right._sign;

            if (ls < rs)
                return left;
            if (rs < ls)
                return right;

            var r = CoreCompare(left._data, right._data);
            if (ls == -1)
                r = -r;

            if (r <= 0)
                return left;
            return right;
        }

        /// <summary>
        /// Performs modulus division on a number raised to the power of another number.
        /// </summary>
        /// <param name="value">The number to raise to the exponent power.</param>
        /// <param name="exponent">The exponent to raise value by.</param>
        /// <param name="modulus">The value to divide valueexponent by.</param>
        /// <returns>The remainder after dividing valueexponent by modulus.</returns>
        public static BigInteger ModPow(BigInteger value, BigInteger exponent, BigInteger modulus)
        {
            if (exponent._sign == -1)
                throw new ArgumentOutOfRangeException("exponent", "power must be >= 0");
            if (modulus._sign == 0)
                throw new DivideByZeroException();

            var result = One % modulus;
            while (exponent._sign != 0)
            {
                if (!exponent.IsEven)
                {
                    result = result * value;
                    result = result % modulus;
                }

                if (exponent.IsOne)
                    break;

                value = value * value;
                value = value % modulus;
                exponent >>= 1;
            }
            return result;
        }

        /// <summary>
        /// Mods the inverse.
        /// </summary>
        /// <param name="bi">The bi.</param>
        /// <param name="modulus">The modulus.</param>
        /// <returns>Modulus inverted number.</returns>
        public static BigInteger ModInverse(BigInteger bi, BigInteger modulus)
        {
            BigInteger a = modulus, b = bi % modulus;
            BigInteger p0 = 0, p1 = 1;

            while (!b.IsZero)
            {
                if (b.IsOne)
                    return p1;

                p0 += (a / b) * p1;
                a %= b;

                if (a.IsZero)
                    break;

                if (a.IsOne)
                    return modulus - p0;

                p1 += (b / a) * p0;
                b %= a;

            }
            return 0;
        }

        /// <summary>
        /// Returns positive remainder that results from division with two specified <see cref="BigInteger"/> values.
        /// </summary>
        /// <param name="dividend">The value to be divided.</param>
        /// <param name="divisor">The value to divide by.</param>
        /// <returns>
        /// Positive remainder that results from the division.
        /// </returns>
        public static BigInteger PositiveMod(BigInteger dividend, BigInteger divisor)
        {
            var result = dividend % divisor;
            if (result < 0)
                result += divisor;

            return result;
        }

        /// <summary>
        /// Returns the product of two System.Numerics.BigInteger values.
        /// </summary>
        /// <param name="left">The first number to multiply.</param>
        /// <param name="right">The second number to multiply.</param>
        /// <returns>The product of the left and right parameters.</returns>
        public static BigInteger Multiply(BigInteger left, BigInteger right)
        {
            return left * right;
        }

        /// <summary>
        /// Negates a specified System.Numerics.BigInteger value.
        /// </summary>
        /// <param name="value">The value to negate.</param>
        /// <returns>The result of the value parameter multiplied by negative one (-1).</returns>
        public static BigInteger Negate(BigInteger value)
        {
            return -value;
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style and culture-specific format to its <see cref="BigInteger"/> equivalent.
        /// </summary>
        /// <param name="value">A string that contains a number to convert.</param>
        /// <param name="style">A bitwise combination of the enumeration values that specify the permitted format of value.</param>
        /// <param name="provider">An object that provides culture-specific formatting information about value.</param>
        /// <returns>Parsed <see cref="BigInteger"/> number</returns>
        public static BigInteger Parse(string value, NumberStyles style, IFormatProvider provider)
        {
            Exception ex;
            BigInteger result;

            if (!Parse(value, false, style, provider, out result, out ex))
                throw ex;
            return result;
        }

        /// <summary>
        /// Converts the string representation of a number in a specified culture-specific format to its System.Numerics.BigInteger equivalent.
        /// </summary>
        /// <param name="value">A string that contains a number to convert.</param>
        /// <param name="provider">An object that provides culture-specific formatting information about value.</param>
        /// <returns>A value that is equivalent to the number specified in the value parameter.</returns>
        public static BigInteger Parse(string value, IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the string representation of a number in a specified style to its System.Numerics.BigInteger equivalent.
        /// </summary>
        /// <param name="value">A string that contains a number to convert.</param>
        /// <param name="style">A bitwise combination of the enumeration values that specify the permitted format of value.</param>
        /// <returns>A value that is equivalent to the number specified in the value parameter.</returns>
        public static BigInteger Parse(string value, NumberStyles style)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Raises a System.Numerics.BigInteger value to the power of a specified value.
        /// </summary>
        /// <param name="value">The number to raise to the exponent power.</param>
        /// <param name="exponent">The exponent to raise value by.</param>
        /// <returns>The result of raising value to the exponent power.</returns>
        public static BigInteger Pow(BigInteger value, int exponent)
        {
            if (exponent < 0)
                throw new ArgumentOutOfRangeException("exponent", "exp must be >= 0");
            if (exponent == 0)
                return One;
            if (exponent == 1)
                return value;

            var result = One;
            while (exponent != 0)
            {
                if ((exponent & 1) != 0)
                    result = result * value;
                if (exponent == 1)
                    break;

                value = value * value;
                exponent >>= 1;
            }
            return result;
        }

        /// <summary>
        /// Performs integer division on two System.Numerics.BigInteger values and returns the remainder.
        /// </summary>
        /// <param name="dividend">The value to be divided.</param>
        /// <param name="divisor">The value to divide by.</param>
        /// <returns>The remainder after dividing dividend by divisor.</returns>
        public static BigInteger Remainder(BigInteger dividend, BigInteger divisor)
        {
            return dividend % divisor;
        }

        /// <summary>
        /// Subtracts one System.Numerics.BigInteger value from another and returns the result.
        /// </summary>
        /// <param name="left">The value to subtract from (the minuend).</param>
        /// <param name="right">The value to subtract (the subtrahend).</param>
        /// <returns>The result of subtracting right from left.</returns>
        public static BigInteger Subtract(BigInteger left, BigInteger right)
        {
            return left - right;
        }

        /// <summary>
        /// Converts a System.Numerics.BigInteger value to a byte array.
        /// </summary>
        /// <returns>The value of the current System.Numerics.BigInteger object converted to an array of bytes.</returns>
        public byte[] ToByteArray()
        {
            if (_sign == 0)
                return new byte[1];

            //number of bytes not counting upper word
            var bytes = (_data.Length - 1) * 4;
            var needExtraZero = false;

            var topWord = _data[_data.Length - 1];
            int extra;

            //if the topmost bit is set we need an extra
            if (_sign == 1)
            {
                extra = TopByte(topWord);
                var mask = 0x80u << ((extra - 1) * 8);
                if ((topWord & mask) != 0)
                {
                    needExtraZero = true;
                }
            }
            else
            {
                extra = TopByte(topWord);
            }

            var res = new byte[bytes + extra + (needExtraZero ? 1 : 0)];
            if (_sign == 1)
            {
                var j = 0;
                var end = _data.Length - 1;
                for (var i = 0; i < end; ++i)
                {
                    var word = _data[i];

                    res[j++] = (byte)word;
                    res[j++] = (byte)(word >> 8);
                    res[j++] = (byte)(word >> 16);
                    res[j++] = (byte)(word >> 24);
                }
                while (extra-- > 0)
                {
                    res[j++] = (byte)topWord;
                    topWord >>= 8;
                }
            }
            else
            {
                var j = 0;
                var end = _data.Length - 1;

                uint carry = 1, word;
                ulong add;
                for (var i = 0; i < end; ++i)
                {
                    word = _data[i];
                    add = (ulong)~word + carry;
                    word = (uint)add;
                    carry = (uint)(add >> 32);

                    res[j++] = (byte)word;
                    res[j++] = (byte)(word >> 8);
                    res[j++] = (byte)(word >> 16);
                    res[j++] = (byte)(word >> 24);
                }

                add = (ulong)~topWord + (carry);
                word = (uint)add;
                carry = (uint)(add >> 32);
                if (carry == 0)
                {
                    int ex = FirstNonFFByte(word);
                    bool needExtra = (word & (1 << (ex * 8 - 1))) == 0;
                    var to = ex + (needExtra ? 1 : 0);

                    if (to != extra)
                        res = Resize(res, bytes + to);

                    while (ex-- > 0)
                    {
                        res[j++] = (byte)word;
                        word >>= 8;
                    }
                    if (needExtra)
                        res[j++] = 0xFF;
                }
                else
                {
                    res = Resize(res, bytes + 5);
                    res[j++] = (byte)word;
                    res[j++] = (byte)(word >> 8);
                    res[j++] = (byte)(word >> 16);
                    res[j++] = (byte)(word >> 24);
                    res[j++] = 0xFF;
                }
            }

            return res;
        }

        /// <summary>
        /// Converts the numeric value of the current System.Numerics.BigInteger object to its equivalent string representation.
        /// </summary>
        /// <returns>
        /// The string representation of the current System.Numerics.BigInteger value.
        /// </returns>
        public override string ToString()
        {
            return ToString(10, null);
        }

        /// <summary>
        /// Converts the numeric value of the current System.Numerics.BigInteger object 
        /// to its equivalent string representation by using the specified culture-specific 
        /// formatting information.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// The string representation of the current System.Numerics.BigInteger value 
        /// in the format specified by the provider parameter.
        /// </returns>
        public string ToString(IFormatProvider provider)
        {
            return ToString(null, provider);
        }

        /// <summary>
        /// Converts the numeric value of the current System.Numerics.BigInteger object
        /// to its equivalent string representation by using the specified format.
        /// </summary>
        /// <param name="format">A standard or custom numeric format string.</param>
        /// <returns>
        /// The string representation of the current System.Numerics.BigInteger value
        /// in the format specified by the format parameter.
        /// </returns>
        public string ToString(string format)
        {
            return ToString(format, null);
        }

        /// <summary>
        /// Converts the numeric value of the current System.Numerics.BigInteger object
        /// to its equivalent string representation by using the specified format and
        /// culture-specific format information.
        /// </summary>
        /// <param name="format">A standard or custom numeric format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// The string representation of the current System.Numerics.BigInteger value 
        /// as specified by the format and provider parameters.
        /// </returns>
        public string ToString(string format, IFormatProvider provider)
        {
            if (string.IsNullOrEmpty(format))
                return ToString(10, provider);

            switch (format[0])
            {
                case 'd':
                case 'D':
                case 'g':
                case 'G':
                case 'r':
                case 'R':
                    return ToStringWithPadding(format, 10, provider);
                case 'x':
                case 'X':
                    return ToStringWithPadding(format, 16, null);
                default:
                    throw new FormatException(string.Format("format '{0}' not implemented", format));
            }
        }

        /// <summary>
        /// Tries to convert the string representation of a number in a specified style
        /// and culture-specific format to its System.Numerics.BigInteger equivalent,
        /// and returns a value that indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="value">The string representation of a number. The string is interpreted using the style specified by style.</param>
        /// <param name="style">A bitwise combination of enumeration values that indicates the style elements
        /// that can be present in value. A typical value to specify is System.Globalization.NumberStyles.Integer.</param>
        /// <param name="cultureInfo">An object that supplies culture-specific formatting information about value.</param>
        /// <param name="result">When this method returns, contains the System.Numerics.BigInteger equivalent
        /// to the number that is contained in value, or System.Numerics.BigInteger.Zero
        /// if the conversion failed. The conversion fails if the value parameter is
        /// null or is not in a format that is compliant with style. This parameter is
        /// passed uninitialized.</param>
        /// <returns>true if the value parameter was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string value, NumberStyles style, CultureInfo cultureInfo, out BigInteger result)
        {
            Exception ex;
            return Parse(value, true, style, cultureInfo, out result, out ex);
        }

        /// <summary>
        /// Tries to convert the string representation of a number to its System.Numerics.BigInteger
        /// equivalent, and returns a value that indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="value">The string representation of a number.</param>
        /// <param name="result">When this method returns, contains the System.Numerics.BigInteger equivalent
        /// to the number that is contained in value, or zero (0) if the conversion fails.
        /// The conversion fails if the value parameter is null or is not of the correct
        /// format. This parameter is passed uninitialized.</param>
        /// <returns>true if value was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string value, out BigInteger result)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Compares this instance to a signed 64-bit integer and returns an integer 
        /// that indicates whether the value of this instance is less than, equal to, 
        /// or greater than the value of the signed 64-bit integer.
        /// </summary>
        /// <param name="other">The signed 64-bit integer to compare.</param>
        /// <returns>A signed integer value that indicates the relationship of this instance to 
        /// other, as shown in the following table.Return valueDescriptionLess than zeroThe 
        /// current instance is less than other.ZeroThe current instance equals other.Greater 
        /// than zero.The current instance is greater than other.</returns>
        public int CompareTo(long other)
        {
            var ls = _sign;
            var rs = Math.Sign(other);

            if (ls != rs)
                return ls > rs ? 1 : -1;

            if (ls == 0)
                return 0;

            if (_data.Length > 2)
                return _sign;

            if (other < 0)
                other = -other;
            var low = (uint)other;
            var high = (uint)((ulong)other >> 32);

            var r = LongCompare(low, high);
            if (ls == -1)
                r = -r;

            return r;
        }

        /// <summary>
        /// Compares two System.Numerics.BigInteger values and returns an integer that 
        /// indicates whether the first value is less than, equal to, or greater than the second value.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>A signed integer that indicates the relative values of left and right, 
        /// as shown in the following table.ValueConditionLess than zeroleft is less than right.Zeroleft 
        /// equals right.Greater than zeroleft is greater than right.</returns>
        public static int Compare(BigInteger left, BigInteger right)
        {
            var ls = left._sign;
            var rs = right._sign;

            if (ls != rs)
                return ls > rs ? 1 : -1;

            var r = CoreCompare(left._data, right._data);
            if (ls < 0)
                r = -r;
            return r;
        }

        private static bool Negative(byte[] v)
        {
            return ((v[7] & 0x80) != 0);
        }

        private static ushort Exponent(byte[] v)
        {
            return (ushort)((((ushort)(v[7] & 0x7F)) << (ushort)4) | (((ushort)(v[6] & 0xF0)) >> 4));
        }

        private static ulong Mantissa(byte[] v)
        {
            var i1 = ((uint)v[0] | ((uint)v[1] << 8) | ((uint)v[2] << 16) | ((uint)v[3] << 24));
            var i2 = ((uint)v[4] | ((uint)v[5] << 8) | ((uint)(v[6] & 0xF) << 16));

            return (ulong)i1 | ((ulong)i2 << 32);
        }

        /// <summary>
        /// Populations the count.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns>Returns the number of bits set in x</returns>
        private static int PopulationCount(uint x)
        {
            x = x - ((x >> 1) & 0x55555555);
            x = (x & 0x33333333) + ((x >> 2) & 0x33333333);
            x = (x + (x >> 4)) & 0x0F0F0F0F;
            x = x + (x >> 8);
            x = x + (x >> 16);
            return (int)(x & 0x0000003F);
        }

        private string ToStringWithPadding(string format, uint radix, IFormatProvider provider)
        {
            if (format.Length > 1)
            {
                var precision = Convert.ToInt32(format.Substring(1), CultureInfo.InvariantCulture.NumberFormat);
                var baseStr = ToString(radix, provider);
                if (baseStr.Length < precision)
                {
                    var additional = new string('0', precision - baseStr.Length);
                    if (baseStr[0] != '-')
                    {
                        return additional + baseStr;
                    }
                    return "-" + additional + baseStr.Substring(1);
                }
                return baseStr;
            }
            return ToString(radix, provider);
        }

        private static uint[] MakeTwoComplement(uint[] v)
        {
            var res = new uint[v.Length];

            ulong carry = 1;
            for (var i = 0; i < v.Length; ++i)
            {
                var word = v[i];
                carry = (ulong)~word + carry;
                word = (uint)carry;
                carry = (uint)(carry >> 32);
                res[i] = word;
            }

            var last = res[res.Length - 1];
            var idx = FirstNonFFByte(last);
            uint mask = 0xFF;
            for (var i = 1; i < idx; ++i)
                mask = (mask << 8) | 0xFF;

            res[res.Length - 1] = last & mask;
            return res;
        }

        private string ToString(uint radix, IFormatProvider provider)
        {
            const string characterSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if (characterSet.Length < radix)
                throw new ArgumentException("charSet length less than radix", "characterSet");
            if (radix == 1)
                throw new ArgumentException("There is no such thing as radix one notation", "radix");

            if (_sign == 0)
                return "0";
            if (_data.Length == 1 && _data[0] == 1)
                return _sign == 1 ? "1" : "-1";

            var digits = new List<char>(1 + _data.Length * 3 / 10);

            BigInteger a;
            if (_sign == 1)
                a = this;
            else
            {
                uint[] dt = _data;
                if (radix > 10)
                    dt = MakeTwoComplement(dt);
                a = new BigInteger(1, dt);
            }

            while (a != 0)
            {
                BigInteger rem;
                a = DivRem(a, radix, out rem);
                digits.Add(characterSet[(int)rem]);
            }

            if (_sign == -1 && radix == 10)
            {
                NumberFormatInfo info = null;
                if (provider != null)
                    info = provider.GetFormat(typeof(NumberFormatInfo)) as NumberFormatInfo;
                if (info != null)
                {
                    var str = info.NegativeSign;
                    for (var i = str.Length - 1; i >= 0; --i)
                        digits.Add(str[i]);
                }
                else
                {
                    digits.Add('-');
                }
            }

            var last = digits[digits.Count - 1];
            if (_sign == 1 && radix > 10 && (last < '0' || last > '9'))
                digits.Add('0');

            digits.Reverse();

            return new string(digits.ToArray());
        }

        private static Exception GetFormatException()
        {
            return new FormatException("Input string was not in the correct format");
        }

        private static bool ProcessTrailingWhitespace(bool tryParse, string s, int position, ref Exception exc)
        {
            var len = s.Length;

            for (var i = position; i < len; i++)
            {
                var c = s[i];

                if (c != 0 && !char.IsWhiteSpace(c))
                {
                    if (!tryParse)
                        exc = GetFormatException();
                    return false;
                }
            }
            return true;
        }

        private static bool Parse(string s, bool tryParse, NumberStyles style, IFormatProvider provider, out BigInteger result, out Exception exc)
        {
            int i, sign = 1;
            var digitsSeen = false;

            var baseNumber = 10;
            switch (style)
            {
                case NumberStyles.None:
                    break;
                case NumberStyles.HexNumber:
                case NumberStyles.AllowHexSpecifier:
                    baseNumber = 16;
                    break;
                case NumberStyles.AllowCurrencySymbol:
                case NumberStyles.AllowDecimalPoint:
                case NumberStyles.AllowExponent:
                case NumberStyles.AllowLeadingSign:
                case NumberStyles.AllowLeadingWhite:
                case NumberStyles.AllowParentheses:
                case NumberStyles.AllowThousands:
                case NumberStyles.AllowTrailingSign:
                case NumberStyles.AllowTrailingWhite:
                case NumberStyles.Any:
                case NumberStyles.Currency:
                case NumberStyles.Float:
                case NumberStyles.Integer:
                case NumberStyles.Number:
                default:
                    throw new NotSupportedException(string.Format("Style '{0}' is not supported.", style));
            }

            result = Zero;
            exc = null;

            if (s == null)
            {
                if (!tryParse)
                    exc = new ArgumentNullException("value");
                return false;
            }

            var len = s.Length;

            char c;
            for (i = 0; i < len; i++)
            {
                c = s[i];
                if (!char.IsWhiteSpace(c))
                    break;
            }

            if (i == len)
            {
                if (!tryParse)
                    exc = GetFormatException();
                return false;
            }

            var info = (NumberFormatInfo) provider.GetFormat(typeof (NumberFormatInfo));

            var negative = info.NegativeSign;
            var positive = info.PositiveSign;

            if (string.CompareOrdinal(s, i, positive, 0, positive.Length) == 0)
                i += positive.Length;
            else if (string.CompareOrdinal(s, i, negative, 0, negative.Length) == 0)
            {
                sign = -1;
                i += negative.Length;
            }

            var val = Zero;
            for (; i < len; i++)
            {
                c = s[i];

                if (c == '\0')
                {
                    i = len;
                    continue;
                }

                if (c >= '0' && c <= '9')
                {
                    var d = (byte)(c - '0');

                    val = val * baseNumber + d;

                    digitsSeen = true;
                }
                else if (c >= 'A' && c <= 'F')
                {
                    var d = (byte)(c - 'A' + 10);

                    val = val * baseNumber + d;

                    digitsSeen = true;
                }
                else if (!ProcessTrailingWhitespace(tryParse, s, i, ref exc))
                    return false;
            }

            if (!digitsSeen)
            {
                if (!tryParse)
                    exc = GetFormatException();
                return false;
            }

            if (val._sign == 0)
                result = val;
            else if (sign == -1)
                result = new BigInteger(-1, val._data);
            else
                result = new BigInteger(1, val._data);

            return true;
        }

        private int LongCompare(uint low, uint high)
        {
            uint h = 0;
            if (_data.Length > 1)
                h = _data[1];

            if (h > high)
                return 1;
            if (h < high)
                return -1;

            var l = _data[0];

            if (l > low)
                return 1;
            if (l < low)
                return -1;

            return 0;
        }

        private bool AsInt32(out int val)
        {
            val = 0;
            if (_data.Length > 1) return false;
            var d = _data[0];

            if (_sign == 1)
            {
                if (d > (uint)int.MaxValue)
                    return false;
                val = (int)d;
            }
            else if (_sign == -1)
            {
                if (d > 0x80000000u)
                    return false;
                val = -(int)d;
            }
            return true;
        }

        /// <summary>
        /// Returns the 0-based index of the most significant set bit
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns>0 if no bit is set</returns>
        private static int BitScanBackward(uint word)
        {
            for (var i = 31; i >= 0; --i)
            {
                var mask = 1u << i;
                if ((word & mask) == mask)
                    return i;
            }
            return 0;
        }

        private static int TopByte(uint x)
        {
            if ((x & 0xFFFF0000u) != 0)
            {
                if ((x & 0xFF000000u) != 0)
                    return 4;
                return 3;
            }
            if ((x & 0xFF00u) != 0)
                return 2;
            return 1;
        }

        private static int FirstNonFFByte(uint word)
        {
            if ((word & 0xFF000000u) != 0xFF000000u)
                return 4;
            if ((word & 0xFF0000u) != 0xFF0000u)
                return 3;
            if ((word & 0xFF00u) != 0xFF00u)
                return 2;
            return 1;
        }

        private static byte[] Resize(byte[] v, int len)
        {
            var res = new byte[len];
            Buffer.BlockCopy(v, 0, res, 0, Math.Min(v.Length, len));
            Array.Copy(v, res, Math.Min(v.Length, len));
            return res;
        }

        private static uint[] Resize(uint[] v, int len)
        {
            var res = new uint[len];
            Buffer.BlockCopy(v, 0, res, 0, Math.Min(v.Length, len) * sizeof(uint));
            return res;
        }

        private static uint[] CoreAdd(uint[] a, uint[] b)
        {
            if (a.Length < b.Length)
            {
                var tmp = a;
                a = b;
                b = tmp;
            }

            var bl = a.Length;
            var sl = b.Length;

            var res = new uint[bl];

            ulong sum = 0;

            var i = 0;
            for (; i < sl; i++)
            {
                sum = sum + a[i] + b[i];
                res[i] = (uint)sum;
                sum >>= 32;
            }

            for (; i < bl; i++)
            {
                sum = sum + a[i];
                res[i] = (uint)sum;
                sum >>= 32;
            }

            if (sum != 0)
            {
                res = Resize(res, bl + 1);
                res[i] = (uint)sum;
            }

            return res;
        }

        /*invariant a > b*/
        private static uint[] CoreSub(uint[] a, uint[] b)
        {
            var bl = a.Length;
            var sl = b.Length;

            var res = new uint[bl];

            ulong borrow = 0;
            int i;
            for (i = 0; i < sl; ++i)
            {
                borrow = (ulong)a[i] - b[i] - borrow;

                res[i] = (uint)borrow;
                borrow = (borrow >> 32) & 0x1;
            }

            for (; i < bl; i++)
            {
                borrow = (ulong)a[i] - borrow;
                res[i] = (uint)borrow;
                borrow = (borrow >> 32) & 0x1;
            }

            //remove extra zeroes
            for (i = bl - 1; i >= 0 && res[i] == 0; --i) ;
            if (i < bl - 1)
                res = Resize(res, i + 1);

            return res;
        }

        private static uint[] CoreAdd(uint[] a, uint b)
        {
            var len = a.Length;
            var res = new uint[len];

            ulong sum = b;
            int i;
            for (i = 0; i < len; i++)
            {
                sum = sum + a[i];
                res[i] = (uint)sum;
                sum >>= 32;
            }

            if (sum != 0)
            {
                res = Resize(res, len + 1);
                res[i] = (uint)sum;
            }

            return res;
        }

        private static uint[] CoreSub(uint[] a, uint b)
        {
            var len = a.Length;
            var res = new uint[len];

            ulong borrow = b;
            int i;
            for (i = 0; i < len; i++)
            {
                borrow = (ulong)a[i] - borrow;
                res[i] = (uint)borrow;
                borrow = (borrow >> 32) & 0x1;
            }

            //remove extra zeroes
            for (i = len - 1; i >= 0 && res[i] == 0; --i) ;
            if (i < len - 1)
                res = Resize(res, i + 1);

            return res;
        }

        private static int CoreCompare(uint[] a, uint[] b)
        {
            var al = a.Length;
            var bl = b.Length;

            if (al > bl)
                return 1;
            if (bl > al)
                return -1;

            for (var i = al - 1; i >= 0; --i)
            {
                var ai = a[i];
                var bi = b[i];
                if (ai > bi)
                    return 1;
                if (ai < bi)
                    return -1;
            }
            return 0;
        }

        private static int GetNormalizeShift(uint value)
        {
            var shift = 0;

            if ((value & 0xFFFF0000) == 0) { value <<= 16; shift += 16; }
            if ((value & 0xFF000000) == 0) { value <<= 8; shift += 8; }
            if ((value & 0xF0000000) == 0) { value <<= 4; shift += 4; }
            if ((value & 0xC0000000) == 0) { value <<= 2; shift += 2; }
            if ((value & 0x80000000) == 0) { value <<= 1; shift += 1; }

            return shift;
        }

        private static void Normalize(uint[] u, int l, uint[] un, int shift)
        {
            uint carry = 0;
            int i;
            if (shift > 0)
            {
                var rshift = 32 - shift;
                for (i = 0; i < l; i++)
                {
                    var ui = u[i];
                    un[i] = (ui << shift) | carry;
                    carry = ui >> rshift;
                }
            }
            else
            {
                for (i = 0; i < l; i++)
                {
                    un[i] = u[i];
                }
            }

            while (i < un.Length)
            {
                un[i++] = 0;
            }

            if (carry != 0)
            {
                un[l] = carry;
            }
        }

        private static void Unnormalize(uint[] un, out uint[] r, int shift)
        {
            var length = un.Length;
            r = new uint[length];

            if (shift > 0)
            {
                var lshift = 32 - shift;
                uint carry = 0;
                for (var i = length - 1; i >= 0; i--)
                {
                    var uni = un[i];
                    r[i] = (uni >> shift) | carry;
                    carry = (uni << lshift);
                }
            }
            else
            {
                for (var i = 0; i < length; i++)
                {
                    r[i] = un[i];
                }
            }
        }

        private static void DivModUnsigned(uint[] u, uint[] v, out uint[] q, out uint[] r)
        {
            var m = u.Length;
            var n = v.Length;

            if (n <= 1)
            {
                // Divide by single digit
                //
                ulong rem = 0;
                var v0 = v[0];
                q = new uint[m];
                r = new uint[1];

                for (var j = m - 1; j >= 0; j--)
                {
                    rem *= Base;
                    rem += u[j];

                    var div = rem / v0;
                    rem -= div * v0;
                    q[j] = (uint)div;
                }
                r[0] = (uint)rem;
            }
            else if (m >= n)
            {
                var shift = GetNormalizeShift(v[n - 1]);

                var un = new uint[m + 1];
                var vn = new uint[n];

                Normalize(u, m, un, shift);
                Normalize(v, n, vn, shift);

                q = new uint[m - n + 1];
                r = null;

                // Main division loop
                //
                for (var j = m - n; j >= 0; j--)
                {
                    int i;

                    var rr = Base * un[j + n] + un[j + n - 1];
                    var qq = rr / vn[n - 1];
                    rr -= qq * vn[n - 1];

                    for (; ; )
                    {
                        // Estimate too big ?
                        //
                        if ((qq >= Base) || (qq * vn[n - 2] > (rr * Base + un[j + n - 2])))
                        {
                            qq--;
                            rr += (ulong)vn[n - 1];
                            if (rr < Base)
                                continue;
                        }
                        break;
                    }


                    // Multiply and subtract
                    //
                    long b = 0;
                    long t;
                    for (i = 0; i < n; i++)
                    {
                        ulong p = vn[i] * qq;
                        t = (long)un[i + j] - (long)(uint)p - b;
                        un[i + j] = (uint)t;
                        p >>= 32;
                        t >>= 32;
                        b = (long)p - t;
                    }
                    t = (long)un[j + n] - b;
                    un[j + n] = (uint)t;

                    // Store the calculated value
                    //
                    q[j] = (uint)qq;

                    // Add back vn[0..n] to un[j..j+n]
                    //
                    if (t < 0)
                    {
                        q[j]--;
                        ulong c = 0;
                        for (i = 0; i < n; i++)
                        {
                            c = (ulong)vn[i] + un[j + i] + c;
                            un[j + i] = (uint)c;
                            c >>= 32;
                        }
                        c += (ulong)un[j + n];
                        un[j + n] = (uint)c;
                    }
                }

                Unnormalize(un, out r, shift);
            }
            else
            {
                q = new uint[] { 0 };
                r = u;
            }
        }
    }
}