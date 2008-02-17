
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
import traceback
from StringIO import StringIO
from copy import copy


import antlr

from stringtemplate3.language import (
    FormalArgument, UNKNOWN_ARGS,
    ChunkToken,
    ASTExpr, StringTemplateAST,
    TemplateParser,
    ActionLexer, ActionParser,
    ConditionalExpr, NewlineRef,
    StringTemplateToken,
    )

from stringtemplate3.writers import StringTemplateWriter
from stringtemplate3.utils import deprecated
import stringtemplate3


class STAttributeList(list):
    """
    Just an alias for list, but this way I can track whether a
    list is something ST created or it's an incoming list.
    """

    pass


class Aggregate(object):
    """
    An automatically created aggregate of properties.

    I often have lists of things that need to be formatted, but the list
    items are actually pieces of data that are not already in an object.  I
    need ST to do something like:

    Ter=3432
    Tom=32234
    ....

    using template:

    $items:{$attr.name$=$attr.type$$

    This example will call getName() on the objects in items attribute, but
    what if they aren't objects?  I have perhaps two parallel arrays
    instead of a single array of objects containing two fields.  One
    solution is allow dictionaries to be handled like properties so that
    it.name would fail getName() but then see that it's a dictionary and
    do it.get('name') instead.

    This very clean approach is espoused by some, but the problem is that
    it's a hole in my separation rules.  People can put the logic in the
    view because you could say: 'go get bob's data' in the view:

    Bob's Phone: $db.bob.phone$

    A view should not be part of the program and hence should never be able
    to go ask for a specific person's data.

    After much thought, I finally decided on a simple solution.  I've
    added setAttribute variants that pass in multiple property values,
    with the property names specified as part of the name using a special
    attribute name syntax: 'name.{propName1,propName2,...'.  This
    object is a special kind of dictionary that hopefully prevents people
    from passing a subclass or other variant that they have created as
    it would be a loophole.  Anyway, the ASTExpr.getObjectProperty()
    method looks for Aggregate as a special case and does a get() instead
    of getPropertyName.
    """
    
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

    
# <@r()>
REGION_IMPLICIT = 1
# <@r>...<@end>
REGION_EMBEDDED = 2
# @t.r() ::= "..." defined manually by coder
REGION_EXPLICIT = 3

ANONYMOUS_ST_NAME = "anonymous"


## incremental counter for templates IDs
templateCounter = 0

def getNextTemplateCounter():
    global templateCounter
    templateCounter += 1
    return templateCounter

def resetTemplateCounter():
    """
    reset the template ID counter to 0; def that testing routine
    can access but not really of interest to the user.
    """
    
    global templateCounter
    templateCounter = 0



