' 12/28/2006

Imports System.Runtime.InteropServices

<CLSCompliant(False)> _
Public NotInheritable Class Console

    Private Shared m_handler As ConsoleWindowEventHandler

    Private Delegate Function ConsoleWindowEventHandler(ByVal controlType As ConsoleEventType) As Boolean

    Private Enum ConsoleEventType As UInteger
        CtrlCKeyPress = 0
        CtrlBreakKeyPress = 1
        ConsoleClosing = 2
        UserLoggingOff = 5
        SystemShutdown = 6
    End Enum

    Public Shared Event CtrlCKeyPress As EventHandler(Of System.ComponentModel.CancelEventArgs)

    Public Shared Event CtrlBreakKeyPress As EventHandler(Of System.ComponentModel.CancelEventArgs)

    Public Shared Event ConsoleClosing As EventHandler(Of System.ComponentModel.CancelEventArgs)

    Public Shared Event UserLoggingOff As EventHandler

    Public Shared Event SystemShutdown As EventHandler

    Public Shared Sub EnableRaisingEvents()

        ' Member variable is used here so that the delegate is not garbage collected by the time it is called by
        ' WIN API when any of the control events take place.
        ' http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=996045&SiteID=1
        m_handler = AddressOf HandleConsoleWindowEvents
        SetConsoleWindowEventRaising(m_handler, True)

    End Sub

    Public Shared Sub DisableRaisingEvents()

        m_handler = AddressOf HandleConsoleWindowEvents
        SetConsoleWindowEventRaising(m_handler, False)

    End Sub

    Private Sub New()

    End Sub

    <DllImport("kernel32.dll", EntryPoint:="SetConsoleCtrlHandler")> _
    Private Shared Function SetConsoleWindowEventRaising(ByVal handler As ConsoleWindowEventHandler, ByVal enable As Boolean) As Boolean

    End Function

    Private Shared Function HandleConsoleWindowEvents(ByVal controlType As ConsoleEventType) As Boolean

        ' ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.WIN32COM.v10.en/dllproc/base/handlerroutine.htm

        ' When this function doesn't return True, the default handler is called and the default action takes place.
        Select Case controlType
            Case ConsoleEventType.CtrlCKeyPress
                Dim ctrlCKeyPressEventData As New System.ComponentModel.CancelEventArgs()
                RaiseEvent CtrlCKeyPress(Nothing, ctrlCKeyPressEventData)
                If ctrlCKeyPressEventData.Cancel Then Return True
            Case ConsoleEventType.CtrlBreakKeyPress
                Dim ctrlBreakKeyPressEventData As New System.ComponentModel.CancelEventArgs()
                RaiseEvent CtrlBreakKeyPress(Nothing, ctrlBreakKeyPressEventData)
                If ctrlBreakKeyPressEventData.Cancel Then Return True
            Case ConsoleEventType.ConsoleClosing
                Dim consoleClosingEventData As New System.ComponentModel.CancelEventArgs()
                RaiseEvent ConsoleClosing(Nothing, consoleClosingEventData)
                If consoleClosingEventData.Cancel Then Return True
            Case ConsoleEventType.UserLoggingOff
                RaiseEvent UserLoggingOff(Nothing, EventArgs.Empty)
            Case ConsoleEventType.SystemShutdown
                RaiseEvent SystemShutdown(Nothing, EventArgs.Empty)
        End Select

        Return False

    End Function

End Class
