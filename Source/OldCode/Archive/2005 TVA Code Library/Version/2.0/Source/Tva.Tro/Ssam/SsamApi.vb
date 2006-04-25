' 04-24-06

Imports System.ComponentModel
Imports Tva.Data.Common
Imports Tva.Configuration.Common

Namespace Ssam

    <TypeConverter(GetType(ExpandableObjectConverter))> _
    Public Class SsamApi

        Private m_server As SsamServer
        Private m_keepConnectionOpen As Boolean
        Private m_connection As System.Data.SqlClient.SqlConnection
        Private m_connectionState As SsamConnectionStates

        Public Event LogException(ByVal ex As Exception)

        Public Enum SsamServer As Integer
            Development
            <EditorBrowsable(EditorBrowsableState.Never), Browsable(False)> _
            Acceptance
            Production
        End Enum

        <Flags()> _
        Public Enum SsamConnectionStates As Integer
            Open = 1
            Closed = 2
            Active = 4
            Inactive = 8
            OpenAndActive = Open And Active
            OpenAndInactive = Open And Inactive
        End Enum

        Public Sub New()
            Me.New(SsamServer.Development)
        End Sub

        Public Sub New(ByVal server As SsamServer)
            Me.New(server, True)
        End Sub

        Public Sub New(ByVal server As SsamServer, ByVal keepConnectionOpen As Boolean)
            MyBase.New()
            m_connection = New System.Data.SqlClient.SqlConnection()
            m_connectionState = SsamConnectionStates.Closed
            Me.Server = server
            Me.KeepConnectionOpen = keepConnectionOpen
            InitializeApi()
        End Sub

        <Description("The SSAM server to which event are to be logged."), DefaultValue(GetType(SsamServer), "Development")> _
        Public Property Server() As SsamServer
            Get
                Return m_server
            End Get
            Set(ByVal value As SsamServer)
                ' If connection to a SSAM server is open, disconnect from it and connect to the selected server.
                If m_connectionState <> SsamConnectionStates.Closed Then
                    Disconnect()
                    Connect()
                End If
                m_server = value
            End Set
        End Property

        <Description("Determines whether the connection with SSAM server is to be kept open after logging an event."), DefaultValue(GetType(Boolean), "True")> _
        Public Property KeepConnectionOpen() As Boolean
            Get
                Return m_keepConnectionOpen
            End Get
            Set(ByVal value As Boolean)
                ' We will only close the connection with SSAM if it is already open, but is not to be
                ' kept open after logging an event. However, if the connection is to be kept open after
                ' logging an event, but is closed at present, we will only open it when the first event
                ' is logged and keep if open until it is explicitly closed.
                If value = False AndAlso m_connectionState <> SsamConnectionStates.Closed Then
                    Disconnect()
                End If
                m_keepConnectionOpen = value
            End Set
        End Property

        <Browsable(False)> _
        Public ReadOnly Property ConnectionState() As SsamConnectionStates
            Get
                Return m_connectionState
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property ConnectionString() As String
            Get
                ' Return the connection string for connecting to the selected SSAM server from the config
                ' of the application that is using the API.
                Dim server As String = ""
                Select Case m_server
                    Case SsamServer.Development
                        server = "Development"
                    Case SsamServer.Production
                        server = "Production"
                End Select
                Return CategorizedSettings("ssam")(server).Value()
            End Get
        End Property

        Public Sub Connect()

            If m_connection.State <> System.Data.ConnectionState.Closed Then Disconnect()
            m_connection.ConnectionString = Me.ConnectionString()
            m_connection.Open()
            m_connectionState = SsamConnectionStates.OpenAndInactive

        End Sub

        Public Sub Disconnect()

            If m_connection.State <> System.Data.ConnectionState.Closed Then m_connection.Close()
            m_connectionState = SsamConnectionStates.Closed

        End Sub

        Public Function LogEvent(ByVal newEvent As SsamEvent) As Boolean

            Try
                If m_connectionState = SsamConnectionStates.Closed Then
                    Connect()
                End If
                m_connectionState = SsamConnectionStates.OpenAndActive

                ' Log the event to SSAM.
                ExecuteScalar("sp_LogSsamEvent", m_connection, New Object() {newEvent.EventType(), newEvent.EntityType(), newEvent.EntityId()})

                m_connectionState = SsamConnectionStates.OpenAndInactive
            Catch ex As Exception
                RaiseEvent LogException(ex)
            Finally
                If m_keepConnectionOpen = False Then
                    ' Connection with SSAM is not to be kept open after logging an event, so close it.
                    Disconnect()
                End If
            End Try

        End Function

        Private Sub InitializeApi()

            ' Make sure all of the SSAM connection strings are present in the config file of the 
            ' application using the API.
            CategorizedSettings("ssam").Add("Development", "Server=RGOCSQLD;Database=Ssam;Trusted_Connection=True;", _
                "Connection string for connecting to development SSAM server.", True)
            'CategorizedSettings("ssam").Add("Acceptance", "N/A", _
            '    "Connection string for connecting to acceptance SSAM server.", True)
            CategorizedSettings("ssam").Add("Production", "Server=OPSSAMSQL;Database=Ssam;Trusted_Connection=True;", _
                "Connection string for connecting to production SSAM server.", True)
            SaveSettings()

        End Sub

    End Class

End Namespace