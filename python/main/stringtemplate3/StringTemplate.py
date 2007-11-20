
# [The "BSD licence"]
# Copyright (c) 2003-2006 Terence Parr
# All rights reserved.
#
# Redistribution and use in source and binary forms, with or without
# modification, are permitted provided that the following conditions
# are met:
# 1. Redistributions of source code must retain the above copyright
#    notice, this list of conditions and the following disclaimer.
# 2. Redistributions in binary form must reproduce the above copyright
#    notice, this list of conditions and the following disclaimer in the
#    documentation and/or other materials provided with the distribution.
# 3. The name of the author may not be used to endorse or promote products
#    derived from this software without specific prior written permission.
#
# THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
# IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
# OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
# IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
# INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
# NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
# DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
# THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
# (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
# THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#

import sys
import os
import traceback
import imp
import time
from StringIO import StringIO
from copy import copy, deepcopy
import codecs

import antlr
#from synchronize import *

from stringtemplate3.language import (
    FormalArgument, ChunkToken,
    ASTExpr, StringTemplateAST,
    AngleBracketTemplateLexer,
    DefaultTemplateLexer, TemplateParser,
    GroupLexer, GroupParser,
    ActionLexer, ActionParser,
    ConditionalExpr, NewlineRef,
    StringTemplateToken,
    InterfaceLexer, InterfaceParser
    )


class STAttributeList(list):
    """
    Just an alias for list, but this way I can track whether a
    list is something ST created or it's an incoming list.
    """

    pass

                
class AttributeRenderer(object):
    """
    This interface describes an object that knows how to format or otherwise
    render an object appropriately.  Usually this is used for locale changes
    for objects such as Date and floating point numbers...  You can either
    have an object that is sensitive to the locale or have a different object
    per locale.

    Each template may have a renderer for each object type or can default
    to the group's renderer or the super group's renderer if the group doesn't
    have one.

    The toString(Object,String) method is used when the user uses the
    format option: $o; format="f"$.  It checks the formatName and applies the
    appropriate formatting.  If the format string passed to the renderer is
    not recognized then simply call toString().
    """

    def __init__(self):
        pass

    def toString(self, o, formatName=None):
        pass


class StringTemplateWriter(object):
    """
    Generic StringTemplate output writer filter.
 
    Literals and the elements of expressions are emitted via write().
    Separators are emitted via writeSeparator() because they must be
    handled specially when wrapping lines (we don't want to wrap
    in between an element and it's separator).
    """

    NO_WRAP = -1
    
    def __init__(self):
        pass

    def pushIndentation(self, indent):
        raise NotImplementedError

    def popIndentation(self):
        raise NotImplementedError

    def pushAnchorPoint(self):
        raise NotImplementedError

    
    def popAnchorPoint(self):
        raise NotImplementedError


    def setLineWidth(self, lineWidth):
        raise NotImplementdError
    

    def write(self, str, wrap=None):
        """
        Write the string and return how many actual chars were written.
        With autoindentation and wrapping, more chars than length(str)
        can be emitted. No wrapping is done unless wrap is not None
        """

        raise NotImplementedError


    def writeWrapSeparator(self, wrap):
        """
        Because we might need to wrap at a non-atomic string boundary
 	(such as when we wrap in between template applications
 	 <data:{v|[<v>]}; wrap>) we need to expose the wrap string
 	writing just like for the separator.
 	"""

        raise NotImplementedError


    def writeSeparator(self, str):
        """
        Write a separator.  Same as write() except that a \n cannot
        be inserted before emitting a separator.
 	"""

        raise NotImplementedError


class AutoIndentWriter(StringTemplateWriter):
    """
    Essentially a char filter that knows how to auto-indent output
    by maintaining a stack of indent levels.  I set a flag upon newline
    and then next nonwhitespace char resets flag and spits out indention.
    The indent stack is a stack of strings so we can repeat original indent
    not just the same number of columns (don't have to worry about tabs vs
    spaces then).

    Anchors are char positions (tabs won't work) that indicate where all
    future wraps should justify to.  The wrap position is actually the
    larger of either the last anchor or the indentation level.
    
    This is a filter on a Writer.

    It may be screwed up for '\r' '\n' on PC's.
    """

    def __init__(self, out):
	## stack of indents
	self.indents = [None] # start with no indent

        ## Stack of integer anchors (char positions in line)
        self.anchors = []

        self.out = out
        self.atStartOfLine = True

	## Track char position in the line (later we can think about tabs).
	#  Indexed from 0.  We want to keep charPosition <= lineWidth.
	#  This is the position we are *about* to write not the position
	#  last written to.
        self.charPosition = 0
	self.lineWidth = self.NO_WRAP

        self.charPositionOfStartOfExpr = 0

        
    def setLineWidth(self, lineWidth):
        self.lineWidth = lineWidth

        
    def pushIndentation(self, indent):
	"""
        Push even blank (null) indents as they are like scopes; must
        be able to pop them back off stack.
        """

        self.indents.append(indent)


    def popIndentation(self):
        return self.indents.pop(-1)


    def getIndentationWidth(self):
        return sum(len(ind) for ind in self.indents if ind is not None)


    def pushAnchorPoint(self):
        self.anchors.append(self.charPosition)


    def popAnchorPoint(self):
        self.anchors.pop(-1)


    def write(self, text, wrap=None):
        """
        Write out a string literal or attribute expression or expression
        element.

        If doing line wrap, then check wrap before emitting this str.  If
        at or beyond desired line width then emit a \n and any indentation
        before spitting out this str.
        """

        assert isinstance(text, basestring), repr(text)
        
        n = 0
        if wrap is not None:
            n += self.writeWrapSeparator(wrap)
            
        for c in text:
            if c == '\n':
                self.atStartOfLine = True;
                self.charPosition = -1 # set so the write below sets to 0

            else:
                if self.atStartOfLine:
                    n += self.indent()
                    self.atStartOfLine = False

            n += 1
            self.out.write(c)
            self.charPosition += 1

        return n
        

    def writeWrapSeparator(self, wrap):
        n = 0
        
        # if want wrap and not already at start of line (last char was \n)
        # and we have hit or exceeded the threshold
        if ( self.lineWidth != self.NO_WRAP and
             not self.atStartOfLine and
             self.charPosition >= self.lineWidth ):
            # ok to wrap
            # Walk wrap string and look for A\nB.  Spit out A\n
            # then spit indent or anchor, whichever is larger
            # then spit out B
            for c in wrap:
                if c == '\n':
                    n += 1
                    self.out.write(c)
                    #atStartOfLine = true;
                    self.charPosition = 0
                    
                    indentWidth = self.getIndentationWidth()
                    try:
                        lastAnchor = self.anchors[-1]
                    except IndexError: # no anchors saved
                        lastAnchor = 0
                        
                    if lastAnchor > indentWidth:
                        # use anchor not indentation
                        n += self.indent(lastAnchor)

                    else:
                        # indent is farther over than last anchor, ignore anchor
                        n += self.indent()

                    # continue writing any chars out

                else: # write A or B part
                    n += 1
                    self.out.write(c)
                    self.charPosition += 1

        return n

    def writeSeparator(self, text):
        return self.write(text)


    def indent(self, spaces=None):
        if spaces is None:
            n = 0
            for ind in self.indents:
                if ind is not None:
                    n += len(ind)
                    self.out.write(ind)

            self.charPosition += n
            return n

        else:
            self.out.write(' ' * spaces)
            self.charPosition += spaces
            return spaces


