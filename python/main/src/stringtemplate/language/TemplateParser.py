### $ANTLR 2.7.5 (20050401): "template.g" -> "TemplateParser.py"$
### import antlr and other modules ..
import sys
import antlr

version = sys.version.split()[0]
if version < '2.2.1':
    False = 0
if version < '2.3':
    True = not False
### header action >>> 
import stringtemplate
from ChunkToken import ChunkToken
from StringRef import StringRef
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
ACTION = 5
IF = 6
ELSE = 7
ENDIF = 8
NL = 9
EXPR = 10
ESC = 11
SUBTEMPLATE = 12
INDENT = 13
COMMENT = 14


###/** A parser used to break up a single template into chunks, text literals
### *  and attribute expressions.
### */
class Parser(antlr.LLkParser):
    ### user action >>>
    def reportError(self, e):
       self.error("template parse error", e)
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
                elif la1 and la1 in [ACTION,IF]:
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
                subtemplate = stringtemplate.StringTemplate(this.getGroup(), None)
                subtemplate.setEnclosingInstance(this)
                subtemplate.setName(i.getText() + " subtemplate")
                this.addChunk(c)
                self.template(subtemplate)
                if c: c.setSubtemplate(subtemplate)
                la1 = self.LA(1)
                if False:
                    pass
                elif la1 and la1 in [ELSE]:
                    pass
                    self.match(ELSE)
                    # create and precompile the subtemplate
                    elseSubtemplate = stringtemplate.StringTemplate(this.getGroup(), None)
                    elseSubtemplate.setEnclosingInstance(this)
                    elseSubtemplate.setName("else subtemplate")
                    self.template(elseSubtemplate)
                    if c: c.setElseSubtemplate(elseSubtemplate)
                elif la1 and la1 in [ENDIF]:
                    pass
                else:
                        raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                    
                self.match(ENDIF)
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
    "ACTION", 
    "IF", 
    "ELSE", 
    "ENDIF", 
    "NL", 
    "EXPR", 
    "ESC", 
    "SUBTEMPLATE", 
    "INDENT", 
    "COMMENT"
]
    

### generate bit set
def mk_tokenSet_0(): 
    ### var1
    data = [ 384L, 0L]
    return data
_tokenSet_0 = antlr.BitSet(mk_tokenSet_0())

### generate bit set
def mk_tokenSet_1(): 
    ### var1
    data = [ 496L, 0L]
    return data
_tokenSet_1 = antlr.BitSet(mk_tokenSet_1())
    
