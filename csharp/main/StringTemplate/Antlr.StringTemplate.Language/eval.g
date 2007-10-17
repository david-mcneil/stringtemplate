header {
/*
 [The "BSD licence"]
 Copyright (c) 2003-2004 Terence Parr
 All rights reserved.

 Redistribution and use in source and binary forms, with or without
 modification, are permitted provided that the following conditions
 are met:
 1. Redistributions of source code must retain the above copyright
    notice, this list of conditions and the following disclaimer.
 2. Redistributions in binary form must reproduce the above copyright
    notice, this list of conditions and the following disclaimer in the
    documentation and/or other materials provided with the distribution.
 3. The name of the author may not be used to endorse or promote products
    derived from this software without specific prior written permission.

 THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using Antlr.StringTemplate;
using Antlr.StringTemplate.Collections;
using System.Collections;
using System.IO;
using System.Reflection;
}

options {
	language="CSharp";
	namespace="Antlr.StringTemplate.Language";
}

class ActionEvaluator extends TreeParser;

options {
    importVocab=ActionParser;
    ASTLabelType = "Antlr.StringTemplate.Language.StringTemplateAST";
}

{
    public class NameValuePair {
        public String name;
        public object value;
    };

    protected StringTemplate self = null;
    protected IStringTemplateWriter @out = null;
    protected ASTExpr chunk = null;

    /** Create an evaluator using attributes from self */
    public ActionEvaluator(StringTemplate self, ASTExpr chunk, IStringTemplateWriter @out) {
        this.self = self;
        this.chunk = chunk;
        this.@out = @out;
    }
 
	override public void reportError(RecognitionException e) {
		self.Error("eval tree parse error", e);
	}
}

action returns [int numCharsWritten=0]
{
    object e=null;
}
    :   e=expr {numCharsWritten = chunk.WriteAttribute(self,e,@out);}
    ;

expr returns [object value=null]
{
    object a=null, b=null, e=null;
}
    :   #(PLUS a=expr b=expr {value = chunk.Add(a,b);})
    |   value=templateApplication
    |   value=attribute
    |   value=templateInclude
    |	value=function
    |	value=list    
    |   #(VALUE e=expr)
        // convert to string (force early eval)
        {
	        StringWriter buf = new StringWriter();
	        IStringTemplateWriter sw = self.Group.CreateInstanceOfTemplateWriter(buf);
	        int n = chunk.WriteAttribute(self,e,sw);
			if (n > 0)
			{
	        	value = buf.ToString();
	
	        }
		}
    ;
    
/** create a new list of expressions as a new multi-value attribute */
list returns [object value=null]
{
object e = null;
IList elements = new ArrayList();
value = new CatIterator(elements);
}
	:	#(	LIST
			(	e=expr
			  	{
			  	if ( e!=null ) {
					e = ASTExpr.ConvertAnythingToIterator(e);
			  		elements.Add(e);
			  	}
			  	}
			)+
		 )
	;

templateInclude returns [object value=null]
{
    StringTemplateAST args = null;
    string name = null;
    object n = null;
}
    :   #( INCLUDE
//        {value = chunk.GetTemplateInclude(self, name.getText(), #args);}
            (   id:ID a1:.
                {name=id.getText(); args=#a1;}

            |   #( VALUE n=expr a2:. )
                {if (n!=null) {name=n.ToString();} args=#a2;}

            )
         )
        {
        if ( name!=null ) {
        	value = chunk.GetTemplateInclude(self, name, args);
        }
        }
    ;

/** Apply template(s) to an attribute; can be applied to another apply
 *  result.
 */
templateApplication returns [object value=null]
{
object a=null;
ArrayList templatesToApply=new ArrayList();
ArrayList attributes=new ArrayList();
}
    :   #(  APPLY a=expr
    		(template[templatesToApply])+
	        {value = chunk.ApplyListOfAlternatingTemplates(self,a,templatesToApply);}
         )
    |	#(	MULTI_APPLY (a=expr {attributes.Add(a);} )+ COLON
			anon:ANONYMOUS_TEMPLATE
			{
			StringTemplate anonymous = anon.StringTemplate;
			templatesToApply.Add(anonymous);
			value = chunk.ApplyTemplateToListOfAttributes(self,
														  attributes,
														  anon.StringTemplate);
			}
    	 )
    ;
    
function returns [object value=null]
{
object a;
}
    :	#(	FUNCTION
    		(	"first"  a=singleFunctionArg	{value=chunk.First(a);}
    		|	"rest" 	 a=singleFunctionArg	{value=chunk.Rest(a);}
    		|	"last"   a=singleFunctionArg	{value=chunk.Last(a);}
    		|	"length" a=singleFunctionArg	{value=chunk.Length(a);}
    		|	"strip"  a=singleFunctionArg	{value=chunk.Strip(a);}
    		|	"trunc"  a=singleFunctionArg	{value=chunk.Trunc(a);}
    		)

    	 )
	;

