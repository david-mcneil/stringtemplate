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
using antlr.stringtemplate;
using System.Collections;
using System.IO;
using System.Reflection;
}

options {
	language="CSharp";
	namespace="antlr.stringtemplate.language";
}

class ActionEvaluator extends TreeParser;

options {
    importVocab=ActionParser;
    ASTLabelType = "antlr.stringtemplate.language.StringTemplateAST";
}

{
    public class NameValuePair {
        public String name;
        public Object value;
    };

    protected StringTemplate self = null;
    protected StringTemplateWriter @out = null;
    protected ASTExpr chunk = null;

    /** Create an evaluator using attributes from self */
    public ActionEvaluator(StringTemplate self, ASTExpr chunk, StringTemplateWriter @out) {
        this.self = self;
        this.chunk = chunk;
        this.@out = @out;
    }
 
	override public void reportError(RecognitionException e) {
		self.error("template parse error", e);
	}
}

action returns [int numCharsWritten=0]
{
    Object e=null;
}
    :   e=expr {numCharsWritten = chunk.writeAttribute(self,e,@out);}
    ;

expr returns [Object value=null]
{
    Object a=null, b=null, e=null;
    IDictionary argumentContext=null;
}
    :   #(PLUS a=expr b=expr {value = chunk.add(a,b);})
    |   value=templateApplication
    |   value=attribute
    |   value=templateInclude
    |   #(VALUE e=expr)
        // convert to string (force early eval)
        {
        StringWriter buf = new StringWriter();
        Type writerClass = @out.GetType();
        StringTemplateWriter sw = null;
        try {
            ConstructorInfo ctor =
            	writerClass.GetConstructor(new Type[] {typeof(TextWriter)});
            sw = (StringTemplateWriter)ctor.Invoke(new Object[] {buf});
        }
        catch (Exception exc) {
        	// default new AutoIndentWriter(buf)
        	self.error("cannot make implementation of StringTemplateWriter",exc);
        	sw = new AutoIndentWriter(buf);
      	}
        chunk.writeAttribute(self,e,sw);
        value = buf.ToString();
        }
    ;

templateInclude returns [Object value=null]
{
    StringTemplateAST args = null;
    String name = null;
    Object n = null;
}
    :   #( INCLUDE
//        {value = chunk.getTemplateInclude(self, name.getText(), #args);}
            (   id:ID a1:.
                {name=id.getText(); args=#a1;}

            |   #( VALUE n=expr a2:. )
                {if (n!=null) {name=n.ToString();} args=#a2;}

            )
         )
        {
        if ( name!=null ) {
        	value = chunk.getTemplateInclude(self, name, args);
        }
        }
    ;

/** Apply template(s) to an attribute; can be applied to another apply
 *  result.
 */
templateApplication returns [Object value=null]
{
Object a=null;
ArrayList templatesToApply=new ArrayList();
}
    :   #(  APPLY a=expr (template[templatesToApply])+
            {value = chunk.applyListOfAlternatingTemplates(self,a,templatesToApply);}
         )
    ;

template[ArrayList templatesToApply]
{
IDictionary argumentContext = null;
Object n = null;
}
    :   #(  TEMPLATE
            (   t:ID args:. // don't eval argList now; must re-eval each iteration
                {
                String templateName = t.getText();
                StringTemplateGroup group = self.getGroup();
                StringTemplate embedded = group.getEmbeddedInstanceOf(self, templateName);
                if ( embedded!=null ) {
                    embedded.setArgumentsAST(#args);
                    templatesToApply.Add(embedded);
                }
                }

            |	anon:ANONYMOUS_TEMPLATE
                {
                StringTemplate anonymous = anon.getStringTemplate();
                templatesToApply.Add(anonymous);
                }

            |   #( VALUE n=expr argumentContext=argList[null] )
                {
                if ( n!=null ) {
                	String templateName = n.ToString();
					StringTemplateGroup group = self.getGroup();
					StringTemplate embedded = group.getEmbeddedInstanceOf(self, templateName);
					if ( embedded!=null ) {
						embedded.setArgumentsAST(#args);
						templatesToApply.Add(embedded);
					}
                }
                }
            )
         )
    ;

ifCondition returns [bool value=false]
{
    Object a=null, b=null;
}
    :   a=ifAtom {value = chunk.testAttributeTrue(a);}
    |   #(NOT a=ifAtom) {value = !chunk.testAttributeTrue(a);}
	;

ifAtom returns [Object value=null]
    :   value=expr
    ;

attribute returns [Object value=null]
{
    Object obj = null;
}
    :   #( DOT obj=attribute prop:ID )
        {value = chunk.getObjectProperty(self,obj,prop.getText());}

    |   i3:ID
        {
        try {
            value=self.getAttribute(i3.getText());
        }
        catch (ArgumentOutOfRangeException nse) {
            // rethrow with more precise error message
			throw new ArgumentOutOfRangeException(nse.ParamName, String.Format(
				"no such attribute: {0} in template {1}", i3.getText(), self.getName()));
        }
        }

    |   i:INT {value=Int32.Parse(i.getText());}

    |   s:STRING {value=s.getText();}
    ;

argList[IDictionary initialContext]
    returns [IDictionary argumentContext=null]
{
    argumentContext = initialContext;
    if ( argumentContext==null ) {
        argumentContext=new Hashtable();
    }
}
    :   #( ARGS (argumentAssignment[argumentContext])* )
	;

argumentAssignment[IDictionary argumentContext]
{
    Object e = null;
}
	:	#( ASSIGN arg:ID e=expr
	       {
	       if ( e!=null )
	           self.rawSetAttribute(argumentContext,arg.getText(),e);
	       }
	     )
	;

