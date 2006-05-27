header {
import stringtemplate
import stringtemplate.language.TemplateParser
from ChunkToken import ChunkToken
}

header "AngleBracketTemplateLexer.__init__" {
    self.currentIndent = None
    self.this = None
}

options {
    language = "Python";
}

/** Break up an input text stream into chunks of either plain text
 *  or template actions in "<...>".  Treat IF and ENDIF tokens
 *  specially.
 */
class AngleBracketTemplateLexer extends Lexer;

options {
    importVocab = TemplateParser;
    k=7; // see "<endif>"
    charVocabulary = '\u0001'..'\uFFFE';
}

{
    def initialize(self, this, r):
        self.this = this

    def reportError(self, e):
        self.error("template parse error", e)

    def upcomingELSE(self, i):
        return self.LA(i) == '<' and \
               self.LA(i+1) == 'e' and \
               self.LA(i+2) == 'l' and \
               self.LA(i+3) == 's' and \
               self.LA(i+4) == 'e' and \
               self.LA(i+5) == '>'

    def upcomingENDIF(self, i):
        return self.LA(i) == '<' and \
               self.LA(i+1) == 'e' and \
               self.LA(i+2) == 'n' and \
               self.LA(i+3) == 'd' and \
               self.LA(i+4) == 'i' and \
               self.LA(i+5) == 'f' and \
               self.LA(i+6) == '>'

}

LITERAL
    :   { self.LA(1) != '\r' and self.LA(1) != '\n' }?
        ( options { generateAmbigWarnings=false; }
          {
            loopStartIndex = self.text.length()
            col = self.getColumn()
          }
        : '\\'! '<'  // allow escaped delimiter
        | '\\'! '>'
        | '\\' ~('<'|'>')   // otherwise ignore escape char
        | ind:INDENT
          {
            if col == 1 and self.LA(1) == '<':
                // store indent in ASTExpr not in a literal
                self.currentIndent = ind.getText()
                // reset length to wack text
                self.text.setLength(loopStartIndex)
            else:
                self.currentIndent = None
          }
        | ~('<'|'\r'|'\n')
        )+
        { if not len($getText): $skip } // pure indent?
    ;

protected
INDENT
    :   ( options {greedy=true;}: ' ' | '\t')+
    ;

NEWLINE
    :   ('\r')? '\n'
        {
          $newline
          self.currentIndent = None
        }
    ;


ACTION
options {
    generateAmbigWarnings=false; // <EXPR> is ambig with <!..!>
}
{
    startCol = self.getColumn()
}
    :   "<\\n>"! {$setText('\n'); $setType(LITERAL)}
    |   "<\\r>"! {$setText('\r'); $setType(LITERAL)}
    |   "<\\t>"! {$setText('\t'); $setType(LITERAL)}
    |   "<\\ >"! {$setText(' '); $setType(LITERAL)}
    |   COMMENT {$skip}
    |   (
        options {
                generateAmbigWarnings=false; // <EXPR> is ambig with <endif> etc...
                }
        :       '<'! "if" (' '!)* "(" (~')')+ ")" '>'! {$setType(IF)}
        ( NL! {$newline})? // ignore any newline right after an IF
    |   '<'! "else" '>'! {$setType(ELSE)}
        ( NL! {$newline})? // ignore any newline right after an ELSE
    |   '<'! "endif" '>'! {$setType(ENDIF)}
        ( {startCol==1}? NL! {$newline})? // ignore after ENDIF if on line by itself
    |   '<'! EXPR '>'!
        )
        {
            t = ChunkToken(_ttype, $getText, self.currentIndent)
            $setToken(t)
        }
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

protected
EXPR:   ( ESC
        | NL {$newline}
        | SUBTEMPLATE
        | ~'>'
        )+
    ;

protected
ESC :   '\\' ('<'|'>'|'n'|'t'|'\\'|'"'|'\''|':'|'{'|'}')
    ;

protected
SUBTEMPLATE
    :    '{' (SUBTEMPLATE|ESC|~'}')+ '}'
    ;

protected
COMMENT
{
    startCol = self.getColumn()
}
    :   "<!"
        (       options {greedy=false;}
        :       NL {$newline}
        |       .
        )*
        "!>" ( {startCol==1}? NL {$newline} )?
    ;
