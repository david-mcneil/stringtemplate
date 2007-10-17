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

/** Parse the individual attribute expressions */
class ActionParser extends Parser;
options {
	k = 2;
    buildAST = true;
    ASTLabelType = "Antlr.StringTemplate.Language.StringTemplateAST";
}

tokens {
    APPLY;  // template application
    MULTI_APPLY; // parallel array template application
    ARGS;   // subtree is a list of (possibly empty) arguments
    INCLUDE;// isolated template include (no attribute)
    CONDITIONAL="if";
    VALUE;  // used for (foo): #(VALUE foo)
    TEMPLATE;
    FUNCTION;
    SINGLEVALUEARG;
    LIST; // [a,b,c]
}

{
    protected StringTemplate self = null;

    public ActionParser(TokenStream lexer, StringTemplate self) : this(lexer, 2) {        
        this.self = self;
    }

	override public void reportError(RecognitionException e) {
        StringTemplateGroup group = self.Group;
        if ( group==StringTemplate.defaultGroup ) {
            self.Error("action parse error; template context is "+self.GetEnclosingInstanceStackString(), e);
        }
        else {
            self.Error("action parse error in group "+self.Group.Name+" line "+self.GroupFileLine+"; template context is "+self.GetEnclosingInstanceStackString(), e);
        }
	}
}

dummyTopRule returns [object dummy = null]
	:	dummy=action EOF
	;
	
action returns [IDictionary opts=null]
	:	templatesExpr (SEMI! opts=optionList)?
	|	"if"^ LPAREN! ifCondition RPAREN!
	;

optionList! returns [IDictionary opts=new Hashtable()]
    :   option[opts] (COMMA option[opts])*
	;
