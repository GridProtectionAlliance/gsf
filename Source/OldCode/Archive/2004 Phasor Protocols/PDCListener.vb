'***********************************************************************
'  Listener.vb - PDCstream DatAWare Listener
'  Copyright © 2004 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports System.Text
Imports TVA.Shared.DateTime
Imports TVA.Services

Public Class PDCListener

    Implements IServiceComponent
    Implements IDisposable

    Private m_parent As DatAWarePDC
    Private m_connection As DatAWare.Connection
    Private m_listener As DatAWare.Listener

    Public UserName As String
    Public Password As String

    Public Sub New(ByVal parent As DatAWarePDC, ByVal serverPort As Integer, ByVal server As String, ByVal plantCode As String, ByVal timeZone As String, ByVal userName As String, ByVal password As String)

        m_parent = parent
        m_connection = New DatAWare.Connection(server, plantCode, timeZone, DatAWare.AccessMode.ReadOnly)
        m_listener = New DatAWare.Listener(AddressOf QueueEventData, AddressOf UpdateStatus, serverPort)

    End Sub

    Protected Overrides Sub Finalize()

        MyBase.Finalize()
        Dispose()

    End Sub

    Public Overridable Sub Dispose() Implements IServiceComponent.Dispose, IDisposable.Dispose

        GC.SuppressFinalize(Me)

        ' Any needed shutdown code for your primary service process should be added here - note that this class
        ' instance is available for the duration of the service lifetime...
        [Stop]()

    End Sub

    Public ReadOnly Property Connection() As DatAWare.Connection
        Get
            Return m_connection
        End Get
    End Property

    Public Sub Start()

        m_listener.Start()

    End Sub

    Public Sub [Stop]()

        m_listener.Stop()

    End Sub

    Private Sub QueueEventData(ByVal eventBuffer As Byte())

        m_parent.EventQueue.QueueEventData(m_connection.PlantCode, eventBuffer)

    End Sub

    Private Sub UpdateStatus(ByVal status As String)

        m_parent.UpdateStatus(status)

    End Sub

#Region " IService Component Implementation "

    ' Service component implementation
    Public ReadOnly Property Name() As String Implements IServiceComponent.Name
        Get
            Return "DatAWare Listener for " & m_connection.Server & " (" & m_connection.PlantCode & ") on port " & m_listener.ServerPort
        End Get
    End Property

    Public Sub ProcessStateChanged(ByVal NewState As ProcessState) Implements IServiceComponent.ProcessStateChanged

        ' This class has no interaction with the primary archiving process, so nothing to do...

    End Sub

    Public Sub ServiceStateChanged(ByVal NewState As ServiceState) Implements IServiceComponent.ServiceStateChanged

        ' We respect changes in service state by enabling or disabling our processing state as needed...
        Select Case NewState
            Case ServiceState.Paused
                m_listener.Enabled = False
            Case ServiceState.Resumed
                m_listener.Enabled = True
        End Select

    End Sub

    Public ReadOnly Property Status() As String Implements IServiceComponent.Status
        Get
            With New StringBuilder
                .Append(m_listener.Status)
                .Append("                Plant code: " & m_connection.PlantCode & vbCrLf)
                .Append("                 Time zone: " & m_connection.TimeZone.DisplayName & vbCrLf)

                Return .ToString()
            End With
        End Get
    End Property

#End Region

End Class
