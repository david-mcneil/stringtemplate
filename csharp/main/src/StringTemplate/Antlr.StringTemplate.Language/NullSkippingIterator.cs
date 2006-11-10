/*
[The "BSD licence"]
Copyright (c) 2005-2006 Kunle Odutola
Copyright (c) 2003-2005 Terence Parr
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:
1. Redistributions of source code must retain the above copyright
notice, this list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright
notice, this list of conditions and the following disclaimer in the
documentation and/or other materials provided with the distribution.
3. The name of the author may not be used to endorse or promote products
derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/


namespace Antlr.StringTemplate.Language
{
	using System;
	using IEnumerator = System.Collections.IEnumerator;
	using StringBuilder = System.Text.StringBuilder;

	/// <summary>
	/// An IEnumerator adapter that skips all null values.
	/// </summary>
	public sealed class NullSkippingIterator : IEnumerator
	{
		/// <summary>IEnumerator instance being wrapped/adapted</summary>
		private IEnumerator innerEnumerator;
		private bool hasCurrent = false;

		public NullSkippingIterator(IEnumerator enumerator)
		{
			this.innerEnumerator = enumerator;
		}

		public object Current
		{
			get
			{
				if (!hasCurrent)
					throw new InvalidOperationException("Enumeration not started or already finished.");
				return innerEnumerator.Current;
			}
		}

		public bool MoveNext()
		{
			while (innerEnumerator.MoveNext())
			{
				// skip nulls
				if (innerEnumerator.Current == null)
					continue;
				else
					return (hasCurrent = true);
			}
			return (hasCurrent = false);
		}

		/// <summary>
		/// The result of asking for the string of an iterator is the list of elements
		/// and so this is just the cat'd list of both elements.  This is destructive
		/// in that the iterator cursors have moved to the end after printing.
		/// </summary>
		public override string ToString()
		{
			StringBuilder buf = new StringBuilder();
			//this.Reset();
			while (this.MoveNext())
			{
				buf.Append(this.Current);
			}
			return buf.ToString();
		}

		public void Reset()
		{
			hasCurrent = false;
			innerEnumerator.Reset();
		}
	}
}
