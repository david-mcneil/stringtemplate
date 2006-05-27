#!/usr/bin/env python

from distutils.core import setup

setup(name="stringtemplate",
      version="0.2",
      description="Python version of StringTemplate template engine",
      author="Terence Parr / Marq Kole",
      author_email="parrt AT antlr.org / marq.kole AT philips.com",
      url="http://www.stringtemplate.org/",
      packages=['stringtemplate', 'stringtemplate.language'],
     )
