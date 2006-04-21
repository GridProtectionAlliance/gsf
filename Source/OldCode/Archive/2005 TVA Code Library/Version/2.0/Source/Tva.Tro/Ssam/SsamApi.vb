' 04-20-06

Imports System.Text
Imports System.Data
Imports System.Data.SqlClient
Imports System.ComponentModel
Imports System.Drawing
Imports Tva.Services
Imports Tva.Data.Common
Imports Tva.Configuration.Common

Namespace Ssam

#Region " Enumerations "

    Public Enum SsamDatabase        ' Constants for Ssam database connection choices (see arrConnections below)
        SsamProduction = 0        ' Use production database connection
        SsamDevelopment = 1       ' Use development database connection
    End Enum

    '
    'constants for Ssam entity type IDs
    'Note: these IDs must match the IDs in the database, which depends on the order created in SsamSupport.bas
    '
    Public Enum SsamEntityType
        SsamFlowEntity = 1          ' This entity type represents a data-flow
        SsamEquipmentEntity = 2     ' This entity type represents a piece of equipment
        SsamProcessEntity = 3       ' This entity type represents a Process
        SsamSystemEntity = 4        ' This entity type represents a System
        SsamDataEntity = 5          ' This entity type represents a data item like a file or table
    End Enum

    '
    'constants for Ssam event class IDs
    'Note: these IDs must match the IDs in the database, which depends on the order created in SsamSupport.bas
    '
    Public Enum SsamEventType
        SsamSuccessEvent = 1        ' This event reports a successful action on some entity
        SsamWarningEvent = 2        ' This event is a warning that something MAY be going wrong soon
        SsamAlarmEvent = 3          ' This event is an alarm that something HAS ALREADY gone wrong
        SsamErrorEvent = 4          ' This event reports an unexpected ERROR in an application that may or may not matter
        SsamInfoEvent = 5           ' This event reports information that may be of interest to someone
        SsamEscalationEvent = 6     ' This event reports an Alarm notification that was sent that has not been acknowledged
        SsamFailoverEvent = 7       ' This event reports a cluster failover on some process (informational)
        SsamQuitEvent = 8           ' This event halts the Ssam monitoring/dispatching process - remove later? [fixme]?
        SsamSyncEvent = 9           ' This action handles a synchronize-Ssam notification by synchronizing the monitor database with the system-configuration database
        SsamSchedEvent = 10         ' This action handles a terminate-Ssam notification by rescheduling all events
        SsamCatchUpEvent = 11       ' This action makes the monitor skip old events, reschedule, and return to real-time processing
    End Enum

