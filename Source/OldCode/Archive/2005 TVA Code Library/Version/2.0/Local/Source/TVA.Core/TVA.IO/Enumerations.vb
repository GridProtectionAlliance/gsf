Namespace IO

    ''' <summary>
    ''' Specifies the operation to be performed on the log file when it is full.
    ''' </summary>
    Public Enum LogFileFullOperation As Integer
        ''' <summary>
        ''' Truncate the existing entries in the log file to make space for new entries.
        ''' </summary>
        Truncate
        ''' <summary>
        ''' Rollover to a new log file and keep the full log file for reference.
        ''' </summary>
        Rollover
    End Enum

End Namespace