/*
[Adapted from BSD licence]
Copyright (c) 2005 Kunle Odutola
Copyright (c) 2003-2005 Terence Parr
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
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.*/
using System;
//UPGRADE_TODO: The type 'antlr.collections.AST' could not be found. If it was not included in the conversion, there may be compiler issues. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1262_3"'
using AST = antlr.collections.AST;
using StringTemplate = Antlr.StringTemplate.StringTemplate;
using StringTemplateGroup = Antlr.StringTemplate.StringTemplateGroup;
using Antlr.StringTemplate.Language;
namespace Antlr.StringTemplate.misc
{
	
	/// <summary>A model that pulls data from a string template hierarchy.  This code
	/// is extremely ugly!
	/// </summary>
	//UPGRADE_TODO: Interface 'javax.swing.tree.TreeModel' was converted to 'System.Windows.Forms.TreeNode' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_3"'
	public class JTreeStringTemplateModel : System.Windows.Forms.TreeNode
	{
		//UPGRADE_TODO: Class 'java.util.HashMap' was converted to 'System.Collections.Hashtable' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javautilHashMap_3"'
		internal static System.Collections.IDictionary classNameToWrapperMap = new System.Collections.Hashtable();
		internal abstract class Wrapper
		{
			public abstract object WrappedObject{get;}
			public abstract int getChildCount(object parent);
			public abstract int getIndexOfChild(object parent, object child);
			public abstract object getChild(object parent, int index);
			public virtual bool isLeaf(object node)
			{
				return true;
			}
		}
		
		internal class StringTemplateWrapper:Wrapper
		{
			override public object WrappedObject
			{
				get
				{
					return StringTemplate;
				}
				
			}
			virtual public StringTemplate StringTemplate
			{
				get
				{
					return st;
				}
				
			}
			internal StringTemplate st = null;
			public StringTemplateWrapper(object o)
			{
				this.st = (StringTemplate) o;
			}
			public override object getChild(object parent, int index)
			{
				StringTemplate st = ((StringTemplateWrapper) parent).StringTemplate;
				if (index == 0)
				{
					// System.out.println("chunk type index "+index+" is attributes");
					// return attributes
					return new HashtableWrapper(st.Attributes);
				}
				Expr chunk = (Expr) st.Chunks[index - 1];
				// System.out.println("chunk type index "+index+" is "+chunk.getClass().getName());
				if (chunk is StringRef)
				{
					return chunk;
				}
				return new ExprWrapper(chunk);
			}
			public override int getChildCount(object parent)
			{
				return st.Chunks.Count + 1; // extra one is attribute list
			}
			public override int getIndexOfChild(object parent, object child)
			{
				if (child is Wrapper)
				{
					child = ((Wrapper) child).WrappedObject;
				}
				int index = st.Chunks.IndexOf(child) + 1;
				// System.out.println("index of "+child+" is "+index);
				return index;
			}
			public override bool isLeaf(object node)
			{
				return false;
			}
			public override string ToString()
			{
				if (st == null)
				{
					return "<invalid template>";
				}
				return st.Name;
			}
		}
		
		internal class ExprWrapper:Wrapper
		{
			virtual public Expr Expr
			{
				get
				{
					return expr;
				}
				
			}
			override public object WrappedObject
			{
				get
				{
					return expr;
				}
				
			}
			internal Expr expr = null;
			public ExprWrapper(object o)
			{
				this.expr = (Expr) o;
			}
			public override object getChild(object parent, int index)
			{
				Expr expr = ((ExprWrapper) parent).Expr;
				if (expr is ConditionalExpr)
				{
					// System.out.println("return wrapped subtemplate");
					return new StringTemplateWrapper(((ConditionalExpr) expr).Subtemplate);
				}
				if (expr is ASTExpr)
				{
					ASTExpr astExpr = (ASTExpr) expr;
					AST root = astExpr.AST;
					if (root.getType() == Antlr.StringTemplate.Language.ActionEvaluatorTokenTypes_Fields.INCLUDE)
					{
						switch (index)
						{
							
							case 0: 
								return root.getFirstChild().getNextSibling().toStringList();
							
							case 1: 
								string templateName = root.getFirstChild().getText();
								StringTemplate enclosingST = expr.EnclosingTemplate;
								StringTemplateGroup group = enclosingST.Group;
								StringTemplate embedded = group.GetEmbeddedInstanceOf(enclosingST, templateName);
								return new StringTemplateWrapper(embedded);
						}
					}
				}
				return "<invalid>";
			}
			public override int getChildCount(object parent)
			{
				if (expr is ConditionalExpr)
				{
					return 1;
				}
				AST tree = ((ASTExpr) expr).AST;
				if (tree.getType() == Antlr.StringTemplate.Language.ActionEvaluatorTokenTypes_Fields.INCLUDE)
				{
					return 2; // one for args and one for template
				}
				return 0;
			}
			public override int getIndexOfChild(object parent, object child)
			{
				//System.out.println("Get index of child of "+parent.getClass().getName());
				if (expr is ConditionalExpr)
				{
					return 0;
				}
				return - 1;
			}
			public override bool isLeaf(object node)
			{
				if (expr is ConditionalExpr)
				{
					return false;
				}
				if (expr is ASTExpr)
				{
					AST tree = ((ASTExpr) expr).AST;
					if (tree.getType() == Antlr.StringTemplate.Language.ActionEvaluatorTokenTypes_Fields.INCLUDE)
					{
						return false;
					}
				}
				return true;
			}
			/// <summary>Display different things depending on the ASTExpr type </summary>
			public override string ToString()
			{
				if (expr is ASTExpr)
				{
					AST tree = ((ASTExpr) expr).AST;
					if (tree.getType() == Antlr.StringTemplate.Language.ActionEvaluatorTokenTypes_Fields.INCLUDE)
					{
						return "$include$";
					}
					return "$" + ((ASTExpr) expr).AST.toStringList() + "$";
				}
				if (expr is StringRef)
				{
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043_3"'
					return expr.ToString();
				}
				return "<invalid node type>";
			}
		}
		
