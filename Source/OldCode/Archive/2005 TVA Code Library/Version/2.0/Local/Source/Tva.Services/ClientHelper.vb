'*******************************************************************************************************
'  Tva.Services.ClientHelper.vb - Client Request to Service
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C. Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  08/29/2006 - Pinal C. Patel
'       Original version of source code generated
'
'*******************************************************************************************************

Imports System.ComponentModel
Imports Tva.Communication
Imports Tva.Serialization

Public Class ClientHelper

    ''' <summary>
    ''' Occurs when a response is received from the service.
    ''' </summary>
    ''' <param name="response">The response received from the service.</param>
    Public Event ReceivedServiceResponse(ByVal response As ServiceResponse)

    ''' <summary>
    ''' Occurs when the service client needs to update its status.
    ''' </summary>
    ''' <param name="message">The message that the service client must display in its status.</param>
    Public Event UpdateStatus(ByVal message As String)

    ''' <summary>
    ''' Gets the instance of TCP client used for communicating with the service.
    ''' </summary>
    ''' <value></value>
    ''' <returns>An instance of TCP client.</returns>
    <TypeConverter(GetType(ExpandableObjectConverter)), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)> _
    Public ReadOnly Property TcpClient() As TcpClient
        Get
            Return CHTcpClient
        End Get
    End Property

    ''' <summary>
    ''' Attempts to connect to the service.
    ''' </summary>
    ''' <remarks>This method must be called in order to establish connection with the service.</remarks>
    Public Sub Initialize()

        RaiseEvent UpdateStatus("Connecting...")
        CHTcpClient.Connect()

    End Sub

    ''' <summary>
    ''' Sends a request to the service.
    ''' </summary>
    ''' <param name="request">The request to be sent to the service.</param>
    Public Sub SendRequest(ByVal request As ClientRequest)

        CHTcpClient.Send(request)

    End Sub

#Region " TcpClient Events "

    Private Sub CHTcpClient_Connected(ByVal sender As Object, ByVal e As System.EventArgs) Handles CHTcpClient.Connected

        CHTcpClient.Send(New ClientInfo())
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
