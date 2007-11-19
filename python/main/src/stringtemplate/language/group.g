
header {
from ASTExpr import *
import stringtemplate
import traceback
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
            self.group_.error("template group parse error", e)
        else:
            sys.stderr.write("template group parse error: " + str(e) + '\n')
            traceback.print_exc()
}

group[g]
{
    self.group_ = g
}
    :   "group" name:ID { g.setName(name.getText()) } 
        ( COLON s:ID {g.setSuperGroup(s.getText())} )?
        ( "implements" i:ID {g.implementInterface(i.getText())}
            ( COMMA i2:ID {g.implementInterface(i2.getText())} )*
        )?
        SEMI
        ( template[g] | mapdef[g] )*
        EOF
    ;

template[g]
{
    formalArgs = {}
    st = None
    ignore = False
    templateName = None
    line = self.LT(1).getLine()
}
    :   (   AT scope:ID DOT region:ID
            {
                templateName = g.getMangledRegionName(scope.getText(),region.getText())
                if g.isDefinedInThisGroup(templateName):
                    g.error("group "+g.getName()+" line "+str(line)+": redefinition of template region: @"+
                        scope.getText()+"."+region.getText())
                    st = stringtemplate.StringTemplate() // create bogus template to fill in

                else:
                    err = False
                    // @template.region() ::= "..."
                    scopeST = g.lookupTemplate(scope.getText())
                    if scopeST is None:
                        g.error("group "+g.getName()+" line "+str(line)+": reference to region within undefined template: "+
                            scope.getText())
                        err = True

                    if not scopeST.containsRegionName(region.getText()):
                        g.error("group "+g.getName()+" line "+str(line)+": template "+scope.getText()+" has no region called "+
                            region.getText())
                        err = True

                    if err:
                        st = stringtemplate.StringTemplate()

                    else:
                        st = g.defineRegionTemplate(
                            scope.getText(),
                            region.getText(),
                            None,
                            stringtemplate.StringTemplate.REGION_EXPLICIT
                        )

            }
        |   name:ID {templateName = name.getText()}
            {
                if g.isDefinedInThisGroup(templateName):
                    g.error("redefinition of template: " + templateName)
                    // create bogus template to fill in
                    st = stringtemplate.StringTemplate()
                else:
                    st = g.defineTemplate(templateName, None)
            }
        )
        {
            if st is not None:
                st.setGroupFileLine(line)
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
    :   LBRACK mapPairs[mapping] RBRACK
    ;

mapPairs[mapping]
    : keyValuePair[mapping] ( COMMA keyValuePair[mapping] )*
        (COMMA defaultValuePair[mapping])?
    | defaultValuePair[mapping]
    ;

defaultValuePair[mapping]
    : "default" COLON v=keyValue
        { mapping[stringtemplate.language.ASTExpr.DEFAULT_MAP_VALUE_NAME] = v }
    ;

keyValuePair[mapping]
    : key:STRING COLON v=keyValue { mapping[key.getText()] = v }
    ;

keyValue returns [value = None]
    :    s1:STRING
         { value = stringtemplate.StringTemplate(self.group_, s1.getText()) }
    |    s2:BIGSTRING
         { value = stringtemplate.StringTemplate(self.group_, s2.getText()) }
    |    k:ID { k.getText() == "key" }?
         { value = stringtemplate.language.ASTExpr.MAP_KEY_VALUE }
    |    
    ;

class GroupLexer extends Lexer;

options {
    k=2;
    charVocabulary = '\u0000'..'\uFFFE';
    testLiterals=false;
}

ID 
options {
    testLiterals=true;
}
    :   ('a'..'z'|'A'..'Z'|'_') ('a'..'z'|'A'..'Z'|'0'..'9'|'-'|'_')*
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
        | '\\'! '>'                 // \> escape
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
        : ( '\r' )? '\n' { $newline }   // else keep
        | '\\'! '>'                     // \> escape
        | .
        )*
        '}'!
    ;

AT:     '@' ;
LPAREN: '(' ;
RPAREN: ')' ;
LBRACK: '[' ;
RBRACK: ']' ;
COMMA:  ',' ;
DOT:    '.' ;
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
