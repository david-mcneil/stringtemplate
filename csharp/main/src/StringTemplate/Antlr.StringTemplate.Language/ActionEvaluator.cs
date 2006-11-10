// $ANTLR 2.7.6 (2005-12-22): "eval.g" -> "ActionEvaluator.cs"$

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
using Antlr.StringTemplate;
using Antlr.StringTemplate.Collections;
using System.Collections;
using System.IO;
using System.Reflection;

namespace Antlr.StringTemplate.Language
{
	// Generate header specific to the tree-parser CSharp file
	using System;
	
	using TreeParser = antlr.TreeParser;
	using Token                    = antlr.Token;
	using IToken                   = antlr.IToken;
	using AST                      = antlr.collections.AST;
	using RecognitionException     = antlr.RecognitionException;
	using ANTLRException           = antlr.ANTLRException;
	using NoViableAltException     = antlr.NoViableAltException;
	using MismatchedTokenException = antlr.MismatchedTokenException;
	using SemanticException        = antlr.SemanticException;
	using BitSet                   = antlr.collections.impl.BitSet;
	using ASTPair                  = antlr.ASTPair;
	using ASTFactory               = antlr.ASTFactory;
	using ASTArray                 = antlr.collections.impl.ASTArray;
	
	
	public 	class ActionEvaluator : antlr.TreeParser
	{
		public const int EOF = 1;
		public const int NULL_TREE_LOOKAHEAD = 3;
		public const int APPLY = 4;
		public const int MULTI_APPLY = 5;
		public const int ARGS = 6;
		public const int INCLUDE = 7;
		public const int CONDITIONAL = 8;
		public const int VALUE = 9;
		public const int TEMPLATE = 10;
		public const int FUNCTION = 11;
		public const int SINGLEVALUEARG = 12;
		public const int LIST = 13;
		public const int SEMI = 14;
		public const int LPAREN = 15;
		public const int RPAREN = 16;
		public const int COMMA = 17;
		public const int ID = 18;
		public const int ASSIGN = 19;
		public const int COLON = 20;
		public const int NOT = 21;
		public const int PLUS = 22;
		public const int LITERAL_super = 23;
		public const int DOT = 24;
		public const int LITERAL_first = 25;
		public const int LITERAL_rest = 26;
		public const int LITERAL_last = 27;
		public const int LITERAL_length = 28;
		public const int LITERAL_strip = 29;
		public const int LITERAL_trunc = 30;
		public const int ANONYMOUS_TEMPLATE = 31;
		public const int STRING = 32;
		public const int INT = 33;
		public const int LBRACK = 34;
		public const int RBRACK = 35;
		public const int DOTDOTDOT = 36;
		public const int TEMPLATE_ARGS = 37;
		public const int NESTED_ANONYMOUS_TEMPLATE = 38;
		public const int ESC_CHAR = 39;
		public const int WS = 40;
		public const int WS_CHAR = 41;
		
		
    public class NameValuePair {
        public String name;
        public object value;
    };

    protected StringTemplate self = null;
    protected IStringTemplateWriter @out = null;
    protected ASTExpr chunk = null;

    /** Create an evaluator using attributes from self */
    public ActionEvaluator(StringTemplate self, ASTExpr chunk, IStringTemplateWriter @out) {
        this.self = self;
        this.chunk = chunk;
        this.@out = @out;
    }
 
	override public void reportError(RecognitionException e) {
		self.Error("eval tree parse error", e);
	}
		public ActionEvaluator()
		{
			tokenNames = tokenNames_;
		}
		
	public int  action(AST _t) //throws RecognitionException
{
		int numCharsWritten=0;
		
		Antlr.StringTemplate.Language.StringTemplateAST action_AST_in = (Antlr.StringTemplate.Language.StringTemplateAST)_t;
		
		object e=null;
		
		
		try {      // for error handling
			e=expr(_t);
			_t = retTree_;
			numCharsWritten = chunk.WriteAttribute(self,e,@out);
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			if (null != _t)
			{
				_t = _t.getNextSibling();
			}
		}
		retTree_ = _t;
		return numCharsWritten;
	}
	
