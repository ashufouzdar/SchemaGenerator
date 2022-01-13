#region Copyright © 2008 Ashu Fouzdar. All rights reserved.
/*
Copyright © 2008 Ashu Fouzdar. All rights reserved.

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

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR
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
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Text;

namespace aicl.SchemaGenerator
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		PrintDocument doc=null;
		StringBuilder sb=null;
		Schema sch=null;
		private string [] lines = null;
		int LineCtr=0;
		bool success=false;

		public MainForm()
		{
			InitializeComponent();
			doc=new PrintDocument();
			doc.PrintPage+= new PrintPageEventHandler(this.PrntPage);
			BGWrkr.RunWorkerCompleted+=new System.ComponentModel.RunWorkerCompletedEventHandler(this.BGWrkrWorkCompleted);
		}

		
		private void PrntPage(object sender, PrintPageEventArgs ev)
		{
			float linesPerPage = 0;
			float yPos = 0;
			int count = 0;
			float leftMargin = ev.MarginBounds.Left;
			float topMargin = ev.MarginBounds.Top;
			Font printFont = new Font("Tahoma", 10);
			string line = null;

			// Calculate the number of lines per page.
			linesPerPage = ev.MarginBounds.Height /
				printFont.GetHeight(ev.Graphics);

			// Print each line of the file.
			while(count < linesPerPage  && this.LineCtr < lines.Length )
			{
				line=lines[LineCtr];
				yPos = topMargin + (count *
				                    printFont.GetHeight(ev.Graphics));
				ev.Graphics.DrawString(line, printFont, Brushes.Black,
				                       leftMargin, yPos/*, new StringFormat()*/);
				count++;
				LineCtr++;
			}

			// If more lines exist, print another page.
			if(line != null)
			{
				ev.HasMorePages = true;
			}
			else
			{
				ev.HasMorePages = false;
				this.LineCtr=0;
			}
			

		}

		
		void BtnGenerateClick(object sender, EventArgs e)
		{
			sch=null;
			success=false;
			
			if (this.txtServername.Text != string.Empty && this.txtDatabaseName.Text != string.Empty && this.txtUsername.Text != string.Empty && this.txtPassword.Text != string.Empty) {
				sch=new Schema(this.txtServername.Text,this.txtDatabaseName.Text,this.txtUsername.Text,this.txtPassword.Text);
			}
			else if (this.txtServername.Text != string.Empty && this.txtDatabaseName.Text != string.Empty) {
				sch=new Schema(this.txtServername.Text,this.txtDatabaseName.Text);
			}
			else if (this.txtDatabaseName.Text != string.Empty) {
				sch=new Schema(this.txtDatabaseName.Text);
			}
			else {
				MessageBox.Show("Please provide database name ","Required",MessageBoxButtons.OK,MessageBoxIcon.Warning);
				sch=null;
			}
			this.picProcessing.Visible=true;
			this.Cursor=Cursors.WaitCursor;
			this.BGWrkr.RunWorkerAsync();
		}
		
		void BGWrkrDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			try {
				if (sch != null ){
					this.LineCtr=0;
					sch.DoSort=this.chkSort.Checked;
					this.sb=sch.GetDBSchema();
					if (sb != null) {
						this.lines=sb.ToString().Split('\n');
						this.LineCtr=0;
					}
				}
				success=true;
			} catch (Exception ex) {
				MessageBox.Show(ex.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Warning);
				success=false;
			}
			
			
		}
		
		void BGWrkrWorkCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
		{
			this.picProcessing.Visible=false;
			this.Cursor=Cursors.Arrow;
			if (success)
			{
				this.previewDlg.Document=this.doc;
				this.previewDlg.ShowDialog();
			}
		}
	}
}
