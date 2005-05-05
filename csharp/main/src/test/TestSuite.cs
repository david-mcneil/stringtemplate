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
namespace antlr.stringtemplate.test
{
	
	/// <summary>Terence's own version of a test case.  Got sick of trying to figure out
	/// the quirks of junit...this is pretty much the same functionality and
	/// only took me a few minutes to write.  Only missing the gui I guess from junit.
	/// </summary>
	public abstract class TestSuite
	{
		public String testName = null;
		
		internal int failures = 0, successes = 0;
		
		public virtual void assertEqual(Object result, Object expecting)
		{
			if (result == null && expecting != null)
			{
				throw new FailedAssertionException("expecting \"" + expecting + "\"; found null");
			}
			assertTrue(result.Equals(expecting), "expecting \"" + expecting + "\"; found \"" + result + "\"");
		}
		
		public virtual void assertEqual(int result, int expecting)
		{
			assertTrue(result == expecting, "expecting \"" + expecting + "\"; found \"" + result + "\"");
		}
		
		public virtual void assertTrue(bool test)
		{
			if (!test)
			{
				throw new FailedAssertionException();
			}
		}
		
		public virtual void assertTrue(bool test, String message)
		{
			if (!test)
			{
				if (message != null)
				{
					throw new FailedAssertionException(message);
				}
				else
				{
					throw new FailedAssertionException("assertTrue failed");
				}
			}
		}
		
		public virtual void time(String name, int n)
		{
			System.GC.Collect();
			long start = (System.DateTime.Now.Ticks - 621355968000000000) / 10000;
			System.Console.Out.Write("TIME: " + name);
			for (int i = 1; i <= n; i++)
			{
				invokeTest(name);
			}
			long finish = (System.DateTime.Now.Ticks - 621355968000000000) / 10000;
			long t = (finish - start);
			System.Console.Out.WriteLine("; n=" + n + " " + t + "ms (" + ((((double) t) / n) * 1000.0) + " microsec/eval)");
		}
		
		public virtual void runTest(String name)
		{
			try
			{
				System.Console.Out.WriteLine("TEST: " + name);
				invokeTest(name);
				successes++;
			}
			catch (System.Reflection.TargetInvocationException ite)
			{
				failures++;
				try
				{
					throw ite.GetBaseException();
				}
				catch (FailedAssertionException fae)
				{
					System.Console.Error.WriteLine(name + " failed: " + fae.Message);
				}
				catch (System.Exception e)
				{
					System.Console.Error.Write("exception during test " + name + ":");
					SupportClass.WriteStackTrace(e, Console.Error);
				}
			}
		}
		
		public virtual void invokeTest(String name)
		{
			testName = name;
			try
			{
				System.Type c = this.GetType();
				System.Reflection.MethodInfo m = c.GetMethod(name, new System.Type[0]);
				m.Invoke(this, (Object[]) null);
			}
			catch (System.UnauthorizedAccessException iae)
			{
				System.Console.Error.WriteLine("no permission to exec test " + name);
			}
			catch (System.MethodAccessException nsme)
			{
				System.Console.Error.WriteLine("no such test " + name);
			}
		}
		
		public virtual int getFailures()
		{
			return failures;
		}
		
		public virtual int getSuccesses()
		{
			return successes;
		}
	}
}