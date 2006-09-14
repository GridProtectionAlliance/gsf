'*******************************************************************************************************
'  Tva.Services.ServiceResponse.vb - Service Response to Clients
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

Imports Tva.Text.Common

<Serializable()> _
Public Class ServiceResponse

    Private m_type As String
    Private m_message As String
    Private m_attachments As List(Of Object)

    ''' <summary>
    ''' Initializes a default instance of service response.
    ''' </summary>
    Public Sub New()
        MyClass.New("UNDETERMINED")
    End Sub

    ''' <summary>
    ''' Initializes a instance of service response with the specified type.
    ''' </summary>
    ''' <param name="type">The type of service response.</param>
    Public Sub New(ByVal type As String)
        MyClass.New(type, "")
    End Sub

    ''' <summary>
    ''' Initializes a instance of service response with the specified type and message.
    ''' </summary>
    ''' <param name="type">The type of service response.</param>
    ''' <param name="message">The message of the service response.</param>
    Public Sub New(ByVal type As String, ByVal message As String)
        m_type = type.ToUpper()
        m_message = message
        m_attachments = New List(Of Object)
    End Sub

    ''' <summary>
    ''' Gets or sets the type of response being sent to the client.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The type of response being sent to the client.</returns>
    Public Property Type() As String
        Get
            Return m_type
        End Get
        Set(ByVal value As String)
            ' Standard Response Type
            '    UpdateStatus
            '    ServiceStateChanged
            '    ProcessStateChanged
            m_type = value.ToUpper()
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the message being sent to the client.
    ''' </summary>
    ''' <value></value>
    ''' <returns>The message being sent to the client.</returns>
    Public Property Message() As String
        Get
            Return m_message
        End Get
        Set(ByVal value As String)
            m_message = value
        End Set
    End Property

    ''' <summary>
    ''' Gets a list of attachments being sent to the client.
    ''' </summary>
    ''' <value></value>
    ''' <returns>A list of attachments being sent to the client.</returns>
    Public ReadOnly Property Attachments() As List(Of Object)
        Get
            Return m_attachments
        End Get
    End Property

End Class