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
	using IList					= System.Collections.IList;
	using IDictionary			= System.Collections.IDictionary;
	using ArrayList				= System.Collections.ArrayList;
	using TextReader			= System.IO.TextReader;
	using StringBuilder			= System.Text.StringBuilder;
	using InterfaceLexer		= Antlr.StringTemplate.Language.InterfaceLexer;
	using InterfaceParser		= Antlr.StringTemplate.Language.InterfaceParser;
	using HashList				= Antlr.StringTemplate.Collections.HashList;
	using CollectionUtils		= Antlr.StringTemplate.Collections.CollectionUtils;
	
	/// <summary>
	/// A group interface is like a group without the template implementations.
	/// All there is are template-names and argument-lists like these:
	/// 
	///   interface foo;
	///   class(name,fields);
	///   method(name,args,body);
	/// </summary>
	public class StringTemplateGroupInterface
	{
		public static IStringTemplateErrorListener DEFAULT_ERROR_LISTENER = ConsoleErrorListener.DefaultConsoleListener;

		
		/// <summary>What is the group interface's name</summary>
		protected string name;
		public string Name
		{
			get { return name;  }
			set { name = value; }
		}


		/// <summary>Maps template name to TemplateDefinition object</summary>
		protected IDictionary templates = new HashList();

		/// <summary>
		/// Are we derived from another group interface?  Templates not found in this 
		/// group interface will be sought in the superInterface recursively.
		/// </summary>
		protected StringTemplateGroupInterface superInterface = null;

		/// <summary>
		/// Where to report errors.  All string templates in this group interface
		/// use this error handler by default.
		///</summary>
		protected IStringTemplateErrorListener errorListener = DEFAULT_ERROR_LISTENER;
		
		virtual public IStringTemplateErrorListener ErrorListener
		{
			get { return errorListener;  }
			set { errorListener = value; }
		}

		/// <summary>
		/// All the info we need to track for a template defined in an interface
		/// </summary>
		protected sealed class TemplateDefinition 
		{
			public string name;
			public HashList formalArgs; // LinkedHashMap<FormalArgument>
			public bool optional = false;

			public TemplateDefinition(string name, HashList formalArgs, bool optional) 
			{
				this.name = name;
				this.formalArgs = formalArgs;
				this.optional = optional;
			}
		}

		public StringTemplateGroupInterface(TextReader r)
			: this(r, DEFAULT_ERROR_LISTENER, (StringTemplateGroupInterface)null)
		{
		}

		public StringTemplateGroupInterface(TextReader r, IStringTemplateErrorListener errorListener) 
			: this(r, errorListener,(StringTemplateGroupInterface)null)
		{
		}

		/// <summary>
		/// Create an interface from the input stream
		/// </summary>
		public StringTemplateGroupInterface(
			TextReader r, 
			IStringTemplateErrorListener errorListener,
			StringTemplateGroupInterface superInterface)
		{
			ErrorListener = errorListener;
			SuperInterface = superInterface;
			ParseInterface(r);
		}

		public StringTemplateGroupInterface SuperInterface
		{
			get { return superInterface;  }
			set { superInterface = value; }
		}

		protected void ParseInterface(TextReader r) 
		{
			try 
			{
				InterfaceLexer lexer = new InterfaceLexer(r);
				InterfaceParser parser = new InterfaceParser(lexer);
				parser.groupInterface(this);
				//System.out.println("read interface\n"+this.toString());
			}
			catch (Exception e) 
			{
				string name = (Name == null) ? "<unknown>" : Name;
				Error("problem parsing group "+name+": "+e, e);
			}
		}

		public void DefineTemplate(string name, HashList formalArgs, bool optional) 
		{
			TemplateDefinition tdef = new TemplateDefinition(name, formalArgs, optional);
			templates.Add(tdef.name, tdef);
		}

		/// <summary>
		/// Return a list of all template names missing from group that are defined
		/// in this interface. Return null if all is well.
		/// </summary>
		/// <param name="group">Group to check</param>
		/// <returns>List of templates missing from specified group or null</returns>
		public IList GetMissingTemplates(StringTemplateGroup group) 
		{
			ArrayList missing = null;
			foreach (string name in templates.Keys) 
			{
				TemplateDefinition tdef = (TemplateDefinition) templates[name];
				if ( !tdef.optional && !group.IsDefined(tdef.name) )
				{
					if (missing == null)
					{
						missing = new ArrayList();
					}
					missing.Add(tdef.name);
				}
			}
			return missing;
		}

		/// <summary>
		/// Return a list of all template sigs that are present in the group, but
		/// that have wrong formal argument lists.  Return null if all is well.
		/// </summary>
		/// <param name="group">Group to check</param>
		/// <returns>
		/// List of templates with mismatched signatures from specified group or null
		/// </returns>
		public IList GetMismatchedTemplates(StringTemplateGroup group) 
		{
			ArrayList mismatched = null;
			foreach (string name in templates.Keys) 
			{
				TemplateDefinition tdef = (TemplateDefinition) templates[name];
				if ( group.IsDefined(tdef.name) ) 
				{
					StringTemplate defST = group.GetTemplateDefinition(tdef.name);
					IDictionary formalArgs = defST.FormalArguments;
					bool ack = false;
					if ( (tdef.formalArgs!=null && formalArgs==null) ||
						(tdef.formalArgs==null && formalArgs!=null) ||
						(tdef.formalArgs.Count != formalArgs.Count) )
					{
						ack = true;
					}
					if ( !ack ) 
					{
						foreach (string argName in formalArgs.Keys)
						{
							if ( tdef.formalArgs[argName] == null ) 
							{
								ack=true;
								break;
							}
						}
					}
					if ( ack ) 
					{
						// Console.Out.WriteLine(CollectionUtils.DictionaryToString(tdef.formalArgs) 
						//	+ "!=" + CollectionUtils.DictionaryToString(formalArgs));
						if (mismatched == null)
						{
							mismatched = new ArrayList();
						}
						mismatched.Add(GetTemplateSignature(tdef));
					}
				}
			}
			return mismatched;
		}

		public void Error(string msg) 
		{
			ErrorListener.Error(msg, null);
		}

		public void Error(string msg, Exception e) 
		{
			ErrorListener.Error(msg, e);
		}

		public override string ToString() 
		{
			StringBuilder buf = new StringBuilder();
			buf.Append("interface ");
			buf.Append(Name);
			buf.Append(";\n");
			foreach (string name in templates.Keys) 
			{
				TemplateDefinition tdef = (TemplateDefinition) templates[name];
				buf.Append( GetTemplateSignature(tdef) );
				buf.Append(";\n");
			}
			return buf.ToString();
		}

		protected string GetTemplateSignature(TemplateDefinition tdef) 
		{
			StringBuilder buf = new StringBuilder();
			if ( tdef.optional ) 
			{
				buf.Append("optional ");
			}
			buf.Append(tdef.name);
			if ( tdef.formalArgs != null ) 
			{
				buf.Append('(');
				bool needSeparator = false;
				foreach (string name in tdef.formalArgs.Keys) 
				{
					if ( needSeparator ) 
					{
						buf.Append(", ");
					}
					buf.Append(name);
					needSeparator = true;
				}
				buf.Append(')');
			}
			else 
			{
				buf.Append("()");
			}
			return buf.ToString();
		}
	}
}