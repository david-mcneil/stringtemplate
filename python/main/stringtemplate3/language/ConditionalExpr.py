
import antlr

from stringtemplate3.language import ASTExpr
from stringtemplate3.language import ActionEvaluator
from stringtemplate3.utils import deprecated


class ElseIfClauseData(object):
    def __init__(self, expr, st):
        self.expr = expr
        self.st = st


class ConditionalExpr(ASTExpr):
    """A conditional reference to an embedded subtemplate."""

    def __init__(self, enclosingTemplate, tree):
        super(ConditionalExpr, self).__init__(enclosingTemplate, tree, None)
        self.subtemplate = None
        self.elseIfSubtemplates = None
        self.elseSubtemplate = None


    @deprecated
    def setSubtemplate(self, subtemplate):
        self.subtemplate = subtemplate

    @deprecated
    def getSubtemplate(self):
        return self.subtemplate


    @deprecated
    def setElseSubtemplate(self, elseSubtemplate):
        self.elseSubtemplate = elseSubtemplate

    @deprecated
    def getElseSubtemplate(self):
        return self.elseSubtemplate


    def addElseIfSubtemplate(self, conditionalTree, subtemplate):
        if self.elseIfSubtemplates is None:
            self.elseIfSubtemplates = []

        d = ElseIfClauseData(conditionalTree, subtemplate)
        self.elseIfSubtemplates.append(d)


    def write(self, this, out):
        """
        To write out the value of a condition expr, invoke the evaluator in
        eval.g to walk the condition tree computing the boolean value.
        If result is true, then write subtemplate.
        """

        if self.exprTree is None or this is None or out is None:
            return 0
        
        evaluator = ActionEvaluator.Walker()
        evaluator.initialize(this, self, out)
        n = 0
        try:
            testedTrue = False
            # get conditional from tree and compute result
            cond = self.exprTree.getFirstChild()

            # eval and write out tree. In case the condition refers to an
            # undefined attribute, we catch the KeyError exception and proceed
            # with a False value.
            try:
                includeSubtemplate = evaluator.ifCondition(cond)
            except KeyError, ke:
                includeSubtemplate = False

            if includeSubtemplate:
                n = self.writeSubTemplate(this, out, self.subtemplate)
                testedTrue = True

            elif ( self.elseIfSubtemplates is not None and
                   len(self.elseIfSubtemplates) > 0 ):
                for elseIfClause in self.elseIfSubtemplates:
                    try:
                        includeSubtemplate = evaluator.ifCondition(elseIfClause.expr.exprTree)
                    except KeyError, ke:
                        includeSubtemplate = False
                    if includeSubtemplate:
                        n = self.writeSubTemplate(this, out, elseIfClause.st)
                        testedTrue = True
                        break

            if not testedTrue and self.elseSubtemplate is not None:
                # evaluate ELSE clause if present and IF/ELSEIF conditions
                # failed
                n = self.writeSubTemplate(this, out, self.elseSubtemplate)
                        
        except antlr.RecognitionException, re:
            this.error(
                "can't evaluate tree: " + self.exprTree.toStringList(), re
                )

        return n


    def writeSubTemplate(self, this, out, subtemplate):
        # To evaluate the IF chunk, make a new instance whose enclosingInstance
        # points at 'this' so get attribute works. Otherwise, enclosingInstance
        # points at the template used to make the precompiled code.  We need a
        # new template instance every time we exec this chunk to get the new
        # "enclosing instance" pointer.

        s = subtemplate.getInstanceOf()
        s.enclosingInstance = this
        # make sure we evaluate in context of enclosing template's
        # group so polymorphism works. :)
        s.group = this.group
        s.nativeGroup = this.nativeGroup
        return s.write(out)