	public object  expr(AST _t) //throws RecognitionException
{
		object value=null;
		
		Antlr.StringTemplate.Language.StringTemplateAST expr_AST_in = (Antlr.StringTemplate.Language.StringTemplateAST)_t;
		
		object a=null, b=null, e=null;
		
		
		try {      // for error handling
			if (null == _t)
				_t = ASTNULL;
			switch ( _t.Type )
			{
			case PLUS:
			{
				AST __t3 = _t;
				Antlr.StringTemplate.Language.StringTemplateAST tmp1_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
				match((AST)_t,PLUS);
				_t = _t.getFirstChild();
				a=expr(_t);
				_t = retTree_;
				b=expr(_t);
				_t = retTree_;
				value = chunk.Add(a,b);
				_t = __t3;
				_t = _t.getNextSibling();
				break;
			}
			case APPLY:
			case MULTI_APPLY:
			{
				value=templateApplication(_t);
				_t = retTree_;
				break;
			}
			case ID:
			case DOT:
			case ANONYMOUS_TEMPLATE:
			case STRING:
			case INT:
			{
				value=attribute(_t);
				_t = retTree_;
				break;
			}
			case INCLUDE:
			{
				value=templateInclude(_t);
				_t = retTree_;
				break;
			}
			case FUNCTION:
			{
				value=function(_t);
				_t = retTree_;
				break;
			}
			case LIST:
			{
				value=list(_t);
				_t = retTree_;
				break;
			}
			case VALUE:
			{
				AST __t4 = _t;
				Antlr.StringTemplate.Language.StringTemplateAST tmp2_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
				match((AST)_t,VALUE);
				_t = _t.getFirstChild();
				e=expr(_t);
				_t = retTree_;
				_t = __t4;
				_t = _t.getNextSibling();
				
					        StringWriter buf = new StringWriter();
					        IStringTemplateWriter sw = self.Group.CreateInstanceOfTemplateWriter(buf);
					        int n = chunk.WriteAttribute(self,e,sw);
							if (n > 0)
							{
					        	value = buf.ToString();
					
					        }
						
				break;
			}
			default:
			{
				throw new NoViableAltException(_t);
			}
			 }
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			if (null != _t)
			{
				_t = _t.getNextSibling();
			}
		}
		retTree_ = _t;
		return value;
	}
	
/** Apply template(s) to an attribute; can be applied to another apply
 *  result.
 */
	public object  templateApplication(AST _t) //throws RecognitionException
{
		object value=null;
		
		Antlr.StringTemplate.Language.StringTemplateAST templateApplication_AST_in = (Antlr.StringTemplate.Language.StringTemplateAST)_t;
		Antlr.StringTemplate.Language.StringTemplateAST anon = null;
		
