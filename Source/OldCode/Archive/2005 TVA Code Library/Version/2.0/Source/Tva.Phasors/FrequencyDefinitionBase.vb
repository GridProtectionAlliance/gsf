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

' This class represents the common implementation of the protocol independent definition of a frequency and df/dt value.
<CLSCompliant(False)> _
Public MustInherit Class FrequencyDefinitionBase

    Inherits ChannelDefinitionBase
    Implements IFrequencyDefinition

    Private m_dfdtScale As Integer
    Private m_dfdtOffset As Double

    Protected Sub New(ByVal parent As IConfigurationCell)

        MyBase.New(parent)

        m_dfdtScale = 1

    End Sub

    Protected Sub New(ByVal parent As IConfigurationCell, ByVal dataFormat As DataFormat, ByVal index As Integer, ByVal label As String, ByVal scale As Integer, ByVal offset As Double, ByVal dfdtScale As Double, ByVal dfdtOffset As Double)

        MyBase.New(parent, dataFormat, index, label, scale, offset)

        Me.DfDtScalingFactor = dfdtScale
        m_dfdtOffset = dfdtOffset

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal frequencyDefinition As IFrequencyDefinition)
    Protected Sub New(ByVal frequencyDefinition As IFrequencyDefinition)

        MyClass.New(frequencyDefinition.Parent, frequencyDefinition.DataFormat, frequencyDefinition.Index, frequencyDefinition.Label, frequencyDefinition.ScalingFactor, _
            frequencyDefinition.Offset, frequencyDefinition.DfDtScalingFactor, frequencyDefinition.DfDtOffset)

    End Sub

    Public ReadOnly Property NominalFrequency() As LineFrequency Implements IFrequencyDefinition.NominalFrequency
        Get
            Return Parent.NominalFrequency
        End Get
    End Property

    Public Overridable Property DfDtOffset() As Double Implements IFrequencyDefinition.DfDtOffset
        Get
            Return m_dfdtOffset
        End Get
        Set(ByVal value As Double)
            m_dfdtOffset = value
        End Set
    End Property

    Public Overridable Property DfDtScalingFactor() As Integer Implements IFrequencyDefinition.DfDtScalingFactor
        Get
            Return m_dfdtScale
        End Get
        Set(ByVal value As Integer)
            If value > MaximumDfDtScalingFactor Then Throw New OverflowException("DfDt scaling factor value cannot exceed " & MaximumDfDtScalingFactor)
            m_dfdtScale = value
        End Set
    End Property

    Public Overridable ReadOnly Property MaximumDfDtScalingFactor() As Integer Implements IFrequencyDefinition.MaximumDfDtScalingFactor
        Get
            ' Typical scaling/conversion factors should fit within 3 bytes (i.e., 24 bits) of space
            Return 2 ^ 24
        End Get
    End Property

End Class
