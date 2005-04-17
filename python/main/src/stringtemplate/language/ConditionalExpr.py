
import sys

import antlr

from ASTExpr import *
import ActionEvaluator

## A conditional reference to an embedded subtemplate.
#
class ConditionalExpr(ASTExpr):

    def __init__(self, enclosingTemplate, tree):
        super(ConditionalExpr, self).__init__(enclosingTemplate, tree, None)
        self.subtemplate = None
        self.elseSubtemplate = None

    def setSubtemplate(self, subtemplate):
        self.subtemplate = subtemplate

    def getSubtemplate():
        return self.subtemplate

    def setElseSubtemplate(self, elseSubtemplate):
        self.elseSubtemplate = elseSubtemplate

    def getElseSubtemplate():
        return self.elseSubtemplate

    ## To write out the value of a condition expr, invoke the evaluator in eval.g
    #  to walk the condition tree computing the boolean value.  If result
    #  is true, then write subtemplate.
    #
    def write(self, this, out):
        if not self.exprTree or not this or not out:
            return
        # sys.stderr.write('evaluating conditional tree: ' + exprTree.toStringList())
        eval_ = ActionEvaluator.Walker()
	eval_.initialize(this,self,out)
        try:
            # get conditional from tree and compute result
            cond = self.exprTree.getFirstChild()
	    # sys.stderr.write("cond = " +  str(cond) + '\n')
	    # eval and write out tree. In case the condition refers to an undefined
	    # attribute, we catch the KeyError exception and proceed with a False
	    # value.
	    try:
                includeSubtemplate = eval_.ifCondition(cond)
	    except KeyError, ke:
	        includeSubtemplate = False
            # sys.stderr.write("subtemplate "+subtemplate + '\n');
            if includeSubtemplate:
                # To evaluate the IF chunk, make a new instance whose enclosingInstance
                # points at 'this' so get attribute works.  Otherwise, enclosingInstance
                # points at the template used to make the precompiled code.  We need a
                # new template instance every time we exec this chunk to get the new
                # "enclosing instance" pointer.
                #
                s = self.subtemplate.getInstanceOf()
                s.setEnclosingInstance(this)
                s.write(out)
            elif self.elseSubtemplate:
                # evaluate ELSE clause if present and IF condition failed
                s = self.elseSubtemplate.getInstanceOf()
                s.setEnclosingInstance(this)
                s.write(out)
        except antlr.RecognitionException, re:
            this.error('can\'t evaluate tree: ' + exprTree.toStringList(), re)
