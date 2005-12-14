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
using System.Text;
using DefaultTemplateLexer = antlr.stringtemplate.language.DefaultTemplateLexer;
using GroupLexer = antlr.stringtemplate.language.GroupLexer;
using GroupParser = antlr.stringtemplate.language.GroupParser;

namespace antlr.stringtemplate
{
	
	/// <summary>Manages a group of named mutually-referential StringTemplate objects.
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
	/// </summary>
	public class StringTemplateGroup
	{
		public class AnonymousClassStringTemplateErrorListener : StringTemplateErrorListener
		{
			public virtual void error(String s, System.Exception e)
			{
				System.Console.Error.WriteLine(s);
				if (e != null)
				{
					SupportClass.WriteStackTrace(e, System.Console.Error);
				}
			}
			public virtual void warning(String s)
			{
				System.Console.Out.WriteLine(s);
			}
			public virtual void debug(String s)
			{
				System.Console.Out.WriteLine(s);
			}
		}
		/// <summary>What is the group name </summary>
		protected internal String name;
		
		/// <summary>Maps template name to StringTemplate object </summary>
		protected internal IDictionary templates = new Hashtable();

		/// <summary>
		/// Maps map names to HashMap objects.  This is the list of maps
		/// defined by the user like typeInitMap ::= ["int":"0"]
		/// </summary>
		protected internal IDictionary maps = new Hashtable();
		
		/// <summary>How to pull apart a template into chunks? </summary>
		protected internal System.Type templateLexerClass = typeof(DefaultTemplateLexer);
		
		/// <summary>Under what directory should I look for templates?  If null,
		/// to look into the CLASSPATH for templates as resources.
		/// </summary>
		protected internal String rootDir = null;
		
		/// <summary>Track all groups by name; maps name to StringTemplateGroup </summary>
		protected internal static IDictionary nameToGroupMap = new Hashtable();
		
		/// <summary>Are we derived from another group?  Templates not found in this group
		/// will be searched for in the superGroup recursively.
		/// </summary>
		protected internal StringTemplateGroup superGroup = null;
		
		/// <summary>When templates are files on the disk, the refresh interval is used
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
		protected internal bool templatesDefinedInGroupFile = false;
		
		/// <summary>Normally AutoIndentWriter is used to filter output, but user can
		/// specify a new one.
		/// </summary>
		protected internal System.Type userSpecifiedWriter;

		/// <summary>
		/// A Map that allows people to register a renderer for
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
		protected internal IDictionary attributeRenderers;

		/// <summary>Where to report errors.  All string templates in this group
		/// use this error handler by default.
		/// </summary>
		protected internal StringTemplateErrorListener listener = DEFAULT_ERROR_LISTENER;
		
		public static StringTemplateErrorListener DEFAULT_ERROR_LISTENER;
		
		/// <summary>Used to indicate that the template doesn't exist.
		/// We don't have to check disk for it; we know it's not there.
		/// </summary>
		protected internal static readonly StringTemplate NOT_FOUND_ST = new StringTemplate();
		
		/// <summary>How long before tossing out all templates in seconds. </summary>
		protected internal int refreshIntervalInSeconds = System.Int32.MaxValue / 1000; // default: no refreshing from disk
		protected internal DateTime lastCheckedDisk = DateTime.Now;
		
		/// <summary>How are the files encode (ascii, UTF8, ...)?  You might want to read
		/// UTF8 for example on a ascii machine.
		/// </summary>
		internal String fileCharEncoding = Encoding.Default.BodyName;

		// IAttributeStrategy
		protected IAttributeStrategy atttributeStrategy;
		
		/// <summary>Create a group manager for some templates, all of which are
		/// at or below the indicated directory.
		/// </summary>
		public StringTemplateGroup(String name, String rootDir):this(name, rootDir, typeof(DefaultTemplateLexer))
		{
		}
		
