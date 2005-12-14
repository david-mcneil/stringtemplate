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
using System.IO;
using System.Collections;
using StringTemplateGroup = antlr.stringtemplate.StringTemplateGroup;
using StringTemplate = antlr.stringtemplate.StringTemplate;
using StringTemplateErrorListener = antlr.stringtemplate.StringTemplateErrorListener;
using StringTemplateWriter = antlr.stringtemplate.StringTemplateWriter;
using AngleBracketTemplateLexer = antlr.stringtemplate.language.AngleBracketTemplateLexer;
using NUnit.Framework;

namespace antlr.stringtemplate.test
{
	
	/// <summary>Test the various functionality of StringTemplate. Seems to run only
	/// on unix due to \r\n vs \n issue.  David Scurrah says:
	/// 
	/// "I don't think you were necessarily sloppy with your newlines, but Java make it very difficult to be consistant.
	/// The stringtemplate library used unix end of lines for writing toString methods and the like,
	/// while the testing was using the system local end of line. The other problem with end of lines was any template
	/// file used in the testing will also have a specific end of line ( this case unix) and when read into a string that can the unique problem
	/// of having end of line unix and local system end of line in the on  line.
	/// My solution was not very elegant but I think it required the least changes and only to the testing.
	/// I simply converted all strings to use unix end of line characters inside the assertTrue and then compared them.
	/// The only other problem I found was writing a file out to the /tmp directory won't work on windows so I used the
	/// system property  java.io.tmpdir to get a temp directory."
	/// I'll fix later.
	/// </summary>
	[TestFixture]
	public class TestStringTemplate
	{
		private class AnonymousClassStringTemplateWriter : StringTemplateWriter
		{
			public AnonymousClassStringTemplateWriter(System.Text.StringBuilder buf, TestStringTemplate enclosingInstance)
			{
				InitBlock(buf, enclosingInstance);
			}
			private void InitBlock(System.Text.StringBuilder buf, TestStringTemplate enclosingInstance)
			{
				this.buf = buf;
				this.enclosingInstance = enclosingInstance;
			}

			private System.Text.StringBuilder buf;
			private TestStringTemplate enclosingInstance;

			public TestStringTemplate Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public virtual void pushIndentation(String indent)
			{
			}
			public virtual String popIndentation()
			{
				return null;
			}
			public virtual int write(String str)
			{
				buf.Append(str); // just pass thru
				return str.Length;
			}
		}

		internal String newline = Environment.NewLine;
		
		public TestStringTemplate()
		{
		}
		
		internal class ErrorBuffer : StringTemplateErrorListener
		{
			internal System.Text.StringBuilder errorOutput = new System.Text.StringBuilder(500);
			public virtual void error(String msg, System.Exception e)
			{
				if (e != null)
				{
					errorOutput.Append(msg + ": " + e);
				}
				else
				{
					errorOutput.Append(msg);
				}
			}
			public virtual void warning(String msg)
			{
				errorOutput.Append(msg);
			}
			public virtual void debug(String msg)
			{
				errorOutput.Append(msg);
			}
			public  override bool Equals(Object o)
			{
				String me = ToString();
				String them = o.ToString();
				return me.Equals(them);
			}
			public override String ToString()
			{
				return errorOutput.ToString();
			}
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}
		}
		
		[Test]
		public virtual void testGroupFileFormat()
		{
			String templates = "group test;" + newline + "t() ::= \"literal template\"" + newline + "bold(item) ::= \"<b>$item$</b>\"" + newline + "duh() ::= <<" + newline + "xx" + newline + ">>" + newline;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			
			String expecting = "group test;\nduh() ::= <<xx>>\nbold(item) ::= <<<b>$item$</b>>>\nt() ::= <<literal template>>\n";
			Assert.AreEqual(group.ToString(), expecting);
			
			StringTemplate a = group.getInstanceOf("t");
			expecting = "literal template";
			Assert.AreEqual(a.ToString(), expecting);
			
			StringTemplate b = group.getInstanceOf("bold");
			b.setAttribute("item", "dork");
			expecting = "<b>dork</b>";
			Assert.AreEqual(b.ToString(), expecting);
		}
		
		/// <summary>Check syntax and setAttribute-time errors </summary>
		[Test]
		public virtual void testTemplateParameterDecls()
		{
			String templates = "group test;" + newline + "t() ::= \"no args but ref $foo$\"" + newline + "t2(item) ::= \"decl but not used is ok\"" + newline + "t3(a,b,c,d) ::= <<$a$ $d$>>" + newline + "t4(a,b,c,d) ::= <<$a$ $b$ $c$ $d$>>" + newline;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			
			// check setting unknown arg in empty formal list
			StringTemplate a = group.getInstanceOf("t");
			String error = null;
			try
			{
				a.setAttribute("foo", "x"); // want ArgumentOutOfRangeException
			}
			catch (System.ArgumentOutOfRangeException e)
			{
				error = e.Message;
			}
			String expecting = "no such attribute: foo in template t or in enclosing template\r\nParameter name: name";
			Assert.AreEqual(error, expecting);
			
			// check setting known arg
			a = group.getInstanceOf("t2");
			a.setAttribute("item", "x"); // shouldn't get exception
			
			// check setting unknown arg in nonempty list of formal args
			a = group.getInstanceOf("t3");
			a.setAttribute("b", "x");
		}
		
