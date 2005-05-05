/*
[Adapted from BSD licence]
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
using AST = antlr.collections.AST;
using StringTemplate = antlr.stringtemplate.StringTemplate;
using StringTemplateGroup = antlr.stringtemplate.StringTemplateGroup;
using antlr.stringtemplate.language;
namespace antlr.stringtemplate.misc
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
			public abstract int getChildCount(System.Object parent);
			public abstract int getIndexOfChild(System.Object parent, System.Object child);
			public abstract System.Object getChild(System.Object parent, int index);
			public abstract System.Object getWrappedObject();
			public virtual bool isLeaf(System.Object node)
			{
				return true;
			}
		}
		
		internal class StringTemplateWrapper:Wrapper
		{
			internal StringTemplate st = null;
			public StringTemplateWrapper(System.Object o)
			{
				this.st = (StringTemplate) o;
			}
			public override System.Object getWrappedObject()
			{
				return getStringTemplate();
			}
			public virtual StringTemplate getStringTemplate()
			{
				return st;
			}
			public override System.Object getChild(System.Object parent, int index)
			{
				StringTemplate st = ((StringTemplateWrapper) parent).getStringTemplate();
				if (index == 0)
				{
					// System.out.println("chunk type index "+index+" is attributes");
					// return attributes
					return new HashtableWrapper(st.getAttributes());
				}
				Expr chunk = (Expr) st.getChunks()[index - 1];
				// System.out.println("chunk type index "+index+" is "+chunk.getClass().getName());
				if (chunk is StringRef)
				{
					return chunk;
				}
				return new ExprWrapper(chunk);
			}
			public override int getChildCount(System.Object parent)
			{
				return st.getChunks().Count + 1; // extra one is attribute list
			}
			public override int getIndexOfChild(System.Object parent, System.Object child)
			{
				if (child is Wrapper)
				{
					child = ((Wrapper) child).getWrappedObject();
				}
				int index = st.getChunks().IndexOf(child) + 1;
				// System.out.println("index of "+child+" is "+index);
				return index;
			}
			public override bool isLeaf(System.Object node)
			{
				return false;
			}
			public override System.String ToString()
			{
				if (st == null)
				{
					return "<invalid template>";
				}
				return st.getName();
			}
		}
		
		internal class ExprWrapper:Wrapper
		{
			internal Expr expr = null;
			public ExprWrapper(System.Object o)
			{
				this.expr = (Expr) o;
			}
			public virtual Expr getExpr()
			{
				return expr;
			}
			public override System.Object getWrappedObject()
			{
				return expr;
			}
			public override System.Object getChild(System.Object parent, int index)
			{
				Expr expr = ((ExprWrapper) parent).getExpr();
				if (expr is ConditionalExpr)
				{
					// System.out.println("return wrapped subtemplate");
					return new StringTemplateWrapper(((ConditionalExpr) expr).getSubtemplate());
				}
				if (expr is ASTExpr)
				{
					ASTExpr astExpr = (ASTExpr) expr;
					AST root = astExpr.getAST();
					if (root.getType() == ActionEvaluatorTokenTypes.INCLUDE)
					{
						switch (index)
						{
							
							case 0: 
								return root.getFirstChild().getNextSibling().toStringList();
							
							case 1: 
								System.String templateName = root.getFirstChild().getText();
								StringTemplate enclosingST = expr.getEnclosingTemplate();
								StringTemplateGroup group = enclosingST.getGroup();
								StringTemplate embedded = group.getEmbeddedInstanceOf(enclosingST, templateName);
								return new StringTemplateWrapper(embedded);
							}
					}
				}
				return "<invalid>";
			}
			public override int getChildCount(System.Object parent)
			{
				if (expr is ConditionalExpr)
				{
					return 1;
				}
				AST tree = ((ASTExpr) expr).getAST();
				if (tree.getType() == ActionEvaluatorTokenTypes.INCLUDE)
				{
					return 2; // one for args and one for template
				}
				return 0;
			}
			public override int getIndexOfChild(System.Object parent, System.Object child)
			{
				//System.out.println("Get index of child of "+parent.getClass().getName());
				if (expr is ConditionalExpr)
				{
					return 0;
				}
				return - 1;
			}
			public override bool isLeaf(System.Object node)
			{
				if (expr is ConditionalExpr)
				{
					return false;
				}
				if (expr is ASTExpr)
				{
					AST tree = ((ASTExpr) expr).getAST();
					if (tree.getType() == ActionEvaluatorTokenTypes.INCLUDE)
					{
						return false;
					}
				}
				return true;
			}
			/// <summary>Display different things depending on the ASTExpr type </summary>
			public override System.String ToString()
			{
				if (expr is ASTExpr)
				{
					AST tree = ((ASTExpr) expr).getAST();
					if (tree.getType() == ActionEvaluatorTokenTypes.INCLUDE)
					{
						return "$include$";
					}
					return "$" + ((ASTExpr) expr).getAST().toStringList() + "$";
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
			internal System.Collections.IList v = null;
			public ListWrapper(System.Object o)
			{
				v = (System.Collections.IList) o;
			}
			public override int getChildCount(System.Object parent)
			{
				return v.Count;
			}
			
			public override int getIndexOfChild(System.Object parent, System.Object child)
			{
				if (child is Wrapper)
				{
					child = ((Wrapper) child).getWrappedObject();
				}
				return v.IndexOf(child);
			}
			
			public override System.Object getChild(System.Object parent, int index)
			{
				return v[index];
			}
			
			public override System.Object getWrappedObject()
			{
				return v;
			}
			
			public override bool isLeaf(System.Object node)
			{
				return false;
			}
		}
		
		/// <summary>Wrap an entry in a map so that name appears as the root and the value
		/// appears as the child or children.
		/// </summary>
		internal class MapEntryWrapper:Wrapper
		{
			internal System.Object key, value_Renamed;
			public MapEntryWrapper(System.Object key, System.Object value_Renamed)
			{
				this.key = key;
				this.value_Renamed = value_Renamed;
			}
			public override System.Object getWrappedObject()
			{
				return antlr.stringtemplate.misc.JTreeStringTemplateModel.wrap(value_Renamed);
			}
			public override int getChildCount(System.Object parent)
			{
				if (value_Renamed is Wrapper)
				{
					return ((Wrapper) value_Renamed).getChildCount(value_Renamed);
				}
				return 1;
			}
			
			public override int getIndexOfChild(System.Object parent, System.Object child)
			{
				if (value_Renamed is Wrapper)
				{
					return ((Wrapper) value_Renamed).getIndexOfChild(value_Renamed, child);
				}
				return 0;
			}
			
			public override System.Object getChild(System.Object parent, int index)
			{
				if (value_Renamed is Wrapper)
				{
					return ((Wrapper) value_Renamed).getChild(value_Renamed, index);
				}
				return value_Renamed;
			}
			
			public override bool isLeaf(System.Object node)
			{
				return false;
			}
			
			public override System.String ToString()
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Object.toString' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043_3"'
				return key.ToString();
			}
		}
		
		internal class HashtableWrapper:Wrapper
		{
			internal System.Collections.Hashtable table;
			public HashtableWrapper(System.Object o)
			{
				this.table = (System.Collections.Hashtable) o;
			}
			public override System.Object getWrappedObject()
			{
				return table;
			}
			public override System.Object getChild(System.Object parent, int index)
			{
				System.Collections.IList attributes = getTableAsListOfKeys();
				System.String key = (System.String) attributes[index];
				System.Object attr = table[key];
				System.Object wrappedAttr = antlr.stringtemplate.misc.JTreeStringTemplateModel.wrap(attr);
				return new MapEntryWrapper(key, wrappedAttr);
			}
			public override int getChildCount(System.Object parent)
			{
				System.Collections.IList attributes = getTableAsListOfKeys();
				return attributes.Count;
			}
			public override int getIndexOfChild(System.Object parent, System.Object child)
			{
				System.Collections.IList attributes = getTableAsListOfKeys();
				return attributes.IndexOf(child);
			}
			public override bool isLeaf(System.Object node)
			{
				return false;
			}
			public override System.String ToString()
			{
				return "attributes";
			}
			private System.Collections.IList getTableAsListOfKeys()
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
					System.String attributeName = (System.String) keys.Current;
					v.Add(attributeName);
				}
				return v;
			}
		}
		
		internal Wrapper root = null;
		
		/// <summary>Get a wrapper object by adding "Wrapper" to class name.
		/// If not present, return the object.
		/// </summary>
		public static System.Object wrap(System.Object o)
		{
			System.Object wrappedObject = o;
			System.Type wrapperClass = null;
			try
			{
				//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Class.getName' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043_3"'
				wrapperClass = (System.Type) classNameToWrapperMap[o.GetType().FullName];
				System.Reflection.ConstructorInfo ctor = wrapperClass.GetConstructor(new System.Type[]{typeof(System.Object)});
				wrappedObject = ctor.Invoke(new System.Object[]{o});
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
		public virtual System.Object getChild(System.Object parent, int index)
		{
			//System.out.println("Get index "+index+" of "+parent.toString()+":"+parent.getClass().getName());
			if (parent == null)
			{
				return null;
			}
			return ((Wrapper) parent).getChild(parent, index);
		}
		
		//UPGRADE_NOTE: The equivalent of method 'javax.swing.tree.TreeModel.getChildCount' is not an override method. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1143_3"'
		public virtual int getChildCount(System.Object parent)
		{
			if (parent == null)
			{
				throw new System.ArgumentException("root is null");
			}
			return ((Wrapper) parent).getChildCount(parent);
		}
		
		//UPGRADE_NOTE: The equivalent of method 'javax.swing.tree.TreeModel.getIndexOfChild' is not an override method. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1143_3"'
		public virtual int getIndexOfChild(System.Object parent, System.Object child)
		{
			if (parent == null || child == null)
			{
				throw new System.ArgumentException("root or child is null");
			}
			return ((Wrapper) parent).getIndexOfChild(parent, child);
		}
		
		//UPGRADE_NOTE: The equivalent of method 'javax.swing.tree.TreeModel.getRoot' is not an override method. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1143_3"'
		public virtual System.Object getRoot()
		{
			return root;
		}
		
		//UPGRADE_NOTE: The equivalent of method 'javax.swing.tree.TreeModel.isLeaf' is not an override method. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1143_3"'
		public virtual bool isLeaf(System.Object node)
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
		public virtual void  valueForPathChanged(TreePath path, System.Object newValue)
		{
			System.Console.Out.WriteLine("heh, who is calling this mystery method?");
		}
		static JTreeStringTemplateModel()
		{
			{
				classNameToWrapperMap["antlr.stringtemplate.StringTemplate"] = typeof(StringTemplateWrapper);
				classNameToWrapperMap["antlr.stringtemplate.language.ASTExpr"] = typeof(ExprWrapper);
				classNameToWrapperMap["java.util.Hashtable"] = typeof(HashtableWrapper);
				classNameToWrapperMap["java.util.ArrayList"] = typeof(ListWrapper);
				classNameToWrapperMap["java.util.Vector"] = typeof(ListWrapper);
			}
		}
	}
}