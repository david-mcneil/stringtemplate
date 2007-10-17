StringTemplate.NET - A C# implementation of the StringTemplate Library

Version 3.0.1

8 November, 2006

Kunle Odutola		: kunle UNDERSCORE odutola AT hotmail DOT com
Micheal Jordan
Anthony Oguntimehin


1. INTRODUCTION 

StringTemplate.NET (or ST#) is an idiomatic port of the StringTemplate 
MVC templating library developed by Terence Parr. It is written in 
the C# programming language and executes on C#/CLI platforms such as 
Microsoft .NET, Novell Mono and dotGNU. 

We hope you find StringTemplate.NET delightful and useful even but, 
as per the license under which you may use it, this software is not 
guaranteed to work. See LICENSE.TXT for the full text of the license 
and NOTICE.TXT for attribution notices.


2. WHAT IS StringTemplate.NET?

StringTemplate.NET (or ST#) is a template engine for generating formatted 
text output. Examples of such output includes source code, web pages and 
emails.

A template engine is at it's most basic, a code generator that emits 
text using templates. These templates are really just "documents with 
holes". The "holes" are markers for where you can stick values. ST# 
breaks up your templates into chunks of text and attribute expressions, 
which are by default enclosed in dollar signs (e.g. $attribute-expression$). 
Everthing except attribute expressions is treated as literal text and 
copied verbatim to the output when you call the method ToString() on 
a StringTemplate instance.

StringTemplate differs from most other template engines by [more] strictly
enforcing Model-View separation.


3. WHAT'S IN THE PACK?

This distribution contains five projects and the project files needed to 
build them with Microsoft Visual Studio 2003 and 2005. The projects are:

	1. StringTemplate		- the core ST# runtime library

	2. StringTemplateTests		- unit tests for the ST# library above

	3. StringTemplateTreeView	- a library for visualizing StringTemplates
					[Alpha quality release - not reliable]

	4. StringTemplateViewer		- a StringTemplateTreeView demo app
	
	5. StringTemplate.Benchmark	- embryonic effort to provide ST# benchmarks
					[Currently empty]
	
In addition the Lib sub-directory contains copies of the ANTLR 2.7.7 runtime 
assemblies. ST# *needs* ANTLR version 2.7.6 or later but, you should probably 
use your own versions of these assemblies if you already have ANTLR 2.7.7.

3.1 Dependenicies

All projects have a dependence on ANTLR 2.7.6+ as stated above. In addition, 
StringTemplateTests has a dependency on NUnit v2.2+.


4. USING StringTemplate.NET

Tou use ST# in your projects, add references to the following files in 
your projects:
  - StringTemplate.dll 
  - antlr.runtime.dll


5. BUILDING StringTemplate.NET

If you wish to re-build ST# for any reason, this is what you need to know.

a) This release of ST# was built with ANTLR 2.7.7. If you change any of the 
ANTLR grammar files supplied with ST#, you will need to use the ANTLR tool to 
regenerate the corresponding C# source files for the Lexer/Parser/TreeParsers. 
The grammar files - all named <something>.g - are in:
	%ST_DIR%\StringTemplate\Antlr.StringTemplate.Language\

  An executable version of v2.7.6 of the ANTLR tool is included in the Tools 
  subdirectory.
  
b) Once you've regenerated the Lexers/Parsers/Treeparsers, you can then use the 
supplied VS.NET project files to build the projects. 

NOTE: Some of the ANTLR grammar files above trigger a curious bug in ANTLR 2.x 
that can only by fixed by hand. The errors are related to the redefinition of 
a variable. There are about 4-6 such manual fixes required although the number 
of error/warning messages you get may be more than that. Errors are:

	CS0128: A local variable named 'variable' is already defined in this scope
	CS0136: A local variable named 'var' cannot be declared in this scope because 
	        it would give a different meaning to 'var', which is already used in 
	        a 'parent or current' scope to denote something else

Using NAnt:
Alternatively, if you have Nant just type the following in the root directory of 
the StringTemplate source tree on your computer to regenerate the sources:

	nant regenerate

You will get builds error due to the ANTLR issue described above. Once you've 
fixed those issues in the generated source files, type the following:

	nant release -t:net-1.1
or
	nant release -t:net-2.0


6. StringTemplate.NET STATUS

This release of StringTemplate.NET is a stable release (ST# TreeView visualizer 
is an Alpha release).

There are currently 0 failures in the unit test suite.


Don't forget to visit the www.stringtemplate.org for further info. The mailing 
list is (currently) very low volume but has a very high Signal-to-Noise ratio. 
We'd like to hear about how you're using ST#.


7. CONTRIBUTORS

Kunle Odutola		-- primary developer
Micheal Jordan		-- template loader abstraction, unit tests
Anthony Oguntimehin	-- developer/tester
Luis Leal			-- IAttributeStrategy abstraction


Enjoy!

Kunle Odutola
