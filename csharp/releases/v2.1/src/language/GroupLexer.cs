// $ANTLR 2.7.5rc2 (20050108): "group.g" -> "GroupLexer.cs"$

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

namespace antlr.stringtemplate.language
{
	// Generate header specific to lexer CSharp file
	using System;
	using Stream                          = System.IO.Stream;
	using TextReader                      = System.IO.TextReader;
	using Hashtable                       = System.Collections.Hashtable;
	using Comparer                        = System.Collections.Comparer;
	
	using TokenStreamException            = antlr.TokenStreamException;
	using TokenStreamIOException          = antlr.TokenStreamIOException;
	using TokenStreamRecognitionException = antlr.TokenStreamRecognitionException;
	using CharStreamException             = antlr.CharStreamException;
	using CharStreamIOException           = antlr.CharStreamIOException;
	using ANTLRException                  = antlr.ANTLRException;
	using CharScanner                     = antlr.CharScanner;
	using InputBuffer                     = antlr.InputBuffer;
	using ByteBuffer                      = antlr.ByteBuffer;
	using CharBuffer                      = antlr.CharBuffer;
	using Token                           = antlr.Token;
	using IToken                          = antlr.IToken;
	using CommonToken                     = antlr.CommonToken;
	using SemanticException               = antlr.SemanticException;
	using RecognitionException            = antlr.RecognitionException;
	using NoViableAltForCharException     = antlr.NoViableAltForCharException;
	using MismatchedCharException         = antlr.MismatchedCharException;
	using TokenStream                     = antlr.TokenStream;
	using LexerSharedInputState           = antlr.LexerSharedInputState;
	using BitSet                          = antlr.collections.impl.BitSet;
	
	public 	class GroupLexer : antlr.CharScanner	, TokenStream
	 {
		public const int EOF = 1;
		public const int NULL_TREE_LOOKAHEAD = 3;
		public const int LITERAL_group = 4;
		public const int ID = 5;
		public const int SEMI = 6;
		public const int LPAREN = 7;
		public const int RPAREN = 8;
		public const int DEFINED_TO_BE = 9;
		public const int TEMPLATE = 10;
		public const int COMMA = 11;
		public const int STAR = 12;
		public const int PLUS = 13;
		public const int OPTIONAL = 14;
		public const int SL_COMMENT = 15;
		public const int ML_COMMENT = 16;
		public const int WS = 17;
		
		public GroupLexer(Stream ins) : this(new ByteBuffer(ins))
		{
		}
		
		public GroupLexer(TextReader r) : this(new CharBuffer(r))
		{
		}
		
		public GroupLexer(InputBuffer ib)		 : this(new LexerSharedInputState(ib))
		{
		}
		
		public GroupLexer(LexerSharedInputState state) : base(state)
		{
			initialize();
		}
		private void initialize()
		{
			caseSensitiveLiterals = true;
			setCaseSensitive(true);
			literals = new Hashtable(100, (float) 0.4, null, Comparer.Default);
			literals.Add("group", 4);
		}
		
