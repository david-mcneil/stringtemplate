from stringtemplate3.utils import deprecated

class Expr(object):
    """
    A string template expression embedded within the template.
    A template is parsed into a tokenized vector of Expr objects
    and then executed after the user sticks in attribute values.

    This list of Expr objects represents a "program" for the StringTemplate
    evaluator.
    """
    
    def __init__(self, enclosingTemplate):
        ## The StringTemplate object surrounding this expr
        self.enclosingTemplate = enclosingTemplate

        ## Anything spit out as a chunk (even plain text) must be indented
        #  according to whitespace before the action that generated it.  So,
        #  plain text in the outermost template is never indented, but the
        #  text and attribute references in a nested template will all be
        #  indented by the amount seen directly in front of the attribute
        #  reference that initiates construction of the nested template.
        self.indentation = ''


    def write(self, this, out):
        """How to write this node to output"""
        raise NotImplementedError


    @deprecated
    def getEnclosingTemplate(self):
        return self.enclosingTemplate


    @deprecated
    def getIndentation(self):
        return self.indentation

    @deprecated
    def setIndentation(self, indentation):
        self.indentation = indentation
