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
	/// Encapsulates the logic involved in loading a StringTemplate.
	/// </summary>
	public abstract class StringTemplateLoader
	{
		protected string locationRoot;
		protected bool raiseExceptionForEmptyTemplate;

		protected StringTemplateLoader()
		{
		}

		public StringTemplateLoader(string locationRoot) : this(locationRoot, true)
		{
		}

		protected StringTemplateLoader(string locationRoot, bool raiseExceptionForEmptyTemplate)
		{
			this.locationRoot = locationRoot;
			this.raiseExceptionForEmptyTemplate = raiseExceptionForEmptyTemplate;
		}

		/// <summary>
		/// Returns the location root for this StringTemplateLoader.
		/// </summary>
		public string LocationRoot
		{
			get { return locationRoot; }
		}

		/// <summary>
		/// Loads and returns the contents of the named StringTemplate.
		/// </summary>
		/// <param name="templateName">Name of the StringTemplate to load</param>
		/// <returns>
		/// A string containing the StringTemplate's contents
		/// </returns>
		public string LoadTemplate(string templateName)
		{
			string templateText = InternalLoadTemplateContents(templateName);

			if ((templateText != null) && (templateText.Length > 0))
				return templateText;

			if (raiseExceptionForEmptyTemplate)
				throw new TemplateLoadException("no text in template '" + templateName + "'");

			return null;
		}

		/// <summary>
		/// Determines if the specified template has changed.
		/// </summary>
		/// <param name="templateName">template name</param>
		/// <returns>True if the named template has changed</returns>
		public abstract bool HasChanged(string templateName);

		/// <summary>
		/// Returns the location that corresponds to the specified template name.
		/// </summary>
		/// <param name="templateName">template name</param>
		/// <returns>The corresponding template location or null</returns>
		public abstract string GetLocationFromTemplateName(string templateName);

		/// <summary>
		/// Returns the template name that corresponds to the specified location.
		/// </summary>
		/// <param name="templateName">template location</param>
		/// <returns>The corresponding template name or null</returns>
		public abstract string GetTemplateNameFromLocation(string location);

		/// <summary>
		/// Loads the contents of the named StringTemplate.
		/// </summary>
		/// <param name="templateName">Name of the StringTemplate to load</param>
		/// <returns>
		/// The contexts of the named StringTemplate or null if the template wasn't found
		/// </returns>
		/// <exception cref="TemplateLoadException">Thrown if error prevents successful template load</exception>
		protected abstract string InternalLoadTemplateContents(string templateName);
	}
}