#!/usr/bin/python
# -*- coding: utf-8 -*-

# Make sure our version of stringtemplate3 is imported. Using
#   PYTHONPATH=../.. python TestStringTemplate.py
# does not work as expected, if the stringtemplate3 egg is installed, because
# that gets inserted into the path even before PYTHONPATH!
import os
import sys
sys.path.insert(0, os.path.abspath('../..'))

import cgi
import codecs
import unittest
import warnings
import stringtemplate3

warnings.simplefilter('error', Warning)
stringtemplate3.crashOnActionParseError = True


class TestEncoding(unittest.TestCase):
    def runTest(self):
        datadir = os.path.join(os.path.dirname(__file__), 'testencoding-data')
        pages_group = stringtemplate3.StringTemplateGroup(
            name='pages',
            rootDir=datadir)

        group = stringtemplate3.StringTemplateGroup(
            fileName=os.path.join(datadir, 'page.stg'),
            lexer='default',
            superGroup=pages_group)

        class EscapeRenderer(stringtemplate3.AttributeRenderer):
            def toString(self, o, formatName=None):
                if formatName is None:
                    # no formatting specified -> escape it
                    return cgi.escape(o)

                if formatName == "noescape":
                    return o
                else:
                    raise ValueError("Unsupported format name")

        pages_group.registerRenderer(unicode, EscapeRenderer())

        menu = group.getInstanceOf('menu')

        menuItem = group.getInstanceOf('menuItem')
        menuItem['url'] = 'http://www.stringtemplate.org/'
        menuItem['text'] = u"Îñţérñåţîöñåļîžåţîöñ"
        menu['items'] = menuItem

        menuItem = group.getInstanceOf('menuItem')
        menuItem['url'] = 'http://www.google.com/'
        menuItem['text'] = "<Google>"
        menu['items'] = menuItem

        body = group.getInstanceOf('page_index')

        page = group.getInstanceOf('page')
        page['title'] = u"This is <just> a Îñţérñåţîöñåļîžåţîöñ demo"
        page['menu'] = menu
        page['body'] = body

        fp = codecs.open(os.path.join(datadir, 'expected'), 'r', 'utf-8')
        expected = fp.read()
        fp.close()

        self.assertEqual(page.toString(), expected)


if __name__ == '__main__':
    unittest.main()
