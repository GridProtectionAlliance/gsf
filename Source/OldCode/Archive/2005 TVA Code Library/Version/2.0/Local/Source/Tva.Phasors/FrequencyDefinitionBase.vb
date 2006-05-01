'*******************************************************************************************************
'  FrequencyDefinitionBase.vb - Frequency and df/dt value definition base class
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

Imports Tva.Phasors.Common

' This class represents the common implementation of the protocol independent definition of a frequency and df/dt value.
<CLSCompliant(False)> _
Public MustInherit Class FrequencyDefinitionBase

    Inherits ChannelDefinitionBase
    Implements IFrequencyDefinition

    Private m_dfdtScale As Int32
    Private m_dfdtOffset As Single

    Protected Sub New(ByVal parent As IConfigurationCell)

        MyBase.New(parent)

    End Sub

    Protected Sub New(ByVal parent As IConfigurationCell, ByVal index As Int32, ByVal label As String, ByVal scale As Int32, ByVal offset As Single, ByVal dfdtScale As Int32, ByVal dfdtOffset As Single)

        MyBase.New(parent, index, label, scale, offset)

        m_dfdtScale = dfdtScale
        m_dfdtOffset = dfdtOffset

    End Sub

    Protected Sub New(ByVal parent As IConfigurationCell, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        MyBase.New(parent, binaryImage, startIndex)

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal frequencyDefinition As IFrequencyDefinition)
    Protected Sub New(ByVal frequencyDefinition As IFrequencyDefinition)

        MyClass.New(frequencyDefinition.Parent, frequencyDefinition.Index, frequencyDefinition.Label, frequencyDefinition.ScalingFactor, _
            frequencyDefinition.Offset, frequencyDefinition.DfDtScalingFactor, frequencyDefinition.DfDtOffset)

    End Sub

    Public Overrides ReadOnly Property DataFormat() As DataFormat
        Get
            Return Parent.FrequencyDataFormat
        End Get
    End Property

    Public ReadOnly Property NominalFrequency() As LineFrequency Implements IFrequencyDefinition.NominalFrequency
        Get
            Return Parent.NominalFrequency
        End Get
    End Property

    Public Overrides Property Offset() As Single
        Get
            Return NominalFrequencyValue(Parent.NominalFrequency)
        End Get
        Set(ByVal value As Single)
            Throw New NotSupportedException("Frequency offset is read-only - it is determined by nominal frequency specified in containing condiguration cell")
        End Set
    End Property

    Public Overridable Property DfDtOffset() As Single Implements IFrequencyDefinition.DfDtOffset
        Get
            Return m_dfdtOffset
        End Get
        Set(ByVal value As Single)
            m_dfdtOffset = value
        End Set
    End Property

    Public Overridable Property DfDtScalingFactor() As Int32 Implements IFrequencyDefinition.DfDtScalingFactor
        Get
            Return m_dfdtScale
        End Get
        Set(ByVal value As Int32)
            m_dfdtScale = value
        End Set
    End Property

End Class
