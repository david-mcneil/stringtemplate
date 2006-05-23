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
	using ArrayList							= System.Collections.ArrayList;
	using IList								= System.Collections.IList;
	using IDictionary						= System.Collections.IDictionary;
	using Hashtable							= System.Collections.Hashtable;
	using HybridDictionary					= System.Collections.Specialized.HybridDictionary;
	using StringBuilder						= System.Text.StringBuilder;
	using StringReader						= System.IO.StringReader;
	using StreamReader						= System.IO.StreamReader;
	using StreamWriter						= System.IO.StreamWriter;
	using Path								= System.IO.Path;
	using File								= System.IO.File;
	using Directory							= System.IO.Directory;
	using DirectoryInfo						= System.IO.DirectoryInfo;
	using StringTemplate					= Antlr.StringTemplate.StringTemplate;
	using CommonGroupLoader					= Antlr.StringTemplate.CommonGroupLoader;
	using StringTemplateGroup				= Antlr.StringTemplate.StringTemplateGroup;
	using StringTemplateGroupInterface		= Antlr.StringTemplate.StringTemplateGroupInterface;
	using AngleBracketTemplateLexer			= Antlr.StringTemplate.Language.AngleBracketTemplateLexer;
	using DefaultTemplateLexer				= Antlr.StringTemplate.Language.DefaultTemplateLexer;
	using NUnit.Framework;
	
	/// <summary>
	/// Test the various functionality of StringTemplate. Seems to run only
	/// on unix due to \r\n vs \n issue.  David Scurrah says:
	/// 
	/// "I don't think you were necessarily sloppy with your newlines, but Java 
	/// make it very difficult to be consistent.
	/// The stringtemplate library used unix end of lines for writing toString 
	/// methods and the like, while the testing was using the system local end of 
	/// line. 
	/// 
	/// The other problem with end of lines was any template file used in  the 
	/// testing will also have a specific end of line ( this case unix) and when 
	/// read into a string that can the unique problem of having end of line unix 
	/// and local system end of line in the one line.
	/// 
	/// My solution was not very elegant but I think it required the least changes 
	/// and only to the testing. I simply converted all strings to use unix end of 
	/// line characters inside the assertTrue and then compared them.
	/// The only other problem I found was writing a file out to the /tmp directory 
	/// won't work on windows so I used the system property  java.io.tmpdir to get 
	/// a temp directory."
	/// 
	/// I'll fix later.
	/// </summary>
	[TestFixture] public class TestStringTemplate
	{
		#region Nested Helper Classes

		private class AlternativeWriter : IStringTemplateWriter
		{
			private StringBuilder buf;

			public AlternativeWriter(StringBuilder buf)
			{
				this.buf = buf;
			}
			public virtual void PushIndentation(string indent)
			{
			}
			public virtual string PopIndentation()
			{
				return null;
			}
			public virtual int Write(string str)
			{
				buf.Append(str); // just pass thru
				return str.Length;
			}
		}

		/// <summary>
		/// Helper class that contains various types of non-public attributes - property, accessor, field.
		/// </summary>
		private class NonPublicPropertyAccessHelper
		{
			virtual protected int Age
			{
				get { return 21; }
			}
			protected internal bool IsAdmin
			{
				get { return false; }
			}
			private string GetNickname()
			{
				return "Floating Lotus";
			}
			protected bool IsCool()
			{
				return true;
			}
			internal int get_Bar()
			{
				return 34;
			}
			private string get_Email()
			{
				return "some.label@somedomain.com";
			}
			int foo = 9;
		}

		private class ErrorBuffer : IStringTemplateErrorListener
		{
			private StringBuilder errorOutput = new StringBuilder(500);
			int n = 0;

			public virtual void  Error(string msg, Exception e)
			{
				n++;
				if ( n>1 ) 
				{
					errorOutput.Append('\n');
				}
				if (e != null)
				{
					errorOutput.Append(msg + ": " + e.StackTrace);
				}
				else
				{
					errorOutput.Append(msg);
				}
			}
			public virtual void  Warning(string msg)
			{
				n++;
				errorOutput.Append(msg);
			}
			public override bool Equals(object o)
			{
				return ToString().Equals(o.ToString());
			}
			public override string ToString()
			{
				return errorOutput.ToString();
			}
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}
		}
		
		public class Connector
		{
			virtual public int ID					{ get { return 1; } }
			virtual public string FirstName			{ get { return "Terence"; } }
			virtual public string LastName			{ get { return "Parr"; } }
			virtual public string Email				{ get { return "parrt@jguru.com"; } }
			virtual public string Bio				{ get { return "Superhero by night..."; } }

			/// <summary>As of 2.0, booleans work as you expect.  In 1.x,
			/// a missing value simulated a boolean.
			/// </summary>
			virtual public bool CanEdit				{ get { return false; } }
		}
		
		public class Connector2
		{
			virtual public int ID					{ get { return 2; } }
			virtual public string FirstName			{ get { return "Tom"; } }
			virtual public string LastName			{ get { return "Burns"; } }
			virtual public string Email				{ get { return "tombu@jguru.com"; } }
			virtual public string Bio				{ get { return "Superhero by day..."; } }
			virtual public bool	CanEdit				{ get { return true; } }
		}
		
		public class Tree
		{
			virtual public string Text					{ get { return text; } }
			virtual public Tree FirstChild
			{
				get
				{
					if (children.Count == 0)
					{
						return null;
					}
					return (Tree) children[0];
				}
			}
			virtual public IList Children				{ get { return children; } }

			public Tree(string t)
			{
				text = t;
			}
			public virtual void  addChild(Tree c)
			{
				children.Add(c);
			}

			protected IList children = new ArrayList();
			protected string text;
		}
		
		public class Connector3
		{
			virtual public int[] Values				{ get { return new int[]{1, 2, 3}; } }
			virtual public IDictionary Stuff
			{
				get
				{
					IDictionary m = new Hashtable();
					m["a"] = "1"; 
					m["b"] = "2"; 
					return m;
				}
			}
		}
		
		public class Decl
		{
			public Decl(string name, string type)
			{
				this.name = name; 
				this.type = type;
			}

			virtual public string Name				{ get { return name; } }
			virtual public string Type				{ get { return type; } }

			string name;
			string type;
		}
		
		internal class Duh
		{
			public IList users = new ArrayList();
		}
		
		public class DateRenderer : IAttributeRenderer
		{
			public virtual string ToString(object o)
			{
				return ((DateTime) o).ToString("yyyy.MM.dd");
			}
		}
		
		public class DateRenderer2 : IAttributeRenderer
		{
			public virtual string ToString(object o)
			{
				return ((DateTime) o).ToString("MM/dd/yyyy");
			}
		}
		
		#endregion

		private static readonly object lockObject = new object();

		//static readonly string NL = System.Environment.NewLine;
		static readonly string NL = "\n";
		static readonly string TEMPDIR = System.IO.Path.GetTempPath();
		static readonly string TRUE = bool.TrueString;
		static readonly string FALSE = bool.FalseString;

		private StreamReader file1;
		private StreamReader file2;

		[SetUp] public virtual void SetUp()
		{
			file1 = null;
			file2 = null;
		}
		
		[TearDown] public virtual void TearDown()
		{
			try 
			{
				if (file1 != null)
				{
					file1.Close();
					file1 = null;
				}
				if (file2 != null)
				{
					file2.Close();
					file2 = null;
				}
			}
			catch
			{
				// ignore
			}
		}
		
		[Test] public virtual void testInterfaceFileFormat()
		{
			string groupIStr = ""
				+ "interface test;" + NL
				+ "t();" + NL
				+ "bold(item);"+ NL
				+ "optional duh(a,b,c);"+ NL;
			StringTemplateGroupInterface groupI = 
				new StringTemplateGroupInterface(new StringReader(groupIStr));

			string expecting = ""
				+ "interface test;\n" 
				+ "t();\n" 
				+ "bold(item);\n"
				+ "optional duh(a, b, c);\n";
			Assert.AreEqual(expecting, groupI.ToString());
		}

		[Test] public virtual void testNoGroupLoader()
		{
			// Next line needed to avoid test failure in NUnit due to an
			// earlier test (all tests are run in the same AppDomain) having
			// already set a default GroupLoader.
			StringTemplateGroup.RegisterGroupLoader(null);

			// this also tests the group loader
			IStringTemplateErrorListener errors = new ErrorBuffer();

			string templates = ""
				+ "group testG implements blort;" + NL
				+ "t() ::= <<foo>>" + NL
				+ "bold(item) ::= <<foo>>" + NL
				+ "duh(a,b,c) ::= <<foo>>" + NL;

			WriteFile(TEMPDIR, "testG.stg", templates);

			file1 = new StreamReader(Path.Combine(TEMPDIR, "testG.stg"));
			StringTemplateGroup group = new StringTemplateGroup(file1, errors);

			string expecting = "no group loader registered";
			Assert.AreEqual(expecting, errors.ToString());
		}

		[Test] public virtual void testCannotFindInterfaceFile()
		{
			// this also tests the group loader
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup.RegisterGroupLoader(new CommonGroupLoader(errors, TEMPDIR));

			string templates = ""
				+ "group testG implements blort;" + NL 
				+ "t() ::= <<foo>>" + NL 
				+ "bold(item) ::= <<foo>>" + NL
				+ "duh(a,b,c) ::= <<foo>>" + NL;

			WriteFile(TEMPDIR, "testG.stg", templates);

			file1 = new StreamReader(Path.Combine(TEMPDIR, "testG.stg"));
			StringTemplateGroup group = new StringTemplateGroup(file1, errors);

			string expecting = "no such interface file 'blort.sti'";
			Assert.AreEqual(expecting, errors.ToString());
		}

		[Test] public virtual void testMultiDirGroupLoading()
		{
			// this also tests the group loader
			IStringTemplateErrorListener errors = new ErrorBuffer();
			string subdir = Path.Combine(TEMPDIR, "sub");
			if ( !( Directory.Exists(subdir)) ) 
			{
				try
				{
					DirectoryInfo di = Directory.CreateDirectory(subdir);
				}
				catch
				{
					Console.Error.WriteLine("Couldn't create sub-dir in test");
					return;
				}
			}
			StringTemplateGroup.RegisterGroupLoader(
				new CommonGroupLoader(errors, TEMPDIR, subdir)
				);

			string templates = ""
				+ "group testG2;" + NL 
				+ "t() ::= <<foo>>" + NL 
				+ "bold(item) ::= <<foo>>" + NL
				+ "duh(a,b,c) ::= <<foo>>" + NL;

			WriteFile(subdir, "testG2.stg", templates);

			StringTemplateGroup group =
				StringTemplateGroup.LoadGroup("testG2");
			string expecting = ""
				+ "group testG2;\n" 
				+ "bold(item) ::= <<foo>>\n" 
				+ "duh(a,b,c) ::= <<foo>>\n" 
				+ "t() ::= <<foo>>\n";
			Assert.AreEqual(expecting, group.ToString());
			DeleteFile(TEMPDIR, "testG2.stg");
		}

		[Test] public virtual void testGroupSatisfiesSingleInterface()
		{
			// this also tests the group loader
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup.RegisterGroupLoader(new CommonGroupLoader(errors, TEMPDIR));
			string groupIStr = ""
				+  "interface testI;" + NL 
				+ "t();" + NL 
				+ "bold(item);" + NL
				+ "optional duh(a,b,c);" + NL;
			WriteFile(TEMPDIR, "testI.sti", groupIStr);

			string templates = ""
				+ "group testG implements testI;" + NL 
				+ "t() ::= <<foo>>" + NL 
				+ "bold(item) ::= <<foo>>" + NL
				+ "duh(a,b,c) ::= <<foo>>" + NL;

			WriteFile(TEMPDIR, "testG.stg", templates);

			file1 = new StreamReader(Path.Combine(TEMPDIR, "testG.stg"));
			StringTemplateGroup group = new StringTemplateGroup(file1, errors);

			string expecting = ""; // should be no errors
			Assert.AreEqual(expecting, errors.ToString());
		}

		[Test] public virtual void testGroupExtendsSuperGroup()
		{
			// this also tests the group loader
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup.RegisterGroupLoader(
				new CommonGroupLoader(errors, TEMPDIR)
				);
			string superGroup = ""
				+ "group superG;" + NL 
				+ "bold(item) ::= <<*$item$*>>;\n" + NL;
			WriteFile(TEMPDIR, "superG.stg", superGroup);

			string templates = ""
				+ "group testG : superG;" + NL 
				+ "main(x) ::= <<$bold(x)$>>" + NL;

			WriteFile(TEMPDIR, "testG.stg", templates);

			file1 = new StreamReader(Path.Combine(TEMPDIR, "testG.stg"));
			StringTemplateGroup group = new StringTemplateGroup(file1, typeof(DefaultTemplateLexer), errors);
			StringTemplate st = group.GetInstanceOf("main");
			st.SetAttribute("x", "foo");

			string expecting = "*foo*";
			Assert.AreEqual(expecting, st.ToString());
		}

		[Test] public virtual void testMissingInterfaceTemplate()
		{
			// this also tests the group loader
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup.RegisterGroupLoader(new CommonGroupLoader(errors, TEMPDIR));
			string groupIStr = ""
				+ "interface testI;" + NL 
				+ "t();" + NL 
				+ "bold(item);" + NL
				+ "optional duh(a,b,c);" + NL;
			WriteFile(TEMPDIR, "testI.sti", groupIStr);

			string templates = ""
				+  "group testG implements testI;" + NL 
				+ "t() ::= <<foo>>" + NL 
				+ "duh(a,b,c) ::= <<foo>>" + NL;

			WriteFile(TEMPDIR, "testG.stg", templates);

			file1 = new StreamReader(Path.Combine(TEMPDIR, "testG.stg"));
			StringTemplateGroup group = new StringTemplateGroup(file1, errors);

			string expecting = "group 'testG' does not satisfy interface 'testI': missing templates [bold]";
			Assert.AreEqual(expecting, errors.ToString());
		}

		[Test] public virtual void testMissingOptionalInterfaceTemplate()
		{
			// this also tests the group loader
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup.RegisterGroupLoader(new CommonGroupLoader(errors, TEMPDIR));
			string groupIStr = ""
				+ "interface testI;" + NL 
				+ "t();" + NL 
				+ "bold(item);" + NL
				+ "optional duh(a,b,c);" + NL;
			WriteFile(TEMPDIR, "testI.sti", groupIStr);

			string templates = ""
				+ "group testG implements testI;" + NL 
				+ "t() ::= <<foo>>" + NL 
				+ "bold(item) ::= <<foo>>";

			WriteFile(TEMPDIR, "testG.stg", templates);

			file1 = new StreamReader(Path.Combine(TEMPDIR, "testG.stg"));
			StringTemplateGroup group = new StringTemplateGroup(file1, errors);

			string expecting = ""; // should be NO errors
			Assert.AreEqual(expecting, errors.ToString());
		}

		[Test] public virtual void testMismatchedInterfaceTemplate()
		{
			// this also tests the group loader
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup.RegisterGroupLoader(new CommonGroupLoader(errors, TEMPDIR));
			string groupIStr = ""
				+ "interface testI;" + NL 
				+ "t();" + NL 
				+ "bold(item);" + NL
				+ "optional duh(a,b,c);" + NL;
			WriteFile(TEMPDIR, "testI.sti", groupIStr);

			string templates = ""
				+ "group testG implements testI;" + NL 
				+ "t() ::= <<foo>>" + NL 
				+ "bold(item) ::= <<foo>>" + NL
				+ "duh(a,c) ::= <<foo>>" + NL;

			WriteFile(TEMPDIR, "testG.stg", templates);

			file1 = new StreamReader(Path.Combine(TEMPDIR, "testG.stg"));
			StringTemplateGroup group = new StringTemplateGroup(file1, errors);

			string expecting = "group 'testG' does not satisfy interface 'testI': mismatched arguments on these templates [optional duh(a, b, c)]";
			Assert.AreEqual(expecting, errors.ToString());
		}

		[Test] public virtual void  testGroupFileFormat()
		{
			string templates = ""
				+ "group test;" + NL 
				+ @"t() ::= ""literal template""" + NL 
				+ @"bold(item) ::= ""<b>$item$</b>""" + NL 
				+ "duh() ::= <<" + NL 
				+ "xx" + NL 
				+ ">>" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer));
			
			string expecting = ""
				+ "group test;" + NL 
				+ "bold(item) ::= <<<b>$item$</b>>>" + NL
				+ "duh() ::= <<xx>>" + NL
				+ "t() ::= <<literal template>>" + NL;

			Assert.AreEqual(expecting, group.ToString());
			
			StringTemplate a = group.GetInstanceOf("t");
			expecting = "literal template";
			Assert.AreEqual(expecting, a.ToString());
			
			StringTemplate b = group.GetInstanceOf("bold");
			b.SetAttribute("item", "dork");
			expecting = "<b>dork</b>";
			Assert.AreEqual(expecting, b.ToString());
		}
		
		[Test] public void testEscapedTemplateDelimiters()
		{
			string templates = ""
				+ "group test;" + NL
				+ "t() ::= <<$\"literal\":{a|$a$\\}}$ template\n>>" + NL
				+ "bold(item) ::= <<<b>$item$</b\\>>>" + NL
				+ "duh() ::= <<" + NL
				+ "xx" + NL
				+ ">>" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer));

			string expecting = ""
				+ "group test;" + NL
				+ "bold(item) ::= <<<b>$item$</b>>>" + NL
				+ "duh() ::= <<xx>>" + NL
				+ "t() ::= <<$\"literal\":{a|$a$\\}}$ template>>" + NL;
			Assert.AreEqual(expecting, group.ToString());

			StringTemplate b = group.GetInstanceOf("bold");
			b.SetAttribute("item", "dork");
			expecting = "<b>dork</b>";
			Assert.AreEqual(expecting, b.ToString());

			StringTemplate a = group.GetInstanceOf("t");
			expecting = "literal} template";
			Assert.AreEqual(expecting, a.ToString());
		}

		/// <summary>Check syntax and setAttribute-time errors </summary>
		[Test] public virtual void  testTemplateParameterDecls()
		{
			string templates = ""
				+ "group test;" + NL 
				+ @"t() ::= ""no args but ref $foo$""" + NL 
				+ @"t2(item) ::= ""decl but not used is ok""" + NL 
				+ "t3(a,b,c,d) ::= <<$a$ $d$>>" + NL 
				+ "t4(a,b,c,d) ::= <<$a$ $b$ $c$ $d$>>" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			
			// check setting unknown arg in empty formal list
			StringTemplate a = group.GetInstanceOf("t");
			string error = null;
			try
			{
				a.SetAttribute("foo", "x"); // want NoSuchElementException
			}
			catch (InvalidOperationException e)
			{
				error = e.Message;
			}
			string expecting = "no such attribute: foo in template context [t]";
			Assert.AreEqual(expecting, error);
			
			// check setting known arg
			a = group.GetInstanceOf("t2");
			a.SetAttribute("item", "x"); // shouldn't get exception
			
			// check setting unknown arg in nonempty list of formal args
			a = group.GetInstanceOf("t3");
			a.SetAttribute("b", "x");
		}
		
		[Test] public virtual void  testTemplateRedef()
		{
			string templates = ""
				+ "group test;" + NL 
				+ @"a() ::= ""x""" + NL 
				+ @"b() ::= ""y""" + NL 
				+ @"a() ::= ""z""" + NL;
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), errors);
			string expecting = "redefinition of template: a";
			Assert.AreEqual(expecting, errors.ToString());
		}
		
		[Test] public virtual void  testMissingInheritedAttribute()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "page(title,font) ::= <<" + NL 
				+ "<html>" + NL 
				+ "<body>" + NL 
				+ "$title$<br>" + NL 
				+ "$body()$" + NL 
				+ "</body>" + NL 
				+ "</html>" + NL 
				+ ">>" + NL 
				+ @"body() ::= ""<font face=$font$>my body</font>""" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate t = group.GetInstanceOf("page");
			t.SetAttribute("title", "my title");
			t.SetAttribute("font", "Helvetica"); // body() will see it
			t.ToString(); // should be no problem
		}
		
		[Test] public virtual void  testFormalArgumentAssignment()
		{
			string templates = ""
				+ "group test;" + NL 
				+ @"page() ::= <<$body(font=""Times"")$>>" + NL 
				+ @"body(font) ::= ""<font face=$font$>my body</font>""" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer));
			StringTemplate t = group.GetInstanceOf("page");
			string expecting = "<font face=Times>my body</font>";
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testUndefinedArgumentAssignment()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "page(x) ::= <<$body(font=x)$>>" + NL 
				+ @"body() ::= ""<font face=$font$>my body</font>""" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer));
			StringTemplate t = group.GetInstanceOf("page");
			t.SetAttribute("x", "Times");
			string error = "";
			try
			{
				t.ToString();
			}
			catch (InvalidOperationException iae)
			{
				error = iae.Message;
			}
			string expecting = "template body has no such attribute: font in template context [page <invoke body arg context>]";
			Assert.AreEqual(expecting, error);
		}
		
		[Test] public virtual void  testFormalArgumentAssignmentInApply()
		{
			string templates = ""
				+ "group test;" + NL 
				+ @"page(name) ::= <<$name:bold(font=""Times"")$>>" + NL 
				+ @"bold(font) ::= ""<font face=$font$><b>$it$</b></font>""" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer));
			StringTemplate t = group.GetInstanceOf("page");
			t.SetAttribute("name", "Ter");
			string expecting = "<font face=Times><b>Ter</b></font>";
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testUndefinedArgumentAssignmentInApply()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "page(name,x) ::= <<$name:bold(font=x)$>>" + NL 
				+ @"bold() ::= ""<font face=$font$><b>$it$</b></font>""" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer));
			StringTemplate t = group.GetInstanceOf("page");
			t.SetAttribute("x", "Times");
			t.SetAttribute("name", "Ter");
			string error = "";
			try
			{
				t.ToString();
			}
			catch (InvalidOperationException iae)
			{
				error = iae.Message;
			}
			string expecting = "template bold has no such attribute: font in template context [page <invoke bold arg context>]";
			Assert.AreEqual(expecting, error);
		}
		
		[Test] public virtual void  testUndefinedAttributeReference()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "page() ::= <<$bold()$>>" + NL 
				+ @"bold() ::= ""$name$""" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer));
			StringTemplate t = group.GetInstanceOf("page");
			string error = "";
			try
			{
				t.ToString();
			}
			catch (InvalidOperationException iae)
			{
				error = iae.Message;
			}
			string expecting = "no such attribute: name in template context [page bold]";
			Assert.AreEqual(expecting, error);
		}
		
		[Test] public virtual void  testUndefinedDefaultAttributeReference()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "page() ::= <<$bold()$>>" + NL 
				+ @"bold() ::= ""$it$""" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer));
			StringTemplate t = group.GetInstanceOf("page");
			string error = "";
			try
			{
				t.ToString();
			}
			catch (InvalidOperationException nse)
			{
				error = nse.Message;
			}
			string expecting = "no such attribute: it in template context [page bold]";
			Assert.AreEqual(expecting, error);
		}
		
		[Test] public virtual void  testAngleBracketsWithGroupFile()
		{
			string templates = ""
				+ "group test;" + NL 
				+ @"a(s) ::= ""<s:{case <i> : <it> break;}>""" + NL 
				+ @"b(t) ::= ""<t; separator=\"",\"">""" + NL 
				+ @"c(t) ::= << <t; separator="",""> >>" + NL;
			// mainly testing to ensure we don't get parse errors of above
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate t = group.GetInstanceOf("a");
			t.SetAttribute("s", "Test");
			string expecting = "case 1 : Test break;";
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testAngleBracketsNoGroup()
		{
			StringTemplate st = new StringTemplate(@"Tokens : <rules; separator=""|""> ;", typeof(AngleBracketTemplateLexer));
			st.SetAttribute("rules", "A");
			st.SetAttribute("rules", "B");
			string expecting = "Tokens : A|B ;";
			Assert.AreEqual(expecting, st.ToString());
		}
		
		[Test] public void testRegionRef() 
		{
			string templates = ""
				+ "group test;" + NL
				+ "a() ::= \"X$@r()$Y\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer));
			StringTemplate st = group.GetInstanceOf("a");
			string result = st.ToString();
			string expecting = "XY";
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testEmbeddedRegionRef()
		{
			string templates = ""
				+ "group test;" + NL
				+ "a() ::= \"X$@r$blort$@end$Y\"" + NL;
			StringTemplateGroup group =
				new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer));
			StringTemplate st = group.GetInstanceOf("a");
			string result = st.ToString();
			string expecting = "XblortY";
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testRegionRefAngleBrackets()
		{
			string templates = ""
				+ "group test;" + NL
				+ "a() ::= \"X<@r()>Y\"" + NL;
			StringTemplateGroup group =
				new StringTemplateGroup(new StringReader(templates));
			StringTemplate st = group.GetInstanceOf("a");
			string result = st.ToString();
			string expecting = "XY";
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testEmbeddedRegionRefAngleBrackets()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "a() ::= \"X<@r>blort<@end>Y\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate st = group.GetInstanceOf("a");
			string result = st.ToString();
			string expecting = "XblortY";
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testEmbeddedRegionRefWithNewlinesAngleBrackets()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "a() ::= \"X<@r>" + NL
				+ "blort" + NL
				+ "<@end>" + NL
				+ "Y\"" + NL;
			StringTemplateGroup group =
				new StringTemplateGroup(new StringReader(templates));
			StringTemplate st = group.GetInstanceOf("a");
			string result = st.ToString();
			string expecting = "XblortY";
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testRegionRefWithDefAngleBrackets()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "a() ::= \"X<@r()>Y\"" + NL
				+ "@a.r() ::= \"foo\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate st = group.GetInstanceOf("a");
			string result = st.ToString();
			string expecting = "XfooY";
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testRegionRefWithDefInConditional()
		{
			string templates = ""
				+ "group test;" +NL
				+ "a(v) ::= \"X<if(v)>A<@r()>B<endif>Y\"" +NL
				+ "@a.r() ::= \"foo\"" +NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate st = group.GetInstanceOf("a");
			st.SetAttribute("v", "true");
			string result = st.ToString();
			string expecting = "XAfooBY";
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testRegionRefWithImplicitDefInConditional()
		{
			string templates = ""
				+ "group test;" +NL
				+ "a(v) ::= \"X<if(v)>A<@r>yo<@end>B<endif>Y\"" +NL
				+ "@a.r() ::= \"foo\"" +NL;
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group =
				new StringTemplateGroup(new StringReader(templates), errors);
			StringTemplate st = group.GetInstanceOf("a");
			st.SetAttribute("v", "true");
			string result = st.ToString();
			string expecting = "XAyoBY";
			Assert.AreEqual(expecting, result);

			string err_result = errors.ToString();
			string err_expecting = "group test line 3: redefinition of template region: @a.r";
			Assert.AreEqual(err_expecting, err_result);
		}

		[Test] public void testRegionOverride()
		{
			string templates1 = ""
				+ "group super;" +NL
				+ "a() ::= \"X<@r()>Y\"" 
				+ "@a.r() ::= \"foo\"" +NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates1));

			string templates2 = ""
				+ "group sub;" +NL
				+ "@a.r() ::= \"foo\"" +NL;
			StringTemplateGroup subGroup = new StringTemplateGroup(
				new StringReader(templates2),
				typeof(AngleBracketTemplateLexer),
				null,
				group);

			StringTemplate st = subGroup.GetInstanceOf("a");
			string result = st.ToString();
			string expecting = "XfooY";
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testRegionOverrideRefSuperRegion()
		{
			string templates1 = ""
				+ "group super;" +NL
				+ "a() ::= \"X<@r()>Y\"" 
				+ "@a.r() ::= \"foo\"" +NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates1));

			string templates2 = ""
				+ "group sub;" +NL
				+ "@a.r() ::= \"A<@super.r()>B\"" +NL;
			StringTemplateGroup subGroup = new StringTemplateGroup(
				new StringReader(templates2),
				(IStringTemplateErrorListener)null,
				group);

			StringTemplate st = subGroup.GetInstanceOf("a");
			string result = st.ToString();
			string expecting = "XAfooBY";
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testRegionOverrideRefSuperRegion3Levels()
		{
			// Bug: This was causing infinite recursion:
			// getInstanceOf(super::a)
			// getInstanceOf(sub::a)
			// getInstanceOf(subsub::a)
			// getInstanceOf(subsub::region__a__r)
			// getInstanceOf(subsub::super.region__a__r)
			// getInstanceOf(subsub::super.region__a__r)
			// getInstanceOf(subsub::super.region__a__r)
			// ...
			// Somehow, the ref to super in subsub is not moving up the chain
			// to the @super.r(); oh, i introduced a bug when i put setGroup
			// into STG.getInstanceOf()!

			string templates1 = ""
				+ "group super;" +NL
				+ "a() ::= \"X<@r()>Y\"" 
				+ "@a.r() ::= \"foo\"" +NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates1));

			string templates2 = ""
				+ "group sub;" +NL
				+ "@a.r() ::= \"<@super.r()>2\"" +NL;
			StringTemplateGroup subGroup = new StringTemplateGroup(
				new StringReader(templates2),
				typeof(AngleBracketTemplateLexer),
				null,
				group);

			string templates3 = ""
				+ "group subsub;" +NL
				+ "@a.r() ::= \"<@super.r()>3\"" +NL;
			StringTemplateGroup subSubGroup = new StringTemplateGroup(
				new StringReader(templates3),
				typeof(AngleBracketTemplateLexer),
				null,
				subGroup);

			StringTemplate st = subSubGroup.GetInstanceOf("a");
			string result = st.ToString();
			string expecting = "Xfoo23Y";
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testRegionOverrideRefSuperImplicitRegion()
		{
			string templates1 = ""
				+ "group super;" +NL
				+ "a() ::= \"X<@r>foo<@end>Y\""+NL;
			StringTemplateGroup group = new StringTemplateGroup(
				new StringReader(templates1));

			string templates2 = ""
				+ "group sub;" +NL
				+ "@a.r() ::= \"A<@super.r()>\"" +NL;
			StringTemplateGroup subGroup = new StringTemplateGroup(
				new StringReader(templates2),
				null,		// should default to typeof(AngleBracketTemplateLexer),
				null,
				group);

			StringTemplate st = subGroup.GetInstanceOf("a");
			string result = st.ToString();
			string expecting = "XAfooY";
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testEmbeddedRegionRedefError()
		{
			// cannot define an embedded template within group
			string templates = ""
				+ "group test;" +NL
				+ "a() ::= \"X<@r>dork<@end>Y\"" 
				+ "@a.r() ::= \"foo\"" +NL;
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), errors);
			StringTemplate st = group.GetInstanceOf("a");
			st.ToString();
			string result = errors.ToString();
			string expecting = "group test line 2: redefinition of template region: @a.r";
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testImplicitRegionRedefError()
		{
			// cannot define an implicitly-defined template more than once
			string templates = ""
				+ "group test;" +NL
				+ "a() ::= \"X<@r()>Y\"" +NL
				+ "@a.r() ::= \"foo\"" +NL
				+ "@a.r() ::= \"bar\"" +NL;
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group =
				new StringTemplateGroup(new StringReader(templates), errors);
			StringTemplate st = group.GetInstanceOf("a");
			st.ToString();
			string result = errors.ToString();
			string expecting = "group test line 4: redefinition of template region: @a.r";
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testImplicitOverriddenRegionRedefError()
		{
			string templates1 = ""
				+ "group super;" +NL
				+ "a() ::= \"X<@r()>Y\"" 
				+ "@a.r() ::= \"foo\"" +NL;
			StringTemplateGroup group = new StringTemplateGroup(
				new StringReader(templates1));

			string templates2 = ""
				+ "group sub;" +NL
				+ "@a.r() ::= \"foo\"" +NL
				+ "@a.r() ::= \"bar\"" +NL;
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup subGroup = new StringTemplateGroup(
				new StringReader(templates2), errors, group);

			StringTemplate st = subGroup.GetInstanceOf("a");
			string result = errors.ToString();
			string expecting = "group sub line 3: redefinition of template region: @a.r";
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testUnknownRegionDefError()
		{
			// cannot define an implicitly-defined template more than once
			string templates = ""
				+  "group test;" +NL
				+ "a() ::= \"X<@r()>Y\"" +NL
				+ "@a.q() ::= \"foo\"" +NL;
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group = new StringTemplateGroup(
				new StringReader(templates),
				errors);
			StringTemplate st = group.GetInstanceOf("a");
			st.ToString();
			string result = errors.ToString();
			string expecting = "group test line 3: template a has no region called q";
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testSuperRegionRefError()
		{
			string templates1 = ""
				+ "group super;" +NL
				+ "a() ::= \"X<@r()>Y\"" 
				+ "@a.r() ::= \"foo\"" +NL;
			StringTemplateGroup group = new StringTemplateGroup(
				new StringReader(templates1),
				typeof(AngleBracketTemplateLexer));

			string templates2 = ""
				+ "group sub;" +NL
				+ "@a.r() ::= \"A<@super.q()>B\"" +NL;
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup subGroup = new StringTemplateGroup(
				new StringReader(templates2),
				errors,
				group);

			StringTemplate st = subGroup.GetInstanceOf("a");
			string result = errors.ToString();
			string expecting = "template a has no region called q";
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testMissingEndRegionError()
		{
			// cannot define an implicitly-defined template more than once
			string templates = ""
				+ "group test;" +NL
				+ "a() ::= \"X$@r$foo\"" +NL;
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group = new StringTemplateGroup(
				new StringReader(templates),
				typeof(DefaultTemplateLexer),
				errors);
			StringTemplate st = group.GetInstanceOf("a");
			st.ToString();
			string result = errors.ToString();
			string expecting = "missing region r $@end$ tag";
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testMissingEndRegionErrorAngleBrackets()
		{
			// cannot define an implicitly-defined template more than once
			string templates = ""
				+ "group test;" +NL
				+ "a() ::= \"X<@r>foo\"" +NL;
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group = new StringTemplateGroup(
				new StringReader(templates),
				typeof(AngleBracketTemplateLexer),
				errors);
			StringTemplate st = group.GetInstanceOf("a");
			st.ToString();
			string result = errors.ToString();
			string expecting = "missing region r <@end> tag";
			Assert.AreEqual(expecting, result);
		}

		[Test] public virtual void  testSimpleInheritance()
		{
			// make a bold template in the super group that you can inherit from sub
			StringTemplateGroup supergroup = new StringTemplateGroup("super");
			StringTemplateGroup subgroup = new StringTemplateGroup("sub");
			StringTemplate bold = supergroup.DefineTemplate("bold", "<b>$it$</b>");
			subgroup.SuperGroup = supergroup;
			IStringTemplateErrorListener errors = new ErrorBuffer();
			subgroup.ErrorListener = errors;
			supergroup.ErrorListener = errors;
			StringTemplate duh = new StringTemplate(subgroup, "$name:bold()$");
			duh.SetAttribute("name", "Terence");
			string expecting = "<b>Terence</b>";
			Assert.AreEqual(expecting, duh.ToString());
		}
		
		[Test] public virtual void  testOverrideInheritance()
		{
			// make a bold template in the super group and one in sub group
			StringTemplateGroup supergroup = new StringTemplateGroup("super");
			StringTemplateGroup subgroup = new StringTemplateGroup("sub");
			supergroup.DefineTemplate("bold", "<b>$it$</b>");
			subgroup.DefineTemplate("bold", "<strong>$it$</strong>");
			subgroup.SuperGroup = supergroup;
			IStringTemplateErrorListener errors = new ErrorBuffer();
			subgroup.ErrorListener = errors;
			supergroup.ErrorListener = errors;
			StringTemplate duh = new StringTemplate(subgroup, "$name:bold()$");
			duh.SetAttribute("name", "Terence");
			string expecting = "<strong>Terence</strong>";
			Assert.AreEqual(expecting, duh.ToString());
		}
		
		[Test] public virtual void  testMultiLevelInheritance()
		{
			// must loop up two levels to find bold()
			StringTemplateGroup rootgroup = new StringTemplateGroup("root");
			StringTemplateGroup level1 = new StringTemplateGroup("level1");
			StringTemplateGroup level2 = new StringTemplateGroup("level2");
			rootgroup.DefineTemplate("bold", "<b>$it$</b>");
			level1.SuperGroup = rootgroup;
			level2.SuperGroup = level1;
			IStringTemplateErrorListener errors = new ErrorBuffer();
			rootgroup.ErrorListener = errors;
			level1.ErrorListener = errors;
			level2.ErrorListener = errors;
			StringTemplate duh = new StringTemplate(level2, "$name:bold()$");
			duh.SetAttribute("name", "Terence");
			string expecting = "<b>Terence</b>";
			Assert.AreEqual(expecting, duh.ToString());
		}
		
		[Test] public void testComplicatedInheritance()
		{
			// in super: decls invokes labels
			// in sub:   overridden decls which calls super.decls
			//           overridden labels
			// Bug: didn't see the overridden labels.  In other words,
			// the overridden decls called super which called labels, but
			// didn't get the subgroup overridden labels--it calls the
			// one in the superclass.  Ouput was "DL" not "DSL"; didn't
			// invoke sub's labels().
			string basetemplates = ""
				+ "group base;" + NL 
				+ "decls() ::= \"D<labels()>\""+ NL 
				+ "labels() ::= \"L\"" + NL
				;
			StringTemplateGroup @base = new StringTemplateGroup(
				new StringReader(basetemplates),
				typeof(AngleBracketTemplateLexer));
			string subtemplates = ""
				+ "group sub;" + NL 
				+ "decls() ::= \"<super.decls()>\""+ NL 
				+ "labels() ::= \"SL\"" + NL
				;
			StringTemplateGroup sub = new StringTemplateGroup(
				new StringReader(subtemplates),
				typeof(AngleBracketTemplateLexer));
			sub.SuperGroup = @base;
			StringTemplate st = sub.GetInstanceOf("decls");
			string expecting = "DSL";
			string result = st.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public void test3LevelSuperRef()
		{
			string templates1 = ""
				+ "group super;" + NL 
				+ "r() ::= \"foo\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(
				new StringReader(templates1),
				typeof(AngleBracketTemplateLexer));

			string templates2 =
				"group sub;" + NL +
				"r() ::= \"<super.r()>2\"" + NL;
			StringTemplateGroup subGroup =
				new StringTemplateGroup(new StringReader(templates2),
				typeof(AngleBracketTemplateLexer),
				null,
				group);

			string templates3 = ""
				+ "group subsub;" + NL 
				+ "r() ::= \"<super.r()>3\"" + NL;
			StringTemplateGroup subSubGroup = new StringTemplateGroup(
				new StringReader(templates3),
				typeof(AngleBracketTemplateLexer),
				null,
				subGroup);

			StringTemplate st = subSubGroup.GetInstanceOf("r");
			string result = st.ToString();
			string expecting = "foo23";
			Assert.AreEqual(expecting, result);
		}

		[Test] public virtual void  testExprInParens()
		{
			// specify a template to apply to an attribute
			// Use a template group so we can specify the start/stop chars
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate bold = group.DefineTemplate("bold", "<b>$it$</b>");
			StringTemplate duh = new StringTemplate(group, @"$(""blort: ""+(list)):bold()$");
			duh.SetAttribute("list", "a");
			duh.SetAttribute("list", "b");
			duh.SetAttribute("list", "c");
			// System.out.println(duh);
			string expecting = "<b>blort: abc</b>";
			Assert.AreEqual(expecting, duh.ToString());
		}
		
		[Test] public virtual void  testMultipleAdditions()
		{
			// specify a template to apply to an attribute
			// Use a template group so we can specify the start/stop chars
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			group.DefineTemplate("link", @"<a href=""$url$""><b>$title$</b></a>");
			StringTemplate duh = new StringTemplate(
				group, @"$link(url=""/member/view?ID=""+ID+""&x=y""+foo, title=""the title"")$"
				);
			duh.SetAttribute("ID", "3321");
			duh.SetAttribute("foo", "fubar");
			string expecting = @"<a href=""/member/view?ID=3321&x=yfubar""><b>the title</b></a>";
			Assert.AreEqual(expecting, duh.ToString());
		}
		
		[Test] public virtual void  testCollectionAttributes()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate bold = group.DefineTemplate("bold", "<b>$it$</b>");
			StringTemplate t = new StringTemplate(
				group, "$data$, $data:bold()$, " + "$list:bold():bold()$, $array$, $a2$, $a3$, $a4$"
				);
			ArrayList v = new ArrayList(10);
			v.Add("1");
			v.Add("2");
			v.Add("3");
			IList list = new ArrayList();
			list.Add("a");
			list.Add("b");
			list.Add("c");
			t.SetAttribute("data", v);
			t.SetAttribute("list", list);
			t.SetAttribute("array", ((object) new string[]{"x", "y"}));
			t.SetAttribute("a2", ((object) new int[]{10, 20}));
			t.SetAttribute("a3", ((object) new float[]{1.2f, 1.3f}));
			t.SetAttribute("a4", ((object) new double[]{8.7, 9.2}));

			string expecting = "123, <b>1</b><b>2</b><b>3</b>, " 
				+ "<b><b>a</b></b><b><b>b</b></b><b><b>c</b></b>, xy, 1020, 1.21.3, 8.79.2";
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testParenthesizedExpression()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate bold = group.DefineTemplate("bold", "<b>$it$</b>");
			StringTemplate t = new StringTemplate(group, "$(f+l):bold()$");
			t.SetAttribute("f", "Joe");
			t.SetAttribute("l", "Schmoe");

			string expecting = "<b>JoeSchmoe</b>";
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testApplyTemplateNameExpression()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate bold = group.DefineTemplate("foobar", "foo$attr$bar");
			StringTemplate t = new StringTemplate(group, @"$data:(name+""bar"")()$");
			t.SetAttribute("data", "Ter");
			t.SetAttribute("data", "Tom");
			t.SetAttribute("name", "foo");

			string expecting = "fooTerbarfooTombar";
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public void testApplyTemplateNameTemplateEval()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate foobar = group.DefineTemplate("foobar", "foo$it$bar");
			StringTemplate a = group.DefineTemplate("a", "$it$bar");
			StringTemplate t = new StringTemplate(group, "$data:(\"foo\":a())()$");
			t.SetAttribute("data", "Ter");
			t.SetAttribute("data", "Tom");
			string expecting = "fooTerbarfooTombar";
			Assert.AreEqual(expecting, t.ToString());
		}

		[Test] public virtual void  testTemplateNameExpression()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate foo = group.DefineTemplate("foo", "hi there!");
			StringTemplate t = new StringTemplate(group, "$(name)()$");
			t.SetAttribute("name", "foo");
			string expecting = "hi there!";
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public void  testMissingEndDelimiter()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			IStringTemplateErrorListener errors = new ErrorBuffer();
			group.ErrorListener = errors;
			StringTemplate t = new StringTemplate(group, "stuff $a then more junk etc...");
			//string expectingError = "problem parsing template 'anonymous': line 1:31: expecting '$', found '<EOF>'";
			// different error output from C# Antlr?
			string expectingError = "problem parsing template 'anonymous'";
			//System.out.println("error: '"+errors+"'");
			//System.out.println("expecting: '"+expectingError+"'");
			Assert.IsTrue(errors.ToString().StartsWith(expectingError));
		}
		
		[Test] public virtual void  testSetButNotRefd()
		{
			StringTemplate.LintMode = true;
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate t = new StringTemplate(group, "$a$ then $b$ and $c$ refs.");
			t.SetAttribute("a", "Terence");
			t.SetAttribute("b", "Terence");
			t.SetAttribute("cc", "Terence"); // oops...should be 'c'
			IStringTemplateErrorListener errors = new ErrorBuffer();
			group.ErrorListener = errors;
			string expectingError = "anonymous: set but not used: cc";
			string result = t.ToString(); // result is irrelevant
			//System.out.println("result error: '"+errors+"'");
			//System.out.println("expecting: '"+expectingError+"'");
			StringTemplate.LintMode = false;
			Assert.AreEqual(expectingError, errors.ToString());
		}
		
		[Test] public virtual void  testNullTemplateApplication()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			IStringTemplateErrorListener errors = new ErrorBuffer();
			group.ErrorListener = errors;
			StringTemplate t = new StringTemplate(group, "$names:bold(x=it)$");
			t.SetAttribute("names", "Terence");
			string result = null;
			string error = null;
			try
			{
				result = t.ToString();
			}
			catch (TemplateLoadException iae)
			{
				error = iae.Message;
			}
			Assert.AreEqual("Can't load template 'bold.st'; context is [anonymous]", error);
		}
		
		[Test] public virtual void  testNullTemplateToMultiValuedApplication()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			IStringTemplateErrorListener errors = new ErrorBuffer();
			group.ErrorListener = errors;
			StringTemplate t = new StringTemplate(group, "$names:bold(x=it)$");
			t.SetAttribute("names", "Terence");
			t.SetAttribute("names", "Tom");
			string result = null;
			string error = null;
			try
			{
				result = t.ToString();
			}
			catch (TemplateLoadException iae)
			{
				error = iae.Message;
			}
			Assert.AreEqual("Can't load template 'bold.st'; context is [anonymous]", error);
		}
		
		[Test] public virtual void  testChangingAttrValueTemplateApplicationToVector()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate bold = group.DefineTemplate("bold", "<b>$x$</b>");
			StringTemplate t = new StringTemplate(group, "$names:bold(x=it)$");
			t.SetAttribute("names", "Terence");
			t.SetAttribute("names", "Tom");
			string expecting = "<b>Terence</b><b>Tom</b>";
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testChangingAttrValueRepeatedTemplateApplicationToVector()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate bold = group.DefineTemplate("bold", "<b>$item$</b>");
			StringTemplate italics = group.DefineTemplate("italics", "<i>$it$</i>");
			StringTemplate members = new StringTemplate(group, "$members:bold(item=it):italics(it=it)$");
			members.SetAttribute("members", "Jim");
			members.SetAttribute("members", "Mike");
			members.SetAttribute("members", "Ashar");
			string expecting = "<i><b>Jim</b></i><i><b>Mike</b></i><i><b>Ashar</b></i>";
			Assert.AreEqual(expecting, members.ToString());
		}
		
		[Test] public virtual void  testAlternatingTemplateApplication()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate listItem = group.DefineTemplate("listItem", "<li>$it$</li>");
			StringTemplate bold = group.DefineTemplate("bold", "<b>$it$</b>");
			StringTemplate italics = group.DefineTemplate("italics", "<i>$it$</i>");
			StringTemplate item = new StringTemplate(group, "$item:bold(),italics():listItem()$");
			item.SetAttribute("item", "Jim");
			item.SetAttribute("item", "Mike");
			item.SetAttribute("item", "Ashar");
			string expecting = "<li><b>Jim</b></li><li><i>Mike</i></li><li><b>Ashar</b></li>";
			Assert.AreEqual(expecting, item.ToString());
		}
		
		[Test] public virtual void  testExpressionAsRHSOfAssignment()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate hostname = group.DefineTemplate("hostname", "$machine$.jguru.com");
			StringTemplate bold = group.DefineTemplate("bold", "<b>$x$</b>");
			StringTemplate t = new StringTemplate(group, @"$bold(x=hostname(machine=""www""))$");
			string expecting = "<b>www.jguru.com</b>";
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testTemplateApplicationAsRHSOfAssignment()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate hostname = group.DefineTemplate("hostname", "$machine$.jguru.com");
			StringTemplate bold = group.DefineTemplate("bold", "<b>$x$</b>");
			StringTemplate italics = group.DefineTemplate("italics", "<i>$it$</i>");
			StringTemplate t = new StringTemplate(group, @"$bold(x=hostname(machine=""www""):italics())$");
			string expecting = "<b><i>www.jguru.com</i></b>";
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testParameterAndAttributeScoping()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate italics = group.DefineTemplate("italics", "<i>$x$</i>");
			StringTemplate bold = group.DefineTemplate("bold", "<b>$x$</b>");
			StringTemplate t = new StringTemplate(group, "$bold(x=italics(x=name))$");
			t.SetAttribute("name", "Terence");
			string expecting = "<b><i>Terence</i></b>";
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testComplicatedSeparatorExpr()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate bold = group.DefineTemplate("bulletSeparator", "</li>$foo$<li>");
			// make separator a complicated expression with args passed to included template
			StringTemplate t = new StringTemplate(
				group, @"<ul>$name; separator=bulletSeparator(foo="" "")+""&nbsp;""$</ul>"
				);
			t.SetAttribute("name", "Ter");
			t.SetAttribute("name", "Tom");
			t.SetAttribute("name", "Mel");
			//System.out.println(t);
			string expecting = "<ul>Ter</li> <li>&nbsp;Tom</li> <li>&nbsp;Mel</ul>";
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testAttributeRefButtedUpAgainstEndifAndWhitespace()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate a = new StringTemplate(group, "$if (!firstName)$$email$$endif$");
			a.SetAttribute("email", "parrt@jguru.com");
			string expecting = "parrt@jguru.com";
			Assert.AreEqual(expecting, a.ToString());
		}
		
		[Test] public virtual void  testStringCatenationOnSingleValuedAttributeViaTemplateLiteral()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate bold = group.DefineTemplate("bold", "<b>$it$</b>");
			StringTemplate b = new StringTemplate(group, "$bold(it={$name$ Parr})$");
			b.SetAttribute("name", "Terence");
			string expecting = "<b>Terence Parr</b>";
			Assert.AreEqual(expecting, b.ToString());
		}
		
		[Test] public void testStringCatenationOpOnArg()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate bold = group.DefineTemplate("bold", "<b>$it$</b>");
			StringTemplate b = new StringTemplate(group, "$bold(it=name+\" Parr\")$");
			b.SetAttribute("name", "Terence");
			string expecting = "<b>Terence Parr</b>";
			Assert.AreEqual(expecting, b.ToString());
		}

		[Test] public void testStringCatenationOpOnArgWithEqualsInString()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate bold = group.DefineTemplate("bold", "<b>$it$</b>");
			StringTemplate b = new StringTemplate(group, "$bold(it=name+\" Parr=\")$");
			b.SetAttribute("name", "Terence");
			string expecting = "<b>Terence Parr=</b>";
			Assert.AreEqual(expecting, b.ToString());
		}

		[Test] public void testStringCatenationOpOnArgWithEqualsInStringAngleBrackets()
		{
			StringTemplateGroup group = new StringTemplateGroup("test", typeof(AngleBracketTemplateLexer));
			StringTemplate bold = group.DefineTemplate("bold", "#b#<it>#/b#");
			StringTemplate b = new StringTemplate(group, "<bold(it=name+\" Parr=\")>");
			b.SetAttribute("name", "Terence");
			string expecting = "#b#Terence Parr=#/b#";
			Assert.AreEqual(expecting, b.ToString());
		}

		[Test] public virtual void  testApplyingTemplateFromDiskWithPrecompiledIF()
		{
			// First write the template files to the TEMPDIR
			string pageTemplateFileName = Path.Combine(TEMPDIR, "page.st");
			StreamWriter fw = new StreamWriter(pageTemplateFileName, false, System.Text.Encoding.Default);
			fw.Write("<html><head>" + NL);
			//fw.Write("  <title>PeerScope: $title$</title>" + NL);
			fw.Write("</head>" + NL);
			fw.Write("<body>" + NL);
			fw.Write("$if(member)$User: $member:terse()$$endif$" + NL);
			fw.Write("</body>" + NL);
			fw.Write("</head>");
			fw.Close();
			
			string terseTemplateFileName = Path.Combine(TEMPDIR, "terse.st");
			fw = new StreamWriter(terseTemplateFileName, false, System.Text.Encoding.Default);
			fw.Write("$it.firstName$ $it.lastName$ (<tt>$it.email$</tt>)");
			fw.Close();
			
			// specify a template to apply to an attribute
			// Use a template group so we can specify the start/stop chars
			StringTemplateGroup group = new StringTemplateGroup("dummy", TEMPDIR);
			
			StringTemplate a = group.GetInstanceOf("page");
			a.SetAttribute("member", new Connector());
			string expecting = ""
				+ "<html><head>" + NL 
				+ "</head>" + NL 
				+ "<body>" + NL 
				+ "User: Terence Parr (<tt>parrt@jguru.com</tt>)" + NL 
				+ "</body>" + NL 
				+ "</head>";
			Assert.AreEqual(expecting, a.ToString());
		}
		
		[Test] public virtual void  testMultiValuedAttributeWithAnonymousTemplateUsingIndexVariableI()
		{
			StringTemplateGroup tgroup = new StringTemplateGroup("dummy", ".");
			StringTemplate t = new StringTemplate(
				tgroup, " List:" + NL 
				+ "  " + NL 
				+ "foo" + NL 
				+ NL 
				+ "$names:{<br>$i$. $it$" + NL 
				+ "}$"
				);
			t.SetAttribute("names", "Terence");
			t.SetAttribute("names", "Jim");
			t.SetAttribute("names", "Sriram");
			string expecting = " List:" + NL 
				+ "  " + NL 
				+ "foo" + NL 
				+ NL 
				+ "<br>1. Terence" + NL + "<br>2. Jim" + NL + "<br>3. Sriram" + NL;
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test]
		public virtual void  testFindTemplateInCLASSPATH_Nope_JustRelativeToAssemblyLocation()
		{
			// Look for templates in CLASSPATH as resources
			StringTemplateGroup mgroup = new StringTemplateGroup("method stuff", typeof(AngleBracketTemplateLexer));
			StringTemplate m = mgroup.GetInstanceOf("tests/method");
			// "method.st" references body() so "body.st" will be loaded too
			m.SetAttribute("visibility", "public");
			m.SetAttribute("name", "foobar");
			m.SetAttribute("returnType", "void");
			m.SetAttribute("statements", "i=1;"); // body inherits these from method
			m.SetAttribute("statements", "x=i;");
			string expecting = "public void foobar() {" + NL
				+ "\t// start of a body" + NL
				+ "\ti=1;" + NL + "\tx=i;" + NL
				+ "\t// end of a body" + NL
				+ "}";
			//.Replace(Environment.NewLine, "\n") below mitigates against failure due to line-ending differences
			Assert.AreEqual(expecting, m.ToString().Replace(Environment.NewLine, "\n"));
		}
		
		[Test] public virtual void  testApplyTemplateToSingleValuedAttribute()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate bold = group.DefineTemplate("bold", "<b>$x$</b>");
			StringTemplate name = new StringTemplate(group, "$name:bold(x=name)$");
			name.SetAttribute("name", "Terence");
			Assert.AreEqual(name.ToString(), "<b>Terence</b>");
		}
		
		[Test] public virtual void  testStringLiteralAsAttribute()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate bold = group.DefineTemplate("bold", "<b>$it$</b>");
			StringTemplate name = new StringTemplate(group, "$\"Terence\":bold()$");
			Assert.AreEqual(name.ToString(), "<b>Terence</b>");
		}
		
		[Test] public virtual void  testApplyTemplateToSingleValuedAttributeWithDefaultAttribute()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate bold = group.DefineTemplate("bold", "<b>$it$</b>");
			StringTemplate name = new StringTemplate(group, "$name:bold()$");
			name.SetAttribute("name", "Terence");
			Assert.AreEqual(name.ToString(), "<b>Terence</b>");
		}
		
		[Test] public virtual void  testApplyAnonymousTemplateToSingleValuedAttribute()
		{
			// specify a template to apply to an attribute
			// Use a template group so we can specify the start/stop chars
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate item = new StringTemplate(group, "$item:{<li>$it$</li>}$");
			item.SetAttribute("item", "Terence");
			Assert.AreEqual(item.ToString(), "<li>Terence</li>");
		}
		
		[Test] public virtual void  testApplyAnonymousTemplateToMultiValuedAttribute()
		{
			// specify a template to apply to an attribute
			// Use a template group so we can specify the start/stop chars
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate list = new StringTemplate(group, "<ul>$items$</ul>");
			// demonstrate setting arg to anonymous subtemplate
			StringTemplate item = new StringTemplate(group, "$item:{<li>$it$</li>}; separator=\",\"$");
			item.SetAttribute("item", "Terence");
			item.SetAttribute("item", "Jim");
			item.SetAttribute("item", "John");
			list.SetAttribute("items", item); // nested template
			Assert.AreEqual(list.ToString(), "<ul><li>Terence</li>,<li>Jim</li>,<li>John</li></ul>");
		}
		
		[Test] public virtual void  testApplyAnonymousTemplateToAggregateAttribute()
		{
			StringTemplate st = new StringTemplate("$items:{$it.lastName$, $it.firstName$\n}$");
			// also testing wacky spaces and tabs in aggregate spec
			st.SetAttribute("items.{ firstName,lastName }", "Ter", "Parr");
			st.SetAttribute("items.{firstName	, lastName}", "Tom", "Burns");
			string expecting = "Parr, Ter" + NL + "Burns, Tom" + NL;
			Assert.AreEqual(expecting, st.ToString());
		}
		
		[Test] public virtual void  testRepeatedApplicationOfTemplateToSingleValuedAttribute()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate search = group.DefineTemplate("bold", "<b>$it$</b>");
			StringTemplate item = new StringTemplate(group, "$item:bold():bold()$");
			item.SetAttribute("item", "Jim");
			Assert.AreEqual(item.ToString(), "<b><b>Jim</b></b>");
		}
		
		[Test] public virtual void  testRepeatedApplicationOfTemplateToMultiValuedAttributeWithSeparator()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate search = group.DefineTemplate("bold", "<b>$it$</b>");
			StringTemplate item = new StringTemplate(group, "$item:bold():bold(); separator=\",\"$");
			item.SetAttribute("item", "Jim");
			item.SetAttribute("item", "Mike");
			item.SetAttribute("item", "Ashar");
			// first application of template must yield another vector!
			//System.out.println("ITEM="+item);
			Assert.AreEqual(item.ToString(), "<b><b>Jim</b></b>,<b><b>Mike</b></b>,<b><b>Ashar</b></b>");
		}
		
		// ### NEED A TEST OF obj ASSIGNED TO ARG?
		
		[Test] public virtual void  testMultiValuedAttributeWithSeparator()
		{
			StringTemplate query;
			
			// if column can be multi-valued, specify a separator
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".", typeof(AngleBracketTemplateLexer));
			query = new StringTemplate(group, "SELECT <distinct> <column; separator=\", \"> FROM <table>;");
			query.SetAttribute("column", "name");
			query.SetAttribute("column", "email");
			query.SetAttribute("table", "User");
			// uncomment next line to make "DISTINCT" appear in output
			// query.SetAttribute("distince", "DISTINCT");
			// System.out.println(query);
			Assert.AreEqual(query.ToString(), "SELECT  name, email FROM User;");
		}
		
		[Test] public virtual void  testSingleValuedAttributes()
		{
			// all attributes are single-valued:
			StringTemplate query = new StringTemplate("SELECT $column$ FROM $table$;");
			query.SetAttribute("column", "name");
			query.SetAttribute("table", "User");
			// System.out.println(query);
			Assert.AreEqual(query.ToString(), "SELECT name FROM User;");
		}
		
		[Test] public virtual void  testIFTemplate()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".", typeof(AngleBracketTemplateLexer));
			StringTemplate t = new StringTemplate(group, "SELECT <column> FROM PERSON " + "<if(cond)>WHERE ID=<id><endif>;");
			t.SetAttribute("column", "name");
			t.SetAttribute("cond", "true");
			t.SetAttribute("id", "231");
			Assert.AreEqual(t.ToString(), "SELECT name FROM PERSON WHERE ID=231;");
		}
		
		[Test] public virtual void  testIFCondWithParensTemplate()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".", typeof(AngleBracketTemplateLexer));
			StringTemplate t = new StringTemplate(group, "<if(map.(type))><type> <prop>=<map.(type)>;<endif>");
			Hashtable map = new Hashtable();
			map["int"] = "0";
			t.SetAttribute("map", map);
			t.SetAttribute("prop", "x");
			t.SetAttribute("type", "int");
			Assert.AreEqual("int x=0;", t.ToString());
		}
		
		[Test] public virtual void  testIFCondWithParensDollarDelimsTemplate()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate t = new StringTemplate(group, "$if(map.(type))$$type$ $prop$=$map.(type)$;$endif$");
			Hashtable map = new Hashtable();
			map["int"] = "0";
			t.SetAttribute("map", map);
			t.SetAttribute("prop", "x");
			t.SetAttribute("type", "int");
			Assert.AreEqual("int x=0;", t.ToString());
		}
		
		/// <summary>As of 2.0, you can test a boolean value </summary>
		[Test] public virtual void  testIFBoolean()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate t = new StringTemplate(group, "$if(b)$x$endif$ $if(!b)$y$endif$");
			t.SetAttribute("b", (object) true);
			Assert.AreEqual(t.ToString(), "x ");
			
			t = t.GetInstanceOf();
			t.SetAttribute("b", (object) false);
			Assert.AreEqual(t.ToString(), " y");
		}
		
		[Test] public virtual void  testNestedIFTemplate()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".", typeof(AngleBracketTemplateLexer));
			StringTemplate t = new StringTemplate(
				group, "ack<if(a)>" + NL 
				+ "foo" + NL 
				+ "<if(!b)>stuff<endif>" + NL 
				+ "<if(b)>no<endif>" + NL 
				+ "junk" + NL 
				+ "<endif>"
				);
			t.SetAttribute("a", "blort");
			// leave b as null
			//System.out.println("t="+t);
			string expecting = "ackfoo" + NL + "stuff" + NL + "junk";
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testObjectPropertyReference()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate t = new StringTemplate(
				group, "<b>Name: $p.firstName$ $p.lastName$</b><br>" + NL 
				+ "<b>Email: $p.email$</b><br>" + NL 
				+ "$p.bio$"
				);
			t.SetAttribute("p", new Connector());
			string expecting = "<b>Name: Terence Parr</b><br>" + NL 
				+ "<b>Email: parrt@jguru.com</b><br>" + NL 
				+ "Superhero by night...";
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testApplyRepeatedAnonymousTemplateWithForeignTemplateRefToMultiValuedAttribute()
		{
			// specify a template to apply to an attribute
			// Use a template group so we can specify the start/stop chars
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			group.DefineTemplate("link", @"<a href=""$url$""><b>$title$</b></a>");
			StringTemplate duh = new StringTemplate(
				group, @"start|$p:{$link(url=""/member/view?ID=""+it.ID, title=it.firstName)$ $if(it.canEdit)$canEdit$endif$}:" 
				+ "{$it$<br>\n}$|end"
				);
			duh.SetAttribute("p", new Connector());
			duh.SetAttribute("p", new Connector2());
			string expecting = @"start|<a href=""/member/view?ID=1""><b>Terence</b></a> <br>" + NL 
				+ @"<a href=""/member/view?ID=2""><b>Tom</b></a> canEdit<br>" + NL + "|end";
			Assert.AreEqual(expecting, duh.ToString());
		}
		
		[Test] public virtual void  testRecursion()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".", typeof(AngleBracketTemplateLexer));
			group.DefineTemplate("tree", ""
				+ "<if(it.firstChild)>" 
				+ "( <it.text> <it.children:tree(); separator=\" \"> )" 
				+ "<else>" 
				+ "<it.text>" 
				+ "<endif>"
				);
			StringTemplate tree = group.GetInstanceOf("tree");
			// build ( a b (c d) e )
			Tree root = new Tree("a");
			root.addChild(new Tree("b"));
			Tree subtree = new Tree("c");
			subtree.addChild(new Tree("d"));
			root.addChild(subtree);
			root.addChild(new Tree("e"));
			tree.SetAttribute("it", root);
			string expecting = "( a b ( c d ) e )";
			Assert.AreEqual(expecting, tree.ToString());
		}
		
		[Test] public virtual void  testNestedAnonymousTemplates()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate t = new StringTemplate(
				group, "$A:{" + NL + "<i>$it:{" + NL + "<b>$it$</b>" + NL + "}$</i>" + NL + "}$"
				);
			t.SetAttribute("A", "parrt");
			string expecting = NL + "<i>" + NL + "<b>parrt</b>" + NL + "</i>" + NL;
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testAnonymousTemplateAccessToEnclosingAttributes()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate t = new StringTemplate(
				group, ""
				+ "$A:{" + NL 
				+ "<i>$it:{" + NL 
				+ "<b>$it$, $B$</b>" + NL 
				+ "}$</i>" + NL 
				+ "}$"
				);
			t.SetAttribute("A", "parrt");
			t.SetAttribute("B", "tombu");
			string expecting = NL 
				+ "<i>" + NL 
				+ "<b>parrt, tombu</b>" + NL 
				+ "</i>" + NL;
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testNestedAnonymousTemplatesAgain()
		{
			
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate t = new StringTemplate(
				group, "<table>" + NL 
				+ "$names:{<tr>$it:{<td>$it:{<b>$it$</b>}$</td>}$</tr>}$" + NL 
				+ "</table>" + NL
				);
			t.SetAttribute("names", "parrt");
			t.SetAttribute("names", "tombu");
			string expecting = "<table>" + NL 
				+ "<tr><td><b>parrt</b></td></tr><tr><td><b>tombu</b></td></tr>" + NL 
				+ "</table>" + NL;
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testEscapes()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			group.DefineTemplate("foo", "$x$ && $it$");
			StringTemplate t = new StringTemplate(group, "$A:foo(x=\"dog\\\"\\\"\")$");
			StringTemplate u = new StringTemplate(group, "$A:foo(x=\"dog\\\"g\")$");
			StringTemplate v = new StringTemplate(group, "$A:{$it:foo(x=\"\\{dog\\}\\\"\")$ is cool}$");
			t.SetAttribute("A", "ick");
			u.SetAttribute("A", "ick");
			v.SetAttribute("A", "ick");

			string expecting = "dog\"\" && ick";
			Assert.AreEqual(expecting, t.ToString());
			expecting = "dog\"g && ick";
			Assert.AreEqual(expecting, u.ToString());
			expecting = "{dog}\" && ick is cool";
			Assert.AreEqual(expecting, v.ToString());
		}
		
		[Test] public virtual void  testEscapesOutsideExpressions()
		{
			StringTemplate b = new StringTemplate("It\\'s ok...\\$; $a:{\\'hi\\', $it$}$");
			b.SetAttribute("a", "Ter");
			string expecting = "It\\'s ok...$; \\'hi\\', Ter";
			string result = b.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testLiteralStringEscapes()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			group.DefineTemplate("foo", "$x$ && $it$");
			StringTemplate t = new StringTemplate(group, @"$A:foo(x=""dog\""\"""")$");
			StringTemplate u = new StringTemplate(group, @"$A:foo(x=""dog\""g"")$");
			StringTemplate v = new StringTemplate(group, @"$A:{$it:foo(x=""\{dog\}\"""")$ is cool}$");
			t.SetAttribute("A", "ick");
			u.SetAttribute("A", "ick");
			v.SetAttribute("A", "ick");
			string expecting = @"dog"""" && ick";
			Assert.AreEqual(expecting, t.ToString());
			expecting = @"dog""g && ick";
			Assert.AreEqual(expecting, u.ToString());
			expecting = @"{dog}"" && ick is cool";
			Assert.AreEqual(expecting, v.ToString());
		}
		
		[Test] public virtual void  testLiteralStringEscapesOutsideExpressions()
		{
			StringTemplate b = new StringTemplate(@"It\'s ok...\$; $a:{\'hi\', $it$}$");
			b.SetAttribute("a", "Ter");
			string expecting = @"It\'s ok...$; \'hi\', Ter";
			string result = b.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testElseClause()
		{
			StringTemplate e = new StringTemplate("$if(title)$" + NL 
				+ "foo" + NL 
				+ "$else$" + NL 
				+ "bar" + NL 
				+ "$endif$"
				);
			e.SetAttribute("title", "sample");
			string expecting = "foo";
			Assert.AreEqual(expecting, e.ToString());
			
			e = e.GetInstanceOf();
			expecting = "bar";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testNestedIF()
		{
			StringTemplate e = new StringTemplate("$if(title)$" + NL 
				+ "foo" + NL 
				+ "$else$" + NL 
				+ "$if(header)$" + NL 
				+ "bar" + NL 
				+ "$else$" + NL 
				+ "blort" + NL 
				+ "$endif$" + NL 
				+ "$endif$");
			e.SetAttribute("title", "sample");
			string expecting = "foo";
			Assert.AreEqual(expecting, e.ToString());
			
			e = e.GetInstanceOf();
			e.SetAttribute("header", "more");
			expecting = "bar";
			Assert.AreEqual(expecting, e.ToString());
			
			e = e.GetInstanceOf();
			expecting = "blort";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testEmbeddedMultiLineIF()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate main = new StringTemplate(group, "$sub$");
			StringTemplate sub = new StringTemplate(
				group, ""
				+ "begin" + NL 
				+ "$if(foo)$" + NL 
				+ "$foo$" + NL 
				+ "$else$" + NL 
				+ "blort" + NL 
				+ "$endif$" + NL
				);
			sub.SetAttribute("foo", "stuff");
			main.SetAttribute("sub", sub);
			string expecting = "begin" + NL 
				+ "stuff";
			Assert.AreEqual(expecting, main.ToString());
			
			main = new StringTemplate(group, "$sub$");
			sub = sub.GetInstanceOf();
			main.SetAttribute("sub", sub);
			expecting = "begin" + NL 
				+ "blort";
			Assert.AreEqual(expecting, main.ToString());
		}
		
		[Test] public virtual void  testSimpleIndentOfAttributeList()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "list(names) ::= <<" + @"  $names; separator=""\n""$" + NL 
				+ ">>" + NL;
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer), errors);
			StringTemplate t = group.GetInstanceOf("list");
			t.SetAttribute("names", "Terence");
			t.SetAttribute("names", "Jim");
			t.SetAttribute("names", "Sriram");
			string expecting = ""
				+ "  Terence" + NL 
				+ "  Jim" + NL 
				+ "  Sriram";
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testIndentOfMultilineAttributes()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "list(names) ::= <<" + @"  $names; separator=""\n""$" + NL 
				+ ">>" + NL;
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer), errors);
			StringTemplate t = group.GetInstanceOf("list");
			t.SetAttribute("names", "Terence\nis\na\nmaniac");
			t.SetAttribute("names", "Jim");
			t.SetAttribute("names", "Sriram\nis\ncool");
			string expecting = ""
				+ "  Terence" + NL 
				+ "  is" + NL 
				+ "  a" + NL 
				+ "  maniac" + NL 
				+ "  Jim" + NL 
				+ "  Sriram" + NL 
				+ "  is" + NL 
				+ "  cool";
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testIndentOfMultipleBlankLines()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "list(names) ::= <<" + "  $names$" + NL 
				+ ">>" + NL;
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer), errors);
			StringTemplate t = group.GetInstanceOf("list");
			t.SetAttribute("names", "Terence\n\nis a maniac");
			string expecting = ""
				+ "  Terence\n\n"
				+ "  is a maniac";
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testIndentBetweenLeftJustifiedLiterals()
		{
			string templates = "" 
				+ "group test;" + NL 
				+ "list(names) ::= <<" + "Before:" + NL 
				+ @"  $names; separator=""\n""$" + NL 
				+ "after" + NL 
				+ ">>" + NL;
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer), errors);
			StringTemplate t = group.GetInstanceOf("list");
			t.SetAttribute("names", "Terence");
			t.SetAttribute("names", "Jim");
			t.SetAttribute("names", "Sriram");
			string expecting = "" 
				+ "Before:" + NL 
				+ "  Terence" + NL 
				+ "  Jim" + NL 
				+ "  Sriram" + NL 
				+ "after";
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testNestedIndent()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "method(name,stats) ::= <<" + "void $name$() {" + NL 
				+ "\t$stats; separator=\"\\n\"$" + NL 
				+ "}" + NL 
				+ ">>" + NL 
				+ "ifstat(expr,stats) ::= <<" + NL 
				+ "if ($expr$) {" + NL 
				+ "  $stats; separator=\"\\n\"$" + NL 
				+ "}" + ">>" + NL 
				+ "assign(lhs,expr) ::= <<$lhs$=$expr$;>>" + NL;
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer), errors);
			StringTemplate t = group.GetInstanceOf("method");
			t.SetAttribute("name", "foo");
			StringTemplate s1 = group.GetInstanceOf("assign");
			s1.SetAttribute("lhs", "x");
			s1.SetAttribute("expr", "0");
			StringTemplate s2 = group.GetInstanceOf("ifstat");
			s2.SetAttribute("expr", "x>0");
			StringTemplate s2a = group.GetInstanceOf("assign");
			s2a.SetAttribute("lhs", "y");
			s2a.SetAttribute("expr", "x+y");
			StringTemplate s2b = group.GetInstanceOf("assign");
			s2b.SetAttribute("lhs", "z");
			s2b.SetAttribute("expr", "4");
			s2.SetAttribute("stats", s2a);
			s2.SetAttribute("stats", s2b);
			t.SetAttribute("stats", s1);
			t.SetAttribute("stats", s2);
			string expecting = "" 
				+ "void foo() {" + NL 
				+ "\tx=0;" + NL 
				+ "\tif (x>0) {" + NL 
				+ "\t  y=x+y;" + NL 
				+ "\t  z=4;" + NL 
				+ "\t}" + NL 
				+ "}";
			Assert.AreEqual(expecting, t.ToString());
		}
		
		[Test] public virtual void  testAlternativeWriter()
		{
			StringBuilder buf = new StringBuilder();
			IStringTemplateWriter w = new AlternativeWriter(buf);
			StringTemplateGroup group = new StringTemplateGroup("test");
			group.DefineTemplate("bold", "<b>$x$</b>");
			StringTemplate name = new StringTemplate(group, "$name:bold(x=name)$");
			name.SetAttribute("name", "Terence");
			name.Write(w);
			Assert.AreEqual(buf.ToString(), "<b>Terence</b>");
		}

		// WAS: public virtual void  testApplyAnonymousTemplateToMapAndSet()
		[Test] public virtual void  testApplyAnonymousTemplateToHashtableAndHybridDictionary()
		{
			StringTemplate st = new StringTemplate("$items:{<li>$it$</li>}$");
			IDictionary m = new Hashtable();
			m["a"] = "1";
			m["b"] = "2";
			m["c"] = "3";
			st.SetAttribute("items", m);
			string expecting = "<li>1</li><li>2</li><li>3</li>";
			Assert.AreEqual(expecting, st.ToString());
			
			st = st.GetInstanceOf();
			IDictionary s = new HybridDictionary();
			s.Add("1", "1");
			s.Add("2", "2");
			s.Add("3", "3");
			st.SetAttribute("items", s);
			expecting = "<li>1</li><li>2</li><li>3</li>";
			Assert.AreEqual(expecting, st.ToString());
		}

		//WAS: public virtual void  testDumpMapAndSet()
		[Test] public virtual void  testDumpHashtableAndHybridDictionary()
		{
			StringTemplate st = new StringTemplate("$items; separator=\",\"$");
			IDictionary m = new Hashtable();
			m["a"] = "1";
			m["b"] = "2";
			m["c"] = "3";
			st.SetAttribute("items", m);
			string expecting = "1,2,3";
			Assert.AreEqual(expecting, st.ToString());
			
			st = st.GetInstanceOf();
			IDictionary s = new HybridDictionary();
			s.Add("1", "1");
			s.Add("2", "2");
			s.Add("3", "3");
			st.SetAttribute("items", s);
			expecting = "1,2,3";
			Assert.AreEqual(expecting, st.ToString());
		}
		
		[Test] public virtual void  testApplyAnonymousTemplateToArrayAndMapProperty()
		{
			StringTemplate st = new StringTemplate("$x.values:{<li>$it$</li>}$");
			st.SetAttribute("x", new Connector3());
			string expecting = "<li>1</li><li>2</li><li>3</li>";
			Assert.AreEqual(expecting, st.ToString());
			
			st = new StringTemplate("$x.stuff:{<li>$it$</li>}$");
			st.SetAttribute("x", new Connector3());
			expecting = "<li>1</li><li>2</li>";
			Assert.AreEqual(expecting, st.ToString());
		}
		
		[Test] public virtual void  testSuperTemplateRef()
		{
			// you can refer to a template defined in a super group via super.t()
			StringTemplateGroup group = new StringTemplateGroup("super");
			StringTemplateGroup subGroup = new StringTemplateGroup("sub");
			subGroup.SuperGroup = group;
			group.DefineTemplate("page", "$font()$:text");
			group.DefineTemplate("font", "Helvetica");
			subGroup.DefineTemplate("font", "$super.font()$ and Times");
			StringTemplate st = subGroup.GetInstanceOf("page");
			string expecting = "Helvetica and Times:text";
			Assert.AreEqual(expecting, st.ToString());
		}
		
		[Test] public virtual void  testApplySuperTemplateRef()
		{
			StringTemplateGroup group = new StringTemplateGroup("super");
			StringTemplateGroup subGroup = new StringTemplateGroup("sub");
			subGroup.SuperGroup = group;
			group.DefineTemplate("bold", "<b>$it$</b>");
			subGroup.DefineTemplate("bold", "<strong>$it$</strong>");
			subGroup.DefineTemplate("page", "$name:super.bold()$");
			StringTemplate st = subGroup.GetInstanceOf("page");
			st.SetAttribute("name", "Ter");
			string expecting = "<b>Ter</b>";
			Assert.AreEqual(expecting, st.ToString());
		}
		
		[Test] public virtual void  testLazyEvalOfSuperInApplySuperTemplateRef()
		{
			StringTemplateGroup group = new StringTemplateGroup("base");
			StringTemplateGroup subGroup = new StringTemplateGroup("sub");
			subGroup.SuperGroup = group;
			group.DefineTemplate("bold", "<b>$it$</b>");
			subGroup.DefineTemplate("bold", "<strong>$it$</strong>");
			// this is the same as testApplySuperTemplateRef() test
			// 'cept notice that here the supergroup defines page
			// As long as you create the instance via the subgroup,
			// "super." will evaluate lazily (i.e., not statically
			// during template compilation) to the templates
			// getGroup().superGroup value.  If I create instance
			// of page in group not subGroup, however, I will get
			// an error as superGroup is null for group "group".
			group.DefineTemplate("page", "$name:super.bold()$");
			StringTemplate st = subGroup.GetInstanceOf("page");
			st.SetAttribute("name", "Ter");
			string error = null;
			try 
			{
				st.ToString();
			}
			catch (StringTemplateException iae) 
			{
				error = iae.Message;
			}
			string expectingError = "base has no super group; invalid template: super.bold";
			Assert.AreEqual(expectingError, error);
		}
		
		[Test] public virtual void  testTemplatePolymorphism()
		{
			StringTemplateGroup group = new StringTemplateGroup("super");
			StringTemplateGroup subGroup = new StringTemplateGroup("sub");
			subGroup.SuperGroup = group;
			// bold is defined in both super and sub
			// if you create an instance of page via the subgroup,
			// then bold() should evaluate to the subgroup not the super
			// even though page is defined in the super.  Just like polymorphism.
			group.DefineTemplate("bold", "<b>$it$</b>");
			group.DefineTemplate("page", "$name:bold()$");
			subGroup.DefineTemplate("bold", "<strong>$it$</strong>");
			StringTemplate st = subGroup.GetInstanceOf("page");
			st.SetAttribute("name", "Ter");
			string expecting = "<strong>Ter</strong>";
			Assert.AreEqual(expecting, st.ToString());
		}
		
		[Test] public virtual void  testListOfEmbeddedTemplateSeesEnclosingAttributes()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "output(cond,items) ::= <<page: $items$>>" + NL 
				+ "mybody() ::= <<$font()$stuff>>" + NL 
				+ "font() ::= <<$if(cond)$this$else$that$endif$>>";
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer), errors);
			StringTemplate outputST = group.GetInstanceOf("output");
			StringTemplate bodyST1 = group.GetInstanceOf("mybody");
			StringTemplate bodyST2 = group.GetInstanceOf("mybody");
			StringTemplate bodyST3 = group.GetInstanceOf("mybody");
			outputST.SetAttribute("items", bodyST1);
			outputST.SetAttribute("items", bodyST2);
			outputST.SetAttribute("items", bodyST3);
			string expecting = "page: thatstuffthatstuffthatstuff";
			Assert.AreEqual(expecting, outputST.ToString());
		}
		
		[Test] public virtual void  testInheritArgumentFromRecursiveTemplateApplication()
		{
			// do not inherit attributes through formal args
			string templates = ""
				+ "group test;" + NL 
				+ "block(stats) ::= \"<stats>\"" + "ifstat(stats) ::= \"IF true then <stats>\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate b = group.GetInstanceOf("block");
			b.SetAttribute("stats", group.GetInstanceOf("ifstat"));
			b.SetAttribute("stats", group.GetInstanceOf("ifstat"));
			string expecting = "IF true then IF true then ";
			string result = b.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		
		[Test] public virtual void  testDeliberateRecursiveTemplateApplication()
		{
			// This test will cause infinite loop.  block contains a stat which
			// contains the same block.  Must be in lintMode to detect
			string templates = ""
				+ "group test;" + NL 
				+ "block(stats) ::= \"<stats>\"" 
				+ "ifstat(stats) ::= \"IF true then <stats>\"" + NL;
			StringTemplate.LintMode = true;
			StringTemplate.resetTemplateCounter();
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate b = group.GetInstanceOf("block");
			StringTemplate ifstat = group.GetInstanceOf("ifstat");
			b.SetAttribute("stats", ifstat); // block has if stat
			ifstat.SetAttribute("stats", b); // but make "if" contain block
			string expectingError = ""
				+ "infinite recursion to <ifstat([stats])@4> referenced in <block([stats])@3>; stack trace:" + NL 
				+ "<ifstat([stats])@4>, attributes=[stats=<block()@3>]>" + NL 
				+ "<block([stats])@3>, attributes=[stats=<ifstat()@4>], references=[stats]>" + NL 
				+ "<ifstat([stats])@4> (start of recursive cycle)" + NL 
				+ "...";
			// note that attributes attribute doesn't show up in ifstat() because
			// recursion detection traps the problem before it writes out the
			// infinitely-recursive template; I set the attributes attribute right
			// before I render.
			string errors = "";
			try
			{
				string result = b.ToString();
			}
			catch (SystemException ise)
			{
				errors = ise.Message;
			}
			StringTemplate.LintMode = false;
			Assert.AreEqual(expectingError, errors);
		}
		
		
		[Test] public virtual void  testImmediateTemplateAsAttributeLoop()
		{
			// even though block has a stats value that refers to itself,
			// there is no recursion because each instance of block hides
			// the stats value from above since it's a formal arg.
			string templates = ""
				+ "group test;" + NL 
				+ "block(stats) ::= \"{<stats>}\"";
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate b = group.GetInstanceOf("block");
			b.SetAttribute("stats", group.GetInstanceOf("block"));
			string expecting = "{{}}";
			string result = b.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		
		[Test] public virtual void  testTemplateAlias()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "page(name) ::= \"name is <name>\"" + "other ::= page" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate b = group.GetInstanceOf("other"); // alias for page
			b.SetAttribute("name", "Ter");
			string expecting = "name is Ter";
			string result = b.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testTemplateGetPropertyGetsAttribute()
		{
			// This test will cause infinite loop if missing attribute no
			// properly caught in getAttribute
			string templates = ""
				+ "group test;" + NL 
				+ "Cfile(funcs) ::= <<" + NL 
				+ "#include \\<stdio.h>" + NL 
				+ "<funcs:{public void <it.name>(<it.args>);}; separator=\"\\n\">" + NL 
				+ "<funcs; separator=\"\\n\">" + NL 
				+ ">>" + NL 
				+ "func(name,args,body) ::= <<" + NL 
				+ "public void <name>(<args>) {<body>}" + NL 
				+ ">>" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate b = group.GetInstanceOf("Cfile");
			StringTemplate f1 = group.GetInstanceOf("func");
			StringTemplate f2 = group.GetInstanceOf("func");
			f1.SetAttribute("name", "f");
			f1.SetAttribute("args", "");
			f1.SetAttribute("body", "i=1;");
			f2.SetAttribute("name", "g");
			f2.SetAttribute("args", "int arg");
			f2.SetAttribute("body", "y=1;");
			b.SetAttribute("funcs", f1);
			b.SetAttribute("funcs", f2);
			string expecting = ""
				+ "#include <stdio.h>" + NL 
				+ "public void f();" + NL 
				+ "public void g(int arg);" + NL 
				+ "public void f() {i=1;}" + NL 
				+ "public void g(int arg) {y=1;}";
			Assert.AreEqual(expecting, b.ToString());
		}
		
		[Test] public virtual void  testComplicatedIndirectTemplateApplication()
		{
			string templates = ""
				+ "group Java;" + NL 
				+ "" + NL 
				+ "file(variables) ::= <<" + "<variables:{ v | <v.decl:(v.format)()>}; separator=\"\\n\">" + NL 
				+ ">>" + NL 
				+ "intdecl(decl) ::= \"int <decl.name> = 0;\"" + NL 
				+ "intarray(decl) ::= \"int[] <decl.name> = null;\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate f = group.GetInstanceOf("file");
			f.SetAttribute("variables.{decl,format}", new Decl("i", "int"), "intdecl");
			f.SetAttribute("variables.{decl,format}", new Decl("a", "int-array"), "intarray");
			string expecting = ""
				+ "int i = 0;" + NL 
				+ "int[] a = null;";
			Assert.AreEqual(expecting, f.ToString());
		}
		
		[Test] public virtual void  testIndirectTemplateApplication()
		{
			string templates = ""
				+ "group dork;" + NL 
				+ "" + NL 
				+ "test(name) ::= <<" + "<(name)()>" + NL 
				+ ">>" + NL 
				+ "first() ::= \"the first\"" + NL 
				+ "second() ::= \"the second\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate f = group.GetInstanceOf("test");
			f.SetAttribute("name", "first");
			string expecting = "the first";
			Assert.AreEqual(expecting, f.ToString());
		}
		
		[Test] public virtual void  testIndirectTemplateWithArgsApplication()
		{
			string templates = ""
				+ "group dork;" + NL 
				+ "" + NL 
				+ "test(name) ::= <<" + "<(name)(a=\"foo\")>" + NL 
				+ ">>" + NL 
				+ "first(a) ::= \"the first: <a>\"" + NL 
				+ "second(a) ::= \"the second <a>\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate f = group.GetInstanceOf("test");
			f.SetAttribute("name", "first");
			string expecting = "the first: foo";
			Assert.AreEqual(expecting, f.ToString());
		}
		
		[Test] public virtual void  testNullIndirectTemplateApplication()
		{
			string templates = ""
				+ "group dork;" + NL 
				+ "" + NL 
				+ "test(names) ::= <<" + "<names:(ind)()>" + NL 
				+ ">>" + NL 
				+ "ind() ::= \"[<it>]\"" + NL;
			;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate f = group.GetInstanceOf("test");
			f.SetAttribute("names", "me");
			f.SetAttribute("names", "you");
			string expecting = "";
			Assert.AreEqual(expecting, f.ToString());
		}
		
		[Test] public virtual void  testNullIndirectTemplate()
		{
			string templates = ""
				+ "group dork;" + NL 
				+ "" + NL 
				+ "test(name) ::= <<" + "<(name)()>" + NL 
				+ ">>" + NL 
				+ "first() ::= \"the first\"" + NL 
				+ "second() ::= \"the second\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate f = group.GetInstanceOf("test");
			string expecting = "";
			Assert.AreEqual(expecting, f.ToString());
		}
		
		[Test] public virtual void  testHashMapPropertyFetch()
		{
			StringTemplate a = new StringTemplate("$stuff.prop$");
			Hashtable map = new Hashtable();
			a.SetAttribute("stuff", map);
			map["prop"] = "Terence";
			string results = a.ToString();
			string expecting = "Terence";
			Assert.AreEqual(expecting, results);
		}
		
		[Test] public virtual void  testHashMapPropertyFetchEmbeddedStringTemplate()
		{
			StringTemplate a = new StringTemplate("$stuff.prop$");
			Hashtable map = new Hashtable();
			a.SetAttribute("stuff", map);
			a.SetAttribute("title", "ST rocks");
			map["prop"] = new StringTemplate("embedded refers to $title$");
			string results = a.ToString();
			string expecting = "embedded refers to ST rocks";
			Assert.AreEqual(expecting, results);
		}
		
		[Test] public virtual void  testEmbeddedComments()
		{
			StringTemplate st = new StringTemplate("Foo $! ignore !$bar" + NL);
			string expecting = "Foo bar" + NL;
			string result = st.ToString();
			Assert.AreEqual(expecting, result);
			
			st = new StringTemplate(""
				+ "Foo $! ignore" + NL 
				+ " and a line break!$" + NL 
				+ "bar" + NL);
			expecting = ""
				+ "Foo " + NL 
				+ "bar" + NL;
			result = st.ToString();
			Assert.AreEqual(expecting, result);
			
			st = new StringTemplate(""
				+ "$! start of line $ and $! ick" + NL 
				+ "!$boo" + NL);
			expecting = "boo" + NL;
			result = st.ToString();
			Assert.AreEqual(expecting, result);
			
			st = new StringTemplate(""
				+ "$! start of line !$" + NL 
				+ "$! another to ignore !$" + NL 
				+ "$! ick" + NL 
				+ "!$boo" + NL);
			expecting = "boo" + NL;
			result = st.ToString();
			Assert.AreEqual(expecting, result);
			
			st = new StringTemplate(""
				+ "$! back !$$! to back !$" + NL 
				+ "$! ick" + NL 
				+ "!$boo" + NL);
			expecting = NL 
				+ "boo" + NL;
			result = st.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testEmbeddedCommentsAngleBracketed()
		{
			StringTemplate st = new StringTemplate("Foo <! ignore !>bar" + NL, typeof(AngleBracketTemplateLexer));
			string expecting = "Foo bar" + NL;
			string result = st.ToString();
			Assert.AreEqual(expecting, result);
			
			st = new StringTemplate(""
				+ "Foo <! ignore" + NL 
				+ " and a line break!>" + NL 
				+ "bar" + NL, 
				typeof(AngleBracketTemplateLexer));
			expecting = ""
				+ "Foo " + NL 
				+ "bar" + NL;
			result = st.ToString();
			Assert.AreEqual(expecting, result);
			
			st = new StringTemplate("<! start of line $ and <! ick" + NL + "!>boo" + NL, typeof(AngleBracketTemplateLexer));
			expecting = "boo" + NL;
			result = st.ToString();
			Assert.AreEqual(expecting, result);
			
			st = new StringTemplate("<! start of line !>" + "<! another to ignore !>" + "<! ick" + NL + "!>boo" + NL, typeof(AngleBracketTemplateLexer));
			expecting = "boo" + NL;
			result = st.ToString();
			//System.out.println(result);
			Assert.AreEqual(expecting, result);
			
			st = new StringTemplate("<! back !><! to back !>" + NL + "<! ick" + NL + "!>boo" + NL, typeof(AngleBracketTemplateLexer));
			expecting = NL + "boo" + NL;
			result = st.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testCharLiterals()
		{
			StringTemplate st = new StringTemplate("Foo <\\n><\\t> bar" + NL, typeof(AngleBracketTemplateLexer));
			string expecting = "Foo " + NL 
				+ "\t bar" + NL;
			string result = st.ToString();
			Assert.AreEqual(expecting, result);
			
			st = new StringTemplate("Foo $\\n$$\\t$ bar" + NL);
			expecting = "Foo " + NL 
				+ "\t bar" + NL;
			result = st.ToString();
			Assert.AreEqual(expecting, result);
			
			st = new StringTemplate("Foo$\\ $bar$\\n$");
			expecting = "Foo bar" + NL;
			result = st.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testEmptyIteratedValueGetsSeparator()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			IStringTemplateErrorListener errors = new ErrorBuffer();
			group.ErrorListener = errors;
			StringTemplate t = new StringTemplate(group, "$names; separator=\",\"$");
			t.SetAttribute("names", "Terence");
			t.SetAttribute("names", "");
			t.SetAttribute("names", "");
			t.SetAttribute("names", "Tom");
			t.SetAttribute("names", "Frank");
			t.SetAttribute("names", "");
			// empty values get separator still
			string expecting = "Terence,,,Tom,Frank,";
			string result = t.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testEmptyIteratedConditionalValueGetsNoSeparator()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			IStringTemplateErrorListener errors = new ErrorBuffer();
			group.ErrorListener = errors;
			StringTemplate t = new StringTemplate(group, "$users:{$if(it.ok)$$it.name$$endif$}; separator=\",\"$");
			t.SetAttribute("users.{name,ok}", "Terence", (object) true);
			t.SetAttribute("users.{name,ok}", "Tom", (object) false);
			t.SetAttribute("users.{name,ok}", "Frank", (object) true);
			t.SetAttribute("users.{name,ok}", "Johnny", (object) false);
			// empty conditional values get no separator
			string expecting = "Terence,Frank,"; // haven't solved the last empty value problem yet
			string result = t.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testEmptyIteratedConditionalWithElseValueGetsSeparator()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			IStringTemplateErrorListener errors = new ErrorBuffer();
			group.ErrorListener = errors;
			StringTemplate t = new StringTemplate(group, "$users:{$if(it.ok)$$it.name$$else$$endif$}; separator=\",\"$");
			t.SetAttribute("users.{name,ok}", "Terence", (object) true);
			t.SetAttribute("users.{name,ok}", "Tom", (object) false);
			t.SetAttribute("users.{name,ok}", "Frank", (object) true);
			t.SetAttribute("users.{name,ok}", "Johnny", (object) false);
			// empty conditional values get no separator
			string expecting = "Terence,,Frank,"; // haven't solved the last empty value problem yet
			string result = t.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testWhiteSpaceAtEndOfTemplate()
		{
			StringTemplateGroup group = new StringTemplateGroup("group");
			StringTemplate pageST = group.GetInstanceOf("tests/page");
			StringTemplate listST = group.GetInstanceOf("tests/users_list");
			// users.list references row.st which has a single blank line at the end.
			// I.e., there are 2 \n in a row at the end
			// ST should eat all whitespace at end
			listST.SetAttribute("users", new Connector());
			listST.SetAttribute("users", new Connector2());
			pageST.SetAttribute("title", "some title");
			pageST.SetAttribute("body", listST);
			string expecting = "some title" + NL + "Terence parrt@jguru.comTom tombu@jguru.com";
			string result = pageST.ToString();
			//.Replace(Environment.NewLine, "\n") below mitigates against failure due to line-ending differences
			Assert.AreEqual(expecting, result.Replace(Environment.NewLine, "\n"));
		}
		
		[Test] public virtual void  testSizeZeroButNonNullListGetsNoOutput()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			IStringTemplateErrorListener errors = new ErrorBuffer();
			group.ErrorListener = errors;
			StringTemplate t = new StringTemplate(group,
				"begin\n"
				+ "$duh.users:{name: $it$}; separator=\", \"$\n"
				+ "end\n");
			t.SetAttribute("duh", new Duh());
			string expecting = "begin\nend\n";
			string result = t.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void testNullListGetsNoOutput()
		{
			StringTemplateGroup group =
				new StringTemplateGroup("test");
			IStringTemplateErrorListener errors = new ErrorBuffer();
			group.ErrorListener = errors;
			StringTemplate t = new StringTemplate(group,
				"begin\n" +
				"$users:{name: $it$}; separator=\", \"$\n" +
				"end\n");
			//t.setAttribute("users", new Duh());
			string expecting="begin\nend\n";
			string result = t.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public virtual void testEmptyListGetsNoOutput()
		{
			StringTemplateGroup group =
				new StringTemplateGroup("test");
			IStringTemplateErrorListener errors = new ErrorBuffer();
			group.ErrorListener = errors;
			StringTemplate t = new StringTemplate(group,
				"begin\n" +
				"$users:{name: $it$}; separator=\", \"$\n" +
				"end\n");
			t.SetAttribute("users", new ArrayList());
			string expecting = "begin\nend\n";
			string result = t.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public virtual void testEmptyListNoIteratorGetsNoOutput()
		{
			StringTemplateGroup group =
				new StringTemplateGroup("test");
			IStringTemplateErrorListener errors = new ErrorBuffer();
			group.ErrorListener = errors;
			StringTemplate t = new StringTemplate(group,
				"begin\n" +
				"$users; separator=\", \"$\n" +
				"end\n");
			t.SetAttribute("users", new ArrayList());
			string expecting = "begin\nend\n";
			string result = t.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public virtual void testEmptyExprAsFirstLineGetsNoOutput()
		{
			StringTemplateGroup group =
				new StringTemplateGroup("test");
			IStringTemplateErrorListener errors = new ErrorBuffer();
			group.ErrorListener = errors;
			group.DefineTemplate("bold", "<b>$it$</b>");
			StringTemplate t = new StringTemplate(group,
				"$users$\n" +
				"end\n");
			string expecting = "end\n";
			string result = t.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public virtual void  testSizeZeroOnLineByItselfGetsNoOutput()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			IStringTemplateErrorListener errors = new ErrorBuffer();
			group.ErrorListener = errors;
			StringTemplate t = new StringTemplate(group, "begin\n" + "$name$\n" + "$users:{name: $it$}$\n" + "$users:{name: $it$}; separator=\", \"$\n" + "end\n");
			string expecting = "begin\nend\n";
			string result = t.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testSizeZeroOnLineWithIndentGetsNoOutput()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			IStringTemplateErrorListener errors = new ErrorBuffer();
			group.ErrorListener = errors;
			StringTemplate t = new StringTemplate(group, "begin\n" + "  $name$\n" + "	$users:{name: $it$}$\n" + "	$users:{name: $it$$\\n$}$\n" + "end\n");
			string expecting = "begin\nend\n";
			string result = t.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testSimpleAutoIndent()
		{
			StringTemplate a = new StringTemplate("$title$: {\n" + "	$name; separator=\"\n\"$\n" + "}");
			a.SetAttribute("title", "foo");
			a.SetAttribute("name", "Terence");
			a.SetAttribute("name", "Frank");
			string results = a.ToString();
			string expecting = "foo: {\n" + "	Terence\n" + "	Frank\n" + "}";
			Assert.AreEqual(expecting, results);
		}
		
		[Test] public virtual void  testComputedPropertyName()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			IStringTemplateErrorListener errors = new ErrorBuffer();
			group.ErrorListener = errors;
			StringTemplate t = new StringTemplate(group, "variable property $propName$=$v.(propName)$");
			t.SetAttribute("v", new Decl("i", "int"));
			t.SetAttribute("propName", "type");
			string expecting = "variable property type=int";
			string result = t.ToString();
			Assert.AreEqual(errors.ToString(), "");
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testNonNullButEmptyIteratorTestsFalse()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate t = new StringTemplate(group, "$if(users)$\n" + "Users: $users:{$it.name$ }$\n" + "$endif$");
			t.SetAttribute("users", new ArrayList());
			string expecting = "";
			string result = t.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testDoNotInheritAttributesThroughFormalArgs()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "method(name) ::= \"<stat()>\"" + NL 
				+ "stat(name) ::= \"x=y; // <name>\"" + NL;
			// name is not visible in stat because of the formal arg called name.
			// somehow, it must be set.
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate b = group.GetInstanceOf("method");
			b.SetAttribute("name", "foo");
			string expecting = "x=y; // ";
			string result = b.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testArgEvaluationContext()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "method(name) ::= \"<stat(name=name)>\"" + NL 
				+ "stat(name) ::= \"x=y; // <name>\"" + NL;
			// attribute name is not visible in stat because of the formal
			// arg called name in template stat.  However, we can set it's value
			// with an explicit name=name.  This looks weird, but makes total
			// sense as the rhs is evaluated in the context of method and the lhs
			// is evaluated in the context of stat's arg list.
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate b = group.GetInstanceOf("method");
			b.SetAttribute("name", "foo");
			string expecting = "x=y; // foo";
			string result = b.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testPassThroughAttributes()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "method(name) ::= \"<stat(...)>\"" + NL 
				+ "stat(name) ::= \"x=y; // <name>\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate b = group.GetInstanceOf("method");
			b.SetAttribute("name", "foo");
			string expecting = "x=y; // foo";
			string result = b.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testPassThroughAttributes2()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "method(name) ::= <<" + NL 
				+ "<stat(value=\"34\",...)>" + NL 
				+ ">>" + NL 
				+ "stat(name,value) ::= \"x=<value>; // <name>\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate b = group.GetInstanceOf("method");
			b.SetAttribute("name", "foo");
			string expecting = "x=34; // foo";
			string result = b.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testDefaultArgument()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "method(name) ::= <<" + NL 
				+ "<stat(...)>" + NL 
				+ ">>" + NL 
				+ "stat(name,value=\"99\") ::= \"x=<value>; // <name>\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate b = group.GetInstanceOf("method");
			b.SetAttribute("name", "foo");
			string expecting = "x=99; // foo";
			string result = b.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testDefaultArgument2()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "stat(name,value=\"99\") ::= \"x=<value>; // <name>\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate b = group.GetInstanceOf("stat");
			b.SetAttribute("name", "foo");
			string expecting = "x=99; // foo";
			string result = b.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testDefaultArgumentAsTemplate()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "method(name,size) ::= <<" + NL 
				+ "<stat(...)>" + NL 
				+ ">>" + NL 
				+ "stat(name,value={<name>}) ::= \"x=<value>; // <name>\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate b = group.GetInstanceOf("method");
			b.SetAttribute("name", "foo");
			b.SetAttribute("size", "2");
			string expecting = "x=foo; // foo";
			string result = b.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testDefaultArgumentAsTemplate2()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "method(name,size) ::= <<" + NL 
				+ "<stat(...)>" + NL 
				+ ">>" + NL 
				+ "stat(name,value={ [<name>] }) ::= \"x=<value>; // <name>\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate b = group.GetInstanceOf("method");
			b.SetAttribute("name", "foo");
			b.SetAttribute("size", "2");
			string expecting = "x= [foo] ; // foo";
			string result = b.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testDoNotUseDefaultArgument()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "method(name) ::= <<" + NL 
				+ "<stat(value=\"34\",...)>" + NL 
				+ ">>" + NL 
				+ "stat(name,value=\"99\") ::= \"x=<value>; // <name>\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate b = group.GetInstanceOf("method");
			b.SetAttribute("name", "foo");
			string expecting = "x=34; // foo";
			string result = b.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testArgumentsAsTemplates()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "method(name,size) ::= <<" + NL 
				+ "<stat(value={<size>})>" + NL 
				+ ">>" + NL 
				+ "stat(value) ::= \"x=<value>;\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate b = group.GetInstanceOf("method");
			b.SetAttribute("name", "foo");
			b.SetAttribute("size", "34");
			string expecting = "x=34;";
			string result = b.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testArgumentsAsTemplatesDefaultDelimiters()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "method(name,size) ::= <<" + NL 
				+ "$stat(value={$size$})$" + NL 
				+ ">>" + NL 
				+ "stat(value) ::= \"x=$value$;\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer));
			StringTemplate b = group.GetInstanceOf("method");
			b.SetAttribute("name", "foo");
			b.SetAttribute("size", "34");
			string expecting = "x=34;";
			string result = b.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testDefaultArgsWhenNotInvoked()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "b(name=\"foo\") ::= \".<name>.\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate b = group.GetInstanceOf("b");
			string expecting = ".foo.";
			string result = b.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testRendererForST()
		{
			StringTemplate st = new StringTemplate("date: <created>", typeof(AngleBracketTemplateLexer));
			st.SetAttribute("created", new DateTime(2005, 7, 5));
			st.RegisterAttributeRenderer(typeof(DateTime), new DateRenderer());
			string expecting = "date: 2005.07.05";
			string result = st.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public void testEmbeddedRendererSeesEnclosing()
		{
			// st is embedded in outer; set renderer on outer, st should
			// still see it.
			StringTemplate outer = new StringTemplate("X: <x>", typeof(AngleBracketTemplateLexer));
			StringTemplate st = new StringTemplate("date: <created>", typeof(AngleBracketTemplateLexer));
			st.SetAttribute("created", new DateTime(2005, 07, 05));
			outer.SetAttribute("x", st);
			outer.RegisterAttributeRenderer(typeof(DateTime), new DateRenderer());
			string expecting = "X: date: 2005.07.05";
			string result = outer.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public virtual void testEmbeddedRendererSeesEnclosing_UsingGroupFile()
		{
			string templates = ""
				+ "group Messages;" + NL
				+ "PrintMessage(date) ::= <<" + NL
				+ "Remember this date: $PrintDate(date=date)$" + NL
				+ ">>" + NL
				+ "PrintDate(date) ::= <<$date$>>" + NL
				;
			StringTemplateGroup group = new StringTemplateGroup(
				new StringReader(templates), typeof(DefaultTemplateLexer)
				);
			StringTemplate printMessageTemplate = group.GetInstanceOf("PrintMessage");
			printMessageTemplate.SetAttribute("date", new DateTime(2005, 7, 5));
			group.RegisterAttributeRenderer(typeof(DateTime), new DateRenderer2());
			printMessageTemplate.RegisterAttributeRenderer(typeof(DateTime), new DateRenderer());
			string expecting = "Remember this date: 2005.07.05";
			string result = printMessageTemplate.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public virtual void  testRendererForGroup()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "dateThing(created) ::= \"date: <created>\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate st = group.GetInstanceOf("dateThing");
			st.SetAttribute("created", new DateTime(2005, 7, 5));
			group.RegisterAttributeRenderer(typeof(DateTime), new DateRenderer());
			string expecting = "date: 2005.07.05";
			string result = st.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testOverriddenRenderer()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "dateThing(created) ::= \"date: <created>\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate st = group.GetInstanceOf("dateThing");
			st.SetAttribute("created", new DateTime(2005, 7, 5));
			group.RegisterAttributeRenderer(typeof(DateTime), new DateRenderer());
			st.RegisterAttributeRenderer(typeof(DateTime), new DateRenderer2());
			string expecting = "date: 07/05/2005";
			string result = st.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testMap()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "typeInit ::= [\"int\":\"0\", \"float\":\"0.0\"] " + NL 
				+ "var(type,name) ::= \"<type> <name> = <typeInit.(type)>;\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate st = group.GetInstanceOf("var");
			st.SetAttribute("type", "int");
			st.SetAttribute("name", "x");
			string expecting = "int x = 0;";
			string result = st.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testMapValuesAreTemplates()
		{
			string templates = ""
				+ "group test;" + NL
				+ "typeInit ::= [\"int\":\"0<w>\", \"float\":\"0.0<w>\"] " + NL
				+ "var(type,w,name) ::= \"<type> <name> = <typeInit.(type)>;\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate st = group.GetInstanceOf("var");
			st.SetAttribute("w", "L");
			st.SetAttribute("type", "int");
			st.SetAttribute("name", "x");
			string expecting = "int x = 0L;";
			string result = st.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testMapMissingDefaultValueIsEmpty()
		{
			string templates = ""
				+ "group test;" + NL
				+ "typeInit ::= [\"int\":\"0\", \"float\":\"0.0\"] " + NL
				+ "var(type,w,name) ::= \"<type> <name> = <typeInit.(type)>;\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate st = group.GetInstanceOf("var");
			st.SetAttribute("w", "L");
			st.SetAttribute("type", "double"); // double not in typeInit map
			st.SetAttribute("name", "x");
			string expecting = "double x = ;"; // weird, but tests default value is key
			string result = st.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public virtual void  testMapHiddenByFormalArg()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "typeInit ::= [\"int\":\"0\", \"float\":\"0.0\"] " + NL 
				+ "var(typeInit,type,name) ::= \"<type> <name> = <typeInit.(type)>;\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate st = group.GetInstanceOf("var");
			st.SetAttribute("type", "int");
			st.SetAttribute("name", "x");
			string expecting = "int x = ;";
			string result = st.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testMapEmptyValueAndAngleBracketStrings()
		{
			string templates = ""
				+ "group test;" + NL
				+ "typeInit ::= [\"int\":\"0\", \"float\":, \"double\":<<0.0L>>] " + NL
				+ "var(type,name) ::= \"<type> <name> = <typeInit.(type)>;\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate st = group.GetInstanceOf("var");
			st.SetAttribute("type", "float");
			st.SetAttribute("name", "x");
			string expecting = "float x = ;";
			string result = st.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public virtual void  testMapDefaultValue()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "typeInit ::= [\"int\":\"0\", \"default\":\"null\"] " + NL 
				+ "var(type,name) ::= \"<type> <name> = <typeInit.(type)>;\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate st = group.GetInstanceOf("var");
			st.SetAttribute("type", "UserRecord");
			st.SetAttribute("name", "x");
			string expecting = "UserRecord x = null;";
			string result = st.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testMapEmptyDefaultValue()
		{
			string templates = ""
				+ "group test;" + NL
				+ "typeInit ::= [\"int\":\"0\", \"default\":] " + NL
				+ "var(type,name) ::= \"<type> <name> = <typeInit.(type)>;\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate st = group.GetInstanceOf("var");
			st.SetAttribute("type", "UserRecord");
			st.SetAttribute("name", "x");
			String expecting = "UserRecord x = ;";
			String result = st.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testMapEmptyDefaultValueIsKey()
		{
			string templates = ""
				+ "group test;" + NL
				+ "typeInit ::= [\"int\":\"0\", \"default\":key] " + NL
				+ "var(type,name) ::= \"<type> <name> = <typeInit.(type)>;\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate st = group.GetInstanceOf("var");
			st.SetAttribute("type", "UserRecord");
			st.SetAttribute("name", "x");
			string expecting = "UserRecord x = UserRecord;";
			string result = st.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public virtual void  testMapViaEnclosingTemplates()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "typeInit ::= [\"int\":\"0\", \"float\":\"0.0\"] " + NL 
				+ "intermediate(type,name) ::= \"<var(...)>\"" + NL 
				+ "var(type,name) ::= \"<type> <name> = <typeInit.(type)>;\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate st = group.GetInstanceOf("intermediate");
			st.SetAttribute("type", "int");
			st.SetAttribute("name", "x");
			string expecting = "int x = 0;";
			string result = st.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testMapViaEnclosingTemplates2()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "typeInit ::= [\"int\":\"0\", \"float\":\"0.0\"] " + NL 
				+ "intermediate(stuff) ::= \"<stuff>\"" + NL 
				+ "var(type,name) ::= \"<type> <name> = <typeInit.(type)>;\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate interm = group.GetInstanceOf("intermediate");
			StringTemplate var = group.GetInstanceOf("var");
			var.SetAttribute("type", "int");
			var.SetAttribute("name", "x");
			interm.SetAttribute("stuff", var);
			string expecting = "int x = 0;";
			string result = interm.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testEmptyGroupTemplate()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "foo() ::= \"\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate a = group.GetInstanceOf("foo");
			string expecting = "";
			string result = a.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public void testEmptyStringAndEmptyAnonTemplateAsParameterUsingAngleBracketLexer()
		{
			string templates = ""
				+ "group test;" + NL
				+ "top() ::= <<<x(a=\"\", b={})\\>>>"+ NL
				+ "x(a,b) ::= \"a=<a>, b=<b>\""+ NL;
			;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate a = group.GetInstanceOf("top");
			string expecting = "a=, b=";
			string result = a.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public void testEmptyStringAndEmptyAnonTemplateAsParameterUsingDollarLexer()
		{
			string templates = ""
				+ "group test;" + NL
				+ "top() ::= <<$x(a=\"\", b={})$>>"+ NL
				+ "x(a,b) ::= \"a=$a$, b=$b$\""+ NL;
			;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer));
			StringTemplate a = group.GetInstanceOf("top");
			string expecting = "a=, b=";
			string result = a.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public void  test8BitEuroChars()
		{
			StringTemplate e = new StringTemplate("Danish: Å char");
			e = e.GetInstanceOf();
			string expecting = "Danish: Å char";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public void  testFirstOp()
		{
			StringTemplate e = new StringTemplate("$first(names)$");
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			e.SetAttribute("names", "Sriram");
			string expecting = "Ter";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testRestOp()
		{
			StringTemplate e = new StringTemplate("$rest(names); separator=\", \"$");
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			e.SetAttribute("names", "Sriram");
			string expecting = "Tom, Sriram";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testLastOp()
		{
			StringTemplate e = new StringTemplate("$last(names)$");
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			e.SetAttribute("names", "Sriram");
			string expecting = "Sriram";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testCombinedOp()
		{
			// replace first of yours with first of mine
			StringTemplate e = new StringTemplate("$[first(mine),rest(yours)]; separator=\", \"$");
			e = e.GetInstanceOf();
			e.SetAttribute("mine", "1");
			e.SetAttribute("mine", "2");
			e.SetAttribute("mine", "3");
			e.SetAttribute("yours", "a");
			e.SetAttribute("yours", "b");
			string expecting = "1, b";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testCatListAndSingleAttribute()
		{
			// replace first of yours with first of mine
			StringTemplate e = new StringTemplate("$[mine,yours]; separator=\", \"$");
			e = e.GetInstanceOf();
			e.SetAttribute("mine", "1");
			e.SetAttribute("mine", "2");
			e.SetAttribute("mine", "3");
			e.SetAttribute("yours", "a");
			string expecting = "1, 2, 3, a";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testCatListAndEmptyAttributes()
		{
			// + is overloaded to be cat strings and cat lists so the
			// two operands (from left to right) determine which way it
			// goes.  In this case, x+mine is a list so everything from their
			// to the right becomes list cat.
			StringTemplate e = new StringTemplate("$[x,mine,y,yours,z]; separator=\", \"$");
			e = e.GetInstanceOf();
			e.SetAttribute("mine", "1");
			e.SetAttribute("mine", "2");
			e.SetAttribute("mine", "3");
			e.SetAttribute("yours", "a");
			string expecting = "1, 2, 3, a";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testNestedOp()
		{
			StringTemplate e = new StringTemplate("$first(rest(names))$");
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			e.SetAttribute("names", "Sriram");
			string expecting = "Tom";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testFirstWithOneAttributeOp()
		{
			StringTemplate e = new StringTemplate("$first(names)$");
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			string expecting = "Ter";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testLastWithOneAttributeOp()
		{
			StringTemplate e = new StringTemplate("$last(names)$");
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			string expecting = "Ter";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testLastWithLengthOneListAttributeOp()
		{
			StringTemplate e = new StringTemplate("$last(names)$");
			e = e.GetInstanceOf();
			e.SetAttribute("names", new ArrayList(new object[] { "Ter" }));
			string expecting = "Ter";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testRestWithOneAttributeOp()
		{
			StringTemplate e = new StringTemplate("$rest(names)$");
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			string expecting = "";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testRestWithLengthOneListAttributeOp()
		{
			StringTemplate e = new StringTemplate("$rest(names)$");
			e = e.GetInstanceOf();
			e.SetAttribute("names", new ArrayList(new object[] { "Ter" }));
			string expecting = "";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public void testRepeatedRestOp()
		{
			StringTemplate e = new StringTemplate(
				"$rest(names)$, $rest(names)$" // gets 2nd element
				);
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			string expecting = "Tom, Tom";
			Assert.AreEqual(expecting, e.ToString());
		}

		/** BUG!  Fix this.  Iterator is not reset from first to second $x$
		 *  Either reset the iterator or pass an attribute that knows to get
		 *  the iterator each time.  Seems like first, tail do not
		 *  have same problem as they yield objects.
		 *
		 *  Maybe make a RestIterator like I have CatIterator.
		 */
		[Test] public void testRepeatedRestOpAsArg()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "root(names) ::= \"$other(rest(names))$\""+ NL 
				+ "other(x) ::= \"$x$, $x$\""+ NL
				;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer));
			StringTemplate e = group.GetInstanceOf("root");
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			string expecting = "Tom, Tom";
			Assert.AreEqual(expecting, e.ToString());
		}

		[Test] public void testIncomingLists()
		{
			StringTemplate e = new StringTemplate(
				"$rest(names)$, $rest(names)$" // gets 2nd element
				);
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			string expecting = "Tom, Tom";
			Assert.AreEqual(expecting, e.ToString());
		}

		[Test] public void testIncomingListsAreNotModified()
		{
			StringTemplate e = new StringTemplate(
				"$names; separator=\", \"$" // gets 2nd element
				);
			e = e.GetInstanceOf();
			IList names = new ArrayList();
			names.Add("Ter");
			names.Add("Tom");
			e.SetAttribute("names", names);
			e.SetAttribute("names", "Sriram");
			string expecting = "Ter, Tom, Sriram";
			Assert.AreEqual(expecting, e.ToString());

			Assert.AreEqual(names.Count, 2);
			Assert.AreEqual(names[0], "Ter");
			Assert.AreEqual(names[1], "Tom");
		}

		[Test] public void testIncomingListsAreNotModified2() 
		{
			StringTemplate e = new StringTemplate(
				"$names; separator=\", \"$" // gets 2nd element
				);
			e = e.GetInstanceOf();
			IList names = new ArrayList();
			names.Add("Ter");
			names.Add("Tom");
			e.SetAttribute("names", "Sriram"); // single element first now
			e.SetAttribute("names", names);
			string expecting = "Sriram, Ter, Tom";
			Assert.AreEqual(expecting, e.ToString());

			Assert.AreEqual(names.Count, 2);
			Assert.AreEqual(names[0], "Ter");
			Assert.AreEqual(names[1], "Tom");
		}

		[Test] public void testIncomingArraysAreOk()
		{
			StringTemplate e = new StringTemplate(
				"$names; separator=\", \"$" // gets 2nd element
				);
			e = e.GetInstanceOf();
			e.SetAttribute("names", new String[] {"Ter","Tom"});
			e.SetAttribute("names", "Sriram");
			string expecting = "Ter, Tom, Sriram";
			Assert.AreEqual(expecting, e.ToString());
		}

		[Test] public virtual void  testApplyTemplateWithSingleFormalArgs()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "test(names) ::= <<<names:bold(item=it); separator=\", \"> >>" + NL 
				+ "bold(item) ::= <<*<item>*>>" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate e = group.GetInstanceOf("test");
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			string expecting = "*Ter*, *Tom* ";
			string result = e.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testApplyTemplateWithNoFormalArgs()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "test(names) ::= <<<names:bold(); separator=\", \"> >>" + NL 
				+ "bold() ::= <<*<it>*>>" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate e = group.GetInstanceOf("test");
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			string expecting = "*Ter*, *Tom* ";
			string result = e.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testAnonTemplateArgs()
		{
			StringTemplate e = new StringTemplate("$names:{n| $n$}; separator=\", \"$");
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			string expecting = "Ter, Tom";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testAnonTemplateArgs2()
		{
			StringTemplate e = new StringTemplate("$names:{n| .$n$.}:{ n | _$n$_}; separator=\", \"$");
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			string expecting = "_.Ter._, _.Tom._";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[ExpectedException(
			 typeof(InvalidOperationException), 
			 "no such attribute: it in template context [anonymous anonymous]")
		]
		[Test] public void testAnonTemplateWithArgHasNoITArg()
		{
			StringTemplate e = new StringTemplate(
				"$names:{n| $n$:$it$}; separator=\", \"$"
				);
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			string s = e.ToString();
		}

		[Test] public virtual void  testFirstWithCatAttribute()
		{
			StringTemplate e = new StringTemplate("$first([names,phones])$");
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			e.SetAttribute("phones", "1");
			e.SetAttribute("phones", "2");
			string expecting = "Ter";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testJustCat()
		{
			StringTemplate e = new StringTemplate("$[names,phones]$");
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			e.SetAttribute("phones", "1");
			e.SetAttribute("phones", "2");
			string expecting = "TerTom12";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testCat2Attributes()
		{
			StringTemplate e = new StringTemplate("$[names,phones]; separator=\", \"$");
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			e.SetAttribute("phones", "1");
			e.SetAttribute("phones", "2");
			string expecting = "Ter, Tom, 1, 2";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testCat2AttributesWithApply()
		{
			StringTemplate e = new StringTemplate("$[names,phones]:{a|$a$.}$");
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			e.SetAttribute("phones", "1");
			e.SetAttribute("phones", "2");
			string expecting = "Ter.Tom.1.2.";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testCat3Attributes()
		{
			StringTemplate e = new StringTemplate("$[names,phones,salaries]; separator=\", \"$");
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			e.SetAttribute("phones", "1");
			e.SetAttribute("phones", "2");
			e.SetAttribute("salaries", "big");
			e.SetAttribute("salaries", "huge");
			string expecting = "Ter, Tom, 1, 2, big, huge";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testListAsTemplateArgument()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "test(names,phones) ::= \"<foo([names,phones])>\"" + NL 
				+ "foo(items) ::= \"<items:{a | *<a>*}>\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate e = group.GetInstanceOf("test");
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			e.SetAttribute("phones", "1");
			e.SetAttribute("phones", "2");
			string expecting = "*Ter**Tom**1**2*";
			string result = e.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testSingleExprTemplateArgument()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "test(name) ::= \"<bold(name)>\"" + NL 
				+ "bold(item) ::= \"*<item>*\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate e = group.GetInstanceOf("test");
			e.SetAttribute("name", "Ter");
			string expecting = "*Ter*";
			string result = e.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testSingleExprTemplateArgumentInApply()
		{
			// when you specify a single arg on a template application
			// it overrides the setting of the iterated value "it" to that
			// same single formal arg.  Your arg hides the implicitly set "it".
			string templates = ""
				+ "group test;" + NL 
				+ "test(names,x) ::= \"<names:bold(x)>\"" + NL 
				+ "bold(item) ::= \"*<item>*\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate e = group.GetInstanceOf("test");
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			e.SetAttribute("x", "ick");
			string expecting = "*ick**ick*";
			string result = e.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testSoleFormalTemplateArgumentInMultiApply()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "test(names) ::= \"<names:bold(),italics()>\"" + NL 
				+ "bold(x) ::= \"*<x>*\"" + NL 
				+ "italics(y) ::= \"_<y>_\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate e = group.GetInstanceOf("test");
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			string expecting = "*Ter*_Tom_";
			string result = e.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testSingleExprTemplateArgumentError()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "test(name) ::= \"<bold(name)>\"" + NL 
				+ "bold(item,ick) ::= \"*<item>*\"" + NL;
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer), errors);
			StringTemplate e = group.GetInstanceOf("test");
			e.SetAttribute("name", "Ter");
			string result = e.ToString();
			string expecting = "template bold must have exactly one formal arg in template context [test <invoke bold arg context>]";
			Assert.AreEqual(expecting, errors.ToString());
		}
		
		[Test] public virtual void  testInvokeIndirectTemplateWithSingleFormalArgs()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "test(templateName,arg) ::= \"<(templateName)(arg)>\"" + NL 
				+ "bold(x) ::= <<*<x>*>>" + NL 
				+ "italics(y) ::= <<_<y>_>>" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate e = group.GetInstanceOf("test");
			e.SetAttribute("templateName", "italics");
			e.SetAttribute("arg", "Ter");
			string expecting = "_Ter_";
			string result = e.ToString();
			Assert.AreEqual(expecting, result);
		}
		
		[Test] public virtual void  testParallelAttributeIteration()
		{
			StringTemplate e = new StringTemplate("$names,phones,salaries:{n,p,s | $n$@$p$: $s$\n}$");
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			e.SetAttribute("phones", "1");
			e.SetAttribute("phones", "2");
			e.SetAttribute("salaries", "big");
			e.SetAttribute("salaries", "huge");
			string expecting = "Ter@1: big" + NL + "Tom@2: huge" + NL;
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public void testParallelAttributeIterationHasI()
		{
			StringTemplate e = new StringTemplate(
				"$names,phones,salaries:{n,p,s | $i0$. $n$@$p$: $s$\n}$"
				);
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			e.SetAttribute("phones", "1");
			e.SetAttribute("phones", "2");
			e.SetAttribute("salaries", "big");
			e.SetAttribute("salaries", "huge");
			string expecting = "0. Ter@1: big" +  NL + "1. Tom@2: huge" + NL;
			Assert.AreEqual(expecting, e.ToString());
		}

		[Test] public virtual void  testParallelAttributeIterationWithDifferentSizes()
		{
			StringTemplate e = new StringTemplate("$names,phones,salaries:{n,p,s | $n$@$p$: $s$}; separator=\", \"$");
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			e.SetAttribute("names", "Sriram");
			e.SetAttribute("phones", "1");
			e.SetAttribute("phones", "2");
			e.SetAttribute("salaries", "big");
			string expecting = "Ter@1: big, Tom@2: , Sriram@: ";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testParallelAttributeIterationWithSingletons()
		{
			StringTemplate e = new StringTemplate("$names,phones,salaries:{n,p,s | $n$@$p$: $s$}; separator=\", \"$");
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			e.SetAttribute("phones", "1");
			e.SetAttribute("salaries", "big");
			string expecting = "Ter@1: big";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public virtual void  testParallelAttributeIterationWithMismatchArgListSizes()
		{
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplate e = new StringTemplate("$names,phones,salaries:{n,p | $n$@$p$}; separator=\", \"$");
			e.ErrorListener = errors;
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Ter");
			e.SetAttribute("names", "Tom");
			e.SetAttribute("phones", "1");
			e.SetAttribute("phones", "2");
			e.SetAttribute("salaries", "big");
			string expecting = "Ter@1, Tom@2";
			Assert.AreEqual(expecting, e.ToString());
			string errorExpecting = "number of arguments [n, p] mismatch between attribute list and anonymous template in context [anonymous]";
			Assert.AreEqual(errorExpecting, errors.ToString());
		}
		
		[Test] public virtual void  testParallelAttributeIterationWithMissingArgs()
		{
			IStringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplate e = new StringTemplate("$names,phones,salaries:{$n$@$p$}; separator=\", \"$");
			e.ErrorListener = errors;
			e = e.GetInstanceOf();
			e.SetAttribute("names", "Tom");
			e.SetAttribute("phones", "2");
			e.SetAttribute("salaries", "big");
			e.ToString(); // generate the error
			string errorExpecting = "missing arguments in anonymous template in context [anonymous]";
			Assert.AreEqual(errorExpecting, errors.ToString());
		}
		
		[Test] public virtual void  testParallelAttributeIterationWithDifferentSizesTemplateRefInsideToo()
		{
			string templates = ""
				+ "group test;" + NL 
				+ "page(names,phones,salaries) ::= " + NL 
				+ "	<<$names,phones,salaries:{n,p,s | $value(n)$@$value(p)$: $value(s)$}; separator=\", \"$>>" + NL 
				+ "value(x=\"n/a\") ::= \"$x$\"" + NL;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(DefaultTemplateLexer));
			StringTemplate p = group.GetInstanceOf("page");
			p.SetAttribute("names", "Ter");
			p.SetAttribute("names", "Tom");
			p.SetAttribute("names", "Sriram");
			p.SetAttribute("phones", "1");
			p.SetAttribute("phones", "2");
			p.SetAttribute("salaries", "big");
			string expecting = "Ter@1: big, Tom@2: n/a, Sriram@n/a: n/a";
			Assert.AreEqual(expecting, p.ToString());
		}
		
		[Test] public virtual void  testAnonTemplateOnLeftOfApply()
		{
			StringTemplate e = new StringTemplate("${foo}:{($it$)}$");
			string expecting = "(foo)";
			Assert.AreEqual(expecting, e.ToString());
		}
		
		[Test] public void testOverrideThroughConditional()
		{
			string templates = ""
				+ "group base;" + NL 
				+ "body(ick) ::= \"<if(ick)>ick<f()><else><f()><endif>\"" 
				+ "f() ::= \"foo\""+ NL
				;
			StringTemplateGroup group = new StringTemplateGroup(
				new StringReader(templates),
				typeof(AngleBracketTemplateLexer));
			string templates2 = ""
				+ "group sub;" + NL 
				+ "f() ::= \"bar\""+ NL
				;
			StringTemplateGroup subgroup = new StringTemplateGroup(
				new StringReader(templates2),
				typeof(AngleBracketTemplateLexer),
				null,
				group);

			StringTemplate b = subgroup.GetInstanceOf("body");
			string expecting ="bar";
			string result = b.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public virtual void  testNonPublicPropertyAccess()
		{
			StringTemplate st = new StringTemplate("$x.age$:$x.IsAdmin$:$x.nickName$:$x.Cool$:$x.bar$:$x.eMail$:$x.FOO$");
			NonPublicPropertyAccessHelper helper = new NonPublicPropertyAccessHelper();
			
			st.SetAttribute("x", helper);
			string expecting = "21:"+FALSE+":Floating Lotus:"+TRUE+":34:some.label@somedomain.com:9";
			Assert.AreEqual(expecting, st.ToString());
		}

		[Test] public void testIndexVar()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate t = new StringTemplate(
				group,
				"$A:{$i$. $it$}; separator=\"\\n\"$"
				);
			t.SetAttribute("A", "parrt");
			t.SetAttribute("A", "tombu");
			string expecting =
				"1. parrt" + NL +
				"2. tombu";
			Assert.AreEqual(expecting, t.ToString());
		}

		[Test] public void testIndex0Var()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate t = new StringTemplate(
				group,
				"$A:{$i0$. $it$}; separator=\"\\n\"$"
				);
			t.SetAttribute("A", "parrt");
			t.SetAttribute("A", "tombu");
			string expecting =
				"0. parrt" + NL +
				"1. tombu";
			Assert.AreEqual(expecting, t.ToString());
		}

		[Test] public void testIndexVarWithMultipleExprs()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate t = new StringTemplate(
				group,
				"$A,B:{a,b|$i$. $a$@$b$}; separator=\"\\n\"$"
				);
			t.SetAttribute("A", "parrt");
			t.SetAttribute("A", "tombu");
			t.SetAttribute("B", "x5707");
			t.SetAttribute("B", "x5000");
			string expecting =
				"1. parrt@x5707" + NL +
				"2. tombu@x5000";
			Assert.AreEqual(expecting, t.ToString());
		}

		[Test] public void testIndex0VarWithMultipleExprs()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate t = new StringTemplate(
				group,
				"$A,B:{a,b|$i0$. $a$@$b$}; separator=\"\\n\"$"
				);
			t.SetAttribute("A", "parrt");
			t.SetAttribute("A", "tombu");
			t.SetAttribute("B", "x5707");
			t.SetAttribute("B", "x5000");
			string expecting =
				"0. parrt@x5707" + NL +
				"1. tombu@x5000";
			Assert.AreEqual(expecting, t.ToString());
		}

		[ExpectedException(typeof(ArgumentException))]
		[Test] public void TestAttributeNameCannotHaveDots()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate t = new StringTemplate(group, "$user.Name$");
			t.SetAttribute("user.Name", "Kunle");
		}

		[Ignore("Should it be possible to directly create STs with dotted names?")]
		[ExpectedException(typeof(ArgumentException))]
		[Test] public void TestTemplateNameCannotHaveDots()
		{
			StringTemplate t = new StringTemplate("user.name");
		}

		[ExpectedException(typeof(ArgumentException))]
		[Test] public void TestTemplateNameCannotHaveDots2()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate t = group.DefineTemplate("user.name", "$person.email$");
		}

		[Ignore("Should group files allow dotted template names? (Regions may be affected?")]
		[ExpectedException(typeof(ArgumentException))]
		[Test] public void TestTemplateNameCannotHaveDots_UsingGroupFile()
		{
			string templates = ""
				+ "group Dotted;" + NL 
				+ "CreateTable(name) ::= <<" + NL
				+ "$user.name(a=name, b=name)$" + NL
				+ ">>" + NL
				+"user.name(a , b) ::= <<" + NL
				+ "  $a$" + NL
				+ ">>" + NL
				;
			StringTemplateGroup group = new StringTemplateGroup(
				new StringReader(templates), typeof(DefaultTemplateLexer));
			StringTemplate st = group.GetInstanceOf("CreateTable");
		}

		#region Tests for Issues reported by Users

		/// <summary>
		/// Used to give error message: "action parse error in group General line 3"
		/// Reported by: Vlad Chistiakov (20060313_04.28.34)
		/// </summary>
		[Test] public virtual void  testSubTemplateWithTwoParameters()
		{
			string templates = ""
				+ "group General;" + NL 
				+ "CreateTable(name) ::= <<" + NL
				+ "$Tset(a=name, b=name)$" + NL
				+ ">>" + NL
				+"Tset(a , b) ::= <<" + NL
				+ "  $a$" + NL
				+ ">>" + NL
				;
			StringTemplateGroup group = new StringTemplateGroup(
				new StringReader(templates), typeof(DefaultTemplateLexer));
			StringTemplate createTableTemplate = group.GetInstanceOf("CreateTable");
			createTableTemplate.SetAttribute("name", "A");
			string expecting = "  A";
			string result = createTableTemplate.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public virtual void  testSubTemplateWithTwoParametersAngleBrackets()
		{
			string templates = ""
				+ "group General;" + NL 
				+ "CreateTable(name) ::= <<" + NL
				+ "<Tset(a=name, b=name)>" + NL
				+ ">>" + NL
				+"Tset(a , b) ::= <<" + NL
				+ "  <a>" + NL
				+ ">>" + NL
				;
			StringTemplateGroup group = new StringTemplateGroup(
				new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate createTableTemplate = group.GetInstanceOf("CreateTable");
			createTableTemplate.SetAttribute("name", "A");
			string expecting = "  A";
			string result = createTableTemplate.ToString();
			Assert.AreEqual(expecting, result);
		}

		/// <summary>
		/// Used to throw a System.ArgumentException("invalid aggregate attribute format:")
		/// in ParseAggregateAttributeSpec(string, Ilist).
		/// Reported by: Ulf Dreyer (2006-05-16_14.59)
		/// </summary>
		[Test] public virtual void  TestAttributeIsArrayOfDataClass()
		{
			string templates = ""
				+ "group General;" + NL 
				+ "DisplayData(userList) ::= <<" + NL
				+ "<userList:{user|<user.Name>, <user.Email>}; separator=\"\\n\">" + NL
				+ ">>" + NL
				;
			StringTemplateGroup group = new StringTemplateGroup(
				new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate displayDataST= group.GetInstanceOf("DisplayData");
			UlfUserData[] data = { new UlfUserData("Kunle", "kunle@email.com"),
									 new UlfUserData("Micheal", "micheal@email.com"),
									 new UlfUserData("Ulf", "ulf@email.com")
								 };
			displayDataST.SetAttribute("userList", data);
			string expecting = ""
				+ "Kunle, kunle@email.com\n"
				+ "Micheal, micheal@email.com\n"
				+ "Ulf, ulf@email.com"
				;
			string result = displayDataST.ToString();
			Assert.AreEqual(expecting, result);
		}

		class UlfUserData
		{
			string name;
			string email;
			public UlfUserData(string name, string email)
			{
				this.name  = name;
				this.email = email;
			}
			public string GetName() { return name; }
			public string GetEmail() { return email; }
		}

		
		[Test] public virtual void TestEmptyStringAndEmptyAnonTemplateAsParameterUsingAngleBracketLexer()
		{
			string templates = ""
				+ "group Html;" + NL
				+ "CreateButton(name) ::= <<" + NL
				+ "<Button(name=name, id={}, value=\"\")>" + NL
				+ ">>" + NL
				+ "Button(name, id, value) ::= <<" + NL
				+ "\\<input type=\"button\" name=\"<name>\" id=\"<id>\" value=\"<value>\"/>" + NL
				+ ">>" + NL
				;
			StringTemplateGroup group = new StringTemplateGroup(
				new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate createTableTemplate = group.GetInstanceOf("CreateButton");
			createTableTemplate.SetAttribute("name", "btn");
			string expecting = "<input type=\"button\" name=\"btn\" id=\"\" value=\"\"/>";
			string result = createTableTemplate.ToString();
			Assert.AreEqual(expecting, result);
		}

		[Test] public virtual void TestEmptyStringAndEmptyAnonTemplateAsParameterUsingDollarLexer()
		{
			string templates = ""
				+ "group Html;" + NL
				+ "CreateButton(name) ::= <<" + NL
				+ "$Button(name=name, id={}, value=\"\")$" + NL
				+ ">>" + NL
				+ "Button(name, id, value) ::= <<" + NL
				+ "<input type=\"button\" name=\"$name$\" id=\"$id$\" value=\"$value$\"/>" + NL
				+ ">>" + NL
				;
			StringTemplateGroup group = new StringTemplateGroup(
				new StringReader(templates), typeof(DefaultTemplateLexer));
			StringTemplate createTableTemplate = group.GetInstanceOf("CreateButton");
			createTableTemplate.SetAttribute("name", "btn");
			string expecting = "<input type=\"button\" name=\"btn\" id=\"\" value=\"\"/>";
			string result = createTableTemplate.ToString();
			Assert.AreEqual(expecting, result);
		}

		#endregion

	
		private static void WriteFile(string dir, string fileName, string content) 
		{
			try 
			{
				lock(lockObject)
				{
					try 
					{
						File.Delete(Path.Combine(dir, fileName));
					}
					catch
					{
						// ignore
					}
					using (StreamWriter fw = new StreamWriter(Path.Combine(dir, fileName), false, System.Text.Encoding.Default))
					{
						fw.Write(content);
					}
				}
			}
			catch (Exception ex) 
			{
				Console.Error.WriteLine("Error creating or writing to file.");
				Console.Error.WriteLine(ex.StackTrace);
			}
		}
	
		private static void DeleteFile(string dir, string fileName) 
		{
			try 
			{
				lock(lockObject)
				{
				
					File.Delete(Path.Combine(dir, fileName));
				}
			}
			catch (Exception ex) 
			{
				Console.Error.WriteLine("Error deleting file.");
				Console.Error.WriteLine(ex.StackTrace);
			}
		}

	}
}