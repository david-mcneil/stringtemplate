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
using System.Collections;
using antlr.stringtemplate.language;
using AttributeReflectionController = antlr.stringtemplate.language.AttributeReflectionController;
using antlr;
using AST = antlr.collections.AST;
namespace antlr.stringtemplate
{
	
	/// <summary>A <TT>StringTemplate</TT> is a "document" with holes in it where you can stick
	/// values.  <TT>StringTemplate</TT> breaks up your template into chunks of text and
	/// attribute expressions, which are by default enclosed in angle brackets:
	/// <TT>&lt;</TT><em>attribute-expression</em><TT>&gt;</TT>.  <TT>StringTemplate</TT>
	/// ignores everything outside of attribute expressions, treating it as just text to spit
	/// out when you call <TT>StringTemplate.toString()</TT>.
	/// 
	/// <P><TT>StringTemplate</TT> is not a "system" or "engine" or "server"; it's a lib
	/// rary with two classes of interest: <TT>StringTemplate</TT> and <TT>StringTemplat
	/// eGroup</TT>.  You can directly create a <TT>StringTemplate</TT> in Java code or
	/// you can load a template from a file.
	/// <P>
	/// A StringTemplate describes an output pattern/language like an exemplar.
	/// <p>
	/// StringTemplate and associated code is released under the BSD licence.  See
	/// source.  <br><br>
	/// Copyright (c) 2003-2005 Terence Parr<br><br>
	/// A particular instance of a template may have a set of attributes that
	/// you set programmatically.  A template refers to these single or multi-
	/// valued attributes when writing itself out.  References within a
	/// template conform to a simple language with attribute references and
	/// references to other, embedded, templates.  The references are surrounded
	/// by user-defined start/stop strings (default of <...>, but $...$ works
	/// well when referencing attributes in HTML to distinguish from tags).
	/// 
	/// <p>StringTemplateGroup is a self-referential group of StringTemplate
	/// objects kind of like a grammar.  It is very useful for keeping a
	/// group of templates together.  For example, jGuru.com's premium and
	/// guest sites are completely separate sets of template files organized
	/// with a StringTemplateGroup.  Changing "skins" is a simple matter of
	/// switching groups.  Groups know where to load templates by either
	/// looking under a rootDir you can specify for the group or by simply
	/// looking for a resource file in the current class path.  If no rootDir
	/// is specified, template files are assumed to be resources.  So, if
	/// you reference template foo() and you have a rootDir, it looks for
	/// file rootDir/foo.st.  If you don't have a rootDir, it looks for
	/// file foo.st in the CLASSPATH.  note that you can use org/antlr/misc/foo()
	/// (qualified template names) as a template ref.
	/// 
	/// <p>StringTemplateErrorListener is an interface you can implement to
	/// specify where StringTemplate reports errors.  Setting the listener
	/// for a group automatically makes all associated StringTemplate
	/// objects use the same listener.  For example,
	/// 
	/// <font size=2><pre>
	/// StringTemplateGroup group = new StringTemplateGroup("loutSyndiags");
	/// group.setErrorListener(
	/// new StringTemplateErrorListener() {
	/// public void error(String msg, Exception e) {
	/// System.err.println("StringTemplate error: "+
	/// msg+((e!=null)?": "+e.getMessage():""));
	/// }
	/// }
	/// );
	/// </pre></font>
	/// 
	/// <p>IMPLEMENTATION
	/// 
	/// <p>A StringTemplate is both class and instance like in Self.  Given
	/// any StringTemplate (even one with attributes filled in), you can
	/// get a new "blank" instance of it.
	/// 
	/// <p>When you define a template, the string pattern is parsed and
	/// broken up into chunks of either String or attribute/template actions.
	/// These are typically just attribute references.  If a template is
	/// embedded within another template either via setAttribute or by
	/// implicit inclusion by referencing a template within a template, it
	/// inherits the attribute scope of the enclosing StringTemplate instance.
	/// All StringTemplate instances with the same pattern point to the same
	/// list of chunks since they are immutable there is no reason to have
	/// a copy in every instance of that pattern.  The only thing that differs
	/// is that every StringTemplate Java object can have its own set of
	/// attributes.  Each chunk points back at the original StringTemplate
	/// Java object whence they were constructed.  So, there are multiple
	/// pointers to the list of chunks (one for each instance with that
	/// pattern) and only one back ptr from a chunk to the original pattern
	/// object.  This is used primarily to get the grcoup of that original
	/// so new templates can be loaded into that group.
	/// 
	/// <p>To write out a template, the chunks are walked in order and asked to
	/// write themselves out.  String chunks are obviously just written out,
	/// but the attribute expressions/actions are evaluated in light of the
	/// attributes in that object and possibly in an enclosing instance.
	/// </summary>
	public class StringTemplate
	{
		private void InitBlock()
		{
			formalArguments = FormalArgument.UNKNOWN;
		}
		public const String VERSION = "2.2";
		
		/// <summary>An automatically created aggregate of properties.
		/// 
		/// I often have lists of things that need to be formatted, but the list
		/// items are actually pieces of data that are not already in an object.  I
		/// need ST to do something like:
		/// 
		/// Ter=3432
		/// Tom=32234
		/// ....
		/// 
		/// using template:
		/// 
		/// $items:{$attr.name$=$attr.type$}$
		/// 
		/// This example will call getName() on the objects in items attribute, but
		/// what if they aren't objects?  I have perhaps two parallel arrays
		/// instead of a single array of objects containing two fields.  One
		/// solution is allow Maps to be handled like properties so that it.name
		/// would fail getName() but then see that it's a Map and do
		/// it.get("name") instead.
		/// 
		/// This very clean approach is espoused by some, but the problem is that
		/// it's a hole in my separation rules.  People can put the logic in the
		/// view because you could say: "go get bob's data" in the view:
		/// 
		/// Bob's Phone: $db.bob.phone$
		/// 
		/// A view should not be part of the program and hence should never be able
		/// to go ask for a specific person's data.
		/// 
		/// After much thought, I finally decided on a simple solution.  I've
		/// added setAttribute variants that pass in multiple property values,
		/// with the property names specified as part of the name using a special
		/// attribute name syntax: "name.{propName1,propName2,...}".  This
		/// object is a special kind of HashMap that hopefully prevents people
		/// from passing a subclass or other variant that they have created as
		/// it would be a loophole.  Anyway, the ASTExpr.getObjectProperty()
		/// method looks for Aggregate as a special case and does a get() instead
		/// of getPropertyName.
		/// </summary>
		public sealed class Aggregate
		{
			internal Hashtable properties = new Hashtable();
			/// <summary>Allow StringTemplate to add values, but prevent the end
			/// user from doing so.
			/// </summary>
			internal void put(String propName, Object propValue)
			{
				properties[propName] = propValue;
			}
			public Object get(String propName)
			{
				return properties[propName];
			}
		}
		
