
import sys
import os
import traceback
import imp
import time
from StringIO import StringIO
from copy import copy, deepcopy

import antlr
#from synchronize import *

from stringtemplate.language import FormalArgument, ChunkToken, \
                                    ASTExpr, StringTemplateAST, \
                                    DefaultTemplateLexer, TemplateParser, \
                                    GroupLexer, GroupParser, \
                                    ActionLexer, ActionParser, \
                                    AttributeReflectionController, \
                                    ConditionalExpr, NewlineRef

## Generic StringTemplate output writer filter
#
class StringTemplateWriter(object):

    def __init__(self):
        pass

    def pushIndentation(self, indent):
        raise NotImplementedError

    def popIndentation(self):
        raise NotImplementedError

    def write(self, str):
        raise NotImplementedError

## Essentially a char filter that knows how to auto-indent output
#  by maintaining a stack of indent levels.  I set a flag upon newline
#  and then next nonwhitespace char resets flag and spits out indention.
#  The indent stack is a stack of strings so we can repeat original indent
#  not just the same number of columns (don't have to worry about tabs vs
#  spaces then).
# 
#  This is a filter on a Writer.
# 
#  It may be screwed up for '\r' '\n' on PC's.
# 
class AutoIndentWriter(StringTemplateWriter):

    BUFFER_SIZE = 50

    def __init__(self, out = None):
        self.indents = []
        self.atStartOfLine = True
        self.out = out
        # start with no indent
        self.indents.append(None)

    # Push even blank (null) indents as they are like scopes; must
    # be able to pop them back off stack.
    #
    def pushIndentation(self, indent):
        self.indents.append(indent)

    def popIndentation(self):
        return self.indents.pop()

    def write(self, str_):
        # sys.stderr.write('write(' + str(str_) + ')' + \
        #                  '; indents=' + str(self.indents) + '\n')
        n = 0
        for c in str(str_):
            if c == '\n':
                self.atStartOfLine = True
            elif self.atStartOfLine:
                n += self.indent()
                self.atStartOfLine = False
            n += 1
            self.out.write(c)
        return n

    def indent(self):
        n = 0
        for ind in self.indents:
            if ind:
		self.out.write(ind)
                n += len(ind)
        return n

AutoIndentWriter.BUFFER_SIZE = 50

## Just pass through the text
#
class NoIndentWriter(AutoIndentWriter):

    def __init__(self, out):
        super(NoIndentWriter, self).__init__(out)

    def write(self, str):
        self.out.write(str)
        return len(str)

## Lets you specify where errors, warnings go. Warning: debug is useless at
#  the moment.
#
class StringTemplateErrorListener(object):

    def error(self, msg, e):
        raise NotImplementedError

    def warning(self, msg):
        raise NotImplementedError

    ## Deluge of debugging output.  Totally useless at this point.
    #
    def debug(self, msg):
        raise NotImplementedError

## An automatically created aggregate of properties.
#
#  I often have lists of things that need to be formatted, but the list
#  items are actually pieces of data that are not already in an object.  I
#  need ST to do something like:
#
#  Ter=3432
#  Tom=32234
#  ....
#
#  using template:
#
#  $items:{$attr.name$=$attr.type$$
#
#  This example will call getName() on the objects in items attribute, but
#  what if they aren't objects?  I have perhaps two parallel arrays
#  instead of a single array of objects containing two fields.  One
#  solution is allow dictionaries to be handled like properties so that
#  it.name would fail getName() but then see that it's a dictionary and
#  do it.get("name") instead.
#
#  This very clean approach is espoused by some, but the problem is that
#  it's a hole in my separation rules.  People can put the logic in the
#  view because you could say: "go get bob's data" in the view:
#
#  Bob's Phone: $db.bob.phone$
#
#  A view should not be part of the program and hence should never be able
#  to go ask for a specific person's data.
#
#  After much thought, I finally decided on a simple solution.  I've
#  added setAttribute variants that pass in multiple property values,
#  with the property names specified as part of the name using a special
#  attribute name syntax: "name.{propName1,propName2,...".  This
#  object is a special kind of dictionary that hopefully prevents people
#  from passing a subclass or other variant that they have created as
#  it would be a loophole.  Anyway, the ASTExpr.getObjectProperty()
#  method looks for Aggregate as a special case and does a get() instead
#  of getPropertyName.
#
class Aggregate(object):
    def __init__(self, master):
        self.properties = {}
        self.master = master

    ## Allow StringTemplate to add values, but prevent the end
    #  user from doing so.
    #
    def __setitem__(self, propName, propValue):
        # Instead of relying on data hiding, we check the type of the
        # master of this aggregate.
        if isinstance(self.master, StringTemplate):
            self.properties[propName] = propValue
        else:
            raise AttributeError

    def __getitem__(self, propName):
        # Instead of relying on data hiding, we check the type of the
        # master of this aggregate.
        if isinstance(self.master, StringTemplate):
            if self.properties.has_key(propName):
                return self.properties[propName]
            return None
        raise AttributeError

    def __contains__(self, propName):
        # Instead of relying on data hiding, we check the type of the
        # master of this aggregate.
        if isinstance(self.master, StringTemplate):
            return self.properties.has_key(propName)
        raise AttributeError