		public StringTemplateGroup(String name, String rootDir, System.Type lexer)
		{
			this.name = name;
			this.rootDir = rootDir;
			lastCheckedDisk = DateTime.Now;
			nameToGroupMap[name] = this;
			this.templateLexerClass = lexer;
		}
		
		/// <summary>Create a group manager for some templates, all of which are
		/// loaded as resources via the classloader.
		/// </summary>
		public StringTemplateGroup(String name):this(name, null, typeof(DefaultTemplateLexer))
		{
		}
		
		public StringTemplateGroup(String name, System.Type lexer):this(name, null, lexer)
		{
		}
		
		/// <summary>Create a group from the template group defined by a input stream.
		/// The name is pulled from the file.  The format is
		/// 
		/// group name;
		/// 
		/// t1(args) : "..."
		/// t2 : <<
		/// >>
		/// ...
		/// </summary>
		public StringTemplateGroup(System.IO.TextReader r):this(r, typeof(DefaultTemplateLexer), DEFAULT_ERROR_LISTENER)
		{
		}
		
		public StringTemplateGroup(System.IO.TextReader r, StringTemplateErrorListener errors):this(r, typeof(DefaultTemplateLexer), errors)
		{
		}
		
		public StringTemplateGroup(System.IO.TextReader r, System.Type lexer):this(r, lexer, null)
		{
		}
		
		/// <summary>Create a group from the input stream, but use a nondefault lexer
		/// to break the templates up into chunks.  This is usefor changing
		/// the delimiter from the default $...$ to <...>, for example.
		/// </summary>
		public StringTemplateGroup(System.IO.TextReader r, System.Type lexer, StringTemplateErrorListener errors)
		{
			this.templatesDefinedInGroupFile = true;
			this.templateLexerClass = lexer;
			this.listener = errors;
			parseGroup(r);
		}
		
		public virtual System.Type getTemplateLexerClass()
		{
			return templateLexerClass;
		}
		
		public virtual String getName()
		{
			return name;
		}
		
		public virtual void setName(String name)
		{
			this.name = name;
		}
		
		public virtual void setSuperGroup(StringTemplateGroup superGroup)
		{
			this.superGroup = superGroup;
		}
		
		public virtual void setSuperGroup(String groupName)
		{
			this.superGroup = (StringTemplateGroup) nameToGroupMap[groupName];
		}
		
		public virtual StringTemplateGroup getSuperGroup()
		{
			return superGroup;
		}
		
		public virtual String getRootDir()
		{
			return rootDir;
		}
		
		public virtual void setRootDir(String rootDir)
		{
			this.rootDir = rootDir;
		}
		
		/// <summary>StringTemplate object factory; each group can have its own. </summary>
		public virtual StringTemplate createStringTemplate()
		{
			return new StringTemplate();
		}
		
		public virtual StringTemplate getInstanceOf(String name)
		{
			StringTemplate st = lookupTemplate(name);
			return st.getInstanceOf();
		}
		
		public virtual StringTemplate getEmbeddedInstanceOf(StringTemplate enclosingInstance, String name)
		{
			StringTemplate st = getInstanceOf(name);
			st.setEnclosingInstance(enclosingInstance);
			return st;
		}
		
