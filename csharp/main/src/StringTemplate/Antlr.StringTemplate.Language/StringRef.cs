/*
[The "BSD licence"]
Copyright (c) 2005 Kunle Odutola
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
	using StringTemplate = Antlr.StringTemplate.StringTemplate;
	
	/// <summary>
	/// Represents a chunk of just simple text to spit out; nothing to "evaluate"
	/// </summary>
	public class StringRef : Expr
	{
		private string str;
		
		public StringRef(StringTemplate enclosingTemplate, string str) : base(enclosingTemplate)
		{
			this.str = str;
		}
		
		/// <summary>
		/// Just print out the string; no reference to self because this
		/// is a literal--not sensitive to attribute values.
		/// </summary>
		public override int Write(StringTemplate self, IStringTemplateWriter output)
		{
			if (str != null)
			{
				int n = output.Write(str);
				return n;
			}
			return 0;
		}
		
		public override string ToString()
		{
			if (str != null)
			{
				return str;
			}
			return "";
		}
	}
}