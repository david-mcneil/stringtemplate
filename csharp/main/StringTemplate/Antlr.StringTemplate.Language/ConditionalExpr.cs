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
	using StringTemplate = Antlr.StringTemplate.StringTemplate;
	using IStringTemplateWriter = Antlr.StringTemplate.IStringTemplateWriter;
	using AST = antlr.collections.AST;
	using RecognitionException = antlr.RecognitionException;
	
	/// <summary>
	/// A conditional reference to an embedded subtemplate. 
	/// </summary>
	public class ConditionalExpr : ASTExpr
	{
		virtual public StringTemplate Subtemplate
		{
			get { return subtemplate; }
			set { this.subtemplate = value; }
		}
		virtual public StringTemplate ElseSubtemplate
		{
			get { return elseSubtemplate; }
			set { this.elseSubtemplate = value; }
		}
		internal StringTemplate subtemplate = null;
		internal StringTemplate elseSubtemplate = null;
		
		public ConditionalExpr(StringTemplate enclosingTemplate, AST tree)
			: base(enclosingTemplate, tree, null)
		{
		}
		
		/// <summary>
		/// To write out the value of a condition expr, we invoke the evaluator 
		/// in eval.g to walk the condition tree computing the boolean value.
		/// If result is true, then we write subtemplate.
		/// </summary>
		public override int Write(StringTemplate self, IStringTemplateWriter output)
		{
			if (exprTree == null || self == null || output == null)
			{
				return 0;
			}
			// System.out.println("evaluating conditional tree: "+exprTree.toStringList());
			ActionEvaluator eval = new ActionEvaluator(self, this, output);
			ActionParser.initializeASTFactory(eval.getASTFactory());
			int n = 0;
			try
			{
				// get conditional from tree and compute result
				AST cond = exprTree.getFirstChild();
				bool includeSubtemplate = eval.ifCondition(cond); // eval and write out tree
				// System.out.println("subtemplate "+subtemplate);
				if (includeSubtemplate)
				{
					/* To evaluate the IF chunk, make a new instance whose enclosingInstance
					* points at 'self' so get attribute works.  Otherwise, enclosingInstance
					* points at the template used to make the precompiled code.  We need a
					* new template instance every time we exec this chunk to get the new
					* "enclosing instance" pointer.
					*/
					StringTemplate s = subtemplate.GetInstanceOf();
					s.EnclosingInstance = self;
					// make sure we evaluate in context of enclosing template's
					// group so polymorphism works. :)
					s.Group = self.Group;
					s.NativeGroup = self.NativeGroup;
					n = s.Write(output);
				}
				else if (elseSubtemplate != null)
				{
					// evaluate ELSE clause if present and IF condition failed
					StringTemplate s = elseSubtemplate.GetInstanceOf();
					s.EnclosingInstance = self;
					s.Group = self.Group;
					s.NativeGroup = self.NativeGroup;
					n = s.Write(output);
				}
			}
			catch (RecognitionException re)
			{
				self.Error("can't evaluate tree: " + exprTree.ToStringList(), re);
			}
			return n;
		}
	}
}