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
	using IList = System.Collections.IList;
	using ArrayList = System.Collections.ArrayList;
	using StackList = Antlr.StringTemplate.Collections.StackList;
	using TextWriter = System.IO.TextWriter;

	/// <summary>
	/// Essentially a char filter that knows how to auto-indent output
	/// by maintaining a stack of indent levels.  I set a flag upon newline
	/// and then next nonwhitespace char resets flag and spits out indention.
	/// The indent stack is a stack of strings so we can repeat original indent
	/// not just the same number of columns (don't have to worry about tabs vs
	/// spaces then).
	/// 
	/// Anchors are char positions (tabs won't work) that indicate where all
	/// future wraps should justify to.  The wrap position is actually the
	/// larger of either the last anchor or the indentation level.
	/// 
	/// This is a filter on a Writer.
	/// 
	/// It may be screwed up for '\r' '\n' on PC's.
	/// </summary>
	public class AutoIndentWriter : IStringTemplateWriter
	{
		public static readonly string newline = Environment.NewLine;

		/// <summary>
		/// Stack of indents. Grows from 0..n-1.  <![CDATA[List<string>]]>
		/// </summary>
		internal StackList indents = new StackList();

		/// <summary>
		/// Stack of integer anchors (char positions in line).
		/// </summary>
		protected int[] anchors = new int[10];
		protected int anchors_sp = -1;

		protected TextWriter output = null;
		protected bool atStartOfLine = true;

		/// <summary>
		/// Track char position in the line (later we can think about tabs).
		/// Indexed from 0.  We want to keep <![CDATA[charPosition <= lineWidth]]>.
		/// This is the position we are *about* to write not the position
		/// last written to.
		/// </summary>
		protected int charPosition = 0;
		protected int lineWidth = Constants.TEMPLATE_WRITER_NO_WRAP;

		protected int charPositionOfStartOfExpr = 0;

		public AutoIndentWriter(TextWriter output)
		{
			this.output = output;
			indents.Push(null); // start with no indent
		}

		public int LineWidth
		{
			set { this.lineWidth = value; }
		}

		/// <summary>Push even blank (null) indents as they are like scopes; must
		/// be able to pop them back off stack.
		/// </summary>
		public virtual void PushIndentation(string indent)
		{
			indents.Push(indent);
		}

		public virtual string PopIndentation()
		{
			return (string)indents.Pop();
		}

		public virtual void PushAnchorPoint()
		{
			if ((anchors_sp + 1) >= anchors.Length)
			{
				int[] resized = new int[anchors.Length * 2];
				Array.Copy(anchors, 0, resized, 0, anchors.Length - 1);
				anchors = resized;
			}
			anchors_sp++;
			anchors[anchors_sp] = charPosition;
		}

		public virtual void PopAnchorPoint()
		{
			anchors_sp--;
		}

		public virtual int GetIndentationWidth()
		{
			int n = 0;
			for (int i = 0; i < indents.Count; i++)
			{
				string ind = (string)indents[i];
				if (ind != null)
				{
					n += ind.Length;
				}
			}
			return n;
		}

		/// <summary>
		/// Write out a string literal or attribute expression or expression element.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public virtual int Write(string str)
		{
			int n = 0;
			for (int i = 0; i < str.Length; i++)
			{
				char c = str[i];
				if (c == '\n')
				{
					atStartOfLine = true;
					charPosition = -1; // set so the write below sets to 0
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
				charPosition++;
			}
			return n;
		}

		public virtual int WriteSeparator(string str)
		{
			return Write(str);
		}

		/// <summary>
		/// Write out a string literal or attribute expression or expression element.
		/// </summary>
		/// <remarks>
		/// If doing line wrap, check wrap before emitting this string.  If at or 
		/// beyond the desired line width then, emit a \n and any indentation before 
		/// spitting out this string.
		/// </remarks>
		/// <exception cref="IOException"
		public int Write(string str, string wrap)
		{
			int n = WriteWrapSeparator(wrap);
			return n + Write(str);
		}

		public int WriteWrapSeparator(string wrap)
		{
			int n = 0;
			// if want wrap and not already at start of line (last char was \n)
			// and we have hit or exceeded the threshold
			if ((lineWidth != Constants.TEMPLATE_WRITER_NO_WRAP) && (wrap != null)
				&& !atStartOfLine && (charPosition >= lineWidth))
			{
				// ok to wrap
				// Walk wrap string and look for A\nB.  Spit out A\n
				// then spit indent or anchor, whichever is larger
				// then spit out B
				for (int i = 0; i < wrap.Length; i++)
				{
					char c = wrap[i];
					if (c == '\n')
					{
						n++;
						output.Write(c);
						//atStartOfLine = true;
						charPosition = 0;
						int indentWidth = GetIndentationWidth();
						int lastAnchor = 0;
						if (anchors_sp >= 0)
						{
							lastAnchor = anchors[anchors_sp];
						}
						if (lastAnchor > indentWidth)
						{
							// use anchor not indentation
							n += Indent(lastAnchor);
						}
						else
						{
							// indent is farther over than last anchor, ignore anchor
							n += Indent();
						}
						// continue writing any chars out
					}
					else
					{  // write A or B part
						n++;
						output.Write(c);
						charPosition++;
					}
				}
			}
			return n;
		}

		public virtual int Indent()
		{
			int n = 0;
			for (int i = 0; i < indents.Count; i++)
			{
				string ind = (string)indents[i];
				if (ind != null)
				{
					n += ind.Length;
					output.Write(ind);
				}
			}
			charPosition += n;
			return n;
		}

		public int Indent(int spaces)
		{
			for (int i = 1; i <= spaces; i++)
			{
				output.Write(' ');
			}
			charPosition += spaces;
			return spaces;
		}
	}
}