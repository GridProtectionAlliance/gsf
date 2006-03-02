'*******************************************************************************************************
'  PhasorDefinitionBase.vb - Phasor value definition base class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

' This class represents the common implementation of the protocol independent definition of a phasor value.
Public MustInherit Class PhasorDefinitionBase

    Inherits ChannelDefinitionBase
    Implements IPhasorDefinition

    Private m_format As CoordinateFormat
    Private m_type As PhasorType
    Private m_voltageReference As IPhasorDefinition

    Protected Sub New(ByVal parent As IConfigurationCell)

        MyBase.New(parent)

        m_format = CoordinateFormat.Rectangular
        m_type = PhasorType.Voltage
        m_voltageReference = Me

    End Sub

    Protected Sub New(ByVal parent As IConfigurationCell, ByVal dataFormat As DataFormat, ByVal index As Integer, ByVal label As String, ByVal scale As Integer, ByVal offset As Double, ByVal format As CoordinateFormat, ByVal type As PhasorType, ByVal voltageReference As IPhasorDefinition)

        MyBase.New(parent, dataFormat, index, label, scale, offset)

        m_format = format
        m_type = type

        If type = PhasorType.Voltage Then
            m_voltageReference = Me
        Else
            m_voltageReference = voltageReference
        End If

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal phasorDefinition As IPhasorDefinition)
    Protected Sub New(ByVal phasorDefinition As IPhasorDefinition)

        MyClass.New(phasorDefinition.Parent, phasorDefinition.DataFormat, phasorDefinition.Index, phasorDefinition.Label, _
            phasorDefinition.ScalingFactor, phasorDefinition.Offset, phasorDefinition.CoordinateFormat, phasorDefinition.Type, _
            phasorDefinition.VoltageReference)

    End Sub

    Public Overridable Property CoordinateFormat() As CoordinateFormat Implements IPhasorDefinition.CoordinateFormat
        Get
            Return m_format
        End Get
        Set(ByVal value As CoordinateFormat)
            m_format = Value
        End Set
    End Property

    Public Overridable Property [Type]() As PhasorType Implements IPhasorDefinition.Type
        Get
            Return m_type
        End Get
        Set(ByVal value As PhasorType)
            m_type = Value
        End Set
    End Property

    Public Overridable Property VoltageReference() As IPhasorDefinition Implements IPhasorDefinition.VoltageReference
        Get
            Return m_voltageReference
        End Get
        Set(ByVal value As IPhasorDefinition)
            If m_type = PhasorType.Voltage Then
                If Not Value Is Me Then
                    Throw New NotImplementedException("Voltage phasors do not have a voltage reference")
                End If
            Else
                m_voltageReference = Value
            End If
        End Set
    End Property

End Class

