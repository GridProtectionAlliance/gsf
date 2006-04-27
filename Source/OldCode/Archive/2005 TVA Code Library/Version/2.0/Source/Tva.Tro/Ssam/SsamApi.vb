'*******************************************************************************************************
'  Tva.Tro.Ssam.SsamApi.vb - SSAM API
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: Pinal C Patel, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: pcpatel@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  04/24/2006 - Pinal C Patel
'       Original version of source code generated
'
'*******************************************************************************************************

Imports System.Data.SqlClient
Imports System.ComponentModel
Imports Tva.Data.Common
Imports Tva.Configuration.Common

Namespace Ssam

    ''' <summary>
    ''' Defines the mechanism for communicating with SSAM programatically.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class SsamApi

        Private m_server As SsamServer
        Private m_keepConnectionOpen As Boolean
        Private m_connection As SqlConnection
        Private m_connectionState As SsamConnectionState

        ''' <summary>
        ''' The SSAM server with which we want to connect.
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum SsamServer As Integer
            ''' <summary>
            ''' The development SSAM server.
            ''' </summary>
            ''' <remarks></remarks>
            Development
            <EditorBrowsable(EditorBrowsableState.Never), Browsable(False)> _
            Acceptance
            ''' <summary>
            ''' The production SSAM server.
            ''' </summary>
            ''' <remarks></remarks>
            Production
        End Enum

        ''' <summary>
        ''' The current state of the connection with the SSAM server.
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum SsamConnectionState As Integer
            ''' <summary>
            ''' Connection with the SSAM server is open and some activity is in progress.
            ''' </summary>
            ''' <remarks></remarks>
            OpenAndActive
            ''' <summary>
            ''' Connection with the SSAM server is open, but no activity is in progress.
            ''' </summary>
            ''' <remarks></remarks>
            OpenAndInactive
            ''' <summary>
            ''' Connection with the SSAM server is closed.
            ''' </summary>
            ''' <remarks></remarks>
            Closed
        End Enum

        ''' <summary>
        ''' Initializes a default instance of Tva.Tro.Ssam.SsamApi.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()
            MyClass.New(SsamServer.Development)
        End Sub

        ''' <summary>
        ''' Initializes a instance of Tva.Tro.Ssam.SsamApi with the specified information.
        ''' </summary>
        ''' <param name="server">One of the Tva.Tro.Ssam.SsamApi.SsamServer values.</param>
        ''' <remarks></remarks>
        Public Sub New(ByVal server As SsamServer)
            MyClass.New(server, True)
        End Sub

        ''' <summary>
        ''' Initializes a instance of Tva.Tro.Ssam.SsamApi with the specified information.
        ''' </summary>
        ''' <param name="server">One of the Tva.Tro.Ssam.SsamApi.SsamServer values.</param>
        ''' <param name="keepConnectionOpen">
        ''' True if connection with the SSAM server is to be kept open after the first event is loggged for 
        ''' any consecutive events that will follow; otherwise False.
        ''' </param>
        ''' <remarks></remarks>
        Public Sub New(ByVal server As SsamServer, ByVal keepConnectionOpen As Boolean)
            MyClass.New(server, keepConnectionOpen, True)
        End Sub

        ''' <summary>
        ''' This constructor is for internal use only.
        ''' </summary>
        ''' <param name="server">One of the Tva.Tro.Ssam.SsamApi.SsamServer values.</param>
        ''' <param name="keepConnectionOpen">
        ''' True if connection with the SSAM server is to be kept open after the first event is loggged for 
        ''' any consecutive events that will follow; otherwise False.
        ''' </param>
        ''' <param name="initializeApi">
        ''' True to update the configuration file of the client application with the connection strings required 
        ''' for connecting with any of the SSAM servers.
        ''' </param>
        ''' <remarks></remarks>
        Friend Sub New(ByVal server As SsamServer, ByVal keepConnectionOpen As Boolean, ByVal initializeApi As Boolean)
            MyBase.New()
            m_connection = New SqlConnection()
            MyClass.ConnectionState = SsamConnectionState.Closed
            MyClass.Server = server
            MyClass.KeepConnectionOpen = keepConnectionOpen
            If initializeApi Then Initialize()
        End Sub

        ''' <summary>
        ''' Gets or sets the SSAM server with which we are connected.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The SSAM server with which we are connected.</returns>
        ''' <remarks></remarks>
        Public Property Server() As SsamServer
            Get
                Return m_server
            End Get
            Set(ByVal value As SsamServer)
                ' If connection to a SSAM server is open, disconnect from it and connect to the selected server.
                If MyClass.ConnectionState() <> SsamConnectionState.Closed Then
                    Disconnect()
                    Connect()
                End If
                m_server = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a boolean value indicating whether connection with the SSAM server is to be kept open after 
        ''' the first event is loggged for any consecutive events that will follow.
        ''' </summary>
        ''' <value></value>
        ''' <returns>
        ''' True if connection with the SSAM server is to be kept open after the first event is loggged for any 
        ''' consecutive events that will follow; otherwise False.
        ''' </returns>
        ''' <remarks></remarks>
        Public Property KeepConnectionOpen() As Boolean
            Get
                Return m_keepConnectionOpen
            End Get
            Set(ByVal value As Boolean)
                ' We will only close the connection with SSAM if it is already open, but is not to be
                ' kept open after logging an event. However, if the connection is to be kept open after
                ' logging an event, but is closed at present, we will only open it when the first event
                ' is logged and keep if open until it is explicitly closed.
                If value = False AndAlso MyClass.ConnectionState() <> SsamConnectionState.Closed Then
                    Disconnect()
                End If
                m_keepConnectionOpen = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the current state of the connection with SSAM server.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The current state of the connection with SSAM server.</returns>
        ''' <remarks></remarks>
        Public Property ConnectionState() As SsamConnectionState
            Get
                If m_connection.State <> System.Data.ConnectionState.Closed Then
                    Try
                        ' Verify that connection with the SSAM database is active by switching to
                        ' the SSAM database. This will raise an exception if the operation cannot
                        ' be performed due to network or database server issues.
                        m_connection.ChangeDatabase(m_connection.Database())
                    Catch ex As Exception
                        m_connectionState = SsamConnectionState.Closed
                    End Try
                End If
                Return m_connectionState
            End Get
            Private Set(ByVal value As SsamConnectionState)
                m_connectionState = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the connection string used for connecting with the current SSAM server.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The connection string used for connecting with the current SSAM server.</returns>
        ''' <remarks></remarks>
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

        ''' <summary>
        ''' Connects with the selected SSAM server.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Connect()

            If m_connection.State <> System.Data.ConnectionState.Closed Then Disconnect()
            m_connection.ConnectionString = MyClass.ConnectionString()
            m_connection.Open()
            MyClass.ConnectionState = SsamConnectionState.OpenAndInactive

        End Sub

        ''' <summary>
        ''' Disconnects from the connected SSAM server.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Disconnect()

            If m_connection.State <> System.Data.ConnectionState.Closed Then m_connection.Close()
            MyClass.ConnectionState = SsamConnectionState.Closed

        End Sub

        ''' <summary>
        ''' Logs the specified Tva.Tro.Ssam.SsamEvent to the current SSAM server.
        ''' </summary>
        ''' <param name="newEvent">The Tva.Tro.Ssam.SsamEvent to log.</param>
        ''' <returns>True if the event is logged successfully; otherwise False.</returns>
        ''' <remarks></remarks>
        Public Function LogEvent(ByVal newEvent As SsamEvent) As Boolean

            Dim logResult As Boolean = False
            Try
                If MyClass.ConnectionState() = SsamConnectionState.Closed Then
                    Connect()
                End If
                MyClass.ConnectionState = SsamConnectionState.OpenAndActive

                ' Log the event to SSAM.
                ExecuteScalar("sp_LogSsamEvent", m_connection, _
                    New Object() {newEvent.EventType(), newEvent.EntityType(), newEvent.EntityId(), newEvent.ErrorNumber(), newEvent.Message(), newEvent.Description()})
                MyClass.ConnectionState = SsamConnectionState.OpenAndInactive
                logResult = True
            Catch ex As Exception
                logResult = False
                Throw
            Finally
                If m_keepConnectionOpen = False Then
                    ' Connection with SSAM is not to be kept open after logging an event, so close it.
                    Disconnect()
                End If
            End Try
            Return logResult

        End Function

        ''' <summary>
        ''' Updates the configuration file of the client application with the connection strings required 
        ''' for connecting with any of the SSAM servers.
        ''' </summary>
        ''' <remarks>This method is for internal use only and must be called be using the API.</remarks>
        Friend Sub Initialize()

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