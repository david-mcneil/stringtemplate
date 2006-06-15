header {
import stringtemplate
from ChunkToken import ChunkToken
from StringRef import StringRef
from NewlineRef import NewlineRef
}

header "TemplateParser.__init__" {
    self.this = None
}

header "DefaultTemplateLexer.__init__" {
    self.currentIndent = None
    //self.this = None
}

options {
    language = "Python";
}

/** A parser used to break up a single template into chunks, text literals
 *  and attribute expressions.
 */
class TemplateParser extends Parser;

{
    def reportError(self, e):
        self.error("template parse error", e)
}

template[this]
    :   (   s:LITERAL {this.addChunk(StringRef(this,s.getText()))}
        |   nl:NEWLINE
            {
              if self.LA(1) != ELSE and self.LA(1) != ENDIF:
                  this.addChunk(NewlineRef(this,nl.getText()))
            }
        |   action[this]
        )*
    ;

action[this]
    :   a:ACTION
        {
            indent = a.getIndentation()
            c = this.parseAction(a.getText())
            c.setIndentation(indent)
            this.addChunk(c)
        }

    |   i:IF
        {
            c = this.parseAction(i.getText())
            // create and precompile the subtemplate
            subtemplate = stringtemplate.StringTemplate(this.getGroup(), None)
            subtemplate.setEnclosingInstance(this)
            subtemplate.setName(i.getText() + " subtemplate")
            this.addChunk(c)
        }

        template[subtemplate] { if c: c.setSubtemplate(subtemplate) }

        (   ELSE
            {
                // create and precompile the subtemplate
                elseSubtemplate = stringtemplate.StringTemplate(this.getGroup(), None)
                elseSubtemplate.setEnclosingInstance(this)
                elseSubtemplate.setName("else subtemplate")
            }

            template[elseSubtemplate]
            { if c: c.setElseSubtemplate(elseSubtemplate) }
        )?

        ENDIF
    ;

/** Break up an input text stream into chunks of either plain text
 *  or template actions in "$...$".  Treat IF and ENDIF tokens
 *  specially.
 */
class DefaultTemplateLexer extends Lexer;

options {
    k=7; // see "$endif$"
//    charVocabulary = '\u0001'..'\uFFFE';
}

{
    def initialize(self, this, r):
        super(Lexer, self).__init__(r)
        self.this = this

    def reportError(self, e):
        self.error("template parse error", e)

    def upcomingELSE(self, i):
        return self.LA(i) == '$' and \
               self.LA(i+1) == 'e' and \
               self.LA(i+2) == 'l' and \
               self.LA(i+3) == 's' and \
               self.LA(i+4) == 'e' and \
               self.LA(i+5) == '$'

    def upcomingENDIF(self, i):
        return self.LA(i) == '$' and \
               self.LA(i+1) == 'e' and \
               self.LA(i+2) == 'n' and \
               self.LA(i+3) == 'd' and \
               self.LA(i+4) == 'i' and \
               self.LA(i+5) == 'f' and \
               self.LA(i+6) == '$'

}

LITERAL
    :   { self.LA(1) != '\r' and self.LA(1) != '\n' }?
        ( options { generateAmbigWarnings=false; }
          {
            loopStartIndex = self.text.length()
            col = self.getColumn()
          }
        : '\\'! '$'  // allow escaped delimiter
        | '\\' ~'$'  // otherwise ignore escape char
        | ind:INDENT
          {
              if col == 1 and self.LA(1) == '$':
                  // store indent in ASTExpr not in a literal
                  self.currentIndent = ind.getText()
                  // reset length to wack text
                  self.text.setLength(loopStartIndex)
              else:
                  self.currentIndent = None
          }
        | ~('$'|'\r'|'\n')
        )+
        {
          if not len($getText):
              $skip // pure indent?
        }
    ;

NEWLINE
    :   NL
        {
          $newline
          self.currentIndent = None
        }
    ;
 
ACTION
    options {
        generateAmbigWarnings=false; // $EXPR$ is ambig with $!..!$
    }
{
    startCol = self.getColumn()
}
    :   "$\\n$"! {$setText('\n'); $setType(LITERAL) }
    |   "$\\r$"! {$setText('\r'); $setType(LITERAL) }
    |   "$\\t$"! {$setText('\t'); $setType(LITERAL) }
    |   "$\\ $"! {$setText(' '); $setType(LITERAL) }
    |   COMMENT {$skip}
    |   ( // $EXPR$ is ambig with $endif$ etc...
          options { generateAmbigWarnings=false; }
        : '$'! "if" (' '!)* "(" IF_EXPR ")" '$'! { $setType(IF) }
          ( NL! { $newline} )? // ignore any newline right after an IF
        | '$'! "else" '$'! { $setType(ELSE) }
          ( NL! { $newline} )? // ignore any newline right after an ELSE
        | '$'! "endif" '$'! { $setType(ENDIF) }
          // ignore after ENDIF if on line by itself
          ( { startCol == 1 }? NL! { $newline } )?
        | '$'! EXPR '$'! // (Can't start with '!', which would mean comment)
        )
        {
            t = ChunkToken(_ttype, $getText, self.currentIndent)
            $setToken(t);
        }
    ;

protected
NL
// single '\r' is ambig with '\r' '\n'
options { generateAmbigWarnings = false; }
    :   '\r'
    |   '\n'
    |   '\r' '\n'
    ;

protected
EXPR
    :   ( ESC
        | NL {$newline}
        | SUBTEMPLATE
        | '=' TEMPLATE
        | '=' SUBTEMPLATE
        | '=' ~('"'|'<'|'{')
        | ~'$'
        )+
    ;

protected
TEMPLATE
    :   '"' ( ESC | ~'"' )+ '"'
    |   "<<"
        ( options { greedy = true; }
        : ( '\r'! )? '\n'! { $newline } )? // consume 1st \n
        ( options { greedy = false; }      // stop when you see the >>
        : { self.LA(3) == '>' and self.LA(4) == '>' }?
          '\r'! '\n'! { $newline }         // kill last \r\n
        | { self.LA(2) == '>' and self.LA(3) == '>' }?
          '\n'! { $newline }               // kill last \n
        | ('\r')? '\n' { $newline }        // else keep
        | .
        )*
        ">>"
    ;

protected
IF_EXPR
    :   ( ESC
        | ( '\r' )? '\n' { $newline }
        | SUBTEMPLATE
        | NESTED_PARENS
        | ~')'
        )+
    ;

protected
ESC :   '\\' ('$'|'n'|'t'|'\\'|'"'|'\''|':'|'{'|'}')
    ;

protected
SUBTEMPLATE
    :    '{' (SUBTEMPLATE|ESC|~'}')+ '}'
    ;

protected
NESTED_PARENS
    :    '(' ( options { greedy = false; } : NESTED_PARENS | ESC | ~')' )+ ')'
    ;

protected
INDENT
    :   ( options { greedy = true; } : ' ' | '\t' )+
    ;

protected
COMMENT
{
    startCol = self.getColumn()
}
    :   "$!"
        ( options { greedy = false; }
        : NL { $newline }
        | .
        )*
        "!$" ( { startCol == 1 }? NL { $newline } )?
    ;
