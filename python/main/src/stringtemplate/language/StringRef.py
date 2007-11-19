
from Expr import Expr

class StringRef(Expr):
    """
    Represents a chunk of just simple text to spit out; nothing to "evaluate"
    """

    def __init__(self, enclosingTemplate, str_):
	super(StringRef, self).__init__(enclosingTemplate)
	self.str = str_

    def write(self, this, out):
        """
        Just print out the string; no reference to self because this
        is a literal -- not sensitive to attribute values. These strings
        never wrap because they are not part of an <...> expression.
        <"foo"; wrap="\n"> should wrap though if necessary.
        """
        
	if self.str is not None:
	    n = out.write(self.str)
            return n

        return 0

    
    def __str__(self):
	if self.str:
	    return self.str
	return ''