class StringTemplate(object):
    """
    A StringTemplate is a "document" with holes in it where you can stick
    values. StringTemplate breaks up your template into chunks of text and
    attribute expressions.  StringTemplate< ignores everything outside
    of attribute expressions, treating it as just text to spit
    out when you call StringTemplate.toString().
    """

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
    def __init__(self, template=None, group=None, lexer=None, attributes=None):
        self.referencedAttributes = None

        ## What's the name of self template?
        self.name = ANONYMOUS_ST_NAME

        self.templateID = getNextTemplateCounter()

        ## Enclosing instance if I'm embedded within another template.
        #  IF-subtemplates are considered embedded as well.
        self._enclosingInstance = None

        ## A list of embedded templates
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
        self.argumentsAST = None
        
        ## When templates are defined in a group file format, the attribute
        #  list is provided including information about attribute cardinality
        #  such as present, optional, ...  When self information is available,
        #  rawSetAttribute should do a quick existence check as should the
        #  invocation of other templates.  So if you ref bold(item="foo") but
        #  item is not defined in bold(), then an exception should be thrown.
        #  When actually rendering the template, the cardinality is checked.
        #  This is a {str:FormalArgument} dictionary.
        self.formalArgumentKeys = None
        self.formalArguments = UNKNOWN_ARGS

	## How many formal arguments to this template have default values
	#  specified?
        self.numberOfDefaultArgumentValues = 0

	## Normally, formal parameters hide any attributes inherited from the
	#  enclosing template with the same name.  This is normally what you
	#  want, but makes it hard to invoke another template passing in all
	#  the data.  Use notation now: <otherTemplate(...)> to say "pass in
	#  all data".  Works great. Can also say <otherTemplate(foo="xxx",...)>
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
        if group is not None:
            assert isinstance(group, StringTemplateGroup)
            self.group = group
        else:
            self.group = StringTemplateGroup(name='defaultGroup', rootDir='.')

        if lexer is not None:
            self.group.templateLexerClass = lexer

        ## If this template is defined within a group file, what line number?
        self._groupFileLine = None
        
        ## Where to report errors
        self.listener = None
        
        ## The original, immutable pattern/language (not really used again
        #  after initial "compilation", setup/parsing).
        self.pattern = None

        ## Map an attribute name to its value(s).  These values are set by
        #  outside code via st[name] = value.  StringTemplate is like self in
        #  that a template is both the "class def" and "instance".  When you
        #  create a StringTemplate or setTemplate, the text is broken up into
        #  chunks (i.e., compiled down into a series of chunks that can be
        #  evaluated later).
        #  You can have multiple.
        self.attributes = None

	## A Map<Class,Object> that allows people to register a renderer for
	#  a particular kind of object to be displayed in this template.  This
	#  overrides any renderer set for this template's group.
	#
	#  Most of the time this map is not used because the StringTemplateGroup
	#  has the general renderer map for all templates in that group.
	#  Sometimes though you want to override the group's renderers.
        self.attributeRenderers = None

        ## A list of alternating string and ASTExpr references.
        #  This is compiled when the template is loaded/defined and walked to
        #  write out a template instance.
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

        if template is not None:
            assert isinstance(template, basestring)
            self.template = template

        if attributes is not None:
            assert isinstance(attributes, dict)
            self.attributes = attributes


    def dup(self, fr, to):
        """
        Make the 'to' template look exactly like the 'from' template
        except for the attributes.  This is like creating an instance
        of a class in that the executable code is the same (the template
        chunks), but the instance data is blank (the attributes).  Do
        not copy the enclosingInstance pointer since you will want self
        template to eval in a context different from the examplar.
        """
        
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
        
    def getInstanceOf(self):
        """
        Make an instance of self template; it contains an exact copy of
        everything (except the attributes and enclosing instance pointer).
        So the new template refers to the previously compiled chunks of self
        template but does not have any attribute values.
        """
        
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
        return self._enclosingInstance

    def setEnclosingInstance(self, enclosingInstance):
        if self == self._enclosingInstance:
            raise AttributeError('cannot embed template ' +
                                 str(self.name) + ' in itself')
        # set the parent for this template
        self._enclosingInstance = enclosingInstance
        # make the parent track self template as an embedded template
        if enclosingInstance:
            self._enclosingInstance.addEmbeddedInstance(self)

    enclosingInstance = property(getEnclosingInstance, setEnclosingInstance)
    getEnclosingInstance = deprecated(getEnclosingInstance)
    setEnclosingInstance = deprecated(setEnclosingInstance)
    
    def getOutermostEnclosingInstance(self):
        if self.enclosingInstance is not None:
            return self.enclosingInstance.getOutermostEnclosingInstance()

        return self

    def addEmbeddedInstance(self, embeddedInstance):
        if not self.embeddedInstances:
            self.embeddedInstances = []
        self.embeddedInstances.append(embeddedInstance)


    @deprecated
    def getArgumentContext(self):
        return self.argumentContext

    @deprecated
    def setArgumentContext(self, ac):
        self.argumentContext = ac


    @deprecated
    def getArgumentsAST(self):
        return self.argumentsAST

    @deprecated
    def setArgumentsAST(self, argumentsAST):
        self.argumentsAST = argumentsAST


    @deprecated
    def getName(self):
        return self.name

    @deprecated
    def setName(self, name):
        self.name = name


    def getOutermostName(self):
        if self.enclosingInstance is not None:
            return self.enclosingInstance.getOutermostName()
        return self.name


    @deprecated
    def getGroup(self):
        return self.group

    @deprecated
    def setGroup(self, group):
        self.group = group


    @deprecated
    def getNativeGroup(self):
        return self.nativeGroup

    @deprecated
    def setNativeGroup(self, group):
        self.nativeGroup = group


    def getGroupFileLine(self):
        """Return the outermost template's group file line number"""

        if self.enclosingInstance is not None:
            return self.enclosingInstance.getGroupFileLine()

        return self._groupFileLine

    def setGroupFileLine(self, groupFileLine):
        self._groupFileLine = groupFileLine

    groupFileLine = property(getGroupFileLine, setGroupFileLine)
    getGroupFileLine = deprecated(getGroupFileLine)
    setGroupFileLine = deprecated(setGroupFileLine)

        
    def setTemplate(self, template):
        self.pattern = template
        self.breakTemplateIntoChunks()

    def getTemplate(self):
        return self.pattern

    template = property(getTemplate, setTemplate)
    getTemplate = deprecated(getTemplate)
    setTemplate = deprecated(setTemplate)


    def setErrorListener(self, listener):
        self.listener = listener

    def getErrorListener(self):
        if not self.listener:
            return self.group.errorListener
        return self.listener

    errorListener = property(getErrorListener, setErrorListener)
    getErrorListener = deprecated(getErrorListener)
    setErrorListener = deprecated(setErrorListener)

    
    def reset(self):
        # just throw out table and make new one
        self.attributes = {}

    def setPredefinedAttributes(self):
        # only do self method so far in lint mode
        if not stringtemplate3.lintMode:
            return

    def removeAttribute(self, name):
        del self.attributes[name]

    __delitem__ = removeAttribute


    def setAttribute(self, name, *values):
        """
        Set an attribute for self template.  If you set the same
        attribute more than once, you get a multi-valued attribute.
        If you send in a StringTemplate object as a value, its
        enclosing instance (where it will inherit values from) is
        set to 'self'.  This would be the normal case, though you
        can set it back to None after this call if you want.
        If you send in a List plus other values to the same
        attribute, they all get flattened into one List of values.
        This will be a new list object so that incoming objects are
        not altered.
        If you send in an array, it is converted to a List.  Works
        with arrays of objects and arrays of:int,float,double.
        """
        
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
                value.enclosingInstance = self

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
                self.rawSetAttribute(self.attributes, name, v) # replace attribute w/list

            else:
                # non-list second attribute, must convert existing to ArrayList
                v = STAttributeList() # make list to hold multiple values
                # make it point to list now
                self.rawSetAttribute(self.attributes, name, v) # replace attribute w/list
                v.append(o)  # add previous single-valued attribute

            if isinstance(value, list):
                # flatten incoming list into existing
                if v != value: # avoid weird cyclic add
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
            for i, value in enumerate(values):
                if isinstance(value, StringTemplate):
                    value.setEnclosingInstance(self)
                #else:
                #    value = AST.Expr.convertArrayToList(value)
                property_ = properties[i]
                aggr[property_] = value
            self.setAttribute(aggrName, aggr)

    __setitem__ = setAttribute


    def parseAggregateAttributeSpec(self, aggrSpec):
        """
        Split "aggrName.{propName1,propName2" into list [propName1,propName2]
        and the aggrName. Space is allowed around ','
        """

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


    def rawSetAttribute(self, attributes, name, value):
        """
        Map a value to a named attribute.  Throw KeyError if
        the named attribute is not formally defined in self's specific template
        and a formal argument list exists.
        """
        
        if self.formalArguments != UNKNOWN_ARGS and \
           not self.hasFormalArgument(name):
            # a normal call to setAttribute with unknown attribute
            raise KeyError("no such attribute: " + name +
               " in template context " + self.enclosingInstanceStackString)
        if value is not None:
            attributes[name] = value
        elif isinstance(value, list) or \
             isinstance(value, dict) or \
             isinstance(value, set):
            attributes[name] = value


    def rawSetArgumentAttribute(self, embedded, attributes, name, value):
        """
        Argument evaluation such as foo(x=y), x must
        be checked against foo's argument list not this's (which is
        the enclosing context).  So far, only eval.g uses arg self as
        something other than "this".
        """

        if embedded.formalArguments != UNKNOWN_ARGS and \
           not embedded.hasFormalArgument(name):
            raise KeyError("template " + embedded.name +
                " has no such attribute: " + name + " in template context " +
                self.enclosingInstanceStackString)
        if value:
            attributes[name] = value
        elif isinstance(value, list) or \
             isinstance(value, dict) or \
             isinstance(value, set):
            attributes[name] = value


    def write(self, out):
        """
        Walk the chunks, asking them to write themselves out according
        to attribute values of 'self.attributes'.  This is like evaluating or
        interpreting the StringTemplate as a program using the
        attributes.  The chunks will be identical (point at same list)
        for all instances of self template.
        """
        
        if self.group.debugTemplateOutput:
            self.group.emitTemplateStartDebugString(self, out)
            
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
            self.group.emitTemplateStopDebugString(self, out)

        if stringtemplate3.lintMode:
            self.checkForTrouble()
            
        return n


    def get(self, this, attribute):
        """
        Resolve an attribute reference.  It can be in four possible places:

        1. the attribute list for the current template
        2. if self is an embedded template, somebody invoked us possibly
           with arguments--check the argument context
        3. if self is an embedded template, the attribute list for the
           enclosing instance (recursively up the enclosing instance chain)
        4. if nothing is found in the enclosing instance chain, then it might
           be a map defined in the group or the its supergroup etc...

        Attribute references are checked for validity.  If an attribute has
        a value, its validity was checked before template rendering.
        If the attribute has no value, then we must check to ensure it is a
        valid reference.  Somebody could reference any random value like $xyz$
        formal arg checks before rendering cannot detect self--only the ref
        can initiate a validity check.  So, if no value, walk up the enclosed
        template tree again, this time checking formal parameters not
        attributes dictionary.  The formal definition must exist even if no
        value.

        To avoid infinite recursion in str(), we have another condition
        to check regarding attribute values.  If your template has a formal
        argument, foo, then foo will hide any value available from "above"
        in order to prevent infinite recursion.

        This method is not static so people can override its functionality.
        """
            
        if not this:
            return None

        if stringtemplate3.lintMode:
            this.trackAttributeReference(attribute)

        # is it here?
        o = None
        if this.attributes and this.attributes.has_key(attribute):
            o = this.attributes[attribute]
            return o

        # nope, check argument context in case embedded
        if not o:
            argContext = this.argumentContext
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
            lexerClass = self.group.templateLexerClass
            chunkStream = lexerClass(StringIO(self.pattern))
            chunkStream.this = self
            chunkStream.setTokenObjectClass(ChunkToken)
            chunkifier = TemplateParser.Parser(chunkStream)
            chunkifier.template(self)
        except Exception, e:
            name = "<unknown>"
            outerName = self.getOutermostName()
            if self.name:
                name = self.name
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


    @deprecated
    def getTemplateID(self):
        return self.templateID


    @deprecated
    def getAttributes(self):
        return self.attributes

    @deprecated
    def setAttributes(self, attributes):
        self.attributes = attributes


    ## Get a list of the strings and subtemplates and attribute
    #  refs in a template.
    @deprecated
    def getChunks(self):
        return self.chunks

    def addChunk(self, e):
        if not self.chunks:
            self.chunks = []
        self.chunks.append(e)


