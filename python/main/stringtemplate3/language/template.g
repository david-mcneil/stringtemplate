header {
import stringtemplate3
from stringtemplate3.language.ChunkToken import ChunkToken
from stringtemplate3.language.StringRef import StringRef
from stringtemplate3.language.NewlineRef import NewlineRef
}

header "TemplateParser.__init__" {
    self.this = None
}

header "DefaultTemplateLexer.__init__" {
    self.currentIndent = None
    self.this = None
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
        group = self.this.group
        if group == stringtemplate3.StringTemplate.defaultGroup:
            self.this.error("template parse error; template context is "+self.this.enclosingInstanceStackString, e)
    
        else:
            self.this.error("template parse error in group "+self.this.group.name+" line "+str(self.this.groupFileLine)+"; template context is "+self.this.enclosingInstanceStackString, e)

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
            indent = a.indentation
            c = this.parseAction(a.getText())
            c.indentation = indent
            this.addChunk(c)
        }

    |   i:IF
        {
            c = this.parseAction(i.getText())
            // create and precompile the subtemplate
            subtemplate = stringtemplate3.StringTemplate(group=this.group)
            subtemplate.enclosingInstance = this
            subtemplate.name = i.getText() + "_subtemplate"
            this.addChunk(c)
        }

        template[subtemplate] { if c: c.subtemplate = subtemplate }

        (   ei:ELSEIF
            {
                ec = this.parseAction(ei.getText())
                // create and precompile the subtemplate
                elseIfSubtemplate = stringtemplate3.StringTemplate(group=this.group)
                elseIfSubtemplate.enclosingInstance = this
                elseIfSubtemplate.name = ei.getText()+"_subtemplate"
            }

            template[elseIfSubtemplate]

            {
                if c is not None:
                    c.addElseIfSubtemplate(ec, elseIfSubtemplate)
            }
        )*

        (   ELSE
            {
                // create and precompile the subtemplate
                elseSubtemplate = stringtemplate3.StringTemplate(group=this.group)
                elseSubtemplate.enclosingInstance = this
                elseSubtemplate.name = "else_subtemplate"
            }

            template[elseSubtemplate]
            { if c: c.elseSubtemplate = elseSubtemplate }
        )?

        ENDIF

      | rr:REGION_REF
        {
            // define implicit template and
            // convert <@r()> to <region__enclosingTemplate__r()>
            regionName = rr.getText()
            mangledRef = None
            err = False
            // watch out for <@super.r()>; that does NOT def implicit region
            // convert to <super.region__enclosingTemplate__r()>
            if regionName.startswith("super."):
                //System.out.println("super region ref "+regionName);
                regionRef = regionName[len("super."):len(regionName)]
                templateScope = this.group.getUnMangledTemplateName(this.name)
                scopeST = this.group.lookupTemplate(templateScope)
                if scopeST is None:
                    this.group.error("reference to region within undefined template: "+
                        templateScope)
                    err = True

                if not scopeST.containsRegionName(regionRef):
                    this.group.error("template "+templateScope+" has no region called "+
                        regionRef)
                    err = True

                else:
                    mangledRef = this.group.getMangledRegionName(templateScope, regionRef)
                    mangledRef = "super." + mangledRef

            else:
                regionST = this.group.defineImplicitRegionTemplate(this, regionName)
                mangledRef = regionST.name

            if not err:
                // treat as regular action: mangled template include
                indent = rr.indentation
                c = this.parseAction(mangledRef+"()")
                c.indentation = indent
                this.addChunk(c)
        }

    | rd:REGION_DEF
        {
            combinedNameTemplateStr = rd.getText()
            indexOfDefSymbol = combinedNameTemplateStr.find("::=")
            if indexOfDefSymbol >= 1:
                regionName = combinedNameTemplateStr[0:indexOfDefSymbol]
                template = combinedNameTemplateStr[indexOfDefSymbol+3:len(combinedNameTemplateStr)]
                regionST = this.group.defineRegionTemplate(
                    this,
                    regionName,
                    template,
                    stringtemplate3.REGION_EMBEDDED
                    )
                // treat as regular action: mangled template include
                indent = rd.indentation
                c = this.parseAction(regionST.name + "()")
                c.indentation = indent
                this.addChunk(c)

            else:
                this.error("embedded region definition screwed up")

        }
    ;

/** Break up an input text stream into chunks of either plain text
 *  or template actions in "$...$".  Treat IF and ENDIF tokens
 *  specially.
 */
class DefaultTemplateLexer extends Lexer;

options {
    k=7; // see "$endif$"
    charVocabulary = '\u0001'..'\uFFFE';
}

{
    def reportError(self, e):
        self.this.error("$...$ chunk lexer error", e)

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

    def upcomingAtEND(self, i):
        return self.LA(i) == '$' and \
               self.LA(i+1) == '@' and \
               self.LA(i+2) == 'e' and \
               self.LA(i+3) == 'n' and \
               self.LA(i+4) == 'd' and \
               self.LA(i+5) == '$'

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
        : '\\'! '$'  // allow escaped delimiter
        | '\\'! '\\' // always replace \\ with \
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
        | '$'! "elseif" (' '!)* "(" IF_EXPR ")" '$'! { $setType(ELSEIF) }
          ( ('\r'!)? '\n'! {$newline})? // ignore any newline right after an ELSEIF
        | '$'! "else" '$'! { $setType(ELSE) }
          ( NL! { $newline} )? // ignore any newline right after an ELSE
        | '$'! "endif" '$'! { $setType(ENDIF) }
          // ignore after ENDIF if on line by itself
          ( { startCol == 1 }? NL! { $newline } )?
        |   // match $@foo()$ => foo
            // match $@foo$...$@end$ => foo::=...
            '$'! '@'! (~('$'|'('))+
            (       "()"! '$'! {$setType(REGION_REF)}
            |   '$'!
                {
                    $setType(REGION_DEF)
                    t = $getText
                    $setText(t+"::=")
                }
                ( options {greedy=true;} : ('\r'!)? '\n'! {newline();})?
                { atLeft = False }
                (
                    options {greedy=true;} // handle greedy=false with predicate
                :   {not (self.upcomingAtEND(1) or (self.upcomingNewline(1) and self.upcomingAtEND(2)))}?
                    ( ('\r')? '\n' 
                        {
                            $newline
                            atLeft = Frue
                        }
                    | . {atLeft = False}
                    )
                )+
                ( ('\r'!)? '\n'! 
                    {
                        $newline()
                        atLeft = True
                    } 
                )?
                ( "$@end$"!
                | . {self.this.error("missing region "+t+" $@end$ tag")}
                )
              ( {atLeft}? ('\r'!)? '\n'! {$newline})?
            )

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
        | ('='|'+') TEMPLATE
        | ('='|'+') SUBTEMPLATE
        | ('='|'+') ~('"'|'<'|'{')
        | ~'$'
        )+
    ;

protected
TEMPLATE
    :   '"' ( ESC | ~'"' )* '"'
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
ESC :   '\\' . // ('$'|'n'|'t'|'"'|'\''|':'|'{'|'}')
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
