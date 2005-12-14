/*
[The "BSD licence"]
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
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.*/
using System;
using System.Reflection;
using System.Collections;
using antlr.stringtemplate;
using AST = antlr.collections.AST;
using RecognitionException = antlr.RecognitionException;
namespace antlr.stringtemplate.language
{
	
	/// <summary>A single string template expression enclosed in $...; separator=...$
	/// parsed into an AST chunk to be evaluated.
	/// </summary>
	public class ASTExpr:Expr
	{
		public const String DEFAULT_ATTRIBUTE_NAME = "it";
		public const String DEFAULT_ATTRIBUTE_NAME_DEPRECATED = "attr";
		public const String DEFAULT_INDEX_VARIABLE_NAME = "i";
		
		/// <summary>How to refer to the surrounding template object;
		/// this is attribute name to use.
		/// public static final String SELF_ATTR = "self";
		/// </summary>
		
		/// <summary>When lint mode is on, this attribute contains a list of reflection
		/// objects that tell you about the attributes set by the user of the
		/// template.
		/// </summary>
		public const String REFLECTION_ATTRIBUTES = "attributes";
		
		internal AST exprTree = null;
		
		/// <summary>store separator etc... </summary>
		internal IDictionary options = null;
		
		public ASTExpr(StringTemplate enclosingTemplate, AST exprTree, IDictionary options):base(enclosingTemplate)
		{
			this.exprTree = exprTree;
			this.options = options;
		}
		
		/// <summary>Return the tree interpreted when this template is written out. </summary>
		public virtual AST getAST()
		{
			return exprTree;
		}
		
		/// <summary>To write out the value of an ASTExpr, invoke the evaluator in eval.g
		/// to walk the tree writing out the values.  For efficiency, don't
		/// compute a bunch of strings and then pack them together.  Write out directly.
		/// </summary>
		public override int write(StringTemplate self, StringTemplateWriter outWriter)
		{
			if (exprTree == null || self == null || outWriter == null)
			{
				return 0;
			}
			outWriter.pushIndentation(getIndentation());
			//System.out.println("evaluating tree: "+exprTree.toStringList());
			ActionEvaluator eval = new ActionEvaluator(self, this, outWriter);
			int n = 0;
			try
			{
				n = eval.action(exprTree); // eval and write out tree
			}
			catch (RecognitionException re)
			{
				self.error("can't evaluate tree: " + exprTree.ToStringList(), re);
			}
			outWriter.popIndentation();
			return n;
		}
		
		// HELP ROUTINES CALLED BY EVALUATOR TREE WALKER
		
		public virtual Object applyListOfAlternatingTemplates(StringTemplate self, Object attributeValue, IList templatesToApply)
		{
			if (attributeValue == null || templatesToApply == null || templatesToApply.Count == 0) {
				return null; // do not apply if missing templates or empty value
			}

			StringTemplate embedded = null;
			IDictionary argumentContext = null;
			
			// normalize collections and such to use iterators
			// anything iteratable can be used for "APPLY"
			attributeValue = convertAnythingIteratableToIterator(attributeValue);
			
			if (attributeValue is IEnumerator) {
				IList resultVector = new ArrayList();
				IEnumerator iter = (IEnumerator) attributeValue;
				int i = 0;

				while (iter.MoveNext()) {
					Object ithValue = iter.Current;

					if (ithValue == null) {
						// weird...a null value in the list; ignore
						continue;
					}

					// rotate through
					int templateIndex = i % templatesToApply.Count; 
					embedded = (StringTemplate) templatesToApply[templateIndex];

					// template to apply is an actual StringTemplate (created in
					// eval.g), but that is used as the examplar.  We must create
					// a new instance of the embedded template to apply each time
					// to get new attribute sets etc...
					StringTemplateAST args = embedded.getArgumentsAST();
					embedded = embedded.getInstanceOf(); // make new instance
					embedded.setEnclosingInstance(self);
					embedded.setArgumentsAST(args);

					argumentContext = new Hashtable();
					argumentContext[DEFAULT_ATTRIBUTE_NAME] = ithValue;
					argumentContext[DEFAULT_ATTRIBUTE_NAME_DEPRECATED] = ithValue;
					argumentContext[DEFAULT_INDEX_VARIABLE_NAME] = i + 1;
					embedded.setArgumentContext(argumentContext);

					evaluateArguments(embedded);

					/*
					System.err.println("i="+i+": applyTemplate("+embedded.getName()+
					", args="+argumentContext+
					" to attribute value "+ithValue);
					*/

					resultVector.Add(embedded);
					i++;
				}

				return resultVector;
			}
			else {
				/*
				Console.WriteLine("setting attribute "+DefaultAttributeName+" in arg context of "+
				embedded.getName()+
				" to "+attributeValue);
				*/
				embedded = (StringTemplate) templatesToApply[0];

				argumentContext = new Hashtable();
				argumentContext[DEFAULT_ATTRIBUTE_NAME] = attributeValue;
				argumentContext[DEFAULT_ATTRIBUTE_NAME_DEPRECATED] = attributeValue;
				argumentContext[DEFAULT_INDEX_VARIABLE_NAME] = 1;
				embedded.setArgumentContext(argumentContext);
				evaluateArguments(embedded);

				return embedded;
			}
		}
		
