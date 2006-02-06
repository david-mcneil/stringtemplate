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


namespace Antlr.StringTemplate
{
	using System;
	using IList					= System.Collections.IList;
	using ICollection			= System.Collections.ICollection;
	using IEnumerator			= System.Collections.IEnumerator;
	using IDictionary			= System.Collections.IDictionary;
	using DictionaryEntry		= System.Collections.DictionaryEntry;
	using ArrayList				= System.Collections.ArrayList;
	using Hashtable				= System.Collections.Hashtable;
	using Encoding				= System.Text.Encoding;
	using Stream				= System.IO.Stream;
	using StreamReader			= System.IO.StreamReader;
	using TextReader			= System.IO.TextReader;
	using TextWriter			= System.IO.TextWriter;
	using IOException			= System.IO.IOException;
	using Path					= System.IO.Path;
	using FileMode				= System.IO.FileMode;
	using FileAccess			= System.IO.FileAccess;
	using FileStream			= System.IO.FileStream;
	using StringBuilder			= System.Text.StringBuilder;
	using ConstructorInfo		= System.Reflection.ConstructorInfo;
	using DefaultTemplateLexer	= Antlr.StringTemplate.Language.DefaultTemplateLexer;
	using GroupLexer			= Antlr.StringTemplate.Language.GroupLexer;
	using GroupParser			= Antlr.StringTemplate.Language.GroupParser;
	using HashList				= Antlr.StringTemplate.Collections.HashList;
	using CollectionUtils		= Antlr.StringTemplate.Collections.CollectionUtils;
	
	/// <summary>
	/// Manages a group of named mutually-referential StringTemplate objects.
	/// Currently the templates must all live under a directory so that you
	/// can reference them as foo.st or gutter/header.st.  To refresh a
	/// group of templates, just create a new StringTemplateGroup and start
	/// pulling templates from there.  Or, set the refresh interval.
	/// 
	/// Use getInstanceOf(template-name) to get a string template
	/// to fill in.
	/// 
	/// The name of a template is the file name minus ".st" ending if present
	/// unless you name it as you load it.
	///
	/// You can use the group file format also to define a group of templates
	/// (this works better for code gen than for html page gen).  You must give
	/// a Reader to the ctor for it to load the group; this is general and
	/// distinguishes it from the ctors for the old-style "load template files
	/// from the disk".
	///
	/// 10/2005 I am adding a StringTemplateGroupLoader concept so people can 
	/// define supergroups within a group and have it load that group automatically.
	/// </summary>
	public class StringTemplateGroup
	{
		/// <summary>
		/// What lexer class to use to break up templates.  If no lexer class
		/// is set for this group, use the static default.
		/// </summary>
		virtual public Type TemplateLexerClass
		{
			get 
			{
				if (templateLexerClass != null)
				{
					return templateLexerClass;
				}
				return DEFAULT_TEMPLATE_LEXER_TYPE; 
			}
		}

		virtual public string Name
		{
			get { return name; }
			set { this.name = value; }
		}

		virtual public IStringTemplateErrorListener ErrorListener
		{
			get { return errorListener; }
			set { this.errorListener = value; }
		}

		/// <summary>
		/// Specify a complete map of what object classes should map to which
		/// renderer objects for every template in this group (that doesn't
		/// override it per template).
		/// </summary>
		virtual public IDictionary AttributeRenderers
		{
			set { this.attributeRenderers = value; }
		}
		/// <summary>What is the group name </summary>
		protected string name;
		
		/// <summary>Maps template name to StringTemplate object </summary>
		protected IDictionary templates = new HashList();
		
		/// <summary>
		/// Maps map names to HashMap objects.  This is the list of maps
		/// defined by the user like typeInitMap ::= ["int":"0"]
		/// </summary>
		protected IDictionary maps = new Hashtable();
		
		/// <summary>How to pull apart a template into chunks? </summary>
		protected Type templateLexerClass = null;
		
