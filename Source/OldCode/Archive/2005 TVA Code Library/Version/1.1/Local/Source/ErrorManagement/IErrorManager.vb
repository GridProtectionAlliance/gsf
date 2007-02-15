Imports System.Data

Namespace ErrorManagement

    Public Interface IErrorManager

        Property ErrorType() As ErrorType
        Property Timestamp() As Date
        Property PointTag() As String
        Property ErrorException() As Exception
        Property ErrorNumber() As String
        Property ErrorMessage() As String
        Property Sql() As String
        Property DbCommand() As IDbCommand
        Property Unhandled() As Boolean
        Sub LogMessage(ByVal msg As String, Optional ByVal pointTag As String = Nothing)
        Sub LogWarning(ByVal msg As String, Optional ByVal pointTag As String = Nothing)
        Sub LogError(ByVal num As String, ByVal msg As String, Optional ByVal pointTag As String = Nothing)
        Sub LogError(ByVal ex As Exception, Optional ByVal msg As String = Nothing)
        Sub LogUnexpectedError(ByVal num As String, ByVal msg As String, Optional ByVal pointTag As String = Nothing)
        Sub LogUnexpectedError(ByVal ex As Exception, Optional ByVal msg As String = Nothing)
        Sub LogSqlError(ByVal msg As String, Optional ByVal sql As String = Nothing, Optional ByVal pointTag As String = Nothing)
        Sub LogSqlError(ByVal ex As Exception, ByVal oDbCommand As IDbCommand, Optional ByVal msg As String = Nothing)
        Sub LogSqlError(ByVal msg As String, ByVal oDbCommand As IDbCommand, Optional ByVal pointTag As String = Nothing)
        Sub LogUnexpectedSqlError(ByVal msg As String, Optional ByVal sql As String = Nothing, Optional ByVal pointTag As String = Nothing)
        Sub LogUnexpectedSqlError(ByVal ex As Exception, ByVal oDbCommand As IDbCommand, Optional ByVal msg As String = Nothing)
        Sub LogUnexpectedSqlError(ByVal msg As String, ByVal oDbCommand As IDbCommand, Optional ByVal pointTag As String = Nothing)

    End Interface

End Namespace