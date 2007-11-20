### $ANTLR 2.7.7 (2006-11-01): "interface.g" -> "InterfaceParser.py"$
### import antlr and other modules ..
import sys
import antlr

version = sys.version.split()[0]
if version < '2.2.1':
    False = 0
if version < '2.3':
    True = not False
### header action >>> 
#
# [The "BSD licence"]
# Copyright (c) 2003-2004 Terence Parr
# All rights reserved.
# 
# Redistribution and use in source and binary forms, with or without
# modification, are permitted provided that the following conditions
# are met:
# 1. Redistributions of source code must retain the above copyright
# notice, this list of conditions and the following disclaimer.
# 2. Redistributions in binary form must reproduce the above copyright
# notice, this list of conditions and the following disclaimer in the
# documentation and/or other materials provided with the distribution.
# 3. The name of the author may not be used to endorse or promote products
# derived from this software without specific prior written permission.
# 
# THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
# IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
# OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
# IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
# INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
# NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
# DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
# THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
# (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
# THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
# 


import sys
import traceback

from stringtemplate3.language.FormalArgument import FormalArgument
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
LITERAL_interface = 4
ID = 5
SEMI = 6
LITERAL_optional = 7
LPAREN = 8
RPAREN = 9
COMMA = 10
COLON = 11
SL_COMMENT = 12
ML_COMMENT = 13
WS = 14


###/** Match an ST group interface.  Just a list of template names with args.
### *  Here is a sample interface file:
### *
### *	interface nfa;
### *	nfa(states,edges);
### *	optional state(name);
### */
class Parser(antlr.LLkParser):
    ### user action >>>
    def reportError(self, e):
       if self.group_:
           self.group_.error("template group interface parse error", e)
       else:
           sys.stderr.write("template group interface parse error: " + str(e) + '\n')
           traceback.print_exc()
    ### user action <<<
    
    def __init__(self, *args, **kwargs):
        antlr.LLkParser.__init__(self, *args, **kwargs)
        self.tokenNames = _tokenNames
        ### __init__ header action >>> 
        self.groupI = None
        ### __init__ header action <<< 
        
    def groupInterface(self,
        groupI
    ):    
        
        name = None
        self.groupI = groupI
        try:      ## for error handling
            pass
            self.match(LITERAL_interface)
            name = self.LT(1)
            self.match(ID)
            groupI.name = name.getText()
            self.match(SEMI)
            _cnt3= 0
            while True:
                if (self.LA(1)==ID or self.LA(1)==LITERAL_optional):
                    pass
                    self.template(groupI)
                else:
                    break
                
                _cnt3 += 1
            if _cnt3 < 1:
                raise antlr.NoViableAltException(self.LT(1), self.getFilename())
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            self.consume()
            self.consumeUntil(_tokenSet_0)
        
    
    def template(self,
        groupI
    ):    
        
        opt = None
        name = None
        formalArgs = {} # leave blank if no args
        templateName = None
        try:      ## for error handling
            pass
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in [LITERAL_optional]:
                pass
                opt = self.LT(1)
                self.match(LITERAL_optional)
            elif la1 and la1 in [ID]:
                pass
            else:
                    raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                
            name = self.LT(1)
            self.match(ID)
            self.match(LPAREN)
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in [ID]:
                pass
                formalArgs=self.args()
            elif la1 and la1 in [RPAREN]:
                pass
            else:
                    raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                
            self.match(RPAREN)
            self.match(SEMI)
            templateName = name.getText()
            groupI.defineTemplate(templateName, formalArgs, opt != None)
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            self.consume()
            self.consumeUntil(_tokenSet_1)
        
    
    def args(self):    
        args={}
        
        a = None
        b = None
        try:      ## for error handling
            pass
            a = self.LT(1)
            self.match(ID)
            args[a.getText()] = FormalArgument(a.getText())
            while True:
                if (self.LA(1)==COMMA):
                    pass
                    self.match(COMMA)
                    b = self.LT(1)
                    self.match(ID)
                    args[b.getText()] = FormalArgument(b.getText())
                else:
                    break
                
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            self.consume()
            self.consumeUntil(_tokenSet_2)
        
        return args
    

_tokenNames = [
    "<0>", 
    "EOF", 
    "<2>", 
    "NULL_TREE_LOOKAHEAD", 
    "\"interface\"", 
    "ID", 
    "SEMI", 
    "\"optional\"", 
    "LPAREN", 
    "RPAREN", 
    "COMMA", 
    "COLON", 
    "SL_COMMENT", 
    "ML_COMMENT", 
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
    data = [ 162L, 0L]
    return data
_tokenSet_1 = antlr.BitSet(mk_tokenSet_1())

### generate bit set
def mk_tokenSet_2(): 
    ### var1
    data = [ 512L, 0L]
    return data
_tokenSet_2 = antlr.BitSet(mk_tokenSet_2())
    
