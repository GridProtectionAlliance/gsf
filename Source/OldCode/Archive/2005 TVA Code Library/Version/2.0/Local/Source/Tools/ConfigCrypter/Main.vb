'*******************************************************************************************************
'  ConfigCrypter.Main.vb - Utility for encrypting and decrypting configuration values
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  07/10/2007 - Pinal C. Patel
'       Original version of source code generated
'
'*******************************************************************************************************

Imports TVA.Assembly
Imports TVA.Security.Cryptography
Imports TVA.Security.Cryptography.Common

Public Class Main

    Private Const CryptoKey As String = "0679d9ae-aca5-4702-a3f5-604415096987"

    Private Sub Main_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        ' Set the window title to application name and version.
        Me.Text = String.Format(Me.Text, EntryAssembly.Product, EntryAssembly.Version.ToString(2))

    End Sub

    Private Sub RadioButtonEncrypt_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RadioButtonEncrypt.CheckedChanged, RadioButtonEncrypt.CheckedChanged

        TextBoxInput.Clear()
        TextBoxOutput.Clear()

    End Sub

    Private Sub TextBoxInput_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextBoxInput.TextChanged

        Try
            Select Case True
                Case RadioButtonEncrypt.Checked
                    ' Encrypt the specified text and display the result.
                    TextBoxOutput.Text = Encrypt(TextBoxInput.Text, CryptoKey, EncryptLevel.Level4)
                Case RadioButtonDecrypt.Checked
                    ' Decrypt the specified text and display the result.
                    TextBoxOutput.Text = Decrypt(TextBoxInput.Text, CryptoKey, EncryptLevel.Level4)
            End Select
        Catch ex As Exception
            TextBoxOutput.Text = "[Input is invalid]"
        End Try

    End Sub

    Private Sub LinkLabelCopy_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles LinkLabelCopy.LinkClicked

        If Not String.IsNullOrEmpty(TextBoxOutput.Text) Then
            ' Copy the text displayed in the output box to the clipboard.
            My.Computer.Clipboard.SetText(TextBoxOutput.Text)
            MessageBox.Show("Output text has been copied to the clipboard.", "Copy Text", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            MessageBox.Show("No output text available for copying to the clipboard.", "Copy Text", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If

    End Sub

End Class