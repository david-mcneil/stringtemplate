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
		public const String DEFAULT_MAP_VALUE_NAME = "_default_";
		
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

		/// <summary>
		/// For names,phones:{n,p | ...} treat the names, phones as lists
		/// to be walked in lock step as n=names[i], p=phones[i].
		/// </summary>
		public virtual Object applyTemplateToListOfAttributes(StringTemplate self, IList attributes, StringTemplate templateToApply)
		{
			if ( attributes==null || templateToApply==null || attributes.Count==0 ) 
			{
				return null; // do not apply if missing templates or empty values
			}
			IDictionary argumentContext = null;
			IList results = new ArrayList();

			// convert all attributes to iterators even if just one value
			for (int a = 0; a < attributes.Count; a++) 
			{
				Object o = attributes[a];
				o = convertAnythingToIterator(o);
				attributes[a] = o;
			}

			int numAttributes = attributes.Count;

			// ensure arguments line up
			IDictionary formalArguments = templateToApply.getFormalArguments();
			if ( formalArguments==null || formalArguments.Count==0 ) 
			{
				self.error("missing arguments in anonymous"+
					" template in context "+self.getEnclosingInstanceStackString());
				return null;
			}
			ICollection keys = formalArguments.Keys;
			Object[] formalArgumentNames = new object[keys.Count];
			keys.CopyTo(formalArgumentNames,0);
			if ( formalArgumentNames.Length!=numAttributes ) 
			{
				self.error("number of arguments "+SupportClass.CollectionToString(formalArguments.Keys)+
					" mismatch between attribute list and anonymous"+
					" template in context "+self.getEnclosingInstanceStackString());
				// truncate arg list to match smaller size
				int shorterSize = Math.Min(formalArgumentNames.Length, numAttributes);
				numAttributes = shorterSize;
				Object[] newFormalArgumentNames = new Object[shorterSize];
				System.Array.Copy(formalArgumentNames, 0,
					newFormalArgumentNames, 0,
					shorterSize);
				formalArgumentNames = newFormalArgumentNames;
			}

			// keep walking while at least one attribute has values
			while ( true ) 
			{
				argumentContext = new Hashtable();
				// get a value for each attribute in list; put into arg context
				// to simulate template invocation of anonymous template
				int numEmpty = 0;
				for (int a = 0; a < numAttributes; a++) 
				{
					IEnumerator it = (IEnumerator)attributes[a];
					if ( it.MoveNext() ) 
					{
						String argName = (String)formalArgumentNames[a];
						Object iteratedValue = it.Current;
						argumentContext[argName] = iteratedValue;
					}
					else 
					{
						numEmpty++;
					}
				}
				if ( numEmpty==numAttributes ) 
				{
					break;
				}
				StringTemplate embedded = templateToApply.getInstanceOf();
				embedded.setEnclosingInstance(self);
				embedded.setArgumentContext(argumentContext);
				results.Add(embedded);
			}

			return results;
		}
		
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
					setSoleFormalArgumentToIthValue(embedded, argumentContext, ithValue);
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
				setSoleFormalArgumentToIthValue(embedded, argumentContext, attributeValue);
				argumentContext[DEFAULT_ATTRIBUTE_NAME] = attributeValue;
				argumentContext[DEFAULT_ATTRIBUTE_NAME_DEPRECATED] = attributeValue;
				argumentContext[DEFAULT_INDEX_VARIABLE_NAME] = 1;
				embedded.setArgumentContext(argumentContext);
				evaluateArguments(embedded);

				return embedded;
			}
		}

		protected void setSoleFormalArgumentToIthValue(StringTemplate embedded, IDictionary argumentContext, Object ithValue) 
		{
			IDictionary formalArgs = embedded.getFormalArguments();
			if ( formalArgs!=null ) 
			{
				String soleArgName = null;
				bool isAnonymous = embedded.getName().Equals(StringTemplate.ANONYMOUS_ST_NAME);
				if ( formalArgs.Count==1 || (isAnonymous&&formalArgs.Count>0) ) 
				{
					if ( isAnonymous && formalArgs.Count>1 ) 
					{
						embedded.error("too many arguments on {...} template: "+formalArgs);
					}
					// if exactly 1 arg or anonymous, give that the value of
					// "it" as a convenience like they said
					// $list:template(arg=it)$
					ICollection argNames = formalArgs.Keys;
					string[] argNamesArray = new string[argNames.Count];
					argNames.CopyTo(argNamesArray,0);
					soleArgName = argNamesArray[0];
					argumentContext[soleArgName] = ithValue;
				}
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
				if ( value==null ) 
				{
					// no property defined; if a map in this group
					// then there may be a default value
					value = map[DEFAULT_MAP_VALUE_NAME];
				}
			}
	
			else {
				// lookup as property or accessor-method
				string lookupName = Char.ToUpper(propertyName[0])+
					propertyName.Substring(1);

				// LL Add NonPublic
				//see if it's a property
				PropertyInfo pi = c.GetProperty(lookupName, 
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase, 
					null, null, new Type[0], null);

				if (pi != null) {
					value = pi.GetValue(o, null);
				}
				else {
					//see if it's a method
					String methodName = "Get" + lookupName;
					MethodInfo mi = c.GetMethod(methodName, 
						BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase, 
						null, new Type[0], null);

					if (mi == null) {
						mi = c.GetMethod("Is" + lookupName, 
							BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase, 
							null, new Type[0], null);
					}

					// LL Changed
					if (mi != null)
					{
						value = mi.Invoke(o, null);
					}
					else 
					{
						// Check for string indexer
						Type[] paramarray = new Type[1];
						paramarray[0] = typeof(string);
						PropertyInfo ipi = c.GetProperty("Item", paramarray);

						if (ipi != null) 
						{
							object[] invparam = new object[1];
							invparam[0] = propertyName;
							value = ipi.GetValue(o, invparam);
						}
						else
						{
							self.error("Can't get property " +propertyName+ " as C# property '" +lookupName+ 
								"' or as C# methods 'Get" +lookupName+ "' or 'Is" +lookupName+
								"' from " +c.FullName+ " instance");
						}
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
		/// 
		/// Post 2.1, I added a check for non-null Iterators, Collections, ...
		/// with size==0 to return false. TJP 5/1/2005
		///
		/// </summary>
		public virtual bool testAttributeTrue(Object a)
		{
			IAttributeStrategy attribstrat = enclosingTemplate.getGroup().AttributeStrategy;
			if (attribstrat != null)
			{
				return attribstrat.testAttributeTrue(a);
			}
			else
			{
				if (a==null) 
					return false;
				if (a is Boolean)
					return (bool)a;
				if (a is ICollection)
					return ((ICollection)a).Count>0;
				if (a is IDictionary)
					return ((IDictionary)a).Count>0;
				if (a is IEnumerator) 
					return ((IEnumerator)a).MoveNext();
				return true;
			}
		}
		
		/// <summary>For now, we can only add two objects as strings; convert objects to Strings then cat.</summary>
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
					// failsafe: perhaps enclosing instance not set
					// Or, it could be set to another context!  This occurs
					// when you store a template instance as an attribute of more
					// than one template (like both a header file and C file when
					// generating C code).  It must execute within the context of
					// the enclosing template.
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
					AttributeRenderer renderer = self.getAttributeRenderer(o.GetType());
					if ( renderer!=null ) 
					{
						n = outWriter.write(renderer.ToString(o));
					}
					else 
					{
                		n = outWriter.write(o.ToString());
					}			
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
		
		/// <summary>
		/// Evaluate an argument list within the context of the enclosing
		/// template but store the values in the context of self, the
		/// new embedded template.  For example, bold(item=item) means
		/// that bold.item should get the value of enclosing.item.
		///	</summary>
		protected internal virtual void evaluateArguments(StringTemplate self)
		{
			StringTemplateAST argumentsAST = self.getArgumentsAST();
			if (argumentsAST == null || argumentsAST.getFirstChild() == null)
			{
				// return immediately if missing tree or no actual args
				return ;
			}
			// Evaluate args in the context of the enclosing template, but we
			// need the predefined args like 'it', 'attr', and 'i' to be
			// available as well so we put a dummy ST between the enclosing
			// context and the embedded context.  The dummy has the predefined
			// context as does the embedded.
			StringTemplate enclosing = self.getEnclosingInstance();
			StringTemplate argContextST = new StringTemplate(self.getGroup(), "");
			argContextST.setName("<invoke "+self.getName()+" arg context>");
			argContextST.setEnclosingInstance(enclosing);
			argContextST.setArgumentContext(self.getArgumentContext());

			ActionEvaluator eval = new ActionEvaluator(argContextST, this, null);
			try
			{
				// using any initial argument context (such as when obj is set),
				// evaluate the arg list like bold(item=obj).  Since we pass
				// in any existing arg context, that context gets filled with
				// new values.  With bold(item=obj), context becomes:
				// {[obj=...],[item=...]}.
				IDictionary ac = eval.argList(argumentsAST, self, self.getArgumentContext());
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

		public static IEnumerator convertAnythingToIterator(Object o) 
		{
			IEnumerator iter = null;
			if (o is ICollection)
			{
				iter = ((ICollection)o).GetEnumerator();
			}
			else if (o is IDictionary)
			{
				iter = ((IDictionary)o).Values.GetEnumerator();
			}
			else if (o is IEnumerator)
			{
				iter = (IEnumerator)o;
			}

			if ( iter==null ) 
			{
				IList singleton = new ArrayList(1);
				singleton.Add(o);
				iter=singleton.GetEnumerator();
			}
			return iter;
		}

		/// <summary>
		/// Return the first attribute if multiple valued or the attribute
		/// itself if single-valued.  Used in names:first()
		/// </summary>
		public Object first(Object attribute) 
		{
			if ( attribute==null ) 
			{
				return null;
			}
			Object f = attribute;
			attribute = convertAnythingIteratableToIterator(attribute);
			if (attribute is IEnumerator) 
			{
				IEnumerator it = (IEnumerator)attribute;
				if (it.MoveNext()) 
				{
					f = it.Current;
				}
			}
			return f;
		}

		/// <summary>
		/// Return the everything but the first attribute if multiple valued
		/// or null if single-valued.  Used in names:rest().
		/// </summary>
		public Object rest(Object attribute) 
		{
			if ( attribute==null ) 
			{
				return null;
			}
			Object theRest = attribute;
			attribute = convertAnythingIteratableToIterator(attribute);
			if (attribute is IEnumerator) 
			{
				IEnumerator it = (IEnumerator)attribute;
				if (!it.MoveNext()) 
				{
					return null; // if not even one value return null
				}
				it.MoveNext(); // ignore first value
				if (!it.MoveNext()) 
				{
					return null; // if not more than one value, return null
				}
				theRest = it;    // return suitably altered iterator
			}
			else 
			{
				theRest = null;  // rest of single-valued attribute is null
			}
			return theRest;
		}

		/// <summary>
		/// Return the last attribute if multiple valued or the attribute
		/// itself if single-valued.  Used in names:last().  This is pretty
		/// slow as it iterates until the last element.  Ultimately, I could
		/// make a special case for a List or Vector.
		/// </summary>
		public Object last(Object attribute) 
		{
			if ( attribute==null ) 
			{
				return null;
			}
			Object last = attribute;
			attribute = convertAnythingIteratableToIterator(attribute);
			if (attribute is IEnumerator) 
			{
				IEnumerator it = (IEnumerator)attribute;
				while (it.MoveNext()) 
				{
					last = it.Current;
				}
			}
			return last;
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