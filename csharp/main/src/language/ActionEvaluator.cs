// $ANTLR 2.7.5rc2 (20050108): "eval.g" -> "ActionEvaluator.cs"$

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
using antlr.stringtemplate;
using System.Collections;
using System.IO;
using System.Reflection;

namespace antlr.stringtemplate.language
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
		
		
    public class NameValuePair {
        public String name;
        public Object value;
    };

    protected StringTemplate self = null;
    protected StringTemplateWriter @out = null;
    protected ASTExpr chunk = null;

    /** Create an evaluator using attributes from self */
    public ActionEvaluator(StringTemplate self, ASTExpr chunk, StringTemplateWriter @out) {
        this.self = self;
        this.chunk = chunk;
        this.@out = @out;
    }
 
	override public void reportError(RecognitionException e) {
		self.error("template parse error", e);
	}
		public ActionEvaluator()
		{
			tokenNames = tokenNames_;
		}
		
	public int  action(AST _t) //throws RecognitionException
{
		int numCharsWritten=0;
		
		antlr.stringtemplate.language.StringTemplateAST action_AST_in = (antlr.stringtemplate.language.StringTemplateAST)_t;
		
		Object e=null;
		
		
		try {      // for error handling
			e=expr(_t);
			_t = retTree_;
			numCharsWritten = chunk.writeAttribute(self,e,@out);
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
	
	public Object  expr(AST _t) //throws RecognitionException
{
		Object value=null;
		
		antlr.stringtemplate.language.StringTemplateAST expr_AST_in = (antlr.stringtemplate.language.StringTemplateAST)_t;
		
		Object a=null, b=null, e=null;
		IDictionary argumentContext=null;
		
		
		try {      // for error handling
			if (null == _t)
				_t = ASTNULL;
			switch ( _t.Type )
			{
			case PLUS:
			{
				AST __t3 = _t;
				antlr.stringtemplate.language.StringTemplateAST tmp1_AST_in = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
				match((AST)_t,PLUS);
				_t = _t.getFirstChild();
				a=expr(_t);
				_t = retTree_;
				b=expr(_t);
				_t = retTree_;
				value = chunk.add(a,b);
				_t = __t3;
				_t = _t.getNextSibling();
				break;
			}
			case APPLY:
			{
				value=templateApplication(_t);
				_t = retTree_;
				break;
			}
			case ID:
			case DOT:
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
			case VALUE:
			{
				AST __t4 = _t;
				antlr.stringtemplate.language.StringTemplateAST tmp2_AST_in = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
				match((AST)_t,VALUE);
				_t = _t.getFirstChild();
				e=expr(_t);
				_t = retTree_;
				_t = __t4;
				_t = _t.getNextSibling();
				
				StringWriter buf = new StringWriter();
				Type writerClass = @out.GetType();
				StringTemplateWriter sw = null;
				try {
				ConstructorInfo ctor =
					writerClass.GetConstructor(new Type[] {typeof(TextWriter)});
				sw = (StringTemplateWriter)ctor.Invoke(new Object[] {buf});
				}
				catch (Exception exc) {
					// default new AutoIndentWriter(buf)
					self.error("cannot make implementation of StringTemplateWriter",exc);
					sw = new AutoIndentWriter(buf);
					}
				chunk.writeAttribute(self,e,sw);
				value = buf.ToString();
				
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
	public Object  templateApplication(AST _t) //throws RecognitionException
{
		Object value=null;
		
		antlr.stringtemplate.language.StringTemplateAST templateApplication_AST_in = (antlr.stringtemplate.language.StringTemplateAST)_t;
		
		Object a=null;
		ArrayList templatesToApply=new ArrayList();
		
		
		try {      // for error handling
			AST __t10 = _t;
			antlr.stringtemplate.language.StringTemplateAST tmp3_AST_in = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
			match((AST)_t,APPLY);
			_t = _t.getFirstChild();
			a=expr(_t);
			_t = retTree_;
			{ // ( ... )+
				int _cnt12=0;
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
						if (_cnt12 >= 1) { goto _loop12_breakloop; } else { throw new NoViableAltException(_t);; }
					}
					
					_cnt12++;
				}
_loop12_breakloop:				;
			}    // ( ... )+
			value = chunk.applyListOfAlternatingTemplates(self,a,templatesToApply);
			_t = __t10;
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
	
	public Object  attribute(AST _t) //throws RecognitionException
{
		Object value=null;
		
		antlr.stringtemplate.language.StringTemplateAST attribute_AST_in = (antlr.stringtemplate.language.StringTemplateAST)_t;
		antlr.stringtemplate.language.StringTemplateAST prop = null;
		antlr.stringtemplate.language.StringTemplateAST i3 = null;
		antlr.stringtemplate.language.StringTemplateAST i = null;
		antlr.stringtemplate.language.StringTemplateAST s = null;
		
		Object obj = null;
		
		
		try {      // for error handling
			if (null == _t)
				_t = ASTNULL;
			switch ( _t.Type )
			{
			case DOT:
			{
				AST __t21 = _t;
				antlr.stringtemplate.language.StringTemplateAST tmp4_AST_in = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
				match((AST)_t,DOT);
				_t = _t.getFirstChild();
				obj=attribute(_t);
				_t = retTree_;
				prop = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
				match((AST)_t,ID);
				_t = _t.getNextSibling();
				_t = __t21;
				_t = _t.getNextSibling();
				value = chunk.getObjectProperty(self,obj,prop.getText());
				break;
			}
			case ID:
			{
				i3 = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
				match((AST)_t,ID);
				_t = _t.getNextSibling();
				
				try {
				value=self.getAttribute(i3.getText());
				}
				catch (ArgumentOutOfRangeException nse) {
				// rethrow with more precise error message
				throw new ArgumentOutOfRangeException(nse.ParamName, String.Format(
					"no such attribute: {0} in template {1}", i3.getText(), self.getName()));
				}
				
				break;
			}
			case INT:
			{
				i = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
				match((AST)_t,INT);
				_t = _t.getNextSibling();
				value=Int32.Parse(i.getText());
				break;
			}
			case STRING:
			{
				s = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
				match((AST)_t,STRING);
				_t = _t.getNextSibling();
				value=s.getText();
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
	
	public Object  templateInclude(AST _t) //throws RecognitionException
{
		Object value=null;
		
		antlr.stringtemplate.language.StringTemplateAST templateInclude_AST_in = (antlr.stringtemplate.language.StringTemplateAST)_t;
		antlr.stringtemplate.language.StringTemplateAST id = null;
		antlr.stringtemplate.language.StringTemplateAST a1 = null;
		antlr.stringtemplate.language.StringTemplateAST a2 = null;
		
		StringTemplateAST args = null;
		String name = null;
		Object n = null;
		
		
		try {      // for error handling
			AST __t6 = _t;
			antlr.stringtemplate.language.StringTemplateAST tmp5_AST_in = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
			match((AST)_t,INCLUDE);
			_t = _t.getFirstChild();
			{
				if (null == _t)
					_t = ASTNULL;
				switch ( _t.Type )
				{
				case ID:
				{
					id = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
					match((AST)_t,ID);
					_t = _t.getNextSibling();
					a1 = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
					if (null == _t) throw new MismatchedTokenException();
					_t = _t.getNextSibling();
					name=id.getText(); args=a1;
					break;
				}
				case VALUE:
				{
					AST __t8 = _t;
					antlr.stringtemplate.language.StringTemplateAST tmp6_AST_in = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
					match((AST)_t,VALUE);
					_t = _t.getFirstChild();
					n=expr(_t);
					_t = retTree_;
					a2 = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
					if (null == _t) throw new MismatchedTokenException();
					_t = _t.getNextSibling();
					_t = __t8;
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
			_t = __t6;
			_t = _t.getNextSibling();
			
			if ( name!=null ) {
				value = chunk.getTemplateInclude(self, name, args);
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
	
	public void template(AST _t,
		ArrayList templatesToApply
	) //throws RecognitionException
{
		
		antlr.stringtemplate.language.StringTemplateAST template_AST_in = (antlr.stringtemplate.language.StringTemplateAST)_t;
		antlr.stringtemplate.language.StringTemplateAST t = null;
		antlr.stringtemplate.language.StringTemplateAST args = null;
		antlr.stringtemplate.language.StringTemplateAST anon = null;
		
		IDictionary argumentContext = null;
		Object n = null;
		
		
		try {      // for error handling
			AST __t14 = _t;
			antlr.stringtemplate.language.StringTemplateAST tmp7_AST_in = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
			match((AST)_t,TEMPLATE);
			_t = _t.getFirstChild();
			{
				if (null == _t)
					_t = ASTNULL;
				switch ( _t.Type )
				{
				case ID:
				{
					t = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
					match((AST)_t,ID);
					_t = _t.getNextSibling();
					args = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
					if (null == _t) throw new MismatchedTokenException();
					_t = _t.getNextSibling();
					
					String templateName = t.getText();
					StringTemplateGroup group = self.getGroup();
					StringTemplate embedded = group.getEmbeddedInstanceOf(self, templateName);
					if ( embedded!=null ) {
					embedded.setArgumentsAST(args);
					templatesToApply.Add(embedded);
					}
					
					break;
				}
				case ANONYMOUS_TEMPLATE:
				{
					anon = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
					match((AST)_t,ANONYMOUS_TEMPLATE);
					_t = _t.getNextSibling();
					
					StringTemplate anonymous = anon.getStringTemplate();
					templatesToApply.Add(anonymous);
					
					break;
				}
				case VALUE:
				{
					AST __t16 = _t;
					antlr.stringtemplate.language.StringTemplateAST tmp8_AST_in = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
					match((AST)_t,VALUE);
					_t = _t.getFirstChild();
					n=expr(_t);
					_t = retTree_;
					argumentContext=argList(_t,null);
					_t = retTree_;
					_t = __t16;
					_t = _t.getNextSibling();
					
					if ( n!=null ) {
						String templateName = n.ToString();
										StringTemplateGroup group = self.getGroup();
										StringTemplate embedded = group.getEmbeddedInstanceOf(self, templateName);
										if ( embedded!=null ) {
											embedded.setArgumentsAST(args);
											templatesToApply.Add(embedded);
										}
					}
					
					break;
				}
				default:
				{
					throw new NoViableAltException(_t);
				}
				 }
			}
			_t = __t14;
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
	
	public IDictionary  argList(AST _t,
		IDictionary initialContext
	) //throws RecognitionException
{
		IDictionary argumentContext=null;
		
		antlr.stringtemplate.language.StringTemplateAST argList_AST_in = (antlr.stringtemplate.language.StringTemplateAST)_t;
		
		argumentContext = initialContext;
		if ( argumentContext==null ) {
		argumentContext=new Hashtable();
		}
		
		
		try {      // for error handling
			AST __t23 = _t;
			antlr.stringtemplate.language.StringTemplateAST tmp9_AST_in = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
			match((AST)_t,ARGS);
			_t = _t.getFirstChild();
			{    // ( ... )*
				for (;;)
				{
					if (_t == null)
						_t = ASTNULL;
					if ((_t.Type==ASSIGN))
					{
						argumentAssignment(_t,argumentContext);
						_t = retTree_;
					}
					else
					{
						goto _loop25_breakloop;
					}
					
				}
_loop25_breakloop:				;
			}    // ( ... )*
			_t = __t23;
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
		return argumentContext;
	}
	
	public bool  ifCondition(AST _t) //throws RecognitionException
{
		bool value=false;
		
		antlr.stringtemplate.language.StringTemplateAST ifCondition_AST_in = (antlr.stringtemplate.language.StringTemplateAST)_t;
		
		Object a=null, b=null;
		
		
		try {      // for error handling
			if (null == _t)
				_t = ASTNULL;
			switch ( _t.Type )
			{
			case APPLY:
			case INCLUDE:
			case VALUE:
			case PLUS:
			case ID:
			case DOT:
			case STRING:
			case INT:
			{
				a=ifAtom(_t);
				_t = retTree_;
				value = chunk.testAttributeTrue(a);
				break;
			}
			case NOT:
			{
				AST __t18 = _t;
				antlr.stringtemplate.language.StringTemplateAST tmp10_AST_in = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
				match((AST)_t,NOT);
				_t = _t.getFirstChild();
				a=ifAtom(_t);
				_t = retTree_;
				_t = __t18;
				_t = _t.getNextSibling();
				value = !chunk.testAttributeTrue(a);
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
	
	public Object  ifAtom(AST _t) //throws RecognitionException
{
		Object value=null;
		
		antlr.stringtemplate.language.StringTemplateAST ifAtom_AST_in = (antlr.stringtemplate.language.StringTemplateAST)_t;
		
		try {      // for error handling
			value=expr(_t);
			_t = retTree_;
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
	
	public void argumentAssignment(AST _t,
		IDictionary argumentContext
	) //throws RecognitionException
{
		
		antlr.stringtemplate.language.StringTemplateAST argumentAssignment_AST_in = (antlr.stringtemplate.language.StringTemplateAST)_t;
		antlr.stringtemplate.language.StringTemplateAST arg = null;
		
		Object e = null;
		
		
		try {      // for error handling
			AST __t27 = _t;
			antlr.stringtemplate.language.StringTemplateAST tmp11_AST_in = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
			match((AST)_t,ASSIGN);
			_t = _t.getFirstChild();
			arg = (_t==ASTNULL) ? null : (antlr.stringtemplate.language.StringTemplateAST)_t;
			match((AST)_t,ID);
			_t = _t.getNextSibling();
			e=expr(_t);
			_t = retTree_;
			
				       if ( e!=null )
				           self.rawSetAttribute(argumentContext,arg.getText(),e);
				
			_t = __t27;
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
	
	public new antlr.stringtemplate.language.StringTemplateAST getAST()
	{
		return (antlr.stringtemplate.language.StringTemplateAST) returnAST;
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
	
}

}
