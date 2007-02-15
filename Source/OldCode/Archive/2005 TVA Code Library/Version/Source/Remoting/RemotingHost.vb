Imports System.Reflection
Imports System.Reflection.BindingFlags

Namespace Remoting

    Class RemotingHost

        Shared Server As IServer

        Shared Sub Main()

            Dim Args As String()
            Dim ServerAssembly As [Assembly]
            Dim ServerType As Type
            Dim PropertyCall As PropertyInfo
            Dim FieldCall As FieldInfo
            Dim KeyValueEntry As String
            Dim KeyValuePair As String()
            Dim ConsoleLine As String
            Dim x As Integer

            ' Parse primary command line arguments
            Args = Command.Split("|"c)

            If Args.Length >= 2 Then
                Console.WriteLine([Assembly].GetExecutingAssembly.FullName & vbCrLf & vbCrLf)
                For x = 0 To Args.Length - 1
                    Console.WriteLine("Arg({0}): {1}", x, Args(x) & vbCrLf)
                Next

                ServerAssembly = [Assembly].LoadFrom(Args(0))
                ServerType = ServerAssembly.GetType(Args(1))
                Server = Activator.CreateInstance(ServerType)

                If Args.Length >= 3 Then
                    ' Step through each key/value pair and initialize each specifed property value on new server object
                    For Each KeyValueEntry In Args(2).Split(";"c)
                        KeyValuePair = KeyValueEntry.Split("="c)
                        If KeyValuePair.Length = 2 Then
                            ' See if we can lookup given method as a property
                            PropertyCall = ServerType.GetProperty(KeyValuePair(0), [Public] Or [Instance] Or [Static] Or [IgnoreCase])
                            If PropertyCall Is Nothing Then
                                ' If not, see if it is a field type propery
                                FieldCall = ServerType.GetField(KeyValuePair(0), [Public] Or [Instance] Or [Static] Or [IgnoreCase])
                                If FieldCall Is Nothing Then
                                    Console.WriteLine("Property [" & KeyValuePair(0) & "] not found in [" & Args(1) & "] class!")
                                Else
                                    FieldCall.SetValue(Server, Convert.ChangeType(KeyValuePair(1), FieldCall.FieldType))
                                End If
                            Else
                                PropertyCall.SetValue(Server, Convert.ChangeType(KeyValuePair(1), PropertyCall.PropertyType), Nothing)
                            End If
                        End If
                    Next
                End If

                ' Establish new server instance
                Server.Start()

                Do While True
                    ' This console application stays open by continually reading in console lines
                    ConsoleLine = Console.ReadLine()
                    If StrComp(ConsoleLine, "Exit", CompareMethod.Text) = 0 Then
                        Exit Do
                    End If
                Loop
            End If

            If Not Server Is Nothing Then Server.Shutdown()
            End

        End Sub

    End Class

End Namespace