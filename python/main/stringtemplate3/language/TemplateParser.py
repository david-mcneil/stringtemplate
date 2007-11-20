### $ANTLR 2.7.7 (2006-11-01): "template.g" -> "TemplateParser.py"$
### import antlr and other modules ..
import sys
import antlr

version = sys.version.split()[0]
if version < '2.2.1':
    False = 0
if version < '2.3':
    True = not False
### header action >>> 
import stringtemplate3
from stringtemplate3.language.ChunkToken import ChunkToken
from stringtemplate3.language.StringRef import StringRef
from stringtemplate3.language.NewlineRef import NewlineRef
### header action <<< 
### preamble action>>>

### preamble action <<<

### import antlr.Token 
from antlr import Token
### >>>The Known Token Types <<<
SKIP                = antlr.SKIP
INVALID_TYPE        = antlr.INVALID_TYPE
EOF_TYPE            = antlr.EOF_TYPE
EOF                 = antlr.EOF
NULL_TREE_LOOKAHEAD = antlr.NULL_TREE_LOOKAHEAD
MIN_USER_TYPE       = antlr.MIN_USER_TYPE
LITERAL = 4
NEWLINE = 5
ACTION = 6
IF = 7
ELSEIF = 8
ELSE = 9
ENDIF = 10
REGION_REF = 11
REGION_DEF = 12
NL = 13
EXPR = 14
TEMPLATE = 15
IF_EXPR = 16
ESC = 17
SUBTEMPLATE = 18
NESTED_PARENS = 19
INDENT = 20
COMMENT = 21


