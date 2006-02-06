header {
/*
 [The "BSD licence"]
 Copyright (c) 2003-2004 Terence Parr
 All rights reserved.

 Redistribution and use in source and binary forms, with or without
 modification, are permitted provided that the following conditions
 are met:
 1. Redistributions of source code must retain the above copyright
    notice, this list of conditions and the following disclaimer.
 2. Redistributions in binary form must reproduce the above copyright
    notice, this list of conditions and the following disclaimer in the
    documentation and/or other materials provided with the distribution.
 3. The name of the author may not be used to endorse or promote products
    derived from this software without specific prior written permission.

 THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System.Collections;
}

options {
	language="CSharp";
	namespace="Antlr.StringTemplate.Language";
}

/** Match a group of template definitions beginning
 *  with a group name declaration.  Templates are enclosed
 *  in double-quotes or <<...>> quotes for multi-line templates.
 *  Template names have arg lists that indicate the cardinality
 *  of the attribute: present, optional, zero-or-more, one-or-more.
 *  Here is a sample group file:

	group nfa;

	// an NFA has edges and states
	nfa(states,edges) ::= <<
	digraph NFA {
	rankdir=LR;
	<states; separator="\\n">
	<edges; separator="\\n">
	}
	>>

	state(name) ::= "node [shape = circle]; <name>;"

 */
class GroupParser extends Parser;

options {
    k=3;
}

{
protected StringTemplateGroup _group;

override public void reportError(RecognitionException e) {
	if (_group != null)
		_group.Error("template group parse error", e);
	else {
		Console.Error.WriteLine("template group parse error: "+e);
	    Console.Error.WriteLine(e.StackTrace);
	}
}
}

group[StringTemplateGroup g]
	:	"group" name:ID {g.Name = name.getText();}
		( COLON s:ID {g.SetSuperGroup(s.getText());} )?
	    ( "implements" i:ID {g.ImplementInterface(i.getText());}
	      (COMMA i2:ID {g.ImplementInterface(i2.getText());} )*
	    )?
	    SEMI
	    ( template[g] | mapdef[g] )+
    ;

template[StringTemplateGroup g]
{
    StringTemplate st = null;
    string templateName=null;
    int line = LT(1).getLine();
}
	:	(	AT scope:ID DOT region:ID
			{
			templateName=g.GetMangledRegionName(scope.getText(),region.getText());
	    	if ( g.IsDefinedInThisGroup(templateName) ) {
	        	g.Error("group "+g.Name+" line "+line+": redefinition of template region: @"+
	        		scope.getText()+"."+region.getText());
				st = new StringTemplate(); // create bogus template to fill in
			}
			else {
				bool err = false;
				// @template.region() ::= "..."
				StringTemplate scopeST = g.LookupTemplate(scope.getText());
				if ( scopeST==null ) {
					g.Error("group "+g.Name+" line "+line+": reference to region within undefined template: "+
						scope.getText());
					err=true;
				}
				if ( !scopeST.ContainsRegionName(region.getText()) ) {
					g.Error("group "+g.Name+" line "+line+": template "+scope.getText()+" has no region called "+
						region.getText());
					err=true;
				}
				if ( err ) {
					st = new StringTemplate();
				}
				else {
					st = g.DefineRegionTemplate(scope.getText(),
												region.getText(),
												null,
												StringTemplate.REGION_EXPLICIT);
				}
			}
			}
	|	name:ID {templateName = name.getText();}
	    {
	    if ( g.IsDefinedInThisGroup(templateName) ) {
	        g.Error("redefinition of template: "+templateName);
	        st = new StringTemplate(); // create bogus template to fill in
	    }
	    else {
	        st = g.DefineTemplate(templateName, null);
	    }
	    }
		)
        {if ( st!=null ) {st.GroupFileLine = line;}}
	    LPAREN
	        (args[st]|{st.DefineEmptyFormalArgumentList();})
	    RPAREN
		DEFINED_TO_BE
	    (	t:STRING     {st.Template = t.getText();}
	    |	bt:BIGSTRING {st.Template = bt.getText();}
	    )
	|   alias:ID DEFINED_TO_BE target:ID
	    {g.DefineTemplateAlias(alias.getText(), target.getText());}
	;

args[StringTemplate st]
    :	arg[st] ( COMMA arg[st] )*
	;
	
arg[StringTemplate st]
{
	StringTemplate defaultValue = null;
}
	:	name:ID
		(	ASSIGN s:STRING
			{
			defaultValue=new StringTemplate("$_val_$");
			defaultValue.SetAttribute("_val_", s.getText());
			defaultValue.DefineFormalArgument("_val_");
			defaultValue.Name = "<"+st.Name+"'s arg "+name.getText()+" default value subtemplate>";
			}
		|	ASSIGN bs:ANONYMOUS_TEMPLATE
			{
			defaultValue=new StringTemplate(st.Group, bs.getText());
			defaultValue.Name = "<"+st.Name+"'s arg "+name.getText()+" default value subtemplate>";
			}
		)?
        {st.DefineFormalArgument(name.getText(), defaultValue);}
    ;

/*
suffix returns [int cardinality=FormalArgument.REQUIRED]
    :   OPTIONAL {cardinality=FormalArgument.OPTIONAL;}
    |   STAR     {cardinality=FormalArgument.ZERO_OR_MORE;}
    |   PLUS     {cardinality=FormalArgument.ONE_OR_MORE;}
	|
    ;
    */
    
mapdef[StringTemplateGroup g]
{
IDictionary m=null;
}
	:	name:ID
	    DEFINED_TO_BE m=map
	    {
	    if ( g.GetMap(name.getText())!=null ) {
	        g.Error("redefinition of map: "+name.getText());
	    }
	    else if ( g.IsDefinedInThisGroup(name.getText()) ) {
	        g.Error("redefinition of template as map: "+name.getText());
	    }
	    else {
	    	g.DefineMap(name.getText(), m);
	    }
	    }
	;

map returns [IDictionary mapping=new Hashtable()]
	:   LBRACK keyValuePair[mapping] (COMMA keyValuePair[mapping])* RBRACK
	;

keyValuePair[IDictionary mapping]
	:	key1:STRING COLON s1:STRING 	{mapping[key1.getText()]=s1.getText();}
	|	key2:STRING COLON s2:BIGSTRING  {mapping[key2.getText()]=s2.getText();}
	|	"default" COLON s3:STRING
	    {mapping[ASTExpr.DEFAULT_MAP_VALUE_NAME]=s3.getText();}
	|	"default" COLON s4:BIGSTRING
	    {mapping[ASTExpr.DEFAULT_MAP_VALUE_NAME]=s4.getText();}
	;    

class GroupLexer extends Lexer;

options {
	k=2;
	charVocabulary = '\u0000'..'\uFFFE';
}

ID	:	('a'..'z'|'A'..'Z') ('a'..'z'|'A'..'Z'|'0'..'9'|'-'|'_')*
	;

STRING
	:	'"'! ( '\\'! '"' | '\\' ~'"' | ~'"' )* '"'!
	;

BIGSTRING
	:	"<<"!
	 	(options {greedy=true;}:('\r'!)?'\n'! {newline();})? // consume 1st \n
		(	options {greedy=false;}  // stop when you see the >>
		:	{LA(3)=='>'&&LA(4)=='>'}? '\r'! '\n'! {newline();} // kill last \r\n
		|	{LA(2)=='>'&&LA(3)=='>'}? '\n'! {newline();}       // kill last \n
		|	('\r')? '\n' {newline();}                          // else keep
		|	.
		)*
        ">>"!
	;
	
ANONYMOUS_TEMPLATE
	:	'{'!
		(	options {greedy=false;}  // stop when you see the }
		:	('\r')? '\n' {newline();}                          // else keep
		|	.
		)*
	    '}'!
	;	

AT	:	'@' ;

LPAREN: '(' ;
RPAREN: ')' ;
LBRACK: '[' ;
RBRACK: ']' ;
COMMA:  ',' ;
DOT:  	'.' ;
DEFINED_TO_BE:  "::=" ;
SEMI:   ';' ;
COLON:  ':' ;
STAR:   '*' ;
PLUS:   '+' ;
ASSIGN:   '=' ;
OPTIONAL : '?' ;

// Single-line comments
SL_COMMENT
    :   "//"
        (~('\n'|'\r'))* (('\r')? '\n')?
        {$setType(Token.SKIP); newline();}
    ;

// multiple-line comments
ML_COMMENT
    :   "/*"
        (   
            options {
                greedy=false;
            }
        :   ('\r')? '\n'    {newline();}
        |   .
        )*
        "*/"
        {$setType(Token.SKIP);}
    ;

// Whitespace -- ignored
WS  :   (   ' '
        |   '\t'
        |   '\f'
        |   ('\r')? '\n' { newline(); }
        )+
        { $setType(Token.SKIP); }
    ;
