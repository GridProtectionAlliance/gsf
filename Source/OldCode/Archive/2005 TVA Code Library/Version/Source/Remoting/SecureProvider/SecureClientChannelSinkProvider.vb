' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.Runtime.Remoting
Imports System.Runtime.Remoting.Channels

Namespace Remoting.SecureProvider

    Public Class SecureClientChannelSinkProvider

        Implements IClientChannelSinkProvider

        Public Shared SecureSink As SecureClientChannelSink
        Friend NextProvider As IClientChannelSinkProvider
        Friend PrivateKey As String

        Public Event ClientSinkCreated()

        Public Sub New(ByVal PrivateKey As String)

            Me.PrivateKey = PrivateKey

        End Sub

        Public Sub New(ByVal Properties As IDictionary, ByVal ProviderData As ICollection)

            Dim de As DictionaryEntry

            For Each de In Properties
                If StrComp(de.Key, "PrivateKey", CompareMethod.Text) = 0 Then
                    PrivateKey = de.Value
                    Exit For
                End If
            Next

        End Sub

        Public Function CreateSink(ByVal channel As IChannelSender, ByVal url As String, ByVal remoteChannelData As Object) As IClientChannelSink Implements IClientChannelSinkProvider.CreateSink

            Dim ccsSink As IClientChannelSink

            If Not NextProvider Is Nothing Then
                ' Call CreateSink on the next provider in the chain
                ccsSink = NextProvider.CreateSink(channel, url, remoteChannelData)

                If Not ccsSink Is Nothing Then
                    ' Create a new secure server channel sink for this provider
                    SecureSink = New SecureClientChannelSink(ccsSink, PrivateKey)
                    RaiseEvent ClientSinkCreated()
                End If
            End If

            Return SecureSink

        End Function

        Public Property [Next]() As IClientChannelSinkProvider Implements IClientChannelSinkProvider.Next
            Get
                Return NextProvider
            End Get
            Set(ByVal Value As IClientChannelSinkProvider)
                NextProvider = Value
            End Set
        End Property

    End Class

End Namespace