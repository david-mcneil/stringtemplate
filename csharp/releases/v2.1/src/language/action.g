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
	namespace="antlr.stringtemplate.language";
}

/** Parse the individual attribute expressions */
class ActionParser extends Parser;
options {
	k = 2;
    buildAST = true;
    ASTLabelType = "antlr.stringtemplate.language.StringTemplateAST";
}

tokens {
    APPLY;  // template application
    ARGS;   // subtree is a list of (possibly empty) arguments
    INCLUDE;// isolated template include (no attribute)
    CONDITIONAL="if";
    VALUE;  // used for (foo): #(VALUE foo)
    TEMPLATE;
}

{
    protected StringTemplate self = null;

    public ActionParser(TokenStream lexer, StringTemplate self) : this(lexer, 2) {        
        this.self = self;
    }

	override public void reportError(RecognitionException e) {
		self.error("template parse error", e);
	}
}

/*
test!: (    a:action
            {
                System.out.println(#a.toStringList());
                StringTemplateLanguageEvaluator eval =
                    new StringTemplateLanguageEvaluator();
                eval.expr(#a);
            }
        )+ ;

        */
action returns [IDictionary opts=null]
	:	templatesExpr (SEMI! opts=optionList)?
	|	"if"^ LPAREN! ifCondition RPAREN!
	;

optionList! returns [IDictionary opts=new Hashtable()]
    :   "separator" ASSIGN e:expr {opts["separator"]=#e;}
    ;

ifCondition
	:   ifAtom
	|   NOT^ ifAtom
	;

ifAtom
    :   expr
    ;

expr:   atom (PLUS^ atom)*
    ;

atom:   attribute
    |   (templateInclude)=>templateInclude  // (see past parens to arglist)
    |   eval:LPAREN^ templatesExpr RPAREN!  // parens implies "evaluate to string"
        {#eval.setType(VALUE);}
    ;

templatesExpr
    :   expr ( c:COLON^ {#c.setType(APPLY);} template (COMMA! template)* )*
    ;

nonAlternatingTemplateExpr
    :   expr ( c:COLON^ {#c.setType(APPLY);} template )*
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
        anonymous.setGroup(self.getGroup());
        anonymous.setEnclosingInstance(self);
        anonymous.setTemplate(t.getText());
        #t.setStringTemplate(anonymous);
        }
	;

attribute
    :   ID
    |   objPropertyRef
	|	STRING
    |   INT
    ;

templateInclude
	:	(   ID argList
	    |   "super"! DOT! qid:ID {#qid.setText("super."+#qid.getText());} argList
        |   indirectTemplate
	    )
        {#templateInclude = #(#[INCLUDE,"include"], templateInclude);}
    ;

/** Match (foo)() and (foo+".terse")()
    breaks encapsulation
 */
indirectTemplate!
    :   LPAREN e:expr RPAREN args:argList
        {#indirectTemplate = #(#[VALUE,"value"],e,args);}
	;

objPropertyRef
    :   ID (DOT^ ID)+
    ;

argList
	:!	LPAREN! RPAREN! {#argList = #[ARGS,"ARGS"];}
	|	LPAREN! argumentAssignment (COMMA! argumentAssignment)* RPAREN!
        {#argList = #(#[ARGS,"ARGS"],#argList);}
	;

argumentAssignment
	:	ID ASSIGN^ nonAlternatingTemplateExpr
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
	:	'{'! (ESC_CHAR[false] | NESTED_ANONYMOUS_TEMPLATE | ~'}')* '}'!
	;

protected
NESTED_ANONYMOUS_TEMPLATE
	:	'{' (ESC_CHAR[false] | NESTED_ANONYMOUS_TEMPLATE | ~'}')* '}'
	;

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

LPAREN : '(' ;
RPAREN : ')' ;
COMMA  : ',' ;
DOT    : '.' ;
ASSIGN : '=' ;
COLON  : ':' ;
PLUS   : '+' ;
SEMI   : ';' ;
NOT	   : '!' ;

WS	:	(' '|'\t'|'\r'|'\n'{newline();})+ {$setType(Token.SKIP);}
	;