## Just pass through the text
#
class NoIndentWriter(AutoIndentWriter):

    def __init__(self, out):
        super(NoIndentWriter, self).__init__(out)

    def write(self, str):
        self.out.write(str)
        return len(str)

## Lets you specify where errors, warnings go.
class StringTemplateErrorListener(object):

    def error(self, msg, e):
        raise NotImplementedError

    def warning(self, msg):
        raise NotImplementedError


class DefaultStringTemplateErrorListener(StringTemplateErrorListener):
    def __init__(self, output=None):
        self.output = output or sys.stderr

    def error(self, msg, exc):
        self.output.write(msg + '\n')
        if exc is not None:
            traceback.print_exc(file=self.output)

    def warning(self, msg):
        self.output.write(msg + '\n')


DEFAULT_ERROR_LISTENER = DefaultStringTemplateErrorListener()


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

    def get(self, propName, default=None):
        # Instead of relying on data hiding, we check the type of the
        # master of this aggregate.
        if isinstance(self.master, StringTemplate):
            return self.properties.get(propName, default)
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


    def __str__(self):
        return str(self.properties)

    
class StringTemplate(object):
    """
    A StringTemplate is a "document" with holes in it where you can stick
    values. StringTemplate breaks up your template into chunks of text and
    attribute expressions.  StringTemplate< ignores everything outside
    of attribute expressions, treating it as just text to spit
    out when you call StringTemplate.toString().
    """
    
    VERSION = None

    ANONYMOUS_ST_NAME = None

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
        self.name = StringTemplate.ANONYMOUS_ST_NAME
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
        #  This is a {str:FormalArgument} dictionary.
        #
        self.formalArgumentKeys = None
        self.formalArguments = FormalArgument.UNKNOWN
	## How many formal arguments to this template have default values
	#  specified?
	#
	self.numberOfDefaultArgumentValues = 0
	## Normally, formal parameters hide any attributes inherited from the
	#  enclosing template with the same name.  This is normally what you
	#  want, but makes it hard to invoke another template passing in all
	#  the data.  Use notation now: <otherTemplate(...)> to say "pass in
	#  all data".  Works great.  Can also say <otherTemplate(foo="xxx",...)>
	#
	self.passThroughAttributes = False

        
        ## What group originally defined the prototype for self template?
        #  This affects the set of templates I can refer to.  super.t() must
        #  always refer to the super of the original group.
        #
        #  group base;
        #  t ::= "base";
        #
        #  group sub;
        #  t ::= "super.t()2"
        #
        #  group subsub;
        #  t ::= "super.t()3"
        self.nativeGroup = None

        ## This template was created as part of what group?  Even if this
        #  template was created from a prototype in a supergroup, its group
        #  will be the subgroup.  That's the way polymorphism works.
        self.group = None

        ## If this template is defined within a group file, what line number?
        self.groupFileLine = None
        
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
	## A Map<Class,Object> that allows people to register a renderer for
	#  a particular kind of object to be displayed in this template.  This
	#  overrides any renderer set for this template's group.
	#
	#  Most of the time this map is not used because the StringTemplateGroup
	#  has the general renderer map for all templates in that group.
	#  Sometimes though you want to override the group's renderers.
 	#
	self.attributeRenderers = None
        ## A list of alternating string and ASTExpr references.
        #  This is compiled when the template is loaded/defined and walked to
        #  write out a template instance.
        #
        self.chunks = None

        ## If someone refs <@r()> in template t, an implicit
        #
        # @t.r() ::= ""
        #
        # is defined, but you can overwrite this def by defining your
        # own.  We need to prevent more than one manual def though.  Between
        # this var and isEmbeddedRegion we can determine these cases.
        self.regionDefType = None

        ## Does this template come from a <@region>...<@end> embedded in
        # another template?
        self._isRegion = False

        ## Set of implicit and embedded regions for this template */
        self.regions = set()


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
            self.setGroup(StringTemplateGroup("defaultGroup", args[0]))
            self.setTemplate(args[1])
        elif len(args) > 0 and isinstance(args[0], StringTemplateGroup):
            self.setGroup(args[0])
            if len(args) > 1 and type(args[1]) == type(antlr.CharScanner):
                self.group.templateLexerClass = args[1]
                if len(args) > 2 and isinstance(args[2], dict):
                    self.attributes = args[2]
            self.setTemplate(args[1])
        elif len(args) > 0 and isinstance(args[0], basestring):
            self.group = StringTemplateGroup('defaultGroup', '.')
            if len(args) > 1 and type(args[1]) == type(antlr.CharScanner):
                self.group.templateLexerClass = args[1]
            self.setTemplate(args[0])
        else:
            self.group = StringTemplateGroup('defaultGroup', '.')


    ## Make the 'to' template look exactly like the 'from' template
    #  except for the attributes.  This is like creating an instance
    #  of a class in that the executable code is the same (the template
    #  chunks), but the instance data is blank (the attributes).  Do
    #  not copy the enclosingInstance pointer since you will want self
    #  template to eval in a context different from the examplar.
    #
    def dup(self, fr, to):
        to.attributeRenderers = fr.attributeRenderers
        to.pattern = copy(fr.pattern)
        to.chunks = copy(fr.chunks)
        to.formalArgumentKeys = copy(fr.formalArgumentKeys)
        to.formalArguments = copy(fr.formalArguments)
        to.numberOfDefaultArgumentValues = fr.numberOfDefaultArgumentValues
        to.name = copy(fr.name)
        to.nativeGroup = fr.nativeGroup
        to.group = fr.group
        to.listener = copy(fr.listener)
        to.regions = fr.regions
        to._isRegion = fr._isRegion
        to.regionDefTyep = fr.regionDefType
        
    ## Make an instance of self template; it contains an exact copy of
    #  everything (except the attributes and enclosing instance pointer).
    #  So the new template refers to the previously compiled chunks of self
    #  template but does not have any attribute values.
    #
    def getInstanceOf(self):
        if self.nativeGroup is not None:
            # create a template using the native group for this template
            # but it's "group" is set to this.group by dup after creation so
            # polymorphism still works.
            t = self.nativeGroup.createStringTemplate()

        else:
            t = self.group.createStringTemplate()
            
        self.dup(self, t)
        return t

    def getEnclosingInstance(self):
        return self.enclosingInstance

    def getOutermostEnclosingInstance(self):
        if self.enclosingInstance is not None:
            return self.enclosingInstance.getOutermostEnclosingInstance()

        return self

    def setEnclosingInstance(self, enclosingInstance):
        if self == self.enclosingInstance:
            raise AttributeError('cannot embed template ' +
                                 str(self.getName()) + ' in itself')
        # set the parent for this template
        self.enclosingInstance = enclosingInstance
        # make the parent track self template as an embedded template
	if enclosingInstance:
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
        if self.enclosingInstance is not None:
            return self.enclosingInstance.getOutermostName()
        return self.getName()

    def setName(self, name):
        self.name = name

    def getGroup(self):
        return self.group

    def setGroup(self, group):
        self.group = group

    def getNativeGroup(self):
        return self.nativeGroup

    def setNativeGroup(self, group):
        self.nativeGroup = group

    def getGroupFileLine(self):
        """Return the outermost template's group file line number"""

        if self.enclosingInstance is not None:
            return self.enclosingInstance.getGroupFileLine()

        return self.groupFileLine

    def setGroupFileLine(self, groupFileLine):
        self.groupFileLine = groupFileLine
        
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
    #  This will be a new list object so that incoming objects are
    #  not altered.
    #  If you send in an array, it is converted to a List.  Works
    #  with arrays of objects and arrays of:int,float,double.
    #
    def setAttribute(self, name, *values):
        if len(values) == 0:
            return
        if len(values) == 1:
            value = values[0]

            if value is None or name is None:
                return

            if '.' in name:
                raise ValueError("cannot have '.' in attribute names")
            
            if self.attributes is None:
                self.attributes = {}

            if isinstance(value, StringTemplate):
                value.setEnclosingInstance(self)

            elif (isinstance(value, (list, tuple)) and
                  not isinstance(value, STAttributeList)):
                # convert to STAttributeList
                value = STAttributeList(value)

            # convert plain collections
            # get exactly in this scope (no enclosing)
            o = self.attributes.get(name, None)
            if o is None: # new attribute
                self.rawSetAttribute(self.attributes, name, value)
                return

            # it will be a multi-value attribute
            if isinstance(o, STAttributeList): # already a list made by ST
                v = o

            elif isinstance(o, list): # existing attribute is non-ST List
                # must copy to an ST-managed list before adding new attribute
                v = STAttributeList()
                v.extend(o)
                self.rawSetAttribute(self.attributes, name, v); # replace attribute w/list

            else:
                # non-list second attribute, must convert existing to ArrayList
                v = STAttributeList() # make list to hold multiple values
                # make it point to list now
                self.rawSetAttribute(self.attributes, name, v); # replace attribute w/list
                v.append(o)  # add previous single-valued attribute

            if isinstance(value, list):
                # flatten incoming list into existing
                if v!=value: # avoid weird cyclic add
                    v.extend(value)

            else:
                v.append(value)

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
    #  and the aggrName. Space is allowed around ','
    #
    def parseAggregateAttributeSpec(self, aggrSpec):
        dot = aggrSpec.find('.')
        if dot <= 0:
            raise ValueError('invalid aggregate attribute format: ' + aggrSpec)
        aggrName = aggrSpec[:dot].strip()
        propString = aggrSpec[dot+1:]
        propString = [
            p.strip()
            for p in propString.split('{',2)[-1].split('}',2)[0].split(',')
            ]

        return aggrName, propString

    ## Map a value to a named attribute.  Throw KeyError if
    #  the named attribute is not formally defined in self's specific template
    #  and a formal argument list exists.
    #
    def rawSetAttribute(self, attributes, name, value):
        if self.formalArguments != FormalArgument.UNKNOWN and \
           not self.hasFormalArgument(name):
            # a normal call to setAttribute with unknown attribute
            raise KeyError("no such attribute: " + name +
               " in template context " + self.getEnclosingInstanceStackString())
        if value:
            attributes[name] = value
        elif isinstance(value, list) or \
             isinstance(value, dict) or \
             isinstance(value, set):
            attributes[name] = value

    ## Argument evaluation such as foo(x=y), x must
    #  be checked against foo's argument list not this's (which is
    #  the enclosing context).  So far, only eval.g uses arg self as
    #  something other than "this".
    #
    def rawSetArgumentAttribute(self, embedded, attributes, name, value):
        if embedded.formalArguments != FormalArgument.UNKNOWN and \
           not embedded.hasFormalArgument(name):
            raise KeyError("template " + embedded.getName() +
	        " has no such attribute: " + name + " in template context " +
		self.getEnclosingInstanceStackString())
        if value:
            attributes[name] = value
        elif isinstance(value, list) or \
             isinstance(value, dict) or \
             isinstance(value, set):
            attributes[name] = value

    ## Walk the chunks, asking them to write themselves out according
    #  to attribute values of 'self.attributes'.  This is like evaluating or
    #  interpreting the StringTemplate as a program using the
    #  attributes.  The chunks will be identical (point at same list)
    #  for all instances of self template.
    #
    def write(self, out):
        if self.group.debugTemplateOutput:
            self.emitTemplateStartDebugString(self, out)
            
        n = 0
        self.setPredefinedAttributes()
	self.setDefaultArgumentValues()
        if self.chunks:
	    i = 0
	    while i < len(self.chunks):
	        a = self.chunks[i]
                chunkN = a.write(self, out)
                
                # expr-on-first-line-with-no-output NEWLINE => NEWLINE
                if ( chunkN == 0 and
                     i == 0 and
                     i + 1 < len(self.chunks) and
                     isinstance(self.chunks[i+1], NewlineRef) ):
                    # skip next NEWLINE
                    i += 2 # skip *and* advance!
                    continue
                
                # NEWLINE expr-with-no-output NEWLINE => NEWLINE
                # Indented $...$ have the indent stored with the ASTExpr
                # so the indent does not come out as a StringRef
                if (not chunkN) and (i-1) >= 0 and \
                   isinstance(self.chunks[i-1], NewlineRef) and \
                   (i+1) < len(self.chunks) and \
                   isinstance(self.chunks[i+1], NewlineRef):
                    #sys.stderr.write('found pure \\n blank \\n pattern\n')
                    i += 1 # make it skip over the next chunk, the NEWLINE
                n += chunkN
		i += 1
                
        if self.group.debugTemplateOutput:
            self.emitTemplateStopDebugString(self, out)

        if StringTemplate.lintMode:
            self.checkForTrouble()
            
        return n

    ## Resolve an attribute reference.  It can be in four possible places:
    #
    #  1. the attribute list for the current template
    #  2. if self is an embedded template, somebody invoked us possibly
    #     with arguments--check the argument context
    #  3. if self is an embedded template, the attribute list for the enclosing
    #     instance (recursively up the enclosing instance chain)
    #  4. if nothing is found in the enclosing instance chain, then it might
    #     be a map defined in the group or the its supergroup etc...
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

        if (not o) and \
           (not this.passThroughAttributes) and \
           this.hasFormalArgument(attribute):
            # if you've defined attribute as formal arg for self
            # template and it has no value, do not look up the
            # enclosing dynamic scopes.  This avoids potential infinite
            # recursion.
            return None

        # not locally defined, check enclosingInstance if embedded
        if (not o) and this.enclosingInstance:
            #sys.stderr.write('looking for ' + self.getName() + '.' + \
            #                 str(attribute) + ' in super [=' + \
            #                 this.enclosingInstance.getName() + ']\n')
            valueFromEnclosing = self.get(this.enclosingInstance, attribute)
            if not valueFromEnclosing:
                self.checkNullAttributeAgainstFormalArguments(this, attribute)
            o = valueFromEnclosing

	# not found and no enclosing instance to look at
	elif (not o) and (not this.enclosingInstance):
	    # It might be a map in the group or supergroup...
	    o = this.group.getMap(attribute)

        return o


    def getAttribute(self, name):
        #sys.stderr.write(self.getName() + '.getAttribute(' + str(name) + ')\n')
        return self.get(self, name)

    __getitem__ = getAttribute


    def breakTemplateIntoChunks(self):
        """
        Walk a template, breaking it into a list of
        chunks: Strings and actions/expressions.
        """

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
            chunkStream.this = self
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
        lexer = ActionLexer.Lexer(StringIO(str(action)))
        parser = ActionParser.Parser(lexer, self)
        parser.setASTNodeClass(StringTemplateAST)
        lexer.setTokenObjectClass(StringTemplateToken)
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

    def getFormalArgumentKeys(self):
        return self.formalArgumentKeys

    def getFormalArguments(self):
        return self.formalArguments

    def setFormalArguments(self, args):
        self.formalArguments = args

    ## Set any default argument values that were not set by the
    #  invoking template or by setAttribute directly.  Note
    #  that the default values may be templates.  Their evaluation
    #  context is the template itself and, hence, can see attributes
    #  within the template, any arguments, and any values inherited
    #  by the template.
    #
    #  Default values are stored in the argument context rather than
    #  the template attributes table just for consistency's sake.
    #
    def setDefaultArgumentValues(self):
        if not self.numberOfDefaultArgumentValues:
            return
        if not self.argumentContext:
            self.argumentContext = {}
        if self.formalArguments != FormalArgument.UNKNOWN:
            argNames = self.formalArgumentKeys
            for argName in argNames:
                # use the default value then
                arg = self.formalArguments[argName]
                if arg.defaultValueST:
                    existingValue = self.getAttribute(argName)
                    if not existingValue: # value unset?
                        # if no value for attribute, set arg context
                        # to the default value.  We don't need an instance
                        # here because no attributes can be set in
                        # the arg templates by the user.
                        self.argumentContext[argName] = arg.defaultValueST

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
        self.formalArgumenttKeys = []
        self.formalArguments = {}

    def defineFormalArgument(self, names, defaultValue = None):
        if not names:
	    return
        if isinstance(names, basestring):
            name = names
	    if defaultValue:
	        self.numberOfDefaultArgumentValues += 1
            a = FormalArgument(name, defaultValue)
            if self.formalArguments == FormalArgument.UNKNOWN:
        	self.formalArguments = {}
            self.formalArgumentKeys = [name]
            self.formalArguments[name] = a
	elif isinstance(names, list):
	    for name in names:
        	a = FormalArgument(name, defaultValue)
        	if self.formalArguments == FormalArgument.UNKNOWN:
                    self.formalArgumentKeys = []
        	    self.formalArguments = {}
                self.formalArgumentKeys.append(name)
        	self.formalArguments[name] = a
	        
    ## Normally if you call template y from x, y cannot see any attributes
    #  of x that are defined as formal parameters of y.  Setting this
    #  passThroughAttributes to true, will override that and allow a
    #  template to see through the formal arg list to inherited values.
    #
    def setPassThroughAttributes(self, passThroughAttributes):
        self.passThroughAttributes = passThroughAttributes

    ## Specify a complete map of what object classes should map to which
    #  renderer objects.
    #
    def setAttributeRenderers(self, renderers):
        self.attributeRenderers = renderers

    ## Register a renderer for all objects of a particular type.  This
    #  overrides any renderer set in the group for this class type.
    #
    def registerRenderer(self, attributeClassType, renderer):
        if not self.attributeRenderers:
            self.attributeRenderers = {}
        self.attributeRenderers[attributeClassType] = renderer

    ## What renderer is registered for this attributeClassType for
    #  this template.  If not found, the template's group is queried.
    #
    def getAttributeRenderer(self, attributeClassType):
        renderer = None
        if self.attributeRenderers is not None:
            renderer = self.attributeRenderers.get(attributeClassType, None)

        if renderer is not None:
            # found it
            return renderer

        # we have no renderer overrides for the template or none for class arg
        # check parent template if we are embedded
        if self.enclosingInstance is not None:
            return self.enclosingInstance.getAttributeRenderer(attributeClassType)

        # else check group
        return self.group.getAttributeRenderer(attributeClassType)