		/// <summary>Return o.getPropertyName() given o and propertyName.  If o is
		/// a stringtemplate then access it's attributes looking for propertyName
		/// instead (don't check any of the enclosing scopes; look directly into
		/// that object).  Also try isXXX() for booleans.  Allow HashMap,
		/// Hashtable as special case (grab value for key).
		/// </summary>
		/// <summary>
		/// Return o.getPropertyName() given o and propertyName.  If o is
		/// a stringtemplate then access it's attributes looking for propertyName
		/// instead (don't check any of the enclosing scopes; look directly into
		/// that object).  Also try isXXX() for booleans.  Allow HashMap,
		/// Hashtable as special case (grab value for key).
		/// </summary>
		public virtual Object getObjectProperty(StringTemplate self, object o, string propertyName) {
			if (o == null || propertyName == null) {
				return null;
			}

			Type c = o.GetType();
			Object value = null;
			
			// Special case: our automatically created Aggregates via
			// attribute name: "{obj.{prop1,prop2}}"
			if (c == typeof(StringTemplate.Aggregate)) {
				value = ((StringTemplate.Aggregate) o).get(propertyName);
			}
				// Special case: if it's a template, pull property from
				// it's attribute table.
				// TODO: TJP just asked himself why we can't do inherited attr here?
			else if (c == typeof(StringTemplate)) {
				IDictionary attributes = ((StringTemplate) o).getAttributes();
				if ( attributes!=null ) {
					value = attributes[propertyName];
				}
			}
				// Special case: if it's a HashMap, Hashtable then pull using
				// key not the property method.  Do NOT allow general Map interface
				// as people could pass in their database masquerading as a Map.
			else if ( isValidMapInstance(c) ) {
				IDictionary map = (IDictionary)o;
				value = map[propertyName];
			}
	
			else {
				// lookup as property or accessor-method
				string lookupName = Char.ToUpper(propertyName[0])+
					propertyName.Substring(1);

				//see if it's a property
				PropertyInfo pi = c.GetProperty(lookupName, 
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase, 
					null, null, new Type[0], null);

				if (pi != null) {
					value = pi.GetValue(o, null);
				}
				else {
					//see if it's a method
					String methodName = "Get" + lookupName;
					MethodInfo mi = c.GetMethod(methodName, 
						BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase, 
						null, new Type[0], null);

					if (mi == null) {
						mi = c.GetMethod("Is" + lookupName, 
							BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase, 
							null, new Type[0], null);
					}

					if (mi == null) {
						self.error("Can't get property " +propertyName+ " as C# property '" +lookupName+ 
							"' or as C# methods 'Get" +lookupName+ "' or 'Is" +lookupName+
							"' from " +c.FullName+ " instance");
					}
					else {
						value = mi.Invoke(o, null);
					}
				}
			}
			
			return value;
		}
		
