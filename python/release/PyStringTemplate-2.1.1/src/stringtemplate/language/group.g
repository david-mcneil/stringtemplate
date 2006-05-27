
header {
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
        self.error("template parse error", e)
}

group[g]
        :       "group" name:ID {g.setName(name.getText())} SEMI
            (template[g])*
    ;

template[g]
{
    formalArgs = {}
    st = None
    ignore = False
}
        :       name:ID
            {
                if g.isDefinedInThisGroup(name.getText()):
                    g.error("redefinition of template: " + name.getText())
                    st = stringtemplate.StringTemplate() // create bogus template to fill in
                else:
                    st = g.defineTemplate(name.getText(), None)
            }
            LPAREN
                (args[st] | { st.defineEmptyFormalArgumentList() })
            RPAREN
            DEFINED_TO_BE t:TEMPLATE { st.setTemplate(t.getText()) }

        |   alias:ID DEFINED_TO_BE target:ID
            { g.defineTemplateAlias(alias.getText(), target.getText()) }
        ;

args[st]
    :   name:ID { st.defineFormalArgument(name.getText()) }
        (COMMA name2:ID { st.defineFormalArgument(name2.getText()) })*
        ;

// suffix returns [int cardinality=FormalArgument.REQUIRED]
//     :   OPTIONAL {cardinality=FormalArgument.OPTIONAL;}
//     |   STAR     {cardinality=FormalArgument.ZERO_OR_MORE;}
//     |   PLUS     {cardinality=FormalArgument.ONE_OR_MORE;}
//     |
//     ;

class GroupLexer extends Lexer;

options {
        k=2;
        charVocabulary = '\u0000'..'\uFFFE';
}

ID      :       ('a'..'z'|'A'..'Z') ('a'..'z'|'A'..'Z'|'0'..'9'|'-'|'_')*
        ;

TEMPLATE
        :       '"'! ( '\\'! '"' | '\\' ~'"' | ~'"' )+ '"'!
        |       "<<"!
                (options {greedy=true;}: NL! { $newline })? // consume 1st newline
                (       options {greedy=false;}  // stop when you see the >>
                :       { self.LA(3)=='>' and self.LA(4)=='>' }? '\r'! '\n'! { $newline } // kill last \r\n
                |       { self.LA(2)=='>' and self.LA(3)=='>' }? '\n'! { $newline }       // kill last \n
                |       { self.LA(2)=='>' and self.LA(3)=='>' }? '\r'! { $newline }       // kill last \r
                |       NL { $newline }                          // else keep
                |       .
                )*
        ">>"!
        ;

LPAREN: '(' ;
RPAREN: ')' ;
COMMA:  ',' ;
DEFINED_TO_BE:  "::=" ;
SEMI:   ';' ;
STAR:   '*' ;
PLUS:   '+' ;
OPTIONAL : '?' ;

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
