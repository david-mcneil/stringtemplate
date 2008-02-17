header {
import stringtemplate3
import stringtemplate3.language.TemplateParser
from stringtemplate3.language.ChunkToken import ChunkToken
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
    def reportError(self, e):
        self.this.error("<...> chunk lexer error", e)

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

    def upcomingAtEND(self, i):
        return self.LA(i) == '<' and \
               self.LA(i+1) == '@' and \
               self.LA(i+2) == 'e' and \
               self.LA(i+3) == 'n' and \
               self.LA(i+4) == 'd' and \
               self.LA(i+5) == '>'

    def upcomingNewline(self, i):
        return (self.LA(i) == '\r' and
                self.LA(i+1) == '\n') or \
               self.LA(i) == '\n'

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
        | '\\'! '\\' // always replace \\ with \o
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
    :   ( options { greedy = true; } : ' ' | '\t' )+
    ;

NEWLINE
    :   ( '\r' )? '\n'
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
    : // Match escapes not in a string like <\n\ufea5>
        {
            buf = u""
            uc = u"\000"
        }
        '<'! (uc=ESC_CHAR {buf += uc} )+'>'!
        {
            $setText(buf)
            $setType(LITERAL)
        }
    |   COMMENT {$skip}
    |   ( // <EXPR> is ambig with <endif> etc...
          options { generateAmbigWarnings = false; }
          : '<'! "if" (' '!)* "(" IF_EXPR ")" '>'! { $setType(IF) }
            ( NL! {$newline})? // ignore any newline right after an IF
          | '<'! "else" '>'! { $setType(ELSE) }
            ( NL! {$newline})? // ignore any newline right after an ELSE
          | '<'! "endif" '>'! { $setType(ENDIF) }
            ( {startCol==1}? NL! {$newline})? // ignore after ENDIF if on line by itself
        |   // match <@foo()> => foo
            // match <@foo>...<@end> => foo::=...
            '<'! '@'! (~('>'|'('))+
            (   "()"! '>'! {$setType(REGION_REF)}
            |   '>'!
                {
                    $setType(REGION_DEF)
                    t = $getText
                    $setText(t+"::=")
                }
                ( options {greedy=true;} : ('\r'!)? '\n'! {$newline})?
                { atLeft = False }
                (
                    options {greedy=true;} // handle greedy=false with predicate
                :   {not (self.upcomingAtEND(1) or (self.upcomingNewline(1) and self.upcomingAtEND(2)))}?
                    ( ('\r')? '\n' 
                        {
                            $newline
                            atLeft = True
                        }
                    | . {atLeft = False}
                    )
                )+
                ( ('\r'!)? '\n'! 
                    {
                        $newline
                        atLeft = True
                    } 
                )?
                ( "<@end>"!
                | . {self.this.error("missing region "+t+" <@end> tag")}
                )
                ( {atLeft}? ('\r'!)? '\n'! {$newline})?
            )
          | '<'! EXPR '>'!
        )
        {
            t = ChunkToken(_ttype, $getText, self.currentIndent)
            $setToken(t)
        }
    ;

protected
NL
options { generateAmbigWarnings = false; }
// single '\r' is ambig with '\r' '\n'
    :   '\r'
    |   '\n'
    |   '\r' '\n'
    ;

protected
EXPR
    :   ( ESC
        | NL { $newline }
        | SUBTEMPLATE
        | ('='|'+') TEMPLATE
        | ('='|'+') SUBTEMPLATE
        | ('='|'+') ~('"'|'<'|'{')
        | ~'>'
        )+
    ;

protected
TEMPLATE
    :    '"' ( ESC | ~'"' )* '"'
    |    "<<"
         ( options { greedy = true; }
           : ( '\r'! )? '\n'! { $newline } )? // consume 1st \n
         ( options { greedy = false; }    // stop when you see the >>
           : { self.LA(3) == '>' and self.LA(4) == '>' }?
             '\r'! '\n'! { $newline }     // kill last \r\n
           | { self.LA(2) == '>' and self.LA(3) == '>' }?
             '\n'! { $newline }           // kill last \n
           | ( '\r' )? '\n' { $newline }  // else keep
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
ESC_CHAR returns [uc='\u0000']
      : "\\n"! {uc = '\n'}
      | "\\r"! {uc = '\r'}
      | "\\t"! {uc = '\t'}
      | "\\ "! {uc = ' '}
      | "\\u"! a:HEX! b:HEX! c:HEX! d:HEX!
        {uc = unichr(int(a.getText()+b.getText()+c.getText()+d.getText(), 16))}
      ;

protected
ESC :   '\\' . // ('<'|'>'|'n'|'t'|'\\'|'"'|'\''|':'|'{'|'}')
    ;

protected
HEX : '0'..'9'|'A'..'F'|'a'..'f'
    ;

protected
SUBTEMPLATE
    :    '{' (SUBTEMPLATE|ESC|~'}')* '}'
    ;

protected
NESTED_PARENS
    :    '(' ( options { greedy = false; } : NESTED_PARENS | ESC | ~')' )+ ')'
    ;

protected
COMMENT
{
    startCol = self.getColumn()
}
    :   "<!"
        ( options { greedy = false; }
          : NL { $newline }
          | .
        )*
        "!>" ( { startCol == 1 }? NL { $newline } )?
    ;
