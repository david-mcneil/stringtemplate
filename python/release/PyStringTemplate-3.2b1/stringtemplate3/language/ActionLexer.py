### $ANTLR 2.7.7 (2006-11-01): "action.g" -> "ActionLexer.py"$
### import antlr and other modules ..
import sys
import antlr

version = sys.version.split()[0]
if version < '2.2.1':
    False = 0
if version < '2.3':
    True = not False
### header action >>> 
from stringtemplate3.language.StringTemplateToken import StringTemplateToken
import stringtemplate3
### header action <<< 
### preamble action >>> 

### preamble action <<< 
### >>>The Literals<<<
literals = {}
literals[u"super"] = 32
literals[u"if"] = 8
literals[u"first"] = 26
literals[u"last"] = 28
literals[u"rest"] = 27
literals[u"trunc"] = 31
literals[u"strip"] = 30
literals[u"length"] = 29
literals[u"elseif"] = 18


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
MULTI_APPLY = 5
ARGS = 6
INCLUDE = 7
CONDITIONAL = 8
VALUE = 9
TEMPLATE = 10
FUNCTION = 11
SINGLEVALUEARG = 12
LIST = 13
NOTHING = 14
SEMI = 15
LPAREN = 16
RPAREN = 17
LITERAL_elseif = 18
COMMA = 19
ID = 20
ASSIGN = 21
COLON = 22
NOT = 23
PLUS = 24
DOT = 25
LITERAL_first = 26
LITERAL_rest = 27
LITERAL_last = 28
LITERAL_length = 29
LITERAL_strip = 30
LITERAL_trunc = 31
LITERAL_super = 32
ANONYMOUS_TEMPLATE = 33
STRING = 34
INT = 35
LBRACK = 36
RBRACK = 37
DOTDOTDOT = 38
TEMPLATE_ARGS = 39
NESTED_ANONYMOUS_TEMPLATE = 40
ESC_CHAR = 41
WS = 42
WS_CHAR = 43

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
                            elif la1 and la1 in u'[':
                                pass
                                self.mLBRACK(True)
                                theRetToken = self._returnToken
                            elif la1 and la1 in u']':
                                pass
                                self.mRBRACK(True)
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
                                if (self.LA(1)==u'.') and (self.LA(2)==u'.'):
                                    pass
                                    self.mDOTDOTDOT(True)
                                    theRetToken = self._returnToken
                                elif (self.LA(1)==u'.') and (True):
                                    pass
                                    self.mDOT(True)
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
        _cnt63= 0
        while True:
            if ((self.LA(1) >= u'0' and self.LA(1) <= u'9')):
                pass
                self.matchRange(u'0', u'9')
            else:
                break
            
            _cnt63 += 1
        if _cnt63 < 1:
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
    

    ###/** Match escape sequences, optionally translating them for strings, but not
    ### *  for templates.  Do \} only when in {...} templates.
    ### */
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
            if not self.inputState.guessing:
                if doEscape: self.text.setLength(_begin) ; self.text.append("\n")
        elif (self.LA(1)==u'r') and ((self.LA(2) >= u'\u0003' and self.LA(2) <= u'\ufffe')):
            pass
            self.match('r')
            if not self.inputState.guessing:
                if doEscape: self.text.setLength(_begin) ; self.text.append("\r")
        elif (self.LA(1)==u't') and ((self.LA(2) >= u'\u0003' and self.LA(2) <= u'\ufffe')):
            pass
            self.match('t')
            if not self.inputState.guessing:
                if doEscape: self.text.setLength(_begin) ; self.text.append("\t")
        elif (self.LA(1)==u'b') and ((self.LA(2) >= u'\u0003' and self.LA(2) <= u'\ufffe')):
            pass
            self.match('b')
            if not self.inputState.guessing:
                if doEscape: self.text.setLength(_begin) ; self.text.append("\b")
        elif (self.LA(1)==u'f') and ((self.LA(2) >= u'\u0003' and self.LA(2) <= u'\ufffe')):
            pass
            self.match('f')
            if not self.inputState.guessing:
                if doEscape: self.text.setLength(_begin) ; self.text.append("\f")
        elif ((self.LA(1) >= u'\u0003' and self.LA(1) <= u'\ufffe')) and ((self.LA(2) >= u'\u0003' and self.LA(2) <= u'\ufffe')):
            pass
            c = self.LA(1)
            self.matchNot(antlr.EOF_CHAR)
            if not self.inputState.guessing:
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
        args = None
        t = None
        pass
        _saveIndex = self.text.length()
        self.match('{')
        self.text.setLength(_saveIndex)
        synPredMatched70 = False
        if (_tokenSet_1.member(self.LA(1))) and (_tokenSet_2.member(self.LA(2))):
            _m70 = self.mark()
            synPredMatched70 = True
            self.inputState.guessing += 1
            try:
                pass
                self.mTEMPLATE_ARGS(False)
            except antlr.RecognitionException, pe:
                synPredMatched70 = False
            self.rewind(_m70)
            self.inputState.guessing -= 1
        if synPredMatched70:
            pass
            args=self.mTEMPLATE_ARGS(False)
            if (_tokenSet_3.member(self.LA(1))) and ((self.LA(2) >= u'\u0003' and self.LA(2) <= u'\ufffe')):
                pass
                _saveIndex = self.text.length()
                self.mWS_CHAR(False)
                self.text.setLength(_saveIndex)
            elif ((self.LA(1) >= u'\u0003' and self.LA(1) <= u'\ufffe')) and (True):
                pass
            else:
                self.raise_NoViableAlt(self.LA(1))
            
            if not self.inputState.guessing:
                # create a special token to track args
                t = StringTemplateToken(ANONYMOUS_TEMPLATE, self.text.getString(_begin), args)
                _token = t
        elif ((self.LA(1) >= u'\u0003' and self.LA(1) <= u'\ufffe')) and (True):
            pass
        else:
            self.raise_NoViableAlt(self.LA(1))
        
        while True:
            if (self.LA(1)==u'\\') and (self.LA(2)==u'{'):
                pass
                _saveIndex = self.text.length()
                self.match('\\')
                self.text.setLength(_saveIndex)
                self.match('{')
            elif (self.LA(1)==u'\\') and (self.LA(2)==u'}'):
                pass
                _saveIndex = self.text.length()
                self.match('\\')
                self.text.setLength(_saveIndex)
                self.match('}')
            elif (self.LA(1)==u'\\') and ((self.LA(2) >= u'\u0003' and self.LA(2) <= u'\ufffe')):
                pass
                self.mESC_CHAR(False, False)
            elif (self.LA(1)==u'{'):
                pass
                self.mNESTED_ANONYMOUS_TEMPLATE(False)
            elif (_tokenSet_4.member(self.LA(1))):
                pass
                self.matchNot('}')
            else:
                break
            
        if not self.inputState.guessing:
            if t:
               t.setText(self.text.getString(_begin))
        _saveIndex = self.text.length()
        self.match('}')
        self.text.setLength(_saveIndex)
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mTEMPLATE_ARGS(self, _createToken):    
        args=[]
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = TEMPLATE_ARGS
        _saveIndex = 0
        a = None
        a2 = None
        pass
        la1 = self.LA(1)
        if False:
            pass
        elif la1 and la1 in u'\t\n\r ':
            pass
            _saveIndex = self.text.length()
            self.mWS_CHAR(False)
            self.text.setLength(_saveIndex)
        elif la1 and la1 in u'ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz':
            pass
        else:
                self.raise_NoViableAlt(self.LA(1))
            
        _saveIndex = self.text.length()
        self.mID(True)
        self.text.setLength(_saveIndex)
        a = self._returnToken
        if not self.inputState.guessing:
            args.append(a.getText())
        while True:
            if (_tokenSet_5.member(self.LA(1))) and (_tokenSet_6.member(self.LA(2))):
                pass
                la1 = self.LA(1)
                if False:
                    pass
                elif la1 and la1 in u'\t\n\r ':
                    pass
                    _saveIndex = self.text.length()
                    self.mWS_CHAR(False)
                    self.text.setLength(_saveIndex)
                elif la1 and la1 in u',':
                    pass
                else:
                        self.raise_NoViableAlt(self.LA(1))
                    
                _saveIndex = self.text.length()
                self.match(',')
                self.text.setLength(_saveIndex)
                la1 = self.LA(1)
                if False:
                    pass
                elif la1 and la1 in u'\t\n\r ':
                    pass
                    _saveIndex = self.text.length()
                    self.mWS_CHAR(False)
                    self.text.setLength(_saveIndex)
                elif la1 and la1 in u'ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz':
                    pass
                else:
                        self.raise_NoViableAlt(self.LA(1))
                    
                _saveIndex = self.text.length()
                self.mID(True)
                self.text.setLength(_saveIndex)
                a2 = self._returnToken
                if not self.inputState.guessing:
                    args.append(a2.getText())
            else:
                break
            
        la1 = self.LA(1)
        if False:
            pass
        elif la1 and la1 in u'\t\n\r ':
            pass
            _saveIndex = self.text.length()
            self.mWS_CHAR(False)
            self.text.setLength(_saveIndex)
        elif la1 and la1 in u'|':
            pass
        else:
                self.raise_NoViableAlt(self.LA(1))
            
        _saveIndex = self.text.length()
        self.match('|')
        self.text.setLength(_saveIndex)
        self.set_return_token(_createToken, _token, _ttype, _begin)
        return args
    
    def mWS_CHAR(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = WS_CHAR
        _saveIndex = 0
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
            if (self.LA(1)==u'\r') and ((self.LA(2) >= u'\u0003' and self.LA(2) <= u'\ufffe')):
                pass
                self.match('\r')
            elif (self.LA(1)==u'\r') and (self.LA(2)==u'\n'):
                pass
                self.match('\r')
                self.match('\n')
            elif (self.LA(1)==u'\n'):
                pass
                self.match('\n')
            else:
                self.raise_NoViableAlt(self.LA(1))
            
            if not self.inputState.guessing:
                self.newline()
        else:
                self.raise_NoViableAlt(self.LA(1))
            
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
            if (self.LA(1)==u'\\') and (self.LA(2)==u'{'):
                pass
                _saveIndex = self.text.length()
                self.match('\\')
                self.text.setLength(_saveIndex)
                self.match('{')
            elif (self.LA(1)==u'\\') and (self.LA(2)==u'}'):
                pass
                _saveIndex = self.text.length()
                self.match('\\')
                self.text.setLength(_saveIndex)
                self.match('}')
            elif (self.LA(1)==u'\\') and ((self.LA(2) >= u'\u0003' and self.LA(2) <= u'\ufffe')):
                pass
                self.mESC_CHAR(False, False)
            elif (self.LA(1)==u'{'):
                pass
                self.mNESTED_ANONYMOUS_TEMPLATE(False)
            elif (_tokenSet_4.member(self.LA(1))):
                pass
                self.matchNot('}')
            else:
                break
            
        self.match('}')
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
    
    def mDOTDOTDOT(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = DOTDOTDOT
        _saveIndex = 0
        pass
        self.match("...")
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
            
            if not self.inputState.guessing:
                self.newline()
        else:
                self.raise_NoViableAlt(self.LA(1))
            
        if not self.inputState.guessing:
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
    data = [0L] * 1025 ### init list
    data[0] =4294977024L
    data[1] =576460745995190270L
    return data
_tokenSet_1 = antlr.BitSet(mk_tokenSet_1())

### generate bit set
def mk_tokenSet_2(): 
    data = [0L] * 1025 ### init list
    data[0] =288107235144377856L
    data[1] =1729382250602037246L
    return data
_tokenSet_2 = antlr.BitSet(mk_tokenSet_2())

### generate bit set
def mk_tokenSet_3(): 
    data = [0L] * 1025 ### init list
    data[0] =4294977024L
    return data
_tokenSet_3 = antlr.BitSet(mk_tokenSet_3())

### generate bit set
def mk_tokenSet_4(): 
    data = [0L] * 2048 ### init list
    data[0] =-8L
    data[1] =-2882303761785552897L
    for x in xrange(2, 1023):
        data[x] = -1L
    data[1023] =9223372036854775807L
    return data
_tokenSet_4 = antlr.BitSet(mk_tokenSet_4())

### generate bit set
def mk_tokenSet_5(): 
    data = [0L] * 1025 ### init list
    data[0] =17596481021440L
    return data
_tokenSet_5 = antlr.BitSet(mk_tokenSet_5())

### generate bit set
def mk_tokenSet_6(): 
    data = [0L] * 1025 ### init list
    data[0] =17596481021440L
    data[1] =576460745995190270L
    return data
_tokenSet_6 = antlr.BitSet(mk_tokenSet_6())
    
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
