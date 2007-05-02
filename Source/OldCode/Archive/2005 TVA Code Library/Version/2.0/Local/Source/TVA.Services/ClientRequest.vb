'*******************************************************************************************************
'  TVA.Services.ClientRequest.vb - Client Request to Service
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
'  04/27/2007 - Pinal C. Patel
'       Added Attachments property for clients to send serializable objects as part of the request
'
'*******************************************************************************************************

Imports TVA.Common
Imports TVA.Text.Common
Imports TVA.Console

<Serializable()> _
Public Class ClientRequest

    Private m_type As String
    Private m_arguments As Arguments
    Private m_attachments As List(Of Object)

    ''' <summary>
    ''' Initializes a default instance of client request.
    ''' </summary>
    Public Sub New()

        MyClass.New("UNDETERMINED")

    End Sub

    ''' <summary>
    ''' Initializes a instance of client request with the specified type.
    ''' </summary>
    ''' <param name="type">The type of client request.</param>
    Public Sub New(ByVal type As String)

        MyClass.New(type, New Arguments(""))

    End Sub

    ''' <summary>
    ''' Initializes a instance of client request with the specified type and parameters
    ''' </summary>
    ''' <param name="type"></param>
    ''' <param name="arguments"></param>
    Public Sub New(ByVal type As String, ByVal arguments As Arguments)

        MyBase.New()
        m_type = type.ToUpper()
        m_arguments = arguments
        m_attachments = New List(Of Object)()

    End Sub

    ''' <summary>
    ''' Gets or sets the type of request being sent to the service.
    ''' </summary>
    ''' <value>The type of request being sent to the service.</value>
    Public Property Type() As String
        Get
            Return m_type
        End Get
        Set(ByVal value As String)
            m_type = value.ToUpper()
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets additional parameters being sent to the service.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Additional parameters being sent to the service.</returns>
    Public Property Arguments() As Arguments
        Get
            Return m_arguments
        End Get
        Set(ByVal value As Arguments)
            m_arguments = value
        End Set
    End Property

    ''' <summary>
    ''' Gets a list of attachments being sent to the service.
    ''' </summary>
    ''' <value></value>
    ''' <returns>A list of attachments being sent to the service.</returns>
    Public ReadOnly Property Attachments() As List(Of Object)
        Get
            Return m_attachments
        End Get
    End Property

    ''' <summary>
    ''' Parses the specified text into TVA.Services.ClientRequest.
    ''' </summary>
    ''' <param name="text">The text to be parsed.</param>
    ''' <returns>A TVA.Services.ClientRequest instance.</returns>
    Public Shared Function Parse(ByVal text As String) As ClientRequest

        Dim request As ClientRequest = Nothing
        If Not String.IsNullOrEmpty(text) Then
            Dim textSegments As String() = text.Split(" "c)
            If textSegments.Length > 0 Then
                request = New ClientRequest()
                request.Type = textSegments(0).ToUpper()
                If textSegments.Length = 1 Then
                    request.Arguments = New Arguments("")
                Else
                    request.Arguments = New Arguments(text.Remove(0, text.IndexOf(" "c)))
                End If
            End If
        End If
        Return request

    End Function

End Class