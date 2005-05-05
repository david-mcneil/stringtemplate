/*
[The "BSD licence"]
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
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.*/
using System;
using System.Reflection;
using StringTemplateGroup = antlr.stringtemplate.StringTemplateGroup;
using StringTemplate = antlr.stringtemplate.StringTemplate;
using NUnit.Framework;
namespace antlr.stringtemplate.test
{
	
	/// <summary>A semi-nonserious attempt to gauge speed of StringTemplate.  Given
	/// the problems associated with using the java system clock as a time
	/// measure, I have run this with -Xint (interpreter only mode) to see
	/// the relative performance of StringTemplate.  I'm just getting started
	/// on this.
	/// </summary>
	[TestFixture]
	public class TestEvalSpeed
	{
		public const int N = 1000;

		internal String newline = System.Environment.NewLine;
		internal StringTemplateGroup group;
		internal String gString = "group speed;\n" + "literal() ::= <<A template with just a single longish literal in it>>\n" + "bold() ::= << <b>$attr$</b> >>\n" + "st1(name) ::= <<A template with just a single attr $name$ in it>>\n" + "st2(names) ::= <<$names:bold()$>>\n";
		
		public TestEvalSpeed()
		{
			group = new StringTemplateGroup(new System.IO.StringReader(gString));
			/*
			// warm up the on-the-fly compiler
			for (int i=1; i<=2*N; i++) {
			testBaselineLiteral();
			testSingleLocalAttributeReference();
			testApplyTemplateToList();
			}
			*/
		}
		
		[Test]
		public virtual void runTests()
		{
			time("testBaselineLiteral", N);
			time("testSingleLocalAttributeReference", N);
			time("testApplyTemplateToList", N);
		}
		
		public virtual void testBaselineLiteral()
		{
			StringTemplate st = group.getInstanceOf("literal");
			st.ToString();
		}
		
		public virtual void testSingleLocalAttributeReference()
		{
			StringTemplate st = group.getInstanceOf("st1");
			st.setAttribute("name", "Ter");
			st.ToString();
		}
		
		public virtual void testApplyTemplateToList()
		{
			StringTemplate t = group.getInstanceOf("st2");
			t.setAttribute("names", "Terence");
			t.setAttribute("names", "Tom");
			t.setAttribute("names", "Sriram");
			t.setAttribute("names", "Mike");
			t.ToString();
		}

		public virtual void time(String name, int n) {
			System.GC.Collect();
			TimeSpan start = DateTime.Now.TimeOfDay;
			System.Console.Out.Write("TIME: " + name);
			for (int i = 1; i <= n; i++) {
				invokeTest(name);
			}
			TimeSpan finish = DateTime.Now.TimeOfDay;
			TimeSpan t = (finish - start);
			System.Console.Out.WriteLine("; n=" + n + " " + t.TotalMilliseconds + "ms (" + (t.TotalMilliseconds / n) + " microsec/eval)");
		}

		public virtual void invokeTest(String name) {
			try {
				Type c = this.GetType();
				MethodInfo m = c.GetMethod(name, new System.Type[0]);
				m.Invoke(this, (Object[]) null);
			}
			catch (System.UnauthorizedAccessException) {
				System.Console.Error.WriteLine("no permission to exec test " + name);
			}
			catch (System.MethodAccessException) {
				System.Console.Error.WriteLine("no such test " + name);
			}
		}
	}
}