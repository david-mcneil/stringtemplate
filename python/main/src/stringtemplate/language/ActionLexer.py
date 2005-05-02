### $ANTLR 2.7.5 (20050419): "action.g" -> "ActionLexer.py"$
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
### preamble action >>> 

### preamble action <<< 
### >>>The Literals<<<
literals = {}
literals[u"separator"] = 13
literals[u"if"] = 7
literals[u"super"] = 21


### import antlr.Token 
from antlr import Token
### >>>The Known Token Types <<<
SKIP                = antlr.SKIP
INVALID_TYPE        = antlr.INVALID_TYPE
EOF_TYPE            = antlr.EOF_TYPE
EOF                 = antlr.EOF
NULL_TREE_LOOKAHEAD = antlr.NULL_TREE_LOOKAHEAD
MIN_USER_TYPE       = antlr.MIN_USER_TYPE
APPLY = 4
ARGS = 5
INCLUDE = 6
CONDITIONAL = 7
VALUE = 8
TEMPLATE = 9
SEMI = 10
LPAREN = 11
RPAREN = 12
LITERAL_separator = 13
ASSIGN = 14
COLON = 15
COMMA = 16
NOT = 17
PLUS = 18
DOT = 19
ID = 20
LITERAL_super = 21
ANONYMOUS_TEMPLATE = 22
STRING = 23
INT = 24
NESTED_ANONYMOUS_TEMPLATE = 25
ESC_CHAR = 26
WS = 27

