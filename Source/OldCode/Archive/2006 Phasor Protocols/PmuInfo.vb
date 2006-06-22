'*******************************************************************************************************
'  PmuInfo.vb - PMU Information Definition
'  Copyright © 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  06/22/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Public Class PmuInfo

    Implements IComparable(Of PmuInfo)

    Private m_id As UShort
    Private m_tag As String
    Private m_lastReportTime As Long

    Public Sub New(ByVal id As UShort, ByVal tag As String)

        m_id = id
        m_tag = tag

    End Sub

    Public ReadOnly Property ID() As UShort
        Get
            Return m_id
        End Get
    End Property

    Public ReadOnly Property Tag() As String
        Get
            Return m_tag
        End Get
    End Property

    Public Property LastReportTime() As Long
        Get
            Return m_lastReportTime
        End Get
        Set(ByVal value As Long)
            m_lastReportTime = value
        End Set
    End Property

    Public Function CompareTo(ByVal other As PmuInfo) As Integer Implements System.IComparable(Of PmuInfo).CompareTo

        Return m_id.CompareTo(other.ID)

    End Function

End Class
