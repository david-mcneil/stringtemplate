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
namespace Antlr.StringTemplate.misc
{
	
	[Serializable]
	public class JTreeStringTemplatePanel:System.Windows.Forms.Panel
	{
		//UPGRADE_TODO: Class 'javax.swing.JTree' was converted to 'System.Windows.Forms.TreeView' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_3"'
		internal System.Windows.Forms.TreeView tree;
		
		//UPGRADE_TODO: Interface 'javax.swing.tree.TreeModel' was converted to 'System.Windows.Forms.TreeNode' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_3"'
		//UPGRADE_TODO: Interface 'javax.swing.event.TreeSelectionListener' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095_3"'
		public JTreeStringTemplatePanel(System.Windows.Forms.TreeNode tm, TreeSelectionListener listener)
		{
			// use a layout that will stretch tree to panel size
			//UPGRADE_ISSUE: Method 'java.awt.Container.setLayout' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000_javaawtContainersetLayout_javaawtLayoutManager_3"'
			//UPGRADE_ISSUE: Constructor 'java.awt.BorderLayout.BorderLayout' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000_javaawtBorderLayout_3"'
			/*
			setLayout(new BorderLayout());*/
			
			// Create tree
			tree = SupportClass.TreeSupport.CreateTreeView(tm);
			
			// Change line style
			//UPGRADE_ISSUE: Method 'javax.swing.JComponent.putClientProperty' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000_javaxswingJComponentputClientProperty_javalangObject_javalangObject_3"'
			tree.putClientProperty("JTree.lineStyle", "Angled");
			
			// Add TreeSelectionListener
			if (listener != null)
				tree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(listener.valueChanged);
			
			// Put tree in a scrollable pane's viewport
			//UPGRADE_TODO: Class 'javax.swing.JScrollPane' was converted to 'System.Windows.Forms.ScrollableControl' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_3"'
			//UPGRADE_TODO: Constructor 'javax.swing.JScrollPane.JScrollPane' was converted to 'System.Windows.Forms.ScrollableControl.ScrollableControl' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javaxswingJScrollPaneJScrollPane_3"'
			System.Windows.Forms.ScrollableControl temp_scrollablecontrol;
			temp_scrollablecontrol = new System.Windows.Forms.ScrollableControl();
			temp_scrollablecontrol.AutoScroll = true;
			System.Windows.Forms.ScrollableControl sp = temp_scrollablecontrol;
			//UPGRADE_ISSUE: Method 'javax.swing.JScrollPane.getViewport' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000_javaxswingJScrollPanegetViewport_3"'
			//UPGRADE_TODO: Method 'java.awt.Container.add' was converted to 'System.Windows.Forms.ContainerControl.Controls.Add' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javaawtContaineradd_javaawtComponent_3"'
			sp.getViewport().Controls.Add(tree);
			
			//UPGRADE_TODO: Method 'java.awt.Container.add' was converted to 'System.Windows.Forms.ContainerControl.Controls.Add' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javaawtContaineradd_javaawtComponent_javalangObject_3"'
			Controls.Add(sp);
			sp.Dock = System.Windows.Forms.DockStyle.Fill;
			sp.BringToFront();
		}
	}
}