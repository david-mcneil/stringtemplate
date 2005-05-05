// $ANTLR 2.7.5rc2 (20050108): "action.g" -> "ActionParser.cs"$

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
	using AST                      = antlr.collections.AST;
	using ASTPair                  = antlr.ASTPair;
	using ASTFactory               = antlr.ASTFactory;
	using ASTArray                 = antlr.collections.impl.ASTArray;
	
/** Parse the individual attribute expressions */
	public 	class ActionParser : antlr.LLkParser
	{
		public const int EOF = 1;
		public const int NULL_TREE_LOOKAHEAD = 3;
		public const int APPLY = 4;
		public const int ARGS = 5;
		public const int INCLUDE = 6;
		public const int CONDITIONAL = 7;
		public const int VALUE = 8;
		public const int TEMPLATE = 9;
		public const int SEMI = 10;
		public const int LPAREN = 11;
		public const int RPAREN = 12;
		public const int LITERAL_separator = 13;
		public const int ASSIGN = 14;
		public const int NOT = 15;
		public const int PLUS = 16;
		public const int COLON = 17;
		public const int COMMA = 18;
		public const int ID = 19;
		public const int LITERAL_super = 20;
		public const int DOT = 21;
		public const int ANONYMOUS_TEMPLATE = 22;
		public const int STRING = 23;
		public const int INT = 24;
		public const int NESTED_ANONYMOUS_TEMPLATE = 25;
		public const int ESC_CHAR = 26;
		public const int WS = 27;
		
		
    protected StringTemplate self = null;

    public ActionParser(TokenStream lexer, StringTemplate self) : this(lexer, 2) {        
        this.self = self;
    }

	override public void reportError(RecognitionException e) {
		self.error("template parse error", e);
	}
		
		protected void initialize()
		{
			tokenNames = tokenNames_;
			initializeFactory();
		}
		
		
		protected ActionParser(TokenBuffer tokenBuf, int k) : base(tokenBuf, k)
		{
			initialize();
		}
		
		public ActionParser(TokenBuffer tokenBuf) : this(tokenBuf,2)
		{
		}
		
		protected ActionParser(TokenStream lexer, int k) : base(lexer,k)
		{
			initialize();
		}
		
		public ActionParser(TokenStream lexer) : this(lexer,2)
		{
		}
		
		public ActionParser(ParserSharedInputState state) : base(state,2)
		{
			initialize();
		}
		
	public IDictionary  action() //throws RecognitionException, TokenStreamException
{
		IDictionary opts=null;
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		antlr.stringtemplate.language.StringTemplateAST action_AST = null;
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case LPAREN:
			case ID:
			case LITERAL_super:
			case STRING:
			case INT:
			{
				templatesExpr();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(currentAST, (AST)returnAST);
				}
				{
					switch ( LA(1) )
					{
					case SEMI:
					{
						match(SEMI);
						opts=optionList();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(currentAST, (AST)returnAST);
						}
						break;
					}
					case EOF:
					{
						break;
					}
					default:
					{
						throw new NoViableAltException(LT(1), getFilename());
					}
					 }
				}
				action_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
				break;
			}
			case CONDITIONAL:
			{
				antlr.stringtemplate.language.StringTemplateAST tmp2_AST = null;
				tmp2_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(LT(1));
				astFactory.makeASTRoot(currentAST, (AST)tmp2_AST);
				match(CONDITIONAL);
				match(LPAREN);
				ifCondition();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(currentAST, (AST)returnAST);
				}
				match(RPAREN);
				action_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
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
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_0_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = action_AST;
		return opts;
		ASTPair.PutInstance(currentAST);
	}
	
	public void templatesExpr() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		antlr.stringtemplate.language.StringTemplateAST templatesExpr_AST = null;
		IToken  c = null;
		antlr.stringtemplate.language.StringTemplateAST c_AST = null;
		
		try {      // for error handling
			expr();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(currentAST, (AST)returnAST);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==COLON))
					{
						c = LT(1);
						c_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(c);
						astFactory.makeASTRoot(currentAST, (AST)c_AST);
						match(COLON);
						if (0==inputState.guessing)
						{
							c_AST.setType(APPLY);
						}
						template();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(currentAST, (AST)returnAST);
						}
						{    // ( ... )*
							for (;;)
							{
								if ((LA(1)==COMMA))
								{
									match(COMMA);
									template();
									if (0 == inputState.guessing)
									{
										astFactory.addASTChild(currentAST, (AST)returnAST);
									}
								}
								else
								{
									goto _loop15_breakloop;
								}
								
							}
_loop15_breakloop:							;
						}    // ( ... )*
					}
					else
					{
						goto _loop16_breakloop;
					}
					
				}