# ----------------------------------------------------------------------------
#                      F o r m a l  A r g  S t u f f
# ----------------------------------------------------------------------------

    @deprecated
    def getFormalArgumentKeys(self):
        return self.formalArgumentKeys


    @deprecated
    def getFormalArguments(self):
        return self.formalArguments

    @deprecated
    def setFormalArguments(self, args):
        self.formalArguments = args


    def setDefaultArgumentValues(self):
        """
        Set any default argument values that were not set by the
        invoking template or by setAttribute directly.  Note
        that the default values may be templates.  Their evaluation
        context is the template itself and, hence, can see attributes
        within the template, any arguments, and any values inherited
        by the template.

        Default values are stored in the argument context rather than
        the template attributes table just for consistency's sake.
        """

        if not self.numberOfDefaultArgumentValues:
            return
        if not self.argumentContext:
            self.argumentContext = {}
        if self.formalArguments != UNKNOWN_ARGS:
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


    def lookupFormalArgument(self, name):
        """
        From self template upward in the enclosing template tree,
        recursively look for the formal parameter.
        """

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
        self.formalArgumentKeys = []
        self.formalArguments = {}

    def defineFormalArgument(self, names, defaultValue = None):
        if not names:
            return
        if isinstance(names, basestring):
            name = names
            if defaultValue:
                self.numberOfDefaultArgumentValues += 1
            a = FormalArgument(name, defaultValue)
            if self.formalArguments == UNKNOWN_ARGS:
                self.formalArguments = {}
            self.formalArgumentKeys = [name]
            self.formalArguments[name] = a
        elif isinstance(names, list):
            for name in names:
                a = FormalArgument(name, defaultValue)
                if self.formalArguments == UNKNOWN_ARGS:
                    self.formalArgumentKeys = []
                    self.formalArguments = {}
                self.formalArgumentKeys.append(name)
                self.formalArguments[name] = a
                

    @deprecated
    def setPassThroughAttributes(self, passThroughAttributes):
        """
        Normally if you call template y from x, y cannot see any attributes
        of x that are defined as formal parameters of y.  Setting this
        passThroughAttributes to true, will override that and allow a
        template to see through the formal arg list to inherited values.
        """
        
        self.passThroughAttributes = passThroughAttributes


    @deprecated
    def setAttributeRenderers(self, renderers):
        """
        Specify a complete map of what object classes should map to which
        renderer objects.
        """
        
        self.attributeRenderers = renderers


    def registerRenderer(self, attributeClassType, renderer):
        """
        Register a renderer for all objects of a particular type.  This
        overrides any renderer set in the group for this class type.
        """
        
        if not self.attributeRenderers:
            self.attributeRenderers = {}
        self.attributeRenderers[attributeClassType] = renderer

    def getAttributeRenderer(self, attributeClassType):
        """
        What renderer is registered for this attributeClassType for
        this template.  If not found, the template's group is queried.
        """
        
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
        if self.errorListener is not None:
            self.errorListener.warning(msg)
        else:
            sys.stderr.write('StringTemplate: warning: ' + msg)

    def error(self, msg, e = None):
        if self.errorListener is not None:
            self.errorListener.error(msg, e)
        elif e:
            sys.stderr.write('StringTemplate: error: ' + msg + ': ' +
                             str(e))
            traceback.print_exc()
        else:
            sys.stderr.write('StringTemplate: error: ' + msg)


    def trackAttributeReference(self, name):
        """
        Indicates that 'name' has been referenced in self template.
        """
        
        if not self.referencedAttributes:
            self.referencedAttributes = []
        if not name in self.referencedAttributes:
            self.referencedAttributes.append(name)


    @classmethod
    def isRecursiveEnclosingInstance(cls, st):
        """
        Look up the enclosing instance chain (and include self) to see
        if st is a template already in the enclosing instance chain.
        """
        
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


    def getEnclosingInstanceStackTrace(self):
        buf = StringIO()
        seen = {}
        p = self
        while p:
            if hash(p) in seen:
                buf.write(p.templateDeclaratorString)
                buf.write(" (start of recursive cycle)\n...")
                break
            seen[hash(p)] = p
            buf.write(p.templateDeclaratorString)
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
                        buf.write('=<' + o.name + '()@')
                        buf.write(str(o.templateID) + '>')
                    elif isinstance(o, list):
                        buf.write("=List[..")
                        n = 0
                        for st in o:
                            if isinstance(st, StringTemplate):
                                if n > 0:
                                    buf.write(", ")
                                n += 1
                                buf.write('<' + st.name + '()@')
                                buf.write(str(st.templateID) + '>')

                        buf.write("..]")

                buf.write(']')
            if p.referencedAttributes:
                buf.write(', references=')
                buf.write(p.referencedAttributes)
            buf.write('>\n')
            p = p.enclosingInstance
        # if self.enclosingInstance:
        #     buf.write(enclosingInstance.getEnclosingInstanceStackTrace())

        return buf.getvalue()

    enclosingInstanceStackTrace = property(getEnclosingInstanceStackTrace)
    getEnclosingInstanceStackTrace = deprecated(getEnclosingInstanceStackTrace)


    def getTemplateDeclaratorString(self):
        return '<' + self.name + '(' + str(self.formalArgumentKeys) + \
               ')' + '@' + str(self.templateID) + '>'

    templateDeclaratorString = property(getTemplateDeclaratorString)
    getTemplateDeclaratorString = deprecated(getTemplateDeclaratorString)


    def getTemplateHeaderString(self, showAttributes):
        if showAttributes and self.attributes is not None:
            return self.name + str(self.attributes.keys())

        return self.name


    def checkNullAttributeAgainstFormalArguments(self, this, attribute):
        """
        A reference to an attribute with no value, must be compared against
        the formal parameter to see if it exists; if it exists all is well,
        but if not, throw an exception.

        Don't do the check if no formal parameters exist for self template
        ask enclosing.
        """

        if this.formalArguments == UNKNOWN_ARGS:
            # bypass unknown arg lists
            if this.enclosingInstance:
                self.checkNullAttributeAgainstFormalArguments(
                    this.enclosingInstance, attribute)
        else:
            formalArg = this.lookupFormalArgument(attribute)
            if not formalArg:
                raise KeyError('no such attribute: ' + str(attribute) +
                               ' in template context ' +
                               self.enclosingInstanceStackString)


    def checkForTrouble(self):
        """
        Executed after evaluating a template.  For now, checks for setting
        of attributes not reference.
        """
        
        # we have table of set values and list of values referenced
        # compare, looking for SET BUT NOT REFERENCED ATTRIBUTES
        if not self.attributes:
            return
        for name in self.attributes.keys():
            if self.referencedAttributes and \
               not name in self.referencedAttributes:
                self.warning(self.name + ': set but not used: ' + name)

        # can do the reverse, but will have lots of False warnings :(


    def getEnclosingInstanceStackString(self):
        """
        If an instance of x is enclosed in a y which is in a z, return
        a String of these instance names in order from topmost to lowest;
        here that would be "[z y x]".
        """
        
        names = []
        p = self
        while p:
            names.append(p.name)
            p = p.enclosingInstance
        names.reverse()
        s = '['
        while names:
            s += names[0]
            if len(names) > 1:
                s += ' '
            names = names[1:]
        return s + ']'

    enclosingInstanceStackString = property(getEnclosingInstanceStackString)
    getEnclosingInstanceStackString = deprecated(getEnclosingInstanceStackString)
    
    def isRegion(self):
        return self._isRegion

    def setIsRegion(self, isRegion):
        self._isRegion = isRegion

    def addRegionName(self, name):
        self.regions.add(name)

    def containsRegionName(self, name):
        return name in self.regions


    @deprecated
    def getRegionDefType(self):
        return self.regionDefType

    @deprecated
    def setRegionDefType(self, regionDefType):
        self.regionDefType = regionDefType


    def toDebugString(self):
        buf = StringIO()
        buf.write('template-' + self.getTemplateDeclaratorString() + ': ')
        buf.write('chunks=')
        if self.chunks:
            buf.write(str(self.chunks))
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
        buf.write(self.name)
        buf.write(str(self.attributes.keys())) # FIXME: errr.. that's correct?
        buf.write(":\n")
        if self.attributes is not None:
            attrNames = self.attributes.keys()
            for name in attrNames:
                value = self.attributes[name]
                if isinstance(value, StringTemplate): # descend
                    buf.write(value.toStructureString(indent+1))

                else:
                    if isinstance(value, list):
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

        graphST.setAttribute("shape", "none")
        graphST.setAttribute("fontsize", "11")
        graphST.setAttribute("height", "0") # make height
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
                    self.putToMultiValuedMap(edges, srcNode, targetNode)
                    value.getDependencyGraph(edges, showAttributes) # descend

                else:
                    if isinstance(value, list):
                        for o in value:
                            if isinstance(o, StringTemplate):
                                targetNode = o.getTemplateHeaderString(showAttributes)
                                self.putToMultiValuedMap(edges, srcNode, targetNode)
                                o.getDependencyGraph(edges, showAttributes) # descend

                    elif isinstance(value, dict):
                        for o in value.values():
                            if isinstance(o, StringTemplate):
                                targetNode = o.getTemplateHeaderString(showAttributes)
                                self.putToMultiValuedMap(edges, srcNode, targetNode)
                                o.getDependencyGraph(edges, showAttributes) # descend

        # look in chunks too for template refs
        for chunk in self.chunks:
            if not isinstance(chunk, ASTExpr):
                continue

            from stringtemplate3.language.ActionEvaluator import INCLUDE
            tree = chunk.getAST()
            includeAST = antlr.CommonAST(
                antlr.CommonToken(INCLUDE,"include")
                )
            
            for t in tree.findAllPartial(includeAST):
                templateInclude = t.getFirstChild().getText()
                #System.out.println("found include "+templateInclude);
                self.putToMultiValuedMap(edges, srcNode, templateInclude)
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


    def printDebugString(self, out=sys.stderr):
        out.write('template-' + self.name + ':\n')
        out.write('chunks=' + str(self.chunks))
        if not self.attributes:
            return
        out.write("attributes=[")
        n = 0
        for name in self.attributes.keys():
            if n > 0:
                out.write(',')
            value = self.attributes[name]
            if isinstance(value, StringTemplate):
                out.write(name + '=')
                value.printDebugString()
            else:
                if isinstance(value, list):
                    i = 0
                    for o in value:
                        out.write(name + '[' + i + '] is ' +
                                         o.__class__.__name__ + '=')
                        if isinstance(o, StringTemplate):
                            o.printDebugString()
                        else:
                            out.write(o)
                        i += 1
                else:
                    out.write(name + '=' + value + '\n')

            n += 1
        out.write("]\n")


    def toString(self, lineWidth=StringTemplateWriter.NO_WRAP):
        # Write the output to a StringIO
        out = StringIO(u'')
        wr = self.group.getStringTemplateWriter(out)
        wr.lineWidth = lineWidth
        try:
            self.write(wr)
        except IOError, io:
            self.error("Got IOError writing to writer" + \
                       str(wr.__class__.__name__))
            
        # reset so next toString() does not wrap; normally this is a new writer
        # each time, but just in case they override the group to reuse the
        # writer.
        wr.lineWidth = StringTemplateWriter.NO_WRAP
        
        return out.getvalue()

    __str__ = toString
    

# initialize here, because of cyclic imports
from stringtemplate3.groups import StringTemplateGroup
StringTemplateGroup.NOT_FOUND_ST = StringTemplate()
ASTExpr.MAP_KEY_VALUE = StringTemplate()