		/// <summary>
		/// You can set the lexer once if you know all of your groups use the
		/// same separator.  If the instance has templateLexerClass set
		/// then it is used as an override.
		/// </summary>
		protected static Type DEFAULT_TEMPLATE_LEXER_TYPE = typeof(DefaultTemplateLexer);

																					/// <summary>
		/// Encapsulates the logic for finding and loading [external] templates.
		/// </summary>
		protected StringTemplateLoader templateLoader;
		
		/// <summary>Track all groups by name; maps name to StringTemplateGroup </summary>
		protected static IDictionary nameToGroupMap = new Hashtable();
		
		/// <summary>Track all interfaces by name; maps name to StringTemplateGroupInterface</summary>
		protected static IDictionary nameToInterfaceMap = new Hashtable();

		/// <summary>Are we derived from another group?  Templates not found in this group
		/// will be searched for in the superGroup recursively.
		/// </summary>
		protected StringTemplateGroup superGroup = null;
		
		/// <summary>Keep track of all interfaces implemented by this group.</summary>
		protected IList interfaces = null;

		/// <summary>
		/// When templates are files on the disk, the refresh interval is used
		/// to know when to reload.  When a Reader is passed to the ctor,
		/// it is a stream full of template definitions.  The former is used
		/// for web development, but the latter is most likely used for source
		/// code generation for translators; a refresh is unlikely.  Anyway,
		/// I decided to track the source of templates in case such info is useful
		/// in other situations than just turning off refresh interval.  I just
		/// found another: don't ever look on the disk for individual templates
		/// if this group is a group file...immediately look into any super group.
		/// If not in the super group, report no such template.
		/// </summary>
		protected bool templatesDefinedInGroupFile = false;
		
		/// <summary>
		/// Normally AutoIndentWriter is used to filter output, but user can
		/// specify a new one.
		/// </summary>
		protected Type userSpecifiedWriter;
		
		/// <summary>
		/// A Map<Class,Object> that allows people to register a renderer for
		/// a particular kind of object to be displayed for any template in this
		/// group.  For example, a date should be formatted differently depending
		/// on the locale.  You can set Date.class to an object whose
		/// toString(Object) method properly formats a Date attribute
		/// according to locale.  Or you can have a different renderer object
		/// for each locale.
		/// 
		/// These render objects are used way down in the evaluation chain
		/// right before an attribute's toString() method would normally be
		/// called in ASTExpr.write().
		/// </summary>
		protected IDictionary attributeRenderers;
		
		/// <summary>
		/// If a group file indicates it derives from a supergroup, how do we
		/// find it?  Shall we make it so the initial StringTemplateGroup file
		/// can be loaded via this loader?  Right now we pass a Reader to ctor
		/// to distinguish from the other variety.
		/// </summary>
		private static IStringTemplateGroupLoader groupLoader = null;

		/// <summary>
		/// Where to report errors.  All string templates in this group
		/// use this error handler by default.
		/// </summary>
		protected IStringTemplateErrorListener errorListener = DEFAULT_ERROR_LISTENER;
		
		public static IStringTemplateErrorListener DEFAULT_ERROR_LISTENER = ConsoleErrorListener.DefaultConsoleListener;
		
		/// <summary>
		/// Used to indicate that the template doesn't exist.
		/// We don't have to check disk for it; we know it's not there.
		/// </summary>
		protected static readonly StringTemplate NOT_FOUND_ST = new StringTemplate();
		
		/// <summary>
		/// Create a group manager for some templates, all of which are
		/// at or below the indicated directory.
		/// </summary>
		public StringTemplateGroup(string name, string rootDir)
			: this(name, new FileSystemTemplateLoader(rootDir), typeof(DefaultTemplateLexer))
		{
		}
		
		public StringTemplateGroup(string name, string rootDir, Type lexer)
			: this(name, new FileSystemTemplateLoader(rootDir), lexer)
		{
		}

