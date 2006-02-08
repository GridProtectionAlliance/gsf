'***********************************************************************
'  Measurement.vb - Basic measurement
'  Copyright © 2004 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Public Class Measurement

    Implements IMeasurement

    Private m_index As Integer
    Private m_value As Double
    Private m_timestamp As Date

    Public Sub New()

        m_index = -1
        m_value = Double.NaN
        m_timestamp = Date.MinValue

    End Sub

    Public Sub New(ByVal index As Integer, ByVal value As Double, ByVal timestamp As Date)

        m_index = index
        m_value = value
        m_timestamp = timestamp

    End Sub

    Public ReadOnly Property This() As IMeasurement Implements IMeasurement.This
        Get
            Return Me
        End Get
    End Property

    Public Overridable Property Index() As Integer Implements IMeasurement.Index
        Get
            Return m_index
        End Get
        Set(ByVal value As Integer)
            m_index = value
        End Set
    End Property

    Public Overridable Property Value() As Double Implements IMeasurement.Value
        Get
            Return m_value
        End Get
        Set(ByVal value As Double)
            m_value = value
        End Set
    End Property

    Public Overridable Property Timestamp() As Date Implements IMeasurement.Timestamp
        Get
            Return m_timestamp
        End Get
        Set(ByVal value As Date)
            m_timestamp = value
        End Set
    End Property

End Class