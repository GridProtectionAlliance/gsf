Imports System
Imports System.Diagnostics
Imports System.Windows.Forms.Application
Imports System.Web
Imports System.Drawing
Imports System.ComponentModel

Namespace ErrorManagement
    <ToolboxBitmap(GetType(EventLogErrorListener), "EventLogErrorListener.bmp")> _
    Public Class EventLogErrorListener

        Inherits Component
        Implements TVA.ErrorManagement.IErrorListener

        Public Sub New()
            MyBase.New()
            ErrorHandler.AddListener(Me)
        End Sub

        Public Function FilterError(ByVal oErrorManager As TVA.ErrorManagement.IErrorManager) As Boolean Implements TVA.ErrorManagement.IErrorListener.FilterError
            Return True
        End Function

        Public Sub Finish() Implements TVA.ErrorManagement.IErrorListener.Finish

        End Sub

        Public Sub PostError(ByVal oErrorManager As TVA.ErrorManagement.IErrorManager) Implements TVA.ErrorManagement.IErrorListener.PostError
            If FilterError(oErrorManager) Then
                OutputError(oErrorManager)
            End If
        End Sub

        Public Sub Start() Implements TVA.ErrorManagement.IErrorListener.Start

        End Sub

        Protected bErrorReceived As Boolean
        Public Property WasErrorReceived() As Boolean
            Get
                Return bErrorReceived
            End Get
            Set(ByVal Value As Boolean)
                bErrorReceived = Value
            End Set
        End Property

        Protected hApplication As HttpApplication
        Public Property Application() As HttpApplication
            Get
                Return hApplication
            End Get
            Set(ByVal Value As HttpApplication)
                hApplication = Value
            End Set
        End Property

        Protected Overridable Sub OutputError(ByVal oErrorManager As IErrorManager)
            bErrorReceived = False
            If oErrorManager.ErrorException Is Nothing Then

            Else
                Dim AppSource As String = Reflection.Assembly.GetExecutingAssembly.GetName.Name
                If Len(AppSource) <= 0 Then AppSource = "Log error handler"

                If Not EventLog.SourceExists(AppSource) Then
                    EventLog.CreateEventSource(AppSource, "application")
                End If

                EventLog.WriteEntry(AppSource, "Exception: [" & oErrorManager.ErrorMessage & "] " & _
                    oErrorManager.ErrorException.ToString(), EventLogEntryType.Error)

                bErrorReceived = True

            End If
        End Sub

    End Class

End Namespace