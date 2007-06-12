' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.Threading
Imports System.Reflection

Namespace Threading

    ' This class uses reflection to invoke an existing sub or function on a new thread, its usage
    ' can be as simple as RunThread.ExecuteMethod(Me, "MyMethod", "param1", "param2", True)
    Public Class RunThread

        Inherits ThreadBase
        Implements IComparable

        Public Event ThreadComplete()
        Public Event ThreadExecError(ByVal ex As Exception)

        Public ObjectType As Type
        Public Instance As Object
        Public MethodName As String
        Public Parameters As Object()
        Public InvokeAttributes As BindingFlags
        Public ReturnValue As Object
        Protected ID As Guid = Guid.NewGuid()
        Protected Shared AllocatedThreads As New ArrayList()

        Public Sub New()

            ' Create a static reference to this thread
            SyncLock AllocatedThreads.SyncRoot
                AllocatedThreads.Add(Me)
                AllocatedThreads.Sort()
            End SyncLock

        End Sub

        Public Shared Function ExecuteMethod(ByVal Instance As Object, ByVal MethodName As String, ByVal ParamArray Params As Object()) As RunThread

            Return Execute(Instance.GetType(), Instance, MethodName, BindingFlags.InvokeMethod Or BindingFlags.Instance Or BindingFlags.Public, Params)

        End Function

        Public Shared Function ExecuteNonPublicMethod(ByVal Instance As Object, ByVal MethodName As String, ByVal ParamArray Params As Object()) As RunThread

            Return Execute(Instance.GetType(), Instance, MethodName, BindingFlags.InvokeMethod Or BindingFlags.Instance Or BindingFlags.NonPublic, Params)

        End Function

        Public Shared Function ExecuteSharedMethod(ByVal ObjectType As Type, ByVal MethodName As String, ByVal ParamArray Params As Object()) As RunThread

            Return Execute(ObjectType, Nothing, MethodName, BindingFlags.InvokeMethod Or BindingFlags.Static Or BindingFlags.Public, Params)

        End Function

        Public Shared Function ExecuteNonPublicSharedMethod(ByVal ObjectType As Type, ByVal MethodName As String, ByVal ParamArray Params As Object()) As RunThread

            Return Execute(ObjectType, Nothing, MethodName, BindingFlags.InvokeMethod Or BindingFlags.Static Or BindingFlags.NonPublic, Params)

        End Function

        Public Shared Function ExecutePropertyGet(ByVal Instance As Object, ByVal MethodName As String, ByVal ParamArray Params As Object()) As RunThread

            Return Execute(Instance.GetType(), Instance, MethodName, BindingFlags.GetProperty Or BindingFlags.Instance Or BindingFlags.Public, Params)

        End Function

        Public Shared Function ExecutePropertySet(ByVal Instance As Object, ByVal MethodName As String, ByVal ParamArray Params As Object()) As RunThread

            Return Execute(Instance.GetType(), Instance, MethodName, BindingFlags.SetProperty Or BindingFlags.Instance Or BindingFlags.Public, Params)

        End Function

        Public Shared Function Execute(ByVal ObjectType As Type, ByVal Instance As Object, ByVal MethodName As String, ByVal InvokeAttributes As BindingFlags, ByVal ParamArray Params As Object()) As RunThread

            Dim rt As New RunThread()

            With rt
                .ObjectType = ObjectType
                .Instance = Instance
                .MethodName = MethodName
                .InvokeAttributes = (InvokeAttributes Or BindingFlags.IgnoreCase)
                ReDim .Parameters(Params.Length - 1)
                Params.CopyTo(.Parameters, 0)
                .Start()
            End With

            Return rt

        End Function

        Protected Overrides Sub ThreadProc()

            Try
                ' Invoke user method
                ReturnValue = ObjectType.InvokeMember(MethodName, InvokeAttributes, Nothing, Instance, Parameters)
            Catch ex As Exception
                RaiseEvent ThreadExecError(ex)
            End Try

        End Sub

        Protected Overrides Sub ThreadStopped()

            ' Remove the static reference to this thread
            SyncLock AllocatedThreads.SyncRoot
                Dim intIndex As Integer = AllocatedThreads.BinarySearch(Me)
                If intIndex >= 0 Then AllocatedThreads.RemoveAt(intIndex)
            End SyncLock

            RaiseEvent ThreadComplete()

        End Sub

        Public Function CompareTo(ByVal obj As Object) As Integer Implements IComparable.CompareTo

            ' Allocated threads are sorted by ID
            If TypeOf obj Is RunThread Then
                Return ID.CompareTo(DirectCast(obj, RunThread).ID)
            Else
                Throw New ArgumentException("RunThread can only be compared to other RunThreads")
            End If

        End Function

    End Class

End Namespace