		object a=null;
		ArrayList templatesToApply=new ArrayList();
		ArrayList attributes=new ArrayList();
		
		
		try {      // for error handling
			if (null == _t)
				_t = ASTNULL;
			switch ( _t.Type )
			{
			case APPLY:
			{
				AST __t14 = _t;
				Antlr.StringTemplate.Language.StringTemplateAST tmp3_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
				match((AST)_t,APPLY);
				_t = _t.getFirstChild();
				a=expr(_t);
				_t = retTree_;
				{ // ( ... )+
					int _cnt16=0;
					for (;;)
					{
						if (_t == null)
							_t = ASTNULL;
						if ((_t.Type==TEMPLATE))
						{
							template(_t,templatesToApply);
							_t = retTree_;
						}
						else
						{
							if (_cnt16 >= 1) { goto _loop16_breakloop; } else { throw new NoViableAltException(_t);; }
						}
						
						_cnt16++;
					}
_loop16_breakloop:					;
				}    // ( ... )+
				value = chunk.ApplyListOfAlternatingTemplates(self,a,templatesToApply);
				_t = __t14;
				_t = _t.getNextSibling();
				break;
			}
			case MULTI_APPLY:
			{
				AST __t17 = _t;
				Antlr.StringTemplate.Language.StringTemplateAST tmp4_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
				match((AST)_t,MULTI_APPLY);
				_t = _t.getFirstChild();
				{ // ( ... )+
					int _cnt19=0;
					for (;;)
					{
						if (_t == null)
							_t = ASTNULL;
						if ((tokenSet_0_.member(_t.Type)))
						{
							a=expr(_t);
							_t = retTree_;
							attributes.Add(a);
						}
						else
						{
							if (_cnt19 >= 1) { goto _loop19_breakloop; } else { throw new NoViableAltException(_t);; }
						}
						
						_cnt19++;
					}
_loop19_breakloop:					;
				}    // ( ... )+
				Antlr.StringTemplate.Language.StringTemplateAST tmp5_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
				match((AST)_t,COLON);
				_t = _t.getNextSibling();
				anon = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
				match((AST)_t,ANONYMOUS_TEMPLATE);
				_t = _t.getNextSibling();
				
							StringTemplate anonymous = anon.StringTemplate;
							templatesToApply.Add(anonymous);
							value = chunk.ApplyTemplateToListOfAttributes(self,
																		  attributes,
																		  anon.StringTemplate);
							
				_t = __t17;
				_t = _t.getNextSibling();
				break;
			}
			default:
			{
				throw new NoViableAltException(_t);
			}
			 }
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			if (null != _t)
			{
				_t = _t.getNextSibling();
			}
		}
		retTree_ = _t;
		return value;
	}
	
	public object  attribute(AST _t) //throws RecognitionException
{
		object value=null;
		
		Antlr.StringTemplate.Language.StringTemplateAST attribute_AST_in = (Antlr.StringTemplate.Language.StringTemplateAST)_t;
		Antlr.StringTemplate.Language.StringTemplateAST prop = null;
		Antlr.StringTemplate.Language.StringTemplateAST i3 = null;
		Antlr.StringTemplate.Language.StringTemplateAST i = null;
		Antlr.StringTemplate.Language.StringTemplateAST s = null;
		Antlr.StringTemplate.Language.StringTemplateAST at = null;
		
		object obj = null;
		string propName = null;
		object e = null;
		
		
		try {      // for error handling
			if (null == _t)
				_t = ASTNULL;
			switch ( _t.Type )
			{
			case DOT:
			{
				AST __t32 = _t;
				Antlr.StringTemplate.Language.StringTemplateAST tmp6_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
				match((AST)_t,DOT);
				_t = _t.getFirstChild();
				obj=expr(_t);
				_t = retTree_;
				{
					if (null == _t)
						_t = ASTNULL;
					switch ( _t.Type )
					{
					case ID:
					{
						prop = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
						match((AST)_t,ID);
						_t = _t.getNextSibling();
						propName = prop.getText();
						break;
					}
					case VALUE:
					{
						AST __t34 = _t;
						Antlr.StringTemplate.Language.StringTemplateAST tmp7_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
						match((AST)_t,VALUE);
						_t = _t.getFirstChild();
						e=expr(_t);
						_t = retTree_;
						_t = __t34;
						_t = _t.getNextSibling();
						if (e!=null) {propName=e.ToString();}
						break;
					}
					default:
					{
						throw new NoViableAltException(_t);
					}
					 }
				}
				_t = __t32;
				_t = _t.getNextSibling();
				value = chunk.GetObjectProperty(self,obj,propName);
				break;
			}
			case ID:
			{
				i3 = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
				match((AST)_t,ID);
				_t = _t.getNextSibling();
				
				value=self.GetAttribute(i3.getText());
				
				break;
			}
			case INT:
			{
				i = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
				match((AST)_t,INT);
				_t = _t.getNextSibling();
				value=Int32.Parse(i.getText());
				break;
			}
			case STRING:
			{
				s = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
				match((AST)_t,STRING);
				_t = _t.getNextSibling();
				
					value=s.getText();
					
				break;
			}
			case ANONYMOUS_TEMPLATE:
			{
				at = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
				match((AST)_t,ANONYMOUS_TEMPLATE);
				_t = _t.getNextSibling();
				
					value=at.getText();
						if ( at.getText()!=null ) {
							StringTemplate valueST =new StringTemplate(self.Group, at.getText());
							valueST.EnclosingInstance = self;
							valueST.Name = "<anonymous template argument>";
							value = valueST;
					}
					
				break;
			}
			default:
			{
				throw new NoViableAltException(_t);
			}
			 }
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			if (null != _t)
			{
				_t = _t.getNextSibling();
			}
		}
		retTree_ = _t;
		return value;
	}
	
