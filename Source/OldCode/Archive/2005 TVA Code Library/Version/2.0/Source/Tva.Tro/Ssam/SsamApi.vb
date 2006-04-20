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
        SsamProductionDB = 0        ' Use production database connection
        SsamDevelopmentDB = 1       ' Use development database connection
        <Browsable(False), EditorBrowsable(EditorBrowsableState.Never)> _
        SsamClusterDB = 2           ' Use cluster database connection (deprecated)
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

        'shared SQL for stored procedure call, with parameters
        Private Shared sSqlLogEvent As String = "execute sp_LogSsamEvent @EventType, @EntityType, @EntityName, @ErrorCode, @Message, @Description"
        Private Shared sSqlGetProp As String = "execute sp_getProperty @name"
        Private Shared sSqlSetProp As String = "execute sp_setProperty @name, @value"

        'object state - database connection type, connection object, and command object
        Private m_connection As SqlConnection
        Private m_logEventCommand As SqlCommand  'main command to log ssam event
        Private m_getPropertyCommand As SqlCommand   'command to get property
        Private m_setPropertyCommand As SqlCommand   'command to set property
        Private m_ssamDb As SsamDatabase             'choice of database to connect to
        Private m_errorMessage As String                 'last error message received
        Private m_success As Boolean                     'true if last operation was successful
        Private m_keepConnectionOpen As Boolean = True   'true if connection should always stay open
        Private m_lastPropertyTimestamp As String

        'call this to init the Ssam API to use something other than the default development database connection
        Public Sub New(ByVal iConnect As SsamDatabase)
            init(iConnect)
        End Sub

        'call this to init the Ssam API to use something other than the default development database connection
        'and to specify whether the connection is keep open or not (default is to keep connection open)
        Public Sub New(ByVal iConnect As SsamDatabase, ByVal bKeepOpen As Boolean)
            KeepConnectionOpen = bKeepOpen
            init(iConnect)
        End Sub

        'make sure the database connection is closed before going away
        Protected Overrides Sub Finalize()

            Dispose(True)

        End Sub

        Public Sub BeginInit() Implements System.ComponentModel.ISupportInitialize.BeginInit

        End Sub

        Public Sub EndInit() Implements System.ComponentModel.ISupportInitialize.EndInit

            If Not DesignMode Then init(m_ssamDb)

        End Sub

        <Browsable(False)> _
        Public ReadOnly Property Success() As Boolean
            Get
                Return m_success
            End Get
        End Property

        <Browsable(False)> _
        Public ReadOnly Property ErrorMessage() As String
            Get
                Return m_errorMessage
            End Get
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

        <Browsable(True), Category("Configuration"), Description("Set this value to select the desired Ssam connection."), DefaultValue(SsamDatabase.SsamDevelopmentDB)> _
        Public Property Connection() As SsamDatabase
            Get
                Return m_ssamDb
            End Get
            Set(ByVal Value As SsamDatabase)
                m_ssamDb = Value
                If Not DesignMode Then init(m_ssamDb)
            End Set
        End Property

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property LastPropertyTimestamp() As String
            Get
                Return m_lastPropertyTimestamp
            End Get
            Set(ByVal Value As String)
                m_lastPropertyTimestamp = Value
            End Set
        End Property

        ' This function has been deprecated, so it is hidden from the editor - it still works as expected,
        ' but the preferred method for initializing a new connection is to use the Connection property
        ' call this to change the database connection and re/open it
        <EditorBrowsable(EditorBrowsableState.Never)> _
        Public Sub init(ByVal iConnect As SsamDatabase)
            ClearSuccess()
            Try
                If Not m_connection Is Nothing Then
                    If Not m_connection.State = ConnectionState.Closed Then
                        CloseConnection()
                    End If
                    'release the current connection object
                    m_connection = Nothing
                    m_logEventCommand = Nothing
                    m_getPropertyCommand = Nothing
                    m_setPropertyCommand = Nothing
                End If

                'set the connection type
                m_ssamDb = iConnect

                'construct the connection object
                m_connection = New SqlConnection()    'persistent connection for writing Ssam events
                m_logEventCommand = New SqlCommand()    'see ConfigureDbCommands()
                m_getPropertyCommand = New SqlCommand()    'see ConfigureDbCommands()
                m_setPropertyCommand = New SqlCommand()    'see ConfigureDbCommands()

                'open the new connection
                m_connection.ConnectionString = GetSsamConnectString(m_ssamDb)
                If KeepConnectionOpen Then
                    OpenConnection()    'only open if it is to be kept open
                End If

                'reconfigure the sql commands
                ConfigureDbCommands()

                SetSuccess()
            Catch ex As Exception
                SetError(ex.Message())
            End Try
        End Sub

        Private Sub ConfigureDbCommands()
            With m_logEventCommand
                .Connection = m_connection
                .CommandText = sSqlLogEvent
                .Parameters.Clear()
                .Parameters.Add("@EventType", SqlDbType.Int)
                .Parameters.Add("@EntityType", SqlDbType.Int)
                .Parameters.Add("@EntityName", SqlDbType.VarChar)
                .Parameters.Add("@ErrorCode", SqlDbType.VarChar)
                .Parameters.Add("@Message", SqlDbType.VarChar)
                .Parameters.Add("@Description", SqlDbType.Text)
            End With
            With m_getPropertyCommand
                .Connection = m_connection
                .CommandText = sSqlGetProp
                .Parameters.Clear()
                .Parameters.Add("@name", SqlDbType.VarChar)
            End With
            With m_setPropertyCommand
                .Connection = m_connection
                .CommandText = sSqlSetProp
                .Parameters.Clear()
                .Parameters.Add("@name", SqlDbType.VarChar)
                .Parameters.Add("@value", SqlDbType.VarChar)
            End With
        End Sub

        Private Sub SetDbCommandParms(ByVal iEventType As Integer, ByVal iEntityType As Integer, _
        ByVal sEntityID As String, ByVal sError As String, _
        ByVal sMsg As String, ByVal sDesc As String)

            With m_logEventCommand
                .Parameters("@EventType").Value = iEventType
                .Parameters("@EntityType").Value = iEntityType
                .Parameters("@EntityName").Value = sEntityID
                .Parameters("@ErrorCode").Value = DBNull.Value
                If Not sError Is Nothing Then
                    If Not Len(sError.Trim()) <= 0 Then
                        .Parameters("@ErrorCode").Value = sError
                    End If
                End If
                .Parameters("@Message").Value = DBNull.Value
                If Not sMsg Is Nothing Then
                    If Not Len(sMsg.Trim()) <= 0 Then
                        .Parameters("@Message").Value = sMsg
                    End If
                End If
                .Parameters("@Description").Value = DBNull.Value
                If Not sDesc Is Nothing Then
                    If Not Len(sDesc.Trim()) <= 0 Then
                        .Parameters("@Description").Value = sDesc
                    End If
                End If
            End With
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
                SetSuccess()
            Catch ex As Exception
                SetError(ex.Message())
            End Try
        End Sub

        <Obsolete("This method will be deprecated. Please use CloseConnection() instead.")> _
        Public Sub Shutdown()

            CloseConnection()

        End Sub

        'return Ssam database connect string.
        'note that by default the DEVELOPMENT database connect string is returned.
        'for use by other modules in Testbed utility and external clients.
        'no longer support separate function to get oracle connect string.
        Public Function GetSsamConnectString(ByVal iConnect As SsamDatabase) As String

            Dim strConnectString As String = ""

            ' We make sure the configuration variables exist only when they are used
            Select Case iConnect
                Case SsamDatabase.SsamProductionDB
                    'Variables.Create("Ssam.ConnectString.Production", "Data Source=OPSSAMSQL;Initial Catalog=Ssam;Integrated Security=SSPI;Persist Security Info=False;Packet Size=4096", VariableType.Text, "Ssam production connect string", 1000)
                    'Variables("Ssam.ConnectString.Production")
                    CategorizedSettings("ssam").Add("ConnectString.Production", "Data Source=OPSSAMSQL;Initial Catalog=Ssam;Integrated Security=SSPI;Persist Security Info=False;Packet Size=4096", "Ssam production connect string", True)
                    strConnectString = CategorizedSettings("ssam")("ConnectString.Production").Value()
                Case SsamDatabase.SsamDevelopmentDB
                    'Variables.Create("Ssam.ConnectString.Development", "Data Source=RGOCSQLD;Initial Catalog=Ssam;Integrated Security=SSPI;Persist Security Info=False;Packet Size=4096", VariableType.Text, "Ssam developement connect string", 1001)
                    'Variables("Ssam.ConnectString.Development")
                    CategorizedSettings("ssam").Add("ConnectString.Development", "Data Source=RGOCSQLD;Initial Catalog=Ssam;Integrated Security=SSPI;Persist Security Info=False;Packet Size=4096", "Ssam developement connect string", True)
                    strConnectString = CategorizedSettings("ssam")("ConnectString.Development").Value()
                Case SsamDatabase.SsamClusterDB
                    'Variables.Create("Ssam.ConnectString.Cluster", "Data Source=OPSSAMSQL;Initial Catalog=Ssam;Integrated Security=SSPI;Persist Security Info=False;Packet Size=4096", VariableType.Text, "Ssam cluster connect string", 1002)
                    'Variables("Ssam.ConnectString.Cluster")
                    CategorizedSettings("ssam").Add("ConnectString.Cluster", "Data Source=OPSSAMSQL;Initial Catalog=Ssam;Integrated Security=SSPI;Persist Security Info=False;Packet Size=4096", "Ssam cluster connect string", True)
                    strConnectString = CategorizedSettings("ssam")("ConnectString.Cluster").Value()
            End Select

            ' Save config file if anything was added...
            SaveSettings()

            Return strConnectString

        End Function

        Public Function isDbOpen() As Boolean
            Return (m_connection.State = ConnectionState.Open)
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
        Public Function LogSsamEvent( _
          ByVal iEventType As SsamEventType, _
          ByVal iEntityType As SsamEntityType, _
          ByRef sEntityID As String, _
          Optional ByRef sError As String = Nothing, _
          Optional ByRef sMsg As String = Nothing, _
          Optional ByRef sDesc As String = Nothing) As Integer

            Dim iID As Integer = 0

            ClearSuccess()
            Try
                SetDbCommandParms(iEventType, iEntityType, sEntityID, sError, sMsg, sDesc)

                If Not m_connection.State = ConnectionState.Open Then
                    OpenConnection()
                End If
                'stored procedure returns ID for event row created as elog_ID field in one-row Recordset
                iID = CInt(m_logEventCommand.ExecuteScalar())
                If Not KeepConnectionOpen Then
                    CloseConnection()
                End If
                SetSuccess()
            Catch ex As Exception
                SetError(ex.Message())
            End Try

            Return iID
        End Function

        'set property; returns ID
        Public Function SetSsamProperty(ByVal sName As String, ByVal sValue As String) As Integer
            Dim iID As Integer = 0

            ClearSuccess()
            Try
                m_setPropertyCommand.Parameters("@name").Value = sName
                m_setPropertyCommand.Parameters("@value").Value = sValue

                If Not m_connection.State = ConnectionState.Open Then
                    OpenConnection()
                End If
                'stored procedure returns ID for property row as PropertyId field in one-row Recordset
                iID = CInt(m_setPropertyCommand.ExecuteScalar())
                SetSuccess()
            Catch ex As Exception
                SetError(ex.Message())
            Finally
                If Not KeepConnectionOpen Then
                    CloseConnection()
                End If
            End Try

            Return iID
        End Function

        'returns value directly, puts timestamp into LastPropertyTimestamp property as string
        Public Function GetSsamProperty(ByVal sName As String) As String
            Dim sValue As String = ""

            ClearSuccess()
            Try
                m_getPropertyCommand.Parameters("@name").Value = sName

                If Not m_connection.State = ConnectionState.Open Then
                    OpenConnection()
                End If
                'stored procedure returns ID for property row as PropertyId field in one-row Recordset
                Dim dr As SqlDataReader = m_getPropertyCommand.ExecuteReader(CommandBehavior.SingleRow)
                If dr.Read() Then
                    sValue = dr("value").ToString()
                    LastPropertyTimestamp = dr("Timestamp").ToString()
                Else    'property not found
                    sValue = Nothing
                    LastPropertyTimestamp = Nothing
                End If
                dr.Close()
                SetSuccess()
            Catch ex As Exception
                SetError(ex.Message())
            Finally
                If Not KeepConnectionOpen Then
                    CloseConnection()
                End If
            End Try

            Return sValue    'may be nothing if property undefined
        End Function

        Private Sub ClearSuccess()
            m_success = False    'assume failure, prove success
            m_errorMessage = ""
        End Sub

        Private Sub SetSuccess()
            m_success = True
            m_errorMessage = ""
        End Sub

        Private Sub SetError(ByVal sMsg As String)
            m_success = False
            m_errorMessage = sMsg
        End Sub

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

                strStatus.Append("Connection String: " & GetSsamConnectString(m_ssamDb) & vbCrLf)
                strStatus.Append("    Connection Is: " & IIf(isDbOpen(), "open", "closed").ToString() & ":" & vbCrLf)

                Return strStatus.ToString()
            End Get
        End Property

    End Class

End Namespace
