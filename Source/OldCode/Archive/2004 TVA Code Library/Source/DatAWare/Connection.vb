'***********************************************************************
'  Connection.vb - DatAWare Connection Class
'  Copyright © 2005 - TVA, all rights reserved
'  
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  10/1/2004 - James R Carroll
'       Initial version of source created
'
'***********************************************************************
Option Explicit On 

Imports TVA.Shared.DateTime

Namespace DatAWare

    Public Class Connection

        Implements IDisposable

        Private WithEvents m_dwAPI As DWApi.DWAPIdllClass
        Private m_points As Points
        Private m_server As String
        Private m_plantCode As String
        Private m_access As AccessMode
        Private m_timeZone As Win32TimeZone
        Private m_connected As Boolean

        Public Event ServerMessage(ByVal message As String)
        Public Event ServerDebugMessage(ByVal message As String)

        Public Sub New(ByVal server As String, ByVal plantCode As String, Optional ByVal timeZone As String = "Central Standard Time", Optional ByVal access As AccessMode = AccessMode.ReadWrite)

            m_dwAPI = New DWApi.DWAPIdllClass
            m_points = New Points(Me)
            m_server = server
            m_plantCode = plantCode
            m_access = access
            m_timeZone = GetWin32TimeZone(timeZone)

        End Sub

        Protected Overrides Sub Finalize()

            Close()

        End Sub

        Public ReadOnly Property DWAPI() As DWApi.DWAPIdllClass
            Get
                Return m_dwAPI
            End Get
        End Property

        Public ReadOnly Property Points() As Points
            Get
                Return m_points
            End Get
        End Property

        Public ReadOnly Property Server() As String
            Get
                Return m_server
            End Get
        End Property

        Public ReadOnly Property PlantCode() As String
            Get
                Return m_plantCode
            End Get
        End Property

        Public ReadOnly Property Access() As AccessMode
            Get
                Return m_access
            End Get
        End Property

        Public ReadOnly Property TimeZone() As Win32TimeZone
            Get
                Return m_timeZone
            End Get
        End Property

        Public Sub Open()

            Dim errorMessage As String

            ' Open using integrated NT authentication
            m_dwAPI.ConnectTo(m_server, m_plantCode, m_access, errorMessage)

            If Len(errorMessage) > 0 Then
                Throw New InvalidOperationException("Failed to connect to DatAWare server """ & m_server & """ due to exception: " & errorMessage)
            Else
                m_connected = True
            End If

        End Sub

        Public Sub Open(ByVal userName As String, ByVal password As String)

            Dim errorMessage As String

            ' Open using specific username and password
            m_dwAPI.ConnectTo(m_server, m_plantCode, m_access, errorMessage, userName & "/" & password)

            If Len(errorMessage) > 0 Then
                Throw New InvalidOperationException("Failed to connect to DatAWare server """ & m_server & """ due to exception: " & errorMessage)
            Else
                m_connected = True
            End If

        End Sub

        Public Sub Close() Implements IDisposable.Dispose

            GC.SuppressFinalize(Me)
            If Not m_dwAPI Is Nothing Then m_dwAPI.Disconnect(m_plantCode)
            m_connected = False

        End Sub

        Public ReadOnly Property IsOpen() As Boolean
            Get
                Return m_connected
            End Get
        End Property

        Private Sub m_dwAPI_NormalMessage(ByRef mMsg As String) Handles m_dwAPI.GotMessage

            RaiseEvent ServerMessage(mMsg)
#If DEBUG Then
            Debug.WriteLine("DatAWare Server """ & m_server & """ Message: " & mMsg)
#End If

        End Sub

        Private Sub m_dwAPI_DebugMessage(ByRef mMsg As String) Handles m_dwAPI.DEBUGMESSAGE

            RaiseEvent ServerDebugMessage(mMsg)
#If DEBUG Then
            Debug.WriteLine("DatAWare Server """ & m_server & """ Debug Message: " & mMsg)
#End If

        End Sub

    End Class

End Namespace
