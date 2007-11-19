#/usr/bin/env python
# -*- coding: utf-8 -*-

import sys
import os
import traceback
import unittest
from StringIO import StringIO
from datetime import date
import tempfile

import stringtemplate


class BrokenTest(unittest.TestCase.failureException):
    def __str__(self):
        name = self.args[0]
        return '%s: %s: works now' % (
            (self.__class__.__name__, name))


def broken(test_method):
    def wrapper(*args, **kwargs):
        try:
            test_method(*args, **kwargs)
        except Exception:
            pass
        else:
            raise BrokenTest(test_method.__name__)
    wrapper.__doc__ = test_method.__doc__
    wrapper.__name__ = 'XXX_' + test_method.__name__
    return wrapper


def writeFile(dir, fileName, content):
    fp = open(os.path.join(dir, fileName), 'w')
    fp.write(content)
    fp.close()


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


class TestInterfaces(unittest.TestCase):
    def testInterfaceFileFormat(self):
        groupI = (
            "interface test;" +os.linesep+
            "t();" +os.linesep+
            "bold(item);"+os.linesep+
            "optional duh(a,b,c);"+os.linesep
            )
        I = stringtemplate.StringTemplateGroupInterface(StringIO(groupI))

        expecting = (
            "interface test;\n" +
            "bold(item);\n" +
            "optional duh(a, b, c);\n" +
            "t();\n"
            )
        self.assertEqual(str(I), expecting)


    def testNoGroupLoader(self):
        # this also tests the group loader
        errors = ErrorBuffer()
        tmpdir = tempfile.gettempdir()

        # need to reset groupLoader, because it is a class attribute
        stringtemplate.StringTemplateGroup.groupLoader = None
        
        templates = (
            "group testG implements blort;" +os.linesep+
            "t() ::= <<foo>>" +os.linesep+
            "bold(item) ::= <<foo>>"+os.linesep+
            "duh(a,b,c) ::= <<foo>>"+os.linesep
            )
        writeFile(tmpdir, "testG.stg", templates)

        group = stringtemplate.StringTemplateGroup(
            open(tmpdir+"/testG.stg"),
            errors
            )

        expecting = "no group loader registered\n"
        self.assertEqual(str(errors), expecting)


    def testCannotFindInterfaceFile(self):
        # this also tests the group loader
        errors = ErrorBuffer()
        tmpdir = tempfile.gettempdir()
        stringtemplate.StringTemplateGroup.registerGroupLoader(
            stringtemplate.PathGroupLoader(tmpdir,errors)
            )

        templates = (
            "group testG implements blort;" +os.linesep+
            "t() ::= <<foo>>" +os.linesep+
            "bold(item) ::= <<foo>>"+os.linesep+
            "duh(a,b,c) ::= <<foo>>"+os.linesep
            )
        writeFile(tmpdir, "testG.stg", templates)

        group = stringtemplate.StringTemplateGroup(
            open(tmpdir+"/testG.stg"), errors
            )

        expecting = "no such interface file blort.sti\n"
        self.assertEqual(str(errors), expecting)


    def testGroupSatisfiesSingleInterface(self):
        # this also tests the group loader
        errors = ErrorBuffer()
        tmpdir = tempfile.gettempdir()
        stringtemplate.StringTemplateGroup.registerGroupLoader(
            stringtemplate.PathGroupLoader(tmpdir, errors)
            )

        groupI = (
            "interface testI;" +os.linesep+
            "t();" +os.linesep+
            "bold(item);"+os.linesep+
            "optional duh(a,b,c);"+os.linesep
            )
        writeFile(tmpdir, "testI.sti", groupI)

        templates = (
            "group testG implements testI;" +os.linesep+
            "t() ::= <<foo>>" +os.linesep+
            "bold(item) ::= <<foo>>"+os.linesep+
            "duh(a,b,c) ::= <<foo>>"+os.linesep
            )

        writeFile(tmpdir, "testG.stg", templates)

        group = stringtemplate.StringTemplateGroup(
            open(tmpdir+"/testG.stg"), errors
            )

        expecting = "" # should be no errors
        self.assertEqual(str(errors), expecting)


    def testMissingInterfaceTemplate(self):
        # this also tests the group loader
        errors = ErrorBuffer()
        tmpdir = tempfile.gettempdir()
        stringtemplate.StringTemplateGroup.registerGroupLoader(
            stringtemplate.PathGroupLoader(tmpdir,errors)
            )
        
        groupI = (
            "interface testI;" +os.linesep+
            "t();" +os.linesep+
            "bold(item);"+os.linesep+
            "optional duh(a,b,c);"+os.linesep
            )
        writeFile(tmpdir, "testI.sti", groupI)

        templates = (
            "group testG implements testI;" +os.linesep+
            "t() ::= <<foo>>" +os.linesep+
            "duh(a,b,c) ::= <<foo>>"+os.linesep
            )
        writeFile(tmpdir, "testG.stg", templates)

        group = stringtemplate.StringTemplateGroup(
            open(tmpdir+"/testG.stg"), errors
            )

        expecting = "group testG does not satisfy interface testI: missing templates ['bold']\n"
        self.assertEqual(str(errors), expecting)


    def testMissingOptionalInterfaceTemplate(self):
        # this also tests the group loader
        errors = ErrorBuffer()
        tmpdir = tempfile.gettempdir()
        stringtemplate.StringTemplateGroup.registerGroupLoader(
            stringtemplate.PathGroupLoader(tmpdir,errors)
            )
        
        groupI = (
            "interface testI;" +os.linesep+
            "t();" +os.linesep+
            "bold(item);"+os.linesep+
            "optional duh(a,b,c);"+os.linesep
            )
        writeFile(tmpdir, "testI.sti", groupI)

        templates = (
            "group testG implements testI;" +os.linesep+
            "t() ::= <<foo>>" +os.linesep+
            "bold(item) ::= <<foo>>"
            )
        writeFile(tmpdir, "testG.stg", templates)

        group = stringtemplate.StringTemplateGroup(
            open(tmpdir+"/testG.stg"), errors
            )

        expecting = "" # should be NO errors
        self.assertEqual(str(errors), expecting)


    def testMismatchedInterfaceTemplate(self):
        # this also tests the group loader
        errors = ErrorBuffer()
        tmpdir = tempfile.gettempdir()
        stringtemplate.StringTemplateGroup.registerGroupLoader(
            stringtemplate.PathGroupLoader(tmpdir,errors)
            )
        
        groupI = (
            "interface testI;" +os.linesep+
            "t();" +os.linesep+
            "bold(item);"+os.linesep+
            "optional duh(a,b,c);"+os.linesep
            )
        writeFile(tmpdir, "testI.sti", groupI)

        templates = (
            "group testG implements testI;" +os.linesep+
            "t() ::= <<foo>>" +os.linesep+
            "bold(item) ::= <<foo>>"+os.linesep+
            "duh(a,c) ::= <<foo>>"+os.linesep
            )
        writeFile(tmpdir, "testG.stg", templates)

        group = stringtemplate.StringTemplateGroup(
            open(tmpdir+"/testG.stg"), errors
            )

        expecting = "group testG does not satisfy interface testI: mismatched arguments on these templates ['optional duh(a, b, c)']\n"
        self.assertEqual(str(errors), expecting)

    
class TestGroupLoader(unittest.TestCase):
    def testMultiDirGroupLoading(self):
        # this also tests the group loader
        errors = ErrorBuffer()
        tmpdir = tempfile.gettempdir()
        if not os.path.isdir(os.path.join(tmpdir, 'sub')):
            os.mkdir(os.path.join(tmpdir, 'sub'))

        stringtemplate.StringTemplateGroup.registerGroupLoader(
            stringtemplate.PathGroupLoader(tmpdir+":"+tmpdir+"/sub",errors)
            )

        templates = (
            "group testG2;" +os.linesep+
            "t() ::= <<foo>>" +os.linesep+
            "bold(item) ::= <<foo>>"+os.linesep+
            "duh(a,b,c) ::= <<foo>>"+os.linesep
            )
        writeFile(tmpdir+"/sub", "testG2.stg", templates)

        group = stringtemplate.StringTemplateGroup.loadGroup("testG2")
        expecting = (
            "group testG2;\n" +
            "bold(item) ::= <<foo>>\n" +
            "duh(a,b,c) ::= <<foo>>\n" +
            "t() ::= <<foo>>\n"
            )
        self.assertEqual(str(group), expecting)


    #FIXME: which lexer should be used when using super group?
    @broken
    def testGroupExtendsSuperGroup(self):
        # this also tests the group loader
        errors = ErrorBuffer()
        tmpdir = tempfile.gettempdir()
        stringtemplate.StringTemplateGroup.registerGroupLoader(
            stringtemplate.PathGroupLoader(tmpdir,errors)
            )
        superGroup = (
            "group superG;" +os.linesep+
            "bold(item) ::= <<*$item$*>>\n"+os.linesep
            )
        writeFile(tmpdir, "superG.stg", superGroup)

        templates = (
            "group testG : superG;" +os.linesep+
            "main(x) ::= <<$bold(x)$>>"+os.linesep
            )
        writeFile(tmpdir, "testG.stg", templates)

        group = stringtemplate.StringTemplateGroup(
            open(tmpdir+"/testG.stg"),
            stringtemplate.language.DefaultTemplateLexer.Lexer,
            errors
            )
        st = group.getInstanceOf("main")
        st.setAttribute("x", "foo")

        expecting = "*foo*"
        self.assertEqual(str(st), expecting)


class TestGroupFileFormat(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "t() ::= \"literal template\"" +os.linesep+ \
            "bold(item) ::= \"<b>$item$</b>\""+os.linesep+ \
            "duh() ::= <<"+os.linesep+"xx"+os.linesep+">>"+os.linesep
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer
            )

    def testGroupFileFormat(self):
        expecting = (
            "group test;" + os.linesep +
            "bold(item) ::= <<<b>$item$</b>>>" + os.linesep +
            "duh() ::= <<xx>>" + os.linesep +
            "t() ::= <<literal template>>" + os.linesep
            )
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

class TestEscapedTemplateDelimiters(unittest.TestCase):

    def runTest(self):
        templates = (
            "group test;" +os.linesep+
            "t() ::= <<$\"literal\":{a|$a$\\}}$ template\n>>" +os.linesep+
            "bold(item) ::= <<<b>$item$</b\\>>>"+os.linesep+
            "duh() ::= <<"+os.linesep+"xx"+os.linesep+">>"+os.linesep
            )
        
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer
            )
	
        expecting = (
            "group test;" +os.linesep+
            "bold(item) ::= <<<b>$item$</b>>>" +os.linesep+
            "duh() ::= <<xx>>" +os.linesep+
            "t() ::= <<$\"literal\":{a|$a$\\}}$ template>>"+os.linesep
            )
        self.assertEqual(str(group), expecting)
	
        b = group.getInstanceOf("bold")
        b.setAttribute("item", "dork")
        expecting = "<b>dork</b>"
        self.assertEqual(str(b), expecting)
	
        a = group.getInstanceOf("t")
        expecting = "literal} template"
        self.assertEqual(str(a), expecting)


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
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer
            )

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
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer
            )

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
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer
            )

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
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer
            )
        
    def runTest(self):
        t = self.group.getInstanceOf("page")
        t["x"] = "Times"
	error = None
	try:
	    str(t)
	except KeyError, e:
	    error = str(e)
        expecting = "'template body has no such attribute: font in template context [page <invoke body arg context>]'";
        self.assertEqual(error, expecting);


class TestFormalArgumentAssignmentInApply(unittest.TestCase):

    def setUp(self):
        self.templates = \
        "group test;" +os.linesep+ \
        "page(name)::= <<$name:bold(font=\"Times\")$>>"+os.linesep + \
        "bold(font)::= \"<font face=$font$><b>$it$</b></font>\"" +os.linesep
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer
            )

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
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer
            )

    def runTest(self):
        t = self.group.getInstanceOf("page")
        t["x"] = "Times"
        t["name"] = "Ter"
	error = None
	try:
	    str(t)
	except KeyError, e:
	    error = str(e)
        expecting = "'template bold has no such attribute: font in template context [page <invoke bold arg context>]'";
        self.assertEqual(error, expecting);


class TestUndefinedAttributeReference(unittest.TestCase):

    def setUp(self):
        self.templates = \
            "group test;" +os.linesep+ \
            "page()::= <<$bold()$>>"+os.linesep + \
            "bold()::= \"$name$\"" +os.linesep
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer
            )

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
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer
            )

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
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates))

    def runTest(self):
        t = self.group.getInstanceOf("a")
        t["s"] = "Test"
        expecting = "case 1: Test break;"
        self.assertEqual(str(t), expecting)


class TestAngleBracketsWithGroupFile(unittest.TestCase):

    def setUp(self):
        self.filename = "test.stg";
        # mainly testing to ensure we don't get parse errors of above
        self.group = stringtemplate.StringTemplateGroup(
            file("test.stg")
            )

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


