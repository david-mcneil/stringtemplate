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
	using Assembly				= System.Reflection.Assembly;
	using Stream				= System.IO.Stream;
	using TextReader			= System.IO.TextReader;
	using StreamReader			= System.IO.StreamReader;
	using DefaultTemplateLexer	= Antlr.StringTemplate.Language.DefaultTemplateLexer;
	
	/// <summary>
	/// A group loader that loads StringTemplate groups and interfaces from 
	/// embedded resources in an assembly. The loader only searches in the 
	/// namespaceRoot and assembly specified in it's constructor.
	/// </summary>
	public class EmbeddedResourceGroupLoader : IStringTemplateGroupLoader
	{
		protected IStringTemplateGroupFactory factory;
		protected Assembly assembly;
		protected string namespaceRoot;
		protected IStringTemplateErrorListener errorListener;

		protected EmbeddedResourceGroupLoader()
		{
		}

		public EmbeddedResourceGroupLoader(IStringTemplateGroupFactory factory, Assembly assembly, string namespaceRoot)
			: this(factory, null, assembly, namespaceRoot)
		{
		}

		/// <summary>
		/// Construct an instance that loads groups/interfaces from the single dir or 
		/// multiple dirs specified and uses the specified encoding.
		/// </summary>
		/// <remarks>
		/// If a specified directory is an absolute path, the full path is used to 
		/// locate the template group or interface file.
		/// 
		/// If a specified directory is a relative path, the path is taken to be
		/// relative to the location of the stringtemplate assembly itself.
		/// 
		/// TODO: Check shadow-copying doesn't knock this outta whack.
		/// </remarks>
		public EmbeddedResourceGroupLoader(
			IStringTemplateGroupFactory factory,
			IStringTemplateErrorListener errorListener, 
			Assembly assembly,
			string namespaceRoot) 
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly", "An assembly must be specified");

			if (namespaceRoot == null)
				throw new ArgumentNullException("namespaceRoot", "A namespace must be specified");

			this.factory = (factory == null) ? new DefaultGroupFactory() : factory;
			this.errorListener = (errorListener == null) ? NullErrorListener.DefaultNullListener : errorListener;
			this.assembly = assembly;
			this.namespaceRoot = namespaceRoot;
		}

		/// <summary>
		/// Loads the named StringTemplateGroup instance from somewhere.
		/// </summary>
		/// <param name="groupName">Name of the StringTemplateGroup to load</param>
		/// <returns>A StringTemplateGroup instance or null if no group is found</returns>
		public StringTemplateGroup LoadGroup(string groupName) 
		{
			return LoadGroup(groupName, null, null);
		}

		/// <summary>
		/// Loads the named StringTemplateGroup instance with the specified super 
		/// group from somewhere. 
		/// </summary>
		/// <remarks>
		/// Groups with region definitions must know their supergroup to find 
		/// templates during parsing.
		/// </remarks>
		/// <param name="groupName">Name of the StringTemplateGroup to load</param>
		/// <param name="superGroup">Super group</param>
		/// <returns>A StringTemplateGroup instance or null if no group is found</returns>
		public StringTemplateGroup LoadGroup(string groupName, StringTemplateGroup superGroup)
		{
			return LoadGroup(groupName, superGroup, null);
		}

		/// <summary>
		/// Loads the named StringTemplateGroup instance with the specified super 
		/// group from somewhere. Configure to use specified lexer.
		/// </summary>
		/// <remarks>
		/// Groups with region definitions must know their supergroup to find 
		/// templates during parsing.
		/// </remarks>
		/// <param name="groupName">Name of the StringTemplateGroup to load</param>
		/// <param name="superGroup">Super group</param>
		/// <param name="lexer">Type of lexer to use to break up templates into chunks</param>
		/// <returns>A StringTemplateGroup instance or null if no group is found</returns>
		public StringTemplateGroup LoadGroup(string groupName, StringTemplateGroup superGroup, Type lexer)
		{
			StringTemplateGroup group = null;
			try
			{
				Stream groupStream = assembly.GetManifestResourceStream(namespaceRoot + "." + groupName + ".stg");
				group = factory.CreateGroup(new StreamReader(groupStream), lexer, errorListener, superGroup);
			}
			catch (Exception ex)
			{
				Error("Resource Error: can't load group '" + namespaceRoot + "." + groupName + ".stg' from assembly '" + assembly.FullName + "'", ex);
			}
			return group;
		}

		public StringTemplateGroupInterface LoadInterface(string interfaceName) 
		{
			StringTemplateGroupInterface groupInterface = null;
			try
			{
				Stream interfaceStream = assembly.GetManifestResourceStream(namespaceRoot + "." + interfaceName + ".sti");
				if (interfaceStream != null)
				{
					groupInterface = factory.CreateInterface(new StreamReader(interfaceStream), errorListener, null);
				}
			}
			catch(Exception ex)
			{
				Error("Resource Error: can't load interface '" +namespaceRoot+"."+interfaceName+ ".sti' from assembly '" +assembly.FullName+ "'", ex);
			}
			return groupInterface;
		}

		public void Error(String msg) 
		{
			errorListener.Error(msg, null);
		}

		public void Error(String msg, Exception e) 
		{
			errorListener.Error(msg, e);
		}
	}
}