	public object  templateInclude(AST _t) //throws RecognitionException
{
		object value=null;
		
		Antlr.StringTemplate.Language.StringTemplateAST templateInclude_AST_in = (Antlr.StringTemplate.Language.StringTemplateAST)_t;
		Antlr.StringTemplate.Language.StringTemplateAST id = null;
		Antlr.StringTemplate.Language.StringTemplateAST a1 = null;
		Antlr.StringTemplate.Language.StringTemplateAST a2 = null;
		
		StringTemplateAST args = null;
		string name = null;
		object n = null;
		
		
		try {      // for error handling
			AST __t10 = _t;
			Antlr.StringTemplate.Language.StringTemplateAST tmp8_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
			match((AST)_t,INCLUDE);
			_t = _t.getFirstChild();
			{
				if (null == _t)
					_t = ASTNULL;
				switch ( _t.Type )
				{
				case ID:
				{
					id = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
					match((AST)_t,ID);
					_t = _t.getNextSibling();
					a1 = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
					if (null == _t) throw new MismatchedTokenException();
					_t = _t.getNextSibling();
					name=id.getText(); args=a1;
					break;
				}
				case VALUE:
				{
					AST __t12 = _t;
					Antlr.StringTemplate.Language.StringTemplateAST tmp9_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
					match((AST)_t,VALUE);
					_t = _t.getFirstChild();
					n=expr(_t);
					_t = retTree_;
					a2 = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
					if (null == _t) throw new MismatchedTokenException();
					_t = _t.getNextSibling();
					_t = __t12;
					_t = _t.getNextSibling();
					if (n!=null) {name=n.ToString();} args=a2;
					break;
				}
				default:
				{
					throw new NoViableAltException(_t);
				}
				 }
			}
			_t = __t10;
			_t = _t.getNextSibling();
			
			if ( name!=null ) {
				value = chunk.GetTemplateInclude(self, name, args);
			}
			
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			if (null != _t)
			{
				_t = _t.getNextSibling();
			}
		}
		retTree_ = _t;
		return value;
	}
	
	public object  function(AST _t) //throws RecognitionException
{
		object value=null;
		
		Antlr.StringTemplate.Language.StringTemplateAST function_AST_in = (Antlr.StringTemplate.Language.StringTemplateAST)_t;
		
