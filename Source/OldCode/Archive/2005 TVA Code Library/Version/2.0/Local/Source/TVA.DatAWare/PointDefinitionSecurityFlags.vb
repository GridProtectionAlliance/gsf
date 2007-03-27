' 03/06/2007

Imports TVA.Interop.Bit

Public Class PointDefinitionSecurityFlags

#Region " Member Declaration "

    Private m_value As Int16

    Private Const RecordSecurityMask As Int16 = Bit0 Or Bit1 Or Bit2
    Private Const AccessSecurityMask As Int16 = Bit3 Or Bit4 Or Bit5

#End Region

#Region " Public Code "

    Public Sub New(ByVal value As Int16)

        MyBase.New()
        m_value = value

    End Sub

    Public Property RecordSecurity() As Short
        Get
            Return m_value And RecordSecurityMask
        End Get
        Set(ByVal value As Short)
            m_value = (m_value And Not RecordSecurityMask Or value)
        End Set
    End Property

    Public Property AccessSecurity() As Short
        Get
            Return Convert.ToInt16((m_value And AccessSecurityMask) \ 8) ' <- 1st 3 bit are record security, so 2 ^ 3 = 8.
        End Get
        Set(ByVal value As Short)
            m_value = (m_value And Not AccessSecurityMask Or Convert.ToInt16(value * 8))
        End Set
    End Property

    Public Property Value() As Int16
        Get
            Return m_value
        End Get
        Set(ByVal value As Int16)
            m_value = value
        End Set
    End Property

#End Region

End Class
