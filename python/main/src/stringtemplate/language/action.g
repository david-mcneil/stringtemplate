header {
import stringtemplate
}

header "ActionParser.__init__" {
    if len(args) > 1 and isinstance(args[1], stringtemplate.StringTemplate):
        self.this = args[1]
    else:
        raise ValueError("ActionParser requires a StringTemplate instance")
}

options {
    language = "Python";
}

/** Parse the individual attribute expressions */
class ActionParser extends Parser;
options {
    k = 2;
    buildAST = true;
    ASTLabelType = "stringtemplate.language.StringTemplateAST";
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
    def reportError(self, e):
        self.this.error("template parse error", e)
}


action returns [opts = None]
        :       templatesExpr (SEMI! opts=optionList)?
        |       "if"^ LPAREN! ifCondition RPAREN!
        ;

optionList! returns [opts = {}]
        :   "separator" ASSIGN e:expr {opts["separator"] = #e}
        ;

templatesExpr
        :   expr
            ( c:COLON^ { #c.setType(APPLY) }
              template ( COMMA! template )*
            )*
        ;

ifCondition
        :   ifAtom
        |   NOT^ ifAtom
        ;

ifAtom
        :   expr
        ;

expr
        :   primaryExpr ( PLUS^ primaryExpr )*
        ;

primaryExpr
        :   atom
            ( DOT^
              ( ID
              | valueExpr
              )
            )*
        |   (templateInclude)=>templateInclude
            // (see past parens to arglist)
        |   valueExpr
        ;

valueExpr
        :   eval:LPAREN^ templatesExpr RPAREN!
            // parens implies "evaluate to string"
            {
                #eval.setType(VALUE)
                #eval.setText("value")
            }
        ;

/*
objPropertyRef
        :   ID
            ( DOT^
              ( ID
              | lp:LPAREN^ expr RPAREN!
                {
                    #lp.setType(VALUE)
                    #lp.setText("value")
                }
              )
            )+
        ;
*/

nonAlternatingTemplateExpr
        :   expr ( c:COLON^ {#c.setType(APPLY)} template )*
        ;

template
        :   (   namedTemplate       // foo()
            |   anonymousTemplate   // {foo}
            )
            { #template = #(#[TEMPLATE],template) }
        ;

namedTemplate
        :       ID argList
        |       "super"! DOT! qid:ID
                {#qid.setText("super."+#qid.getText())} argList
        |       indirectTemplate
        ;

anonymousTemplate
        :       t:ANONYMOUS_TEMPLATE
        {
            anonymous = stringtemplate.StringTemplate()
            anonymous.setGroup(self.this.getGroup())
            anonymous.setEnclosingInstance(self.this)
            anonymous.setTemplate(t.getText())
            #t.setStringTemplate(anonymous)
        }
        ;

atom
        :   ID
        |   STRING
        |   INT
        ;

templateInclude
        :       ( ID argList
                | "super"! DOT! qid:ID
                  {#qid.setText("super."+#qid.getText())} argList
                | indirectTemplate
                )
                {#templateInclude = #(#[INCLUDE,"include"], templateInclude)}
        ;

/** Match (foo)() and (foo+".terse")()
 *  breaks encapsulation
 */
indirectTemplate!
        :   LPAREN e:expr RPAREN args:argList
            { #indirectTemplate = #(#[VALUE,"value"],e,args) }
        ;

argList
        :!      LPAREN! RPAREN! {#argList = #[ARGS,"ARGS"];}
        |       LPAREN! argumentAssignment (COMMA! argumentAssignment)* RPAREN!
                {#argList = #(#[ARGS,"ARGS"],#argList)}
        ;

argumentAssignment
        :       ID ASSIGN^ nonAlternatingTemplateExpr
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
        :       ('a'..'z'|'A'..'Z'|'_') ('a'..'z'|'A'..'Z'|'0'..'9'|'_'|'/')*
        ;

INT
        :       ('0'..'9')+
        ;

STRING
        :       '"'! (ESC_CHAR[True] | ~'"')* '"'!
        ;

ANONYMOUS_TEMPLATE
        :       '{'! (ESC_CHAR[False] | NESTED_ANONYMOUS_TEMPLATE | ~'}')* '}'!
        ;

protected
NESTED_ANONYMOUS_TEMPLATE
        :       '{' (ESC_CHAR[False] | NESTED_ANONYMOUS_TEMPLATE | ~'}')* '}'
        ;

protected
ESC_CHAR[doEscape]
        :       '\\'
                ( options {generateAmbigWarnings=false;} // . ambig with others
                : 'n' {if doEscape: $setText("\n") }
                | 'r' {if doEscape: $setText("\r") }
                | 't' {if doEscape: $setText("\t") }
                | 'b' {if doEscape: $setText("\b") }
                | 'f' {if doEscape: $setText("\f") }
                | c:. {if doEscape: $setText(str(c)) }
                )
        ;

LPAREN  : '(' ;
RPAREN  : ')' ;
COMMA   : ',' ;
DOT     : '.' ;
ASSIGN  : '=' ;
COLON   : ':' ;
PLUS    : '+' ;
SEMI    : ';' ;
NOT     : '!' ;

WS
        :   ( ' '
            | '\t'
            | ( options { generateAmbigWarnings=false; }
              : '\r'
              | '\n'
              | '\r' '\n'
              ) { $newline }
            ) { $skip }
        ;