		object a;
		
		
		try {      // for error handling
			AST __t21 = _t;
			Antlr.StringTemplate.Language.StringTemplateAST tmp10_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
			match((AST)_t,FUNCTION);
			_t = _t.getFirstChild();
			{
				if (null == _t)
					_t = ASTNULL;
				switch ( _t.Type )
				{
				case LITERAL_first:
				{
					Antlr.StringTemplate.Language.StringTemplateAST tmp11_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
					match((AST)_t,LITERAL_first);
					_t = _t.getNextSibling();
					a=singleFunctionArg(_t);
					_t = retTree_;
					value=chunk.First(a);
					break;
				}
				case LITERAL_rest:
				{
					Antlr.StringTemplate.Language.StringTemplateAST tmp12_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
					match((AST)_t,LITERAL_rest);
					_t = _t.getNextSibling();
					a=singleFunctionArg(_t);
					_t = retTree_;
					value=chunk.Rest(a);
					break;
				}
				case LITERAL_last:
				{
					Antlr.StringTemplate.Language.StringTemplateAST tmp13_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
					match((AST)_t,LITERAL_last);
					_t = _t.getNextSibling();
					a=singleFunctionArg(_t);
					_t = retTree_;
					value=chunk.Last(a);
					break;
				}
				case LITERAL_length:
				{
					Antlr.StringTemplate.Language.StringTemplateAST tmp14_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
					match((AST)_t,LITERAL_length);
					_t = _t.getNextSibling();
					a=singleFunctionArg(_t);
					_t = retTree_;
					value=chunk.Length(a);
					break;
				}
				case LITERAL_strip:
				{
					Antlr.StringTemplate.Language.StringTemplateAST tmp15_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
					match((AST)_t,LITERAL_strip);
					_t = _t.getNextSibling();
					a=singleFunctionArg(_t);
					_t = retTree_;
					value=chunk.Strip(a);
					break;
				}
				case LITERAL_trunc:
				{
					Antlr.StringTemplate.Language.StringTemplateAST tmp16_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
					match((AST)_t,LITERAL_trunc);
					_t = _t.getNextSibling();
					a=singleFunctionArg(_t);
					_t = retTree_;
					value=chunk.Trunc(a);
					break;
				}
				default:
				{
					throw new NoViableAltException(_t);
				}
				 }
			}
			_t = __t21;
			_t = _t.getNextSibling();
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			if (null != _t)
			{
				_t = _t.getNextSibling();
			}
		}
		retTree_ = _t;
		return value;
	}
	
/** create a new list of expressions as a new multi-value attribute */
	public object  list(AST _t) //throws RecognitionException
{
		object value=null;
		
		Antlr.StringTemplate.Language.StringTemplateAST list_AST_in = (Antlr.StringTemplate.Language.StringTemplateAST)_t;
		
		object e = null;
		IList elements = new ArrayList();
		value = new CatIterator(elements);
		
		
		try {      // for error handling
			AST __t6 = _t;
			Antlr.StringTemplate.Language.StringTemplateAST tmp17_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
			match((AST)_t,LIST);
			_t = _t.getFirstChild();
			{ // ( ... )+
				int _cnt8=0;
				for (;;)
				{
					if (_t == null)
						_t = ASTNULL;
					if ((tokenSet_0_.member(_t.Type)))
					{
						e=expr(_t);
						_t = retTree_;
						
									  	if ( e!=null ) {
											e = ASTExpr.ConvertAnythingToIterator(e);
									  		elements.Add(e);
									  	}
									  	
					}
					else
					{
						if (_cnt8 >= 1) { goto _loop8_breakloop; } else { throw new NoViableAltException(_t);; }
					}
					
					_cnt8++;
				}
_loop8_breakloop:				;
			}    // ( ... )+
			_t = __t6;
			_t = _t.getNextSibling();
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			if (null != _t)
			{
				_t = _t.getNextSibling();
			}
		}
		retTree_ = _t;
		return value;
	}
	
