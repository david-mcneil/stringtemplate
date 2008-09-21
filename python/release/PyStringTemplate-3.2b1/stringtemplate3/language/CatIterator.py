
from StringIO import StringIO

class CatList(object):
    """Given a list of lists, return the combined elements one by one."""

    def __init__(self, lists):
        from stringtemplate3.language.ASTExpr import convertAnythingToList

        self.elements = []
        for attribute in lists:
            self.elements.extend(convertAnythingToList(attribute))


    def __len__(self):
        return len(self.elements)


    def __iter__(self):
        for item in self.elements:
            yield item


    ## The result of asking for the string of a CatList is the list of
    #  items and so this is just the cat'd list of both items.  This
    #  is destructive in that the iterator cursors have moved to the end
    #  after printing.
    def __str__(self):
        return ''.join(str(item) for item in self)

    __repr__ = __str__
