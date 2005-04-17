header {
import stringtemplate
from ChunkToken import ChunkToken
from StringRef import StringRef
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
    :   ( options { greedy=true;}
          { col = self.getColumn() }
        : '\\'! '$'  // allow escaped delimiter
        | '\\' ~'$'  // otherwise ignore escape char
        | { self.upcomingELSE(3) or self.upcomingENDIF(3) }?
          NL! {$newline}
        | { self.upcomingELSE(2) or self.upcomingENDIF(2) }?
          NL! {$newline}
        | NL {$newline} // newline not before else or endif
        | ind:INDENT
          {
              if col == 1 and self.LA(1) == '$':
                  self.currentIndent = ind.getText()
              else:
                  self.currentIndent = None
          }
        | ~'$'
        )+
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
    |   (
        options {
                generateAmbigWarnings=false; // $EXPR$ is ambig with $endif$ etc...
                }
        :       '$'! "if" (' '!)* "(" (~')')+ ")" '$'! {$setType(IF)}
        ( NL! {$newline})? // ignore any newline right after an IF
    |   '$'! "else" '$'! {$setType(ELSE)}
        ( NL! {$newline})? // ignore any newline right after an ELSE
    |   '$'! "endif" '$'!{$setType(ENDIF)}
        ( {startCol==1}? NL! {$newline})? // ignore after ENDIF if on line by itself
    |   '$'! EXPR '$'! // (Can't start with '!', which would mean comment)
        )
        {
            t = ChunkToken(_ttype, $getText, self.currentIndent)
            $setToken(t);
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
        | ~'$'
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
INDENT
    :   ( options {greedy=true;}: ' ' | '\t')+
    ;

protected
COMMENT
{
    startCol = self.getColumn()
}
    :   "$!"
        (       options {greedy=false;}
        :       NL {$newline}
        |       .
        )*
        "!$" ( {startCol == 1}? NL {$newline} )?
    ;
