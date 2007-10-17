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
	using StringTemplate = Antlr.StringTemplate.StringTemplate;
	
	/// <summary>
	/// This class visually illustrates a StringTemplate instance including
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
	public class StringTemplateTreeView : System.Windows.Forms.Form
	{
		// The initial width and height of the frame
		internal const int WIDTH = 500;
		internal const int HEIGHT = 400;
		
		/// <summary>		/// Required designer variable.		/// </summary>		private System.ComponentModel.Container components = null;		#region Constructors		private StringTemplateTreeView()
		{			//			// Required for Windows Form Designer support			//			InitializeComponent();			//			// TODO: Add any constructor code after InitializeComponent call			//			this.Size = new System.Drawing.Size(WIDTH, HEIGHT);			Application.ApplicationExit += new EventHandler(Form_OnExit);		}
		public StringTemplateTreeView(string text, StringTemplate st) : this()
		{
			this.Text = text + " [**ALPHA release (wrong/incomplete info)**]";
			
			StringTemplatePanel stPanel = new StringTemplatePanel(null, st);

			this.Controls.Add(stPanel);			stPanel.Location	= new System.Drawing.Point(5, 5);			stPanel.Dock		= DockStyle.Fill;			stPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;		}

		#endregion
		#region Windows Form Designer generated code
		/// <summary>		/// Required method for Designer support - do not modify		/// the contents of this method with the code editor.		/// </summary>		private void InitializeComponent()		{			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);			this.ClientSize = new System.Drawing.Size(292, 273);			this.Name = "StringTemplateFrame";			this.Text = "StringTemplateFrame";		}		#endregion
		#region Event Handlers
		private void Form_OnExit(object sender, EventArgs e)		{			this.Visible = false;			this.Dispose();		}
		#endregion

		/// <summary>		/// Clean up any resources being used.		/// </summary>		protected override void Dispose( bool disposing )
		{			if( disposing )			{				if(components != null)				{					components.Dispose();				}			}			base.Dispose( disposing );		}	}
}