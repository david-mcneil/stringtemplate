
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
import codecs

from stringtemplate3.utils import deprecated
from stringtemplate3.groups import StringTemplateGroup
from stringtemplate3.interfaces import StringTemplateGroupInterface
from stringtemplate3.language import AngleBracketTemplateLexer


class StringTemplateGroupLoader(object):
    """
    When group files derive from another group, we have to know how to
    load that group and its supergroups. This interface also knows how
    to load interfaces
    """

    def loadGroup(self, groupName, superGroup=None, lexer=None):
        """
        Load the group called groupName from somewhere.  Return null
        if no group is found.
        Groups with region definitions must know their supergroup to find
        templates during parsing.
        Specify the template lexer to use for parsing templates.  If null,
        it assumes angle brackets <...>.
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
        
        StringTemplateGroupLoader.__init__(self)

        ## List of ':' separated dirs to pull groups from
        self.dirs = dir.split(':')
        self.errors = errors

        ## How are the files encoded (ascii, UTF8, ...)?
        #  You might want to read UTF8 for example on an ascii machine.
        self.fileCharEncoding = sys.getdefaultencoding()
        

    def loadGroup(self, groupName, superGroup=None, lexer=None):
        if lexer is None:
            lexer = AngleBracketTemplateLexer.Lexer
        try:
            fr = self.locate(groupName+".stg")
            if fr is None:
                self.error("no such group file "+groupName+".stg")
                return None

            try:
                return StringTemplateGroup(
                    file=fr,
                    lexer=lexer,
                    errors=self.errors,
                    superGroup=superGroup
                    )
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
                if self.fileCharEncoding is not None:
                    fr = codecs.getreader(self.fileCharEncoding)(fr)
                return fr

        return None
    

    @deprecated
    def getFileCharEncoding(self):
        return self.fileCharEncoding

    @deprecated
    def setFileCharEncoding(self, fileCharEncoding):
        self.fileCharEncoding = fileCharEncoding


    def error(self, msg, exc=None):
        if self.errors is not None:
            self.errors.error(msg, exc)
            
        else:
            sys.stderr.write("StringTemplate: "+msg+"\n")
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


