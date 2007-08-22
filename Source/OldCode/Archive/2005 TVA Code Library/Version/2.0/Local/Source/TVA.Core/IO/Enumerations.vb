Namespace IO

    ''' <summary>
    ''' Specifies the operation to be performed on the log file when it is full.
    ''' </summary>
    Public Enum LogFileFullOperation As Integer
        ''' <summary>
        ''' Truncates the existing entries in the log file to make space for new entries.
        ''' </summary>
        Truncate
        ''' <summary>
        ''' Rolls over to a new log file, and keeps the full log file for reference.
        ''' </summary>
        Rollover
    End Enum

End Namespace