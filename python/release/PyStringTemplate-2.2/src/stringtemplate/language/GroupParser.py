### $ANTLR 2.7.6 (20060527): "group.g" -> "GroupParser.py"$
### import antlr and other modules ..
import sys
import antlr

version = sys.version.split()[0]
if version < '2.2.1':
    False = 0
if version < '2.3':
    True = not False
### header action >>> 
from ASTExpr import *
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
STRING = 10
BIGSTRING = 11
COMMA = 12
ASSIGN = 13
ANONYMOUS_TEMPLATE = 14
LBRACK = 15
RBRACK = 16
COLON = 17
LITERAL_default = 18
STAR = 19
PLUS = 20
OPTIONAL = 21
SL_COMMENT = 22
ML_COMMENT = 23
NL = 24
WS = 25


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
       if self.group_:
           self.group_.error("template parse error", e)
       else:
           sys.stderr.write("template parse error: " + str(e) + '\n')
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
                if (self.LA(1)==ID) and (self.LA(2)==LPAREN or self.LA(2)==DEFINED_TO_BE) and (self.LA(3)==ID or self.LA(3)==RPAREN):
                    pass
                    self.template(g)
                elif (self.LA(1)==ID) and (self.LA(2)==DEFINED_TO_BE) and (self.LA(3)==LBRACK):
                    pass
                    self.mapdef(g)
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
        bt = None
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
                   # create bogus template to fill in
                   st = stringtemplate.StringTemplate()
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
                la1 = self.LA(1)
                if False:
                    pass
                elif la1 and la1 in [STRING]:
                    pass
                    t = self.LT(1)
                    self.match(STRING)
                    st.setTemplate(t.getText())
                elif la1 and la1 in [BIGSTRING]:
                    pass
                    bt = self.LT(1)
                    self.match(BIGSTRING)
                    st.setTemplate(bt.getText())
                else:
                        raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                    
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
        
    
    def mapdef(self,
        g
    ):    
        
        name = None
        m = None
        try:      ## for error handling
            pass
            name = self.LT(1)
            self.match(ID)
            self.match(DEFINED_TO_BE)
            m=self.map()
            if g.getMap(name.getText()):
               g.error("redefinition of map: " + name.getText())
            elif g.isDefinedInThisGroup(name.getText()):
               g.error("redefinition of template as map: " + name.getText())
            else:
               g.defineMap(name.getText(), m)
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            self.consume()
            self.consumeUntil(_tokenSet_1)
        
    
    def args(self,
        st
    ):    
        
        try:      ## for error handling
            pass
            self.arg(st)
            while True:
                if (self.LA(1)==COMMA):
                    pass
                    self.match(COMMA)
                    self.arg(st)
                else:
                    break
                
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            self.consume()
            self.consumeUntil(_tokenSet_2)
        
    
    def arg(self,
        st
    ):    
        
        name = None
        s = None
        bs = None
        defaultValue = None
        try:      ## for error handling
            pass
            name = self.LT(1)
            self.match(ID)
            if (self.LA(1)==ASSIGN) and (self.LA(2)==STRING):
                pass
                self.match(ASSIGN)
                s = self.LT(1)
                self.match(STRING)
                defaultValue = stringtemplate.StringTemplate("$_val_$")
                defaultValue["_val_"] = s.getText()
                defaultValue.defineFormalArgument("_val_")
                defaultValue.setName("<" + st.getName() + "'s arg " + \
                                    name.getText() + \
                                    " default value subtemplate>")
            elif (self.LA(1)==ASSIGN) and (self.LA(2)==ANONYMOUS_TEMPLATE):
                pass
                self.match(ASSIGN)
                bs = self.LT(1)
                self.match(ANONYMOUS_TEMPLATE)
                defaultValue = stringtemplate.StringTemplate(st.getGroup(), \
                   bs.getText())
                defaultValue.setName("<" + st.getName() + "'s arg " + \
                                    name.getText() + \
                                    " default value subtemplate>")
            elif (self.LA(1)==RPAREN or self.LA(1)==COMMA):
                pass
            else:
                raise antlr.NoViableAltException(self.LT(1), self.getFilename())
            
            st.defineFormalArgument(name.getText(), defaultValue)
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            self.consume()
            self.consumeUntil(_tokenSet_3)
        
    
    def map(self):    
        mapping = {}
        
        try:      ## for error handling
            pass
            self.match(LBRACK)
            self.keyValuePair(mapping)
            while True:
                if (self.LA(1)==COMMA):
                    pass
                    self.match(COMMA)
                    self.keyValuePair(mapping)
                else:
                    break
                
            self.match(RBRACK)
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            self.consume()
            self.consumeUntil(_tokenSet_1)
        
        return mapping
    
    def keyValuePair(self,
        mapping
    ):    
        
        key1 = None
        s1 = None
        key2 = None
        s2 = None
        s3 = None
        s4 = None
        try:      ## for error handling
            if (self.LA(1)==STRING) and (self.LA(2)==COLON) and (self.LA(3)==STRING):
                pass
                key1 = self.LT(1)
                self.match(STRING)
                self.match(COLON)
                s1 = self.LT(1)
                self.match(STRING)
                mapping[key1.getText()] = s1.getText()
            elif (self.LA(1)==STRING) and (self.LA(2)==COLON) and (self.LA(3)==BIGSTRING):
                pass
                key2 = self.LT(1)
                self.match(STRING)
                self.match(COLON)
                s2 = self.LT(1)
                self.match(BIGSTRING)
                mapping[key2.getText()] = s2.getText()
            elif (self.LA(1)==LITERAL_default) and (self.LA(2)==COLON) and (self.LA(3)==STRING):
                pass
                self.match(LITERAL_default)
                self.match(COLON)
                s3 = self.LT(1)
                self.match(STRING)
                mapping[ASTExpr.DEFAULT_MAP_VALUE_NAME] = s3.getText()
            elif (self.LA(1)==LITERAL_default) and (self.LA(2)==COLON) and (self.LA(3)==BIGSTRING):
                pass
                self.match(LITERAL_default)
                self.match(COLON)
                s4 = self.LT(1)
                self.match(BIGSTRING)
                mapping[ASTExpr.DEFAULT_MAP_VALUE_NAME] = s4.getText()
            else:
                raise antlr.NoViableAltException(self.LT(1), self.getFilename())
            
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            self.consume()
            self.consumeUntil(_tokenSet_4)
        
    

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
    "STRING", 
    "BIGSTRING", 
    "COMMA", 
    "ASSIGN", 
    "ANONYMOUS_TEMPLATE", 
    "LBRACK", 
    "RBRACK", 
    "COLON", 
    "\"default\"", 
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

### generate bit set
def mk_tokenSet_3(): 
    ### var1
    data = [ 4352L, 0L]
    return data
_tokenSet_3 = antlr.BitSet(mk_tokenSet_3())

### generate bit set
def mk_tokenSet_4(): 
    ### var1
    data = [ 69632L, 0L]
    return data
_tokenSet_4 = antlr.BitSet(mk_tokenSet_4())
    
