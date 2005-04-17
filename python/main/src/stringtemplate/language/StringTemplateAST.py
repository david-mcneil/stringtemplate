
import antlr

class StringTemplateAST(antlr.CommonAST):

    def __init__(self):
	# track template for ANONYMOUS blocks
	super(StringTemplateAST, self).__init__()
        self.st = None

    def getStringTemplate(self):
        return self.st

    def setStringTemplate(self, st):
        self.st = st
