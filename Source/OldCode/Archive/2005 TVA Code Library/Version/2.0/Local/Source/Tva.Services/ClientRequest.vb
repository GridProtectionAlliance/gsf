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
'  10/26/2006 - J. Ritchie Carroll
'       Modified TryParse to make sure extraneous spaces were removed from command string and
'       made all command parameters upper-case for easier compare
'
'*******************************************************************************************************

Imports TVA.Common
Imports TVA.Text.Common

<Serializable()> _
Public Class ClientRequest

    Private m_type As String
    Private m_parameters As String()
    Private m_serviceHandled As Boolean

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

        MyClass.New(type, New String() {})

    End Sub

    ''' <summary>
    ''' Initializes a instance of client request with the specified type and parameters
    ''' </summary>
    ''' <param name="type"></param>
    ''' <param name="parameters"></param>
    Public Sub New(ByVal type As String, ByVal parameters As String())

        MyBase.New()
        m_type = type.ToUpper()
        m_parameters = parameters
        m_serviceHandled = False

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
    ''' Gets or sets the additional parameters being sent to the service.
    ''' </summary>
    ''' <value></value>
    ''' <returns>Additional parameters being sent to the service.</returns>
    Public Property Parameters() As String()
        Get
            Return m_parameters
        End Get
        Set(ByVal value As String())
            m_parameters = value
        End Set
    End Property

    ''' <summary>
    ''' Parses text into a type and parameters array.
    ''' </summary>
    ''' <param name="text">The text to be parsed.</param>
    ''' <returns>A TVA.Services.ClientRequest instance.</returns>
    Public Shared Function Parse(ByVal text As String) As ClientRequest

        Dim request As ClientRequest = Nothing
        If Not String.IsNullOrEmpty(text) Then
            Dim textSegments As String() = TVA.Console.Common.ParseCommand(text)

            If textSegments.Length > 0 Then
                request = New ClientRequest()
                request.Type = textSegments(0).ToUpper()

                If textSegments.Length > 1 Then
                    request.Parameters = CreateArray(Of String)(textSegments.Length - 1)
                    Array.ConstrainedCopy(textSegments, 1, request.Parameters, 0, request.Parameters.Length)
                End If
            End If
        End If
        Return request

    End Function

End Class