		public StringTemplateGroup(string name, StringTemplateLoader templateLoader)
			: this(name, templateLoader, typeof(DefaultTemplateLexer))
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="templateLoader"></param>
		/// <param name="lexer"></param>
		public StringTemplateGroup(string name, StringTemplateLoader templateLoader, Type lexer)
		{
			this.name = name;
			nameToGroupMap[name] = this;
			if (templateLoader == null)
				this.templateLoader = new NullTemplateLoader();
			else
				this.templateLoader = templateLoader;
			this.templateLexerClass = lexer;
		}
		
		/// <summary>
		/// Create a group manager for some templates, all of which are
		/// loaded as resources via the classloader.
		/// </summary>
		public StringTemplateGroup(string name)
			: this(name, (StringTemplateLoader)null, null)
		{
		}
		
		public StringTemplateGroup(string name, Type lexer)
			: this(name, (StringTemplateLoader)null, lexer)
		{
		}
		
		/// <summary>
		/// Create a group from the template group defined by a input stream.
		/// The name is pulled from the file.  The format is
		/// 
		/// group name;
		/// 
		/// t1(args) ::= "..."
		/// t2() ::= <<
		/// >>
		/// ...
		/// </summary>
		public StringTemplateGroup(TextReader r)
			: this(r, null, DEFAULT_ERROR_LISTENER, null)
		{
		}
		
		public StringTemplateGroup(TextReader r, IStringTemplateErrorListener errorListener)
			: this(r, null, errorListener, null)
		{
		}
		
		public StringTemplateGroup(TextReader r, Type lexer)
			: this(r, lexer, null, null)
		{
		}
		
		public StringTemplateGroup(TextReader r, Type lexer, IStringTemplateErrorListener errorListener)
			: this(r, lexer, errorListener, null)
		{
		}
		
		/// <summary>
		/// Create a group from the input stream, but use a nondefault lexer
		/// to break the templates up into chunks.  This is usefor changing
		/// the delimiter from the default $...$ to <...>, for example.
		/// </summary>
		public StringTemplateGroup(
			TextReader r, 
			Type lexer, 
			IStringTemplateErrorListener errorListener,
			StringTemplateGroup superGroup)
		{
			this.templatesDefinedInGroupFile = true;
			this.templateLexerClass = lexer;
			if (errorListener == null)
				this.errorListener = DEFAULT_ERROR_LISTENER;
			else
				this.errorListener = errorListener;
			this.superGroup = superGroup;
			this.templateLoader = new NullTemplateLoader();
			ParseGroup(r);
			VerifyInterfaceImplementations();
		}
		
		public virtual void  SetSuperGroup(string groupName)
		{
			StringTemplateGroup group = (StringTemplateGroup) nameToGroupMap[groupName];
			if ( group!=null ) 
			{ // we've seen before; just use it
				this.SuperGroup = group;
				return;
			}
			group = LoadGroup(groupName); // else load it
			if ( group!=null ) 
			{
				nameToGroupMap[groupName] = group;
				this.SuperGroup = group;
			}
			else 
			{
				if ( groupLoader==null ) 
				{
					Error("no group loader registered", null);
				}
			}
		}
		
		public virtual StringTemplateGroup SuperGroup
		{
			get { return superGroup; }
			set { superGroup = value; }
		}
		
		/// <summary>
		/// Just track the new interface; check later.  Allows dups, but no biggie.
		/// </summary>
		/// <param name="iface"></param>
		public void ImplementInterface(StringTemplateGroupInterface iface) 
		{
			if ( interfaces==null ) 
			{
				interfaces = new ArrayList();
			}
			interfaces.Add(iface);
		}