## A StringTemplate is a "document" with holes in it where you can stick
#  values. StringTemplate breaks up your template into chunks of text and
#  attribute expressions, which are by default enclosed in angle brackets:
#  <attribute-expression>.
#  StringTemplate ignores everything outside of attribute expressions,
#  treating it as just text to spit out when you call str(StringTemplate).
#
#  StringTemplate is not a "system" or "engine" or "server"; it's a library
#  with two classes of interest: StringTemplate and StringTemplateGroup.
#  You can directly create a StringTemplate in Java code or you can load a
#  template from a file.
# 
#  A StringTemplate describes an output pattern/language like an exemplar.
#  StringTemplate and associated code is released under the BSD licence. See
#  source.
#
#  Copyright (c) 2003-2004 Terence Parr
#  A particular instance of a template may have a set of attributes that
#  you set programmatically.  A template refers to these single or multi-
#  valued attributes when writing itself out.  References within a
#  template conform to a simple language with attribute references and
#  references to other, embedded, templates.  The references are surrounded
#  by user-defined start/stop strings (default of <...>, but $...$ works
#  well when referencing attributes in HTML to distinguish from tags).
#
#  StringTemplateGroup is a self-referential group of StringTemplate
#  objects kind of like a grammar.  It is very useful for keeping a
#  group of templates together.  For example, jGuru.com's premium and
#  guest sites are completely separate sets of template files organized
#  with a StringTemplateGroup.  Changing "skins" is a simple matter of
#  switching groups.  Groups know where to load templates by either
#  looking under a rootDir you can specify for the group or by simply
#  looking for a resource file in the current class path.  If no rootDir
#  is specified, template files are assumed to be resources.  So, if
#  you reference template foo() and you have a rootDir, it looks for
#  file rootDir/foo.st.  If you don't have a rootDir, it looks for
#  file foo.st in the CLASSPATH.  note that you can use org/antlr/misc/foo()
#  (qualified template names) as a template ref.
#
#  StringTemplateErrorListener is an interface you can implement to
#  specify where StringTemplate reports errors.  Setting the listener
#  for a group automatically makes all associated StringTemplate
#  objects use the same listener.  For example,
#
#  group = StringTemplateGroup("loutSyndiags")
#  class MyStringTemplateErrorListener(StringTemplateErrorListener):
#      def error(self, msg, e):
#          sys.stderr.write('StringTemplate error: ' + msg)
#          if e:
#              sys.stderr.write(': ' + e.getMessage())
#  
#  group.setErrorListener(MyStringTemplateErrorListener)
#
#  IMPLEMENTATION
#
#  A StringTemplate is both class and instance like in Self.  Given
#  any StringTemplate (even one with attributes filled in), you can
#  get a new "blank" instance of it.
#
#  When you define a template, the string pattern is parsed and
#  broken up into chunks of either String or attribute/template actions.
#  These are typically just attribute references.  If a template is
#  embedded within another template either via setAttribute or by
#  implicit inclusion by referencing a template within a template, it
#  inherits the attribute scope of the enclosing StringTemplate instance.
#  All StringTemplate instances with the same pattern point to the same
#  list of chunks since they are immutable there is no reason to have
#  a copy in every instance of that pattern.  The only thing that differs
#  is that every StringTemplate Java object can have its own set of
#  attributes.  Each chunk points back at the original StringTemplate
#  Java object whence they were constructed.  So, there are multiple
#  pointers to the list of chunks (one for each instance with that
#  pattern) and only one back ptr from a chunk to the original pattern
#  object.  This is used primarily to get the grcoup of that original
#  so new templates can be loaded into that group.
#
#  To write out a template, the chunks are walked in order and asked to
#  write themselves out.  String chunks are obviously just written out,
#  but the attribute expressions/actions are evaluated in light of the
#  attributes in that object and possibly in an enclosing instance.
#
class StringTemplate(object):

    VERSION = None

    debugMode = None

    ## track probable issues like setting attribute that is not referenced.
    #
    lintMode = None
    
    ## this is a list of attributes (properties in StringTemplate jargon)
    #  that are derived for the StringTemplate, but which we want to hide
    #  from the properties overview in the AttributeReflectionController.
    #
    hideProperties = []

    def getNextTemplateCounter():
        StringTemplate.templateCounter += 1
        return StringTemplate.templateCounter
    getNextTemplateCounter = staticmethod(getNextTemplateCounter)
    # getNextTemplateCounter = synchronized(getNextTemplateCounter)

    ## reset the template ID counter to 0; def that testing routine
    #  can access but not really of interest to the user.
    #
    def resetTemplateCounter():
        StringTemplate.templateCounter = 0
    resetTemplateCounter = staticmethod(resetTemplateCounter)

    def initialize(self):
        self.referencedAttributes = None
        ## What's the name of self template?#
        self.name = "anonymous"
        self.templateID = StringTemplate.getNextTemplateCounter()
        ## Enclosing instance if I'm embedded within another template.
        #  IF-subtemplates are considered embedded as well.
        #
        self.enclosingInstance = None
        ## A list of embedded templates#
        self.embeddedInstances = None
        ## If self template is an embedded template such as when you apply
        #  a template to an attribute, then the arguments passed to self
        #  template represent the argument context--a set of values
        #  computed by walking the argument assignment list.  For example,
        #  <name:bold(item=name, foo="x")> would result in an
        #  argument context of:[item=name], [foo="x"] for self
        #  template.  This template would be the bold() template and
        #  the enclosingInstance would point at the template that held
        #  that <name:bold(...)> template call.  When you want to get
        #  an attribute value, you first check the attributes for the
        #  'self' template then the arg context then the enclosingInstance
        #  like resolving variables in pascal-like language with nested
        #  procedures.
        #
        #  With multi-valued attributes such as <faqList:briefFAQDisplay()>
        #  attribute "i" is set to 1..n.
        #
        self.argumentContext = None
        ## If self template is embedded in another template, the arguments
        #  must be evaluated just before each application when applying
        #  template to a list of values.  The "it" attribute must change
        #  with each application so that $names:bold(item=it)$ works.  If
        #  you evaluate once before starting the application loop then it
        #  has a single fixed value.  Eval.g saves the AST rather than evaluating
        #  before invoking applyListOfAlternatingTemplates().  Each iteration
        #  of a template application to a multi-valued attribute, these args
        #  are re-evaluated with an initial context of:[it=...], [i=...].
        #
        self.argumentsAST = None
        ## When templates are defined in a group file format, the attribute
        #  list is provided including information about attribute cardinality
        #  such as present, optional, ...  When self information is available,
        #  rawSetAttribute should do a quick existence check as should the
        #  invocation of other templates.  So if you ref bold(item="foo") but
        #  item is not defined in bold(), then an exception should be thrown.
        #  When actually rendering the template, the cardinality is checked.
        #  This is a { str:FormalArgument} dictionary.
        #
        self.formalArguments = FormalArgument.UNKNOWN
        ## What group originally defined the prototype for self template?
        #  This affects the set of templates I can refer to.
        #
        self.group = None
        ## Where to report errors
        #
        self.listener = None
        ## The original, immutable pattern/language (not really used again after
        #  initial "compilation", setup/parsing).
        #
        self.pattern = None
        ## Map an attribute name to its value(s).  These values are set by
        #  outside code via st[name] = value.  StringTemplate is like self in
        #  that a template is both the "class def" and "instance".  When you
        #  create a StringTemplate or setTemplate, the text is broken up into
        #  chunks (i.e., compiled down into a series of chunks that can be
        #  evaluated later).
        #  You can have multiple.
        #
        self.attributes = None
        ## A list of alternating string and ASTExpr references.
        #  This is compiled when the template is loaded/defined and walked to
        #  write out a template instance.
        #
        self.chunks = None
        self.defaultGroup = StringTemplateGroup("defaultGroup", ".")

    ## Either:
    #    Create a blank template with no pattern and no attributes
    #  Or:
    #    Create an anonymous template.  It has no name just
    #    chunks (which point to self anonymous template) and attributes.
    #  Or:
    #    Create an anonymous template with no name, but with a group
    #  Or:
    #    Create a template
    #
    def __init__(self, *args):
        self.initialize()
        # make sure has a group even if default
        if len(args) > 0 and type(args[0]) == type(antlr.CharScanner):
            self.setGroup(StringTemplateGroup("angleBracketsGroup", args[0]))
            self.setTemplate(args[1])
        elif len(args) > 0 and isinstance(args[0], StringTemplateGroup):
            self.setGroup(args[0])
            if len(args) > 1 and type(args[1]) == type(antlr.CharScanner):
                self.group.templateLexerClass = args[1]
            self.setTemplate(args[1])
            if StringTemplate.debugMode:
                self.debug('new StringTemplate(group, [\'' + str(self.pattern) +
                           '\']): ' + str(self.getTemplateID()))
        elif len(args) > 0 and isinstance(args[0], str):
            self.group = self.defaultGroup
            if len(args) > 1 and type(args[1]) == type(antlr.CharScanner):
                self.group.templateLexerClass = args[1]
            self.setTemplate(args[0])
            if StringTemplate.debugMode:
                self.debug('new StringTemplate(\'' + str(self.pattern) +
                           '\'): ' + str(self.getTemplateID()))
        else:
            self.group = self.defaultGroup
            if StringTemplate.debugMode:
                self.debug('new StringTemplate(): ' +
                           str(self.getTemplateID()))


    ## Make the 'to' template look exactly like the 'from' template
    #  except for the attributes.  This is like creating an instance
    #  of a class in that the executable code is the same (the template
    #  chunks), but the instance data is blank (the attributes).  Do
    #  not copy the enclosingInstance pointer since you will want self
    #  template to eval in a context different from the examplar.
    #
    def dup(self, fr, to):
        if StringTemplate.debugMode:
            self.debug('dup template ID ' + str(fr.getTemplateID()) +
                       ' to get ' + str(to.getTemplateID()))
        to.pattern = copy(fr.pattern)
        to.chunks = copy(fr.chunks)
        to.formalArguments = copy(fr.formalArguments)
        to.name = copy(fr.name)
        to.group = copy(fr.group)
        to.listener = copy(fr.listener)

    ## Make an instance of self template; it contains an exact copy of
    #  everything (except the attributes and enclosing instance pointer).
    #  So the new template refers to the previously compiled chunks of self
    #  template but does not have any attribute values.
    #
    def getInstanceOf(self):
        if StringTemplate.debugMode:
            self.debug('getInstanceOf(' + str(self.getName()) + ')')
        t = self.group.createStringTemplate()
        self.dup(self, t)
        return t

    def getEnclosingInstance(self):
        return self.enclosingInstance

    def setEnclosingInstance(self, enclosingInstance):
        if self == self.enclosingInstance:
            raise AttributeError('cannot embed template ' +
                                 str(self.getName()) + ' in itself')
        # set the parent for self template
        self.enclosingInstance = enclosingInstance
        # make the parent track self template as an embedded template
        self.enclosingInstance.addEmbeddedInstance(self)

    def addEmbeddedInstance(self, embeddedInstance):
        if not self.embeddedInstances:
            self.embeddedInstances = []
        self.embeddedInstances.append(embeddedInstance)

    def getArgumentContext(self):
        return self.argumentContext

    def setArgumentContext(self, ac):
        self.argumentContext = ac

    def getArgumentsAST(self):
        return self.argumentsAST

    def setArgumentsAST(self, argumentsAST):
        self.argumentsAST = argumentsAST

    def getName(self):
        return self.name

    def getOutermostName(self):
        if self.enclosingInstance:
            return self.enclosingInstance.getOutermostName()
        return self.getName()

    def setName(self, name):
        self.name = name

    def getGroup(self):
        return self.group

    def setGroup(self, group):
        self.group = group

    def setTemplate(self, template):
        self.pattern = template
        self.breakTemplateIntoChunks()

    def getTemplate(self):
        return self.pattern

    def setErrorListener(self, listener):
        self.listener = listener

    def getErrorListener(self):
        if not self.listener:
            return self.group.getErrorListener()
        return self.listener

    def reset(self):
        # just throw out table and make new one
        self.attributes = {}

    def setPredefinedAttributes(self):
        # only do self method so far in lint mode
        if not StringTemplate.lintMode:
            return
        if self.formalArguments != FormalArgument.UNKNOWN:
            # must define self before setting if the user has defined
            # formal arguments to the template
            self.defineFormalArgument(ASTExpr.REFLECTION_ATTRIBUTES)
        if not self.attributes:
            self.attributes = {}
        self.rawSetAttribute(self.attributes, ASTExpr.REFLECTION_ATTRIBUTES,
                             str(AttributeReflectionController(self)))

    def removeAttribute(self, name):
        del self.attributes[name]

    __delitem__ = removeAttribute

    ## Set an attribute for self template.  If you set the same
    #  attribute more than once, you get a multi-valued attribute.
    #  If you send in a StringTemplate object as a value, its
    #  enclosing instance (where it will inherit values from) is
    #  set to 'self'.  This would be the normal case, though you
    #  can set it back to None after this call if you want.
    #  If you send in a List plus other values to the same
    #  attribute, they all get flattened into one List of values.
    #  If you send in an array, it is converted to a List.  Works
    #  with arrays of objects and arrays of:int,float,double.
    #
    def setAttribute(self, name, *values):
        if len(values) == 0:
            return
        if len(values) == 1:
            value = values[0]
            if StringTemplate.debugMode:
                self.debug(self.getName() + '.setAttribute(' + str(name) +
                           ', ' + str(value) + ')')
            if not self.attributes:
                self.attributes = {}
            if isinstance(value, StringTemplate):
                value.setEnclosingInstance(self)
            #else:
            #    # convert value if array
            #    value = ASTExpr.convertArrayToList(value)
            # convert plain collections
            # get exactly in self scope (no enclosing)
            obj = self.attributes.has_key(name)
            if obj: # it's a multi-value attribute
                obj = self.attributes[name]
                # sys.stderr.write('exists: ' + name + '=' + str(obj) + '\n')
                if isinstance(obj, list):
                    v = deepcopy(obj)
                    # make it pt to list now
                    self.rawSetAttribute(self.attributes, name, v)
                    v.append(value)

                else:
                    # second attribute, must convert existing to list
                    # make list to hold multiple values
                    v = []
                    # make it pt to list now
                    self.rawSetAttribute(self.attributes, name, v)
                    # add previous single-valued attribute
                    v.append(obj)
                    v.append(value)

            else:
                self.rawSetAttribute(self.attributes, name, value)
        else:
            ## Create an aggregate from the list of properties in aggrSpec and
            #  fill with values from values array.
            #
            aggrSpec = name
            aggrName, properties = self.parseAggregateAttributeSpec(aggrSpec)
            if not values or len(properties) == 0:
                raise ValueError('missing properties or values for \'' + aggrSpec + '\'')
            if len(values) != len(properties):
                raise IndexError('number of properties in \'' + aggrSpec + '\' != number of values')
            aggr = Aggregate(self)
            for i in range(len(values)):
                value = values[i]
                if isinstance(value, StringTemplate):
                    value.setEnclosingInstance(self)
                #else:
                #    value = AST.Expr.convertArrayToList(value)
                property_ = properties[i]
                aggr[property_] = value
            self.setAttribute(aggrName, aggr)

    __setitem__ = setAttribute

    ## Split "aggrName.{propName1,propName2" into list [propName1,propName2]
    #  and the aggrName.
    #
    def parseAggregateAttributeSpec(self, aggrSpec):
        dot = aggrSpec.find('.')
        if dot <= 0:
            raise ValueError('invalid aggregate attribute format: ' + aggrSpec)
        aggrName = aggrSpec[:dot]
        propString = aggrSpec[dot+1:]
        propString = propString.split('{',2)[-1].split('}',2)[0].split(',')

        return aggrName, propString

    ## Map a value to a named attribute.  Throw KeyError if
    #  the named attribute is not formally defined in this specific template
    #  and a formal argument list exists.  This is only used internally by
    #  eval.g
    #
    def rawSetAttribute(self, attributes, name, value):
        if StringTemplate.debugMode:
            self.debug(self.getName() + '.rawSetAttribute(' + str(name) +
                       ', ' + str(value) + ')')
        if self.formalArguments != FormalArgument.UNKNOWN and \
           not self.hasFormalArgument(name):
            raise KeyError('no such attribute: ' + name + ' in template ' +
	                   'context ' + self.getEnclosingInstanceStackString())