	public void template(AST _t,
		ArrayList templatesToApply
	) //throws RecognitionException
{
		
		Antlr.StringTemplate.Language.StringTemplateAST template_AST_in = (Antlr.StringTemplate.Language.StringTemplateAST)_t;
		Antlr.StringTemplate.Language.StringTemplateAST t = null;
		Antlr.StringTemplate.Language.StringTemplateAST args = null;
		Antlr.StringTemplate.Language.StringTemplateAST anon = null;
		Antlr.StringTemplate.Language.StringTemplateAST args2 = null;
		
		object n = null;
		
		
		try {      // for error handling
			AST __t26 = _t;
			Antlr.StringTemplate.Language.StringTemplateAST tmp18_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
			match((AST)_t,TEMPLATE);
			_t = _t.getFirstChild();
			{
				if (null == _t)
					_t = ASTNULL;
				switch ( _t.Type )
				{
				case ID:
				{
					t = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
					match((AST)_t,ID);
					_t = _t.getNextSibling();
					args = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
					if (null == _t) throw new MismatchedTokenException();
					_t = _t.getNextSibling();
					
					string templateName = t.getText();
					StringTemplateGroup group = self.Group;
					StringTemplate embedded = group.GetEmbeddedInstanceOf(self, templateName);
					if ( embedded!=null ) {
					embedded.ArgumentsAST = args;
					templatesToApply.Add(embedded);
					}
					
					break;
				}
				case ANONYMOUS_TEMPLATE:
				{
					anon = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
					match((AST)_t,ANONYMOUS_TEMPLATE);
					_t = _t.getNextSibling();
					
					StringTemplate anonymous = anon.StringTemplate;
					// to properly see overridden templates, always set
					// anonymous' group to be self's group
									anonymous.Group = self.Group;
					templatesToApply.Add(anonymous);
					
					break;
				}
				case VALUE:
				{
					AST __t28 = _t;
					Antlr.StringTemplate.Language.StringTemplateAST tmp19_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
					match((AST)_t,VALUE);
					_t = _t.getFirstChild();
					n=expr(_t);
					_t = retTree_;
					args2 = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
					if (null == _t) throw new MismatchedTokenException();
					_t = _t.getNextSibling();
					
											StringTemplate embedded = null;
											if ( n!=null ) {
								String templateName = n.ToString();
												StringTemplateGroup group = self.Group;
												embedded = group.GetEmbeddedInstanceOf(self, templateName);
												if ( embedded!=null ) {
													embedded.ArgumentsAST = args2;
													templatesToApply.Add(embedded);
												}
											}
									
					_t = __t28;
					_t = _t.getNextSibling();
					break;
				}
				default:
				{
					throw new NoViableAltException(_t);
				}
				 }
			}
			_t = __t26;
			_t = _t.getNextSibling();
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			if (null != _t)
			{
				_t = _t.getNextSibling();
			}
		}
		retTree_ = _t;
	}
	
	public object  singleFunctionArg(AST _t) //throws RecognitionException
{
		object value=null;
		
		Antlr.StringTemplate.Language.StringTemplateAST singleFunctionArg_AST_in = (Antlr.StringTemplate.Language.StringTemplateAST)_t;
		
		try {      // for error handling
			AST __t24 = _t;
			Antlr.StringTemplate.Language.StringTemplateAST tmp20_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
			match((AST)_t,SINGLEVALUEARG);
			_t = _t.getFirstChild();
			value=expr(_t);
			_t = retTree_;
			_t = __t24;
			_t = _t.getNextSibling();
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			if (null != _t)
			{
				_t = _t.getNextSibling();
			}
		}
		retTree_ = _t;
		return value;
	}
	