# ----------------------------------------------------------------------------
#                      U t i l i t y  R o u t i n e s
# ----------------------------------------------------------------------------

    def warning(self, msg):
        if self.getErrorListener():
            self.getErrorListener().warning(msg)
        else:
            sys.stderr.write('StringTemplate: warning: ' + msg)

    def error(self, msg, e = None):
        if self.getErrorListener():
            self.getErrorListener().error(msg, e)
        elif e:
            sys.stderr.write('StringTemplate: error: ' + msg + ': ' +
                             str(e))
            traceback.print_exc()
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
        return '<' + self.getName() + '(' + str(self.formalArgumentKeys) + \
               ')' + '@' + str(self.getTemplateID()) + '>'

    def getTemplateHeaderString(self, showAttributes):
        if showAttributes and self.attributes is not None:
            return self.getName() + str(self.attributes.keys())

        return self.getName()

    ## A reference to an attribute with no value, must be compared against
    #  the formal parameter to see if it exists; if it exists all is well,
    #  but if not, throw an exception.
    #
    #  Don't do the check if no formal parameters exist for self template
    #  ask enclosing.
    #
    def checkNullAttributeAgainstFormalArguments(self, this, attribute):
        if this.getFormalArguments() == FormalArgument.UNKNOWN:
            # bypass unknown arg lists
            if this.enclosingInstance:
                self.checkNullAttributeAgainstFormalArguments(
                    this.enclosingInstance, attribute)
        else:
            formalArg = this.lookupFormalArgument(attribute)
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
               not name in self.referencedAttributes:
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

    def isRegion(self):
        return self._isRegion

    def setIsRegion(self, isRegion):
        self._isRegion = isRegion

    def addRegionName(self, name):
        self.regions.add(name)

    def containsRegionName(self, name):
        return name in self.regions

    def getRegionDefType(self):
        return self.regionDefType

    def setRegionDefType(self, regionDefType):
        self.regionDefType = regionDefType
        
    def toDebugString(self):
        buf = StringIO()
        buf.write('template-' + self.getTemplateDeclaratorString() + ': ')
        buf.write('chunks=')
	if self.chunks:
            buf.write(str(chunks))
        buf.write('attributes=[')
	if self.attributes:
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

    def toStructureString(self, indent=0):
        """
        Don't print values, just report the nested structure with attribute names.
        Follow (nest) attributes that are templates only.
        """

        buf = StringIO()

        buf.write('  '*indent)  # indent
        buf.write(self.getName())
        buf.write(str(self.attributes.keys())) # FIXME: errr.. that's correct?
        buf.write(":\n")
        if self.attributes is not None:
            attrNames = self.attributes.keys()
            for name in attrName:
                value = self.attributes[name]
                if isinstance(value, StringTemplate): # descend
                    buf.write(value.toStructureString(indent+1))

                else:
                    if isistance(value, list):
                        for o in value:
                            if isinstance(o, StringTemplate): # descend
                                buf.write(o.toStructureString(indent+1))

                    elif isinstance(value, dict):
                        for o in value.values():
                            if isinstance(o, StringTemplate): # descend
                                buf.write(o.toStructureString(indent+1))

        return buf.getvalue()

    def getDOTForDependencyGraph(self, showAttributes):
        """
        Generate a DOT file for displaying the template enclosure graph; e.g.,
        digraph prof {
            "t1" -> "t2"
            "t1" -> "t3"
            "t4" -> "t5"
        }
        """
        structure = (
            "digraph StringTemplateDependencyGraph {\n" +
            "node [shape=$shape$, $if(width)$width=$width$,$endif$" +
            "      $if(height)$height=$height$,$endif$ fontsize=$fontsize$];\n" +
            "$edges:{e|\"$e.src$\" -> \"$e.trg$\"\n}$" +
           "}\n"
            )
        
        graphST = StringTemplate(structure)
        edges = {}
        self.getDependencyGraph(edges, showAttributes)
        # for each source template
        for src, targetNodes in edges.iteritems():
            # for each target template
            for trg in targetNodes:
                graphST.setAttribute("edges.{src,trg}", src, trg)

        graphST.setAttribute("shape", "none");
        graphST.setAttribute("fontsize", "11");
        graphST.setAttribute("height", "0"); # make height
        return graphST


    def getDependencyGraph(self, edges, showAttributes):
        """
        Get a list of n->m edges where template n contains template m.
        The map you pass in is filled with edges: key->value.  Useful
        for having DOT print out an enclosing template graph. It
        finds all direct template invocations too like <foo()> but not
        indirect ones like <(name)()>.

        Ack, I just realized that this is done statically and hence
        cannot see runtime arg values on statically included templates.
        Hmm...someday figure out to do this dynamically as if we were
        evaluating the templates.  There will be extra nodes in the tree
        because we are static like method and method[...] with args.
        """
        
        srcNode = self.getTemplateHeaderString(showAttributes)
        if self.attributes is not None:
            for name, value in self.attributes.iteritems():
                if isinstance(value, StringTemplate):
                    targetNode = value.getTemplateHeaderString(showAttributes)
                    self.putToMultiValuedMap(edges,srcNode,targetNode)
                    value.getDependencyGraph(edges,showAttributes) # descend

                else:
                    if isinstance(value, list):
                        for o in value:
                            if isinstance(o, StringTemplate):
                                targetNode = o.getTemplateHeaderString(showAttributes)
                                self.putToMultiValuedMap(edges,srcNode,targetNode)
                                o.getDependencyGraph(edges,showAttributes) # descend

                    elif isinstance(value, dict):
                        for o in value.values():
                            if isinstance(o, StringTemplate):
                                targetNode = o.getTemplateHeaderString(showAttributes)
                                self.putToMultiValuedMap(edges,srcNode,targetNode)
                                o.getDependencyGraph(edges,showAttributes) # descend

        # look in chunks too for template refs
        for chunk in self.chunks:
            if not isinstance(chunk, ASTExpr):
                continue

            from stringtemplate3.language.ActionEvaluator import INCLUDE
            tree = expr.getAST()
            includeAST = CommonAST(
                CommonToken(INCLUDE,"include")
                )
            
            for t in tree.findAllPartial(includeAST):
                templateInclude = t.getFirstChild().getText()
                #System.out.println("found include "+templateInclude);
                self.putToMultiValuedMap(edges,srcNode,templateInclude)
                group = self.getGroup()
                if group is not None:
                    st = group.getInstanceOf(templateInclude)
                    # descend into the reference template
                    st.getDependencyGraph(edges, showAttributes)


    def putToMultiValuedMap(self, map, key, value):
        """Manage a hash table like it has multiple unique values."""

        try:
            map[key].append(value)
        except KeyError:
            map[key] = [value]


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


    def toString(self, lineWidth=StringTemplateWriter.NO_WRAP):
        # Write the output to a StringIO
        out = StringIO()
        wr = self.group.getStringTemplateWriter(out)
        wr.setLineWidth(lineWidth)
        try:
            self.write(wr)
        except IOError, io:
            self.error("Got IOError writing to writer" + \
	               str(wr.__class__.__name__))
            
        # reset so next toString() does not wrap; normally this is a new writer
 	# each time, but just in case they override the group to reuse the
 	# writer.
        wr.setLineWidth(StringTemplateWriter.NO_WRAP)
        
        return out.getvalue()

    __str__ = toString
    

