// $ANTLR 2.7.6 (2005-12-22): "interface.g" -> "InterfaceParser.cs"$

/*
 [The "BSD licence"]
 Copyright (c) 2003-2004 Terence Parr
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
using System.Collections;
using Antlr.StringTemplate.Collections;

namespace Antlr.StringTemplate.Language
{
	// Generate the header common to all output files.
	using System;
	
	using TokenBuffer              = antlr.TokenBuffer;
	using TokenStreamException     = antlr.TokenStreamException;
	using TokenStreamIOException   = antlr.TokenStreamIOException;
	using ANTLRException           = antlr.ANTLRException;
	using LLkParser = antlr.LLkParser;
	using Token                    = antlr.Token;
	using IToken                   = antlr.IToken;
	using TokenStream              = antlr.TokenStream;
	using RecognitionException     = antlr.RecognitionException;
	using NoViableAltException     = antlr.NoViableAltException;
	using MismatchedTokenException = antlr.MismatchedTokenException;
	using SemanticException        = antlr.SemanticException;
	using ParserSharedInputState   = antlr.ParserSharedInputState;
	using BitSet                   = antlr.collections.impl.BitSet;
	
/** Match an ST group interface.  Just a list of template names with args.
 *  Here is a sample interface file:
 *
 *	interface nfa;
 *	nfa(states,edges);
 *	optional state(name);
 */
	public 	class InterfaceParser : antlr.LLkParser
	{
		public const int EOF = 1;
		public const int NULL_TREE_LOOKAHEAD = 3;
		public const int LITERAL_interface = 4;
		public const int ID = 5;
		public const int SEMI = 6;
		public const int LITERAL_optional = 7;
		public const int LPAREN = 8;
		public const int RPAREN = 9;
		public const int COMMA = 10;
		public const int COLON = 11;
		public const int SL_COMMENT = 12;
		public const int ML_COMMENT = 13;
		public const int WS = 14;
		
		
protected StringTemplateGroupInterface groupI;

override public void reportError(RecognitionException e) {
	if ( groupI!=null ) {
	    groupI.Error("template group interface parse error", e);
	}
	else {
	    Console.Error.WriteLine("template group interface parse error: "+e);
	    Console.Error.WriteLine(e.StackTrace);
	}
}
		
		protected void initialize()
		{
			tokenNames = tokenNames_;
		}
		
		
		protected InterfaceParser(TokenBuffer tokenBuf, int k) : base(tokenBuf, k)
		{
			initialize();
		}
		
		public InterfaceParser(TokenBuffer tokenBuf) : this(tokenBuf,3)
		{
		}
		
		protected InterfaceParser(TokenStream lexer, int k) : base(lexer,k)
		{
			initialize();
		}
		
		public InterfaceParser(TokenStream lexer) : this(lexer,3)
		{
		}
		
		public InterfaceParser(ParserSharedInputState state) : base(state,3)
		{
			initialize();
		}
		
	public void groupInterface(
		StringTemplateGroupInterface groupI
	) //throws RecognitionException, TokenStreamException
{
		
		IToken  name = null;
		this.groupI = groupI;
		
		try {      // for error handling
			match(LITERAL_interface);
			name = LT(1);
			match(ID);
			groupI.Name = name.getText();
			match(SEMI);
			{ // ( ... )+
				int _cnt3=0;
				for (;;)
				{
					if ((LA(1)==ID||LA(1)==LITERAL_optional))
					{
						template(groupI);
					}
					else
					{
						if (_cnt3 >= 1) { goto _loop3_breakloop; } else { throw new NoViableAltException(LT(1), getFilename());; }
					}
					
					_cnt3++;
				}
_loop3_breakloop:				;
			}    // ( ... )+
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			recover(ex,tokenSet_0_);
		}
	}
	
	public void template(
		StringTemplateGroupInterface groupI
	) //throws RecognitionException, TokenStreamException
{
		
		IToken  opt = null;
		IToken  name = null;
		
		HashList formalArgs = new HashList(); // leave blank if no args
		string templateName=null;
		
		
		try {      // for error handling
			{
				switch ( LA(1) )
				{
				case LITERAL_optional:
				{
					opt = LT(1);
					match(LITERAL_optional);
					break;
				}
				case ID:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			name = LT(1);
			match(ID);
			match(LPAREN);
			{
				switch ( LA(1) )
				{
				case ID:
				{
					formalArgs=args();
					break;
				}
				case RPAREN:
				{
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			match(RPAREN);
			match(SEMI);
			
			templateName = name.getText();
			groupI.DefineTemplate(templateName, formalArgs, opt!=null);
			
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			recover(ex,tokenSet_1_);
		}
	}
	
	public HashList  args() //throws RecognitionException, TokenStreamException
{
		HashList args=new HashList();
		
		IToken  a = null;
		IToken  b = null;
		
		try {      // for error handling
			a = LT(1);
			match(ID);
			args[a.getText()] = new FormalArgument(a.getText());
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==COMMA))
					{
						match(COMMA);
						b = LT(1);
						match(ID);
						args[b.getText()] = new FormalArgument(b.getText());
					}
					else
					{
						goto _loop9_breakloop;
					}
					
				}
_loop9_breakloop:				;
			}    // ( ... )*
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			recover(ex,tokenSet_2_);
		}
		return args;
	}
	
	private void initializeFactory()
	{
	}
	
	public static readonly string[] tokenNames_ = new string[] {
		@"""<0>""",
		@"""EOF""",
		@"""<2>""",
		@"""NULL_TREE_LOOKAHEAD""",
		@"""interface""",
		@"""ID""",
		@"""SEMI""",
		@"""optional""",
		@"""LPAREN""",
		@"""RPAREN""",
		@"""COMMA""",
		@"""COLON""",
		@"""SL_COMMENT""",
		@"""ML_COMMENT""",
		@"""WS"""
	};
	
	private static long[] mk_tokenSet_0_()
	{
		long[] data = { 2L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());
	private static long[] mk_tokenSet_1_()
	{
		long[] data = { 162L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());
	private static long[] mk_tokenSet_2_()
	{
		long[] data = { 512L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_2_ = new BitSet(mk_tokenSet_2_());
	
}
}