		internal class ListWrapper:Wrapper
		{
			override public object WrappedObject
			{
				get
				{
					return v;
				}
				
			}
			internal System.Collections.IList v = null;
			public ListWrapper(object o)
			{
				v = (System.Collections.IList) o;
			}
			public override int getChildCount(object parent)
			{
				return v.Count;
			}
			
			public override int getIndexOfChild(object parent, object child)
			{
				if (child is Wrapper)
				{
					child = ((Wrapper) child).WrappedObject;
				}
				return v.IndexOf(child);
			}
			
			public override object getChild(object parent, int index)
			{
				return v[index];
			}
			
			public override bool isLeaf(object node)
			{
				return false;
			}
		}
		
		/// <summary>Wrap an entry in a map so that name appears as the root and the value
		/// appears as the child or children.
		/// </summary>
		internal class MapEntryWrapper:Wrapper
		{
			override public object WrappedObject
			{
				get
				{
					return Antlr.StringTemplate.misc.JTreeStringTemplateModel.wrap(value_Renamed);
				}
				
			}
			internal object key, value_Renamed;
			public MapEntryWrapper(object key, object value_Renamed)
			{
				this.key = key;
				this.value_Renamed = value_Renamed;
			}
			public override int getChildCount(object parent)
			{
				if (value_Renamed is Wrapper)
				{
					return ((Wrapper) value_Renamed).getChildCount(value_Renamed);
				}
				return 1;
			}
			
			public override int getIndexOfChild(object parent, object child)
			{
				if (value_Renamed is Wrapper)
				{
					return ((Wrapper) value_Renamed).getIndexOfChild(value_Renamed, child);
				}
				return 0;
			}
			
			public override object getChild(object parent, int index)
			{
				if (value_Renamed is Wrapper)
				{
					return ((Wrapper) value_Renamed).getChild(value_Renamed, index);
				}
				return value_Renamed;
			}
			
			public override bool isLeaf(object node)
			{
				return false;
			}
			
			public override string ToString()
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043_3"'
				return key.ToString();
			}
		}
		
		internal class HashtableWrapper:Wrapper
		{
			override public object WrappedObject
			{
				get
				{
					return table;
				}
				
			}
			private System.Collections.IList TableAsListOfKeys
			{
				get
				{
					if (table == null)
					{
						//UPGRADE_TODO: Class 'java.util.LinkedList' was converted to 'System.Collections.ArrayList' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javautilLinkedList_3"'
						return new System.Collections.ArrayList();
					}
					System.Collections.IEnumerator keys = table.Keys.GetEnumerator();
					//UPGRADE_TODO: Class 'java.util.LinkedList' was converted to 'System.Collections.ArrayList' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javautilLinkedList_3"'
					System.Collections.IList v = new System.Collections.ArrayList();
					//UPGRADE_TODO: Method 'java.util.Enumeration.hasMoreElements' was converted to 'System.Collections.IEnumerator.MoveNext' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javautilEnumerationhasMoreElements_3"'
					while (keys.MoveNext())
					{
						//UPGRADE_TODO: Method 'java.util.Enumeration.nextElement' was converted to 'System.Collections.IEnumerator.Current' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javautilEnumerationnextElement_3"'
						string attributeName = (string) keys.Current;
						v.Add(attributeName);
					}
					return v;
				}
				
			}
			internal System.Collections.Hashtable table;
			public HashtableWrapper(object o)
			{
				this.table = (System.Collections.Hashtable) o;
			}
			public override object getChild(object parent, int index)
			{
				System.Collections.IList attributes = TableAsListOfKeys;
				string key = (string) attributes[index];
				object attr = table[key];
				object wrappedAttr = Antlr.StringTemplate.misc.JTreeStringTemplateModel.wrap(attr);
				return new MapEntryWrapper(key, wrappedAttr);
			}
			public override int getChildCount(object parent)
			{
				System.Collections.IList attributes = TableAsListOfKeys;
				return attributes.Count;
			}
			public override int getIndexOfChild(object parent, object child)
			{
				System.Collections.IList attributes = TableAsListOfKeys;
				return attributes.IndexOf(child);
			}
			public override bool isLeaf(object node)
			{
				return false;
			}
			public override string ToString()
			{
				return "attributes";
			}
		}
		
