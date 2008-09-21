#/usr/bin/env python
# -*- coding: utf-8 -*-

# Make sure our version of stringtemplate3 is imported. Using
#   PYTHONPATH=../.. python TestStringTemplate.py
# does not work as expected, if the stringtemplate3 egg is installed, because
# that gets inserted into the path even before PYTHONPATH!
import sys
sys.path.insert(0, '../..')

# Bail out on any warning
import warnings
warnings.simplefilter('error', Warning)

import unittest
from StringIO import StringIO
import textwrap

from stringtemplate3 import utils


class TestDecodeFile(unittest.TestCase):
    """Test case for stringtemplate3.utils.decodeFile."""

    def testPlainASCIINoDeclaration(self):
        buf = StringIO("// nothing\n// to\n// see\n")
        fp = utils.decodeFile(buf, 'test.txt')
        result = fp.read()
        self.assert_(isinstance(result, unicode))
        self.assertEqual(result, "// nothing\n// to\n// see\n")


    def testUTF8DeclarationFirstLine(self):
        content = textwrap.dedent(
            u"""\
            // coding: utf-8
            trällölü
            """)
        encoding = 'utf-8'
        buf = StringIO(content.encode(encoding))
        fp = utils.decodeFile(buf, 'test.txt')
        result = fp.read()
        self.assert_(isinstance(result, unicode))
        self.assertEqual(result, content)


    def testUTF8DeclarationSecondLine(self):
        content = textwrap.dedent(
            u"""\
            // this is a file
            // coding: utf-8
            trällölü
            """)
        encoding = 'utf-8'
        buf = StringIO(content.encode(encoding))
        fp = utils.decodeFile(buf, 'test.txt')
        result = fp.read()
        self.assert_(isinstance(result, unicode))
        self.assertEqual(result, content)


    def testUTF8DeclarationThirdLine(self):
        content = textwrap.dedent(
            u"""\
            // this is a file
            // oops, declaration on 3rd line
            // coding: utf-8
            trällölü
            """)
        encoding = 'utf-8'
        buf = StringIO(content.encode(encoding))
        fp = utils.decodeFile(buf, 'test.txt')
        try:
            fp.read()
            self.fail()
        except UnicodeDecodeError:
            pass


    def testUTF8NoDeclarationWithBOM(self):
        content = textwrap.dedent(
            u"""\
            // this is a file has a BOM
            trällölü
            """)
        encoding = 'utf-8'
        buf = StringIO('\xef\xbb\xbf' + content.encode(encoding))
        fp = utils.decodeFile(buf, 'test.txt')
        result = fp.read()
        self.assert_(isinstance(result, unicode))
        self.assertEqual(result, content)


    def testUTF8NoDeclarationButDefault(self):
        content = textwrap.dedent(
            u"""\
            // this is a file has no declaration,
            // but I know it's utf-8
            trällölü
            """)
        encoding = 'utf-8'
        buf = StringIO(content.encode(encoding))
        fp = utils.decodeFile(buf, 'test.txt', 'utf-8')
        result = fp.read()
        self.assert_(isinstance(result, unicode))
        self.assertEqual(result, content)


if __name__ == '__main__':
    unittest.main()
