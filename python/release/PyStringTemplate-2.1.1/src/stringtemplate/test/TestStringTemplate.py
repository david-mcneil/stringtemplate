#/usr/bin/env python

import sys
import os
import traceback
import unittest
from StringIO import StringIO

import stringtemplate

#stringtemplate.StringTemplate.debugMode = True

class ErrorBuffer(stringtemplate.StringTemplateErrorListener):

    def __init__(self):
        self.errorOutput = StringIO()

    def error(self, msg, e):
        if e:
            self.errorOutput.write(msg + ': ' + str(e) + '\n')
        else:
            self.errorOutput.write(msg + '\n')

    def warning(self, msg):
        self.errorOutput.write(msg + '\n')

    def debug(self, msg):
        self.errorOutput.write(msg + '\n')

    def __cmp__(self, o):
        me = str(self)
        them = str(o)
        return cmp(me, them)

    def __str__(self):
        return self.errorOutput.getvalue()

    __repr__ = __str__

class TestGroupFileFormat(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "t() ::= \"literal template\"" +os.linesep+ \
            "bold(item) ::= \"<b>$item$</b>\""+os.linesep+ \
            "duh() ::= <<"+os.linesep+"xx"+os.linesep+">>"+os.linesep
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def testGroupFileFormat(self):
        expecting = \
            "group test;" +os.linesep+ \
            "duh() ::= <<xx>>" +os.linesep+ \
            "t() ::= <<literal template>>"+os.linesep+ \
            "bold(item) ::= <<<b>$item$</b>>>" +os.linesep
        self.assertEqual(str(self.group), expecting)

    def testGroupLiteralTemplate(self):
        a = self.group.getInstanceOf("t")
        expecting = "literal template"
        self.assertEqual(str(a), expecting)

    def testGroupSetAttribute(self):
        b = self.group.getInstanceOf("bold")
        b["item"] = "dork"
        expecting = "<b>dork</b>"
        self.assertEqual(str(b), expecting)

    def runTest(self):
        self.testGroupFileFormat()
        self.testGroupLiteralTemplate()
        self.testGroupSetAttribute()

### Check syntax and setAttribute-time errors
#
class TestTemplateParameterDecls(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "t()::= \"no args but ref $foo$\"" +os.linesep+ \
            "t2(item)::= \"decl but not used is ok\""+os.linesep + \
            "t3(a,b,c,d)::= <<$a$ $d$>>"+os.linesep+ \
            "t4(a,b,c,d)::= <<$a$ $b$ $c$ $d$>>"+os.linesep
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    # check setting unknown arg in empty formal list
    def testSetUnknownArgInEmptyFormalArgList(self):
        a = self.group.getInstanceOf("t")
        error = None
        try:
            a["foo"] = "x" # want KeyError
        except KeyError, e:
            error = str(e)
        expecting = "'no such attribute: foo in template context [t]'"
        self.assertEqual(error, expecting)

    # check setting known arg
    def testSetKnownArg(self):
        a = self.group.getInstanceOf("t2")
        try:
            a['item'] = "x" # shouldn't get exception
        except Exception, e:
            self.fail()

    # check setting unknown arg in nonempty list of formal args
    def testSetUnknownArgInNonEmptyFormalArgList(self):
        a = self.group.getInstanceOf("t3")
        try:
            a["b"] = "x"
        except Exception, e:
            self.fail()

    def runTest(self):
        self.testSetUnknownArgInEmptyFormalArgList()
        self.testSetKnownArg()
        self.testSetUnknownArgInNonEmptyFormalArgList()


class TestTemplateRedef(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "a()::= \"x\"" +os.linesep+ \
            "b()::= \"y\"" +os.linesep+ \
            "a()::= \"z\"" +os.linesep
        self.errors = ErrorBuffer()
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
                                                        self.errors)

    def runTest(self):
        expecting = "redefinition of template: a\n"
        self.assertEqual(str(self.errors), expecting)


class TestMissingInheritedAttribute(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "page(title,font)::= <<"+os.linesep + \
            "<html>"+os.linesep + \
            "<body>"+os.linesep + \
            "$title$<br>"+os.linesep + \
            "$body()$"+os.linesep + \
            "</body>"+os.linesep + \
            "</html>"+os.linesep + \
            ">>"+os.linesep + \
            "body()::= \"<font face=$font$>my body</font>\"" +os.linesep
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        t = self.group.getInstanceOf("page")
        t["title"] = "my title"
        t["font"] = "Helvetica" # body() will see it
        try:
            str(t) # should be no problem
        except Exception, e:
	    traceback.print_exc()
            self.fail()


class TestFormalArgumentAssignment(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "page()::= <<$body(font=\"Times\")$>>"+os.linesep + \
            "body(font)::= \"<font face=$font$>my body</font>\"" +os.linesep
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        t = self.group.getInstanceOf("page")
        expecting = "<font face=Times>my body</font>"
        self.assertEqual(str(t), expecting)


class TestUndefinedArgumentAssignment(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "page(x)::= <<$body(font=x)$>>"+os.linesep + \
            "body()::= \"<font face=$font$>my body</font>\"" +os.linesep
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))
        
    def runTest(self):
        t = self.group.getInstanceOf("page")
        t["x"] = "Times"
	error = None
	try:
	    str(t)
	except KeyError, e:
	    error = str(e)
        expecting = "'no such attribute: font in template context [page body]'";
        self.assertEqual(error, expecting);


class TestFormalArgumentAssignmentInApply(unittest.TestCase):

    def setUp(self):
        self.templates = \
        "group test;" +os.linesep+ \
        "page(name)::= <<$name:bold(font=\"Times\")$>>"+os.linesep + \
        "bold(font)::= \"<font face=$font$><b>$it$</b></font>\"" +os.linesep
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        t = self.group.getInstanceOf("page")
        t["name"] = "Ter"
        expecting = "<font face=Times><b>Ter</b></font>"
        self.assertEqual(str(t), expecting)


class TestUndefinedArgumentAssignmentInApply(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "page(name,x)::= <<$name:bold(font=x)$>>"+os.linesep + \
            "bold()::= \"<font face=$font$><b>$it$</b></font>\"" +os.linesep
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        t = self.group.getInstanceOf("page")
        t["x"] = "Times"
        t["name"] = "Ter"
	error = None
	try:
	    str(t)
	except KeyError, e:
	    error = str(e)
        expecting = "'no such attribute: font in template context [page bold]'";
        self.assertEqual(error, expecting);


class TestUndefinedAttributeReference(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "page()::= <<$bold()$>>"+os.linesep + \
            "bold()::= \"$name$\"" +os.linesep
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        t = self.group.getInstanceOf("page")
	error = None
	try:
	    str(t)
	except KeyError, e:
	    error = str(e)
        expecting = "'no such attribute: name in template context [page bold]'";
        self.assertEqual(error, expecting);


class TestUndefinedDefaultAttributeReference(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "page()::= <<$bold()$>>"+os.linesep + \
            "bold()::= \"$it$\"" +os.linesep
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        t = self.group.getInstanceOf("page")
	error = None
	try:
	    str(t)
	except KeyError, e:
	    error = str(e)
        expecting = "'no such attribute: it in template context [page bold]'";
        self.assertEqual(error, expecting);


class TestAngleBracketsWithGroupFileAsString(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "a(s)::= \"<s:{case <i>: <it> break;}>\""+os.linesep + \
            "b(t)::= \"<t; separator=\\\",\\\">\"" +os.linesep+ \
            "c(t)::= << <t; separator=\",\"> >>" +os.linesep
        # mainly testing to ensure we don't get parse errors of above
        self.group = stringtemplate.StringTemplateGroup( \
            StringIO(self.templates), \
            stringtemplate.language.AngleBracketTemplateLexer.Lexer, None)

    def runTest(self):
        t = self.group.getInstanceOf("a")
        t["s"] = "Test"
        expecting = "case 1: Test break;"
        self.assertEqual(str(t), expecting)


class TestAngleBracketsWithGroupFile(unittest.TestCase):

    def setUp(self):
        self.filename = "test.stg";
        # mainly testing to ensure we don't get parse errors of above
        self.group = stringtemplate.StringTemplateGroup( \
            file("test.stg"), \
            stringtemplate.language.AngleBracketTemplateLexer.Lexer, None)

    def runTest(self):
        t = self.group.getInstanceOf("a")
        t["s"] = "Test"
        expecting = "case 1: Test break;"
        self.assertEqual(str(t), expecting)


class TestAngleBracketsNoGroup(unittest.TestCase):

    def runTest(self):
        st = stringtemplate.StringTemplate( \
            "Tokens: <rules; separator=\"|\"> ;",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer)
        st["rules"] = "A"
        st["rules"] = "B"
        expecting = "Tokens: A|B ;"
        self.assertEqual(str(st), expecting)


class TestSimpleInheritance(unittest.TestCase):

    # make a bold template in the super group that you can inherit from sub
    def runTest(self):
        supergroup = stringtemplate.StringTemplateGroup("super")
        subgroup = stringtemplate.StringTemplateGroup("sub")
        bold = supergroup.defineTemplate("bold", "<b>$it$</b>")
        subgroup.setSuperGroup(supergroup)
        errors = ErrorBuffer()
        subgroup.setErrorListener(errors)
        supergroup.setErrorListener(errors)
        duh = stringtemplate.StringTemplate(subgroup, "$name:bold()$")
        duh["name"] = "Terence"
        expecting = "<b>Terence</b>"
        self.assertEqual(str(duh), expecting)


class TestOverrideInheritance(unittest.TestCase):

    # make a bold template in the super group and one in sub group
    def runTest(self):
        supergroup = stringtemplate.StringTemplateGroup("super")
        subgroup = stringtemplate.StringTemplateGroup("sub")
        supergroup.defineTemplate("bold", "<b>$it$</b>")
        subgroup.defineTemplate("bold", "<strong>$it$</strong>")
        subgroup.setSuperGroup(supergroup)
        errors = ErrorBuffer()
        subgroup.setErrorListener(errors)
        supergroup.setErrorListener(errors)
        duh = stringtemplate.StringTemplate(subgroup, "$name:bold()$")
        duh["name"] = "Terence"
        expecting = "<strong>Terence</strong>"
        self.assertEqual(str(duh), expecting)


class TestMultiLevelInheritance(unittest.TestCase):

    # must loop up two levels to find bold()
    def runTest(self):
        rootgroup = stringtemplate.StringTemplateGroup("root")
        level1 = stringtemplate.StringTemplateGroup("level1")
        level2 = stringtemplate.StringTemplateGroup("level2")
        rootgroup.defineTemplate("bold", "<b>$it$</b>")
        level1.setSuperGroup(rootgroup)
        level2.setSuperGroup(level1)
        errors = ErrorBuffer()
        rootgroup.setErrorListener(errors)
        level1.setErrorListener(errors)
        level2.setErrorListener(errors)
        duh = stringtemplate.StringTemplate(level2, "$name:bold()$")
        duh["name"] = "Terence"
        expecting = "<b>Terence</b>"
        self.assertEqual(str(duh), expecting)


class TestExprInParens(unittest.TestCase):

    # specify a template to apply to an attribute
    # Use a template group so we can specify the start/stop chars
    def runTest(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        bold = group.defineTemplate("bold", "<b>$it$</b>")
        duh = stringtemplate.StringTemplate( \
	    group, "$(\"blort: \"+(list)):bold()$")
        duh["list"] = "a"
        duh["list"] = "b"
        duh["list"] = "c"
        # sys.stderr.write(str(duh) + os.linesep)
        expecting = "<b>blort: abc</b>"
        self.assertEqual(str(duh), expecting)


class TestMultipleAdditions(unittest.TestCase):

    # specify a template to apply to an attribute
    # Use a template group so we can specify the start/stop chars
    def runTest(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        group.defineTemplate("link", "<a href=\"$url$\"><b>$title$</b></a>")
        duh = stringtemplate.StringTemplate( \
            group,
            "$link(url=\"/member/view?ID=\"+ID+\"&x=y\"+foo, title=\"the title\")$")
        duh["ID"] = "3321"
        duh["foo"] = "fubar"
        expecting = "<a href=\"/member/view?ID=3321&x=yfubar\"><b>the title</b></a>"
        self.assertEqual(str(duh), expecting)


class TestCollectionAttributes(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("test")
        bold = group.defineTemplate("bold", "<b>$it$</b>")
        t = stringtemplate.StringTemplate(group, "$data$, $data:bold()$, "+ \
                           "$list:bold():bold()$, $tuple:bold()$, $map$")
	data = 123
        list_ = [1, 2, 3]
        tuple_ = ('a', 'b', 'c')
        map_ = { "First":1, "Second":2, "Third":3 }
	t["data"] = data
        t["list"] = list_
        t["tuple"] = tuple_
        t["map"] = map_
        # sys.stderr.write(str(t) + os.linesep)
        expecting = "123, <b>123</b>, <b><b>1</b></b><b><b>2</b></b>" + \
	    "<b><b>3</b></b>, <b>a</b><b>b</b><b>c</b>, 231"
        self.assertEqual(str(t), expecting)


class TestParenthesizedExpression(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("test")
        bold = group.defineTemplate("bold", "<b>$it$</b>")
        t = stringtemplate.StringTemplate(group, "$(first+last):bold()$")
        t["first"] = "Joe"
        t["last"] = "Schmoe"
        # sys.stderr.write(t + os.linesep)
        expecting="<b>JoeSchmoe</b>"
        self.assertEqual(str(t), expecting)


class TestApplyTemplateNameExpression(unittest.TestCase):

    def runTest(self):
	group = stringtemplate.StringTemplateGroup("test")
	bold = group.defineTemplate("foobar", "foo$attr$bar")
	t = stringtemplate.StringTemplate(group, "$data:(name+\"bar\")()$")
	t["data"] = "Ter"
	t["data"] = "Tom"
	t["name"] = "foo"
	# sys.stderr.write(str(t) + '\n')
	expecting="fooTerbarfooTombar"
	self.assertEqual(str(t), expecting)


class TestTemplateNameExpression(unittest.TestCase):

    def runTest(self):
	group = stringtemplate.StringTemplateGroup("test")
	foo = group.defineTemplate("foo", "hi there!")
	t = stringtemplate.StringTemplate(group, "$(name)()$")
	t["name"] = "foo"
	# sys.stderr.write(str(t) + '\n')
	expecting="hi there!"
	self.assertEqual(str(t), expecting)


class TestMissingEndDelimiter(unittest.TestCase):

    def runTest(self):
	debugMode = stringtemplate.StringTemplate.inDebugMode()
	stringtemplate.StringTemplate.setDebugMode(False)
	group = stringtemplate.StringTemplateGroup("test")
	errors = ErrorBuffer()
	group.setErrorListener(errors)
	t = stringtemplate.StringTemplate(group, "stuff $a then more junk etc...")
	expectingError="problem parsing template 'anonymous' : line 1:31: expecting '$', found '<EOF>'\n"
	# sys.stderr.write("error: '"+errors+"'\n")
	# sys.stderr.write("expecting: '"+expectingError+"'\n")
	stringtemplate.StringTemplate.setDebugMode(debugMode)
	self.assertEqual(str(errors), expectingError)


class TestSetButNotRefd(unittest.TestCase):

    def runTest(self):
	stringtemplate.StringTemplate.setLintMode(True)
	debugMode = stringtemplate.StringTemplate.inDebugMode()
	stringtemplate.StringTemplate.setDebugMode(False)
	group = stringtemplate.StringTemplateGroup("test")
	errors = ErrorBuffer()
	group.setErrorListener(errors)
	t = stringtemplate.StringTemplate(group, "$a$ then $b$ and $c$ refs.")
	t["a"] = "Terence"
	t["b"] = "Terence"
	t["cc"] = "Terence" # oops...should be 'c'
	expectingError="anonymous: set but not used: cc\n"
	result = str(t)    # result is irrelevant
	# sys.stderr.write("result error: '"+str(errors)+"'\n")
	# sys.stderr.write("expecting: '"+expectingError+"'\n")
	stringtemplate.StringTemplate.setLintMode(False)
	stringtemplate.StringTemplate.setDebugMode(debugMode)
	self.assertEqual(str(errors), expectingError)


class TestNullTemplateApplication(unittest.TestCase):

    def runTest(self):
	group = stringtemplate.StringTemplateGroup("test")
	errors = ErrorBuffer()
	group.setErrorListener(errors)
	t = stringtemplate.StringTemplate(group, "$names:bold(x=it)$")
	t["names"] = "Terence"
	# sys.stderr.write(str(t)+'\n')
	#expecting="" # bold not found...empty string
	#self.assertEqual(str(t), expecting)
	expecting = "Can't load template bold.st"
	error = ""
	try:
	    result = str(t)
	except ValueError, ve:
	    error = str(ve)
	self.assertEqual(error, expecting)


class TestNullTemplateToMultiValuedApplication(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("test")
	errors = ErrorBuffer()
	group.setErrorListener(errors)
	t = stringtemplate.StringTemplate(group, "$names:bold(x=it)$")
	t["names"] = "Terence"
	t["names"] = "Tom"
	# sys.stderr.write(str(t)+'\n')
	#expecting="" # bold not found...empty string
	#self.assertEqual(str(t), expecting)
	expecting = "Can't load template bold.st"
	error = ""
	try:
	    result = str(t)
	except ValueError, ve:
	    error = str(ve)
	self.assertEqual(error, expecting)


class TestChangingAttrValueTemplateApplicationToVector(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("test")
        italics = group.defineTemplate("italics", "<i>$x$</i>")
        bold = group.defineTemplate("bold", "<b>$x$</b>")
        t = stringtemplate.StringTemplate(group, "$names:bold(x=it)$")
	t["names"] = "Terence"
	t["names"] = "Tom"
	# sys.stderr.write("'"+str(t)+"'\n")
	expecting="<b>Terence</b><b>Tom</b>"
	self.assertEqual(str(t), expecting)


class TestChangingAttrValueRepeatedTemplateApplicationToVector(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        bold = group.defineTemplate("bold", "<b>$item$</b>")
        italics = group.defineTemplate("italics", "<i>$it$</i>")
        members = stringtemplate.StringTemplate(group, "$members:bold(item=it):italics(it=it)$")
	members["members"] = "Jim"
	members["members"] = "Mike"
	members["members"] = "Ashar"
	# sys.stderr.write("members="+members+'\n')
	expecting = "<i><b>Jim</b></i><i><b>Mike</b></i><i><b>Ashar</b></i>"
	self.assertEqual(str(members), expecting)


class TestAlternatingTemplateApplication(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        listItem = group.defineTemplate("listItem", "<li>$it$</li>")
        bold = group.defineTemplate("bold", "<b>$it$</b>")
        italics = group.defineTemplate("italics", "<i>$it$</i>")
        item = stringtemplate.StringTemplate(group, "$item:bold(),italics():listItem()$")
	item["item"] = "Jim"
	item["item"] = "Mike"
	item["item"] = "Ashar"
	# sys.stderr.write("ITEM="+item+'\n')
	expecting = "<li><b>Jim</b></li><li><i>Mike</i></li><li><b>Ashar</b></li>"
	self.assertEqual(str(item), expecting)


class TestExpressionAsRHSOfAssignment(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("test")
        hostname = group.defineTemplate("hostname", "$machine$.jguru.com")
        bold = group.defineTemplate("bold", "<b>$x$</b>")
        t = stringtemplate.StringTemplate(group, "$bold(x=hostname(machine=\"www\"))$")
	expecting="<b>www.jguru.com</b>"
	self.assertEqual(str(t), expecting)


class TestTemplateApplicationAsRHSOfAssignment(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("test")
        hostname = group.defineTemplate("hostname", "$machine$.jguru.com")
        bold = group.defineTemplate("bold", "<b>$x$</b>")
        italics = group.defineTemplate("italics", "<i>$it$</i>")
        t = stringtemplate.StringTemplate(group, "$bold(x=hostname(machine=\"www\"):italics())$")
	expecting="<b><i>www.jguru.com</i></b>"
	self.assertEqual(str(t), expecting)


class TestParameterAndAttributeScoping(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("test")
        italics = group.defineTemplate("italics", "<i>$x$</i>")
        bold = group.defineTemplate("bold", "<b>$x$</b>")
        t = stringtemplate.StringTemplate(group, "$bold(x=italics(x=name))$")
	t["name"] = "Terence"
	# sys.stderr.write(str(t)+'\n')
	expecting="<b><i>Terence</i></b>"
	self.assertEqual(str(t), expecting)


class TestComplicatedSeparatorExpr(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("test")
        bold = group.defineTemplate("bulletSeparator", "</li>$foo$<li>")
	# make separator a complicated expression with args passed to included template
        t = stringtemplate.StringTemplate(group,
            "<ul>$name; separator=bulletSeparator(foo=\" \")+\"&nbsp;\"$</ul>")
	t["name"] = "Ter"
	t["name"] = "Tom"
	t["name"] = "Mel"
	# sys.stderr.write(str(t)+'\n')
	expecting = "<ul>Ter</li> <li>&nbsp;Tom</li> <li>&nbsp;Mel</ul>"
	self.assertEqual(str(t), expecting)


class TestAttributeRefButtedUpAgainstEndifAndWhitespace(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("test")
        a = stringtemplate.StringTemplate(group,
            "$if (!firstName)$$email$$endif$")
	a["email"] = "parrt@jguru.com"
	expecting = "parrt@jguru.com"
	self.assertEqual(str(a), expecting)


class TestStringCatenationOnSingleValuedAttribute(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("test")
        bold = group.defineTemplate("bold", "<b>$it$</b>")
        a = stringtemplate.StringTemplate(group, "$name+\" Parr\":bold()$")
        b = stringtemplate.StringTemplate(group, "$bold(it=name+\" Parr\")$")
	a["name"] = "Terence"
	b["name"] = "Terence"
	expecting = "<b>Terence Parr</b>"
	self.assertEqual(str(a), expecting)
	self.assertEqual(str(b), expecting)


class TestApplyingTemplateFromDiskWithPrecompiledIF(unittest.TestCase):

    def runTest(self):
        # write the template files first to /tmp
        fw = file("/tmp/page.st", 'w')
        fw.write("<html><head>"+os.linesep)
        # fw.write("  <title>PeerScope: $title$</title>"+os.linesep)
        fw.write("</head>"+os.linesep)
        fw.write("<body>"+os.linesep)
        fw.write("$if(member)$User: $member:terse()$$endif$"+os.linesep)
        fw.write("</body>"+os.linesep)
        fw.write("</head>"+os.linesep)
        fw.close()

        fw = file("/tmp/terse.st", 'w')
        fw.write("$it.firstName$ $it.lastName$ (<tt>$it.email$</tt>)"+os.linesep)
        fw.close()

        # specify a template to apply to an attribute
        # Use a template group so we can specify the start/stop chars
        group = stringtemplate.StringTemplateGroup("dummy", "/tmp")
	# sys.stderr.write(str(group)+"\n")

        a = group.getInstanceOf("page")
        a.setAttribute("member", Connector())
        expecting = "<html><head>"+os.linesep+ \
            "</head>"+os.linesep+ \
            "<body>"+os.linesep+ \
            "User: Terence Parr (<tt>parrt@jguru.com</tt>)"+os.linesep+ \
            "</body>"+os.linesep+ \
            "</head>"
        # sys.stderr.write("'"+a+"'\n")
        self.assertEqual(str(a), expecting)


class TestMultiValuedAttributeWithAnonymousTemplateUsingIndexVariableI(unittest.TestCase):

    def runTest(self):
        tgroup = stringtemplate.StringTemplateGroup("dummy", ".")
        t = stringtemplate.StringTemplate(tgroup,
            " List:"+os.linesep+"  "+os.linesep+"foo"+os.linesep+os.linesep+ \
            "$names:{<br>$i$. $it$"+os.linesep+ \
            "}$")
        t["names"] = "Terence"
        t["names"] = "Jim"
        t["names"] = "Sriram"
        # sys.stderr.write(str(t)+'\n')
        expecting = \
            " List:"+os.linesep+ \
            "  "+os.linesep+ \
            "foo"+os.linesep+os.linesep+ \
            "<br>1. Terence"+os.linesep+ \
            "<br>2. Jim"+os.linesep+ \
            "<br>3. Sriram"+os.linesep
        self.assertEqual(str(t), expecting)


class TestDictAttributeWithAnonymousTemplateUsingKeyVariableIk(unittest.TestCase):

    def runTest(self):
        tgroup = stringtemplate.StringTemplateGroup("dummy", ".")
        t = stringtemplate.StringTemplate(tgroup,
            " Dict:"+os.linesep+"  "+os.linesep+"foo"+os.linesep+os.linesep+ \
            "$data:{<br>$ik$: $it$"+os.linesep+ \
            "}$")
	map_ = {}
        t["data"] = map_
        map_["name"] = "Jimmy"
        map_["family"] = "Neutron"
	map_["encoding"] = "Strange"
	map_["year"] = 2005
        # sys.stderr.write(str(t)+'\n')
        expecting = \
            " Dict:"+os.linesep+ \
            "  "+os.linesep+ \
            "foo"+os.linesep+os.linesep+ \
	    "<br>year: 2005"+os.linesep+ \
            "<br>name: Jimmy"+os.linesep+ \
            "<br>family: Neutron"+os.linesep+ \
            "<br>encoding: Strange"+os.linesep
        self.assertEqual(str(t), expecting)


class TestFindTemplateInCurrentDir(unittest.TestCase):

    def runTest(self):
        # Look for templates in the current directory
        mgroup = stringtemplate.StringTemplateGroup("method stuff", ".",
	    stringtemplate.language.AngleBracketTemplateLexer.Lexer)
        m = mgroup.getInstanceOf("method")
	# "method.st" references body() so "body.st" will be loaded too
	m["visibility"] = "public"
	m["name"] = "foobar"
	m["returnType"] = "void"
	m["statements"] = "i=1;" # body inherits these from method
	m["statements"] = "x=i;"
	expecting = \
            "public void foobar() {"+os.linesep+ \
            "\t// start of a body"+os.linesep+ \
            "\ti=1;"+os.linesep+ \
            "\tx=i;"+os.linesep+ \
            "\t// end of a body"+os.linesep+ \
            "}"
	# sys.stderr.write(str(m)+'\n')
	self.assertEqual(str(m), expecting)


class TestTiming(unittest.TestCase):

    def setUp(self):
	self.group = stringtemplate.StringTemplateGroup('mygroup', 'templates')
        self.group.setRootDir('.')
        self.table = self.group.getInstanceOf('table')

    def runTest(self):
        s = time.time()
        for line in range(0,50):
            cell = self.group.getInstanceOf('cell')
            cell['data'] = 'X'
            self.table['cell'] = cell
	expecting = \
          "<table>" + os.linesep + \
          "<tr>" + os.linesep + \
          "<td>X</td><td>X</td><td>X</td><td>X</td><td>X</td>" + \
          "<td>X</td><td>X</td><td>X</td><td>X</td><td>X</td>" + \
          "<td>X</td><td>X</td><td>X</td><td>X</td><td>X</td>" + \
          "<td>X</td><td>X</td><td>X</td><td>X</td><td>X</td>" + \
          "<td>X</td><td>X</td><td>X</td><td>X</td><td>X</td>" + \
          "<td>X</td><td>X</td><td>X</td><td>X</td><td>X</td>" + \
          "<td>X</td><td>X</td><td>X</td><td>X</td><td>X</td>" + \
          "<td>X</td><td>X</td><td>X</td><td>X</td><td>X</td>" + \
          "<td>X</td><td>X</td><td>X</td><td>X</td><td>X</td>" + \
          "<td>X</td><td>X</td><td>X</td><td>X</td><td>X</td>" + os.linesep + \
          "</tr>" + os.linesep + \
          "</table>"
	result = str(self.table)
        e = time.time()
	self.assertEqual(result, expecting)
        self.assert_((e-s) < 1.0)


class TestApplyTemplateToSingleValuedAttribute(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("test")
        bold = group.defineTemplate("bold", "<b>$x$</b>")
        name = stringtemplate.StringTemplate(group, "$name:bold(x=name)$")
	name["name"] = "Terence"
	self.assertEqual(str(name), "<b>Terence</b>")


class TestStringLiteralAsAttribute(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("test")
        bold = group.defineTemplate("bold", "<b>$it$</b>")
        name = stringtemplate.StringTemplate(group, "$\"Terence\":bold()$")
	self.assertEqual(str(name), "<b>Terence</b>")


class TestApplyTemplateToSingleValuedAttributeWithDefaultAttribute(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("test")
        bold = group.defineTemplate("bold", "<b>$it$</b>")
        name = stringtemplate.StringTemplate(group, "$name:bold()$")
	name["name"] = "Terence"
	self.assertEqual(str(name), "<b>Terence</b>")


class TestApplyAnonymousTemplateToSingleValuedAttribute(unittest.TestCase):

    def runTest(self):
        # specify a template to apply to an attribute
	# Use a template group so we can specify the start/stop chars
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        item = stringtemplate.StringTemplate(group, "$item:{<li>$it$</li>}$")
	item["item"] = "Terence"
	self.assertEqual(str(item), "<li>Terence</li>")


class TestApplyAnonymousTemplateToMultiValuedAttribute(unittest.TestCase):

    def runTest(self):
        # specify a template to apply to an attribute
	# Use a template group so we can specify the start/stop chars
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        list = stringtemplate.StringTemplate(group, "<ul>$items$</ul>")
	# demonstrate setting arg to anonymous subtemplate
        item = stringtemplate.StringTemplate(group, "$item:{<li>$it$</li>}; separator=\",\"$")
	item["item"] = "Terence"
	item["item"] = "Jim"
	item["item"] = "John"
	list.setAttribute("items", item); # nested template
	self.assertEqual(str(list), "<ul><li>Terence</li>,<li>Jim</li>,<li>John</li></ul>")


class TestApplyAnonymousTemplateToAggregateAttribute(unittest.TestCase):

    def runTest(self):
        st = stringtemplate.StringTemplate("$items:{$it.last$, $it.first$\n}$")
	st.setAttribute("items.{first,last}", "Ter", "Parr")
	st.setAttribute("items.{first,last}", "Tom", "Burns")
	expecting = \
            "Parr, Ter"+os.linesep + \
            "Burns, Tom"+os.linesep
	self.assertEqual(str(st), expecting)


class TestRepeatedApplicationOfTemplateToSingleValuedAttribute(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        search = group.defineTemplate("bold", "<b>$it$</b>")
        item = stringtemplate.StringTemplate(group, "$item:bold():bold()$")
	item["item"] = "Jim"
	self.assertEqual(str(item), "<b><b>Jim</b></b>")


class TestRepeatedApplicationOfTemplateToMultiValuedAttributeWithSeparator(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        search = group.defineTemplate("bold", "<b>$it$</b>")
        item = stringtemplate.StringTemplate(group, "$item:bold():bold(); separator=\",\"$")
	item["item"] = "Jim"
	item["item"] = "Mike"
	item["item"] = "Ashar"
	# first application of template must yield another vector!
	# sys.stderr.write("ITEM="+item+'\n')
	self.assertEqual(str(item), "<b><b>Jim</b></b>,<b><b>Mike</b></b>,<b><b>Ashar</b></b>")


# NEED A TEST OF obj ASSIGNED TO ARG?

class TestMultiValuedAttributeWithSeparator(unittest.TestCase):

    def runTest(self):
	# if column can be multi-valued, specify a separator
        group = stringtemplate.StringTemplateGroup("dummy", ".",
	    stringtemplate.language.AngleBracketTemplateLexer.Lexer)
	query = stringtemplate.StringTemplate(group, "SELECT <distinct> <column; separator=\", \"> FROM <table>;")
	query["column"] = "name"
	query["column"] = "email"
	query["table"] = "User"
	# uncomment next line to make "DISTINCT" appear in output
	# query["distince"] = "DISTINCT"
	# sys.stderr.write(str(query)+'\n')
	self.assertEqual(str(query), "SELECT  name, email FROM User;")


class TestSingleValuedAttributes(unittest.TestCase):

    def runTest(self):
        # all attributes are single-valued:
        query = stringtemplate.StringTemplate("SELECT $column$ FROM $table$;")
	query["column"] = "name"
	query["table"] = "User"
	# sys.stderr.write(str(query)+'\n')
	self.assertEqual(str(query), "SELECT name FROM User;")


class TestIFTemplate(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".",
	    stringtemplate.language.AngleBracketTemplateLexer.Lexer)
        t = stringtemplate.StringTemplate(group, \
                               "SELECT <column> FROM PERSON "+ \
                               "<if(cond)>WHERE ID=<id><endif>;")
	t["column"] = "name"
	t["cond"] = "True"
	t["id"] = "231"
	self.assertEqual(str(t), "SELECT name FROM PERSON WHERE ID=231;")


### As of 2.0, you can test a boolean value
#
class TestIFBoolean(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        t = stringtemplate.StringTemplate(group, \
            "$if(b)$x$endif$ $if(!b)$y$endif$")
	t.setAttribute("b", True)
	self.assertEqual(str(t), "x ")

	t = t.getInstanceOf()
	t.setAttribute("b", False)
	self.assertEqual(str(t), " y")


class TestNestedIFTemplate(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".",
	    stringtemplate.language.AngleBracketTemplateLexer.Lexer)
        t = stringtemplate.StringTemplate(group, \
                        		  "ack<if(a)>"+os.linesep+ \
                        		  "foo"+os.linesep+ \
                        		  "<if(!b)>stuff<endif>"+os.linesep+ \
                        		  "<if(b)>no<endif>"+os.linesep+ \
                        		  "junk"+os.linesep+ \
                        		  "<endif>")
	t["a"] = "blort"
	# leave b as None
	# sys.stderr.write("t="+t+'\n')
	expecting = \
            "ackfoo"+os.linesep+ \
            "stuff"+os.linesep+ \
            "junk"
	self.assertEqual(str(t), expecting)


class Connector(object):

    def getID(self):
        return 1

    def getFirstName(self):
        return "Terence"

    def getLastName(self):
        return "Parr"

    def getEmail(self):
        return "parrt@jguru.com"

    def getBio(self):
        return "Superhero by night..."

    ### As of 2.0, booleans work as you expect.  In 1.x,
    #   a missing value simulated a boolean.
    #
    def getCanEdit(self):
        return False


class Connector2(object):

    def getID(self):
        return 2

    def getFirstName(self):
        return "Tom"

    def getLastName(self):
        return "Burns"

    def getEmail(self):
        return "tombu@jguru.com"

    def getBio(self):
        return "Superhero by day..."

    def getCanEdit(self):
        return True


class TestObjectPropertyReference(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        t = stringtemplate.StringTemplate(group, \
        	"<b>Name: $p.firstName$ $p.lastName$</b><br>"+os.linesep+ \
        	"<b>Email: $p.email$</b><br>"+os.linesep+ \
        	"$p.bio$")
	t.setAttribute("p", Connector())
	# sys.stderr.write("t is "+str(t)+'\n')
	expecting = \
            "<b>Name: Terence Parr</b><br>"+os.linesep+ \
            "<b>Email: parrt@jguru.com</b><br>"+os.linesep+ \
            "Superhero by night..."
	self.assertEqual(str(t), expecting)


class TestApplyRepeatedAnonymousTemplateWithForeignTemplateRefToMultiValuedAttribute(unittest.TestCase):

    def runTest(self):
        # specify a template to apply to an attribute
	# Use a template group so we can specify the start/stop chars
        group = stringtemplate.StringTemplateGroup("dummy", ".")
	group.defineTemplate("link", "<a href=\"$url$\"><b>$title$</b></a>")
        duh = stringtemplate.StringTemplate(group, \
            "start|$p:{$link(url=\"/member/view?ID=\"+it.ID, title=it.firstName)$ $if(it.canEdit)$canEdit$endif$}:"+ \
            "{$it$<br>\n}$|end")
	duh.setAttribute("p", Connector())
	duh.setAttribute("p", Connector2())
	# sys.stderr.write(str(duh)+'\n')
	expecting = "start|<a href=\"/member/view?ID=1\"><b>Terence</b></a> <br>"+os.linesep+ \
            "<a href=\"/member/view?ID=2\"><b>Tom</b></a> canEdit<br>"+os.linesep+ \
            "|end"
	self.assertEqual(str(duh), expecting)


class Tree(object):

    def __init__(self, t):
	self.children = []
	self.text = t

    def getText(self):
        return self.text

    def addChild(self, c):
        self.children.append(c)

    def getFirstChild(self):
        if len(self.children) == 0:
            return None
        return self.children[0]

    def getChildren(self):
        return self.children


class TestRecursion(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".",
	    stringtemplate.language.AngleBracketTemplateLexer.Lexer)
	group.defineTemplate("tree", \
                             "<if(it.firstChild)>"+ \
                             "( <it.text> <it.children:tree(); separator=\" \"> )" + \
                             "<else>" + \
                             "<it.text>" + \
                             "<endif>")
        tree = group.getInstanceOf("tree")
	# build ( a b (c d) e )
	root = Tree("a")
	root.addChild(Tree("b"))
	subtree = Tree("c")
	subtree.addChild(Tree("d"))
	root.addChild(subtree)
	root.addChild(Tree("e"))
	tree.setAttribute("it", root)
	expecting = "( a b ( c d ) e )"
	self.assertEqual(str(tree), expecting)


class TestNestedAnonymousTemplates(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        t = stringtemplate.StringTemplate( \
        	group, \
        	"$A:{" + os.linesep + \
        	"<i>$it:{" + os.linesep + \
        	"<b>$it$</b>" + os.linesep + \
        	"}$</i>" + os.linesep + \
        	"}$")
	t["A"] = "parrt"
	expecting = os.linesep + \
            "<i>" + os.linesep + \
            "<b>parrt</b>" + os.linesep + \
            "</i>" + os.linesep
	self.assertEqual(str(t), expecting)


class TestAnonymousTemplateAccessToEnclosingAttributes(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        t = stringtemplate.StringTemplate( \
        	group, \
        	"$A:{" + os.linesep + \
        	"<i>$it:{" + os.linesep + \
        	"<b>$it$, $B$</b>" + os.linesep + \
        	"}$</i>" + os.linesep + \
        	"}$")
	t["A"] = "parrt"
	t["B"] = "tombu"
	expecting = os.linesep + \
            "<i>" + os.linesep + \
            "<b>parrt, tombu</b>" + os.linesep + \
            "</i>" + os.linesep
	self.assertEqual(str(t), expecting)


class TestNestedAnonymousTemplatesAgain(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        t = stringtemplate.StringTemplate( \
        	group, \
        	"<table>"+os.linesep + \
        	"$names:{<tr>$it:{<td>$it:{<b>$it$</b>}$</td>}$</tr>}$"+os.linesep + \
        	"</table>"+os.linesep
        	)
	t["names"] = "parrt"
	t["names"] = "tombu"
	expecting = \
            "<table>" + os.linesep + \
            "<tr><td><b>parrt</b></td></tr><tr><td><b>tombu</b></td></tr>" + os.linesep + \
            "</table>" + os.linesep
	self.assertEqual(str(t), expecting)


class TestEscapes(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".")
	group.defineTemplate("foo", "$x$ && $it$")
        t = stringtemplate.StringTemplate( \
        	group, \
        	"$A:foo(x=\"dog\\\"\\\"\")$") # $A:foo("dog\"\"")$
        u = stringtemplate.StringTemplate( \
        	group, \
        	"$A:foo(x=\"dog\\\"g\")$") # $A:foo(x="dog\"g")$
        v = stringtemplate.StringTemplate( \
        	group, \
        	"$A:{$it:foo(x=\"\\{dog\\}\\\"\")$ is cool}$")
        	# $A:{$attr:foo(x="\:dog\\"")$ is nested}$

	t["A"] = "ick"
	# sys.stderr.write("t is '"+str(t)+"'\n")
	expecting = "dog\"\" && ick"
	self.assertEqual(str(t), expecting)

	u["A"] = "ick"
	# sys.stderr.write("u is '"+str(u)+"'\n")
	expecting = "dog\"g && ick"
	self.assertEqual(str(u), expecting)

	v["A"] = "ick"
	# sys.stderr.write("v is '"+str(v)+"'\n")
	expecting = "{dog}\" && ick is cool"
	self.assertEqual(str(v), expecting)


class TestEscapesOutsideExpressions(unittest.TestCase):

    def runTest(self):
        b = stringtemplate.StringTemplate("It\\'s ok...\\$; $a:{\\'hi\\', $it$}$")
	b["a"] = "Ter"
	expecting = "It\\'s ok...$; \\'hi\\', Ter"
	result = str(b)
	self.assertEqual(result, expecting)


class TestElseClause(unittest.TestCase):

    def runTest(self):
        e = stringtemplate.StringTemplate( \
            "$if(title)$"+os.linesep + \
            "foo"+os.linesep + \
            "$else$"+os.linesep + \
            "bar"+os.linesep + \
            "$endif$")
	e["title"] = "sample"
	expecting = "foo"
	self.assertEqual(str(e), expecting)

	e = e.getInstanceOf()
	expecting = "bar"
	self.assertEqual(str(e), expecting)


class TestNestedIF(unittest.TestCase):

    def runTest(self):
        e = stringtemplate.StringTemplate( \
            "$if(title)$"+os.linesep + \
            "foo"+os.linesep + \
            "$else$"+os.linesep + \
            "$if(header)$"+os.linesep + \
            "bar"+os.linesep + \
            "$else$"+os.linesep + \
            "blort"+os.linesep + \
            "$endif$"+os.linesep + \
            "$endif$")
	e["title"] = "sample"
	expecting = "foo"
	self.assertEqual(str(e), expecting)

	e = e.getInstanceOf()
	e["header"] = "more"
	expecting = "bar"
	self.assertEqual(str(e), expecting)

	e = e.getInstanceOf()
	expecting = "blort"
	self.assertEqual(str(e), expecting)


class TestEmbeddedMultiLineIF(unittest.TestCase):

    def setUp(self):
        self.group = stringtemplate.StringTemplateGroup("test")
        self.main = stringtemplate.StringTemplate(self.group, "$sub$")
        self.sub = stringtemplate.StringTemplate(self.group, 
            "begin" +os.linesep+ \
            "$if(foo)$" +os.linesep+ \
            "$foo$" +os.linesep+ \
            "$else$" +os.linesep+ \
            "blort" +os.linesep+ \
            "$endif$" +os.linesep
	    )

    def runTest(self):
	self.sub["foo"] = "stuff"
	self.main["sub"] = self.sub
	expecting = \
            "begin" +os.linesep+ \
            "stuff"
	self.assertEqual(str(self.main), expecting)

        self.main = stringtemplate.StringTemplate(self.group, "$sub$")
	self.sub = self.sub.getInstanceOf()
	self.main["sub"] = self.sub
        expecting = \
            "begin"+os.linesep+ \
            "blort"
        self.assertEqual(str(self.main), expecting)


class TestSimpleIndentOfAttributeList(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "list(names)::= <<" + \
            "  $names; separator=\"\n\"$"+os.linesep+ \
            ">>"+os.linesep
        self.errors = ErrorBuffer()
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
							self.errors)

    def runTest(self):
        t = self.group.getInstanceOf("list")
        t["names"] = "Terence"
        t["names"] = "Jim"
        t["names"] = "Sriram"
        expecting = \
            "  Terence"+os.linesep+ \
            "  Jim"+os.linesep+ \
            "  Sriram"
        self.assertEqual(str(t), expecting)


class TestIndentOfMultilineAttributes(unittest.TestCase):
    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "list(names)::= <<" + \
            "  $names; separator=\"\n\"$"+os.linesep+ \
            ">>"+os.linesep
        self.errors = ErrorBuffer()
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
							self.errors)

    def runTest(self):
        t = self.group.getInstanceOf("list")
        t["names"] = "Terence\nis\na\nmaniac"
        t["names"] = "Jim"
        t["names"] = "Sriram\nis\ncool"
        expecting = \
            "  Terence"+os.linesep+ \
            "  is"+os.linesep+ \
            "  a"+os.linesep+ \
            "  maniac"+os.linesep+ \
            "  Jim"+os.linesep+ \
            "  Sriram"+os.linesep+ \
            "  is"+os.linesep+ \
            "  cool"
        self.assertEqual(str(t), expecting)


class TestIndentOfMultipleBlankLines(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "list(names)::= <<" + \
            "  $names$"+os.linesep+ \
            ">>"+os.linesep
        self.errors = ErrorBuffer()
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
							self.errors)

    def runTest(self):
        t = self.group.getInstanceOf("list")
        t["names"] = "Terence\n\nis a maniac"
        # no indent on blank line
        expecting = \
            "  Terence"+os.linesep+ \
            ""+os.linesep+ \
            "  is a maniac"
        self.assertEqual(str(t), expecting)


class TestIndentBetweenLeftJustifiedLiterals(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "list(names)::= <<" + \
            "Before:"+os.linesep + \
            "  $names; separator=\"\\n\"$"+os.linesep+ \
            "after" +os.linesep+ \
            ">>"+os.linesep
        self.errors = ErrorBuffer()
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
							self.errors)

    def runTest(self):
        t = self.group.getInstanceOf("list")
        t["names"] = "Terence"
        t["names"] = "Jim"
        t["names"] = "Sriram"
        expecting = \
            "Before:" +os.linesep+ \
            "  Terence"+os.linesep+ \
            "  Jim"+os.linesep+ \
            "  Sriram"+os.linesep+ \
            "after"
        self.assertEqual(str(t), expecting)


class TestNestedIndent(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "method(name,stats)::= <<" + \
            "void $name$() {"+os.linesep + \
            "\t$stats; separator=\"\\n\"$"+os.linesep+ \
            "}" +os.linesep+ \
            ">>"+os.linesep+ \
            "ifstat(expr,stats)::= <<"+os.linesep + \
            "if ($expr$) {"+os.linesep + \
            "  $stats; separator=\"\\n\"$"+os.linesep + \
            "}" + \
            ">>"+os.linesep + \
            "assign(lhs,expr)::= <<$lhs$=$expr$;>>"+os.linesep
        self.errors = ErrorBuffer()
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
							self.errors)

    def runTest(self):
        t = self.group.getInstanceOf("method")
        t["name"] = "foo"
        s1 = self.group.getInstanceOf("assign")
        s1["lhs"] = "x"
        s1["expr"] = "0"
        s2 = self.group.getInstanceOf("ifstat")
        s2["expr"] = "x>0"
        s2a = self.group.getInstanceOf("assign")
        s2a["lhs"] = "y"
        s2a["expr"] = "x+y"
        s2b = self.group.getInstanceOf("assign")
        s2b["lhs"] = "z"
        s2b["expr"] = "4"
        s2["stats"] = s2a
        s2["stats"] = s2b
        t["stats"] = s1
        t["stats"] = s2
        expecting = \
            "void foo() {"+os.linesep+ \
            "\tx=0;"+os.linesep+ \
            "\tif (x>0) {"+os.linesep+ \
            "\t  y=x+y;"+os.linesep+ \
            "\t  z=4;"+os.linesep+ \
            "\t}"+os.linesep+ \
            "}"
        self.assertEqual(str(t), expecting)


class TestApplyAnonymousTemplateToDictTupleAndList(unittest.TestCase):

    def runTest(self):
        st = stringtemplate.StringTemplate("$items:{<li>$it$</li>}$")
	m = {}
	m["a"] = "1"
	m["b"] = "2"
	m["c"] = "3"
	st["items"] = m
	expecting = "<li>1</li><li>3</li><li>2</li>"
	self.assertEqual(str(st), expecting)

	st = st.getInstanceOf()
	s = ["1", "2", "3"]
	st["items"] = s
	expecting = "<li>1</li><li>2</li><li>3</li>"
	self.assertEqual(str(st), expecting)

	st = st.getInstanceOf()
	t = ("1", "2", "3")
	st["items"] = t
	expecting = "<li>1</li><li>2</li><li>3</li>"
	self.assertEqual(str(st), expecting)


class TestApplyAnonymousTemplateToDictTupleAndList2(unittest.TestCase):

    def runTest(self):
        st = stringtemplate.StringTemplate("$items:{<li>$ik$</li>}$")
	m = {}
	m["a"] = "1"
	m["b"] = "2"
	m["c"] = "3"
	st["items"] = m
	expecting = "<li>a</li><li>c</li><li>b</li>"
	self.assertEqual(str(st), expecting)

	st = st.getInstanceOf()
	s = ["1", "2", "3"]
	st["items"] = s
	expecting = "<li></li><li></li><li></li>"
	self.assertEqual(str(st), expecting)

	st = st.getInstanceOf()
	t = ("1", "2", "3")
	st["items"] = t
	expecting = "<li></li><li></li><li></li>"
	self.assertEqual(str(st), expecting)


class TestDumpDictTupleAndList(unittest.TestCase):

    def runTest(self):
        st = stringtemplate.StringTemplate("$items; separator=\",\"$")
	m = {}
	m["a"] = "1"
	m["b"] = "2"
	m["c"] = "3"
	st["items"] = m
	expecting = "1,3,2"
	self.assertEqual(str(st), expecting)

	st = st.getInstanceOf()
	s = ["1", "2", "3"]
	st["items"] = s
	expecting = "1,2,3"
	self.assertEqual(str(st), expecting)

	st = st.getInstanceOf()
	t = ("1", "2", "3")
	st["items"] = t
	expecting = "1,2,3"
	self.assertEqual(str(st), expecting)


class Connector3(object):

    def getValues(self):
        return [1,2,3]

    def getStuff(self):
        return { "a":"1", "b":"2" }


class TestApplyAnonymousTemplateToListAndDictProperty(unittest.TestCase):

    def runTest(self):
        st = stringtemplate.StringTemplate("$x.values:{<li>$it$</li>}$")
	st.setAttribute("x", Connector3())
	expecting = "<li>1</li><li>2</li><li>3</li>"
	self.assertEqual(str(st), expecting)

	st = stringtemplate.StringTemplate("$x.stuff:{<li>$ik$: $it$</li>}$")
	st.setAttribute("x", Connector3())
	expecting = "<li>a: 1</li><li>b: 2</li>"
	self.assertEqual(str(st), expecting)


class TestSuperTemplateRef(unittest.TestCase):

    def runTest(self):
        # you can refer to a template defined in a super group via super.t()
        group = stringtemplate.StringTemplateGroup("super")
        subGroup = stringtemplate.StringTemplateGroup("sub")
        subGroup.setSuperGroup(group)
        group.defineTemplate("page", "$font()$:text")
        group.defineTemplate("font", "Helvetica")
        subGroup.defineTemplate("font", "$super.font()$ and Times")
        st = subGroup.getInstanceOf("page")
        expecting = "Helvetica and Times:text"
        self.assertEqual(str(st), expecting)


class TestApplySuperTemplateRef(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("super")
        subGroup = stringtemplate.StringTemplateGroup("sub")
        subGroup.setSuperGroup(group)
        group.defineTemplate("bold", "<b>$it$</b>")
        subGroup.defineTemplate("bold", "<strong>$it$</strong>")
        subGroup.defineTemplate("page", "$name:super.bold()$")
        st = subGroup.getInstanceOf("page")
        st["name"] = "Ter"
        expecting = "<b>Ter</b>"
        self.assertEqual(str(st), expecting)


class TestLazyEvalOfSuperInApplySuperTemplateRef(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("super")
        subGroup = stringtemplate.StringTemplateGroup("sub")
        subGroup.setSuperGroup(group)
        group.defineTemplate("bold", "<b>$it$</b>")
        subGroup.defineTemplate("bold", "<strong>$it$</strong>")
        # this is the same as testApplySuperTemplateRef() test
        # 'cept notice that here the supergroup defines page
        # As long as you create the instance via the subgroup,
        # "super." will evaluate lazily (i.e., not statically
        # during template compilation) to the templates
        # getGroup().superGroup value.  If I create instance
        # of page in group not subGroup, however, I will get
        # an error as superGroup is None for group "group".
        group.defineTemplate("page", "$name:super.bold()$")
        st = subGroup.getInstanceOf("page")
        st["name"] = "Ter"
        expecting = "<b>Ter</b>"
        self.assertEqual(str(st), expecting)


class TestTemplatePolymorphism(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("super")
        subGroup = stringtemplate.StringTemplateGroup("sub")
        subGroup.setSuperGroup(group)
        # bold is defined in both super and sub
        # if you create an instance of page via the subgroup,
        # then bold() should evaluate to the subgroup not the super
        # even though page is defined in the super.  Just like polymorphism.
        group.defineTemplate("bold", "<b>$it$</b>")
        group.defineTemplate("page", "$name:bold()$")
        subGroup.defineTemplate("bold", "<strong>$it$</strong>")
        st = subGroup.getInstanceOf("page")
        st["name"] = "Ter"
        expecting = "<strong>Ter</strong>"
        self.assertEqual(str(st), expecting)


class TestListOfEmbeddedTemplateSeesEnclosingAttributes(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "output(cond,items)::= <<page: $items$>>" +os.linesep+ \
            "mybody()::= <<$font()$stuff>>" +os.linesep+ \
            "font()::= <<$if(cond)$this$else$that$endif$>>"+os.linesep
	self.errors = ErrorBuffer()
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
							self.errors)

    def runTest(self):
        stringtemplate.StringTemplate.setLintMode(True)
        outputST = self.group.getInstanceOf("output")
        bodyST1 = self.group.getInstanceOf("mybody")
        bodyST2 = self.group.getInstanceOf("mybody")
        bodyST3 = self.group.getInstanceOf("mybody")
	outputST["items"] = bodyST1
	outputST["items"] = bodyST2
	outputST["items"] = bodyST3
	expecting = "page: thatstuffthatstuffthatstuff"
        stringtemplate.StringTemplate.setLintMode(False)
	self.assertEqual(str(outputST), expecting)


class TestInheritArgumentFromRecursiveTemplateApplication(unittest.TestCase):

    # do not inherit attributes through formal args
    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "block(stats)::= \"<stats>\"" + \
            "ifstat(stats)::= \"IF True then <stats>\""+os.linesep
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
            stringtemplate.language.AngleBracketTemplateLexer.Lexer)

    def runTest(self):
        b = self.group.getInstanceOf("block")
	b["stats"] = self.group.getInstanceOf("ifstat")
	b["stats"] = self.group.getInstanceOf("ifstat")
	expecting = "IF True then IF True then "
	result = str(b)
	# sys.stderr.write("result='"+result+"'\n")
	self.assertEqual(result, expecting)


class TestDeliberateRecursiveTemplateApplication(unittest.TestCase):

    # This test will cause infinite loop.  block contains a stat which
    # contains the same block.  Must be in lintMode to detect
    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "block(stats)::= \"<stats>\"" + \
            "ifstat(stats)::= \"IF True then <stats>\""+os.linesep
	stringtemplate.StringTemplate.resetTemplateCounter()
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
            stringtemplate.language.AngleBracketTemplateLexer.Lexer)
	stringtemplate.StringTemplate.setLintMode(True)

    def tearDown(self):
	stringtemplate.StringTemplate.setLintMode(False)

    def runTest(self):
        b = self.group.getInstanceOf("block")
        ifstat = self.group.getInstanceOf("ifstat")
	b["stats"] = ifstat # block has if stat
	expecting = "IF True then "
	expectingError = \
	    "infinite recursion to <ifstat(['attributes', 'stats'])@4> referenced in <block(['attributes', 'stats'])@3>; stack trace:"+os.linesep + \
	    "<ifstat(['attributes', 'stats'])@4>, attributes=[attributes], references=['stats']>"+os.linesep + \
	    "<block(['attributes', 'stats'])@3>, attributes=[attributes, stats=<ifstat()@4>], references=['stats']>"+os.linesep + \
	    "<ifstat(['attributes', 'stats'])@4> (start of recursive cycle)"+os.linesep + \
            "..."
	# note that attributes attribute doesn't show up in ifstat() because
	# recursion detection traps the problem before it writes out the
	# infinitely-recursive template; I set the attributes attribute right
	# before I render.
	errors = ""
        debugMode = stringtemplate.StringTemplate.inDebugMode()
        stringtemplate.StringTemplate.setDebugMode(True)
	try:
	    ifstat["stats"] = b # but make "if" contain block
            result = str(b)
	except stringtemplate.language.IllegalStateException, e:
            errors = str(e)
        except Exception, e:
            stringtemplate.StringTemplate.setDebugMode(debugMode)
            raise e
        stringtemplate.StringTemplate.setDebugMode(debugMode)

	# sys.stderr.write("errors="+errors+"'\n")
	# sys.stderr.write("expecting="+expectingError+"'\n")
	self.assertEqual(errors, expectingError)



class TestImmediateTemplateAsAttributeLoop(unittest.TestCase):

    # even though block has a stats value that refers to itself,
    # there is no recursion because each instance of block hides
    # the stats value from above since it's a formal arg.
    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "block(stats)::= \":<stats>\""
	self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
	    stringtemplate.language.AngleBracketTemplateLexer.Lexer)

    def runTest(self):
        b = self.group.getInstanceOf("block")
	b["stats"] = self.group.getInstanceOf("block")
	expecting = "::"
	result = str(b)
	# sys.stderr.write(result)
	self.assertEqual(result, expecting)


class TestTemplateAlias(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "page(name)::= \"name is <name>\"" + \
            "other::= page"+os.linesep
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
             stringtemplate.language.AngleBracketTemplateLexer.Lexer)

    def runTest(self):
        b = self.group.getInstanceOf("other")  # alias for page
	b["name"] = "Ter"
	expecting ="name is Ter"
	result = str(b)
	self.assertEqual(result, expecting)


class TestTemplateGetPropertyGetsAttribute(unittest.TestCase):

    # This test will cause infinite loop if missing attribute no
    # properly caught in getAttribute
    def setUp(self):
        self.templates = \
            "group test;"+os.linesep+ \
            "Cfile(funcs)::= <<"+os.linesep + \
            "#include \\<stdio.h>"+os.linesep+ \
            "<funcs:{public void <it.name>(<it.args>);}; separator=\"\\n\">"+os.linesep+ \
            "<funcs; separator=\"\\n\">"+os.linesep+ \
            ">>"+os.linesep + \
            "func(name,args,body)::= <<"+os.linesep+ \
            "public void <name>(<args>){<body>}"+os.linesep + \
            ">>"+os.linesep
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
             stringtemplate.language.AngleBracketTemplateLexer.Lexer)

    def runTest(self):
        b = self.group.getInstanceOf("Cfile")
        f1 = self.group.getInstanceOf("func")
        f2 = self.group.getInstanceOf("func")
	f1["name"] = "f"
	f1["args"] = ""
	f1["body"] = "i=1;"
	f2["name"] = "g"
	f2["args"] = "int arg"
	f2["body"] = "y=1;"
	b["funcs"] = f1
	b["funcs"] = f2
	expecting = "#include <stdio.h>" +os.linesep+ \
            "public void f();"+os.linesep+ \
            "public void g(int arg);" +os.linesep+ \
            "public void f(){i=1;}"+os.linesep+ \
            "public void g(int arg){y=1;}"
	self.assertEqual(str(b), expecting)


class Decl(object):

    def __init__(self, name, type_):
	self.name = name
	self.type = type_

    def getName(self):
        return self.name

    def getType(self):
        return self.type


class TestComplicatedIndirectTemplateApplication(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group Java;"+os.linesep + \
            ""+os.linesep + \
            "file(variables,methods)::= <<" + \
            "<variables:{<it.decl:(it.format)()>}; separator=\"\\n\">"+os.linesep + \
            "<methods>"+os.linesep + \
            ">>"+os.linesep+ \
            "intdecl()::= \"int <it.name> = 0;\""+os.linesep + \
            "intarray()::= \"int[] <it.name> = null;\""+os.linesep
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
             stringtemplate.language.AngleBracketTemplateLexer.Lexer)

    def runTest(self):
        f = self.group.getInstanceOf("file")
	f.setAttribute("variables.{decl,format}", Decl("i","int"), "intdecl")
	f.setAttribute("variables.{decl,format}", Decl("a","int-array"), "intarray")
	# sys.stderr.write("f='"+f+"'\n")
	expecting = "int i = 0;" +os.linesep+ \
            "int[] a = null;"+os.linesep
	self.assertEqual(str(f), expecting)


class TestIndirectTemplateApplication(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group dork;"+os.linesep + \
            ""+os.linesep + \
            "test(name)::= <<" + \
            "<(name)()>"+os.linesep + \
            ">>"+os.linesep+ \
            "first()::= \"the first\""+os.linesep + \
            "second()::= \"the second\""+os.linesep
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
             stringtemplate.language.AngleBracketTemplateLexer.Lexer)

    def runTest(self):
        f = self.group.getInstanceOf("test")
	f["name"] = "first"
	expecting = "the first"
	self.assertEqual(str(f), expecting)


class TestIndirectTemplateWithArgsApplication(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group dork;" + os.linesep + \
            "" + os.linesep + \
            "test(name) ::= <<" + \
            "<(name)(a=\"foo\")>" + os.linesep + \
            ">>" + os.linesep + \
            "first(a) ::= \"the first: <a>\"" + os.linesep + \
            "second(a) ::= \"the second <a>\"" + os.linesep

        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
             stringtemplate.language.AngleBracketTemplateLexer.Lexer)

    def runTest(self):
        f = self.group.getInstanceOf("test")
	f["name"] = "first"
        expecting = "the first: foo"
        self.assertEqual(str(f), expecting)


class TestNullIndirectTemplateApplication(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group dork;"+os.linesep + \
            ""+os.linesep + \
            "test(names)::= <<" + \
            "<names:(ind)()>"+os.linesep + \
            ">>"+os.linesep+ \
            "ind()::= \"[<it>]\""+os.linesep
	
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
             stringtemplate.language.AngleBracketTemplateLexer.Lexer)

    def runTest(self):
        f = self.group.getInstanceOf("test")
	f["names"] = "me"
	f["names"] = "you"
	expecting = ""
	self.assertEqual(str(f), expecting)


class TestNullIndirectTemplate(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group dork;"+os.linesep + \
            ""+os.linesep + \
            "test(name)::= <<" + \
            "<(name)()>"+os.linesep + \
            ">>"+os.linesep+ \
            "first()::= \"the first\""+os.linesep + \
            "second()::= \"the second\""+os.linesep
            
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
             stringtemplate.language.AngleBracketTemplateLexer.Lexer)

    def runTest(self):
        f = self.group.getInstanceOf("test")
	#f["name"] = "first"
	expecting = ""
	self.assertEqual(str(f), expecting)


class TestReflection(unittest.TestCase):

    def runTest(self):
        stringtemplate.StringTemplate.setLintMode(True)
        a = stringtemplate.StringTemplate("$attributes$")
	errors = ErrorBuffer()
	a.setErrorListener(errors)
	a["name"] = "Terence"
	a["name"] = "Tom"
        embedded = stringtemplate.StringTemplate("embedded")
	embedded["z"] = "hi"
	a["name"] = embedded
	a["notUsed"] = "foo"
	expecting = \
            "Template anonymous :"+os.linesep + \
            "    1. Attribute notUsed values :"+os.linesep + \
            "        1. str"+os.linesep+ \
            "    2. Attribute name values :"+os.linesep + \
            "        1. str"+os.linesep + \
            "        2. str"+os.linesep + \
            "        3. Template anonymous :"+os.linesep + \
            "            1. Attribute z values :"+os.linesep + \
            "                1. str"+os.linesep
	a.setPredefinedAttributes()
	results = str(a)
	# sys.stderr.write(results + '\n')
	stringtemplate.StringTemplate.setLintMode(False)
	self.assertEqual(results, expecting)


class R1:

    def getFirstName(self):
        return "Terence"

    def getLastName(self):
        return "Parr"

    def getSubTemplate(self):
        s = stringtemplate.StringTemplate("sub ST")
        s.setName("a template created in R1")
        s["fruit"] = "Apple"
        s["nestedAggr"] = R2()
        return s

    def getSubAggregate(self):
        return R2()


class R2:

    def getEmail(self):
        return "parrt"
    

class TestReflectionRecursive(unittest.TestCase):

    def runTest(self):
        stringtemplate.StringTemplate.setLintMode(True)
        a = stringtemplate.StringTemplate("$attributes$")
	errors = ErrorBuffer()
	a.setErrorListener(errors)
	a["name"] = "Terence"
	a["name"] = "Tom"
        b = stringtemplate.StringTemplate("embedded template text")
	b.setName("EmbeddedTemplate")
	b["x"] = 1
	b["y"] = "foo"
	a["name"] = b
	a["name"] = R1()
	a["duh"] = "foo"
	results = str(a)
	# sys.stderr.write(results + '\n')
	expecting = \
            "Template anonymous :"+os.linesep + \
            "    1. Attribute duh values :"+os.linesep + \
            "        1. str"+os.linesep + \
            "    2. Attribute name values :"+os.linesep + \
            "        1. str"+os.linesep + \
            "        2. str"+os.linesep + \
            "        3. Template EmbeddedTemplate :"+os.linesep + \
            "            1. Attribute y values :"+os.linesep + \
            "                1. str"+os.linesep + \
            "            2. Attribute x values :"+os.linesep + \
            "                1. int"+os.linesep + \
            "        1. Property firstName : str"+os.linesep + \
            "        2. Property lastName : str"+os.linesep + \
            "        3. Property subAggregate : R2"+os.linesep+ \
            "        4. Property subTemplate : StringTemplate"+os.linesep + \
            "            4. Template a template created in R1 :"+os.linesep + \
            "                1. Attribute fruit values :"+os.linesep + \
            "                    1. str"+os.linesep + \
            "                2. Attribute nestedAggr values :"+os.linesep + \
            "                    1. Property email : str"+os.linesep
        stringtemplate.StringTemplate.setLintMode(False)
	self.assertEqual(results, expecting)


class A(object):

    def getB(self):
        return None


class B(object):

    def getA(self):
        return None

    def getHarmless1(self):
        return ""


class C(object):

    def getSelf(self):
        return None

    def getHarmless2(self):
        return ""
    

class TestReflectionTypeLoop(unittest.TestCase):

    def runTest(self):
        stringtemplate.StringTemplate.setLintMode(True)
        a = stringtemplate.StringTemplate("$attributes$")
	errors = ErrorBuffer()
	a.setErrorListener(errors)
	a["name"] = "Terence"
	a["c"] = C()
	a["a"] = A()
	results = str(a)
	# sys.stderr.write(results + '\n')
	expecting = \
            "Template anonymous :"+os.linesep + \
            "    1. Attribute a values :"+os.linesep + \
            "        1. Property b : <any>"+os.linesep + \
            "    2. Attribute c values :"+os.linesep + \
            "        1. Property harmless2 : str"+os.linesep + \
            "        2. Property self : <any>"+os.linesep + \
            "    3. Attribute name values :"+os.linesep + \
            "        1. str"+os.linesep
        stringtemplate.StringTemplate.setLintMode(False)
	self.assertEqual(results, expecting)


class R3(object):

    def __init__(self):
        pass

    def getEmail(self):
        return "parrt"

    def getAMap(self):
        return { "v":33 }

    def getATuple(self):
        return ( 1, 2 )
    

class TestReflectionWithDict(unittest.TestCase):

    def runTest(self):
        stringtemplate.StringTemplate.setLintMode(True)
        a = stringtemplate.StringTemplate("$attributes$")
	errors = ErrorBuffer()
	a.setErrorListener(errors)
	map_ = { 1:2 }
	a["stuff"] = map_
	map_["prop1"] = "Terence"
	map_["prop2"] = R3()
	results = str(a)
	# sys.stderr.write(results + '\n')
	expecting = \
            "Template anonymous :"+os.linesep + \
            "    1. Attribute stuff values :"+os.linesep + \
            "        1. Key 1 : int"+os.linesep + \
            "        2. Key prop1 : str"+os.linesep + \
            "        3. Key prop2 : R3"+os.linesep + \
            "            1. Property aMap : dict"+os.linesep + \
            "                1. Key v : int"+os.linesep + \
            "            2. Property aTuple : tuple"+os.linesep + \
            "            3. Property email : str"+os.linesep
        stringtemplate.StringTemplate.setLintMode(False)
	self.assertEqual(results, expecting)


class TestDictPropertyFetch(unittest.TestCase):

    def runTest(self):
        a = stringtemplate.StringTemplate("$stuff.prop$")
	map_ = {}
	a["stuff"] = map_
	map_["prop"] = "Terence"
	results = str(a)
	# sys.stderr.write(results + '\n')
	expecting = "Terence"
	self.assertEqual(results, expecting)


class TestHashMapPropertyFetchEmbeddedStringTemplate(unittest.TestCase):

    def runTest(self):
        a = stringtemplate.StringTemplate("$stuff.prop$")
        map_ = {}
        a["stuff"] = map_
        a["title"] = "ST rocks"
        map_["prop"] = stringtemplate.StringTemplate("embedded refers to $title$")
        results = str(a)
        # sys.stderr.write(results + '\n')
        expecting = "embedded refers to ST rocks"
        self.assertEqual(results, expecting)

class TestEmbeddedComments(unittest.TestCase):

    def runTest(self):
        st = stringtemplate.StringTemplate(
            "Foo $! ignore !$bar" +os.linesep)
	expecting = "Foo bar"+os.linesep
	result = str(st)
	self.assertEqual(result, expecting)

	st = stringtemplate.StringTemplate(
            "Foo $! ignore" +os.linesep+ \
            " and a line break!$" +os.linesep+ \
            "bar" +os.linesep)
	expecting = "Foo "+os.linesep+"bar"+os.linesep
	result = str(st)
	self.assertEqual(result, expecting)

	st = stringtemplate.StringTemplate(
            "$! start of line $ and $! ick" +os.linesep+ \
            "!$boo"+os.linesep)
	expecting = "boo"+os.linesep
	result = str(st)
	self.assertEqual(result, expecting)

	st = stringtemplate.StringTemplate(
            "$! start of line !$" +os.linesep+ \
            "$! another to ignore !$" +os.linesep+ \
            "$! ick" +os.linesep+ \
            "!$boo"+os.linesep)
	expecting = "boo"+os.linesep
	result = str(st)
	self.assertEqual(result, expecting)

	st = stringtemplate.StringTemplate(
            "$! back !$$! to back !$" +os.linesep+ # can't detect; leaves \n
            "$! ick" +os.linesep+ \
            "!$boo"+os.linesep)
	expecting = os.linesep+"boo"+os.linesep
	result = str(st)
	self.assertEqual(result, expecting)


class TestEmbeddedCommentsAngleBracketed(unittest.TestCase):

    def runTest(self):
        st = stringtemplate.StringTemplate(
            "Foo <! ignore !>bar" +os.linesep,
             stringtemplate.language.AngleBracketTemplateLexer.Lexer)
	expecting = "Foo bar"+os.linesep
	result = str(st)
	self.assertEqual(result, expecting)

	st = stringtemplate.StringTemplate(
            "Foo <! ignore" +os.linesep+ \
            " and a line break!>" +os.linesep+ \
            "bar" +os.linesep,
             stringtemplate.language.AngleBracketTemplateLexer.Lexer)
	expecting = "Foo "+os.linesep+"bar"+os.linesep
	result = str(st)
	self.assertEqual(result, expecting)

	st = stringtemplate.StringTemplate(
            "<! start of line $ and <! ick" +os.linesep+ \
            "!>boo"+os.linesep,
             stringtemplate.language.AngleBracketTemplateLexer.Lexer)
	expecting = "boo"+os.linesep
	result = str(st)
	self.assertEqual(result, expecting)

	st = stringtemplate.StringTemplate(
            "<! start of line !>" + \
            "<! another to ignore !>" + \
            "<! ick" +os.linesep+ \
            "!>boo"+os.linesep,
             stringtemplate.language.AngleBracketTemplateLexer.Lexer)
	expecting = "boo"+os.linesep
	result = str(st)
	# sys.stderr.write(result)
	self.assertEqual(result, expecting)

	st = stringtemplate.StringTemplate(
            "<! back !><! to back !>" +os.linesep+ # can't detect; leaves \n
            "<! ick" +os.linesep+ \
            "!>boo"+os.linesep,
             stringtemplate.language.AngleBracketTemplateLexer.Lexer)
	expecting = os.linesep+"boo"+os.linesep
	result = str(st)
	self.assertEqual(result, expecting)


class TestCharLiterals(unittest.TestCase):

    def runTest(self):
        st = stringtemplate.StringTemplate(
            "Foo <\\n><\\t> bar" +os.linesep,
             stringtemplate.language.AngleBracketTemplateLexer.Lexer)
	expecting = "Foo "+os.linesep+"\t bar"+os.linesep
	result = str(st)
	self.assertEqual(result, expecting)

	st = stringtemplate.StringTemplate(
            "Foo $\\n$$\\t$ bar" +os.linesep)
	expecting = "Foo "+os.linesep+"\t bar"+os.linesep
	result = str(st)
	self.assertEqual(result, expecting)

	st = stringtemplate.StringTemplate(
            "Foo$\\ $bar$\\n$")
	expecting = "Foo bar"+os.linesep
	result = str(st)
	self.assertEqual(result, expecting)


class TestEmptyIteratedValueGetsSeparator(unittest.TestCase):

    def setUp(self):
	self.group = stringtemplate.StringTemplateGroup("test")
	self.errors = ErrorBuffer()
	self.group.setErrorListener(self.errors)

    def runTest(self):
	t = stringtemplate.StringTemplate(self.group, \
	    "$names; separator=\",\"$")
	t["names"] = "Terence"
	t["names"] = ""
	t["names"] = ""
	t["names"] = "Tom"
	t["names"] = "Frank"
	t["names"] = ""
	# empty values get separator still
	expecting = "Terence,,,Tom,Frank,"
	result = str(t)
	self.assertEqual(result, expecting)


class TestEmptyIteratedConditionalValueGetsNoSeparator(unittest.TestCase):

    def setUp(self):
	self.group = stringtemplate.StringTemplateGroup("test")
	self.errors = ErrorBuffer()
	self.group.setErrorListener(self.errors)

    def runTest(self):
	t = stringtemplate.StringTemplate(self.group, \
	    "$users:{$if(it.ok)$$it.name$$endif$}; separator=\",\"$")
	t.setAttribute("users.{name,ok}", "Terence", True)
	t.setAttribute("users.{name,ok}", "Tom", False)
	t.setAttribute("users.{name,ok}", "Frank", True)
	t.setAttribute("users.{name,ok}", "Johnny", False)
	# empty conditional values get no separator
	expecting = "Terence,Frank,"
	# haven't solved the last empty value problem yet
	result = str(t)
	self.assertEqual(result, expecting);


class TestEmptyIteratedConditionalWithElseValueGetsSeparator(unittest.TestCase):

    def setUp(self):
	self.group = stringtemplate.StringTemplateGroup("test")
	self.errors = ErrorBuffer()
	self.group.setErrorListener(self.errors)

    def runTest(self):
	t = stringtemplate.StringTemplate(self.group, \
	    "$users:{$if(it.ok)$$it.name$$else$$endif$}; separator=\",\"$")
	t.setAttribute("users.{name,ok}", "Terence", True)
	t.setAttribute("users.{name,ok}", "Tom", False)
	t.setAttribute("users.{name,ok}", "Frank", True)
	t.setAttribute("users.{name,ok}", "Johnny", False)
	# empty conditional values get no separator
	expecting = "Terence,,Frank,"
	# haven't solved the last empty value problem yet
	result = str(t)
	self.assertEqual(result, expecting)


class TestWhiteSpaceAtEndOfTemplate(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("group", ".")
        pageST = group.getInstanceOf("page")
        listST = group.getInstanceOf("users.list")
	# users.list references row.st which has a single blank line at the end.
	# I.e., there are 2 \n in a row at the end
	# ST should eat all whitespace at end
	listST["users"] = Connector()
	listST["users"] = Connector2()
	pageST["title"] = "some title"
	pageST["body"] = listST
	expecting ="some title" +os.linesep+ \
            "Terence parrt@jguru.comTom tombu@jguru.com"
	result = str(pageST)
	# sys.stderr.write("'" + result + "'\n")
	self.assertEqual(result, expecting)

class Duh:

    def __init__(self):
	self.users = []


class TestSizeZeroButNonNullListGetsNoOutput(unittest.TestCase):

    def setUp(self):
	self.group = stringtemplate.StringTemplateGroup("test")
	self.errors = ErrorBuffer()
	self.group.setErrorListener(self.errors)

    def runTest(self):
	t = stringtemplate.StringTemplate(self.group, \
	    "$duh.users:{name: $it$}; separator=\", \"$")
	t["duh",] = Duh()
	expecting = ""
	result = str(t)
	self.assertEqual(result, expecting)


class TestSizeZeroOnLineByItselfGetsNoOutput(unittest.TestCase):

    def setUp(self):
	self.group = stringtemplate.StringTemplateGroup("test")
	self.errors = ErrorBuffer()
	self.group.setErrorListener(self.errors)

    def runTest(self):
	t = stringtemplate.StringTemplate(self.group, \
	    "begin\n" + \
	    "$name$\n" + \
	    "$users:{name: $it$}$\n" + \
	    "$users:{name: $it$}; separator=\", \"$\n" + \
	    "end\n" \
	    )
	expecting = "begin\nend\n"
	result = str(t)
	self.assertEqual(result, expecting)


class TestSizeZeroOnLineWithIndentGetsNoOutput(unittest.TestCase):

    def setUp(self):
	self.group = stringtemplate.StringTemplateGroup("test")
	self.errors = ErrorBuffer()
	self.group.setErrorListener(self.errors)

    def runTest(self):
	t = stringtemplate.StringTemplate(self.group, \
	    "begin\n" + \
	    "  $name$\n" + \
	    "	$users:{name: $it$}$\n" + \
	    "	$users:{name: $it$$\\n$}$\n" + \
	    "end\n" \
	    )
	expecting = "begin\nend\n"
	result = str(t)
	self.assertEqual(result, expecting)


class TestSimpleAutoIndent(unittest.TestCase):

    def runTest(self):
	a = stringtemplate.StringTemplate( \
	    "$title$: {\n" + \
	    "	$name; separator=\"\n\"$\n" + \
	    "}" \
	    )
	a["title"] = "foo"
	a["name"] = "Terence"
	a["name"] = "Frank"
	results = str(a)
	# sys.stderr.write("'" + results + "'\n")
	expecting = \
	    "foo: {\n" + \
	    "	Terence\n" + \
	    "	Frank\n" + \
	    "}"
	self.assertEqual(results, expecting)

class TestComputedPropertyName(unittest.TestCase):

    def setUp(self):
        self.group = stringtemplate.StringTemplateGroup("test")
	self.errors = ErrorBuffer()
        self.group.setErrorListener(self.errors)

    def runTest(self):
        t = stringtemplate.StringTemplate(self.group,
            "variable property $propName$=$v.(propName)$")
        t["v"] = Decl("i", "int")
        t["propName"] = "type"
        expecting = "variable property type=int"
        result = str(t)
        self.assertEqual(str(self.errors), "")
        self.assertEqual(result, expecting)


class R:

    pass

class TestMultipleAttributeRefThroughReflection(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("test")
        s = stringtemplate.StringTemplate(group,
            "$p:{blah$it.name$washere}$")
	t = R()
	t.name = 'nom'
	tt = R()
	tt.name = 'deplume'
	s['p'] = [t, tt]
	expecting = "blahnomwashereblahdeplumewashere"
	self.assertEqual(str(s), expecting)


if __name__ == '__main__':

    from optparse import OptionParser

    parser = OptionParser()
    parser.add_option("-g", "--gui",
    		      action="store_true", dest="guiRunner", default=False,
    		      help="use TkInter based GUI for test")

    (options, args) = parser.parse_args()

    suite = unittest.TestSuite()
    suite.addTest(TestGroupFileFormat())
    suite.addTest(TestTemplateParameterDecls())
    suite.addTest(TestTemplateRedef())
    suite.addTest(TestMissingInheritedAttribute())
    suite.addTest(TestFormalArgumentAssignment())
    suite.addTest(TestUndefinedArgumentAssignment())
    suite.addTest(TestFormalArgumentAssignmentInApply())
    suite.addTest(TestUndefinedArgumentAssignmentInApply())
    suite.addTest(TestUndefinedAttributeReference())
    suite.addTest(TestUndefinedDefaultAttributeReference())
    suite.addTest(TestAngleBracketsWithGroupFileAsString())
    suite.addTest(TestAngleBracketsWithGroupFile())
    suite.addTest(TestAngleBracketsNoGroup())
    suite.addTest(TestSimpleInheritance())
    suite.addTest(TestOverrideInheritance())
    suite.addTest(TestMultiLevelInheritance())
    suite.addTest(TestExprInParens())
    suite.addTest(TestMultipleAdditions())
    suite.addTest(TestCollectionAttributes())
    suite.addTest(TestParenthesizedExpression())
    suite.addTest(TestApplyTemplateNameExpression())
    suite.addTest(TestTemplateNameExpression())
    suite.addTest(TestMissingEndDelimiter())
    suite.addTest(TestSetButNotRefd())
    suite.addTest(TestNullTemplateApplication())
    suite.addTest(TestNullTemplateToMultiValuedApplication())
    suite.addTest(TestChangingAttrValueTemplateApplicationToVector())
    suite.addTest(TestChangingAttrValueRepeatedTemplateApplicationToVector())
    suite.addTest(TestAlternatingTemplateApplication())
    suite.addTest(TestExpressionAsRHSOfAssignment())
    suite.addTest(TestTemplateApplicationAsRHSOfAssignment())
    suite.addTest(TestParameterAndAttributeScoping())
    suite.addTest(TestComplicatedSeparatorExpr())
    suite.addTest(TestAttributeRefButtedUpAgainstEndifAndWhitespace())
    suite.addTest(TestStringCatenationOnSingleValuedAttribute())
    suite.addTest(TestApplyingTemplateFromDiskWithPrecompiledIF())
    suite.addTest(TestMultiValuedAttributeWithAnonymousTemplateUsingIndexVariableI())
    suite.addTest(TestDictAttributeWithAnonymousTemplateUsingKeyVariableIk())
    suite.addTest(TestFindTemplateInCurrentDir())
    suite.addTest(TestTiming())
    suite.addTest(TestApplyTemplateToSingleValuedAttribute())
    suite.addTest(TestStringLiteralAsAttribute())
    suite.addTest(TestApplyTemplateToSingleValuedAttributeWithDefaultAttribute())
    suite.addTest(TestApplyAnonymousTemplateToSingleValuedAttribute())
    suite.addTest(TestApplyAnonymousTemplateToMultiValuedAttribute())
    suite.addTest(TestApplyAnonymousTemplateToAggregateAttribute())
    suite.addTest(TestRepeatedApplicationOfTemplateToSingleValuedAttribute())
    suite.addTest(TestRepeatedApplicationOfTemplateToMultiValuedAttributeWithSeparator())
    suite.addTest(TestMultiValuedAttributeWithSeparator())
    suite.addTest(TestSingleValuedAttributes())
    suite.addTest(TestIFTemplate())
    suite.addTest(TestIFBoolean())
    suite.addTest(TestNestedIFTemplate())
    suite.addTest(TestObjectPropertyReference())
    suite.addTest(TestApplyRepeatedAnonymousTemplateWithForeignTemplateRefToMultiValuedAttribute())
    suite.addTest(TestRecursion())
    suite.addTest(TestNestedAnonymousTemplates())
    suite.addTest(TestAnonymousTemplateAccessToEnclosingAttributes())
    suite.addTest(TestNestedAnonymousTemplatesAgain())
    suite.addTest(TestEscapes())
    suite.addTest(TestEscapesOutsideExpressions())
    suite.addTest(TestElseClause())
    suite.addTest(TestNestedIF())
    suite.addTest(TestEmbeddedMultiLineIF())
    suite.addTest(TestSimpleIndentOfAttributeList())
    suite.addTest(TestIndentOfMultilineAttributes())
    suite.addTest(TestIndentOfMultipleBlankLines())
    suite.addTest(TestIndentBetweenLeftJustifiedLiterals())
    suite.addTest(TestNestedIndent())
    suite.addTest(TestApplyAnonymousTemplateToDictTupleAndList())
    suite.addTest(TestApplyAnonymousTemplateToDictTupleAndList2())
    suite.addTest(TestDumpDictTupleAndList())
    suite.addTest(TestApplyAnonymousTemplateToListAndDictProperty())
    suite.addTest(TestSuperTemplateRef())
    suite.addTest(TestApplySuperTemplateRef())
    suite.addTest(TestLazyEvalOfSuperInApplySuperTemplateRef())
    suite.addTest(TestTemplatePolymorphism())
    suite.addTest(TestListOfEmbeddedTemplateSeesEnclosingAttributes())
    suite.addTest(TestInheritArgumentFromRecursiveTemplateApplication())
    suite.addTest(TestDeliberateRecursiveTemplateApplication())
    suite.addTest(TestImmediateTemplateAsAttributeLoop())
    suite.addTest(TestTemplateAlias())
    suite.addTest(TestTemplateGetPropertyGetsAttribute())
    suite.addTest(TestComplicatedIndirectTemplateApplication())
    suite.addTest(TestIndirectTemplateApplication())
    suite.addTest(TestIndirectTemplateWithArgsApplication())
    suite.addTest(TestNullIndirectTemplateApplication())
    suite.addTest(TestNullIndirectTemplate())
    suite.addTest(TestReflection())
    suite.addTest(TestReflectionRecursive())
    suite.addTest(TestReflectionTypeLoop())
    suite.addTest(TestReflectionWithDict())
    suite.addTest(TestDictPropertyFetch())
    suite.addTest(TestHashMapPropertyFetchEmbeddedStringTemplate())
    suite.addTest(TestEmbeddedComments())
    suite.addTest(TestEmbeddedCommentsAngleBracketed())
    suite.addTest(TestCharLiterals())
    suite.addTest(TestEmptyIteratedValueGetsSeparator())
    suite.addTest(TestEmptyIteratedConditionalValueGetsNoSeparator())
    suite.addTest(TestEmptyIteratedConditionalWithElseValueGetsSeparator())
    suite.addTest(TestWhiteSpaceAtEndOfTemplate())
    suite.addTest(TestSizeZeroButNonNullListGetsNoOutput())
    suite.addTest(TestSizeZeroOnLineByItselfGetsNoOutput())
    suite.addTest(TestSizeZeroOnLineWithIndentGetsNoOutput())
    suite.addTest(TestSimpleAutoIndent())
    suite.addTest(TestComputedPropertyName())
    suite.addTest(TestMultipleAttributeRefThroughReflection())

    doGUI = options.guiRunner
    if doGUI:
	try:
            import unittestgui
	    tk = unittestgui.tk
	except:
            doGUI = False

    if doGUI:

        class STTkTestRunner(unittestgui.TkTestRunner):

	    def __init__(self, *args, **kwargs):
	    	self.root = tk.Tk()
		self.root.title("PyUnit")
		self.test = None
        	self.currentResult = None
        	self.running = 0
        	self.__rollbackImporter = None
	        super(STTkTestRunner, self).__init__(self.root, 'TestStringTemplate')

	    def runClicked(self):
        	"To be called in response to user choosing to run a test"
        	if self.running:
		    return
		if not self.test:
		    self.errorDialog("Test entry", "You must provide a TestCase or TestSuite to run")
        	    return
        	if self.__rollbackImporter:
        	    self.__rollbackImporter.rollbackImports()
        	self.__rollbackImporter = unittestgui.RollbackImporter()
        	self.currentResult = unittestgui.GUITestResult(self)
        	self.totalTests = self.test.countTestCases()
        	self.running = 1
        	self.notifyRunning()
        	self.test(self.currentResult)
        	self.running = 0
        	self.notifyStopped()

            def run(self, test):
	        self.test = test
		self.root.protocol('WM_DELETE_WINDOW', self.root.quit)
		self.root.mainloop()

	runner = STTkTestRunner(suite)
	result = runner.run(suite)

    else: # doGUI = False

        runner = unittest.TextTestRunner(sys.stdout)
	result = runner.run(suite)
	sys.exit(not result.wasSuccessful())



#if __name__ == '__main__':
#    unittest.main()
