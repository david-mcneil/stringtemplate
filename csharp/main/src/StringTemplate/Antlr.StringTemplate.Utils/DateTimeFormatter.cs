/*
[The "BSD licence"]
Copyright (c) 2006 Kunle Odutola
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


namespace Antlr.StringTemplate.Utils
{
	using System;

	/// <summary>
	/// Utility class useful for wrapping attribbutes of type DateTime to 
	/// support different date output formats.
	/// 
	/// e.g $orderDate.ShortDate$
	/// </summary>
	public class DateTimeFormatter
	{
		protected DateTime dateObj;

		protected DateTimeFormatter() {}

		public DateTimeFormatter(DateTime dateObj)
		{
			this.dateObj = dateObj;
		}

		#region Formatted Output Accessors

		public string ShortDate
		{
			get { return dateObj.ToShortDateString(); }
		}

		public string ShortDateTime
		{
			get { return dateObj.ToString("g", null); }
		}

		public string LongDate
		{
			get { return dateObj.ToLongDateString(); }
		}

		public string LongDateTime
		{
			get { return dateObj.ToString("f", null); }
		}

		public string SortableDateTime
		{
			get { return dateObj.ToString("u", null); }
		}

		#endregion
	}
}