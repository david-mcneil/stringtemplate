#!/usr/bin/env python

from distutils.core import setup


setup(
    name="stringtemplate3",
    version="3.1b1",
    description="A powerful template engine with strict model-view separation",
    long_description="""
    ST (StringTemplate) is a template engine for generating source code, web
    pages, emails, or any other formatted text output. ST is particularly
    good at multi-targeted code generators, multiple site skins, and
    internationalization/localization. It evolved over years of effort
    developing jGuru.com. ST also generates this website and powers the
    ANTLR v3 code generator. Its distinguishing characteristic is that it
    strictly enforces model-view separation unlike other engines.
    """,
    classifiers=[
    'Development Status :: 4 - Beta',
    'Intended Audience :: Developers',
    'License :: OSI Approved :: BSD License',
    'Operating System :: OS Independent',
    'Programming Language :: Python',
    'Topic :: Software Development :: Libraries :: Python Modules',
    'Topic :: Text Processing',
    ],
    author="Terence Parr / Marq Kole / Benjamin Niemann",
    author_email="parrt AT antlr.org / marq.kole AT philips.com / pink@odahoda.de",
    maintainer="Benjamin Niemann",
    maintainer_email="pink@odahoda.de",
    url="http://www.stringtemplate.org/",
    license="BSD",
    platform="any",
    packages=['stringtemplate3', 'stringtemplate3.language'],
    )
