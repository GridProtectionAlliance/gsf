//*******************************************************************************************************
//  Main.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/05/2009 - Pinal C. Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.Windows.Forms;
using TVA.Reflection;
using TVA.Security.Cryptography;

namespace ConfigCrypter
{
    public partial class Main : Form
    {
        #region [ Members ]

        // Constants
        private const string DefaultCryptoKey = "0679d9ae-aca5-4702-a3f5-604415096987";
        private const string InvalidInputPrompt = "[Input string is not in valid format]";

        #endregion

        #region [ Constructors ]

        public Main()
        {
            InitializeComponent();
        }

        #endregion

        #region [ Methods ]

        private void Main_Load(object sender, EventArgs e)
        {
            // Set the window title to application name and version.
            this.Text = string.Format(this.Text, AssemblyInfo.EntryAssembly.Product, AssemblyInfo.EntryAssembly.Version.ToString(2));
        }

        private void RadioButtonEncrypt_CheckedChanged(object sender, EventArgs e)
        {
            TextBoxInput.Clear();
            TextBoxOutput.Clear();
        }

        private void TextBoxKey_TextChanged(object sender, EventArgs e)
        {
            PerformCipher();
        }

        private void TextBoxInput_TextChanged(object sender, EventArgs e)
        {
            PerformCipher();
        }

        private void LinkLabelCopy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!(string.IsNullOrEmpty(TextBoxOutput.Text) || TextBoxOutput.Text == InvalidInputPrompt))
            {
                // Copy the text displayed in the output box to the clipboard.
                Clipboard.SetText(TextBoxOutput.Text);
                MessageBox.Show("Output text has been copied to the clipboard.", "Copy Text", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No valid output text available for copying to the clipboard.", "Copy Text", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        
        private void PerformCipher()
        {
            try
            {
                if (true == RadioButtonEncrypt.Checked)
                {
                    // Encrypt the specified text and display the result.
                    if (!string.IsNullOrEmpty(TextBoxKey.Text))
                        TextBoxOutput.Text = Cipher.Encrypt(TextBoxInput.Text, TextBoxKey.Text, CipherStrength.Level4);
                    else
                        TextBoxOutput.Text = Cipher.Encrypt(TextBoxInput.Text, DefaultCryptoKey, CipherStrength.Level4);
                }
                else if (true == RadioButtonDecrypt.Checked)
                {
                    // Decrypt the specified text and display the result.
                    if (!string.IsNullOrEmpty(TextBoxKey.Text))
                        TextBoxOutput.Text = Cipher.Decrypt(TextBoxInput.Text, TextBoxKey.Text, CipherStrength.Level4);
                    else
                        TextBoxOutput.Text = Cipher.Decrypt(TextBoxInput.Text, DefaultCryptoKey, CipherStrength.Level4);
                }
            }
            catch (Exception)
            {
                TextBoxOutput.Text = "[Input is not valid]";
            }
        }

        #endregion
    }
}
