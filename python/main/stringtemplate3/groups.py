
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
import imp
import time
from StringIO import StringIO

import antlr

from stringtemplate3.language import (
    AngleBracketTemplateLexer,
    DefaultTemplateLexer,
    GroupLexer, GroupParser,
    )

from stringtemplate3.utils import deprecated
from stringtemplate3.errors import (
    DEFAULT_ERROR_LISTENER
    )
from stringtemplate3.templates import (
    StringTemplate, REGION_IMPLICIT
    )
from stringtemplate3.writers import AutoIndentWriter
from stringtemplate3.interfaces import StringTemplateGroupInterface

DEFAULT_EXTENSION = '.st'

## Used to indicate that the template doesn't exist.
#  We don't have to check disk for it; we know it's not there.
#  Set later to work around cyclic class definitions
NOT_FOUND_ST = None


class StringTemplateGroup(object):
    """
    Manages a group of named mutually-referential StringTemplate objects.
    Currently the templates must all live under a directory so that you
    can reference them as foo.st or gutter/header.st.  To refresh a
    group of templates, just create a new StringTemplateGroup and start
    pulling templates from there.  Or, set the refresh interval.

    Use getInstanceOf(template-name) to get a string template
    to fill in.

    The name of a template is the file name minus ".st" ending if present
    unless you name it as you load it.

    You can use the group file format also to define a group of templates
    (this works better for code gen than for html page gen).  You must give
    a Reader to the ctor for it to load the group; this is general and
    distinguishes it from the ctors for the old-style "load template files
    from the disk".

    10/2005 I am adding a StringTemplateGroupLoader concept so people can
    define supergroups within a group and have it load that group
    automatically.
    """
    
    ## Track all groups by name; maps name to StringTemplateGroup
    nameToGroupMap = {}

    ## Track all interfaces by name; maps name to StringTemplateGroupInterface
    nameToInterfaceMap = {}
    
    ## If a group file indicates it derives from a supergroup, how do we
    #  find it?  Shall we make it so the initial StringTemplateGroup file
    #  can be loaded via this loader?  Right now we pass a Reader to ctor
    #  to distinguish from the other variety.
    groupLoader = None

    ## You can set the lexer once if you know all of your groups use the
    #  same separator.  If the instance has templateLexerClass set
    #  then it is used as an override.
    defaultTemplateLexerClass = DefaultTemplateLexer.Lexer
        
    def __init__(self, name=None, rootDir=None, lexer=None, file=None, errors=None, superGroup=None):
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
        self._templateLexerClass = None
        
        ## Under what directory should I look for templates?  If None,
        #  to look into the CLASSPATH for templates as resources.
        #
        self.rootDir = None

        ## Are we derived from another group?  Templates not found in this
        #  group will be searched for in the superGroup recursively.
        self._superGroup = None

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
        if errors is not None:
            self.listener = errors
        else:
            self.listener = DEFAULT_ERROR_LISTENER
            
        ## How long before tossing out all templates in seconds.
        #  default: no refreshing from disk
        #
        self.refreshInterval = sys.maxint/1000
        self.lastCheckedDisk = 0L
        
        if name is not None:
            assert isinstance(name, basestring)
            self.name = name

            assert rootDir is None or isinstance(rootDir, basestring)
            self.rootDir = rootDir
            self.lastCheckedDisk = time.time()
            StringTemplateGroup.nameToGroupMap[self.name] = self

            self.templateLexerClass = lexer
            
            assert superGroup is None or isinstance(superGroup, StringTemplateGroup)
            self.superGroup = superGroup


        if file is not None:
            assert hasattr(file, 'read')
            
            self.templatesDefinedInGroupFile = True

            if lexer is not None:
                self.templateLexerClass = lexer
            else:
                self.templateLexerClass = AngleBracketTemplateLexer.Lexer

            assert superGroup is None or isinstance(superGroup, StringTemplateGroup)
            self.superGroup = superGroup

            self.parseGroup(file)
            assert self.name is not None
            StringTemplateGroup.nameToGroupMap[self.name] = self
            self.verifyInterfaceImplementations()


    def getTemplateLexerClass(self):
        """
        What lexer class to use to break up templates.  If not lexer set
        for this group, use static default.
        """

        if self._templateLexerClass is not None:
            return self._templateLexerClass

        return self.defaultTemplateLexerClass

    def setTemplateLexerClass(self, lexer):
        if isinstance(lexer, basestring):
            try:
                self._templateLexerClass = {
                    'default': DefaultTemplateLexer.Lexer,
                    'angle-bracket': AngleBracketTemplateLexer.Lexer,
                    }[lexer]
            except KeyError:
                raise ValueError('Unknown lexer id %r' % lexer)

        elif isinstance(lexer, type) and issubclass(lexer, antlr.CharScanner):
            self._templateLexerClass = lexer

        elif lexer is not None:
            raise TypeError(
                "Lexer must be string or lexer class, got %r"
                % type(lexer).__name__
                )

    templateLexerClass = property(getTemplateLexerClass, setTemplateLexerClass)
    getTemplateLexerClass = deprecated(getTemplateLexerClass)
    setTemplateLexerClass = deprecated(setTemplateLexerClass)


    @deprecated
    def getName(self):
        return self.name

    @deprecated
    def setName(self, name):
        self.name = name


    def setSuperGroup(self, superGroup):
        if superGroup is None or isinstance(superGroup, StringTemplateGroup):
            self._superGroup = superGroup
            
        elif isinstance(superGroup, basestring):
            # Called by group parser when ": supergroupname" is found.
            # This method forces the supergroup's lexer to be same as lexer
            # for this (sub) group.

            superGroupName = superGroup
            superGroup = StringTemplateGroup.nameToGroupMap.get(
                superGroupName, None)
            if superGroup is not None:
                # we've seen before; just use it
                self.superGroup = superGroup

            else:
                # else load it using this group's template lexer
                superGroup = self.loadGroup(
                    superGroupName, lexer=self.templateLexerClass)
                if superGroup is not None:
                    StringTemplateGroup.nameToGroupMap[superGroup] = superGroup
                    self._superGroup = superGroup

                elif self.groupLoader is None:
                    self.listener.error("no group loader registered", None)

        else:
            raise TypeError(
                "Need StringTemplateGroup or string, got %s"
                % type(superGroup).__name__
                )

    def getSuperGroup(self):
        return self._superGroup

    superGroup = property(getSuperGroup, setSuperGroup)
    getSuperGroup = deprecated(getSuperGroup)
    setSuperGroup = deprecated(setSuperGroup)
    

    def getGroupHierarchyStackString(self):
        """Walk up group hierarchy and show top down to this group"""

        groupNames = []
        p = self
        while p is not None:
            groupNames.insert(0, p.name)
            p = p.superGroup

        return '[' + ' '.join(groupNames) + ']'


    @deprecated
    def getRootDir(self):
        return self.rootDir

    @deprecated
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


    def getInstanceOf(self, name, enclosingInstance=None, attributes=None):
        """
        A support routine that gets an instance of name knowing which
        ST encloses it for error messages.
        """

        assert isinstance(name, basestring)
        assert enclosingInstance is None or isinstance(enclosingInstance, StringTemplate)
        assert attributes is None or isinstance(attributes, dict)

        st = self.lookupTemplate(name, enclosingInstance)
        if st is not None:
            st = st.getInstanceOf()
            
            if attributes is not None:
                st.attributes = attributes
                
            return st
        
        return None

    def getEmbeddedInstanceOf(self, name, enclosingInstance):
        assert isinstance(name, basestring)
        assert enclosingInstance is None or isinstance(enclosingInstance, StringTemplate)

        st = None
        # TODO: seems like this should go into lookupTemplate
        if name.startswith("super."):
            # for super.foo() refs, ensure that we look at the native
            # group for the embedded instance not the current evaluation
            # group (which is always pulled down to the original group
            # from which somebody did group.getInstanceOf("foo");
            st = enclosingInstance.nativeGroup.getInstanceOf(
                name, enclosingInstance
                )
            
        else:
            st = self.getInstanceOf(name, enclosingInstance)

        # make sure all embedded templates have the same group as enclosing
        # so that polymorphic refs will start looking at the original group
        st.group = self
        st.enclosingInstance = enclosingInstance
        return st

    ## Get the template called 'name' from the group.  If not found,
    #  attempt to load.  If not found on disk, then try the superGroup
    #  if any.  If not even there, then record that it's
    #  NOT_FOUND so we don't waste time looking again later.  If we've gone
    #  past refresh interval, flush and look again.
    #
    #  If I find a template in a super group, copy an instance down here
    def lookupTemplate(self, name, enclosingInstance=None):
        assert isinstance(name, basestring)
        assert enclosingInstance is None or isinstance(enclosingInstance, StringTemplate)

        if name.startswith('super.'):
            if self.superGroup:
                dot = name.find('.')
                name = name[dot+1:]
                return self.superGroup.lookupTemplate(name, enclosingInstance)
            raise ValueError(self.name + ' has no super group; ' +
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
                    st.group = self

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
                        enclosingInstance.enclosingInstanceStackString
                        )
                hier = self.getGroupHierarchyStackString()
                context += "; group hierarchy is "+hier
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
        if self.refreshInterval == 0 or \
           (time.time() - self.lastCheckedDisk) >= \
           self.refreshInterval:
            # throw away all pre-compiled references
            self.templates = {}
            self.lastCheckedDisk = time.time()


    def loadTemplate(self, name, src):
        if isinstance(src, basestring):
            template = None
            try:
                br = open(src, 'r')
                try:
                    template = self.loadTemplate(name, br)
                finally:
                    br.close()

            # FIXME: eek, that's ugly
            except Exception, e:
                raise
            
            return template
        
        elif hasattr(src, 'readlines'):
            buf = src.readlines()
            
            # strip newlines etc.. from front/back since filesystem
            # may add newlines etc...
            pattern = str().join(buf).strip()
            
            if not pattern:
                self.error("no text in template '"+name+"'")
                return None
            
            return self.defineTemplate(name, pattern)
        
        raise TypeError(
            'loadTemplate should be called with a file or filename'
            )


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
                except IOError, ioe:
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
        return templateName + DEFAULT_EXTENSION

    ## Convert a filename relativePath/name.st to relativePath/name.
    #  (def that people can override behavior; not a general
    #  purpose method)
    #
    def getTemplateNameFromFileName(self, fileName):
        name = fileName
        suffix = name.rfind(DEFAULT_EXTENSION)
        if suffix >= 0:
            name = name[:suffix]
        return name


    ## Define an examplar template; precompiled and stored
    #  with no attributes.  Remove any previous definition.
    #
    def defineTemplate(self, name, template):
        if name is not None and '.' in name:
            raise ValueError("cannot have '.' in template names")
        
        st = self.createStringTemplate()
        st.name = name
        st.group = self
        st.nativeGroup = self
        st.template = template
        st.errorListener = self.listener
        
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
        regionST.regionDefType = type
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
            REGION_IMPLICIT
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
                if st.regionDefType == REGION_IMPLICIT:
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
            if self.name:
                name = self.name
            self.error('problem parsing group ' + name + ': ' + str(e), e)

    def verifyInterfaceImplementations(self):
        """verify that this group satisfies its interfaces"""

        for interface in self.interfaces:
            missing = interface.getMissingTemplates(self)
            mismatched = interface.getMismatchedTemplates(self)
            
            if missing:
                self.error(
                    "group " + self.name +
                    " does not satisfy interface " + interface.name +
                    ": missing templates [" + ', '.join(["'%s'" % m for m in missing]) + ']'
                    )

            if mismatched:
                self.error(
                    "group " + self.name +
                    " does not satisfy interface " + interface.name +
                    ": mismatched arguments on these templates [" + ', '.join(["'%s'" % m for m in mismatched]) + ']'
                    )


    @deprecated
    def getRefreshInterval(self):
        return self.refreshInterval

    ## How often to refresh all templates from disk.  This is a crude
    #  mechanism at the moment--just tosses everything out at this
    #  frequency.  Set interval to 0 to refresh constantly (no caching).
    #  Set interval to a huge number like MAX_INT to have no refreshing
    #  at all (DEFAULT); it will cache stuff.
    @deprecated
    def setRefreshInterval(self, refreshInterval):
        self.refreshInterval = refreshInterval


    def setErrorListener(self, listener):
        self.listener = listener

    def getErrorListener(self):
        return self.listener

    errorListener = property(getErrorListener, setErrorListener)
    getErrorListener = deprecated(getErrorListener)
    setErrorListener = deprecated(setErrorListener)

    
    ## Specify a StringTemplateWriter implementing class to use for
    #  filtering output
    def setStringTemplateWriter(self, c):
        self.userSpecifiedWriter = c

    ## return an instance of a StringTemplateWriter that spits output to w.
    #  If a writer is specified, use it instead of the default.
    def getStringTemplateWriter(self, w):
        stw = None
        if self.userSpecifiedWriter:
            try:
                stw = self.userSpecifiedWriter(w)
            except RuntimeError, e: #FIXME Exception, e:
                self.error('problems getting StringTemplateWriter', e)

        if not stw:
            stw = AutoIndentWriter(w)
        return stw

    ## Specify a complete map of what object classes should map to which
    #  renderer objects for every template in this group (that doesn't
    #  override it per template).
    @deprecated
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
            return self.attributeRenderers[attributeClassType]

        elif self.superGroup is not None:
            # no renderer registered for this class, check super group
            return self.superGroup.getAttributeRenderer(attributeClassType)
        
        return None

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
    def loadGroup(cls, name, superGroup=None, lexer=None):
        if cls.groupLoader is not None:
            return cls.groupLoader.loadGroup(name, superGroup, lexer)

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
            st.name not in self.noDebugStartStopStrings ):
            out.write("<"+st.name+">")


    def emitTemplateStopDebugString(self, st, out):
        if (self.noDebugStartStopStrings is None or
            st.name not in self.noDebugStartStopStrings ):
            out.write("</"+st.name+">")


    def toString(self, showTemplatePatterns=True):
        buf = StringIO()
        buf.write('group ' + str(self.name) + ';\n')
        sortedNames = self.templates.keys()
        sortedNames.sort()
        for tname in sortedNames:
            st = self.templates[tname]
            if st != StringTemplateGroup.NOT_FOUND_ST:
                args = st.formalArguments.keys()
                args.sort()
                buf.write(str(tname) + '(' + ",".join(args) + ')')
                if showTemplatePatterns:
                    buf.write(' ::= <<' + str(st.template) + '>>\n')
                else:
                    buf.write('\n')

        retval = buf.getvalue()
        buf.close()
        return retval

    __str__ = toString
