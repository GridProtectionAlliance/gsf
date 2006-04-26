Attribute VB_Name = "SsamAPI"
Option Explicit

Const DEBUG_MODE = 0    'set to 1 to point at SSAM_dev on SOCOPDEV (Development) instead of SSAM on SOCOPFILE (production)

'
' Note: might be a good optimization to have ssam-init and ssam-stop subroutines to open and close
'       the database connection, instead of opening and closing the database connection for each event posted
'       [fixme]? Also cache the recordset object.
'

'
'constants for SSAM database connection choices
'
Public Enum SsamDatabase
    ssamProductionDB = 0        'use production database connection
    ssamDevelopmentDB = 1       'use development database connection
    ssamClusterDB = 2           'use cluster database connection
End Enum

'
'constants for SSAM entity type IDs
'Note: these IDs must match the IDs in the database, which depends on the order created in SsamSupport.bas
'
Public Enum SsamEntityTypeID
    ssamFlowEntity = 1          'This entity type represents a data-flow
    ssamEquipmentEntity = 2     'This entity type represents a piece of equipment
    ssamProcessEntity = 3       'This entity type represents a Process
    ssamSystemEntity = 4        'This entity type represents a System
    ssamDataEntity = 5          'This entity type represents a data item like a file or table
End Enum

'
'constants for SSAM event class IDs
'Note: these IDs must match the IDs in the database, which depends on the order created in SsamSupport.bas
'
Public Enum SsamEventTypeID
    ssamSuccessEvent = 1        'This event reports a successful action on some entity
    ssamWarningEvent = 2        'This event is a warning that something MAY be going wrong soon
    ssamAlarmEvent = 3          'This event is an alarm that something HAS ALREADY gone wrong
    ssamErrorEvent = 4          'This event reports an unexpected ERROR in an application that may or may not matter
    ssamInfoEvent = 5           'This event reports information that may be of interest to someone
    ssamEscalationEvent = 6     'This event reports an Alarm notification that was sent that has not been acknowledged
    ssamFailoverEvent = 7       'This event reports a cluster failover on some process (informational)
    ssamQuitEvent = 8           'This event halts the SSAM monitoring/dispatching process - remove later? [fixme]?
    ssamSyncEvent = 9           'This action handles a synchronize-SSAM notification by synchronizing the monitor database with the system-configuration database
    ssamSchedEvent = 10         'This action handles a terminate-SSAM notification by rescheduling all events
    ssamCatchUpEvent = 11       'This action makes the monitor skip old events, reschedule, and return to real-time processing
End Enum

'note: using integrated security for the moment, but
'should have admin create new account for this [fixme]
Const sDB_CONN_PRODUCTION = "Provider=SQLOLEDB.1;Integrated Security=SSPI;" & _
        "Persist Security Info=False;Initial Catalog=SSAM;Data Source=SOCOPFILE"

Const sDB_CONN_DEVELOPMENT = "Provider=SQLOLEDB.1;Integrated Security=SSPI;" & _
        "Persist Security Info=False;Initial Catalog=SSAM;Data Source=SOCOPDEV"

Const sDB_CONN_CLUSTER = "Provider=SQLOLEDB.1;Integrated Security=SSPI;" & _
       "Persist Security Info=False;Initial Catalog=SSAM;Data Source=OPDATSQL"

Const sDB_CONN_ORACLE_PRODUCTION = "Provider=MSDAORA.1;User ID=SSAM_ADMIN;" & _
        "Data Source=THRESHER_CHAGP4;Password=moneso;Persist Security Info=False"

Const sDB_CONN_ORACLE_TEST = "Provider=MSDAORA.1;User ID=SSAM_ADMIN;" & _
        "Data Source=nimitz_BPCT;Password=moneso;Persist Security Info=False"

Public oDbConn As New ADODB.Connection      'persistent connection for writing SSAM events
Public iConnection As Integer               'choice of database to connect to