# Set class data attribute values.
StringTemplate.VERSION = "3.1b1"
StringTemplate.ANONYMOUS_ST_NAME = "anonymous"
StringTemplate.lintMode = False
StringTemplate.templateCounter=0
StringTemplate.hideProperties = \
['argumentContext', 'argumentsAST', 'attributeRenderers', 'attributes',
'chunks', 'enclosingInstance', 'enclosingInstanceStackTrace', 'errorListener',
'formalArgumentKeys', 'formalArguments', 'group', 'instanceOf', 'name',
'nextTemplateCounter', 'outermostName', 'template', 'templateDeclaratorString',
'templateID']
# <@r()>
StringTemplate.REGION_IMPLICIT = 1
# <@r>...<@end>
StringTemplate.REGION_EMBEDDED = 2
# @t.r() ::= "..." defined manually by coder
StringTemplate.REGION_EXPLICIT = 3

class StringTemplateGroupErrorListener(StringTemplateErrorListener):

    def error(self, s, e):
        sys.stderr.write(s + '\n')
        if e:
            traceback.print_exc()

    def warning(self, s):
        sys.stderr.write(s + '\n')


class StringTemplateGroupLoader(object):
    """
    When group files derive from another group, we have to know how to
    load that group and its supergroups. This interface also knows how
    to load interfaces
    """

    def loadGroup(self, groupName, superGroup=None):
        """
        Load the group called groupName from somewhere.  Return null
        if no group is found.
        Groups with region definitions must know their supergroup to find
        templates during parsing.
        """
        
        raise NotImplementedError


    def loadInterface(self, interfaceName):
        """
        Load the interface called interfaceName from somewhere.  Return null
        if no interface is found.
        """

        raise NotImplementedError
    

