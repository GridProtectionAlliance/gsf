//******************************************************************************************************
//  ErrorDialog.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/25/2008 - Pinal C. Patel
//       Generated original version of source code.
//  11/03/2008 - Pinal C. Patel
//       Edited code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Drawing;
using System.Security;
using System.Windows.Forms;

namespace GSF.Windows.ErrorManagement
{
    /// <summary>
    /// Represents a dialog box that can be used to display detailed exception information.
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

        private static void SizeBox(RichTextBox ctl)
        {
            Graphics g = null;

            try
            {
                // Note that the height is taken as MAXIMUM, so size the label for maximum desired height!
                g = Graphics.FromHwnd(ctl.Handle);
                SizeF objSizeF = g.MeasureString(ctl.Text, ctl.Font, new SizeF(ctl.Width, ctl.Height));
                ctl.Height = (int)objSizeF.Height + 5;
            }
            catch (SecurityException)
            {
                // Do nothing; we can't set control sizes without full trust.
            }
            finally
            {
                g?.Dispose();
            }
        }

        #endregion
    }
}