		/// <summary>Get the template called 'name' from the group.  If not found,
		/// attempt to load.  If not found on disk, then try the superGroup
		/// if any.  If not even there, then record that it's
		/// NOT_FOUND so we don't waste time looking again later.  If we've gone
		/// past refresh interval, flush and look again.
		/// </summary>
		public virtual StringTemplate lookupTemplate(String name)
		{
			if (name.StartsWith("super."))
			{
				if (superGroup != null)
				{
					int dot = name.IndexOf('.');
					name = name.Substring(dot + 1, (name.Length) - (dot + 1));
					return superGroup.lookupTemplate(name);
				}
				throw new System.ArgumentException(getName() + " has no super group; invalid template: " + name);
			}
			checkRefreshInterval();
			StringTemplate st = (StringTemplate) templates[name];
			if (st == null)
			{
				// not there?  Attempt to load
				if (!templatesDefinedInGroupFile)
				{
					// only check the disk for individual template
					st = loadTemplateFromBeneathRootDirOrCLASSPATH(getFileNameFromTemplateName(name));
				}
				if (st == null && superGroup != null)
				{
					// try to resolve in super group
					st = superGroup.getInstanceOf(name);
					if (st != null)
					{
						st.setGroup(this);
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
					throw new System.ArgumentException("Can't load template " + getFileNameFromTemplateName(name));
				}
			}
			else if (st == NOT_FOUND_ST)
			{
				throw new System.ArgumentException("Can't load template " + getFileNameFromTemplateName(name));
			}
			
			return st;
		}
		
		private void checkRefreshInterval() {
			if (templatesDefinedInGroupFile) {
				return;
			}

			bool timeToFlush = (refreshIntervalInSeconds == 0 || 
				(DateTime.Now - lastCheckedDisk).TotalSeconds >= refreshIntervalInSeconds);

			if (timeToFlush) {
				// throw away all pre-compiled references
				templates.Clear();
				lastCheckedDisk = DateTime.Now;
			}
		}

		protected internal virtual StringTemplate loadTemplate(String name, System.IO.TextReader r)
		{
			String line;
			String nl = System.Environment.NewLine;
			System.Text.StringBuilder buf = new System.Text.StringBuilder(300);
			while ((line = r.ReadLine()) != null)
			{
				buf.Append(line);
				buf.Append(nl);
			}
			// strip newlines etc.. from front/back since filesystem
			// may add newlines etc...
			String pattern = buf.ToString().Trim();
			if (pattern.Length == 0)
			{
				error("no text in template '" + name + "'");
				return null;
			}
			return defineTemplate(name, pattern);
		}
		
		/// <summary>Load a template whose name is derived from the template filename.
		/// If there is no root directory, try to load the template from
		/// the classpath.  If there is a rootDir, try to load the file
		/// from there.
		/// </summary>
		protected internal virtual StringTemplate loadTemplateFromBeneathRootDirOrCLASSPATH(String fileName)
		{
			StringTemplate template = null;
			String name = getTemplateNameFromFileName(fileName);
			// if no rootDir, try to load as a resource in CLASSPATH
			if (rootDir == null) {
				// load via rootDir
				template = loadTemplate(name, System.IO.Path.Combine(
					AppDomain.CurrentDomain.BaseDirectory, fileName));
			}
			else 
			{
				// load via rootDir
				template = loadTemplate(name, rootDir + "/" + fileName);
			}
			return template;
		}
		
		/// <summary>(public so that people can override behavior; not a general
		/// purpose method)
		/// </summary>
		public virtual String getFileNameFromTemplateName(String templateName)
		{
			return templateName + ".st";
		}
		
		/// <summary>Convert a filename relativePath/name.st to relativePath/name.
		/// (public so that people can override behavior; not a general
		/// purpose method)
		/// </summary>
		public virtual String getTemplateNameFromFileName(String fileName)
		{
			String name = fileName;
			int suffix = name.LastIndexOf(".st");
			if (suffix >= 0)
			{
				name = name.Substring(0, (suffix) - (0));
			}
			return name;
		}
		
		protected internal virtual StringTemplate loadTemplate(String name, String fileName)
		{
			System.IO.StreamReader br = null;
			StringTemplate template = null;
			try
			{
				System.IO.Stream fin = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				br = getInputStreamReader(fin);
				template = loadTemplate(name, br);
				br.Close();
				br = null;
			}
			catch (System.IO.IOException)
			{
				if (br != null)
				{
					try
					{
						br.Close();
					}
					catch (System.IO.IOException)
					{
						error("Cannot close template file: " + fileName);
					}
				}
			}
			return template;
		}
		
		protected internal virtual System.IO.StreamReader getInputStreamReader(System.IO.Stream instr)
		{
			System.IO.StreamReader isr = null;
			try
			{
				isr = new System.IO.StreamReader(instr, System.Text.Encoding.GetEncoding(fileCharEncoding));
			}
			catch (System.IO.IOException)
			{
				error("Invalid file character encoding: " + fileCharEncoding);
			}
			return isr;
		}
		
		public virtual String getFileCharEncoding()
		{
			return fileCharEncoding;
		}
		
		public virtual void setFileCharEncoding(String fileCharEncoding)
		{
			this.fileCharEncoding = fileCharEncoding;
		}
		
		/// <summary>Define an examplar template; precompiled and stored
		/// with no attributes.  Remove any previous definition.
		/// </summary>
		public virtual StringTemplate defineTemplate(String name, String template)
		{
			StringTemplate st = createStringTemplate();
			st.setName(name);
			st.setGroup(this);
			st.setTemplate(template);
			st.setErrorListener(listener);
			templates[name] = st;
			return st;
		}
		
		/// <summary>Make name and alias for target.  Replace any previous def of name </summary>
		public virtual StringTemplate defineTemplateAlias(String name, String target)
		{
			StringTemplate targetST = getTemplateDefinition(target);
			if (targetST == null)
			{
				error("cannot alias " + name + " to undefined template: " + target);
				return null;
			}
			templates[name] = targetST;
			return targetST;
		}
		
		public virtual bool isDefinedInThisGroup(String name)
		{
			return templates[name] != null;
		}
		
		/// <summary>Get the ST for 'name' in this group only </summary>
		public virtual StringTemplate getTemplateDefinition(String name)
		{
			return (StringTemplate) templates[name];
		}
		
		/// <summary>Is there *any* definition for template 'name' in this template
		/// or above it in the group hierarchy?
		/// </summary>
		public virtual bool isDefined(String name)
		{
			try
			{
				lookupTemplate(name);
				return true;
			}
			catch (System.ArgumentException)
			{
				return false;
			}
		}
		
		protected internal virtual void parseGroup(System.IO.TextReader r)
		{
			try
			{
				GroupLexer lexer = new GroupLexer(r);
				GroupParser parser = new GroupParser(lexer);
				parser.group(this);
				//System.out.println("read group\n"+this.toString());
			}
			catch (System.Exception e)
			{
				String name = "<unknown>";
				if (getName() != null)
				{
					name = getName();
				}
				error("problem parsing group '" + name + "'", e);
			}
		}
		
		public virtual int getRefreshInterval()
		{
			return refreshIntervalInSeconds;
		}
		
		/// <summary>How often to refresh all templates from disk.  This is a crude
		/// mechanism at the moment--just tosses everything out at this
		/// frequency.  Set interval to 0 to refresh constantly (no caching).
		/// Set interval to a huge number like MAX_INT to have no refreshing
		/// at all (DEFAULT); it will cache stuff.
		/// </summary>
		public virtual void setRefreshInterval(int refreshInterval)
		{
			this.refreshIntervalInSeconds = refreshInterval;
		}
		
		public virtual void setErrorListener(StringTemplateErrorListener listener)
		{
			this.listener = listener;
		}
		
		public virtual StringTemplateErrorListener getErrorListener()
		{
			return listener;
		}
		
		/// <summary>Specify a StringTemplateWriter implementing class to use for
		/// filtering output
		/// </summary>
		public virtual void setStringTemplateWriter(System.Type c)
		{
			userSpecifiedWriter = c;
		}
		
		/// <summary>return an instance of a StringTemplateWriter that spits output to w.
		/// If a writer is specified, use it instead of the default.
		/// </summary>
		public virtual StringTemplateWriter getStringTemplateWriter(System.IO.TextWriter w)
		{
			StringTemplateWriter stw = null;
			if (userSpecifiedWriter != null)
			{
				try
				{
					System.Reflection.ConstructorInfo ctor = userSpecifiedWriter.GetConstructor(new System.Type[]{typeof(System.IO.TextWriter)});
					stw = (StringTemplateWriter) ctor.Invoke(new Object[]{w});
				}
				catch (System.Exception e)
				{
					error("problems getting StringTemplateWriter", e);
				}
			}
			if (stw == null)
			{
				stw = new AutoIndentWriter(w);
			}
			return stw;
		}

		/// <summary>
		/// Specify a complete map of what object classes should map to which
		/// renderer objects for every template in this group (that doesn't
		/// override it per template).
		/// </summary>
		public void setAttributeRenderers(IDictionary renderers) 
		{
			this.attributeRenderers = renderers;
		}

		/// <summary>
		/// Register a renderer for all objects of a particular type for all
		/// templates in this group.
		/// </summary>
		public void registerRenderer(Type attributeClassType, Object renderer) 
		{
			if ( attributeRenderers==null ) 
			{
				attributeRenderers = new Hashtable();
			}
			attributeRenderers[attributeClassType] = renderer;
		}

		/// <summary>
		/// What renderer is registered for this attributeClassType for
		/// this group?  If not found, as superGroup if it has one.
		/// </summary>
		public AttributeRenderer getAttributeRenderer(Type attributeClassType) 
		{
			if ( attributeRenderers==null ) 
			{
				if ( superGroup==null ) 
				{
					return null; // no renderers and no parent?  Stop.
				}
				// no renderers; consult super group
				return superGroup.getAttributeRenderer(attributeClassType);
			}

			AttributeRenderer renderer = (AttributeRenderer)attributeRenderers[attributeClassType];
			if ( renderer==null ) 
			{
				if ( superGroup==null ) 
				{
					return null; // no renderers and no parent?  Stop.
				}
				// no renderer registered for this class, check super group
				renderer = superGroup.getAttributeRenderer(attributeClassType);
			}
			return renderer;
		}

		public IDictionary getMap(String name) 
		{
			if ( maps==null ) 
			{
				if ( superGroup==null ) 
				{
					return null;
				}
				return superGroup.getMap(name);
			}
			IDictionary m = (IDictionary)maps[name];
			if ( m==null && superGroup!=null ) 
			{
				m = superGroup.getMap(name);
			}
			return m;
		}

		public void defineMap(String name, IDictionary mapping) 
		{
			maps[name] = mapping;
		}
		
		public virtual void error(String msg)
		{
			error(msg, null);
		}
		
		public virtual void error(String msg, System.Exception e)
		{
			if (listener != null)
			{
				listener.error(msg, e);
			}
			else
			{
				System.Console.Error.WriteLine("StringTemplate: " + msg + ": " + e);
			}
		}
		
		public override String ToString()
		{
			System.Text.StringBuilder buf = new System.Text.StringBuilder();
			IEnumerator iter = templates.Keys.GetEnumerator();
			buf.Append("group " + getName() + ";\n");
			StringTemplate formalArgs = new StringTemplate("$args;separator=\",\"$");

			while (iter.MoveNext())
			{
				String tname = (String) iter.Current;
				StringTemplate st = (StringTemplate) templates[tname];
				if (st != NOT_FOUND_ST)
				{
					formalArgs = formalArgs.getInstanceOf();
					formalArgs.setAttribute("args", st.getFormalArguments());
					buf.Append(tname + "(" + formalArgs + ") ::= ");
					buf.Append("<<");
					buf.Append(st.getTemplate());
					buf.Append(">>\n");
				}
			}
			return buf.ToString();
		}
		static StringTemplateGroup()
		{
			DEFAULT_ERROR_LISTENER = new AnonymousClassStringTemplateErrorListener();
		}

		public IAttributeStrategy AttributeStrategy
		{
			get
			{
				return atttributeStrategy;
			}

			set
			{
				atttributeStrategy = value;
			}
		}
	}
}