'call this to init the SSAM API to use something other than the default production database connection
Public Sub init(ByVal iConnect As SsamDatabase)
    iConnection = iConnect
    'open the connection the first time, and leave it open
    If oDbConn.State <> 1 Then
        oDbConn.ConnectionString = GetSSAMConnectString(iConnection)
        oDbConn.Open
    End If
End Sub

Public Sub Shutdown()
    If oDbConn.State = 1 Then
        oDbConn.Close
    End If
End Sub

'return SSAM database connect string - note that by default the PRODUCTION database connect string is returned
'unless we're in debugging mode
'for use by other modules in Testbed utility and external clients
Public Function GetSSAMConnectString(Optional ByVal iConnect As SsamDatabase = ssamProductionDB) As String
    If DEBUG_MODE = 1 Then      'if we're in debug/test mode, use development connect string
        GetSSAMConnectString = sDB_CONN_DEVELOPMENT
    Else    'otherwise, check flag
        Select Case iConnect
            Case ssamProductionDB      'default is production
                GetSSAMConnectString = sDB_CONN_PRODUCTION
            Case ssamDevelopmentDB      '1 for development
                GetSSAMConnectString = sDB_CONN_DEVELOPMENT
            Case ssamClusterDB      '2 for cluster testing
                GetSSAMConnectString = sDB_CONN_CLUSTER
            Case Else   'otherwise production
                GetSSAMConnectString = sDB_CONN_DEVELOPMENT
        End Select
    End If
End Function

'return SSAM database connect string for Oracle
'for use by other modules in Testbed utility and external clients
Public Function GetSSAMConnectStringOracle(Optional ByVal iConnect As SsamDatabase = ssamProductionDB) As String
    If DEBUG_MODE = 1 Then      'if we're in debug/test mode, use development connect string
        GetSSAMConnectStringOracle = sDB_CONN_ORACLE_TEST
    Else    'otherwise, check flag
        Select Case iConnect
            Case ssamProductionDB      'default is production on cluster
                GetSSAMConnectStringOracle = sDB_CONN_ORACLE_PRODUCTION
            Case Else
                GetSSAMConnectStringOracle = sDB_CONN_ORACLE_TEST
        End Select
    End If
End Function

'
'this is the base function to log an event to the System Status and Alarm Monitoring (SSAM) system;
'it returns the elog_ID for the new EventLog row
'NOTE: The entity ID number/mnemonic is validated by the stored procedure underlying this function.
'      If an invalid identifier is passed, this function will throw a custom error (#50000 usually)
'
'Note also: this function will need to be cluster-aware later [fixme]
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
        Optional ByRef sError As String = "", _
        Optional ByRef sMsg As String = "", _
        Optional ByRef sDesc As String = "") As Long

    Dim sSQL As String
    Dim oRecSet As ADODB.Recordset
    
    On Error Resume Next
    'stored procedure returns ID for event row created as elog_ID field in one-row Recordset
    sSQL = "execute sp_LogSsamEvent " & _
            iEventType & _
            "," & iEntityType & _
            ",'" & sEntityID & "'" & _
            "," & IIf(sError = "", "NULL", "'" & sqlText(sError) & "'") & _
            "," & IIf(sMsg = "", "NULL", "'" & sqlText(sMsg) & "'") & _
            "," & IIf(sDesc = "", "NULL", "'" & sqlText(sDesc) & "'")

    'we open the connection the first time, and leave it open
    If oDbConn.State <> 1 Then
        oDbConn.ConnectionString = GetSSAMConnectString(iConnection)
        oDbConn.Open
    ElseIf oDbConn.Properties(14).Value = 2 Then    'connection broken, close and reopen
        oDbConn.Close
        oDbConn.Open
    End If
    
    Set oRecSet = oDbConn.Execute(sSQL)
    LogSsamEvent = Trim(oRecSet!elog_ID)    'return ID for new EventLog row
    oRecSet.Close
    Set oRecSet = Nothing
End Function

'replace single-quotes with two single-quotes, respect nulls
Public Function sqlText(ByRef str As Variant) As Variant
    If IsNull(str) Then
        sqlText = ""
    Else
        sqlText = Replace(Trim(str), "'", "''")
    End If
End Function