		override public IToken nextToken()			//throws TokenStreamException
		{
			IToken theRetToken = null;
tryAgain:
			for (;;)
			{
				IToken _token = null;
				int _ttype = Token.INVALID_TYPE;
				resetText();
				try     // for char stream error handling
				{
					try     // for lexical error handling
					{
						switch ( cached_LA1 )
						{
						case 'A':  case 'B':  case 'C':  case 'D':
						case 'E':  case 'F':  case 'G':  case 'H':
						case 'I':  case 'J':  case 'K':  case 'L':
						case 'M':  case 'N':  case 'O':  case 'P':
						case 'Q':  case 'R':  case 'S':  case 'T':
						case 'U':  case 'V':  case 'W':  case 'X':
						case 'Y':  case 'Z':  case 'a':  case 'b':
						case 'c':  case 'd':  case 'e':  case 'f':
						case 'g':  case 'h':  case 'i':  case 'j':
						case 'k':  case 'l':  case 'm':  case 'n':
						case 'o':  case 'p':  case 'q':  case 'r':
						case 's':  case 't':  case 'u':  case 'v':
						case 'w':  case 'x':  case 'y':  case 'z':
						{
							mID(true);
							theRetToken = returnToken_;
							break;
						}
						case '"':  case '<':
						{
							mTEMPLATE(true);
							theRetToken = returnToken_;
							break;
						}
						case '(':
						{
							mLPAREN(true);
							theRetToken = returnToken_;
							break;
						}
						case ')':
						{
							mRPAREN(true);
							theRetToken = returnToken_;
							break;
						}
						case ',':
						{
							mCOMMA(true);
							theRetToken = returnToken_;
							break;
						}
						case ':':
						{
							mDEFINED_TO_BE(true);
							theRetToken = returnToken_;
							break;
						}
						case ';':
						{
							mSEMI(true);
							theRetToken = returnToken_;
							break;
						}
						case '*':
						{
							mSTAR(true);
							theRetToken = returnToken_;
							break;
						}
						case '+':
						{
							mPLUS(true);
							theRetToken = returnToken_;
							break;
						}
						case '?':
						{
							mOPTIONAL(true);
							theRetToken = returnToken_;
							break;
						}
						case '\t':  case '\n':  case '\u000c':  case '\r':
						case ' ':
						{
							mWS(true);
							theRetToken = returnToken_;
							break;
						}
						default:
							if ((cached_LA1=='/') && (cached_LA2=='/'))
							{
								mSL_COMMENT(true);
								theRetToken = returnToken_;
							}
							else if ((cached_LA1=='/') && (cached_LA2=='*')) {
								mML_COMMENT(true);
								theRetToken = returnToken_;
							}
						else
						{
							if (cached_LA1==EOF_CHAR) { uponEOF(); returnToken_ = makeToken(Token.EOF_TYPE); }
				else {throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());}
						}
						break; }
						if ( null==returnToken_ ) goto tryAgain; // found SKIP token
						_ttype = returnToken_.Type;
						_ttype = testLiteralsTable(_ttype);
						returnToken_.Type = _ttype;
						return returnToken_;
					}
					catch (RecognitionException e) {
							throw new TokenStreamRecognitionException(e);
					}
				}
				catch (CharStreamException cse) {
					if ( cse is CharStreamIOException ) {
						throw new TokenStreamIOException(((CharStreamIOException)cse).io);
					}
					else {
						throw new TokenStreamException(cse.Message);
					}
				}
			}
		}
		
