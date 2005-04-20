
import sys
import traceback
from StringIO import StringIO

import antlr

from Expr import *
import ActionEvaluator
from StringTemplateAST import *
import stringtemplate

class IllegalStateException(Exception):

    def __init__(self, *args):
        Exception.__init__(self, *args)

## A single string template expression enclosed in $...; separator=...$
#  parsed into an AST chunk to be evaluated.
#
class ASTExpr(Expr):

    DEFAULT_ATTRIBUTE_NAME = None
    DEFAULT_ATTRIBUTE_NAME_DEPRECATED = None
    DEFAULT_ATTRIBUTE_KEY = None
    DEFAULT_INDEX_VARIABLE_NAME = None

    ## How to refer to the surrounding template object;
    #  self is attribute name to use.
    SELF_ATTR = None

    ## When lint mode is on, self attribute contains a list of reflection
    #  objects that tell you about the attributes set by the user of the
    #  template.
    #
    REFLECTION_ATTRIBUTES = None

    def __init__(self, enclosingTemplate, exprTree, options):
	super(ASTExpr, self).__init__(enclosingTemplate)
        self.exprTree = exprTree
        ## store separator etc...
        self.options = options

    ## Return the tree interpreted when self template is written out.
    #
    def getAST(self):
	return self.exprTree

    ## To write out the value of an ASTExpr, invoke the evaluator in eval.g
    #  to walk the tree writing out the values.  For efficiency, don't
    #  compute a bunch of strings and then pack them together.  Write out
    #  directly.
    #
    def write(self, this, out):
        if not self.exprTree or not this or not out:
            return 0
        out.pushIndentation(self.getIndentation())
        #sys.stderr.write('evaluating tree: ' +
	#                 self.exprTree.toStringList() + '\n')
        eval_ = ActionEvaluator.Walker()
	eval_.initialize(this,self,out)
        n = 0
        try:
	    # eval and write out tree
            n = eval_.action(self.exprTree)
        except antlr.RecognitionException, re:
            self.error('can\'t evaluate tree: ' + self.exprTree.toStringList(),
	               re)
        out.popIndentation()
        return n