		internal Wrapper root = null;
		
		/// <summary>Get a wrapper object by adding "Wrapper" to class name.
		/// If not present, return the object.
		/// </summary>
		public static object wrap(object o)
		{
			object wrappedObject = o;
			System.Type wrapperClass = null;
			try
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Class.getName' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043_3"'
				wrapperClass = (System.Type) classNameToWrapperMap[o.GetType().FullName];
				System.Reflection.ConstructorInfo ctor = wrapperClass.GetConstructor(new System.Type[]{typeof(object)});
				wrappedObject = ctor.Invoke(new object[]{o});
			}
			catch (System.Exception e)
			{
				// some problem...oh well, just use the object
				;
			}
			return wrappedObject;
		}
		
		public JTreeStringTemplateModel(StringTemplate st)
		{
			if (st == null)
			{
				throw new System.ArgumentException("root is null");
			}
			root = new StringTemplateWrapper(st);
		}
		
		//UPGRADE_NOTE: The equivalent of method 'javax.swing.tree.TreeModel.addTreeModelListener' is not an override method. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1143_3"'
		//UPGRADE_TODO: Interface 'javax.swing.event.TreeModelListener' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095_3"'
		public virtual void  addTreeModelListener(TreeModelListener l)
		{
		}
		
		/// <summary>Get a child object.  If Conditional then return subtemplate.
		/// If ASTExpr and INCLUDE then return ith chunk of sub StringTemplate
		/// </summary>
		//UPGRADE_NOTE: The equivalent of method 'javax.swing.tree.TreeModel.getChild' is not an override method. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1143_3"'
		public virtual object getChild(object parent, int index)
		{
			//System.out.println("Get index "+index+" of "+parent.toString()+":"+parent.getClass().getName());
			if (parent == null)
			{
				return null;
			}
			return ((Wrapper) parent).getChild(parent, index);
		}
		
		//UPGRADE_NOTE: The equivalent of method 'javax.swing.tree.TreeModel.getChildCount' is not an override method. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1143_3"'
		public virtual int getChildCount(object parent)
		{
			if (parent == null)
			{
				throw new System.ArgumentException("root is null");
			}
			return ((Wrapper) parent).getChildCount(parent);
		}
		
		//UPGRADE_NOTE: The equivalent of method 'javax.swing.tree.TreeModel.getIndexOfChild' is not an override method. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1143_3"'
		public virtual int getIndexOfChild(object parent, object child)
		{
			if (parent == null || child == null)
			{
				throw new System.ArgumentException("root or child is null");
			}
			return ((Wrapper) parent).getIndexOfChild(parent, child);
		}
		
		//UPGRADE_NOTE: The equivalent of method 'javax.swing.tree.TreeModel.getRoot' is not an override method. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1143_3"'
		public virtual object getRoot()
		{
			return root;
		}
		
		//UPGRADE_NOTE: The equivalent of method 'javax.swing.tree.TreeModel.isLeaf' is not an override method. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1143_3"'
		public virtual bool isLeaf(object node)
		{
			if (node == null)
			{
				throw new System.ArgumentException("node is null");
			}
			if (node is Wrapper)
			{
				return ((Wrapper) node).isLeaf(node);
			}
			return true;
		}
		
		//UPGRADE_NOTE: The equivalent of method 'javax.swing.tree.TreeModel.removeTreeModelListener' is not an override method. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1143_3"'
		//UPGRADE_TODO: Interface 'javax.swing.event.TreeModelListener' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095_3"'
		public virtual void  removeTreeModelListener(TreeModelListener l)
		{
		}
		
		//UPGRADE_NOTE: The equivalent of method 'javax.swing.tree.TreeModel.valueForPathChanged' is not an override method. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1143_3"'
		//UPGRADE_ISSUE: Class 'javax.swing.tree.TreePath' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000_javaxswingtreeTreePath_3"'
		public virtual void  valueForPathChanged(TreePath path, object newValue)
		{
			System.Console.Out.WriteLine("heh, who is calling this mystery method?");
		}
		static JTreeStringTemplateModel()
		{
		{
			classNameToWrapperMap["Antlr.StringTemplate.StringTemplate"] = typeof(StringTemplateWrapper);
			classNameToWrapperMap["Antlr.StringTemplate.Language.ASTExpr"] = typeof(ExprWrapper);
			classNameToWrapperMap["java.util.Hashtable"] = typeof(HashtableWrapper);
			classNameToWrapperMap["java.util.ArrayList"] = typeof(ListWrapper);
			classNameToWrapperMap["java.util.Vector"] = typeof(ListWrapper);
		}
		}
	}
}