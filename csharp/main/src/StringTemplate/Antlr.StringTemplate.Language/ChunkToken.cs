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
	using IToken	= antlr.IToken;
	using CommonToken	= antlr.CommonToken;
	using TokenCreator	= antlr.TokenCreator;
	
	/// <summary>
	/// Tracks the various string and attribute chunks discovered
	/// by the lexer.  Subclassed CommonToken so that I could pass
	/// the indentation to the parser, which will add it to the
	/// ASTExpr created for the $...$ attribute reference.
	/// </summary>
	public class ChunkToken : CommonToken
	{
		new public static readonly ChunkToken.ChunkTokenCreator Creator = new ChunkTokenCreator();

		virtual public string Indentation
		{
			get { return _indentation; }
			set { _indentation = value; }
		}

		private string _indentation;
		
		public ChunkToken() : base()
		{
		}
		
		public ChunkToken(int type, string text, string indentation) : base(type, text)
		{
			this._indentation = indentation;
		}
		
		public override string ToString()
		{
			return base.ToString() + "<indent='" + _indentation + "'>";
		}

		public class ChunkTokenCreator : TokenCreator
		{
			public ChunkTokenCreator() {}

			/// <summary>
			/// Returns the fully qualified name of the Token type that this
			/// class creates.
			/// </summary>
			public override string TokenTypeName
			{
				get 
				{ 
					return typeof(ChunkToken).FullName;; 
				}
			}

			/// <summary>
			/// Constructs a <see cref="Token"/> instance.
			/// </summary>
			public override IToken Create()
			{
				return new ChunkToken();
			}
		}
	}
}