		internal static string ANONYMOUS_ST_NAME = "anonymous";
		
		/// <summary>track probable issues like setting attribute that is not referenced. </summary>
		internal static bool lintMode = false;
		
		protected internal IList referencedAttributes = null;
		
		/// <summary>What's the name of this template? </summary>
		protected internal String name = ANONYMOUS_ST_NAME;
		
		private static int templateCounter = 0;

		private static int getNextTemplateCounter()
		{
			lock (typeof(antlr.stringtemplate.StringTemplate))
			{
				templateCounter++;
				return templateCounter;
			}
		}
		/// <summary>reset the template ID counter to 0; public so that testing routine
		/// can access but not really of interest to the user.
		/// </summary>
		public static void resetTemplateCounter()
		{
			templateCounter = 0;
		}
		
		protected internal int templateID = getNextTemplateCounter();
		
		/// <summary>Enclosing instance if I'm embedded within another template.
		/// IF-subtemplates are considered embedded as well.
		/// </summary>
		protected internal StringTemplate enclosingInstance = null;
		
		/// <summary>A list of embedded templates </summary>
		protected internal IList embeddedInstances = null;
		
		/// <summary>If this template is an embedded template such as when you apply
		/// a template to an attribute, then the arguments passed to this
		/// template represent the argument context--a set of values
		/// computed by walking the argument assignment list.  For example,
		/// <name:bold(item=name, foo="x")> would result in an
		/// argument context of {[item=name], [foo="x"]} for this
		/// template.  This template would be the bold() template and
		/// the enclosingInstance would point at the template that held
		/// that <name:bold(...)> template call.  When you want to get
		/// an attribute value, you first check the attributes for the
		/// 'self' template then the arg context then the enclosingInstance
		/// like resolving variables in pascal-like language with nested
		/// procedures.
		/// 
		/// With multi-valued attributes such as <faqList:briefFAQDisplay()>
		/// attribute "i" is set to 1..n.
		/// </summary>
		protected internal IDictionary argumentContext = null;
		
		/// <summary>If this template is embedded in another template, the arguments
		/// must be evaluated just before each application when applying
		/// template to a list of values.  The "it" attribute must change
		/// with each application so that $names:bold(item=it)$ works.  If
		/// you evaluate once before starting the application loop then it
		/// has a single fixed value.  Eval.g saves the AST rather than evaluating
		/// before invoking applyListOfAlternatingTemplates().  Each iteration
		/// of a template application to a multi-valued attribute, these args
		/// are re-evaluated with an initial context of {[it=...], [i=...]}.
		/// </summary>
		protected internal StringTemplateAST argumentsAST = null;
		
		/// <summary>When templates are defined in a group file format, the attribute
		/// list is provided including information about attribute cardinality
		/// such as present, optional, ...  When this information is available,
		/// rawSetAttribute should do a quick existence check as should the
		/// invocation of other templates.  So if you ref bold(item="foo") but
		/// item is not defined in bold(), then an exception should be thrown.
		/// When actually rendering the template, the cardinality is checked.
		/// This is a Map<String,FormalArgument>.
		/// </summary>
		protected internal IDictionary formalArguments;

		/** How many formal arguments to this template have default values
		*  specified?
		*/
		protected internal int numberOfDefaultArgumentValues = 0;

		/** Normally, formal parameters hide any attributes inherited from the
		 *  enclosing template with the same name.  This is normally what you
		 *  want, but makes it hard to invoke another template passing in all
		 *  the data.  Use notation now: <otherTemplate(...)> to say "pass in
		 *  all data".  Works great.  Can also say <otherTemplate(foo="xxx",...)>
		 */
		protected internal bool passThroughAttributes = false;
		
		/// <summary>What group originally defined the prototype for this template?
		/// This affects the set of templates I can refer to.
		/// </summary>
		protected internal StringTemplateGroup group;
		
		/// <summary>Where to report errors </summary>
		internal StringTemplateErrorListener listener = null;
		
		/// <summary>The original, immutable pattern/language (not really used again after
		/// initial "compilation", setup/parsing).
		/// </summary>
		protected internal String pattern;
		
		/// <summary>Map an attribute name to its value(s).  These values are set by outside
		/// code via st.setAttribute(name, value).  StringTemplate is like self in
		/// that a template is both the "class def" and "instance".  When you
		/// create a StringTemplate or setTemplate, the text is broken up into chunks
		/// (i.e., compiled down into a series of chunks that can be evaluated later).
		/// You can have multiple
		/// </summary>
		protected internal IDictionary attributes;
		
		/// <summary>
		/// A Map that allows people to register a renderer for
		///	a particular kind of object to be displayed in this template.  This
		/// overrides any renderer set for this template's group.
		///
		/// Most of the time this map is not used because the StringTemplateGroup
		/// has the general renderer map for all templates in that group.
		/// Sometimes though you want to override the group's renderers.
		/// </summary>
		protected internal IDictionary attributeRenderers;

		/// <summary>A list of alternating string and ASTExpr references.
		/// This is compiled to when the template is loaded/defined and walked to
		/// write out a template instance.
		/// </summary>
		protected internal IList chunks;
		
