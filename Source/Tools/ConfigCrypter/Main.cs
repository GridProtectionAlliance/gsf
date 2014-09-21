//******************************************************************************************************
//  Main.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/05/2009 - Pinal C. Patel
//       Generated original version of source code.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/14/2009 - Pinal C. Patel
//       Added option to import and export key IV via clipboard.
//  04/26/2011 - J. Ritchie Carroll
//       Added call to flush crypto cache on shutdown.
//
//******************************************************************************************************

using System;
using System.Windows.Forms;
using GSF.Configuration;
using GSF.Reflection;
using GSF.Security.Cryptography;

namespace ConfigCrypter
{
    public partial class Main : Form
    {
        #region [ Members ]

        // Constants
        private const CipherStrength CryptoStrength = CipherStrength.Aes256;
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

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cipher.FlushCache();
            ConfigurationFile.Current.Save();
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

        private void LinkLabelImportIV_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string keyIV = Clipboard.GetData(DataFormats.Text).ToString();
                if (!string.IsNullOrEmpty(TextBoxKey.Text))
                    Cipher.ImportKeyIV(TextBoxKey.Text, (int)CryptoStrength, keyIV);
                else
                    Cipher.ImportKeyIV(DefaultCryptoKey, (int)CryptoStrength, keyIV);

                MessageBox.Show(string.Format("Key IV imported from clipboard: \r\n{0}", keyIV), "Import Key IV", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Key IV import failed: \r\n{0}", ex.Message), "Import Key IV", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LinkLabelExportIV_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                string keyIV;
                if (!string.IsNullOrEmpty(TextBoxKey.Text))
                    keyIV = Cipher.ExportKeyIV(TextBoxKey.Text, (int)CryptoStrength);
                else
                    keyIV = Cipher.ExportKeyIV(DefaultCryptoKey, (int)CryptoStrength);
                Clipboard.SetText(keyIV);

                MessageBox.Show(string.Format("Key IV exported to clipboard: \r\n{0}", keyIV), "Export Key IV", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Key IV export failed: \r\n{0}", ex.Message), "Export Key IV", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LinkLabelCopy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!(string.IsNullOrEmpty(TextBoxOutput.Text) ||
                  TextBoxOutput.Text == InvalidInputPrompt ||
                  TextBoxOutput.Text.StartsWith("Error", StringComparison.CurrentCultureIgnoreCase)))
            {
                // Copy the text displayed in the output box to the clipboard.
                Clipboard.SetText(TextBoxOutput.Text);
                MessageBox.Show("Output text has been copied to the clipboard.", "Copy Text", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Current text in the output box is not valid cipher output.
                MessageBox.Show("No valid output text available for copying to the clipboard.", "Copy Text", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void PerformCipher()
        {
            try
            {
                if (RadioButtonEncrypt.Checked)
                {
                    // Encrypt the specified text and display the result.
                    if (!string.IsNullOrEmpty(TextBoxKey.Text))
                        TextBoxOutput.Text = TextBoxInput.Text.Encrypt(TextBoxKey.Text, CryptoStrength);
                    else
                        TextBoxOutput.Text = TextBoxInput.Text.Encrypt(DefaultCryptoKey, CryptoStrength);
                }
                else if (RadioButtonDecrypt.Checked)
                {
                    // Decrypt the specified text and display the result.
                    if (!string.IsNullOrEmpty(TextBoxKey.Text))
                        TextBoxOutput.Text = TextBoxInput.Text.Decrypt(TextBoxKey.Text, CryptoStrength);
                    else
                        TextBoxOutput.Text = TextBoxInput.Text.Decrypt(DefaultCryptoKey, CryptoStrength);
                }
            }
            catch (Exception ex)
            {
                TextBoxOutput.Text = string.Format("ERROR: {0}", ex.Message);
            }
        }

        #endregion
    }
}
