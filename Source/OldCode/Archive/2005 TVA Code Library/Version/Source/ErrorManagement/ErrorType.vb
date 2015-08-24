'general classifications of informational/error messages
Namespace ErrorManagement
    Public Enum ErrorType
        tMessage = 0 'an informational message
        tWarning = 1 'a warning message
        tError = 2 'an ordinary error message, i.e. when logging an expected/handled error
        tUnexpectedError = 3 'an extraordinary error message, i.e. when top-level trap catches something unexpected
        tSqlError = 4 'an ordinary sql error message, i.e. when logging en expected/handled non-fatal error
        tUnexpectedSqlError = 5 'an extraordinary sql error message, i.e. when transaction trap catches something unexpected
    End Enum
End Namespace
