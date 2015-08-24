' 05/23/2007

<Serializable()> _
Public Class ProcessProgress(Of TUnit)

#Region " Member Declaration "

    Private m_processName As String
    Private m_progressMessage As String
    Private m_total As TUnit
    Private m_complete As TUnit

#End Region

#Region " Code Scope: Public "

    Public Sub New(ByVal processName As String)

        MyBase.New()
        m_processName = processName

    End Sub

    Public Property ProcessName() As String
        Get
            Return m_processName
        End Get
        Set(ByVal value As String)
            m_processName = value
        End Set
    End Property

    Public Property ProgressMessage() As String
        Get
            Return m_progressMessage
        End Get
        Set(ByVal value As String)
            m_progressMessage = value
        End Set
    End Property

    Public Property Total() As TUnit
        Get
            Return m_total
        End Get
        Set(ByVal value As TUnit)
            m_total = value
        End Set
    End Property

    Public Property Complete() As TUnit
        Get
            Return m_complete
        End Get
        Set(ByVal value As TUnit)
            m_complete = value
        End Set
    End Property

#End Region

End Class
