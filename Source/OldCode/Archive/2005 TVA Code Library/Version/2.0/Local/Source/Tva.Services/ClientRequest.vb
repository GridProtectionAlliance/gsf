'*******************************************************************************************************
'  Tva.Services.ClientRequest.vb - Client Request to Service
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

Imports Tva.Common
Imports Tva.Text.Common

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
        MyClass.New(type, Nothing)
    End Sub

    ''' <summary>
    ''' Initializes a instance of client request with the specified type and parameters
    ''' </summary>
    ''' <param name="type"></param>
    ''' <param name="parameters"></param>
    Public Sub New(ByVal type As String, ByVal parameters As String())
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
            ' Standard Request Type
            '    Undetermined
            '    ListProcesses
            '    StartProcess
            '    AbortProcess
            '    UnscheduleProcess
            '    RescheduleProcess
            '    PingService
            '    PingAllClients
            '    ListClients
            '    GetServiceStatus
            '    GetProcessStatus
            '    GetCommandHistory
            '    GetDirectoryListing
            '    ListSettings
            '    UpdateSetting
            '    SaveSettings
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
    ''' Gets or sets a boolean value indicating whether the service has handled the request and need not be 
    ''' handled by the service helper component.
    ''' </summary>
    ''' <value></value>
    ''' <returns>True if the service has handled the request; otherwise False.</returns>
    Public Property ServiceHandled() As Boolean
        Get
            Return m_serviceHandled
        End Get
        Set(ByVal value As Boolean)
            m_serviceHandled = value
        End Set
    End Property

    Public Shared Function TryParse(ByVal command As String, ByRef result As ClientRequest) As Boolean
        If Not String.IsNullOrEmpty(command) Then
            Dim request As New ClientRequest()
            Dim commandSegments As String() = command.Split(" "c)

            If commandSegments.Length > 0 Then
                request.Type = commandSegments(0).ToUpper()
                If commandSegments.Length > 1 Then
                    request.Parameters = CreateArray(Of String)(commandSegments.Length - 1)
                    Array.ConstrainedCopy(commandSegments, 1, request.Parameters, 0, request.Parameters.Length)
                End If

                result = request
                Return True
            End If
        End If

        Return False

    End Function

End Class