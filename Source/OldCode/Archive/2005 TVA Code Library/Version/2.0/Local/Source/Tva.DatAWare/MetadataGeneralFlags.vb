Imports Tva.Interop.Bit

' 02/20/2007

Public Class MetadataGeneralFlags

#Region " Member Declaration "

    Private m_flagWord As Int32

    Private Const PointTypeMask As Int32 = Bit0 Or Bit1 Or Bit2
    Private Const ArchivedMask As Int32 = Bit3
    Private Const UsedMask As Int32 = Bit4
    Private Const AlarmEnabledMask As Int32 = Bit5
    Private Const NotifyByEmailMask As Int32 = Bit6
    Private Const NotifyByPagerMask As Int32 = Bit7
    Private Const NotifyByPhoneMask As Int32 = Bit8
    Private Const LogToFileMask As Int32 = Bit9
    Private Const SpareMask As Int32 = Bit10
    Private Const ChangedMask As Int32 = Bit11
    Private Const StepCheckMask As Int32 = Bit12

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
            m_flagWord = (m_flagWord Or value)
        End Set
    End Property

    Public Property Archived() As Boolean
        Get
            Return Convert.ToBoolean(m_flagWord And ArchivedMask)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                ' Set the 'Archived' bit.
                m_flagWord = (m_flagWord Or ArchivedMask)
            Else
                ' Clear the 'Archived' bit.
                m_flagWord = (m_flagWord And Not ArchivedMask)
            End If
        End Set
    End Property

    Public Property Used() As Boolean
        Get
            Return Convert.ToBoolean(m_flagWord And UsedMask)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                ' Set the 'Used' bit.
                m_flagWord = (m_flagWord Or UsedMask)
            Else
                ' Clear the 'Used' bit.
                m_flagWord = (m_flagWord And Not UsedMask)
            End If
        End Set
    End Property

    Public Property AlarmEnabled() As Boolean
        Get
            Return Convert.ToBoolean(m_flagWord And AlarmEnabledMask)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                ' Set the 'Alarm Enabled' bit.
                m_flagWord = (m_flagWord Or AlarmEnabledMask)
            Else
                ' Clear the 'Alarm Enabled' bit.
                m_flagWord = (m_flagWord And Not AlarmEnabledMask)
            End If
        End Set
    End Property

    Public Property NotifyByEmail() As Boolean
        Get
            Return Convert.ToBoolean(m_flagWord And NotifyByEmailMask)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                ' Set the 'Notify By Email' bit.
                m_flagWord = (m_flagWord Or NotifyByEmailMask)
            Else
                ' Clear the 'Nofity By Email' bit.
                m_flagWord = (m_flagWord And Not NotifyByEmailMask)
            End If
        End Set
    End Property

    Public Property NotifyByPager() As Boolean
        Get
            Return Convert.ToBoolean(m_flagWord And NotifyByPagerMask)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                ' Set the 'Notify By Pager' bit.
                m_flagWord = (m_flagWord Or NotifyByPagerMask)
            Else
                ' Clear the 'Notify By Pager' bit.
                m_flagWord = (m_flagWord And Not NotifyByPagerMask)
            End If
        End Set
    End Property

    Public Property NotifyByPhone() As Boolean
        Get
            Return Convert.ToBoolean(m_flagWord And NotifyByPhoneMask)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                ' Set the 'Notify By Phone' bit.
                m_flagWord = (m_flagWord Or NotifyByPhoneMask)
            Else
                ' Clear the 'Notify By Phone' bit.
                m_flagWord = (m_flagWord And Not NotifyByPhoneMask)
            End If
        End Set
    End Property

    Public Property LogToFile() As Boolean
        Get
            Return Convert.ToBoolean(m_flagWord And LogToFileMask)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                ' Set the 'Log To File' bit.
                m_flagWord = (m_flagWord Or LogToFileMask)
            Else
                ' Clear the 'Log To File' bit.
                m_flagWord = (m_flagWord And Not LogToFileMask)
            End If
        End Set
    End Property

    Public Property Changed() As Boolean
        Get
            Return Convert.ToBoolean(m_flagWord And ChangedMask)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                ' Set the 'Changed' bit.
                m_flagWord = (m_flagWord Or ChangedMask)
            Else
                ' Clear the 'Changed' bit.
                m_flagWord = (m_flagWord And Not ChangedMask)
            End If
        End Set
    End Property

    Public Property StepCheck() As Boolean
        Get
            Return Convert.ToBoolean(m_flagWord And StepCheckMask)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                ' Set the 'Step Check' bit.
                m_flagWord = (m_flagWord Or StepCheckMask)
            Else
                ' Clear the 'Step Check' bit.
                m_flagWord = (m_flagWord And Not StepCheckMask)
            End If
        End Set
    End Property

    Public ReadOnly Property Value() As Int32
        Get
            Return m_flagWord
        End Get
    End Property

#End Region

End Class