		protected internal static StringTemplateGroup defaultGroup = new StringTemplateGroup("defaultGroup", ".");
		
		/// <summary>Create a blank template with no pattern and no attributes </summary>
		public StringTemplate()
		{
			InitBlock();
			group = defaultGroup; // make sure has a group even if default
		}
		
		/// <summary>Create an anonymous template.  It has no name just
		/// chunks (which point to this anonymous template) and attributes.
		/// </summary>
		public StringTemplate(String template):this(null, template)
		{
		}
		
		public StringTemplate(String template, System.Type lexer):this()
		{
			setGroup(new StringTemplateGroup("angleBracketsGroup", lexer));
			setTemplate(template);
		}
		
		/// <summary>Create an anonymous template with no name, but with a group </summary>
		public StringTemplate(StringTemplateGroup group, String template):this()
		{
			if (group != null)
			{
				setGroup(group);
			}
			setTemplate(template);
		}
		
		/// <summary>Make the 'to' template look exactly like the 'from' template
		/// except for the attributes.  This is like creating an instance
		/// of a class in that the executable code is the same (the template
		/// chunks), but the instance data is blank (the attributes).  Do
		/// not copy the enclosingInstance pointer since you will want this
		/// template to eval in a context different from the examplar.
		/// </summary>
		protected internal virtual void dup(StringTemplate from, StringTemplate to)
		{
			to.pattern = from.pattern;
			to.chunks = from.chunks;
			to.formalArguments = from.formalArguments;
			to.numberOfDefaultArgumentValues = from.numberOfDefaultArgumentValues;
			to.name = from.name;
			to.group = from.group;
			to.listener = from.listener;
		}
		
		/// <summary>Make an instance of this template; it contains an exact copy of
		/// everything (except the attributes and enclosing instance pointer).
		/// So the new template refers to the previously compiled chunks of this
		/// template but does not have any attribute values.
		/// </summary>
		public virtual StringTemplate getInstanceOf()
		{
			StringTemplate t = group.createStringTemplate();
			dup(this, t);
			return t;
		}
		
		public virtual StringTemplate getEnclosingInstance()
		{
			return enclosingInstance;
		}
		
		public virtual void setEnclosingInstance(StringTemplate enclosingInstance)
		{
			if (this == enclosingInstance)
			{
				throw new System.ArgumentException("cannot embed template " + getName() + " in itself");
			}
			// set the parent for this template
			this.enclosingInstance = enclosingInstance;
			// make the parent track this template as an embedded template
			if (enclosingInstance!=null)
			{
				this.enclosingInstance.addEmbeddedInstance(this);
			}
		}
		
		public virtual void addEmbeddedInstance(StringTemplate embeddedInstance)
		{
			if (this.embeddedInstances == null)
			{
				this.embeddedInstances = new ArrayList();
			}
			this.embeddedInstances.Add(embeddedInstance);
		}
		
		public virtual IDictionary getArgumentContext()
		{
			return argumentContext;
		}
		
		public virtual void setArgumentContext(IDictionary ac)
		{
			argumentContext = ac;
		}
		
		public virtual StringTemplateAST getArgumentsAST()
		{
			return argumentsAST;
		}
		
		public virtual void setArgumentsAST(StringTemplateAST argumentsAST)
		{
			this.argumentsAST = argumentsAST;
		}
		
		public virtual String getName()
		{
			return name;
		}
		
		public virtual String getOutermostName()
		{
			if (enclosingInstance != null)
			{
				return enclosingInstance.getOutermostName();
			}
			return getName();
		}
		
		public virtual void setName(String name)
		{
			this.name = name;
		}
		
		public virtual StringTemplateGroup getGroup()
		{
			return group;
		}
		
		public virtual void setGroup(StringTemplateGroup group)
		{
			this.group = group;
		}
		
		public virtual void setTemplate(String template)
		{
			this.pattern = template;
			breakTemplateIntoChunks();
		}
		
		public virtual String getTemplate()
		{
			return pattern;
		}
		
		public virtual void setErrorListener(StringTemplateErrorListener listener)
		{
			this.listener = listener;
		}
		
		public virtual StringTemplateErrorListener getErrorListener()
		{
			if (listener == null)
			{
				return group.getErrorListener();
			}
			return listener;
		}
		
		public virtual void reset()
		{
			attributes = new Hashtable(); // just throw out table and make new one
		}
		
		public virtual void setPredefinedAttributes()
		{
			if (!inLintMode())
			{
				return ; // only do this method so far in lint mode
			}
			if (formalArguments != FormalArgument.UNKNOWN)
			{
				// must define self before setting if the user has defined
				// formal arguments to the template
				defineFormalArgument(ASTExpr.REFLECTION_ATTRIBUTES);
			}
			if (attributes == null)
			{
				attributes = new Hashtable();
			}
			rawSetAttribute(attributes, ASTExpr.REFLECTION_ATTRIBUTES, new AttributeReflectionController(this).ToString());
		}
		
		public virtual void removeAttribute(String name)
		{
			attributes.Remove(name);
		}