singleFunctionArg returns [object value=null]
	:	#( SINGLEVALUEARG value=expr )
	;    

template[ArrayList templatesToApply]
{
object n = null;
}
    :   #(  TEMPLATE
            (   t:ID args:. // don't eval argList now; must re-eval each iteration
                {
                string templateName = t.getText();
                StringTemplateGroup group = self.Group;
                StringTemplate embedded = group.GetEmbeddedInstanceOf(self, templateName);
                if ( embedded!=null ) {
                    embedded.ArgumentsAST = #args;
                    templatesToApply.Add(embedded);
                }
                }

            |	anon:ANONYMOUS_TEMPLATE
                {
                StringTemplate anonymous = anon.StringTemplate;
                // to properly see overridden templates, always set
                // anonymous' group to be self's group
				anonymous.Group = self.Group;
                templatesToApply.Add(anonymous);
                }

            |   #(	VALUE n=expr args2:. 
					{
						StringTemplate embedded = null;
						if ( n!=null ) {
                			String templateName = n.ToString();
							StringTemplateGroup group = self.Group;
							embedded = group.GetEmbeddedInstanceOf(self, templateName);
							if ( embedded!=null ) {
								embedded.ArgumentsAST = #args2;
								templatesToApply.Add(embedded);
							}
						}
				   }
                )
            )
         )
    ;

ifCondition returns [bool value=false]
{
    object a=null;
}
    :   a=expr {value = chunk.TestAttributeTrue(a);}
    |   #(NOT a=expr) {value = !chunk.TestAttributeTrue(a);}
	;

attribute returns [object value=null]
{
    object obj = null;
    string propName = null;
    object e = null;
}
    :   #( DOT obj=expr
           ( prop:ID {propName = prop.getText();}
           | #(VALUE e=expr)
             {if (e!=null) {propName=e.ToString();}}
           )
         )
        {value = chunk.GetObjectProperty(self,obj,propName);}

    |   i3:ID
        {
        value=self.GetAttribute(i3.getText());
        }

    |   i:INT {value=Int32.Parse(i.getText());}

    |   s:STRING
    	{
    	value=s.getText();
    	}

    |   at:ANONYMOUS_TEMPLATE
    	{
    	value=at.getText();
		if ( at.getText()!=null ) {
			StringTemplate valueST =new StringTemplate(self.Group, at.getText());
			valueST.EnclosingInstance = self;
			valueST.Name = "<anonymous template argument>";
			value = valueST;
    	}
    	}
    ;
    
/** self is assumed to be the enclosing context as foo(x=y) must find y in
 *  the template that encloses the ref to foo(x=y).  We must pass in
 *  the embedded template (the one invoked) so we can check formal args
 *  in RawSetArgumentAttribute.
 */
argList[StringTemplate embedded, IDictionary initialContext]
    returns [IDictionary argumentContext=null]
{
    argumentContext = initialContext;
    if ( argumentContext==null ) {
        argumentContext=new Hashtable();
    }
}
    :   #( ARGS (argumentAssignment[embedded,argumentContext])* )
    |	singleTemplateArg[embedded,argumentContext]
	;
	
singleTemplateArg[StringTemplate embedded, IDictionary argumentContext]
{
    object e = null;
}
	:	#( SINGLEVALUEARG e=expr )
	    {
	    if ( e!=null ) {
	    	string soleArgName = null;
	    	// find the sole defined formal argument for embedded
	    	bool error = false;
			HashList formalArgs = (HashList) embedded.FormalArguments;
			if ( formalArgs!=null ) {
				ICollection argNames = formalArgs.Keys;
				if ( argNames.Count==1 ) {
					IEnumerator iter = argNames.GetEnumerator();
					iter.MoveNext();
					soleArgName = (string) iter.Current;
					//Console.WriteLine("sole formal arg of "+embedded.Name+" is "+soleArgName);
				}
				else {
					error=true;
				}
			}
			else {
				error=true;
			}
			if ( error ) {
				self.Error("template "+embedded.Name+
				           " must have exactly one formal arg in template context "+
						   self.GetEnclosingInstanceStackString());
		   	}
		   	else {
		   		self.RawSetArgumentAttribute(embedded,argumentContext,soleArgName,e);
		   	}
	    }
	    }
	;
	
argumentAssignment[StringTemplate embedded, IDictionary argumentContext]
{
    object e = null;
}
	:	#( ASSIGN arg:ID e=expr )
	    {
	    if ( e!=null ) {
			self.RawSetArgumentAttribute(embedded,argumentContext,arg.getText(),e);
		}
	    }
	|	DOTDOTDOT {embedded.PassThroughAttributes = true;}
	;