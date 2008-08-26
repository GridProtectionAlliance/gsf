using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Drawing;

// PCP: 04/13/2007



namespace TVA
{
	namespace ErrorManagement
	{
		
		public partial class GelDialog
		{
			public GelDialog()
			{
				InitializeComponent();
			}
			
			private const int Spacing = 10;
			
			private void GelDialog_Load(System.Object sender, System.EventArgs e)
			{
				
				this.TopMost = true;
				this.TopMost = false;
				
				//-- More >> has to be expanded
				RichTextBoxMoreInfo.Anchor = System.Windows.Forms.AnchorStyles.None;
				RichTextBoxMoreInfo.Visible = false;
				
				//-- size the labels' height to accommodate the amount of text in them
				SizeBox(RichTextBoxScope);
				SizeBox(RichTextBoxAction);
				SizeBox(RichTextBoxError);
				
				//-- now shift everything up
				LabelScope.Top = RichTextBoxError.Top + RichTextBoxError.Height + Spacing;
				RichTextBoxScope.Top = LabelScope.Top + LabelScope.Height + Spacing;
				
				LabelAction.Top = RichTextBoxScope.Top + RichTextBoxScope.Height + Spacing;
				RichTextBoxAction.Top = LabelAction.Top + LabelAction.Height + Spacing;
				
				LabelMoreInfo.Top = RichTextBoxAction.Top + RichTextBoxAction.Height + Spacing;
				ButtonMore.Top = LabelMoreInfo.Top - 3;
				
				this.Height = ButtonMore.Top + ButtonMore.Height + Spacing + 45;
				
				this.CenterToScreen();
				
			}
			
			private void ButtonMore_Click(System.Object sender, System.EventArgs e)
			{
				
				if (ButtonMore.Text == ">>")
				{
					this.Height = this.Height + 300;
					RichTextBoxMoreInfo.Location = new System.Drawing.Point(LabelMoreInfo.Left, LabelMoreInfo.Top + LabelMoreInfo.Height + Spacing);
					RichTextBoxMoreInfo.Height = this.ClientSize.Height - RichTextBoxMoreInfo.Top - 45;
					RichTextBoxMoreInfo.Width = this.ClientSize.Width - 2 * Spacing;
					RichTextBoxMoreInfo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
					RichTextBoxMoreInfo.Visible = true;
					ButtonOK.Focus();
					ButtonMore.Text = "<<";
				}
				else
				{
					this.SuspendLayout();
					ButtonMore.Text = ">>";
					this.Height = ButtonMore.Top + ButtonMore.Height + Spacing + 45;
					RichTextBoxMoreInfo.Visible = false;
					RichTextBoxMoreInfo.Anchor = System.Windows.Forms.AnchorStyles.None;
					this.ResumeLayout();
				}
				
			}
			
			private void SizeBox(System.Windows.Forms.RichTextBox ctl)
			{
				
				Graphics g;
				try
				{
					//-- note that the height is taken as MAXIMUM, so size the label for maximum desired height!
					g = Graphics.FromHwnd(ctl.Handle);
					SizeF objSizeF = g.MeasureString(ctl.Text, ctl.Font, new SizeF(ctl.Width, ctl.Height));
					g.Dispose();
					ctl.Height = Convert.ToInt32(objSizeF.Height) + 5;
				}
				catch (System.Security.SecurityException)
				{
					//-- do nothing; we can't set control sizes without full trust
				}
				finally
				{
					if (g != null)
					{
						g.Dispose();
					}
				}
				
			}
			
			private void ButtonOK_Click(System.Object sender, System.EventArgs e)
			{
				
				this.Close();
				this.DialogResult = System.Windows.Forms.DialogResult.OK;
				
			}
			
		}
		
	}
}