		/// <summary>Normally StringTemplate tests presence or absence of attributes
		/// for adherence to my principles of separation, but some people
		/// disagree and want to change.
		/// 
		/// For 2.0, if the object is a boolean, do something special. $if(boolean)$
		/// will actually test the value.  Now, this breaks my rules of entanglement
		/// listed in my paper, but it truly surprises programmers to have booleans
		/// always true.  Further, the key to isolating logic in the model is avoiding
		/// operators (for which you need attribute values).  But, no operator is
		/// needed to use boolean values.  Well, actually I guess "!" (not) is
		/// an operator.  Regardless, for practical reasons, I'm going to technically
		/// violate my rules as I currently have them defined.  Perhaps for a future
		/// version of the paper I will refine the rules.
		/// </summary>
		public virtual bool testAttributeTrue(Object a)
		{
			if (a != null && a is System.Boolean)
			{
				return ((System.Boolean) a);
			}
			return a != null;
		}
		
		/// <summary>For strings or other objects, catenate and return.</summary>
		public virtual Object add(Object a, Object b)
		{
			if (a == null)
			{
				// a null value means don't do cat, just return other value
				return b;
			}
			else if (b == null)
			{
				return a;
			}
			return a.ToString() + b.ToString();
		}
		
		/// <summary>Call a string template with args and return result.  Do not convert
		/// to a string yet.  It may need attributes that will be available after
		/// this is inserted into another template.
		/// </summary>
		public virtual StringTemplate getTemplateInclude(StringTemplate enclosing, String templateName, StringTemplateAST argumentsAST)
		{
			StringTemplateGroup group = enclosing.getGroup();
			StringTemplate embedded = group.getEmbeddedInstanceOf(enclosing, templateName);
			if (embedded == null)
			{
				enclosing.error("cannot make embedded instance of " + templateName + " in template " + enclosing.getName());
				return null;
			}
			embedded.setArgumentsAST(argumentsAST);
			evaluateArguments(embedded);
			return embedded;
		}
		
		/// <summary>How to spit out an object.  If it's not a StringTemplate nor a
		/// List, just do o.toString().  If it's a StringTemplate,
		/// do o.write(out).  If it's a Vector, do a write(out,
		/// o.elementAt(i)) for all elements.  Note that if you do
		/// something weird like set the values of a multivalued tag
		/// to be vectors, it will effectively flatten it.
		/// 
		/// If self is an embedded template, you might have specified
		/// a separator arg; used when is a vector.
		/// </summary>
		public virtual int writeAttribute(StringTemplate self, Object o, StringTemplateWriter outWriter)
		{
			Object separator = null;
			if (options != null)
			{
				separator = options["separator"];
			}
			return write(self, o, outWriter, separator);
		}
		
		protected internal virtual int write(StringTemplate self, Object o, StringTemplateWriter outWriter, Object separator)
		{
			if (o == null)
			{
				return 0;
			}
			int n = 0;
			try
			{
				if (o is StringTemplate)
				{
					StringTemplate stToWrite = (StringTemplate) o;
					stToWrite.setEnclosingInstance(self);
					// if self is found up the enclosing instance chain, then
					// infinite recursion
					if (StringTemplate.inLintMode() && StringTemplate.isRecursiveEnclosingInstance(stToWrite))
					{
						// throw exception since sometimes eval keeps going
						// even after I ignore this write of o.
						throw new System.SystemException("infinite recursion to " + stToWrite.getTemplateDeclaratorString() + " referenced in " + stToWrite.getEnclosingInstance().getTemplateDeclaratorString() + "; stack trace:\n" + stToWrite.getEnclosingInstanceStackTrace());
					}
					else
					{
						n = stToWrite.write(outWriter);
					}
					return n;
				}
				// normalize anything iteratable to iterator
				o = convertAnythingIteratableToIterator(o);
				if (o is IEnumerator)
				{
					IEnumerator iter = (IEnumerator) o;
					String separatorString = null;
					if (separator != null)
					{
						separatorString = computeSeparator(self, outWriter, separator);
					}

					int i = 0;
					int charWrittenForValue = 0;
					Object iterValue = null;

					while (iter.MoveNext())
					{
						if (i > 0) 
						{
							bool valueIsPureConditional = false;
							if (iterValue is StringTemplate)
							{
								StringTemplate iterValueST = (StringTemplate) iterValue;
								IList chunks = (IList) iterValueST.getChunks();
								Expr firstChunk = (Expr) chunks[0];
								valueIsPureConditional = firstChunk is ConditionalExpr && ((ConditionalExpr) firstChunk).getElseSubtemplate() == null;
							}
							bool emptyIteratedValue = valueIsPureConditional && charWrittenForValue == 0;
							if (!emptyIteratedValue && separator != null)
							{
								n += outWriter.write(separatorString);
							}
						}

						iterValue = iter.Current;
						charWrittenForValue = write(self, iterValue, outWriter, separator);
						n += charWrittenForValue;
						i++;
					}
				}
				else
				{
					n = outWriter.write(o.ToString());
					return n;
				}
			}
			catch (System.IO.IOException io)
			{
				self.error("problem writing object: " + o, io);
			}
			return n;
		}
		
