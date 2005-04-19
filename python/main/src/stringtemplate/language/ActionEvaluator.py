### $ANTLR 2.7.5 (20050419): "eval.g" -> "ActionEvaluator.py"$
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

from cStringIO import StringIO

class NameValuePair(object):

   def __init__(self):
       self.name = None
       self.value = None
### header action <<< 

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
NOT = 15
PLUS = 16
COLON = 17
COMMA = 18
ID = 19
LITERAL_super = 20
DOT = 21
ANONYMOUS_TEMPLATE = 22
STRING = 23
INT = 24
NESTED_ANONYMOUS_TEMPLATE = 25
ESC_CHAR = 26
WS = 27

### user code>>>

### user code<<<

class Walker(antlr.TreeParser):

# ctor ..
def __init__(self, *args, **kwargs):
    antlr.TreeParser.__init__(self, *args, **kwargs)
    self.tokenNames = _tokenNames
    ### __init__ header action >>> 
    self.this = None
    self.out = None
    self.chunk = None
    ### __init__ header action <<< 

### user action >>>
def initialize(self, this, chunk, out):
   self.this = this
   self.chunk = chunk
   self.out = out

def reportError(self, e):
   self.this.error("template parse error", e)
### user action <<<
    def action(self, _t):    
        numCharsWritten = 0
        
        action_AST_in = None
        if _t != antlr.ASTNULL:
            action_AST_in = _t
        e = None
        try:      ## for error handling
            pass
            e=self.expr(_t)
            _t = self._retTree
            numCharsWritten = self.chunk.writeAttribute(self.this,e,self.out)
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            if _t:
                _t = _t.getNextSibling()
        
        self._retTree = _t
        return numCharsWritten
    
    def expr(self, _t):    
        value = None
        
        expr_AST_in = None
        if _t != antlr.ASTNULL:
            expr_AST_in = _t
        a = None
        b = None
        e = None
        try:      ## for error handling
            if not _t:
                _t = antlr.ASTNULL
            la1 = _t.getType()
            if False:
                pass
            elif la1 and la1 in [PLUS]:
                pass
                _t3 = _t
                tmp1_AST_in = _t
                self.match(_t,PLUS)
                _t = _t.getFirstChild()
                a=self.expr(_t)
                _t = self._retTree
                b=self.expr(_t)
                _t = self._retTree
                value = self.chunk.add(a,b)
                _t = _t3
                _t = _t.getNextSibling()
            elif la1 and la1 in [APPLY]:
                pass
                value=self.templateApplication(_t)
                _t = self._retTree
            elif la1 and la1 in [ID,DOT,STRING,INT]:
                pass
                value=self.attribute(_t)
                _t = self._retTree
            elif la1 and la1 in [INCLUDE]:
                pass
                value=self.templateInclude(_t)
                _t = self._retTree
            elif la1 and la1 in [VALUE]:
                pass
                _t4 = _t
                tmp2_AST_in = _t
                self.match(_t,VALUE)
                _t = _t.getFirstChild()
                e=self.expr(_t)
                _t = self._retTree
                _t = _t4
                _t = _t.getNextSibling()
                buf = StringIO()
                writerClass = self.out.__class__
                sw = None
                try:
                   sw = writerClass(buf)
                except Exception, exc:
                   # default stringtemplate.AutoIndentWriter
                   self.this.error("eval: cannot make implementation of " +
                                   "StringTemplateWriter", exc)
                   sw = stringtemplate.AutoIndentWriter(buf)
                self.chunk.writeAttribute(self.this, e, sw)
                value = buf.getvalue()
                buf.close()
            else:
                    raise antlr.NoViableAltException(_t)
                
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            if _t:
                _t = _t.getNextSibling()
        
        self._retTree = _t
        return value
    

    ###/** Apply template(s) to an attribute; can be applied to another apply
    ### *  result.
    ### */
    def templateApplication(self, _t):    
        value = None
        
        templateApplication_AST_in = None
        if _t != antlr.ASTNULL:
            templateApplication_AST_in = _t
        a = None
        templatesToApply = []
        try:      ## for error handling
            pass
            _t10 = _t
            tmp3_AST_in = _t
            self.match(_t,APPLY)
            _t = _t.getFirstChild()
            a=self.expr(_t)
            _t = self._retTree
            _cnt12= 0
            while True:
                if not _t:
                    _t = antlr.ASTNULL
                if (_t.getType()==TEMPLATE):
                    pass
                    self.template(_t, templatesToApply)
                    _t = self._retTree
                else:
                    break
                
                _cnt12 += 1
            if _cnt12 < 1:
                raise antlr.NoViableAltException(_t)
            value = self.chunk.applyListOfAlternatingTemplates(self.this,a,templatesToApply)
            _t = _t10
            _t = _t.getNextSibling()
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            if _t:
                _t = _t.getNextSibling()
        
        self._retTree = _t
        return value
    
    def attribute(self, _t):    
        value = None
        
        attribute_AST_in = None
        if _t != antlr.ASTNULL:
            attribute_AST_in = _t
        prop = None
        i3 = None
        i = None
        s = None
        obj = None
        try:      ## for error handling
            if not _t:
                _t = antlr.ASTNULL
            la1 = _t.getType()
            if False:
                pass
            elif la1 and la1 in [DOT]:
                pass
                _t21 = _t
                tmp4_AST_in = _t
                self.match(_t,DOT)
                _t = _t.getFirstChild()
                obj=self.attribute(_t)
                _t = self._retTree
                prop = _t
                self.match(_t,ID)
                _t = _t.getNextSibling()
                _t = _t21
                _t = _t.getNextSibling()
                value = self.chunk.getObjectProperty(self.this,obj,prop.getText())
            elif la1 and la1 in [ID]:
                pass
                i3 = _t
                self.match(_t,ID)
                _t = _t.getNextSibling()
                try:
                   value=self.this.getAttribute(i3.getText())
                except KeyError, nse:
                   # rethrow with more precise error message
                   raise KeyError(str(nse) + " in template " +
                                  self.this.getName())
            elif la1 and la1 in [INT]:
                pass
                i = _t
                self.match(_t,INT)
                _t = _t.getNextSibling()
                value = int(i.getText())
            elif la1 and la1 in [STRING]:
                pass
                s = _t
                self.match(_t,STRING)
                _t = _t.getNextSibling()
                value = s.getText()
            else:
                    raise antlr.NoViableAltException(_t)
                
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            if _t:
                _t = _t.getNextSibling()
        
        self._retTree = _t
        return value
    
    def templateInclude(self, _t):    
        value = None
        
        templateInclude_AST_in = None
        if _t != antlr.ASTNULL:
            templateInclude_AST_in = _t
        id = None
        a1 = None
        a2 = None
        args = None
        name = ""
        n = None
        try:      ## for error handling
            pass
            _t6 = _t
            tmp5_AST_in = _t
            self.match(_t,INCLUDE)
            _t = _t.getFirstChild()
            if not _t:
                _t = antlr.ASTNULL
            la1 = _t.getType()
            if False:
                pass
            elif la1 and la1 in [ID]:
                pass
                id = _t
                self.match(_t,ID)
                _t = _t.getNextSibling()
                a1 = _t
                if not _t:
                    raise MismatchedTokenException()
                _t = _t.getNextSibling()
                name=id.getText(); args=a1
            elif la1 and la1 in [VALUE]:
                pass
                _t8 = _t
                tmp6_AST_in = _t
                self.match(_t,VALUE)
                _t = _t.getFirstChild()
                n=self.expr(_t)
                _t = self._retTree
                a2 = _t
                if not _t:
                    raise MismatchedTokenException()
                _t = _t.getNextSibling()
                _t = _t8
                _t = _t.getNextSibling()
                if n: name = str(n); args=a2
            else:
                    raise antlr.NoViableAltException(_t)
                
            _t = _t6
            _t = _t.getNextSibling()
            if name:
               value = self.chunk.getTemplateInclude(self.this, name, args)
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            if _t:
                _t = _t.getNextSibling()
        
        self._retTree = _t
        return value
    
    def template(self, _t,
        templatesToApply
    ):    
        
        template_AST_in = None
        if _t != antlr.ASTNULL:
            template_AST_in = _t
        t = None
        args = None
        anon = None
        argumentContext = {}
        n = None
        try:      ## for error handling
            pass
            _t14 = _t
            tmp7_AST_in = _t
            self.match(_t,TEMPLATE)
            _t = _t.getFirstChild()
            if not _t:
                _t = antlr.ASTNULL
            la1 = _t.getType()
            if False:
                pass
            elif la1 and la1 in [ID]:
                pass
                t = _t
                self.match(_t,ID)
                _t = _t.getNextSibling()
                args = _t
                if not _t:
                    raise MismatchedTokenException()
                _t = _t.getNextSibling()
                templateName = t.getText()
                group = self.this.getGroup()
                embedded = group.getEmbeddedInstanceOf(self.this, templateName)
                if embedded:
                   embedded.setArgumentsAST(args)
                   templatesToApply.append(embedded)
            elif la1 and la1 in [ANONYMOUS_TEMPLATE]:
                pass
                anon = _t
                self.match(_t,ANONYMOUS_TEMPLATE)
                _t = _t.getNextSibling()
                anonymous = anon.getStringTemplate()
                templatesToApply.append(anonymous)
            elif la1 and la1 in [VALUE]:
                pass
                _t16 = _t
                tmp8_AST_in = _t
                self.match(_t,VALUE)
                _t = _t.getFirstChild()
                n=self.expr(_t)
                _t = self._retTree
                argumentContext=self.argList(_t, None)
                _t = self._retTree
                _t = _t16
                _t = _t.getNextSibling()
                if n:
                   templateName = str(n)
                   group = self.this.getGroup()
                   embedded = group.getEmbeddedInstanceOf(self.this, templateName)
                   if embedded:
                       embedded.setArgumentsAST(args)
                       templatesToApply.append(embedded)
            else:
                    raise antlr.NoViableAltException(_t)
                
            _t = _t14
            _t = _t.getNextSibling()
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            if _t:
                _t = _t.getNextSibling()
        
        self._retTree = _t
    
    def argList(self, _t,
        initialContext
    ):    
        argumentContext = None
        
        argList_AST_in = None
        if _t != antlr.ASTNULL:
            argList_AST_in = _t
        argumentContext = initialContext
        if not argumentContext:
           argumentContext = {}
        try:      ## for error handling
            pass
            _t23 = _t
            tmp9_AST_in = _t
            self.match(_t,ARGS)
            _t = _t.getFirstChild()
            while True:
                if not _t:
                    _t = antlr.ASTNULL
                if (_t.getType()==ASSIGN):
                    pass
                    self.argumentAssignment(_t, argumentContext)
                    _t = self._retTree
                else:
                    break
                
            _t = _t23
            _t = _t.getNextSibling()
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            if _t:
                _t = _t.getNextSibling()
        
        self._retTree = _t
        return argumentContext
    
    def ifCondition(self, _t):    
        value = False
        
        ifCondition_AST_in = None
        if _t != antlr.ASTNULL:
            ifCondition_AST_in = _t
        a = None
        try:      ## for error handling
            if not _t:
                _t = antlr.ASTNULL
            la1 = _t.getType()
            if False:
                pass
            elif la1 and la1 in [APPLY,INCLUDE,VALUE,PLUS,ID,DOT,STRING,INT]:
                pass
                a=self.ifAtom(_t)
                _t = self._retTree
                value = self.chunk.testAttributeTrue(a)
            elif la1 and la1 in [NOT]:
                pass
                _t18 = _t
                tmp10_AST_in = _t
                self.match(_t,NOT)
                _t = _t.getFirstChild()
                a=self.ifAtom(_t)
                _t = self._retTree
                _t = _t18
                _t = _t.getNextSibling()
                value = not self.chunk.testAttributeTrue(a)
            else:
                    raise antlr.NoViableAltException(_t)
                
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            if _t:
                _t = _t.getNextSibling()
        
        self._retTree = _t
        return value
    
    def ifAtom(self, _t):    
        value = None
        
        ifAtom_AST_in = None
        if _t != antlr.ASTNULL:
            ifAtom_AST_in = _t
        try:      ## for error handling
            pass
            value=self.expr(_t)
            _t = self._retTree
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            if _t:
                _t = _t.getNextSibling()
        
        self._retTree = _t
        return value
    
    def argumentAssignment(self, _t,
        argumentContext
    ):    
        
        argumentAssignment_AST_in = None
        if _t != antlr.ASTNULL:
            argumentAssignment_AST_in = _t
        arg = None
        e = None
        try:      ## for error handling
            pass
            _t27 = _t
            tmp11_AST_in = _t
            self.match(_t,ASSIGN)
            _t = _t.getFirstChild()
            arg = _t
            self.match(_t,ID)
            _t = _t.getNextSibling()
            e=self.expr(_t)
            _t = self._retTree
            if e:
               self.this.rawSetAttribute(argumentContext,arg.getText(), e)
            _t = _t27
            _t = _t.getNextSibling()
        
        except antlr.RecognitionException, ex:
            self.reportError(ex)
            if _t:
                _t = _t.getNextSibling()
        
        self._retTree = _t
    

_tokenNames = [
    "<0>", 
    "EOF", 
    "<2>", 
    "NULL_TREE_LOOKAHEAD", 
    "APPLY", 
    "ARGS", 
    "INCLUDE", 
    "\"if\"", 
    "VALUE", 
    "TEMPLATE", 
    "SEMI", 
    "LPAREN", 
    "RPAREN", 
    "\"separator\"", 
    "ASSIGN", 
    "NOT", 
    "PLUS", 
    "COLON", 
    "COMMA", 
    "ID", 
    "\"super\"", 
    "DOT", 
    "ANONYMOUS_TEMPLATE", 
    "STRING", 
    "INT", 
    "NESTED_ANONYMOUS_TEMPLATE", 
    "ESC_CHAR", 
    "WS"
]
    