_loop16_breakloop:				;
			}    // ( ... )*
			templatesExpr_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_1_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = templatesExpr_AST;
		ASTPair.PutInstance(currentAST);
	}
	
	public IDictionary  optionList() //throws RecognitionException, TokenStreamException
{
		IDictionary opts=new Hashtable();
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		antlr.stringtemplate.language.StringTemplateAST optionList_AST = null;
		antlr.stringtemplate.language.StringTemplateAST e_AST = null;
		
		try {      // for error handling
			match(LITERAL_separator);
			antlr.stringtemplate.language.StringTemplateAST tmp7_AST = null;
			tmp7_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(LT(1));
			match(ASSIGN);
			expr();
			if (0 == inputState.guessing)
			{
				e_AST = (antlr.stringtemplate.language.StringTemplateAST)returnAST;
			}
			if (0==inputState.guessing)
			{
				opts["separator"]=e_AST;
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_0_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = optionList_AST;
		return opts;
		ASTPair.PutInstance(currentAST);
	}
	
	public void ifCondition() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		antlr.stringtemplate.language.StringTemplateAST ifCondition_AST = null;
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case LPAREN:
			case ID:
			case LITERAL_super:
			case STRING:
			case INT:
			{
				ifAtom();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(currentAST, (AST)returnAST);
				}
				ifCondition_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
				break;
			}
			case NOT:
			{
				antlr.stringtemplate.language.StringTemplateAST tmp8_AST = null;
				tmp8_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(LT(1));
				astFactory.makeASTRoot(currentAST, (AST)tmp8_AST);
				match(NOT);
				ifAtom();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(currentAST, (AST)returnAST);
				}
				ifCondition_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
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
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = ifCondition_AST;
		ASTPair.PutInstance(currentAST);
	}
	
	public void expr() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		antlr.stringtemplate.language.StringTemplateAST expr_AST = null;
		
		try {      // for error handling
			atom();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(currentAST, (AST)returnAST);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==PLUS))
					{
						antlr.stringtemplate.language.StringTemplateAST tmp9_AST = null;
						tmp9_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(LT(1));
						astFactory.makeASTRoot(currentAST, (AST)tmp9_AST);
						match(PLUS);
						atom();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(currentAST, (AST)returnAST);
						}
					}
					else
					{
						goto _loop8_breakloop;
					}
					
				}
