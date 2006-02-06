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
	using Antlr.StringTemplate;
	using StringWriter				= System.IO.StringWriter;
	using ArrayList					= System.Collections.ArrayList;
	using IList						= System.Collections.IList;
	using ICollection				= System.Collections.ICollection;
	using IDictionary				= System.Collections.IDictionary;
	using IEnumerator				= System.Collections.IEnumerator;
	using Hashtable					= System.Collections.Hashtable;
	using HybridDictionary			= System.Collections.Specialized.HybridDictionary;
	using MemberInfo				= System.Reflection.MemberInfo;
	using PropertyInfo				= System.Reflection.PropertyInfo;
	using FieldInfo					= System.Reflection.FieldInfo;
	using MethodInfo				= System.Reflection.MethodInfo;
	using BindingFlags				= System.Reflection.BindingFlags;
	using AST						= antlr.collections.AST;
	using RecognitionException		= antlr.RecognitionException;
	using HashList					= Antlr.StringTemplate.Collections.HashList;
	using CollectionUtils			= Antlr.StringTemplate.Collections.CollectionUtils;

	/// <summary>
	/// A single string template expression enclosed in $...; separator=...$
	/// parsed into an AST chunk to be evaluated.
	/// </summary>
	public class ASTExpr : Expr
	{
		public const string DEFAULT_ATTRIBUTE_NAME = "it";
		public const string DEFAULT_ATTRIBUTE_NAME_DEPRECATED = "attr";
		public const string DEFAULT_INDEX_VARIABLE_NAME = "i";
		public const string DEFAULT_MAP_VALUE_NAME = "_default_";

		public ASTExpr(StringTemplate enclosingTemplate, AST exprTree, IDictionary options)
			: base(enclosingTemplate)
		{
			this.exprTree = exprTree;
			this.options = options;
		}
		
		/// <summary>
		/// Return the tree interpreted when this template is written out.
		/// </summary>
		virtual public AST AST
		{
			get { return exprTree; }
		}
		
		protected AST exprTree = null;
		
		/// <summary>store separator etc... </summary>
		private IDictionary options = null;
		
		/// <summary>
		/// To write out the value of an ASTExpr, invoke the evaluator in eval.g
		/// to walk the tree writing out the values.  For efficiency, don't
		/// compute a bunch of strings and then pack them together.  Write out directly.
		/// </summary>
		public override int Write(StringTemplate self, IStringTemplateWriter output)
		{
			if (exprTree == null || self == null || output == null)
			{
				return 0;
			}
			output.PushIndentation(Indentation);
			//System.out.println("evaluating tree: "+exprTree.toStringList());
			ActionEvaluator eval = new ActionEvaluator(self, this, output);
			ActionParser.initializeASTFactory(eval.getASTFactory());
			int n = 0;
			try
			{
				n = eval.action(exprTree); // eval and write out tree
			}
			catch (RecognitionException re)
			{
				self.Error("can't evaluate tree: " + exprTree.ToStringList(), re);
			}
			output.PopIndentation();
			return n;
		}
		
		// HELP ROUTINES CALLED BY EVALUATOR TREE WALKER
		
		/// <summary>
		/// For <names,phones:{n,p | ...}> treat the names, phones as lists
		/// to be walked in lock step as n=names[i], p=phones[i].
		/// </summary>
		public virtual object ApplyTemplateToListOfAttributes(StringTemplate self, IList attributes, StringTemplate templateToApply)
		{
			if (attributes == null || templateToApply == null || attributes.Count == 0)
			{
				return null; // do not apply if missing templates or empty values
			}
			IDictionary argumentContext = null;
			IList results = new ArrayList();
			
			// convert all attributes to iterators even if just one value
			for (int a = 0; a < attributes.Count; a++)
			{
				object o = (object) attributes[a];
				if (o != null)
				{
					o = ConvertAnythingToIterator(o);
					attributes[a] = o; // alter the list in place
				}
			}
			
			int numAttributes = attributes.Count;
			
			// ensure arguments line up
			HashList formalArguments = (HashList) templateToApply.FormalArguments;
			if (formalArguments == null || formalArguments.Count == 0)
			{
				self.Error("missing arguments in anonymous" + " template in context " + self.GetEnclosingInstanceStackString());
				return null;
			}
			object[] formalArgumentNames = new object[formalArguments.Count];
			formalArguments.Keys.CopyTo(formalArgumentNames, 0);
			if (formalArgumentNames.Length != numAttributes)
			{
				self.Error("number of arguments " + formalArguments.Keys.ToString() + " mismatch between attribute list and anonymous" + " template in context " + self.GetEnclosingInstanceStackString());
				// truncate arg list to match smaller size
				int shorterSize = System.Math.Min(formalArgumentNames.Length, numAttributes);
				numAttributes = shorterSize;
				object[] newFormalArgumentNames = new object[shorterSize];
				Array.Copy(formalArgumentNames, 0, newFormalArgumentNames, 0, shorterSize);
				formalArgumentNames = newFormalArgumentNames;
			}
			
			// keep walking while at least one attribute has values
			while (true)
			{
				argumentContext = new Hashtable();
				// get a value for each attribute in list; put into arg context
				// to simulate template invocation of anonymous template
				int numEmpty = 0;
				for (int a = 0; a < numAttributes; a++)
				{
					IEnumerator it = (IEnumerator) attributes[a];
					if ((it != null) && it.MoveNext())
					{
						string argName = (string) formalArgumentNames[a];
						object iteratedValue = it.Current;
						argumentContext[argName] = iteratedValue;
					}
					else
					{
						numEmpty++;
					}
				}
				if (numEmpty == numAttributes)
				{
					break;
				}
				StringTemplate embedded = templateToApply.GetInstanceOf();
				embedded.EnclosingInstance = self;
				embedded.ArgumentContext = argumentContext;
				results.Add(embedded);
			}
			
			return results;
		}
		
		public virtual object ApplyListOfAlternatingTemplates(StringTemplate self, object attributeValue, IList templatesToApply)
		{
			if (attributeValue == null || templatesToApply == null || templatesToApply.Count == 0)
			{
				return null; // do not apply if missing templates or empty value
			}
			StringTemplate embedded = null;
			IDictionary argumentContext = null;
			
			// normalize collections and such to use iterators
			// anything iteratable can be used for "APPLY"
			attributeValue = ConvertAnythingIteratableToIterator(attributeValue);
			
			if (attributeValue is IEnumerator)
			{
				IList resultVector = new ArrayList();
				IEnumerator iter = (IEnumerator) attributeValue;
				int i = 0;
				while (iter.MoveNext())
				{
					object ithValue = iter.Current;
					if (ithValue == null)
					{
						// weird...a null value in the list; ignore
						continue;
					}
					int templateIndex = i % templatesToApply.Count; // rotate through
					embedded = (StringTemplate) templatesToApply[templateIndex];
					// template to apply is an actual StringTemplate (created in
					// eval.g), but that is used as the examplar.  We must create
					// a new instance of the embedded template to apply each time
					// to get new attribute sets etc...
					StringTemplateAST args = embedded.ArgumentsAST;
					embedded = embedded.GetInstanceOf(); // make new instance
					embedded.EnclosingInstance = self;
					embedded.ArgumentsAST = args;

					argumentContext = new Hashtable();
					SetSoleFormalArgumentToIthValue(embedded, argumentContext, ithValue);
					argumentContext[DEFAULT_ATTRIBUTE_NAME] = ithValue;
					argumentContext[DEFAULT_ATTRIBUTE_NAME_DEPRECATED] = ithValue;
					argumentContext[DEFAULT_INDEX_VARIABLE_NAME] = (System.Int32) (i + 1);
					embedded.ArgumentContext = argumentContext;
					EvaluateArguments(embedded);
					/*
					System.err.println("i="+i+": applyTemplate("+embedded.getName()+
					", args="+argumentContext+
					" to attribute value "+ithValue);
					*/
					resultVector.Add(embedded);
					i++;
				}
				if (resultVector.Count == 0)
				{
					resultVector = null;
				}
				return resultVector;
			}
			else
			{
				/*
				System.out.println("setting attribute "+DEFAULT_ATTRIBUTE_NAME+" in arg context of "+
				embedded.getName()+
				" to "+attributeValue);
				*/
				embedded = (StringTemplate) templatesToApply[0];
				argumentContext = new Hashtable();
				SetSoleFormalArgumentToIthValue(embedded, argumentContext, attributeValue);
				argumentContext[DEFAULT_ATTRIBUTE_NAME] = attributeValue;
				argumentContext[DEFAULT_ATTRIBUTE_NAME_DEPRECATED] = attributeValue;
				argumentContext[DEFAULT_INDEX_VARIABLE_NAME] = 1;
				embedded.ArgumentContext = argumentContext;
				EvaluateArguments(embedded);
				return embedded;
			}
		}
		
		protected internal virtual void  SetSoleFormalArgumentToIthValue(StringTemplate embedded, IDictionary argumentContext, object ithValue)
		{
			IDictionary formalArgs = embedded.FormalArguments;
			if (formalArgs != null)
			{
				string soleArgName = null;
				bool isAnonymous = embedded.Name.Equals(StringTemplate.ANONYMOUS_ST_NAME);
				if (formalArgs.Count == 1 || (isAnonymous && formalArgs.Count > 0))
				{
					if (isAnonymous && formalArgs.Count > 1)
					{
						embedded.Error("too many arguments on {...} template: " + CollectionUtils.DictionaryToString(formalArgs));
					}
					// if exactly 1 arg or anonymous, give that the value of
					// "it" as a convenience like they said
					// $list:template(arg=it)$
					IEnumerator iter = formalArgs.Keys.GetEnumerator();
					// Retrieve first key
					iter.MoveNext();
					soleArgName = (string) iter.Current;
					argumentContext[soleArgName] = ithValue;
				}
			}
		}

		/// <summary>
		/// Return o.getPropertyName() given o and propertyName.  If o is
		/// a stringtemplate then access it's attributes looking for propertyName
		/// instead (don't check any of the enclosing scopes; look directly into
		/// that object).  Also try isXXX() for booleans.  Allow HashMap,
		/// Hashtable as special case (grab value for key).
		/// </summary>
		public virtual object GetObjectProperty(StringTemplate self, object o, string propertyName)
		{
			if (o == null || propertyName == null)
			{
				return null;
			}
			Type c = o.GetType();
			object val = null;
			
			// Special case: our automatically created Aggregates via
			// attribute name: "{obj.{prop1,prop2}}"
			if (c == typeof(StringTemplate.Aggregate))
			{
				val = ((StringTemplate.Aggregate) o).Get(propertyName);
			}
				// Special case: if it's a template, pull property from
				// it's attribute table.
				// TODO: TJP just asked himself why we can't do inherited attr here?
			else if (c == typeof(StringTemplate))
			{
				IDictionary attributes = ((StringTemplate) o).Attributes;
				if (attributes != null)
				{
					val = attributes[propertyName];
				}
			}
				// Special case: if it's a HashMap, Hashtable then pull using
				// key not the property method.  Do NOT allow general Map interface
				// as people could pass in their database masquerading as a Map.
			else if (typeof(IDictionary).IsAssignableFrom(c))
			{
				IDictionary map = (IDictionary) o;
				val = map[propertyName];
				if (val == null)
				{
					// no property defined; if a map in this group
					// then there may be a default value
					val = map[DEFAULT_MAP_VALUE_NAME];
				}
			}
			else
			{
				// Search for propertyName as: 
				//   Property (non-indexed), Method(get_XXX, GetXXX, IsXXX, getXXX, isXXX), Field, this[string]
				string methodSuffix = System.Char.ToUpper(propertyName[0]) + propertyName.Substring(1);
				if (!GetPropertyValueByName(self, c, o, methodSuffix, ref val))
				{
					bool found = false;
					foreach (string prefix in new string[] { "get_", "Get", "Is", "get", "is" })
					{
						if (found = GetMethodValueByName(self, c, o, prefix + methodSuffix, ref val))
							break;
					}

					if (!found)
					{
						if (!GetFieldValueByName(self, c, o, methodSuffix, ref val))
						{
							PropertyInfo pi = c.GetProperty("Item", new Type[] { typeof(string) });
							if (pi != null)
							{
								try
								{
									val = pi.GetValue(o, new object[] { propertyName });
								}
								catch(Exception ex)
								{
									self.Error("Can't get property " + propertyName + " via C# string indexer from " + c.FullName + " instance", ex);
								}
							}
							else
							{
								self.Error("Class " + c.FullName + " has no such attribute: " + propertyName + " in template context " + self.GetEnclosingInstanceStackString());
							}
						}
					}
				}
			}
			return val;
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
		/// </summary>
		public virtual bool TestAttributeTrue(object a)
		{
			if (a == null)
			{
				return false;
			}
			if (a is System.Boolean)
			{
				return ((System.Boolean) a);
			}
			if (a is System.Collections.ICollection)
			{
				return ((System.Collections.ICollection) a).Count > 0;
			}
			if (a is IDictionary)
			{
				return ((IDictionary) a).Count > 0;
			}
			if (a is IEnumerator)
			{
				return ((IEnumerator) a).MoveNext();
			}
			return true; // any other non-null object, return true--it's present
		}
		
		/// <summary>For now, we can only add two objects as strings; convert objects to
		/// Strings then cat.
		/// </summary>
		public virtual object Add(object a, object b)
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
		
		/// <summary>
		/// Call a string template with args and return result.  Do not convert
		/// to a string yet.  It may need attributes that will be available after
		/// this is inserted into another template.
		/// </summary>
		public virtual StringTemplate GetTemplateInclude(StringTemplate enclosing, string templateName, StringTemplateAST argumentsAST)
		{
			//Console.WriteLine("getTemplateInclude: look up "+enclosing.Group.Name+"::"+templateName);
			StringTemplateGroup group = enclosing.Group;
			StringTemplate embedded = group.GetEmbeddedInstanceOf(enclosing, templateName);
			if (embedded == null)
			{
				enclosing.Error("cannot make embedded instance of " + templateName + " in template " + enclosing.Name);
				return null;
			}
			embedded.ArgumentsAST = argumentsAST;
			EvaluateArguments(embedded);
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
		public virtual int WriteAttribute(StringTemplate self, object o, IStringTemplateWriter output)
		{
			object separator = null;
			if (options != null)
			{
				separator = options["separator"];
			}
			return Write(self, o, output, separator);
		}
		
		protected internal virtual int Write(StringTemplate self, object o, IStringTemplateWriter output, object separator)
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
					stToWrite.EnclosingInstance = self;
					// if self is found up the enclosing instance chain, then
					// infinite recursion
					if (StringTemplate.IsInLintMode && StringTemplate.IsRecursiveEnclosingInstance(stToWrite))
					{
						// throw exception since sometimes eval keeps going
						// even after I ignore this write of o.
						throw new System.SystemException("infinite recursion to " + stToWrite.GetTemplateDeclaratorString() + " referenced in " + stToWrite.EnclosingInstance.GetTemplateDeclaratorString() + "; stack trace:\n" + stToWrite.GetEnclosingInstanceStackTrace());
					}
					else
					{
						n = stToWrite.Write(output);
					}
					return n;
				}
				// normalize anything iteratable to iterator
				o = ConvertAnythingIteratableToIterator(o);
				if (o is IEnumerator)
				{
					IEnumerator iter = (IEnumerator) o;
					bool processingFirstItem = true;
					int charWrittenForValue = 0;
					string separatorString = null;
					if (separator != null)
					{
						separatorString = ComputeSeparator(self, output, separator);
					}
					while (iter.MoveNext())
					{
						object iterValue = iter.Current;
						if (!processingFirstItem)
						{
							bool valueIsPureConditional = false;
							if (iterValue is StringTemplate)
							{
								StringTemplate iterValueST = (StringTemplate) iterValue;
								IList chunks = (IList) iterValueST.Chunks;
								Expr firstChunk = (Expr) chunks[0];
								valueIsPureConditional = firstChunk is ConditionalExpr && ((ConditionalExpr) firstChunk).ElseSubtemplate == null;
							}
							bool emptyIteratedValue = valueIsPureConditional && charWrittenForValue == 0;
							if (!emptyIteratedValue && separator != null)
							{
								n += output.Write(separatorString);
							}
						}
						charWrittenForValue = Write(self, iterValue, output, separator);
						n += charWrittenForValue;
						processingFirstItem = false;
					}
				}
				else
				{
					IAttributeRenderer renderer = self.GetAttributeRenderer(o.GetType());
					if (renderer != null)
					{
						n = output.Write(renderer.ToString(o));
					}
					else
					{
						n = output.Write(o.ToString());
					}
					return n;
				}
			}
			catch (System.IO.IOException io)
			{
				self.Error("problem writing object: " + o, io);
			}
			return n;
		}
		
		/// <summary>A separator is normally just a string literal, but is still an AST that
		/// we must evaluate.  The separator can be any expression such as a template
		/// include or string cat expression etc...
		/// </summary>
		protected internal virtual string ComputeSeparator(StringTemplate self, IStringTemplateWriter output, object separator)
		{
			if (separator == null)
			{
				return null;
			}
			if (separator is StringTemplateAST)
			{
				StringTemplateAST separatorTree = (StringTemplateAST) separator;
				// must evaluate, writing to a string so we can hand on to it
				ASTExpr e = new ASTExpr(EnclosingTemplate, separatorTree, null);
				StringWriter buf = new StringWriter();
				// create a new instance of whatever IStringTemplateWriter
				// implementation they are using.  Default is AutoIndentWriter.
				// Defalut behavior is to indent but without
				// any prior indents surrounding this attribute expression
				IStringTemplateWriter sw = null;
				System.Type writerClass = output.GetType();
				try
				{
					//UPGRADE_ISSUE: Class hierarchy differences between 'java.io.Writer' and 'System.IO.StreamWriter' may cause compilation errors. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1186_3"'
					System.Reflection.ConstructorInfo ctor = writerClass.GetConstructor(new System.Type[]{typeof(System.IO.StreamWriter)});
					sw = (IStringTemplateWriter) ctor.Invoke(new object[]{buf});
				}
				catch (System.Exception exc)
				{
					// default new AutoIndentWriter(buf)
					self.Error("cannot make implementation of IStringTemplateWriter", exc);
					sw = new AutoIndentWriter(buf);
				}
				
				try
				{
					e.Write(self, sw);
				}
				catch (System.IO.IOException ioe)
				{
					self.Error("can't evaluate separator expression", ioe);
				}
				return buf.ToString();
			}
			else
			{
				// just in case we expand in the future and it's something else
				return separator.ToString();
			}
		}
		
		/// <summary>Evaluate an argument list within the context of the enclosing
		/// template but store the values in the context of self, the
		/// new embedded template.  For example, bold(item=item) means
		/// that bold.item should get the value of enclosing.item.
		/// </summary>
		protected internal virtual void  EvaluateArguments(StringTemplate self)
		{
			StringTemplateAST argumentsAST = self.ArgumentsAST;
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
			StringTemplate enclosing = self.EnclosingInstance;
			StringTemplate argContextST = new StringTemplate(self.Group, "");
			argContextST.Name = "<invoke " + self.Name + " arg context>";
			argContextST.EnclosingInstance = enclosing;
			argContextST.ArgumentContext = self.ArgumentContext;
			
			ActionEvaluator eval = new ActionEvaluator(argContextST, this, null);
			ActionParser.initializeASTFactory(eval.getASTFactory());
			/*
			System.out.println("eval args: "+argumentsAST.toStringList());
			System.out.println("ctx is "+self.getArgumentContext());
			*/
			try
			{
				// using any initial argument context (such as when obj is set),
				// evaluate the arg list like bold(item=obj).  Since we pass
				// in any existing arg context, that context gets filled with
				// new values.  With bold(item=obj), context becomes:
				// {[obj=...],[item=...]}.
				IDictionary ac = eval.argList(argumentsAST, self, self.ArgumentContext);
				self.ArgumentContext = ac;
			}
			catch (RecognitionException re)
			{
				self.Error("can't evaluate tree: " + argumentsAST.ToStringList(), re);
			}
		}
		
		private static object ConvertAnythingIteratableToIterator(object o)
		{
			IEnumerator iter = null;
			if (o is IDictionary)
			{
				iter = ((IDictionary) o).Values.GetEnumerator();
			}
			else if (o is ICollection)
			{
				iter = ((ICollection) o).GetEnumerator();
			}
			else if (o is IEnumerator)
			{
				iter = (IEnumerator) o;
			}
			if (iter == null)
			{
				return o;
			}
			return iter;
		}
		
		internal static IEnumerator ConvertAnythingToIterator(object o)
		{
			IEnumerator iter = null;
			if (o is IDictionary)
			{
				iter = ((IDictionary) o).Values.GetEnumerator();
			}
			else if (o is ICollection)
			{
				iter = ((ICollection) o).GetEnumerator();
			}
			else if (o is IEnumerator)
			{
				iter = (IEnumerator) o;
			}
			if (iter == null)
			{
				IList singleton = new ArrayList(1);
				singleton.Add(o);
				return singleton.GetEnumerator();
			}
			return iter;
		}
		
		/// <summary>Return the first attribute if multiple valued or the attribute
		/// itself if single-valued.  Used in <names:first()>
		/// </summary>
		public virtual object First(object attribute)
		{
			if (attribute == null)
			{
				return null;
			}
			object f = attribute;
			attribute = ConvertAnythingIteratableToIterator(attribute);
			if (attribute is IEnumerator)
			{
				IEnumerator it = (IEnumerator) attribute;
				if (it.MoveNext())
				{
					f = it.Current;
				}
			}
			
			return f;
		}
		
		/// <summary>Return the everything but the first attribute if multiple valued
		/// or null if single-valued.  Used in <names:rest()>.
		/// </summary>
		public virtual object Rest(object attribute)
		{
			if (attribute == null)
			{
				return null;
			}
			object theRest = attribute;
			attribute = ConvertAnythingIteratableToIterator(attribute);
			if (attribute is IEnumerator)
			{
				IEnumerator it = (IEnumerator) attribute;
				if (!it.MoveNext())
				{
					return null; // if not even one value return null
				}
				theRest = it; // return suitably altered iterator
				//				theRest = new ArrayList();
				//				while (it.MoveNext())
				//					((ArrayList) theRest).Add(it.Current);
			}
			else
			{
				theRest = null; // rest of single-valued attribute is null
			}
			
			return theRest;
		}
		
		/// <summary>Return the last attribute if multiple valued or the attribute
		/// itself if single-valued.  Used in <names:last()>.  This is pretty
		/// slow as it iterates until the last element.  Ultimately, I could
		/// make a special case for a List or Vector.
		/// </summary>
		public virtual object Last(object attribute)
		{
			if (attribute == null)
			{
				return null;
			}
			object last = attribute;
			attribute = ConvertAnythingIteratableToIterator(attribute);
			if (attribute is IEnumerator)
			{
				IEnumerator it = (IEnumerator) attribute;
				while (it.MoveNext())
				{
					last = it.Current;
				}
			}
			return last;
		}

		public override string ToString()
		{
			return exprTree.ToStringList();
		}

		#region Private Helper Methods

		private bool GetMethodValueByName(StringTemplate self, Type prototype, object instance, string methodName, ref object val)
		{
			MethodInfo mi = null;
			try 
			{
				mi = prototype.GetMethod(
					methodName,
					BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.IgnoreCase,
					null,
					Type.EmptyTypes,
					null
					);

				if (mi != null)
				{
					try
					{
						val = mi.Invoke(instance, (object[]) null);
						return true;
					}
					catch(Exception ex)
					{
						self.Error("Can't get property " + methodName + " using method get_/Get/Is/get/is" + methodName + " from " + prototype.FullName + " instance", ex);
					}
				}
			}
			catch(Exception)
			{
				;
			}
			return false;
		}

		private bool GetPropertyValueByName(StringTemplate self, Type prototype, object instance, string propertyName, ref object val)
		{
			PropertyInfo pi = null;
			try 
			{
				pi = prototype.GetProperty(
					propertyName,
					BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.IgnoreCase,
					null,
					null,
					Type.EmptyTypes,
					null
					);

				if (pi != null)
				{
					if (pi.CanRead)
					{
						try
						{
							val = pi.GetValue(instance, (object[]) null);
							return true;
						}
						catch(Exception ex)
						{
							self.Error("Can't get property " + propertyName + " as CLR property " + propertyName + " from " + prototype.FullName + " instance", ex);
						}
					}
					else
					{
						self.Error("Class " + prototype.FullName + " property: " + propertyName + " is write-only in template context " + self.GetEnclosingInstanceStackString());
					}
				}
			}
			catch(Exception)
			{
				;
			}
			return false;
		}

		private bool GetFieldValueByName(StringTemplate self, Type prototype, object instance, string fieldName, ref object val)
		{
			FieldInfo fi = null;
			try 
			{
				fi = prototype.GetField(
					fieldName,
					BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.IgnoreCase
					);

				if (fi != null)
				{
					try
					{
						val = fi.GetValue(instance);
						return true;
					}
					catch(Exception ex)
					{
						self.Error("Can't get property " + fieldName + " using direct field access from " + prototype.FullName + " instance", ex);
					}
				}
			}
			catch(Exception)
			{
				;
			}
			return false;
		}

		#endregion
	}
}