
import antlr

class StringTemplateAST(antlr.CommonAST):

    def __init__(self, type=None, text=None):
        super(StringTemplateAST, self).__init__() 

        if type is not None:
            self.setType(type)

        if text is not None:
            self.setText(text)

	# track template for ANONYMOUS blocks
        self.st = None

    def getStringTemplate(self):
        return self.st

    def setStringTemplate(self, st):
        self.st = st
