import sys

## Represents the name of a formal argument
#  defined in a template:
#
#  group test;
#  test(a,b) : "$a$ $b$"
#  t() : "blort"
#
#  Each template has a set of these formal arguments or uses
#  a placeholder object: UNKNOWN (indicating that no arguments
#  were specified such as when a template is loaded from a file.st).
#
#  Note: originally, I tracked cardinality as well as the name of an
#  attribute.  I'm leaving the code here as I suspect something may come
#  of it later.  Currently, though, cardinality is not used.
#
class FormalArgument(object):

    # the following represent bit positions emulating a cardinality bitset.
    OPTIONAL = None		# a?
    REQUIRED = None		# a
    ZERO_OR_MORE = None		# a*
    ONE_OR_MORE = None		# a+

    suffixes = []

    ## When template arguments are not available such as when the user
    #  uses "new StringTemplate(...)", then the list of formal arguments
    #  must be distinguished from the case where a template can specify
    #  args and there just aren't any such as the t() template above.
    #
    UNKNOWN = {}

    def __init__(self, name):
        self.name = name
        # self.cardinality = REQUIRED

    def getName(self):
        return self.name

    def setName(self, name):
        self.name = name

    def getCardinalityName(cardinality):
        if cardinality == FormalArgument.OPTIONAL:
	    return 'optional'
        elif cardinality == FormalArgument.REQUIRED:
	    return 'exactly one'
        elif cardinality == FormalArgument.ZERO_OR_MORE:
	    return 'zero-or-more'
        elif cardinality == FormalArgument.ONE_OR_MORE:
	    return 'one-or-more'
        else:
	    return 'unknown'
    getCardinalityName = staticmethod(getCardinalityName)

    def __str__(self):
        return self.getName()

# Set class data attribute values.
FormalArgument.OPTIONAL = 1
FormalArgument.REQUIRED = 2
FormalArgument.ZERO_OR_MORE = 4
FormalArgument.ONE_OR_MORE = 8

FormalArgument.suffixes = [
    None,
    "?",
    "",
    None,
    "*",
    None,
    None,
    None,
    "+"
]

FormalArgument.UNKNOWN = { '<unknown>': -1 }
