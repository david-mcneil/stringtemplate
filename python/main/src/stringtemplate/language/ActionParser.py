### $ANTLR 2.7.6 (20060527): "action.g" -> "ActionParser.py"$
### import antlr and other modules ..
import sys
import antlr

version = sys.version.split()[0]
if version < '2.2.1':
    False = 0
if version < '2.3':
    True = not False
### header action >>> 
from StringTemplateToken import *
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
SEMI = 14
LPAREN = 15
RPAREN = 16
LITERAL_separator = 17
ASSIGN = 18
COLON = 19
COMMA = 20
NOT = 21
PLUS = 22
DOT = 23
ID = 24
LITERAL_first = 25
LITERAL_rest = 26
LITERAL_last = 27
LITERAL_super = 28
ANONYMOUS_TEMPLATE = 29
STRING = 30
INT = 31
LBRACK = 32
RBRACK = 33
DOTDOTDOT = 34
TEMPLATE_ARGS = 35
NESTED_ANONYMOUS_TEMPLATE = 36
ESC_CHAR = 37
WS = 38
WS_CHAR = 39


###/** Parse the individual attribute expressions */
class Parser(antlr.LLkParser):
    ### user action >>>
    def reportError(self, e):
       self.this.error("template parse error", e)
    ### user action <<<
    
    def __init__(self, *args, **kwargs):
        antlr.LLkParser.__init__(self, *args, **kwargs)
        self.tokenNames = _tokenNames
        self.buildTokenTypeASTClassMap()
        self.astFactory = antlr.ASTFactory(self.getTokenTypeToASTClassMap())
        self.astFactory.setASTNodeClass(stringtemplate.language.StringTemplateAST)
        ### __init__ header action >>> 
        if len(args) > 1 and isinstance(args[1], stringtemplate.StringTemplate):
           self.this = args[1]
        else:
           raise ValueError("ActionParser requires a StringTemplate instance")
        ### __init__ header action <<< 
        
    def action(self):    
        opts = None
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        action_AST = None
        try:      ## for error handling
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in [LPAREN,ID,LITERAL_first,LITERAL_rest,LITERAL_last,LITERAL_super,ANONYMOUS_TEMPLATE,STRING,INT,LBRACK]:
                pass
                self.templatesExpr()
                self.addASTChild(currentAST, self.returnAST)
                la1 = self.LA(1)
                if False:
                    pass
                elif la1 and la1 in [SEMI]:
                    pass
                    self.match(SEMI)
                    opts=self.optionList()
                    self.addASTChild(currentAST, self.returnAST)
                elif la1 and la1 in [EOF]:
                    pass
                else:
                        raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                    
                action_AST = currentAST.root
            elif la1 and la1 in [CONDITIONAL]:
                pass
                tmp2_AST = None
                tmp2_AST = self.astFactory.create(self.LT(1))
                self.makeASTRoot(currentAST, tmp2_AST)
                self.match(CONDITIONAL)
                self.match(LPAREN)
                self.ifCondition()
                self.addASTChild(currentAST, self.returnAST)
                self.match(RPAREN)
                action_AST = currentAST.root
            else:
                    raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_0)
            else:
                raise ex
        
        self.returnAST = action_AST
        return opts
    
    def templatesExpr(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        templatesExpr_AST = None
        c = None
        c_AST = None
        try:      ## for error handling
            synPredMatched6 = False
            if (_tokenSet_1.member(self.LA(1))) and (_tokenSet_2.member(self.LA(2))):
                _m6 = self.mark()
                synPredMatched6 = True
                self.inputState.guessing += 1
                try:
                    pass
                    self.parallelArrayTemplateApplication()
                except antlr.RecognitionException, pe:
                    synPredMatched6 = False
                self.rewind(_m6)
                self.inputState.guessing -= 1
            if synPredMatched6:
                pass
                self.parallelArrayTemplateApplication()
                self.addASTChild(currentAST, self.returnAST)
                templatesExpr_AST = currentAST.root
            elif (_tokenSet_1.member(self.LA(1))) and (_tokenSet_3.member(self.LA(2))):
                pass
                self.expr()
                self.addASTChild(currentAST, self.returnAST)
                while True:
                    if (self.LA(1)==COLON):
                        pass
                        c = self.LT(1)
                        c_AST = self.astFactory.create(c)
                        self.makeASTRoot(currentAST, c_AST)
                        self.match(COLON)
                        if not self.inputState.guessing:
                            c_AST.setType(APPLY)
                        self.template()
                        self.addASTChild(currentAST, self.returnAST)
                        while True:
                            if (self.LA(1)==COMMA):
                                pass
                                self.match(COMMA)
                                self.template()
                                self.addASTChild(currentAST, self.returnAST)
                            else:
                                break
                            
                    else:
                        break
                    
                templatesExpr_AST = currentAST.root
            else:
                raise antlr.NoViableAltException(self.LT(1), self.getFilename())
            
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_4)
            else:
                raise ex
        
        self.returnAST = templatesExpr_AST
    
    def optionList(self):    
        opts = {}
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        optionList_AST = None
        e_AST = None
        try:      ## for error handling
            pass
            self.match(LITERAL_separator)
            tmp7_AST = None
            tmp7_AST = self.astFactory.create(self.LT(1))
            self.match(ASSIGN)
            self.expr()
            e_AST = self.returnAST
            if not self.inputState.guessing:
                opts["separator"] = e_AST
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_0)
            else:
                raise ex
        
        self.returnAST = optionList_AST
        return opts
    
    def ifCondition(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        ifCondition_AST = None
        try:      ## for error handling
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in [LPAREN,ID,LITERAL_first,LITERAL_rest,LITERAL_last,LITERAL_super,ANONYMOUS_TEMPLATE,STRING,INT,LBRACK]:
                pass
                self.ifAtom()
                self.addASTChild(currentAST, self.returnAST)
                ifCondition_AST = currentAST.root
            elif la1 and la1 in [NOT]:
                pass
                tmp8_AST = None
                tmp8_AST = self.astFactory.create(self.LT(1))
                self.makeASTRoot(currentAST, tmp8_AST)
                self.match(NOT)
                self.ifAtom()
                self.addASTChild(currentAST, self.returnAST)
                ifCondition_AST = currentAST.root
            else:
                    raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_5)
            else:
                raise ex
        
        self.returnAST = ifCondition_AST
    
    def expr(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        expr_AST = None
        try:      ## for error handling
            pass
            self.primaryExpr()
            self.addASTChild(currentAST, self.returnAST)
            while True:
                if (self.LA(1)==PLUS):
                    pass
                    tmp9_AST = None
                    tmp9_AST = self.astFactory.create(self.LT(1))
                    self.makeASTRoot(currentAST, tmp9_AST)
                    self.match(PLUS)
                    self.primaryExpr()
                    self.addASTChild(currentAST, self.returnAST)
                else:
                    break
                
            expr_AST = currentAST.root
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_6)
            else:
                raise ex
        
        self.returnAST = expr_AST
    
    def parallelArrayTemplateApplication(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        parallelArrayTemplateApplication_AST = None
        c = None
        c_AST = None
        try:      ## for error handling
            pass
            self.expr()
            self.addASTChild(currentAST, self.returnAST)
            _cnt13= 0
            while True:
                if (self.LA(1)==COMMA):
                    pass
                    self.match(COMMA)
                    self.expr()
                    self.addASTChild(currentAST, self.returnAST)
                else:
                    break
                
                _cnt13 += 1
            if _cnt13 < 1:
                raise antlr.NoViableAltException(self.LT(1), self.getFilename())
            c = self.LT(1)
            c_AST = self.astFactory.create(c)
            self.addASTChild(currentAST, c_AST)
            self.match(COLON)
            self.anonymousTemplate()
            self.addASTChild(currentAST, self.returnAST)
            if not self.inputState.guessing:
                parallelArrayTemplateApplication_AST = currentAST.root
                parallelArrayTemplateApplication_AST = \
                   antlr.make(self.astFactory.create(MULTI_APPLY,"MULTI_APPLY"), parallelArrayTemplateApplication_AST)
                currentAST.root = parallelArrayTemplateApplication_AST
                if (parallelArrayTemplateApplication_AST != None) and (parallelArrayTemplateApplication_AST.getFirstChild() != None):
                    currentAST.child = parallelArrayTemplateApplication_AST.getFirstChild()
                else:
                    currentAST.child = parallelArrayTemplateApplication_AST
                currentAST.advanceChildToEnd()
            parallelArrayTemplateApplication_AST = currentAST.root
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_4)
            else:
                raise ex
        
        self.returnAST = parallelArrayTemplateApplication_AST
    
    def template(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        template_AST = None
        try:      ## for error handling
            pass
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in [LPAREN,ID,LITERAL_super]:
                pass
                self.namedTemplate()
                self.addASTChild(currentAST, self.returnAST)
            elif la1 and la1 in [ANONYMOUS_TEMPLATE]:
                pass
                self.anonymousTemplate()
                self.addASTChild(currentAST, self.returnAST)
            else:
                    raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                
            if not self.inputState.guessing:
                template_AST = currentAST.root
                template_AST = antlr.make(self.astFactory.create(TEMPLATE), template_AST)
                currentAST.root = template_AST
                if (template_AST != None) and (template_AST.getFirstChild() != None):
                    currentAST.child = template_AST.getFirstChild()
                else:
                    currentAST.child = template_AST
                currentAST.advanceChildToEnd()
            template_AST = currentAST.root
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_7)
            else:
                raise ex
        
        self.returnAST = template_AST
    
    def anonymousTemplate(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        anonymousTemplate_AST = None
        t = None
        t_AST = None
        try:      ## for error handling
            pass
            t = self.LT(1)
            t_AST = self.astFactory.create(t)
            self.addASTChild(currentAST, t_AST)
            self.match(ANONYMOUS_TEMPLATE)
            if not self.inputState.guessing:
                anonymous = stringtemplate.StringTemplate()
                anonymous.setGroup(self.this.getGroup())
                anonymous.setEnclosingInstance(self.this)
                anonymous.setTemplate(t.getText())
                anonymous.defineFormalArgument(t.args)
                t_AST.setStringTemplate(anonymous)
            anonymousTemplate_AST = currentAST.root
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_7)
            else:
                raise ex
        
        self.returnAST = anonymousTemplate_AST
    
    def ifAtom(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        ifAtom_AST = None
        try:      ## for error handling
            pass
            self.expr()
            self.addASTChild(currentAST, self.returnAST)
            ifAtom_AST = currentAST.root
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_5)
            else:
                raise ex
        
        self.returnAST = ifAtom_AST
    
    def primaryExpr(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        primaryExpr_AST = None
        try:      ## for error handling
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in [LITERAL_first,LITERAL_rest,LITERAL_last]:
                pass
                self.function()
                self.addASTChild(currentAST, self.returnAST)
                primaryExpr_AST = currentAST.root
            elif la1 and la1 in [LBRACK]:
                pass
                self.list_()
                self.addASTChild(currentAST, self.returnAST)
                primaryExpr_AST = currentAST.root
            else:
                if (_tokenSet_8.member(self.LA(1))) and (_tokenSet_9.member(self.LA(2))):
                    pass
                    self.atom()
                    self.addASTChild(currentAST, self.returnAST)
                    while True:
                        if (self.LA(1)==DOT):
                            pass
                            tmp11_AST = None
                            tmp11_AST = self.astFactory.create(self.LT(1))
                            self.makeASTRoot(currentAST, tmp11_AST)
                            self.match(DOT)
                            la1 = self.LA(1)
                            if False:
                                pass
                            elif la1 and la1 in [ID]:
                                pass
                                tmp12_AST = None
                                tmp12_AST = self.astFactory.create(self.LT(1))
                                self.addASTChild(currentAST, tmp12_AST)
                                self.match(ID)
                            elif la1 and la1 in [LPAREN]:
                                pass
                                self.valueExpr()
                                self.addASTChild(currentAST, self.returnAST)
                            else:
                                    raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                                
                        else:
                            break
                        
                    primaryExpr_AST = currentAST.root
                else:
                    synPredMatched24 = False
                    if (self.LA(1)==LPAREN or self.LA(1)==ID or self.LA(1)==LITERAL_super) and (_tokenSet_10.member(self.LA(2))):
                        _m24 = self.mark()
                        synPredMatched24 = True
                        self.inputState.guessing += 1
                        try:
                            pass
                            self.templateInclude()
                        except antlr.RecognitionException, pe:
                            synPredMatched24 = False
                        self.rewind(_m24)
                        self.inputState.guessing -= 1
                    if synPredMatched24:
                        pass
                        self.templateInclude()
                        self.addASTChild(currentAST, self.returnAST)
                        primaryExpr_AST = currentAST.root
                    elif (self.LA(1)==LPAREN) and (_tokenSet_1.member(self.LA(2))):
                        pass
                        self.valueExpr()
                        self.addASTChild(currentAST, self.returnAST)
                        primaryExpr_AST = currentAST.root
                    else:
                        raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                    
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_11)
            else:
                raise ex
        
        self.returnAST = primaryExpr_AST
    
    def atom(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        atom_AST = None
        try:      ## for error handling
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in [ID]:
                pass
                tmp13_AST = None
                tmp13_AST = self.astFactory.create(self.LT(1))
                self.addASTChild(currentAST, tmp13_AST)
                self.match(ID)
                atom_AST = currentAST.root
            elif la1 and la1 in [STRING]:
                pass
                tmp14_AST = None
                tmp14_AST = self.astFactory.create(self.LT(1))
                self.addASTChild(currentAST, tmp14_AST)
                self.match(STRING)
                atom_AST = currentAST.root
            elif la1 and la1 in [INT]:
                pass
                tmp15_AST = None
                tmp15_AST = self.astFactory.create(self.LT(1))
                self.addASTChild(currentAST, tmp15_AST)
                self.match(INT)
                atom_AST = currentAST.root
            elif la1 and la1 in [ANONYMOUS_TEMPLATE]:
                pass
                tmp16_AST = None
                tmp16_AST = self.astFactory.create(self.LT(1))
                self.addASTChild(currentAST, tmp16_AST)
                self.match(ANONYMOUS_TEMPLATE)
                atom_AST = currentAST.root
            else:
                    raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_9)
            else:
                raise ex
        
        self.returnAST = atom_AST
    
    def valueExpr(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        valueExpr_AST = None
        eval = None
        eval_AST = None
        try:      ## for error handling
            pass
            eval = self.LT(1)
            eval_AST = self.astFactory.create(eval)
            self.makeASTRoot(currentAST, eval_AST)
            self.match(LPAREN)
            self.templatesExpr()
            self.addASTChild(currentAST, self.returnAST)
            self.match(RPAREN)
            if not self.inputState.guessing:
                eval_AST.setType(VALUE)
                eval_AST.setText("value")
            valueExpr_AST = currentAST.root
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_9)
            else:
                raise ex
        
        self.returnAST = valueExpr_AST
    
    def templateInclude(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        templateInclude_AST = None
        qid = None
        qid_AST = None
        try:      ## for error handling
            pass
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in [ID]:
                pass
                tmp18_AST = None
                tmp18_AST = self.astFactory.create(self.LT(1))
                self.addASTChild(currentAST, tmp18_AST)
                self.match(ID)
                self.argList()
                self.addASTChild(currentAST, self.returnAST)
            elif la1 and la1 in [LITERAL_super]:
                pass
                self.match(LITERAL_super)
                self.match(DOT)
                qid = self.LT(1)
                qid_AST = self.astFactory.create(qid)
                self.addASTChild(currentAST, qid_AST)
                self.match(ID)
                if not self.inputState.guessing:
                    qid_AST.setText("super."+qid_AST.getText())
                self.argList()
                self.addASTChild(currentAST, self.returnAST)
            elif la1 and la1 in [LPAREN]:
                pass
                self.indirectTemplate()
                self.addASTChild(currentAST, self.returnAST)
            else:
                    raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                
            if not self.inputState.guessing:
                templateInclude_AST = currentAST.root
                templateInclude_AST = antlr.make(self.astFactory.create(INCLUDE,"include"), templateInclude_AST)
                currentAST.root = templateInclude_AST
                if (templateInclude_AST != None) and (templateInclude_AST.getFirstChild() != None):
                    currentAST.child = templateInclude_AST.getFirstChild()
                else:
                    currentAST.child = templateInclude_AST
                currentAST.advanceChildToEnd()
            templateInclude_AST = currentAST.root
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_11)
            else:
                raise ex
        
        self.returnAST = templateInclude_AST
    
    def function(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        function_AST = None
        try:      ## for error handling
            pass
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in [LITERAL_first]:
                pass
                tmp21_AST = None
                tmp21_AST = self.astFactory.create(self.LT(1))
                self.addASTChild(currentAST, tmp21_AST)
                self.match(LITERAL_first)
            elif la1 and la1 in [LITERAL_rest]:
                pass
                tmp22_AST = None
                tmp22_AST = self.astFactory.create(self.LT(1))
                self.addASTChild(currentAST, tmp22_AST)
                self.match(LITERAL_rest)
            elif la1 and la1 in [LITERAL_last]:
                pass
                tmp23_AST = None
                tmp23_AST = self.astFactory.create(self.LT(1))
                self.addASTChild(currentAST, tmp23_AST)
                self.match(LITERAL_last)
            else:
                    raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                
            self.singleArg()
            self.addASTChild(currentAST, self.returnAST)
            if not self.inputState.guessing:
                function_AST = currentAST.root
                function_AST = antlr.make(self.astFactory.create(FUNCTION), function_AST)
                currentAST.root = function_AST
                if (function_AST != None) and (function_AST.getFirstChild() != None):
                    currentAST.child = function_AST.getFirstChild()
                else:
                    currentAST.child = function_AST
                currentAST.advanceChildToEnd()
            function_AST = currentAST.root
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_11)
            else:
                raise ex
        
        self.returnAST = function_AST
    
    def list_(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        list__AST = None
        lb = None
        lb_AST = None
        try:      ## for error handling
            pass
            lb = self.LT(1)
            lb_AST = self.astFactory.create(lb)
            self.makeASTRoot(currentAST, lb_AST)
            self.match(LBRACK)
            if not self.inputState.guessing:
                lb_AST.setType(LIST)
                lb_AST.setText("value")
            self.expr()
            self.addASTChild(currentAST, self.returnAST)
            while True:
                if (self.LA(1)==COMMA):
                    pass
                    self.match(COMMA)
                    self.expr()
                    self.addASTChild(currentAST, self.returnAST)
                else:
                    break
                
            self.match(RBRACK)
            list__AST = currentAST.root
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_11)
            else:
                raise ex
        
        self.returnAST = list__AST
    
    def nonAlternatingTemplateExpr(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        nonAlternatingTemplateExpr_AST = None
        c = None
        c_AST = None
        try:      ## for error handling
            pass
            self.expr()
            self.addASTChild(currentAST, self.returnAST)
            while True:
                if (self.LA(1)==COLON):
                    pass
                    c = self.LT(1)
                    c_AST = self.astFactory.create(c)
                    self.makeASTRoot(currentAST, c_AST)
                    self.match(COLON)
                    if not self.inputState.guessing:
                        c_AST.setType(APPLY)
                    self.template()
                    self.addASTChild(currentAST, self.returnAST)
                else:
                    break
                
            nonAlternatingTemplateExpr_AST = currentAST.root
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_12)
            else:
                raise ex
        
        self.returnAST = nonAlternatingTemplateExpr_AST
    
    def singleArg(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        singleArg_AST = None
        try:      ## for error handling
            pass
            self.match(LPAREN)
            self.nonAlternatingTemplateExpr()
            self.addASTChild(currentAST, self.returnAST)
            self.match(RPAREN)
            if not self.inputState.guessing:
                singleArg_AST = currentAST.root
                singleArg_AST = antlr.make(self.astFactory.create(SINGLEVALUEARG,"SINGLEVALUEARG"), singleArg_AST)
                currentAST.root = singleArg_AST
                if (singleArg_AST != None) and (singleArg_AST.getFirstChild() != None):
                    currentAST.child = singleArg_AST.getFirstChild()
                else:
                    currentAST.child = singleArg_AST
                currentAST.advanceChildToEnd()
            singleArg_AST = currentAST.root
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_11)
            else:
                raise ex
        
        self.returnAST = singleArg_AST
    
    def namedTemplate(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        namedTemplate_AST = None
        qid = None
        qid_AST = None
        try:      ## for error handling
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in [ID]:
                pass
                tmp28_AST = None
                tmp28_AST = self.astFactory.create(self.LT(1))
                self.addASTChild(currentAST, tmp28_AST)
                self.match(ID)
                self.argList()
                self.addASTChild(currentAST, self.returnAST)
                namedTemplate_AST = currentAST.root
            elif la1 and la1 in [LITERAL_super]:
                pass
                self.match(LITERAL_super)
                self.match(DOT)
                qid = self.LT(1)
                qid_AST = self.astFactory.create(qid)
                self.addASTChild(currentAST, qid_AST)
                self.match(ID)
                if not self.inputState.guessing:
                    qid_AST.setText("super."+qid_AST.getText())
                self.argList()
                self.addASTChild(currentAST, self.returnAST)
                namedTemplate_AST = currentAST.root
            elif la1 and la1 in [LPAREN]:
                pass
                self.indirectTemplate()
                self.addASTChild(currentAST, self.returnAST)
                namedTemplate_AST = currentAST.root
            else:
                    raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_7)
            else:
                raise ex
        
        self.returnAST = namedTemplate_AST
    
    def argList(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        argList_AST = None
        try:      ## for error handling
            if (self.LA(1)==LPAREN) and (self.LA(2)==RPAREN):
                pass
                self.match(LPAREN)
                self.match(RPAREN)
                if not self.inputState.guessing:
                    argList_AST = currentAST.root
                    argList_AST = self.astFactory.create(ARGS,"ARGS")
                    currentAST.root = argList_AST
                    if (argList_AST != None) and (argList_AST.getFirstChild() != None):
                        currentAST.child = argList_AST.getFirstChild()
                    else:
                        currentAST.child = argList_AST
                    currentAST.advanceChildToEnd()
            else:
                synPredMatched44 = False
                if (self.LA(1)==LPAREN) and (_tokenSet_1.member(self.LA(2))):
                    _m44 = self.mark()
                    synPredMatched44 = True
                    self.inputState.guessing += 1
                    try:
                        pass
                        self.singleArg()
                    except antlr.RecognitionException, pe:
                        synPredMatched44 = False
                    self.rewind(_m44)
                    self.inputState.guessing -= 1
                if synPredMatched44:
                    pass
                    self.singleArg()
                    self.addASTChild(currentAST, self.returnAST)
                    argList_AST = currentAST.root
                elif (self.LA(1)==LPAREN) and (self.LA(2)==ID or self.LA(2)==DOTDOTDOT):
                    pass
                    self.match(LPAREN)
                    self.argumentAssignment()
                    self.addASTChild(currentAST, self.returnAST)
                    while True:
                        if (self.LA(1)==COMMA):
                            pass
                            self.match(COMMA)
                            self.argumentAssignment()
                            self.addASTChild(currentAST, self.returnAST)
                        else:
                            break
                        
                    self.match(RPAREN)
                    if not self.inputState.guessing:
                        argList_AST = currentAST.root
                        argList_AST = antlr.make(self.astFactory.create(ARGS,"ARGS"), argList_AST)
                        currentAST.root = argList_AST
                        if (argList_AST != None) and (argList_AST.getFirstChild() != None):
                            currentAST.child = argList_AST.getFirstChild()
                        else:
                            currentAST.child = argList_AST
                        currentAST.advanceChildToEnd()
                    argList_AST = currentAST.root
                else:
                    raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_11)
            else:
                raise ex
        
        self.returnAST = argList_AST
    

    ###/** Match (foo)() and (foo+".terse")() */
    def indirectTemplate(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        indirectTemplate_AST = None
        e_AST = None
        args_AST = None
        try:      ## for error handling
            pass
            tmp36_AST = None
            tmp36_AST = self.astFactory.create(self.LT(1))
            self.match(LPAREN)
            self.expr()
            e_AST = self.returnAST
            tmp37_AST = None
            tmp37_AST = self.astFactory.create(self.LT(1))
            self.match(RPAREN)
            self.argList()
            args_AST = self.returnAST
            if not self.inputState.guessing:
                indirectTemplate_AST = currentAST.root
                indirectTemplate_AST = antlr.make(self.astFactory.create(VALUE,"value"), e_AST, args_AST)
                currentAST.root = indirectTemplate_AST
                if (indirectTemplate_AST != None) and (indirectTemplate_AST.getFirstChild() != None):
                    currentAST.child = indirectTemplate_AST.getFirstChild()
                else:
                    currentAST.child = indirectTemplate_AST
                currentAST.advanceChildToEnd()
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_11)
            else:
                raise ex
        
        self.returnAST = indirectTemplate_AST
    
    def argumentAssignment(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        argumentAssignment_AST = None
        try:      ## for error handling
            la1 = self.LA(1)
            if False:
                pass
            elif la1 and la1 in [ID]:
                pass
                tmp38_AST = None
                tmp38_AST = self.astFactory.create(self.LT(1))
                self.addASTChild(currentAST, tmp38_AST)
                self.match(ID)
                tmp39_AST = None
                tmp39_AST = self.astFactory.create(self.LT(1))
                self.makeASTRoot(currentAST, tmp39_AST)
                self.match(ASSIGN)
                self.nonAlternatingTemplateExpr()
                self.addASTChild(currentAST, self.returnAST)
                argumentAssignment_AST = currentAST.root
            elif la1 and la1 in [DOTDOTDOT]:
                pass
                tmp40_AST = None
                tmp40_AST = self.astFactory.create(self.LT(1))
                self.addASTChild(currentAST, tmp40_AST)
                self.match(DOTDOTDOT)
                argumentAssignment_AST = currentAST.root
            else:
                    raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_12)
            else:
                raise ex
        
        self.returnAST = argumentAssignment_AST
    
    
    def buildTokenTypeASTClassMap(self):
        self.tokenTypeToASTClassMap = None