		/// <summary>
		/// Indicate that this group implements this interface; load if necessary
		/// if not in the nameToInterfaceMap.
		/// </summary>
		/// <param name="interfaceName"></param>
		public void ImplementInterface(string interfaceName) 
		{
			StringTemplateGroupInterface iface = (StringTemplateGroupInterface)nameToInterfaceMap[interfaceName];
			if ( iface!=null ) 
			{ // we've seen before; just use it
				ImplementInterface(iface);
				return;
			}
			iface = LoadInterface(interfaceName); // else load it
			if ( iface!=null ) 
			{
				nameToInterfaceMap[interfaceName] = iface;
				ImplementInterface(iface);
			}
			else 
			{
				if ( groupLoader==null ) 
				{
					Error("no group loader registered", null);
				}
			}
		}

		/// <summary>StringTemplate object factory; each group can have its own. </summary>
		public virtual StringTemplate CreateStringTemplate()
		{
			return new StringTemplate();
		}
		
		/// <summary>
		/// A support routine that gets an instance of the StringTemplate with the 
		/// specified name knowing which ST encloses it for error messages.
		/// </summary>
		/// <param name="enclosingInstance"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual StringTemplate GetInstanceOf(StringTemplate enclosingInstance, string name)
		{
			StringTemplate st = LookupTemplate(enclosingInstance, name);
			StringTemplate instanceST = st.GetInstanceOf();
			return instanceST;
		}

		/// <summary>
		/// The primary means of getting an instance of a template from this group.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual StringTemplate GetInstanceOf(string name)
		{
			return GetInstanceOf(null, name);
		}
		
		/// <summary>
		/// The primary means of getting an instance of a template from this
		/// group when you have a predefined set of attributes you want to
		/// use.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="attributes"></param>
		/// <returns></returns>
		public StringTemplate GetInstanceOf(string name, IDictionary attributes) 
		{
			StringTemplate st = GetInstanceOf(name);
			st.Attributes = attributes;
			return st;
		}

		public virtual StringTemplate GetEmbeddedInstanceOf(StringTemplate enclosingInstance, string name)
		{
			StringTemplate st = null;
			// TODO: seems like this should go into LookupTemplate
			if ( name.StartsWith("super.") ) 
			{
				// for super.foo() refs, ensure that we look at the native
				// group for the embedded instance not the current evaluation
				// group (which is always pulled down to the original group
				// from which somebody did group.getInstanceOf("foo");
				st = enclosingInstance.NativeGroup.GetInstanceOf(enclosingInstance, name);
			}
			else 
			{
				st = GetInstanceOf(enclosingInstance, name);
			}
			// make sure all embedded templates have the same group as enclosing
			// so that polymorphic refs will start looking at the original group
			st.Group = this;
			st.EnclosingInstance = enclosingInstance;
			return st;
		}
		
		/// <summary>
		/// Get the template called 'name' from the group.  If not found,
		/// attempt to load.  If not found on disk, then try the superGroup
		/// if any.  If not even there, then record that it's
		/// NOT_FOUND so we don't waste time looking again later.  If we've gone
		/// past refresh interval, flush and look again.
		/// 
		/// If I find a template in a super group, copy an instance down here
		/// </summary>
		public virtual StringTemplate LookupTemplate(StringTemplate enclosingInstance, string name)
		{
			if (name.StartsWith("super."))
			{
				if (superGroup != null)
				{
					int dot = name.IndexOf('.');
					name = name.Substring(dot + 1, (name.Length) - (dot + 1));
					StringTemplate superScopeST =
						superGroup.LookupTemplate(enclosingInstance,name);
					return superScopeST;
				}
				throw new StringTemplateException(Name + " has no super group; invalid template: " + name);
			}
			// Discard cached template?
			if (templateLoader.HasChanged(name))
			{
				templates.Remove(name);
			}
			StringTemplate st = (StringTemplate) templates[name];
			if (st == null)
			{
				// not there?  Attempt to load
				if (!templatesDefinedInGroupFile)
				{
					// only check the disk for individual template
					st = LoadTemplate(name);
				}
				if (st == null && superGroup != null)
				{
					// try to resolve in super group
					st = superGroup.GetInstanceOf(name);
					// make sure that when we inherit a template, that it's
					// group is reset; it's nativeGroup will remain where it was
					if (st != null)
					{
						st.Group = this;
					}
				}
				if (st != null)
				{
					// found in superGroup
					// insert into this group; refresh will allow super
					// to change it's def later or this group to add
					// an override.
					templates[name] = st;
				}
				else
				{
					// not found; remember that this sucker doesn't exist
					templates[name] = NOT_FOUND_ST;
					string context = "";
					if ( enclosingInstance!=null ) 
					{
						context = "; context is "+ enclosingInstance.GetEnclosingInstanceStackString();
					}
					throw new TemplateLoadException(this, "Can't load template '"+ GetLocationFromTemplateName(name) + "'" + context);
				}
			}
			else if (st == NOT_FOUND_ST)
			{
				return null;
			}
			return st;
		}
		
