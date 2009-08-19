//*******************************************************************************************************
//  ErrorDialog.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR 2W-C
//       Phone: 423-751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/25/2008 - Pinal C. Patel
//       Generated original version of source code.
//  11/03/2008 - Pinal C. Patel
//       Edited code comments.
//
//*******************************************************************************************************

using System;
using System.Drawing;
using System.Windows.Forms;

namespace TVA.ErrorManagement
{
    /// <summary>
    /// Represents a dialog box that can be used to display detailed exception inmormation.
    /// </summary>
    /// <seealso cref="ErrorLogger"/>
    public partial class ErrorDialog : Form
    {
        #region [ Members ]

        // Constants
        private const int Spacing = 10;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorDialog"/> class.
        /// </summary>
        public ErrorDialog()
        {
            InitializeComponent();
        }

        #endregion

        #region [ Methods ]

        private void ErrorDialog_Load(object sender, EventArgs e)
        {
            this.TopMost = true;
            this.TopMost = false;

            // More >> has to be expanded.
            RichTextBoxMoreInfo.Anchor = AnchorStyles.None;
            RichTextBoxMoreInfo.Visible = false;

            // Size the label height to accommodate of text in them.
            SizeBox(RichTextBoxScope);
            SizeBox(RichTextBoxAction);
            SizeBox(RichTextBoxError);

            // Now shift everything up.
            LabelScope.Top = RichTextBoxError.Top + RichTextBoxError.Height + Spacing;
            RichTextBoxScope.Top = LabelScope.Top + LabelScope.Height + Spacing;

            LabelAction.Top = RichTextBoxScope.Top + RichTextBoxScope.Height + Spacing;
            RichTextBoxAction.Top = LabelAction.Top + LabelAction.Height + Spacing;

            LabelMoreInfo.Top = RichTextBoxAction.Top + RichTextBoxAction.Height + Spacing;
            ButtonMore.Top = LabelMoreInfo.Top - 3;

            this.Height = ButtonMore.Top + ButtonMore.Height + Spacing + 45;

            this.CenterToScreen();
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            this.Close();
            this.DialogResult = DialogResult.OK;
        }

        private void ButtonMore_Click(object sender, EventArgs e)
        {
            if (ButtonMore.Text == ">>")
            {
                this.Height = this.Height + 300;
                RichTextBoxMoreInfo.Location = new Point(LabelMoreInfo.Left, LabelMoreInfo.Top + LabelMoreInfo.Height + Spacing);
                RichTextBoxMoreInfo.Height = this.ClientSize.Height - RichTextBoxMoreInfo.Top - 45;
                RichTextBoxMoreInfo.Width = this.ClientSize.Width - 2 * Spacing;
                RichTextBoxMoreInfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
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
                RichTextBoxMoreInfo.Anchor = AnchorStyles.None;
                this.ResumeLayout();
            }
        }

        private static void SizeBox(System.Windows.Forms.RichTextBox ctl)
        {
            Graphics g = null;
            try
            {
                // Note that the height is taken as MAXIMUM, so size the label for maximum desired height!
                g = Graphics.FromHwnd(ctl.Handle);
                SizeF objSizeF = g.MeasureString(ctl.Text, ctl.Font, new SizeF(ctl.Width, ctl.Height));
                g.Dispose();
                ctl.Height = (int)objSizeF.Height + 5;
            }
            catch (System.Security.SecurityException)
            {
                // Do nothing; we can't set control sizes without full trust.
            }
            finally
            {
                if (g != null)
                {
                    g.Dispose();
                }
            }
        }

        #endregion
    }
}
