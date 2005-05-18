'***********************************************************************
'  PhasorDefinitionBase.vb - Phasor value definition base class
'  Copyright © 2005 - TVA, all rights reserved
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
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

Namespace EE.Phasor

    ' This class represents the common implementation of the protocol independent definition of a phasor value.
    Public MustInherit Class PhasorDefinitionBase

        Inherits ChannelDefinitionBase
        Implements IPhasorDefinition

        Private m_type As PhasorType
        Private m_voltageReference As IPhasorDefinition

        Protected Sub New()

            MyBase.New()

            m_type = PhasorType.Voltage
            m_voltageReference = Me

        End Sub

        Protected Sub New(ByVal index As Integer, ByVal label As String, ByVal scale As Integer, ByVal offset As Double, ByVal type As PhasorType, ByVal voltageReference As IPhasorDefinition)

            MyBase.New(index, label, scale, offset)

            m_type = type

            If type = PhasorType.Voltage Then
                m_voltageReference = Me
            Else
                m_voltageReference = voltageReference
            End If

        End Sub

        ' Dervied classes are expected to expose a Public Sub New(ByVal phasorDefinition As IPhasorDefinition)
        Protected Sub New(ByVal phasorDefinition As IPhasorDefinition)

            Me.New(phasorDefinition.Index, phasorDefinition.Label, phasorDefinition.ScalingFactor, phasorDefinition.Offset, _
                phasorDefinition.Type, phasorDefinition.VoltageReference)

        End Sub

        Public Overridable Property [Type]() As PhasorType Implements IPhasorDefinition.Type
            Get
                Return m_type
            End Get
            Set(ByVal Value As PhasorType)
                m_type = Value
            End Set
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