#        if not value:
#            return
        attributes[name] = value

    ## Walk the chunks, asking them to write themselves out according
    #  to attribute values of 'self.attributes'.  This is like evaluating or
    #  interpreting the StringTemplate as a program using the
    #  attributes.  The chunks will be identical (point at same list)
    #  for all instances of self template.
    #
    def write(self, out):
        n = 0
        self.setPredefinedAttributes()
        if self.chunks:
	    i = 0
	    while i < len(self.chunks):
	        a = self.chunks[i]
                chunkN = a.write(self, out)
                # NEWLINE expr-with-no-output NEWLINE => NEWLINE
                # Indented $...$ have the indent stored with the ASTExpr
                # so the indent does not come out as a StringRef
                if not chunkN and (i-1) >= 0 and \
                   isinstance(self.chunks[i-1], NewlineRef) and \
                   (i+1) < len(self.chunks) and \
                   isinstance(self.chunks[i+1], NewlineRef):
                    #sys.stderr.write('found pure \\n blank \\n pattern\n')
                    i += 1 # make it skip over the next chunk, the NEWLINE
                n += chunkN
		i += 1
        if StringTemplate.lintMode:
            self.checkForTrouble()
        return n

    ## Resolve an attribute reference.  It can be in three possible places:
    #
    #  1. the attribute list for the current template
    #  2. if self is an embedded template, somebody invoked us possibly
    #     with arguments--check the argument context
    #  3. if self is an embedded template, the attribute list for the enclosing
    #     instance (recursively up the enclosing instance chain)
    #
    #  Attribute references are checked for validity.  If an attribute has
    #  a value, its validity was checked before template rendering.
    #  If the attribute has no value, then we must check to ensure it is a
    #  valid reference.  Somebody could reference any random value like $xyz$
    #  formal arg checks before rendering cannot detect self--only the ref
    #  can initiate a validity check.  So, if no value, walk up the enclosed
    #  template tree again, this time checking formal parameters not
    #  attributes dictionary.  The formal definition must exist even if no
    #  value.
    #
    #  To avoid infinite recursion in str(), we have another condition
    #  to check regarding attribute values.  If your template has a formal
    #  argument, foo, then foo will hide any value available from "above"
    #  in order to prevent infinite recursion.
    #
    #  This method is not static so people can override its functionality.
    #
    def get(self, this, attribute):
        #sys.stderr.write('get(' + self.getName() + ', ' + str(attribute) +
        #                 ')\n')
        if not this:
            return None

        if StringTemplate.lintMode:
            this.trackAttributeReference(attribute)

        # is it here?
        o = None
        if this.attributes and this.attributes.has_key(attribute):
            o = this.attributes[attribute]
            return o

        # nope, check argument context in case embedded
        if not o:
            argContext = this.getArgumentContext()
            if argContext and argContext.has_key(attribute):
                o = argContext[attribute]
                return o

        if not o and this.hasFormalArgument(attribute):
            # if you've defined attribute as formal arg for self
            # template and it has no value, do not look up the
            # enclosing dynamic scopes.  This avoids potential infinite
            # recursion.
            return None

        # not locally defined, check enclosingInstance if embedded
        if not o and this.enclosingInstance:
            #sys.stderr.write('looking for ' + self.getName() + '.' + \
            #                 str(attribute) + ' in super [=' + \
            #                 this.enclosingInstance.getName() + ']\n')
            valueFromEnclosing = self.get(this.enclosingInstance, attribute)
            if not valueFromEnclosing:
                self.checkNullAttributeAgainstFormalArguments(this, attribute)
            o = valueFromEnclosing

        return o

    def getAttribute(self, name):
        #sys.stderr.write(self.getName() + '.getAttribute(' + str(name) + ')\n')
        return self.get(self, name)

    __getitem__ = getAttribute

    ## Walk a template, breaking it into a list of
    #  chunks: Strings and actions/expressions.
    #
    def breakTemplateIntoChunks(self):
        #sys.stderr.write('parsing template: ' + str(self.pattern) + '\n')
        if not self.pattern:
            return
        try:
            # instead of creating a specific template lexer, use
            # an instance of the class specified by the user.
            # The default is DefaultTemplateLexer.
            # The only constraint is that you use an ANTLR lexer
            # so I can use the special ChunkToken.
            lexerClass = self.group.getTemplateLexerClass()
            chunkStream = lexerClass(StringIO(self.pattern))
            chunkStream.setTokenObjectClass(ChunkToken)
            chunkifier = TemplateParser.Parser(chunkStream)
            chunkifier.template(self)
        except Exception, e:
            name = "<unknown>"
            outerName = self.getOutermostName()
            if self.getName():
                name = self.getName()
            if outerName and not name == outerName:
                name = name + ' nested in ' + outerName
            self.error('problem parsing template \'' + name + '\' ', e)

    def parseAction(self, action):
        if StringTemplate.debugMode:
            self.debug(str(self.getName()) + '.parseAction(' + str(action) + ')')
        lexer = ActionLexer.Lexer(StringIO(str(action)))
        parser = ActionParser.Parser(lexer, self)
        parser.setASTNodeClass(StringTemplateAST)
        a = None
        try:
            options = parser.action()
            tree = parser.getAST()
            if tree:
                if tree.getType() == ActionParser.CONDITIONAL:
                    a = ConditionalExpr(self, tree)
                else:
                    a = ASTExpr(self, tree, options)

        except antlr.RecognitionException, re:
            self.error('Can\'t parse chunk: ' + str(action), re)
        except antlr.TokenStreamException, tse:
            self.error('Can\'t parse chunk: ' + str(action), tse)
        return a

    def getTemplateID(self):
        return self.templateID

    def getAttributes(self):
        return self.attributes

    ## Get a list of the strings and subtemplates and attribute
    #  refs in a template.
    #
    def getChunks(self):
        return self.chunks

    def addChunk(self, e):
        if not self.chunks:
            self.chunks = []
        self.chunks.append(e)

    def setAttributes(self, attributes):
        self.attributes = attributes

