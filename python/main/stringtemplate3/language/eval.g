header {
from stringtemplate3.language.CatIterator import CatList
from stringtemplate3.language.StringTemplateAST import StringTemplateAST

from StringIO import StringIO

class NameValuePair(object):

    def __init__(self):
        self.name = None
        self.value = None

}

header "ActionEvaluator.__init__" {
    self.this = None
    self.out = None
    self.chunk = None
}

options {
    language = "Python";
}

class ActionEvaluator extends TreeParser;

options {
    importVocab=ActionParser;
    ASTLabelType = "StringTemplateAST";
}

{
    def initialize(self, this, chunk, out):
        self.this = this
        self.chunk = chunk
        self.out = out

    def reportError(self, e):
        self.this.error("eval tree parse error", e)
}

action returns [numCharsWritten = 0]
{
    e = None
}
    :   e = expr
        {
          numCharsWritten = self.chunk.writeAttribute(self.this, e, self.out)
        }
    ;

expr returns [value]
{
    a = None
    b = None
    e = None
}
    :   #( PLUS a=expr b=expr { value = self.chunk.add(a,b) } )
    |   value=templateApplication
    |   value=attribute
    |   value=templateInclude
    |   value=function
    |   value=list
    |   #( VALUE e=expr )
        // convert to string (force early eval)
        {
            buf = StringIO()
            sw = self.this.group.getStringTemplateWriter(buf)
            n = self.chunk.writeAttribute(self.this, e, sw)
            if n > 0:
                value = buf.getvalue()
        }
    ;

/** create a new list of expressions as a new multi-value attribute */
list returns [value=None]
{
    e = None
    elements = []
    value = CatList(elements)
}
    :   #( LIST
          ( e=expr
            {
              if e:
                  from stringtemplate3.language.ASTExpr import convertAnythingToList
                  e = convertAnythingToList(e)
                  elements.append(e)
            }
          )+
        )
    |   NOTHING
        {
            nullSingleton = [None]
            element.append(nullSingleton)
        }
    ;

templateInclude returns [value = None]
{
    args = None
    name = ""
    n = None
}
    :   #( INCLUDE
           (   id:ID a1:.
               { name=id.getText(); args=#a1 }
           |   #( VALUE n=expr a2:. )
               {if n: name = str(n); args=#a2 }
           )
        )
        {
            if name:
                value = self.chunk.getTemplateInclude(self.this, name, args)
        }
    ;

/** Apply template(s) to an attribute; can be applied to another apply
 *  result.
 */
templateApplication returns [value = None]
{
    a = None
    templatesToApply = []
    attributes = []
}
    :   #( APPLY a=expr
           ( template[templatesToApply] )+
           {
               value = self.chunk.applyListOfAlternatingTemplates(self.this, \
                   a, templatesToApply)
           }
        )
    |   #( MULTI_APPLY ( a=expr { attributes.append(a) } )+ COLON
           anon:ANONYMOUS_TEMPLATE
           {
               anonymous = anon.getStringTemplate()
               templatesToApply.append(anonymous)
               value = self.chunk.applyTemplateToListOfAttributes(self.this, \
                   attributes, anon.getStringTemplate())
           }
        )
    ;

function returns [value = None]
{
    a = None
}
    :   #( FUNCTION
           ( "first" a=singleFunctionArg { value = self.chunk.first(a) }
           | "rest"  a=singleFunctionArg { value = self.chunk.rest(a) }
           | "last"  a=singleFunctionArg { value = self.chunk.last(a) }
           | "length"  a=singleFunctionArg { value = self.chunk.length(a) }
           | "strip"  a=singleFunctionArg { value = self.chunk.strip(a) }
           | "trunc"  a=singleFunctionArg { value = self.chunk.trunc(a) }
           )
        )
    ;

singleFunctionArg returns [value = None]
    :   #( SINGLEVALUEARG value=expr )
    ;

