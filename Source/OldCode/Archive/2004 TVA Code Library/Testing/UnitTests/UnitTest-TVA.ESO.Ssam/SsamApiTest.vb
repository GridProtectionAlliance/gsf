'	SsamApiTest
'
'	this is a test class for the SsamApi class. See TVA.Testing.TestBase for overview.
'
Imports TVA.Testing
Imports TVA.ESO.Ssam

Public Class SsamApiTest

    Inherits TestBase

    'a minimal test of the API lifecycle:
    '	create API object - this should open the database connection
    '	report success on dummy process (SSAM_MONITOR) - this should return a non-zero ID
    '	shut down the API object - this should close the database connection
    '	destroy the API object - this should be automatic
    Public Sub testLifecycle()
        Dim iID As Integer = 0

        Dim oSsamApi As New SsamApi(SsamDatabase.ssamDevelopmentDB)    'make object and init for use
        assert(oSsamApi.Success, "Create/Open", oSsamApi.ErrorMessage)    'should succeed
        assert(oSsamApi.isDbOpen(), "isOpen after Create")    'database should be open now

        iID = oSsamApi.LogSsamEvent(SsamEventTypeID.ssamSuccessEvent, SsamEntityTypeID.ssamProcessEntity, "SSAM_MONITOR")
        assert(oSsamApi.Success, "Log Success", oSsamApi.ErrorMessage)    'should succeed
        assert(iID > 0, "Valid ID for Success event")    'log event method should return new ID > 0 if successful

        oSsamApi.Shutdown()    'shut object down
        assert(oSsamApi.Success, "Shut down", oSsamApi.ErrorMessage)    'should succeed
        assert(Not oSsamApi.isDbOpen(), "DB Closed after shutdown")    'database should be closed now
    End Sub

    'test all of the database connections using constructor method
    Public Sub testDbConnectionsInConstructor()
        Dim iDbNumber As SsamDatabase

        For iDbNumber = SsamDatabase.ssamProductionDB To SsamDatabase.ssamClusterDB
            Dim oSsamApi As New SsamApi(iDbNumber)
            assert(oSsamApi.Success, "Create/Open for DB #" & CStr(iDbNumber), oSsamApi.ErrorMessage)    'should succeed
            assert(oSsamApi.isDbOpen(), "DB #" & CStr(iDbNumber) & " isOpen")    'database should be open now
            oSsamApi.Shutdown()    'shut object down
            assert(oSsamApi.Success, "Shut down", oSsamApi.ErrorMessage)    'should succeed
            assert(Not oSsamApi.isDbOpen(), "DB #" & CStr(iDbNumber) & " Closed after shut down")    'database should be closed now
        Next
    End Sub

    'test all of the database connections using init method
    Public Sub testDbConnectionsViaInit()
        Dim iDbNumber As SsamDatabase
        Dim oSsamApi As New SsamApi
        oSsamApi.Connection = SsamDatabase.ssamDevelopmentDB
        assert(oSsamApi.Success, "Create/Open disconnected", oSsamApi.ErrorMessage)    'should succeed

        For iDbNumber = SsamDatabase.ssamProductionDB To SsamDatabase.ssamClusterDB
            oSsamApi.Connection = iDbNumber    'change databases
            assert(oSsamApi.Success, "Change to DB #" & CStr(iDbNumber), oSsamApi.ErrorMessage)    'should succeed
            assert(oSsamApi.isDbOpen(), "DB #" & CStr(iDbNumber) & " isOpen")    'database should be open now
        Next
        oSsamApi.Shutdown()    'shut object down
        assert(oSsamApi.Success, "Shut down", oSsamApi.ErrorMessage)    'should succeed
        assert(Not oSsamApi.isDbOpen(), "DB Closed after shut down")    'database should be closed now
    End Sub

    'test a failure mode - bad monitored object name
    Public Sub testBadName()
        Dim iID As Integer = 0

        Dim oSsamApi As New SsamApi(SsamDatabase.ssamDevelopmentDB)
        assert(oSsamApi.Success, "Create/Open", oSsamApi.ErrorMessage)    'should succeed
        assert(oSsamApi.isDbOpen(), "isOpen after Create")    'database should be open now

        iID = oSsamApi.LogSsamEvent(SsamEventTypeID.ssamSuccessEvent, SsamEntityTypeID.ssamProcessEntity, "SSAM_MOONITOR")
        assert(Not oSsamApi.Success, "Log Expected Failure: " & oSsamApi.ErrorMessage)    'should fail
        assert(iID = 0, "Zero ID for Failure")    'log event method should return new ID > 0 if successful

        oSsamApi.Shutdown()    'shut object down
        assert(oSsamApi.Success, "Shut down", oSsamApi.ErrorMessage)    'should succeed
        assert(Not oSsamApi.isDbOpen(), "DB Closed after shutdown")    'database should be closed now
    End Sub

    'test a failure mode - incorrect monitored object type
    Public Sub testBadType()
        Dim iID As Integer = 0

        Dim oSsamApi As New SsamApi(SsamDatabase.ssamDevelopmentDB)
        assert(oSsamApi.Success, "Create/Open", oSsamApi.ErrorMessage)    'should succeed
        assert(oSsamApi.isDbOpen(), "isOpen after Create")    'database should be open now

        iID = oSsamApi.LogSsamEvent(SsamEventTypeID.ssamSuccessEvent, SsamEntityTypeID.ssamFlowEntity, "SSAM_MONITOR")
        assert(Not oSsamApi.Success, "Log Expected Failure: " & oSsamApi.ErrorMessage)    'should fail
        assert(iID = 0, "Zero ID for Failure")    'log event method should return new ID > 0 if successful

        oSsamApi.Shutdown()    'shut object down
        assert(oSsamApi.Success, "Shut down", oSsamApi.ErrorMessage)    'should succeed
        assert(Not oSsamApi.isDbOpen(), "DB Closed after shutdown")    'database should be closed now
    End Sub

    'test all event types normally used by applications
    Public Sub testApplicationEventTypes()
        Dim iID As Integer = 0

        Dim oSsamApi As New SsamApi(SsamDatabase.ssamDevelopmentDB)    'make object and init for use
        assert(oSsamApi.Success, "Create/Open", oSsamApi.ErrorMessage)    'should succeed
        assert(oSsamApi.isDbOpen(), "isOpen after Create")    'database should be open now

        'test success
        iID = oSsamApi.LogSsamEvent(SsamEventTypeID.ssamSuccessEvent, SsamEntityTypeID.ssamProcessEntity, "SSAM_MONITOR")
        assert(oSsamApi.Success, "Success", oSsamApi.ErrorMessage)    'should succeed
        assert(iID > 0, "Valid ID for Success event")    'log event method should return new ID > 0 if successful

        'test info
        iID = oSsamApi.LogSsamEvent(SsamEventTypeID.ssamInfoEvent, SsamEntityTypeID.ssamProcessEntity, "SSAM_MONITOR", _
           Nothing, "Info test message", Nothing)
        assert(oSsamApi.Success, "Info", oSsamApi.ErrorMessage)    'should succeed
        assert(iID > 0, "Valid ID for Info event")    'log event method should return new ID > 0 if successful

        'test warning
        iID = oSsamApi.LogSsamEvent(SsamEventTypeID.ssamWarningEvent, SsamEntityTypeID.ssamProcessEntity, "SSAM_MONITOR", _
           "Warn-Test", "Test Warning Message", Nothing)
        assert(oSsamApi.Success, "Warning", oSsamApi.ErrorMessage)    'should succeed
        assert(iID > 0, "Valid ID for Warning event")    'log event method should return new ID > 0 if successful

        'test error
        iID = oSsamApi.LogSsamEvent(SsamEventTypeID.ssamErrorEvent, SsamEntityTypeID.ssamProcessEntity, "SSAM_MONITOR", _
           "ERR-123-TEST", "Test Error Message", Nothing)
        assert(oSsamApi.Success, "Info", oSsamApi.ErrorMessage)    'should succeed
        assert(iID > 0, "Valid ID for Info event")    'log event method should return new ID > 0 if successful

        'remaining event types should not be sent by applications under normal circumstances

        oSsamApi.Shutdown()    'shut object down
        assert(oSsamApi.Success, "Shut down", oSsamApi.ErrorMessage)    'should succeed
        assert(Not oSsamApi.isDbOpen(), "DB Closed after shutdown")    'database should be closed now
    End Sub

    'test KeepConnectionOpen=False property effects
    Public Sub testKeepOpen()
        Dim iID As Integer = 0

        Dim oSsamApi As New SsamApi(SsamDatabase.ssamDevelopmentDB, True)
        assert(oSsamApi.Success, "Create/Open", oSsamApi.ErrorMessage)    'should succeed
        assert(oSsamApi.isDbOpen(), "isOpen after Create")    'database should be open now

        oSsamApi.KeepConnectionOpen = False
        assert(Not oSsamApi.isDbOpen(), "not isOpen after reset")    'database should be closed now

        iID = oSsamApi.LogSsamEvent(SsamEventTypeID.ssamSuccessEvent, SsamEntityTypeID.ssamProcessEntity, "SSAM_MONITOR")
        assert(oSsamApi.Success, "Log Success", oSsamApi.ErrorMessage)    'should succeed
        assert(iID > 0, "Non-zero ID for Success")    'log event method should return new ID > 0 if successful
        assert(Not oSsamApi.isDbOpen(), "not isOpen after log")    'connection should be closed now

        oSsamApi.Shutdown()    'shut object down
        assert(oSsamApi.Success, "Shut down", oSsamApi.ErrorMessage)    'should succeed
        assert(Not oSsamApi.isDbOpen(), "DB Closed after shutdown")    'database should still be closed
    End Sub

    'a minimal test of the API lifecycle:
    '	create API object - this should open the database connection
    '	report success on dummy process (SSAM_MONITOR) - this should return a non-zero ID
    '	shut down the API object - this should close the database connection
    '	destroy the API object - this should be automatic
    Public Sub testAllDbLifecycle()
        Dim iID As Integer = 0
        Dim iDbNumber As SsamDatabase

        For iDbNumber = SsamDatabase.ssamProductionDB To SsamDatabase.ssamClusterDB
            Dim oSsamApi As New SsamApi(iDbNumber)   'make object and init for use
            assert(oSsamApi.Success, "Create/Open DB #" & CStr(iDbNumber), oSsamApi.ErrorMessage)    'should succeed
            assert(oSsamApi.isDbOpen(), "isOpen after Create")    'database should be open now

            iID = oSsamApi.LogSsamEvent(SsamEventTypeID.ssamSuccessEvent, SsamEntityTypeID.ssamProcessEntity, "SSAM_MONITOR")
            assert(oSsamApi.Success, "Log Success DB #" & CStr(iDbNumber), oSsamApi.ErrorMessage)    'should succeed
            assert(iID > 0, "Valid ID for Success event")    'log event method should return new ID > 0 if successful

            oSsamApi.Shutdown()    'shut object down
            assert(oSsamApi.Success, "Shut down", oSsamApi.ErrorMessage)    'should succeed
            assert(Not oSsamApi.isDbOpen(), "DB #" & CStr(iDbNumber) & " Closed after shutdown")    'database should be closed now
        Next
    End Sub

End Class
