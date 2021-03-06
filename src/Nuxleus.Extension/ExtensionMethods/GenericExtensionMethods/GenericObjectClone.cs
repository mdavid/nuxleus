//
// GenericObjectClone.cs
//
// Author:
//       M. David Peterson <m.david@3rdandurban.com>
//
// Copyright (c) 2013 M. David Peterson
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;

namespace Nuxleus.Core
{
	public static class GenericObjectClone
	{
		static readonly object lockObject = new object ();

		public static T Clone<T> (this T cloneable) where T : ICloneable
		{
			return (T)cloneable.Clone ();
		}

		public static T LockAndCopy<T> (this T instance) where T : ICopyable<T>
		{
			lock (lockObject) {
				return instance.Copy ();
			}
		}
	}
}

