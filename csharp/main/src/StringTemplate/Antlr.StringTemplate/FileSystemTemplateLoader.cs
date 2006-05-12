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
	using IOException				= System.IO.IOException;
	using Path						= System.IO.Path;
	using StreamReader				= System.IO.StreamReader;
	using FileSystemWatcher			= System.IO.FileSystemWatcher;
	using FileSystemEventHandler	= System.IO.FileSystemEventHandler;
	using FileSystemEventArgs		= System.IO.FileSystemEventArgs;
	using RenamedEventHandler		= System.IO.RenamedEventHandler;
	using RenamedEventArgs			= System.IO.RenamedEventArgs;
	using NotifyFilters				= System.IO.NotifyFilters;
	using HybridDictionary			= System.Collections.Specialized.HybridDictionary;
	using Encoding					= System.Text.Encoding;

	/// <summary>
	/// A StringTemplateLoader that can load StringTemplates from the file system.
	/// </summary>
	public class FileSystemTemplateLoader : StringTemplateLoader
	{
		/// <summary>
		/// How are the files encoded (ascii, UTF8, ...)?  You might want to read
		/// UTF8 for example on a ascii machine.
		/// </summary>
		private Encoding encoding;

		private FileSystemWatcher filesWatcher;
		private HybridDictionary fileSet;
		private object sentinel = new object();

		public FileSystemTemplateLoader() : this(null, Encoding.Default)
		{
		}

		public FileSystemTemplateLoader(string locationRoot)
			: this(locationRoot, Encoding.Default)
		{
		}

		public FileSystemTemplateLoader(string locationRoot, Encoding encoding)
			: base(locationRoot)
		{
			if (locationRoot == null)
			{
				this.locationRoot = AppDomain.CurrentDomain.BaseDirectory;
			}
			this.encoding = encoding;
			fileSet = new HybridDictionary();
		}

		/// <summary>
		/// Determines if the specified template has changed.
		/// </summary>
		/// <param name="templateName">template name</param>
		/// <returns>True if the named template has changed</returns>
		public override bool HasChanged(string templateName)
		{
			string templateLocation = Path.Combine(LocationRoot, GetLocationFromTemplateName(templateName));
			object o = fileSet[templateLocation];
			if ((o != null) && (o != sentinel))
			{
				fileSet.Remove(templateLocation);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Loads the contents of the named StringTemplate.
		/// </summary>
		/// <param name="templateName">Name of the StringTemplate to load</param>
		/// <returns>
		/// The contexts of the named StringTemplate or null if the template wasn't found
		/// </returns>
		/// <exception cref="TemplateLoadException">Thrown if error prevents successful template load</exception>
		protected override string InternalLoadTemplateContents(string templateName)
		{
			string templateText = null;
			string templateLocation = null;

			try
			{
				templateLocation = Path.Combine(LocationRoot, GetLocationFromTemplateName(templateName));

				using (StreamReader br = new StreamReader(templateLocation, encoding))
				{
					templateText = br.ReadToEnd();
					if ((templateText != null) && (templateText.Length > 0))
					{
						templateText = templateText.Trim();

						if (filesWatcher == null)
						{
							filesWatcher = new FileSystemWatcher(LocationRoot, "*.st");
							filesWatcher.NotifyFilter = 
								NotifyFilters.LastWrite 
								| NotifyFilters.Size 
								| NotifyFilters.CreationTime 
								| NotifyFilters.DirectoryName 
								| NotifyFilters.FileName;
							filesWatcher.IncludeSubdirectories = true;
							filesWatcher.Changed += new FileSystemEventHandler(OnChanged);
							filesWatcher.Deleted += new FileSystemEventHandler(OnChanged);
							filesWatcher.Created += new FileSystemEventHandler(OnChanged);
							filesWatcher.Renamed += new RenamedEventHandler(OnRenamed);
							filesWatcher.EnableRaisingEvents = true;
						}
						fileSet[templateLocation] = sentinel;
					}
				}
			}
			catch (ArgumentException ex) 
			{
				string message;
				if (templateText == null)
					message = string.Format("Invalid file character encoding: {0}", encoding);
				else
					message = string.Format("The location root '{0}' and/or the template name '{1}' is invalid.", LocationRoot, templateName);

				throw new TemplateLoadException(message, ex);
			}
			catch (IOException ex) 
			{
				throw new TemplateLoadException("Cannot close template file: " + templateLocation, ex);
			}
			return templateText;
		}

		/// <summary>
		/// Returns the location that corresponds to the specified template name.
		/// </summary>
		/// <param name="templateName">template name</param>
		/// <returns>The corresponding template location or null</returns>
		public override string GetLocationFromTemplateName(string templateName)
		{
			return templateName + ".st";
		}

		/// <summary>
		/// Returns the template name that corresponds to the specified location.
		/// </summary>
		/// <param name="templateName">template location</param>
		/// <returns>The corresponding template name or null</returns>
		public override string GetTemplateNameFromLocation(string location)
		{
			//return Path.ChangeExtension(location, string.Empty);
			return Path.ChangeExtension(location, null);
		}

		#region FileSystemWatcher Events

		private void OnChanged(object source, FileSystemEventArgs e)
		{
			string fullpath = e.FullPath;
			if (fileSet.Contains(fullpath))
			{
				fileSet[fullpath] = fullpath;
			}
		}

		private void OnRenamed(object source, RenamedEventArgs e)
		{
			string fullpath = e.FullPath;
			if (fileSet.Contains(fullpath))
			{
				fileSet[fullpath] = fullpath;
			}

			fullpath = e.OldFullPath;
			if (fileSet.Contains(fullpath))
			{
				fileSet[fullpath] = fullpath;
			}
		}

		#endregion
	}
}