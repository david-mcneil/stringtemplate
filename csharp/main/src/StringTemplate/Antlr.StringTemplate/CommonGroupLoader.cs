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
	using TextReader			= System.IO.TextReader;
	using StreamReader			= System.IO.StreamReader;
	using File					= System.IO.File;
	using IOException			= System.IO.IOException;
	
	/// <summary>
	/// A brain dead group loader that looks only in the directory(ies) specified 
	/// in it's constructor.
	/// </summary>
	public class CommonGroupLoader : IStringTemplateGroupLoader
	{
		protected ArrayList directories = null;
		protected IStringTemplateErrorListener errorListener = null;

		public CommonGroupLoader(IStringTemplateErrorListener errorListener) 
		{
			this.errorListener = errorListener;
		}

		/// <summary>
		/// Construct an instance that loads groups/interfaces from the single dir or 
		/// multiple dirs specified.
		/// </summary>
		public CommonGroupLoader(IStringTemplateErrorListener errorListener, params string[] directoryNames) 
		{
			this.errorListener = errorListener;
			foreach (string directoryName in directoryNames)
			{
				if ( File.Exists(directoryName) )
				{
					Error("group loader: no such dir "+directoryName);
				}
				else 
				{
					if (directories == null)
					{
						directories = new ArrayList();
					}
					directories.Add(directoryName);
				}
			}
		}

		public StringTemplateGroup LoadGroup(string groupName) 
		{
			StringTemplateGroup group = null;
			string fileName = LocateFile(groupName + ".stg");
			if ( fileName==null ) 
			{
				Error("no such group file '" +groupName+ ".stg'");
			}
			else
			{

				try 
				{
					group = new StringTemplateGroup(new StreamReader(fileName), errorListener);
				}
				catch (ArgumentException argx) 
				{
					Error("Path Error: can't load group '" +groupName+ "'", argx);
				}
				catch (IOException iox) 
				{
					Error("IO Error: can't load group '" +groupName+ "'", iox);
				}
			}
			return group;
		}

		public StringTemplateGroupInterface LoadInterface(string interfaceName) 
		{
			StringTemplateGroupInterface groupInterface = null;
			string fileName = LocateFile(interfaceName + ".sti");
			if ( fileName==null ) 
			{
				Error("no such interface file '" +interfaceName+ ".sti'");
			}
			else
			{

				try 
				{
					groupInterface = new StringTemplateGroupInterface(new StreamReader(fileName), errorListener);
				}
				catch (ArgumentException argx) 
				{
					Error("Path Error: can't load interface '" +interfaceName+ "'", argx);
				}
				catch (IOException iox) 
				{
					Error("IO Error: can't load interface '" +interfaceName+ "'", iox);
				}
			}
			return groupInterface;
		}

		/// <summary>
		/// Look in each of this loader's source directories for a file with 
		/// the specified name.
		/// </summary>
		/// <param name="filename">Name of file to locate (including extension)</param>
		/// <returns>Full name of file if found otherwise null.</returns>
		protected string LocateFile(string filename) 
		{
			for (int i = 0; i < directories.Count; i++) 
			{
				string directoryName = (string) directories[i];
				string fullname = System.IO.Path.Combine(directoryName, filename);
				if ( File.Exists(fullname) ) 
				{
					return fullname;
				}
			}
			return null;
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