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
	using TextReader		= System.IO.TextReader;

	/// <summary>
	/// Abstracts the creation of concrete <see cref="StringTemplateGroup"/>
	/// and <see cref="StringTemplateGroupInterface"/> instances.
	/// </summary>
	public interface IStringTemplateGroupFactory 
	{
		/// <summary>
		/// Creates a StringTemplateGroup instance that manages a set of
		/// templates defined in a "group file".
		/// </summary>
		/// <param name="reader">Input stream for group file data</param>
		/// <param name="lexer">Lexer to use for breaking up templates into chunks</param>
		/// <param name="errorListener">Error message sink</param>
		/// <param name="superGroup">Parent (or super/base) group</param>
		/// <returns>A StringTemplateGroup instance or null if no group is found</returns>
		StringTemplateGroup CreateGroup(
			TextReader reader, 
			Type lexer, 
			IStringTemplateErrorListener errorListener,
			StringTemplateGroup superGroup);

		/// <summary>
		/// Creates a StringTemplateGroup instance that manages a set of 
		/// templates that are accessible via a specified 
		/// <seealso cref="StringTemplateLoader"/>.
		/// </summary>
		/// <param name="name">Input stream for group file data</param>
		/// <param name="templateLoader">Loader for retrieving this group's templates</param>
		/// <param name="lexer">Lexer to use for breaking up templates into chunks</param>
		/// <param name="errorListener">Error message sink</param>
		/// <param name="superGroup">Parent (or super/base) group</param>
		/// <returns>A StringTemplateGroup instance or null</returns>
		StringTemplateGroup CreateGroup(
			string name, 
			StringTemplateLoader templateLoader, 
			Type lexer, 
			IStringTemplateErrorListener errorListener,
			StringTemplateGroup superGroup);

		/// <summary>
		/// Creates a StringTemplateGroupInterface instance that manages a set of
		/// template interface definitions defined in a "group interface file".
		/// </summary>
		/// <param name="reader">Input stream for group-interface file data</param>
		/// <param name="errorListener">Error message sink</param>
		/// <param name="superGroup">Parent (or super/base) group interface</param>
		/// <returns>A StringTemplateGroupInterface instance or null</returns>
		StringTemplateGroupInterface CreateInterface(
			TextReader reader, 
			IStringTemplateErrorListener errorListener,
			StringTemplateGroupInterface superInterface);

	}
}