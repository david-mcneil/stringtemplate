
from stringtemplate3.language.StringRef import *

## Represents a newline.  Separated so I can do smart things like not
#  spitting out newlines when the only thing on a line is an attr expr.
#
class NewlineRef(StringRef):
    def __init__(self, enclosingTemplate, str):
	super(NewlineRef, self).__init__(enclosingTemplate, str)