		[Test]
		public virtual void testTemplateRedef()
		{
			String templates = "group test;" + newline + "a() ::= \"x\"" + newline + "b() ::= \"y\"" + newline + "a() ::= \"z\"" + newline;
			StringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), errors);
			String expecting = "redefinition of template: a";
			Assert.AreEqual(errors.ToString(), expecting);
		}
		
		[Test]
		public virtual void testMissingInheritedAttribute()
		{
			String templates = "group test;" + newline + "page(title,font) ::= <<" + newline + "<html>" + newline + "<body>" + newline + "$title$<br>" + newline + "$body()$" + newline + "</body>" + newline + "</html>" + newline + ">>" + newline + "body() ::= \"<font face=$font$>my body</font>\"" + newline;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate t = group.getInstanceOf("page");
			t.setAttribute("title", "my title");
			t.setAttribute("font", "Helvetica"); // body() will see it
			t.ToString(); // should be no problem
		}
		
		[Test]
		public virtual void testFormalArgumentAssignment()
		{
			String templates = "group test;" + newline + "page() ::= <<$body(font=\"Times\")$>>" + newline + "body(font) ::= \"<font face=$font$>my body</font>\"" + newline;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate t = group.getInstanceOf("page");
			String expecting = "<font face=Times>my body</font>";
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testUndefinedArgumentAssignment()
		{
			String templates = "group test;" + newline + "page(x) ::= <<$body(font=x)$>>" + newline + "body() ::= \"<font face=$font$>my body</font>\"" + newline;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate t = group.getInstanceOf("page");
			t.setAttribute("x", "Times");
			String error = "";
			try
			{
				t.ToString();
			}
			catch (System.ArgumentOutOfRangeException iae)
			{
				error = iae.Message;
			}
			String expecting = "no such attribute: font in template body or in enclosing template\r\nParameter name: name";
			Assert.AreEqual(error, expecting);
		}
		
		[Test]
		public virtual void testFormalArgumentAssignmentInApply()
		{
			String templates = "group test;" + newline + "page(name) ::= <<$name:bold(font=\"Times\")$>>" + newline + "bold(font) ::= \"<font face=$font$><b>$it$</b></font>\"" + newline;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate t = group.getInstanceOf("page");
			t.setAttribute("name", "Ter");
			String expecting = "<font face=Times><b>Ter</b></font>";
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testUndefinedArgumentAssignmentInApply()
		{
			String templates = "group test;" + newline + "page(name,x) ::= <<$name:bold(font=x)$>>" + newline + "bold() ::= \"<font face=$font$><b>$it$</b></font>\"" + newline;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate t = group.getInstanceOf("page");
			t.setAttribute("x", "Times");
			t.setAttribute("name", "Ter");
			String error = "";
			try
			{
				t.ToString();
			}
			catch (System.ArgumentOutOfRangeException iae)
			{
				error = iae.Message;
			}
			String expecting = "no such attribute: font in template bold or in enclosing template\r\nParameter name: name";
			Assert.AreEqual(error, expecting);
		}
		
		[Test]
		public virtual void testUndefinedAttributeReference()
		{
			String templates = "group test;" + newline + "page() ::= <<$bold()$>>" + newline + "bold() ::= \"$name$\"" + newline;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate t = group.getInstanceOf("page");
			String error = "";
			try
			{
				t.ToString();
			}
			catch (System.ArgumentOutOfRangeException iae)
			{
				error = iae.Message;
			}
			String expecting = "no such attribute: name in template bold\r\nParameter name: attribute";
			Assert.AreEqual(error, expecting);
		}
		
		[Test]
		public virtual void testUndefinedDefaultAttributeReference()
		{
			String templates = "group test;" + newline + "page() ::= <<$bold()$>>" + newline + "bold() ::= \"$it$\"" + newline;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates));
			StringTemplate t = group.getInstanceOf("page");
			String error = "";
			try
			{
				t.ToString();
			}
			catch (System.ArgumentOutOfRangeException nse)
			{
				error = nse.Message;
			}
			String expecting = "no such attribute: it in template bold\r\nParameter name: attribute";
			Assert.AreEqual(error, expecting);
		}
		
		[Test]
		public virtual void testAngleBracketsWithGroupFile()
		{
			String templates = "group test;" + newline + "a(s) ::= \"<s:{case <i> : <it> break;}>\"" + newline + "b(t) ::= \"<t; separator=\\\",\\\">\"" + newline + "c(t) ::= << <t; separator=\",\"> >>" + newline;
			// mainly testing to ensure we don't get parse errors of above
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer), null);
			StringTemplate t = group.getInstanceOf("a");
			t.setAttribute("s", "Test");
			String expecting = "case 1 : Test break;";
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testAngleBracketsNoGroup()
		{
			StringTemplate st = new StringTemplate("Tokens : <rules; separator=\"|\"> ;", typeof(AngleBracketTemplateLexer));
			st.setAttribute("rules", "A");
			st.setAttribute("rules", "B");
			String expecting = "Tokens : A|B ;";
			Assert.AreEqual(st.ToString(), expecting);
		}
		
		[Test]
		public virtual void testSimpleInheritance()
		{
			// make a bold template in the super group that you can inherit from sub
			StringTemplateGroup supergroup = new StringTemplateGroup("super");
			StringTemplateGroup subgroup = new StringTemplateGroup("sub");
			StringTemplate bold = supergroup.defineTemplate("bold", "<b>$it$</b>");
			subgroup.setSuperGroup(supergroup);
			StringTemplateErrorListener errors = new ErrorBuffer();
			subgroup.setErrorListener(errors);
			supergroup.setErrorListener(errors);
			StringTemplate duh = new StringTemplate(subgroup, "$name:bold()$");
			duh.setAttribute("name", "Terence");
			String expecting = "<b>Terence</b>";
			Assert.AreEqual(duh.ToString(), expecting);
		}
		
		[Test]
		public virtual void testOverrideInheritance()
		{
			// make a bold template in the super group and one in sub group
			StringTemplateGroup supergroup = new StringTemplateGroup("super");
			StringTemplateGroup subgroup = new StringTemplateGroup("sub");
			supergroup.defineTemplate("bold", "<b>$it$</b>");
			subgroup.defineTemplate("bold", "<strong>$it$</strong>");
			subgroup.setSuperGroup(supergroup);
			StringTemplateErrorListener errors = new ErrorBuffer();
			subgroup.setErrorListener(errors);
			supergroup.setErrorListener(errors);
			StringTemplate duh = new StringTemplate(subgroup, "$name:bold()$");
			duh.setAttribute("name", "Terence");
			String expecting = "<strong>Terence</strong>";
			Assert.AreEqual(duh.ToString(), expecting);
		}
		
		[Test]
		public virtual void testMultiLevelInheritance()
		{
			// must loop up two levels to find bold()
			StringTemplateGroup rootgroup = new StringTemplateGroup("root");
			StringTemplateGroup level1 = new StringTemplateGroup("level1");
			StringTemplateGroup level2 = new StringTemplateGroup("level2");
			rootgroup.defineTemplate("bold", "<b>$it$</b>");
			level1.setSuperGroup(rootgroup);
			level2.setSuperGroup(level1);
			StringTemplateErrorListener errors = new ErrorBuffer();
			rootgroup.setErrorListener(errors);
			level1.setErrorListener(errors);
			level2.setErrorListener(errors);
			StringTemplate duh = new StringTemplate(level2, "$name:bold()$");
			duh.setAttribute("name", "Terence");
			String expecting = "<b>Terence</b>";
			Assert.AreEqual(duh.ToString(), expecting);
		}
		
		[Test]
		public virtual void testExprInParens()
		{
			// specify a template to apply to an attribute
			// Use a template group so we can specify the start/stop chars
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate bold = group.defineTemplate("bold", "<b>$it$</b>");
			StringTemplate duh = new StringTemplate(group, "$(\"blort: \"+(list)):bold()$");
			duh.setAttribute("list", "a");
			duh.setAttribute("list", "b");
			duh.setAttribute("list", "c");
			// System.out.println(duh);
			String expecting = "<b>blort: abc</b>";
			Assert.AreEqual(duh.ToString(), expecting);
		}
		
		[Test]
		public virtual void testMultipleAdditions()
		{
			// specify a template to apply to an attribute
			// Use a template group so we can specify the start/stop chars
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			group.defineTemplate("link", "<a href=\"$url$\"><b>$title$</b></a>");
			StringTemplate duh = new StringTemplate(group, "$link(url=\"/member/view?ID=\"+ID+\"&x=y\"+foo, title=\"the title\")$");
			duh.setAttribute("ID", "3321");
			duh.setAttribute("foo", "fubar");
			String expecting = "<a href=\"/member/view?ID=3321&x=yfubar\"><b>the title</b></a>";
			Assert.AreEqual(duh.ToString(), expecting);
		}
		
		[Test]
		public virtual void testCollectionAttributes()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate bold = group.defineTemplate("bold", "<b>$it$</b>");
			StringTemplate t = new StringTemplate(group, "$data$, $data:bold()$, " + "$list:bold():bold()$, $array$, $a2$, $a3$, $a4$");
			ArrayList v = ArrayList.Synchronized(new ArrayList(10));
			v.Add("1");
			v.Add("2");
			v.Add("3");
			IList list = new ArrayList();
			list.Add("a");
			list.Add("b");
			list.Add("c");
			t.setAttribute("data", v);
			t.setAttribute("list", list);
			t.setAttribute("array", new String[]{"x", "y"});
			t.setAttribute("a2", new int[]{10, 20});
			t.setAttribute("a3", new float[]{1.2f, 1.3f});
			t.setAttribute("a4", new double[]{8.7, 9.2});
			//System.out.println(t);
			String expecting = "123, <b>1</b><b>2</b><b>3</b>, " + "<b><b>a</b></b><b><b>b</b></b><b><b>c</b></b>, xy, 1020, 1.21.3, 8.79.2";
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testParenthesizedExpression()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate bold = group.defineTemplate("bold", "<b>$it$</b>");
			StringTemplate t = new StringTemplate(group, "$(first+last):bold()$");
			t.setAttribute("first", "Joe");
			t.setAttribute("last", "Schmoe");
			//System.out.println(t);
			String expecting = "<b>JoeSchmoe</b>";
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testApplyTemplateNameExpression()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate bold = group.defineTemplate("foobar", "foo$attr$bar");
			StringTemplate t = new StringTemplate(group, "$data:(name+\"bar\")()$");
			t.setAttribute("data", "Ter");
			t.setAttribute("data", "Tom");
			t.setAttribute("name", "foo");
			//System.out.println(t);
			String expecting = "fooTerbarfooTombar";
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testTemplateNameExpression()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate foo = group.defineTemplate("foo", "hi there!");
			StringTemplate t = new StringTemplate(group, "$(name)()$");
			t.setAttribute("name", "foo");
			//System.out.println(t);
			String expecting = "hi there!";
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testMissingEndDelimiter()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplateErrorListener errors = new ErrorBuffer();
			group.setErrorListener(errors);
			StringTemplate t = new StringTemplate(group, "stuff $a then more junk etc...");
			String expectingError = "problem parsing template 'anonymous': line 1:31: expecting '$', found '<EOF>'";
			//System.out.println("error: '"+errors+"'");
			//System.out.println("expecting: '"+expectingError+"'");
			Assert.AreEqual(errors.ToString(), expectingError);
		}
		
		[Test]
		public virtual void testSetButNotRefd()
		{
			StringTemplate.setLintMode(true);
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate t = new StringTemplate(group, "$a$ then $b$ and $c$ refs.");
			t.setAttribute("a", "Terence");
			t.setAttribute("b", "Terence");
			t.setAttribute("cc", "Terence"); // oops...should be 'c'
			String newline = System.Environment.NewLine;
			StringTemplateErrorListener errors = new ErrorBuffer();
			group.setErrorListener(errors);
			String expectingError = "anonymous: set but not used: cc";
			String result = t.ToString(); // result is irrelevant
			//System.out.println("result error: '"+errors+"'");
			//System.out.println("expecting: '"+expectingError+"'");
			StringTemplate.setLintMode(false);
			Assert.AreEqual(errors.ToString(), expectingError);
		}
		
		[Test]
		public virtual void testNullTemplateApplication()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplateErrorListener errors = new ErrorBuffer();
			group.setErrorListener(errors);
			StringTemplate t = new StringTemplate(group, "$names:bold(x=it)$");
			t.setAttribute("names", "Terence");
			//System.out.println(t);
			String expecting = null;
			String result = null;
			String error = null;
			try
			{
				result = t.ToString();
			}
			catch (System.ArgumentException iae)
			{
				error = iae.Message;
			}
			Assert.AreEqual(error, "Can't load template bold.st");
		}
		
		[Test]
		public virtual void testNullTemplateToMultiValuedApplication()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplateErrorListener errors = new ErrorBuffer();
			group.setErrorListener(errors);
			StringTemplate t = new StringTemplate(group, "$names:bold(x=it)$");
			t.setAttribute("names", "Terence");
			t.setAttribute("names", "Tom");
			//System.out.println(t);
			String expecting = null; // bold not found...empty string
			String result = null;
			String error = null;
			try
			{
				result = t.ToString();
			}
			catch (System.ArgumentException iae)
			{
				error = iae.Message;
			}
			Assert.AreEqual(error, "Can't load template bold.st");
		}
		
		[Test]
		public virtual void testChangingAttrValueTemplateApplicationToVector()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate italics = group.defineTemplate("italics", "<i>$x$</i>");
			StringTemplate bold = group.defineTemplate("bold", "<b>$x$</b>");
			StringTemplate t = new StringTemplate(group, "$names:bold(x=it)$");
			t.setAttribute("names", "Terence");
			t.setAttribute("names", "Tom");
			//System.out.println("'"+t.toString()+"'");
			String expecting = "<b>Terence</b><b>Tom</b>";
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testChangingAttrValueRepeatedTemplateApplicationToVector()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate bold = group.defineTemplate("bold", "<b>$item$</b>");
			StringTemplate italics = group.defineTemplate("italics", "<i>$it$</i>");
			StringTemplate members = new StringTemplate(group, "$members:bold(item=it):italics(it=it)$");
			members.setAttribute("members", "Jim");
			members.setAttribute("members", "Mike");
			members.setAttribute("members", "Ashar");
			//System.out.println("members="+members);
			String expecting = "<i><b>Jim</b></i><i><b>Mike</b></i><i><b>Ashar</b></i>";
			Assert.AreEqual(members.ToString(), expecting);
		}
		
		[Test]
		public virtual void testAlternatingTemplateApplication()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate listItem = group.defineTemplate("listItem", "<li>$it$</li>");
			StringTemplate bold = group.defineTemplate("bold", "<b>$it$</b>");
			StringTemplate italics = group.defineTemplate("italics", "<i>$it$</i>");
			StringTemplate item = new StringTemplate(group, "$item:bold(),italics():listItem()$");
			item.setAttribute("item", "Jim");
			item.setAttribute("item", "Mike");
			item.setAttribute("item", "Ashar");
			//System.out.println("ITEM="+item);
			String expecting = "<li><b>Jim</b></li><li><i>Mike</i></li><li><b>Ashar</b></li>";
			Assert.AreEqual(item.ToString(), expecting);
		}
		
		[Test]
		public virtual void testExpressionAsRHSOfAssignment()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate hostname = group.defineTemplate("hostname", "$machine$.jguru.com");
			StringTemplate bold = group.defineTemplate("bold", "<b>$x$</b>");
			StringTemplate t = new StringTemplate(group, "$bold(x=hostname(machine=\"www\"))$");
			String expecting = "<b>www.jguru.com</b>";
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testTemplateApplicationAsRHSOfAssignment()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate hostname = group.defineTemplate("hostname", "$machine$.jguru.com");
			StringTemplate bold = group.defineTemplate("bold", "<b>$x$</b>");
			StringTemplate italics = group.defineTemplate("italics", "<i>$it$</i>");
			StringTemplate t = new StringTemplate(group, "$bold(x=hostname(machine=\"www\"):italics())$");
			String expecting = "<b><i>www.jguru.com</i></b>";
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testParameterAndAttributeScoping()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate italics = group.defineTemplate("italics", "<i>$x$</i>");
			StringTemplate bold = group.defineTemplate("bold", "<b>$x$</b>");
			StringTemplate t = new StringTemplate(group, "$bold(x=italics(x=name))$");
			t.setAttribute("name", "Terence");
			//System.out.println(t);
			String expecting = "<b><i>Terence</i></b>";
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testComplicatedSeparatorExpr()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate bold = group.defineTemplate("bulletSeparator", "</li>$foo$<li>");
			// make separator a complicated expression with args passed to included template
			StringTemplate t = new StringTemplate(group, "<ul>$name; separator=bulletSeparator(foo=\" \")+\"&nbsp;\"$</ul>");
			t.setAttribute("name", "Ter");
			t.setAttribute("name", "Tom");
			t.setAttribute("name", "Mel");
			//System.out.println(t);
			String expecting = "<ul>Ter</li> <li>&nbsp;Tom</li> <li>&nbsp;Mel</ul>";
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testAttributeRefButtedUpAgainstEndifAndWhitespace()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate a = new StringTemplate(group, "$if (!firstName)$$email$$endif$");
			a.setAttribute("email", "parrt@jguru.com");
			String expecting = "parrt@jguru.com";
			Assert.AreEqual(a.ToString(), expecting);
		}
		
		[Test]
		public virtual void testStringCatenationOnSingleValuedAttribute()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate bold = group.defineTemplate("bold", "<b>$it$</b>");
			StringTemplate a = new StringTemplate(group, "$name+\" Parr\":bold()$");
			StringTemplate b = new StringTemplate(group, "$bold(it=name+\" Parr\")$");
			a.setAttribute("name", "Terence");
			b.setAttribute("name", "Terence");
			String expecting = "<b>Terence Parr</b>";
			Assert.AreEqual(a.ToString(), expecting);
			Assert.AreEqual(b.ToString(), expecting);
		}
		
		[Test]
		public virtual void testApplyingTemplateFromDiskWithPrecompiledIF()
		{
			String newline = System.Environment.NewLine;
			// write the template files first to /tmp
			DirectoryInfo tmpDir = new DirectoryInfo("tmp");
			
			if (tmpDir.Exists) {
				tmpDir.Delete(true);
			}

			tmpDir.Create();
			StreamWriter fw = new StreamWriter("tmp/page.st", false, System.Text.Encoding.Default);
			fw.Write("<html><head>" + newline);
			//fw.write("  <title>PeerScope: $title$</title>"+newline);
			fw.Write("</head>" + newline);
			fw.Write("<body>" + newline);
			fw.Write("$if(member)$User: $member:terse()$$endif$" + newline);
			fw.Write("</body>" + newline);
			fw.Write("</head>" + newline);
			fw.Close();
			
			fw = new StreamWriter("tmp/terse.st", false, System.Text.Encoding.Default);
			fw.Write("$it.firstName$ $it.lastName$ (<tt>$it.email$</tt>)" + newline);
			fw.Close();
			
			// specify a template to apply to an attribute
			// Use a template group so we can specify the start/stop chars
			StringTemplateGroup group = new StringTemplateGroup("dummy", "tmp");
			
			StringTemplate a = group.getInstanceOf("page");
			a.setAttribute("member", new Connector(this));
			String expecting = "<html><head>" + newline + "</head>" + newline + "<body>" + newline + "User: Terence Parr (<tt>parrt@jguru.com</tt>)" + newline + "</body>" + newline + "</head>";
			//System.out.println("'"+a+"'");
			Assert.AreEqual(a.ToString(), expecting);
		}
		
		[Test]
		public virtual void testMultiValuedAttributeWithAnonymousTemplateUsingIndexVariableI()
		{
			StringTemplateGroup tgroup = new StringTemplateGroup("dummy", ".");
			StringTemplate t = new StringTemplate(tgroup, " List:" + this.newline + "  " + this.newline + "foo" + this.newline + this.newline + "$names:{<br>$i$. $it$" + this.newline + "}$");
			t.setAttribute("names", "Terence");
			t.setAttribute("names", "Jim");
			t.setAttribute("names", "Sriram");
			String newline = System.Environment.NewLine;
			//System.out.println(t);
			String expecting = " List:" + newline + "  " + newline + "foo" + newline + newline + "<br>1. Terence" + newline + "<br>2. Jim" + newline + "<br>3. Sriram" + newline;
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testFindTemplateInCLASSPATH()
		{
			// Look for templates in CLASSPATH as resources
			StringTemplateGroup mgroup = new StringTemplateGroup("method stuff", typeof(AngleBracketTemplateLexer));
			StringTemplate m = mgroup.getInstanceOf("test/method");
			// "method.st" references body() so "body.st" will be loaded too
			m.setAttribute("visibility", "public");
			m.setAttribute("name", "foobar");
			m.setAttribute("returnType", "void");
			m.setAttribute("statements", "i=1;"); // body inherits these from method
			m.setAttribute("statements", "x=i;");
			String newline = System.Environment.NewLine;
			String expecting = "public void foobar() {" + newline + "\t// start of a body" + newline + "\ti=1;\n\tx=i;" + newline + "\t// end of a body" + newline + "}";
			//System.out.println(m);
			Assert.AreEqual(m.ToString(), expecting);
		}
		
		[Test]
		public virtual void testApplyTemplateToSingleValuedAttribute()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate bold = group.defineTemplate("bold", "<b>$x$</b>");
			StringTemplate name = new StringTemplate(group, "$name:bold(x=name)$");
			name.setAttribute("name", "Terence");
			Assert.AreEqual(name.ToString(), "<b>Terence</b>");
		}
		
		[Test]
		public virtual void testStringLiteralAsAttribute()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate bold = group.defineTemplate("bold", "<b>$it$</b>");
			StringTemplate name = new StringTemplate(group, "$\"Terence\":bold()$");
			Assert.AreEqual(name.ToString(), "<b>Terence</b>");
		}
		
		[Test]
		public virtual void testApplyTemplateToSingleValuedAttributeWithDefaultAttribute()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate bold = group.defineTemplate("bold", "<b>$it$</b>");
			StringTemplate name = new StringTemplate(group, "$name:bold()$");
			name.setAttribute("name", "Terence");
			Assert.AreEqual(name.ToString(), "<b>Terence</b>");
		}
		
		[Test]
		public virtual void testApplyAnonymousTemplateToSingleValuedAttribute()
		{
			// specify a template to apply to an attribute
			// Use a template group so we can specify the start/stop chars
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate item = new StringTemplate(group, "$item:{<li>$it$</li>}$");
			item.setAttribute("item", "Terence");
			Assert.AreEqual(item.ToString(), "<li>Terence</li>");
		}
		
		[Test]
		public virtual void testApplyAnonymousTemplateToMultiValuedAttribute()
		{
			// specify a template to apply to an attribute
			// Use a template group so we can specify the start/stop chars
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate list = new StringTemplate(group, "<ul>$items$</ul>");
			// demonstrate setting arg to anonymous subtemplate
			StringTemplate item = new StringTemplate(group, "$item:{<li>$it$</li>}; separator=\",\"$");
			item.setAttribute("item", "Terence");
			item.setAttribute("item", "Jim");
			item.setAttribute("item", "John");
			list.setAttribute("items", item); // nested template
			Assert.AreEqual(list.ToString(), "<ul><li>Terence</li>,<li>Jim</li>,<li>John</li></ul>");
		}
		
		[Test]
		public virtual void testApplyAnonymousTemplateToAggregateAttribute()
		{
			StringTemplate st = new StringTemplate("$items:{$it.last$, $it.first$\n}$");
			st.setAttribute("items.{first,last}", "Ter", "Parr");
			st.setAttribute("items.{first,last}", "Tom", "Burns");
			String expecting = "Parr, Ter\nBurns, Tom\n";
			Assert.AreEqual(st.ToString(), expecting);
		}
		
		[Test]
		public virtual void testRepeatedApplicationOfTemplateToSingleValuedAttribute()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate search = group.defineTemplate("bold", "<b>$it$</b>");
			StringTemplate item = new StringTemplate(group, "$item:bold():bold()$");
			item.setAttribute("item", "Jim");
			Assert.AreEqual(item.ToString(), "<b><b>Jim</b></b>");
		}
		
		[Test]
		public virtual void testRepeatedApplicationOfTemplateToMultiValuedAttributeWithSeparator()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate search = group.defineTemplate("bold", "<b>$it$</b>");
			StringTemplate item = new StringTemplate(group, "$item:bold():bold(); separator=\",\"$");
			item.setAttribute("item", "Jim");
			item.setAttribute("item", "Mike");
			item.setAttribute("item", "Ashar");
			// first application of template must yield another vector!
			//System.out.println("ITEM="+item);
			Assert.AreEqual(item.ToString(), "<b><b>Jim</b></b>,<b><b>Mike</b></b>,<b><b>Ashar</b></b>");
		}
		
		// ### NEED A TEST OF obj ASSIGNED TO ARG?
		
		[Test]
		public virtual void testMultiValuedAttributeWithSeparator()
		{
			StringTemplate query;
			
			// if column can be multi-valued, specify a separator
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".", typeof(AngleBracketTemplateLexer));
			query = new StringTemplate(group, "SELECT <distinct> <column; separator=\", \"> FROM <table>;");
			query.setAttribute("column", "name");
			query.setAttribute("column", "email");
			query.setAttribute("table", "User");
			// uncomment next line to make "DISTINCT" appear in output
			// query.setAttribute("distince", "DISTINCT");
			// System.out.println(query);
			Assert.AreEqual(query.ToString(), "SELECT  name, email FROM User;");
		}
		
		[Test]
		public virtual void testSingleValuedAttributes()
		{
			// all attributes are single-valued:
			StringTemplate query = new StringTemplate("SELECT $column$ FROM $table$;");
			query.setAttribute("column", "name");
			query.setAttribute("table", "User");
			// System.out.println(query);
			Assert.AreEqual(query.ToString(), "SELECT name FROM User;");
		}
		
		[Test]
		public virtual void testIFTemplate()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".", typeof(AngleBracketTemplateLexer));
			StringTemplate t = new StringTemplate(group, "SELECT <column> FROM PERSON " + "<if(cond)>WHERE ID=<id><endif>;");
			t.setAttribute("column", "name");
			t.setAttribute("cond", "true");
			t.setAttribute("id", "231");
			Assert.AreEqual(t.ToString(), "SELECT name FROM PERSON WHERE ID=231;");
		}
		
		/// <summary>As of 2.0, you can test a boolean value </summary>
		[Test]
		public virtual void testIFBoolean()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			StringTemplate t = new StringTemplate(group, "$if(b)$x$endif$ $if(!b)$y$endif$");
			t.setAttribute("b", true);
			Assert.AreEqual(t.ToString(), "x ");
			
			t = t.getInstanceOf();
			t.setAttribute("b", false);
			Assert.AreEqual(t.ToString(), " y");
		}
		
		[Test]
		public virtual void testNestedIFTemplate()
		{
			String newline = System.Environment.NewLine;
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".", typeof(AngleBracketTemplateLexer));
			StringTemplate t = new StringTemplate(group, "ack<if(a)>" + newline + "foo" + newline + "<if(!b)>stuff<endif>" + newline + "<if(b)>no<endif>" + newline + "junk" + newline + "<endif>");
			t.setAttribute("a", "blort");
			// leave b as null
			//System.out.println("t="+t);
			String expecting = "ackfoo" + newline + "stuff" + newline + "junk";
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		public class Connector
		{
			public Connector(TestStringTemplate enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void InitBlock(TestStringTemplate enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private TestStringTemplate enclosingInstance;
			public TestStringTemplate Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public virtual int getID()
			{
				return 1;
			}
			public virtual String getFirstName()
			{
				return "Terence";
			}
			public virtual String getLastName()
			{
				return "Parr";
			}
			public virtual String getEmail()
			{
				return "parrt@jguru.com";
			}
			public virtual String getBio()
			{
				return "Superhero by night...";
			}
			/// <summary>As of 2.0, booleans work as you expect.  In 1.x,
			/// a missing value simulated a boolean.
			/// </summary>
			public virtual bool getCanEdit()
			{
				return false;
			}
		}
		
		public class Connector2
		{
			public Connector2(TestStringTemplate enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void InitBlock(TestStringTemplate enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private TestStringTemplate enclosingInstance;
			public TestStringTemplate Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public virtual int getID()
			{
				return 2;
			}
			public virtual String getFirstName()
			{
				return "Tom";
			}
			public virtual String getLastName()
			{
				return "Burns";
			}
			public virtual String getEmail()
			{
				return "tombu@jguru.com";
			}
			public virtual String getBio()
			{
				return "Superhero by day...";
			}
			public virtual System.Boolean getCanEdit()
			{
				return true;
			}
		}
		
		[Test]
		public virtual void testObjectPropertyReference()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			String newline = System.Environment.NewLine;
			StringTemplate t = new StringTemplate(group, "<b>Name: $p.firstName$ $p.lastName$</b><br>" + newline + "<b>Email: $p.email$</b><br>" + newline + "$p.bio$");
			t.setAttribute("p", new Connector(this));
			//System.out.println("t is "+t.toString());
			String expecting = "<b>Name: Terence Parr</b><br>" + newline + "<b>Email: parrt@jguru.com</b><br>" + newline + "Superhero by night...";
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testApplyRepeatedAnonymousTemplateWithForeignTemplateRefToMultiValuedAttribute()
		{
			// specify a template to apply to an attribute
			// Use a template group so we can specify the start/stop chars
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			group.defineTemplate("link", "<a href=\"$url$\"><b>$title$</b></a>");
			StringTemplate duh = new StringTemplate(group, "start|$p:{$link(url=\"/member/view?ID=\"+it.ID, title=it.firstName)$ $if(it.canEdit)$canEdit$endif$}:" + "{$it$<br>\n}$|end");
			duh.setAttribute("p", new Connector(this));
			duh.setAttribute("p", new Connector2(this));
			String newline = System.Environment.NewLine;
			//System.out.println(duh);
			String expecting = "start|<a href=\"/member/view?ID=1\"><b>Terence</b></a> <br>\n<a href=\"/member/view?ID=2\"><b>Tom</b></a> canEdit<br>\n|end";
			Assert.AreEqual(duh.ToString(), expecting);
		}
		
		public class Tree
		{
			protected internal IList children = new ArrayList();
			protected internal String text;
			public Tree(String t)
			{
				text = t;
			}
			public virtual String getText()
			{
				return text;
			}
			public virtual void addChild(Tree c)
			{
				children.Add(c);
			}
			public virtual Tree getFirstChild()
			{
				if (children.Count == 0)
				{
					return null;
				}
				return (Tree) children[0];
			}
			public virtual IList getChildren()
			{
				return children;
			}
		}
		
		[Test]
		public virtual void testRecursion()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".", typeof(AngleBracketTemplateLexer));
			group.defineTemplate("tree", "<if(it.firstChild)>" + "( <it.text> <it.children:tree(); separator=\" \"> )" + "<else>" + "<it.text>" + "<endif>");
			StringTemplate tree = group.getInstanceOf("tree");
			// build ( a b (c d) e )
			Tree root = new Tree("a");
			root.addChild(new Tree("b"));
			Tree subtree = new Tree("c");
			subtree.addChild(new Tree("d"));
			root.addChild(subtree);
			root.addChild(new Tree("e"));
			tree.setAttribute("it", root);
			String expecting = "( a b ( c d ) e )";
			Assert.AreEqual(tree.ToString(), expecting);
		}
		
		[Test]
		public virtual void testNestedAnonymousTemplates()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			String newline = System.Environment.NewLine;
			StringTemplate t = new StringTemplate(group, "$A:{" + newline + "<i>$it:{" + newline + "<b>$it$</b>" + newline + "}$</i>" + newline + "}$");
			t.setAttribute("A", "parrt");
			String expecting = newline + "<i>" + newline + "<b>parrt</b>" + newline + "</i>" + newline;
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testAnonymousTemplateAccessToEnclosingAttributes()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			String newline = System.Environment.NewLine;
			StringTemplate t = new StringTemplate(group, "$A:{" + newline + "<i>$it:{" + newline + "<b>$it$, $B$</b>" + newline + "}$</i>" + newline + "}$");
			t.setAttribute("A", "parrt");
			t.setAttribute("B", "tombu");
			String expecting = newline + "<i>" + newline + "<b>parrt, tombu</b>" + newline + "</i>" + newline;
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testNestedAnonymousTemplatesAgain()
		{
			
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			String newline = System.Environment.NewLine;
			StringTemplate t = new StringTemplate(group, "<table>" + newline + "$names:{<tr>$it:{<td>$it:{<b>$it$</b>}$</td>}$</tr>}$" + newline + "</table>" + newline);
			t.setAttribute("names", "parrt");
			t.setAttribute("names", "tombu");
			String expecting = "<table>" + newline + "<tr><td><b>parrt</b></td></tr><tr><td><b>tombu</b></td></tr>" + newline + "</table>" + newline;
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testEscapes()
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy", ".");
			String newline = System.Environment.NewLine;
			group.defineTemplate("foo", "$x$ && $it$");
			StringTemplate t = new StringTemplate(group, "$A:foo(x=\"dog\\\"\\\"\")$");
			StringTemplate u = new StringTemplate(group, "$A:foo(x=\"dog\\\"g\")$");
			StringTemplate v = new StringTemplate(group, "$A:{$it:foo(x=\"\\{dog\\}\\\"\")$ is cool}$");
			t.setAttribute("A", "ick");
			u.setAttribute("A", "ick");
			v.setAttribute("A", "ick");
			//System.out.println("t is '"+t.toString()+"'");
			//System.out.println("u is '"+u.toString()+"'");
			//System.out.println("v is '"+v.toString()+"'");
			String expecting = "dog\"\" && ick";
			Assert.AreEqual(t.ToString(), expecting);
			expecting = "dog\"g && ick";
			Assert.AreEqual(u.ToString(), expecting);
			expecting = "{dog}\" && ick is cool";
			Assert.AreEqual(v.ToString(), expecting);
		}
		
		[Test]
		public virtual void testEscapesOutsideExpressions()
		{
			StringTemplate b = new StringTemplate("It\\'s ok...\\$; $a:{\\'hi\\', $it$}$");
			b.setAttribute("a", "Ter");
			String expecting = "It\\'s ok...$; \\'hi\\', Ter";
			String result = b.ToString();
			Assert.AreEqual(result, expecting);
		}
		
		[Test]
		public virtual void testElseClause()
		{
			StringTemplate e = new StringTemplate("$if(title)$" + newline + "foo" + newline + "$else$" + newline + "bar" + newline + "$endif$");
			e.setAttribute("title", "sample");
			String expecting = "foo";
			Assert.AreEqual(e.ToString(), expecting);
			
			e = e.getInstanceOf();
			expecting = "bar";
			Assert.AreEqual(e.ToString(), expecting);
		}
		
		[Test]
		public virtual void testNestedIF()
		{
			StringTemplate e = new StringTemplate("$if(title)$" + newline + "foo" + newline + "$else$" + newline + "$if(header)$" + newline + "bar" + newline + "$else$" + newline + "blort" + newline + "$endif$" + newline + "$endif$");
			e.setAttribute("title", "sample");
			String expecting = "foo";
			Assert.AreEqual(e.ToString(), expecting);
			
			e = e.getInstanceOf();
			e.setAttribute("header", "more");
			expecting = "bar";
			Assert.AreEqual(e.ToString(), expecting);
			
			e = e.getInstanceOf();
			expecting = "blort";
			Assert.AreEqual(e.ToString(), expecting);
		}
		
		[Test]
		public virtual void testEmbeddedMultiLineIF()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplate main = new StringTemplate(group, "$sub$");
			StringTemplate sub = new StringTemplate(group, "begin" + newline + "$if(foo)$" + newline + "$foo$" + newline + "$else$" + newline + "blort" + newline + "$endif$" + newline);
			sub.setAttribute("foo", "stuff");
			main.setAttribute("sub", sub);
			String expecting = "begin" + newline + "stuff";
			Assert.AreEqual(main.ToString(), expecting);
			
			main = new StringTemplate(group, "$sub$");
			sub = sub.getInstanceOf();
			main.setAttribute("sub", sub);
			expecting = "begin" + newline + "blort";
			Assert.AreEqual(main.ToString(), expecting);
		}
		
		[Test]
		public virtual void testSimpleIndentOfAttributeList()
		{
			String templates = "group test;" + newline + "list(names) ::= <<" + "  $names; separator=\"\n\"$" + newline + ">>" + newline;
			StringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), errors);
			StringTemplate t = group.getInstanceOf("list");
			t.setAttribute("names", "Terence");
			t.setAttribute("names", "Jim");
			t.setAttribute("names", "Sriram");
			String expecting = "  Terence\n  Jim\n  Sriram";
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testIndentOfMultilineAttributes()
		{
			String templates = "group test;" + newline + "list(names) ::= <<" + "  $names; separator=\"\n\"$" + newline + ">>" + newline;
			StringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), errors);
			StringTemplate t = group.getInstanceOf("list");
			t.setAttribute("names", "Terence\nis\na\nmaniac");
			t.setAttribute("names", "Jim");
			t.setAttribute("names", "Sriram\nis\ncool");
			String expecting = "  Terence\n  is\n  a\n  maniac\n  Jim\n  Sriram\n  is\n  cool";
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testIndentOfMultipleBlankLines()
		{
			String templates = "group test;" + newline + "list(names) ::= <<" + "  $names$" + newline + ">>" + newline;
			StringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), errors);
			StringTemplate t = group.getInstanceOf("list");
			t.setAttribute("names", "Terence\n\nis a maniac");
			String expecting = "  Terence\n\n  is a maniac";
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testIndentBetweenLeftJustifiedLiterals()
		{
			String templates = "group test;" + newline + "list(names) ::= <<" + "Before:" + newline + "  $names; separator=\"\\n\"$" + newline + "after" + newline + ">>" + newline;
			StringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), errors);
			StringTemplate t = group.getInstanceOf("list");
			t.setAttribute("names", "Terence");
			t.setAttribute("names", "Jim");
			t.setAttribute("names", "Sriram");
			String expecting = "Before:" + newline + "  Terence\n  Jim\n  Sriram" + newline + "after";
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testNestedIndent()
		{
			String templates = "group test;" + newline + "method(name,stats) ::= <<" + "void $name$() {" + newline + "\t$stats; separator=\"\\n\"$" + newline + "}" + newline + ">>" + newline + "ifstat(expr,stats) ::= <<" + newline + "if ($expr$) {" + newline + "  $stats; separator=\"\\n\"$" + newline + "}" + ">>" + newline + "assign(lhs,expr) ::= <<$lhs$=$expr$;>>" + newline;
			StringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), errors);
			StringTemplate t = group.getInstanceOf("method");
			t.setAttribute("name", "foo");
			StringTemplate s1 = group.getInstanceOf("assign");
			s1.setAttribute("lhs", "x");
			s1.setAttribute("expr", "0");
			StringTemplate s2 = group.getInstanceOf("ifstat");
			s2.setAttribute("expr", "x>0");
			StringTemplate s2a = group.getInstanceOf("assign");
			s2a.setAttribute("lhs", "y");
			s2a.setAttribute("expr", "x+y");
			StringTemplate s2b = group.getInstanceOf("assign");
			s2b.setAttribute("lhs", "z");
			s2b.setAttribute("expr", "4");
			s2.setAttribute("stats", s2a);
			s2.setAttribute("stats", s2b);
			t.setAttribute("stats", s1);
			t.setAttribute("stats", s2);
			String expecting = "void foo() {" + newline + "\tx=0;\n\tif (x>0) {" + newline + "\t  y=x+y;\n\t  z=4;" + newline + "\t}" + newline + "}";
			Assert.AreEqual(t.ToString(), expecting);
		}
		
		[Test]
		public virtual void testAlternativeWriter()
		{
			System.Text.StringBuilder buf = new System.Text.StringBuilder();
			StringTemplateWriter w = new AnonymousClassStringTemplateWriter(buf, this);
			StringTemplateGroup group = new StringTemplateGroup("test");
			group.defineTemplate("bold", "<b>$x$</b>");
			StringTemplate name = new StringTemplate(group, "$name:bold(x=name)$");
			name.setAttribute("name", "Terence");
			name.write(w);
			Assert.AreEqual(buf.ToString(), "<b>Terence</b>");
		}
		
		[Test]
		public virtual void testApplyAnonymousTemplateToMapAndSet()
		{
			StringTemplate st = new StringTemplate("$items:{<li>$it$</li>}$");
			IDictionary m = new Hashtable();
			m["a"] = "1";
			m["b"] = "2";
			m["c"] = "3";
			st.setAttribute("items", m);
			String expecting = "<li>1</li><li>2</li><li>3</li>";
			Assert.AreEqual(st.ToString(), expecting);
			
			st = st.getInstanceOf();
			IDictionary s = new Hashtable();
			s.Add("1", null);
			s.Add("2", null);
			s.Add("3", null);
			st.setAttribute("items", s.Keys);
			expecting = "<li>2</li><li>3</li><li>1</li>";
			Assert.AreEqual(st.ToString(), expecting);
		}
		
		[Test]
		public virtual void testDumpMapAndSet()
		{
			StringTemplate st = new StringTemplate("$items; separator=\",\"$");
			IDictionary m = new Hashtable();
			m["a"] = "1";
			m["b"] = "2";
			m["c"] = "3";
			st.setAttribute("items", m);
			String expecting = "1,2,3";
			Assert.AreEqual(st.ToString(), expecting);
			
			st = st.getInstanceOf();
			IDictionary s = new Hashtable();
			s.Add("1", null);
			s.Add("2", null);
			s.Add("3", null);
			st.setAttribute("items", s.Keys);
			expecting = "2,3,1";
			Assert.AreEqual(st.ToString(), expecting);
		}
		
		public class Connector3
		{
			public Connector3(TestStringTemplate enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void InitBlock(TestStringTemplate enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private TestStringTemplate enclosingInstance;
			public TestStringTemplate Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public virtual int[] getValues()
			{
				return new int[]{1, 2, 3};
			}
			public virtual IDictionary getStuff()
			{
				IDictionary m = new Hashtable();
				m["a"] = "1"; m["b"] = "2"; return m;
			}
		}
		
		[Test]
		public virtual void testApplyAnonymousTemplateToArrayAndMapProperty()
		{
			StringTemplate st = new StringTemplate("$x.values:{<li>$it$</li>}$");
			st.setAttribute("x", new Connector3(this));
			String expecting = "<li>1</li><li>2</li><li>3</li>";
			Assert.AreEqual(st.ToString(), expecting);
			
			st = new StringTemplate("$x.stuff:{<li>$it$</li>}$");
			st.setAttribute("x", new Connector3(this));
			expecting = "<li>1</li><li>2</li>";
			Assert.AreEqual(st.ToString(), expecting);
		}
		
		[Test]
		public virtual void testSuperTemplateRef()
		{
			// you can refer to a template defined in a super group via super.t()
			StringTemplateGroup group = new StringTemplateGroup("super");
			StringTemplateGroup subGroup = new StringTemplateGroup("sub");
			subGroup.setSuperGroup(group);
			group.defineTemplate("page", "$font()$:text");
			group.defineTemplate("font", "Helvetica");
			subGroup.defineTemplate("font", "$super.font()$ and Times");
			StringTemplate st = subGroup.getInstanceOf("page");
			String expecting = "Helvetica and Times:text";
			Assert.AreEqual(st.ToString(), expecting);
		}
		
		[Test]
		public virtual void testApplySuperTemplateRef()
		{
			StringTemplateGroup group = new StringTemplateGroup("super");
			StringTemplateGroup subGroup = new StringTemplateGroup("sub");
			subGroup.setSuperGroup(group);
			group.defineTemplate("bold", "<b>$it$</b>");
			subGroup.defineTemplate("bold", "<strong>$it$</strong>");
			subGroup.defineTemplate("page", "$name:super.bold()$");
			StringTemplate st = subGroup.getInstanceOf("page");
			st.setAttribute("name", "Ter");
			String expecting = "<b>Ter</b>";
			Assert.AreEqual(st.ToString(), expecting);
		}
		
		[Test]
		public virtual void testLazyEvalOfSuperInApplySuperTemplateRef()
		{
			StringTemplateGroup group = new StringTemplateGroup("super");
			StringTemplateGroup subGroup = new StringTemplateGroup("sub");
			subGroup.setSuperGroup(group);
			group.defineTemplate("bold", "<b>$it$</b>");
			subGroup.defineTemplate("bold", "<strong>$it$</strong>");
			// this is the same as testApplySuperTemplateRef() test
			// 'cept notice that here the supergroup defines page
			// As long as you create the instance via the subgroup,
			// "super." will evaluate lazily (i.e., not statically
			// during template compilation) to the templates
			// getGroup().superGroup value.  If I create instance
			// of page in group not subGroup, however, I will get
			// an error as superGroup is null for group "group".
			group.defineTemplate("page", "$name:super.bold()$");
			StringTemplate st = subGroup.getInstanceOf("page");
			st.setAttribute("name", "Ter");
			String expecting = "<b>Ter</b>";
			Assert.AreEqual(st.ToString(), expecting);
		}
		
		[Test]
		public virtual void testTemplatePolymorphism()
		{
			StringTemplateGroup group = new StringTemplateGroup("super");
			StringTemplateGroup subGroup = new StringTemplateGroup("sub");
			subGroup.setSuperGroup(group);
			// bold is defined in both super and sub
			// if you create an instance of page via the subgroup,
			// then bold() should evaluate to the subgroup not the super
			// even though page is defined in the super.  Just like polymorphism.
			group.defineTemplate("bold", "<b>$it$</b>");
			group.defineTemplate("page", "$name:bold()$");
			subGroup.defineTemplate("bold", "<strong>$it$</strong>");
			StringTemplate st = subGroup.getInstanceOf("page");
			st.setAttribute("name", "Ter");
			String expecting = "<strong>Ter</strong>";
			Assert.AreEqual(st.ToString(), expecting);
		}
		
		[Test]
		public virtual void testListOfEmbeddedTemplateSeesEnclosingAttributes()
		{
			String templates = "group test;" + newline + "output(cond,items) ::= <<page: $items$>>" + newline + "mybody() ::= <<$font()$stuff>>" + newline + "font() ::= <<$if(cond)$this$else$that$endif$>>";
			StringTemplateErrorListener errors = new ErrorBuffer();
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), errors);
			StringTemplate outputST = group.getInstanceOf("output");
			StringTemplate bodyST1 = group.getInstanceOf("mybody");
			StringTemplate bodyST2 = group.getInstanceOf("mybody");
			StringTemplate bodyST3 = group.getInstanceOf("mybody");
			outputST.setAttribute("items", bodyST1);
			outputST.setAttribute("items", bodyST2);
			outputST.setAttribute("items", bodyST3);
			String expecting = "page: thatstuffthatstuffthatstuff";
			Assert.AreEqual(outputST.ToString(), expecting);
		}
		
		[Test]
		public virtual void testInheritArgumentFromRecursiveTemplateApplication()
		{
			// do not inherit attributes through formal args
			String templates = "group test;" + newline + "block(stats) ::= \"<stats>\"" + "ifstat(stats) ::= \"IF true then <stats>\"" + newline;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate b = group.getInstanceOf("block");
			b.setAttribute("stats", group.getInstanceOf("ifstat"));
			b.setAttribute("stats", group.getInstanceOf("ifstat"));
			String expecting = "IF true then IF true then ";
			String result = b.ToString();
			//System.err.println("result='"+result+"'");
			Assert.AreEqual(result, expecting);
		}
		
		
		[Test]
		public virtual void testDeliberateRecursiveTemplateApplication()
		{
			// This test will cause infinite loop.  block contains a stat which
			// contains the same block.  Must be in lintMode to detect
			String templates = "group test;" + newline + "block(stats) ::= \"<stats>\"" + "ifstat(stats) ::= \"IF true then <stats>\"" + newline;
			StringTemplate.setLintMode(true);
			StringTemplate.resetTemplateCounter();
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate b = group.getInstanceOf("block");
			StringTemplate ifstat = group.getInstanceOf("ifstat");
			b.setAttribute("stats", ifstat); // block has if stat
			ifstat.setAttribute("stats", b); // but make "if" contain block
			String expecting = "IF true then ";
			String expectingError = "infinite recursion to <ifstat([stats])@4> referenced in <block([attributes, stats])@3>; stack trace:\n<ifstat([stats])@4>, attributes=[stats=<block()@3>]>\n<block([attributes, stats])@3>, attributes=[attributes, stats=<ifstat()@4>], references=[stats]>\n<ifstat([stats])@4> (start of recursive cycle)\n...";
			// note that attributes attribute doesn't show up in ifstat() because
			// recursion detection traps the problem before it writes out the
			// infinitely-recursive template; I set the attributes attribute right
			// before I render.
			String errors = "";
			try
			{
				String result = b.ToString();
			}
			catch (System.SystemException ise)
			{
				errors = ise.Message;
			}
			//System.err.println("errors="+errors+"'");
			//System.err.println("expecting="+expectingError+"'");
			StringTemplate.setLintMode(false);
			Assert.AreEqual(errors, expectingError);
		}
		
		
		[Test]
		public virtual void testImmediateTemplateAsAttributeLoop()
		{
			// even though block has a stats value that refers to itself,
			// there is no recursion because each instance of block hides
			// the stats value from above since it's a formal arg.
			String templates = "group test;" + newline + "block(stats) ::= \"{<stats>}\"";
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate b = group.getInstanceOf("block");
			b.setAttribute("stats", group.getInstanceOf("block"));
			String expecting = "{{}}";
			String result = b.ToString();
			//System.err.println(result);
			Assert.AreEqual(result, expecting);
		}
		
		
		[Test]
		public virtual void testTemplateAlias()
		{
			String templates = "group test;" + newline + "page(name) ::= \"name is <name>\"" + "other ::= page" + newline;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate b = group.getInstanceOf("other"); // alias for page
			b.setAttribute("name", "Ter");
			String expecting = "name is Ter";
			String result = b.ToString();
			Assert.AreEqual(result, expecting);
		}
		
		[Test]
		public virtual void testTemplateGetPropertyGetsAttribute()
		{
			// This test will cause infinite loop if missing attribute no
			// properly caught in getAttribute
			String templates = "group test;" + newline + "Cfile(funcs) ::= <<" + newline + "#include \\<stdio.h>" + newline + "<funcs:{public void <it.name>(<it.args>);}; separator=\"\\n\">" + newline + "<funcs; separator=\"\\n\">" + newline + ">>" + newline + "func(name,args,body) ::= <<" + newline + "public void <name>(<args>) {<body>}" + newline + ">>" + newline;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate b = group.getInstanceOf("Cfile");
			StringTemplate f1 = group.getInstanceOf("func");
			StringTemplate f2 = group.getInstanceOf("func");
			f1.setAttribute("name", "f");
			f1.setAttribute("args", "");
			f1.setAttribute("body", "i=1;");
			f2.setAttribute("name", "g");
			f2.setAttribute("args", "int arg");
			f2.setAttribute("body", "y=1;");
			b.setAttribute("funcs", f1);
			b.setAttribute("funcs", f2);
			String expecting = "#include <stdio.h>" + newline + "public void f();\npublic void g(int arg);" + newline + "public void f() {i=1;}\npublic void g(int arg) {y=1;}";
			Assert.AreEqual(b.ToString(), expecting);
		}
		
		public class Decl
		{
			internal String name;
			internal String type;
			public Decl(String name, String type)
			{
				this.name = name; this.type = type;
			}
			public virtual String getName()
			{
				return name;
			}
			public virtual String getType()
			{
				return type;
			}
		}
		
		[Test]
		public virtual void testComplicatedIndirectTemplateApplication()
		{
			String templates = "group Java;" + newline + "" + newline + "file(variables,methods) ::= <<" + "<variables:{<it.decl:(it.format)()>}; separator=\"\\n\">" + newline + "<methods>" + newline + ">>" + newline + "intdecl() ::= \"int <it.name> = 0;\"" + newline + "intarray() ::= \"int[] <it.name> = null;\"" + newline;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate f = group.getInstanceOf("file");
			f.setAttribute("variables.{decl,format}", new Decl("i", "int"), "intdecl");
			f.setAttribute("variables.{decl,format}", new Decl("a", "int-array"), "intarray");
			//System.out.println("f='"+f+"'");
			String expecting = "int i = 0;\nint[] a = null;" + newline;
			Assert.AreEqual(f.ToString(), expecting);
		}
		
		[Test]
		public virtual void testIndirectTemplateApplication()
		{
			String templates = "group dork;" + newline + "" + newline + "test(name) ::= <<" + "<(name)()>" + newline + ">>" + newline + "first() ::= \"the first\"" + newline + "second() ::= \"the second\"" + newline;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate f = group.getInstanceOf("test");
			f.setAttribute("name", "first");
			String expecting = "the first";
			Assert.AreEqual(f.ToString(), expecting);
		}
		
		[Test]
		public virtual void testNullIndirectTemplateApplication()
		{
			String templates = "group dork;" + newline + "" + newline + "test(names) ::= <<" + "<names:(ind)()>" + newline + ">>" + newline + "ind() ::= \"[<it>]\"" + newline;
			;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate f = group.getInstanceOf("test");
			f.setAttribute("names", "me");
			f.setAttribute("names", "you");
			String expecting = "";
			Assert.AreEqual(f.ToString(), expecting);
		}
		
		[Test]
		public virtual void testNullIndirectTemplate()
		{
			String templates = "group dork;" + newline + "" + newline + "test(name) ::= <<" + "<(name)()>" + newline + ">>" + newline + "first() ::= \"the first\"" + newline + "second() ::= \"the second\"" + newline;
			StringTemplateGroup group = new StringTemplateGroup(new StringReader(templates), typeof(AngleBracketTemplateLexer));
			StringTemplate f = group.getInstanceOf("test");
			//f.setAttribute("name", "first");
			String expecting = "";
			Assert.AreEqual(f.ToString(), expecting);
		}
		
		[Test]
		public virtual void testReflection()
		{
			StringTemplate.setLintMode(true);
			StringTemplate a = new StringTemplate("$attributes$");
			StringTemplateErrorListener errors = new ErrorBuffer();
			a.setErrorListener(errors);
			a.setAttribute("name", "Terence");
			a.setAttribute("name", "Tom");
			StringTemplate embedded = new StringTemplate("embedded");
			embedded.setAttribute("z", "hi");
			a.setAttribute("name", embedded);
			a.setAttribute("notUsed", "foo");
			String expecting = "Template anonymous:" + newline + 
				"    1. Attribute notUsed values:" + newline + 
				"        1. String" + newline +
				"    2. Attribute name values:" + newline + 
				"        1. String" + newline + 
				"        2. String" + newline + 
				"        3. Template anonymous:" + newline + 
				"            1. Attribute z values:" + newline + 
				"                1. String" + newline; 
			a.setPredefinedAttributes();
			String results = a.ToString();
			//System.out.println(results);
			StringTemplate.setLintMode(false);
			Assert.AreEqual(results, expecting);
		}
		
		public class R1
		{
			public virtual String getFirstName()
			{
				return "Terence";
			}
			public virtual String getLastName()
			{
				return "Parr";
			}
			public virtual StringTemplate getSubTemplate()
			{
				StringTemplate s = new StringTemplate("sub ST");
				s.setName("a template created in R1");
				s.setAttribute("fruit", "Apple");
				s.setAttribute("nestedAggr", new R2());
				return s;
			}
			public virtual R2 getSubAggregate()
			{
				return new R2();
			}
		}
		
		
		public class R2
		{
			public virtual String getEmail()
			{
				return "parrt";
			}
		}
		
		
		[Test]
		public virtual void testReflectionRecursive()
		{
			StringTemplate.setLintMode(true);
			StringTemplate a = new StringTemplate("$attributes$");
			StringTemplateErrorListener errors = new ErrorBuffer();
			a.setErrorListener(errors);
			a.setAttribute("name", "Terence");
			a.setAttribute("name", "Tom");
			StringTemplate b = new StringTemplate("embedded template text");
			b.setName("EmbeddedTemplate");
			b.setAttribute("x", 1);
			b.setAttribute("y", "foo");
			a.setAttribute("name", b);
			a.setAttribute("name", new R1());
			a.setAttribute("duh", "foo");
			String results = a.ToString();
			//System.out.println(results);
			String expecting = "Template anonymous:" + newline + 
				"    1. Attribute duh values:" + newline + 
				"        1. String" + newline + 
				"    2. Attribute name values:" + newline + 
				"        1. String" + newline + 
				"        2. String" + newline + 
				"        3. Template EmbeddedTemplate:" + newline + 
				"            1. Attribute x values:" + newline + 
				"                1. Int32" + newline + 
				"            2. Attribute y values:" + newline + 
				"                1. String" + newline + 
				"        1. Property subAggregate : TestStringTemplate+R2" + newline +
				"            1. Property email : String" + newline + 
				"        2. Property subTemplate : StringTemplate" + newline + 
				"            2. Template a template created in R1:" + newline + 
				"                1. Attribute nestedAggr values:" + newline + 
				"                2. Attribute fruit values:" + newline + 
				"                    1. String" + newline + 
				"        3. Property lastName : String" + newline + 
				"        4. Property firstName : String" + newline; 
				StringTemplate.setLintMode(false);
			Assert.AreEqual(results, expecting);
		}
		
		public class A
		{
			public virtual B getB()
			{
				return null;
			}
		}
		
		
		public class B
		{
			public virtual A getA()
			{
				return null;
			}
			public virtual String getHarmless1()
			{
				return "";
			}
		}
		
		
		public class C
		{
			public virtual C getSelf()
			{
				return null;
			}
			public virtual String getHarmless2()
			{
				return "";
			}
		}
		
		
		[Test]
		public virtual void testReflectionTypeLoop()
		{
			StringTemplate.setLintMode(true);
			StringTemplate a = new StringTemplate("$attributes$");
			StringTemplateErrorListener errors = new ErrorBuffer();
			a.setErrorListener(errors);
			a.setAttribute("name", "Terence");
			a.setAttribute("c", new C());
			a.setAttribute("a", new A());
			String results = a.ToString();
			//System.out.println(results);
			String expecting = "Template anonymous:" + newline + 
				"    1. Attribute a values:" + newline + 
				"        1. Property b : TestStringTemplate+B" + newline + 
				"            1. Property harmless1 : String" + newline + 
				"            2. Property a : TestStringTemplate+A" + newline + 
				"    2. Attribute name values:" + newline + 
				"        1. String" + newline +
				"    3. Attribute c values:" + newline + 
				"        1. Property harmless2 : String" + newline + 
				"        2. Property self : TestStringTemplate+C" + newline; 
			StringTemplate.setLintMode(false);
			Assert.AreEqual(results, expecting);
		}
		
		public class R3
		{
			public virtual String getEmail()
			{
				return "parrt";
			}
			public virtual IDictionary getAMap()
			{
				Hashtable m = new Hashtable();
				m["v"] = "33"; return m;
			}
			public virtual Hashtable getAnotherMap()
			{
				return null;
			}
		}
		
		
		[Test]
		public virtual void testReflectionWithMap()
		{
			StringTemplate.setLintMode(true);
			StringTemplate a = new StringTemplate("$attributes$");
			StringTemplateErrorListener errors = new ErrorBuffer();
			a.setErrorListener(errors);
			Hashtable map = new Hashtable();
			a.setAttribute("stuff", map);
			map["prop1"] = "Terence";
			map["prop2"] = new R3();
			String results = a.ToString();
			//System.out.println(results);
			String expecting = "Template anonymous:" + newline + 
			"    1. Attribute stuff values:" + newline + 
			"        1. Key prop1 : String" + newline + 
			"        2. Key prop2 : TestStringTemplate+R3" + newline + 
			"            1. Property anotherMap : Hashtable" + newline +
			"            2. Property aMap : IDictionary" + newline + 
			"                1. Key v : String" + newline + 
			"            3. Property email : String" + newline; 
			StringTemplate.setLintMode(false);
			Assert.AreEqual(results, expecting);
		}
		
		[Test]
		public virtual void testHashMapPropertyFetch()
		{
			StringTemplate a = new StringTemplate("$stuff.prop$");
			Hashtable map = new Hashtable();
			a.setAttribute("stuff", map);
			map["prop"] = "Terence";
			String results = a.ToString();
			//System.out.println(results);
			String expecting = "Terence";
			Assert.AreEqual(results, expecting);
		}
		
		[Test]
		public virtual void testEmbeddedComments()
		{
			StringTemplate st = new StringTemplate("Foo $! ignore !$bar" + newline);
			String expecting = "Foo bar" + newline;
			String result = st.ToString();
			Assert.AreEqual(result, expecting);
			
			st = new StringTemplate("Foo $! ignore" + newline + " and a line break!$" + newline + "bar" + newline);
			expecting = "Foo " + newline + "bar" + newline;
			result = st.ToString();
			Assert.AreEqual(result, expecting);
			
			st = new StringTemplate("$! start of line $ and $! ick" + newline + "!$boo" + newline);
			expecting = "boo" + newline;
			result = st.ToString();
			Assert.AreEqual(result, expecting);
			
			st = new StringTemplate("$! start of line !$" + newline + "$! another to ignore !$" + newline + "$! ick" + newline + "!$boo" + newline);
			expecting = "boo" + newline;
			result = st.ToString();
			Assert.AreEqual(result, expecting);
			
			st = new StringTemplate("$! back !$$! to back !$" + newline + "$! ick" + newline + "!$boo" + newline);
			expecting = newline + "boo" + newline;
			result = st.ToString();
			Assert.AreEqual(result, expecting);
		}
		
		[Test]
		public virtual void testEmbeddedCommentsAngleBracketed()
		{
			StringTemplate st = new StringTemplate("Foo <! ignore !>bar" + newline, typeof(AngleBracketTemplateLexer));
			String expecting = "Foo bar" + newline;
			String result = st.ToString();
			Assert.AreEqual(result, expecting);
			
			st = new StringTemplate("Foo <! ignore" + newline + " and a line break!>" + newline + "bar" + newline, typeof(AngleBracketTemplateLexer));
			expecting = "Foo " + newline + "bar" + newline;
			result = st.ToString();
			Assert.AreEqual(result, expecting);
			
			st = new StringTemplate("<! start of line $ and <! ick" + newline + "!>boo" + newline, typeof(AngleBracketTemplateLexer));
			expecting = "boo" + newline;
			result = st.ToString();
			Assert.AreEqual(result, expecting);
			
			st = new StringTemplate("<! start of line !>" + "<! another to ignore !>" + "<! ick" + newline + "!>boo" + newline, typeof(AngleBracketTemplateLexer));
			expecting = "boo" + newline;
			result = st.ToString();
			//System.out.println(result);
			Assert.AreEqual(result, expecting);
			
			st = new StringTemplate("<! back !><! to back !>" + newline + "<! ick" + newline + "!>boo" + newline, typeof(AngleBracketTemplateLexer));
			expecting = newline + "boo" + newline;
			result = st.ToString();
			Assert.AreEqual(result, expecting);
		}
		
		[Test]
		public virtual void testCharLiterals()
		{
			StringTemplate st = new StringTemplate("Foo <\\n><\\t> bar" + newline, typeof(AngleBracketTemplateLexer));
			String expecting = "Foo \n\t bar" + newline;
			String result = st.ToString();
			Assert.AreEqual(result, expecting);
			
			st = new StringTemplate("Foo $\\n$$\\t$ bar\n");
			expecting = "Foo \n\t bar\n";
			result = st.ToString();
			Assert.AreEqual(result, expecting);
			
			st = new StringTemplate("Foo$\\ $bar$\\n$");
			expecting = "Foo bar\n";
			result = st.ToString();
			Assert.AreEqual(result, expecting);
		}
		
		[Test]
		public virtual void testEmptyIteratedValueGetsSeparator()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplateErrorListener errors = new ErrorBuffer();
			group.setErrorListener(errors);
			StringTemplate t = new StringTemplate(group, "$names; separator=\",\"$");
			t.setAttribute("names", "Terence");
			t.setAttribute("names", "");
			t.setAttribute("names", "");
			t.setAttribute("names", "Tom");
			t.setAttribute("names", "Frank");
			t.setAttribute("names", "");
			// empty values get separator still
			String expecting = "Terence,,,Tom,Frank,";
			String result = t.ToString();
			Assert.AreEqual(result, expecting);
		}
		
		[Test]
		public virtual void testEmptyIteratedConditionalValueGetsNoSeparator()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplateErrorListener errors = new ErrorBuffer();
			group.setErrorListener(errors);
			StringTemplate t = new StringTemplate(group, "$users:{$if(it.ok)$$it.name$$endif$}; separator=\",\"$");
			t.setAttribute("users.{name,ok}", "Terence", true);
			t.setAttribute("users.{name,ok}", "Tom", false);
			t.setAttribute("users.{name,ok}", "Frank", true);
			t.setAttribute("users.{name,ok}", "Johnny", false);
			// empty conditional values get no separator
			String expecting = "Terence,Frank,"; // haven't solved the last empty value problem yet
			String result = t.ToString();
			Assert.AreEqual(result, expecting);
		}
		
		[Test]
		public virtual void testEmptyIteratedConditionalWithElseValueGetsSeparator()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplateErrorListener errors = new ErrorBuffer();
			group.setErrorListener(errors);
			StringTemplate t = new StringTemplate(group, "$users:{$if(it.ok)$$it.name$$else$$endif$}; separator=\",\"$");
			t.setAttribute("users.{name,ok}", "Terence", true);
			t.setAttribute("users.{name,ok}", "Tom", false);
			t.setAttribute("users.{name,ok}", "Frank", true);
			t.setAttribute("users.{name,ok}", "Johnny", false);
			// empty conditional values get no separator
			String expecting = "Terence,,Frank,"; // haven't solved the last empty value problem yet
			String result = t.ToString();
			Assert.AreEqual(result, expecting);
		}
		
		[Test]
		public virtual void testWhiteSpaceAtEndOfTemplate()
		{
			StringTemplateGroup group = new StringTemplateGroup("group");
			StringTemplate pageST = group.getInstanceOf("test/page");
			StringTemplate listST = group.getInstanceOf("test/users.list");
			// users.list references row.st which has a single blank line at the end.
			// I.e., there are 2 \n in a row at the end
			// ST should eat all whitespace at end
			listST.setAttribute("users", new Connector(this));
			listST.setAttribute("users", new Connector2(this));
			pageST.setAttribute("title", "some title");
			pageST.setAttribute("body", listST);
			String expecting = "some title" + newline + "Terence parrt@jguru.comTom tombu@jguru.com";
			String result = pageST.ToString();
			//System.out.println("'"+result+"'");
			Assert.AreEqual(result, expecting);
		}
		
		internal class Duh
		{
			public IList users = new ArrayList();
		}
		
		[Test]
		public virtual void testSizeZeroButNonNullListGetsNoOutput()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplateErrorListener errors = new ErrorBuffer();
			group.setErrorListener(errors);
			StringTemplate t = new StringTemplate(group, "$duh.users:{name: $it$}; separator=\", \"$");
			t.setAttribute("duh", new Duh());
			String expecting = "";
			String result = t.ToString();
			Assert.AreEqual(result, expecting);
		}
		
		[Test]
		public virtual void testSizeZeroOnLineByItselfGetsNoOutput()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplateErrorListener errors = new ErrorBuffer();
			group.setErrorListener(errors);
			StringTemplate t = new StringTemplate(group, "begin\n" + "$name$\n" + "$users:{name: $it$}$\n" + "$users:{name: $it$}; separator=\", \"$\n" + "end\n");
			String expecting = "begin\nend\n";
			String result = t.ToString();
			Assert.AreEqual(result, expecting);
		}
		
		[Test]
		public virtual void testSizeZeroOnLineWithIndentGetsNoOutput()
		{
			StringTemplateGroup group = new StringTemplateGroup("test");
			StringTemplateErrorListener errors = new ErrorBuffer();
			group.setErrorListener(errors);
			StringTemplate t = new StringTemplate(group, "begin\n" + "  $name$\n" + "	$users:{name: $it$}$\n" + "	$users:{name: $it$$\\n$}$\n" + "end\n");
			String expecting = "begin\nend\n";
			String result = t.ToString();
			Assert.AreEqual(result, expecting);
		}
		
		[Test]
		public virtual void testSimpleAutoIndent()
		{
			StringTemplate a = new StringTemplate("$title$: {\n" + "	$name; separator=\"\n\"$\n" + "}");
			a.setAttribute("title", "foo");
			a.setAttribute("name", "Terence");
			a.setAttribute("name", "Frank");
			String results = a.ToString();
			//System.out.println(results);
			String expecting = "foo: {\n" + "	Terence\n" + "	Frank\n" + "}";
			Assert.AreEqual(results, expecting);
		}
	}
}