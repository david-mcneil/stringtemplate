
header {
from ASTExpr import *
import stringtemplate
}

header "GroupParser.__init__" {
    self.group_ = None
}

options {
    language="Python";
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
    def reportError(self, e):
        if self.group_:
            self.group_.error("template parse error", e)
        else:
            sys.stderr.write("template parse error: " + str(e) + '\n')
}

group[g]
    :   "group" name:ID { g.setName(name.getText()) } SEMI
        ( template[g] | mapdef[g] )*
    ;

template[g]
{
    formalArgs = {}
    st = None
    ignore = False
}
    :   name:ID
        {
            if g.isDefinedInThisGroup(name.getText()):
                g.error("redefinition of template: " + name.getText())
                // create bogus template to fill in
                st = stringtemplate.StringTemplate()
            else:
                st = g.defineTemplate(name.getText(), None)
        }
        LPAREN
        ( args[st] | { st.defineEmptyFormalArgumentList() } )
        RPAREN
        DEFINED_TO_BE
        ( t:STRING { st.setTemplate(t.getText()) }
        | bt:BIGSTRING { st.setTemplate(bt.getText()) }
        )
    |   alias:ID DEFINED_TO_BE target:ID
        { g.defineTemplateAlias(alias.getText(), target.getText()) }
    ;

args[st]
    :    arg[st] ( COMMA arg[st] )*
    ;

arg[st]
{
    defaultValue = None
}
    :   name:ID
        ( ASSIGN s:STRING
          {
              defaultValue = stringtemplate.StringTemplate("$_val_$")
              defaultValue["_val_"] = s.getText()
              defaultValue.defineFormalArgument("_val_")
              defaultValue.setName("<" + st.getName() + "'s arg " + \
                                   name.getText() + \
                                   " default value subtemplate>")
          }
        | ASSIGN bs:ANONYMOUS_TEMPLATE
          {
              defaultValue = stringtemplate.StringTemplate(st.getGroup(), \
                  bs.getText())
              defaultValue.setName("<" + st.getName() + "'s arg " + \
                                   name.getText() + \
                                   " default value subtemplate>")
          }
        )?
        { st.defineFormalArgument(name.getText(), defaultValue) }
    ;

// suffix returns [int cardinality=FormalArgument.REQUIRED]
//     :   OPTIONAL {cardinality=FormalArgument.OPTIONAL;}
//     |   STAR     {cardinality=FormalArgument.ZERO_OR_MORE;}
//     |   PLUS     {cardinality=FormalArgument.ONE_OR_MORE;}
//     |
//     ;

mapdef[g]
{
    m = None
}
    :   name:ID
        DEFINED_TO_BE m=map
        {
            if g.getMap(name.getText()):
                g.error("redefinition of map: " + name.getText())
            elif g.isDefinedInThisGroup(name.getText()):
                g.error("redefinition of template as map: " + name.getText())
            else:
                g.defineMap(name.getText(), m)
        }
    ;

map returns [mapping = {}]
    :   LBRACK keyValuePair[mapping] ( COMMA keyValuePair[mapping] )* RBRACK
    ;

keyValuePair[mapping]
    :    key1:STRING COLON s1:STRING
         { mapping[key1.getText()] = s1.getText() }
    |    key2:STRING COLON s2:BIGSTRING
         { mapping[key2.getText()] = s2.getText() }
    |    "default" COLON s3:STRING
         { mapping[ASTExpr.DEFAULT_MAP_VALUE_NAME] = s3.getText() }
    |    "default" COLON s4:BIGSTRING
         { mapping[ASTExpr.DEFAULT_MAP_VALUE_NAME] = s4.getText() }
    ;

class GroupLexer extends Lexer;

options {
        k=2;
        charVocabulary = '\u0000'..'\uFFFE';
}

ID      :       ('a'..'z'|'A'..'Z') ('a'..'z'|'A'..'Z'|'0'..'9'|'-'|'_')*
        ;

STRING
    :   '"'! ( '\\'! '"' | '\\' ~'"' | ~'"' )* '"'!
    ;

BIGSTRING
    :   "<<"!
        ( options { greedy = true; }
          : NL! { $newline } )?       // consume 1st newline
        ( options { greedy = false; } // stop when you see the >>
        : { self.LA(3) == '>' and self.LA(4) == '>' }?
          '\r'! '\n'! { $newline }  // kill last \r\n
        | { self.LA(2) == '>' and self.LA(3) == '>' }?
          '\n'! { $newline }        // kill last \n
        | { self.LA(2) == '>' and self.LA(3) == '>' }?
          '\r'! { $newline }        // kill last \r
        | NL { $newline }           // else keep
        | .
        )*
        ">>"!
    ;

ANONYMOUS_TEMPLATE
{
    args = None
    t = None
}
    :   '{'!
        ( options { greedy = false; }   // stop when you see the >>
        : ( '\r' )? '\n' { $newline } // else keep
        | .
        )*
        '}'!
    ;

LPAREN: '(' ;
RPAREN: ')' ;
LBRACK: '[' ;
RBRACK: ']' ;
COMMA:  ',' ;
DEFINED_TO_BE: "::=" ;
SEMI:   ';' ;
COLON:  ':' ;
STAR:   '*' ;
PLUS:   '+' ;
ASSIGN: '=' ;
OPTIONAL: '?' ;

// Single-line comments
SL_COMMENT
    :   "//"
        (~('\n'|'\r'))* (NL)?
        { $skip; $newline }
    ;

// multiple-line comments
ML_COMMENT
    :   "/*"
        (   
            options {
                greedy=false;
            }
        :   NL    { $newline }
        |   .
        )*
        "*/"
        { $skip }
    ;

protected
NL
options {
    generateAmbigWarnings=false; // single '\r' is ambig with '\r' '\n'
}
        : '\r'
        | '\n'
        | '\r' '\n'
    ;

// Whitespace -- ignored
WS  :   (   ' '
        |   '\t'
        |   '\f'
        |   NL { $newline }
        )+
        { $skip }
    ;
