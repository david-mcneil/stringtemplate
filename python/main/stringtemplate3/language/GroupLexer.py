### $ANTLR 2.7.7 (2006-11-01): "group.g" -> "GroupLexer.py"$
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
import stringtemplate3
import traceback
### header action <<< 
### preamble action >>> 

### preamble action <<< 
### >>>The Literals<<<
literals = {}
literals[u"default"] = 21
literals[u"group"] = 4
literals[u"implements"] = 7


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
COLON = 6
LITERAL_implements = 7
COMMA = 8
SEMI = 9
AT = 10
DOT = 11
LPAREN = 12
RPAREN = 13
DEFINED_TO_BE = 14
STRING = 15
BIGSTRING = 16
ASSIGN = 17
ANONYMOUS_TEMPLATE = 18
LBRACK = 19
RBRACK = 20
LITERAL_default = 21
STAR = 22
PLUS = 23
OPTIONAL = 24
SL_COMMENT = 25
ML_COMMENT = 26
NL = 27
WS = 28

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
                            elif la1 and la1 in u'"':
                                pass
                                self.mSTRING(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'<':
                                pass
                                self.mBIGSTRING(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'{':
                                pass
                                self.mANONYMOUS_TEMPLATE(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'@':
                                pass
                                self.mAT(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'(':
                                pass
                                self.mLPAREN(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u')':
                                pass
                                self.mRPAREN(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'[':
                                pass
                                self.mLBRACK(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u']':
                                pass
                                self.mRBRACK(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u',':
                                pass
                                self.mCOMMA(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'.':
                                pass
                                self.mDOT(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u';':
                                pass
                                self.mSEMI(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'*':
                                pass
                                self.mSTAR(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'+':
                                pass
                                self.mPLUS(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'=':
                                pass
                                self.mASSIGN(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'?':
                                pass
                                self.mOPTIONAL(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u'\t\n\u000c\r ':
                                pass
                                self.mWS(True)
                                theRetToken = self._returnToken
                            else:
                                if (self.LA(1)==u':') and (self.LA(2)==u':'):
                                    pass
                                    self.mDEFINED_TO_BE(True)
                                    theRetToken = self._returnToken
                                elif (self.LA(1)==u'/') and (self.LA(2)==u'/'):
                                    pass
                                    self.mSL_COMMENT(True)
                                    theRetToken = self._returnToken
                                elif (self.LA(1)==u'/') and (self.LA(2)==u'*'):
                                    pass
                                    self.mML_COMMENT(True)
                                    theRetToken = self._returnToken
                                elif (self.LA(1)==u':') and (True):
                                    pass
                                    self.mCOLON(True)
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
            elif la1 and la1 in u'-':
                pass
                self.match('-')
            elif la1 and la1 in u'_':
                pass
                self.match('_')
            else:
                    break
                
        ### option { testLiterals=true } 
        _ttype = self.testLiteralsTable(_ttype)
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
            if (self.LA(1)==u'\\') and (self.LA(2)==u'"'):
                pass
                _saveIndex = self.text.length()
                self.match('\\')
                self.text.setLength(_saveIndex)
                self.match('"')
            elif (self.LA(1)==u'\\') and (_tokenSet_0.member(self.LA(2))):
                pass
                self.match('\\')
                self.matchNot('"')
            elif (_tokenSet_1.member(self.LA(1))):
                pass
                self.matchNot('"')
            else:
                break
            
        _saveIndex = self.text.length()
        self.match('"')
        self.text.setLength(_saveIndex)
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mBIGSTRING(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = BIGSTRING
        _saveIndex = 0
        pass
        _saveIndex = self.text.length()
        self.match("<<")
        self.text.setLength(_saveIndex)
        if (self.LA(1)==u'\n' or self.LA(1)==u'\r') and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\ufffe')):
            pass
            _saveIndex = self.text.length()
            self.mNL(False)
            self.text.setLength(_saveIndex)
            self.newline()
        elif ((self.LA(1) >= u'\u0000' and self.LA(1) <= u'\ufffe')) and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\ufffe')):
            pass
        else:
            self.raise_NoViableAlt(self.LA(1))
        
        while True:
            ###  nongreedy exit test
            if ((self.LA(1)==u'>') and (self.LA(2)==u'>')):
                break
            if ((self.LA(1)==u'\r') and (self.LA(2)==u'\n') and ( self.LA(3) == '>' and self.LA(4) == '>' )):
                pass
                _saveIndex = self.text.length()
                self.match('\r')
                self.text.setLength(_saveIndex)
                _saveIndex = self.text.length()
                self.match('\n')
                self.text.setLength(_saveIndex)
                self.newline()
            elif ((self.LA(1)==u'\n') and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\ufffe')) and ( self.LA(2) == '>' and self.LA(3) == '>' )):
                pass
                _saveIndex = self.text.length()
                self.match('\n')
                self.text.setLength(_saveIndex)
                self.newline()
            elif ((self.LA(1)==u'\r') and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\ufffe')) and ( self.LA(2) == '>' and self.LA(3) == '>' )):
                pass
                _saveIndex = self.text.length()
                self.match('\r')
                self.text.setLength(_saveIndex)
                self.newline()
            elif (self.LA(1)==u'\n' or self.LA(1)==u'\r') and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\ufffe')):
                pass
                self.mNL(False)
                self.newline()
            elif (self.LA(1)==u'\\') and (self.LA(2)==u'>'):
                pass
                _saveIndex = self.text.length()
                self.match('\\')
                self.text.setLength(_saveIndex)
                self.match('>')
            elif ((self.LA(1) >= u'\u0000' and self.LA(1) <= u'\ufffe')) and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\ufffe')):
                pass
                self.matchNot(antlr.EOF_CHAR)
            else:
                break
            
        _saveIndex = self.text.length()
        self.match(">>")
        self.text.setLength(_saveIndex)
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mNL(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = NL
        _saveIndex = 0
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
        
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mANONYMOUS_TEMPLATE(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = ANONYMOUS_TEMPLATE
        _saveIndex = 0
        args = None
        t = None
        pass
        _saveIndex = self.text.length()
        self.match('{')
        self.text.setLength(_saveIndex)
        while True:
            ###  nongreedy exit test
            if ((self.LA(1)==u'}') and (True)):
                break
            if (self.LA(1)==u'\n' or self.LA(1)==u'\r') and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\ufffe')):
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
            elif (self.LA(1)==u'\\') and (self.LA(2)==u'>'):
                pass
                _saveIndex = self.text.length()
                self.match('\\')
                self.text.setLength(_saveIndex)
                self.match('>')
            elif ((self.LA(1) >= u'\u0000' and self.LA(1) <= u'\ufffe')) and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\ufffe')):
                pass
                self.matchNot(antlr.EOF_CHAR)
            else:
                break
            
        _saveIndex = self.text.length()
        self.match('}')
        self.text.setLength(_saveIndex)
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mAT(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = AT
        _saveIndex = 0
        pass
        self.match('@')
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
    
    def mLBRACK(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = LBRACK
        _saveIndex = 0
        pass
        self.match('[')
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mRBRACK(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = RBRACK
        _saveIndex = 0
        pass
        self.match(']')
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
    
    def mDEFINED_TO_BE(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = DEFINED_TO_BE
        _saveIndex = 0
        pass
        self.match("::=")
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
    
    def mCOLON(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = COLON
        _saveIndex = 0
        pass
        self.match(':')
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mSTAR(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = STAR
        _saveIndex = 0
        pass
        self.match('*')
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
    
    def mASSIGN(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = ASSIGN
        _saveIndex = 0
        pass
        self.match('=')
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mOPTIONAL(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = OPTIONAL
        _saveIndex = 0
        pass
        self.match('?')
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mSL_COMMENT(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = SL_COMMENT
        _saveIndex = 0
        pass
        self.match("//")
        while True:
            if (_tokenSet_2.member(self.LA(1))):
                pass
                self.match(_tokenSet_2)
            else:
                break
            
        if (self.LA(1)==u'\n' or self.LA(1)==u'\r'):
            pass
            self.mNL(False)
        else: ## <m4>
                pass
            
        _ttype = SKIP; self.newline()
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mML_COMMENT(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = ML_COMMENT
        _saveIndex = 0
        pass
        self.match("/*")
        while True:
            ###  nongreedy exit test
            if ((self.LA(1)==u'*') and (self.LA(2)==u'/')):
                break
            if (self.LA(1)==u'\n' or self.LA(1)==u'\r') and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\ufffe')):
                pass
                self.mNL(False)
                self.newline()
            elif ((self.LA(1) >= u'\u0000' and self.LA(1) <= u'\ufffe')) and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\ufffe')):
                pass
                self.matchNot(antlr.EOF_CHAR)
            else:
                break
            
        self.match("*/")
        _ttype = SKIP
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mWS(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = WS
        _saveIndex = 0
        pass
        _cnt66= 0
        while True:
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in u' ':
                pass
                self.match(' ')
            elif la1 and la1 in u'\t':
                pass
                self.match('\t')
            elif la1 and la1 in u'\u000c':
                pass
                self.match('\f')
            elif la1 and la1 in u'\n\r':
                pass
                self.mNL(False)
                self.newline()
            else:
                    break
                
            _cnt66 += 1
        if _cnt66 < 1:
            self.raise_NoViableAlt(self.LA(1))
        _ttype = SKIP
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    

### generate bit set
def mk_tokenSet_0(): 
    data = [0L] * 2048 ### init list
    data[0] =-17179869185L
    for x in xrange(1, 1023):
        data[x] = -1L
    data[1023] =9223372036854775807L
    return data
_tokenSet_0 = antlr.BitSet(mk_tokenSet_0())

### generate bit set
def mk_tokenSet_1(): 
    data = [0L] * 2048 ### init list
    data[0] =-17179869185L
    data[1] =-268435457L
    for x in xrange(2, 1023):
        data[x] = -1L
    data[1023] =9223372036854775807L
    return data
_tokenSet_1 = antlr.BitSet(mk_tokenSet_1())

### generate bit set
def mk_tokenSet_2(): 
    data = [0L] * 2048 ### init list
    data[0] =-9217L
    for x in xrange(1, 1023):
        data[x] = -1L
    data[1023] =9223372036854775807L
    return data
_tokenSet_2 = antlr.BitSet(mk_tokenSet_2())
    
### __main__ header action >>> 
if __name__ == '__main__' :
    import sys
    import antlr
    import GroupLexer
    
    ### create lexer - shall read from stdin
    try:
        for token in GroupLexer.Lexer():
            print token
            
    except antlr.TokenStreamException, e:
        print "error: exception caught while lexing: ", e
### __main__ header action <<< 
