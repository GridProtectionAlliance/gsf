'***********************************************************************
'  PhasorDefinitionBase.vb - Phasor value definition base class
'  Copyright © 2005 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [WESTAFF]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  ---------------------------------------------------------------------
'  02/18/2005 - James R Carroll
'       Initial version of source generated
'
'***********************************************************************

Imports System.Text

Namespace EE.Phasor

    ' This class represents the common implementation of the protocol independent definition of a phasor value.
    Public MustInherit Class PhasorDefinitionBase

        Inherits ChannelDefinitionBase
        Implements IPhasorDefinition

        Protected m_type As PhasorType
        Protected m_voltageReference As IPhasorDefinition

        Protected Sub New()

            MyBase.New()

            m_type = PhasorType.Voltage
            m_voltageReference = Me

        End Sub

        Protected Sub New(ByVal index As Integer, ByVal label As String, ByVal scale As Double, ByVal type As PhasorType, ByVal voltageReference As IPhasorDefinition)

            MyBase.New(index, label, scale)

            m_type = type
            m_voltageReference = voltageReference

        End Sub

        Public Overridable Property [Type]() As PhasorType Implements IPhasorDefinition.Type
            Get
                Return m_type
            End Get
            Set(ByVal Value As PhasorType)
                m_type = Value
            End Set
        End Property

        Public Overrides Property ScalingFactor() As Double
            Get
                Return m_scale
            End Get
            Set(ByVal Value As Double)
                m_scale = Value
                If ConversionFactor > MaximumConversionFactor Then Throw New OverflowException("Conversion factor value (ScalingFactor * 10000) cannot exceed " & MaximumConversionFactor)
            End Set
        End Property

        Protected Overridable ReadOnly Property ConversionFactor() As Int32
            Get
                Return Convert.ToInt32(m_scale * 100000)
            End Get
        End Property

        Protected Overridable ReadOnly Property MaximumConversionFactor() As Int32
            Get
                ' Typical conversion factor should fit within 3 bytes (i.e., 24 bits) of space
                Return 2 ^ 24
            End Get
        End Property

        Public Overrides ReadOnly Property MaximumLabelLength() As Integer
            Get
                Return 16
            End Get
        End Property

        Public Overridable Property VoltageReference() As IPhasorDefinition Implements IPhasorDefinition.VoltageReference
            Get
                Return m_voltageReference
            End Get
            Set(ByVal Value As IPhasorDefinition)
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

End Namespace
