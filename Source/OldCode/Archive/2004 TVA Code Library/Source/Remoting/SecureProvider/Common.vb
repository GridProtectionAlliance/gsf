' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.Runtime.Remoting.Channels
Imports System.Runtime.Serialization.Formatters

Namespace Remoting.SecureProvider

    Public Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ' End users can use these functions to create secure client and server provider chains programmatically, they can also
        ' always just use a config file for this.  If user is using a config file, they must follow these ordering rules:
        '
        '   For server, secure server provider must come "before" deserialization sink (e.g., before SoapServerFormatterSinkProvider)
        '   For client, secure client provider must come "after" serialization sink (e.g., after SoapClientFormatterSinkProvider)
        '
        Public Shared Function CreateSecureClientProviderChain(Optional ByVal PrivateKey As String = "") As IClientChannelSinkProvider

            Dim chain As New BinaryClientFormatterSinkProvider

            chain.Next = New SecureClientChannelSinkProvider(PrivateKey)

            Return chain

        End Function

        Public Shared Function CreateSecureServerProviderChain(Optional ByVal PrivateKey As String = "", Optional ByVal NoActivityLimit As Long = 600) As IServerChannelSinkProvider

            Dim chain As New SecureServerChannelSinkProvider(PrivateKey, NoActivityLimit)
            Dim formatter As New BinaryServerFormatterSinkProvider

#If FRAMEWORK_1_1 Then
            ' This is new a required RPC security setting in the .NET 1.1 framework...
            formatter.TypeFilterLevel = TypeFilterLevel.Full
#End If
            chain.Next = formatter

            Return chain

        End Function

    End Class

End Namespace