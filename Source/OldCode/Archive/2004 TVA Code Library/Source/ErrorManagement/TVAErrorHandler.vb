Imports System
Imports System.Threading
Imports System.ComponentModel
Imports System.Windows.Forms.Application
'Imports TVAErrorManager
Imports TVA
Imports System.Web

Namespace ErrorManagement

    Public Class ErrorHandler

        Protected Shared OEM As New TVA.ErrorManagement.ErrorManager()
        Public Shared IsInit As Boolean = False
        Public Shared IsUnhandl As Boolean = False
        Public Shared Sub addlistener(ByRef adlis As IErrorListener)
            If Not IsInit Then
                Init()
            End If
            OEM.AddListener(adlis)
        End Sub

        Public Shared Sub removelistener(ByRef adlis As IErrorListener)
            OEM.RemoveListener(adlis)
        End Sub

        Public Shared Sub LogError(ByRef ex As Exception, Optional ByVal str As String = Nothing)
            SyncLock GetType(ErrorHandler)
                OEM.LogError(ex, str)
            End SyncLock
        End Sub

        Public Shared Sub Init()
            IsInit = True
            AddHandler System.Windows.Forms.Application.ThreadException, AddressOf ErrorHandler.HandleUnhandled
            'AddHandler Thread.GetDomain().UnhandledException, AddressOf ErrorHandler.HandleUnhandled
            AddHandler System.AppDomain.CurrentDomain.UnhandledException, AddressOf ErrorHandler.HandleUnhandled
        End Sub

        Public Shared Sub MethodHandler(ByVal ex As Exception, Optional ByVal sender As Object = Nothing)
            SyncLock GetType(ErrorHandler)
                IsUnhandl = False
                LogError(ex, "m1")
            End SyncLock
        End Sub

        'Not the most exciting unhandled exception handler in the world
        Public Shared Sub HandleUnhandled(ByVal sender As Object, ByVal args As UnhandledExceptionEventArgs)
            Try
                SyncLock GetType(ErrorHandler)
                    IsUnhandl = True
				LogError(args.ExceptionObject, "Unhandled Application Exception")
                    IsUnhandl = False
                End SyncLock
            Catch
                'log error somewhere
            End Try
        End Sub

        Public Shared Sub HandleUnhandled(ByVal sender As Object, ByVal args As ThreadExceptionEventArgs)
            Try
                SyncLock GetType(ErrorHandler)
                    IsUnhandl = True
				LogError(args.Exception, "Unhandled Thread Exception")
                    IsUnhandl = False
                End SyncLock
            Catch
                'log error somewhere
            End Try
        End Sub




    End Class
End Namespace


