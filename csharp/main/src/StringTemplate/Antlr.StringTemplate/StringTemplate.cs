/*
[The "BSD licence"]
Copyright (c) 2005-2006 Kunle Odutola
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


namespace Antlr.StringTemplate
{
	using System;
	using StringWriter						= System.IO.StringWriter;
	using StringReader						= System.IO.StringReader;
	using TextReader						= System.IO.TextReader;
	using IOException						= System.IO.IOException;
	using StringBuilder						= System.Text.StringBuilder;
	using Hashtable							= System.Collections.Hashtable;
	using ArrayList							= System.Collections.ArrayList;
	using ICollection						= System.Collections.ICollection;
	using IList								= System.Collections.IList;
	using IDictionary						= System.Collections.IDictionary;
	using IEnumerator						= System.Collections.IEnumerator;
	using ConstructorInfo					= System.Reflection.ConstructorInfo;
	using AST								= antlr.collections.AST;
	using CharScanner						= antlr.CharScanner;
	using CommonAST							= antlr.CommonAST;
	using CommonToken						= antlr.CommonToken;
	using RecognitionException				= antlr.RecognitionException;
	using TokenStreamException				= antlr.TokenStreamException;
	using CollectionUtils					= Antlr.StringTemplate.Collections.CollectionUtils;
	using HashList							= Antlr.StringTemplate.Collections.HashList;
	using ASTExpr							= Antlr.StringTemplate.Language.ASTExpr;
	using Expr								= Antlr.StringTemplate.Language.Expr;
	using ConditionalExpr					= Antlr.StringTemplate.Language.ConditionalExpr;
	using ChunkToken						= Antlr.StringTemplate.Language.ChunkToken;
	using StringTemplateToken				= Antlr.StringTemplate.Language.StringTemplateToken;
	using StringTemplateAST					= Antlr.StringTemplate.Language.StringTemplateAST;
	using FormalArgument					= Antlr.StringTemplate.Language.FormalArgument;
	using TemplateParser					= Antlr.StringTemplate.Language.TemplateParser;
	using ActionLexer						= Antlr.StringTemplate.Language.ActionLexer;
	using ActionParser						= Antlr.StringTemplate.Language.ActionParser;
	using ActionEvaluator					= Antlr.StringTemplate.Language.ActionEvaluator;
	using ActionParserTokenTypes			= Antlr.StringTemplate.Language.ActionParserTokenTypes;
	using NewlineRef						= Antlr.StringTemplate.Language.NewlineRef;
	
	/// <summary>
	/// A <TT>StringTemplate</TT> is a "document" with holes in it where you can stick
	/// values.  <TT>StringTemplate</TT> breaks up your template into chunks of text and
	/// attribute expressions, which are by default enclosed in angle brackets:
	/// <TT>&lt;</TT><em>attribute-expression</em><TT>&gt;</TT>.  <TT>StringTemplate</TT>
	/// ignores everything outside of attribute expressions, treating it as just text to spit
	/// out when you call <TT>StringTemplate.toString()</TT>.
	/// 
	/// <P><TT>StringTemplate</TT> is not a "system" or "engine" or "server"; 
	/// it's a library with two classes of interest: <TT>StringTemplate</TT> and 
	/// <TT>StringTemplateGroup</TT>.  You can directly create a <TT>StringTemplate</TT> 
	/// in Java code or you can load a template from a file.
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
	/// <p>IStringTemplateErrorListener is an interface you can implement to
	/// specify where StringTemplate reports errors.  Setting the listener
	/// for a group automatically makes all associated StringTemplate
	/// objects use the same listener.  For example,
	/// 
	/// <font size=2><pre>
	/// StringTemplateGroup group = new StringTemplateGroup("loutSyndiags");
	/// group.setErrorListener(
	/// new IStringTemplateErrorListener() {
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
	/// object.  This is used primarily to get the group of that original
	/// so new templates can be loaded into that group.
	/// 
	/// <p>To write out a template, the chunks are walked in order and asked to
	/// write themselves out.  String chunks are obviously just written out,
	/// but the attribute expressions/actions are evaluated in light of the
	/// attributes in that object and possibly in an enclosing instance.
	/// </summary>
	public class StringTemplate
	{
		public const string VERSION = "2.3b7";

		/// <summary><@r()></summary>
		internal const int REGION_IMPLICIT = 1;

		/// <summary><@r>...<@end></summary>
		internal const int REGION_EMBEDDED = 2;

		/// <summary>@t.r() ::= "..." defined manually by coder</summary>
		internal const int REGION_EXPLICIT = 3;

		private static int GetNextTemplateCounter
		{
			get
			{
				lock (typeof(StringTemplate))
				{
					templateCounter++;
					return templateCounter;
				}
			}
		}
		/// <summary>
		/// Make an instance of this template; it contains an exact copy of
		/// everything (except the attributes and enclosing instance pointer).
		/// So the new template refers to the previously compiled chunks of this
		/// template but does not have any attribute values.
		/// </summary>
		virtual public StringTemplate GetInstanceOf()
		{
			//StringTemplate t = group.CreateStringTemplate();
			StringTemplate t = nativeGroup.CreateStringTemplate();
			dup(this, t);
			return t;
		}

		virtual public StringTemplate EnclosingInstance
		{
			get { return enclosingInstance; }
			set
			{
				if (this == value)
				{
					throw new ArgumentException("cannot embed template " + Name + " in itself");
				}
				// set the parent for this template
				this.enclosingInstance = value;
				// make the parent track this template as an embedded template
				if (value != null)
				{
					this.enclosingInstance.AddEmbeddedInstance(this);
				}
			}
		}

		virtual public StringTemplate OutermostEnclosingInstance
		{
			get
			{
				if ( enclosingInstance!=null ) 
				{
					return enclosingInstance.OutermostEnclosingInstance;
				}
				return this;
			}
		}

		virtual public IDictionary ArgumentContext
		{
			get { return argumentContext; }
			set { argumentContext = value; }
		}
		virtual public StringTemplateAST ArgumentsAST
		{
			get { return argumentsAST; }
			set { this.argumentsAST = value; }
		}
		virtual public string Name
		{
			get { return name; }
			set { this.name = value; }
		}
		virtual public string OutermostName
		{
			get
			{
				if (enclosingInstance != null)
				{
					return enclosingInstance.OutermostName;
				}
				return Name;
			}
			
		}
		virtual public StringTemplateGroup Group
		{
			get { return group; }
			set { this.group = value; }
		}
		virtual public StringTemplateGroup NativeGroup 
		{
			get { return nativeGroup; }
			set { this.nativeGroup = value; }
		}

		/// <summary>
		/// Return the outermost template's group file line number
		/// </summary>
		public int GroupFileLine 
		{
			get
			{
				if ( enclosingInstance!=null ) 
				{
					return enclosingInstance.GroupFileLine;
				}
				return groupFileLine;
			}
			set { this.groupFileLine = value; }
		}

		virtual public string Template
		{
			get { return pattern; }
			set
			{
				this.pattern = value;
				BreakTemplateIntoChunks();
			}
		}
		virtual public IStringTemplateErrorListener ErrorListener
		{
			get
			{
				if (errorListener == NullErrorListener.DefaultNullListener)
				{
					return group.ErrorListener;
				}
				return errorListener;
			}
			set 
			{
				if (value == null)
					this.errorListener = NullErrorListener.DefaultNullListener;
				else
					this.errorListener = value;
			}
		}
		virtual public int TemplateID
		{
			get { return templateID; }
		}
		virtual public IDictionary Attributes
		{
			get { return attributes; }
			set { this.attributes = value; }
		}

		/// <summary>
		/// Get a list of the strings and subtemplates and attribute
		/// refs in a template.
		/// </summary>
		virtual public IList Chunks
		{
			get { return chunks; }
		}

		/// <summary>
		/// Normally if you call template y from x, y cannot see any attributes
		/// of x that are defined as formal parameters of y.  Setting this
		/// passThroughAttributes to true, will override that and allow a
		/// template to see through the formal arg list to inherited values.
		/// </summary>
		virtual public bool PassThroughAttributes
		{
			set { this.passThroughAttributes = value; }
		}

		/// <summary>
		/// Specify a complete map of what object classes should map to which
		/// renderer objects.
		/// </summary>
		virtual public IDictionary AttributeRenderers
		{
			set { this.attributeRenderers = value; }
		}
		/// <summary>
		/// Make StringTemplate check your work as it evaluates templates.
		/// Problems are sent to error listener.   Currently warns when
		/// you set attributes that are not used.
		/// </summary>
		public static bool LintMode
		{
			set { StringTemplate.lintMode = value; }
		}

		virtual public string GetEnclosingInstanceStackTrace()
		{
			StringBuilder buf = new StringBuilder();
			Hashtable seen = new Hashtable();
			StringTemplate p = this;
			while (p != null)
			{
				if (seen.Contains(p))
				{
					buf.Append(p.GetTemplateDeclaratorString());
					buf.Append(" (start of recursive cycle)");
					buf.Append("\n");
					buf.Append("...");
					break;
				}
				seen[p] = null;
				buf.Append(p.GetTemplateDeclaratorString());
				if (p.attributes != null)
				{
					buf.Append(", attributes=[");
					int i = 0;
					for (IEnumerator iter = p.attributes.Keys.GetEnumerator(); iter.MoveNext(); )
					{
						string attrName = (string) iter.Current;
						if (i > 0)
						{
							buf.Append(", ");
						}
						i++;
						buf.Append(attrName);
						object o = p.attributes[attrName];
						if (o is StringTemplate)
						{
							StringTemplate st = (StringTemplate) o;
							buf.Append("=");
							buf.Append("<");
							buf.Append(st.Name);
							buf.Append("()@");
							buf.Append(st.TemplateID.ToString());
							buf.Append(">");
						}
						else if (o is IList)
						{
							buf.Append("=List[..");
							IList list = (IList) o;
							int n = 0;
							for (int j = 0; j < list.Count; j++)
							{
								object listValue = list[j];
								if (listValue is StringTemplate)
								{
									if (n > 0)
									{
										buf.Append(", ");
									}
									n++;
									StringTemplate st = (StringTemplate) listValue;
									buf.Append("<");
									buf.Append(st.Name);
									buf.Append("()@");
									buf.Append(st.TemplateID.ToString());
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
					buf.Append(CollectionUtils.ListToString(p.referencedAttributes));
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

		virtual public string GetTemplateDeclaratorString()
		{
			StringBuilder buf = new StringBuilder();
			buf.Append("<");
			buf.Append(Name);
			buf.Append("(");
			buf.Append(formalArguments.Keys.ToString());
			buf.Append(")@");
			buf.Append(TemplateID.ToString());
			buf.Append(">");
			return buf.ToString();
		}

		protected string GetTemplateHeaderString(bool showAttributes) 
		{
			if ( showAttributes ) 
			{
				StringBuilder buf = new StringBuilder();
				buf.Append(Name);
				if ( attributes != null ) 
				{
					buf.Append("[");

					bool appendSeparator = false;
					foreach (string attrName in attributes.Keys)
					{
						if (appendSeparator)
						{
							buf.Append(", ");
						}
						appendSeparator = true;

						buf.Append(attrName);
					}

					buf.Append("]");
				}
				return buf.ToString();
			}
			return Name;
		}

		/// <summary>
		/// If an instance of x is enclosed in a y which is in a z, return
		/// a String of these instance names in order from topmost to lowest;
		/// here that would be "[z y x]".
		/// </summary>
		virtual public string GetEnclosingInstanceStackString()
		{
			IList names = new ArrayList();
			StringTemplate p = this;
			while (p != null)
			{
				string name = p.Name;
				names.Insert(0, name + (p.passThroughAttributes ? "(...)" : ""));
				p = p.enclosingInstance;
			}
			return CollectionUtils.ListToString(names).Replace(",", "");
		}
		
		public bool IsRegion
		{
			get { return isRegion; }
			set { this.isRegion = value; }
		}

		public void AddRegionName(string name) 
		{
			if ( regions==null ) 
			{
				regions = new Hashtable();
			}
			regions.Add(name, name);
		}

		/// <summary>
		/// Does this template ref or embed region name?
		/// </summary>
		/// <param name="name">Region name</param>
		/// <returns>True if named region is found else false.</returns>
		public bool ContainsRegionName(string name) 
		{
			if ( regions==null ) 
			{
				return false;
			}
			return regions.Contains(name);
		}

		public int RegionDefType
		{
			get { return regionDefType; }
			set { this.regionDefType = value; }
		}

		/// <summary>
		/// An automatically created aggregate of properties.
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

			/// <summary>
			/// Allow StringTemplate to add values, but prevent the end
			/// user from doing so.
			/// </summary>
			internal void Put(string propName, object propValue)
			{
				properties[propName] = propValue;
			}
			public object Get(string propName)
			{
				object returnVal;
				try
				{
					returnVal = properties[propName];
				}
				catch
				{
					returnVal = null;
				}
				return returnVal;
			}
		}

		/// <summary>
		/// An internal ArrayList sub-class so we can differentiate between a 
		/// multi-valued attribute list and a plain incoming list.
		/// </summary>
		internal class STAttributeList : ArrayList
		{
			public STAttributeList() : base() {}
			public STAttributeList(int capacity) : base(capacity) {}
			public STAttributeList(ICollection collection) : base(collection) {}
		}

	public const string ANONYMOUS_ST_NAME = "anonymous";
		
		/// <summary>track probable issues like setting attribute that is not referenced. </summary>
		internal static bool lintMode = false;
		
		protected internal IList referencedAttributes = null;
		
		/// <summary>What's the name of this template? </summary>
		protected internal string name = ANONYMOUS_ST_NAME;
		
		private static int templateCounter = 0;
		/// <summary>
		/// reset the template ID counter to 0; public so that testing routine
		/// can access but not really of interest to the user.
		/// </summary>
		public static void  resetTemplateCounter()
		{
			templateCounter = 0;
		}
		
		protected internal int templateID = GetNextTemplateCounter;
		
		/// <summary>
		/// Enclosing instance if I'm embedded within another template.
		/// IF-subtemplates are considered embedded as well.
		/// </summary>
		protected internal StringTemplate enclosingInstance = null;
		
		/// <summary>A list of embedded templates </summary>
		protected internal IList embeddedInstances = null;
		
		/// <summary>
		/// If this template is an embedded template such as when you apply
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
		
		/// <summary>
		/// If this template is embedded in another template, the arguments
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
		
		/// <summary>
		/// When templates are defined in a group file format, the attribute
		/// list is provided including information about attribute cardinality
		/// such as present, optional, ...  When this information is available,
		/// RawSetAttribute should do a quick existence check as should the
		/// invocation of other templates.  So if you ref bold(item="foo") but
		/// item is not defined in bold(), then an exception should be thrown.
		/// When actually rendering the template, the cardinality is checked.
		/// This is a Map<String,FormalArgument>.
		/// </summary>
		protected internal IDictionary formalArguments = FormalArgument.UNKNOWN;
		
		/// <summary>
		/// How many formal arguments to this template have default values
		/// specified?
		/// </summary>
		protected internal int numberOfDefaultArgumentValues = 0;
		
		/// <summary>
		/// Normally, formal parameters hide any attributes inherited from the
		/// enclosing template with the same name.  This is normally what you
		/// want, but makes it hard to invoke another template passing in all
		/// the data.  Use notation now: <otherTemplate(...)> to say "pass in
		/// all data".  Works great.  Can also say <otherTemplate(foo="xxx",...)>
		/// </summary>
		protected internal bool passThroughAttributes = false;
		
		/// <summary>
		/// What group originally defined the prototype for this template?
		/// This affects the set of templates I can refer to.
		/// always refer to the super of the original group.
		/// 
		/// group base;
		/// t ::= "base";
		/// 
		/// group sub;
		/// t ::= "super.t()2"
		/// 
		/// group subsub;
		/// t ::= "super.t()3"
		/// </summary>
		protected StringTemplateGroup nativeGroup;

		/// <summary>
		/// This template was created as part of what group?  Even if this
		/// template was created from a prototype in a supergroup, its group
		/// will be the subgroup.  That's the way polymorphism works.
		/// </summary>
		protected internal StringTemplateGroup group;
		
		/// <summary>If this template is defined within a group file, what line number?</summary>
		protected int groupFileLine;

		/// <summary>Where to report errors </summary>
		protected IStringTemplateErrorListener errorListener = NullErrorListener.DefaultNullListener;
		
		/// <summary>
		/// The original, immutable pattern/language (not really used again after
		/// initial "compilation", setup/parsing).
		/// </summary>
		protected internal string pattern;
		
		/// <summary>
		/// Map an attribute name to its value(s).  These values are set by outside
		/// code via st.SetAttribute(name, value).  StringTemplate is like self in
		/// that a template is both the "class def" and "instance".  When you
		/// create a StringTemplate or setTemplate, the text is broken up into chunks
		/// (i.e., compiled down into a series of chunks that can be evaluated later).
		/// You can have multiple
		/// </summary>
		protected internal IDictionary attributes;
		
		/// <summary>
		/// A Map<Class,Object> that allows people to register a renderer for
		/// a particular kind of object to be displayed in this template.  This
		/// overrides any renderer set for this template's group.
		/// 
		/// Most of the time this map is not used because the StringTemplateGroup
		/// has the general renderer map for all templates in that group.
		/// Sometimes though you want to override the group's renderers.
		/// </summary>
		protected internal IDictionary attributeRenderers;
		
		/// <summary>
		/// A list of alternating string and ASTExpr references.
		/// This is compiled to when the template is loaded/defined and walked to
		/// write out a template instance.
		/// </summary>
		protected internal IList chunks;
		
		/// <summary>
		/// If someone refs <@r()> in template t, an implicit
		/// 
		/// @t.r() ::= ""
		/// 
		/// is defined, but you can overwrite this def by defining your
		/// own.  We need to prevent more than one manual def though.  Between
		/// this var and isEmbeddedRegion we can determine these cases. 
		/// </summary>
		protected int regionDefType;

		/// <summary>
		/// Does this template come from a <@region>...<@end> embedded in
		/// another template?
		/// </summary>
		protected bool isRegion;

		/// <summary>Set of implicit and embedded regions for this template</summary>
		protected IDictionary regions;

		protected internal static StringTemplateGroup defaultGroup = new StringTemplateGroup("defaultGroup", ".");
		
		/// <summary>
		/// Create a blank template with no pattern and no attributes
		/// </summary>
		public StringTemplate()
		{
			group = defaultGroup;		// make sure has a group even if default
			nativeGroup = defaultGroup; // same with nativeGroup
		}
		
		/// <summary>
		/// Create an anonymous template.  It has no name just
		/// chunks (which point to this anonymous template) and attributes.
		/// </summary>
		public StringTemplate(string template):this(null, template)
		{
		}
		
		public StringTemplate(string template, Type lexer):this()
		{
			Group = new StringTemplateGroup("defaultGroup", lexer);
			NativeGroup = Group;
			Template = template;
		}
		
		/// <summary>
		/// Create an anonymous template with no name, but with a group
		/// </summary>
		public StringTemplate(StringTemplateGroup group, string template):this()
		{
			if (group != null)
			{
				Group = group;
				NativeGroup = group;
			}
			Template = template;
		}
		
		public StringTemplate(StringTemplateGroup group, string template, Hashtable attributes)
			: this(group, template)
		{
			Attributes = attributes;
		}

		/// <summary>
		/// Make the 'to' template look exactly like the 'from' template
		/// except for the attributes.  This is like creating an instance
		/// of a class in that the executable code is the same (the template
		/// chunks), but the instance data is blank (the attributes).  Do
		/// not copy the enclosingInstance pointer since you will want this
		/// template to eval in a context different from the examplar.
		/// </summary>
		protected internal virtual void  dup(StringTemplate from, StringTemplate to)
		{
			to.pattern = from.pattern;
			to.chunks = from.chunks;
			to.formalArguments = from.formalArguments;
			to.numberOfDefaultArgumentValues = from.numberOfDefaultArgumentValues;
			to.name = from.name;
			to.group = from.group;
			to.nativeGroup = from.nativeGroup;
			to.errorListener = from.errorListener;
			to.regions = from.regions;
			to.isRegion = from.isRegion;
			to.regionDefType = from.regionDefType;
		}
		
		public virtual void  AddEmbeddedInstance(StringTemplate embeddedInstance)
		{
			if (this.embeddedInstances == null)
			{
				this.embeddedInstances = new ArrayList();
			}
			this.embeddedInstances.Add(embeddedInstance);
		}
		
		public virtual void  Reset()
		{
			attributes = new Hashtable(); // just throw out table and make new one
		}
		
		public virtual void  SetPredefinedAttributes()
		{
			if (!IsInLintMode)
			{
				return ; // only do this method so far in lint mode
			}
		}
		
		public virtual void  RemoveAttribute(string name)
		{
			if (attributes != null)
			{
				attributes.Remove(name);
			}
		}
		
		/// <summary>
		/// Set an attribute for this template.  If you set the same
		/// attribute more than once, you get a multi-valued attribute.
		/// If you send in a StringTemplate object as a value, it's
		/// enclosing instance (where it will inherit values from) is
		/// set to 'this'.  This would be the normal case, though you
		/// can set it back to null after this call if you want.
		/// 
		/// If you send in a List plus other values to the same
		/// attribute, they all get flattened into one List of values.
		/// 
		/// This will be a new list object so that incoming objects are
		/// not altered.
		/// 
		/// If you send in an array, it is converted to a List.  Works
		/// with arrays of objects and arrays of {int,float,double}.
		/// </summary>
		public virtual void  SetAttribute(string name, object val)
		{
			if ((name == null) || (val == null))
			{
				return ;
			}
			if (name.IndexOf('.') != -1)
			{
				throw new ArgumentException("cannot have '.' in attribute names", "name");
			}

			if (attributes == null)
			{
				attributes = new Hashtable();
			}
			
			if (val is StringTemplate)
			{
				((StringTemplate) val).EnclosingInstance = this;
			}
			
			// convert plain collections
			// get exactly in this scope (no enclosing)
			object o = attributes[name];
			if (o != null)
			{
				// it is or will be a multi-value attribute
				//Console.WriteLine("exists: "+name+"="+o);
				STAttributeList v = null;
				if (o is STAttributeList)
				{
					// already a multi-valued
					v = (STAttributeList) o;
				}
				else if (o is IList)
				{
					// existing attribute is non-ST List
					// must copy to an STAttributeList before adding new attribute
					v = new STAttributeList((ICollection)o);
					// replace existing attribute with the new STAttributeList
					RawSetAttribute(attributes, name, v);
				}
				else
				{
					// existing attribute is not a list
					// must convert to an STAttributeList before adding new attribute
					v = new STAttributeList();
					// replace existing attribute with the new STAttributeList
					RawSetAttribute(attributes, name, v);
					// add the pre-existing attribute to new STAttributeList
					v.Add(o);
				}

				// now we process the newly added attribute 
				if (val is IList)
				{
					// flatten incoming list into existing
					// (do nothing if same List to avoid trouble)
					if (v != val)
					{
						v.AddRange((ICollection)val);
					}
				}
				else
				{
					v.Add(val);
				}
			}
			else
			{
				// new attribute
				RawSetAttribute(attributes, name, val);
			}
		}
		
		/// <summary>
		/// Create an aggregate from the list of properties in aggrSpec and fill
		/// with values from values array.  This is not publically visible because
		/// it conflicts semantically with setAttribute("foo",new Object[] {...});
		/// </summary>
		public virtual void  SetAttribute(string aggrSpec, params object[] values)
		{
			IList properties = new ArrayList();
			if (aggrSpec.IndexOf(".{") == -1)
			{
				SetAttribute(aggrSpec, (object) values);
				return;
			}
			string aggrName = ParseAggregateAttributeSpec(aggrSpec, properties);
			if (values == null || properties.Count == 0)
			{
				throw new ArgumentException("missing properties or values for '" + aggrSpec + "'");
			}
			if (values.Length != properties.Count)
			{
				throw new ArgumentException("number of properties in '" + aggrSpec + "' != number of values");
			}
			Aggregate aggr = new Aggregate();
			for (int i = 0; i < values.Length; i++)
			{
				object val = values[i];
				if (val is StringTemplate)
				{
					((StringTemplate) val).EnclosingInstance = this;
				}
				aggr.Put((string) properties[i], val);
			}
			SetAttribute(aggrName, aggr);
		}
		
		/// <summary>
		/// Split "aggrName.{propName1,propName2}" into list [propName1,propName2]
		/// and the aggrName. Space is allowed around ','.
		/// </summary>
		protected virtual string ParseAggregateAttributeSpec(string aggrSpec, IList properties)
		{
			int dot = aggrSpec.IndexOf((char) '.');
			if (dot <= 0)
			{
				throw new ArgumentException("invalid aggregate attribute format: " + aggrSpec);
			}
			string aggrName = aggrSpec.Substring(0, (dot) - (0));
			string propString = aggrSpec.Substring(dot + 2, (aggrSpec.Length) - (dot + 3));
			string[] propList = propString.Split(new char[]{','});
			for (int i = 0; i < propList.Length; i++)
			{
				// Whitespace is allowed around ','. Trim it.
				properties.Add(propList[i].Trim());
			}
			return aggrName;
		}
		
		/// <summary>
		/// Map a value to a named attribute.  Throw NoSuchElementException if
		/// the named attribute is not formally defined in self's specific template
		/// and a formal argument list exists.
		/// </summary>
		protected internal virtual void  RawSetAttribute(IDictionary attributes, string name, object objValue)
		{
			if (formalArguments != FormalArgument.UNKNOWN && GetFormalArgument(name) == null)
			{
				// a normal call to setAttribute with unknown attribute
				throw new InvalidOperationException("no such attribute: " + name + " in template context " + GetEnclosingInstanceStackString());
			}
			if (objValue == null)
			{
				return ;
			}
			attributes[name] = objValue;
		}
		
		/// <summary>
		/// Argument evaluation such as foo(x=y), x must
		/// be checked against foo's argument list not this's (which is
		/// the enclosing context).  So far, only eval.g uses arg self as
		/// something other than "this".
		/// </summary>
		protected internal virtual void  RawSetArgumentAttribute(StringTemplate embedded, IDictionary attributes, string name, object objValue)
		{
			if (embedded.formalArguments != FormalArgument.UNKNOWN && embedded.GetFormalArgument(name) == null)
			{
				throw new InvalidOperationException("template " + embedded.Name + " has no such attribute: " + name + " in template context " + GetEnclosingInstanceStackString());
			}
			if (objValue == null)
			{
				return ;
			}
			attributes[name] = objValue;
		}
		
		public virtual object GetAttribute(string name)
		{
			return Get(this, name);
		}
		
		/// <summary>
		/// Walk the chunks, asking them to write themselves out according
		/// to attribute values of 'this.attributes'.  This is like evaluating or
		/// interpreting the StringTemplate as a program using the
		/// attributes.  The chunks will be identical (point at same list)
		/// for all instances of this template.
		/// </summary>
		public virtual int Write(IStringTemplateWriter output)
		{
			int n = 0;
			SetPredefinedAttributes();
			SetDefaultArgumentValues();
			for (int i = 0; chunks != null && i < chunks.Count; i++)
			{
				Expr a = (Expr) chunks[i];
				int chunkN = a.Write(this, output);
				// expr-on-first-line-with-no-output NEWLINE => NEWLINE
				if ( chunkN==0 
					&& i==0 
					&& (i+1)< chunks.Count 
					&& chunks[i+1] is NewlineRef )
				{
					//System.out.println("found pure first-line-blank \\n pattern");
					i++; // skip next NEWLINE;
					continue;
				}
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
				CheckForTrouble();
			}
			return n;
		}
		
		/// <summary>
		/// Resolve an attribute reference.  It can be in four possible places:
		/// 
		/// 1. the attribute list for the current template
		/// 2. if self is an embedded template, somebody invoked us possibly
		/// with arguments--check the argument context
		/// 3. if self is an embedded template, the attribute list for the enclosing
		/// instance (recursively up the enclosing instance chain)
		/// 4. if nothing is found in the enclosing instance chain, then it might
		/// be a map defined in the group or the its supergroup etc...
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
		/// This method is not static so people can override functionality.
		/// </summary>
		public virtual object Get(StringTemplate self, string attribute)
		{
			//System.out.println("### get("+self.getEnclosingInstanceStackString()+", "+attribute+")");
			//System.out.println("attributes="+(self.attributes!=null?self.attributes.keySet().toString():"none"));
			if (self == null)
			{
				return null;
			}
			
			if (lintMode)
			{
				self.TrackAttributeReference(attribute);
			}
			
			// is it here?
			object o = null;
			if (self.attributes != null)
			{
				o = self.attributes[attribute];
			}
			
			// nope, check argument context in case embedded
			if (o == null)
			{
				IDictionary argContext = self.ArgumentContext;
				if (argContext != null)
				{
					o = argContext[attribute];
				}
			}
			
			if (o == null && !self.passThroughAttributes && self.GetFormalArgument(attribute) != null)
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
				object valueFromEnclosing = Get(self.enclosingInstance, attribute);
				if (valueFromEnclosing == null)
				{
					CheckNullAttributeAgainstFormalArguments(self, attribute);
				}
				o = valueFromEnclosing;
			}
				// not found and no enclosing instance to look at
			else if (o == null && self.enclosingInstance == null)
			{
				// It might be a map in the group or supergroup...
				o = self.group.GetMap(attribute);
			}
			
			return o;
		}
		
		/// <summary>
		/// Walk a template, breaking it into a list of
		/// chunks: Strings and actions/expressions.
		/// </summary>
		protected internal virtual void  BreakTemplateIntoChunks()
		{
			//System.out.println("parsing template: "+pattern);
			if (pattern == null)
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
				Type lexerClass = group.TemplateLexerClass;
				ConstructorInfo ctor = lexerClass.GetConstructor(new Type[]{typeof(StringTemplate), typeof(TextReader)});
				CharScanner chunkStream = (CharScanner) ctor.Invoke(new object[]{this, new StringReader(pattern)});
				chunkStream.setTokenCreator(ChunkToken.Creator);
				TemplateParser chunkifier = new TemplateParser(chunkStream);
				chunkifier.template(this);
			}
			catch (Exception e)
			{
				string name = "<unknown>";
				string outerName = OutermostName;
				if (Name != null)
				{
					name = Name;
				}
				if (outerName != null && !name.Equals(outerName))
				{
					name = name + " nested in " + outerName;
				}
				Error("problem parsing template '" + name + "'", e);
			}
		}
		
		public virtual ASTExpr ParseAction(string action)
		{
			ActionLexer lexer = new ActionLexer(new StringReader(action.ToString()));
			lexer.setTokenCreator(StringTemplateToken.Creator);
			ActionParser parser = new ActionParser(lexer, this);
			parser.getASTFactory().setASTNodeCreator(StringTemplateAST.Creator);
			ASTExpr a = null;
			try
			{
				IDictionary options = parser.action();
				AST tree = parser.getAST();
				if (tree != null)
				{
					if (tree.Type == ActionParserTokenTypes.CONDITIONAL)
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
				Error("Can't parse chunk: " + action.ToString(), re);
			}
			catch (TokenStreamException tse)
			{
				Error("Can't parse chunk: " + action.ToString(), tse);
			}
			return a;
		}
		
		protected internal virtual void  AddChunk(Expr e)
		{
			if (chunks == null)
			{
				chunks = new ArrayList();
			}
			chunks.Add(e);
		}
		
		// F o r m a l  A r g  S t u f f
		
		public virtual IDictionary FormalArguments
		{
			get { return formalArguments; }
			set { formalArguments = value; }
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
		public virtual void  SetDefaultArgumentValues()
		{
			if (numberOfDefaultArgumentValues == 0)
			{
				return ;
			}
			if (argumentContext == null)
			{
				argumentContext = new Hashtable();
			}
			if (formalArguments != FormalArgument.UNKNOWN)
			{
				ICollection argNames = formalArguments.Keys;
				for (IEnumerator it = argNames.GetEnumerator(); it.MoveNext(); )
				{
					string argName = (string) it.Current;
					// use the default value then
					FormalArgument arg = (FormalArgument) formalArguments[argName];
					if (arg.defaultValueST != null)
					{
						object existingValue = GetAttribute(argName);
						if (existingValue == null)
						{
							// value unset?
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
		
		/// <summary>
		/// From this template upward in the enclosing template tree,
		/// recursively look for the formal parameter.
		/// </summary>
		public virtual FormalArgument LookupFormalArgument(string name)
		{
			FormalArgument arg = GetFormalArgument(name);
			if (arg == null && enclosingInstance != null)
			{
				arg = enclosingInstance.LookupFormalArgument(name);
			}
			return arg;
		}
		
		public virtual FormalArgument GetFormalArgument(string name)
		{
			return (FormalArgument) formalArguments[name];
		}
		
		public virtual void  DefineEmptyFormalArgumentList()
		{
			FormalArguments = new HashList();
		}
		
		public virtual void  DefineFormalArgument(string name)
		{
			DefineFormalArgument(name, null);
		}
		
		public virtual void  DefineFormalArguments(IList names)
		{
			if (names == null)
			{
				return ;
			}
			for (int i = 0; i < names.Count; i++)
			{
				string name = (string) names[i];
				DefineFormalArgument(name);
			}
		}
		
		public virtual void  DefineFormalArgument(string name, StringTemplate defaultValue)
		{
			if (defaultValue != null)
			{
				numberOfDefaultArgumentValues++;
			}
			FormalArgument a = new FormalArgument(name, defaultValue);
			if (formalArguments == FormalArgument.UNKNOWN)
			{
				formalArguments = new HashList();
			}
			formalArguments[name] = a;
		}
		
		/// <summary>
		/// Register a renderer for all objects of a particular type.  This
		/// overrides any renderer set in the group for this class type.
		/// </summary>
		public virtual void  RegisterAttributeRenderer(Type attributeClassType, object renderer)
		{
			if (attributeRenderers == null)
			{
				attributeRenderers = new Hashtable();
			}
			attributeRenderers[attributeClassType] = renderer;
		}
		
		/// <summary>
		/// What renderer is registered for this attributeClassType for
		/// this template.  If not found, the template's group is queried.
		/// </summary>
		public virtual IAttributeRenderer GetAttributeRenderer(Type attributeClassType)
		{
			IAttributeRenderer renderer = null;
			if (attributeRenderers != null)
			{
				renderer = (IAttributeRenderer) attributeRenderers[attributeClassType];
			}

			if (renderer == null)
			{
				// No renderer overrides exist for this template for 'attributeClassType'
				// check parent template if we are embedded
				if ( enclosingInstance != null ) 
				{
					renderer = enclosingInstance.GetAttributeRenderer(attributeClassType);
				}
				else
				{
					// We found no renderer overrides for the template and,
					// we aren't embedded so, check group
					renderer = group.GetAttributeRenderer(attributeClassType);
				}
			}
			return renderer;
		}
		
		// U T I L I T Y  R O U T I N E S
		
		public virtual void  Error(string msg)
		{
			Error(msg, null);
		}
		
		public virtual void  Warning(string msg)
		{
			ErrorListener.Warning(msg);
		}
		
		public virtual void  Error(string msg, Exception e)
		{
			ErrorListener.Error(msg, e);
		}
		
		public static bool IsInLintMode
		{
			get { return lintMode; }
		}
		
		/// <summary>
		/// Indicates that 'name' has been referenced in this template.
		/// </summary>
		protected internal virtual void  TrackAttributeReference(string name)
		{
			if (referencedAttributes == null)
			{
				referencedAttributes = new ArrayList();
			}
			referencedAttributes.Add(name);
		}
		
		/// <summary>
		/// Look up the enclosing instance chain (and include this) to see
		/// if st is a template already in the enclosing instance chain.
		/// </summary>
		public static bool IsRecursiveEnclosingInstance(StringTemplate st)
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
		
		/// <summary>
		/// A reference to an attribute with no value, must be compared against
		/// the formal parameter to see if it exists; if it exists all is well,
		/// but if not, throw an exception.
		/// 
		/// Don't do the check if no formal parameters exist for this template;
		/// ask enclosing.
		/// </summary>
		protected internal virtual void  CheckNullAttributeAgainstFormalArguments(StringTemplate self, string attribute)
		{
			if (self.FormalArguments == FormalArgument.UNKNOWN)
			{
				// bypass unknown arg lists
				if (self.enclosingInstance != null)
				{
					CheckNullAttributeAgainstFormalArguments(self.enclosingInstance, attribute);
				}
				return ;
			}
			FormalArgument formalArg = self.LookupFormalArgument(attribute);
			if (formalArg == null)
			{
				throw new InvalidOperationException("no such attribute: " + attribute + " in template context " + GetEnclosingInstanceStackString());
			}
		}
		
		/// <summary>
		/// Executed after evaluating a template.  For now, checks for setting
		/// of attributes not reference.
		/// </summary>
		protected internal virtual void  CheckForTrouble()
		{
			// we have table of set values and list of values referenced
			// compare, looking for SET BUT NOT REFERENCED ATTRIBUTES
			if (attributes == null)
			{
				return ;
			}
			// if in names and not in referenced attributes, trouble
			foreach (string name in attributes.Keys)
			{
				if (referencedAttributes != null && !referencedAttributes.Contains(name))
				{
					Warning(Name + ": set but not used: " + name);
				}
			}
			// can do the reverse, but will have lots of false warnings :(
		}
		
		public virtual string ToDebugString()
		{
			StringBuilder buf = new StringBuilder();
			buf.Append("template-" + GetTemplateDeclaratorString() + ":");
			buf.Append("chunks=");
			if (chunks != null)
			{
				buf.Append(CollectionUtils.ListToString(chunks));
			}
			buf.Append("attributes=[");
			if (attributes != null)
			{
				int n = 0;
				foreach (string name in attributes.Keys)
				{
					if (n > 0)
					{
						buf.Append(',');
					}
					buf.Append(name + "=");
					object @value = attributes[name];
					if (@value is StringTemplate)
						buf.Append(((StringTemplate) @value).ToDebugString());
					else
						buf.Append(@value);
					n++;
				}
				buf.Append("]");
			}
			return buf.ToString();
		}
		
		/// <summary>
		/// Don't print values, just report the nested structure with attribute names.
		/// Follow (nest) attributes that are templates only.
		/// </summary>
		/// <returns></returns>
		public string ToStructureString() 
		{
			return ToStructureString(0);
		}

		public string ToStructureString(int indent) 
		{
			StringBuilder buf = new StringBuilder();
			for (int i=1; i<=indent; i++) 
			{ // indent
				buf.Append("  ");
			}
			buf.Append(Name);
			//buf.Append(attributes.keySet());
			if ( attributes != null ) 
			{
				buf.Append("[");

				bool appendSeparator = false;
				foreach (string attrName in attributes.Keys)
				{
					if (appendSeparator)
					{
						buf.Append(", ");
					}
					appendSeparator = true;

					buf.Append(attrName);
				}

				buf.Append("]");
			}
			buf.Append(":\n");
			if ( attributes!=null ) 
			{
				for (IEnumerator iter = attributes.Keys.GetEnumerator(); iter.MoveNext(); ) 
				{
					string name = (string) iter.Current;
					object @value = attributes[name];
					if ( @value is StringTemplate ) 
					{ // descend
						buf.Append(((StringTemplate) @value).ToStructureString(indent+1));
					}
					else 
					{
						if ( @value is IList ) 
						{
							IList alist = (IList)@value;
							for (int i = 0; i < alist.Count; i++) 
							{
								object o = (object) alist[i];
								if ( o is StringTemplate ) 
								{ // descend
									buf.Append(((StringTemplate)o).ToStructureString(indent+1));
								}
							}
						}
						else if ( @value is IDictionary ) 
						{
							IDictionary m = (IDictionary)@value;
							ICollection mvalues = m.Values;
							for (IEnumerator iterator = mvalues.GetEnumerator(); iterator.MoveNext(); ) 
							{
								object o = (object) iterator.Current;
								if ( o is StringTemplate ) 
								{ // descend
									buf.Append(((StringTemplate)o).ToStructureString(indent+1));
								}
							}
						}
					}
				}
			}
			return buf.ToString();
		}

		/// <summary>
		/// Generate a DOT file for displaying the template enclosure graph; e.g.,
		///   digraph prof {
		///     "t1" -> "t2"
		///     "t1" -> "t3"
		///     "t4" -> "t5"
		///   }
		/// </summary>
		/// <param name="showAttributes"></param>
		/// <returns></returns>
		public StringTemplate GetDOTForDependencyGraph(bool showAttributes) 
		{
			string structure =
				"digraph StringTemplateDependencyGraph {\n" +
				"node [shape=$shape$, $if(width)$width=$width$,$endif$" +
				"      $if(height)$height=$height$,$endif$ fontsize=$fontsize$];\n" +
				"$edges:{e|\"$e.src$\" -> \"$e.trg$\"\n}$" +
				"}\n";
			StringTemplate graphST = new StringTemplate(structure);
			HashList edges = new HashList();
			this.GetDependencyGraph(edges, showAttributes);

			// for each source template
			foreach (string sourceTemplate in edges.Keys) 
			{
				IList targetNodes = (IList)edges[sourceTemplate];
				// for each target template
				foreach (string targetTemplate in targetNodes) 
				{
					graphST.SetAttribute("edges.{src,trg}", sourceTemplate, targetTemplate);
				}
			}
			graphST.SetAttribute("shape", "none");
			graphST.SetAttribute("fontsize", "11");
			graphST.SetAttribute("height", "0"); // make height
			return graphST;
		}

		/// <summary>
		/// Get a list of n->m edges where template n contains template m.
		/// The map you pass in is filled with edges: key->value.  Useful
		/// for having DOT print out an enclosing template graph.  It
		/// finds all direct template invocations too like <foo()> but not
		/// indirect ones like <(name)()>.
		/// 
		/// Ack, I just realized that this is done statically and hence
		/// cannot see runtime arg values on statically included templates.
		/// Hmm...someday figure out to do this dynamically as if we were
		/// evaluating the templates.  There will be extra nodes in the tree
		/// because we are static like method and method[...] with args.
		/// </summary>
		/// <param name="edges"></param>
		/// <param name="showAttributes"></param>
		public void GetDependencyGraph(IDictionary edges, bool showAttributes) 
		{
			string srcNode = this.GetTemplateHeaderString(showAttributes);
			if ( attributes!=null ) 
			{
				foreach (string name in attributes.Keys) 
				{
					object @value = attributes[name];
					if ( @value is StringTemplate ) 
					{
						string targetNode = ((StringTemplate)@value).GetTemplateHeaderString(showAttributes);
						PutToMultiValuedMap(edges, srcNode, targetNode);
						((StringTemplate)@value).GetDependencyGraph(edges, showAttributes); // descend
					}
					else 
					{
						if ( value is IList ) 
						{
							IList alist = (IList)@value;
							for (int i = 0; i < alist.Count; i++) 
							{
								object o = (object) alist[i];
								if ( o is StringTemplate ) 
								{
									string targetNode = ((StringTemplate)o).GetTemplateHeaderString(showAttributes);
									PutToMultiValuedMap(edges, srcNode, targetNode);
									((StringTemplate)o).GetDependencyGraph(edges, showAttributes); // descend
								}
							}
						}
						else if ( value is IDictionary ) 
						{
							IDictionary m = (IDictionary)@value;
							ICollection mvalues = m.Values;
							for (IEnumerator iterator = mvalues.GetEnumerator(); iterator.MoveNext();) 
							{
								object o = (object) iterator.Current;
								if ( o is StringTemplate ) 
								{
									string targetNode = ((StringTemplate)o).GetTemplateHeaderString(showAttributes);
									PutToMultiValuedMap(edges, srcNode, targetNode);
									((StringTemplate)o).GetDependencyGraph(edges, showAttributes); // descend
								}
							}
						}
					}
				}
			}
			// look in chunks too for template refs
			for (int i = 0; chunks!=null && i < chunks.Count; i++) 
			{
				Expr expr = (Expr) chunks[i];
				if ( expr is ASTExpr ) 
				{
					ASTExpr e = (ASTExpr)expr;
					AST tree = e.AST;
					AST includeAST = new CommonAST(new CommonToken(ActionEvaluator.INCLUDE, "include"));
					IEnumerator it = tree.findAllPartial(includeAST);
					while (it.MoveNext()) 
					{
						AST t = (AST) it.Current;
						string templateInclude = t.getFirstChild().getText();
						Console.Out.WriteLine("found include "+templateInclude);
						PutToMultiValuedMap(edges, srcNode, templateInclude);
						StringTemplateGroup group = Group;
						if ( group!=null ) 
						{
							StringTemplate st = group.GetInstanceOf(templateInclude);
							// descend into the reference template
							st.GetDependencyGraph(edges, showAttributes);
						}
					}
				}
			}
		}

		/// <summary>
		/// Manage a hash table like it has multiple unique values.  Dictionary<object,ISet>.
		/// Simulates ISet using an IList.
		/// </summary>
		/// <param name="map"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		protected void PutToMultiValuedMap(IDictionary map, object key, object val) 
		{
			ArrayList bag = (ArrayList) map[key];
			if ( bag==null ) 
			{
				bag = new ArrayList();
				map[key] = bag;
			}
			if ( !bag.Contains(val) )
				bag.Add(val);
		}

		public virtual void  PrintDebugString()
		{
			Console.Out.WriteLine("template-" + Name + ":");
			Console.Out.Write("chunks=");
			Console.Out.WriteLine(CollectionUtils.ListToString(chunks));
			if (attributes == null)
			{
				return ;
			}
			Console.Out.Write("attributes=[");
			int n = 0;
			foreach (string name in attributes.Keys)
			{
				if (n > 0)
				{
					Console.Out.Write(',');
				}
				object val = attributes[name];
				if (val is StringTemplate)
				{
					Console.Out.Write(name + "=");
					((StringTemplate) val).PrintDebugString();
				}
				else
				{
					if (val is IList)
					{
						ArrayList alist = (ArrayList) val;
						for (int i = 0; i < alist.Count; i++)
						{
							object o = (object) alist[i];
							Console.Out.Write(name + "[" + i + "] is " + o.GetType().FullName + "=");
							if (o is StringTemplate)
							{
								((StringTemplate) o).PrintDebugString();
							}
							else
							{
								Console.Out.WriteLine(o);
							}
						}
					}
					else
					{
						Console.Out.Write(name + "=");
						Console.Out.WriteLine(val);
					}
				}
				n++;
			}
			Console.Out.Write("]\n");
		}
		
		public override string ToString()
		{
			StringWriter output = new StringWriter();
			// Write the output to a StringWriter
			// TODO seems slow to create all these objects, can I use a singleton?
			IStringTemplateWriter wr = group.CreateInstanceOfTemplateWriter(output);
			try
			{
				Write(wr);
			}
			catch(IOException)
			{
				Error("Got IOException writing to writer " + wr.GetType().FullName);
			}
			return output.ToString();
		}
	}
}