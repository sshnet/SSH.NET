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

using Microsoft.VisualStudio.TestTools.UnitTesting;

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

using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    /// Represents an arbitrarily large signed integer.
    /// </summary>
    [TestClass]
    public class BigIntegerTest : TestBase
    {
    }
}