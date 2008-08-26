'*******************************************************************************************************
'  TVA.Security.Application.Controls.AccessDenied.vb - Access denied windows dialog box 
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
'  10/16/2006 - Pinal C. Patel
'       Original version of source code generated.
'  05/06/2008 - Pinal C. Patel
'       Moved from TVA.Security.Application namespace to TVA.Security.Application.Controls namespace.
'
'*******************************************************************************************************

Namespace Application.Controls

    Public Class AccessDenied

        Private Sub AccessDenied_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

            If Me.Owner IsNot Nothing Then
                Me.Font = Me.Owner.Font
                Me.Text = Windows.Forms.Application.ProductName & " - " & Me.Text
            End If

        End Sub

        Private Sub ButtonExitApplication_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonExitApplication.Click

            Me.DialogResult = Windows.Forms.DialogResult.No
            Me.Close()

        End Sub

        Private Sub ButtonRequestAccess_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ButtonRequestAccess.Click

            Me.DialogResult = Windows.Forms.DialogResult.Yes
            Me.Close()

        End Sub

    End Class

End Namespace