#End Region

    ' Note: might be a good optimization to have ssam-init and ssam-stop subroutines to open and close
    '       the database connection, instead of opening and closing the database connection for each event posted
    '       [fixme]? Also cache the recordset object.
    '

    <ToolboxBitmap(GetType(SsamApi), "Ssam.SsamApi.bmp"), DefaultProperty("Connection")> _
    Public Class SsamApi

        Inherits Component
        Implements ISupportInitialize
        Implements IServiceComponent

        'object state - database connection type, connection object, and command object
        Private m_connection As SqlConnection
        Private m_database As SsamDatabase             'choice of database to connect to
        Private m_operationSuccessful As Boolean                     'true if last operation was successful
        Private m_errorMessage As String                 'last error message received
        Private m_keepConnectionOpen As Boolean = True   'true if connection should always stay open

        'call this to init the Ssam API to use something other than the default development database connection
        Public Sub New(ByVal database As SsamDatabase)
            Me.New(database, True)
        End Sub

        'call this to init the Ssam API to use something other than the default development database connection
        'and to specify whether the connection is keep open or not (default is to keep connection open)
        Public Sub New(ByVal database As SsamDatabase, ByVal keepConnectionOpen As Boolean)
            MyBase.New()
            Me.Database = database
            Me.KeepConnectionOpen = keepConnectionOpen
            Initialize()
        End Sub


        <Browsable(True), Category("Configuration"), Description("Set this value to select the desired Ssam connection."), DefaultValue(SsamDatabase.SsamDevelopment)> _
        Public Property Database() As SsamDatabase
            Get
                Return m_database
            End Get
            Set(ByVal Value As SsamDatabase)
                m_database = Value
                Initialize()
            End Set
        End Property

        <Browsable(False)> _
        Public Property OperationSuccessful() As Boolean
            Get
                Return m_operationSuccessful
            End Get
            Private Set(ByVal value As Boolean)
                If value Then
                    m_errorMessage = ""
                End If
                m_operationSuccessful = value
            End Set
        End Property

        <Browsable(False)> _
        Public Property ErrorMessage() As String
            Get
                Return m_errorMessage
            End Get
            Private Set(ByVal value As String)
                m_operationSuccessful = False
                m_errorMessage = value
            End Set
        End Property

        <Browsable(True), Category("Configuration"), Description("Set this value to True to keep the Ssam api connection open."), DefaultValue(True)> _
        Public Property KeepConnectionOpen() As Boolean
            Get
                Return m_keepConnectionOpen
            End Get
            Set(ByVal Value As Boolean)
                If m_keepConnectionOpen And Not Value Then    'if was leave-open but not it's not, close it
                    CloseConnection()
                End If
                m_keepConnectionOpen = Value
            End Set
        End Property

        ' This function has been deprecated, so it is hidden from the editor - it still works as expected,
        ' but the preferred method for initializing a new connection is to use the Connection property
        ' call this to change the database connection and re/open it
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Sub Initialize()

            If Not DesignMode Then
                ClearSuccess()
                Try
                    CategorizedSettings("ssam").Add("ConnectString.Production", "Data Source=OPSSAMSQL;Initial Catalog=Ssam;Integrated Security=SSPI;Persist Security Info=False;Packet Size=4096", "Ssam production connect string", True)
                    CategorizedSettings("ssam").Add("ConnectString.Development", "Data Source=RGOCSQLD;Initial Catalog=Ssam;Integrated Security=SSPI;Persist Security Info=False;Packet Size=4096", "Ssam developement connect string", True)
                    SaveSettings()

                    If m_connection IsNot Nothing Then
                        If m_connection.State <> ConnectionState.Closed Then
                            ' Close the connection if its not closed.
                            CloseConnection()
                        End If
                        'release the current connection object
                        m_connection = Nothing
                    End If
                    'construct the connection object
                    m_connection = New SqlConnection(GetSsamConnectString(m_database))    'persistent connection for writing Ssam events

                    'open the new connection
                    If KeepConnectionOpen Then
                        OpenConnection()    'only open if it is to be kept open
                    End If

                    Me.OperationSuccessful = True
                Catch ex As Exception
                    Me.ErrorMessage = ex.Message()
                End Try
            End If

        End Sub

        'open the database connection
        Public Sub OpenConnection()
            m_connection.Open()    'assumes oDbConn exists and has a valid connect string
        End Sub

        'close the database connection
        Public Sub CloseConnection()
            ClearSuccess()
            Try
                If Not m_connection.State = ConnectionState.Closed Then
                    m_connection.Close()
                End If
                Me.OperationSuccessful = True
            Catch ex As Exception
                Me.ErrorMessage = ex.Message()
            End Try
        End Sub

        'return Ssam database connect string.
        'note that by default the DEVELOPMENT database connect string is returned.
        'for use by other modules in Testbed utility and external clients.
        'no longer support separate function to get oracle connect string.
        Public Function GetSsamConnectString() As String

            Dim strConnectString As String = ""

            ' We make sure the configuration variables exist only when they are used
            Select Case m_database
                Case SsamDatabase.SsamProduction
                    strConnectString = CategorizedSettings("ssam")("ConnectString.Production").Value()
                Case SsamDatabase.SsamDevelopment
                    strConnectString = CategorizedSettings("ssam")("ConnectString.Development").Value()
            End Select

            Return strConnectString

        End Function

        '
        'this is the base function to log an event to the System Status and Alarm Monitoring (Ssam) system;
        'it returns the elog_ID for the new EventLog row, or raises an error if the transaction failed.
        'returning a 0 is also an error, but should never happen without an error being raised
        '
        'NOTE: The entity ID number/mnemonic is validated by the stored procedure underlying this function.
        '      If an invalid identifier is passed, this function will throw a custom error (#50000 usually)
        '
        'Parameters:
        '   sEntityID   string              entity mnemonic key value OR numeric MonitoredObject ID
        '                                   e.g. 'FL_OPIS_AANSTAFL' might be the mnemonic for MonitoredObject ID#1
        '   iEntityType SsamEntityTypeID    enum value for the type of the source entity, e.g. flow, equipment, etc.
        '   iEventType  SsamEventTypeID     enum value for the type of the event, e.g. warning, alarm, error, etc.
        '   sError      String              the error number encountered, if appropriate; defaults to null
        '   sMsg        String              short description of event, or error message, if appropriate; max 120 chars; defaults to null
        '   sDesc       String              long description of event or detailed error message, if appropriate; max 2GB; defaults to null
        '
        Public Function LogSsamEvent(ByVal eventType As SsamEventType, ByVal entityType As SsamEntityType, _
                ByVal entityID As String, ByVal errorMessage As String, ByVal shortMesssage As String, _
                ByVal longMessage As String) As Integer

            Dim eventLogID As Integer = -1

            Try
                ClearSuccess()

                If Not m_connection.State = ConnectionState.Open Then
                    OpenConnection()
                End If

                'stored procedure returns ID for event row created as elog_ID field in one-row Recordset
                eventLogID = Convert.ToInt32(ExecuteScalar("sp_LogSsamEvent", m_connection, _
                    New Object() {eventType, entityType, entityID, errorMessage, shortMesssage, longMessage}))

                Me.OperationSuccessful = True
            Catch ex As Exception
                Me.ErrorMessage = ex.Message()
            Finally
                If Not KeepConnectionOpen Then
                    CloseConnection()
                End If
            End Try

            Return eventLogID

        End Function

        Private Sub ClearSuccess()
            m_operationSuccessful = False    'assume failure, prove success
            m_errorMessage = ""
        End Sub

