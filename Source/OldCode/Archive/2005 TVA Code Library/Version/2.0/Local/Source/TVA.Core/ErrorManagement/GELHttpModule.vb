'*******************************************************************************************************
'  TVA.ErrorManagement.GELHttpModule.vb - Common Configuration Functions
'  Copyright © 2007 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  04/17/2007 - Pinal C. Patel
'       Generated original version of source code.
'  09/13/2007 - Darrell Zuercher
'       Edited code comments.
'
'*******************************************************************************************************

Option Strict On

Imports System.Web
Imports TVA.Configuration.Common

Namespace ErrorManagement

    Public Class GELHttpModule
        Implements IHttpModule

        Public Sub Dispose() Implements System.Web.IHttpModule.Dispose

            ' We do not have to dispose of anything.

        End Sub

        Public Sub Init(ByVal context As System.Web.HttpApplication) Implements System.Web.IHttpModule.Init

            Try
                If CategorizedSettings(GetType(GlobalExceptionLogger).Name).Count = 0 Then
                    With New GlobalExceptionLogger()
                        .PersistSettings = True
                        .SaveSettings()
                    End With
                End If
            Catch ex As Exception

            End Try

            AddHandler context.Error, AddressOf OnError

        End Sub

        Private Sub OnError(ByVal sender As Object, ByVal e As System.EventArgs)

            With New GlobalExceptionLogger()
                ' Logs the encountered exception.
                .BeginInit()
                .EndInit()
                .Log(HttpContext.Current.Server.GetLastError())
            End With

        End Sub

    End Class

End Namespace