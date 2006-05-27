
import sys
import traceback
from StringIO import StringIO

import stringtemplate
from ASTExpr import ASTExpr

builtins = ['bool', 'buffer', 'complex', 'dict', 'file', 'float', 'int',
	    'list', 'long', 'map', 'object', 'str', 'tuple', 'xrange']

## This class knows how to recursively walk a StringTemplate and all
#  of its attributes to dump a type tree out.  STs contain attributes
#  which can contain multiple values.  Those values could be
#  other STs or have types that are aggregates (have properties).  Those
#  types could have properties etc...
#
#  I am not using ST itself to print out the text for $attributes$ because
#  it kept getting into nasty self-recursive loops that made my head really
#  hurt.  Pretty hard to get ST to print itselt out.  Oh well, it was
#  a cool thought while I had it.  I just dump raw text to an output buffer
#  now.  Easier to understand also.
#
class AttributeReflectionController(object):

    def __init__(self, st):
	## To avoid infinite loops with cyclic type refs, track the types
	self.typesVisited = {}
	## Build up a string buffer as you walk the nested data structures
	self.output = StringIO()
	## Can't use ST to output so must do our own indentation
	self.indentation = 0
	## If in a list, itemIndex=1..n; used to print ordered list in indent().
	#  Use a stack since we have nested lists.
	#
	self.itemIndexStack = []
	self.st = st

    def __str__(self):
        self.typesVisited = {}
        self.output = StringIO()
        self.saveItemIndex()
        self.walkStringTemplate(self.st)
        self.restoreItemIndex();
        self.typesVisited = None
        return self.output.getvalue()

    def walkStringTemplate(self, st):
        # make sure the exact ST instance has not been visited
	if not st or st in self.typesVisited:
            return
        self.typesVisited[st.getName()] = st
        self.indent()
        self.output.write('Template ' + st.getName() + ' :\n')
        self.indentation += 1
        self.walkAttributes(st)
        self.indentation -= 1

    ## Walk all the attributes in this template, spitting them out.
    #
    def walkAttributes(self, st):
        if not st or not st.getAttributes():
            return
        self.saveItemIndex()
	for name in st.getAttributes().keys():
            if name == ASTExpr.REFLECTION_ATTRIBUTES:
                continue
            value = st.getAttributes()[name]
            self.incItemIndex()
            self.indent()
            self.output.write('Attribute ' + name + ' values :\n')
            self.indentation += 1
            self.walkAttributeValues(value)
            self.indentation -= 1
        self.restoreItemIndex()

    def walkAttributeValues(self, attributeValue):
        self.saveItemIndex()
        if isinstance(attributeValue, list) or \
	   isinstance(attributeValue, tuple):
	    for value in attributeValue:
                self.incItemIndex()
                self.walkValue(value, value.__class__)
        else:
            self.incItemIndex()
            self.walkValue(attributeValue, attributeValue.__class__)
        self.restoreItemIndex()

    ## Get the list of properties by looking for get/isXXX methods.
    #  The value is the instance of "type" we are concerned with.
    #  Return None if no properties
    #
    def walkPropertiesList(self, aggregateValue, type_):
        if type_.__name__ in self.typesVisited:
            return
	# We do not want the properties of any built-in types
	if type_.__name__ in builtins:
	    return
        self.typesVisited[type_.__name__] = type_
        self.saveItemIndex()
	# Now we are going to search the properties of the given type by
	# looking for is/get methods. The problem is that we cannot derive
	# the return type of any of these methods directly. Moreover, their
	# uses are actually quite different: an is property can only be used
	# in a conditional expression, under the condition that that method
	# takes no arguments other than 'self'. A get property can be used
	# in both a conditional expression as well as a normal expression.
	# Again the condition holds that the method cannot take any other
	# arguments than 'self'.
	for methodName in dir(type_):
	    returnType = None
            instanceReturnType = None
            if len(methodName) > 3 and methodName.startswith('get'):
                propName = methodName[3].lower() + methodName[4:]
	        try:
		    # Try to create a default object of the given type
		    o = type_()
		except Exception:
		    # Cannot create a default object, we assume this is a
		    # valid property, though.
		    pass
		else:
		    try:
			# Try to call the method with only the self argument
			r = getattr(o, methodName)()
		    except Exception, e:
			# Cannot call the method this way, this is not a valid
			# property.
			continue
		    returnType = type(r)
		    # Sometimes the return type can be instance instead of a
		    # class, in which case we need to 
		    if returnType.__name__ == 'instance':
		        instanceReturnType = r.__class__
		#sys.stderr.write("propName = " + propName + '\n')
            elif len(methodName) > 2 and methodName.startswith('is'):
                propName = methodName[2].lower() + methodName[3:]
		returnType = None
		#sys.stderr.write("propName = " + propName + '\n')
            else:
                continue
	    # if type_ has a hideProperties field, check whether the name of
	    # the property we've found is in it. If so, skip this property
	    # name.
	    if hasattr(type_, 'hideProperties'):
	        if propName in type_.hideProperties:
		    continue
            self.incItemIndex()
            self.indent()
            self.output.write('Property ' + propName + ' : ')
	    if not returnType or returnType == type(None):
                self.output.write('<any>')
	    elif instanceReturnType:
	        self.output.write(instanceReturnType.__name__)
	    else:
	        self.output.write(returnType.__name__)
            self.output.write('\n')

            # get actual return type for get methods for STs or for Maps; otherwise type is enough
	    
            if returnType == stringtemplate.StringTemplate:
                self.indentation += 1
                self.walkStringTemplate(self.getRawValue(aggregateValue, methodName))
                self.indentation -= 1
            elif ASTExpr.isValidReturnTypeDictInstance(returnType):
                rawMap = self.getRawValue(aggregateValue, methodName)
                # if valid map instance, show key/values
                # otherwise don't descend
                if rawMap and ASTExpr.isValidDictInstance(type(rawMap)):
                    self.indentation += 1
                    self.walkValue(rawMap, type(rawMap))
                    self.indentation -= 1
            elif not AttributeReflectionController.isAtomicType(returnType):
                self.indentation += 1
                self.walkValue(None, returnType)
                self.indentation -= 1
        self.restoreItemIndex()

    def walkValue(self, value, type_):
        # ugh, must switch on type here; can't use polymorphism
        if AttributeReflectionController.isAtomicType(type_):
            self.walkAtomicType(type_)
        elif type_ == stringtemplate.StringTemplate:
            self.walkStringTemplate(value)
        elif ASTExpr.isValidDictInstance(type_):
            self.walkMap(value)
        else:
            self.walkPropertiesList(value, type_)

    def walkAtomicType(self, type_):
        self.indent()
        self.output.write(str(self.terseType(type_.__name__)) + '\n')

    ## Walk all the attributes in this template, spitting them out.
    #
    def walkMap(self, map_):
        if not map_:
            return
        self.saveItemIndex();
	for name in map_.keys():
            if name == ASTExpr.REFLECTION_ATTRIBUTES:
                continue
            value = map_[name]
            self.incItemIndex()
            self.indent()
            self.output.write('Key ' + str(name) + ' : ')
            self.output.write(str(self.terseType(value.__class__.__name__)) + '\n')
            if not AttributeReflectionController.isAtomicType(value.__class__):
                self.indentation += 1
                self.walkValue(value, value.__class__)
                self.indentation -= 1
        self.restoreItemIndex()

    ## For now, assume only builtins is atomic as long as it does not
    #  support the len() operator (except for strings)
    #
    def isAtomicType(type_):
        atomics = ['bool', 'float', 'int', 'long', 'str']
        if type_.__name__ in atomics:
	    return True
	return False
    isAtomicType = staticmethod(isAtomicType)

    def terseType(self, typeName):
        if typeName:
	    return typeName.split('.')[-1]
        return ''

    ## Normally we don't actually get the value of properties since we
    #  can use normal reflection to get the list of properties
    #  of the return type.  If they ask for a value, though, go get it.
    #  Need it for string template return types of properties so we
    #  can get the attribute lists.
    #
    def getRawValue(self, value, method):
        propertyValue = None
        try:
            propertyValue = getattr(value, method)()
        except Exception, e:
            self.st.error('Can\'t get property ' + method + ' value', e)
        return propertyValue

    def indent(self):
        for i in range(self.indentation):
            self.output.write('    ')
        itemIndex = self.itemIndexStack[-1]
        if itemIndex > 0:
            self.output.write(str(itemIndex) + '. ')

    def incItemIndex(self):
        itemIndex = self.itemIndexStack.pop()
        self.itemIndexStack.append(itemIndex+1)

    def saveItemIndex(self):
        self.itemIndexStack.append(0)

    def restoreItemIndex(self):
        self.itemIndexStack.pop()

