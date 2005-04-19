### $ANTLR 2.7.5 (20050419): "angle.bracket.template.g" -> "AngleBracketTemplateLexer.py"$
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
import stringtemplate.language.TemplateParser
from ChunkToken import ChunkToken
### header action <<< 
### preamble action >>> 

### preamble action <<< 
### >>>The Literals<<<
literals = {}


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
ELSE = 8
ENDIF = 9
NL = 10
EXPR = 11
ESC = 12
SUBTEMPLATE = 13
INDENT = 14
COMMENT = 15


###/** Break up an input text stream into chunks of either plain text
### *  or template actions in "<...>".  Treat IF and ENDIF tokens
### *  specially.
### */
class Lexer(antlr.CharScanner) :
    ### user action >>>
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
    ### user action <<<
    def __init__(self, *argv, **kwargs) :
        antlr.CharScanner.__init__(self, *argv, **kwargs)
        self.caseSensitiveLiterals = True
        self.setCaseSensitive(True)
        self.literals = literals
        ### __init__ header action >>> 
        self.currentIndent = None
        self.this = None
        ### __init__ header action <<< 
    
    def nextToken(self):
        while True:
            try: ### try again ..
                while True:
                    _token = None
                    _ttype = INVALID_TYPE
                    self.resetText()
                    try: ## for char stream error handling
                        try: ##for lexical error handling
                            la1 = self.LA(1)
                            if False:
                                pass
                            elif la1 and la1 in u'\n\r':
                                pass
                                self.mNEWLINE(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'<':
                                pass
                                self.mACTION(True)
                                theRetToken = self._returnToken
                            else:
                                if ((_tokenSet_0.member(self.LA(1))) and ( self.LA(1) != '\r' and self.LA(1) != '\n' )):
                                    pass
                                    self.mLITERAL(True)
                                    theRetToken = self._returnToken
                                else:
                                    self.default(self.LA(1))
                                
                            if not self._returnToken:
                                raise antlr.TryAgain ### found SKIP token
                            ### option { testLiterals=true } 
                            self.testForLiteral(self._returnToken)
                            ### return token to caller
                            return self._returnToken
                        ### handle lexical errors ....
                        except antlr.RecognitionException, e:
                            raise antlr.TokenStreamRecognitionException(e)
                    ### handle char stream errors ...
                    except antlr.CharStreamException,cse:
                        if isinstance(cse, antlr.CharStreamIOException):
                            raise antlr.TokenStreamIOException(cse.io)
                        else:
                            raise antlr.TokenStreamException(str(cse))
            except antlr.TryAgain:
                pass
        
    def mLITERAL(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = LITERAL
        _saveIndex = 0
        ind = None
        if not  self.LA(1) != '\r' and self.LA(1) != '\n' :
            raise SemanticException(" self.LA(1) != '\\r' and self.LA(1) != '\\n' ")
        pass
        _cnt5= 0
        while True:
            loopStartIndex = self.text.length()
            col = self.getColumn()
            if (self.LA(1)==u'\\') and (self.LA(2)==u'<'):
                pass
                _saveIndex = self.text.length()
                self.match('\\')
                self.text.setLength(_saveIndex)
                self.match('<')
            elif (self.LA(1)==u'\\') and (self.LA(2)==u'>') and (True) and (True) and (True) and (True) and (True):
                pass
                _saveIndex = self.text.length()
                self.match('\\')
                self.text.setLength(_saveIndex)
                self.match('>')
            elif (self.LA(1)==u'\\') and (_tokenSet_1.member(self.LA(2))) and (True) and (True) and (True) and (True) and (True):
                pass
                self.match('\\')
                self.match(_tokenSet_1)
            elif (self.LA(1)==u'\t' or self.LA(1)==u' ') and (True) and (True) and (True) and (True) and (True) and (True):
                pass
                self.mINDENT(True)
                ind = self._returnToken
                if col == 1 and self.LA(1) == '<':
                   # store indent in ASTExpr not in a literal
                   self.currentIndent = ind.getText()
                   # reset length to wack text
                   self.text.setLength(loopStartIndex)
                else:
                   self.currentIndent = None
            elif (_tokenSet_0.member(self.LA(1))) and (True) and (True) and (True) and (True) and (True) and (True):
                pass
                self.match(_tokenSet_0)
            else:
                break
            
            _cnt5 += 1
        if _cnt5 < 1:
            self.raise_NoViableAlt(self.LA(1))
        if not len(self.text.getString(_begin)): _ttype = SKIP
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mINDENT(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = INDENT
        _saveIndex = 0
        pass
        _cnt8= 0
        while True:
            if (self.LA(1)==u' ') and (True) and (True) and (True) and (True) and (True) and (True):
                pass
                self.match(' ')
            elif (self.LA(1)==u'\t') and (True) and (True) and (True) and (True) and (True) and (True):
                pass
                self.match('\t')
            else:
                break
            
            _cnt8 += 1
        if _cnt8 < 1:
            self.raise_NoViableAlt(self.LA(1))
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mNEWLINE(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = NEWLINE
        _saveIndex = 0
        pass
        la1 = self.LA(1)
        if False:
            pass
        elif la1 and la1 in u'\r':
            pass
            self.match('\r')
        elif la1 and la1 in u'\n':
            pass
        else:
                self.raise_NoViableAlt(self.LA(1))
            
        self.match('\n')
        self.newline()
        self.currentIndent = None
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mACTION(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = ACTION
        _saveIndex = 0
        startCol = self.getColumn()
        if (self.LA(1)==u'<') and (self.LA(2)==u'\\') and (self.LA(3)==u'n') and (self.LA(4)==u'>') and (True) and (True) and (True):
            pass
            _saveIndex = self.text.length()
            self.match("<\\n>")
            self.text.setLength(_saveIndex)
            self.text.setLength(_begin) ; self.text.append('\n'); _ttype = LITERAL
        elif (self.LA(1)==u'<') and (self.LA(2)==u'\\') and (self.LA(3)==u'r') and (self.LA(4)==u'>') and (True) and (True) and (True):
            pass
            _saveIndex = self.text.length()
            self.match("<\\r>")
            self.text.setLength(_saveIndex)
            self.text.setLength(_begin) ; self.text.append('\r'); _ttype = LITERAL
        elif (self.LA(1)==u'<') and (self.LA(2)==u'\\') and (self.LA(3)==u't') and (self.LA(4)==u'>') and (True) and (True) and (True):
            pass
            _saveIndex = self.text.length()
            self.match("<\\t>")
            self.text.setLength(_saveIndex)
            self.text.setLength(_begin) ; self.text.append('\t'); _ttype = LITERAL
        elif (self.LA(1)==u'<') and (self.LA(2)==u'\\') and (self.LA(3)==u' ') and (self.LA(4)==u'>') and (True) and (True) and (True):
            pass
            _saveIndex = self.text.length()
            self.match("<\\ >")
            self.text.setLength(_saveIndex)
            self.text.setLength(_begin) ; self.text.append(' '); _ttype = LITERAL
        elif (self.LA(1)==u'<') and (self.LA(2)==u'!') and ((self.LA(3) >= u'\u0001' and self.LA(3) <= u'\ufffe')) and ((self.LA(4) >= u'\u0001' and self.LA(4) <= u'\ufffe')) and (True) and (True) and (True):
            pass
            self.mCOMMENT(False)
            _ttype = SKIP
        elif (self.LA(1)==u'<') and (_tokenSet_2.member(self.LA(2))) and ((self.LA(3) >= u'\u0001' and self.LA(3) <= u'\ufffe')) and (True) and (True) and (True) and (True):
            pass
            if (self.LA(1)==u'<') and (self.LA(2)==u'i') and (self.LA(3)==u'f') and (self.LA(4)==u' ' or self.LA(4)==u'(') and (_tokenSet_3.member(self.LA(5))) and ((self.LA(6) >= u'\u0001' and self.LA(6) <= u'\ufffe')) and ((self.LA(7) >= u'\u0001' and self.LA(7) <= u'\ufffe')):
                pass
                _saveIndex = self.text.length()
                self.match('<')
                self.text.setLength(_saveIndex)
                self.match("if")
                while True:
                    if (self.LA(1)==u' '):
                        pass
                        _saveIndex = self.text.length()
                        self.match(' ')
                        self.text.setLength(_saveIndex)
                    else:
                        break
                    
                self.match("(")
                _cnt16= 0
                while True:
                    if (_tokenSet_3.member(self.LA(1))):
                        pass
                        self.matchNot(')')
                    else:
                        break
                    
                    _cnt16 += 1
                if _cnt16 < 1:
                    self.raise_NoViableAlt(self.LA(1))
                self.match(")")
                _saveIndex = self.text.length()
                self.match('>')
                self.text.setLength(_saveIndex)
                _ttype = IF
                if (self.LA(1)==u'\n' or self.LA(1)==u'\r'):
                    pass
                    _saveIndex = self.text.length()
                    self.mNL(False)
                    self.text.setLength(_saveIndex)
                    self.newline()
                else: ## <m4>
                        pass
                    
            elif (self.LA(1)==u'<') and (self.LA(2)==u'e') and (self.LA(3)==u'n') and (self.LA(4)==u'd') and (self.LA(5)==u'i') and (self.LA(6)==u'f') and (self.LA(7)==u'>'):
                pass
                _saveIndex = self.text.length()
                self.match('<')
                self.text.setLength(_saveIndex)
                self.match("endif")
                _saveIndex = self.text.length()
                self.match('>')
                self.text.setLength(_saveIndex)
                _ttype = ENDIF
                if ((self.LA(1)==u'\n' or self.LA(1)==u'\r') and (startCol==1)):
                    pass
                    _saveIndex = self.text.length()
                    self.mNL(False)
                    self.text.setLength(_saveIndex)
                    self.newline()
                else: ## <m4>
                        pass
                    
            elif (self.LA(1)==u'<') and (self.LA(2)==u'e') and (self.LA(3)==u'l') and (self.LA(4)==u's') and (self.LA(5)==u'e') and (self.LA(6)==u'>') and (True):
                pass
                _saveIndex = self.text.length()
                self.match('<')
                self.text.setLength(_saveIndex)
                self.match("else")
                _saveIndex = self.text.length()
                self.match('>')
                self.text.setLength(_saveIndex)
                _ttype = ELSE
                if (self.LA(1)==u'\n' or self.LA(1)==u'\r'):
                    pass
                    _saveIndex = self.text.length()
                    self.mNL(False)
                    self.text.setLength(_saveIndex)
                    self.newline()
                else: ## <m4>
                        pass
                    
            elif (self.LA(1)==u'<') and (_tokenSet_2.member(self.LA(2))) and ((self.LA(3) >= u'\u0001' and self.LA(3) <= u'\ufffe')) and (True) and (True) and (True) and (True):
                pass
                _saveIndex = self.text.length()
                self.match('<')
                self.text.setLength(_saveIndex)
                self.mEXPR(False)
                _saveIndex = self.text.length()
                self.match('>')
                self.text.setLength(_saveIndex)
            else:
                self.raise_NoViableAlt(self.LA(1))
            
            t = ChunkToken(_ttype, self.text.getString(_begin), self.currentIndent)
            _token = t
        else:
            self.raise_NoViableAlt(self.LA(1))
        
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mCOMMENT(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = COMMENT
        _saveIndex = 0
        startCol = self.getColumn()
        pass
        self.match("<!")
        while True:
            ###  nongreedy exit test
            if ((self.LA(1)==u'!') and (self.LA(2)==u'>') and (True) and (True) and (True) and (True) and (True)):
                break
            if (self.LA(1)==u'\n' or self.LA(1)==u'\r') and ((self.LA(2) >= u'\u0001' and self.LA(2) <= u'\ufffe')) and ((self.LA(3) >= u'\u0001' and self.LA(3) <= u'\ufffe')) and (True) and (True) and (True) and (True):
                pass
                self.mNL(False)
                self.newline()
            elif ((self.LA(1) >= u'\u0001' and self.LA(1) <= u'\ufffe')) and ((self.LA(2) >= u'\u0001' and self.LA(2) <= u'\ufffe')) and ((self.LA(3) >= u'\u0001' and self.LA(3) <= u'\ufffe')) and (True) and (True) and (True) and (True):
                pass
                self.matchNot(antlr.EOF_CHAR)
            else:
                break
            
        self.match("!>")
        if ((self.LA(1)==u'\n' or self.LA(1)==u'\r') and (startCol==1)):
            pass
            self.mNL(False)
            self.newline()
        else: ## <m4>
                pass
            
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mNL(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = NL
        _saveIndex = 0
        if (self.LA(1)==u'\r') and (self.LA(2)==u'\n') and (True) and (True) and (True) and (True) and (True):
            pass
            self.match('\r')
            self.match('\n')
        elif (self.LA(1)==u'\r') and (True) and (True) and (True) and (True) and (True) and (True):
            pass
            self.match('\r')
        elif (self.LA(1)==u'\n'):
            pass
            self.match('\n')
        else:
            self.raise_NoViableAlt(self.LA(1))
        
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mEXPR(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = EXPR
        _saveIndex = 0
        pass
        _cnt23= 0
        while True:
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in u'\\':
                pass
                self.mESC(False)
            elif la1 and la1 in u'\n\r':
                pass
                self.mNL(False)
                self.newline()
            elif la1 and la1 in u'{':
                pass
                self.mSUBTEMPLATE(False)
            else:
                if (_tokenSet_4.member(self.LA(1))):
                    pass
                    self.matchNot('>')
                else:
                    break
                
            _cnt23 += 1
        if _cnt23 < 1:
            self.raise_NoViableAlt(self.LA(1))
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mESC(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = ESC
        _saveIndex = 0
        pass
        self.match('\\')
        la1 = self.LA(1)
        if False:
            pass
        elif la1 and la1 in u'<':
            pass
            self.match('<')
        elif la1 and la1 in u'>':
            pass
            self.match('>')
        elif la1 and la1 in u'n':
            pass
            self.match('n')
        elif la1 and la1 in u't':
            pass
            self.match('t')
        elif la1 and la1 in u'\\':
            pass
            self.match('\\')
        elif la1 and la1 in u'"':
            pass
            self.match('"')
        elif la1 and la1 in u'\'':
            pass
            self.match('\'')
        elif la1 and la1 in u':':
            pass
            self.match(':')
        elif la1 and la1 in u'{':
            pass
            self.match('{')
        elif la1 and la1 in u'}':
            pass
            self.match('}')
        else:
                self.raise_NoViableAlt(self.LA(1))
            
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mSUBTEMPLATE(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = SUBTEMPLATE
        _saveIndex = 0
        pass
        self.match('{')
        _cnt28= 0
        while True:
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in u'{':
                pass
                self.mSUBTEMPLATE(False)
            elif la1 and la1 in u'\\':
                pass
                self.mESC(False)
            else:
                if (_tokenSet_5.member(self.LA(1))):
                    pass
                    self.matchNot('}')
                else:
                    break
                
            _cnt28 += 1
        if _cnt28 < 1:
            self.raise_NoViableAlt(self.LA(1))
        self.match('}')
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    

### generate bit set
def mk_tokenSet_0(): 
    data = [0L] * 2048 ### init list
    data[0] =-1152921504606856194L
    for x in xrange(1, 1023):
        data[x] = -1L
    data[1023] =9223372036854775807L
    return data
_tokenSet_0 = antlr.BitSet(mk_tokenSet_0())

### generate bit set
def mk_tokenSet_1(): 
    data = [0L] * 2048 ### init list
    data[0] =-5764607523034234882L
    for x in xrange(1, 1023):
        data[x] = -1L
    data[1023] =9223372036854775807L
    return data
_tokenSet_1 = antlr.BitSet(mk_tokenSet_1())

### generate bit set
def mk_tokenSet_2(): 
    data = [0L] * 2048 ### init list
    data[0] =-4611686018427387906L
    for x in xrange(1, 1023):
        data[x] = -1L
    data[1023] =9223372036854775807L
    return data
_tokenSet_2 = antlr.BitSet(mk_tokenSet_2())

### generate bit set
def mk_tokenSet_3(): 
    data = [0L] * 2048 ### init list
    data[0] =-2199023255554L
    for x in xrange(1, 1023):
        data[x] = -1L
    data[1023] =9223372036854775807L
    return data
_tokenSet_3 = antlr.BitSet(mk_tokenSet_3())

### generate bit set
def mk_tokenSet_4(): 
    data = [0L] * 2048 ### init list
    data[0] =-4611686018427397122L
    data[1] =-576460752571858945L
    for x in xrange(2, 1023):
        data[x] = -1L
    data[1023] =9223372036854775807L
    return data
_tokenSet_4 = antlr.BitSet(mk_tokenSet_4())

### generate bit set
def mk_tokenSet_5(): 
    data = [0L] * 2048 ### init list
    data[0] =-2L
    data[1] =-2882303761785552897L
    for x in xrange(2, 1023):
        data[x] = -1L
    data[1023] =9223372036854775807L
    return data
_tokenSet_5 = antlr.BitSet(mk_tokenSet_5())
    
### __main__ header action >>> 
if __name__ == '__main__' :
    import sys
    import antlr
    import AngleBracketTemplateLexer
    
    ### create lexer - shall read from stdin
    try:
        for token in AngleBracketTemplateLexer.Lexer():
            print token
            
    except antlr.TokenStreamException, e:
        print "error: exception caught while lexing: ", e
### __main__ header action <<< 