		public virtual StringTemplate LookupTemplate(string name)
		{
			return LookupTemplate(null, name);
		}

		protected virtual string GetLocationFromTemplateName(string templateName)
		{
			return templateLoader.GetLocationFromTemplateName(templateName);
		}
		
		/// <summary>
		/// Loads and returns the named [external] template.
		/// </summary>
		/// <param name="templateName">Name of template to load</param>
		/// <returns>The named template instance or null</returns>
		protected virtual StringTemplate LoadTemplate(string templateName)
		{
			string templateText = templateLoader.LoadTemplate(templateName);
			if (templateText != null)
			{
				return DefineTemplate(templateName, templateText);
			}
			return null;
		}
		
		/// <summary>
		/// Define an examplar template; precompiled and stored
		/// with no attributes.  Remove any previous definition.
		/// </summary>
		public virtual StringTemplate DefineTemplate(string name, string template)
		{
			StringTemplate st = CreateStringTemplate();
			st.Name = name;
			st.Group = this;
			st.NativeGroup = this;
			st.Template = template;
			st.ErrorListener = errorListener;
			templates[name] = st;
			return st;
		}
		
		/// <summary>
		/// Track all references to regions &lt;@foo&gt;...&lt;@end&gt; or &lt;@foo()&gt;.
		/// </summary>
		public StringTemplate DefineRegionTemplate(
			string enclosingTemplateName, 
			string regionName, 
			string template, 
			int type)
		{
			string mangledName = GetMangledRegionName(enclosingTemplateName,regionName);
			StringTemplate regionST = DefineTemplate(mangledName, template);
			regionST.IsRegion = true;
			regionST.RegionDefType = type;
			return regionST;
		}

		/// <summary>
		/// Track all references to regions &lt;@foo&gt;...&lt;@end&gt; or &lt;@foo()&gt;.
		/// </summary>
		public StringTemplate DefineRegionTemplate(
			StringTemplate enclosingTemplate, 
			string regionName, 
			string template, 
			int type)
		{
			StringTemplate regionST =
				DefineRegionTemplate(enclosingTemplate.OutermostName,
				regionName,
				template,
				type);
			enclosingTemplate.OutermostEnclosingInstance.AddRegionName(regionName);
			return regionST;
		}

		/// <summary>
		/// Track all references to regions &lt;@foo()&gt;. We automatically
		/// define as
		///
		///    @enclosingtemplate.foo() ::= ""
		///
		/// You cannot set these manually in the same group; you have to subgroup
		/// to override.
		/// </summary>
		public StringTemplate DefineImplicitRegionTemplate(StringTemplate enclosingTemplate, string name)
		{
			return DefineRegionTemplate(enclosingTemplate,
				name,
				"",
				StringTemplate.REGION_IMPLICIT);
		}

