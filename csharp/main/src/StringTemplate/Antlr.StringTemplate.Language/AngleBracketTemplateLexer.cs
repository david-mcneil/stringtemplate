// $ANTLR 2.7.6 (2005-12-22): "angle.bracket.template.g" -> "AngleBracketTemplateLexer.cs"$

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
using System.IO;

namespace Antlr.StringTemplate.Language
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
	
/** Break up an input text stream into chunks of either plain text
 *  or template actions in "<...>".  Treat IF and ENDIF tokens
 *  specially.
 */
	public 	class AngleBracketTemplateLexer : antlr.CharScanner	, TokenStream
	 {
		public const int EOF = 1;
		public const int NULL_TREE_LOOKAHEAD = 3;
		public const int LITERAL = 4;
		public const int NEWLINE = 5;
		public const int ACTION = 6;
		public const int IF = 7;
		public const int ELSE = 8;
		public const int ENDIF = 9;
		public const int REGION_REF = 10;
		public const int REGION_DEF = 11;
		public const int EXPR = 12;
		public const int TEMPLATE = 13;
		public const int IF_EXPR = 14;
		public const int ESC = 15;
		public const int SUBTEMPLATE = 16;
		public const int NESTED_PARENS = 17;
		public const int INDENT = 18;
		public const int COMMENT = 19;
		
		
protected String currentIndent = null;
protected StringTemplate self;

public AngleBracketTemplateLexer(StringTemplate self, TextReader r) : this(r) {
	this.self = self;
}

override public void reportError(RecognitionException e) {
	self.Error("<...> chunk lexer error", e);
}

protected bool upcomingELSE(int i) {
 	return LA(i)=='<'&&LA(i+1)=='e'&&LA(i+2)=='l'&&LA(i+3)=='s'&&LA(i+4)=='e'&&
 	       LA(i+5)=='>';
}

protected bool upcomingENDIF(int i) {
	return LA(i)=='<'&&LA(i+1)=='e'&&LA(i+2)=='n'&&LA(i+3)=='d'&&LA(i+4)=='i'&&
	       LA(i+5)=='f'&&LA(i+6)=='>';
}
protected bool upcomingAtEND(int i) {
	return LA(i)=='<'&&LA(i+1)=='@'&&LA(i+2)=='e'&&LA(i+3)=='n'&&LA(i+4)=='d'&&LA(i+5)=='>';
}
protected bool upcomingNewline(int i) {
	return (LA(i)=='\r'&&LA(i+1)=='\n')||LA(i)=='\n';
}

		public AngleBracketTemplateLexer(Stream ins) : this(new ByteBuffer(ins))
		{
		}
		
		public AngleBracketTemplateLexer(TextReader r) : this(new CharBuffer(r))
		{
		}
		
		public AngleBracketTemplateLexer(InputBuffer ib)		 : this(new LexerSharedInputState(ib))
		{
		}
		
		public AngleBracketTemplateLexer(LexerSharedInputState state) : base(state)
		{
			initialize();
		}
		private void initialize()
		{
			caseSensitiveLiterals = true;
			setCaseSensitive(true);
			literals = new Hashtable(100, (float) 0.4, null, Comparer.Default);
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
						case '\n':  case '\r':
						{
							mNEWLINE(true);
							theRetToken = returnToken_;
							break;
						}
						case '<':
						{
							mACTION(true);
							theRetToken = returnToken_;
							break;
						}
						default:
							if (((tokenSet_0_.member(cached_LA1)))&&(((cached_LA1 != '\r') && (cached_LA1 != '\n'))))
							{
								mLITERAL(true);
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
		
	public void mLITERAL(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = LITERAL;
		IToken ind = null;
		
		if (!(((cached_LA1 != '\r') && (cached_LA1 != '\n'))))
		  throw new SemanticException("((cached_LA1 != '\\r') && (cached_LA1 != '\\n'))");
		{ // ( ... )+
			int _cnt5=0;
			for (;;)
			{
				
				int loopStartIndex=text.Length;
				int col=getColumn();
				
				if ((cached_LA1=='\\') && (cached_LA2=='<'))
				{
					int _saveIndex = 0;
					_saveIndex = text.Length;
					match('\\');
					text.Length = _saveIndex;
					match('<');
				}
				else if ((cached_LA1=='\\') && (cached_LA2=='>') && (true) && (true) && (true) && (true) && (true)) {
					int _saveIndex = 0;
					_saveIndex = text.Length;
					match('\\');
					text.Length = _saveIndex;
					match('>');
				}
				else if ((cached_LA1=='\\') && (tokenSet_1_.member(cached_LA2)) && (true) && (true) && (true) && (true) && (true)) {
					match('\\');
					{
						match(tokenSet_1_);
					}
				}
				else if ((cached_LA1=='\t'||cached_LA1==' ') && (true) && (true) && (true) && (true) && (true) && (true)) {
					mINDENT(true);
					ind = returnToken_;
					
					if ( (col == 1) && (cached_LA1 == '<') ) {
					// store indent in ASTExpr not in a literal
					currentIndent=ind.getText();
								  text.Length = loopStartIndex; // reset length to wack text
					}
					else currentIndent=null;
					
				}
				else if ((tokenSet_0_.member(cached_LA1)) && (true) && (true) && (true) && (true) && (true) && (true)) {
					{
						match(tokenSet_0_);
					}
				}
				else
				{
					if (_cnt5 >= 1) { goto _loop5_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
				}
				
				_cnt5++;
			}
_loop5_breakloop:			;
		}    // ( ... )+
		if ((text.ToString(_begin, text.Length-_begin)).Length==0) {_ttype = Token.SKIP;}
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	protected void mINDENT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = INDENT;
		
		{ // ( ... )+
			int _cnt8=0;
			for (;;)
			{
				if ((cached_LA1==' ') && (true) && (true) && (true) && (true) && (true) && (true))
				{
					match(' ');
				}
				else if ((cached_LA1=='\t') && (true) && (true) && (true) && (true) && (true) && (true)) {
					match('\t');
				}
				else
				{
					if (_cnt8 >= 1) { goto _loop8_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
				}
				
				_cnt8++;
			}
_loop8_breakloop:			;
		}    // ( ... )+
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mNEWLINE(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = NEWLINE;
		
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
		newline(); currentIndent=null;
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	public void mACTION(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = ACTION;
		
		int startCol = getColumn();
		
		
		if ((cached_LA1=='<') && (cached_LA2=='\\') && (LA(3)=='n') && (LA(4)=='>') && (true) && (true) && (true))
		{
			int _saveIndex = 0;
			_saveIndex = text.Length;
			match("<\\n>");
			text.Length = _saveIndex;
			text.Length = _begin; text.Append('\n'); _ttype = LITERAL;
		}
		else if ((cached_LA1=='<') && (cached_LA2=='\\') && (LA(3)=='r') && (LA(4)=='>') && (true) && (true) && (true)) {
			int _saveIndex = 0;
			_saveIndex = text.Length;
			match("<\\r>");
			text.Length = _saveIndex;
			text.Length = _begin; text.Append('\r'); _ttype = LITERAL;
		}
		else if ((cached_LA1=='<') && (cached_LA2=='\\') && (LA(3)=='t') && (LA(4)=='>') && (true) && (true) && (true)) {
			int _saveIndex = 0;
			_saveIndex = text.Length;
			match("<\\t>");
			text.Length = _saveIndex;
			text.Length = _begin; text.Append('\t'); _ttype = LITERAL;
		}
		else if ((cached_LA1=='<') && (cached_LA2=='\\') && (LA(3)==' ') && (LA(4)=='>') && (true) && (true) && (true)) {
			int _saveIndex = 0;
			_saveIndex = text.Length;
			match("<\\ >");
			text.Length = _saveIndex;
			text.Length = _begin; text.Append(' '); _ttype = LITERAL;
		}
		else if ((cached_LA1=='<') && (cached_LA2=='!') && ((LA(3) >= '\u0001' && LA(3) <= '\ufffe')) && ((LA(4) >= '\u0001' && LA(4) <= '\ufffe')) && (true) && (true) && (true)) {
			mCOMMENT(false);
			_ttype = Token.SKIP;
		}
		else if ((cached_LA1=='<') && (tokenSet_2_.member(cached_LA2)) && ((LA(3) >= '\u0001' && LA(3) <= '\ufffe')) && (true) && (true) && (true) && (true)) {
			{
				if ((cached_LA1=='<') && (cached_LA2=='i') && (LA(3)=='f') && (LA(4)==' '||LA(4)=='(') && (tokenSet_3_.member(LA(5))) && ((LA(6) >= '\u0001' && LA(6) <= '\ufffe')) && ((LA(7) >= '\u0001' && LA(7) <= '\ufffe')))
				{
					int _saveIndex = 0;
					_saveIndex = text.Length;
					match('<');
					text.Length = _saveIndex;
					match("if");
					{    // ( ... )*
						for (;;)
						{
							if ((cached_LA1==' '))
							{
								_saveIndex = text.Length;
								match(' ');
								text.Length = _saveIndex;
							}
							else
							{
								goto _loop14_breakloop;
							}
							
						}
_loop14_breakloop:						;
					}    // ( ... )*
					match("(");
					mIF_EXPR(false);
					match(")");
					_saveIndex = text.Length;
					match('>');
					text.Length = _saveIndex;
					_ttype = TemplateParser.IF;
					{
						if ((cached_LA1=='\n'||cached_LA1=='\r'))
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
						else {
						}
						
					}
				}
				else if ((cached_LA1=='<') && (cached_LA2=='e') && (LA(3)=='n') && (LA(4)=='d') && (LA(5)=='i') && (LA(6)=='f') && (LA(7)=='>')) {
					int _saveIndex = 0;
					_saveIndex = text.Length;
					match('<');
					text.Length = _saveIndex;
					match("endif");
					_saveIndex = text.Length;
					match('>');
					text.Length = _saveIndex;
					_ttype = TemplateParser.ENDIF;
					{
						if (((cached_LA1=='\n'||cached_LA1=='\r'))&&(startCol==1))
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
						else {
						}
						
					}
				}
				else if ((cached_LA1=='<') && (cached_LA2=='e') && (LA(3)=='l') && (LA(4)=='s') && (LA(5)=='e') && (LA(6)=='>') && (true)) {
					int _saveIndex = 0;
					_saveIndex = text.Length;
					match('<');
					text.Length = _saveIndex;
					match("else");
					_saveIndex = text.Length;
					match('>');
					text.Length = _saveIndex;
					_ttype = TemplateParser.ELSE;
					{
						if ((cached_LA1=='\n'||cached_LA1=='\r'))
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
						else {
						}
						
					}
				}
				else if ((cached_LA1=='<') && (cached_LA2=='@') && (tokenSet_4_.member(LA(3))) && ((LA(4) >= '\u0001' && LA(4) <= '\ufffe')) && ((LA(5) >= '\u0001' && LA(5) <= '\ufffe')) && ((LA(6) >= '\u0001' && LA(6) <= '\ufffe')) && (true)) {
					int _saveIndex = 0;
					_saveIndex = text.Length;
					match('<');
					text.Length = _saveIndex;
					_saveIndex = text.Length;
					match('@');
					text.Length = _saveIndex;
					{ // ( ... )+
						int _cnt23=0;
						for (;;)
						{
							if ((tokenSet_4_.member(cached_LA1)))
							{
								{
									match(tokenSet_4_);
								}
							}
							else
							{
								if (_cnt23 >= 1) { goto _loop23_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
							}
							
							_cnt23++;
						}
_loop23_breakloop:						;
					}    // ( ... )+
					{
						switch ( cached_LA1 )
						{
						case '(':
						{
							_saveIndex = text.Length;
							match("()");
							text.Length = _saveIndex;
							_saveIndex = text.Length;
							match('>');
							text.Length = _saveIndex;
							_ttype = TemplateParser.REGION_REF;
							break;
						}
						case '>':
						{
							_saveIndex = text.Length;
							match('>');
							text.Length = _saveIndex;
							
									_ttype = TemplateParser.REGION_DEF;
									string t=text.ToString(_begin, text.Length-_begin);
									text.Length = _begin; text.Append(t+"::=");
									
							{
								if ((cached_LA1=='\n'||cached_LA1=='\r') && ((cached_LA2 >= '\u0001' && cached_LA2 <= '\ufffe')) && ((LA(3) >= '\u0001' && LA(3) <= '\ufffe')) && (true) && (true) && (true) && (true))
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
								else if (((cached_LA1 >= '\u0001' && cached_LA1 <= '\ufffe')) && ((cached_LA2 >= '\u0001' && cached_LA2 <= '\ufffe')) && (true) && (true) && (true) && (true) && (true)) {
								}
								else
								{
									throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
								}
								
							}
							bool atLeft = false;
							{ // ( ... )+
								int _cnt30=0;
								for (;;)
								{
									if ((((cached_LA1 >= '\u0001' && cached_LA1 <= '\ufffe')) && ((cached_LA2 >= '\u0001' && cached_LA2 <= '\ufffe')) && (true) && (true) && (true) && (true) && (true))&&(!(upcomingAtEND(1) || (upcomingNewline(1) && upcomingAtEND(2)))))
									{
										{
											if ((cached_LA1=='\n'||cached_LA1=='\r') && ((cached_LA2 >= '\u0001' && cached_LA2 <= '\ufffe')) && (true) && (true) && (true) && (true) && (true))
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
												newline(); atLeft = true;
											}
											else if (((cached_LA1 >= '\u0001' && cached_LA1 <= '\ufffe')) && ((cached_LA2 >= '\u0001' && cached_LA2 <= '\ufffe')) && (true) && (true) && (true) && (true) && (true)) {
												matchNot(EOF/*_CHAR*/);
												atLeft = false;
											}
											else
											{
												throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
											}
											
										}
									}
									else
									{
										if (_cnt30 >= 1) { goto _loop30_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
									}
									
									_cnt30++;
								}
_loop30_breakloop:								;
							}    // ( ... )+
							{
								if ((cached_LA1=='\n'||cached_LA1=='\r') && ((cached_LA2 >= '\u0001' && cached_LA2 <= '\ufffe')) && (true) && (true) && (true) && (true) && (true))
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
									newline(); atLeft = true;
								}
								else if (((cached_LA1 >= '\u0001' && cached_LA1 <= '\ufffe')) && (true) && (true) && (true) && (true) && (true) && (true)) {
								}
								else
								{
									throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
								}
								
							}
							{
								if ((cached_LA1=='<') && (cached_LA2=='@'))
								{
									_saveIndex = text.Length;
									match("<@end>");
									text.Length = _saveIndex;
								}
								else if (((cached_LA1 >= '\u0001' && cached_LA1 <= '\ufffe')) && (true)) {
									matchNot(EOF/*_CHAR*/);
									self.Error("missing region "+t+" <@end> tag");
								}
								else
								{
									throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
								}
								
							}
							{
								if (((cached_LA1=='\n'||cached_LA1=='\r'))&&(atLeft))
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
								else {
								}
								
							}
							break;
						}
						default:
						{
							throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
						}
						 }
					}
				}
				else if ((cached_LA1=='<') && (tokenSet_2_.member(cached_LA2)) && ((LA(3) >= '\u0001' && LA(3) <= '\ufffe')) && (true) && (true) && (true) && (true)) {
					int _saveIndex = 0;
					_saveIndex = text.Length;
					match('<');
					text.Length = _saveIndex;
					mEXPR(false);
					_saveIndex = text.Length;
					match('>');
					text.Length = _saveIndex;
				}
				else
				{
					throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
				}
				
			}
			
			ChunkToken ctok = new ChunkToken(_ttype, text.ToString(_begin, text.Length-_begin), currentIndent);
			_token = ctok;
				
		}
		else
		{
			throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
		}
		
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	protected void mCOMMENT(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = COMMENT;
		
		int startCol = getColumn();
		
		
		match("<!");
		{    // ( ... )*
			for (;;)
			{
				// nongreedy exit test
				if ((cached_LA1=='!') && (cached_LA2=='>') && (true) && (true) && (true) && (true) && (true)) goto _loop67_breakloop;
				if ((cached_LA1=='\n'||cached_LA1=='\r') && ((cached_LA2 >= '\u0001' && cached_LA2 <= '\ufffe')) && ((LA(3) >= '\u0001' && LA(3) <= '\ufffe')) && (true) && (true) && (true) && (true))
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
				else if (((cached_LA1 >= '\u0001' && cached_LA1 <= '\ufffe')) && ((cached_LA2 >= '\u0001' && cached_LA2 <= '\ufffe')) && ((LA(3) >= '\u0001' && LA(3) <= '\ufffe')) && (true) && (true) && (true) && (true)) {
					matchNot(EOF/*_CHAR*/);
				}
				else
				{
					goto _loop67_breakloop;
				}
				
			}
_loop67_breakloop:			;
		}    // ( ... )*
		match("!>");
		{
			if (((cached_LA1=='\n'||cached_LA1=='\r'))&&(startCol==1))
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
			else {
			}
			
		}
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	protected void mIF_EXPR(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = IF_EXPR;
		
		{ // ( ... )+
			int _cnt55=0;
			for (;;)
			{
				switch ( cached_LA1 )
				{
				case '\\':
				{
					mESC(false);
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
				case '{':
				{
					mSUBTEMPLATE(false);
					break;
				}
				case '(':
				{
					mNESTED_PARENS(false);
					break;
				}
				default:
					if ((tokenSet_5_.member(cached_LA1)))
					{
						matchNot(')');
					}
				else
				{
					if (_cnt55 >= 1) { goto _loop55_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
				}
				break; }
				_cnt55++;
			}
_loop55_breakloop:			;
		}    // ( ... )+
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	protected void mEXPR(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = EXPR;
		
		{ // ( ... )+
			int _cnt43=0;
			for (;;)
			{
				switch ( cached_LA1 )
				{
				case '\\':
				{
					mESC(false);
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
				case '{':
				{
					mSUBTEMPLATE(false);
					break;
				}
				default:
					if ((cached_LA1=='+'||cached_LA1=='=') && (cached_LA2=='"'||cached_LA2=='<'))
					{
						{
							switch ( cached_LA1 )
							{
							case '=':
							{
								match('=');
								break;
							}
							case '+':
							{
								match('+');
								break;
							}
							default:
							{
								throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
							}
							 }
						}
						mTEMPLATE(false);
					}
					else if ((cached_LA1=='+'||cached_LA1=='=') && (cached_LA2=='{')) {
						{
							switch ( cached_LA1 )
							{
							case '=':
							{
								match('=');
								break;
							}
							case '+':
							{
								match('+');
								break;
							}
							default:
							{
								throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
							}
							 }
						}
						mSUBTEMPLATE(false);
					}
					else if ((cached_LA1=='+'||cached_LA1=='=') && (tokenSet_6_.member(cached_LA2))) {
						{
							switch ( cached_LA1 )
							{
							case '=':
							{
								match('=');
								break;
							}
							case '+':
							{
								match('+');
								break;
							}
							default:
							{
								throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
							}
							 }
						}
						{
							match(tokenSet_6_);
						}
					}
					else if ((tokenSet_7_.member(cached_LA1))) {
						matchNot('>');
					}
				else
				{
					if (_cnt43 >= 1) { goto _loop43_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
				}
				break; }
				_cnt43++;
			}
_loop43_breakloop:			;
		}    // ( ... )+
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	protected void mESC(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = ESC;
		
		match('\\');
		{
			switch ( cached_LA1 )
			{
			case '<':
			{
				match('<');
				break;
			}
			case '>':
			{
				match('>');
				break;
			}
			case 'r':
			{
				match('r');
				break;
			}
			case 'n':
			{
				match('n');
				break;
			}
			case 't':
			{
				match('t');
				break;
			}
			case '\\':
			{
				match('\\');
				break;
			}
			case '"':
			{
				match('"');
				break;
			}
			case '\'':
			{
				match('\'');
				break;
			}
			case ':':
			{
				match(':');
				break;
			}
			case '{':
			{
				match('{');
				break;
			}
			case '}':
			{
				match('}');
				break;
			}
			default:
			{
				throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());
			}
			 }
		}
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	protected void mSUBTEMPLATE(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = SUBTEMPLATE;
		
		match('{');
		{    // ( ... )*
			for (;;)
			{
				switch ( cached_LA1 )
				{
				case '{':
				{
					mSUBTEMPLATE(false);
					break;
				}
				case '\\':
				{
					mESC(false);
					break;
				}
				default:
					if ((tokenSet_8_.member(cached_LA1)))
					{
						matchNot('}');
					}
				else
				{
					goto _loop60_breakloop;
				}
				break; }
			}
_loop60_breakloop:			;
		}    // ( ... )*
		match('}');
		if (_createToken && (null == _token) && (_ttype != Token.SKIP))
		{
			_token = makeToken(_ttype);
			_token.setText(text.ToString(_begin, text.Length-_begin));
		}
		returnToken_ = _token;
	}
	
	protected void mTEMPLATE(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = TEMPLATE;
		
		switch ( cached_LA1 )
		{
		case '"':
		{
			match('"');
			{    // ( ... )*
				for (;;)
				{
					if ((cached_LA1=='\\'))
					{
						mESC(false);
					}
					else if ((tokenSet_9_.member(cached_LA1))) {
						matchNot('"');
					}
					else
					{
						goto _loop46_breakloop;
					}
					
				}
_loop46_breakloop:				;
			}    // ( ... )*
			match('"');
			break;
		}
		case '<':
		{
			match("<<");
			{
				if ((cached_LA1=='\n'||cached_LA1=='\r') && ((cached_LA2 >= '\u0001' && cached_LA2 <= '\ufffe')) && ((LA(3) >= '\u0001' && LA(3) <= '\ufffe')) && ((LA(4) >= '\u0001' && LA(4) <= '\ufffe')) && (true) && (true) && (true))
				{
					int _saveIndex = 0;
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
				else if (((cached_LA1 >= '\u0001' && cached_LA1 <= '\ufffe')) && ((cached_LA2 >= '\u0001' && cached_LA2 <= '\ufffe')) && ((LA(3) >= '\u0001' && LA(3) <= '\ufffe')) && (true) && (true) && (true) && (true)) {
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
					if ((cached_LA1=='>') && (cached_LA2=='>') && ((LA(3) >= '\u0001' && LA(3) <= '\ufffe')) && (true) && (true) && (true) && (true)) goto _loop51_breakloop;
					if (((cached_LA1=='\r') && (cached_LA2=='\n') && ((LA(3) >= '\u0001' && LA(3) <= '\ufffe')) && ((LA(4) >= '\u0001' && LA(4) <= '\ufffe')) && ((LA(5) >= '\u0001' && LA(5) <= '\ufffe')) && (true) && (true))&&(LA(3)=='>'&&LA(4)=='>'))
					{
						int _saveIndex = 0;
						_saveIndex = text.Length;
						match('\r');
						text.Length = _saveIndex;
						_saveIndex = text.Length;
						match('\n');
						text.Length = _saveIndex;
						newline();
					}
					else if (((cached_LA1=='\n') && ((cached_LA2 >= '\u0001' && cached_LA2 <= '\ufffe')) && ((LA(3) >= '\u0001' && LA(3) <= '\ufffe')) && ((LA(4) >= '\u0001' && LA(4) <= '\ufffe')) && (true) && (true) && (true))&&(LA(2)=='>'&&LA(3)=='>')) {
						int _saveIndex = 0;
						_saveIndex = text.Length;
						match('\n');
						text.Length = _saveIndex;
						newline();
					}
					else if ((cached_LA1=='\n'||cached_LA1=='\r') && ((cached_LA2 >= '\u0001' && cached_LA2 <= '\ufffe')) && ((LA(3) >= '\u0001' && LA(3) <= '\ufffe')) && ((LA(4) >= '\u0001' && LA(4) <= '\ufffe')) && (true) && (true) && (true)) {
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
					else if (((cached_LA1 >= '\u0001' && cached_LA1 <= '\ufffe')) && ((cached_LA2 >= '\u0001' && cached_LA2 <= '\ufffe')) && ((LA(3) >= '\u0001' && LA(3) <= '\ufffe')) && ((LA(4) >= '\u0001' && LA(4) <= '\ufffe')) && (true) && (true) && (true)) {
						matchNot(EOF/*_CHAR*/);
					}
					else
					{
						goto _loop51_breakloop;
					}
					
				}
_loop51_breakloop:				;
			}    // ( ... )*
			match(">>");
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
	
	protected void mNESTED_PARENS(bool _createToken) //throws RecognitionException, CharStreamException, TokenStreamException
{
		int _ttype; IToken _token=null; int _begin=text.Length;
		_ttype = NESTED_PARENS;
		
		match('(');
		{ // ( ... )+
			int _cnt63=0;
			for (;;)
			{
				switch ( cached_LA1 )
				{
				case '(':
				{
					mNESTED_PARENS(false);
					break;
				}
				case '\\':
				{
					mESC(false);
					break;
				}
				default:
					if ((tokenSet_10_.member(cached_LA1)))
					{
						matchNot(')');
					}
				else
				{
					if (_cnt63 >= 1) { goto _loop63_breakloop; } else { throw new NoViableAltForCharException(cached_LA1, getFilename(), getLine(), getColumn());; }
				}
				break; }
				_cnt63++;
			}
_loop63_breakloop:			;
		}    // ( ... )+
		match(')');
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
		data[0]=-1152921504606856194L;
		for (int i = 1; i<=1022; i++) { data[i]=-1L; }
		data[1023]=9223372036854775807L;
		for (int i = 1024; i<=2047; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());
	private static long[] mk_tokenSet_1_()
	{
		long[] data = new long[2048];
		data[0]=-5764607523034234882L;
		for (int i = 1; i<=1022; i++) { data[i]=-1L; }
		data[1023]=9223372036854775807L;
		for (int i = 1024; i<=2047; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());
	private static long[] mk_tokenSet_2_()
	{
		long[] data = new long[2048];
		data[0]=-4611686018427387906L;
		for (int i = 1; i<=1022; i++) { data[i]=-1L; }
		data[1023]=9223372036854775807L;
		for (int i = 1024; i<=2047; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_2_ = new BitSet(mk_tokenSet_2_());
	private static long[] mk_tokenSet_3_()
	{
		long[] data = new long[2048];
		data[0]=-2199023255554L;
		for (int i = 1; i<=1022; i++) { data[i]=-1L; }
		data[1023]=9223372036854775807L;
		for (int i = 1024; i<=2047; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_3_ = new BitSet(mk_tokenSet_3_());
	private static long[] mk_tokenSet_4_()
	{
		long[] data = new long[2048];
		data[0]=-4611687117939015682L;
		for (int i = 1; i<=1022; i++) { data[i]=-1L; }
		data[1023]=9223372036854775807L;
		for (int i = 1024; i<=2047; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_4_ = new BitSet(mk_tokenSet_4_());
	private static long[] mk_tokenSet_5_()
	{
		long[] data = new long[2048];
		data[0]=-3298534892546L;
		data[1]=-576460752571858945L;
		for (int i = 2; i<=1022; i++) { data[i]=-1L; }
		data[1023]=9223372036854775807L;
		for (int i = 1024; i<=2047; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_5_ = new BitSet(mk_tokenSet_5_());
	private static long[] mk_tokenSet_6_()
	{
		long[] data = new long[2048];
		data[0]=-1152921521786716162L;
		data[1]=-576460752303423489L;
		for (int i = 2; i<=1022; i++) { data[i]=-1L; }
		data[1023]=9223372036854775807L;
		for (int i = 1024; i<=2047; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_6_ = new BitSet(mk_tokenSet_6_());
	private static long[] mk_tokenSet_7_()
	{
		long[] data = new long[2048];
		data[0]=-6917537823734113282L;
		data[1]=-576460752571858945L;
		for (int i = 2; i<=1022; i++) { data[i]=-1L; }
		data[1023]=9223372036854775807L;
		for (int i = 1024; i<=2047; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_7_ = new BitSet(mk_tokenSet_7_());
	private static long[] mk_tokenSet_8_()
	{
		long[] data = new long[2048];
		data[0]=-2L;
		data[1]=-2882303761785552897L;
		for (int i = 2; i<=1022; i++) { data[i]=-1L; }
		data[1023]=9223372036854775807L;
		for (int i = 1024; i<=2047; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_8_ = new BitSet(mk_tokenSet_8_());
	private static long[] mk_tokenSet_9_()
	{
		long[] data = new long[2048];
		data[0]=-17179869186L;
		data[1]=-268435457L;
		for (int i = 2; i<=1022; i++) { data[i]=-1L; }
		data[1023]=9223372036854775807L;
		for (int i = 1024; i<=2047; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_9_ = new BitSet(mk_tokenSet_9_());
	private static long[] mk_tokenSet_10_()
	{
		long[] data = new long[2048];
		data[0]=-3298534883330L;
		data[1]=-268435457L;
		for (int i = 2; i<=1022; i++) { data[i]=-1L; }
		data[1023]=9223372036854775807L;
		for (int i = 1024; i<=2047; i++) { data[i]=0L; }
		return data;
	}
	public static readonly BitSet tokenSet_10_ = new BitSet(mk_tokenSet_10_());
	
}
}
