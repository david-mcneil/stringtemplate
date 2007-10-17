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
	using ArrayList				= System.Collections.ArrayList;
	
	/// <summary>
	/// A group loader that loads StringTemplate groups and interfaces from 
	/// a set of loaders. The loader only uses the loaders specified in it's 
	/// constructor.
	/// </summary>
	public class CompositeGroupLoader : IStringTemplateGroupLoader
	{
		protected ArrayList loaders;

		protected CompositeGroupLoader()
		{
		}

		/// <summary>
		/// Construct an instance that loads groups/interfaces from a set
		/// of <see cref="IStringTemplateGroupLoader"/>s.
		/// </summary>
		public CompositeGroupLoader(params IStringTemplateGroupLoader[] loaders)
		{
			if ((loaders == null) || (loaders.Length < 1))
				throw new ArgumentNullException("loaders", "At least one IStringTemplateGroupLoader must be specified");

			this.loaders = new ArrayList(loaders);
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
			foreach (IStringTemplateGroupLoader loader in loaders)
			{
				StringTemplateGroup group = loader.LoadGroup(groupName, superGroup, lexer);
				if (group != null)
					return group;
			}
			return null;
		}

		public StringTemplateGroupInterface LoadInterface(string interfaceName) 
		{
			foreach(IStringTemplateGroupLoader loader in loaders)
			{
				StringTemplateGroupInterface groupInterface = loader.LoadInterface(interfaceName);
				if (groupInterface != null)
					return groupInterface;
			}
			return null;
		}
	}
}