# ----------------------------------------------------------------------------
#                      F o r m a l  A r g  S t u f f
# ----------------------------------------------------------------------------

    def getFormArguments(self):
        return self.formalArguments

    def setFormalArguments(self, args):
        self.formalArguments = args

    ## From self template upward in the enclosing template tree,
    #  recursively look for the formal parameter.
    #
    def lookupFormalArgument(self, name):
        if not self.hasFormalArgument(name):
            if self.enclosingInstance:
                arg = self.enclosingInstance.lookupFormalArgument(name)
            else:
                arg = None
        else:
            arg = self.getFormalArgument(name)
        return arg

    def getFormalArgument(self, name):
        return self.formalArguments[name]

    def hasFormalArgument(self, name):
        return self.formalArguments.has_key(name)

    def defineEmptyFormalArgumentList(self):
        self.formalArguments = {}

    def defineFormalArgument(self, name):
        a = FormalArgument(name)
        if self.formalArguments == FormalArgument.UNKNOWN:
            self.formalArguments = {}
        self.formalArguments[name] = a

# ----------------------------------------------------------------------------
#                      U t i l i t y  R o u t i n e s
# ----------------------------------------------------------------------------

    def warning(self, msg):
        if self.getErrorListener():
            self.getErrorListener().warning(msg)
        else:
            sys.stderr.write('StringTemplate: warning: ' + msg)

    def debug(self, msg):
        if self.getErrorListener():
            self.getErrorListener().debug(msg)
        else:
            sys.stderr.write('StringTemplate: debug: ' + msg)

    def error(self, msg, e = None):
        if self.getErrorListener():
            self.getErrorListener().error(msg, e)
        elif e:
            sys.stderr.write('StringTemplate: error: ' + msg + ': ' +
                             str(e))
        else:
            sys.stderr.write('StringTemplate: error: ' + msg)


    ## Make StringTemplate check your work as it evaluates templates.
    #  Problems are sent to error listener.   Currently warns when
    #  you set attributes that are not used.
    #
    def setLintMode(lint):
        StringTemplate.lintMode = lint
    setLintMode = staticmethod(setLintMode)

    def inLintMode():
        return StringTemplate.lintMode
    inLintMode = staticmethod(inLintMode)

    ## Debug mode is pretty much useless at the moment!
    #
    def setDebugMode(debug):
        StringTemplate.debugMode = debug
    setDebugMode = staticmethod(setDebugMode)

    def inDebugMode():
        return StringTemplate.debugMode
    inDebugMode = staticmethod(inDebugMode)

    ## Indicates that 'name' has been referenced in self template.
    #
    def trackAttributeReference(self, name):
        if not self.referencedAttributes:
            self.referencedAttributes = []
        if not name in self.referencedAttributes:
            self.referencedAttributes.append(name)

    ## Look up the enclosing instance chain (and include self) to see
    #  if st is a template already in the enclosing instance chain.
    #
    def isRecursiveEnclosingInstance(st):
        if not st:
            return False
        p = st.enclosingInstance
        if p == st:
            # self-recursive
            return True
        # now look for indirect recursion
        while p:
            if p == st:
                return True
            p = p.enclosingInstance
        return False
    isRecursiveEnclosingInstance = staticmethod(isRecursiveEnclosingInstance)

    def getEnclosingInstanceStackTrace(self):
        buf = StringIO()
        seen = {}
        p = self
        while p:
            if hash(p) in seen:
                buf.write(p.getTemplateDeclaratorString())
                buf.write(" (start of recursive cycle)\n...")
                break
            seen[hash(p)] = p
            buf.write(p.getTemplateDeclaratorString())
            if p.attributes:
                buf.write(", attributes=[")
                i = 0
                for attrName in p.attributes.keys():
                    if i > 0:
                        buf.write(", ")
                    i += 1
                    buf.write(attrName)
                    o = p.attributes[attrName]
                    if isinstance(o, StringTemplate):
                        buf.write('=<' + o.getName() + '()@')
                        buf.write(str(o.getTemplateID()) + '>')
                    elif isinstance(o, list):
                        buf.write("=List[..")
                        n = 0
                        for st in o:
                            if isinstance(st, StringTemplate):
                                if n > 0:
                                    buf.write(", ")
                                n += 1
                                buf.write('<' + st.getName() + '()@')
                                buf.write(str(st.getTemplateID()) + '>')

                        buf.write("..]")

                buf.write(']')
            if p.referencedAttributes:
                buf.write(', references=')
                buf.write(p.referencedAttributes)
            buf.write('>\n')
            p = p.enclosingInstance
        # if self.enclosingInstance:
        #     buf.write(enclosingInstance.getEnclosingInstanceStackTrace())
        retval = buf.getvalue()
        buf.close()
        return retval

    def getTemplateDeclaratorString(self):
        return '<' + self.getName() + '(' + str(self.formalArguments.keys()) + \
               ')' + '@' + str(self.getTemplateID()) + '>'

    ## A reference to an attribute with no value, must be compared against
    #  the formal parameter to see if it exists; if it exists all is well,
    #  but if not, throw an exception.
    #
    #  Don't do the check if no formal parameters exist for self template
    #  ask enclosing.
    #
    def checkNullAttributeAgainstFormalArguments(self, this, attribute):
        if this.getFormArguments() == FormalArgument.UNKNOWN:
            # bypass unknown arg lists
            if this.enclosingInstance:
                self.checkNullAttributeAgainstFormalArguments(
                    this.enclosingInstance, attribute)
        else:
            formalArg = self.lookupFormalArgument(attribute)
            if not formalArg:
                raise KeyError('no such attribute: ' + str(attribute) +
		               ' in template context ' +
			       self.getEnclosingInstanceStackString())

    ## Executed after evaluating a template.  For now, checks for setting
    #  of attributes not reference.
    #
    def checkForTrouble(self):
        # we have table of set values and list of values referenced
        # compare, looking for SET BUT NOT REFERENCED ATTRIBUTES
        if not self.attributes:
            return
        for name in self.attributes.keys():
            if self.referencedAttributes and \
               not name in self.referencedAttributes and \
               not name == ASTExpr.REFLECTION_ATTRIBUTES:
                self.warning(self.getName() + ': set but not used: ' + name)

        # can do the reverse, but will have lots of False warnings :(

    ## If an instance of x is enclosed in a y which is in a z, return
    #  a String of these instance names in order from topmost to lowest;
    #  here that would be "[z y x]".
    #
    def getEnclosingInstanceStackString(self):
	names = []
	p = self
	while p:
	    names.append(p.getName())
	    p = p.enclosingInstance
        names.reverse()
	s = '['
	while names:
	    s += names[0]
	    if len(names) > 1:
	        s += ' '
	    names = names[1:]
        return s + ']'

    def toDebugString(self):
        buf = StringIO()
        buf.write('template-' + self.getName() + ': chunks=' + str(chunks))
        buf.write('attributes=[')
        n = 0
        for name in self.attributes.keys():
            if n > 0:
                buf.write(',')
            buf.write(name + '=')
            value = self.attributes[name]
            if isinstance(value, StringTemplate):
                buf.write(value.toDebugString())
            else:
                buf.write(value)
            n += 1
        buf.write(']')
        retval = buf.getvalue()
        buf.close()
        return retval

    def printDebugString(self):
        sys.stderr.write('template-' + self.getName() + ':\n')
        sys.stderr.write('chunks=' + str(chunks))
        if not self.attributes:
            return
        sys.stderr.write("attributes=[")
        n = 0
        for name in self.attributes.keys():
            if n > 0:
                sys.stderr.write(',')
            value = self.attributes[name]
            if isinstance(value, StringTemplate):
                sys.stderr.write(name + '=')
                value.printDebugString()
            else:
                if isinstance(value, list):
                    i = 0
                    for o in value:
                        sys.stderr.write(name + '[' + i + '] is ' +
                                         o.__class__.__name__ + '=')
                        if isinstance(o, StringTemplate):
                            o.printDebugString()
                        else:
                            sys.stderr.write(o)
                        i += 1
                else:
                    sys.stderr.write(name + '=' + value + '\n')

            n += 1
        sys.stderr.write("]\n")

    def __str__(self):
        # Write the output to a StringIO
        out = StringIO()
        # TODO seems slow to create all these objects, can I use a singleton?
        wr = self.group.getStringTemplateWriter(out)
        #StringTemplateWriter buf = new AutoIndentWriter(out)
        try:
            self.write(wr)
        except IOError, io:
            self.error("Got IOError writing to StringIO")
        retval = out.getvalue()
        out.close()
        return retval

