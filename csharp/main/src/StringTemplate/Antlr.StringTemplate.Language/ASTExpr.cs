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
	using TextWriter				= System.IO.TextWriter;
	using IOException				= System.IO.IOException;
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
	using ConstructorInfo			= System.Reflection.ConstructorInfo;
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
		internal sealed class PropertyLookupParams
		{
			public PropertyLookupParams(StringTemplate s, Type p, object i, string pn, string l)
			{
				SetParams(s, p, i, pn, l);
			}
			public void SetParams(StringTemplate s, Type p, object i, string pn, string l)
			{
				self = s;
				prototype = p;
				instance = i;
				propertyName = pn;
				lookupName = l;
			}

			public StringTemplate self;
			public Type prototype;
			public object instance;
			public string propertyName;
			public string lookupName;
		}

		public const string DEFAULT_ATTRIBUTE_NAME = "it";
		public const string DEFAULT_ATTRIBUTE_NAME_DEPRECATED = "attr";
		public const string DEFAULT_INDEX_VARIABLE_NAME = "i";
		public const string DEFAULT_INDEX0_VARIABLE_NAME = "i0";
		public const string DEFAULT_MAP_VALUE_NAME = "_default_";
		public const string DEFAULT_MAP_KEY_NAME = "key";

		// Used to indicate "default:key" in maps within groups
		public static readonly StringTemplate MAP_KEY_VALUE = new StringTemplate();

		/// <summary>
		/// Using an expr option w/o value, makes options table hold EMPTY_OPTION
		/// value for that key.
		/// </summary>
	public static readonly string EMPTY_OPTION = "empty expr option";

	public static readonly IDictionary defaultOptionValues = new HybridDictionary();

		// used temporarily for checking obj.prop cache
		public static int totalObjPropRefs = 0;
		public static int totalReflectionLookups = 0;

		protected AST exprTree = null;

		/// <summary>store separator etc... </summary>
		private IDictionary options = null;

		/// <summary>
		/// A cached value of wrap=expr from the <![CDATA[<...>]]> expression.
		/// Computed in Write(StringTemplate, IStringTemplateWriter) and used
		/// in WriteAttribute.
		/// </summary>
		string wrapString = null;

		/// <summary>
		/// For null values in iterated attributes and single attributes that
		/// are null, use this value instead of skipping.
		/// 
		/// For single valued
		/// attributes like <![CDATA[<name; null="n/a">]]> it's a shorthand for
		/// <![CDATA[<if(name)><name><else>n/a<endif>]]>
		/// 
		/// For iterated values <![CDATA[<values; null="0", separator=",">]]>, 
		/// you get 0 for for null list values.  
		/// 
		/// Works for template application like: 
		/// <![CDATA[<values:{v| <v>}; null="0">]]> also.
		/// </summary>
		string nullValue = null;

		/// <summary>
		/// A cached value of separator=expr from the <![CDATA[<...>]]> expression.
		/// Computed in Write(StringTemplate, IStringTemplateWriter) and used in
		/// WriteAttribute.
		/// </summary>
		string separatorString = null;

		static ASTExpr()
		{
			defaultOptionValues.Add("anchor", new StringTemplateAST(ActionEvaluator.STRING, "true"));
			defaultOptionValues.Add("wrap",   new StringTemplateAST(ActionEvaluator.STRING, "\n"));
		}

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
		
		/// <summary>
		/// To write out the value of an ASTExpr, invoke the evaluator in eval.g
		/// to walk the tree writing out the values.  For efficiency, don't
		/// compute a bunch of strings and then pack them together.  Write out directly.
		/// 
		/// Compute separator and wrap expressions, save as strings so we don't
		/// recompute for each value in a multi-valued attribute or expression.
		/// 
		/// If they set anchor option, then inform the writer to push current
		/// char position.
		/// </summary>
		public override int Write(StringTemplate self, IStringTemplateWriter output)
		{
			if (exprTree == null || self == null || output == null)
			{
				return 0;
			}
			output.PushIndentation(Indentation);
			// handle options, anchor, wrap, separator...
			StringTemplateAST anchorAST = (StringTemplateAST)GetOption("anchor");
			if (anchorAST != null)	// any non-empty expr means true; check presence
			{
				output.PushAnchorPoint();
			}
			HandleExprOptions(self);
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
			if (anchorAST != null)
			{
				output.PopAnchorPoint();
			}
			return n;
		}

		private void HandleExprOptions(StringTemplate self)
		{
			StringTemplateAST wrapAST = (StringTemplateAST)GetOption("wrap");
			if (wrapAST != null)
			{
				wrapString = EvaluateExpression(self, wrapAST);
			}
			StringTemplateAST nullValueAST = (StringTemplateAST)GetOption("null");
			if (nullValueAST != null)
			{
				nullValue = EvaluateExpression(self, nullValueAST);
			}
			StringTemplateAST separatorAST = (StringTemplateAST)GetOption("separator");
			if (separatorAST != null)
			{
				separatorString = EvaluateExpression(self, separatorAST);
			}
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
			IList results = new StringTemplate.STAttributeList();
			
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
				int shorterSize = Math.Min(formalArgumentNames.Length, numAttributes);
				numAttributes = shorterSize;
				object[] newFormalArgumentNames = new object[shorterSize];
				Array.Copy(formalArgumentNames, 0, newFormalArgumentNames, 0, shorterSize);
				formalArgumentNames = newFormalArgumentNames;
			}
			
			// keep walking while at least one attribute has values
			int i = 0;		// iteration number from 0
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
				argumentContext[DEFAULT_INDEX_VARIABLE_NAME] = i+1;
				argumentContext[DEFAULT_INDEX0_VARIABLE_NAME] = i;
				StringTemplate embedded = templateToApply.GetInstanceOf();
				embedded.EnclosingInstance = self;
				embedded.ArgumentContext = argumentContext;
				results.Add(embedded);
				i++;
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
			// TODO: 2006-08-05 - Needed?: attributeValue = convertArrayToList(attributeValue);
			attributeValue = ConvertAnythingIteratableToIterator(attributeValue);
			
			bool isAnonymous;
			bool hasFormalArgument;
			if (attributeValue is IEnumerator)
			{
				// results can be treated list an attribute, indicate ST created list
				IList resultVector = new StringTemplate.STAttributeList();
				IEnumerator iter = (IEnumerator) attributeValue;
				int i = 0;
				while (iter.MoveNext())
				{
					object ithValue = iter.Current;
					if (ithValue == null)
					{
						if (nullValue == null)
						{
							continue;
						}
						ithValue = nullValue;
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
					isAnonymous = (embedded.Name == StringTemplate.ANONYMOUS_ST_NAME);
					IDictionary formalArgs = embedded.FormalArguments;
					SetSoleFormalArgumentToIthValue(embedded, argumentContext, ithValue);
					hasFormalArgument = ((formalArgs != null) && (formalArgs.Count > 0));
					// if it's an anonymous template with a formal arg, don't set it/attr
					if ( !(isAnonymous && hasFormalArgument) ) 
					{
						argumentContext[DEFAULT_ATTRIBUTE_NAME] = ithValue;
						argumentContext[DEFAULT_ATTRIBUTE_NAME_DEPRECATED] = ithValue;
					}
					argumentContext[DEFAULT_INDEX_VARIABLE_NAME] = (i + 1);
					argumentContext[DEFAULT_INDEX0_VARIABLE_NAME] = i;
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
				IDictionary formalArgs = embedded.FormalArguments;
				SetSoleFormalArgumentToIthValue(embedded, argumentContext, attributeValue);
				isAnonymous = (embedded.Name == StringTemplate.ANONYMOUS_ST_NAME);
				hasFormalArgument = ((formalArgs != null) && (formalArgs.Count > 0));
				// if it's an anonymous template with a formal arg, don't set it/attr
				if ( !(isAnonymous && hasFormalArgument) ) 
				{
					argumentContext[DEFAULT_ATTRIBUTE_NAME] = attributeValue;
					argumentContext[DEFAULT_ATTRIBUTE_NAME_DEPRECATED] = attributeValue;
				}
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
				bool isAnonymous = (embedded.Name == StringTemplate.ANONYMOUS_ST_NAME);
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
		/// that object).  Also try isXXX() for booleans.  Allow IDictionary as
		/// special case (grab value for key).
		/// 
		/// Cache repeated requests for obj.prop within same group.
		/// </summary>
		public virtual object GetObjectProperty(StringTemplate self, object o, string propertyName)
		{
			if ( o==null || propertyName==null ) 
			{
				return null;
			}
			totalObjPropRefs++;
			object valueObj = null;
            IAttributeStrategy strategy = enclosingTemplate.Group.AttributeStrategy;
            if (strategy != null && strategy.UseCustomGetObjectProperty)
                valueObj = strategy.GetObjectProperty(self, o, propertyName);
            else
                valueObj = RawGetObjectProperty(self, o, propertyName);
			// take care of array properties...convert to a List so we can
			// apply templates to the elements etc...

			//TODO: Decide if arrays should be converted to STAttributeList
//			if (typeof(Array).IsAssignableFrom(valueObj.GetType()))
//			{
//				// convert arrays to STAttributeList instances so we know we created these
//				valueObj = StringTemplate.STAttributeList((ICollection)valueObj);
//			}
			return valueObj;
		}

		protected object RawGetObjectProperty(StringTemplate self, object o, string propertyName) 
		{
			Type c = o.GetType();
			object val = null;
			
			// Special case: our automatically created Aggregates via
			// attribute name: "{obj.{prop1,prop2}}"
			if (c == typeof(StringTemplate.Aggregate))
			{
				val = ((StringTemplate.Aggregate) o).Get(propertyName);
				return val;
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
					return val;
				}
			}

			// Special case: if it's an IDictionary then pull using
			// key not the property method.  
			//
			// Original intent was to DISALLOW general IDictionary interface
			// as people could pass in their database masquerading as a IDictionary.
			// Pragmatism won out ;-)
			if (typeof(IDictionary).IsAssignableFrom(c))
			{
				IDictionary map = (IDictionary) o;
				if (propertyName.Equals("keys"))
				{
					val = map.Keys;
				}
				else if (propertyName.Equals("values"))
				{
					val = map.Values;
				}
				else if (map.Contains(propertyName)) 
				{
					val = map[propertyName];
				}
				else
				{
					if ( map.Contains(DEFAULT_MAP_VALUE_NAME) )
						val = map[DEFAULT_MAP_VALUE_NAME];
				}
				if (val == MAP_KEY_VALUE)
				{
					// no property defined; if a map in this group
					// then there may be a default value
					val = propertyName;
				}
				return val;
			}
			
			// try getXXX and isXXX properties

			// check cache
			PropertyLookupParams paramBag;
			//MemberInfo cachedMember = self.Group.GetCachedClassProperty(c, propertyName);
			//if ( cachedMember != null ) 
			//{
			//    try 
			//    {
			//        paramBag = new PropertyLookupParams(self, c, o, propertyName, null);
			//        if ( cachedMember is PropertyInfo ) 
			//        {
			//            // non-indexed property (since we don't cache indexers)
			//            PropertyInfo pi = (PropertyInfo)cachedMember;
			//            GetPropertyValue(pi, paramBag, ref val);
			//        }
			//        else if ( cachedMember is MethodInfo ) 
			//        {
			//            MethodInfo mi = (MethodInfo)cachedMember;
			//            GetMethodValue(mi, paramBag, ref val);
			//        }
			//        else if ( cachedMember is FieldInfo ) 
			//        {
			//            // must be a field
			//            FieldInfo fi = (FieldInfo)cachedMember;
			//            GetFieldValue(fi, paramBag, ref val);
			//        }
			//    }
			//    catch (Exception e) 
			//    {
			//        self.Error("Can't get property '" + propertyName +
			//            "' from '" + c.FullName + "' instance", e);
			//    }
			//    return val;
			//}

			// must look up using reflection
			// Search for propertyName as: 
			//   Property (non-indexed), Method(get_XXX, GetXXX, IsXXX, getXXX, isXXX), Field, this[string]
			string methodSuffix = Char.ToUpper(propertyName[0]) + propertyName.Substring(1);
			paramBag = new PropertyLookupParams(self, c, o, propertyName, methodSuffix);
			totalReflectionLookups++;
			if (!GetPropertyValueByName(paramBag, ref val))
			{
				bool found = false;
				foreach (string prefix in new string[] { "get_", "Get", "Is", "get", "is" })
				{
					paramBag.lookupName = prefix + methodSuffix;
					totalReflectionLookups++;
					if (found = GetMethodValueByName(paramBag, ref val))
						break;
				}

				if (!found)
				{
					paramBag.lookupName = methodSuffix;
					totalReflectionLookups++;
					if (!GetFieldValueByName(paramBag, ref val))
					{
						totalReflectionLookups++;
						PropertyInfo pi = c.GetProperty("Item", new Type[] { typeof(string) });
						if (pi != null)
						{
							// TODO: we don't cache indexer PropertyInfo objects (yet?)
							// it would have complicated getting values from cached property objects
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
			
			return val;
		}
		
		/// <summary>
		/// Normally StringTemplate tests presence or absence of attributes
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
            IAttributeStrategy strategy = enclosingTemplate.Group.AttributeStrategy;
            if (strategy != null && strategy.UseCustomTestAttributeTrue)
                return strategy.TestAttributeTrue(a);
            else
            {
				if (a == null)
				{
					return false;
				}
				if (a is bool)
				{
					return ((bool) a);
				}
				if (a is ICollection)
				{
					return ((ICollection) a).Count > 0;
				}
				if (a is IDictionary)
				{
					return ((IDictionary) a).Count > 0;
				}
				if (a is IEnumerator)
				{
					return !CollectionUtils.IsEmptyEnumerator((IEnumerator)a);
				}
				return true; // any other non-null object, return true--it's present
            }
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
			return Write(self, o, output);
		}
		
		protected internal virtual int Write(StringTemplate self, object o, IStringTemplateWriter output)
		{
			if (o == null)
			{
				if (nullValue == null)
				{
					return 0;
				}
				// continue with null option if specified
				o = nullValue;
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
						throw new SystemException("infinite recursion to " + stToWrite.GetTemplateDeclaratorString() + " referenced in " + stToWrite.EnclosingInstance.GetTemplateDeclaratorString() + "; stack trace:\n" + stToWrite.GetEnclosingInstanceStackTrace());
					}
					else
					{
						// if we have a wrap string, then inform writer it
						// might need to wrap
						if (wrapString != null)
						{
							n = output.WriteWrapSeparator(wrapString);
						}
						n = stToWrite.Write(output);
					}
					return n;
				}
				// normalize anything iteratable to iterator
				o = ConvertAnythingIteratableToIterator(o);
				if (o is IEnumerator)
				{
					IEnumerator iter = (IEnumerator)o;
					object prevIterValue = null;
					bool seenPrevValue = false;
					while (iter.MoveNext())
					{
						object iterValue = iter.Current;
						if (iterValue == null)
						{
							iterValue = nullValue;
						}
						if (iterValue != null)
						{
							if (seenPrevValue /*prevIterValue!=null*/ && (separatorString != null))
							{
								n += output.WriteSeparator(separatorString);
							}
							seenPrevValue = true;
							int nw = Write(self, iterValue, output);
							n += nw;
						}
						prevIterValue = iterValue;
					}
				}
				else
				{
					IAttributeRenderer renderer = self.GetAttributeRenderer(o.GetType());
					string v = null;
					if (renderer != null)
					{
						v = renderer.ToString(o);
					}
					else
					{
						v = o.ToString();
					}

					if (wrapString != null)
					{
						n = output.Write(v, wrapString);
					}
					else
					{
						n = output.Write(v);
					}
					return n;
				}
			}
			catch (IOException io)
			{
				self.Error("problem writing object: " + o, io);
			}
			return n;
		}
		
		/// <summary>
		/// An EXPR is normally just a string literal, but is still an AST that
		/// we must evaluate.  The EXPR can be any expression such as a template
		/// include or string cat expression etc...
		/// 
		/// Evaluate with its own writer so that we can convert to string and then 
		/// reuse, don't want to compute all the time; must precompute w/o writing 
		/// to output buffer.
		/// </summary>
		protected internal string EvaluateExpression(StringTemplate self, object expr)
		{
			if (expr == null)
			{
				return null;
			}
			if (expr is StringTemplateAST)
			{
				StringTemplateAST exprTree = (StringTemplateAST) expr;
				// must evaluate, writing to a string so we can hand on to it
				StringWriter buf = new StringWriter();
				IStringTemplateWriter sw = self.group.CreateInstanceOfTemplateWriter(buf);
				ActionEvaluator eval = new ActionEvaluator(self, this, sw);
				int n = 0;

				
				try
				{
					n = eval.action(exprTree);	// eval tree
				}
				catch (RecognitionException ex)
				{
					self.Error("can't evaluate tree: " + exprTree.ToStringList(), ex);
				}
				return buf.ToString();
			}
			else
			{
				// just in case we expand in the future and it's something else
				return expr.ToString();
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
				IList singleton = new StringTemplate.STAttributeList(1);
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

			if (attribute is ICollection)
			{
				if ( ((ICollection) attribute).Count > 1 )
					return new RestCollection((ICollection) attribute);
			}
			else if (attribute is IEnumerator)
			{
				return new RestCollection((IEnumerator) attribute);
			}
			return null; // rest of single-valued (i.e. all non-ICollection object) attribute is null
		}
		
		/// <summary>
		/// Return the last attribute if multiple valued or the attribute
		/// itself if single-valued.  Used in <![CDATA[<names:last()>]]>.
		/// This is pretty slow as it iterates until the last element.  
		/// Ultimately, I could make a special case for a List or Vector.
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

		/// <summary>
		/// Return an iterator that skips all null values.
		/// </summary>
		public object Strip(object attribute)
		{
			if (attribute == null)
			{
				return null;
			}
			attribute = ConvertAnythingIteratableToIterator(attribute);
			if (attribute is IEnumerator)
			{
				return new NullSkippingIterator((IEnumerator)attribute);
			}
			return attribute; // strip(x)==x when x single-valued attribute
		}

		/// <summary>
		/// Return all but the last element.  trunc(x)=null if x is single-valued.
		/// </summary>
		public object Trunc(object attribute)
		{
			return null; // not impl.
		}

		/// <summary>
		/// Return the length of a multiple valued attribute or 1 if it is a
		/// single attribute. If attribute is null return 0.
		/// Special case several common collections and primitive arrays for
		/// speed.  This method by Kay Roepke.
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
		public object Length(object attribute)
		{
			if (attribute == null)
			{
				return 0;
			}

			int i = 1;		// we have at least one of something. Iterator and arrays might be empty.
			if (attribute is ICollection)
			{
				i = ((ICollection)attribute).Count;
			}
			else if (attribute is IEnumerator)
			{
				IEnumerator theEnumerator = (IEnumerator)attribute;
				i = 0;
				while (theEnumerator.MoveNext())
				{
					i++;
				}
			}
			return i;
		}

		public object GetOption(string name)
		{
			object value = null;
			if (options != null)
			{
				value = options[name];
				if (value == EMPTY_OPTION)
				{
					return defaultOptionValues[name];
				}
			}
			return value;
		}

		public override string ToString()
		{
			return exprTree.ToStringList();
		}

		#region Private Helper Methods

		private bool GetMethodValueByName(PropertyLookupParams paramBag, ref object val)
		{
			MethodInfo mi = null;
			try 
			{
				mi = paramBag.prototype.GetMethod(
					paramBag.lookupName,
					BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.IgnoreCase,
					null,
					Type.EmptyTypes,
					null
					);

				if (mi != null)
				{
					// save to avoid [another expensive] lookup later
					//paramBag.self.Group.CacheClassProperty(paramBag.prototype, paramBag.propertyName, mi);
					return GetMethodValue(mi, paramBag, ref val);
				}
			}
			catch(Exception)
			{
				;
			}
			return false;
		}

		private bool GetMethodValue(MethodInfo mi, PropertyLookupParams paramBag, ref object @value)
		{
			try 
			{
				@value = mi.Invoke(paramBag.instance, (object[]) null);
				return true;
			}
			catch (Exception ex) 
			{
				paramBag.self.Error("Can't get property " + paramBag.lookupName 
					+ " using method get_/Get/Is/get/is as " + mi.Name + " from " 
					+ paramBag.prototype.FullName + " instance", ex);
			}
			return false;
		}

		private bool GetPropertyValueByName(PropertyLookupParams paramBag, ref object val)
		{
			PropertyInfo pi = null;
			try 
			{
				pi = paramBag.prototype.GetProperty(
					paramBag.lookupName,
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
						// save to avoid [another expensive] lookup later
						//paramBag.self.Group.CacheClassProperty(paramBag.prototype, paramBag.propertyName, pi);
						return GetPropertyValue(pi, paramBag, ref val);
					}
					else
					{
						paramBag.self.Error("Class " + paramBag.prototype.FullName + " property: " 
							+ paramBag.lookupName + " is write-only in template context " 
							+ paramBag.self.GetEnclosingInstanceStackString());
					}
				}
			}
			catch(Exception)
			{
				;
			}
			return false;
		}

		private bool GetPropertyValue(PropertyInfo pi, PropertyLookupParams paramBag, ref object @value)
		{
			try
			{
				@value = pi.GetValue(paramBag.instance, (object[]) null);
				return true;
			}
			catch(Exception ex)
			{
				paramBag.self.Error("Can't get property " + paramBag.propertyName 
					+ " as CLR property " + pi.Name + " from " 
					+ paramBag.prototype.FullName + " instance", ex);
			}
			return false;
		}

		private bool GetFieldValueByName(PropertyLookupParams paramBag, ref object val)
		{
			FieldInfo fi = null;
			try 
			{
				fi = paramBag.prototype.GetField(
					paramBag.lookupName,
					BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.IgnoreCase
					);

				if (fi != null)
				{
					// save to avoid [another expensive] lookup later
					//paramBag.self.Group.CacheClassProperty(paramBag.prototype, paramBag.propertyName, fi);
					return GetFieldValue(fi, paramBag, ref val);
				}
			}
			catch(Exception)
			{
				;
			}
			return false;
		}

		private bool GetFieldValue(FieldInfo fi, PropertyLookupParams paramBag, ref object @value)
		{
			try
			{
				@value = fi.GetValue(paramBag.instance);
				return true;
			}
			catch(Exception ex)
			{
				paramBag.self.Error("Can't get property " + fi.Name
					+ " using direct field access from " + paramBag.prototype.FullName 
					+ " instance", ex
					);
			}
			return false;
		}

		#endregion
	}
}