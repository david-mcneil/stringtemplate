### $ANTLR 2.7.5 (20050419): "group.g" -> "GroupParser.py"$
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
LITERAL_group = 4
ID = 5
SEMI = 6
LPAREN = 7
RPAREN = 8
DEFINED_TO_BE = 9
TEMPLATE = 10
COMMA = 11
STAR = 12
PLUS = 13
OPTIONAL = 14
SL_COMMENT = 15
ML_COMMENT = 16
NL = 17
WS = 18


###/** Match a group of template definitions beginning
### *  with a group name declaration.  Templates are enclosed
### *  in double-quotes or <<...>> quotes for multi-line templates.
### *  Template names have arg lists that indicate the cardinality
### *  of the attribute: present, optional, zero-or-more, one-or-more.
### *  Here is a sample group file:
###
###        group nfa;
###
###        // an NFA has edges and states
###        nfa(states,edges) ::= <<
###        digraph NFA {
###        rankdir=LR;
###        <states; separator="\\n">
###        <edges; separator="\\n">
###        }
###        >>
###
###        state(name) ::= "node [shape = circle]; <name>;"
###
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
        self.group_ = None
        ### __init__ header action <<< 
        
    def group(self,
        g
    ):    
        
        name = None
        try:      ## for error handling
            pass
            self.match(LITERAL_group)
            name = self.LT(1)
            self.match(ID)
            g.setName(name.getText())
            self.match(SEMI)
            while True:
                if (self.LA(1)==ID):
                    pass
                    self.template(g)
                else:
                    break
                
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            self.consume()
            self.consumeUntil(_tokenSet_0)
        
    
    def template(self,
        g
    ):    
        
        name = None
        t = None
        alias = None
        target = None
        formalArgs = {}
        st = None
        ignore = False
        try:      ## for error handling
            if (self.LA(1)==ID) and (self.LA(2)==LPAREN):
                pass
                name = self.LT(1)
                self.match(ID)
                if g.isDefinedInThisGroup(name.getText()):
                   g.error("redefinition of template: " + name.getText())
                   st = stringtemplate.StringTemplate() # create bogus template to fill in
                else:
                   st = g.defineTemplate(name.getText(), None)
                self.match(LPAREN)
                la1 = self.LA(1)
                if False:
                    pass
                elif la1 and la1 in [ID]:
                    pass
                    self.args(st)
                elif la1 and la1 in [RPAREN]:
                    pass
                    st.defineEmptyFormalArgumentList()
                else:
                        raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                    
                self.match(RPAREN)
                self.match(DEFINED_TO_BE)
                t = self.LT(1)
                self.match(TEMPLATE)
                st.setTemplate(t.getText())
            elif (self.LA(1)==ID) and (self.LA(2)==DEFINED_TO_BE):
                pass
                alias = self.LT(1)
                self.match(ID)
                self.match(DEFINED_TO_BE)
                target = self.LT(1)
                self.match(ID)
                g.defineTemplateAlias(alias.getText(), target.getText())
            else:
                raise antlr.NoViableAltException(self.LT(1), self.getFilename())
            
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            self.consume()
            self.consumeUntil(_tokenSet_1)
        
    
    def args(self,
        st
    ):    
        
        name = None
        name2 = None
        try:      ## for error handling
            pass
            name = self.LT(1)
            self.match(ID)
            st.defineFormalArgument(name.getText())
            while True:
                if (self.LA(1)==COMMA):
                    pass
                    self.match(COMMA)
                    name2 = self.LT(1)
                    self.match(ID)
                    st.defineFormalArgument(name2.getText())
                else:
                    break
                
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            self.consume()
            self.consumeUntil(_tokenSet_2)
        
    

_tokenNames = [
    "<0>", 
    "EOF", 
    "<2>", 
    "NULL_TREE_LOOKAHEAD", 
    "\"group\"", 
    "ID", 
    "SEMI", 
    "LPAREN", 
    "RPAREN", 
    "DEFINED_TO_BE", 
    "TEMPLATE", 
    "COMMA", 
    "STAR", 
    "PLUS", 
    "OPTIONAL", 
    "SL_COMMENT", 
    "ML_COMMENT", 
    "NL", 
    "WS"
]
    

### generate bit set
def mk_tokenSet_0(): 
    ### var1
    data = [ 2L, 0L]
    return data
_tokenSet_0 = antlr.BitSet(mk_tokenSet_0())

### generate bit set
def mk_tokenSet_1(): 
    ### var1
    data = [ 34L, 0L]
    return data
_tokenSet_1 = antlr.BitSet(mk_tokenSet_1())

### generate bit set
def mk_tokenSet_2(): 
    ### var1
    data = [ 256L, 0L]
    return data
_tokenSet_2 = antlr.BitSet(mk_tokenSet_2())
    