###/** A parser used to break up a single template into chunks, text literals
### *  and attribute expressions.
### */
class Parser(antlr.LLkParser):
    ### user action >>>
    def reportError(self, e):
       group = self.this.getGroup()
       if group == stringtemplate3.StringTemplate.defaultGroup:
           self.this.error("template parse error; template context is "+self.this.getEnclosingInstanceStackString(), e)
    
       else:
           self.this.error("template parse error in group "+self.this.getGroup().getName()+" line "+str(self.this.getGroupFileLine())+"; template context is "+self.this.getEnclosingInstanceStackString(), e)
    ### user action <<<
    
    def __init__(self, *args, **kwargs):
        antlr.LLkParser.__init__(self, *args, **kwargs)
        self.tokenNames = _tokenNames
        ### __init__ header action >>> 
        self.this = None
        ### __init__ header action <<< 
        
    def template(self,
        this
    ):    
        
        s = None
        nl = None
        try:      ## for error handling
            pass
            while True:
                la1 = self.LA(1)
                if False:
                    pass
                elif la1 and la1 in [LITERAL]:
                    pass
                    s = self.LT(1)
                    self.match(LITERAL)
                    this.addChunk(StringRef(this,s.getText()))
                elif la1 and la1 in [NEWLINE]:
                    pass
                    nl = self.LT(1)
                    self.match(NEWLINE)
                    if self.LA(1) != ELSE and self.LA(1) != ENDIF:
                       this.addChunk(NewlineRef(this,nl.getText()))
                elif la1 and la1 in [ACTION,IF,REGION_REF,REGION_DEF]:
                    pass
                    self.action(this)
                else:
                        break
                    
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            self.consume()
            self.consumeUntil(_tokenSet_0)
        
    
    def action(self,
        this
    ):    
        
        a = None
        i = None
        ei = None
        rr = None
        rd = None
        try:      ## for error handling
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in [ACTION]:
                pass
                a = self.LT(1)
                self.match(ACTION)
                indent = a.getIndentation()
                c = this.parseAction(a.getText())
                c.setIndentation(indent)
                this.addChunk(c)
            elif la1 and la1 in [IF]:
                pass
                i = self.LT(1)
                self.match(IF)
                c = this.parseAction(i.getText())
                # create and precompile the subtemplate
                subtemplate = stringtemplate3.StringTemplate(this.getGroup(), None)
                subtemplate.setEnclosingInstance(this)
                subtemplate.setName(i.getText() + "_subtemplate")
                this.addChunk(c)
                self.template(subtemplate)
                if c: c.setSubtemplate(subtemplate)
                while True:
                    if (self.LA(1)==ELSEIF):
                        pass
                        ei = self.LT(1)
                        self.match(ELSEIF)
                        ec = this.parseAction(ei.getText())
                        # create and precompile the subtemplate
                        elseIfSubtemplate = stringtemplate3.StringTemplate(this.getGroup(), None)
                        elseIfSubtemplate.setEnclosingInstance(this)
                        elseIfSubtemplate.setName(ei.getText()+"_subtemplate")
                        self.template(elseIfSubtemplate)
                        if c is not None:
                           c.addElseIfSubtemplate(ec, elseIfSubtemplate)
                    else:
                        break
                    
                la1 = self.LA(1)
                if False:
                    pass
                elif la1 and la1 in [ELSE]:
                    pass
                    self.match(ELSE)
                    # create and precompile the subtemplate
                    elseSubtemplate = stringtemplate3.StringTemplate(this.getGroup(), None)
                    elseSubtemplate.setEnclosingInstance(this)
                    elseSubtemplate.setName("else_subtemplate")
                    self.template(elseSubtemplate)
                    if c: c.setElseSubtemplate(elseSubtemplate)
                elif la1 and la1 in [ENDIF]:
                    pass
                else:
                        raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                    
                self.match(ENDIF)
            elif la1 and la1 in [REGION_REF]:
                pass
                rr = self.LT(1)
                self.match(REGION_REF)
                # define implicit template and
                # convert <@r()> to <region__enclosingTemplate__r()>
                regionName = rr.getText()
                mangledRef = None
                err = False
                # watch out for <@super.r()>; that does NOT def implicit region
                # convert to <super.region__enclosingTemplate__r()>
                if regionName.startswith("super."):
                   #System.out.println("super region ref "+regionName);
                   regionRef = regionName[len("super."):len(regionName)]
                   templateScope = this.getGroup().getUnMangledTemplateName(this.getName())
                   scopeST = this.getGroup().lookupTemplate(templateScope)
                   if scopeST is None:
                       this.getGroup().error("reference to region within undefined template: "+
                           templateScope)
                       err = True
                
                   if not scopeST.containsRegionName(regionRef):
                       this.getGroup().error("template "+templateScope+" has no region called "+
                           regionRef)
                       err = True
                
                   else:
                       mangledRef = this.getGroup().getMangledRegionName(templateScope, regionRef)
                       mangledRef = "super." + mangledRef
                
                else:
                   #System.out.println("region ref "+regionName);
                   regionST = this.getGroup().defineImplicitRegionTemplate(this, regionName)
                   mangledRef = regionST.getName()
                
                if not err:
                   # treat as regular action: mangled template include
                   indent = rr.getIndentation()
                   c = this.parseAction(mangledRef+"()")
                   c.setIndentation(indent)
                   this.addChunk(c)
            elif la1 and la1 in [REGION_DEF]:
                pass
                rd = self.LT(1)
                self.match(REGION_DEF)
                combinedNameTemplateStr = rd.getText()
                indexOfDefSymbol = combinedNameTemplateStr.find("::=")
                if indexOfDefSymbol >= 1:
                   regionName = combinedNameTemplateStr[0:indexOfDefSymbol]
                   template = combinedNameTemplateStr[indexOfDefSymbol+3:len(combinedNameTemplateStr)]
                   regionST = this.getGroup().defineRegionTemplate(
                       this,
                       regionName,
                       template,
                       stringtemplate3.StringTemplate.REGION_EMBEDDED
                       )
                   # treat as regular action: mangled template include
                   indent = rd.getIndentation()
                   c = this.parseAction(regionST.getName() + "()")
                   c.setIndentation(indent)
                   this.addChunk(c)
                
                else:
                   this.error("embedded region definition screwed up")
            else:
                    raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            self.consume()
            self.consumeUntil(_tokenSet_1)
        
    

_tokenNames = [
    "<0>", 
    "EOF", 
    "<2>", 
    "NULL_TREE_LOOKAHEAD", 
    "LITERAL", 
    "NEWLINE", 
    "ACTION", 
    "IF", 
    "ELSEIF", 
    "ELSE", 
    "ENDIF", 
    "REGION_REF", 
    "REGION_DEF", 
    "NL", 
    "EXPR", 
    "TEMPLATE", 
    "IF_EXPR", 
    "ESC", 
    "SUBTEMPLATE", 
    "NESTED_PARENS", 
    "INDENT", 
    "COMMENT"
]
    

### generate bit set
def mk_tokenSet_0(): 
    ### var1
    data = [ 1792L, 0L]
    return data
_tokenSet_0 = antlr.BitSet(mk_tokenSet_0())

### generate bit set
def mk_tokenSet_1(): 
    ### var1
    data = [ 8176L, 0L]
    return data
_tokenSet_1 = antlr.BitSet(mk_tokenSet_1())
    
