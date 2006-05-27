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
            elif la1 and la1 in [LPAREN,ID,LITERAL_super,STRING,INT]:
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
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_1)
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
            elif la1 and la1 in [LPAREN,ID,LITERAL_super,STRING,INT]:
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
                self.consumeUntil(_tokenSet_2)
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
                self.consumeUntil(_tokenSet_3)
            else:
                raise ex
        
        self.returnAST = expr_AST
    
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
                self.consumeUntil(_tokenSet_3)
            else:
                raise ex
        
        self.returnAST = template_AST
    
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
                self.consumeUntil(_tokenSet_2)
            else:
                raise ex
        
        self.returnAST = ifAtom_AST
    
    def primaryExpr(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        primaryExpr_AST = None
        try:      ## for error handling
            if (self.LA(1)==ID or self.LA(1)==STRING or self.LA(1)==INT) and (_tokenSet_4.member(self.LA(2))):
                pass
                self.atom()
                self.addASTChild(currentAST, self.returnAST)
                while True:
                    if (self.LA(1)==DOT):
                        pass
                        tmp10_AST = None
                        tmp10_AST = self.astFactory.create(self.LT(1))
                        self.makeASTRoot(currentAST, tmp10_AST)
                        self.match(DOT)
                        la1 = self.LA(1)
                        if False:
                            pass
                        elif la1 and la1 in [ID]:
                            pass
                            tmp11_AST = None
                            tmp11_AST = self.astFactory.create(self.LT(1))
                            self.addASTChild(currentAST, tmp11_AST)
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
                synPredMatched19 = False
                if (self.LA(1)==LPAREN or self.LA(1)==ID or self.LA(1)==LITERAL_super) and (_tokenSet_5.member(self.LA(2))):
                    _m19 = self.mark()
                    synPredMatched19 = True
                    self.inputState.guessing += 1
                    try:
                        pass
                        self.templateInclude()
                    except antlr.RecognitionException, pe:
                        synPredMatched19 = False
                    self.rewind(_m19)
                    self.inputState.guessing -= 1
                if synPredMatched19:
                    pass
                    self.templateInclude()
                    self.addASTChild(currentAST, self.returnAST)
                    primaryExpr_AST = currentAST.root
                elif (self.LA(1)==LPAREN) and (_tokenSet_6.member(self.LA(2))):
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
                self.consumeUntil(_tokenSet_7)
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
                tmp12_AST = None
                tmp12_AST = self.astFactory.create(self.LT(1))
                self.addASTChild(currentAST, tmp12_AST)
                self.match(ID)
                atom_AST = currentAST.root
            elif la1 and la1 in [STRING]:
                pass
                tmp13_AST = None
                tmp13_AST = self.astFactory.create(self.LT(1))
                self.addASTChild(currentAST, tmp13_AST)
                self.match(STRING)
                atom_AST = currentAST.root
            elif la1 and la1 in [INT]:
                pass
                tmp14_AST = None
                tmp14_AST = self.astFactory.create(self.LT(1))
                self.addASTChild(currentAST, tmp14_AST)
                self.match(INT)
                atom_AST = currentAST.root
            else:
                    raise antlr.NoViableAltException(self.LT(1), self.getFilename())
                
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_4)
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
                self.consumeUntil(_tokenSet_4)
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
                tmp16_AST = None
                tmp16_AST = self.astFactory.create(self.LT(1))
                self.addASTChild(currentAST, tmp16_AST)
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
                self.consumeUntil(_tokenSet_7)
            else:
                raise ex
        
        self.returnAST = templateInclude_AST
    
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
                self.consumeUntil(_tokenSet_8)
            else:
                raise ex
        
        self.returnAST = nonAlternatingTemplateExpr_AST
    
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
                tmp19_AST = None
                tmp19_AST = self.astFactory.create(self.LT(1))
                self.addASTChild(currentAST, tmp19_AST)
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
                self.consumeUntil(_tokenSet_3)
            else:
                raise ex
        
        self.returnAST = namedTemplate_AST
    
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
                t_AST.setStringTemplate(anonymous)
            anonymousTemplate_AST = currentAST.root
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_3)
            else:
                raise ex
        
        self.returnAST = anonymousTemplate_AST
    
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
            elif (self.LA(1)==LPAREN) and (self.LA(2)==ID):
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
                self.consumeUntil(_tokenSet_7)
            else:
                raise ex
        
        self.returnAST = argList_AST
    

    ###/** Match (foo)() and (foo+".terse")()
    ### *  breaks encapsulation
    ### */
    def indirectTemplate(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        indirectTemplate_AST = None
        e_AST = None
        args_AST = None
        try:      ## for error handling
            pass
            tmp27_AST = None
            tmp27_AST = self.astFactory.create(self.LT(1))
            self.match(LPAREN)
            self.expr()
            e_AST = self.returnAST
            tmp28_AST = None
            tmp28_AST = self.astFactory.create(self.LT(1))
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
                self.consumeUntil(_tokenSet_7)
            else:
                raise ex
        
        self.returnAST = indirectTemplate_AST
    
    def argumentAssignment(self):    
        
        self.returnAST = None
        currentAST = antlr.ASTPair()
        argumentAssignment_AST = None
        try:      ## for error handling
            pass
            tmp29_AST = None
            tmp29_AST = self.astFactory.create(self.LT(1))
            self.addASTChild(currentAST, tmp29_AST)
            self.match(ID)
            tmp30_AST = None
            tmp30_AST = self.astFactory.create(self.LT(1))
            self.makeASTRoot(currentAST, tmp30_AST)
            self.match(ASSIGN)
            self.nonAlternatingTemplateExpr()
            self.addASTChild(currentAST, self.returnAST)
            argumentAssignment_AST = currentAST.root
        
        except antlr.RecognitionException, ex:
            if not self.inputState.guessing:
                self.reportError(ex)
                self.consume()
                self.consumeUntil(_tokenSet_8)
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
    "COLON", 
    "COMMA", 
    "NOT", 
    "PLUS", 
    "DOT", 
    "ID", 
    "\"super\"", 
    "ANONYMOUS_TEMPLATE", 
    "STRING", 
    "INT", 
    "NESTED_ANONYMOUS_TEMPLATE", 
    "ESC_CHAR", 
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
    data = [ 5122L, 0L]
    return data
_tokenSet_1 = antlr.BitSet(mk_tokenSet_1())

### generate bit set
def mk_tokenSet_2(): 
    ### var1
    data = [ 4096L, 0L]
    return data
_tokenSet_2 = antlr.BitSet(mk_tokenSet_2())

### generate bit set
def mk_tokenSet_3(): 
    ### var1
    data = [ 103426L, 0L]
    return data
_tokenSet_3 = antlr.BitSet(mk_tokenSet_3())

### generate bit set
def mk_tokenSet_4(): 
    ### var1
    data = [ 889858L, 0L]
    return data
_tokenSet_4 = antlr.BitSet(mk_tokenSet_4())

### generate bit set
def mk_tokenSet_5(): 
    ### var1
    data = [ 28837888L, 0L]
    return data
_tokenSet_5 = antlr.BitSet(mk_tokenSet_5())

### generate bit set
def mk_tokenSet_6(): 
    ### var1
    data = [ 28313600L, 0L]
    return data
_tokenSet_6 = antlr.BitSet(mk_tokenSet_6())

### generate bit set
def mk_tokenSet_7(): 
    ### var1
    data = [ 365570L, 0L]
    return data
_tokenSet_7 = antlr.BitSet(mk_tokenSet_7())

### generate bit set
def mk_tokenSet_8(): 
    ### var1
    data = [ 69632L, 0L]
    return data
_tokenSet_8 = antlr.BitSet(mk_tokenSet_8())
    