		/// <summary>
		/// Set an attribute for this template.  If you set the same
		/// attribute more than once, you get a multi-valued attribute.
		/// If you send in a StringTemplate object as a value, it's
		/// enclosing instance (where it will inherit values from) is
		/// set to 'this'.  This would be the normal case, though you
		/// can set it back to null after this call if you want.
		/// If you send in a List plus other values to the same
		/// attribute, they all get flattened into one List of values.
		/// If you send in an array, it is converted to a List.  Works
		/// with arrays of objects and arrays of {int,float,double}.
		/// </summary>
		public virtual void setAttribute(String name, Object value) {
			if (value == null) {
				return;
			}

			if (attributes == null) {
				attributes = new Hashtable();
			}

			if (value is StringTemplate) {
				((StringTemplate) value).setEnclosingInstance(this);
			}

			// convert plain collections
			// get exactly in this scope (no enclosing)
			Object o = attributes[name];

			if (o != null) {
				// it's a multi-value attribute
				//Console.WriteLine("exists: "+name+"="+o);
				IList v = null;

				if (o is IList) {
					// already a List
					v = (IList) o;

					if (value is IList) {
						// flatten incoming list into existing
						// (do nothing if same List to avoid trouble)
						IList v2 = (IList) value;

						for (int i = 0; v != v2 && i < v2.Count; i++) {
							// Console.WriteLine("flattening "+name+"["+i+"]="+v2.elementAt(i)+" into existing value");
							v.Add(v2[i]);
						}
					}
					if (value is ICollection) {
						// flatten incoming list into existing
						// (do nothing if same List to avoid trouble)
						IList v2 = (IList) value;

						foreach (Object o1 in v2) {
							// Console.WriteLine("flattening "+name+"["+i+"]="+v2.elementAt(i)+" into existing value");
							v.Add(o1);
						}
					}
					else {
						v.Add(value);
					}
				}
				else {
					// second attribute, must convert existing to ArrayList
					v = new ArrayList(); // make list to hold multiple values
					// make it point to list now
					rawSetAttribute(this.attributes, name, v);
					v.Add(o); // add previous single-valued attribute

					if (value is IList) {
						// flatten incoming list into existing
						IList v2 = (IList) value;

						for (int i = 0; i < v2.Count; i++) {
							v.Add(v2[i]);
						}
					}
					else {
						v.Add(value);
					}
				}
			}
			else {
				rawSetAttribute(attributes, name, value);
			}
		}
		

		/// <summary>
		/// Create an aggregate from the list of properties in aggrSpec and fill
		/// with values from values array.  This is not publically visible because
		/// it conflicts semantically with setAttribute("foo",new Object[] {...});
		/// </summary>
		public virtual void setAttribute(String aggrSpec, params Object[] values) {
			IList properties = new ArrayList();
			String aggrName = null;
			
			try {
				aggrName = parseAggregateAttributeSpec(aggrSpec, properties);
			}
			catch (ArgumentException) {
				//hmm, maybe they just want to set an attribute to an array value?
				setAttribute(aggrSpec, (Object) values);
				return;
			}

			if (values == null || properties.Count == 0) {
				throw new ArgumentException("missing properties or values for '" + aggrSpec + "'");
			}

			if (values.Length != properties.Count) {
				throw new ArgumentException("number of properties in '" + aggrSpec + "' != number of values");
			}

			Aggregate aggr = new Aggregate();

			for (int i = 0; i < values.Length; i++) {
				Object value = values[i];
				aggr.put((String) properties[i], value);
			}

			setAttribute(aggrName, aggr);
		}
		

		/// <summary>
		/// Split "aggrName.{propName1,propName2}" into list [propName1,propName2]
		/// and the aggrName.
		/// </summary>
		protected virtual String parseAggregateAttributeSpec(String aggrSpec, IList properties) {
			int dot = aggrSpec.IndexOf('.');

			if (dot <= 0) {
				throw new ArgumentException("invalid aggregate attribute format: " + aggrSpec);
			}

			String aggrName = aggrSpec.Substring(0, dot);
			String propString = aggrSpec.Substring(dot + 1, (aggrSpec.Length) - (dot + 1));
			bool error = true;
			SupportClass.Tokenizer tokenizer = new SupportClass.Tokenizer(propString, "{,}", true);

			if (tokenizer.HasMoreTokens()) {
				String token = tokenizer.NextToken(); // advance to {

				if (token.Equals("{")) {
					token = tokenizer.NextToken(); // advance to first prop name
					properties.Add(token);
					token = tokenizer.NextToken(); // advance to a comma

					while (token.Equals(",")) {
						token = tokenizer.NextToken(); // advance to a prop name
						properties.Add(token);
						token = tokenizer.NextToken(); // advance to a "," or "}"
					}

					if (token.Equals("}")) {
						error = false;
					}
				}
			}

			if (error) {
				throw new ArgumentException("invalid aggregate attribute format: " + aggrSpec);
			}

			return aggrName;
		}
		

		/// <summary>
		/// Map a value to a named attribute.  Throw ArgumentOutOfRangeException if
		/// the named attribute is not formally defined in this specific template
		/// and a formal argument list exists.
		/// </summary>
		public virtual void rawSetAttribute(IDictionary attributes, String name, Object value) 
		{
			if (formalArguments != FormalArgument.UNKNOWN && formalArguments[name] == null) {
				throw new ArgumentOutOfRangeException("name", String.Format("no such attribute: " + 
					"{0} in template {1} or in enclosing template", name, this.getName()));
			}

			if (value == null) {
				return;
			}

			attributes[name] = value;
		}

		/// <summary>
		/// Argument evaluation such as foo(x=y), x must
		/// be checked against foo's argument list not this's (which is
		/// the enclosing context).  So far, only eval.g uses arg self as
		/// something other than "this".
		///	</summary>
		public virtual void rawSetArgumentAttribute(StringTemplate embedded, IDictionary attributes, String name, Object value)
		{
			if ( embedded.formalArguments!=FormalArgument.UNKNOWN &&
				embedded.getFormalArgument(name)==null )
			{
				throw new ArgumentOutOfRangeException("template "+embedded.getName()+
					" has no such attribute: "+name+
					" in template context "+
					getEnclosingInstanceStackString());
			}
			if ( value == null ) 
			{
				return;
			}
			attributes[name] = value;
		}
		
		public virtual Object getAttribute(String name)
		{
			return get(this, name);
		}
		
