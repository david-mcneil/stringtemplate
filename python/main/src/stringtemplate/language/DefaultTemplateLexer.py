### $ANTLR 2.7.7 (2006-11-01): "template.g" -> "DefaultTemplateLexer.py"$
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
from NewlineRef import NewlineRef
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


###/** Break up an input text stream into chunks of either plain text
### *  or template actions in "$...$".  Treat IF and ENDIF tokens
### *  specially.
### */
class Lexer(antlr.CharScanner) :
    ### user action >>>
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
                            elif la1 and la1 in u'$':
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
            raise antlr.SemanticException(" self.LA(1) != '\\r' and self.LA(1) != '\\n' ")
        pass
        _cnt11= 0
        while True:
            loopStartIndex = self.text.length()
            col = self.getColumn()
            if (self.LA(1)==u'\\') and (self.LA(2)==u'$'):
                pass
                _saveIndex = self.text.length()
                self.match('\\')
                self.text.setLength(_saveIndex)
                self.match('$')
            elif (self.LA(1)==u'\\') and (self.LA(2)==u'\\') and (True) and (True) and (True) and (True) and (True):
                pass
                _saveIndex = self.text.length()
                self.match('\\')
                self.text.setLength(_saveIndex)
                self.match('\\')
            elif (self.LA(1)==u'\\') and (_tokenSet_1.member(self.LA(2))) and (True) and (True) and (True) and (True) and (True):
                pass
                self.match('\\')
                self.matchNot('$')
            elif (self.LA(1)==u'\t' or self.LA(1)==u' ') and (True) and (True) and (True) and (True) and (True) and (True):
                pass
                self.mINDENT(True)
                ind = self._returnToken
                if col == 1 and self.LA(1) == '$':
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
            
            _cnt11 += 1
        if _cnt11 < 1:
            self.raise_NoViableAlt(self.LA(1))
        if not len(self.text.getString(_begin)):
           _ttype = SKIP # pure indent?
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mINDENT(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = INDENT
        _saveIndex = 0
        pass
        _cnt68= 0
        while True:
            if (self.LA(1)==u' ') and (True) and (True) and (True) and (True) and (True) and (True):
                pass
                self.match(' ')
            elif (self.LA(1)==u'\t') and (True) and (True) and (True) and (True) and (True) and (True):
                pass
                self.match('\t')
            else:
                break
            
            _cnt68 += 1
        if _cnt68 < 1:
            self.raise_NoViableAlt(self.LA(1))
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mNEWLINE(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = NEWLINE
        _saveIndex = 0
        pass
        self.mNL(False)
        self.newline()
        self.currentIndent = None
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
    
    def mACTION(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = ACTION
        _saveIndex = 0
        startCol = self.getColumn()
        if (self.LA(1)==u'$') and (self.LA(2)==u'\\') and (self.LA(3)==u'n') and (self.LA(4)==u'$') and (True) and (True) and (True):
            pass
            _saveIndex = self.text.length()
            self.match("$\\n$")
            self.text.setLength(_saveIndex)
            self.text.setLength(_begin) ; self.text.append('\n'); _ttype = LITERAL
        elif (self.LA(1)==u'$') and (self.LA(2)==u'\\') and (self.LA(3)==u'r') and (self.LA(4)==u'$') and (True) and (True) and (True):
            pass
            _saveIndex = self.text.length()
            self.match("$\\r$")
            self.text.setLength(_saveIndex)
            self.text.setLength(_begin) ; self.text.append('\r'); _ttype = LITERAL
        elif (self.LA(1)==u'$') and (self.LA(2)==u'\\') and (self.LA(3)==u't') and (self.LA(4)==u'$') and (True) and (True) and (True):
            pass
            _saveIndex = self.text.length()
            self.match("$\\t$")
            self.text.setLength(_saveIndex)
            self.text.setLength(_begin) ; self.text.append('\t'); _ttype = LITERAL
        elif (self.LA(1)==u'$') and (self.LA(2)==u'\\') and (self.LA(3)==u' ') and (self.LA(4)==u'$') and (True) and (True) and (True):
            pass
            _saveIndex = self.text.length()
            self.match("$\\ $")
            self.text.setLength(_saveIndex)
            self.text.setLength(_begin) ; self.text.append(' '); _ttype = LITERAL
        elif (self.LA(1)==u'$') and (self.LA(2)==u'!') and ((self.LA(3) >= u'\u0000' and self.LA(3) <= u'\u007f')) and ((self.LA(4) >= u'\u0000' and self.LA(4) <= u'\u007f')) and (True) and (True) and (True):
            pass
            self.mCOMMENT(False)
            _ttype = SKIP
        elif (self.LA(1)==u'$') and (_tokenSet_1.member(self.LA(2))) and ((self.LA(3) >= u'\u0000' and self.LA(3) <= u'\u007f')) and (True) and (True) and (True) and (True):
            pass
            if (self.LA(1)==u'$') and (self.LA(2)==u'i') and (self.LA(3)==u'f') and (self.LA(4)==u' ' or self.LA(4)==u'(') and (_tokenSet_2.member(self.LA(5))) and ((self.LA(6) >= u'\u0000' and self.LA(6) <= u'\u007f')) and ((self.LA(7) >= u'\u0000' and self.LA(7) <= u'\u007f')):
                pass
                _saveIndex = self.text.length()
                self.match('$')
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
                self.mIF_EXPR(False)
                self.match(")")
                _saveIndex = self.text.length()
                self.match('$')
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
                    
            elif (self.LA(1)==u'$') and (self.LA(2)==u'e') and (self.LA(3)==u'l') and (self.LA(4)==u's') and (self.LA(5)==u'e') and (self.LA(6)==u'i') and (self.LA(7)==u'f'):
                pass
                _saveIndex = self.text.length()
                self.match('$')
                self.text.setLength(_saveIndex)
                self.match("elseif")
                while True:
                    if (self.LA(1)==u' '):
                        pass
                        _saveIndex = self.text.length()
                        self.match(' ')
                        self.text.setLength(_saveIndex)
                    else:
                        break
                    
                self.match("(")
                self.mIF_EXPR(False)
                self.match(")")
                _saveIndex = self.text.length()
                self.match('$')
                self.text.setLength(_saveIndex)
                _ttype = ELSEIF
                if (self.LA(1)==u'\n' or self.LA(1)==u'\r'):
                    pass
                    la1 = self.LA(1)
                    if False:
                        pass
                    elif la1 and la1 in u'\r':
                        pass
                        _saveIndex = self.text.length()
                        self.match('\r')
                        self.text.setLength(_saveIndex)
                    elif la1 and la1 in u'\n':
                        pass
                    else:
                            self.raise_NoViableAlt(self.LA(1))
                        
                    _saveIndex = self.text.length()
                    self.match('\n')
                    self.text.setLength(_saveIndex)
                    self.newline()
                else: ## <m4>
                        pass
                    
            elif (self.LA(1)==u'$') and (self.LA(2)==u'e') and (self.LA(3)==u'n') and (self.LA(4)==u'd') and (self.LA(5)==u'i') and (self.LA(6)==u'f') and (self.LA(7)==u'$'):
                pass
                _saveIndex = self.text.length()
                self.match('$')
                self.text.setLength(_saveIndex)
                self.match("endif")
                _saveIndex = self.text.length()
                self.match('$')
                self.text.setLength(_saveIndex)
                _ttype = ENDIF
                if ((self.LA(1)==u'\n' or self.LA(1)==u'\r') and ( startCol == 1 )):
                    pass
                    _saveIndex = self.text.length()
                    self.mNL(False)
                    self.text.setLength(_saveIndex)
                    self.newline()
                else: ## <m4>
                        pass
                    
            elif (self.LA(1)==u'$') and (self.LA(2)==u'e') and (self.LA(3)==u'l') and (self.LA(4)==u's') and (self.LA(5)==u'e') and (self.LA(6)==u'$') and (True):
                pass
                _saveIndex = self.text.length()
                self.match('$')
                self.text.setLength(_saveIndex)
                self.match("else")
                _saveIndex = self.text.length()
                self.match('$')
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
                    
            elif (self.LA(1)==u'$') and (self.LA(2)==u'@') and (_tokenSet_3.member(self.LA(3))) and ((self.LA(4) >= u'\u0000' and self.LA(4) <= u'\u007f')) and ((self.LA(5) >= u'\u0000' and self.LA(5) <= u'\u007f')) and ((self.LA(6) >= u'\u0000' and self.LA(6) <= u'\u007f')) and (True):
                pass
                _saveIndex = self.text.length()
                self.match('$')
                self.text.setLength(_saveIndex)
                _saveIndex = self.text.length()
                self.match('@')
                self.text.setLength(_saveIndex)
                _cnt26= 0
                while True:
                    if (_tokenSet_3.member(self.LA(1))):
                        pass
                        self.match(_tokenSet_3)
                    else:
                        break
                    
                    _cnt26 += 1
                if _cnt26 < 1:
                    self.raise_NoViableAlt(self.LA(1))
                la1 = self.LA(1)
                if False:
                    pass
                elif la1 and la1 in u'(':
                    pass
                    _saveIndex = self.text.length()
                    self.match("()")
                    self.text.setLength(_saveIndex)
                    _saveIndex = self.text.length()
                    self.match('$')
                    self.text.setLength(_saveIndex)
                    _ttype = REGION_REF
                elif la1 and la1 in u'$':
                    pass
                    _saveIndex = self.text.length()
                    self.match('$')
                    self.text.setLength(_saveIndex)
                    _ttype = REGION_DEF
                    t = self.text.getString(_begin)
                    self.text.setLength(_begin) ; self.text.append(t+"::=")
                    if (self.LA(1)==u'\n' or self.LA(1)==u'\r') and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\u007f')) and ((self.LA(3) >= u'\u0000' and self.LA(3) <= u'\u007f')) and (True) and (True) and (True) and (True):
                        pass
                        la1 = self.LA(1)
                        if False:
                            pass
                        elif la1 and la1 in u'\r':
                            pass
                            _saveIndex = self.text.length()
                            self.match('\r')
                            self.text.setLength(_saveIndex)
                        elif la1 and la1 in u'\n':
                            pass
                        else:
                                self.raise_NoViableAlt(self.LA(1))
                            
                        _saveIndex = self.text.length()
                        self.match('\n')
                        self.text.setLength(_saveIndex)
                        newline();
                    elif ((self.LA(1) >= u'\u0000' and self.LA(1) <= u'\u007f')) and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\u007f')) and (True) and (True) and (True) and (True) and (True):
                        pass
                    else:
                        self.raise_NoViableAlt(self.LA(1))
                    
                    atLeft = False
                    _cnt33= 0
                    while True:
                        if (((self.LA(1) >= u'\u0000' and self.LA(1) <= u'\u007f')) and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\u007f')) and (True) and (True) and (True) and (True) and (True) and (not (self.upcomingAtEND(1) or (self.upcomingNewline(1) and self.upcomingAtEND(2))))):
                            pass
                            if (self.LA(1)==u'\n' or self.LA(1)==u'\r') and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\u007f')) and (True) and (True) and (True) and (True) and (True):
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
                                atLeft = Frue
                            elif ((self.LA(1) >= u'\u0000' and self.LA(1) <= u'\u007f')) and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\u007f')) and (True) and (True) and (True) and (True) and (True):
                                pass
                                self.matchNot(antlr.EOF_CHAR)
                                atLeft = False
                            else:
                                self.raise_NoViableAlt(self.LA(1))
                            
                        else:
                            break
                        
                        _cnt33 += 1
                    if _cnt33 < 1:
                        self.raise_NoViableAlt(self.LA(1))
                    if (self.LA(1)==u'\n' or self.LA(1)==u'\r') and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\u007f')) and (True) and (True) and (True) and (True) and (True):
                        pass
                        la1 = self.LA(1)
                        if False:
                            pass
                        elif la1 and la1 in u'\r':
                            pass
                            _saveIndex = self.text.length()
                            self.match('\r')
                            self.text.setLength(_saveIndex)
                        elif la1 and la1 in u'\n':
                            pass
                        else:
                                self.raise_NoViableAlt(self.LA(1))
                            
                        _saveIndex = self.text.length()
                        self.match('\n')
                        self.text.setLength(_saveIndex)
                        self.newline()()
                        atLeft = True
                    elif ((self.LA(1) >= u'\u0000' and self.LA(1) <= u'\u007f')) and (True) and (True) and (True) and (True) and (True) and (True):
                        pass
                    else:
                        self.raise_NoViableAlt(self.LA(1))
                    
                    if (self.LA(1)==u'$') and (self.LA(2)==u'@'):
                        pass
                        _saveIndex = self.text.length()
                        self.match("$@end$")
                        self.text.setLength(_saveIndex)
                    elif ((self.LA(1) >= u'\u0000' and self.LA(1) <= u'\u007f')) and (True):
                        pass
                        self.matchNot(antlr.EOF_CHAR)
                        self.this.error("missing region "+t+" $@end$ tag")
                    else:
                        self.raise_NoViableAlt(self.LA(1))
                    
                    if ((self.LA(1)==u'\n' or self.LA(1)==u'\r') and (atLeft)):
                        pass
                        la1 = self.LA(1)
                        if False:
                            pass
                        elif la1 and la1 in u'\r':
                            pass
                            _saveIndex = self.text.length()
                            self.match('\r')
                            self.text.setLength(_saveIndex)
                        elif la1 and la1 in u'\n':
                            pass
                        else:
                                self.raise_NoViableAlt(self.LA(1))
                            
                        _saveIndex = self.text.length()
                        self.match('\n')
                        self.text.setLength(_saveIndex)
                        self.newline()
                    else: ## <m4>
                            pass
                        
                else:
                        self.raise_NoViableAlt(self.LA(1))
                    
            elif (self.LA(1)==u'$') and (_tokenSet_1.member(self.LA(2))) and ((self.LA(3) >= u'\u0000' and self.LA(3) <= u'\u007f')) and (True) and (True) and (True) and (True):
                pass
                _saveIndex = self.text.length()
                self.match('$')
                self.text.setLength(_saveIndex)
                self.mEXPR(False)
                _saveIndex = self.text.length()
                self.match('$')
                self.text.setLength(_saveIndex)
            else:
                self.raise_NoViableAlt(self.LA(1))
            
            t = ChunkToken(_ttype, self.text.getString(_begin), self.currentIndent)
            _token = t;
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
        self.match("$!")
        while True:
            ###  nongreedy exit test
            if ((self.LA(1)==u'!') and (self.LA(2)==u'$') and (True) and (True) and (True) and (True) and (True)):
                break
            if (self.LA(1)==u'\n' or self.LA(1)==u'\r') and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\u007f')) and ((self.LA(3) >= u'\u0000' and self.LA(3) <= u'\u007f')) and (True) and (True) and (True) and (True):
                pass
                self.mNL(False)
                self.newline()
            elif ((self.LA(1) >= u'\u0000' and self.LA(1) <= u'\u007f')) and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\u007f')) and ((self.LA(3) >= u'\u0000' and self.LA(3) <= u'\u007f')) and (True) and (True) and (True) and (True):
                pass
                self.matchNot(antlr.EOF_CHAR)
            else:
                break
            
        self.match("!$")
        if ((self.LA(1)==u'\n' or self.LA(1)==u'\r') and ( startCol == 1 )):
            pass
            self.mNL(False)
            self.newline()
        else: ## <m4>
                pass
            
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mIF_EXPR(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = IF_EXPR
        _saveIndex = 0
        pass
        _cnt58= 0
        while True:
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in u'\\':
                pass
                self.mESC(False)
            elif la1 and la1 in u'\n\r':
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
            elif la1 and la1 in u'{':
                pass
                self.mSUBTEMPLATE(False)
            elif la1 and la1 in u'(':
                pass
                self.mNESTED_PARENS(False)
            elif la1 and la1 in u'\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\u0008\t\u000b\u000c\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !"#$%&\'*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz|}~\u007f':
                pass
                self.matchNot(')')
            else:
                    break
                
            _cnt58 += 1
        if _cnt58 < 1:
            self.raise_NoViableAlt(self.LA(1))
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mEXPR(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = EXPR
        _saveIndex = 0
        pass
        _cnt46= 0
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
            elif la1 and la1 in u'\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\u0008\t\u000b\u000c\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !"#%&\'()*,-./0123456789:;<>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz|}~\u007f':
                pass
                self.matchNot('$')
            else:
                if (self.LA(1)==u'+' or self.LA(1)==u'=') and (self.LA(2)==u'"' or self.LA(2)==u'<'):
                    pass
                    la1 = self.LA(1)
                    if False:
                        pass
                    elif la1 and la1 in u'=':
                        pass
                        self.match('=')
                    elif la1 and la1 in u'+':
                        pass
                        self.match('+')
                    else:
                            self.raise_NoViableAlt(self.LA(1))
                        
                    self.mTEMPLATE(False)
                elif (self.LA(1)==u'+' or self.LA(1)==u'=') and (self.LA(2)==u'{'):
                    pass
                    la1 = self.LA(1)
                    if False:
                        pass
                    elif la1 and la1 in u'=':
                        pass
                        self.match('=')
                    elif la1 and la1 in u'+':
                        pass
                        self.match('+')
                    else:
                            self.raise_NoViableAlt(self.LA(1))
                        
                    self.mSUBTEMPLATE(False)
                elif (self.LA(1)==u'+' or self.LA(1)==u'=') and (_tokenSet_4.member(self.LA(2))):
                    pass
                    la1 = self.LA(1)
                    if False:
                        pass
                    elif la1 and la1 in u'=':
                        pass
                        self.match('=')
                    elif la1 and la1 in u'+':
                        pass
                        self.match('+')
                    else:
                            self.raise_NoViableAlt(self.LA(1))
                        
                    self.match(_tokenSet_4)
                else:
                    break
                
            _cnt46 += 1
        if _cnt46 < 1:
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
        self.matchNot(antlr.EOF_CHAR)
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mSUBTEMPLATE(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = SUBTEMPLATE
        _saveIndex = 0
        pass
        self.match('{')
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
            elif la1 and la1 in u'\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\u0008\t\n\u000b\u000c\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz|~\u007f':
                pass
                self.matchNot('}')
            else:
                    break
                
        self.match('}')
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mTEMPLATE(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = TEMPLATE
        _saveIndex = 0
        la1 = self.LA(1)
        if False:
            pass
        elif la1 and la1 in u'"':
            pass
            self.match('"')
            while True:
                la1 = self.LA(1)
                if False:
                    pass
                elif la1 and la1 in u'\\':
                    pass
                    self.mESC(False)
                elif la1 and la1 in u'\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\u0008\t\n\u000b\u000c\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz{|}~\u007f':
                    pass
                    self.matchNot('"')
                else:
                        break
                    
            self.match('"')
        elif la1 and la1 in u'<':
            pass
            self.match("<<")
            if (self.LA(1)==u'\n' or self.LA(1)==u'\r') and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\u007f')) and ((self.LA(3) >= u'\u0000' and self.LA(3) <= u'\u007f')) and ((self.LA(4) >= u'\u0000' and self.LA(4) <= u'\u007f')) and (True) and (True) and (True):
                pass
                la1 = self.LA(1)
                if False:
                    pass
                elif la1 and la1 in u'\r':
                    pass
                    _saveIndex = self.text.length()
                    self.match('\r')
                    self.text.setLength(_saveIndex)
                elif la1 and la1 in u'\n':
                    pass
                else:
                        self.raise_NoViableAlt(self.LA(1))
                    
                _saveIndex = self.text.length()
                self.match('\n')
                self.text.setLength(_saveIndex)
                self.newline()
            elif ((self.LA(1) >= u'\u0000' and self.LA(1) <= u'\u007f')) and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\u007f')) and ((self.LA(3) >= u'\u0000' and self.LA(3) <= u'\u007f')) and (True) and (True) and (True) and (True):
                pass
            else:
                self.raise_NoViableAlt(self.LA(1))
            
            while True:
                ###  nongreedy exit test
                if ((self.LA(1)==u'>') and (self.LA(2)==u'>') and ((self.LA(3) >= u'\u0000' and self.LA(3) <= u'\u007f')) and (True) and (True) and (True) and (True)):
                    break
                if ((self.LA(1)==u'\r') and (self.LA(2)==u'\n') and ((self.LA(3) >= u'\u0000' and self.LA(3) <= u'\u007f')) and ((self.LA(4) >= u'\u0000' and self.LA(4) <= u'\u007f')) and ((self.LA(5) >= u'\u0000' and self.LA(5) <= u'\u007f')) and (True) and (True) and ( self.LA(3) == '>' and self.LA(4) == '>' )):
                    pass
                    _saveIndex = self.text.length()
                    self.match('\r')
                    self.text.setLength(_saveIndex)
                    _saveIndex = self.text.length()
                    self.match('\n')
                    self.text.setLength(_saveIndex)
                    self.newline()
                elif ((self.LA(1)==u'\n') and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\u007f')) and ((self.LA(3) >= u'\u0000' and self.LA(3) <= u'\u007f')) and ((self.LA(4) >= u'\u0000' and self.LA(4) <= u'\u007f')) and (True) and (True) and (True) and ( self.LA(2) == '>' and self.LA(3) == '>' )):
                    pass
                    _saveIndex = self.text.length()
                    self.match('\n')
                    self.text.setLength(_saveIndex)
                    self.newline()
                elif (self.LA(1)==u'\n' or self.LA(1)==u'\r') and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\u007f')) and ((self.LA(3) >= u'\u0000' and self.LA(3) <= u'\u007f')) and ((self.LA(4) >= u'\u0000' and self.LA(4) <= u'\u007f')) and (True) and (True) and (True):
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
                elif ((self.LA(1) >= u'\u0000' and self.LA(1) <= u'\u007f')) and ((self.LA(2) >= u'\u0000' and self.LA(2) <= u'\u007f')) and ((self.LA(3) >= u'\u0000' and self.LA(3) <= u'\u007f')) and ((self.LA(4) >= u'\u0000' and self.LA(4) <= u'\u007f')) and (True) and (True) and (True):
                    pass
                    self.matchNot(antlr.EOF_CHAR)
                else:
                    break
                
            self.match(">>")
        else:
                self.raise_NoViableAlt(self.LA(1))
            
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    def mNESTED_PARENS(self, _createToken):    
        _ttype = 0
        _token = None
        _begin = self.text.length()
        _ttype = NESTED_PARENS
        _saveIndex = 0
        pass
        self.match('(')
        _cnt65= 0
        while True:
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in u'(':
                pass
                self.mNESTED_PARENS(False)
            elif la1 and la1 in u'\\':
                pass
                self.mESC(False)
            elif la1 and la1 in u'\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\u0008\t\n\u000b\u000c\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !"#$%&\'*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[]^_`abcdefghijklmnopqrstuvwxyz{|}~\u007f':
                pass
                self.matchNot(')')
            else:
                    break
                
            _cnt65 += 1
        if _cnt65 < 1:
            self.raise_NoViableAlt(self.LA(1))
        self.match(')')
        self.set_return_token(_createToken, _token, _ttype, _begin)
    
    

### generate bit set
def mk_tokenSet_0(): 
    ### var1
    data = [ -68719485953L, -1L, 0L, 0L]
    return data
_tokenSet_0 = antlr.BitSet(mk_tokenSet_0())

### generate bit set
def mk_tokenSet_1(): 
    ### var1
    data = [ -68719476737L, -1L, 0L, 0L]
    return data
_tokenSet_1 = antlr.BitSet(mk_tokenSet_1())

### generate bit set
def mk_tokenSet_2(): 
    ### var1
    data = [ -2199023255553L, -1L, 0L, 0L]
    return data
_tokenSet_2 = antlr.BitSet(mk_tokenSet_2())

### generate bit set
def mk_tokenSet_3(): 
    ### var1
    data = [ -1168231104513L, -1L, 0L, 0L]
    return data
_tokenSet_3 = antlr.BitSet(mk_tokenSet_3())

### generate bit set
def mk_tokenSet_4(): 
    ### var1
    data = [ -1152921521786716161L, -576460752303423489L, 0L, 0L]
    return data
_tokenSet_4 = antlr.BitSet(mk_tokenSet_4())
    
### __main__ header action >>> 
if __name__ == '__main__' :
    import sys
    import antlr
    import DefaultTemplateLexer
    
    ### create lexer - shall read from stdin
    try:
        for token in DefaultTemplateLexer.Lexer():
            print token
            
    except antlr.TokenStreamException, e:
        print "error: exception caught while lexing: ", e
### __main__ header action <<< 