class PathGroupLoader(StringTemplateGroupLoader):
    """
    A brain dead loader that looks only in the directory(ies) you
    specify in the ctor.
    You may specify the char encoding.
    """
    
    def __init__(self, dir=None, errors=None):
        """
        Pass a single dir or multiple dirs separated by colons from which
        to load groups/interfaces.
        """
        
        super(StringTemplateGroupLoader, self).__init__()

        ## List of ':' separated dirs to pull groups from
        self.dirs = dir.split(':')
        self.errors = errors

        ## How are the files encoded (ascii, UTF8, ...)?
        #  You might want to read UTF8 for example on an ascii machine.
        self.fileCharEncoding = sys.getdefaultencoding()
        

    def loadGroup(self, groupName, superGroup=None):
        try:
            fr = self.locate(groupName+".stg")
            if fr is None:
                self.error("no such group file "+groupName+".stg")
                return None

            try:
                return StringTemplateGroup(fr, None, self.errors, superGroup)
            finally:
                fr.close()

        except IOError, ioe:
            self.error("can't load group "+groupName, ioe)

        return None


    def loadInterface(self, interfaceName):
        try:
            fr = self.locate(interfaceName+".sti")
            if fr is None:
                self.error("no such interface file "+interfaceName+".sti")
                return None

            try:
                return StringTemplateGroupInterface(fr, self.errors)

            finally:
                fr.close()
                
        except (IOError, OSError), ioe:
            self.error("can't load interface "+interfaceName, ioe)

        return None


    def locate(self, name):
        """Look in each directory for the file called 'name'."""
        
        for dir in self.dirs:
            path = os.path.join(dir, name)
            if os.path.isfile(path):
                fr = open(path, 'r')
                # FIXME: something breaks, when stream return unicode
                #if self.fileCharEncoding is not None:
                #    fr = codecs.getreader(self.fileCharEncoding)(fr)
                return fr

        return None
    
            
    def getFileCharEncoding(self):
        return self.fileCharEncoding


    def setFileCharEncoding(self, fileCharEncoding):
        self.fileCharEncoding = fileCharEncoding


    def error(self, msg, exc=None):
        if self.errors is not None:
            self.errors.error(msg, exc)
            
        else:
            sys.stderr.wrote("StringTemplate: "+msg+"\n")
            if exc is not None:
                traceback.print_exc()


