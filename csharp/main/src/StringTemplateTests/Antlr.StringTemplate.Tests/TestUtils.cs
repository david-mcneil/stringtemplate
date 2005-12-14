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


namespace Antlr.StringTemplate.Tests
{
	using System;
	using NUnit.Framework;
	
	public class TestUtils
	{
		public delegate void TestMethod();

		public static void DoTimedRun(TestMethod testMethod, int reps)
		{
			System.GC.Collect();

			long rigging_time = 0;
			long start;
			long finish;

//			start = System.DateTime.Now.Ticks;
//			for (int i = 1; i <= reps; i++)
//			{
//				new TestMethod(DoNothingTestMethod)();
//			}
//			finish = System.DateTime.Now.Ticks;
//			rigging_time = (finish - start);

			Console.Out.Write("TIME: {0}", testMethod.Method.Name);
			start = System.DateTime.Now.Ticks;
			for (int i = 1; i <= reps; i++)
			{
				testMethod();
			}
			finish = System.DateTime.Now.Ticks;
			long millis = ((finish - start) - rigging_time) / TimeSpan.TicksPerMillisecond;

			Console.Out.WriteLine("; repeats = {0} {1}ms ({2} millisec/eval)", reps, millis, (millis/reps));
		}

		private static void DoNothingTestMethod()
		{
		}
	}
}