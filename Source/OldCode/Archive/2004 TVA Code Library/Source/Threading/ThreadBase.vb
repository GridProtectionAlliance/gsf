' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.Threading
Imports System.Reflection

Namespace Threading

    ' This is a convienent base class for new threads - deriving your own thread class from from this
    ' class ensures your thread will properly terminate when your object is ready to be garbage
    ' collected and allows you to define properties for the needed parameters of your thread proc
    Public MustInherit Class ThreadBase

        Implements IDisposable

        Protected WorkerThread As Thread

        Public Sub New()

            WorkerThread = New Thread(AddressOf ThreadExec)

        End Sub

        Protected Overrides Sub Finalize()

            Abort()

        End Sub

        Public Overridable Sub Start()

            WorkerThread.Start()

        End Sub

        Public Sub Abort() Implements IDisposable.Dispose

            If Not WorkerThread Is Nothing Then
                Try
                    If WorkerThread.IsAlive Then
                        WorkerThread.Abort()
                        ThreadStopped()
                    End If
                Catch
                End Try
                WorkerThread = Nothing
            End If
            GC.SuppressFinalize(Me)

        End Sub

        Public ReadOnly Property Thread() As Thread
            Get
                Return WorkerThread
            End Get
        End Property

        Private Sub ThreadExec()

            ThreadStarted()
            ThreadProc()
            ThreadStopped()

        End Sub

        Protected Overridable Sub ThreadStarted()
        End Sub

        Protected MustOverride Sub ThreadProc()

        Protected Overridable Sub ThreadStopped()
        End Sub

    End Class

End Namespace