_tokenNames = [
    "<0>", 
    "EOF", 
    "<2>", 
    "NULL_TREE_LOOKAHEAD", 
    "APPLY", 
    "MULTI_APPLY", 
    "ARGS", 
    "INCLUDE", 
    "\"if\"", 
    "VALUE", 
    "TEMPLATE", 
    "FUNCTION", 
    "SINGLEVALUEARG", 
    "LIST", 
    "SEMI", 
    "LPAREN", 
    "RPAREN", 
    "\"separator\"", 
    "ASSIGN", 
    "COLON", 
    "COMMA", 
    "NOT", 
    "PLUS", 
    "DOT", 
    "ID", 
    "\"first\"", 
    "\"rest\"", 
    "\"last\"", 
    "\"super\"", 
    "ANONYMOUS_TEMPLATE", 
    "STRING", 
    "INT", 
    "LBRACK", 
    "RBRACK", 
    "DOTDOTDOT", 
    "TEMPLATE_ARGS", 
    "NESTED_ANONYMOUS_TEMPLATE", 
    "ESC_CHAR", 
    "WS", 
    "WS_CHAR"
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
    data = [ 8573190144L, 0L]
    return data
_tokenSet_1 = antlr.BitSet(mk_tokenSet_1())

### generate bit set
def mk_tokenSet_2(): 
    ### var1
    data = [ 8586821632L, 0L]
    return data
_tokenSet_2 = antlr.BitSet(mk_tokenSet_2())

### generate bit set
def mk_tokenSet_3(): 
    ### var1
    data = [ 8586379266L, 0L]
    return data
_tokenSet_3 = antlr.BitSet(mk_tokenSet_3())

### generate bit set
def mk_tokenSet_4(): 
    ### var1
    data = [ 81922L, 0L]
    return data
_tokenSet_4 = antlr.BitSet(mk_tokenSet_4())

### generate bit set
def mk_tokenSet_5(): 
    ### var1
    data = [ 65536L, 0L]
    return data
_tokenSet_5 = antlr.BitSet(mk_tokenSet_5())

### generate bit set
def mk_tokenSet_6(): 
    ### var1
    data = [ 8591589378L, 0L]
    return data
_tokenSet_6 = antlr.BitSet(mk_tokenSet_6())

### generate bit set
def mk_tokenSet_7(): 
    ### var1
    data = [ 1654786L, 0L]
    return data
_tokenSet_7 = antlr.BitSet(mk_tokenSet_7())

### generate bit set
def mk_tokenSet_8(): 
    ### var1
    data = [ 3774873600L, 0L]
    return data
_tokenSet_8 = antlr.BitSet(mk_tokenSet_8())

### generate bit set
def mk_tokenSet_9(): 
    ### var1
    data = [ 8604172290L, 0L]
    return data
_tokenSet_9 = antlr.BitSet(mk_tokenSet_9())

### generate bit set
def mk_tokenSet_10(): 
    ### var1
    data = [ 8581578752L, 0L]
    return data
_tokenSet_10 = antlr.BitSet(mk_tokenSet_10())

### generate bit set
def mk_tokenSet_11(): 
    ### var1
    data = [ 8595783682L, 0L]
    return data
_tokenSet_11 = antlr.BitSet(mk_tokenSet_11())

### generate bit set
def mk_tokenSet_12(): 
    ### var1
    data = [ 1114112L, 0L]
    return data
_tokenSet_12 = antlr.BitSet(mk_tokenSet_12())
    
