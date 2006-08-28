'*******************************************************************************************************
'  Tva.Tro.Ssam.SsamApi.vb - SSAM API
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
'  04/24/2006 - Pinal C. Patel
'       Original version of source code generated
'  08/25/2006 - Pinal C. Patel
'       Moved SsamServer and SsamConnectionState enumerations to Enumerations.vb.
'*******************************************************************************************************

Imports System.Data.SqlClient
Imports System.ComponentModel
Imports Tva.Data.Common
Imports Tva.Configuration.Common

Namespace Ssam

    ''' <summary>
    ''' Defines the mechanism for communicating with SSAM programatically.
    ''' </summary>
    <TypeConverter(GetType(ExpandableObjectConverter))> _
    Public Class SsamApi
        Implements IDisposable

        Private m_server As SsamServer
        Private m_keepConnectionOpen As Boolean
        Private m_persistConnectionStrings As Boolean
        Private m_connectionState As SsamConnectionState
        Private m_developmentConnectionString As String
        Private m_productionConnectionString As String
        Private m_connection As SqlConnection

        Private Const ConfigurationElement As String = "Ssam"

        ''' <summary>
        ''' Initializes a default instance of Tva.Tro.Ssam.SsamApi.
        ''' </summary>
        Public Sub New()
            MyClass.New(SsamServer.Development)
        End Sub

        ''' <summary>
        ''' Initializes a instance of Tva.Tro.Ssam.SsamApi with the specified information.
        ''' </summary>
        ''' <param name="server">One of the Tva.Tro.Ssam.SsamApi.SsamServer values.</param>
        Public Sub New(ByVal server As SsamServer)
            MyClass.New(server, True)
        End Sub

        ''' <summary>
        ''' Initializes a instance of Tva.Tro.Ssam.SsamApi with the specified information.
        ''' </summary>
        ''' <param name="server">One of the Tva.Tro.Ssam.SsamApi.SsamServer values.</param>
        ''' <param name="keepConnectionOpen">
        ''' True if connection with the SSAM server is to be kept open after the first event is logged for 
        ''' any consecutive events that will follow; otherwise False.
        ''' </param>
        Public Sub New(ByVal server As SsamServer, ByVal keepConnectionOpen As Boolean)
            MyClass.New(server, keepConnectionOpen, True)
        End Sub

        Public Sub New(ByVal server As SsamServer, ByVal keepConnectionOpen As Boolean, _
                ByVal persistConnectionStrings As Boolean)
            MyClass.New(server, keepConnectionOpen, persistConnectionStrings, True)
        End Sub

        ''' <summary>
        ''' Gets or sets the SSAM server with which we are connected.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The SSAM server with which we are connected.</returns>
        <Category("Configuration"), Description("The SSAM server to which event are to be logged."), DefaultValue(GetType(SsamServer), "Development")> _
        Public Property Server() As SsamServer
            Get
                Return m_server
            End Get
            Set(ByVal value As SsamServer)
                ' If connection to a SSAM server is open, disconnect from it and connect to the selected server.
                If m_connectionState <> SsamConnectionState.Closed Then
                    Disconnect()
                    Connect()
                End If
                m_server = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a boolean value indicating whether connection with the SSAM server is to be kept open after 
        ''' the first event is logged for any consecutive events that will follow.
        ''' </summary>
        ''' <value></value>
        ''' <returns>
        ''' True if connection with the SSAM server is to be kept open after the first event is logged for any 
        ''' consecutive events that will follow; otherwise False.
        ''' </returns>
        <Category("Configuration"), Description("Determines whether the connection with SSAM server is to be kept open after logging an event."), DefaultValue(GetType(Boolean), "True")> _
        Public Property KeepConnectionOpen() As Boolean
            Get
                Return m_keepConnectionOpen
            End Get
            Set(ByVal value As Boolean)
                ' We will only close the connection with SSAM if it is already open, but is not to be
                ' kept open after logging an event. However, if the connection is to be kept open after
                ' logging an event, but is closed at present, we will only open it when the first event
                ' is logged and keep if open until it is explicitly closed.
                If value = False AndAlso m_connectionState <> SsamConnectionState.Closed Then
                    Disconnect()
                End If
                m_keepConnectionOpen = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a boolean value indicating whether the connection strings for connecting to the SSAM server
        ''' are to be saved to the application configuration file when 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        <DefaultValue(GetType(Boolean), "True")> _
        Public Property PersistConnectionStrings() As Boolean
            Get
                Return m_persistConnectionStrings
            End Get
            Set(ByVal value As Boolean)
                m_persistConnectionStrings = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the current state of the connection with SSAM server.
        ''' </summary>
        ''' <value></value>
        ''' <returns>The current state of the connection with SSAM server.</returns>
        <Browsable(False)> _
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
        <Browsable(False)> _
        Public Property ConnectionString() As String
            Get
                ' Return the connection string for connecting to the selected SSAM server.
                Select Case m_server
                    Case SsamServer.Development
                        Return m_developmentConnectionString
                    Case SsamServer.Production
                        Return m_productionConnectionString
                End Select
                Return ""
            End Get
            Set(ByVal value As String)
                Select Case m_server
                    Case SsamServer.Development
                        m_developmentConnectionString = value
                    Case SsamServer.Production
                        m_productionConnectionString = value
                End Select
            End Set
        End Property

        ''' <summary>
        ''' Connects with the selected SSAM server.
        ''' </summary>
        Public Sub Connect()

            CheckDisposed()

            If m_connection.State <> System.Data.ConnectionState.Closed Then Disconnect()
            m_connection.ConnectionString = Me.ConnectionString
            m_connection.Open()
            m_connectionState = SsamConnectionState.OpenAndInactive

        End Sub

        ''' <summary>
        ''' Disconnects from the connected SSAM server.
        ''' </summary>
        Public Sub Disconnect()

            CheckDisposed()

            If m_connection.State <> System.Data.ConnectionState.Closed Then m_connection.Close()
            If m_persistConnectionStrings Then
                ' Save SSAM connection strings to the config file.
                CategorizedSettings(ConfigurationElement).Add("Development", m_developmentConnectionString, _
                    "Connection string for connecting to development SSAM server.", True)
                CategorizedSettings(ConfigurationElement).Add("Production", m_productionConnectionString, _
                    "Connection string for connecting to production SSAM server.", True)
                SaveSettings()
            End If
            m_connectionState = SsamConnectionState.Closed

        End Sub

        ''' <summary>
        ''' Logs the specified Tva.Tro.Ssam.SsamEvent to the current SSAM server.
        ''' </summary>
        ''' <param name="newEvent">The Tva.Tro.Ssam.SsamEvent to log.</param>
        ''' <returns>True if the event is logged successfully; otherwise False.</returns>
        Public Function LogEvent(ByVal newEvent As SsamEvent) As Boolean

            CheckDisposed()

            Try
                If Me.ConnectionState = SsamConnectionState.Closed Then Connect()
                m_connectionState = SsamConnectionState.OpenAndActive

                ' Log the event to SSAM.
                ExecuteScalar("sp_LogSsamEvent", m_connection, _
                    New Object() {newEvent.EventType(), newEvent.EntityType(), newEvent.EntityId(), newEvent.ErrorNumber(), newEvent.Message(), newEvent.Description()})

                m_connectionState = SsamConnectionState.OpenAndInactive
                Return True
            Catch ex As Exception
                Throw
            Finally
                If Not m_keepConnectionOpen Then
                    ' Connection with SSAM is not to be kept open after logging an event, so close it.
                    Disconnect()
                End If
            End Try

        End Function

        ''' <summary>
        ''' Attempts to free resources and perform other cleanup operations before the Tva.Tro.Ssam.SsamApi 
        ''' is reclaimed by garbage collection.
        ''' </summary>
        Protected Overrides Sub Finalize()

            Dispose(False)

        End Sub