	public bool  ifCondition(AST _t) //throws RecognitionException
{
		bool value=false;
		
		Antlr.StringTemplate.Language.StringTemplateAST ifCondition_AST_in = (Antlr.StringTemplate.Language.StringTemplateAST)_t;
		
		object a=null;
		
		
		try {      // for error handling
			if (null == _t)
				_t = ASTNULL;
			switch ( _t.Type )
			{
			case APPLY:
			case MULTI_APPLY:
			case INCLUDE:
			case VALUE:
			case FUNCTION:
			case LIST:
			case ID:
			case PLUS:
			case DOT:
			case ANONYMOUS_TEMPLATE:
			case STRING:
			case INT:
			{
				a=expr(_t);
				_t = retTree_;
				value = chunk.TestAttributeTrue(a);
				break;
			}
			case NOT:
			{
				AST __t30 = _t;
				Antlr.StringTemplate.Language.StringTemplateAST tmp21_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
				match((AST)_t,NOT);
				_t = _t.getFirstChild();
				a=expr(_t);
				_t = retTree_;
				_t = __t30;
				_t = _t.getNextSibling();
				value = !chunk.TestAttributeTrue(a);
				break;
			}
			default:
			{
				throw new NoViableAltException(_t);
			}
			 }
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			if (null != _t)
			{
				_t = _t.getNextSibling();
			}
		}
		retTree_ = _t;
		return value;
	}
	
/** self is assumed to be the enclosing context as foo(x=y) must find y in
 *  the template that encloses the ref to foo(x=y).  We must pass in
 *  the embedded template (the one invoked) so we can check formal args
 *  in RawSetArgumentAttribute.
 */
	public IDictionary  argList(AST _t,
		StringTemplate embedded, IDictionary initialContext
	) //throws RecognitionException
{
		IDictionary argumentContext=null;
		
		Antlr.StringTemplate.Language.StringTemplateAST argList_AST_in = (Antlr.StringTemplate.Language.StringTemplateAST)_t;
		
		argumentContext = initialContext;
		if ( argumentContext==null ) {
		argumentContext=new Hashtable();
		}
		
		
		try {      // for error handling
			if (null == _t)
				_t = ASTNULL;
			switch ( _t.Type )
			{
			case ARGS:
			{
				AST __t36 = _t;
				Antlr.StringTemplate.Language.StringTemplateAST tmp22_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
				match((AST)_t,ARGS);
				_t = _t.getFirstChild();
				{    // ( ... )*
					for (;;)
					{
						if (_t == null)
							_t = ASTNULL;
						if ((_t.Type==ASSIGN||_t.Type==DOTDOTDOT))
						{
							argumentAssignment(_t,embedded,argumentContext);
							_t = retTree_;
						}
						else
						{
							goto _loop38_breakloop;
						}
						
					}
_loop38_breakloop:					;
				}    // ( ... )*
				_t = __t36;
				_t = _t.getNextSibling();
				break;
			}
			case SINGLEVALUEARG:
			{
				singleTemplateArg(_t,embedded,argumentContext);
				_t = retTree_;
				break;
			}
			default:
			{
				throw new NoViableAltException(_t);
			}
			 }
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			if (null != _t)
			{
				_t = _t.getNextSibling();
			}
		}
		retTree_ = _t;
		return argumentContext;
	}
	
	public void argumentAssignment(AST _t,
		StringTemplate embedded, IDictionary argumentContext
	) //throws RecognitionException
{
		
		Antlr.StringTemplate.Language.StringTemplateAST argumentAssignment_AST_in = (Antlr.StringTemplate.Language.StringTemplateAST)_t;
		Antlr.StringTemplate.Language.StringTemplateAST arg = null;
		
		object e = null;
		
		
		try {      // for error handling
			if (null == _t)
				_t = ASTNULL;
			switch ( _t.Type )
			{
			case ASSIGN:
			{
				AST __t42 = _t;
				Antlr.StringTemplate.Language.StringTemplateAST tmp23_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
				match((AST)_t,ASSIGN);
				_t = _t.getFirstChild();
				arg = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
				match((AST)_t,ID);
				_t = _t.getNextSibling();
				e=expr(_t);
				_t = retTree_;
				_t = __t42;
				_t = _t.getNextSibling();
				
					    if ( e!=null ) {
							self.RawSetArgumentAttribute(embedded,argumentContext,arg.getText(),e);
						}
					
				break;
			}
			case DOTDOTDOT:
			{
				Antlr.StringTemplate.Language.StringTemplateAST tmp24_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
				match((AST)_t,DOTDOTDOT);
				_t = _t.getNextSibling();
				embedded.PassThroughAttributes = true;
				break;
			}
			default:
			{
				throw new NoViableAltException(_t);
			}
			 }
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			if (null != _t)
			{
				_t = _t.getNextSibling();
			}
		}
		retTree_ = _t;
	}
	
