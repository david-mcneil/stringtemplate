// $ANTLR 2.7.5 (20050128): "group.g" -> "GroupParser.cs"$

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
	
/** Match a group of template definitions beginning
 *  with a group name declaration.  Templates are enclosed
 *  in double-quotes or <<...>> quotes for multi-line templates.
 *  Template names have arg lists that indicate the cardinality
 *  of the attribute: present, optional, zero-or-more, one-or-more.
 *  Here is a sample group file:

	group nfa;

	// an NFA has edges and states
	nfa(states,edges) ::= <<
	digraph NFA {
	rankdir=LR;
	<states; separator="\\n">
	<edges; separator="\\n">
	}
	>>

	state(name) ::= "node [shape = circle]; <name>;"

 */
	public 	class GroupParser : antlr.LLkParser
	{
		public const int EOF = 1;
		public const int NULL_TREE_LOOKAHEAD = 3;
		public const int LITERAL_group = 4;
		public const int ID = 5;
		public const int SEMI = 6;
		public const int LPAREN = 7;
		public const int RPAREN = 8;
		public const int DEFINED_TO_BE = 9;
		public const int STRING = 10;
		public const int BIGSTRING = 11;
		public const int COMMA = 12;
		public const int ASSIGN = 13;
		public const int ANONYMOUS_TEMPLATE = 14;
		public const int LBRACK = 15;
		public const int RBRACK = 16;
		public const int COLON = 17;
		public const int LITERAL_default = 18;
		public const int STAR = 19;
		public const int PLUS = 20;
		public const int OPTIONAL = 21;
		public const int SL_COMMENT = 22;
		public const int ML_COMMENT = 23;
		public const int WS = 24;
		
		
protected StringTemplateGroup _group;

override public void reportError(RecognitionException e) {
	if (_group != null)
		_group.error("template parse error", e);
	else
		Console.Error.WriteLine("template parse error: "+e);
}
		
		protected void initialize()
		{
			tokenNames = tokenNames_;
		}
		
		
		protected GroupParser(TokenBuffer tokenBuf, int k) : base(tokenBuf, k)
		{
			initialize();
		}
		
		public GroupParser(TokenBuffer tokenBuf) : this(tokenBuf,3)
		{
		}
		
		protected GroupParser(TokenStream lexer, int k) : base(lexer,k)
		{
			initialize();
		}
		
		public GroupParser(TokenStream lexer) : this(lexer,3)
		{
		}
		
		public GroupParser(ParserSharedInputState state) : base(state,3)
		{
			initialize();
		}
		
	public void group(
		StringTemplateGroup g
	) //throws RecognitionException, TokenStreamException
{
		
		IToken  name = null;
		
		try {      // for error handling
			match(LITERAL_group);
			name = LT(1);
			match(ID);
			g.setName(name.getText());
			match(SEMI);
			{ // ( ... )+
				int _cnt3=0;
				for (;;)
				{
					if ((LA(1)==ID) && (LA(2)==LPAREN||LA(2)==DEFINED_TO_BE) && (LA(3)==ID||LA(3)==RPAREN))
					{
						template(g);
					}
					else if ((LA(1)==ID) && (LA(2)==DEFINED_TO_BE) && (LA(3)==LBRACK)) {
						mapdef(g);
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
		StringTemplateGroup g
	) //throws RecognitionException, TokenStreamException
{
		
		IToken  name = null;
		IToken  t = null;
		IToken  bt = null;
		IToken  alias = null;
		IToken  target = null;
		
		IDictionary formalArgs = null;
		StringTemplate st = null;
		bool ignore = false;
		
		
		try {      // for error handling
			if ((LA(1)==ID) && (LA(2)==LPAREN))
			{
				name = LT(1);
				match(ID);
				
					    if ( g.isDefinedInThisGroup(name.getText()) ) {
					        g.error("redefinition of template: "+name.getText());
					        st = new StringTemplate(); // create bogus template to fill in
					    }
					    else {
					        st = g.defineTemplate(name.getText(), null);
					    }
					
				match(LPAREN);
				{
					switch ( LA(1) )
					{
					case ID:
					{
						args(st);
						break;
					}
					case RPAREN:
					{
						st.defineEmptyFormalArgumentList();
						break;
					}
					default:
					{
						throw new NoViableAltException(LT(1), getFilename());
					}
					 }
				}
				match(RPAREN);
				match(DEFINED_TO_BE);
				{
					switch ( LA(1) )
					{
					case STRING:
					{
						t = LT(1);
						match(STRING);
						st.setTemplate(t.getText());
						break;
					}
					case BIGSTRING:
					{
						bt = LT(1);
						match(BIGSTRING);
						st.setTemplate(bt.getText());
						break;
					}
					default:
					{
						throw new NoViableAltException(LT(1), getFilename());
					}
					 }
				}
			}
			else if ((LA(1)==ID) && (LA(2)==DEFINED_TO_BE)) {
				alias = LT(1);
				match(ID);
				match(DEFINED_TO_BE);
				target = LT(1);
				match(ID);
				g.defineTemplateAlias(alias.getText(), target.getText());
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			recover(ex,tokenSet_1_);
		}
	}
	
	public void mapdef(
		StringTemplateGroup g
	) //throws RecognitionException, TokenStreamException
{
		
		IToken  name = null;
		
		IDictionary m=null;
		
		
		try {      // for error handling
			name = LT(1);
			match(ID);
			match(DEFINED_TO_BE);
			m=map();
			
				    if ( g.getMap(name.getText())!=null ) {
				        g.error("redefinition of map: "+name.getText());
				    }
				    else if ( g.isDefinedInThisGroup(name.getText()) ) {
				        g.error("redefinition of template as map: "+name.getText());
				    }
				    else {
				    	g.defineMap(name.getText(), m);
				    }
				
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			recover(ex,tokenSet_1_);
		}
	}
	
	public void args(
		StringTemplate st
	) //throws RecognitionException, TokenStreamException
{
		
		
		try {      // for error handling
			arg(st);
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==COMMA))
					{
						match(COMMA);
						arg(st);
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
	}
	
	public void arg(
		StringTemplate st
	) //throws RecognitionException, TokenStreamException
{
		
		IToken  name = null;
		IToken  s = null;
		IToken  bs = null;
		
			StringTemplate defaultValue = null;
		
		
		try {      // for error handling
			name = LT(1);
			match(ID);
			{
				if ((LA(1)==ASSIGN) && (LA(2)==STRING))
				{
					match(ASSIGN);
					s = LT(1);
					match(STRING);
					
								defaultValue=new StringTemplate("$_val_$");
								defaultValue.setAttribute("_val_", s.getText());
								defaultValue.defineFormalArgument("_val_");
								defaultValue.setName("<"+st.getName()+"'s arg "+name.getText()+" default value subtemplate>");
								
				}
				else if ((LA(1)==ASSIGN) && (LA(2)==ANONYMOUS_TEMPLATE)) {
					match(ASSIGN);
					bs = LT(1);
					match(ANONYMOUS_TEMPLATE);
					
								defaultValue=new StringTemplate(st.getGroup(), bs.getText());
								defaultValue.setName("<"+st.getName()+"'s arg "+name.getText()+" default value subtemplate>");
								
				}
				else if ((LA(1)==RPAREN||LA(1)==COMMA)) {
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				
			}
			st.defineFormalArgument(name.getText(), defaultValue);
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			recover(ex,tokenSet_3_);
		}
	}
	
	public IDictionary  map() //throws RecognitionException, TokenStreamException
{
		IDictionary mapping=new Hashtable();
		
		
		try {      // for error handling
			match(LBRACK);
			keyValuePair(mapping);
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==COMMA))
					{
						match(COMMA);
						keyValuePair(mapping);
					}
					else
					{
						goto _loop15_breakloop;
					}
					
				}
_loop15_breakloop:				;
			}    // ( ... )*
			match(RBRACK);
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			recover(ex,tokenSet_1_);
		}
		return mapping;
	}
	
	public void keyValuePair(
		IDictionary mapping
	) //throws RecognitionException, TokenStreamException
{
		
		IToken  key1 = null;
		IToken  s1 = null;
		IToken  key2 = null;
		IToken  s2 = null;
		IToken  s3 = null;
		IToken  s4 = null;
		
		try {      // for error handling
			if ((LA(1)==STRING) && (LA(2)==COLON) && (LA(3)==STRING))
			{
				key1 = LT(1);
				match(STRING);
				match(COLON);
				s1 = LT(1);
				match(STRING);
				mapping[key1.getText()]=s1.getText();
			}
			else if ((LA(1)==STRING) && (LA(2)==COLON) && (LA(3)==BIGSTRING)) {
				key2 = LT(1);
				match(STRING);
				match(COLON);
				s2 = LT(1);
				match(BIGSTRING);
				mapping[key2.getText()]=s2.getText();
			}
			else if ((LA(1)==LITERAL_default) && (LA(2)==COLON) && (LA(3)==STRING)) {
				match(LITERAL_default);
				match(COLON);
				s3 = LT(1);
				match(STRING);
				mapping[ASTExpr.DEFAULT_MAP_VALUE_NAME]=s3.getText();
			}
			else if ((LA(1)==LITERAL_default) && (LA(2)==COLON) && (LA(3)==BIGSTRING)) {
				match(LITERAL_default);
				match(COLON);
				s4 = LT(1);
				match(BIGSTRING);
				mapping[ASTExpr.DEFAULT_MAP_VALUE_NAME]=s4.getText();
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			recover(ex,tokenSet_4_);
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
		@"""group""",
		@"""ID""",
		@"""SEMI""",
		@"""LPAREN""",
		@"""RPAREN""",
		@"""DEFINED_TO_BE""",
		@"""STRING""",
		@"""BIGSTRING""",
		@"""COMMA""",
		@"""ASSIGN""",
		@"""ANONYMOUS_TEMPLATE""",
		@"""LBRACK""",
		@"""RBRACK""",
		@"""COLON""",
		@"""default""",
		@"""STAR""",
		@"""PLUS""",
		@"""OPTIONAL""",
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
		long[] data = { 34L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());
	private static long[] mk_tokenSet_2_()
	{
		long[] data = { 256L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_2_ = new BitSet(mk_tokenSet_2_());
	private static long[] mk_tokenSet_3_()
	{
		long[] data = { 4352L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_3_ = new BitSet(mk_tokenSet_3_());
	private static long[] mk_tokenSet_4_()
	{
		long[] data = { 69632L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_4_ = new BitSet(mk_tokenSet_4_());
	
}
}
