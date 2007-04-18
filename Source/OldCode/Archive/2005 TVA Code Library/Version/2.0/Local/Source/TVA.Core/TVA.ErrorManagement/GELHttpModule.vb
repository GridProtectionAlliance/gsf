' PCP: 04/17/2007

Option Strict On

Imports System.Web
Imports TVA.Configuration.Common

Namespace ErrorManagement

    Public Class GELHttpModule
        Implements IHttpModule

        Public Sub Dispose() Implements System.Web.IHttpModule.Dispose

            ' We don't have to dispose anything...

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
                .LoadSettings()
                .Log(HttpContext.Current.Server.GetLastError())
            End With

        End Sub

    End Class

End Namespace