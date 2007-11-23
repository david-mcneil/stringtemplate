
from StringIO import StringIO

## Given a list of lists, return the combined elements one by one.
#
class CatList(object):

    def __init__(self, lists):
	## List of lists to cat together 
	#
        self._lists = lists

    def __len__(self):
        k = 0
        for list_ in self._lists:
            k += len(list_)
        return k

    def lists(self):
        for list_ in self._lists:
            for item in list_:
                yield item

    def __iter__(self):
        for list_ in self._lists:
            for item in list_:
                yield item

        
    ## The result of asking for the string of a CatList is the list of
    #  items and so this is just the cat'd list of both items.  This
    #  is destructive in that the iterator cursors have moved to the end
    #  after printing.
    def __str__(self):
        buf = StringIO()
        #buf.write('[')
        k = len(self)
        for item in self.lists():
            buf.write(str(item))
            k -= 1
            #if k:
            #    buf.write(', ')
        #buf.write(']')

        return buf.getvalue()

    __repr__ = __str__
