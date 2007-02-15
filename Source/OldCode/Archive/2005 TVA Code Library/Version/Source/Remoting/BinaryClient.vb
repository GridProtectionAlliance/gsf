' James Ritchie Carroll - 2003
Option Explicit On 

Imports System.ComponentModel
Imports System.Drawing
Imports System.Runtime.Remoting.Channels
Imports System.Runtime.Serialization.Formatters

Namespace Remoting

    <Serializable(), ToolboxBitmap(GetType(BinaryClient), "BinaryClient.bmp"), DefaultEvent("ServerNotification"), DefaultProperty("URI")> _
    Public Class BinaryClient

        Inherits ClientBase
        Implements IComponent

        ' Component Implementation
        Private ComponentSite As ISite
        Public Event Disposed(ByVal sender As Object, ByVal e As EventArgs) Implements IComponent.Disposed

        Public Sub New()

            MyBase.New()
            URI = "tcp://localhost:8090/BinaryServerURI"

        End Sub

        Public Sub New(ByVal URI As String, Optional ByVal TCPPort As Integer = 0)

            MyBase.New(URI, TCPPort)

        End Sub

        Public Overrides Sub Shutdown()

            MyBase.Shutdown()
            RaiseEvent Disposed(Me, EventArgs.Empty)

        End Sub

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Overridable Overloads Property Site() As ISite Implements IComponent.Site
            Get
                Return ComponentSite
            End Get
            Set(ByVal Value As ISite)
                ComponentSite = Value
            End Set
        End Property

        Public Overrides ReadOnly Property Description() As String
            Get
                Return "Binary " & MyBase.Description
            End Get
        End Property

        Public Overrides Function CreateClientProviderChain() As IClientChannelSinkProvider

            Return New BinaryClientFormatterSinkProvider

        End Function

        Public Overrides Function CreateServerProviderChain() As IServerChannelSinkProvider

            Dim formatter As New BinaryServerFormatterSinkProvider

#If FRAMEWORK_1_1 Then
            ' This is new a required RPC security setting in the .NET 1.1 framework...
            formatter.TypeFilterLevel = TypeFilterLevel.Full
#End If
            Return formatter

        End Function

    End Class

End Namespace