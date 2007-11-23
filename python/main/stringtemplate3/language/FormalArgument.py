
class FormalArgument(object):
    """
    Represents the name of a formal argument
    defined in a template:

    group test;
    test(a,b) : "$a$ $b$"
    t() : "blort"

    Each template has a set of these formal arguments or uses
    a placeholder object: UNKNOWN (indicating that no arguments
    were specified such as when a template is loaded from a file.st).

    Note: originally, I tracked cardinality as well as the name of an
    attribute.  I'm leaving the code here as I suspect something may come
    of it later.  Currently, though, cardinality is not used.
    """
    
    # the following represent bit positions emulating a cardinality bitset.
    OPTIONAL = 1      # a?
    REQUIRED = 2      # a
    ZERO_OR_MORE = 4  # a*
    ONE_OR_MORE = 8   # a+

    suffixes = [
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

    ## When template arguments are not available such as when the user
    #  uses "new StringTemplate(...)", then the list of formal arguments
    #  must be distinguished from the case where a template can specify
    #  args and there just aren't any such as the t() template above.
    UNKNOWN = { '<unknown>': -1 }

    def __init__(self, name, defaultValueST = None):
        self.name = name
        # self.cardinality = REQUIRED
        ## If they specified name="value", store the template here
        #
        self.defaultValueST = defaultValueST


    @staticmethod
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


    def __eq__(self, other):
        if not isinstance(other, FormalArgument):
            return False
        
        if self.name != other.name:
            return False

        # only check if there is a default value; that's all
        if ( (self.defaultValueST is not None
              and other.defaultValueST is None) or
             (self.defaultValueST is None
              and other.defaultValueST is not None) ):
            return False

        return True


    def __neq__(self, other):
        return not self.__eq__(other)

    
    def __str__(self):
        if self.defaultValueST:
            return self.name + '=' + str(self.defaultValueST)
        return self.name

    __repr__ = __str__