# -----------------------------------------------------------------------------
#             HELP ROUTINES CALLED BY EVALUATOR TREE WALKER
# -----------------------------------------------------------------------------

    def applyListOfAlternatingTemplates(self, this, attributeValue, templatesToApply):
        if not attributeValue or not templatesToApply or templatesToApply == []:
	    # do not apply if missing templates or empty value
            return None

        embedded = None
        argumentContext = None

        if isinstance(attributeValue, list) or \
	   isinstance(attributeValue, tuple) or \
	   isinstance(attributeValue, dict):
            resultVector = []
            i = 0
            for ithValue in attributeValue:
                if not ithValue:
                    # weird...a None value in the list; ignore
                    continue

                templateIndex = i % len(templatesToApply) # rotate through
                embedded = templatesToApply[templateIndex]
                # template to apply is an actual stringtemplate.StringTemplate (created in
                # eval.g), but that is used as the examplar.  We must create
                # a new instance of the embedded template to apply each time
                # to get new attribute sets etc...
                args = embedded.getArgumentsAST()
                embedded = embedded.getInstanceOf() # make new instance
                embedded.setEnclosingInstance(this)
                embedded.setArgumentsAST(args)
                argumentContext = {}
		if isinstance(attributeValue, dict):
                    argumentContext[ASTExpr.DEFAULT_ATTRIBUTE_KEY] = ithValue
                    argumentContext[ASTExpr.DEFAULT_ATTRIBUTE_NAME] = attributeValue[ithValue]
                    argumentContext[ASTExpr.DEFAULT_ATTRIBUTE_NAME_DEPRECATED] = attributeValue[ithValue]
		else:
                    argumentContext[ASTExpr.DEFAULT_ATTRIBUTE_KEY] = None
                    argumentContext[ASTExpr.DEFAULT_ATTRIBUTE_NAME] = ithValue
                    argumentContext[ASTExpr.DEFAULT_ATTRIBUTE_NAME_DEPRECATED] = ithValue
                argumentContext[ASTExpr.DEFAULT_INDEX_VARIABLE_NAME] = i+1
                embedded.setArgumentContext(argumentContext)
                self.evaluateArguments(embedded)
                #sys.stderr.write('i=' + str(i) + ': applyTemplate(' +
		#                 embedded.getName() + ', args=' +
		#                 str(argumentContext) + ' to attribute value ' +
		#                 str(ithValue) + '\n')
                resultVector.append(embedded)
                i += 1
            if not len(resultVector):
                resultVector = None

            return resultVector

        else:
            embedded = templatesToApply[0]
            #sys.stderr.write('setting attribute ' +
	    #                 ASTExpr.DEFAULT_ATTRIBUTE_NAME +
	    #                 ' in arg context of ' + embedded.getName() +
	    #                 ' to ' + str(attributeValue) + '\n')
            argumentContext = {}
            argumentContext[ASTExpr.DEFAULT_ATTRIBUTE_NAME] = attributeValue
            argumentContext[ASTExpr.DEFAULT_ATTRIBUTE_NAME_DEPRECATED] = attributeValue
            argumentContext[ASTExpr.DEFAULT_INDEX_VARIABLE_NAME] = 1
            embedded.setArgumentContext(argumentContext)
            self.evaluateArguments(embedded)
            return embedded

    ## Return o.getPropertyName() given o and propertyName.  If o is
    #  a stringtemplate then access it's attributes looking for propertyName
    #  instead (don't check any of the enclosing scopes; look directly into
    #  that object).  Also try isXXX() for booleans.  Allow HashMap,
    #  Hashtable as special case (grab value for key).
    #
    def getObjectProperty(self, this, o, propertyName):
        if not o or not propertyName:
            return None
        value = None

        # Special case: our automatically created Aggregates via
        # attribute name: "{obj.{prop1,prop2}}"
        # Or: if it's a dictionary then pull using key not the
	# property method.  Do NOT allow general dictionary interface
        # as people could pass in their database masquerading as a
	# dictionary. (?mk)
        if isinstance(o, stringtemplate.Aggregate) or isinstance(o, dict):
	    #sys.stderr.write("[getObjectProperty] o = " + str(o) + '\n')
            value = o[propertyName]

        # Special case: if it's a template, pull property from
        # it's attribute table.
        # TODO: TJP just asked himself why we can't do inherited attr here?
        elif isinstance(o, stringtemplate.StringTemplate):
            attributes = o.getAttributes()
            if attributes:
                value = attributes[propertyName]

        else:
            # use getPropertyName() lookup
            methodSuffix = propertyName[0].upper() + propertyName[1:]
            methodName = 'get' + methodSuffix
            if hasattr(o, methodName):
	        m = getattr(o, methodName)
            else:
                methodName = 'is' + methodSuffix
        	try:
	            m = getattr(o, methodName)
        	except AttributeError, ae:
                    # try for a visible field
                    try:
                        f = getattr(o, propertyName)
                        try:
                            value = o.f
                            return value
                        except AttributeError, ae2:
                            this.error('Can\'t get property ' + propertyName +
                                       ' using method get/is' + methodSuffix +
                                       ' or direct field access from ' +
                                       o.__class__.__name__ + ' instance', ae2)
                    except AttributeError, ae:
                        this.error('Can\'t get property ' + propertyName +
                                   ' using method get/is' + methodSuffix +
                                   ' or direct field access from ' +
                                   o.__class__.__name__ + ' instance', ae)
            try:
                value = m()
            except Exception, e:
                this.error('Can\'t get property ' + propertyName +
		           ' using method get/is' + methodSuffix +
                           ' or direct field access from ' +
                           o.__class__.__name__ + ' instance', e)

        return value

    ## Normally StringTemplate tests presence or absence of attributes
    #  for adherence to my principles of separation, but some people
    #  disagree and want to change.
    #
    #  For 2.0, if the object is a boolean, do something special. $if(boolean)$
    #  will actually test the value.  Now, self breaks my rules of entanglement
    #  listed in my paper, but it truly surprises programmers to have booleans
    #  always True.  Further, the key to isolating logic in the model is avoiding
    #  operators (for which you need attribute values).  But, no operator is
    #  needed to use boolean values.  Well, actually I guess "!" (not) is
    #  an operator.  Regardless, for practical reasons, I'm going to technically
    #  violate my rules as I currently have them defined.  Perhaps for a future
    #  version of the paper I will refine the rules.
    #
    def testAttributeTrue(self, a):
	if not a:
	    return False
	if isinstance(a, bool):
	    return a
	return True


    ## For strings or other objects, catenate and return.
    #
    def add(self, a, b):
        if not a: # a None value means don't do cat, just return other value
            return b
        elif not b:
            return a

        return str(a) + str(b)

    ## Call a string template with args and return result.  Do not convert
    #  to a string yet.  It may need attributes that will be available after
    #  self is inserted into another template.
    #
    def getTemplateInclude(self, enclosing, templateName, argumentsAST):
        group = enclosing.getGroup()
        embedded = group.getEmbeddedInstanceOf(enclosing, templateName)
        if not embedded:
            enclosing.error('cannot make embedded instance of ' +
	                    templateName + ' in template ' +
			    enclosing.getName())
            return None

        embedded.setArgumentsAST(argumentsAST)
        self.evaluateArguments(embedded)
        return embedded

    ## How to spit out an object.  If it's not a StringTemplate nor a sequence,
    #  just do o.toString().  If it's a StringTemplate, do o.write(out).
    #  If it's a sequence, do a write(out, o[i]) for all elements.
    #  Note that if you do something weird like set the values of a
    #  multivalued tag to be sequences, it will effectively flatten it.
    #
    #  If this is an embedded template, you might have specified
    #  a separator arg; used when is a sequence.
    #
    def writeAttribute(self, this, o, out):
        separator = None
        if self.options:
            separator = self.options["separator"]
        return self._write(this, o, out, separator)

    def _write(self, this, o, out, separator):
        if not o:
            return 0
        n = 0
        try:
            if isinstance(o, stringtemplate.StringTemplate):
                o.setEnclosingInstance(this)
                # if this is found up the enclosing instance chain, then
                # infinite recursion
                if stringtemplate.StringTemplate.inLintMode() and \
                   stringtemplate.StringTemplate.isRecursiveEnclosingInstance(o):
                    # throw exception since sometimes eval keeps going
                    # even after I ignore self write of o.
                    raise IllegalStateException('infinite recursion to ' +
                        o.getTemplateDeclaratorString() + ' referenced in ' +
                        o.getEnclosingInstance().getTemplateDeclaratorString() +
			'; stack trace:\n' + o.getEnclosingInstanceStackTrace())
                else:
                    n = out.write(o)
                return n

            if isinstance(o, list) or \
	       isinstance(o, tuple) or \
               isinstance(o, dict):
                separatorString = None
                if separator:
                    separatorString = self.computeSeparator(this, out, separator)
                i = 0
                for iterValue in o:
                    i += 1
                    charWrittenForValue = 0
		    if isinstance(o, dict):
                        charWrittenForValue += \
                            self._write(this, o[iterValue], out, separator)
		    else:
                        charWrittenForValue += \
                            self._write(this, iterValue, out, separator)
                    n += charWrittenForValue
                    #if i < len(o) and separator:
                    #    out.write(separatorString)
		    if i < len(o):
	                valueIsPureConditional = False
		    	if isinstance(iterValue, stringtemplate.StringTemplate):
			    chunks = iterValue.getChunks()
			    firstChunk = chunks[0]
			    valueIsPureConditional = \
			        isinstance(firstChunk, stringtemplate.language.ConditionalExpr) \
				and \
				not firstChunk.getElseSubtemplate()
		    	emptyIteratedValue = valueIsPureConditional and \
		        		     not charWrittenForValue
			if not emptyIteratedValue and separator:
		            n += out.write(separatorString)
            else:
                n = out.write(str(o))
                return n
        except IOError, io:
            this.error('problem writing object: ' + o, io)
        return n

    ## A separator is normally just a string literal, but is still an AST that
    #  we must evaluate.  The separator can be any expression such as a template
    #  include or string cat expression etc...
    #
    def computeSeparator(self, this, out, separator):
        if not separator:
            return None
        if isinstance(separator, StringTemplateAST):
            # must evaluate, writing to a string so we can hand on to it
            e = ASTExpr(self.getEnclosingTemplate(), separator, None)
            buf = StringIO()
	    # create a new instance of whatever StringTemplateWriter
	    # implementation they are using.  Default is stringtemplate.AutoIndentWriter.
	    # Default behavior is to indent but without
            # any prior indents surrounding self attribute expression
	    sw = None
	    try:
		 sw = out.__class__(buf)
	    except Exception, exc:
		# default stringtemplate.AutoIndentWriter(buf)
		this.error('cannot make implementation of StringTemplateWriter',
			   exc)
		sw = stringtemplate.AutoIndentWriter(buf)

	    try:
		e.write(this, sw)
            except Exception, e:
                this.error('can\'t evaluate separator expression', e)

            retval = buf.getvalue()
	    buf.close()
            return retval

        else:
            # just in case we expand in the future and it's something else
            return str(separator)

    def evaluateArguments(self, this):
        argumentsAST = this.getArgumentsAST()
        if not argumentsAST or not argumentsAST.getFirstChild():
            # return immediately if missing tree or no actual args
            return

        eval_ = ActionEvaluator.Walker()
	eval_.initialize(this,self,None)
        try:
            # using any initial argument context (such as when obj is set),
            # evaluate the arg list like bold(item=obj).  Since we pass
            # in any existing arg context, that context gets filled with
            # new values.  With bold(item=obj), context becomes:
            #:[obj=...],[item=...]}.
            ac = eval_.argList(argumentsAST, this.getArgumentContext())
            this.setArgumentContext(ac)

        except antlr.RecognitionException, re:
            this.error('can\'t evaluate tree: ' + argumentsAST.toStringList(),
	               re)
    def isValidDictInstance(type_):
        return type_ == type({})
    isValidDictInstance = staticmethod(isValidDictInstance)

    ## A property can be declared as dictionary, but the instance must be
    #  isValidDictInstance().
    #
    isValidReturnTypeDictInstance = isValidDictInstance

# Set class data attribute values.
ASTExpr.DEFAULT_ATTRIBUTE_NAME = 'it'
ASTExpr.DEFAULT_ATTRIBUTE_KEY = 'ik'
ASTExpr.DEFAULT_ATTRIBUTE_NAME_DEPRECATED = 'attr'
ASTExpr.DEFAULT_INDEX_VARIABLE_NAME = 'i'

ASTExpr.SELF_ATTR = 'self'

ASTExpr.REFLECTION_ATTRIBUTES = 'attributes'
