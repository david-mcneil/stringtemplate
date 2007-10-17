/*
[Adapted from BSD licence]
Copyright (c) 2005-2006 Kunle Odutola
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
THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/


namespace Antlr.StringTemplate.Viewer
{
	using System;
	using System.Windows.Forms;
	using System.Collections;
	using StringTemplate				= Antlr.StringTemplate.StringTemplate;
	using Expr							= Antlr.StringTemplate.Language.Expr;
	using StringRef						= Antlr.StringTemplate.Language.StringRef;
	using ASTExpr						= Antlr.StringTemplate.Language.ASTExpr;
	using ActionEvaluatorTokenTypes		= Antlr.StringTemplate.Language.ActionEvaluatorTokenTypes;
	using ConditionalExpr				= Antlr.StringTemplate.Language.ConditionalExpr;
	
	[Serializable]
	public class StringTemplatePanel : System.Windows.Forms.Panel
	{
		#region Helper Classes and Types
		private class NodeFactory
		{
			public static StringTemplateTreePanelNode CreateNode(object data, string nodeLabel)
			{
				StringTemplateTreePanelNode node = CreateNode(data);
				node.Text = nodeLabel;
				return node;
			}

			public static StringTemplateTreePanelNode CreateNode(object data)
			{
				if (data is StringTemplate)
					return new StringTemplateTreeNode((StringTemplate)data);
				else if (data is Expr)
					return new ExprTreeNode((Expr)data);
				else if (data is DictionaryEntry)
					return new DictionaryEntryTreeNode((DictionaryEntry)data);
				else if (data is IDictionary)
					return new IDictionaryTreeNode((IDictionary)data);
				else if (data is IList)
					return new IListTreeNode((IList)data);
				return null; //new TreeNode("<invalid node type>");
			}
		}
		private abstract class StringTemplateTreePanelNode : TreeNode		{			private bool isLoaded_ = false;			public bool IsLoaded			{				get { return isLoaded_; }			}			public void Load()			{				RemoveAllChildren();				LoadChildren();				isLoaded_ = true;			}			protected virtual void RemoveAllChildren()			{				Nodes.Clear();			}			protected abstract void LoadChildren();		}
		private class StringTemplateTreeNode : StringTemplateTreePanelNode		{			private StringTemplate st_;			public StringTemplateTreeNode(StringTemplate st)			{				st_ = st;				this.Text = st.Name;				this.Nodes.Add("Loading.....");			}			protected override void LoadChildren()			{				if (st_ != null)				{					if (st_.Attributes == null)						Nodes.Add("Attributes");					else						Nodes.Add(NodeFactory.CreateNode(st_.Attributes));					if (st_.Chunks != null)					{						foreach (Expr expr in st_.Chunks)						{							Nodes.Add(NodeFactory.CreateNode(expr));						}					}				}			}		}
		private class IDictionaryTreeNode : StringTemplateTreePanelNode		{			private  IDictionary	dict_;			public IDictionaryTreeNode(IDictionary dict) : this(dict, "Attributes")			{			}			public IDictionaryTreeNode(IDictionary dict, string nodeText)			{				dict_ = dict;				this.Text = nodeText;				this.Nodes.Add("Loading.....");			}			protected override void LoadChildren()			{				if (dict_ != null)				{					foreach (DictionaryEntry entry in dict_)					{						if (entry.Value is IDictionary)							Nodes.Add(NodeFactory.CreateNode(entry.Value, entry.Key.ToString()));						else                            Nodes.Add(NodeFactory.CreateNode(entry));					}				}			}		}
		private class DictionaryEntryTreeNode : StringTemplateTreePanelNode		{			private  DictionaryEntry entry_;			public DictionaryEntryTreeNode(DictionaryEntry entry)			{				entry_ = entry;				this.Text = entry.Key.ToString();				this.Nodes.Add("Loading.....");			}			protected override void LoadChildren()			{				if (entry_.Value != null)				{					StringTemplateTreePanelNode node;					if (entry_.Value is IList)					{						IList list = (IList)entry_.Value;						foreach (object item in list)						{							node = NodeFactory.CreateNode(item);							if (node == null)								Nodes.Add(item.ToString());							else								Nodes.Add(node);						}					}					else					{						node = NodeFactory.CreateNode(entry_.Value);						if (node == null)							Nodes.Add(entry_.Value.ToString());						else							Nodes.Add(node);					}				}			}		}
		private class IListTreeNode : StringTemplateTreePanelNode		{			private  IList	list_;			public IListTreeNode(IList list)			{				list_ = list;				this.Text = "list";				this.Nodes.Add("Loading.....");			}			protected override void LoadChildren()			{				if (list_ != null)				{				}			}		}
		private class ExprTreeNode : StringTemplateTreePanelNode		{			private  Expr	expr_;			public ExprTreeNode(Expr expr)			{				expr_ = expr;				if (expr is StringRef)				{					this.Text = expr.ToString();				}				else if (expr is ASTExpr)				{					antlr.collections.AST ast = ((ASTExpr)expr).AST;					if (ast.Type == ActionEvaluatorTokenTypes.INCLUDE)						this.Text = "$include$";					else						this.Text = "$" + ast.ToStringList().Trim() + "$";					this.Nodes.Add("Loading.....");				}				else				{					this.Text = "<invalid node type>";				}			}			protected override void LoadChildren()			{				if (expr_ != null)				{				}			}		}
		#endregion

		/// <summary> 		/// Required designer variable.		/// </summary>		private System.ComponentModel.Container components = null;
		private TreeView tree;
		

		private StringTemplatePanel()
		{
			// This call is required by the Windows.Forms Form Designer.			InitializeComponent();			// TODO: Add any initialization after the InitForm call		}

		public StringTemplatePanel(TreeViewEventHandler afterSelectHandler, StringTemplate st) : this()
		{
			//tree.AfterSelect += afterSelectHandler;			tree.BeforeExpand += new TreeViewCancelEventHandler(tree_BeforeExpand);			tree.Nodes.Add(NodeFactory.CreateNode(st));		}

		/// <summary> 		/// Clean up any resources being used.		/// </summary>		protected override void Dispose( bool disposing )		{			if( disposing )			{				if(components != null)				{					components.Dispose();				}			}			base.Dispose( disposing );		}
		internal static void tree_BeforeExpand(object sender, TreeViewCancelEventArgs e)		{			StringTemplateTreePanelNode	node   = (StringTemplateTreePanelNode)e.Node;			if (!node.IsLoaded)			{				node.Load();			}		}		#region Component Designer generated code
		/// <summary> 		/// Required method for Designer support - do not modify 		/// the contents of this method with the code editor.		/// </summary>		private void InitializeComponent()		{			this.tree = new System.Windows.Forms.TreeView();			this.SuspendLayout();			// 			// tree			// 			this.tree.Dock = System.Windows.Forms.DockStyle.Fill;			this.tree.ImageIndex = -1;			this.tree.ImeMode = System.Windows.Forms.ImeMode.NoControl;			this.tree.Location = new System.Drawing.Point(5, 5);			this.tree.Name = "tree";			this.tree.SelectedImageIndex = -1;			this.tree.Size = new System.Drawing.Size(140, 140);			this.tree.TabIndex = 0;			// 			// StringTemplatePanel			// 			this.Controls.AddRange(new System.Windows.Forms.Control[] { this.tree });			this.DockPadding.All = 5;			this.Name = "StringTemplatePanel";			this.ResumeLayout(false);		}
		#endregion	}
}