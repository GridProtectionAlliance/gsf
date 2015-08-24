' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.Runtime.Remoting
Imports System.Runtime.Remoting.Channels

Namespace Remoting.SecureProvider

    Public Class SecureServerChannelSinkProvider

        Implements IServerChannelSinkProvider

        Public SecureSink As SecureServerChannelSink
        Friend NextProvider As IServerChannelSinkProvider
        Friend PrivateKey As String
        Friend NoActivityLimit As Integer

        Public Event ServerSinkCreated()

        Public Sub New(ByVal PrivateKey As String, Optional ByVal NoActivityLimit As Integer = 600)

            Me.PrivateKey = PrivateKey
            Me.NoActivityLimit = NoActivityLimit

        End Sub

        Public Sub New(ByVal Properties As IDictionary, ByVal ProviderData As ICollection)

            Dim de As DictionaryEntry

            For Each de In Properties
                If StrComp(de.Key, "PrivateKey", CompareMethod.Text) = 0 Then
                    PrivateKey = de.Value
                    Exit For
                ElseIf StrComp(de.Key, "NoActivityLimit", CompareMethod.Text) = 0 Then
                    NoActivityLimit = Val(de.Value)
                End If
            Next

            If NoActivityLimit <= 0 Then NoActivityLimit = 600

        End Sub

        Public Function CreateSink(ByVal channel As IChannelReceiver) As IServerChannelSink Implements IServerChannelSinkProvider.CreateSink

            Dim scsNext As IServerChannelSink

            If Not NextProvider Is Nothing Then
                ' Call CreateSink on the next provider in the chain
                scsNext = NextProvider.CreateSink(channel)

                If Not scsNext Is Nothing Then
                    ' Create a new secure server channel sink for this provider
                    SecureSink = New SecureServerChannelSink(scsNext, PrivateKey, NoActivityLimit)
                    RaiseEvent ServerSinkCreated()
                End If
            End If

            Return SecureSink

        End Function

        Public Sub GetChannelData(ByVal channelData As IChannelDataStore) Implements IServerChannelSinkProvider.GetChannelData

            ' We are not using any channel specific data

        End Sub

        Public Property [Next]() As IServerChannelSinkProvider Implements IServerChannelSinkProvider.Next
            Get
                Return NextProvider
            End Get
            Set(ByVal Value As IServerChannelSinkProvider)
                NextProvider = Value
            End Set
        End Property

    End Class

End Namespace