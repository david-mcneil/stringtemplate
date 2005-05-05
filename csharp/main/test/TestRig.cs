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
	/// 
	/// This class is the main testing rig.  It executes all the tests within a
	/// TestSuite.  Invoke like this:
	/// 
	/// $ java antlr.test.unit.TestRig yourTestSuiteClassName {any-testMethod}
	/// 
	/// $ java antlr.test.unit.TestRig antlr.test.TestIntervalSet
	/// 
	/// $ java antlr.test.unit.TestRig antlr.test.TestIntervalSet testNotSet
	/// 
	/// Another benefit to building my own test rig is that users of ANTLR or any
	/// of my other software don't have to download yet another package to make
	/// this code work.  Reducing library dependencies is good.  Also, I can make
	/// this TestRig specific to my needs, hence, making me more productive.
	/// </summary>
	public class TestRig
	{
		protected internal System.Type testCaseClass = null;
		
		/// <summary>Testing program </summary>
		[STAThread]
		public static void Main(String[] args)
		{
			if (args.Length == 0)
			{
				System.Console.Error.WriteLine("Please pass in a test to run; must be class with runTests() method");
				Environment.Exit(-1);
			}
			String className = args[0];
			TestSuite test = null;
			try
			{
				System.Type c;
				try
				{
					c = System.Type.GetType(className);
					test = (TestSuite) System.Activator.CreateInstance(c);
				}
				catch (System.Exception e)
				{
					System.Console.Out.WriteLine("Cannot load class: " + className);
					SupportClass.WriteStackTrace(e, Console.Error);
					return ;
				}
				if (args.Length > 1)
				{
					// run the specific test
					String testName = args[1];
					test.runTest(testName);
				}
				else
				{
					// run them all
					// if they define a runTests, just call it
					System.Reflection.MethodInfo m = c.GetMethod("runTests", new System.Type[0]);
					
					if (m != null) 
					{
						m.Invoke(test, (Object[]) null);
					}
					else 
					{
						// else just call runTest on all methods with "test" prefix
						System.Reflection.MethodInfo[] methods = c.GetMethods();
						for (int i = 0; i < methods.Length; i++)
						{
							System.Reflection.MethodInfo testMethod = methods[i];
							if (testMethod.Name.StartsWith("test"))
							{
								test.runTest(testMethod.Name);
							}
						}
					}
				}
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine("Exception during test " + test.testName);
				SupportClass.WriteStackTrace(e, Console.Error);
			}
			System.Console.Out.WriteLine();
			System.Console.Out.WriteLine("successes: " + test.getSuccesses());
			System.Console.Out.WriteLine("failures: " + test.getFailures());
		}
	}
}