#Region " Internal Code "

        ''' <summary>
        ''' This constructor is for internal use only.
        ''' </summary>
        ''' <param name="server">One of the Tva.Tro.Ssam.SsamApi.SsamServer values.</param>
        ''' <param name="keepConnectionOpen">
        ''' True if connection with the SSAM server is to be kept open after the first event is logged for 
        ''' any consecutive events that will follow; otherwise False.
        ''' </param>
        ''' <param name="persistConnectionStrings"></param>
        ''' <param name="initializeApi">
        ''' True to update the configuration file of the client application with the connection strings required 
        ''' for connecting with any of the SSAM servers.
        ''' </param>
        Friend Sub New(ByVal server As SsamServer, ByVal keepConnectionOpen As Boolean, ByVal persistConnectionStrings As Boolean, _
                 ByVal initializeApi As Boolean)
            MyBase.New()
            m_server = server
            m_keepConnectionOpen = keepConnectionOpen
            m_persistConnectionStrings = persistConnectionStrings
            m_connectionState = SsamConnectionState.Closed
            m_developmentConnectionString = "Server=RGOCSQLD;Database=Ssam;Trusted_Connection=True;"
            m_productionConnectionString = "Server=OPSSAMSQL;Database=Ssam;Trusted_Connection=True;"
            m_connection = New SqlConnection()
            If initializeApi Then Initialize()
        End Sub

        ''' <summary>
        ''' Initializes the connection strings used for connecting to the SSAM server.
        ''' </summary>
        ''' <remarks>This method is for internal use only and must be called before using the API.</remarks>
        Friend Sub Initialize()

            CheckDisposed()

            If m_persistConnectionStrings Then
                ' Initialize the connection string variables from the config file only if the connection strings 
                ' are present (Connection strings are persisted when PersistConnectionStrings = True).
                Dim persistedCS As String = ""

                persistedCS = DefaultConfigFile.CategorizedSettings(ConfigurationElement)("Development").Value
                If Not String.IsNullOrEmpty(persistedCS) Then m_developmentConnectionString = persistedCS

                persistedCS = DefaultConfigFile.CategorizedSettings(ConfigurationElement)("Production").Value
                If Not String.IsNullOrEmpty(persistedCS) Then m_productionConnectionString = persistedCS
            End If

        End Sub

#End Region

#Region " IDisposable Implementation "

        Private m_disposed As Boolean = False

        ''' <summary>
        ''' This is a helper method that check whether the object is disposed and raises a System.ObjectDisposedException if it is.
        ''' </summary>
        ''' <remarks></remarks>
        Protected Sub CheckDisposed()

            If m_disposed Then Throw New ObjectDisposedException(Me.GetType.FullName())

        End Sub

        ''' <summary>
        ''' Releases the unmanaged resources used by the Tva.Tro.Ssam.SsamApi and and optionally releases the 
        ''' managed resources.
        ''' </summary>
        ''' <param name="disposing">
        ''' True to release both managed and unmanaged resources; False to release only unmanaged resources. 
        ''' </param>
        ''' <remarks></remarks>
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not m_disposed Then  ' Object jas not been disposed yet.
                If disposing Then
                    Disconnect()    ' Close connection with the SSAM server.
                End If
            End If
            m_disposed = True   ' Mark the object as been disposed.
        End Sub

#Region " IDisposable Support "
        ''' <summary>
        ''' Releases all resources used by the Tva.Tro.Ssam.SsamApi.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

#End Region

    End Class

End Namespace