	public void mID(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = ID;
		
		{
			switch ( cached_LA1 )
			{
			case 'a':  case 'b':  case 'c':  case 'd':
			case 'e':  case 'f':  case 'g':  case 'h':
			case 'i':  case 'j':  case 'k':  case 'l':
			case 'm':  case 'n':  case 'o':  case 'p':
			case 'q':  case 'r':  case 's':  case 't':
			case 'u':  case 'v':  case 'w':  case 'x':
			case 'y':  case 'z':
			{
				matchRange('a','z');
				break;
			}
			case 'A':  case 'B':  case 'C':  case 'D':
			case 'E':  case 'F':  case 'G':  case 'H':
			case 'I':  case 'J':  case 'K':  case 'L':
			case 'M':  case 'N':  case 'O':  case 'P':
			case 'Q':  case 'R':  case 'S':  case 'T':
			case 'U':  case 'V':  case 'W':  case 'X':
			case 'Y':  case 'Z':
			{
				matchRange('A','Z');
				break;
			}
			default:
			{
				throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
			}
			 }
		}
		{    // ( ... )*
			for (;;)
			{
				switch ( cached_LA1 )
				{
				case 'a':  case 'b':  case 'c':  case 'd':
				case 'e':  case 'f':  case 'g':  case 'h':
				case 'i':  case 'j':  case 'k':  case 'l':
				case 'm':  case 'n':  case 'o':  case 'p':
				case 'q':  case 'r':  case 's':  case 't':
				case 'u':  case 'v':  case 'w':  case 'x':
				case 'y':  case 'z':
				{
					matchRange('a','z');
					break;
				}
				case 'A':  case 'B':  case 'C':  case 'D':
				case 'E':  case 'F':  case 'G':  case 'H':
				case 'I':  case 'J':  case 'K':  case 'L':
				case 'M':  case 'N':  case 'O':  case 'P':
				case 'Q':  case 'R':  case 'S':  case 'T':
				case 'U':  case 'V':  case 'W':  case 'X':
				case 'Y':  case 'Z':
				{
					matchRange('A','Z');
					break;
				}
				case '0':  case '1':  case '2':  case '3':
				case '4':  case '5':  case '6':  case '7':
				case '8':  case '9':
				{
					matchRange('0','9');
					break;
				}
				case '-':
				{
					match('-');
					break;
				}
				case '_':
				{
					match('_');
					break;
				}
				default:
				{
					goto _loop12_breakloop;
				}
				 }
			}
_loop12_breakloop:			;
		}    // ( ... )*
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mTEMPLATE(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = TEMPLATE;
		
		switch ( cached_LA1 )
		{
		case '"':
		{
			int _saveIndex = 0;
			_saveIndex = text.Length;
			match('"');
			text.Length = _saveIndex;
			{ // ( ... )+
				int _cnt15=0;
				for (;;)
				{
					if ((cached_LA1=='\\') && (cached_LA2=='"'))
					{
						_saveIndex = text.Length;
						match('\\');
						text.Length = _saveIndex;
						match('"');
					}
					else if ((cached_LA1=='\\') && (tokenSet_0_.member(cached_LA2))) {
						match('\\');
						matchNot('"');
					}
					else if ((tokenSet_1_.member(cached_LA1))) {
						matchNot('"');
					}
					else
					{
						if (_cnt15 >= 1) { goto _loop15_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
					}
					
					_cnt15++;
				}
_loop15_breakloop:				;
			}    // ( ... )+
			_saveIndex = text.Length;
			match('"');
			text.Length = _saveIndex;
			break;
		}
		case '<':
		{
			int _saveIndex = 0;
			_saveIndex = text.Length;
			match("<<");
			text.Length = _saveIndex;
			{
				if ((cached_LA1=='\n'||cached_LA1=='\r') && ((cached_LA2 >= '\u0000' && cached_LA2 <= '\ufffe')))
				{
					{
						switch ( cached_LA1 )
						{
						case '\r':
						{
							_saveIndex = text.Length;
							match('\r');
							text.Length = _saveIndex;
							break;
						}
						case '\n':
						{
							break;
						}
						default:
						{
							throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
						}
						 }
					}
					_saveIndex = text.Length;
					match('\n');
					text.Length = _saveIndex;
					newline();
				}
				else if (((cached_LA1 >= '\u0000' && cached_LA1 <= '\ufffe')) && ((cached_LA2 >= '\u0000' && cached_LA2 <= '\ufffe'))) {
				}
				else
				{
					throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
				}
				
			}
			{    // ( ... )*
				for (;;)
				{
					// nongreedy exit test
					if ((cached_LA1=='>') && (cached_LA2=='>')) goto _loop20_breakloop;
					if (((cached_LA1=='\r') && (cached_LA2=='\n'))&&(LA(3)=='>'&&LA(4)=='>'))
					{
						_saveIndex = text.Length;
						match('\r');
						text.Length = _saveIndex;
						_saveIndex = text.Length;
						match('\n');
						text.Length = _saveIndex;
						newline();
					}
					else if (((cached_LA1=='\n') && ((cached_LA2 >= '\u0000' && cached_LA2 <= '\ufffe')))&&(LA(2)=='>'&&LA(3)=='>')) {
						_saveIndex = text.Length;
						match('\n');
						text.Length = _saveIndex;
						newline();
					}
					else if ((cached_LA1=='\n'||cached_LA1=='\r') && ((cached_LA2 >= '\u0000' && cached_LA2 <= '\ufffe'))) {
						{
							switch ( cached_LA1 )
							{
							case '\r':
							{
								match('\r');
								break;
							}
							case '\n':
							{
								break;
							}
							default:
							{
								throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
							}
							 }
						}
						match('\n');
						newline();
					}
					else if (((cached_LA1 >= '\u0000' && cached_LA1 <= '\ufffe')) && ((cached_LA2 >= '\u0000' && cached_LA2 <= '\ufffe'))) {
						matchNot(EOF/*_CHAR*/);
					}
					else
					{
						goto _loop20_breakloop;
					}
					
				}
_loop20_breakloop:				;
			}    // ( ... )*
			_saveIndex = text.Length;
			match(">>");
			text.Length = _saveIndex;
			break;
		}
		default:
		{
			throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
		}
		 }
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mLPAREN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = LPAREN;
		
		match('(');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mRPAREN(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = RPAREN;
		
		match(')');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mCOMMA(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = COMMA;
		
		match(',');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mDEFINED_TO_BE(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = DEFINED_TO_BE;
		
		match("::=");
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSEMI(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = SEMI;
		
		match(';');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSTAR(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = STAR;
		
		match('*');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mPLUS(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = PLUS;
		
		match('+');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mOPTIONAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = OPTIONAL;
		
		match('?');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mSL_COMMENT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = SL_COMMENT;
		
		match("//");
		{    // ( ... )*
			for (;;)
			{
				if ((tokenSet_2_.member(cached_LA1)))
				{
					{
						match(tokenSet_2_);
					}
				}
				else
				{
					goto _loop32_breakloop;
				}
				
			}
_loop32_breakloop:			;
		}    // ( ... )*
		{
			if ((cached_LA1=='\n'||cached_LA1=='\r'))
			{
				{
					switch ( cached_LA1 )
					{
					case '\r':
					{
						match('\r');
						break;
					}
					case '\n':
					{
						break;
					}
					default:
					{
						throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
					}
					 }
				}
				match('\n');
			}
			else {
			}
			
		}
		_ttype = Token.SKIP; newline();
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mML_COMMENT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = ML_COMMENT;
		
		match("/*");
		{    // ( ... )*
			for (;;)
			{
				// nongreedy exit test
				if ((cached_LA1=='*') && (cached_LA2=='/')) goto _loop38_breakloop;
				if ((cached_LA1=='\n'||cached_LA1=='\r') && ((cached_LA2 >= '\u0000' && cached_LA2 <= '\ufffe')))
				{
					{
						switch ( cached_LA1 )
						{
						case '\r':
						{
							match('\r');
							break;
						}
						case '\n':
						{
							break;
						}
						default:
						{
							throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
						}
						 }
					}
					match('\n');
					newline();
				}
				else if (((cached_LA1 >= '\u0000' && cached_LA1 <= '\ufffe')) && ((cached_LA2 >= '\u0000' && cached_LA2 <= '\ufffe'))) {
					matchNot(EOF/*_CHAR*/);
				}
				else
				{
					goto _loop38_breakloop;
				}
				
			}
_loop38_breakloop:			;
		}    // ( ... )*
		match("*/");
		_ttype = Token.SKIP;
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mWS(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = WS;
		
		{ // ( ... )+
			int _cnt42=0;
			for (;;)
			{
				switch ( cached_LA1 )
				{
				case ' ':
				{
					match(' ');
					break;
				}
				case '\t':
				{
					match('\t');
					break;
				}
				case '\u000c':
				{
					match('\f');
					break;
				}
				case '\n':  case '\r':
				{
					{
						switch ( cached_LA1 )
						{
						case '\r':
						{
							match('\r');
							break;
						}
						case '\n':
						{
							break;
						}
						default:
						{
							throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
						}
						 }
					}
					match('\n');
					newline();
					break;
				}
				default:
				{
					if (_cnt42 >= 1) { goto _loop42_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
				}
				break; }
				_cnt42++;
			}
_loop42_breakloop:			;
		}    // ( ... )+
		_ttype = Token.SKIP;
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	
	private static long[] mk_tokenSet_0_()
	{
		long[] data = new long[2048];
		data[0]=-17179869185L;
		for (int i = 1; i<=1022; i++) { data[i]=-1L; }
		data[1023]=9223372036854775807L;
		for (int i = 1024; i<=2047; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());
	private static long[] mk_tokenSet_1_()
	{
		long[] data = new long[2048];
		data[0]=-17179869185L;
		data[1]=-268435457L;
		for (int i = 2; i<=1022; i++) { data[i]=-1L; }
		data[1023]=9223372036854775807L;
		for (int i = 1024; i<=2047; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());
	private static long[] mk_tokenSet_2_()
	{
		long[] data = new long[2048];
		data[0]=-9217L;
		for (int i = 1; i<=1022; i++) { data[i]=-1L; }
		data[1023]=9223372036854775807L;
		for (int i = 1024; i<=2047; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_2_ = new BitSet(mk_tokenSet_2_());
	
}
}
