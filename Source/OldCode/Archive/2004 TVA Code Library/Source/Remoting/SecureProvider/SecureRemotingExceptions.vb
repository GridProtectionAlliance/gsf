' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.Runtime.Remoting
Imports System.Runtime.Serialization

Namespace Remoting.SecureProvider

    <Serializable()> _
    Public Class SecureRemotingException

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
    Public Class SecureRemotingConnectionException

        Inherits SecureRemotingException

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
    Public Class SecureRemotingAuthenticationException

        Inherits SecureRemotingException

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
    Public Class SecureRemotingRequestUnidentifiedException

        Inherits SecureRemotingException

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
    Public Class SecureRemotingClientUnidentifiedException

        Inherits SecureRemotingException

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
    Public Class SecureRemotingClientUnauthenticatedException

        Inherits SecureRemotingException

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