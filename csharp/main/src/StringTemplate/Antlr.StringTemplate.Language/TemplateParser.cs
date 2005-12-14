// $ANTLR 2.7.5rc2 (2005-01-08): "template.g" -> "TemplateParser.cs"$

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
	
/** A parser used to break up a single template into chunks, text literals
 *  and attribute expressions.
 */
	public 	class TemplateParser : antlr.LLkParser
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
		
		
protected StringTemplate self;

override public void reportError(RecognitionException e) {
    StringTemplateGroup group = self.Group;
    if ( group==StringTemplate.defaultGroup ) {
        self.Error("template parse error; template context is "+self.GetEnclosingInstanceStackString(), e);
    }
    else {
        self.Error("template parse error in group "+self.Group.Name+" line "+self.GroupFileLine+"; template context is "+self.GetEnclosingInstanceStackString(), e);
    }
}
		
		protected void initialize()
		{
			tokenNames = tokenNames_;
		}
		
		
		protected TemplateParser(TokenBuffer tokenBuf, int k) : base(tokenBuf, k)
		{
			initialize();
		}
		
		public TemplateParser(TokenBuffer tokenBuf) : this(tokenBuf,1)
		{
		}
		
		protected TemplateParser(TokenStream lexer, int k) : base(lexer,k)
		{
			initialize();
		}
		
		public TemplateParser(TokenStream lexer) : this(lexer,1)
		{
		}
		
		public TemplateParser(ParserSharedInputState state) : base(state,1)
		{
			initialize();
		}
		
	public void template(
		StringTemplate self
	) //throws RecognitionException, TokenStreamException
{
		
		IToken  s = null;
		IToken  nl = null;
		
			this.self = self;
		
		
		try {      // for error handling
			{    // ( ... )*
				for (;;)
				{
					switch ( LA(1) )
					{
					case LITERAL:
					{
						s = LT(1);
						match(LITERAL);
						self.AddChunk(new StringRef(self,s.getText()));
						break;
					}
					case NEWLINE:
					{
						nl = LT(1);
						match(NEWLINE);
						
						if ( (LA(1) != ELSE) && (LA(1) != ENDIF) ) {
							self.AddChunk(new NewlineRef(self,nl.getText()));
						}
							
						break;
					}
					case ACTION:
					case IF:
					case REGION_REF:
					case REGION_DEF:
					{
						action(self);
						break;
					}
					default:
					{
						goto _loop3_breakloop;
					}
					 }
				}
_loop3_breakloop:				;
			}    // ( ... )*
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			recover(ex,tokenSet_0_);
		}
	}
	
	public void action(
		StringTemplate self
	) //throws RecognitionException, TokenStreamException
{
		
		IToken  a = null;
		IToken  i = null;
		IToken  rr = null;
		IToken  rd = null;
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case ACTION:
			{
				a = LT(1);
				match(ACTION);
				
				string indent = ((ChunkToken)a).Indentation;
				ASTExpr c = self.ParseAction(a.getText());
				c.Indentation = indent;
				self.AddChunk(c);
				
				break;
			}
			case IF:
			{
				i = LT(1);
				match(IF);
				
				ConditionalExpr c = (ConditionalExpr)self.ParseAction(i.getText());
				// create and precompile the subtemplate
				StringTemplate subtemplate =
					new StringTemplate(self.Group, null);
				subtemplate.EnclosingInstance = self;
				subtemplate.Name = i.getText() + "_subtemplate";
				self.AddChunk(c);
				
				template(subtemplate);
				if ( c!=null ) c.Subtemplate = subtemplate;
				{
					switch ( LA(1) )
					{
					case ELSE:
					{
						match(ELSE);
						
						// create and precompile the subtemplate
						StringTemplate elseSubtemplate =
								new StringTemplate(self.Group, null);
						elseSubtemplate.EnclosingInstance = self;
						elseSubtemplate.Name = "else_subtemplate";
						
						template(elseSubtemplate);
						if ( c!=null ) c.ElseSubtemplate = elseSubtemplate;
						break;
					}
					case ENDIF:
					{
						break;
					}
					default:
					{
						throw new NoViableAltException(LT(1), getFilename());
					}
					 }
				}
				match(ENDIF);
				break;
			}
			case REGION_REF:
			{
				rr = LT(1);
				match(REGION_REF);
				
						// define implicit template and
						// convert <@r()> to <region__enclosingTemplate__r()>
							string regionName = rr.getText();
							string mangledRef = null;
							bool err = false;
							// watch out for <@super.r()>; that does NOT def implicit region
							// convert to <super.region__enclosingTemplate__r()>
							if ( regionName.StartsWith("super.") ) {
								//Console.Out.Writeline("super region ref "+regionName);
								string regionRef =
									regionName.Substring("super.".Length,regionName.Length-"super.".Length);
								string templateScope =
									self.Group.GetUnMangledTemplateName(self.Name);
								StringTemplate scopeST = self.Group.LookupTemplate(templateScope);
								if ( scopeST==null ) {
									self.Group.Error("reference to region within undefined template: "+
										templateScope);
									err=true;
								}
								if ( !scopeST.ContainsRegionName(regionRef) ) {
									self.Group.Error("template "+templateScope+" has no region called "+
										regionRef);
									err=true;
								}
								else {
									mangledRef =
										self.Group.GetMangledRegionName(templateScope,regionRef);
									mangledRef = "super."+mangledRef;
								}
							}
							else {
								//Console.Out.WriteLine("region ref "+regionName);
								StringTemplate regionST =
				self.Group.DefineImplicitRegionTemplate(self,regionName);
				mangledRef = regionST.Name;
				}
				
							if ( !err ) {
								// treat as regular action: mangled template include
								string indent = ((ChunkToken)rr).Indentation;
								ASTExpr c = self.ParseAction(mangledRef+"()");
								c.Indentation = indent;
								self.AddChunk(c);
							}
					
				break;
			}
			case REGION_DEF:
			{
				rd = LT(1);
				match(REGION_DEF);
				
							string combinedNameTemplateStr = rd.getText();
							int indexOfDefSymbol = combinedNameTemplateStr.IndexOf("::=");
							if ( indexOfDefSymbol>=1 ) {
								string regionName = combinedNameTemplateStr.Substring(0,indexOfDefSymbol);
								string template =
									combinedNameTemplateStr.Substring(indexOfDefSymbol+3,
										combinedNameTemplateStr.Length - (indexOfDefSymbol+3));
								StringTemplate regionST =
				self.Group.DefineRegionTemplate(self,
													regionName,
													template,
													StringTemplate.REGION_EMBEDDED);
								// treat as regular action: mangled template include
								string indent = ((ChunkToken)rd).Indentation;
								ASTExpr c = self.ParseAction(regionST.Name+"()");
								c.Indentation = indent;
								self.AddChunk(c);
							}
							else {
								self.Error("embedded region definition screwed up");
							}
						
				break;
			}
			default:
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			 }
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			recover(ex,tokenSet_1_);
		}
	}
	
	private void initializeFactory()
	{
	}
	
	public static readonly string[] tokenNames_ = new string[] {
		@"""<0>""",
		@"""EOF""",
		@"""<2>""",
		@"""NULL_TREE_LOOKAHEAD""",
		@"""LITERAL""",
		@"""NEWLINE""",
		@"""ACTION""",
		@"""IF""",
		@"""ELSE""",
		@"""ENDIF""",
		@"""REGION_REF""",
		@"""REGION_DEF""",
		@"""EXPR""",
		@"""TEMPLATE""",
		@"""IF_EXPR""",
		@"""ESC""",
		@"""SUBTEMPLATE""",
		@"""NESTED_PARENS""",
		@"""INDENT""",
		@"""COMMENT"""
	};
	
	private static long[] mk_tokenSet_0_()
	{
		long[] data = { 768L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());
	private static long[] mk_tokenSet_1_()
	{
		long[] data = { 4080L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());
	
}
}
