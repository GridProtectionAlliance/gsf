'	API for SSAM
'
'	This class provides an interface to the SSAM event-reporting stored
'	procedure, including some enum types to prevent data errors and 
'	support IntelliSense(tm) menus in the Visual Studio .NET IDE.
'
'	INSTRUCTIONS
'
'	1.	Make an instance of this class in your program specifying which database
'		connection to use; if you do not specify a database connection the 
'		default is the MS-SQL development database
'	2.	Report success on processes on start-up; report success on flows after
'		completion of each flow. Use the LogSsamEvent function to report success.
'	3.	call Shutdown() on the instance of this class when the application is
'		done reporting to Ssam, i.e. when the application ends. If Shutdown() is
'		not called, the database connection may remain open until the instance is
'		finalized.
'
'	EXAMPLE
'
'		Private iSsamLogId As Integer = 0
'		Private Shared oSsamApi As New SsamApi(SsamDatabase.ssamDevelopmentDB)
'		iSsamLogId = oSsamApi.LogSsamEvent(SsamEventTypeID.ssamSuccessEvent, _
'			SsamEntityTypeID.ssamProcessEntity, "SSAM_MONITOR")
'		oSsamApi.Shutdown()
'
'	NOTES
'
'	To use this class, make an instance of it in the state of your application; the
'	instance may be declared Shared if only one Ssam connection if desired. Note that
'	each instance of SsamApi will open its own connection to the specified database.
'
'	Processes should report success on start-up, while flows should report success
'	only on completion - this lets us know when a process started but failed to
'	complete vs. a process not starting.
'
'	To report success on a monitored object, use the LogSsamEvent function, which
'	returns the log-entry Id number > 0 if successful, or 0 if unsuccessful.
'
'	By default, a SsamApi object will open a connection to the designated SSAM database
'	and leave the connection open until the SsamApi object is shut down or destroyed.
'	This is under the assumption that SSAM events will be posted by the application using
'	the SsamApi object very frequently, so we leave the database connection open all the
'	time. If SSAM events will be posted infrequently, set KeepConnectionOpen to false
'	by using the property or the two-argument constructor
'
'	Note that all of the nontrivial public methods including the constructors set 
'	a read-only property named Success to reflect whether they worked or not (the
'	constructors open the database connection, so they may fail in that regard).
'	If unsuccessful, the read-only property named ErrorMessage will contain the
'	error message received.
'
'	For more examples, see the test class testSsamApi.
'

Imports System.Data.SqlClient
Imports System.Data.OleDb

Public Enum SsamDatabase 'constants for SSAM database connection choices (see arrConnections below)
	ssamProductionDB = 0	 'use production database connection
	ssamDevelopmentDB = 1	   'use development database connection
	ssamClusterDB = 2	 'use cluster database connection
End Enum

'
'constants for SSAM entity type IDs
'Note: these IDs must match the IDs in the database, which depends on the order created in SsamSupport.bas
'
Public Enum SsamEntityTypeID
	ssamFlowEntity = 1	   'This entity type represents a data-flow
	ssamEquipmentEntity = 2	 'This entity type represents a piece of equipment
	ssamProcessEntity = 3	   'This entity type represents a Process
	ssamSystemEntity = 4	 'This entity type represents a System
	ssamDataEntity = 5	   'This entity type represents a data item like a file or table
End Enum

'
'constants for SSAM event class IDs
'Note: these IDs must match the IDs in the database, which depends on the order created in SsamSupport.bas
'
Public Enum SsamEventTypeID
	ssamSuccessEvent = 1	 'This event reports a successful action on some entity
	ssamWarningEvent = 2	 'This event is a warning that something MAY be going wrong soon
	ssamAlarmEvent = 3	   'This event is an alarm that something HAS ALREADY gone wrong
	ssamErrorEvent = 4	   'This event reports an unexpected ERROR in an application that may or may not matter
	ssamInfoEvent = 5	 'This event reports information that may be of interest to someone
	ssamEscalationEvent = 6	 'This event reports an Alarm notification that was sent that has not been acknowledged
	ssamFailoverEvent = 7	   'This event reports a cluster failover on some process (informational)
	ssamQuitEvent = 8	 'This event halts the SSAM monitoring/dispatching process - remove later? [fixme]?
	ssamSyncEvent = 9	 'This action handles a synchronize-SSAM notification by synchronizing the monitor database with the system-configuration database
	ssamSchedEvent = 10	  'This action handles a terminate-SSAM notification by rescheduling all events
	ssamCatchUpEvent = 11	   'This action makes the monitor skip old events, reschedule, and return to real-time processing
End Enum

' Note: might be a good optimization to have ssam-init and ssam-stop subroutines to open and close
'       the database connection, instead of opening and closing the database connection for each event posted
'       [fixme]? Also cache the recordset object.
'

