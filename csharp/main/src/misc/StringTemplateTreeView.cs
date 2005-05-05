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
using antlr.stringtemplate;
namespace antlr.stringtemplate.misc
{
	
	/// <summary>This class visually illustrates a StringTemplate instance including
	/// the chunks (text + expressions) and the attributes table.  It correctly
	/// handles nested templates and so on.  A prototype really, but may prove
	/// useful for debugging StringTemplate applications.  Use it like this:
	/// 
	/// <pre>
	/// StringTemplate st = ...;
	/// StringTemplateTreeView viz =
	/// new StringTemplateTreeView("sample",st);
	/// viz.setVisible(true);
	/// </Pre>
	/// </summary>
	[Serializable]
	public class StringTemplateTreeView:System.Windows.Forms.Form
	{
		//UPGRADE_NOTE: Field 'EnclosingInstance' was added to class 'AnonymousClassWindowAdapter' to access its enclosing instance. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1019_3"'
		private class AnonymousClassWindowAdapter
		{
			public AnonymousClassWindowAdapter(StringTemplateTreeView enclosingInstance)
			{
				InitBlock(enclosingInstance);
			}
			private void  InitBlock(StringTemplateTreeView enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private StringTemplateTreeView enclosingInstance;
			public StringTemplateTreeView Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public void  windowClosing(System.Object event_sender, System.ComponentModel.CancelEventArgs e)
			{
				e.Cancel = true;
				//UPGRADE_TODO: Class 'java.awt.Frame' was converted to 'System.Windows.Forms.Form' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javaawtFrame_3"'
				System.Windows.Forms.Form f = (System.Windows.Forms.Form) event_sender;
				//UPGRADE_TODO: Method 'java.awt.Component.setVisible' was converted to 'System.Windows.Forms.Control.Visible' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javaawtComponentsetVisible_boolean_3"'
				//UPGRADE_TODO: 'System.Windows.Forms.Application.Run' must be called to start a main form. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1135_3"'
				f.Visible = false;
				f.Dispose();
				// System.exit(0);
			}
		}
		// The initial width and height of the frame
		internal const int WIDTH = 200;
		internal const int HEIGHT = 300;
		
		public StringTemplateTreeView(System.String label, StringTemplate st):base()
		{
			this.Text = label;
			
			JTreeStringTemplatePanel tp = new JTreeStringTemplatePanel(new JTreeStringTemplateModel(st), null);
			//UPGRADE_TODO: Method 'javax.swing.JFrame.getContentPane' was converted to 'System.Windows.Forms.Form' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javaxswingJFramegetContentPane_3"'
			System.Windows.Forms.Control content = ((System.Windows.Forms.ContainerControl) this);
			//UPGRADE_TODO: Method 'java.awt.Container.add' was converted to 'System.Windows.Forms.ContainerControl.Controls.Add' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javaawtContaineradd_javaawtComponent_javalangObject_3"'
			content.Controls.Add(tp);
			tp.Dock = System.Windows.Forms.DockStyle.Fill;
			tp.BringToFront();
			//UPGRADE_NOTE: Some methods of the 'java.awt.event.WindowListener' class are not used in the .NET Framework. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1308_3"'
			Closing += new System.ComponentModel.CancelEventHandler(new AnonymousClassWindowAdapter(this).windowClosing);
			//UPGRADE_TODO: Method 'java.awt.Component.setSize' was converted to 'System.Windows.Forms.Control.Size' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javaawtComponentsetSize_int_int_3"'
			Size = new System.Drawing.Size(WIDTH, HEIGHT);
		}
		
		[STAThread]
		public static void  Main(System.String[] args)
		{
			StringTemplateGroup group = new StringTemplateGroup("dummy");
			StringTemplate bold = group.defineTemplate("bold", "<b>$attr$</b>");
			StringTemplate banner = group.defineTemplate("banner", "the banner");
			StringTemplate st = new StringTemplate(group, "<html>\n" + "$banner(a=b)$" + "<p><b>$name$:$email$</b>" + "$if(member)$<i>$fontTag$member</font></i>$endif$");
			st.setAttribute("name", "Terence");
			st.setAttribute("name", "Tom");
			st.setAttribute("email", "parrt@cs.usfca.edu");
			st.setAttribute("templateAttr", bold);
			StringTemplateTreeView frame = new StringTemplateTreeView("StringTemplate JTree Example", st);
			//UPGRADE_TODO: Method 'java.awt.Component.setVisible' was converted to 'System.Windows.Forms.Control.Visible' which has a different behavior. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1073_javaawtComponentsetVisible_boolean_3"'
			//UPGRADE_TODO: 'System.Windows.Forms.Application.Run' must be called to start a main form. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1135_3"'
			frame.Visible = true;
		}
	}
}