		/// <summary>Walk the chunks, asking them to write themselves out according
		/// to attribute values of 'this.attributes'.  This is like evaluating or
		/// interpreting the StringTemplate as a program using the
		/// attributes.  The chunks will be identical (point at same list)
		/// for all instances of this template.
		/// </summary>
		public virtual int write(StringTemplateWriter outWriter)
		{
			int n = 0;
			setPredefinedAttributes();
			setDefaultArgumentValues();
			for (int i = 0; chunks != null && i < chunks.Count; i++)
			{
				Expr a = (Expr) chunks[i];
				int chunkN = a.write(this, outWriter);
				// NEWLINE expr-with-no-output NEWLINE => NEWLINE
				// Indented $...$ have the indent stored with the ASTExpr
				// so the indent does not come out as a StringRef
				if (chunkN == 0 && (i - 1) >= 0 && chunks[i - 1] is NewlineRef && (i + 1) < chunks.Count && chunks[i + 1] is NewlineRef)
				{
					//System.out.println("found pure \\n blank \\n pattern");
					i++; // make it skip over the next chunk, the NEWLINE
				}
				n += chunkN;
			}
			if (lintMode)
			{
				checkForTrouble();
			}
			return n;
		}
		
		/// <summary>Resolve an attribute reference.  It can be in four possible places:
		/// 
		/// 1. the attribute list for the current template
		/// 2. if self is an embedded template, somebody invoked us possibly
		/// with arguments--check the argument context
		/// 3. if self is an embedded template, the attribute list for the enclosing
		/// instance (recursively up the enclosing instance chain)
		/// 4. if nothing is found in the enclosing instance chain, then it might
		///	be a map defined in the group or the its supergroup etc...
		/// 
		/// Attribute references are checked for validity.  If an attribute has
		/// a value, its validity was checked before template rendering.
		/// If the attribute has no value, then we must check to ensure it is a
		/// valid reference.  Somebody could reference any random value like $xyz$;
		/// formal arg checks before rendering cannot detect this--only the ref
		/// can initiate a validity check.  So, if no value, walk up the enclosed
		/// template tree again, this time checking formal parameters not
		/// attributes Map.  The formal definition must exist even if no value.
		/// 
		/// To avoid infinite recursion in toString(), we have another condition
		/// to check regarding attribute values.  If your template has a formal
		/// argument, foo, then foo will hide any value available from "above"
		/// in order to prevent infinite recursion.
		/// 
		/// This method is not static so people can overrided functionality.
		/// </summary>
		public virtual Object get(StringTemplate self, String attribute)
		{
			if (self == null)
			{
				return null;
			}
			
			if (lintMode)
			{
				self.trackAttributeReference(attribute);
			}
			
			// is it here?
			Object o = null;
			if (self.attributes != null)
			{
				o = self.attributes[attribute];
			}
			
			// nope, check argument context in case embedded
			if (o == null)
			{
				IDictionary argContext = self.getArgumentContext();
				if (argContext != null)
				{
					o = argContext[attribute];
				}
			}
			
			if (o == null && !self.passThroughAttributes && self.getFormalArgument(attribute) != null)
			{
				// if you've defined attribute as formal arg for this
				// template and it has no value, do not look up the
				// enclosing dynamic scopes.  This avoids potential infinite
				// recursion.
				return null;
			}
			
			// not locally defined, check enclosingInstance if embedded
			if (o == null && self.enclosingInstance != null)
			{
				/*
				System.out.println("looking for "+getName()+"."+attribute+" in super="+
				enclosingInstance.getName());
				*/
				Object valueFromEnclosing = get(self.enclosingInstance, attribute);
				if (valueFromEnclosing == null)
				{
					checkNullAttributeAgainstFormalArguments(self, attribute);
				}
				o = valueFromEnclosing;
			}
				// not found and no enclosing instance to look at
			else if ( o==null && self.enclosingInstance==null ) 
			{
				// It might be a map in the group or supergroup...
				o = self.group.getMap(attribute);
			}
			
			return o;
		}
		
		/// <summary>Walk a template, breaking it into a list of
		/// chunks: Strings and actions/expressions.
		/// </summary>
		protected internal virtual void breakTemplateIntoChunks()
		{
			//System.out.println("parsing template: "+pattern);
			if (pattern == null || pattern == "")
			{
				return ;
			}
			try
			{
				// instead of creating a specific template lexer, use
				// an instance of the class specified by the user.
				// The default is DefaultTemplateLexer.
				// The only constraint is that you use an ANTLR lexer
				// so I can use the special ChunkToken.
				System.Type lexerClass = group.getTemplateLexerClass();
				System.Reflection.ConstructorInfo ctor = lexerClass.GetConstructor(new System.Type[]{typeof(StringTemplate), typeof(System.IO.StreamReader)});
				CharScanner chunkStream = (CharScanner) ctor.Invoke(new Object[]{this, new System.IO.StringReader(pattern)});
				chunkStream.setTokenObjectClass("antlr.stringtemplate.language.ChunkToken");
				TemplateParser chunkifier = new TemplateParser(chunkStream);
				chunkifier.template(this);
			}
			catch (System.Exception e)
			{
				String name = "<unknown>";
				String outerName = getOutermostName();
				if (getName() != null)
				{
					name = getName();
				}
				if (outerName != null && !name.Equals(outerName))
				{
					name = name + " nested in " + outerName;
				}
				error("problem parsing template '" + name + "'", e);
			}
		}
		
		public virtual ASTExpr parseAction(String action)
		{
			ActionLexer lexer = new ActionLexer(new System.IO.StringReader(action.ToString()));
			ActionParser parser = new ActionParser(lexer, this);
			parser.setASTNodeClass("antlr.stringtemplate.language.StringTemplateAST");
			lexer.setTokenObjectClass("antlr.stringtemplate.language.StringTemplateToken");
			ASTExpr a = null;
			try
			{
				IDictionary options = parser.action();
				AST tree = parser.getAST();
				if (tree != null)
				{
					if (tree.Type == ActionParser.CONDITIONAL)
					{
						a = new ConditionalExpr(this, tree);
					}
					else
					{
						a = new ASTExpr(this, tree, options);
					}
				}
			}
			catch (RecognitionException re)
			{
				error("Can't parse chunk: " + action.ToString(), re);
			}
			catch (TokenStreamException tse)
			{
				error("Can't parse chunk: " + action.ToString(), tse);
			}
			return a;
		}
		
		public virtual int getTemplateID()
		{
			return templateID;
		}
		
		public virtual IDictionary getAttributes()
		{
			return attributes;
		}
		
