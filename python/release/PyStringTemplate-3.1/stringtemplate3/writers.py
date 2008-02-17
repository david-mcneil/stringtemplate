
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

from stringtemplate3.utils import deprecated


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
        raise NotImplementedError
    

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
        StringTemplateWriter.__init__(self)

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


    @deprecated
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
                self.atStartOfLine = True
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


    def write(self, str, wrap=None):
        self.out.write(str)
        return len(str)