#Region " ISupportInitialize Implementation "

        Public Sub BeginInit() Implements System.ComponentModel.ISupportInitialize.BeginInit

            ' We don't need to do anything when the component is being initialized.

        End Sub

        Public Sub EndInit() Implements System.ComponentModel.ISupportInitialize.EndInit

            Initialize()    ' Begin the necessary API initialization after the component has been initialized.

        End Sub

#End Region

#Region " IServiceComponent Implementation "

        <Browsable(False)> _
        Public ReadOnly Property Name() As String Implements Tva.Services.IServiceComponent.Name
            Get
                Return Me.GetType.Name
            End Get
        End Property

        Public Sub ProcessStateChanged(ByVal NewState As Tva.Services.IServiceComponent.ProcessState) Implements Tva.Services.IServiceComponent.ProcessStateChanged

            ' Ssam, when used as a service component, doesn't need to respond to changes in process state

        End Sub

        Public Sub ServiceStateChanged(ByVal NewState As Tva.Services.IServiceComponent.ServiceState) Implements Tva.Services.IServiceComponent.ServiceStateChanged

            ' The Ssam API doesn't have a need to respond to changes in service state

        End Sub

        <Browsable(False)> _
        Public ReadOnly Property Status() As String Implements Tva.Services.IServiceComponent.Status
            Get
                Dim strStatus As New StringBuilder()

                strStatus.Append("Connection String: " & GetSsamConnectString(m_database) & vbCrLf)
                strStatus.Append("    Connection Is: " & IIf(m_connection.State() = ConnectionState.Open, "open", "closed").ToString() & ":" & vbCrLf)

                Return strStatus.ToString()
            End Get
        End Property

#End Region

        'make sure the database connection is closed before going away
        Protected Overrides Sub Finalize()
            Dispose(True)
        End Sub

    End Class

End Namespace
