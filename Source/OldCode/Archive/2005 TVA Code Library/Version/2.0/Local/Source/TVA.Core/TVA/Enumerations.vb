' PCP: 04-09-2007

''' <summary>
''' Specifies the type of the application.
''' </summary>
Public Enum ApplicationType As Short
    ''' <summary>
    ''' Application is of unknown type.
    ''' </summary>
    Unknown = 0
    ''' <summary>
    ''' Application doesn't require a subsystem.
    ''' </summary>
    Native = 1
    ''' <summary>
    ''' Application runs in the Windows GUI subsystem.
    ''' </summary>
    WindowsGui = 2
    ''' <summary>
    ''' Application runs in the Windows character subsystem.
    ''' </summary>
    WindowsCui = 3
    ''' <summary>
    ''' Application runs in the OS/2 character subsystem.
    ''' </summary>
    OS2Cui = 5
    ''' <summary>
    ''' Application runs in the Posix character subsystem.
    ''' </summary>
    PosixCui = 7
    ''' <summary>
    ''' Application is a native Win9x driver.
    ''' </summary>
    NativeWindows = 8
    ''' <summary>
    ''' Application runs in the Windows CE subsystem.
    ''' </summary>
    WindowsCEGui = 9
    ''' <summary>
    ''' The application is a web site or web application.
    ''' </summary>
    Web = 15
End Enum