		/// <summary>Get a list of the strings and subtemplates and attribute
		/// refs in a template.
		/// </summary>
		public virtual IList getChunks()
		{
			return chunks;
		}
		
		public virtual void addChunk(Expr e)
		{
			if (chunks == null)
			{
				chunks = new ArrayList();
			}
			chunks.Add(e);
		}
		
		public virtual void setAttributes(IDictionary attributes)
		{
			this.attributes = attributes;
		}
		
		// F o r m a l  A r g  S t u f f
		
		public virtual IDictionary getFormalArguments()
		{
			return formalArguments;
		}
		
		public virtual void setFormalArguments(IDictionary args)
		{
			formalArguments = args;
		}

		/// <summary>
		/// Set any default argument values that were not set by the
		/// invoking template or by setAttribute directly.  Note
		/// that the default values may be templates.  Their evaluation
		/// context is the template itself and, hence, can see attributes
		/// within the template, any arguments, and any values inherited
		/// by the template.
		/// 
		/// Default values are stored in the argument context rather than
		/// the template attributes table just for consistency's sake.
		/// </summary>
		public virtual void setDefaultArgumentValues() 
		{
			if ( numberOfDefaultArgumentValues==0 ) 
			{
				return;
			}
			if ( argumentContext==null ) 
			{
				argumentContext = new Hashtable();
			}
			if ( formalArguments!=FormalArgument.UNKNOWN ) 
			{
				ICollection argNames = formalArguments.Keys;
				foreach (string argName in argNames)
				{
					// use the default value then
					FormalArgument arg =
						(FormalArgument)formalArguments[argName];
					if ( arg.defaultValueST!=null ) 
					{
						Object existingValue = getAttribute(argName);
						if ( existingValue==null ) 
						{ // value unset?
							// if no value for attribute, set arg context
							// to the default value.  We don't need an instance
							// here because no attributes can be set in
							// the arg templates by the user.
							argumentContext[argName] = arg.defaultValueST;
						}
					}
				}
			}
		}
		
		/// <summary>From this template upward in the enclosing template tree,
		/// recursively look for the formal parameter.
		/// </summary>
		public virtual FormalArgument lookupFormalArgument(String name)
		{
			FormalArgument arg = getFormalArgument(name);
			if (arg == null && enclosingInstance != null)
			{
				arg = enclosingInstance.lookupFormalArgument(name);
			}
			return arg;
		}
		
		public virtual FormalArgument getFormalArgument(String name)
		{
			return (FormalArgument) formalArguments[name];
		}
		
		public virtual void defineEmptyFormalArgumentList()
		{
			setFormalArguments(new Hashtable());
		}
		
		public virtual void defineFormalArgument(string name) 
		{
			defineFormalArgument(name,null);
		}

		public virtual void defineFormalArguments(IList names) 
		{
			if ( names==null ) 
			{
				return;
			}
			foreach (string name in names)
			{
				defineFormalArgument(name);
			}
		}

		public virtual void defineFormalArgument(string name, StringTemplate defaultValue) 
		{
			if ( defaultValue!=null ) 
			{
				numberOfDefaultArgumentValues++;
			}
			FormalArgument a = new FormalArgument(name,defaultValue);
			if ( formalArguments==FormalArgument.UNKNOWN ) 
			{
				formalArguments = new Hashtable();
			}
			formalArguments[name] = a;
		}

		/// <summary>
		/// Normally if you call template y from x, y cannot see any attributes
		/// of x that are defined as formal parameters of y.  Setting this
		/// passThroughAttributes to true, will override that and allow a
		/// template to see through the formal arg list to inherited values.
		/// </summary>
		public virtual void setPassThroughAttributes(bool passThroughAttributes) 
		{
			this.passThroughAttributes = passThroughAttributes;
		}

		///	<summary>
		/// Specify a complete map of what object classes should map to which
		/// renderer objects.
		///	</summary>
		public virtual void setAttributeRenderers(IDictionary renderers) 
		{
			this.attributeRenderers = renderers;
		}

		/// <summary>
		/// Register a renderer for all objects of a particular type.  This
		/// overrides any renderer set in the group for this class type.
		/// </summary>
		public virtual void registerRenderer(Type attributeClassType, Object renderer) 
		{
			if ( attributeRenderers==null ) 
			{
				attributeRenderers = new Hashtable();
			}
			attributeRenderers[attributeClassType] = renderer;
		}
		/// <summary>
		/// What renderer is registered for this attributeClassType for
		/// this template.  If not found, the template's group is queried.
		/// </summary>
		public virtual AttributeRenderer getAttributeRenderer(Type attributeClassType) 
		{
			if ( attributeRenderers==null ) 
			{
				// we have no renderer overrides for the template, check group
				return group.getAttributeRenderer(attributeClassType);
			}
			AttributeRenderer renderer = (AttributeRenderer)attributeRenderers[attributeClassType];
			if ( renderer==null ) 
			{
				// no renderer override registered for this class, check group
				renderer = group.getAttributeRenderer(attributeClassType);
			}
			return renderer;
		}
		
		// U T I L I T Y  R O U T I N E S
		
		public virtual void error(String msg)
		{
			error(msg, null);
		}
		
		public virtual void warning(String msg)
		{
			if (getErrorListener() != null)
			{
				getErrorListener().warning(msg);
			}
			else
			{
				System.Console.Error.WriteLine("StringTemplate: warning: " + msg);
			}
		}
		
		public virtual void debug(String msg)
		{
			if (getErrorListener() != null)
			{
				getErrorListener().debug(msg);
			}
			else
			{
				System.Console.Error.WriteLine("StringTemplate: debug: " + msg);
			}
		}
		
		public virtual void error(String msg, System.Exception e)
		{
			if (getErrorListener() != null)
			{
				getErrorListener().error(msg, e);
			}
			else
			{
				if (e != null)
				{
					System.Console.Error.WriteLine("StringTemplate: error: " + msg + ": " + e.ToString());
				}
				else
				{
					System.Console.Error.WriteLine("StringTemplate: error: " + msg);
				}
			}
		}
		
