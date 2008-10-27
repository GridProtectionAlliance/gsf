using System.Diagnostics;
using System;
using System.Windows.Forms;
using System.Collections;
using System.Drawing;
using Microsoft.VisualBasic;
using System.Data;
using System.Collections.Generic;
using System.Linq;
//using PCS.Assembly;
using PCS.Security.Cryptography;
//using PCS.Security.Cryptography.Common;

//*******************************************************************************************************
//  ConfigCrypter.Main.vb - Utility for encrypting and decrypting configuration values
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: Pinal C. Patel, Operations Data Architecture [PCS]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/10/2007 - Pinal C. Patel
//       Original version of source code generated
//
//*******************************************************************************************************


namespace ConfigCrypter
{
	public partial class Main
	{
		public Main()
		{
			InitializeComponent();
			
			//Added to support default instance behavour in C#
			if (defaultInstance == null)
				defaultInstance = this;
		}
		
		#region Default Instance
		
		private static Main defaultInstance;
		
		/// <summary>
		/// Added by the VB.Net to C# Converter to support default instance behavour in C#
		/// </summary>
		public static Main Default
		{
			get
			{
				if (defaultInstance == null)
				{
					defaultInstance = new Main();
					defaultInstance.FormClosed += new FormClosedEventHandler(defaultInstance_FormClosed);
				}
				
				return defaultInstance;
			}
		}
		
		static void defaultInstance_FormClosed(object sender, FormClosedEventArgs e)
		{
			defaultInstance = null;
		}
		
		#endregion
		
		private const string CryptoKey = "0679d9ae-aca5-4702-a3f5-604415096987";
		private const string InvalidInputPrompt = "[Input is not valid]";
		
		private void Main_Load(System.Object sender, System.EventArgs e)
		{
			
			// Set the window title to application name and version.
			this.Text = string.Format(this.Text, PCS.Assembly.EntryAssembly.Product, PCS.Assembly.EntryAssembly.Version.ToString(2));
			
		}
		
		private void RadioButtonEncrypt_CheckedChanged(System.Object sender, System.EventArgs e)
		{
			
			TextBoxInput.Clear();
			TextBoxOutput.Clear();
			
		}
		
		private void TextBoxInput_TextChanged(System.Object sender, System.EventArgs e)
		{
			
			try
			{
				if (true == RadioButtonEncrypt.Checked)
				{
					// Encrypt the specified text and display the result.
					TextBoxOutput.Text = PCS.Security.Cryptography.Common.Encrypt(TextBoxInput.Text, CryptoKey, EncryptLevel.Level4);
				}
				else if (true == RadioButtonDecrypt.Checked)
				{
					// Decrypt the specified text and display the result.
					TextBoxOutput.Text = PCS.Security.Cryptography.Common.Decrypt(TextBoxInput.Text, CryptoKey, EncryptLevel.Level4);
				}
			}
			catch (Exception)
			{
				TextBoxOutput.Text = InvalidInputPrompt;
			}
			
		}
		
		private void LinkLabelCopy_LinkClicked(System.Object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			
			if (!(string.IsNullOrEmpty(TextBoxOutput.Text) || TextBoxOutput.Text == InvalidInputPrompt))
			{
				// Copy the text displayed in the output box to the clipboard.
				(new Microsoft.VisualBasic.Devices.Computer()).Clipboard.SetText(TextBoxOutput.Text);
				MessageBox.Show("Output text has been copied to the clipboard.", "Copy Text", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				MessageBox.Show("No valid output text available for copying to the clipboard.", "Copy Text", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			
		}
		
	}
}