		/// <summary>
		/// The "foo" of t() ::= "<@foo()>" is mangled to "region#t#foo"
		/// </summary>
		public string GetMangledRegionName(string enclosingTemplateName, string name)
		{
			return "region__"+enclosingTemplateName+"__"+name;
		}

		/// <summary>
		/// Return "t" from "region__t__foo"
		/// </summary>
		public string GetUnMangledTemplateName(string mangledName)
		{
			return mangledName.Substring("region__".Length, mangledName.LastIndexOf("__") - "region__".Length);
		}

		/// <summary>
		/// Make name and alias for target.  Replace any previous def of name 
		/// </summary>
		public virtual StringTemplate DefineTemplateAlias(string name, string target)
		{
			StringTemplate targetST = GetTemplateDefinition(target);
			if (targetST == null)
			{
				Error("cannot alias " + name + " to undefined template: " + target);
				return null;
			}
			templates[name] = targetST;
			return targetST;
		}
		
		public virtual bool IsDefinedInThisGroup(string name)
		{
			StringTemplate st = (StringTemplate)templates[name];
			if ( st!=null ) 
			{
				if ( st.IsRegion ) 
				{
					// don't allow redef of @t.r() ::= "..." or <@r>...<@end>
					if ( st.RegionDefType == StringTemplate.REGION_IMPLICIT ) 
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}
		
		/// <summary>
		/// Get the ST for 'name' in this group only 
		/// </summary>
		public virtual StringTemplate GetTemplateDefinition(string name)
		{
			return (StringTemplate) templates[name];
		}
		
		/// <summary>Is there *any* definition for template 'name' in this template
		/// or above it in the group hierarchy?
		/// </summary>
		public virtual bool IsDefined(string name)
		{
			try
			{
				return (LookupTemplate(name) != null);
			}
			catch (Exception)
			{
				return false;
			}
		}
		
		protected internal virtual void  ParseGroup(TextReader r)
		{
			try
			{
				GroupLexer lexer = new GroupLexer(r);
				GroupParser parser = new GroupParser(lexer);
				parser.group(this);
			}
			catch (System.Exception e)
			{
				string name = "<unknown>";
				if (Name != null)
				{
					name = Name;
				}
				Error("problem parsing group '" + name + "': "+e, e);
			}
		}
		
		/// <summary>
		/// Verify that this group satisfies its interfaces
		/// </summary>
		protected void VerifyInterfaceImplementations() 
		{
			for (int i = 0; interfaces!=null && i < interfaces.Count; i++) 
			{
				StringTemplateGroupInterface iface = (StringTemplateGroupInterface)interfaces[i];
				IList missing = iface.GetMissingTemplates(this);
				IList mismatched = iface.GetMismatchedTemplates(this);
				if ( missing!=null ) 
				{
					Error("group '" +Name+ "' does not satisfy interface '" +
						iface.Name+ "': missing templates " +CollectionUtils.ListToString(missing));
				}
				if ( mismatched!=null ) 
				{
					Error("group '" +Name+ "' does not satisfy interface '" +
						iface.Name+ "': mismatched template arguments " +CollectionUtils.ListToString(mismatched));
				}
			}
		}

		/// <summary>
		/// Specify a IStringTemplateWriter implementing class to use for
		/// filtering output
		/// </summary>
		public virtual void  SetTemplateWriterType(Type c)
		{
			userSpecifiedWriter = c;
		}
		
		/// <summary>
		/// Return an instance of a IStringTemplateWriter that spits output to w.
		/// If a writer is specified, use it instead of the default.
		/// </summary>
		public virtual IStringTemplateWriter CreateInstanceOfTemplateWriter(TextWriter w)
		{
			IStringTemplateWriter stw = null;
			if (userSpecifiedWriter != null)
			{
				try
				{
					ConstructorInfo ctor = userSpecifiedWriter.GetConstructor(new Type[]{typeof(TextWriter)});
					stw = (IStringTemplateWriter) ctor.Invoke(new object[]{w});
				}
				catch (System.Exception e)
				{
					Error("problems getting IStringTemplateWriter", e);
				}
			}
			if (stw == null)
			{
				stw = new AutoIndentWriter(w);
			}
			return stw;
		}
		
		/// <summary>Register a renderer for all objects of a particular type for all
		/// templates in this group.
		/// </summary>
		public virtual void  RegisterAttributeRenderer(Type attributeClassType, object renderer)
		{
			if (attributeRenderers == null)
			{
				attributeRenderers = new Hashtable();
			}
			attributeRenderers[attributeClassType] = renderer;
		}
		
		/// <summary>What renderer is registered for this attributeClassType for
		/// this group?  If not found, as superGroup if it has one.
		/// </summary>
		public virtual IAttributeRenderer GetAttributeRenderer(Type attributeClassType)
		{
			if (attributeRenderers == null)
			{
				if (superGroup == null)
				{
					return null; // no renderers and no parent?  Stop.
				}
				// no renderers; consult super group
				return superGroup.GetAttributeRenderer(attributeClassType);
			}
			
			IAttributeRenderer renderer = (IAttributeRenderer) attributeRenderers[attributeClassType];
			if (renderer == null)
			{
				if (superGroup != null)
				{
					// no renderer registered for this class, check super group
					renderer = superGroup.GetAttributeRenderer(attributeClassType);
				}
			}
			return renderer;
		}
		
		public virtual IDictionary GetMap(string name)
		{
			if (maps == null)
			{
				if (superGroup == null)
				{
					return null;
				}
				return superGroup.GetMap(name);
			}
			IDictionary m = (IDictionary) maps[name];
			if (m == null && superGroup != null)
			{
				m = superGroup.GetMap(name);
			}
			return m;
		}
		
		public virtual void  DefineMap(string name, IDictionary mapping)
		{
			maps[name] = mapping;
		}
		
		public static void RegisterDefaultLexer(Type lexerClass) 
		{
			DEFAULT_TEMPLATE_LEXER_TYPE = lexerClass;
		}

		public static void RegisterGroupLoader(IStringTemplateGroupLoader loader) 
		{
			groupLoader = loader;
		}

		public static StringTemplateGroup LoadGroup(String name) 
		{
			if ( groupLoader!=null ) 
			{
				return groupLoader.LoadGroup(name);
			}
			return null;
		}

		public static StringTemplateGroupInterface LoadInterface(String name) 
		{
			if ( groupLoader!=null ) 
			{
				return groupLoader.LoadInterface(name);
			}
			return null;
		}

		public virtual void  Error(string msg)
		{
			errorListener.Error(msg, null);
		}
		
		public virtual void  Error(string msg, Exception e)
		{
			errorListener.Error(msg, e);
		}
		
		public ICollection GetTemplateNames() 
		{
			return templates.Keys;
		}

		public override string ToString()
		{
			return ToString(true);
		}

		public virtual string ToString(bool showTemplatePatterns)
		{
			StringBuilder buf = new StringBuilder();
			buf.Append("group " + Name + ";\n");
			StringTemplate formalArgs = new StringTemplate("$args;separator=\",\"$");
			foreach (DictionaryEntry e in new System.Collections.SortedList(templates))
			{
				string tname = (string) e.Key;
				StringTemplate st = (StringTemplate) templates[tname];
				if (st != NOT_FOUND_ST)
				{
					formalArgs = formalArgs.GetInstanceOf();
					formalArgs.SetAttribute("args", st.FormalArguments);
					buf.Append(tname + "(" + formalArgs + ")");
					if ( showTemplatePatterns ) 
					{
						buf.Append(" ::= ");
						buf.Append("<<");
						buf.Append(st.Template);
						buf.Append(">>\n");
					}
					else
					{
						buf.Append('\n');
					}
				}
			}
			return buf.ToString();
		}
	}
}