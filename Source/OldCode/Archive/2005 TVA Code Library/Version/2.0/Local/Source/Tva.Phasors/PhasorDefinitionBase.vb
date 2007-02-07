'*******************************************************************************************************
'  PhasorDefinitionBase.vb - Phasor value definition base class
'  Copyright © 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/18/2005 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization

''' <summary>This class represents the common implementation of the protocol independent definition of a phasor value.</summary>
<CLSCompliant(False), Serializable()> _
Public MustInherit Class PhasorDefinitionBase

    Inherits ChannelDefinitionBase
    Implements IPhasorDefinition

    Private m_type As PhasorType
    Private m_voltageReference As IPhasorDefinition

    Protected Sub New()
    End Sub

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

        ' Deserialize phasor definition
        m_type = info.GetValue("type", GetType(PhasorType))
        m_voltageReference = info.GetValue("voltageReference", GetType(IPhasorDefinition))

    End Sub

    Protected Sub New(ByVal parent As IConfigurationCell)

        MyBase.New(parent)

        m_type = PhasorType.Voltage
        m_voltageReference = Me

    End Sub

    Protected Sub New(ByVal parent As IConfigurationCell, ByVal index As Int32, ByVal label As String, ByVal scale As Int32, ByVal offset As Single, ByVal type As PhasorType, ByVal voltageReference As IPhasorDefinition)

        MyBase.New(parent, index, label, scale, offset)

        m_type = type

        If type = PhasorType.Voltage Then
            m_voltageReference = Me
        Else
            m_voltageReference = voltageReference
        End If

    End Sub

    Protected Sub New(ByVal parent As IConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        MyBase.New(parent, binaryImage, startIndex)

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal phasorDefinition As IPhasorDefinition)
    Protected Sub New(ByVal phasorDefinition As IPhasorDefinition)

        MyClass.New(phasorDefinition.Parent, phasorDefinition.Index, phasorDefinition.Label, _
            phasorDefinition.ScalingFactor, phasorDefinition.Offset, phasorDefinition.Type, phasorDefinition.VoltageReference)

    End Sub

    Public Overrides ReadOnly Property DataFormat() As DataFormat
        Get
            Return Parent.PhasorDataFormat
        End Get
    End Property

    Public Overridable ReadOnly Property CoordinateFormat() As CoordinateFormat Implements IPhasorDefinition.CoordinateFormat
        Get
            Return Parent.PhasorCoordinateFormat
        End Get
    End Property

    Public Overridable Property [Type]() As PhasorType Implements IPhasorDefinition.Type
        Get
            Return m_type
        End Get
        Set(ByVal value As PhasorType)
            m_type = value
        End Set
    End Property

    Public Overridable Property VoltageReference() As IPhasorDefinition Implements IPhasorDefinition.VoltageReference
        Get
            Return m_voltageReference
        End Get
        Set(ByVal value As IPhasorDefinition)
            If m_type = PhasorType.Voltage Then
                If Not value Is Me Then
                    Throw New NotImplementedException("Voltage phasors do not have a voltage reference")
                End If
            Else
                m_voltageReference = value
            End If
        End Set
    End Property

    Public Overrides Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, ByVal context As System.Runtime.Serialization.StreamingContext)

        MyBase.GetObjectData(info, context)

        ' Serialize phasor definition
        info.AddValue("type", m_type, GetType(PhasorType))
        info.AddValue("voltageReference", m_voltageReference, GetType(IPhasorDefinition))

    End Sub

    Public Overrides ReadOnly Property Attributes() As System.Collections.Generic.Dictionary(Of String, String)
        Get
            MyBase.Attributes.Add("Phasor Type", [Type] & ": " & [Enum].GetName(GetType(PhasorType), [Type]))
            Return MyBase.Attributes
        End Get
    End Property

End Class
