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

	/// <summary>
	/// When group files derive from another group, we have to know how to
	/// load that group, its supergroups and any group interfaces.
	/// </summary>
	public interface IStringTemplateGroupLoader 
	{
		/// <summary>
		/// Loads the named StringTemplateGroup instance from somewhere.
		/// </summary>
		/// <param name="groupName">Name of the StringTemplateGroup to load</param>
		/// <returns>A StringTemplateGroup instance or null if no group is found</returns>
		StringTemplateGroup LoadGroup(string groupName);

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
		StringTemplateGroup LoadGroup(string groupName, StringTemplateGroup superGroup);

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
		StringTemplateGroup LoadGroup(string groupName, StringTemplateGroup superGroup, Type lexer);

		/// <summary>
		/// Loads the named StringTemplateGroupInterface instance from somewhere.
		/// </summary>
		/// <param name="groupName">Name of the StringTemplateGroupInterface to load</param>
		/// <returns>A StringTemplateGroupInterface instance or null if no group is found</returns>
		StringTemplateGroupInterface LoadInterface(string interfaceName);
	}
}