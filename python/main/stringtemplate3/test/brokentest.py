#!/usr/bin/python

import unittest

class BrokenTest(unittest.TestCase.failureException):
    def __str__(self):
        name = self.args[0]
        return '%s: %s: works now' % (
            (self.__class__.__name__, name))


def broken(test_method):
    def wrapper(*args, **kwargs):
        try:
            test_method(*args, **kwargs)
        except Exception:
            pass
        else:
            raise BrokenTest(test_method.__name__)
    wrapper.__doc__ = test_method.__doc__
    wrapper.__name__ = 'XXX_' + test_method.__name__
    return wrapper