class TestRegions(unittest.TestCase):
    def testRegionRef(self):
        templates = (
            "group test;" +os.linesep+
            "a() ::= \"X$@r()$Y\"" +os.linesep
            )
        
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer
            )
        st = group.getInstanceOf("a")
        result = str(st)
        expecting = "XY"
        self.assertEqual(result, expecting)


    def testEmbeddedRegionRef(self):
        templates = (
            "group test;" +os.linesep+
            "a() ::= \"X$@r$blort$@end$Y\"" +os.linesep
            )
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer
            )
        st = group.getInstanceOf("a")
        result = str(st)
        expecting = "XblortY"
        self.assertEqual(result, expecting)


    def testRegionRefAngleBrackets(self):
        templates = (
            "group test;" +os.linesep+
            "a() ::= \"X<@r()>Y\"" +os.linesep
            )
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates)
            )
        st = group.getInstanceOf("a")
        result = str(st)
        expecting = "XY"
        self.assertEqual(result, expecting)


    def testEmbeddedRegionRefAngleBrackets(self):
        templates = (
            "group test;" +os.linesep+
            "a() ::= \"X<@r>blort<@end>Y\"" +os.linesep
            )
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates)
            )
        st = group.getInstanceOf("a")
        result = str(st)
        expecting = "XblortY"
        self.assertEqual(result, expecting)


    def testEmbeddedRegionRefWithNewlinesAngleBrackets(self):
        templates = (
            "group test;" +os.linesep+
            "a() ::= \"X<@r>" +os.linesep+
            "blort" +os.linesep+
            "<@end>" +os.linesep+
            "Y\"" +os.linesep
            )
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates)
            )
        st = group.getInstanceOf("a")
        result = str(st)
        expecting = "XblortY"
        self.assertEqual(result, expecting)


    def testRegionRefWithDefAngleBrackets(self):
        templates = (
            "group test;" +os.linesep+
            "a() ::= \"X<@r()>Y\"" + os.linesep +
            "@a.r() ::= \"foo\"" +os.linesep
            )
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates)
            )
        st = group.getInstanceOf("a")
        result = str(st)
        expecting = "XfooY"
        self.assertEqual(result, expecting)


    def testRegionRefWithDefInConditional(self):
        templates = (
            "group test;" + os.linesep +
            "a(v) ::= \"X<if(v)>A<@r()>B<endif>Y\""  + os.linesep +
            "@a.r() ::= \"foo\"" + os.linesep
            )
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates)
            )
        st = group.getInstanceOf("a")
        st.setAttribute("v", "true")
        result = str(st)
        expecting = "XAfooBY"
        self.assertEqual(result, expecting)


    def testRegionRefWithImplicitDefInConditional(self):
        templates = (
            "group test;" + os.linesep +
            "a(v) ::= \"X<if(v)>A<@r>yo<@end>B<endif>Y\"" + os.linesep +
            "@a.r() ::= \"foo\"" + os.linesep
            )
        
        errors = ErrorBuffer()
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates),
            errors
            )
        st = group.getInstanceOf("a")
        st.setAttribute("v", "true")
        result = str(st)
        expecting = "XAyoBY"
        self.assertEqual(result, expecting)

        err_result = str(errors)
        err_expecting = "group test line 3: redefinition of template region: @a.r\n"
        self.assertEqual(err_result, err_expecting)


    def testRegionOverride(self):
        templates1 = (
            "group super;" +os.linesep+
            "a() ::= \"X<@r()>Y\"" +
            "@a.r() ::= \"foo\"" +os.linesep
            )
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates1) 
            )

        templates2 = (
            "group sub;" +os.linesep+
            "@a.r() ::= \"foo\"" +os.linesep
            )
        subGroup = stringtemplate.StringTemplateGroup(
            StringIO(templates2),
            stringtemplate.language.AngleBracketTemplateLexer.Lexer,
            None,
            group
            )

        st = subGroup.getInstanceOf("a")
        result = str(st)
        expecting = "XfooY"
        self.assertEqual(result, expecting)


    def testRegionOverrideRefSuperRegion(self):
        templates1 = (
            "group super;" +os.linesep+
            "a() ::= \"X<@r()>Y\"" +
            "@a.r() ::= \"foo\"" +os.linesep
            )
            
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates1)
            )

        templates2 = (
            "group sub;" +os.linesep+
            "@a.r() ::= \"A<@super.r()>B\"" +os.linesep
            )
        subGroup = stringtemplate.StringTemplateGroup(
            StringIO(templates2),
            stringtemplate.language.AngleBracketTemplateLexer.Lexer,
            None,
            group
            )

        st = subGroup.getInstanceOf("a")
        result = str(st)
        expecting = "XAfooBY"
        self.assertEqual(result, expecting)


    def testRegionOverrideRefSuperRegion3Levels(self):
        # Bug: This was causing infinite recursion:
        # getInstanceOf(super::a)
        # getInstanceOf(sub::a)
        # getInstanceOf(subsub::a)
        # getInstanceOf(subsub::region__a__r)
        # getInstanceOf(subsub::super.region__a__r)
        # getInstanceOf(subsub::super.region__a__r)
        # getInstanceOf(subsub::super.region__a__r)
        # ...
        # Somehow, the ref to super in subsub is not moving up the chain
        # to the @super.r(); oh, i introduced a bug when i put setGroup
        # into STG.getInstanceOf()!

        templates1 = (
            "group super;" +os.linesep+
            "a() ::= \"X<@r()>Y\"" +
            "@a.r() ::= \"foo\"" +os.linesep
            )
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates1)
            )

        templates2 = (
            "group sub;" +os.linesep+
            "@a.r() ::= \"<@super.r()>2\"" +os.linesep
            )
        subGroup = stringtemplate.StringTemplateGroup(
            StringIO(templates2),
            stringtemplate.language.AngleBracketTemplateLexer.Lexer,
            None,
            group
            )

        templates3 = (
            "group subsub;" +os.linesep+
            "@a.r() ::= \"<@super.r()>3\"" +os.linesep
            )
        subSubGroup = stringtemplate.StringTemplateGroup(
            StringIO(templates3),
            stringtemplate.language.AngleBracketTemplateLexer.Lexer,
            None,
            subGroup
            )

        st = subSubGroup.getInstanceOf("a")
        result = str(st)
        expecting = "Xfoo23Y"
        self.assertEqual(result, expecting);


    def testRegionOverrideRefSuperImplicitRegion(self):
        templates1 = (
            "group super;" +os.linesep+
            "a() ::= \"X<@r>foo<@end>Y\""+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates1)
            )
        
        templates2 = (
            "group sub;" +os.linesep+
            "@a.r() ::= \"A<@super.r()>\"" +os.linesep
            )
        subGroup = stringtemplate.StringTemplateGroup(
            StringIO(templates2),
            stringtemplate.language.AngleBracketTemplateLexer.Lexer,
            None,
            group
            )
        st = subGroup.getInstanceOf("a")
        result = str(st)
        expecting = "XAfooY"
        self.assertEqual(result, expecting)
        

    def testEmbeddedRegionRedefError(self):
        # cannot define an embedded template within group
        templates = (
            "group test;" +os.linesep+
            "a() ::= \"X<@r>dork<@end>Y\"" +
            "@a.r() ::= \"foo\"" +os.linesep
            )
        errors = ErrorBuffer()
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates),
            errors
            )
        st = group.getInstanceOf("a")
        str(st)
        result = str(errors)
        expecting = "group test line 2: redefinition of template region: @a.r\n"
        self.assertEqual(result, expecting)


    def testImplicitRegionRedefError(self):
        # cannot define an implicitly-defined template more than once
        templates = (
            "group test;" +os.linesep+
            "a() ::= \"X<@r()>Y\"" +os.linesep+
            "@a.r() ::= \"foo\"" +os.linesep+
            "@a.r() ::= \"bar\"" +os.linesep
            )
        errors = ErrorBuffer()
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates),
            errors
            )
        st = group.getInstanceOf("a")
        str(st)
        result = str(errors)
        expecting = "group test line 4: redefinition of template region: @a.r\n"
        self.assertEqual(result, expecting)


    def testImplicitOverriddenRegionRedefError(self):
        templates1 = (
            "group super;" +os.linesep+
            "a() ::= \"X<@r()>Y\"" +
            "@a.r() ::= \"foo\"" +os.linesep
            )
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates1),
            )

        templates2 = (
            "group sub;" +os.linesep+
            "@a.r() ::= \"foo\"" +os.linesep+
            "@a.r() ::= \"bar\"" +os.linesep
            )
        errors = ErrorBuffer()
        subGroup = stringtemplate.StringTemplateGroup(
            StringIO(templates2),
            stringtemplate.language.AngleBracketTemplateLexer.Lexer,
            errors,
            group
            )

        st = subGroup.getInstanceOf("a")
        result = str(errors)
        expecting = "group sub line 3: redefinition of template region: @a.r\n"
        self.assertEqual(result, expecting)


    def testUnknownRegionDefError(self):
        # cannot define an implicitly-defined template more than once
        templates = (
            "group test;" +os.linesep+
            "a() ::= \"X<@r()>Y\"" +os.linesep+
            "@a.q() ::= \"foo\"" +os.linesep
            )
        errors = ErrorBuffer()
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates),
            errors
            )
        st = group.getInstanceOf("a")
        str(st)
        result = str(errors)
        expecting = "group test line 3: template a has no region called q\n"
        self.assertEqual(result, expecting)


    def testSuperRegionRefError(self):
        templates1 = (
            "group super;" +os.linesep+
            "a() ::= \"X<@r()>Y\"" +
            "@a.r() ::= \"foo\"" +os.linesep
            )
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates1),
            )

        templates2 = (
            "group sub;" +os.linesep+
            "@a.r() ::= \"A<@super.q()>B\"" +os.linesep
            )
        errors = ErrorBuffer()
        subGroup = stringtemplate.StringTemplateGroup(
            StringIO(templates2),
            stringtemplate.language.AngleBracketTemplateLexer.Lexer,
            errors,
            group
            )

        st = subGroup.getInstanceOf("a")
        result = str(errors)
        expecting = "template a has no region called q\n"
        self.assertEqual(result, expecting)


    def testMissingEndRegionError(self):
        # cannot define an implicitly-defined template more than once
        templates = (
            "group test;" +os.linesep+
            "a() ::= \"X$@r$foo\"" +os.linesep
            )
        errors = ErrorBuffer()
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer,
            errors,
            None
            )
        st = group.getInstanceOf("a")
        str(st)
        result = str(errors)
        expecting = "missing region r $@end$ tag\n"
        self.assertEqual(result, expecting)


    def testMissingEndRegionErrorAngleBrackets(self):
        # cannot define an implicitly-defined template more than once
        templates = (
            "group test;" +os.linesep+
            "a() ::= \"X<@r>foo\"" +os.linesep
            )
        errors = ErrorBuffer()
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates),
            errors
            )
        st = group.getInstanceOf("a")
        str(st)
        result = str(errors)
        expecting = "missing region r <@end> tag\n"
        self.assertEqual(result, expecting)


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


class TestComplicatedInheritance(unittest.TestCase):

    def runTest(self):
        # in super: decls invokes labels
        # in sub:   overridden decls which calls super.decls
        #           overridden labels
        # Bug: didn't see the overridden labels.  In other words,
        # the overridden decls called super which called labels, but
        # didn't get the subgroup overridden labels--it calls the
        # one in the superclass.  Ouput was "DL" not "DSL"; didn't
        # invoke sub's labels().
        basetemplates = (
            "group base;" +os.linesep+
            "decls() ::= \"D<labels()>\""+os.linesep+
            "labels() ::= \"L\"" +os.linesep
            )
        base = stringtemplate.StringTemplateGroup(
            StringIO(basetemplates),
            )
        subtemplates = (
            "group sub;" +os.linesep+
            "decls() ::= \"<super.decls()>\""+os.linesep+
            "labels() ::= \"SL\"" +os.linesep
            )
        sub = stringtemplate.StringTemplateGroup(
            StringIO(subtemplates),
            )
        sub.setSuperGroup(base)
        st = sub.getInstanceOf("decls")
        expecting = "DSL"
        result = str(st)
        self.assertEqual(result, expecting)


class Test3LevelSuperRef(unittest.TestCase):

    def runTest(self):
        templates1 = (
            "group super;" +os.linesep+
            "r() ::= \"foo\"" +os.linesep
            )
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates1)
            )

        templates2 = (
            "group sub;" +os.linesep+
            "r() ::= \"<super.r()>2\"" +os.linesep
            )
        subGroup = stringtemplate.StringTemplateGroup(
            StringIO(templates2),
            stringtemplate.language.AngleBracketTemplateLexer.Lexer,
            None,
            group
            )

        templates3 = (
            "group subsub;" +os.linesep+
            "r() ::= \"<super.r()>3\"" +os.linesep
            )
        subSubGroup = stringtemplate.StringTemplateGroup(
            StringIO(templates3),
            stringtemplate.language.AngleBracketTemplateLexer.Lexer,
            None,
            subGroup
            )
        st = subSubGroup.getInstanceOf("r")
        result = str(st)
        expecting = "foo23";
        self.assertEqual(result, expecting)


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
        t = stringtemplate.StringTemplate(group, "$(f+l):bold()$")
        t["f"] = "Joe"
        t["l"] = "Schmoe"
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


class TestApplyTemplateNameTemplateEval(unittest.TestCase):
    def runTest(self):
        group = stringtemplate.StringTemplateGroup("test")
        foobar = group.defineTemplate("foobar", "foo$it$bar")
        a = group.defineTemplate("a", "$it$bar")
        t = stringtemplate.StringTemplate(group, "$data:(\"foo\":a())()$")
        t.setAttribute("data", "Ter")
        t.setAttribute("data", "Tom")
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
	group = stringtemplate.StringTemplateGroup("test")
	errors = ErrorBuffer()
	group.setErrorListener(errors)
	t = stringtemplate.StringTemplate(group, "stuff $a then more junk etc...")
	expectingError="problem parsing template 'anonymous' : line 1:31: expecting '$', found '<EOF>'\n"
	# sys.stderr.write("error: '"+errors+"'\n")
	# sys.stderr.write("expecting: '"+expectingError+"'\n")
	self.assertEqual(str(errors), expectingError)


class TestSetButNotRefd(unittest.TestCase):

    def runTest(self):
	stringtemplate.StringTemplate.setLintMode(True)
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
	expecting = "Can't load template bold.st; context is [anonymous]"
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
	expecting = "Can't load template bold.st; context is [anonymous]"
	error = ""
	try:
	    result = str(t)
	except ValueError, ve:
	    error = str(ve)
	self.assertEqual(error, expecting)


class TestChangingAttrValueTemplateApplicationToVector(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("test")
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


class TestStringCatenationOnSingleValuedAttributeViaTemplateLiteral(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("test")
        bold = group.defineTemplate("bold", "<b>$it$</b>")
        b = stringtemplate.StringTemplate(group, "$bold(it={$name$ Parr})$")
	b["name"] = "Terence"
	expecting = "<b>Terence Parr</b>"
	self.assertEqual(str(b), expecting)


class TestStringCatenationOpOnArg(unittest.TestCase):
    def runTest(self):
        group = stringtemplate.StringTemplateGroup("test")
        bold = group.defineTemplate("bold", "<b>$it$</b>")
        b = stringtemplate.StringTemplate(group, "$bold(it=name+\" Parr\")$")
        b.setAttribute("name", "Terence")
        expecting = "<b>Terence Parr</b>"
        self.assertEqual(str(b), expecting)


class TestStringCatenationOpOnArgWithEqualsInString(unittest.TestCase):
    def runTest(self):
        group = stringtemplate.StringTemplateGroup("test")
        bold = group.defineTemplate("bold", "<b>$it$</b>")
        b = stringtemplate.StringTemplate(group, "$bold(it=name+\" Parr=\")$")
        b.setAttribute("name", "Terence")
        expecting = "<b>Terence Parr=</b>";
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
        st = stringtemplate.StringTemplate("$items:{$it.lastName$, $it.firstName$\n}$")
        # also testing wacky space in aggregate spec
	st.setAttribute("items.{ firstName ,lastName}", "Ter", "Parr")
	st.setAttribute("items.{firstName, lastName }", "Tom", "Burns")
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


class TestIf(unittest.TestCase):
    def testIFTemplate(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".",
	    stringtemplate.language.AngleBracketTemplateLexer.Lexer)
        t = stringtemplate.StringTemplate(group, \
                               "SELECT <column> FROM PERSON "+ \
                               "<if(cond)>WHERE ID=<id><endif>;")
	t["column"] = "name"
	t["cond"] = "True"
	t["id"] = "231"
	self.assertEqual(str(t), "SELECT name FROM PERSON WHERE ID=231;")


    def testIFCondWithParensTemplate(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer)
        t = stringtemplate.StringTemplate(group, \
            "<if(map.(type))><type> "+ \
            "<prop>=<map.(type)>;<endif>")
        map_ = {}
        map_["int"] = "0"
        t["map"] = map_
        t["prop"] = "x"
        t["type"] = "int"
        self.assertEqual(str(t), "int x=0;")


    def testIFCondWithParensDollarDelimsTemplate(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        t = stringtemplate.StringTemplate(group, \
            "$if(map.(type))$$type$ "+ \
            "$prop$=$map.(type)$;$endif$")
        map_ = {}
        map_["int"] = "0"
        t["map"] = map_
        t["prop"] = "x"
        t["type"] = "int"
        self.assertEqual(str(t), "int x=0;")


    def testIFBoolean(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        t = stringtemplate.StringTemplate(group, \
            "$if(b)$x$endif$ $if(!b)$y$endif$")
	t.setAttribute("b", True)
	self.assertEqual(str(t), "x ")

	t = t.getInstanceOf()
	t.setAttribute("b", False)
	self.assertEqual(str(t), " y")


    def testNestedIFTemplate(self):
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


    def testElseIfClause(self):
        e = stringtemplate.StringTemplate(
            "$if(x)$"+os.linesep +
            "foo"+os.linesep +
            "$elseif(y)$"+os.linesep +
            "bar"+os.linesep +
            "$endif$"
            )
        e.setAttribute("y", "yep")
        expecting = "bar"
        self.assertEqual(e.toString(), expecting)


    def testElseIfClause2(self):
        e = stringtemplate.StringTemplate(
            "$if(x)$"+os.linesep +
            "foo"+os.linesep +
            "$elseif(y)$"+os.linesep +
            "bar"+os.linesep +
            "$elseif(z)$"+os.linesep +
            "blort"+os.linesep +
            "$endif$"
            )
        e.setAttribute("z", "yep")
        expecting = "blort"
        self.assertEqual(e.toString(), expecting)


    def testElseIfClauseAndElse(self):
        e = stringtemplate.StringTemplate(
            "$if(x)$"+os.linesep +
            "foo"+os.linesep +
            "$elseif(y)$"+os.linesep +
            "bar"+os.linesep +
            "$elseif(z)$"+os.linesep +
            "z"+os.linesep +
            "$else$"+os.linesep +
            "blort"+os.linesep +
            "$endif$"
            )
        expecting = "blort"
        self.assertEqual(e.toString(), expecting)


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
        self.group = stringtemplate.StringTemplateGroup("dummy", ".",
	    stringtemplate.language.AngleBracketTemplateLexer.Lexer)
	self.group.defineTemplate("tree", \
                             "<if(it.firstChild)>"+ \
                             "( <it.text> <it.children:tree(); separator=\" \"> )" + \
                             "<else>" + \
                             "<it.text>" + \
                             "<endif>")
        tree = self.group.getInstanceOf("tree")
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
        	# $A:{$attr:foo(x="\:dog\\"")$ is cool}$

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
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer,
            self.errors
            )

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
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer,
            self.errors
            )

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
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer,
            self.errors
            )

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
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer,
            self.errors
            )

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
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer,
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


