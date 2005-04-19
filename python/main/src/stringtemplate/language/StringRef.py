
from Expr import Expr

## Represents a chunk of just simple text to spit out; nothing to "evaluate"
#
class StringRef(Expr):

    def __init__(self, enclosingTemplate, str_):
	super(StringRef, self).__init__(enclosingTemplate)
	self.str = str_

    ## Just print out the string; no reference to self because this
    #  is a literal -- not sensitive to attribute values.
    #
    def write(self, this, out):
	n = 0
	if self.str:
	    n = out.write(self.str)
	return n

    def __str__(self):
	if self.str:
	    return self.str
	return ''
