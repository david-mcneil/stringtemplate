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
		private abstract class StringTemplateTreePanelNode : TreeNode
		private class StringTemplateTreeNode : StringTemplateTreePanelNode
		private class IDictionaryTreeNode : StringTemplateTreePanelNode
		private class DictionaryEntryTreeNode : StringTemplateTreePanelNode
		private class IListTreeNode : StringTemplateTreePanelNode
		private class ExprTreeNode : StringTemplateTreePanelNode
		#endregion

		/// <summary> 
		private TreeView tree;
		

		private StringTemplatePanel()
		{
			// This call is required by the Windows.Forms Form Designer.

		public StringTemplatePanel(TreeViewEventHandler afterSelectHandler, StringTemplate st) : this()
		{
			//tree.AfterSelect += afterSelectHandler;

		/// <summary> 
		internal static void tree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		/// <summary> 
		#endregion
}