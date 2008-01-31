' James Ritchie Carroll - 2005
Option Explicit On 

Imports System.Threading
Imports System.Reflection

Namespace Threading

    ' This class uses reflection to invoke an existing sub or function on a new thread, its usage
    ' can be as simple as QueueThread.ExecuteMethod(Me, "MyMethod", "param1", "param2", True)
    Public Class QueueThread

        Public ObjectType As Type
        Public Instance As Object
        Public MethodName As String
        Public Parameters As Object()
        Public InvokeAttributes As BindingFlags

        Public Shared Sub ExecuteMethod(ByVal Instance As Object, ByVal MethodName As String, ByVal ParamArray Params As Object())

            Execute(Instance.GetType(), Instance, MethodName, BindingFlags.InvokeMethod Or BindingFlags.Instance Or BindingFlags.Public, Params)

        End Sub

        Public Shared Sub ExecuteNonPublicMethod(ByVal Instance As Object, ByVal MethodName As String, ByVal ParamArray Params As Object())

            Execute(Instance.GetType(), Instance, MethodName, BindingFlags.InvokeMethod Or BindingFlags.Instance Or BindingFlags.NonPublic, Params)

        End Sub

        Public Shared Sub ExecuteSharedMethod(ByVal ObjectType As Type, ByVal MethodName As String, ByVal ParamArray Params As Object())

            Execute(ObjectType, Nothing, MethodName, BindingFlags.InvokeMethod Or BindingFlags.Static Or BindingFlags.Public, Params)

        End Sub

        Public Shared Sub ExecuteNonPublicSharedMethod(ByVal ObjectType As Type, ByVal MethodName As String, ByVal ParamArray Params As Object())

            Execute(ObjectType, Nothing, MethodName, BindingFlags.InvokeMethod Or BindingFlags.Static Or BindingFlags.NonPublic, Params)

        End Sub

        Public Shared Sub ExecutePropertyGet(ByVal Instance As Object, ByVal MethodName As String, ByVal ParamArray Params As Object())

            Execute(Instance.GetType(), Instance, MethodName, BindingFlags.GetProperty Or BindingFlags.Instance Or BindingFlags.Public, Params)

        End Sub

        Public Shared Sub ExecutePropertySet(ByVal Instance As Object, ByVal MethodName As String, ByVal ParamArray Params As Object())

            Execute(Instance.GetType(), Instance, MethodName, BindingFlags.SetProperty Or BindingFlags.Instance Or BindingFlags.Public, Params)

        End Sub

        Public Shared Sub Execute(ByVal ObjectType As Type, ByVal Instance As Object, ByVal MethodName As String, ByVal InvokeAttributes As BindingFlags, ByVal ParamArray Params As Object())

            Dim qt As New QueueThread

            With qt
                .ObjectType = ObjectType
                .Instance = Instance
                .MethodName = MethodName
                .InvokeAttributes = (InvokeAttributes Or BindingFlags.IgnoreCase)
                ReDim .Parameters(Params.Length - 1)
                Params.CopyTo(.Parameters, 0)
            End With

#If ThreadTracking Then
            With ManagedThreadPool.QueueUserWorkItem(AddressOf ThreadProc, qt)
                .Name = "TVA.Threading.QueueThread.ThreadProc()"
            End With
#Else
            ThreadPool.QueueUserWorkItem(AddressOf ThreadProc, qt)
#End If

        End Sub

        Protected Shared Sub ThreadProc(ByVal stateInfo As Object)

            ' Invoke user method
            With DirectCast(stateInfo, QueueThread)
                .ObjectType.InvokeMember(.MethodName, .InvokeAttributes, Nothing, .Instance, .Parameters)
            End With

        End Sub

    End Class

End Namespace