class TestAlternativeWriter(unittest.TestCase):
    def runTest(self):
        buf = StringIO()

        class TestWriter(stringtemplate.StringTemplateWriter):
            def pushIndentation(self, indent):
                pass

            def popIndentation(self):
                return None
            
            def pushAnchorPoint(self):
                pass

            def popAnchorPoint(self):
                pass
            
            def setLineWidth(self, lineWidth):
                pass
            
            def write(self, s, wrap=None):
                if wrap is not None:
                    return 0

                else:
                    s = str(s)
                    buf.write(s); # just pass thru
                    return len(s)

            def writeWrapSeparator(self, str):
                return 0

            def writeSeparator(self, str):
                return self.write(str)

            
        w = TestWriter()

        group = stringtemplate.StringTemplateGroup("test")
        group.defineTemplate("bold", "<b>$x$</b>")
        name = stringtemplate.StringTemplate(group, "$name:bold(x=name)$")
        name.setAttribute("name", "Terence")
        name.write(w)
        self.assertEqual(buf.getvalue(), "<b>Terence</b>")


class TestApplyAnonymousTemplateToDictTupleAndList(unittest.TestCase):

    def runTest(self):
        st = stringtemplate.StringTemplate(
            "$items:{<li>$it$</li>}$"
            )
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
        group = stringtemplate.StringTemplateGroup("base")
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
        try:
            str(st)
            self.fail()
        except ValueError, exc:
            error = str(exc)
            expecting = "base has no super group; invalid template: super.bold"
            self.assertEqual(error, expecting)


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
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer,
            self.errors
            )

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
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates)
            )


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
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates)
            )
	stringtemplate.StringTemplate.setLintMode(True)

    def tearDown(self):
	stringtemplate.StringTemplate.setLintMode(False)

    def runTest(self):
        b = self.group.getInstanceOf("block")
        ifstat = self.group.getInstanceOf("ifstat")
	b["stats"] = ifstat # block has if stat
	expectingError = \
            "infinite recursion to <ifstat(['stats'])@4> referenced in <block(['stats'])@3>; stack trace:" + os.linesep + \
            "<ifstat(['stats'])@4>, attributes=[stats=<block()@3>]>" + os.linesep + \
            "<block(['stats'])@3>, attributes=[stats=<ifstat()@4>], references=['stats']>" + os.linesep + \
            "<ifstat(['stats'])@4> (start of recursive cycle)" + os.linesep + \
            "..."
	# note that attributes attribute doesn't show up in ifstat() because
	# recursion detection traps the problem before it writes out the
	# infinitely-recursive template; I set the attributes attribute right
	# before I render.
	errors = ""
	try:
	    ifstat["stats"] = b # but make "if" contain block
            result = str(b)
	except stringtemplate.language.IllegalStateException, e:
            errors = str(e)
        except Exception, e:
      	    traceback.print_exc()
            raise e

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
	self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates)
            )

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
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates)
            )

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
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates)
            )

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
            "file(variables) ::= <<" + \
            "<variables:{ v | <v.decl:(v.format)()>}; separator=\"\\n\">"+os.linesep + \
            ">>"+os.linesep+ \
            "intdecl(decl)::= \"int <decl.name> = 0;\""+os.linesep + \
            "intarray(decl)::= \"int[] <decl.name> = null;\""+os.linesep
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates)
            )

    def runTest(self):
        f = self.group.getInstanceOf("file")
	f.setAttribute("variables.{decl,format}", Decl("i","int"), "intdecl")
	f.setAttribute("variables.{decl,format}", Decl("a","int-array"), "intarray")
	# sys.stderr.write("f='"+f+"'\n")
	expecting = "int i = 0;" +os.linesep+ \
            "int[] a = null;"
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
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates)
            )

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

        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

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
	
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

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
            
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        f = self.group.getInstanceOf("test")
	#f["name"] = "first"
	expecting = ""
	self.assertEqual(str(f), expecting)


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


class TestDictPropertyFetchEmbeddedStringTemplate(unittest.TestCase):

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


class TestEmptyIteratedConditionalValueGetsSeparator(unittest.TestCase):

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
	expecting = "Terence,,Frank,"
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
	result = str(t)
	self.assertEqual(result, expecting)


class TestWhiteSpaceAtEndOfTemplate(unittest.TestCase):

    def runTest(self):
        group = stringtemplate.StringTemplateGroup("group", ".")
        pageST = group.getInstanceOf("page")
        listST = group.getInstanceOf("users_list")
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
	t = stringtemplate.StringTemplate(
            self.group,
            "begin\n" +
	    "$duh.users:{name: $it$}; separator=\", \"$\n" +
            "end\n"
            )
	t["duh",] = Duh()
	expecting = "begin\nend\n"
	result = str(t)
	self.assertEqual(result, expecting)


class TestNoOutput(unittest.TestCase):
    def testNullListGetsNoOutput(self):
        group = stringtemplate.StringTemplateGroup("test")
        errors = ErrorBuffer()
        group.setErrorListener(errors)
        t = stringtemplate.StringTemplate(
            group,
            "begin\n" +
            "$users:{name: $it$}; separator=\", \"$\n" +
            "end\n"
            )
        expecting="begin\nend\n"
        result = str(t)
        self.assertEqual(result, expecting)


    def testEmptyListGetsNoOutput(self):
        group = stringtemplate.StringTemplateGroup("test")
        errors = ErrorBuffer()
        group.setErrorListener(errors)
        t = stringtemplate.StringTemplate(
            group,
            "begin\n" +
            "$users:{name: $it$}; separator=\", \"$\n" +
            "end\n"
            )
        t.setAttribute("users", [])
        expecting="begin\nend\n"
        result = str(t)
        self.assertEqual(result, expecting)


    def testEmptyListNoIteratorGetsNoOutput(self):
        group = stringtemplate.StringTemplateGroup("test")
        errors = ErrorBuffer()
        group.setErrorListener(errors)
        t = stringtemplate.StringTemplate(
            group,
            "begin\n" +
            "$users; separator=\", \"$\n" +
            "end\n"
            )
        t.setAttribute("users", [])
        expecting="begin\nend\n"
        result = str(t)
        self.assertEqual(result, expecting)


    def testEmptyExprAsFirstLineGetsNoOutput(self):
        group = stringtemplate.StringTemplateGroup("test")
        errors = ErrorBuffer()
        group.setErrorListener(errors)
        group.defineTemplate("bold", "<b>$it$</b>")
        t = stringtemplate.StringTemplate(
            group,
            "$users$\n" +
            "end\n"
            )
        expecting="end\n"
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


class TestNonNullButEmptyIteratorTestsFalse(unittest.TestCase):

    def runTest(self):
        self.group = stringtemplate.StringTemplateGroup("test")
        t = stringtemplate.StringTemplate(self.group, \
                "$if(users)$" + os.linesep + \
                "Users: $users:{$it.name$ }$" + os.linesep + \
                "$endif$")
        t["users"] = []
        expecting = ""
        result = str(t)
        self.assertEqual(result, expecting)


