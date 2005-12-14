/*
[The "BSD licence"]
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
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.*/
using System;
using System.Collections;
namespace antlr.stringtemplate
{
	
	/// <summary>Essentially a char filter that knows how to auto-indent output
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
	public class AutoIndentWriter : StringTemplateWriter
	{
		public const int BUFFER_SIZE = 50;
		protected internal ArrayList indents = new ArrayList();
		protected internal System.IO.TextWriter outWriter = null;
		protected internal bool atStartOfLine = true;
		
		public AutoIndentWriter(System.IO.TextWriter outWriter)
		{
			this.outWriter = outWriter;
			indents.Add(null); // start with no indent
		}
		
		/// <summary>Push even blank (null) indents as they are like scopes; must
		/// be able to pop them back off stack.
		/// </summary>
		public virtual void pushIndentation(String indent)
		{
			indents.Add(indent);
		}
		
		public virtual String popIndentation()
		{
			return (String) SupportClass.StackSupport.Pop(indents);
		}
		
		public virtual int write(String str)
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
						n += indent();
						atStartOfLine = false;
					}
				}
				n++;
				outWriter.Write(c);
			}
			return n;
		}
		
		public virtual int indent()
		{
			int n = 0;
			for (int i = 0; i < indents.Count; i++)
			{
				String ind = (String) indents[i];
				if (ind != null)
				{
					n += ind.Length;
					outWriter.Write(ind);
				}
			}
			return n;
		}
	}
}