	public void singleTemplateArg(AST _t,
		StringTemplate embedded, IDictionary argumentContext
	) //throws RecognitionException
{
		
		Antlr.StringTemplate.Language.StringTemplateAST singleTemplateArg_AST_in = (Antlr.StringTemplate.Language.StringTemplateAST)_t;
		
		object e = null;
		
		
		try {      // for error handling
			AST __t40 = _t;
			Antlr.StringTemplate.Language.StringTemplateAST tmp25_AST_in = (_t==ASTNULL) ? null : (Antlr.StringTemplate.Language.StringTemplateAST)_t;
			match((AST)_t,SINGLEVALUEARG);
			_t = _t.getFirstChild();
			e=expr(_t);
			_t = retTree_;
			_t = __t40;
			_t = _t.getNextSibling();
			
				    if ( e!=null ) {
				    	string soleArgName = null;
				    	// find the sole defined formal argument for embedded
				    	bool error = false;
						HashList formalArgs = (HashList) embedded.FormalArguments;
						if ( formalArgs!=null ) {
							ICollection argNames = formalArgs.Keys;
							if ( argNames.Count==1 ) {
								IEnumerator iter = argNames.GetEnumerator();
								iter.MoveNext();
								soleArgName = (string) iter.Current;
								//Console.WriteLine("sole formal arg of "+embedded.Name+" is "+soleArgName);
							}
							else {
								error=true;
							}
						}
						else {
							error=true;
						}
						if ( error ) {
							self.Error("template "+embedded.Name+
							           " must have exactly one formal arg in template context "+
									   self.GetEnclosingInstanceStackString());
					   	}
					   	else {
					   		self.RawSetArgumentAttribute(embedded,argumentContext,soleArgName,e);
					   	}
				    }
				
		}
		catch (RecognitionException ex)
		{
			reportError(ex);
			if (null != _t)
			{
				_t = _t.getNextSibling();
			}
		}
		retTree_ = _t;
	}
	
	public new Antlr.StringTemplate.Language.StringTemplateAST getAST()
	{
		return (Antlr.StringTemplate.Language.StringTemplateAST) returnAST;
	}
	
	
	public static readonly string[] tokenNames_ = new string[] {
		@"""<0>""",
		@"""EOF""",
		@"""<2>""",
		@"""NULL_TREE_LOOKAHEAD""",
		@"""APPLY""",
		@"""MULTI_APPLY""",
		@"""ARGS""",
		@"""INCLUDE""",
		@"""if""",
		@"""VALUE""",
		@"""TEMPLATE""",
		@"""FUNCTION""",
		@"""SINGLEVALUEARG""",
		@"""LIST""",
		@"""SEMI""",
		@"""LPAREN""",
		@"""RPAREN""",
		@"""COMMA""",
		@"""ID""",
		@"""ASSIGN""",
		@"""COLON""",
		@"""NOT""",
		@"""PLUS""",
		@"""super""",
		@"""DOT""",
		@"""first""",
		@"""rest""",
		@"""last""",
		@"""length""",
		@"""strip""",
		@"""trunc""",
		@"""ANONYMOUS_TEMPLATE""",
		@"""STRING""",
		@"""INT""",
		@"""LBRACK""",
		@"""RBRACK""",
		@"""DOTDOTDOT""",
		@"""TEMPLATE_ARGS""",
		@"""NESTED_ANONYMOUS_TEMPLATE""",
		@"""ESC_CHAR""",
		@"""WS""",
		@"""WS_CHAR"""
	};
	
	private static long[] mk_tokenSet_0_()
	{
		long[] data = { 15053630128L, 0L};
		return data;
	}
	public static readonly BitSet tokenSet_0_ = new BitSet(mk_tokenSet_0_());
}

}