class CommonGroupLoader(PathGroupLoader):
    """
    Subclass o PathGroupLoader that also works, if the package is
    packaged in a zip file.
    FIXME: this is not yet implemented, behaviour is identical to
    PathGroupLoader!
    """

    # FIXME: this needs to be overridden!
    def locate(self, name):
        """Look in each directory for the file called 'name'."""

        return PathGroupLoader.locate(self, name)


class TemplateDefinition(object):
    """All the info we need to track for a template defined in an interface"""

    def __init__(self, name, formalArgs, optional):
        self.name = name
        self.formalArgs = formalArgs
        self.optional = optional


class StringTemplateGroupInterface(object):
    """
    A group interface is like a group without the template implementations;
    there are just template names/argument-lists like this:

    interface foo;
    class(name,fields);
    method(name,args,body);
    """

    def __init__(self, r, errors=DEFAULT_ERROR_LISTENER, superInterface=None):
        """Create an interface from the input stream"""

	## What is the group name
	self.name = None

	## Maps template name to TemplateDefinition object
        self.templates = {}

	## Are we derived from another group?  Templates not found in this
        # group will be searched for in the superGroup recursively.
        self.superInterface = superInterface

	## Where to report errors.  All string templates in this group
        # use this error handler by default.
        self.listener = errors

        self.parseInterface(r)


    def getSuperInterface(self):
        return self.superInterface


    def setSuperInterface(self, superInterface):
        self.superInterface = superInterface


    def parseInterface(self, r):
        try:
            lexer = InterfaceLexer.Lexer(r)
            parser = InterfaceParser.Parser(lexer)
            parser.groupInterface(self)

        except RuntimeError, exc: #FIXME:  Exception
            name = self.name or "<unknown>";
            self.error("problem parsing group "+name+": "+str(exc), exc)


    def defineTemplate(self, name, formalArgs, optional):
        d = TemplateDefinition(name,formalArgs,optional)
        self.templates[d.name] = d


    def getMissingTemplates(self, group):
	"""
        Return a list of all template names missing from group that are defined
        in this interface.  Return null if all is well.
        """

        missing = []

        templates = self.templates.items()
        templates.sort()
        for name, d in templates:
            if not d.optional and not group.isDefined(d.name):
                missing.append(d.name)

        return missing or None


    def getMismatchedTemplates(self, group):
	"""
        Return a list of all template sigs that are present in the group, but
        that have wrong formal argument lists.  Return null if all is well.
        """

        mismatched = []
        
        templates = self.templates.items()
        templates.sort()
        for name, d in templates:
            if group.isDefined(d.name):
                defST = group.getTemplateDefinition(d.name)
                formalArgs = defST.getFormalArguments()
                ack = False
                if ( (d.formalArgs is not None and formalArgs is None) or
                     (d.formalArgs is None and formalArgs is not None) or
                     (len(d.formalArgs) != len(formalArgs)) ):
                    ack = True

                if not ack:
                    for argName in formalArgs.keys():
                        if d.formalArgs.get(argName, None) == None:
                            ack = True
                            break

                if ack:
                    mismatched.append(self.getTemplateSignature(d))

        return mismatched or None


    def getName(self):
        return self.name


    def setName(self, name):
        self.name = name


    def error(self, msg, exc=None):
        if self.listener is not None:
            self.listener.error(msg, exc)


    def toString(self):
        buf = StringIO()
        buf.write("interface ")
        buf.write(self.name)
        buf.write(";\n")
        templates = self.templates.items()
        templates.sort()
        for name, d in templates:
            buf.write( self.getTemplateSignature(d) )
            buf.write(";\n")

        return buf.getvalue()
    
    __str__ = toString

    
    def getTemplateSignature(self, d):
        buf = StringIO()
        if d.optional:
            buf.write("optional ")

        buf.write(d.name)
        if d.formalArgs is not None:
            buf.write('(')
            args = d.formalArgs.keys()
            args.sort()
            buf.write(", ".join(args))
            buf.write(')')

        else:
            buf.write("()")

        return buf.getvalue()


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
#  You can use the group file format also to define a group of templates
#  (this works better for code gen than for html page gen).  You must give
#  a Reader to the ctor for it to load the group; this is general and
#  distinguishes it from the ctors for the old-style "load template files
#  from the disk".
#
#  10/2005 I am adding a StringTemplateGroupLoader concept so people can define
#  supergroups within a group and have it load that group automatically.
class StringTemplateGroup(object):

    ## Track all groups by name; maps name to StringTemplateGroup
    nameToGroupMap = {}

    ## Track all interfaces by name; maps name to StringTemplateGroupInterface
    nameToInterfaceMap = {}
    
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

    ## If a group file indicates it derives from a supergroup, how do we
    #  find it?  Shall we make it so the initial StringTemplateGroup file
    #  can be loaded via this loader?  Right now we pass a Reader to ctor
    #  to distinguish from the other variety.
    groupLoader = None

    ## You can set the lexer once if you know all of your groups use the
    #  same separator.  If the instance has templateLexerClass set
    #  then it is used as an override.
    defaultTemplateLexerClass = DefaultTemplateLexer.Lexer
        
    def _initialize(self):      
        ## What is the group name
        #
        self.name = None
        ## Maps template name to StringTemplate object
        #
        self.templates = {}
        ## Maps map names to HashMap objects.  This is the list of maps
        #  defined by the user like typeInitMap ::= ["int":"0"]
        #
        self.maps = {}

        ## How to pull apart a template into chunks?
        self.templateLexerClass = None
        
        ## Under what directory should I look for templates?  If None,
        #  to look into the CLASSPATH for templates as resources.
        #
        self.rootDir = None

        ## Are we derived from another group?  Templates not found in this
        #  group will be searched for in the superGroup recursively.
        self.superGroup = None

        ## Keep track of all interfaces implemented by this group.
        self.interfaces = []
        
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

        self.debugTemplateOutput = False

        ## The set of templates to ignore when dumping start/stop debug strings
        self.noDebugStartStopStrings = None
        
        ## A Map<class,object> that allows people to register a renderer for
        #  a particular kind of object to be displayed for any template in this
        #  group.  For example, a date should be formatted differently depending
        #  on the locale.  You can set Date.class to an object whose
        #  str() method properly formats a Date attribute
        #  according to locale.  Or you can have a different renderer object
        #  for each locale.
        #
        #  These render objects are used way down in the evaluation chain
        #  right before an attribute's str() method would normally be
        #  called in ASTExpr.write().
        #
        self.attributeRenderers = None

        ## Where to report errors.  All string templates in this group
        #  use this error handler by default.
        #
        self.listener = StringTemplateGroup.DEFAULT_ERROR_LISTENER
        ## How long before tossing out all templates in seconds.
        #  default: no refreshing from disk
        #
        self.refreshIntervalInSeconds = sys.maxint/1000
        self.lastCheckedDisk = 0L
        
        ## How are the files encoded (ascii, UTF8, ...)?  You might want to
        #  read UTF8 for example on an ascii machine.
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
    #  t1(args) ::= "..."
    #  t2() ::= <<
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
        if len(args) > 0 and isinstance(args[0], basestring):
            self.name = args[0]
            self.lastCheckedDisk = time.time()
            StringTemplateGroup.nameToGroupMap[self.name] = self
            if len(args) > 1 and isinstance(args[1], basestring):
                self.rootDir = args[1]
                self.templateLexerClass = DefaultTemplateLexer.Lexer
                if len(args) > 2 and type(args[2]) == type(antlr.CharScanner):
                    self.templateLexerClass = args[2]
            elif len(args) > 1 and type(args[1]) == type(antlr.CharScanner):
                self.templateLexerClass = args[1]
                
        elif len(args) > 0 and (isinstance(args[0], file) or
                                isinstance(args[0], StringIO)):
            r = args[0]
            self.templatesDefinedInGroupFile = True
            self.templateLexerClass = AngleBracketTemplateLexer.Lexer
            if len(args) > 1 and type(args[1]) == type(antlr.CharScanner):
                self.templateLexerClass = args[1]
                if len(args) > 2 and isinstance(args[2], StringTemplateErrorListener):
                    if args[2] is not None:
                        self.listener = args[2]
                if len(args) > 3 and isinstance(args[3], StringTemplateGroup):
                    self.setSuperGroup(args[3])
            elif len(args) > 1 and isinstance(args[1], StringTemplateErrorListener):
                if args[1] is not None:
                    self.listener = args[1]

            self.parseGroup(r)
            self.verifyInterfaceImplementations()
            
    def getTemplateLexerClass(self):
        """
        What lexer class to use to break up templates.  If not lexer set
        for this group, use static default.
        """

        if self.templateLexerClass is not None:
            return self.templateLexerClass

        return self.defaultTemplateLexerClass

    def getName(self):
        return self.name

    def setName(self, name):
        self.name = name

    def setSuperGroup(self, superGroup):
        if isinstance(superGroup, StringTemplateGroup):
            self.superGroup = superGroup
            
        elif isinstance(superGroup, basestring):
            group = StringTemplateGroup.nameToGroupMap.get(superGroup, None)
            if group is not None:
                # we've seen before; just use it
                self.superGroup = group

            else:
                # else load it
                group = self.loadGroup(superGroup)
                if group is not None:
                    StringTemplateGroup.nameToGroupMap[superGroup] = group
                    self.superGroup = group

                elif self.groupLoader is None:
                    self.listener.error("no group loader registered", None)

                    
    def getSuperGroup(self):
        return self.superGroup

    def getRootDir(self):
        return self.rootDir

    def setRootDir(self, rootDir):
        self.rootDir = rootDir

    def implementInterface(self, interface):
        """
        Indicate that this group implements this interface.
        Load if necessary if not in the nameToInterfaceMap.
        """

        if isinstance(interface, StringTemplateGroupInterface):
            self.interfaces.append(interface)

        else:
            interfaceName = interface

            interface = self.nameToInterfaceMap.get(interfaceName, None)
            if interface is not None:
                # we've seen before; just use it
                self.interfaces.append(interface)
                return

            # else load it
            interface = self.loadInterface(interfaceName)
            if interface is not None:
                self.nameToInterfaceMap[interfaceName] = interface
                self.interfaces.append(interface)

            elif self.groupLoader is None:
                self.listener.error("no group loader registered", None)

    ## StringTemplate object factory; each group can have its own.
    def createStringTemplate(self):
        return StringTemplate()

    # FIXME: signature should be
    # name, enclosingTemplate=None, attributes=None
    # with
    # if attributes is not None:
    #     st.attributes = attributes
    def getInstanceOf(self, *args):
        """
        A support routine that gets an instance of name knowing which
        ST encloses it for error messages.
        """
        try:
            enclosingInstance, name = args
        except ValueError:
            enclosingInstance, name = None, args[0]
            
        st = self.lookupTemplate(enclosingInstance, name)
        if st is not None:
            return st.getInstanceOf()
        return None

    def getEmbeddedInstanceOf(self, enclosingInstance, name):
        st = None
        # TODO: seems like this should go into lookupTemplate
        if name.startswith("super."):
            # for super.foo() refs, ensure that we look at the native
            # group for the embedded instance not the current evaluation
            # group (which is always pulled down to the original group
            # from which somebody did group.getInstanceOf("foo");
            st = enclosingInstance.getNativeGroup().getInstanceOf(
                enclosingInstance, name
                )
            
        else:
            st = self.getInstanceOf(enclosingInstance, name)

        # make sure all embedded templates have the same group as enclosing
        # so that polymorphic refs will start looking at the original group
        st.setGroup(self)
        st.setEnclosingInstance(enclosingInstance)
        return st

    ## Get the template called 'name' from the group.  If not found,
    #  attempt to load.  If not found on disk, then try the superGroup
    #  if any.  If not even there, then record that it's
    #  NOT_FOUND so we don't waste time looking again later.  If we've gone
    #  past refresh interval, flush and look again.
    #
    #  If I find a template in a super group, copy an instance down here
    def lookupTemplate(self, *args):
        try:
            enclosingInstance, name = args
        except ValueError:
            enclosingInstance, name = None, args[0]
            
        if name.startswith('super.'):
            if self.superGroup:
                dot = name.find('.')
                name = name[dot+1:]
                return self.superGroup.lookupTemplate(enclosingInstance, name)
            raise ValueError(self.getName() + ' has no super group; ' +
                             'invalid template: ' + name)
        self.checkRefreshInterval()
        st = self.templates.get(name, None)
        if not st:
            # not there?  Attempt to load
            if not self.templatesDefinedInGroupFile:
                # only check the disk for individual template
                st = self.loadTemplateFromBeneathRootDir(self.getFileNameFromTemplateName(name))
            if (not st) and self.superGroup:
                # try to resolve in super group
                st = self.superGroup.getInstanceOf(name)
                # make sure that when we inherit a template, that it's
                # group is reset; it's nativeGroup will remain where it was
                if st is not None:
                    st.setGroup(self)

            if st: # found in superGroup
                # insert into this group; refresh will allow super
                # to change it's def later or this group to add
                # an override.
                self.templates[name] = st
                
            else:
                # not found; remember that this sucker doesn't exist
                self.templates[name] = StringTemplateGroup.NOT_FOUND_ST
                context = ""
                if enclosingInstance is not None:
                    context = (
                        "; context is "+
                        enclosingInstance.getEnclosingInstanceStackString()
                        )
                raise ValueError(
                    "Can't load template " +
                    self.getFileNameFromTemplateName(name) +
                    context
                    )

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
        template = None
        name = self.getTemplateNameFromFileName(fileName)
        # if no rootDir, try to load as a resource in CLASSPATH
        # In the Python case that is of course the sys.path
        if not self.rootDir:
            try:
                br, pathName, descr = imp.find_module(name)
            except ImportError:
                br = None
            if br is None:
                return None

            try:
                try:
                    template = self.loadTemplate(name, br)
                except IOException, ioe:
                    self.error("Problem reading template file: "+fileName, ioe)

            finally:
                try:
                    br.close()
                except IOError, ioe2:
                    self.error('Cannot close template file: ' + pathName, ioe2)

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
        if isinstance(file_, basestring):
            br = None
            template = None
            try:
                br = file(file_)
                template = self.loadTemplate(name, br)
                br.close()
                br = None
            # FIXME: eek, that's ugly
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
        if name is not None and '.' in name:
            raise ValueError("cannot have '.' in template names")
        
        st = self.createStringTemplate()
        st.setName(name)
        st.setGroup(self)
        st.setNativeGroup(self)
        st.setTemplate(template)
        st.setErrorListener(self.listener)
        self.templates[name] = st
        return st

    def defineRegionTemplate(self, enclosingTemplate, regionName, template, type):
        """Track all references to regions <@foo>...<@end> or <@foo()>."""
        
        if isinstance(enclosingTemplate, StringTemplate):
            enclosingTemplateName = self.getMangledRegionName(
                enclosingTemplate.getOutermostName(),
                regionName
                )
            enclosingTemplate.getOutermostEnclosingInstance().addRegionName(regionName)

        else:
            enclosingTemplateName = self.getMangledRegionName(enclosingTemplate, regionName)

        regionST = self.defineTemplate(enclosingTemplateName, template)
        regionST.setIsRegion(True)
        regionST.setRegionDefType(type)
        return regionST

    def defineImplicitRegionTemplate(self, enclosingTemplate, name):
        """
        Track all references to regions <@foo()>.  We automatically
        define as

        @enclosingtemplate.foo() ::= ""

        You cannot set these manually in the same group; you have to subgroup
        to override.
        """
        return self.defineRegionTemplate(
            enclosingTemplate,
            name,
            "",
            StringTemplate.REGION_IMPLICIT
            )

    def getMangledRegionName(self, enclosingTemplateName, name):
        """
        The 'foo' of t() ::= '<@foo()>' is mangled to 'region__t__foo'
        """
        return "region__"+enclosingTemplateName+"__"+name

    
    def getUnMangledTemplateName(self, mangledName):
        """
        Return "t" from "region__t__foo"
        """
        return mangledName[len("region__"):mangledName.rindex("__")]


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
        st = self.templates.get(name, None)
        if st is not None:
            if st.isRegion():
                # don't allow redef of @t.r() ::= "..." or <@r>...<@end>
                if st.getRegionDefType() == StringTemplate.REGION_IMPLICIT:
                    return False
                
            return True
        
        return False

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
            return self.lookupTemplate(name) is not None
        except ValueError:
            return False

    def parseGroup(self, r):
        try:
            lexer = GroupLexer.Lexer(r)
            parser = GroupParser.Parser(lexer)
            parser.group(self)
            # sys.stderr.write("read group\n" + str(self))
        except "foo", e: # FIXME: Exception, e:
            name = "<unknown>"
            if self.getName():
                name = self.getName()
            self.error('problem parsing group ' + name + ': ' + str(e), e)

    def verifyInterfaceImplementations(self):
        """verify that this group satisfies its interfaces"""

        for interface in self.interfaces:
            missing = interface.getMissingTemplates(self)
            mismatched = interface.getMismatchedTemplates(self)
            
            if missing:
                self.error(
                    "group " + self.getName() +
                    " does not satisfy interface " + interface.name +
                    ": missing templates [" + ', '.join(["'%s'" % m for m in missing]) + ']'
                    )

            if mismatched:
                self.error(
                    "group " + self.getName() +
                    " does not satisfy interface " + interface.getName() +
                    ": mismatched arguments on these templates [" + ', '.join(["'%s'" % m for m in mismatched]) + ']'
                    )


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
            except RuntimeError, e: #FIXME Exception, e:
                self.error('problems getting StringTemplateWriter',e)

        if not stw:
            stw = AutoIndentWriter(w)
        return stw

    ## Specify a complete map of what object classes should map to which
    #  renderer objects for every template in this group (that doesn't
    #  override it per template).
    #
    def setAttributeRenderers(self, renderers):
        self.attributeRenderers = renderers

    ## Register a renderer for all objects of a particular type for all
    #  templates in this group.
    #
    def registerRenderer(self, attributeClassType, renderer):
        if not self.attributeRenderers:
            self.attributeRenderers = {}
        self.attributeRenderers[attributeClassType] = renderer

    ## What renderer is registered for this attributeClassType for
    #  this group?  If not found, as superGroup if it has one.
    #
    def getAttributeRenderer(self, attributeClassType):
        if not self.attributeRenderers:
            if not self.superGroup:
                return None # no renderers and no parent?  Stop.
            # no renderers; consult super group
            return self.superGroup.getAttributeRenderer(attributeClassType)

        if self.attributeRenderers.has_key(attributeClassType):
            renderer = self.attributeRenderers[attributeClassType]
        else:
            # no renderer registered for this class, check super group
            renderer = self.superGroup.getAttributeRenderer(attributeClassType)
        return renderer

    def getMap(self, name):
        if not self.maps:
            if not self.superGroup:
                return None
            return self.superGroup.getMap(name)
        m = None
        if self.maps.has_key(name):
            m = self.maps[name]
        if (not m) and self.superGroup:
            m = self.superGroup.getMap(name)
        return m

    def defineMap(self, name, mapping):
        """
        Define a map for this group; not thread safe...do not keep adding
 	these while you reference them.
        """
        
        self.maps[name] = mapping

    @classmethod
    def registerGroupLoader(cls, loader):
        cls.groupLoader = loader

    @classmethod
    def registerDefaultLexer(cls, lexerClass):
        cls.defaultTemplateLexerClass = lexerClass

    @classmethod
    def loadGroup(cls, name, superGroup=None):
        if cls.groupLoader is not None:
            return cls.groupLoader.loadGroup(name, superGroup)

        return None
    
    @classmethod
    def loadInterface(cls, name):
        if cls.groupLoader is not None:
            return cls.groupLoader.loadInterface(name)

        return None
    
    def error(self, msg, e = None):
        if self.listener:
            self.listener.error(msg, e)
        else:
            sys.stderr.write('StringTemplate: ' + msg + ': ' + e + '\n')
            traceback.print_exc()
            
    def getTemplateNames(self):
        return self.templates.keys()


    def emitDebugStartStopStrings(self, emit):
        """
        Indicate whether ST should emit <templatename>...</templatename>
        strings for debugging around output for templates from this group.
        """
        
        self.debugTemplateOutput = emit


    def doNotEmitDebugStringsForTemplate(self, templateName):
        if self.noDebugStartStopStrings is None:
            self.noDebugStartStopStrings = set()

        self.noDebugStartStopStrings.add(templateName)


    def emitTemplateStartDebugString(self, st, out):
        if (self.noDebugStartStopStrings is None or
            st.getName() not in self.noDebugStartStopStrings ):
            out.write("<"+st.getName()+">")


    def emitTemplateStopDebugString(self, st, out):
        if (self.noDebugStartStopStrings is None or
            st.getName() not in self.noDebugStartStopStrings ):
            out.write("</"+st.getName()+">")


    def toString(self, showTemplatePatterns=True):
        buf = StringIO()
        buf.write('group ' + str(self.getName()) + ';\n')
        sortedNames = self.templates.keys()
        sortedNames.sort()
        for tname in sortedNames:
            st = self.templates[tname]
            if st != StringTemplateGroup.NOT_FOUND_ST:
                args = st.getFormalArguments().keys()
                args.sort()
                buf.write(str(tname) + '(' + ",".join(args) + ')')
                if showTemplatePatterns:
                    buf.write(' ::= <<' + str(st.getTemplate()) + '>>\n')
                else:
                    buf.write('\n')

        retval = buf.getvalue()
        buf.close()
        return retval

    __str__ = toString

    
# Set class data attribute values.
StringTemplateGroup.DEFAULT_ERROR_LISTENER = StringTemplateGroupErrorListener()
StringTemplateGroup.DEFAULT_EXTENSION = '.st'
StringTemplateGroup.NOT_FOUND_ST = StringTemplate()
StringTemplateGroup.hideProperties = \
['attributeRenderers', 'errorListener', 'fileCharEncoding', 'name', 'refreshInterval', 'rootDir',
 'superGroup', 'templateLexerClass']

# FIXME: ugly hack to work around cyclic import conflict
ASTExpr.MAP_KEY_VALUE = StringTemplate()