		/// <summary>Make StringTemplate check your work as it evaluates templates.
		/// Problems are sent to error listener.   Currently warns when
		/// you set attributes that are not used.
		/// </summary>
		public static void setLintMode(bool lint)
		{
			StringTemplate.lintMode = lint;
		}
		
		public static bool inLintMode()
		{
			return lintMode;
		}
				
		/// <summary>Indicates that 'name' has been referenced in this template. </summary>
		protected internal virtual void trackAttributeReference(String name)
		{
			if (referencedAttributes == null)
			{
				referencedAttributes = new ArrayList();
			}
			referencedAttributes.Add(name);
		}
		
		/// <summary>Look up the enclosing instance chain (and include this) to see
		/// if st is a template already in the enclosing instance chain.
		/// </summary>
		public static bool isRecursiveEnclosingInstance(StringTemplate st)
		{
			if (st == null)
			{
				return false;
			}
			StringTemplate p = st.enclosingInstance;
			if (p == st)
			{
				return true; // self-recursive
			}
			// now look for indirect recursion
			while (p != null)
			{
				if (p == st)
				{
					return true;
				}
				p = p.enclosingInstance;
			}
			return false;
		}
		
		public virtual String getEnclosingInstanceStackTrace()
		{
			System.Text.StringBuilder buf = new System.Text.StringBuilder();
			IDictionary seen = new Hashtable();
			StringTemplate p = this;
			while (p != null)
			{
				if (seen.Contains(p))
				{
					buf.Append(p.getTemplateDeclaratorString());
					buf.Append(" (start of recursive cycle)");
					buf.Append("\n");
					buf.Append("...");
					break;
				}
				seen.Add(p, null);
				buf.Append(p.getTemplateDeclaratorString());
				if (p.attributes != null)
				{
					buf.Append(", attributes=[");
					int i = 0;
					for (IEnumerator iter = p.attributes.Keys.GetEnumerator(); iter.MoveNext(); )
					{
						String attrName = (String) iter.Current;
						if (i > 0)
						{
							buf.Append(", ");
						}
						i++;
						buf.Append(attrName);
						Object o = p.attributes[attrName];
						if (o is StringTemplate)
						{
							StringTemplate st = (StringTemplate) o;
							buf.Append("=");
							buf.Append("<");
							buf.Append(st.getName());
							buf.Append("()@");
							buf.Append(System.Convert.ToString(st.getTemplateID()));
							buf.Append(">");
						}
						else if (o is IList)
						{
							buf.Append("=List[..");
							IList list = (IList) o;
							int n = 0;
							for (int j = 0; j < list.Count; j++)
							{
								Object listValue = list[j];
								if (listValue is StringTemplate)
								{
									if (n > 0)
									{
										buf.Append(", ");
									}
									n++;
									StringTemplate st = (StringTemplate) listValue;
									buf.Append("<");
									buf.Append(st.getName());
									buf.Append("()@");
									buf.Append(System.Convert.ToString(st.getTemplateID()));
									buf.Append(">");
								}
							}
							buf.Append("..]");
						}
					}
					buf.Append("]");
				}
				if (p.referencedAttributes != null)
				{
					buf.Append(", references=");
					buf.Append(SupportClass.CollectionToString(p.referencedAttributes));
				}
				buf.Append(">\n");
				p = p.enclosingInstance;
			}
			/*
			if ( enclosingInstance!=null ) {
			buf.append(enclosingInstance.getEnclosingInstanceStackTrace());
			}
			*/
			return buf.ToString();
		}
		
		public virtual String getTemplateDeclaratorString()
		{
			System.Text.StringBuilder buf = new System.Text.StringBuilder();
			buf.Append("<");
			buf.Append(getName());
			buf.Append("(");
			buf.Append(SupportClass.CollectionToString(formalArguments.Keys));
			buf.Append(")@");
			buf.Append(System.Convert.ToString(getTemplateID()));
			buf.Append(">");
			return buf.ToString();
		}
		
		/// <summary>Find "missing attribute" and "cardinality mismatch" errors.
		/// Excecuted before a template writes its chunks out.
		/// When you find a problem, throw an IllegalArgumentException.
		/// We must check the attributes as well as the incoming arguments
		/// in argumentContext.
		/// protected void checkAttributesAgainstFormalArguments() {
		/// Set args = formalArguments.keySet();
		/// /*
		/// if ( (attributes==null||attributes.size()==0) &&
		/// (argumentContext==null||argumentContext.size()==0) &&
		/// formalArguments.size()!=0 )
		/// {
		/// throw new IllegalArgumentException("missing argument(s): "+args+" in template "+getName());
		/// }
		/// Iterator iter = args.iterator();
		/// while ( iter.hasNext() ) {
		/// String argName = (String)iter.next();
		/// FormalArgument arg = getFormalArgument(argName);
		/// int expectedCardinality = arg.getCardinality();
		/// Object value = getAttribute(argName);
		/// int actualCardinality = getActualArgumentCardinality(value);
		/// // if intersection of expected and actual is empty, mismatch
		/// if ( (expectedCardinality&actualCardinality)==0 ) {
		/// throw new IllegalArgumentException("cardinality mismatch: "+
		/// argName+"; expected "+
		/// FormalArgument.getCardinalityName(expectedCardinality)+
		/// " found cardinality="+getObjectLength(value));
		/// }
		/// }
		/// }
		/// </summary>
		
		/// <summary>A reference to an attribute with no value, must be compared against
		/// the formal parameter to see if it exists; if it exists all is well,
		/// but if not, throw an exception.
		/// 
		/// Don't do the check if no formal parameters exist for this template;
		/// ask enclosing.
		/// </summary>
		protected internal virtual void checkNullAttributeAgainstFormalArguments(StringTemplate self, String attribute)
		{
			if (self.getFormalArguments() == FormalArgument.UNKNOWN)
			{
				// bypass unknown arg lists
				if (self.enclosingInstance != null)
				{
					checkNullAttributeAgainstFormalArguments(self.enclosingInstance, attribute);
				}
				return ;
			}
			FormalArgument formalArg = self.lookupFormalArgument(attribute);
			if (formalArg == null)
			{
				throw new ArgumentOutOfRangeException("attribute", "no such attribute: " + attribute +
					" in template context " + getEnclosingInstanceStackString());
			}
		}
		
