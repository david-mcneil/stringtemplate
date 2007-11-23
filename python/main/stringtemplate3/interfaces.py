
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

from StringIO import StringIO

from stringtemplate3.language import (
    InterfaceLexer, InterfaceParser
    )

from stringtemplate3.utils import deprecated
from stringtemplate3.errors import DEFAULT_ERROR_LISTENER


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

    def __init__(self, file, errors=None, superInterface=None):
        """Create an interface from the input stream"""

	## What is the group name
        self.name = None

        ## Maps template name to TemplateDefinition object
        self.templates = {}

	## Are we derived from another group?  Templates not found in this
        # group will be searched for in the superGroup recursively.
        self.superInterface = superInterface

	## Where to report errors.  All string templates in this group
        #  use this error handler by default.
        if errors is not None:
            self.listener = errors
        else:
            self.listener = DEFAULT_ERROR_LISTENER

        self.parseInterface(file)


    @deprecated
    def getSuperInterface(self):
        return self.superInterface

    @deprecated
    def setSuperInterface(self, superInterface):
        self.superInterface = superInterface


    def parseInterface(self, r):
        try:
            lexer = InterfaceLexer.Lexer(r)
            parser = InterfaceParser.Parser(lexer)
            parser.groupInterface(self)

        except RuntimeError, exc: #FIXME:  Exception
            name = self.name or "<unknown>"
            self.error("problem parsing group "+name+": "+str(exc), exc)


    def defineTemplate(self, name, formalArgs, optional):
        d = TemplateDefinition(name, formalArgs, optional)
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
                formalArgs = defST.formalArguments
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


    @deprecated
    def getName(self):
        return self.name

    @deprecated
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