Public Class SsamApi
	'
	'TODO: we'd like to move these to the app.config file, or to another config file, or perhaps a factory class
	'
	Private Shared sDB_CONN_PRODUCTION As String = "Integrated Security=SSPI;" & _
	  "Persist Security Info=False;Initial Catalog=SSAM;Data Source=OPSSAMSQL;packet size=4096"

	Private Shared sDB_CONN_DEVELOPMENT As String = _
	  "data source=RGOCSQLD;initial catalog=SSAM;integrated security=SSPI;" & _
	  "persist security info=False;packet size=4096"

	Private Shared sDB_CONN_CLUSTER As String = "Integrated Security=SSPI;" & _
	  "Persist Security Info=False;Initial Catalog=SSAM;Data Source=OPSSAMSQL;packet size=4096"

	Private Shared arrConnections As String() = { _
	 sDB_CONN_PRODUCTION, _
	 sDB_CONN_DEVELOPMENT, _
	 sDB_CONN_CLUSTER _
	}

	'shared SQL for stored procedure call, with parameters
	Private Shared sSqlLogEvent As String = "execute sp_LogSsamEvent @EventType, @EntityType, @EntityName, @ErrorCode, @Message, @Description"
	Private Shared sSqlGetProp As String = "execute sp_getProperty @name"
	Private Shared sSqlSetProp As String = "execute sp_setProperty @name, @value"

	'object state - database connection type, connection object, and command object
	Private oDbConn As SqlClient.SqlConnection
	Private oDbCmdLogEvent As SqlClient.SqlCommand	'main command to log ssam event
	Private oDbCmdGetProp As SqlClient.SqlCommand	'command to get property
	Private oDbCmdSetProp As SqlClient.SqlCommand	'command to set property
	Private iConnection As SsamDatabase	  'choice of database to connect to
	Private sErrorMessage As String	'last error message received
	Private bSuccess As Boolean	'true if last operation was successful
	Private bKeepConnectionOpen As Boolean = True	'true if connection should always stay open
	Private sLastPropertyTimestamp As String

	Public ReadOnly Property Success() As Boolean
		Get
			Return bSuccess
		End Get
	End Property

	Public ReadOnly Property ErrorMessage() As String
		Get
			Return sErrorMessage
		End Get
	End Property

	Public Property KeepConnectionOpen() As Boolean
		Get
			Return bKeepConnectionOpen
		End Get
		Set(ByVal Value As Boolean)
			If bKeepConnectionOpen And Not Value Then			 'if was leave-open but not it's not, close it
				CloseConnection()
			End If
			bKeepConnectionOpen = Value
		End Set
	End Property

	Public Property LastPropertyTimestamp() As String
		Get
			Return sLastPropertyTimestamp
		End Get
		Set(ByVal Value As String)
			sLastPropertyTimestamp = Value
		End Set
	End Property

	Public Sub New()	'default constructor connects to default database
		init(SsamDatabase.ssamDevelopmentDB)
	End Sub

	'call this to init the SSAM API to use something other than the default development database connection
	Public Sub New(ByVal iConnect As SsamDatabase)
		init(iConnect)
	End Sub

	'call this to init the SSAM API to use something other than the default development database connection
	'and to specify whether the connection is keep open or not (default is to keep connection open)
	Public Sub New(ByVal iConnect As SsamDatabase, ByVal bKeepOpen As Boolean)
		KeepConnectionOpen = bKeepOpen
		init(iConnect)
	End Sub

	Private Sub ClearSuccess()
		bSuccess = False		  'assume failure, prove success
		sErrorMessage = ""
	End Sub

	Private Sub SetSuccess()
		bSuccess = True
		sErrorMessage = ""
	End Sub

	Private Sub SetError(ByVal sMsg As String)
		bSuccess = False
		sErrorMessage = sMsg
	End Sub

	'call this to change the database connection and re/open it
	Public Sub init(ByVal iConnect As SsamDatabase)
		ClearSuccess()
		Try
			If Not oDbConn Is Nothing Then
				If Not oDbConn.State = ConnectionState.Closed Then
					Shutdown()
				End If
				'release the current connection object
				oDbConn = Nothing
				oDbCmdLogEvent = Nothing
				oDbCmdGetProp = Nothing
				oDbCmdSetProp = Nothing
			End If

			'set the connection type
			iConnection = iConnect

			'construct the connection object
			oDbConn = New SqlClient.SqlConnection()			 'persistent connection for writing SSAM events
			oDbCmdLogEvent = New SqlClient.SqlCommand()			 'see ConfigureDbCommands()
			oDbCmdGetProp = New SqlClient.SqlCommand()			 'see ConfigureDbCommands()
			oDbCmdSetProp = New SqlClient.SqlCommand()			 'see ConfigureDbCommands()

			'open the new connection
			oDbConn.ConnectionString = GetSSAMConnectString(iConnection)
			If KeepConnectionOpen Then
				OpenConnection()				'only open if it is to be kept open
			End If

			'reconfigure the sql commands
			ConfigureDbCommands()

			SetSuccess()
		Catch ex As Exception
			SetError(ex.Message())
		End Try
	End Sub

	Private Sub ConfigureDbCommands()
		With oDbCmdLogEvent
			.Connection = oDbConn
			.CommandText = sSqlLogEvent
			.Parameters.Clear()
			.Parameters.Add("@EventType", SqlDbType.Int)
			.Parameters.Add("@EntityType", SqlDbType.Int)
			.Parameters.Add("@EntityName", SqlDbType.VarChar)
			.Parameters.Add("@ErrorCode", SqlDbType.VarChar)
			.Parameters.Add("@Message", SqlDbType.VarChar)
			.Parameters.Add("@Description", SqlDbType.Text)
		End With
		With oDbCmdGetProp
			.Connection = oDbConn
			.CommandText = sSqlGetProp
			.Parameters.Clear()
			.Parameters.Add("@name", SqlDbType.VarChar)
		End With
		With oDbCmdSetProp
			.Connection = oDbConn
			.CommandText = sSqlSetProp
			.Parameters.Clear()
			.Parameters.Add("@name", SqlDbType.VarChar)
			.Parameters.Add("@value", SqlDbType.VarChar)
		End With
	End Sub

	Private Sub SetDbCommandParms(ByVal iEventType As Integer, ByVal iEntityType As Integer, _
	ByVal sEntityID As String, ByVal sError As String, _
	ByVal sMsg As String, ByVal sDesc As String)

		With oDbCmdLogEvent
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
		oDbConn.Open()		  'assumes oDbConn exists and has a valid connect string
	End Sub

	'close the database connection
	Public Sub CloseConnection()
		ClearSuccess()
		Try
			If Not oDbConn.State = ConnectionState.Closed Then
				oDbConn.Close()
			End If
			SetSuccess()
		Catch ex As Exception
			SetError(ex.Message())
		End Try
	End Sub

	'call this to close the database connection and do other cleanup chores if any
	Public Sub Shutdown()
		CloseConnection()
	End Sub

	'return SSAM database connect string.
	'note that by default the DEVELOPMENT database connect string is returned.
	'for use by other modules in Testbed utility and external clients.
	'no longer support separate function to get oracle connect string.
	Public Function GetSSAMConnectString(ByVal iConnect As SsamDatabase) As String
		Return arrConnections(iConnect)
	End Function

	Public Function isDbOpen() As Boolean
		Return (oDbConn.State = ConnectionState.Open)
	End Function

	'
	'this is the base function to log an event to the System Status and Alarm Monitoring (SSAM) system;
	'it returns the elog_ID for the new EventLog row, or raises an error if the transaction failed.
	'returning a 0 is also an error, but should never happen without an error being raised
	'
	'NOTE: The entity ID number/mnemonic is validated by the stored procedure underlying this function.
	'      If an invalid identifier is passed, this function will throw a custom error (#50000 usually)
	'
	'TODO: this function will need to be cluster-aware later
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
	  ByVal iEventType As SsamEventTypeID, _
	  ByVal iEntityType As SsamEntityTypeID, _
	  ByRef sEntityID As String, _
	  Optional ByRef sError As String = Nothing, _
	  Optional ByRef sMsg As String = Nothing, _
	  Optional ByRef sDesc As String = Nothing) As Integer

		Dim iID As Integer = 0

		ClearSuccess()
		Try
			SetDbCommandParms(iEventType, iEntityType, sEntityID, sError, sMsg, sDesc)

			If Not oDbConn.State = ConnectionState.Open Then
				OpenConnection()
			End If
			'stored procedure returns ID for event row created as elog_ID field in one-row Recordset
			iID = CInt(oDbCmdLogEvent.ExecuteScalar())
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
			oDbCmdSetProp.Parameters("@name").Value = sName
			oDbCmdSetProp.Parameters("@value").Value = sValue

			If Not oDbConn.State = ConnectionState.Open Then
				OpenConnection()
			End If
			'stored procedure returns ID for property row as PropertyId field in one-row Recordset
			iID = CInt(oDbCmdSetProp.ExecuteScalar())
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
		Dim sValue As String

		ClearSuccess()
		Try
			oDbCmdGetProp.Parameters("@name").Value = sName

			If Not oDbConn.State = ConnectionState.Open Then
				OpenConnection()
			End If
			'stored procedure returns ID for property row as PropertyId field in one-row Recordset
			Dim dr As SqlClient.SqlDataReader = oDbCmdGetProp.ExecuteReader(CommandBehavior.SingleRow)
			If dr.Read() Then
				sValue = dr("value").ToString()
				LastPropertyTimestamp = dr("Timestamp").ToString()
			Else			 'property not found
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

		Return sValue		  'may be nothing if property undefined
	End Function

	'make sure the database connection is closed before going away
	Protected Overrides Sub Finalize()
		If Not oDbConn Is Nothing Then
			If Not oDbConn.State = ConnectionState.Closed Then
				Shutdown()
			End If
		End If
		MyBase.Finalize()
	End Sub
End Class
