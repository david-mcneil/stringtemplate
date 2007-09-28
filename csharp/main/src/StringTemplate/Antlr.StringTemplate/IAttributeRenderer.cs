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
	
	/// <summary>
	/// This interface describes an object that knows how to format or otherwise
	/// render an object appropriately.  Usually this is used for locale changes
	/// for objects such as Date and floating point numbers...  You can either
	/// have an object that is sensitive to the locale or have a different object
	/// per locale.
	/// 
	/// Each template may have a renderer for each object type or can default
	/// to the group's renderer or the super group's renderer if the group doesn't
	/// have one.
	/// </summary>
	public interface IAttributeRenderer
	{
		string ToString(object o);

		/// <summary>
		/// When the user uses the format option (e.g. <c>$o; format="f"$</c>) it is 
		/// delegated to this method.
		/// </summary>
		/// <remarks>
		/// It should check the 'formatName' and apply the 
		/// appropriate formatting. If the format string passed to the renderer is 
		/// not recognized, it should default to simply calling ToString().
		/// @since 3.1
		/// </remarks>
		string ToString(object o, string formatName);
	}
}