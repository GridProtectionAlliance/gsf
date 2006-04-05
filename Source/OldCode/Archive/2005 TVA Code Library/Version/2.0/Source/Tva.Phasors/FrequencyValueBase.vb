'*******************************************************************************************************
'  FrequencyValueBase.vb - Frequency and DfDt value base class
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
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports Tva.Interop

' This class represents the protocol independent a frequency and dfdt value.
<CLSCompliant(False)> _
Public MustInherit Class FrequencyValueBase

    Inherits ChannelValueBase(Of IFrequencyDefinition)
    Implements IFrequencyValue

    Private m_frequency As Single
    Private m_dfdt As Single

    Protected Sub New(ByVal parent As IDataCell)

        MyBase.New(parent)

    End Sub

    ' Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal frequency As Single, ByVal dfdt As Single)
    Protected Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal frequency As Single, ByVal dfdt As Single)

        MyBase.New(parent, frequencyDefinition)

        m_frequency = frequency
        m_dfdt = dfdt

    End Sub

    ' Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal unscaledFrequency As Int16, ByVal unscaledDfDt As Int16)
    Protected Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal unscaledFrequency As Int16, ByVal unscaledDfDt As Int16)

        MyClass.New(parent, frequencyDefinition, _
            Convert.ToSingle(unscaledFrequency / frequencyDefinition.ScalingFactor + frequencyDefinition.Offset), _
            Convert.ToSingle(unscaledDfDt / frequencyDefinition.DfDtScalingFactor + frequencyDefinition.DfDtOffset))

    End Sub

    ' Derived classes are expected expose a Public Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Int32)
    Protected Sub New(ByVal parent As IDataCell, ByVal frequencyDefinition As IFrequencyDefinition, ByVal binaryImage As Byte(), ByVal startIndex As Int32)

        MyBase.New(parent, frequencyDefinition)
        ParseBinaryImage(Nothing, binaryImage, startIndex)

    End Sub

    ' Derived classes are expected to expose a Public Sub New(ByVal frequencyValue As IFrequencyValue)
    Protected Sub New(ByVal frequencyValue As IFrequencyValue)

        MyClass.New(frequencyValue.Parent, frequencyValue.Definition, frequencyValue.Frequency, frequencyValue.DfDt)

    End Sub

    Public Overridable Property Frequency() As Single Implements IFrequencyValue.Frequency
        Get
            Return m_frequency
        End Get
        Set(ByVal value As Single)
            m_frequency = value
        End Set
    End Property

    Public Overridable Property DfDt() As Single Implements IFrequencyValue.DfDt
        Get
            Return m_dfdt
        End Get
        Set(ByVal value As Single)
            m_dfdt = value
        End Set
    End Property

    Public Overridable Property UnscaledFrequency() As Int16 Implements IFrequencyValue.UnscaledFrequency
        Get
            With Definition
                Return Convert.ToInt16((m_frequency - .Offset) * .ScalingFactor)
            End With
        End Get
        Set(ByVal value As Int16)
            With Definition
                m_frequency = value / .ScalingFactor + .Offset
            End With
        End Set
    End Property

    Public Overridable Property UnscaledDfDt() As Int16 Implements IFrequencyValue.UnscaledDfDt
        Get
            With Definition
                Return Convert.ToInt16((m_dfdt - .DfDtOffset) * .DfDtScalingFactor)
            End With
        End Get
        Set(ByVal value As Int16)
            With Definition
                m_dfdt = value / .DfDtScalingFactor + .DfDtOffset
            End With
        End Set
    End Property

    Public Overrides ReadOnly Property Values() As Single()
        Get
            Return New Single() {m_frequency, m_dfdt}
        End Get
    End Property

    Public Overrides ReadOnly Property IsEmpty() As Boolean
        Get
            Return (m_frequency = 0 And m_dfdt = 0)
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyLength() As UInt16
        Get
            If DataFormat = Phasors.DataFormat.FixedInteger Then
                Return 4
            Else
                Return 8
            End If
        End Get
    End Property

    Protected Overrides ReadOnly Property BodyImage() As Byte()
        Get
            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BodyLength)

            If DataFormat = Phasors.DataFormat.FixedInteger Then
                EndianOrder.BigEndian.CopyBytes(UnscaledFrequency, buffer, 0)
                EndianOrder.BigEndian.CopyBytes(UnscaledDfDt, buffer, 2)
            Else
                EndianOrder.BigEndian.CopyBytes(m_frequency, buffer, 0)
                EndianOrder.BigEndian.CopyBytes(m_dfdt, buffer, 4)
            End If

            Return buffer
        End Get
    End Property

    Protected Overrides Sub ParseBodyImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer)

        If DataFormat = Phasors.DataFormat.FixedInteger Then
            UnscaledFrequency = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex)
            UnscaledDfDt = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2)
        Else
            m_frequency = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex)
            m_dfdt = EndianOrder.BigEndian.ToSingle(binaryImage, startIndex + 4)
        End If

    End Sub

End Class
