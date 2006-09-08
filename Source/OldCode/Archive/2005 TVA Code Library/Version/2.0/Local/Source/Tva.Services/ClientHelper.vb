' 08-29-06

Imports System.ComponentModel
Imports Tva.Communication
Imports Tva.Serialization

Public Class ClientHelper

    Public Event ReceivedServiceResponse(ByVal response As ServiceResponse)
    Public Event UpdateStatus(ByVal message As String)

    <TypeConverter(GetType(ExpandableObjectConverter)), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)> _
    Public ReadOnly Property TcpClient() As TcpClient
        Get
            Return CHTcpClient
        End Get
    End Property

    Public Sub Initialize()

        CHTcpClient.Connect()

    End Sub

    Public Sub SendRequest(ByVal request As ClientRequest)

        RaiseEvent UpdateStatus("Connecting...")
        CHTcpClient.Send(request)

    End Sub

#Region " TcpClient Events "

    Private Sub CHTcpClient_Connected(ByVal sender As Object, ByVal e As System.EventArgs) Handles CHTcpClient.Connected

        RaiseEvent UpdateStatus("Done")

    End Sub

    Private Sub CHTcpClient_Connecting(ByVal sender As Object, ByVal e As System.EventArgs) Handles CHTcpClient.Connecting

        RaiseEvent UpdateStatus(".")

    End Sub

    Private Sub CHTcpClient_ConnectingException(ByVal ex As System.Exception) Handles CHTcpClient.ConnectingException

        RaiseEvent UpdateStatus(ex.ToString())

    End Sub

    Private Sub CHTcpClient_ReceivedData(ByVal data() As System.Byte) Handles CHTcpClient.ReceivedData

        Dim response As ServiceResponse = GetObject(Of ServiceResponse)(data)
        If response IsNot Nothing Then
            RaiseEvent ReceivedServiceResponse(response)
        End If

    End Sub

#End Region

End Class