option[IDictionary opts]
{
	object v=null;
}
	:	i:ID
		( ASSIGN e:expr {v=#e;}
		| {v=ASTExpr.EMPTY_OPTION;}
		)
		{opts[#i.getText()] = v;}
    ;
    
templatesExpr
    :	expr
    	(	// parallelArrayTemplateApplication
    		//(expr COMMA) => expr (COMMA! expr)+ COLON anonymousTemplate
    		( COMMA ) => (COMMA! expr)+ COLON anonymousTemplate
	        {#templatesExpr =
	        	#(#[MULTI_APPLY,"MULTI_APPLY"],#templatesExpr);}
	    |	( 	c:COLON^ {#c.setType(APPLY);} template (COMMA! template)*
	    	)*
	    )
    ;

ifCondition
	:   expr
	|   NOT^ expr
	;

expr:   primaryExpr (PLUS^ primaryExpr)*
    ;

primaryExpr
		// templateInclude
	:	ID argList
        {#primaryExpr = #(#[INCLUDE,"include"], #primaryExpr);}
        
	|   // templateInclude
		"super"! DOT! qid:ID {#qid.setText("super."+#qid.getText());} argList
        {#primaryExpr = #(#[INCLUDE,"include"], #primaryExpr);}

    |	atom
    	( DOT^ // ignore warning on DOT ID
     	  ( ID
          | valueExpr
          )
     	)*
    |	function
    |	list
    
    |   // templateInclude  // (see past parens to arglist)
    	( indirectTemplate )=> indirectTemplate
        {#primaryExpr = #(#[INCLUDE,"include"], #primaryExpr);}
    |   valueExpr
    ;

valueExpr
	:   eval:LPAREN^ templatesExpr RPAREN!
        {#eval.setType(VALUE); #eval.setText("value");}
    ;

nonAlternatingTemplateExpr
    :   expr ( c:COLON^ {#c.setType(APPLY);} template )*
    ;
    
function
	:	(	"first"
   	 	|	"rest"
    	|	"last"
    	|	"length"
    	|	"strip"
    	|	"trunc"
    	)
    	singleArg
        {#function = #(#[FUNCTION],function);}
	;

template
    :   (   namedTemplate       // foo()
        |   anonymousTemplate   // {foo}
        )
        {#template = #(#[TEMPLATE],template);}
    ;

namedTemplate
	:	ID argList
	|   "super"! DOT! qid:ID {#qid.setText("super."+#qid.getText());} argList
    |   indirectTemplate
	;

anonymousTemplate
	:	t:ANONYMOUS_TEMPLATE
        {
        StringTemplate anonymous = new StringTemplate();
        anonymous.Group = self.Group;
        anonymous.EnclosingInstance = self;
        anonymous.Template = t.getText();
        anonymous.DefineFormalArguments(((StringTemplateToken)t).args);
        #t.StringTemplate = anonymous;
        }
	;

atom:   ID
	|	STRING
    |   INT
    |	ANONYMOUS_TEMPLATE
    ;

list:	lb:LBRACK^ {#lb.setType(LIST); #lb.setText("value");}
          expr (COMMA! expr)*
        RBRACK!
    ;

/** Match (foo)() and (foo+".terse")() */
indirectTemplate!
    :   LPAREN e:templatesExpr RPAREN args:argList
        {#indirectTemplate = #(#[VALUE,"value"],e,args);}
	;

argList
	:!	LPAREN! RPAREN! {#argList = #[ARGS,"ARGS"];}  // view()
	|	(singleArg)=>singleArg						  // bold(name)
	|	LPAREN! argumentAssignment (COMMA! argumentAssignment)* RPAREN!
        {#argList = #(#[ARGS,"ARGS"],#argList);}
	;

singleArg
	:	LPAREN! nonAlternatingTemplateExpr RPAREN!
        {#singleArg = #(#[SINGLEVALUEARG,"SINGLEVALUEARG"],#singleArg);}
    ;

argumentAssignment
	:	ID ASSIGN^ nonAlternatingTemplateExpr
	|	DOTDOTDOT
	;

class ActionLexer extends Lexer;

options {
    k=2;
	charVocabulary = '\003'..'\uFFFE';
    testLiterals=false;
}

ID
options {
    testLiterals=true;
}
    :	('a'..'z'|'A'..'Z'|'_') ('a'..'z'|'A'..'Z'|'0'..'9'|'_'|'/')*
	;

INT :   ('0'..'9')+ ;

STRING
	:	'"'! (ESC_CHAR[true] | ~'"')* '"'!
	;

ANONYMOUS_TEMPLATE
{
IList args=null;
StringTemplateToken t = null;
}
	:	'{'!
	        ( (TEMPLATE_ARGS)=> args=TEMPLATE_ARGS (options{greedy=true;}:WS_CHAR!)?
	          {
	          // create a special token to track args
	          t = new StringTemplateToken(ANONYMOUS_TEMPLATE,$getText,args);
	          $setToken(t);
	          }
	        |
	        )
	        ( '\\'! '{' | '\\'! '}' | ESC_CHAR[false] | NESTED_ANONYMOUS_TEMPLATE | ~'}' )*
	        {
	        if ( t!=null ) {
	        	t.setText($getText);
	        }
	        }
	    '}'!
	;

protected
TEMPLATE_ARGS returns [IList args=new ArrayList()]
	:!	(WS_CHAR)? a:ID {args.Add(a.getText());}
	    (options{greedy=true;}:(WS_CHAR)? ',' (WS_CHAR)? a2:ID {args.Add(a2.getText());})*
	    (WS_CHAR)? '|'
	;

protected
NESTED_ANONYMOUS_TEMPLATE
	:	'{' ( '\\'! '{' | '\\'! '}' | ESC_CHAR[false] | NESTED_ANONYMOUS_TEMPLATE | ~'}' )* '}'
	;

/** Match escape sequences, optionally translating them for strings, but not
 *  for templates.  Do \} only when in {...} templates.
 */
protected
ESC_CHAR[bool doEscape]
	:	'\\'
		(	options {generateAmbigWarnings=false;} // . ambig with others
		:   'n' {if (doEscape) { $setText("\n"); }}
		|	'r' {if (doEscape) { $setText("\r"); }}
		|	't' {if (doEscape) { $setText("\t"); }}
		|	'b' {if (doEscape) { $setText("\b"); }}
		|	'f' {if (doEscape) { $setText("\f"); }}
		|   c:. {if (doEscape) {$setText(c);}}
		)
	;

LBRACK: '[' ;
RBRACK: ']' ;
LPAREN : '(' ;
RPAREN : ')' ;
COMMA  : ',' ;
DOT    : '.' ;
ASSIGN : '=' ;
COLON  : ':' ;
PLUS   : '+' ;
SEMI   : ';' ;
NOT	   : '!' ;
DOTDOTDOT : "..." ;

WS	:	(' '|'\t'|'\r'|'\n'{newline();})+ {$setType(Token.SKIP);}
	;

protected
WS_CHAR
	:	' '|'\t'|'\r'|'\n'{newline();}
	;