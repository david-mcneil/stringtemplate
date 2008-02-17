
import antlr

class StringTemplateToken(antlr.CommonToken):

    def __init__(self, type = 0, text = '', args = None):
        super(StringTemplateToken, self).__init__(type=type, text=text)
        ## Track any args for anonymous templates like
        #  <tokens,rules:{t,r | <t> then <r>}>
        #  The lexer in action.g returns a single token ANONYMOUS_TEMPLATE
        #  and so I need to have it parse args in the lexer and make them
        #  available for when I build the anonymous template.

        if args is None:
            args = []
        self.args = args

    def __str__(self):
        return super(StringTemplateToken, self).__str__() + \
               '; args=' + str(self.args)

    __repr__ = __str__
