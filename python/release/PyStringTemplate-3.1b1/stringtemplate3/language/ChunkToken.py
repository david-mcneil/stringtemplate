
import antlr
from stringtemplate3.utils import deprecated

class ChunkToken(antlr.CommonToken):
    """
    Tracks the various string and attribute chunks discovered
    by the lexer.  Subclassed CommonToken so that I could pass
    the indentation to the parser, which will add it to the
    ASTExpr created for the $...$ attribute reference.
    """
    
    def __init__(self, type = None, text = '', indentation = ''):
        antlr.CommonToken.__init__(self, type=type, text=text)
        self.indentation = indentation

    @deprecated
    def getIndentation(self):
        return self.indentation

    @deprecated
    def setIndentation(self, indentation):
        self.indentation = indentation

    def __str__(self):
        return (antlr.CommonToken.__str__(self) +
                " <indent='%d'>" % self.indentation
                )