# Set class data attribute values.
StringTemplate.VERSION = "2.1.1"
StringTemplate.debugMode = False
StringTemplate.lintMode = False
StringTemplate.templateCounter=0
StringTemplate.hideProperties = \
['argumentContext', 'argumentsAST', 'attributes', 'chunks', 'enclosingInstance', 
'enclosingInstanceStackTrace', 'errorListener', 'formArguments', 'group', 
'instanceOf', 'name', 'nextTemplateCounter', 'outermostName', 'template', 
'templateDeclaratorString', 'templateID']

class StringTemplateGroupErrorListener(StringTemplateErrorListener):

    def error(self, s, e):
        sys.stderr.write(s + '\n')
        if e:
            traceback.print_exc()

    def warning(self, s):
        sys.stderr.write(s + '\n')

    def debug(self, s):
        sys.stderr.write(s + '\n')


## Manages a group of named mutually-referential StringTemplate objects.
#  Currently the templates must all live under a directory so that you
#  can reference them as foo.st or gutter/header.st.  To refresh a
#  group of templates, just create a new StringTemplateGroup and start
#  pulling templates from there.  Or, set the refresh interval.
#
#  Use getInstanceOf(template-name) to get a string template
#  to fill in.
#
#  The name of a template is the file name minus ".st" ending if present
#  unless you name it as you load it.
# 
class StringTemplateGroup(object):

    ## Track all groups by name; maps name to StringTemplateGroup
    #
    nameToGroupMap = None

    DEFAULT_ERROR_LISTENER = None

    DEFAULT_EXTENSION = ''

    ## Used to indicate that the template doesn't exist.
    #  We don't have to check disk for it; we know it's not there.
    #
    NOT_FOUND_ST = -1

    ## this is a list of attributes (properties in StringTemplate jargon)
    #  that are derived for the StringTemplate, but which we want to hide
    #  from the properties overview in the AttributeReflectionController.
    #
    hideProperties = []

    def _initialize(self):      
        ## What is the group name
        #
        self.name = None
        ## Maps template name to StringTemplate object
        #
        self.templates = {}
        ## How to pull apart a template into chunks?
        #
        self.templateLexerClass = DefaultTemplateLexer.Lexer
        ## Under what directory should I look for templates?  If None,
        #  to look into the CLASSPATH for templates as resources.
        #
        self.rootDir = None
        ## Are we derived from another group?  Templates not found in this group
        #  will be searched for in the superGroup recursively.
        #
        self.superGroup = None
        ## When templates are files on the disk, the refresh interval is used
        #  to know when to reload.  When a Reader is passed to the ctor,
        #  it is a stream full of template definitions.  The former is used
        #  for web development, but the latter is most likely used for source
        #  code generation for translators; a refresh is unlikely.  Anyway,
        #  I decided to track the source of templates in case such info is useful
        #  in other situations than just turning off refresh interval.  I just
        #  found another: don't ever look on the disk for individual templates
        #  if this group is a group file...immediately look into any super group.
        #  If not in the super group, report no such template.
        #
        self.templatesDefinedInGroupFile = False
        ## Normally AutoIndentWriter is used to filter output, but user can
        #  specify a new one.
        #
        self.userSpecifiedWriter = None
        ## Where to report errors.  All string templates in this group
        #  use this error handler by default.
        #
        self.listener = StringTemplateGroup.DEFAULT_ERROR_LISTENER
        ## How long before tossing out all templates in seconds.
        #  default: no refreshing from disk
        #
        self.refreshIntervalInSeconds = sys.maxint/1000
        self.lastCheckedDisk = 0L
        ## How are the files encode (ascii, UTF8, ...)?  You might want to read
        #  UTF8 for example on a ascii machine.
        #
        self.fileCharEncoding = sys.getdefaultencoding()

    ## Either:
    #    Create a group manager for some templates, all of which are
    #    at or below the indicated directory.
    #  Or:
    #    Create a group from the template group defined by a input stream.
    #    The name is pulled from the file.  The format is
    #
    #  group name
    #
    #  t1(args) : "..."
    #  t2 : <<
    #  >>
    #  ...
    #
    #  Or:
    #    Create a group from the input stream, but use a nondefault lexer
    #    to break the templates up into chunks.  This is usefor changing
    #    the delimiter from the default $...$ to <...>, for example.
    #
    #
    def __init__(self, *args):
        self._initialize()
        if len(args) > 0 and isinstance(args[0], str):
            self.name = args[0]
            self.lastCheckedDisk = time.time()
            StringTemplateGroup.nameToGroupMap[self.name] = self
            if len(args) > 1 and isinstance(args[1], str):
                self.rootDir = args[1]
                if len(args) > 2 and type(args[2]) == type(antlr.CharScanner):
                    self.templateLexerClass = args[2]
            elif len(args) > 1 and type(args[1]) == type(antlr.CharScanner):
                    self.templateLexerClass = args[1]
        elif len(args) > 0 and (isinstance(args[0], file) or
                                isinstance(args[0], StringIO)):
            r = args[0]
            self.templatesDefinedInGroupFile = True
            if len(args) > 1 and type(args[1]) == type(antlr.CharScanner):
                self.templateLexerClass = args[1]
                if len(args) > 2 and isinstance(args[2], StringTemplateErrorListener):
                    self.listener = args[2]
            elif len(args) > 1 and isinstance(args[1], StringTemplateErrorListener):
                    self.listener = args[1]
            
            self.parseGroup(r)

    def getTemplateLexerClass(self):
        return self.templateLexerClass

    def getName(self):
        return self.name

    def setName(self, name):
        self.name = name

    def setSuperGroup(self, superGroup):
        if isinstance(superGroup, StringTemplateGroup):
            self.superGroup = superGroup
        if isinstance(superGroup, str):
            try:
                self.superGroup = StringTemplateGroup.nameToGroupMap[superGroup]
            except:
                self.superGroup = None

    def getSuperGroup(self):
        return self.superGroup

    def getRootDir(self):
        return self.rootDir

    def setRootDir(self, rootDir):
        self.rootDir = rootDir

    ## StringTemplate object factory; each group can have its own.
    #
    def createStringTemplate(self):
        return StringTemplate()

    def getInstanceOf(self, name):
        st = self.lookupTemplate(name)
        #if not st:
        #    return None
        return st.getInstanceOf()

    def getEmbeddedInstanceOf(self, enclosingInstance, name):
        st = self.getInstanceOf(name)
        #if not st:
        #    return None
        st.setEnclosingInstance(enclosingInstance)
        return st

    ## Get the template called 'name' from the group.  If not found,
    #  attempt to load.  If not found on disk, then try the superGroup
    #  if any.  If not even there, then record that it's
    #  NOT_FOUND so we don't waste time looking again later.  If we've gone
    #  past refresh interval, flush and look again.
    #
    def lookupTemplate(self, name):
        if StringTemplate.debugMode:
            self.listener.debug('lookupTemplate(' + name + ')')
        if name.startswith('super.'):
            if self.superGroup:
                dot = name.find('.')
                self.name = name[dot+1:]
                return self.superGroup.lookupTemplate(self.name)
            raise ValueError(getName() + ' has no super group; ' +
                             'invalid template: ' + name)
        self.checkRefreshInterval()
        try:
            st = self.templates[name]
        except:
            st = None
        if not st:
            # not there?  Attempt to load
            if StringTemplate.debugMode:
                self.listener.debug('Attempting load of: ' +
                                    self.getFileNameFromTemplateName(name))
            if not self.templatesDefinedInGroupFile:
                # only check the disk for individual template
                st = self.loadTemplateFromBeneathRootDir(self.getFileNameFromTemplateName(name))
            if (not st) and self.superGroup:
                # try to resolve in super group
                st = self.superGroup.getInstanceOf(name)
                if st:
                    st.setGroup(self)

            if st: # found in superGroup
                # insert into this group; refresh will allow super
                # to change it's def later or this group to add
                # an override.
                self.templates[name] = st
            else:
                # not found; remember that this sucker doesn't exist
                self.templates[name] = StringTemplateGroup.NOT_FOUND_ST
                raise ValueError('Can\'t load template ' + self.getFileNameFromTemplateName(name))

        elif st is StringTemplateGroup.NOT_FOUND_ST:
            return None

        return st

    def checkRefreshInterval(self):
        if self.templatesDefinedInGroupFile:
            return
        if self.refreshIntervalInSeconds == 0 or \
           (time.time() - self.lastCheckedDisk) >= \
           self.refreshIntervalInSeconds:
            # throw away all pre-compiled references
            self.templates = {}
            self.lastCheckedDisk = time.time()

    def loadTemplate(self, name, r):
        nl = os.linesep
        buf = StringIO()
        while True:
            line = r.readline()
            if not line:
                break
            buf.write(line + nl)
        # strip newlines etc.. from front/back since filesystem
        # may add newlines etc...
        pattern = buf.getvalue().strip()
        if not len(pattern):
            self.error('no text in template \'' + name + '\'')
            return None
        return self.defineTemplate(name, pattern)

    ## Load a template whose name is derived from the template filename.
    #  If there is a rootDir, try to load the file from there.
    #
    def loadTemplateFromBeneathRootDir(self, fileName):
        if StringTemplate.debugMode:
            self.listener.debug('loadTemplateFromBeneathRootDir(' + fileName + ')')
        template = None
        name = self.getTemplateNameFromFileName(fileName)
        # if no rootDir, try to load as a resource in CLASSPATH
        # In the Python case that is of course the sys.path
        if not self.rootDir:
            try:
                br, pathName, descr = imp.find_module(name)
            except ImportError:
                br = None
            if not br:
                return None
            try:
                template = self.loadTemplate(name, br)
                br.close()
                br = None
            except IOException, ioe:
                if br:
                    try:
                        br.close()
                    except IOException, ioe2:
                        self.error('Cannot close template file: ' + pathName)

            return template
        # load via rootDir
        template = self.loadTemplate(name, self.rootDir + '/' + fileName)
        return template

    ## (def that people can override behavior; not a general
    #  purpose method)
    #
    def getFileNameFromTemplateName(self, templateName):
        return templateName + StringTemplateGroup.DEFAULT_EXTENSION

    ## Convert a filename relativePath/name.st to relativePath/name.
    #  (def that people can override behavior; not a general
    #  purpose method)
    #
    def getTemplateNameFromFileName(self, fileName):
        name = fileName
        suffix = name.rfind(StringTemplateGroup.DEFAULT_EXTENSION)
        if suffix >= 0:
            name = name[:suffix]
        return name

    def loadTemplate(self, name, file_):
        if isinstance(file_, str):
            br = None
            template = None
            try:
                br = file(file_)
                template = self.loadTemplate(name, br)
                br.close()
                br = None
            except Exception, e:
                if br:
                    try:
                        br.close()
                    except IOError, ioe:
                        self.error('Cannot close template file: ' + file_)
            return template
        elif isinstance(file_, file):
            buf = file_.readlines()
            # strip newlines etc.. from front/back since filesystem
            # may add newlines etc...
            pattern = str().join(buf).strip()
            if not pattern:
                self.error("no text in template '"+name+"'")
                return None
            return self.defineTemplate(name, pattern)
        raise TypeError('loadTemplate should be called with a file or filename')

    def getInputStreamReader(self, ins):
        isr = None
        try:
            isr = InputStreamReader(ins, self.fileCharEncoding)
        except UnsupportedEncodingException, uee:
            self.error('Invalid file character encoding: ' + fileCharEncoding)
        return isr

    def getFileCharEncoding(self):
        return self.fileCharEncoding

    def setFileCharEncoding(fileCharEncoding):
        self.fileCharEncoding = fileCharEncoding

    ## Define an examplar template; precompiled and stored
    #  with no attributes.  Remove any previous definition.
    #
    def defineTemplate(self, name, template):
        st = self.createStringTemplate()
        st.setName(name)
        st.setGroup(self)
        st.setTemplate(template)
        st.setErrorListener(self.listener)
        self.templates[name] = st
        return st

    ## Make name and alias for target.  Replace any previous def of name#
    def defineTemplateAlias(self, name, target):
        targetST = self.getTemplateDefinition(target)
        if not targetST:
            self.error('cannot alias ' + name + ' to undefined template: ' +
                       target)
            return None
        self.templates[name] = targetST
        return targetST

    def isDefinedInThisGroup(self, name):
        return self.templates.has_key(name)

    ## Get the ST for 'name' in this group only
    #
    def getTemplateDefinition(self, name):
        if self.templates.has_key(name):
            return self.templates[name]

    ## Is there *any* definition for template 'name' in this template
    #  or above it in the group hierarchy?
    #
    def isDefined(self, name):
        try:
            self.lookupTemplate(name)
            return True
        except ValueError:
            return False

    def parseGroup(self, r):
        try:
            lexer = GroupLexer.Lexer(r)
            parser = GroupParser.Parser(lexer)
            parser.group(self)
            # sys.stderr.write("read group\n" + str(self))
        except Exception, e:
            name = "<unknown>"
            if self.getName():
                name = self.getName()
            self.error('problem parsing group \'' + name + '\'', e)

    def getRefreshInterval(self):
        return self.refreshIntervalInSeconds

    ## How often to refresh all templates from disk.  This is a crude
    #  mechanism at the moment--just tosses everything out at this
    #  frequency.  Set interval to 0 to refresh constantly (no caching).
    #  Set interval to a huge number like MAX_INT to have no refreshing
    #  at all (DEFAULT); it will cache stuff.
    #
    def setRefreshInterval(self, refreshInterval):
        self.refreshIntervalInSeconds = refreshInterval

    def setErrorListener(self, listener):
        self.listener = listener

    def getErrorListener(self):
        return self.listener

    ## Specify a StringTemplateWriter implementing class to use for
    #  filtering output
    #
    def setStringTemplateWriter(self, c):
        self.userSpecifiedWriter = c

    ## return an instance of a StringTemplateWriter that spits output to w.
    #  If a writer is specified, use it instead of the default.
    #
    def getStringTemplateWriter(self, w):
        stw = None
        if self.userSpecifiedWriter:
            try:
                stw = self.userSpecifiedWriter(w)
            except Exception, e:
                self.error('problems getting StringTemplateWriter',e)

        if not stw:
            stw = AutoIndentWriter(w)
        return stw

    def error(self, msg, e = None):
        if self.listener:
            self.listener.error(msg, e)
        else:
            sys.stderr.write('StringTemplate: ' + msg + ': ' + e + '\n')

    def __str__(self):
        buf = StringIO()
        buf.write('group ' + str(self.getName()) + ';\n')
        formalArgs = StringTemplate('$args;separator=","$')
        for tname in self.templates.keys():
            st = self.templates[tname]
            if st != StringTemplateGroup.NOT_FOUND_ST:
                formalArgs_ = formalArgs.getInstanceOf()
                formalArgs_.setAttribute("args", st.getFormArguments())
                buf.write(str(tname) + '(' + str(formalArgs_) + ') ::= ')
                buf.write('<<' + str(st.getTemplate()) + '>>\n')

        retval = buf.getvalue()
        buf.close()
        return retval

# Set class data attribute values.
StringTemplateGroup.nameToGroupMap = {}
StringTemplateGroup.DEFAULT_ERROR_LISTENER = StringTemplateGroupErrorListener()
StringTemplateGroup.DEFAULT_EXTENSION = '.st'
StringTemplateGroup.NOT_FOUND_ST = StringTemplate()
StringTemplateGroup.hideProperties = \
['errorListener', 'fileCharEncoding', 'name', 'refreshInterval', 'rootDir',
 'superGroup', 'templateLexerClass']
