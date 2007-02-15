Imports System
Imports System.Threading
Imports System.Windows.Forms
Imports System.Drawing
Namespace ErrorManagement
    <ToolboxBitmap(GetType(PopUpErrorListener), "PopUpErrorListener.bmp")> _
    Public Class PopUpErrorListener
        Inherits System.ComponentModel.Component
        Implements IErrorListener

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

        Protected Overridable Sub OutputError(ByVal oErrorManager As IErrorManager)
            bErrorReceived = False
            If oErrorManager.ErrorException Is Nothing Then
                '?
            Else    'got exception
                MessageBox.Show("Exception: [" & oErrorManager.ErrorMessage & "] " & oErrorManager.ErrorException.ToString())
                bErrorReceived = True
            End If
        End Sub
    End Class
End Namespace


