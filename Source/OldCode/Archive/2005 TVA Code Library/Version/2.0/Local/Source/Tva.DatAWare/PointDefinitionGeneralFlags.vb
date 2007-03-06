Imports Tva.Interop.Bit

' 02/20/2007

Public Class PointDefinitionGeneralFlags

#Region " Member Declaration "

    Private m_value As Int32

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

    Public Sub New(ByVal value As Int32)

        MyBase.New()
        m_value = value

    End Sub

    Public Property PointType() As PointType
        Get
            Return CType(m_value And PointTypeMask, PointType)
        End Get
        Set(ByVal value As PointType)
            m_value = (m_value And Not PointTypeMask Or value)
        End Set
    End Property

    Public Property Archived() As Boolean
        Get
            Return Convert.ToBoolean(m_value And ArchivedMask)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                ' Set the 'Archived' bit.
                m_value = (m_value Or ArchivedMask)
            Else
                ' Clear the 'Archived' bit.
                m_value = (m_value And Not ArchivedMask)
            End If
        End Set
    End Property

    Public Property Used() As Boolean
        Get
            Return Convert.ToBoolean(m_value And UsedMask)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                ' Set the 'Used' bit.
                m_value = (m_value Or UsedMask)
            Else
                ' Clear the 'Used' bit.
                m_value = (m_value And Not UsedMask)
            End If
        End Set
    End Property

    Public Property AlarmEnabled() As Boolean
        Get
            Return Convert.ToBoolean(m_value And AlarmEnabledMask)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                ' Set the 'Alarm Enabled' bit.
                m_value = (m_value Or AlarmEnabledMask)
            Else
                ' Clear the 'Alarm Enabled' bit.
                m_value = (m_value And Not AlarmEnabledMask)
            End If
        End Set
    End Property

    Public Property NotifyByEmail() As Boolean
        Get
            Return Convert.ToBoolean(m_value And NotifyByEmailMask)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                ' Set the 'Notify By Email' bit.
                m_value = (m_value Or NotifyByEmailMask)
            Else
                ' Clear the 'Nofity By Email' bit.
                m_value = (m_value And Not NotifyByEmailMask)
            End If
        End Set
    End Property

    Public Property NotifyByPager() As Boolean
        Get
            Return Convert.ToBoolean(m_value And NotifyByPagerMask)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                ' Set the 'Notify By Pager' bit.
                m_value = (m_value Or NotifyByPagerMask)
            Else
                ' Clear the 'Notify By Pager' bit.
                m_value = (m_value And Not NotifyByPagerMask)
            End If
        End Set
    End Property

    Public Property NotifyByPhone() As Boolean
        Get
            Return Convert.ToBoolean(m_value And NotifyByPhoneMask)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                ' Set the 'Notify By Phone' bit.
                m_value = (m_value Or NotifyByPhoneMask)
            Else
                ' Clear the 'Notify By Phone' bit.
                m_value = (m_value And Not NotifyByPhoneMask)
            End If
        End Set
    End Property

    Public Property LogToFile() As Boolean
        Get
            Return Convert.ToBoolean(m_value And LogToFileMask)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                ' Set the 'Log To File' bit.
                m_value = (m_value Or LogToFileMask)
            Else
                ' Clear the 'Log To File' bit.
                m_value = (m_value And Not LogToFileMask)
            End If
        End Set
    End Property

    Public Property Changed() As Boolean
        Get
            Return Convert.ToBoolean(m_value And ChangedMask)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                ' Set the 'Changed' bit.
                m_value = (m_value Or ChangedMask)
            Else
                ' Clear the 'Changed' bit.
                m_value = (m_value And Not ChangedMask)
            End If
        End Set
    End Property

    Public Property StepCheck() As Boolean
        Get
            Return Convert.ToBoolean(m_value And StepCheckMask)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                ' Set the 'Step Check' bit.
                m_value = (m_value Or StepCheckMask)
            Else
                ' Clear the 'Step Check' bit.
                m_value = (m_value And Not StepCheckMask)
            End If
        End Set
    End Property

    Public Property Value() As Int32
        Get
            Return m_value
        End Get
        Set(ByVal value As Int32)
            m_value = value
        End Set
    End Property

#End Region

End Class
