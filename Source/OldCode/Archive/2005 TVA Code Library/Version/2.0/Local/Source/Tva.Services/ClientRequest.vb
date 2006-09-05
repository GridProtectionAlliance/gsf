' 08-29-06

Imports Tva.Text.Common

<Serializable()> _
Public Class ClientRequest

    Private m_type As String
    Private m_parameters As String()

    Public Sub New()
        MyClass.New("UNDETERMINED")
    End Sub

    Public Sub New(ByVal type As String)
        MyClass.New(type, New String() {})
    End Sub

    Public Sub New(ByVal type As String, ByVal parameters As String())
        m_type = type
        m_parameters = parameters
    End Sub

    ' Standard Request Type
    '    Undetermined
    '    PauseService
    '    ResumeService
    '    StopService
    '    RestartService
    '    ListProcesses
    '    StartProcess
    '    AbortProcess
    '    UnscheduleProcess
    '    RescheduleProcess
    '    PingService
    '    PingAllClients
    '    ListClients
    '    GetServiceStatus
    '    GetProcessStatus
    '    GetCommandHistory
    '    GetDirectoryListing
    '    ListSettings
    '    UpdateSetting
    '    SaveSettings
    Public Property Type() As String
        Get
            Return m_type
        End Get
        Set(ByVal value As String)
            m_type = value
        End Set
    End Property

    Public Property Parameters() As String()
        Get
            Return m_parameters
        End Get
        Set(ByVal value As String())
            m_parameters = value
        End Set
    End Property

End Class