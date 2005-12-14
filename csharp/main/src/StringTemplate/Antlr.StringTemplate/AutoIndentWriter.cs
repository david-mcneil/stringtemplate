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


namespace Antlr.StringTemplate
{
	using System;
	using StackList			= Antlr.StringTemplate.Collections.StackList;
	using TextWriter		= System.IO.TextWriter;
	
	/// <summary>
	/// Essentially a char filter that knows how to auto-indent output
	/// by maintaining a stack of indent levels.  I set a flag upon newline
	/// and then next nonwhitespace char resets flag and spits out indention.
	/// The indent stack is a stack of strings so we can repeat original indent
	/// not just the same number of columns (don't have to worry about tabs vs
	/// spaces then).
	/// 
	/// This is a filter on a Writer.
	/// 
	/// It may be screwed up for '\r' '\n' on PC's.
	/// </summary>
	public class AutoIndentWriter : IStringTemplateWriter
	{
		public const int BUFFER_SIZE = 50;
		internal StackList indents = new StackList();
		protected TextWriter output = null;
		protected bool atStartOfLine = true;
		
		public AutoIndentWriter(TextWriter output)
		{
			this.output = output;
			indents.Push(null); // start with no indent
		}
		
		/// <summary>Push even blank (null) indents as they are like scopes; must
		/// be able to pop them back off stack.
		/// </summary>
		public virtual void  PushIndentation(string indent)
		{
			indents.Push(indent);
		}
		
		public virtual string PopIndentation()
		{
			return (string) indents.Pop();
		}
		
		public virtual int Write(string str)
		{
			//System.out.println("write("+str+"); indents="+indents);
			int n = 0;
			for (int i = 0; i < str.Length; i++)
			{
				char c = str[i];
				if (c == '\n')
				{
					atStartOfLine = true;
				}
				else
				{
					if (atStartOfLine)
					{
						n += Indent();
						atStartOfLine = false;
					}
				}
				n++;
				output.Write(c);
			}
			return n;
		}
		
		public virtual int Indent()
		{
			int n = 0;
			for (int i = 0; i < indents.Count; i++)
			{
				string ind = (string) indents[i];
				if (ind != null)
				{
					n += ind.Length;
					output.Write(ind);
				}
			}
			return n;
		}
	}
}