_loop8_breakloop:				;
			}    // ( ... )*
			expr_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_3_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = expr_AST;
		ASTPair.PutInstance(currentAST);
	}
	
	public void ifAtom() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		antlr.stringtemplate.language.StringTemplateAST ifAtom_AST = null;
		
		try {      // for error handling
			expr();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(currentAST, (AST)returnAST);
			}
			ifAtom_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_2_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = ifAtom_AST;
		ASTPair.PutInstance(currentAST);
	}
	
	public void atom() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		antlr.stringtemplate.language.StringTemplateAST atom_AST = null;
		IToken  eval = null;
		antlr.stringtemplate.language.StringTemplateAST eval_AST = null;
		
		try {      // for error handling
			if ((LA(1)==ID||LA(1)==STRING||LA(1)==INT) && (tokenSet_4_.member(LA(2))))
			{
				attribute();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(currentAST, (AST)returnAST);
				}
				atom_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
			}
			else {
				bool synPredMatched11 = false;
				if (((LA(1)==LPAREN||LA(1)==ID||LA(1)==LITERAL_super) && (tokenSet_5_.member(LA(2)))))
				{
					int _m11 = mark();
					synPredMatched11 = true;
					inputState.guessing++;
					try {
						{
							templateInclude();
						}
					}
					catch (RecognitionException)
					{
						synPredMatched11 = false;
					}
					rewind(_m11);
					inputState.guessing--;
				}
				if ( synPredMatched11 )
				{
					templateInclude();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(currentAST, (AST)returnAST);
					}
					atom_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
				}
				else if ((LA(1)==LPAREN) && (tokenSet_6_.member(LA(2)))) {
					eval = LT(1);
					eval_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(eval);
					astFactory.makeASTRoot(currentAST, (AST)eval_AST);
					match(LPAREN);
					templatesExpr();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(currentAST, (AST)returnAST);
					}
					match(RPAREN);
					if (0==inputState.guessing)
					{
						eval_AST.setType(VALUE);
					}
					atom_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
				}
				else
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				}
			}
			catch (RecognitionException ex)
			{
				if (0 == inputState.guessing)
				{
					reportError(ex);
					recover(ex,tokenSet_7_);
				}
				else
				{
					throw ex;
				}
			}
			returnAST = atom_AST;
			ASTPair.PutInstance(currentAST);
		}
		
	public void attribute() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		antlr.stringtemplate.language.StringTemplateAST attribute_AST = null;
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case STRING:
			{
				antlr.stringtemplate.language.StringTemplateAST tmp11_AST = null;
				tmp11_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(LT(1));
				astFactory.addASTChild(currentAST, (AST)tmp11_AST);
				match(STRING);
				attribute_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
				break;
			}
			case INT:
			{
				antlr.stringtemplate.language.StringTemplateAST tmp12_AST = null;
				tmp12_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(LT(1));
				astFactory.addASTChild(currentAST, (AST)tmp12_AST);
				match(INT);
				attribute_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
				break;
			}
			default:
				if ((LA(1)==ID) && (tokenSet_7_.member(LA(2))))
				{
					antlr.stringtemplate.language.StringTemplateAST tmp13_AST = null;
					tmp13_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(LT(1));
					astFactory.addASTChild(currentAST, (AST)tmp13_AST);
					match(ID);
					attribute_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
				}
				else if ((LA(1)==ID) && (LA(2)==DOT)) {
					objPropertyRef();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(currentAST, (AST)returnAST);
					}
					attribute_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
				}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			break; }
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_7_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = attribute_AST;
		ASTPair.PutInstance(currentAST);
	}
	
	public void templateInclude() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		antlr.stringtemplate.language.StringTemplateAST templateInclude_AST = null;
		IToken  qid = null;
		antlr.stringtemplate.language.StringTemplateAST qid_AST = null;
		
		try {      // for error handling
			{
				switch ( LA(1) )
				{
				case ID:
				{
					antlr.stringtemplate.language.StringTemplateAST tmp14_AST = null;
					tmp14_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(LT(1));
					astFactory.addASTChild(currentAST, (AST)tmp14_AST);
					match(ID);
					argList();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(currentAST, (AST)returnAST);
					}
					break;
				}
				case LITERAL_super:
				{
					match(LITERAL_super);
					match(DOT);
					qid = LT(1);
					qid_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(qid);
					astFactory.addASTChild(currentAST, (AST)qid_AST);
					match(ID);
					if (0==inputState.guessing)
					{
						qid_AST.setText("super."+qid_AST.getText());
					}
					argList();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(currentAST, (AST)returnAST);
					}
					break;
				}
				case LPAREN:
				{
					indirectTemplate();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(currentAST, (AST)returnAST);
					}
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			if (0==inputState.guessing)
			{
				templateInclude_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
				templateInclude_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.make((AST)(antlr.stringtemplate.language.StringTemplateAST) astFactory.create(INCLUDE,"include"), (AST)templateInclude_AST);
				currentAST.root = templateInclude_AST;
				if ( (null != templateInclude_AST) && (null != templateInclude_AST.getFirstChild()) )
					currentAST.child = templateInclude_AST.getFirstChild();
				else
					currentAST.child = templateInclude_AST;
				currentAST.advanceChildToEnd();
			}
			templateInclude_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_7_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = templateInclude_AST;
		ASTPair.PutInstance(currentAST);
	}
	
	public void template() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		antlr.stringtemplate.language.StringTemplateAST template_AST = null;
		
		try {      // for error handling
			{
				switch ( LA(1) )
				{
				case LPAREN:
				case ID:
				case LITERAL_super:
				{
					namedTemplate();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(currentAST, (AST)returnAST);
					}
					break;
				}
				case ANONYMOUS_TEMPLATE:
				{
					anonymousTemplate();
					if (0 == inputState.guessing)
					{
						astFactory.addASTChild(currentAST, (AST)returnAST);
					}
					break;
				}
				default:
				{
					throw new NoViableAltException(LT(1), getFilename());
				}
				 }
			}
			if (0==inputState.guessing)
			{
				template_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
				template_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.make((AST)(antlr.stringtemplate.language.StringTemplateAST) astFactory.create(TEMPLATE), (AST)template_AST);
				currentAST.root = template_AST;
				if ( (null != template_AST) && (null != template_AST.getFirstChild()) )
					currentAST.child = template_AST.getFirstChild();
				else
					currentAST.child = template_AST;
				currentAST.advanceChildToEnd();
			}
			template_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_3_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = template_AST;
		ASTPair.PutInstance(currentAST);
	}
	
	public void nonAlternatingTemplateExpr() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		antlr.stringtemplate.language.StringTemplateAST nonAlternatingTemplateExpr_AST = null;
		IToken  c = null;
		antlr.stringtemplate.language.StringTemplateAST c_AST = null;
		
		try {      // for error handling
			expr();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(currentAST, (AST)returnAST);
			}
			{    // ( ... )*
				for (;;)
				{
					if ((LA(1)==COLON))
					{
						c = LT(1);
						c_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(c);
						astFactory.makeASTRoot(currentAST, (AST)c_AST);
						match(COLON);
						if (0==inputState.guessing)
						{
							c_AST.setType(APPLY);
						}
						template();
						if (0 == inputState.guessing)
						{
							astFactory.addASTChild(currentAST, (AST)returnAST);
						}
					}
					else
					{
						goto _loop19_breakloop;
					}
					
				}
_loop19_breakloop:				;
			}    // ( ... )*
			nonAlternatingTemplateExpr_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_8_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = nonAlternatingTemplateExpr_AST;
		ASTPair.PutInstance(currentAST);
	}
	
	public void namedTemplate() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		antlr.stringtemplate.language.StringTemplateAST namedTemplate_AST = null;
		IToken  qid = null;
		antlr.stringtemplate.language.StringTemplateAST qid_AST = null;
		
		try {      // for error handling
			switch ( LA(1) )
			{
			case ID:
			{
				antlr.stringtemplate.language.StringTemplateAST tmp17_AST = null;
				tmp17_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(LT(1));
				astFactory.addASTChild(currentAST, (AST)tmp17_AST);
				match(ID);
				argList();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(currentAST, (AST)returnAST);
				}
				namedTemplate_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
				break;
			}
			case LITERAL_super:
			{
				match(LITERAL_super);
				match(DOT);
				qid = LT(1);
				qid_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(qid);
				astFactory.addASTChild(currentAST, (AST)qid_AST);
				match(ID);
				if (0==inputState.guessing)
				{
					qid_AST.setText("super."+qid_AST.getText());
				}
				argList();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(currentAST, (AST)returnAST);
				}
				namedTemplate_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
				break;
			}
			case LPAREN:
			{
				indirectTemplate();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(currentAST, (AST)returnAST);
				}
				namedTemplate_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
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
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_3_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = namedTemplate_AST;
		ASTPair.PutInstance(currentAST);
	}
	
	public void anonymousTemplate() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		antlr.stringtemplate.language.StringTemplateAST anonymousTemplate_AST = null;
		IToken  t = null;
		antlr.stringtemplate.language.StringTemplateAST t_AST = null;
		
		try {      // for error handling
			t = LT(1);
			t_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(t);
			astFactory.addASTChild(currentAST, (AST)t_AST);
			match(ANONYMOUS_TEMPLATE);
			if (0==inputState.guessing)
			{
				
				StringTemplate anonymous = new StringTemplate();
				anonymous.setGroup(self.getGroup());
				anonymous.setEnclosingInstance(self);
				anonymous.setTemplate(t.getText());
				t_AST.setStringTemplate(anonymous);
				
			}
			anonymousTemplate_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_3_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = anonymousTemplate_AST;
		ASTPair.PutInstance(currentAST);
	}
	
	public void argList() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		antlr.stringtemplate.language.StringTemplateAST argList_AST = null;
		
		try {      // for error handling
			if ((LA(1)==LPAREN) && (LA(2)==RPAREN))
			{
				match(LPAREN);
				match(RPAREN);
				if (0==inputState.guessing)
				{
					argList_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
					argList_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(ARGS,"ARGS");
					currentAST.root = argList_AST;
					if ( (null != argList_AST) && (null != argList_AST.getFirstChild()) )
						currentAST.child = argList_AST.getFirstChild();
					else
						currentAST.child = argList_AST;
					currentAST.advanceChildToEnd();
				}
			}
			else if ((LA(1)==LPAREN) && (LA(2)==ID)) {
				match(LPAREN);
				argumentAssignment();
				if (0 == inputState.guessing)
				{
					astFactory.addASTChild(currentAST, (AST)returnAST);
				}
				{    // ( ... )*
					for (;;)
					{
						if ((LA(1)==COMMA))
						{
							match(COMMA);
							argumentAssignment();
							if (0 == inputState.guessing)
							{
								astFactory.addASTChild(currentAST, (AST)returnAST);
							}
						}
						else
						{
							goto _loop33_breakloop;
						}
						
					}
_loop33_breakloop:					;
				}    // ( ... )*
				match(RPAREN);
				if (0==inputState.guessing)
				{
					argList_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
					argList_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.make((AST)(antlr.stringtemplate.language.StringTemplateAST) astFactory.create(ARGS,"ARGS"), (AST)argList_AST);
					currentAST.root = argList_AST;
					if ( (null != argList_AST) && (null != argList_AST.getFirstChild()) )
						currentAST.child = argList_AST.getFirstChild();
					else
						currentAST.child = argList_AST;
					currentAST.advanceChildToEnd();
				}
				argList_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
			}
			else
			{
				throw new NoViableAltException(LT(1), getFilename());
			}
			
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_7_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = argList_AST;
		ASTPair.PutInstance(currentAST);
	}
	
