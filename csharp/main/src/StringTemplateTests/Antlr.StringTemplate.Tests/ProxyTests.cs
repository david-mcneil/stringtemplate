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


namespace Antlr.StringTemplate.Tests
{
	using System;
#if DOTNET2
	using System.Collections.Generic;
#else
	using System.Collections;
#endif
	using System.Text;

	using Antlr.StringTemplate;
	using Antlr.StringTemplate.Language;
	using Castle.DynamicProxy;
	using NUnit.Framework;

	[TestFixture]
	/*
	 *  Originally contributed by: Sean St. Quentin
	 */
	public class ProxyTests
	{
		[Test]
		public void ProxiedObjectsHandled()
		{
			StringTemplate template = new StringTemplate("Testing $person.Name$");
			
			ProxyGenerator generator = new ProxyGenerator();
			object person = generator.CreateClassProxy(typeof(Person), new NothingInterceptor(), new object[] { "Sean" });
			
			template.SetAttribute("person", person);

			string result = template.ToString();
			
			Assert.AreEqual("Testing Sean", result);
		}
	}

	internal class NothingInterceptor : IInterceptor
	{
		public object Intercept(IInvocation invocation, params object[] args)
		{
			return invocation.Proceed(args);
		}
	}

	public class Person
	{
		private readonly string name;

		public Person(string name)
		{
			this.name = name;
		}

		public virtual string Name
		{
			get { return name; }
		}
	}
}
