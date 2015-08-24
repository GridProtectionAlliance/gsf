' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.Runtime.Remoting
Imports System.Runtime.Serialization

Namespace Remoting

    <Serializable()> _
    Public Class RemotingClientNotRegisteredException

        Inherits RemotingException

        Public Sub New(ByVal Message As String)

            MyBase.New(Message)

        End Sub

        Public Sub New(ByVal Message As String, ByVal InnerException As Exception)

            MyBase.New(Message, InnerException)

        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

        End Sub

    End Class

    <Serializable()> _
    Public Class RemotingClientNotConnectedException

        Inherits RemotingException

        Public Sub New(ByVal Message As String)

            MyBase.New(Message)

        End Sub

        Public Sub New(ByVal Message As String, ByVal InnerException As Exception)

            MyBase.New(Message, InnerException)

        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

        End Sub

    End Class

    <Serializable()> _
    Public Class RemotingServerConnectionsAtCapacityException

        Inherits RemotingException

        Public Sub New(ByVal Message As String)

            MyBase.New(Message)

        End Sub

        Public Sub New(ByVal Message As String, ByVal InnerException As Exception)

            MyBase.New(Message, InnerException)

        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

        End Sub

    End Class

    <Serializable()> _
    Public Class RemotingServerFailedToCreateNewHostException

        Inherits RemotingException

        Public Sub New(ByVal Message As String)

            MyBase.New(Message)

        End Sub

        Public Sub New(ByVal Message As String, ByVal InnerException As Exception)

            MyBase.New(Message, InnerException)

        End Sub

        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

            MyBase.New(info, context)

        End Sub

    End Class

End Namespace