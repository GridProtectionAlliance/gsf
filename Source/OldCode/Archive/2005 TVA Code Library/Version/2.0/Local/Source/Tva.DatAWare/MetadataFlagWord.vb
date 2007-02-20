Imports Tva.Interop.Bit

' 02/20/2007

Public Class MetadataFlagWord

#Region " Member Declaration "

    Private m_flagWord As Int32

    Private Const PointTypeMask As Integer = Bit0 Or Bit1 Or Bit2
    Private Const ArchivedMask As Integer = &H8
    Private Const UsedMask As Integer = &H10
    Private Const AlarmEnabledMask As Integer = &H20
    Private Const NotifyByEmailMask As Integer = &H40
    Private Const NotifyByPagerMask As Integer = &H80
    Private Const NotifyByPhoneMask As Integer = &H100
    Private Const LogToFileMask As Integer = &H200
    Private Const SpareMask As Integer = &H400
    Private Const ChangedMask As Integer = &H800
    Private Const StepCheckMask As Integer = &H1000

#End Region

#Region " Public Code "

    Public Sub New(ByVal flagWord As Int32)

        MyBase.New()
        m_flagWord = flagWord

    End Sub

    Public Property PointType() As PointType
        Get
            Return CType(m_flagWord And PointTypeMask, PointType)
        End Get
        Set(ByVal value As PointType)
            'm_flagWord = (m_flagWord And Not PointTypeMask Or value)
            m_flagWord = (m_flagWord Or value)
        End Set
    End Property

    Public Property Archived() As Boolean
        Get
            Return Convert.ToBoolean(m_flagWord And ArchivedMask)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                m_flagWord = (m_flagWord Or ArchivedMask)
            Else
                m_flagWord = (m_flagWord And Not ArchivedMask)
            End If
        End Set
    End Property

    Public Property Used() As Boolean
        Get
            Return Convert.ToBoolean(m_flagWord And UsedMask)
        End Get
        Set(ByVal value As Boolean)

        End Set
    End Property

    Public Property AlarmEnabled() As Boolean
        Get
            Return Convert.ToBoolean(m_flagWord And AlarmEnabledMask)
        End Get
        Set(ByVal value As Boolean)

        End Set
    End Property

    Public Property NotifyByEmail() As Boolean
        Get
            Convert.ToBoolean(m_flagWord And NotifyByEmailMask)
        End Get
        Set(ByVal value As Boolean)

        End Set
    End Property

    Public Property NotifyByPager() As Boolean
        Get
            Return Convert.ToBoolean(m_flagWord And NotifyByPagerMask)
        End Get
        Set(ByVal value As Boolean)

        End Set
    End Property

    Public Property NotifyByPhone() As Boolean
        Get
            Return Convert.ToBoolean(m_flagWord And NotifyByPhoneMask)
        End Get
        Set(ByVal value As Boolean)

        End Set
    End Property

    Public Property LogToFile() As Boolean
        Get
            Return Convert.ToBoolean(m_flagWord And LogToFileMask)
        End Get
        Set(ByVal value As Boolean)

        End Set
    End Property

    Public Property Changed() As Boolean
        Get
            Return Convert.ToBoolean(m_flagWord And ChangedMask)
        End Get
        Set(ByVal value As Boolean)

        End Set
    End Property

    Public Property StepCheck() As Boolean
        Get
            Convert.ToBoolean(m_flagWord And StepCheckMask)
        End Get
        Set(ByVal value As Boolean)

        End Set
    End Property

    Public ReadOnly Property Value() As Integer
        Get
            Return m_flagWord
        End Get
    End Property

#End Region

End Class
