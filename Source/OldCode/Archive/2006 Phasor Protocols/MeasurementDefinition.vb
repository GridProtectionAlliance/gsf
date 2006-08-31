'*******************************************************************************************************
'  MeasurementDefinition.vb - Measurement definition
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
'  05/31/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Public Class MeasurementDefinition1

    Private m_id As Integer
    Private m_name As String
    Private m_adder As Double
    Private m_multiplier As Double

    Public Sub New(ByVal id As Integer, ByVal name As String, ByVal adder As Double, ByVal multiplier As Double)

        m_id = id
        m_name = name
        m_adder = adder
        m_multiplier = multiplier

    End Sub

    Public Property ID() As Integer
        Get
            Return m_id
        End Get
        Set(ByVal value As Integer)
            m_id = value
        End Set
    End Property

    Public Property Name() As String
        Get
            Return m_name
        End Get
        Set(ByVal value As String)
            m_name = value
        End Set
    End Property

    Public Property Adder() As Double
        Get
            Return m_adder
        End Get
        Set(ByVal value As Double)
            m_adder = value
        End Set
    End Property

    Public Property Multiplier() As Double
        Get
            Return m_multiplier
        End Get
        Set(ByVal value As Double)
            m_multiplier = value
        End Set
    End Property

End Class
