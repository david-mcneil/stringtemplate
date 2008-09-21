StringTemplate 3.2 (Python)
August 9, 2008


AUTHORS
=======

Design & Java implementation:

Terence Parr, parrt at cs usfca edu
  ANTLR project lead and supreme dictator for life
  University of San Francisco


Python port:

Marq Kole <marq.kole at xs4all.nl>
  Python implementation up to V2.2
  Copyright 2003-2006

Benjamin Niemann <pink@odahoda.de>
  Updated Python implementation from V2.2 to V3.
  Copyright 2007


ABSTRACT
========

ST (StringTemplate) is a template engine (with ports for C# and Python)
for generating source code, web pages, emails, or any other formatted text
output. ST is particularly good at multi-targeted code generators, multiple
site skins, and internationalization/localization. It evolved over years of
effort developing jGuru.com. ST also generates this website and powers the
ANTLR v3 code generator. Its distinguishing characteristic is that it strictly
enforces model-view separation unlike other engines.


DOCUMENTATION
=============

The main website is:

	http://www.stringtemplate.org

The documentation is in the wiki: 

	http://www.antlr.org/wiki/display/ST/StringTemplate+Documentation

Here are the 3.2 release notes:

	http://www.antlr.org/wiki/display/ST/3.2+Release+Notes


LICENSE
=======

Per the license in LICENSE.txt, this software is not guaranteed to
work and might even destroy all life on this planet.

See the CHANGES.txt file.


INSTALLATION
============

$ python setup.py install

or just copy the stringtemplate3 directory to a location of your choice - it's
a pure Python package.

StringTemplate has a dependency on ANTLR V2. Download and install it from
<http://www.antlr2.org/download.html>.