		/// <summary>A separator is normally just a string literal, but is still an AST that
		/// we must evaluate.  The separator can be any expression such as a template
		/// include or string cat expression etc...
		/// </summary>
		protected internal virtual String computeSeparator(StringTemplate self, StringTemplateWriter outWriter, Object separator)
		{
			if (separator == null)
			{
				return null;
			}
			if (separator is StringTemplateAST)
			{
				StringTemplateAST separatorTree = (StringTemplateAST) separator;
				// must evaluate, writing to a string so we can hand on to it
				ASTExpr e = new ASTExpr(getEnclosingTemplate(), separatorTree, null);
				System.IO.StringWriter buf = new System.IO.StringWriter();
				// create a new instance of whatever StringTemplateWriter
				// implementation they are using.  Default is AutoIndentWriter.
				// Defalut behavior is to indent but without
				// any prior indents surrounding this attribute expression
				StringTemplateWriter sw = null;
				System.Type writerClass = outWriter.GetType();
				try
				{
					System.Reflection.ConstructorInfo ctor = writerClass.GetConstructor(new System.Type[]{typeof(System.IO.TextWriter)});
					sw = (StringTemplateWriter) ctor.Invoke(new Object[]{buf});
				}
				catch (System.Exception exc)
				{
					// default new AutoIndentWriter(buf)
					self.error("cannot make implementation of StringTemplateWriter", exc);
					sw = new AutoIndentWriter(buf);
				}
				
				try
				{
					e.write(self, sw);
				}
				catch (System.IO.IOException ioe)
				{
					self.error("can't evaluate separator expression", ioe);
				}
				return buf.ToString();
			}
			else
			{
				// just in case we expand in the future and it's something else
				return separator.ToString();
			}
		}
		
		protected internal virtual void evaluateArguments(StringTemplate self)
		{
			StringTemplateAST argumentsAST = self.getArgumentsAST();
			if (argumentsAST == null || argumentsAST.getFirstChild() == null)
			{
				// return immediately if missing tree or no actual args
				return ;
			}
			ActionEvaluator eval = new ActionEvaluator(self, this, null);
			try
			{
				// using any initial argument context (such as when obj is set),
				// evaluate the arg list like bold(item=obj).  Since we pass
				// in any existing arg context, that context gets filled with
				// new values.  With bold(item=obj), context becomes:
				// {[obj=...],[item=...]}.
				IDictionary ac = eval.argList(argumentsAST, self.getArgumentContext());
				self.setArgumentContext(ac);
			}
			catch (RecognitionException re)
			{
				self.error("can't evaluate tree: " + argumentsAST.ToStringList(), re);
			}
		}
		
		/// <summary>Do a standard conversion of array attributes to Lists.</summary>
		private static Object convertAnythingIteratableToIterator(Object o) {
			IEnumerator iter = null;

			if (o is IDictionary) {
				iter = ((IDictionary) o).Values.GetEnumerator();
			}
			else if (o is ICollection) {
				iter = ((ICollection) o).GetEnumerator();
			}
			else if (o is IEnumerator) {
				iter = (IEnumerator) o;
			}

			if (iter == null) {
				return o;
			}

			return iter;
		}

		public static bool isValidMapInstance(System.Type type)
		{
			return type == typeof(Hashtable) || type == typeof(Hashtable);
		}
		
		/// <summary>A property can be declared as Map, but the instance must be
		/// isValidMapInstance().
		/// </summary>
		public static bool isValidReturnTypeMapInstance(System.Type type)
		{
			return isValidMapInstance(type) || type == typeof(IDictionary);
		}
	}
}