		/// <summary>Executed after evaluating a template.  For now, checks for setting
		/// of attributes not reference.
		/// </summary>
		protected internal virtual void checkForTrouble()
		{
			// we have table of set values and list of values referenced
			// compare, looking for SET BUT NOT REFERENCED ATTRIBUTES
			if (attributes == null)
			{
				return ;
			}
			ICollection names = attributes.Keys;
			IEnumerator iter = names.GetEnumerator();
			// if in names and not in referenced attributes, trouble
			while (iter.MoveNext())
			{
				String name = (String) iter.Current;
				if (referencedAttributes != null && !referencedAttributes.Contains(name) && !name.Equals(ASTExpr.REFLECTION_ATTRIBUTES))
				{
					warning(getName() + ": set but not used: " + name);
				}
			}
			// can do the reverse, but will have lots of false warnings :(
		}

		///	<summary>
		/// If an instance of x is enclosed in a y which is in a z, return
		/// a String of these instance names in order from topmost to lowest;
		/// here that would be "[z y x]".
		/// </summary>
		public String getEnclosingInstanceStackString() 
		{
			IList names = new ArrayList();
			StringTemplate p = this;
			while ( p!=null ) 
			{
				String name = p.getName();
				names.Insert(0,name+(p.passThroughAttributes?"(...)":""));
				p = p.enclosingInstance;
			}
			return names.ToString().Replace(",","");
		}
	
		/// <summary>Compute a bitset of valid cardinality for an actual attribute value.
		/// A length of 0, satisfies OPTIONAL|ZERO_OR_MORE
		/// whereas a length of 1 satisfies all. A length>1
		/// satisfies ZERO_OR_MORE|ONE_OR_MORE
		/// 
		/// UNUSED
		/// public static int getActualArgumentCardinality(Object value) {
		/// int actualLength = getObjectLength(value);
		/// if ( actualLength==0 ) {
		/// return FormalArgument.OPTIONAL|FormalArgument.ZERO_OR_MORE;
		/// }
		/// if ( actualLength==1 ) {
		/// return FormalArgument.REQUIRED|
		/// FormalArgument.ONE_OR_MORE|
		/// FormalArgument.ZERO_OR_MORE|
		/// FormalArgument.OPTIONAL;
		/// }
		/// return FormalArgument.ZERO_OR_MORE|FormalArgument.ONE_OR_MORE;
		/// }
		/// </summary>
		
		/// <summary>UNUSED </summary>
		/*
		protected static int getObjectLength(Object value) {
		int actualLength = 0;
		if ( value!=null ) {
		if ( value instanceof Collection ) {
		actualLength = ((Collection)value).size();
		}
		if ( value instanceof Map ) {
		actualLength = ((Map)value).size();
		}
		else {
		actualLength = 1;
		}
		}
		
		return actualLength;
		}
		*/
		
		public virtual String toDebugString()
		{
			System.Text.StringBuilder buf = new System.Text.StringBuilder();
			buf.Append("template-"+getTemplateDeclaratorString()+":");
			buf.Append("chunks=");
			if ( chunks!=null ) 
			{
				buf.Append(SupportClass.CollectionToString(chunks));
			}
			buf.Append("attributes=[");
			if ( attributes!=null ) 
			{
				int n=0;
				for (IEnumerator iter = attributes.Keys.GetEnumerator(); iter.MoveNext(); )
				{
					if ( n>0 ) 
					{
						buf.Append(',');
					}
					String name = (String) iter.Current;
					buf.Append(name+"=");
					Object value = attributes[name];
					if ( value is StringTemplate ) 
					{
						buf.Append(((StringTemplate)value).toDebugString());
					}
					else 
					{
						buf.Append(value);
					}
					n++;
				}
				buf.Append("]");
			}
			return buf.ToString();
		}
		
		public virtual void printDebugString()
		{
			System.Console.Out.WriteLine("template-" + getName() + ":");
			System.Console.Out.Write("chunks=");
			System.Console.Out.WriteLine(SupportClass.CollectionToString(chunks));
			if (attributes == null)
			{
				return ;
			}
			System.Console.Out.Write("attributes=[");
			int n = 0;

			for (IEnumerator iter = attributes.Keys.GetEnumerator(); iter.MoveNext(); )
			{
				if (n > 0)
				{
					System.Console.Out.Write(',');
				}
				String name = (String) iter.Current;
				Object value = attributes[name];
				if (value is StringTemplate)
				{
					System.Console.Out.Write(name + "=");
					((StringTemplate) value).printDebugString();
				}
				else
				{
					if (value is IList)
					{
						ArrayList alist = (ArrayList) value;
						for (int i = 0; i < alist.Count; i++)
						{
							Object o = alist[i];
							System.Console.Out.Write(name + "[" + i + "] is " + o.GetType().FullName + "=");
							if (o is StringTemplate)
							{
								((StringTemplate) o).printDebugString();
							}
							else
							{
								System.Console.Out.WriteLine(o);
							}
						}
					}
					else
					{
						System.Console.Out.Write(name + "=");
						System.Console.Out.WriteLine(value);
					}
				}
				n++;
			}
			System.Console.Out.Write("]\n");
		}
		
		public override String ToString()
		{
			System.IO.StringWriter outWriter = new System.IO.StringWriter();
			// Write the output to a StringWriter
			// TODO seems slow to create all these objects, can I use a singleton?
			StringTemplateWriter wr = group.getStringTemplateWriter(outWriter);
			try
			{
				write(wr);
			}
			catch (System.IO.IOException)
			{
				error("Got IOException writing to StringWriter"+wr.GetType().Name);
			}
			return outWriter.ToString();
		}
	}
}