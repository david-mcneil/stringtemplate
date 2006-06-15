header {
from StringTemplateToken import *
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
    MULTI_APPLY;  // parallel array template application
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
    def reportError(self, e):
        self.this.error("template parse error", e)
}


action returns [opts = None]
    :   templatesExpr (SEMI! opts=optionList)?
    |   "if"^ LPAREN! ifCondition RPAREN!
    ;

optionList! returns [opts = {}]
    :   "separator" ASSIGN e:expr {opts["separator"] = #e}
    ;

templatesExpr
    :   (parallelArrayTemplateApplication)=> parallelArrayTemplateApplication
    |	expr ( c:COLON^ { #c.setType(APPLY) } template (COMMA! template)* )*
    ;

parallelArrayTemplateApplication
    :   expr (COMMA! expr)+ c:COLON anonymousTemplate
        {
            #parallelArrayTemplateApplication = \
                #(#[MULTI_APPLY, "MULTI_APPLY"],
                  parallelArrayTemplateApplication)
        }
    ;

ifCondition
    :   ifAtom
    |   NOT^ ifAtom
    ;

ifAtom
    :   expr
    ;

expr
    :   primaryExpr (PLUS^ primaryExpr)*
    ;

primaryExpr
    :   atom
        ( DOT^
          ( ID
          | valueExpr
          )
        )*
    |   (templateInclude) => templateInclude  // (see past parens to arglist)
    |   function
    |   valueExpr
    |   list_
    ;

valueExpr
    :   eval:LPAREN^ templatesExpr RPAREN!
        {
            #eval.setType(VALUE)
            #eval.setText("value")
        }
    ;

nonAlternatingTemplateExpr
    :   expr ( c:COLON^ { #c.setType(APPLY) } template )*
    ;

function
    :   ( "first"
        | "rest"
        | "last"
        )
        singleArg
        { #function = #(#[FUNCTION], function) }
    ;

template
    :   ( namedTemplate       // foo()
        | anonymousTemplate   // {foo}
        )
        { #template = #(#[TEMPLATE],template) }
    ;

namedTemplate
    :   ID argList
    |   "super"! DOT! qid:ID
        { #qid.setText("super."+#qid.getText()) } argList
    |   indirectTemplate
    ;

anonymousTemplate
    :   t:ANONYMOUS_TEMPLATE
        {
            anonymous = stringtemplate.StringTemplate()
            anonymous.setGroup(self.this.getGroup())
            anonymous.setEnclosingInstance(self.this)
            anonymous.setTemplate(t.getText())
            anonymous.defineFormalArgument(t.args)
            #t.setStringTemplate(anonymous)
        }
    ;

atom
    :   ID
    |   STRING
    |   INT
    |   ANONYMOUS_TEMPLATE
    ;

list_
    :   lb:LBRACK^
        {
            #lb.setType(LIST)
            #lb.setText("value")
        }
        expr (COMMA! expr)*
        RBRACK!
    ;

templateInclude
    :   ( ID argList
        | "super"! DOT! qid:ID
          { #qid.setText("super."+#qid.getText()) } argList
        | indirectTemplate
        )
        { #templateInclude = #(#[INCLUDE, "include"], templateInclude) }
    ;

/** Match (foo)() and (foo+".terse")() */
indirectTemplate!
    :   LPAREN e:expr RPAREN args:argList
        { #indirectTemplate = #(#[VALUE, "value"], e, args) }
    ;

argList
    :!  LPAREN! RPAREN! { #argList = #[ARGS,"ARGS"] }  // view()
    |   (singleArg)=>singleArg // bold(name)
    |   LPAREN! argumentAssignment (COMMA! argumentAssignment)* RPAREN!
        { #argList = #(#[ARGS, "ARGS"], #argList) }
    ;

singleArg
    :   LPAREN! nonAlternatingTemplateExpr RPAREN!
        { #singleArg = #(#[SINGLEVALUEARG, "SINGLEVALUEARG"], #singleArg) }
    ;

argumentAssignment
    :   ID ASSIGN^ nonAlternatingTemplateExpr
    |   DOTDOTDOT
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
    :   ('a'..'z'|'A'..'Z'|'_') ('a'..'z'|'A'..'Z'|'0'..'9'|'_'|'/')*
    ;

INT
    :   ('0'..'9')+ ;

STRING
    :   '"'! (ESC_CHAR[True] | ~'"')* '"'!
    ;

ANONYMOUS_TEMPLATE
{
    args = None
    t = None
}
    :   '{'!
          ( (TEMPLATE_ARGS)=> args=TEMPLATE_ARGS
            ( options { greedy = true; } : WS_CHAR! )?
            {
                // create a special token to track args
                t = StringTemplateToken(ANONYMOUS_TEMPLATE, $getText, args)
                $setToken(t)
            }
          | // empty
          )
          ( ESC_CHAR[False] | NESTED_ANONYMOUS_TEMPLATE | ~'}' )*
          {
              if t:
                  t.setText($getText)
          }
        '}'!
    ;

protected
TEMPLATE_ARGS returns [args=[]]
    :!  (WS_CHAR)? a:ID { args.append(a.getText()) }
        ( options { greedy = true; }
          : (WS_CHAR)? ',' (WS_CHAR)? a2:ID { args.append(a2.getText()) } )*
        (WS_CHAR)? '|'
    ;

protected
NESTED_ANONYMOUS_TEMPLATE
    :   '{' (ESC_CHAR[False] | NESTED_ANONYMOUS_TEMPLATE | ~'}')* '}'
    ;

protected
ESC_CHAR[doEscape]
    :   '\\'
        ( options {
            generateAmbigWarnings=false;
          } // . ambig with others
        : 'n' {if doEscape: $setText("\n") }
        | 'r' {if doEscape: $setText("\r") }
        | 't' {if doEscape: $setText("\t") }
        | 'b' {if doEscape: $setText("\b") }
        | 'f' {if doEscape: $setText("\f") }
        | c:. {if doEscape: $setText(str(c)) }
        )
    ;

LBRACK : '[' ;
RBRACK : ']' ;
LPAREN : '(' ;
RPAREN : ')' ;
COMMA  : ',' ;
DOT    : '.' ;
ASSIGN : '=' ;
COLON  : ':' ;
PLUS   : '+' ;
SEMI   : ';' ;
NOT    : '!' ;
DOTDOTDOT : "..." ;

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

protected
WS_CHAR
    :   ' '
    |   '\t'
    |   ( options { generateAmbigWarnings=false; }
        : '\r'
        | '\n'
        | '\r' '\n'
        ) { $newline }
    ;