class Lexer(antlr.CharScanner) :
    ### user action >>>
    ### user action <<<
    def __init__(self, *argv, **kwargs) :
        antlr.CharScanner.__init__(self, *argv, **kwargs)
        self.caseSensitiveLiterals = True
        self.setCaseSensitive(True)
        self.literals = literals
    
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
                            elif la1 and la1 in u'ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz':
                                pass
                                self.mID(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'0123456789':
                                pass
                                self.mINT(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'"':
                                pass
                                self.mSTRING(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'{':
                                pass
                                self.mANONYMOUS_TEMPLATE(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'(':
                                pass
                                self.mLPAREN(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u')':
                                pass
                                self.mRPAREN(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u',':
                                pass
                                self.mCOMMA(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'.':
                                pass
                                self.mDOT(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'=':
                                pass
                                self.mASSIGN(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u':':
                                pass
                                self.mCOLON(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'+':
                                pass
                                self.mPLUS(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u';':
                                pass
                                self.mSEMI(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'!':
                                pass
                                self.mNOT(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'\t\n\r ':
                                pass
                                self.mWS(True)
                                theRetToken = self._returnToken
                            else:
                                    self.default(self.LA(1))
                                
                            if not self._returnToken:
                                raise antlr.TryAgain ### found SKIP token
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
        
    def mID(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = ID
        _saveIndex = 0
        pass
        la1 = self.LA(1)
        if False:
            pass
        elif la1 and la1 in u'abcdefghijklmnopqrstuvwxyz':
            pass
            self.matchRange(u'a', u'z')
        elif la1 and la1 in u'ABCDEFGHIJKLMNOPQRSTUVWXYZ':
            pass
            self.matchRange(u'A', u'Z')
        elif la1 and la1 in u'_':
            pass
            self.match('_')
        else:
                self.raise_NoViableAlt(self.LA(1))
            
        while True:
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in u'abcdefghijklmnopqrstuvwxyz':
                pass
                self.matchRange(u'a', u'z')
            elif la1 and la1 in u'ABCDEFGHIJKLMNOPQRSTUVWXYZ':
                pass
                self.matchRange(u'A', u'Z')
            elif la1 and la1 in u'0123456789':
                pass
                self.matchRange(u'0', u'9')
            elif la1 and la1 in u'_':
                pass
                self.match('_')
            elif la1 and la1 in u'/':
                pass
                self.match('/')
            else:
                    break
                
        ### option { testLiterals=true } 
        _ttype = self.testLiteralsTable(_ttype)
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mINT(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = INT
        _saveIndex = 0
        pass
        _cnt42= 0
        while True:
            if ((self.LA(1) >= u'0' and self.LA(1) <= u'9')):
                pass
                self.matchRange(u'0', u'9')
            else:
                break
            
            _cnt42 += 1
        if _cnt42 < 1:
            self.raise_NoViableAlt(self.LA(1))
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mSTRING(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = STRING
        _saveIndex = 0
        pass
        _saveIndex = self.text.length()
        self.match('"')
        self.text.setLength(_saveIndex)
        while True:
            if (self.LA(1)==u'\\'):
                pass
                self.mESC_CHAR(False, True)
            elif (_tokenSet_0.member(self.LA(1))):
                pass
                self.matchNot('"')
            else:
                break
            
        _saveIndex = self.text.length()
        self.match('"')
        self.text.setLength(_saveIndex)
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mESC_CHAR(self, _createToken,
        doEscape
    ):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = ESC_CHAR
        _saveIndex = 0
        c = '\0'
        pass
        self.match('\\')
        if (self.LA(1)==u'n') and ((self.LA(2) >= u'\u0003' and self.LA(2) <= u'\ufffe')):
            pass
            self.match('n')
            if doEscape: self.text.setLength(_begin) ; self.text.append("\n")
        elif (self.LA(1)==u'r') and ((self.LA(2) >= u'\u0003' and self.LA(2) <= u'\ufffe')):
            pass
            self.match('r')
            if doEscape: self.text.setLength(_begin) ; self.text.append("\r")
        elif (self.LA(1)==u't') and ((self.LA(2) >= u'\u0003' and self.LA(2) <= u'\ufffe')):
            pass
            self.match('t')
            if doEscape: self.text.setLength(_begin) ; self.text.append("\t")
        elif (self.LA(1)==u'b') and ((self.LA(2) >= u'\u0003' and self.LA(2) <= u'\ufffe')):
            pass
            self.match('b')
            if doEscape: self.text.setLength(_begin) ; self.text.append("\b")
        elif (self.LA(1)==u'f') and ((self.LA(2) >= u'\u0003' and self.LA(2) <= u'\ufffe')):
            pass
            self.match('f')
            if doEscape: self.text.setLength(_begin) ; self.text.append("\f")
        elif ((self.LA(1) >= u'\u0003' and self.LA(1) <= u'\ufffe')) and ((self.LA(2) >= u'\u0003' and self.LA(2) <= u'\ufffe')):
            pass
            c = self.LA(1)
            self.matchNot(antlr.EOF_CHAR)
            if doEscape: self.text.setLength(_begin) ; self.text.append(str(c))
        else:
            self.raise_NoViableAlt(self.LA(1))
        
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mANONYMOUS_TEMPLATE(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = ANONYMOUS_TEMPLATE
        _saveIndex = 0
        pass
        _saveIndex = self.text.length()
        self.match('{')
        self.text.setLength(_saveIndex)
        while True:
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in u'\\':
                pass
                self.mESC_CHAR(False, False)
            elif la1 and la1 in u'{':
                pass
                self.mNESTED_ANONYMOUS_TEMPLATE(False)
            else:
                if (_tokenSet_1.member(self.LA(1))):
                    pass
                    self.matchNot('}')
                else:
                    break
                
        _saveIndex = self.text.length()
        self.match('}')
        self.text.setLength(_saveIndex)
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mNESTED_ANONYMOUS_TEMPLATE(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = NESTED_ANONYMOUS_TEMPLATE
        _saveIndex = 0
        pass
        self.match('{')
        while True:
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in u'\\':
                pass
                self.mESC_CHAR(False, False)
            elif la1 and la1 in u'{':
                pass
                self.mNESTED_ANONYMOUS_TEMPLATE(False)
            else:
                if (_tokenSet_1.member(self.LA(1))):
                    pass
                    self.matchNot('}')
                else:
                    break
                
        self.match('}')
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mLPAREN(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = LPAREN
        _saveIndex = 0
        pass
        self.match('(')
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mRPAREN(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = RPAREN
        _saveIndex = 0
        pass
        self.match(')')
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mCOMMA(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = COMMA
        _saveIndex = 0
        pass
        self.match(',')
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mDOT(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = DOT
        _saveIndex = 0
        pass
        self.match('.')
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mASSIGN(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = ASSIGN
        _saveIndex = 0
        pass
        self.match('=')
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mCOLON(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = COLON
        _saveIndex = 0
        pass
        self.match(':')
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mPLUS(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = PLUS
        _saveIndex = 0
        pass
        self.match('+')
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mSEMI(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = SEMI
        _saveIndex = 0
        pass
        self.match(';')
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mNOT(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = NOT
        _saveIndex = 0
        pass
        self.match('!')
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mWS(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = WS
        _saveIndex = 0
        pass
        la1 = self.LA(1)
        if False:
            pass
        elif la1 and la1 in u' ':
            pass
            self.match(' ')
        elif la1 and la1 in u'\t':
            pass
            self.match('\t')
        elif la1 and la1 in u'\n\r':
            pass
            if (self.LA(1)==u'\r') and (self.LA(2)==u'\n'):
                pass
                self.match('\r')
                self.match('\n')
            elif (self.LA(1)==u'\r') and (True):
                pass
                self.match('\r')
            elif (self.LA(1)==u'\n'):
                pass
                self.match('\n')
            else:
                self.raise_NoViableAlt(self.LA(1))
            
            self.newline()
        else:
                self.raise_NoViableAlt(self.LA(1))
            
        _ttype = SKIP
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    

### generate bit set
def mk_tokenSet_0(): 
    data = [0L] * 2048 ### init list
    data[0] =-17179869192L
    data[1] =-268435457L
    for x in xrange(2, 1023):
        data[x] = -1L
    data[1023] =9223372036854775807L
    return data
_tokenSet_0 = antlr.BitSet(mk_tokenSet_0())

### generate bit set
def mk_tokenSet_1(): 
    data = [0L] * 2048 ### init list
    data[0] =-8L
    data[1] =-2882303761785552897L
    for x in xrange(2, 1023):
        data[x] = -1L
    data[1023] =9223372036854775807L
    return data
_tokenSet_1 = antlr.BitSet(mk_tokenSet_1())
    
### __main__ header action >>> 
if __name__ == '__main__' :
    import sys
    import antlr
    import ActionLexer
    
    ### create lexer - shall read from stdin
    try:
        for token in ActionLexer.Lexer():
            print token
            
    except antlr.TokenStreamException, e:
        print "error: exception caught while lexing: ", e
### __main__ header action <<< 
