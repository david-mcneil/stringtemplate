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
	using TargetInvocationException = System.Reflection.TargetInvocationException;
	
	/// <summary>
	/// A Listener that sends it's output to the console.
	/// </summary>
	public class ConsoleErrorListener : IStringTemplateErrorListener
	{
		public static IStringTemplateErrorListener DefaultConsoleListener = new ConsoleErrorListener();

		/// <summary>
		/// Prints an error message to the standard error console.
		/// </summary>
		public virtual void  Error(string s, Exception e)
		{
			Console.Error.WriteLine(s);
			if (e != null)
			{
				if (e is TargetInvocationException) {
					e = ((TargetInvocationException)e).InnerException;
				}
				Console.Error.WriteLine(e.StackTrace);
			}
		}

		/// <summary>
		/// Prints a warning message to the standard output console.
		/// </summary>
		public virtual void  Warning(string s)
		{
			Console.Out.WriteLine(s);
		}
	}
}