/** Match (foo)() and (foo+".terse")()
    breaks encapsulation
 */
	public void indirectTemplate() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		antlr.stringtemplate.language.StringTemplateAST indirectTemplate_AST = null;
		antlr.stringtemplate.language.StringTemplateAST e_AST = null;
		antlr.stringtemplate.language.StringTemplateAST args_AST = null;
		
		try {      // for error handling
			antlr.stringtemplate.language.StringTemplateAST tmp25_AST = null;
			tmp25_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(LT(1));
			match(LPAREN);
			expr();
			if (0 == inputState.guessing)
			{
				e_AST = (antlr.stringtemplate.language.StringTemplateAST)returnAST;
			}
			antlr.stringtemplate.language.StringTemplateAST tmp26_AST = null;
			tmp26_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(LT(1));
			match(RPAREN);
			argList();
			if (0 == inputState.guessing)
			{
				args_AST = (antlr.stringtemplate.language.StringTemplateAST)returnAST;
			}
			if (0==inputState.guessing)
			{
				indirectTemplate_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
				indirectTemplate_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.make((AST)(antlr.stringtemplate.language.StringTemplateAST) astFactory.create(VALUE,"value"), (AST)e_AST, (AST)args_AST);
				currentAST.root = indirectTemplate_AST;
				if ( (null != indirectTemplate_AST) && (null != indirectTemplate_AST.getFirstChild()) )
					currentAST.child = indirectTemplate_AST.getFirstChild();
				else
					currentAST.child = indirectTemplate_AST;
				currentAST.advanceChildToEnd();
			}
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_7_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = indirectTemplate_AST;
		ASTPair.PutInstance(currentAST);
	}
	
	public void objPropertyRef() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		antlr.stringtemplate.language.StringTemplateAST objPropertyRef_AST = null;
		
		try {      // for error handling
			antlr.stringtemplate.language.StringTemplateAST tmp27_AST = null;
			tmp27_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(LT(1));
			astFactory.addASTChild(currentAST, (AST)tmp27_AST);
			match(ID);
			{ // ( ... )+
				int _cnt30=0;
				for (;;)
				{
					if ((LA(1)==DOT))
					{
						antlr.stringtemplate.language.StringTemplateAST tmp28_AST = null;
						tmp28_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(LT(1));
						astFactory.makeASTRoot(currentAST, (AST)tmp28_AST);
						match(DOT);
						antlr.stringtemplate.language.StringTemplateAST tmp29_AST = null;
						tmp29_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(LT(1));
						astFactory.addASTChild(currentAST, (AST)tmp29_AST);
						match(ID);
					}
					else
					{
						if (_cnt30 >= 1) { goto _loop30_breakloop; } else { throw new NoViableAltException(LT(1), getFilename());; }
					}
					
					_cnt30++;
				}
_loop30_breakloop:				;
			}    // ( ... )+
			objPropertyRef_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_7_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = objPropertyRef_AST;
		ASTPair.PutInstance(currentAST);
	}
	
	public void argumentAssignment() //throws RecognitionException, TokenStreamException
{
		
		returnAST = null;
		ASTPair currentAST = ASTPair.GetInstance();
		antlr.stringtemplate.language.StringTemplateAST argumentAssignment_AST = null;
		
		try {      // for error handling
			antlr.stringtemplate.language.StringTemplateAST tmp30_AST = null;
			tmp30_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(LT(1));
			astFactory.addASTChild(currentAST, (AST)tmp30_AST);
			match(ID);
			antlr.stringtemplate.language.StringTemplateAST tmp31_AST = null;
			tmp31_AST = (antlr.stringtemplate.language.StringTemplateAST) astFactory.create(LT(1));
			astFactory.makeASTRoot(currentAST, (AST)tmp31_AST);
			match(ASSIGN);
			nonAlternatingTemplateExpr();
			if (0 == inputState.guessing)
			{
				astFactory.addASTChild(currentAST, (AST)returnAST);
			}
			argumentAssignment_AST = (antlr.stringtemplate.language.StringTemplateAST)currentAST.root;
		}
		catch (RecognitionException ex)
		{
			if (0 == inputState.guessing)
			{
				reportError(ex);
				recover(ex,tokenSet_8_);
			}
			else
			{
				throw ex;
			}
		}
		returnAST = argumentAssignment_AST;
		ASTPair.PutInstance(currentAST);
	}
	
	public new antlr.stringtemplate.language.StringTemplateAST getAST()
	{
		return (antlr.stringtemplate.language.StringTemplateAST) returnAST;
	}
	
	private void initializeFactory()
	{
		if (astFactory == null)
		{
			astFactory = new ASTFactory("antlr.stringtemplate.language.StringTemplateAST");
		}
		initializeASTFactory( astFactory );
	}
	static public void initializeASTFactory( ASTFactory factory )
	{
		factory.setMaxNodeType(27);
	}
	
	public static readonly string[] tokenNames_ = new string[] {
		@"""<0>""",
		@"""EOF""",
		@"""<2>""",
		@"""NULL_TREE_LOOKAHEAD""",
		@"""APPLY""",
		@"""ARGS""",
		@"""INCLUDE""",
		@"""if""",
		@"""VALUE""",
		@"""TEMPLATE""",
		@"""SEMI""",
		@"""LPAREN""",
		@"""RPAREN""",
		@"""separator""",
		@"""ASSIGN""",
		@"""NOT""",
		@"""PLUS""",
		@"""COLON""",
		@"""COMMA""",
		@"""ID""",
		@"""super""",
		@"""DOT""",
		@"""ANONYMOUS_TEMPLATE""",
		@"""STRING""",
		@"""INT""",
		@"""NESTED_ANONYMOUS_TEMPLATE""",
		@"""ESC_CHAR""",
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
		long[] data = { 5122L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_1_ = new BitSet(mk_tokenSet_1_());
	private static long[] mk_tokenSet_2_()
	{
		long[] data = { 4096L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_2_ = new BitSet(mk_tokenSet_2_());
	private static long[] mk_tokenSet_3_()
	{
		long[] data = { 398338L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_3_ = new BitSet(mk_tokenSet_3_());
	private static long[] mk_tokenSet_4_()
	{
		long[] data = { 2561026L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_4_ = new BitSet(mk_tokenSet_4_());
	private static long[] mk_tokenSet_5_()
	{
		long[] data = { 28837888L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_5_ = new BitSet(mk_tokenSet_5_());
	private static long[] mk_tokenSet_6_()
	{
		long[] data = { 26740736L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_6_ = new BitSet(mk_tokenSet_6_());
	private static long[] mk_tokenSet_7_()
	{
		long[] data = { 463874L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_7_ = new BitSet(mk_tokenSet_7_());
	private static long[] mk_tokenSet_8_()
	{
		long[] data = { 266240L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_8_ = new BitSet(mk_tokenSet_8_());
	
}
}
