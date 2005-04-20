
import antlr

## Tracks the various string and attribute chunks discovered
#  by the lexer.  Subclassed CommonToken so that I could pass
#  the indentation to the parser, which will add it to the
#  ASTExpr created for the $...$ attribute reference.
#
class ChunkToken(antlr.CommonToken):

    def __init__(self, type = None, text = '', indentation = ''):
	super(ChunkToken, self).__init__(type=type, text=text)
	self.indentation = indentation

    def getIndentation(self):
        return self.indentation

    def setIndentation(self, indentation):
        self.indentation = indentation

    def __str__(self):
        return super(ChunkToken, self).__str__() + \
	       ' <indent=\'' + str(self.indentation) + '\'>'