class TestDoNotInheritAttributesThroughFormalArgs(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "method(name) ::= \"<stat()>\"" + os.linesep + \
            "stat(name) ::= \"x=y; // <name>\"" + os.linesep
 
        # name is not visible in stat because of the formal arg called name.
        # somehow, it must be set.
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        b = self.group.getInstanceOf("method")
        b["name"] = "foo"
        expecting = "x=y; // "
        result = str(b)
        #sys.stderr.write("result='"+result+"'")
        self.assertEqual(result, expecting)


class TestArgEvaluationContext(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
          "group test;" + os.linesep + \
          "method(name) ::= \"<stat(name=name)>\"" + os.linesep + \
          "stat(name) ::= \"x=y; // <name>\"" + os.linesep

        # attribute name is not visible in stat because of the formal
        # arg called name in template stat.  However, we can set it's value
        # with an explicit name=name.  This looks weird, but makes total
        # sense as the rhs is evaluated in the context of method and the lhs
        # is evaluated in the context of stat's arg list.
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        b = self.group.getInstanceOf("method")
        b["name"] = "foo"
        expecting = "x=y; // foo"
        result = str(b)
        #sys.stderr.write("result='"+result+"'")
        self.assertEqual(result, expecting)


class TestPassThroughAttributes(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "method(name) ::= \"<stat(...)>\"" + os.linesep + \
            "stat(name) ::= \"x=y; // <name>\"" + os.linesep
                        
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        b = self.group.getInstanceOf("method")
        b["name"] = "foo"
        expecting = "x=y; // foo"
        result = str(b)
        #sys.stderr.write("result='" + result + "'")
        self.assertEqual(result, expecting)


class TestPassThroughAttributes2(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "method(name) ::= <<"+ os.linesep + \
            "<stat(value=\"34\",...)>" + os.linesep + \
            ">>" + os.linesep + \
            "stat(name,value) ::= \"x=<value>; // <name>\"" + os.linesep

        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        b = self.group.getInstanceOf("method")
        b["name"] = "foo"
        expecting = "x=34; // foo"
        result = str(b)
        #sys.stderr.write("result='"+result+"'")
        self.assertEqual(result, expecting)


class TestDefaultArgument(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "method(name) ::= <<"+ os.linesep + \
            "<stat(...)>" + os.linesep + \
            ">>"+ os.linesep + \
            "stat(name,value=\"99\") ::= \"x=<value>; // <name>\"" + os.linesep
                        
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        b = self.group.getInstanceOf("method")
        b["name"] = "foo"
        expecting = "x=99; // foo"
        result = str(b)
        #sys.stderr.write("result='"+result+"'")
        self.assertEqual(result, expecting)


class TestDefaultArgument2(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "stat(name,value=\"99\") ::= \"x=<value>; // <name>\"" + os.linesep
        
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        b = self.group.getInstanceOf("stat")
        b["name"] = "foo"
        expecting = "x=99; // foo"
        result = str(b)
        #sys.stderr.write("result='"+result+"'")
        self.assertEqual(result, expecting)


class TestDefaultArgumentAsTemplate(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "method(name,size) ::= <<"+ os.linesep + \
            "<stat(...)>" + os.linesep + \
            ">>" + os.linesep + \
            "stat(name,value={<name>}) ::= \"x=<value>; // <name>\"" + os.linesep
                        
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        b = self.group.getInstanceOf("method")
        b["name"] = "foo"
        b["size"] = "2"
        expecting = "x=foo; // foo"
        result = str(b)
        #sys.stderr.write("result='"+result+"'")
        self.assertEqual(result, expecting)


class TestDefaultArgumentAsTemplate2(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "method(name,size) ::= <<"+ os.linesep + \
            "<stat(...)>" + os.linesep + \
            ">>"+ os.linesep + \
            "stat(name,value={ [<name>] }) ::= \"x=<value>; // <name>\"" + os.linesep
                        
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        b = self.group.getInstanceOf("method")
        b["name"] = "foo"
        b["size"] = "2"
        expecting = "x= [foo] ; // foo"
        result = str(b)
        #sys.stderr.write("result='"+result+"'")
        self.assertEqual(result, expecting)


class TestDoNotUseDefaultArgument(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "method(name) ::= <<"+ os.linesep + \
            "<stat(value=\"34\",...)>" + os.linesep + \
            ">>"+ os.linesep + \
            "stat(name,value=\"99\") ::= \"x=<value>; // <name>\"" + os.linesep
                        
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        b = self.group.getInstanceOf("method")
        b["name"] = "foo"
        expecting = "x=34; // foo"
        result = str(b)
        self.assertEqual(result, expecting)


class TestArgumentsAsTemplates(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "method(name,size) ::= <<"+ os.linesep + \
            "<stat(value={<size>})>" + os.linesep + \
            ">>"+ os.linesep + \
            "stat(value) ::= \"x=<value>;\"" + os.linesep
                        
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        b = self.group.getInstanceOf("method")
        b["name"] = "foo"
        b["size"] = "34"
        expecting = "x=34;"
        result = str(b)
        self.assertEqual(result, expecting)


class TestArgumentsAsTemplatesDefaultDelimiters(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "method(name,size) ::= <<"+ os.linesep + \
            "$stat(value={$size$})$" + os.linesep + \
            ">>"+ os.linesep + \
            "stat(value) ::= \"x=$value$;\"" + os.linesep
                        
        self.group = stringtemplate.StringTemplateGroup(
            StringIO(self.templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer
            )

    def runTest(self):
        b = self.group.getInstanceOf("method")
        b["name"] = "foo"
        b["size"] = "34"
        expecting = "x=34;"
        result = str(b)
        self.assertEqual(result, expecting)


class TestDefaultArgsWhenNotInvoked(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "b(name=\"foo\") ::= \".<name>.\"" + os.linesep
                        
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        b = self.group.getInstanceOf("b")
        expecting = ".foo."
        result = str(b)
        self.assertEqual(result, expecting)


class DateRenderer(stringtemplate.AttributeRenderer):
    def toString(self, o, formatString=None):
        return o.strftime("%Y.%m.%d")


class DateRenderer2(stringtemplate.AttributeRenderer):
    def toString(self, o, formatString=None):
        return o.strftime("%m/%d/%Y")


class DateRenderer3(stringtemplate.AttributeRenderer):
    def toString(self, o, formatString='%m/%d/%Y'):
        return o.strftime(str(formatString))


class StringRenderer(stringtemplate.AttributeRenderer):
    def toString(self, o, formatString=None):
        if formatString is not None:
            if formatString == "upper":
                return o.upper()
            
        return o

class TestRenderer(unittest.TestCase):
    def testRendererForST(self):
        st = stringtemplate.StringTemplate("date: <created>",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer)
        st["created"] = date(year=2005, month=7, day=5)
        st.registerRenderer(date, DateRenderer())
        expecting = "date: 2005.07.05"
        result = str(st)
        self.assertEqual(result, expecting)


    def testRendererWithFormat(self):
        st = stringtemplate.StringTemplate(
            "date: <created; format=\"%Y.%m.%d\">",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        st.setAttribute("created", date(year=2005, month=7, day=5))
        st.registerRenderer(date, DateRenderer3())
        expecting = "date: 2005.07.05"
        result = st.toString()
        self.assertEqual(result, expecting)


    def testEmbeddedRendererSeesEnclosing(self):
        # st is embedded in outer; set renderer on outer, st should
        # still see it.
        outer = stringtemplate.StringTemplate(
            "X: <x>",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        st = stringtemplate.StringTemplate(
            "date: <created>",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        st.setAttribute("created",
                        date(year=2005, month=07, day=05))
        outer.setAttribute("x", st)
        outer.registerRenderer(date, DateRenderer())
        expecting = "X: date: 2005.07.05"
        result = str(outer)
        self.assertEqual(result, expecting)


    def testRendererForGroup(self):

        templates = \
            "group test;" + os.linesep + \
            "dateThing(created) ::= \"date: <created>\"" + os.linesep
                        
        group = stringtemplate.StringTemplateGroup(StringIO(templates))

        st = group.getInstanceOf("dateThing")
        st["created"] = date(year=2005, month=7, day=5)
        group.registerRenderer(date, DateRenderer())
        expecting = "date: 2005.07.05"
        result = str(st)
        self.assertEqual(result, expecting)


    def testOverriddenRenderer(self):
        templates = \
            "group test;" + os.linesep + \
            "dateThing(created) ::= \"date: <created>\"" + os.linesep
                        
        group = stringtemplate.StringTemplateGroup(StringIO(templates))
        st = group.getInstanceOf("dateThing")
        st["created"] = date(year=2005, month=7, day=5)
        group.registerRenderer(date, DateRenderer())
        st.registerRenderer(date, DateRenderer2())
        expecting = "date: 07/05/2005"
        result = str(st)
        self.assertEqual(result, expecting)


    def testRendererWithFormatAndList(self):
        st = stringtemplate.StringTemplate(
            "The names: <names; format=\"upper\">",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        st.setAttribute("names", "ter")
        st.setAttribute("names", "tom")
        st.setAttribute("names", "sriram")
        st.registerRenderer(str, StringRenderer())
        expecting = "The names: TERTOMSRIRAM"
        result = st.toString()
        self.assertEqual(result, expecting)


    def testRendererWithFormatAndSeparator(self):
        st = stringtemplate.StringTemplate(
            "The names: <names; separator=\" and \", format=\"upper\">",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        st.setAttribute("names", "ter")
        st.setAttribute("names", "tom")
        st.setAttribute("names", "sriram")
        st.registerRenderer(str, StringRenderer())
        expecting = "The names: TER and TOM and SRIRAM"
        result = st.toString()
        self.assertEqual(result, expecting)


    def testRendererWithFormatAndSeparatorAndNull(self):
        st = stringtemplate.StringTemplate(
            "The names: <names; separator=\" and \", null=\"n/a\", format=\"upper\">",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        names = [ "ter", None, "sriram" ]
        st.setAttribute("names", names)
        st.registerRenderer(str, StringRenderer())
        expecting = "The names: TER and N/A and SRIRAM"
        result = st.toString()
        self.assertEqual(result, expecting)


class TestMap(unittest.TestCase):
        
    def testMap(self):
        templates = \
            "group test;" + os.linesep + \
            "typeInit ::= [\"int\":\"0\", \"float\":\"0.0\"] "+ os.linesep + \
            "var(type,name) ::= \"<type> <name> = <typeInit.(type)>;\"" + os.linesep
                        
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates)
            )

        st = group.getInstanceOf("var")
        st["type"] = "int"
        st["name"] = "x"
        expecting = "int x = 0;"
        result = str(st)
        self.assertEqual(result, expecting)


    def testMapValuesAreTemplates(self):
        templates = (
            "group test;" +os.linesep+
            "typeInit ::= [\"int\":\"0<w>\", \"float\":\"0.0<w>\"] "+os.linesep+
            "var(type,w,name) ::= \"<type> <name> = <typeInit.(type)>;\""+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates)
            )
        st = group.getInstanceOf("var")
        st.setAttribute("w", "L")
        st.setAttribute("type", "int")
        st.setAttribute("name", "x")
        expecting = "int x = 0L;"
        result = str(st)
        self.assertEqual(result, expecting)


    def testMapMissingDefaultValueIsEmpty(self):
        templates = (
            "group test;" +os.linesep+
            "typeInit ::= [\"int\":\"0\", \"float\":\"0.0\"] "+os.linesep+
            "var(type,w,name) ::= \"<type> <name> = <typeInit.(type)>;\""+os.linesep
            )
        group =  stringtemplate.StringTemplateGroup(
            StringIO(templates)
            )
        st = group.getInstanceOf("var")
        st.setAttribute("w", "L")
        st.setAttribute("type", "double") # double not in typeInit map
        st.setAttribute("name", "x")
        expecting = "double x = ;" # weird, but tests default value is key
        result = str(st)
        self.assertEqual(result, expecting)


    def testMapHiddenByFormalArg(self):
        templates = \
            "group test;" + os.linesep + \
            "typeInit ::= [\"int\":\"0\", \"float\":\"0.0\"] "+ os.linesep + \
            "var(typeInit,type,name) ::= \"<type> <name> = <typeInit.(type)>;\"" + os.linesep
                        
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates)
            )

        st = group.getInstanceOf("var")
        st["type"] = "int"
        st["name"] = "x"
        expecting = "int x = ;"
        result = str(st)
        self.assertEqual(result, expecting)


    def testMapEmptyValueAndAngleBracketStrings(self):
        templates = (
            "group test;" +os.linesep+
            "typeInit ::= [\"int\":\"0\", \"float\":, \"double\":<<0.0L>>] "+os.linesep+
            "var(type,name) ::= \"<type> <name> = <typeInit.(type)>;\""+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates)
            )
        st = group.getInstanceOf("var")
        st.setAttribute("type", "float")
        st.setAttribute("name", "x")
        expecting = "float x = ;"
        result = str(st)
        self.assertEqual(result, expecting)


    def testMapDefaultValue(self):
        templates = \
            "group test;" + os.linesep + \
            "typeInit ::= [\"int\":\"0\", default:\"null\"] "+ os.linesep + \
            "var(type,name) ::= \"<type> <name> = <typeInit.(type)>;\"" + os.linesep
                        
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates)
            )

        st = group.getInstanceOf("var")
        st["type"] = "UserRecord"
        st["name"] = "x"
        expecting = "UserRecord x = null;"
        result = str(st)
        self.assertEqual(result, expecting)


    def testMapEmptyDefaultValue(self):
        templates = (
            "group test;" +os.linesep+
            "typeInit ::= [\"int\":\"0\", default:] "+os.linesep+
            "var(type,name) ::= \"<type> <name> = <typeInit.(type)>;\""+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates)
            )
        st = group.getInstanceOf("var")
        st.setAttribute("type", "UserRecord")
        st.setAttribute("name", "x")
        expecting = "UserRecord x = ;"
        result = str(st)
        self.assertEqual(result, expecting)


    def testMapDefaultValueIsKey(self):
        templates = (
            "group test;" +os.linesep+
            "typeInit ::= [\"int\":\"0\", default:key] "+os.linesep+
            "var(type,name) ::= \"<type> <name> = <typeInit.(type)>;\""+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates)
            )
        st = group.getInstanceOf("var")
        st.setAttribute("type", "UserRecord")
        st.setAttribute("name", "x")
        expecting = "UserRecord x = UserRecord;"
        result = str(st)
        self.assertEqual(result, expecting)


    def testMapViaEnclosingTemplates(self):
        templates = \
            "group test;" + os.linesep + \
            "typeInit ::= [\"int\":\"0\", \"float\":\"0.0\"] "+ os.linesep + \
            "intermediate(type,name) ::= \"<var(...)>\""+ os.linesep + \
            "var(type,name) ::= \"<type> <name> = <typeInit.(type)>;\"" + os.linesep
                        
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates)
            )

        st = group.getInstanceOf("intermediate")
        st["type"] = "int"
        st["name"] = "x"
        expecting = "int x = 0;"
        result = str(st)
        self.assertEqual(result, expecting)


    def testMapViaEnclosingTemplates2(self):
        templates = \
            "group test;" + os.linesep + \
            "typeInit ::= [\"int\":\"0\", \"float\":\"0.0\"] "+ os.linesep + \
            "intermediate(stuff) ::= \"<stuff>\""+ os.linesep + \
            "var(type,name) ::= \"<type> <name> = <typeInit.(type)>;\"" + os.linesep
                        
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates)
            )

        interm = group.getInstanceOf("intermediate")
        var = group.getInstanceOf("var")
        var["type"] = "int"
        var["name"] = "x"
        interm["stuff"] = var
        expecting = "int x = 0;"
        result = str(interm)
        self.assertEqual(result, expecting)


    def testMapDefaultStringAsKey(self):
        """
        Test that a map can have only the default entry.

        Bug ref: JIRA bug ST-15 (Fixed)
        """
        
        templates = (
            "group test;" +os.linesep+
            "typeInit ::= [\"default\":\"foo\"] "+os.linesep+
            "var(type,name) ::= \"<type> <name> = <typeInit.(type)>;\""+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(StringIO(templates))
        st = group.getInstanceOf("var")
        st.setAttribute("type", "default")
        st.setAttribute("name", "x")
        expecting = "default x = foo;"
        result = st.toString()
        self.assertEqual(result, expecting)


    def testMapDefaultIsDefaultString(self):
        """
        Test that a map can return a <b>string</b> with the word: default.
        
        Bug ref: JIRA bug ST-15 (Fixed)
        """
        
        templates = (
            "group test;" +os.linesep+
            "map ::= [default: \"default\"] "+os.linesep+
            "t1() ::= \"<map.(1)>\""+os.linesep                
            )
        group = stringtemplate.StringTemplateGroup(StringIO(templates))
        st = group.getInstanceOf("t1")
        expecting = "default"
        result = st.toString()
        self.assertEqual(result, expecting)


class TestEmptyGroupTemplate(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "foo() ::= \"\"" + os.linesep
                        
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        a = self.group.getInstanceOf("foo")
        expecting = ""
        result = str(a)
        self.assertEqual(result, expecting)


class TestEmptyString(unittest.TestCase):
    def testEmptyStringAndEmptyAnonTemplateAsParameterUsingAngleBracketLexer(self):
        templates = (
            "group test;" +os.linesep+
            "top() ::= <<<x(a=\"\", b={})\\>>>"+os.linesep+
            "x(a,b) ::= \"a=<a>, b=<b>\""+os.linesep
            )

        group = stringtemplate.StringTemplateGroup(StringIO(templates))
        a = group.getInstanceOf("top")
        expecting = "a=, b="
        result = str(a)
        self.assertEqual(result, expecting)


    def testEmptyStringAndEmptyAnonTemplateAsParameterUsingDollarLexer(self):
        templates = (
            "group test;" +os.linesep+
            "top() ::= <<$x(a=\"\", b={})$>>"+os.linesep+
            "x(a,b) ::= \"a=$a$, b=$b$\""+os.linesep
            )

        group = stringtemplate.StringTemplateGroup(
            StringIO(templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer
            )
        a = group.getInstanceOf("top")
        expecting = "a=, b="
        result = str(a)
        self.assertEqual(result, expecting)


## class Test8BitEuroChars(unittest.TestCase):

##     def runTest(self):
##         e = stringtemplate.StringTemplate(
##             "Danish: Å char"
##             )
##         e = e.getInstanceOf()
##         expecting = "Danish: Å char"
##         self.assertEqual(str(e), expecting)

## public void test16BitUnicodeChar() throws Exception {
## StringTemplate e = new StringTemplate(
## "DINGBAT CIRCLED SANS-SERIF DIGIT ONE: \u2780"
## );
## e = e.getInstanceOf();
## String expecting = "DINGBAT CIRCLED SANS-SERIF DIGIT ONE: \u2780";
## assertEquals(expecting, e.toString());

class TestFirstOp(unittest.TestCase):

    def runTest(self):
        e = stringtemplate.StringTemplate( \
            "$first(names)$" \
            )
        e = e.getInstanceOf()
        e["names"] = "Ter"
        e["names"] = "Tom"
        e["names"] = "Sriram"
        expecting = "Ter"
        self.assertEqual(str(e), expecting)


class TestRestOp(unittest.TestCase):

    def runTest(self):
        e = stringtemplate.StringTemplate( \
            "$rest(names); separator=\", \"$" \
            )
        e = e.getInstanceOf()
        e["names"] = "Ter"
        e["names"] = "Tom"
        e["names"] = "Sriram"
        expecting = "Tom, Sriram"
        self.assertEqual(str(e), expecting)


class TestLastOp(unittest.TestCase):

    def runTest(self):
        e = stringtemplate.StringTemplate( \
            "$last(names)$" \
            )
        e = e.getInstanceOf()
        e["names"] = "Ter"
        e["names"] = "Tom"
        e["names"] = "Sriram"
        expecting = "Sriram"
        self.assertEqual(str(e), expecting)


class TestCombinedOp(unittest.TestCase):

    def runTest(self):
        # replace first of yours with first of mine
        e = stringtemplate.StringTemplate( \
            "$[first(mine),rest(yours)]; separator=\", \"$" \
            )
        e = e.getInstanceOf()
        e["mine"] = "1"
        e["mine"] = "2"
        e["mine"] = "3"
        e["yours"] = "a"
        e["yours"] = "b"
        expecting = "1, b"
        self.assertEqual(str(e), expecting)


class TestCatListAndSingleAttribute(unittest.TestCase):

    def runTest(self):
        # replace first of yours with first of mine
        e = stringtemplate.StringTemplate( \
            "$[mine,yours]; separator=\", \"$" \
            )
        e = e.getInstanceOf()
        e["mine"] = "1"
        e["mine"] = "2"
        e["mine"] = "3"
        e["yours"] = "a"
        expecting = "1, 2, 3, a"
        self.assertEqual(str(e), expecting)


class TestCatListAndEmptyAttributes(unittest.TestCase):

    def runTest(self):
        # + is overloaded to be cat strings and cat lists so the
        # two operands (from left to right) determine which way it
        # goes.  In this case, x+mine is a list so everything from their
        # to the right becomes list cat.
        e = stringtemplate.StringTemplate( \
            "$[x,mine,y,yours,z]; separator=\", \"$" \
            )
        e = e.getInstanceOf()
        e["mine"] = "1"
        e["mine"] = "2"
        e["mine"] = "3"
        e["yours"] = "a"
        expecting = "1, 2, 3, a"
        self.assertEqual(str(e), expecting)


class TestNestedOp(unittest.TestCase):

    def runTest(self):
        # gets 2nd element 
        e = stringtemplate.StringTemplate( \
            "$first(rest(names))$" \
            )
        e = e.getInstanceOf()
        e["names"] = "Ter"
        e["names"] = "Tom"
        e["names"] = "Sriram"
        expecting = "Tom"
        self.assertEqual(str(e), expecting)


class TestFirstWithOneAttributeOp(unittest.TestCase):

    def runTest(self):
        e = stringtemplate.StringTemplate( \
            "$first(names)$" \
            )
        e = e.getInstanceOf()
        e["names"] = "Ter"
        expecting = "Ter"
        self.assertEqual(str(e), expecting)


class TestLastWithOneAttributeOp(unittest.TestCase):

    def runTest(self):
        e = stringtemplate.StringTemplate( \
            "$last(names)$" \
            )
        e = e.getInstanceOf()
        e["names"] = "Ter"
        expecting = "Ter"
        self.assertEqual(str(e), expecting)


class TestLastWithLengthOneListAttributeOp(unittest.TestCase):

    def runTest(self):
        e = stringtemplate.StringTemplate( \
            "$last(names)$" \
            )
        e = e.getInstanceOf()
        e["names"] = ["Ter"]
        expecting = "Ter"
        self.assertEqual(str(e), expecting)


class TestRestOp(unittest.TestCase):

    def testRestWithOneAttributeOp(self):
        e = stringtemplate.StringTemplate( \
            "$rest(names)$" \
            )
        e = e.getInstanceOf()
        e["names"] = "Ter"
        expecting = ""
        self.assertEqual(str(e), expecting)


    def testRestWithLengthOneListAttributeOp(self):
        e = stringtemplate.StringTemplate( \
            "$rest(names)$" \
            )
        e = e.getInstanceOf()
        e["names"] = ["Ter"]
        expecting = ""
        self.assertEqual(str(e), expecting)


    def testRepeatedRestOp(self):
        e = stringtemplate.StringTemplate(
            "$rest(names)$, $rest(names)$" # gets 2nd element
            )
        e = e.getInstanceOf()
        e.setAttribute("names", "Ter")
        e.setAttribute("names", "Tom")
        expecting = "Tom, Tom"
        self.assertEqual(str(e), expecting)


    def testRepeatedIteratedAttrFromArg(self):
        """
        If an iterator is sent into ST, it must be cannot be reset after each
        use so repeated refs yield empty values.  This would
        work if we passed in a List not an iterator.  Avoid sending in
        iterators if you ref it twice.
        """
        
        templates = (
            "group test;" +os.linesep+
            "root(names) ::= \"$other(names)$\""+os.linesep+
            "other(x) ::= \"$x$, $x$\""+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer
            )
        e = group.getInstanceOf("root")
        names = [ "Ter", "Tom" ]
        e.setAttribute("names", iter(names))
        expecting = "TerTom, ";  # This does not give TerTom twice!!
        self.assertEqual(e.toString(), expecting)


    # BUG!  Fix this.  Iterator is not reset from first to second $x$
    # Either reset the iterator or pass an attribute that knows to get
    # the iterator each time.  Seems like first, tail do not
    # have same problem as they yield objects.

    # Maybe make a RestIterator like I have CatIterator.
    def testRepeatedRestOpAsArg(self):
        templates = (
            "group test;" + os.linesep +
            "root(names) ::= \"$other(rest(names))$\"" + os.linesep +
            "other(x) ::= \"$x$, $x$\"" + os.linesep
            )
        
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer
            )
        e = group.getInstanceOf("root")
        e.setAttribute("names", "Ter")
        e.setAttribute("names", "Tom")
        expecting = "Tom, Tom"
        self.assertEqual(str(e), expecting)


class TestIncomingLists(unittest.TestCase):
    def testIncomingLists(self):
        e = stringtemplate.StringTemplate(
            "$rest(names)$, $rest(names)$" # gets 2nd element
            )
        e = e.getInstanceOf()
        e.setAttribute("names", "Ter")
        e.setAttribute("names", "Tom")
        expecting = "Tom, Tom"
        self.assertEqual(str(e), expecting)


    def testIncomingListsAreNotModified(self):
        e = stringtemplate.StringTemplate(
            "$names; separator=\", \"$" # gets 2nd element
            )
        e = e.getInstanceOf()
        names = []
        names.append("Ter")
        names.append("Tom")
        e.setAttribute("names", names)
        e.setAttribute("names", "Sriram")
        expecting = "Ter, Tom, Sriram"
        self.assertEqual(str(e), expecting)
        self.assertEqual(len(names), 2)


    def testIncomingListsAreNotModified2(self):
        e = stringtemplate.StringTemplate(
            "$names; separator=\", \"$" # gets 2nd element
            )
        e = e.getInstanceOf()
        names = []
        names.append("Ter")
        names.append("Tom")
        e.setAttribute("names", "Sriram") # single element first now
        e.setAttribute("names", names)
        expecting = "Sriram, Ter, Tom"
        self.assertEqual(str(e), expecting)
        self.assertEqual(len(names), 2)


    def testIncomingArraysAreOk(self):
        e = stringtemplate.StringTemplate(
            "$names; separator=\", \"$" # gets 2nd element
            )
        e = e.getInstanceOf()
        e.setAttribute("names", ["Ter","Tom"])
        e.setAttribute("names", "Sriram")
        expecting = "Ter, Tom, Sriram"
        self.assertEqual(str(e), expecting)


class TestApplyTemplateWithSingleFormalArgs(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "test(names) ::= <<<names:bold(item=it); separator=\", \"> >>"+ os.linesep + \
            "bold(item) ::= <<*<item>*>>" + os.linesep
                        
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        e = self.group.getInstanceOf("test")
        e["names"] = "Ter"
        e["names"] = "Tom"
        expecting = "*Ter*, *Tom* "
        result = str(e)
        self.assertEqual(result, expecting)


class TestApplyTemplateWithNoFormalArgs(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "test(names) ::= <<<names:bold(); separator=\", \"> >>"+ os.linesep + \
            "bold() ::= <<*<it>*>>" + os.linesep
                        
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
                        stringtemplate.language.AngleBracketTemplateLexer.Lexer)

    def runTest(self):
        e = self.group.getInstanceOf("test")
        e["names"] = "Ter"
        e["names"] = "Tom"
        expecting = "*Ter*, *Tom* "
        result = str(e)
        self.assertEqual(result, expecting)


class TestAnonTemplate(unittest.TestCase):
    def testAnonTemplateWithArgHasNoITArg(self):
        e = stringtemplate.StringTemplate(
            "$names:{n| $n$:$it$}; separator=\", \"$"
            )
        e = e.getInstanceOf()
        e.setAttribute("names", "Ter")
        e.setAttribute("names", "Tom")

        try:
            str(e)
            self.fail()
            
        except KeyError, nse:
            error = str(nse)
            expecting = "'no such attribute: it in template context [anonymous anonymous]'"
            self.assertEqual(error, expecting)


    def testAnonTemplateArgs(self):
        e = stringtemplate.StringTemplate( \
            "$names:{n| $n$}; separator=\", \"$" \
            )
        e = e.getInstanceOf()
        e["names"] = "Ter"
        e["names"] = "Tom"
        expecting = "Ter, Tom"
        self.assertEqual(str(e), expecting)


    def testAnonTemplateArgs2(self):
        e = stringtemplate.StringTemplate( \
            "$names:{n| .$n$.}:{ n | _$n$_}; separator=\", \"$" \
            )
        e = e.getInstanceOf()
        e["names"] = "Ter"
        e["names"] = "Tom"
        expecting = "_.Ter._, _.Tom._"
        self.assertEqual(str(e), expecting)


class TestFirstWithCatAttribute(unittest.TestCase):

    def runTest(self):
        e = stringtemplate.StringTemplate( \
            "$first([names,phones])$" \
            )
        e = e.getInstanceOf()
        e["names"] = "Ter"
        e["names"] = "Tom"
        e["phones"] = "1"
        e["phones"] = "2"
        expecting = "Ter"
        self.assertEqual(str(e), expecting)


class TestJustCat(unittest.TestCase):

    def runTest(self):
        e = stringtemplate.StringTemplate( \
            "$[names,phones]$" \
            )
        e = e.getInstanceOf()
        e["names"] = "Ter"
        e["names"] = "Tom"
        e["phones"] = "1"
        e["phones"] = "2"
        expecting = "TerTom12"
        self.assertEqual(str(e), expecting)


class TestCat2Attributes(unittest.TestCase):

    def runTest(self):
        e = stringtemplate.StringTemplate(
            "$[names,phones]; separator=\", \"$"
            )
        e = e.getInstanceOf()
        e["names"] = "Ter"
        e["names"] = "Tom"
        e["phones"] = "1"
        e["phones"] = "2"
        expecting = "Ter, Tom, 1, 2"
        self.assertEqual(str(e), expecting)


class TestCat2AttributesWithApply(unittest.TestCase):

    def runTest(self):
        e = stringtemplate.StringTemplate( \
            "$[names,phones]:{a|$a$.}$" \
            )
        e = e.getInstanceOf()
        e["names"] = "Ter"
        e["names"] = "Tom"
        e["phones"] = "1"
        e["phones"] = "2"
        expecting = "Ter.Tom.1.2."
        self.assertEqual(str(e), expecting)


class TestCat3Attributes(unittest.TestCase):

    def runTest(self):
        e = stringtemplate.StringTemplate( \
            "$[names,phones,salaries]; separator=\", \"$" \
            )
        e = e.getInstanceOf()
        e["names"] = "Ter"
        e["names"] = "Tom"
        e["phones"] = "1"
        e["phones"] = "2"
        e["salaries"] = "big"
        e["salaries"] = "huge"
        expecting = "Ter, Tom, 1, 2, big, huge"
        self.assertEqual(str(e), expecting)


class TestListAsTemplateArgument(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "test(names,phones) ::= \"<foo([names,phones])>\""+ os.linesep + \
            "foo(items) ::= \"<items:{a | *<a>*}>\"" + os.linesep
                        
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
                        stringtemplate.language.AngleBracketTemplateLexer.Lexer)

    def runTest(self):
        e = self.group.getInstanceOf("test")
        e["names"] = "Ter"
        e["names"] = "Tom"
        e["phones"] = "1"
        e["phones"] = "2"
        expecting = "*Ter**Tom**1**2*"
        result = str(e)
        self.assertEqual(result, expecting)


class TestSingleExprTemplateArgument(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "test(name) ::= \"<bold(name)>\""+ os.linesep + \
            "bold(item) ::= \"*<item>*\"" + os.linesep
                        
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
                        stringtemplate.language.AngleBracketTemplateLexer.Lexer)

    def runTest(self):
        e = self.group.getInstanceOf("test")
        e["name"] = "Ter"
        expecting = "*Ter*"
        result = str(e)
        self.assertEqual(result, expecting)


class TestSingleExprTemplateArgumentInApply(unittest.TestCase):
        # when you specify a single arg on a template application
        # it overrides the setting of the iterated value "it" to that
        # same single formal arg.  Your arg hides the implicitly set "it".
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "test(names,x) ::= \"<names:bold(x)>\""+ os.linesep + \
            "bold(item) ::= \"*<item>*\"" + os.linesep
                        
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
                        stringtemplate.language.AngleBracketTemplateLexer.Lexer)

    def runTest(self):
        e = self.group.getInstanceOf("test")
        e["names"] = "Ter"
        e["names"] = "Tom"
        e["x"] = "ick"
        expecting = "*ick**ick*"
        result = str(e)
        self.assertEqual(result, expecting)


class TestSoleFormalTemplateArgumentInMultiApply(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "test(names) ::= \"<names:bold(),italics()>\""+ os.linesep + \
            "bold(x) ::= \"*<x>*\""+ os.linesep + \
            "italics(y) ::= \"_<y>_\"" + os.linesep
                        
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
                        stringtemplate.language.AngleBracketTemplateLexer.Lexer)

    def runTest(self):
        e = self.group.getInstanceOf("test")
        e["names"] = "Ter"
        e["names"] = "Tom"
        expecting = "*Ter*_Tom_"
        result = str(e)
        self.assertEqual(result, expecting)


class TestSingleExprTemplateArgumentError(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "test(name) ::= \"<bold(name)>\""+ os.linesep + \
            "bold(item,ick) ::= \"*<item>*\"" + os.linesep
                        
        self.errors = ErrorBuffer()
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates),
                        stringtemplate.language.AngleBracketTemplateLexer.Lexer, self.errors)

    def runTest(self):
        e = self.group.getInstanceOf("test")
        e["name"] = "Ter"
        result = str(e)
        expecting = "template bold must have exactly one formal arg in template context [test <invoke bold arg context>]\n"
        self.assertEqual(str(self.errors), expecting)


class TestInvokeIndirectTemplateWithSingleFormalArgs(unittest.TestCase):
        
    def setUp(self):
        self.templates = \
            "group test;" + os.linesep + \
            "test(templateName,arg) ::= \"<(templateName)(arg)>\""+ os.linesep + \
            "bold(x) ::= <<*<x>*>>"+ os.linesep + \
            "italics(y) ::= <<_<y>_>>" + os.linesep
                        
        self.group = stringtemplate.StringTemplateGroup(StringIO(self.templates))

    def runTest(self):
        e = self.group.getInstanceOf("test")
        e["templateName"] = "italics"
        e["arg"] = "Ter"
        expecting = "_Ter_"
        result = str(e)
        self.assertEqual(result, expecting)


class TestParallelAttributeIteration(unittest.TestCase):
    def testParallelAttributeIterationHasI(self):
        e = stringtemplate.StringTemplate(
            "$names,phones,salaries:{n,p,s | $i0$. $n$@$p$: $s$\n}$"
            )
        e = e.getInstanceOf()
        e.setAttribute("names", "Ter")
        e.setAttribute("names", "Tom")
        e.setAttribute("phones", "1")
        e.setAttribute("phones", "2")
        e.setAttribute("salaries", "big")
        e.setAttribute("salaries", "huge")
        expecting = "0. Ter@1: big"+os.linesep+"1. Tom@2: huge"+os.linesep
        self.assertEqual(str(e), expecting)


    def testParallelAttributeIteration(self):
        e = stringtemplate.StringTemplate( \
            "$names,phones,salaries:{n,p,s | $n$@$p$: $s$\n}$" \
            )
        e = e.getInstanceOf()
        e["names"] = "Ter"
        e["names"] = "Tom"
        e["phones"] = "1"
        e["phones"] = "2"
        e["salaries"] = "big"
        e["salaries"] = "huge"
        expecting = "Ter@1: big" + os.linesep + "Tom@2: huge" + os.linesep
        self.assertEqual(str(e), expecting)


    def testParallelAttributeIterationWithDifferentSizes(self):
        e = stringtemplate.StringTemplate( \
            "$names,phones,salaries:{n,p,s | $n$@$p$: $s$}; separator=\", \"$" \
            )
        e = e.getInstanceOf()
        e["names"] = "Ter"
        e["names"] = "Tom"
        e["names"] = "Sriram"
        e["phones"] = "1"
        e["phones"] = "2"
        e["salaries"] = "big"
        expecting = "Ter@1: big, Tom@2: , Sriram@: "
        self.assertEqual(str(e), expecting)


    def testParallelAttributeIterationWithSingletons(self):
        e = stringtemplate.StringTemplate( \
            "$names,phones,salaries:{n,p,s | $n$@$p$: $s$}; separator=\", \"$" \
            )
        e = e.getInstanceOf()
        e["names"] = "Ter"
        e["phones"] = "1"
        e["salaries"] = "big"
        expecting = "Ter@1: big"
        self.assertEqual(str(e), expecting)


    def testParallelAttributeIterationWithMismatchArgListSizes(self):
        self.errors = ErrorBuffer()
        e = stringtemplate.StringTemplate( \
            "$names,phones,salaries:{n,p | $n$@$p$}; separator=\", \"$" \
            )
        e.setErrorListener(self.errors)
        e = e.getInstanceOf()
        e["names"] = "Ter"
        e["names"] = "Tom"
        e["phones"] = "1"
        e["phones"] = "2"
        e["salaries"] = "big"
        expecting = "Ter@1, Tom@2"
        self.assertEqual(str(e), expecting)
        errorExpecting = "number of arguments ['n', 'p'] mismatch between attribute list and anonymous template in context [anonymous]\n"
        self.assertEqual(str(self.errors), errorExpecting)


    def testParallelAttributeIterationWithMissingArgs(self):
        self.errors = ErrorBuffer()
        e = stringtemplate.StringTemplate( \
            "$names,phones,salaries:{$n$@$p$}; separator=\", \"$" \
            )
        e.setErrorListener(self.errors)
        e = e.getInstanceOf()
        e["names"] = "Tom"
        e["phones"] = "2"
        e["salaries"] = "big"
        str(e); # generate the error
        errorExpecting = "missing arguments in anonymous template in context [anonymous]\n"
        self.assertEqual(str(self.errors), errorExpecting)


    def testParallelAttributeIterationWithDifferentSizesTemplateRefInsideToo(self):
        templates = \
            "group test;" + os.linesep + \
            "page(names,phones,salaries) ::= "+ os.linesep + \
            "   <<$names,phones,salaries:{n,p,s | $value(n)$@$value(p)$: $value(s)$}; separator=\", \"$>>" + os.linesep + \
            "value(x=\"n/a\") ::= \"$x$\"" + os.linesep
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer
            )

        p = group.getInstanceOf("page")
        p["names"] = "Ter"
        p["names"] = "Tom"
        p["names"] = "Sriram"
        p["phones"] = "1"
        p["phones"] = "2"
        p["salaries"] = "big"
        expecting = "Ter@1: big, Tom@2: n/a, Sriram@n/a: n/a"
        self.assertEqual(str(p), expecting)


    def testParallelAttributeIterationWithNullValue(self):
        e = stringtemplate.StringTemplate(
            "$names,phones,salaries:{n,p,s | $n$@$p$: $s$\n}$"
            )
        e = e.getInstanceOf()
        e.setAttribute("names", "Ter")
        e.setAttribute("names", "Tom")
        e.setAttribute("names", "Sriram")
        e.setAttribute("phones", ["1", None, "3"])
        e.setAttribute("salaries", "big");
        e.setAttribute("salaries", "huge");
        e.setAttribute("salaries", "enormous");
        expecting = (
            "Ter@1: big"+os.linesep+
            "Tom@: huge"+os.linesep+
            "Sriram@3: enormous"+os.linesep
            )
        self.assertEqual(e.toString(), expecting)


class TestAnonTemplateOnLeftOfApply(unittest.TestCase):

    def runTest(self):
        e = stringtemplate.StringTemplate( \
            "${foo}:{($it$)}$" \
            )
        expecting = "(foo)"
        self.assertEqual(str(e), expecting)


class TestOverrideThroughConditional(unittest.TestCase):

    def runTest(self):
        templates = (
            "group base;" + os.linesep +
            "body(ick) ::= \"<if(ick)>ick<f()><else><f()><endif>\"" +
            "f() ::= \"foo\"" + os.linesep
            )
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates)
            )
        templates2 = (
            "group sub;"  + os.linesep +
            "f() ::= \"bar\"" + os.linesep
            )
        subgroup = stringtemplate.StringTemplateGroup(
            StringIO(templates2),
            stringtemplate.language.AngleBracketTemplateLexer.Lexer,
            None,
            group
            )

        b = subgroup.getInstanceOf("body")
        expecting ="bar"
        result = str(b)
        self.assertEqual(result, expecting)
        

class NonPublicProperty:

    def __init__(self):
        self.foo = 9

    def getBar(self):
        return 34


class TestNonPublicPropertyAccess(unittest.TestCase):

    # Actually this test makes no sense in Python as there is no
    # distinction between public, protected, and private as in Java.
    def runTest(self):
        st = stringtemplate.StringTemplate("$x.foo$:$x.bar$")
        o = NonPublicProperty()
        st["x"] = o
        expecting = "9:34"
        self.assertEqual(str(st), expecting)


class TestIndexVar(unittest.TestCase):
    
    def testIndexVar(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        t = stringtemplate.StringTemplate(
            group,
            "$A:{$i$. $it$}; separator=\"\\n\"$"
            )
        t.setAttribute("A", "parrt")
        t.setAttribute("A", "tombu")
        expecting = (
            "1. parrt" + os.linesep +
            "2. tombu"
            )
        self.assertEqual(str(t), expecting)


    def testIndex0Var(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        t = stringtemplate.StringTemplate(
            group,
            "$A:{$i0$. $it$}; separator=\"\\n\"$"
            )
        t.setAttribute("A", "parrt")
        t.setAttribute("A", "tombu")
        expecting = (
            "0. parrt" + os.linesep +
            "1. tombu"
            )
        self.assertEqual(str(t), expecting)


    def testIndexVarWithMultipleExprs(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        t = stringtemplate.StringTemplate(
            group,
            "$A,B:{a,b|$i$. $a$@$b$}; separator=\"\\n\"$"
            )
        t.setAttribute("A", "parrt")
        t.setAttribute("A", "tombu")
        t.setAttribute("B", "x5707")
        t.setAttribute("B", "x5000")
        expecting = (
            "1. parrt@x5707" + os.linesep +
            "2. tombu@x5000"
            )
        self.assertEqual(str(t), expecting)


    def testIndex0VarWithMultipleExprs(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        t = stringtemplate.StringTemplate(
            group,
            "$A,B:{a,b|$i0$. $a$@$b$}; separator=\"\\n\"$"
            )
        t.setAttribute("A", "parrt")
        t.setAttribute("A", "tombu")
        t.setAttribute("B", "x5707")
        t.setAttribute("B", "x5000")
        expecting = (
            "0. parrt@x5707" + os.linesep +
            "1. tombu@x5000"
            )
        self.assertEqual(str(t), expecting)

class TestArgumentContext(unittest.TestCase):
    
    def testArgumentContext(self):
        # t is referenced within foo and so will be evaluated in that
        # context.  it can therefore see name.
        group = stringtemplate.StringTemplateGroup("test")
        main = group.defineTemplate("main", "$foo(t={Hi, $name$}, name=\"parrt\")$")
        foo = group.defineTemplate("foo", "$t$")
        expecting="Hi, parrt"
        self.assertEqual(str(main), expecting);


class TestDotsInNames(unittest.TestCase):
    def testNoDotsInAttributeNames(self):
        group = stringtemplate.StringTemplateGroup("dummy", ".")
        t = stringtemplate.StringTemplate(group, "$user.Name$")
        try:
            t.setAttribute("user.Name", "Kunle")
            self.fail()

        except ValueError, exc:
            error = str(exc)
            expecting = "cannot have '.' in attribute names"
            self.assertEqual(error, expecting)


    def testNoDotsInTemplateNames(self):
        errors = ErrorBuffer()
        templates = (
            "group test;" +os.linesep+
            "a.b() ::= <<foo>>"+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(
            StringIO(templates),
            stringtemplate.language.DefaultTemplateLexer.Lexer,
            errors
            )
        expecting = "template group parse error: <AST>:2:1:"
        self.assertTrue(str(errors).startswith(expecting), repr(str(errors)))


class TestEscape(unittest.TestCase):
    def testEscapeEscape(self):
        group = stringtemplate.StringTemplateGroup("test")
        t = group.defineTemplate("t", "\\\\$v$")
        t.setAttribute("v", "Joe")
        expecting="\\Joe"
        self.assertEqual(t.toString(), expecting)


    def testEscapeEscapeNestedAngle(self):
        group = stringtemplate.StringTemplateGroup(
            "test",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        t = group.defineTemplate("t", "<v:{a|\\\\<a>}>")
        t.setAttribute("v", "Joe")
        expecting="\\Joe"
        self.assertEqual(t.toString(), expecting)


class TestListOfArrays(unittest.TestCase):
    def testListOfIntArrays(self):
        group = stringtemplate.StringTemplateGroup(
            "test",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        t = group.defineTemplate("t", "<data:array()>")
        group.defineTemplate("array", "[<it:element(); separator=\",\">]")
        group.defineTemplate("element", "<it>")
        data = [ [1,2,3], [10,20,30] ]
        t.setAttribute("data", data)
        expecting="[1,2,3][10,20,30]"
        self.assertEqual(t.toString(), expecting)


class TestListLiteralWithEmptyElements(unittest.TestCase):
    # Added feature for ST-21
    @broken
    def testListLiteralWithEmptyElements(self):
        e = stringtemplate.StringTemplate(
            "$[\"Ter\",,\"Jesse\"]:{n | $i$:$n$}; separator=\", \", null=\"\"$"
            )
        e = e.getInstanceOf()
        e.setAttribute("names", "Ter")
        e.setAttribute("phones", "1")
        e.setAttribute("salaries", "big")
        expecting = "1:Ter, 2:, 3:Jesse"
        self.assertEqual(e.toString(), expecting)


    # Use when super.attr name is implemented
##     public void testArgumentContext2() throws Exception {
##         // t is referenced within foo and so will be evaluated in that
##         // context.  it can therefore see name.
##         StringTemplateGroup group =
##         new StringTemplateGroup("test");
##         StringTemplate main = group.defineTemplate("main", "$foo(t={Hi, $super.name$}, name=\"parrt\")$");
##         main.setAttribute("name", "tombu");
##         StringTemplate foo = group.defineTemplate("foo", "$t$");
##         String expecting="Hi, parrt";
##         assertEqual(main.toString(), expecting);
##         }


class TestLineWrap(unittest.TestCase):
    def testLineWrap(self):
        templates = (
            "group test;" +os.linesep+
            "array(values) ::= <<int[] a = { <values; wrap=\"\\n\", separator=\",\"> };>>"+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(StringIO(templates))
        
        a = group.getInstanceOf("array")
        a.setAttribute("values",
                       [3,9,20,2,1,4,6,32,5,6,77,888,2,1,6,32,5,6,77,
                        4,9,20,2,1,4,63,9,20,2,1,4,6,32,5,6,77,6,32,5,6,77,
                        3,9,20,2,1,4,6,32,5,6,77,888,1,6,32,5]
                       )
        expecting = (
            "int[] a = { 3,9,20,2,1,4,6,32,5,6,77,888,\n" +
            "2,1,6,32,5,6,77,4,9,20,2,1,4,63,9,20,2,1,\n" +
            "4,6,32,5,6,77,6,32,5,6,77,3,9,20,2,1,4,6,\n" +
            "32,5,6,77,888,1,6,32,5 };"
            )
        self.assertEqual(a.toString(40), expecting)


    def testLineWrapAnchored(self):
        templates = (
            "group test;" +os.linesep+
            "array(values) ::= <<int[] a = { <values; anchor, wrap=\"\\n\", separator=\",\"> };>>"+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(StringIO(templates))
        
        a = group.getInstanceOf("array")
        a.setAttribute("values",
                       [3,9,20,2,1,4,6,32,5,6,77,888,2,1,6,32,5,6,77,
                        4,9,20,2,1,4,63,9,20,2,1,4,6,32,5,6,77,6,32,5,6,77,
                        3,9,20,2,1,4,6,32,5,6,77,888,1,6,32,5]
                       )
        expecting = (
            "int[] a = { 3,9,20,2,1,4,6,32,5,6,77,888,\n" +
            "            2,1,6,32,5,6,77,4,9,20,2,1,4,\n" +
            "            63,9,20,2,1,4,6,32,5,6,77,6,\n" +
            "            32,5,6,77,3,9,20,2,1,4,6,32,\n" +
            "            5,6,77,888,1,6,32,5 };"
            )
        self.assertEqual(a.toString(40), expecting)


    def testFortranLineWrap(self):
        templates = (
            "group test;" +os.linesep+
            "func(args) ::= <<       FUNCTION line( <args; wrap=\"\\n      c\", separator=\",\"> )>>"+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(StringIO(templates))

        a = group.getInstanceOf("func")
        a.setAttribute("args", ["a","b","c","d","e","f"])
        expecting = (
            "       FUNCTION line( a,b,c,d,\n" +
            "      ce,f )"
            )
        self.assertEqual(a.toString(30), expecting)


    def testLineWrapWithDiffAnchor(self):
        templates = (
            "group test;" +os.linesep+
            "array(values) ::= <<int[] a = { <{1,9,2,<values; wrap, separator=\",\">}; anchor> };>>"+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(StringIO(templates))

        a = group.getInstanceOf("array")
        a.setAttribute("values",
                       [3,9,20,2,1,4,6,32,5,6,77,888,2,1,6,32,5,6,77,
                        4,9,20,2,1,4,63,9,20,2,1,4,6]
                       )
        expecting = (
            "int[] a = { 1,9,2,3,9,20,2,1,4,\n" +
            "            6,32,5,6,77,888,2,\n" +
            "            1,6,32,5,6,77,4,9,\n" +
            "            20,2,1,4,63,9,20,2,\n" +
            "            1,4,6 };"
            )
        self.assertEqual(a.toString(30), expecting)


    def testLineWrapEdgeCase(self):
        templates = (
            "group test;" +os.linesep+
            "duh(chars) ::= <<<chars; wrap=\"\\n\"\\>>>"+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(StringIO(templates))

        a = group.getInstanceOf("duh")
        a.setAttribute("chars", ["a","b","c","d","e"])
        # lineWidth==3 implies that we can have 3 characters at most
        expecting = (
            "abc\n"+
            "de"
            )
        self.assertEqual(a.toString(3), expecting)


    def testLineWrapLastCharIsNewline(self):
        templates = (
            "group test;" +os.linesep+
            "duh(chars) ::= <<<chars; wrap=\"\\n\"\\>>>"+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(StringIO(templates))

        a = group.getInstanceOf("duh")
        a.setAttribute("chars", ["a","b","\n","d","e"])
        # don't do \n if it's last element anyway
        expecting = (
            "ab\n"+
            "de"
            )
        self.assertEqual(a.toString(3), expecting)


    def testLineWrapCharAfterWrapIsNewline(self):
        templates = (
            "group test;" +os.linesep+
            "duh(chars) ::= <<<chars; wrap=\"\\n\"\\>>>"+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(StringIO(templates))
        
        a = group.getInstanceOf("duh")
        a.setAttribute("chars", ["a","b","c","\n","d","e"])
        # Once we wrap, we must dump chars as we see them.  A os.linesep right
        # after a wrap is just an "unfortunate" event.  People will expect
        # a os.linesep if it's in the data.
        expecting = (
            "abc\n" +
            "\n" +
            "de"
            )
        self.assertEqual(a.toString(3), expecting)


    def testIndentBeyondLineWidth(self):
        templates = (
            "group test;" +os.linesep+
            "duh(chars) ::= <<    <chars; wrap=\"\\n\"\\>>>"+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(StringIO(templates))

        a = group.getInstanceOf("duh")
        a.setAttribute("chars", ["a","b","c","d","e"])

        expecting = (
            "    a\n" +
            "    b\n" +
            "    c\n" +
            "    d\n" +
            "    e"
            )
        self.assertEqual(a.toString(2), expecting)


    def testIndentedExpr(self):
        templates = (
            "group test;" +os.linesep+
            "duh(chars) ::= <<    <chars; wrap=\"\\n\"\\>>>"+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(StringIO(templates))

        a = group.getInstanceOf("duh")
        a.setAttribute("chars", ["a","b","c","d","e"])

        expecting = (
            "    ab\n" +
            "    cd\n" +
            "    e"
            )
        # width=4 spaces + 2 char.
        self.assertEqual(a.toString(6), expecting)


    def testNestedIndentedExpr(self):
        templates = (
            "group test;" +os.linesep+
            "top(d) ::= <<  <d>!>>"+os.linesep+
            "duh(chars) ::= <<  <chars; wrap=\"\\n\"\\>>>"+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(StringIO(templates))

        top = group.getInstanceOf("top")
        duh = group.getInstanceOf("duh")
        duh.setAttribute("chars", ["a","b","c","d","e"])
        top.setAttribute("d", duh)

        expecting = (
            "    ab\n" +
            "    cd\n" +
            "    e!"
            )
        # width=4 spaces + 2 char.
        self.assertEqual(top.toString(6), expecting)


    def testNestedWithIndentAndTrackStartOfExpr(self):
        templates = (
            "group test;" +os.linesep+
            "top(d) ::= <<  <d>!>>"+os.linesep+
            "duh(chars) ::= <<x: <chars; anchor, wrap=\"\\n\"\\>>>"+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(StringIO(templates))

        top = group.getInstanceOf("top")
        duh = group.getInstanceOf("duh")
        duh.setAttribute("chars", ["a","b","c","d","e"])
        top.setAttribute("d", duh)

        expecting = (
            "  x: ab\n" +
            "     cd\n" +
            "     e!"
            )
        self.assertEqual(top.toString(7), expecting)


    def testLineDoesNotWrapDueToLiteral(self):
        templates = (
            "group test;" +os.linesep+
            "m(args,body) ::= <<public void foo(<args; wrap=\"\\n\",separator=\", \">) throws Ick { <body> }>>"+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(StringIO(templates))

        a = group.getInstanceOf("m")
        a.setAttribute("args", ["a", "b", "c"])
        a.setAttribute("body", "i=3;")
        # make it wrap because of ") throws Ick { " literal
        n = len("public void foo(a, b, c")
        expecting = (
            "public void foo(a, b, c) throws Ick { i=3; }"
            )
        self.assertEqual(a.toString(n), expecting)


    def testSingleValueWrap(self):
        templates = (
            "group test;" +os.linesep+
            "m(args,body) ::= <<{ <body; anchor, wrap=\"\\n\"> }>>"+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(StringIO(templates))

        m = group.getInstanceOf("m")
        m.setAttribute("body", "i=3;")
        # make it wrap because of ") throws Ick { " literal
        expecting = (
            "{ \n"+
            "  i=3; }"
            )
        self.assertEqual(m.toString(2), expecting)


    def testLineWrapInNestedExpr(self):
        templates = (
            "group test;" +os.linesep+
            "top(arrays) ::= <<Arrays: <arrays>done>>"+os.linesep+
            "array(values) ::= <<int[] a = { <values; anchor, wrap=\"\\n\", separator=\",\"> };<\\n\\>>>"+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(StringIO(templates))

        top = group.getInstanceOf("top")
        a = group.getInstanceOf("array")
        a.setAttribute("values",
                       [ 3,9,20,2,1,4,6,32,5,6,77,888,2,1,6,32,5,6,77,
                         4,9,20,2,1,4,63,9,20,2,1,4,6,32,5,6,77,6,32,5,6,77,
                         3,9,20,2,1,4,6,32,5,6,77,888,1,6,32,5 ]
                       )
        top.setAttribute("arrays", a)
        top.setAttribute("arrays", a) # add twice
        expecting = (
            "Arrays: int[] a = { 3,9,20,2,1,4,6,32,5,\n" +
            "                    6,77,888,2,1,6,32,5,\n" +
            "                    6,77,4,9,20,2,1,4,63,\n" +
            "                    9,20,2,1,4,6,32,5,6,\n" +
            "                    77,6,32,5,6,77,3,9,20,\n" +
            "                    2,1,4,6,32,5,6,77,888,\n" +
            "                    1,6,32,5 };\n" +
            "int[] a = { 3,9,20,2,1,4,6,32,5,6,77,888,\n" +
            "            2,1,6,32,5,6,77,4,9,20,2,1,4,\n" +
            "            63,9,20,2,1,4,6,32,5,6,77,6,\n" +
            "            32,5,6,77,3,9,20,2,1,4,6,32,\n" +
            "            5,6,77,888,1,6,32,5 };\n" +
            "done"
            )
        self.assertEqual(top.toString(40), expecting)


    def testLineWrapForAnonTemplate(self):
        templates = (
            "group test;" +os.linesep+
            "duh(data) ::= <<!<data:{v|[<v>]}; wrap>!>>"+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(StringIO(templates))

        a = group.getInstanceOf("duh")
        a.setAttribute("data", [1,2,3,4,5,6,7,8,9])
        expecting = (
            "![1][2][3]\n" + # width=9 is the 3 char; don't break til after ]
            "[4][5][6]\n" +
            "[7][8][9]!"
            )
        self.assertEqual(a.toString(9), expecting)


    def testLineWrapForAnonTemplateAnchored(self):
        templates = (
            "group test;" +os.linesep+
            "duh(data) ::= <<!<data:{v|[<v>]}; anchor, wrap>!>>"+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(StringIO(templates))

        a = group.getInstanceOf("duh")
        a.setAttribute("data", [1,2,3,4,5,6,7,8,9])
        expecting = (
            "![1][2][3]\n" +
            " [4][5][6]\n" +
            " [7][8][9]!"
            )
        self.assertEqual(a.toString(9), expecting)


    def testLineWrapForAnonTemplateComplicatedWrap(self):
        templates = (
            "group test;" +os.linesep+
            "top(s) ::= <<  <s>.>>"+
            "str(data) ::= <<!<data:{v|[<v>]}; wrap=\"!+\\n!\">!>>"+os.linesep
            )
        group = stringtemplate.StringTemplateGroup(StringIO(templates))
        
        t = group.getInstanceOf("top")
        s = group.getInstanceOf("str")
        s.setAttribute("data", [1,2,3,4,5,6,7,8,9])
        t.setAttribute("s", s)
        expecting = (
            "  ![1][2]!+\n" +
            "  ![3][4]!+\n" +
            "  ![5][6]!+\n" +
            "  ![7][8]!+\n" +
            "  ![9]!."
            )
        self.assertEqual(t.toString(9), expecting)


class TestNullOption(unittest.TestCase):

    def testNullOptionSingleNullValue(self):
        group = stringtemplate.StringTemplateGroup(
            "test",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        t =group.defineTemplate("t", "<data; null=\"0\">")
        expecting="0"
        self.assertEqual(t.toString(), expecting)


    def testNullOptionHasEmptyNullValue(self):
        group = stringtemplate.StringTemplateGroup(
            "test",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        t = group.defineTemplate("t", "<data; null=\"\", separator=\", \">")
        data = [ None, 1 ]
        t.setAttribute("data", data)
        expecting=", 1"
        self.assertEqual(t.toString(), expecting)


    def testNullOptionSingleNullValueInList(self):
        group = stringtemplate.StringTemplateGroup(
            "test",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        t = group.defineTemplate("t", "<data; null=\"0\">")
        data = [ None ]
        t.setAttribute("data", data)
        expecting="0"
        self.assertEqual(t.toString(), expecting)


    def testNullValueInList(self):
        group = stringtemplate.StringTemplateGroup(
            "test",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        t = group.defineTemplate("t", "<data; null=\"-1\", separator=\", \">")
        data = [ None, 1, None, 3, 4, None ]
        t.setAttribute("data", data)
        expecting="-1, 1, -1, 3, 4, -1"
        self.assertEqual(t.toString(), expecting)


    def testNullValueInListNoNullOption(self):
        group = stringtemplate.StringTemplateGroup(
            "test",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        t = group.defineTemplate("t", "<data; separator=\", \">")
        data = [ None, 1, None, 3, 4, None ]
        t.setAttribute("data", data);
        expecting="1, 3, 4"
        self.assertEqual(t.toString(), expecting)


    def testNullValueInListWithTemplateApply(self):
        group = stringtemplate.StringTemplateGroup(
            "test",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        t = group.defineTemplate("t", "<data:array(); null=\"-1\", separator=\", \">")
        group.defineTemplate("array", "<it>")
        data = [ 0, None, 2, None ]
        t.setAttribute("data", data)
        expecting="0, -1, 2, -1"
        self.assertEqual(t.toString(), expecting)


    def testNullValueInListWithTemplateApplyNullFirstValue(self):
        group = stringtemplate.StringTemplateGroup(
            "test",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        t = group.defineTemplate("t", "<data:array(); null=\"-1\", separator=\", \">")
        group.defineTemplate("array", "<it>")
        data = [ None, 0, None, 2 ]
        t.setAttribute("data", data)
        expecting="-1, 0, -1, 2"
        self.assertEqual(t.toString(), expecting)


    def testNullSingleValueInListWithTemplateApply(self):
        group = stringtemplate.StringTemplateGroup(
            "test",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        t = group.defineTemplate("t", "<data:array(); null=\"-1\", separator=\", \">")
        group.defineTemplate("array", "<it>")
        data = [ None ]
        t.setAttribute("data", data)
        expecting="-1"
        self.assertEqual(t.toString(), expecting)


    def testNullSingleValueWithTemplateApply(self):
        group = stringtemplate.StringTemplateGroup(
            "test",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        t = group.defineTemplate("t", "<data:array(); null=\"-1\", separator=\", \">")
        group.defineTemplate("array", "<it>")
        expecting="-1"
        self.assertEqual(t.toString(), expecting)


class TestLengthOp(unittest.TestCase):
    def testLengthOp(self):
        e = stringtemplate.StringTemplate(
            "$length(names)$"
            )
        e = e.getInstanceOf()
        e.setAttribute("names", "Ter")
        e.setAttribute("names", "Tom")
        e.setAttribute("names", "Sriram")
        expecting = "3"
        self.assertEqual(e.toString(), expecting)


    def testLengthOpNull(self):
        e = stringtemplate.StringTemplate(
            "$length(names)$"
            )
        e = e.getInstanceOf()
        e.setAttribute("names", None)
        expecting = "0"
        self.assertEqual(e.toString(), expecting)


    def testLengthOpSingleValue(self):
        e = stringtemplate.StringTemplate(
            "$length(names)$"
            )
        e = e.getInstanceOf()
        e.setAttribute("names", "Ter")
        expecting = "1"
        self.assertEqual(e.toString(), expecting)


    def testLengthOpPrimitive(self):
        e = stringtemplate.StringTemplate(
            "$length(ints)$"
            )
        e = e.getInstanceOf()
        e.setAttribute("ints", [1,2,3,4] )
        expecting = "4"
        self.assertEqual(e.toString(), expecting)


    def testLengthOpOfListWithNulls(self):
        e = stringtemplate.StringTemplate(
            "$length(data)$"
            )
        e = e.getInstanceOf()
        data = [ "Hi", None, "mom", None ]
        e.setAttribute("data", data);
        expecting = "4" # nulls are counted
        self.assertEqual(e.toString(), expecting)


    def testLengthOpWithMap(self):
        e = stringtemplate.StringTemplate(
            "$length(names)$"
            )
        e = e.getInstanceOf()
        m = { "Tom": "foo",
              "Sriram": "foo",
              "Doug": "foo"
              }
        e.setAttribute("names", m)
        expecting = "3"
        self.assertEqual(e.toString(), expecting)


    def testLengthOpWithSet(self):
        e = stringtemplate.StringTemplate(
            "$length(names)$"
            )
        e = e.getInstanceOf()
        m = set(["Tom", "Sriram", "Doug"])
        e.setAttribute("names", m)
        expecting = "3";
        self.assertEquals(e.toString(), expecting)


class TestStripOp(unittest.TestCase):
    def testStripOpOfListWithNulls(self):
        e = stringtemplate.StringTemplate(
            "$strip(data)$"
            )
        e = e.getInstanceOf()
        data = [ "Hi", None, "mom", None ]
        e.setAttribute("data", data)
        expecting = "Himom" # nulls are skipped
        self.assertEqual(e.toString(), expecting)


    def testStripOpOfListOfListsWithNulls(self):
        e = stringtemplate.StringTemplate(
            "$strip(data):{list | $strip(list)$}; separator=\",\"$"
            )
        e = e.getInstanceOf()
        data = [ [ "Hi", "mom" ],
                 None,
                 [ "Hi", None, "dad", None ]
                 ]
        e.setAttribute("data", data)
        expecting = "Himom,Hidad" # nulls are skipped
        self.assertEqual(e.toString(), expecting)


    def testStripOpOfSingleAlt(self):
        e = stringtemplate.StringTemplate(
            "$strip(data)$"
            )
        e = e.getInstanceOf()
        e.setAttribute("data", "hi")
        expecting = "hi" # nulls are skipped
        self.assertEqual(e.toString(), expecting);


    def testStripOpOfNull(self):
        e = stringtemplate.StringTemplate(
            "$strip(data)$"
            )
        e = e.getInstanceOf()
        expecting = "" # nulls are skipped
        self.assertEqual(e.toString(), expecting);


    def testLengthOpOfStrippedListWithNulls(self):
        e = stringtemplate.StringTemplate(
            "$length(strip(data))$"
            )
        e = e.getInstanceOf()
        data = [ "Hi", None, "mom", None ]
        e.setAttribute("data", data)
        expecting = "2" # nulls are counted
        self.assertEqual(e.toString(), expecting)


    def testLengthOpOfStrippedListWithNullsFrontAndBack(self):
        e = stringtemplate.StringTemplate(
            "$length(strip(data))$"
            )
        e = e.getInstanceOf()
        data = [ None, None, None,
                 "Hi",
                 None, None, None,
                 "mom",
                 None, None, None
                 ]
        e.setAttribute("data", data)
        expecting = "2" # nulls are counted
        self.assertEqual(e.toString(), expecting);


class TestMapProperties(unittest.TestCase):
    def testMapKeys(self):
        group = stringtemplate.StringTemplateGroup(
            "dummy", ".",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        t = stringtemplate.StringTemplate(
            group,
            "<aMap.keys:{k|<k>:<aMap.(k)>}; separator=\", \">"
            )
        map = {}
        map["int"] = "0"
        map["float"] = "0.0"
        t.setAttribute("aMap", map)
        self.assertEqual(t.toString(), "int:0, float:0.0")


    def testMapValues(self):
        group = stringtemplate.StringTemplateGroup(
            "dummy", ".",
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        t = stringtemplate.StringTemplate(
            group,
            "<aMap.values; separator=\", \">"
            )
        map = {}
        map["int"] = "0"
        map["float"] = "0.0"
        t.setAttribute("aMap", map)
        self.assertEqual(t.toString(), "0, 0.0")


    @broken
    def testGroupTrailingSemiColon(self):
        """
        Check what happens when a semicolon is  appended to a single line
        template
        
        Should fail with a parse error(?) and not a missing template error.
        FIXME: This should generate a warning or error about that semi colon.

        Bug ref: JIRA bug ST-2
        """
        
        templates = (
            "group test;" +os.linesep+
            "t1()::=\"R1\"; "+os.linesep+
            "t2() ::= \"R2\""+os.linesep                
            )

        group = stringtemplate.StringTemplateGroup(StringIO(templates))
            
        st = group.getInstanceOf("t1")
        self.assertEqual(st.toString(), "R1")
            
        st = group.getInstanceOf("t2")
        self.assertEqual(st.toString(), "R2")
            
        self.fail("A parse error should have been generated")


class TestSuperReferenceInIfClause(unittest.TestCase):
    def runTest(self):
        superGroupString = (
            "group super;" + os.linesep +
            "a(x) ::= \"super.a\"" + os.linesep +
            "b(x) ::= \"<c()>super.b\"" + os.linesep +
            "c() ::= \"super.c\""
            )
        superGroup = stringtemplate.StringTemplateGroup(
            StringIO(superGroupString),
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        subGroupString = (
            "group sub;\n" +
            "a(x) ::= \"<if(x)><super.a()><endif>\"" + os.linesep +
            "b(x) ::= \"<if(x)><else><super.b()><endif>\"" + os.linesep +
            "c() ::= \"sub.c\""
            )
        subGroup = stringtemplate.StringTemplateGroup(
            StringIO(subGroupString),
            stringtemplate.language.AngleBracketTemplateLexer.Lexer
            )
        subGroup.setSuperGroup(superGroup)
        a = subGroup.getInstanceOf("a")
        a.setAttribute("x", "foo")
        self.assertEqual(a.toString(), "super.a")
        b = subGroup.getInstanceOf("b")
        self.assertEqual(b.toString(), "sub.csuper.b")
        c = subGroup.getInstanceOf("c")
        self.assertEquals(c.toString(), "sub.c")


## if __name__ == '__main__':

##     from optparse import OptionParser

##     parser = OptionParser()

##     parser.set_defaults(guiRunner=False)
##     parser.add_option("-g", "--gui",
##     		      action="store_true", dest="guiRunner",
##     		      help="use TkInter based GUI for test")

##     (options, args) = parser.parse_args()

##     suite = unittest.TestSuite()
##     suite.addTest(TestGroupFileFormat())
##     suite.addTest(TestTemplateParameterDecls())
##     suite.addTest(TestTemplateRedef())
##     suite.addTest(TestMissingInheritedAttribute())
##     suite.addTest(TestFormalArgumentAssignment())
##     suite.addTest(TestUndefinedArgumentAssignment())
##     suite.addTest(TestFormalArgumentAssignmentInApply())
##     suite.addTest(TestUndefinedArgumentAssignmentInApply())
##     suite.addTest(TestUndefinedAttributeReference())
##     suite.addTest(TestUndefinedDefaultAttributeReference())
##     suite.addTest(TestAngleBracketsWithGroupFileAsString())
##     suite.addTest(TestAngleBracketsWithGroupFile())
##     suite.addTest(TestAngleBracketsNoGroup())
##     suite.addTest(TestSimpleInheritance())
##     suite.addTest(TestOverrideInheritance())
##     suite.addTest(TestMultiLevelInheritance())
##     suite.addTest(TestExprInParens())
##     suite.addTest(TestMultipleAdditions())
##     suite.addTest(TestCollectionAttributes())
##     suite.addTest(TestParenthesizedExpression())
##     suite.addTest(TestApplyTemplateNameExpression())
##     suite.addTest(TestTemplateNameExpression())
##     suite.addTest(TestMissingEndDelimiter())
##     suite.addTest(TestSetButNotRefd())
##     suite.addTest(TestNullTemplateApplication())
##     suite.addTest(TestNullTemplateToMultiValuedApplication())
##     suite.addTest(TestChangingAttrValueTemplateApplicationToVector())
##     suite.addTest(TestChangingAttrValueRepeatedTemplateApplicationToVector())
##     suite.addTest(TestAlternatingTemplateApplication())
##     suite.addTest(TestExpressionAsRHSOfAssignment())
##     suite.addTest(TestTemplateApplicationAsRHSOfAssignment())
##     suite.addTest(TestParameterAndAttributeScoping())
##     suite.addTest(TestComplicatedSeparatorExpr())
##     suite.addTest(TestAttributeRefButtedUpAgainstEndifAndWhitespace())
##     suite.addTest(TestStringCatenationOnSingleValuedAttributeViaTemplateLiteral())
##     suite.addTest(TestApplyingTemplateFromDiskWithPrecompiledIF())
##     suite.addTest(TestMultiValuedAttributeWithAnonymousTemplateUsingIndexVariableI())
##     suite.addTest(TestDictAttributeWithAnonymousTemplateUsingKeyVariableIk())
##     suite.addTest(TestFindTemplateInCurrentDir())
##     suite.addTest(TestApplyTemplateToSingleValuedAttribute())
##     suite.addTest(TestStringLiteralAsAttribute())
##     suite.addTest(TestApplyTemplateToSingleValuedAttributeWithDefaultAttribute())
##     suite.addTest(TestApplyAnonymousTemplateToSingleValuedAttribute())
##     suite.addTest(TestApplyAnonymousTemplateToMultiValuedAttribute())
##     suite.addTest(TestApplyAnonymousTemplateToAggregateAttribute())
##     suite.addTest(TestRepeatedApplicationOfTemplateToSingleValuedAttribute())
##     suite.addTest(TestRepeatedApplicationOfTemplateToMultiValuedAttributeWithSeparator())
##     suite.addTest(TestMultiValuedAttributeWithSeparator())
##     suite.addTest(TestSingleValuedAttributes())
##     suite.addTest(TestIFTemplate())
##     suite.addTest(TestIFCondWithParensTemplate())
##     suite.addTest(TestIFCondWithParensDollarDelimsTemplate())
##     suite.addTest(TestIFBoolean())
##     suite.addTest(TestNestedIFTemplate())
##     suite.addTest(TestObjectPropertyReference())
##     suite.addTest(TestApplyRepeatedAnonymousTemplateWithForeignTemplateRefToMultiValuedAttribute())
##     suite.addTest(TestRecursion())
##     suite.addTest(TestNestedAnonymousTemplates())
##     suite.addTest(TestAnonymousTemplateAccessToEnclosingAttributes())
##     suite.addTest(TestNestedAnonymousTemplatesAgain())
##     suite.addTest(TestEscapes())
##     suite.addTest(TestEscapesOutsideExpressions())
##     suite.addTest(TestElseClause())
##     suite.addTest(TestNestedIF())
##     suite.addTest(TestEmbeddedMultiLineIF())
##     suite.addTest(TestSimpleIndentOfAttributeList())
##     suite.addTest(TestIndentOfMultilineAttributes())
##     suite.addTest(TestIndentOfMultipleBlankLines())
##     suite.addTest(TestIndentBetweenLeftJustifiedLiterals())
##     suite.addTest(TestNestedIndent())
##     suite.addTest(TestApplyAnonymousTemplateToDictTupleAndList())
##     suite.addTest(TestApplyAnonymousTemplateToDictTupleAndList2())
##     suite.addTest(TestDumpDictTupleAndList())
##     suite.addTest(TestApplyAnonymousTemplateToListAndDictProperty())
##     suite.addTest(TestSuperTemplateRef())
##     suite.addTest(TestApplySuperTemplateRef())
##     suite.addTest(TestLazyEvalOfSuperInApplySuperTemplateRef())
##     suite.addTest(TestTemplatePolymorphism())
##     suite.addTest(TestListOfEmbeddedTemplateSeesEnclosingAttributes())
##     suite.addTest(TestInheritArgumentFromRecursiveTemplateApplication())
##     suite.addTest(TestDeliberateRecursiveTemplateApplication())
##     suite.addTest(TestImmediateTemplateAsAttributeLoop())
##     suite.addTest(TestTemplateAlias())
##     suite.addTest(TestTemplateGetPropertyGetsAttribute())
##     suite.addTest(TestComplicatedIndirectTemplateApplication())
##     suite.addTest(TestIndirectTemplateApplication())
##     suite.addTest(TestIndirectTemplateWithArgsApplication())
##     suite.addTest(TestNullIndirectTemplateApplication())
##     suite.addTest(TestNullIndirectTemplate())
##     suite.addTest(TestReflection())
##     suite.addTest(TestReflectionRecursive())
##     suite.addTest(TestReflectionTypeLoop())
##     suite.addTest(TestReflectionWithDict())
##     suite.addTest(TestDictPropertyFetch())
##     suite.addTest(TestDictPropertyFetchEmbeddedStringTemplate())
##     suite.addTest(TestEmbeddedComments())
##     suite.addTest(TestEmbeddedCommentsAngleBracketed())
##     suite.addTest(TestCharLiterals())
##     suite.addTest(TestEmptyIteratedValueGetsSeparator())
##     suite.addTest(TestEmptyIteratedConditionalValueGetsNoSeparator())
##     suite.addTest(TestEmptyIteratedConditionalWithElseValueGetsSeparator())
##     suite.addTest(TestWhiteSpaceAtEndOfTemplate())
##     suite.addTest(TestSizeZeroButNonNullListGetsNoOutput())
##     suite.addTest(TestSizeZeroOnLineByItselfGetsNoOutput())
##     suite.addTest(TestSizeZeroOnLineWithIndentGetsNoOutput())
##     suite.addTest(TestSimpleAutoIndent())
##     suite.addTest(TestComputedPropertyName())
##     suite.addTest(TestMultipleAttributeRefThroughReflection())
##     suite.addTest(TestNonNullButEmptyIteratorTestsFalse())
##     suite.addTest(TestDoNotInheritAttributesThroughFormalArgs())
##     suite.addTest(TestArgEvaluationContext())
##     suite.addTest(TestPassThroughAttributes())
##     suite.addTest(TestPassThroughAttributes2())
##     suite.addTest(TestDefaultArgument())
##     suite.addTest(TestDefaultArgument2())
##     suite.addTest(TestDefaultArgumentAsTemplate())
##     suite.addTest(TestDefaultArgumentAsTemplate2())
##     suite.addTest(TestDoNotUseDefaultArgument())
##     suite.addTest(TestArgumentsAsTemplates())
##     suite.addTest(TestArgumentsAsTemplatesDefaultDelimiters())
##     suite.addTest(TestDefaultArgsWhenNotInvoked())
##     suite.addTest(TestRendererForST())
##     suite.addTest(TestRendererForGroup())
##     suite.addTest(TestOverriddenRenderer())
##     suite.addTest(TestMap())
##     suite.addTest(TestMapHiddenByFormalArg())
##     suite.addTest(TestMapDefaultValue())
##     suite.addTest(TestMapViaEnclosingTemplates())
##     suite.addTest(TestMapViaEnclosingTemplates2())
##     suite.addTest(TestEmptyGroupTemplate())
## #    suite.addTest(Test8BitEuroChars())
##     suite.addTest(TestFirstOp())
##     suite.addTest(TestRestOp())
##     suite.addTest(TestLastOp())
##     suite.addTest(TestCombinedOp())
##     suite.addTest(TestCatListAndSingleAttribute())
##     suite.addTest(TestCatListAndEmptyAttributes())
##     suite.addTest(TestNestedOp())
##     suite.addTest(TestFirstWithOneAttributeOp())
##     suite.addTest(TestLastWithOneAttributeOp())
##     suite.addTest(TestLastWithLengthOneListAttributeOp())
##     suite.addTest(TestRestWithOneAttributeOp())
##     suite.addTest(TestRestWithLengthOneListAttributeOp())
##     suite.addTest(TestApplyTemplateWithSingleFormalArgs())
##     suite.addTest(TestApplyTemplateWithNoFormalArgs())
##     suite.addTest(TestAnonTemplateArgs())
##     suite.addTest(TestAnonTemplateArgs2())
##     suite.addTest(TestFirstWithCatAttribute())
##     suite.addTest(TestJustCat())
##     suite.addTest(TestCat2Attributes())
##     suite.addTest(TestCat2AttributesWithApply())
##     suite.addTest(TestCat3Attributes())
##     suite.addTest(TestListAsTemplateArgument())
##     suite.addTest(TestSingleExprTemplateArgument())
##     suite.addTest(TestSingleExprTemplateArgumentInApply())
##     suite.addTest(TestSoleFormalTemplateArgumentInMultiApply())
##     suite.addTest(TestSingleExprTemplateArgumentError())
##     suite.addTest(TestInvokeIndirectTemplateWithSingleFormalArgs())
##     suite.addTest(TestParallelAttributeIteration())
##     suite.addTest(TestParallelAttributeIterationWithDifferentSizes())
##     suite.addTest(TestParallelAttributeIterationWithSingletons())
##     suite.addTest(TestParallelAttributeIterationWithMismatchArgListSizes())
##     suite.addTest(TestParallelAttributeIterationWithMissingArgs())
##     suite.addTest(TestParallelAttributeIterationWithDifferentSizesTemplateRefInsideToo())
##     suite.addTest(TestAnonTemplateOnLeftOfApply())
##     suite.addTest(TestNonPublicPropertyAccess())

##     doGUI = options.guiRunner
##     if doGUI:
## 	try:
##             import unittestgui
## 	    tk = unittestgui.tk
## 	except:
##             doGUI = False

##     if doGUI:

##         class STTkTestRunner(unittestgui.TkTestRunner):

## 	    def __init__(self, *args, **kwargs):
## 	    	self.root = tk.Tk()
## 		self.root.title("PyUnit")
## 		self.test = None
##         	self.currentResult = None
##         	self.running = 0
##         	self.__rollbackImporter = None
## 	        super(STTkTestRunner, self).__init__(self.root, 'TestStringTemplate')

## 	    def runClicked(self):
##         	"To be called in response to user choosing to run a test"
##         	if self.running:
## 		    return
## 		if not self.test:
## 		    self.errorDialog("Test entry", "You must provide a TestCase or TestSuite to run")
##         	    return
##         	if self.__rollbackImporter:
##         	    self.__rollbackImporter.rollbackImports()
##         	self.__rollbackImporter = unittestgui.RollbackImporter()
##         	self.currentResult = unittestgui.GUITestResult(self)
##         	self.totalTests = self.test.countTestCases()
##         	self.running = 1
##         	self.notifyRunning()
##         	self.test(self.currentResult)
##         	self.running = 0
##         	self.notifyStopped()

##             def run(self, test):
## 	        self.test = test
## 		self.root.protocol('WM_DELETE_WINDOW', self.root.quit)
## 		self.root.mainloop()

## 	runner = STTkTestRunner(suite)
## 	result = runner.run(suite)

##     else: # doGUI = False

##         runner = unittest.TextTestRunner(sys.stdout)
## 	result = runner.run(suite)
## 	sys.exit(not result.wasSuccessful())



if __name__ == '__main__':
    unittest.main()