template[templatesToApply]
{
    argumentContext = {}
    n = None
}
    :   #( TEMPLATE
           ( t:ID args:.
             // don't eval argList now; must re-eval each iteration
             {
                 templateName = t.getText()
                 group = self.this.group
                 embedded = group.getEmbeddedInstanceOf(
                        templateName,
                        self.this
                    )
                 if embedded:
                     embedded.argumentsAST = #args
                     templatesToApply.append(embedded)
             }
           | anon:ANONYMOUS_TEMPLATE
             {
                 anonymous = anon.getStringTemplate()
                 // to properly see overridden templates, always set
                 // anonymous' group to be self's group
                 anonymous.group = self.this.group
                 templatesToApply.append(anonymous)
             }
           | #( VALUE n=expr args2:.
                {
                    embedded = None
                    if n:
                        templateName = str(n)
                        group = self.this.group
                        embedded = group.getEmbeddedInstanceOf(
                            templateName,
                            self.this
                            )
                        if embedded:
                            embedded.argumentsAST = #args2
                            templatesToApply.append(embedded)
                }
              )
           )
        )
    ;

ifCondition returns [value = False]
{
    a = None
}
    :   a=ifAtom { value = self.chunk.testAttributeTrue(a) }
    |   #( NOT a=ifAtom ) { value = not self.chunk.testAttributeTrue(a) }
        ;

ifAtom returns [value = None]
    :   value=expr
    ;

attribute returns [value = None]
{
    obj = None
    propName = None
    e = None
}
    :   #( DOT obj=expr
           ( prop:ID { propName = prop.getText() }
           | #(VALUE e=expr)
             { if e: propName = str(e) }
           )
        )
        { value = self.chunk.getObjectProperty(self.this, obj, propName) }
    |   i3:ID
        { value = self.this.getAttribute(i3.getText()) }
    |   i:INT { value = int(i.getText()) }
    |   s:STRING { value = s.getText() }
    |   at:ANONYMOUS_TEMPLATE
        {
            value = at.getText();
            if at.getText():
                from stringtemplate3.templates import StringTemplate
                valueST = StringTemplate(
                    group=self.this.group,
                    template=at.getText()
                    )
                valueST.enclosingInstance = self.this
                valueST.name = "<anonymous template argument>"
                value = valueST
        }
    ;

/** self is assumed to be the enclosing context as foo(x=y) must find y in
 *  the template that encloses the ref to foo(x=y).  We must pass in
 *  the embedded template (the one invoked) so we can check formal args
 *  in rawSetArgumentAttribute.
 */
argList[embedded, initialContext] returns [argumentContext = None]
{
    argumentContext = initialContext
    if not argumentContext:
        argumentContext = {}
}
    :   #( ARGS ( argumentAssignment[embedded, argumentContext] )* )
    |   singleTemplateArg[embedded, argumentContext]
    ;

singleTemplateArg[embedded, argumentContext]
{
    e = None
}
    :    #( SINGLEVALUEARG e=expr )
         {
             if e:
                 soleArgName = None
                 // find the sole defined formal argument for embedded
                 error = False
                 formalArgs = embedded.formalArguments
                 if formalArgs:
                     argNames = formalArgs.keys()
                     if len(argNames) == 1:
                         soleArgName = argNames[0]
                         //sys.stderr.write("sole formal arg of " +
                         //                 embedded.getName() + " is " +
                         //                 soleArgName)
                     else:
                         error = True
             else:
                 error = True
             if error:
                 self.this.error("template " + embedded.name +
                                 " must have exactly one formal arg in" +
                                 " template context " +
                                 self.this.enclosingInstanceStackString)
             else:
                 self.this.rawSetArgumentAttribute(embedded, \
                     argumentContext, soleArgName, e)
         }
    ;

argumentAssignment[embedded, argumentContext]
{
    e = None
}
    :   #( ASSIGN arg:ID e=expr )
        {
            if e:
                self.this.rawSetArgumentAttribute(embedded, argumentContext, \
                    arg.getText(), e)
        }
    |   DOTDOTDOT { embedded.setPassThroughAttributes(True) }
    ;
